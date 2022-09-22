using Kingmaker.Localization;
using Kingmaker.UI.SettingsUI;
using System.Text;
using UnityEngine;

namespace ModMenu.Settings
{
#if DEBUG
  /// <summary>
  /// Test settings group shown on debug builds to validate usage.
  /// </summary>
  internal class TestSettings
  {
    private static readonly string RootKey = "mod-menu.test-settings";
    private enum TestEnum
    {
      First,
      Second,
      Third,
      Last
    }

    private class UISettingsEntityDropdownTestEnum : UISettingsEntityDropdownEnum<TestEnum> { }

    internal void Initialize()
    {
      ModMenu.AddSettings(
        SettingsBuilder.New(RootKey, CreateString("title", "Testing settings"))
          .AddImage(Helpers.CreateSprite("ModMenu.WittleWolfie.png"), 250)
          .AddDefaultButton(OnDefaultsApplied)
          .AddButton(
            Button.New(
              CreateString("button-desc", "This is a button"), CreateString("button-text", "Click Me!"), OnClick))
          .AddToggle(
            Toggle.New(GetKey("toggle"), defaultValue: true, CreateString("toggle-desc", "This is a toggle"))
              .ShowVisualConnection()
              .OnValueChanged(OnToggle))
          .AddDropdown(
            Dropdown<TestEnum>.New(
                GetKey("dropdown"),
                TestEnum.Second,
                CreateString("dropdown-desc", "This is a dropdown"),
                ScriptableObject.CreateInstance<UISettingsEntityDropdownTestEnum>())
              .ShowVisualConnection()
              .IsModificationAllowed(CheckToggle)
              .WithLongDescription(
                CreateString(
                  "dropdown-long-desc",
                  "This is a dropdown based on TestEnum. In order to change the value the connected toggle must be on."
                  +" After switching it on or off exit and enter the menu again to lock/unlock it."))
              .DependsOnSave())
          .AddSubHeader(CreateString("sub-header", "Test Sliders"))
          .AddSliderFloat(
            SliderFloat.New(
              GetKey("float-default"),
              defaultValue: 1.0f,
              CreateString("float-default-desc", "This is a default float slider"),
              minValue: 0.0f,
              maxValue: 2.6f))
          .AddSliderFloat(
            SliderFloat.New(
                GetKey("float"),
                defaultValue: 0.05f,
                CreateString("float-desc", "This is a custom float slider"),
                minValue: 0.05f,
                maxValue: 1.00f)
              .WithStep(0.05f)
              .WithDecimalPlaces(2)
              .HideValueText()
              .OnTempValueChanged(OnSliderFloatChanged))
          .AddSliderInt(
            SliderInt.New(
              GetKey("int-default"),
              defaultValue: 1,
              CreateString("int-default-desc", "This is a default int slider"),
              minValue: 0,
              maxValue: 5))
          .AddSliderInt(
            SliderInt.New(
                GetKey("int"),
                defaultValue: 2,
                CreateString("int-desc", "This is a custom int slider"),
                minValue: 1,
                maxValue: 6)
              .HideValueText()));

      ModMenu.AddSettings(
        SettingsBuilder.New(GetKey("extra"), CreateString("extra-title", "More Test Settings"))
          .AddToggle(
            Toggle.New(
              GetKey("empty-toggle"), defaultValue: false, CreateString("empty-toggle-desc", "A useless toggle")))
          .AddDropdownList(
            DropdownList.New(
                GetKey("dropdown-list"),
                2,
                CreateString("dropdown-list", "A dropdown list"),
                new()
                {
                  CreateString("dropdown-list-1", "Value is 0"),
                  CreateString("dropdown-list-2", "Value is 1"),
                  CreateString("dropdown-list-3", "Value is 2"),
                })
              .OnTempValueChanged(value => Main.Logger.Log($"Currently selected dropdown in list is {value}"))));
    }

    private void OnClick()
    {
      var log = new StringBuilder();
      log.AppendLine("Current settings: ");
      log.AppendLine($"-Toggle: {CheckToggle()}");
      log.AppendLine($"-Dropdown: {ModMenu.GetSettingValue<TestEnum>(GetKey("dropdown"))}");
      log.AppendLine($"-Default Slider Float: {ModMenu.GetSettingValue<float>(GetKey("float-default"))}");
      log.AppendLine($"-Slider Float: {ModMenu.GetSettingValue<float>(GetKey("float"))}");
      log.AppendLine($"-Default Slider Int: {ModMenu.GetSettingValue<int>(GetKey("int-default"))}");
      log.AppendLine($"-Slider Int: {ModMenu.GetSettingValue<int>(GetKey("int"))}");
      Main.Logger.Log(log.ToString());
    }

    private void OnDefaultsApplied()
    {
      Main.Logger.NativeLog("Defaults were applied!");
    }

    private bool CheckToggle()
    {
      Main.Logger.NativeLog("Checking toggle");
      return ModMenu.GetSettingValue<bool>(GetKey("toggle"));
    }

    private void OnToggle(bool value)
    {
      Main.Logger.Log($"Toggle switched to {value}");
    }

    private void OnSliderFloatChanged(float value)
    {
      Main.Logger.Log($"Float slider changed to {value}");
    }

    private static LocalizedString CreateString(string partialKey, string text)
    {
      return Helpers.CreateString(GetKey(partialKey), text);
    }

    private static string GetKey(string partialKey)
    {
      return $"{RootKey}.{partialKey}";
    }
  }
#endif
}
