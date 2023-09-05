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

    /// <summary>
    /// Updated the Description field of a setting that is set with WithLongDescription(). The update works by finding the Title of the setting.
    /// Titles that you wish to update must be unique inorder to update the correct setting.
    /// </summary>
    /// <typeparam name="T">
    /// This needs to be a class of the type that inherits from SettingsEntityWithValueVM that you wish to update. Such as if you are updating a slider the typeparam
    /// must be SettingsEntitySliderVM
    /// </typeparam>
    public class SettingsDescriptionUpdater<T>
        where T : SettingsEntityWithValueVM
    {
      private readonly string pathMainUi;
      private readonly string pathDescriptionUi;

      private Transform mainUI;
      private Transform settingsUI;
      private Transform descriptionUI;

      private List<SettingsEntityWithValueView<T>> settingViews;
      private SettingsDescriptionPCView descriptionView;

      /// <summary>
      /// Expected path as of 2.1.5r
      /// </summary>
      public const string PATH_MAIN_UI = "Canvas/SettingsView/ContentWrapper/VirtualListVertical/Viewport/Content";

      /// <summary>
      /// Expected path as of 2.1.5r
      /// </summary>
      public const string PATH_DESCRIPTION_UI = "Canvas/SettingsView/ContentWrapper/DescriptionView";

      /// <summary>
      /// Constuctor for SettingsDescriptionUpdater. Sets up the paths for where the UI gameobjects at located
      /// </summary>
      /// <param name="pathMainUI">
      /// Optional. This is the path to main UI where setting GameOjects are located are located.
      /// Defaults to PATH_MAIN_UI which should work in 2.1.5r.
      /// </param>
      /// <param name="pathDesriptionUI">
      /// This is the path to the Description UI where the Description GameOject SettingsDescriptionPCView is located.
      /// Defaults to PATH_DESCRIPTION_UI which should work in 2.1.5r.
      /// </param>
      public SettingsDescriptionUpdater(string pathMainUI = PATH_MAIN_UI, string pathDesriptionUI = PATH_DESCRIPTION_UI)
      {
        pathMainUi = pathMainUI;
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

      /// <summary>
      /// This is the method that updates the Description of the SettingsEntityWithValueVM
      /// </summary>
      /// <param name="title">
      /// This is the UNIQUE Title of the setting that you wish to edit. If the Title is
      /// not unique then the incorrect setting may be updated.
      /// </param>
      /// <param name="description">
      /// The text you wish to set the Description to.
      /// </param>
      /// <returns>
      /// Will return true if the update was successfull.
      /// </returns>
      
      public bool TryUpdate(string title, string description)
      {
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

        svm.GetType().GetField("Description").SetValue(svm, description);

        descriptionView.m_DescriptionText.text = description;

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
