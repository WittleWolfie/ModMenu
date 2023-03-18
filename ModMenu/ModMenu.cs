using HarmonyLib;
using Kingmaker.Settings;
using Kingmaker.UI;
using Kingmaker.UI.SettingsUI;
using ModMenu.Settings;
using ModMenu.Window;
using System;
using System.Collections.Generic;

namespace ModMenu
{
  /// <summary>
  /// API mods can use to add settings to the Mods menu page.
  /// </summary>
  public static class ModMenu
  {
    #region Settings
    /// <summary>
    /// Stores all settings entities in the mod menu.
    /// </summary>
    private readonly static Dictionary<string, ISettingsEntity> Settings = new();

    /// <summary>
    /// Adds a new group of settings to the Mods menu page.
    /// </summary>
    /// 
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="settings"/> contains a setting with a key that already exists.
    /// </exception>
    public static void AddSettings(SettingsBuilder settings)
    {
      var settingsGroup = settings.Build();
      foreach (var setting in settingsGroup.settings)
      {
        if (Settings.ContainsKey(setting.Key))
        {
          throw new ArgumentException(
            $"Attempt to add settings failed: a setting with key {setting.Key} already exists.");
        }
        Settings.Add(setting.Key, setting.Value);
      }
      ModsMenuEntity.Add(settingsGroup.group);
    }

    /// <summary>
    /// Adds a new group of settings to the Mods menu page.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    /// Using <see cref="AddSettings(SettingsBuilder)"/> is recommended. If you prefer to construct the settings
    /// on your own you can use this method.
    /// </para>
    /// 
    /// <para>
    /// Settings added in this way cannot be retrieved using <see cref="GetSetting{T, TValue}(string)"/> or
    /// <see cref="GetSettingValue{T}(string)"/>.
    /// </para>
    /// </remarks>
    public static void AddSettings(UISettingsGroup settingsGroup)
    {
      ModsMenuEntity.Add(settingsGroup);
    }

    /// <returns>
    /// The setting with the specified <paramref name="key"/>, or null if it does not exist or has the wrong type.
    /// </returns>
    public static T GetSetting<T, TValue>(string key) where T : SettingsEntity<TValue>
    {
      if (!Settings.ContainsKey(key))
      {
        Main.Logger.Error($"No setting found with key {key}");
        return null;
      }

      var setting = Settings[key] as T;
      if (setting is null)
      {
        Main.Logger.Error($"Type mismatch. Setting {key} is a {setting.GetType()}, but {typeof(T)} was expected.");
        return null;
      }
      return setting;
    }

    /// <returns>
    /// The value of the setting with the specified <paramref name="key"/>, or <c>default</c> if it does not exist or
    /// has the wrong type.
    /// </returns>
    public static T GetSettingValue<T>(string key)
    {
      var setting = GetSetting<SettingsEntity<T>, T>(key);
      return setting is null ? default : setting.GetValue();
    }

    /// <summary>
    /// Attempts to set the value of a setting.
    /// </summary>
    /// 
    /// <remarks>Added in v1.1.0</remarks>
    /// 
    /// <returns>True if the setting was set, false otherwise.</returns>
    public static bool SetSetting<T, TValue>(string key, TValue value) where T : SettingsEntity<TValue>
    {
      Main.Logger.Log($"Attempting to set {key} to {value}");
      var setting = GetSetting<T, TValue>(key);
      if (setting is null)
        return false;

      setting.SetValueAndConfirm(value);
      return true;
    }

    /// <summary>
    /// Convenience method for <see cref="SetSetting{T, TValue}(string, TValue)"/> with
    /// <c>&lt;SettingsEntityBool, bool&gt;</c>.
    /// </summary>
    /// 
    /// <inheritdoc cref="SetSetting{T, TValue}(string, TValue)"/>
    public static bool SetSetting(string key, bool value)
    {
      return SetSetting<SettingsEntityBool, bool>(key, value);
    }

    /// <summary>
    /// Convenience method for <see cref="SetSetting{T, TValue}(string, TValue)"/> with
    /// <c>&lt;SettingsEntityEnum&lt;T&gt;, T&gt;</c>.
    /// </summary>
    /// 
    /// <inheritdoc cref="SetSetting{T, TValue}(string, TValue)"/>
    public static bool SetSetting<T>(string key, T value) where T : Enum
    {
      return SetSetting<SettingsEntityEnum<T>, T>(key, value);
    }

    /// <summary>
    /// Convenience method for <see cref="SetSetting{T, TValue}(string, TValue)"/> with
    /// <c>&lt;SettingsEntityFloat, float&gt;</c>.
    /// </summary>
    /// 
    /// <inheritdoc cref="SetSetting{T, TValue}(string, TValue)"/>
    public static bool SetSetting(string key, float value)
    {
      return SetSetting<SettingsEntityFloat, float>(key, value);
    }

    /// <summary>
    /// Convenience method for <see cref="SetSetting{T, TValue}(string, TValue)"/> with
    /// <c>&lt;SettingsEntityInt, int&gt;</c>.
    /// </summary>
    /// 
    /// <inheritdoc cref="SetSetting{T, TValue}(string, TValue)"/>
    public static bool SetSetting(string key, int value)
    {
      return SetSetting<SettingsEntityInt, int>(key, value);
    }
    #endregion

    #region Windows
    /// <summary>
    /// Stores all windows by key.
    /// </summary>
    private static readonly Dictionary<string, WindowBuilder> Windows = new();

    /// <summary>
    /// Adds a new window.
    /// </summary>
    /// 
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="window"/> uses a key that already exists.
    /// </exception>
    public static void AddWindow(WindowBuilder window)
    {
      if (Windows.ContainsKey(window.Key))
        throw new ArgumentException($"Attempt to add window failed: a window with key {window.Key} already exists.");
      Windows.Add(window.Key, window);
    }

    /// <summary>
    /// Opens the full screen window with the specified <paramref name="key"/>
    /// </summary>
    /// 
    /// <exception cref="ArgumentException">
    /// Thrown when a window does not exist for <paramref name="key"/>
    /// </exception>
    public static void ShowWindow(string key)
    {
      if (Windows.ContainsKey(key))
        throw new ArgumentException($"No window with key {key} exists.");
      WindowView.ShowWindow(Windows[key]);
    }
    #endregion
  }
}
