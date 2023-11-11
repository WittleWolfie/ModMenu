using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Kingmaker.UI.SettingsUI;
using ModMenu.NewTypes;
using System.Text;
using UnityEngine;
using static Kingmaker.UI.KeyboardAccess;
using static ModMenu.Helpers;

namespace ModMenu.Settings
{
#if DEBUG
  /// <summary>
  /// Test settings group shown on debug builds to validate usage.
  /// </summary>
  internal class TestSettings : ISettingsChanged
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
          .SetMod(Main.Entry)
          .SetModDescription(Helpers.CreateString("test-settings-desc", 
          enGB:"This is a test description for mod and let's make it a bit longer to take several lines.", 
          ruRU: "Здесь описание для тестового мода и пусть оно будь достаточно длинным, чтобы занимать пару строчек", 
          zhCN: "这是模组描述的一个测试案例，现在让我们多水一点字数吧，这样的话描述就能有好几行了",
          deDE: "Dies ist eine Testbeschreibung für einen Mod, und sie ist noch ein wenig länger, damit sie mehrere Zeilen benötigt.",
          frFR: "Ceci est un exemple de description de mod qui doit être assez long pour prendre deux lignes mais on n'a pas dit au traducteur quelle est la taille des lignes, j'imagine que c'est assez long maintenant. "))
          .AddImage(Helpers.CreateSprite("ModMenu.WittleWolfie.png"), 250)
          .AddDefaultButton(OnDefaultsApplied)
          .AddButton(
            Button.New(
              CreateString("button-desc", "This is a button"), CreateString("button-text", "Click Me!"), OnClick))
          .AddToggle(
            Toggle.New(GetKey("toggle"), defaultValue: true, CreateString("toggle-desc", "This is a toggle"))
              .ShowVisualConnection()
              .OnValueChanged(OnToggle))
          .AddToggle(
            Toggle.New(GetKey("toggle-updateable"), defaultValue: true, CreateString("toggle-updateable-desc", "This is a toggle changes the LongDescription text!"))
              .ShowVisualConnection()
              .OnTempValueChanged(OnToggleUDescriptionUpdate))
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
                  + " After switching it on or off exit and enter the menu again to lock/unlock it."))
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
              .HideValueText())
          .AddAnotherSettingsGroup(GetKey("extra"), CreateString("extra-title", "More Test Settings"))
          .AddDefaultButton()
          .AddToggle(
            Toggle.New(
              GetKey("empty-toggle"), defaultValue: false, CreateString("empty-toggle-desc", "A useless toggle")))
          .AddDropdownList(
            DropdownList.New(
                GetKey("dropdown-list"),
                defaultSelected: 2,
                CreateString("dropdown-list", "A dropdown list"),
                new()
                {
                  CreateString("dropdown-list-1", "Value is 0"),
                  CreateString("dropdown-list-2", "Value is 1"),
                  CreateString("dropdown-list-3", "Value is 2"),
                })
              .OnTempValueChanged(value => Main.Logger.Log($"Currently selected dropdown in list is {value}")))
          .AddKeyBinding(
            KeyBinding.New(
                GetKey("key-binding"),
                GameModesGroup.All,
                CreateString(GetKey("key-binding-desc"), "This sets a key binding"))
              .SetPrimaryBinding(KeyCode.W, withCtrl: true, withAlt: true),
            OnKeyPress)
          .AddKeyBinding(
            KeyBinding.New(
                GetKey("key-binding-default"),
                GameModesGroup.All,
                CreateString(GetKey("key-binding-default-desc"), "This binding is pre-set"))
              .SetPrimaryBinding(KeyCode.W, withCtrl: true, withAlt: true)
              .SetSecondaryBinding(KeyCode.M, withAlt: true, withShift: true),
            OnKeyPress)
          .AddDropdownButton(
            DropdownButton.New(
                GetKey("dropdown-button"),
                defaultSelected: 2,
                CreateString("dropdown-button", "A dropdown button"),
                CreateString("dropdown-button-text", "Apply"),
                selected => Main.Logger.Log($"Dropdown button clicked: {selected}"),
                new()
                {
                  CreateString("dropdown-button-1", "Button calls onClick(0)"),
                  CreateString("dropdown-button-2", "Button calls onClick(1)"),
                  CreateString("dropdown-button-3", "Button calls onClick(2)"),
                })
              .OnTempValueChanged(value => Main.Logger.Log($"Currently selected dropdown button is {value}"))));

      EventBus.Subscribe(this);
    }

    private void OnKeyPress()
    {
      Main.Logger.Log($"Key was pressed!");
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

    public void OnToggleUDescriptionUpdate(bool value)
    {
      SettingsDescriptionUpdater<SettingsEntityBoolVM> sdu = new();
      sdu.TryUpdate("This is a toggle changes the LongDescription text!", $"Hey this value is now {value}");
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

    public void HandleApplySettings()
    {
      Main.Logger.Log("'Apply' button in the settings window was pressed!");
    }
  }
#endif
}
