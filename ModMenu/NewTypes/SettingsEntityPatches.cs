using HarmonyLib;
using Kingmaker.UI.MVVM._PCView.Settings;
using Kingmaker.UI.MVVM._PCView.Settings.Entities;
using Kingmaker.UI.MVVM._VM.Settings;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Decorative;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Difficulty;
using Kingmaker.UI.SettingsUI;
using ModMenu.Settings;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.Controls.SelectableState;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ModMenu.NewTypes
{
  internal class SettingsEntityPatches
  {
    /// <summary>
    /// Patch to return the correct view model for <see cref="UISettingsEntityImage"/>
    /// </summary>
    [HarmonyPatch(typeof(SettingsVM))]
    static class SettingsVM_Patch
    {
      [HarmonyPatch(nameof(SettingsVM.GetVMForSettingsItem)), HarmonyPrefix]
      static bool Prefix(
        UISettingsEntityBase uiSettingsEntity, SettingsVM __instance, ref VirtualListElementVMBase __result)
      {
        try
        {
          if (uiSettingsEntity is UISettingsEntityImage imageEntity)
          {
            Main.Logger.NativeLog("Returning SettingsEntityImageVM.");
            __result = new SettingsEntityImageVM(imageEntity);
            return false;
          }
          if (uiSettingsEntity is UISettingsEntityButton buttonEntity)
          {
            Main.Logger.NativeLog("Returning SettingsEntityButtonVM.");
            __result = new SettingsEntityButtonVM(buttonEntity);
            return false;
          }
          if (uiSettingsEntity is UISettingsEntityHeaderButton headerButtonEntity)
          {
            Main.Logger.NativeLog("Returning SettingsEntityHeaderButtonVM.");
            __result = new SettingsEntityHeaderButtonVM(headerButtonEntity);
            return false;
          }
        }
        catch (Exception e)
        {
          Main.Logger.LogException("SettingsVM.GetVMForSettingsItem", e);
        }
        return true;
      }

      [HarmonyPatch(nameof(SettingsVM.SwitchSettingsScreen)), HarmonyPostfix]
      static void Postfix(UISettingsManager.SettingsScreen settingsScreen, SettingsVM __instance)
      {
        try
        {
          if (settingsScreen != ModsMenuEntity.SettingsScreenId) { return; }
          Main.Logger.NativeLog("Configuring header buttons.");

          // Add all settings in each group to the corresponding expand/collapse button
          SettingsEntityHeaderButtonVM headerButtonVM = null;
          foreach (var entity in __instance.m_SettingEntities)
          {
            if (entity is SettingsEntityHeaderButtonVM buttonVM)
            {
              headerButtonVM = buttonVM;
            }
            else if (entity is not SettingsEntityHeaderVM)
            {
              headerButtonVM?.SettingsInGroup.Add(entity);
            }
          }
        }
        catch (Exception e)
        {
          Main.Logger.LogException("SettingsVM.SwitchSettingsScreen", e);
        }
      }
    }

    /// <summary>
    /// Patch to add new setting type prefabs.
    /// </summary>
    [HarmonyPatch(typeof(SettingsPCView.SettingsViews))]
    static class SettingsViews_Patch
    {
      [HarmonyPatch(nameof(SettingsPCView.SettingsViews.InitializeVirtualList)), HarmonyPrefix]
      static bool Prefix(SettingsPCView.SettingsViews __instance, VirtualListComponent virtualListComponent)
      {
        try
        {
          Main.Logger.NativeLog("Adding new type prefabs.");

          // Copy the bool settings
          var copyFrom = __instance.m_SettingsEntityBoolViewPrefab.gameObject;
          var imageTemplate = CreateImageTemplate(Object.Instantiate(copyFrom));
          var buttonTemplate =
            CreateButtonTemplate(Object.Instantiate(copyFrom),
            __instance.m_SettingsEntitySliderVisualPerceptionViewPrefab?.m_ResetButton);
          var headerButtonTemplate =
            CreateHeaderButtonTemplate(Object.Instantiate(copyFrom),
            __instance.m_SettingsEntitySliderVisualPerceptionViewPrefab?.m_ResetButton);

          virtualListComponent.Initialize(new IVirtualListElementTemplate[]
          {
            new VirtualListElementTemplate<SettingsEntityHeaderVM>(__instance.m_SettingsEntityHeaderViewPrefab),
            new VirtualListElementTemplate<SettingsEntityBoolVM>(__instance.m_SettingsEntityBoolViewPrefab),
            new VirtualListElementTemplate<SettingsEntityDropdownVM>(__instance.m_SettingsEntityDropdownViewPrefab, 0),
            new VirtualListElementTemplate<SettingsEntitySliderVM>(__instance.m_SettingsEntitySliderViewPrefab, 0),
            new VirtualListElementTemplate<SettingEntityKeyBindingVM>(__instance.m_SettingEntityKeyBindingViewPrefab),
            new VirtualListElementTemplate<SettingsEntityDropdownVM>(__instance.m_SettingsEntityDropdownDisplayModeViewPrefab, 1),
            new VirtualListElementTemplate<SettingsEntityDropdownGameDifficultyVM>(__instance.m_SettingsEntityDropdownGameDifficultyViewPrefab, 0),
            new VirtualListElementTemplate<SettingsEntitySliderVM>(__instance.m_SettingsEntitySliderVisualPerceptionViewPrefab, 1),
            new VirtualListElementTemplate<SettingsEntitySliderVM>(__instance.m_SettingsEntitySliderVisualPerceptionWithImagesViewPrefab, 2),
            new VirtualListElementTemplate<SettingsEntityStatisticsOptOutVM>(__instance.m_SettingsEntityStatisticsOptOutViewPrefab),
            new VirtualListElementTemplate<SettingsEntityImageVM>(imageTemplate),
            new VirtualListElementTemplate<SettingsEntityButtonVM>(buttonTemplate),
            new VirtualListElementTemplate<SettingsEntityHeaderButtonVM>(headerButtonTemplate)
          });
        }
        catch (Exception e)
        {
          Main.Logger.LogException("SettingsViews_Patch", e);
        }
        return false;
      }

      private static SettingsEntityButtonView CreateButtonTemplate(GameObject prefab, OwlcatButton buttonPrefab)
      {
        Main.Logger.NativeLog("Creating button template.");

        // Destroy the stuff we don't want from the source prefab
        Object.DestroyImmediate(prefab.GetComponent<SettingsEntityBoolPCView>());
        Object.DestroyImmediate(prefab.transform.Find("MultiButton").gameObject);
        Object.DontDestroyOnLoad(prefab);

        OwlcatButton buttonControl = null;
        TextMeshProUGUI buttonLabel = null;

        // Add in our own button
        if (buttonPrefab != null)
        {
          var button = Object.Instantiate(buttonPrefab.gameObject, prefab.transform);
          buttonControl = button.GetComponent<OwlcatButton>();
          buttonLabel = button.GetComponentInChildren<TextMeshProUGUI>();

          var layout = button.AddComponent<LayoutElement>();
          layout.ignoreLayout = true;

          var rect = button.transform as RectTransform;

          rect.anchorMin = new(1, 0.5f);
          rect.anchorMax = new(1, 0.5f);
          rect.pivot = new(1, 0.5f);

          rect.anchoredPosition = new(-55, 0);
          rect.sizeDelta = new(430, 45);
        }

        // Add our own View (after destroying the Bool one)
        var templatePrefab = prefab.AddComponent<SettingsEntityButtonView>();

        // Wire up the fields that would have been deserialized if coming from a bundle
        templatePrefab.HighlightedImage =
          prefab.transform.Find("HighlightedImage").gameObject.GetComponent<Image>();
        templatePrefab.Title =
          prefab.transform.Find("HorizontalLayoutGroup/Text").gameObject.GetComponent<TextMeshProUGUI>();
        templatePrefab.Button = buttonControl;
        templatePrefab.ButtonLabel = buttonLabel;

        return templatePrefab;
      }

      private static SettingsEntityImageView CreateImageTemplate(GameObject prefab)
      {
        Main.Logger.NativeLog("Creating image template.");

        //Destroy the stuff we don't want from the source prefab
        Object.DestroyImmediate(prefab.GetComponent<SettingsEntityBoolPCView>());
        Object.DestroyImmediate(prefab.transform.Find("MultiButton").gameObject);
        Object.DestroyImmediate(prefab.transform.Find("HorizontalLayoutGroup").gameObject);
        Object.DestroyImmediate(prefab.transform.Find("HighlightedImage").gameObject);
        Object.DontDestroyOnLoad(prefab);

        // Add our own View (after destroying the Bool one)
        var templatePrefab = prefab.AddComponent<SettingsEntityImageView>();

        // Create an imagePrefab as a child of the view so it can be scaled independently
        var imagePrefab = new GameObject("banner", typeof(RectTransform));
        imagePrefab.transform.SetParent(templatePrefab.transform, false);

        // Wire up the fields that would have been deserialized if coming from a bundle
        templatePrefab.Icon = imagePrefab.AddComponent<Image>();
        templatePrefab.Icon.preserveAspect = true;
        templatePrefab.TopBorder = prefab.transform.Find("TopBorderImage").gameObject;

        return templatePrefab;
      }

      // Allows overriding the behavior of OwlcatButton icons
      private static readonly FieldInfo CommonLayer = AccessTools.Field(typeof(OwlcatSelectable), "m_CommonLayer");

      private static readonly Sprite HoverButton = Helpers.CreateSprite("ModMenu.Assets.HoverButton.png");

      private static SettingsEntityHeaderButtonView CreateHeaderButtonTemplate(
        GameObject prefab, OwlcatButton buttonPrefab)
      {
        Main.Logger.NativeLog("Creating header button template.");

        // Destroy the stuff we don't want from the source prefab
        Object.DestroyImmediate(prefab.GetComponent<SettingsEntityBoolPCView>());
        Object.DestroyImmediate(prefab.transform.Find("MultiButton").gameObject);
        Object.DestroyImmediate(prefab.transform.Find("HorizontalLayoutGroup").gameObject);
        Object.DestroyImmediate(prefab.transform.Find("HighlightedImage").gameObject);
        Object.DestroyImmediate(prefab.transform.Find("VerticalLeftLineImage").gameObject);
        Object.DestroyImmediate(prefab.transform.Find("VerticalRightLineImage").gameObject);
        Object.DestroyImmediate(prefab.transform.Find("TopBorderImage").gameObject);
        Object.DontDestroyOnLoad(prefab);

        OwlcatButton buttonControl = null;
        Image buttonImage = null;
        // Add in our own button
        if (buttonPrefab != null)
        {
          var button = Object.Instantiate(buttonPrefab.gameObject, prefab.transform);
          Object.DestroyImmediate(button.transform.Find("Text").gameObject);

          buttonControl = button.GetComponent<OwlcatButton>();
          buttonImage = button.GetComponent<Image>();

          var buttonLayout = button.AddComponent<LayoutElement>();
          buttonLayout.ignoreLayout = true;

          var buttonRect = button.transform as RectTransform;

          buttonRect.anchorMin = new(0, 0);
          buttonRect.anchorMax = new(0, 0);
          buttonRect.pivot = new(0, 0);

          buttonRect.anchoredPosition = new(30, 50);
          buttonRect.sizeDelta = new(40, 40);

          // Replace Button icons
          var commonLayer = (List<OwlcatSelectableLayerPart>)CommonLayer.GetValue(buttonControl);
          var spriteState = commonLayer.First().SpriteState;
          spriteState.selectedSprite = SettingsEntityHeaderButtonView.ExpandedButton;
          spriteState.pressedSprite = SettingsEntityHeaderButtonView.ExpandedButton;
          spriteState.highlightedSprite = HoverButton;
          commonLayer.First().SpriteState = spriteState;
        }

        // Add our own View (after destroying the Bool one)
        var templatePrefab = prefab.AddComponent<SettingsEntityHeaderButtonView>();

        // Wire up the fields that would have been deserialized if coming from a bundle
        templatePrefab.Button = buttonControl;
        templatePrefab.ButtonImage = buttonImage;
        return templatePrefab;
      }
    }
  }
}
