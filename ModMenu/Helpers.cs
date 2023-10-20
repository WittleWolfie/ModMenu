using HarmonyLib;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModMenu
{
  /// <summary>
  /// Generic utils for simple operations.
  /// </summary>
  internal static class Helpers
  {
    private static readonly List<LocalString> Strings = new();
    internal static LocalizedString EmptyString = CreateString("", "");

    internal static LocalizedString CreateString(string key, string enGB, string ruRU = null)
    {
      var localString = new LocalString(key, enGB, ruRU);
      Strings.Add(localString);
      if (LocalizationManager.Initialized)
        localString.Register();
      return localString.LocalizedString;
    }

    internal static Sprite CreateSprite(string embeddedImage)
    {
      var assembly = Assembly.GetExecutingAssembly();
      using var stream = assembly.GetManifestResourceStream(embeddedImage);
      byte[] bytes = new byte[stream.Length];
      stream.Read(bytes, 0, bytes.Length);
      var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
      _ = texture.LoadImage(bytes);
      var sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), Vector2.zero);
      return sprite;
    }

    private class LocalString
    {
      public readonly LocalizedString LocalizedString;
      private readonly string enGB;
      private readonly string ruRU;
      const string NullString = "<null>";

      public LocalString(string key, string enGB, string ruRU)
      {
        LocalizedString = new LocalizedString() { m_Key = key };
        this.enGB = enGB;
        this.ruRU = ruRU;
      }

      public void Register()
      {
        string localized;
        if (LocalizationManager.CurrentPack.Locale == Locale.enGB)
        {
          localized = enGB;
          goto putString;
        }

        localized = (LocalizationManager.CurrentPack.Locale) switch
        {
          Locale.ruRU => ruRU,
          _ => ""
        };

        if (localized.IsNullOrEmpty() || localized == NullString)
          localized = enGB;

        ;putString:
        LocalizationManager.CurrentPack.PutString(LocalizedString.m_Key, localized);
      }
    }

    [HarmonyPatch(typeof(LocalizationManager))]
    static class LocalizationManager_Patch
    {
      [HarmonyPatch(nameof(LocalizationManager.OnLocaleChanged)), HarmonyPostfix]
      static void Postfix()
      {
        try
        {
          Strings.ForEach(str => str.Register());
        }
        catch (Exception e)
        {
          Main.Logger.LogException("Failed to handle locale change.", e);
        }
      }
    }
  }
}
