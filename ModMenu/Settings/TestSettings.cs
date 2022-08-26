using Kingmaker.Localization;
using Kingmaker.UI.SettingsUI;
using System.Reflection;
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
    private static string RootKey = "mod-menu.test-settings";
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
          .AddImage(CreateSprite())
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
              .DependsOnSave())
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

    private bool CheckToggle()
    {
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

    private static Sprite CreateSprite()
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
