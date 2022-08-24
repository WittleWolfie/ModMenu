using Kingmaker.UI.SettingsUI;
using System.IO;
using System.Reflection;
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
          .AddButton(
            new(
              description: Helpers.CreateString("testsettings.button.description", "This is a button"),
              tooltip: Helpers.CreateString("testsettings.button.tooltip", "This is a button that has a bunch more text about it than the main view"),
              buttonText: Helpers.CreateString("testsettings.button.text", "Click Me!"),
              onClick: OnClick))
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

    private void OnClick()
    {
      Main.Logger.Log("Button was clicked");
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

    internal static Sprite CreateSprite()
    {
      var assembly = Assembly.GetExecutingAssembly();
      var embeddedImage = "ModMenu.WittleWolfie.png";
      using var stream = assembly.GetManifestResourceStream(embeddedImage);
      byte[] bytes = new byte[stream.Length];
      stream.Read(bytes, 0, bytes.Length);
      var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
      _ = texture.LoadImage(bytes);
      var sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), Vector2.zero);
      return sprite;
    }
  }
#endif
}
