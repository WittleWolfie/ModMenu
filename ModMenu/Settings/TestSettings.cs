using Kingmaker.UI.SettingsUI;
using System.IO;
using UnityEngine;

namespace ModMenu.Settings
{
#if DEBUG
  /// <summary>
  /// Test settings group shown on debug builds to validate usage.
  /// </summary>
  internal class TestSettings
  {
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
        new SettingsGroup("testsettings.group", Helpers.CreateString("TestSettings.Title", "Test Settings"))
          .AddImage(CreateSprite())
          .AddToggle(
            new(
              "testsettings.toggle",
              defaultValue: false,
              Helpers.CreateString("testsettings.toggle", "This is a toggle"),
              onValueChanged: OnToggle))
          .AddDropdown(
            new(
              "testsettings.dropdownenum",
              defaultValue: TestEnum.Third,
              Helpers.CreateString("testsettings.dropdownenum", "This is an enum dropdown"),
              onValueChanged: OnDropdownSelected),
            ScriptableObject.CreateInstance<UISettingsEntityDropdownTestEnum>())
          .AddSliderFloat(
            new(
              "testsettings.sliderfloat",
              defaultValue: 1.0f,
              Helpers.CreateString("testsettings.sliderfloat", "This is a slider using a float"),
              onValueChanged: OnSliderFloatChanged),
            minValue: 0.2f,
            maxValue: 2.6f)
          .AddSliderInt(
            new(
              "testsettings.sliderint",
              defaultValue: 2,
              Helpers.CreateString("testsettings.sliderint", "This is a slider using an int"),
              onValueChanged: OnSliderIntChanged),
            minValue: -5,
            maxValue: 15));
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

    public static Sprite CreateSprite(int size = 64)
    {
      var bytes =
        File.ReadAllBytes("D:\\Ithiel\\Documents\\GitHub\\CharacterOptionsPlus\\CharacterOptionsPlus_Unity\\Assets\\Icons\\FuriousFocus.png");
      var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
      _ = texture.LoadImage(bytes);
      var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0, 0));
      return sprite;
    }
  }
#endif
}
