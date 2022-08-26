using HarmonyLib;
using Kingmaker.UI.MVVM._PCView.Settings;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Decorative;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Difficulty;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Kingmaker.UI.MVVM._VM.Settings;
using Kingmaker.UI.SettingsUI;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using UnityEngine;
using Kingmaker.UI.MVVM._PCView.Settings.Entities;
using TMPro;
using UnityEngine.UI;
using Owlcat.Runtime.UI.Controls.Button;

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
      static bool Prefix(UISettingsEntityBase uiSettingsEntity, ref VirtualListElementVMBase __result)
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
        return true;
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
        Main.Logger.NativeLog("Adding SettingsEntityImageVM.");

        // Copy the bool settings
        var copyFrom = __instance.m_SettingsEntityBoolViewPrefab.gameObject;
        var imageTemplate = CreateImageTemplate(Object.Instantiate(copyFrom));
        var buttonTemplate =
          CreateButtonTemplate(Object.Instantiate(copyFrom),
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
        });
        return false;
      }

      private static SettingsEntityButtonView CreateButtonTemplate(GameObject prefab, OwlcatButton buttonPrefab)
      {
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
        //Destroy the stuff we don't want from the source prefab
        Object.DestroyImmediate(prefab.GetComponent<SettingsEntityBoolPCView>());
        Object.DestroyImmediate(prefab.transform.Find("MultiButton").gameObject);
        Object.DestroyImmediate(prefab.transform.Find("HorizontalLayoutGroup").gameObject);
        Object.DestroyImmediate(prefab.transform.Find("HighlightedImage").gameObject);
        Object.DontDestroyOnLoad(prefab);

        // Add our own View (after destroying the Bool one)
        var templatePrefab = prefab.AddComponent<SettingsEntityImageView>();

        // Wire up the fields that would have been deserialized if coming from a bundle
        templatePrefab.Image = prefab.AddComponent<Image>();
        templatePrefab.Image.preserveAspect = true;
        return templatePrefab;
      }
    }
  }
}
