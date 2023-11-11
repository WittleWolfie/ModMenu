using JetBrains.Annotations;
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
      var result = settings.Build();
      foreach (var setting in result.settings)
      {
        if (Settings.ContainsKey(setting.Key))
        {
          throw new ArgumentException(
            $"Attempt to add settings failed: a setting with key {setting.Key} already exists.");
        }
        Settings.Add(setting.Key, setting.Value);
      }
      ModsMenuEntity.Add(result.info, result.groups);
    }

    /// <summary>
    /// Adds a new entry containing the group of settings to the Mods menu dropdown. 
    /// Group's title will be displayed instead of the mod name.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    /// Using <see cref="AddSettings(SettingsBuilder)"/> is recommended. If you prefer to construct the settings
    /// on your own you can use this method, but better choose a less deprecated overload using ModsMenuEntry.
    /// The name of the settings group will be used as the display name for the mod in the selection dropdown .
    /// </para>
    /// 
    /// <para>
    /// Settings added in this way cannot be retrieved using <see cref="GetSetting{T, TValue}(string)"/> or
    /// <see cref="GetSettingValue{T}(string)"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// settingGroups argument must not be null, have at least 1 group in it and none can be null
    /// </exception> 
    [Obsolete("Please, use AddSettings(ModsMenuEntry modInfo) instead")]
    public static void AddSettings([NotNull] UISettingsGroup settingsGroup) 
      => ModsMenuEntity.Add(default, new UISettingsGroup[1] { settingsGroup });

    /// <summary>
    /// Adds a new entry containing groups of settings to the Mods menu dropdown. 
    /// The title of the first group will displayed instead of the mod name
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    /// Using <see cref="AddSettings(SettingsBuilder)"/> is recommended. If you prefer to construct the settings
    /// on your own you can use this method, but better choose a less deprecated overload using ModsMenuEntry.
    /// The name of the first settings group from the list will be used as the display name for the mod in the selection dropdown .
    /// </para>
    /// 
    /// <para>
    /// Settings added in this way cannot be retrieved using <see cref="GetSetting{T, TValue}(string)"/> or
    /// <see cref="GetSettingValue{T}(string)"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// settingGroups argument must not be null, have at least 1 group in it and none can be null
    /// </exception> 
    [Obsolete("Please, use AddSettings(ModsMenuEntry modInfo) instead")]
    public static void AddSettings([NotNull] List<UISettingsGroup> settingsGroups)
      => ModsMenuEntity.Add(default, settingsGroups);

    /// <summary>
    /// Adds a new entry containing groups of settings to the Mods menu page. 
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    /// Use this method if you prefer to construct settings on your own; you can use this method
    /// instead of using the recommended <see cref="AddSettings(SettingsBuilder)"/>. 
    /// </para>
    /// 
    /// <param name="modInfo"> Structure containing basic info about the mod - name, version, description, author name et cetera. 
    /// This information will be displayed when user is choosing the mod from the dropdown
    /// </param>
    /// <param name="group"> Group of settings created with original Owlcat API. If you did not set any mod name inside the modInfo structure,
    /// name of this settings group will be instead used as the display name of the mod.
    /// </param>
    /// 
    /// <para>
    /// Settings added in this way cannot be retrieved using <see cref="GetSetting{T, TValue}(string)"/> or
    /// <see cref="GetSettingValue{T}(string)"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// settingGroups argument must not be null, have at least 1 group in it and none can be null
    /// </exception> 
    /// 
    public static void AddSettings(Info modInfo, [NotNull] UISettingsGroup group) 
      => ModsMenuEntity.Add(modInfo, new UISettingsGroup[1] { group });

    /// <summary>
    /// Adds a new entry containing groups of settings to the Mods menu page. 
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    /// Use this method if you prefer to construct settings on your own; you can use this method
    /// instead of using the recommended <see cref="AddSettings(SettingsBuilder)"/>. 
    /// 
    /// Settings added in this way cannot be retrieved using <see cref="GetSetting{T, TValue}(string)"/> or
    /// <see cref="GetSettingValue{T}(string)"/>.
    /// </para>
    /// </remarks>
    /// 
    /// <param name="modInfo"> Structure containing basic info about the mod - name, version, description, author name et cetera. 
    /// This information will be displayed when user is choosing the mod from the dropdown
    /// </param>param>
    /// <param name="groups"> Groups of settings created with original Owlcat API. If you did not set any mod name inside the modInfo structure,
    /// name of the first settings group from the list will be instead used as the display name of the mod.
    /// </param>param>
    /// <exception cref="ArgumentException">
    /// settingGroups argument must not be null, have at least 1 group in it and none can be null
    /// </exception> 
    public static void AddSettings(Info modInfo, [NotNull] List<UISettingsGroup> groups) 
      => ModsMenuEntity.Add(modInfo, groups);

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
        Main.Logger.Error($"Type mismatch. Setting {key} is a {Settings[key].GetType()}, but {typeof(T)} was expected.");
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
  }
}
