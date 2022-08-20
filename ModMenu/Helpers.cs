using HarmonyLib;
using Kingmaker.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModMenu
{
  /// <summary>
  /// Generic utils for simple operations.
  /// </summary>
  internal static class Helpers
  {
    internal static LocalizedString CreateString(string key, string value)
    {
      var localizedString = new LocalizedString() { m_Key = key };
      PutString(key, value);
      return localizedString;
    }

    private static void PutString(string key, string value)
    {
      if (LocalizationManager.Initialized)
      {
        LocalizationManager.CurrentPack.PutString(key, value);
      }
      else
      {
        Strings.Push((key, value));
      }
    }

    private static readonly Stack<(string key, string value)> Strings = new();
    [HarmonyPatch(typeof(LocalizationManager))]
    static class LocalizationManager_Patch
    {
      [HarmonyPatch(nameof(LocalizationManager.Init)), HarmonyPrefix]
      static void Init_Prefix()
      {
        try
        {
          if (LocalizationManager.Initialized) { return; } // Don't try to add the strings twice

          while (Strings.Any())
          {
            var localString = Strings.Pop();
            LocalizationManager.CurrentPack.PutString(localString.key, localString.value);
          }
        }
        catch (Exception e)
        {
          Main.Logger.LogException(e);
        }
      }

      // TODO: Clean up this logic and handle locale switches
    }
  }
}
