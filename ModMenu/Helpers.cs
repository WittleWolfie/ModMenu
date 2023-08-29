using HarmonyLib;
using Kingmaker;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.UI.MVVM._PCView.Settings.Entities;
using Kingmaker.UI.MVVM._PCView.Settings;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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

    internal static LocalizedString CreateString(string key, string enGB, string ruRU = "")
    {
      var localString = new LocalString(key, enGB, ruRU);
      Strings.Add(localString);
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

      public LocalString(string key, string enGB, string ruRU)
      {
        LocalizedString = new LocalizedString() { m_Key = key };
        this.enGB = enGB;
        this.ruRU = ruRU;
      }

      public void Register()
      {
        var localized = enGB;
        switch (LocalizationManager.CurrentPack.Locale)
        {
          case Locale.ruRU:
            if (!string.IsNullOrEmpty(ruRU))
              localized = ruRU;
            break;
        }
        LocalizationManager.CurrentPack.PutString(LocalizedString.m_Key, localized);
      }
    }


    internal class SettingsDescriptionUpdater<T>
        where T : SettingsEntityWithValueVM
    {
      private readonly string pathMainUi;
      private readonly string pathDescriptionUi;

      private Transform mainUI;
      private Transform settingsUI;
      private Transform descriptionUI;

      private List<SettingsEntityWithValueView<T>> settingViews;
      private SettingsDescriptionPCView descriptionView;

      public SettingsDescriptionUpdater(string pathUI, string pathDesriptionUI)
      {
        pathMainUi = pathUI;
        pathDescriptionUi = pathDesriptionUI;
      }

      private bool Ensure()
      {
        // UI tends to change frequently, ensure that eveything is up to date.

        if ((mainUI = Game.Instance.RootUiContext.m_CommonView.transform) == null)
          return false;

        settingsUI = mainUI.Find(pathMainUi);
        descriptionUI = mainUI.Find(pathDescriptionUi);
        if (settingsUI == null || descriptionUI == null)
          return false;

        settingViews = settingsUI.gameObject.GetComponentsInChildren<SettingsEntityWithValueView<T>>().ToList();
        descriptionView = descriptionUI.GetComponent<SettingsDescriptionPCView>();

        if (settingViews == null || descriptionView == null || settingViews.Count == 0)
          return false;

        return true;
      }

      public bool TryUpdate(string title, string desription)
      {
        // Searches for the title of the setting you are attempt to change. The only downside is that titles must be unique for the description you are attempting to change.

        if (!Ensure()) return false;

        T svm = null;

        foreach (var settingView in settingViews)
        {
          var test = (T)settingView.GetViewModel();
          if (test.Title.Equals(title))
          {
            svm = test;
            break;
          }
        }

        if (svm == null)
          return false;

        svm.GetType().GetField("Description").SetValue(svm, desription);

        descriptionView.m_DescriptionText.text = desription;

        return true;
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
