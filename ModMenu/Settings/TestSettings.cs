using HarmonyLib;
using Kingmaker.Settings;
using Kingmaker.UI.MVVM._PCView.Settings;
using Kingmaker.UI.MVVM._VM.Settings;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Decorative;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Difficulty;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Kingmaker.UI.SettingsUI;
using ModMenu.NewTypes;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using System;
using System.IO;
using UnityEngine;

namespace ModMenu.Settings
{
  // Note: Settings keys cannot have uppercase letters
  // Note 2: Only types that work are in GetVMForSettingsItem

  /// <summary>
  /// Test settings group shown on debug builds to validate usage.
  /// </summary>
  internal class TestSettings
  {
    private readonly UISettingsGroup TestSettingsGroup = ScriptableObject.CreateInstance<UISettingsGroup>();
    private readonly UISettingsEntityBase[] TestSettingsEntities = new UISettingsEntityBase[5];

    public TestSettings()
    {
      TestSettingsGroup.name = "testsettings.group";
      TestSettingsGroup.Title = Helpers.CreateString("TestSettings.Title", "Test Settings");
      TestSettingsGroup.SettingsList = TestSettingsEntities;
    }

    private UISettingsEntityImage Image;

    private UISettingsEntityBool Toggle;
    private SettingsEntityBool ToggleValue = new("testsettings.toggle", defaultValue: false);

    private enum TestEnum
    {
      First,
      Second,
      Third,
      Last
    }
    private UISettingsEntityDropdownEnum<TestEnum> DropdownEnum;
    private SettingsEntityEnum<TestEnum> DropdownEnumValue =
      new("testsettings.dropdownenum", defaultValue: TestEnum.Second);

    private UISettingsEntitySliderFloat SliderFloat;
    private SettingsEntityFloat SliderFloatValue = new("testsettings.sliderfloat", defaultValue: 1.0f);

    private UISettingsEntitySliderInt SliderInt;
    private SettingsEntityInt SliderIntValue = new("testsettings.sliderint", defaultValue: 2);

    private class UISettingsEntityDropdownTestEnum : UISettingsEntityDropdownEnum<TestEnum> { }

    internal void Initialize()
    {
      Image = ScriptableObject.CreateInstance<UISettingsEntityImage>();
      Image.Sprite = Create();

      Toggle = ScriptableObject.CreateInstance<UISettingsEntityBool>();
      Toggle.m_Description = Helpers.CreateString("testsettings.toggle", "This is a toggle");
      Toggle.m_TooltipDescription = Toggle.m_Description;
      Toggle.DefaultValue = ToggleValue.DefaultValue;
      Toggle.LinkSetting(ToggleValue);
      (ToggleValue as IReadOnlySettingEntity<bool>).OnValueChanged += OnToggle;

      Main.Logger.Log("Toggle done.");

      DropdownEnum = ScriptableObject.CreateInstance<UISettingsEntityDropdownTestEnum>();
      DropdownEnum.m_Description = Helpers.CreateString("testsettings.dropdownenum", "This is an enum dropdown");
      DropdownEnum.m_TooltipDescription = DropdownEnum.m_Description;
      DropdownEnum.m_CashedLocalizedValues ??= new();
      foreach (var value in Enum.GetValues(typeof(TestEnum)))
      {
        DropdownEnum.m_CashedLocalizedValues.Add(value.ToString());
      }
      DropdownEnum.LinkSetting(DropdownEnumValue);
      (DropdownEnumValue as IReadOnlySettingEntity<TestEnum>).OnValueChanged += OnDropdownSelected;

      Main.Logger.Log("Dropdown done.");

      SliderFloat = ScriptableObject.CreateInstance<UISettingsEntitySliderFloat>();
      SliderFloat.m_Description = Helpers.CreateString("testsettings.sliderfloat", "This is a slider using a float");
      SliderFloat.m_TooltipDescription = SliderFloat.m_Description;
      SliderFloat.m_MinValue = 0.2f;
      SliderFloat.m_MaxValue = 2.6f;
      SliderFloat.m_Step = 0.1f;
      SliderFloat.m_ShowValueText = true;
      SliderFloat.m_DecimalPlaces = 1;
      SliderFloat.LinkSetting(SliderFloatValue);
      (SliderFloatValue as IReadOnlySettingEntity<float>).OnValueChanged += OnSliderFloatChanged;

      Main.Logger.Log("SliderFloat done.");

      SliderInt = ScriptableObject.CreateInstance<UISettingsEntitySliderInt>();
      SliderInt.m_Description = Helpers.CreateString("testsettings.sliderint", "This is a slider using an int");
      SliderInt.m_TooltipDescription = SliderInt.m_Description;
      SliderInt.m_MinValue = -5;
      SliderInt.m_MaxValue = 15;
      SliderInt.m_ShowValueText = true;
      SliderInt.LinkSetting(SliderIntValue);
      (SliderIntValue as IReadOnlySettingEntity<int>).OnValueChanged += OnSliderIntChanged;

      Main.Logger.Log("SliderInt done.");

      TestSettingsEntities[0] = Image;
      TestSettingsEntities[1] = DropdownEnum;
      TestSettingsEntities[2] = SliderFloat;
      TestSettingsEntities[3] = SliderInt;
      TestSettingsEntities[4] = Toggle;

      ModsMenuEntity.ModSettings.Add(TestSettingsGroup);
    }

    private void OnToggle(bool value)
    {
      Main.Logger.Log($"Toggle switched to {value}");
    }

    private void OnDropdownSelected(TestEnum value)
    {
      Main.Logger.Log($"Dropdown changed to {value}");
    }

    private void OnSliderFloatChanged(float value)
    {
      Main.Logger.Log($"Float slider changed to {value}");
    }

    private void OnSliderIntChanged(int value)
    {
      Main.Logger.Log($"Int slider changed to {value}");
    }

    public static Sprite Create(int size = 64)
    {
      var bytes =
        File.ReadAllBytes("D:\\Ithiel\\Documents\\GitHub\\CharacterOptionsPlus\\CharacterOptionsPlus_Unity\\Assets\\Icons\\FuriousFocus.png");
      var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
      _ = texture.LoadImage(bytes);
      var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0, 0));
      return sprite;
    }

    [HarmonyPatch(typeof(SettingsVM))]
    static class SettingsVM_Patch
    {
      [HarmonyPatch(nameof(SettingsVM.GetVMForSettingsItem)), HarmonyPrefix]
      static bool Prefix(UISettingsEntityBase uiSettingsEntity, ref VirtualListElementVMBase __result)
      {
        if (uiSettingsEntity is UISettingsEntityImage imageEntity)
        {
          Main.Logger.Log("Returning image VM");
          __result = new SettingsEntityImageVM(imageEntity);
          return false;
        }
        return true;
      }
    }

    [HarmonyPatch(typeof(SettingsPCView.SettingsViews))]
    static class SettingsViews_Patch
    {
      [HarmonyPatch(nameof(SettingsPCView.SettingsViews.InitializeVirtualList)), HarmonyPrefix]
      static bool Prefix(SettingsPCView.SettingsViews __instance, VirtualListComponent virtualListComponent)
      {
        Main.Logger.NativeLog("Initializing image view.");
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
