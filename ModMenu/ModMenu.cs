using Kingmaker.Settings;
using Kingmaker.UI.SettingsUI;
using ModMenu.Settings;
using System;
using System.Collections.Generic;

namespace ModMenu
{
  /// <summary>
  /// API mods can use to add settings to the Mods menu page.
  /// </summary>
  public static class ModMenu
  {
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
  }
}
