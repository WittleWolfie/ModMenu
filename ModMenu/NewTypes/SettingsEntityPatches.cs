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
        return true;
      }
    }

    /// <summary>
    /// Patch to add <see cref="SettingsEntityImageVM"/> as an available ViewModel.
    /// </summary>
    [HarmonyPatch(typeof(SettingsPCView.SettingsViews))]
    static class SettingsViews_Patch
    {
      [HarmonyPatch(nameof(SettingsPCView.SettingsViews.InitializeVirtualList)), HarmonyPrefix]
      static bool Prefix(SettingsPCView.SettingsViews __instance, VirtualListComponent virtualListComponent)
      {
        Main.Logger.NativeLog("Adding SettingsEntityImageVM.");
        var obj = new GameObject("ImageView", typeof(RectTransform));
        var prefabToAddtoList = obj.AddComponent<SettingsEntityImageView>();

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
          new VirtualListElementTemplate<SettingsEntityImageVM>(prefabToAddtoList),
        });
        return false;
      }
    }
  }
}
