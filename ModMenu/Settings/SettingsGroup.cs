using Kingmaker.Localization;
using Kingmaker.Settings;
using Kingmaker.UI.SettingsUI;
using ModMenu.NewTypes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModMenu.Settings
{
  // TODO: Create a settings object, probably add builders per setting type, and then don't bother with the out or
  // returning the settings, just handle the settings internally. i.e. SettingsGroup.Build() returns Settings, then 
  // Settings[key] or Settings.GetBool(key) returns the value for that particular one. Maybe even expose the handlers?

  /// <summary>
  /// Represents a group of settings on the Mods menu page, constructed using a builder API.
  /// </summary>
  /// 
  /// <remarks>
  /// <para>
  /// All the <c>AddX</c> methods return <c>this</c> to support builder style method chaining. Once your SettingsGroup
  /// is configured add it to the Mods menu page by calling <see cref="ModMenu.AddSettings(SettingsGroup)"/>.
  /// </para>
  /// 
  /// <para>
  /// Entries are displayed in the order they are added.
  /// </para>
  /// 
  /// <para>
  /// Creats a setting group with a single feature toggle:
  /// </para>
  /// <example>
  /// <code>
  /// ModMenu.AddSettings(
  ///   SettingsGroup.New("mymod.settingsgroup", MySettingsGroupTitle)
  ///     .AddImage(MyModBanner)
  ///     .AddToggle(
  ///       new(
  ///         "mymod.feature.toggle",
  ///         defaultValue: false,
  ///         MyFeatureToggleDescription)));
  /// </code>
  /// </example>
  /// 
  /// <para>
  /// To actually use the settings values you must either handle <c>OnValueChanged</c> events which you can do by
  /// passing in a <see cref="Setting{T}"/> with <c>onValueChanged</c> specified, or by storing the
  /// <c>SettingsEntity</c>:
  /// </para>
  /// <example>
  /// <code>
  /// SettingsEntity&lt;bool&gt; featureToggle;
  /// SettingsGroup.New("mymod.settingsgroup", MySettingsGroupTitle)
  ///   .AddToggle(
  ///     new(
  ///       "mymod.feature.toggle.using.event",
  ///       defaultValue: false,
  ///       MyFeatureToggleDescription,
  ///       // When toggled this calls HandleMyFeatureToggle(value) where value is the new setting.
  ///       onValueChanged: value => HandleMyFeatureToggle(value)))
  ///   .AddToggle(
  ///     new(
  ///       "mymod.feature.toggle.using.entity",
  ///       defaultValue: false,
  ///       MyFeatureToggleDescription),
  ///     // When toggled featureToggle updates its value which can be retrieved by calling featureToggle.GetValue()
  ///     out featureToggle));
  /// </code>
  /// </example>
  /// </remarks>
  public class SettingsGroup
  {
    private readonly UISettingsGroup Group = ScriptableObject.CreateInstance<UISettingsGroup>();
    private readonly List<UISettingsEntityBase> Settings = new();

    /// <param name="key">Unique key / name for the settings group. Use only lowercase letters and '.'</param>
    /// <param name="title">Title of the group, displayed on the settings page</param>
    public SettingsGroup(string key, LocalizedString title)
    {
      Group.name = key.ToLower();
      Group.Title = title;
    }

    /// <inheritdoc cref="SettingsGroup(string, LocalizedString)"/>
    public static SettingsGroup New(string key, LocalizedString title)
    {
      return new(key, title);
    }

    /// <summary>
    /// Adds an On / Off setting toggle.
    /// </summary>
    public SettingsGroup AddToggle(Setting<bool> setting)
    {
      return AddToggle(setting, out _);
    }

    /// <inheritdoc cref="AddToggle(Setting{bool})"/>
    /// <param name="settingValue">
    /// Use this parameter to store the resulting <c>SettingsEntityBool</c> which contains the setting value.
    /// </param>
    public SettingsGroup AddToggle(Setting<bool> setting, out SettingsEntity<bool> settingValue)
    {
      settingValue =
        new SettingsEntityBool(
          setting.Key,
          setting.DefaultValue,
          saveDependent: setting.SaveDependent,
          requireReboot: setting.RequireReboot);

      if (setting.OnValueChanged is not null)
      {
        (settingValue as IReadOnlySettingEntity<bool>).OnValueChanged += setting.OnValueChanged;
      }
      if (setting.OnTempValueChanged is not null)
      {
        (settingValue as IReadOnlySettingEntity<bool>).OnTempValueChanged += setting.OnTempValueChanged;
      }

      var toggle = ScriptableObject.CreateInstance<UISettingsEntityBool>();
      toggle.m_Description = setting.Description;
      toggle.m_TooltipDescription = setting.DescriptionLong;
      toggle.DefaultValue = setting.DefaultValue;
      toggle.LinkSetting(settingValue);
      Settings.Add(toggle);

      return this;
    }

    /// <summary>
    /// Adds a dropdown setting populated using an enum.
    /// </summary>
    /// 
    /// <remarks>
    /// Due to Unity limitations you need to create <paramref name="dropdown"/> yourself:
    /// 
    /// <example>
    /// <code>
    /// public enum MySettingsEnum { /* ... */ }
    /// // Declare a non-generic class which inherits from the generic type
    /// private class UISettingsEntityDropdownMySettingsEnum : UISettingsEntityDropdownEnum&lt;MysettingsEnum&gt; { }
    /// 
    /// mySettingsGroup.AddDropdown(
    ///   new(
    ///     "mymod.feature.enum",
    ///     defaultValue: MySettingsEnum.SomeValue,
    ///     MyEnumFeatureDescription,
    ///     onValueChanged: value => OnDropdownSelected(value)),
    ///   ScriptableObject.CreateInstance&lt;UISettingsEntityDropdownMySettingsEnum&gt;());
    /// </code>
    /// </example>
    /// </remarks>
    /// 
    /// <typeparam name="T">Enum used to populate values</typeparam>
    /// <param name="dropdown">
    /// Instance of class inheriting from <c>UISettingsEntityDropdownEnum&lt;TEnum&gt;</c>, created by calling
    /// <c>ScriptableObject.CreateInstance&lt;T&gt;()</c></param>
    public SettingsGroup AddDropdown<T>(
      Setting<T> setting,
      UISettingsEntityDropdownEnum<T> dropdown) where T : Enum
    {
      return AddDropdown<T>(setting, dropdown, out _);
    }

    /// <inheritdoc cref="AddDropdown{T}(Setting{T}, UISettingsEntityDropdownEnum{T})"/>
    /// <param name="settingValue">
    /// Use this parameter to store the resulting <c>SettingsEntityEnum</c> which contains the setting value.
    /// </param>
    public SettingsGroup AddDropdown<T>(
      Setting<T> setting,
      UISettingsEntityDropdownEnum<T> dropdown,
      out SettingsEntityEnum<T> settingValue) where T : Enum
    {
      settingValue =
        new SettingsEntityEnum<T>(
          setting.Key,
          setting.DefaultValue,
          saveDependent: setting.SaveDependent,
          requireReboot: setting.RequireReboot);

      if (setting.OnValueChanged is not null)
      {
        (settingValue as IReadOnlySettingEntity<T>).OnValueChanged += setting.OnValueChanged;
      }
      if (setting.OnTempValueChanged is not null)
      {
        (settingValue as IReadOnlySettingEntity<T>).OnTempValueChanged += setting.OnTempValueChanged;
      }

      dropdown.m_Description = setting.Description;
      dropdown.m_TooltipDescription = setting.DescriptionLong;
      dropdown.m_CashedLocalizedValues ??= new();
      foreach (var value in Enum.GetValues(typeof(T)))
      {
        dropdown.m_CashedLocalizedValues.Add(value.ToString());
      }
      dropdown.LinkSetting(settingValue);
      Settings.Add(dropdown);

      return this;
    }

    /// <summary>
    /// Adds a slider based on a float.
    /// </summary>
    /// <param name="decimalPlaces">Number of decimal places to use, e.g. 1 would show 2.1 while 2 might show 2.15</param>
    /// <param name="showValueText">Whether the slider shows the current value on it</param>
    public SettingsGroup AddSliderFloat(
      Setting<float> setting,
      float minValue,
      float maxValue,
      float step = 0.1f,
      int decimalPlaces = 1,
      bool showValueText = true)
    {
      return AddSliderFloat(setting, minValue, maxValue, out _, step, decimalPlaces, showValueText);
    }

    /// <inheritdoc cref="AddSliderFloat(Setting{float}, float, float, float, int, bool)"/>
    /// <param name="settingValue">
    /// Use this parameter to store the resulting <c>SettingsEntityFloat</c> which contains the setting value.
    /// </param>
    public SettingsGroup AddSliderFloat(
      Setting<float> setting,
      float minValue,
      float maxValue,
      out SettingsEntityFloat settingValue,
      float step = 0.1f,
      int decimalPlaces = 1,
      bool showValueText = true)
    {
      settingValue =
        new SettingsEntityFloat(
          setting.Key,
          setting.DefaultValue,
          saveDependent: setting.SaveDependent,
          requireReboot: setting.RequireReboot);

      if (setting.OnValueChanged is not null)
      {
        (settingValue as IReadOnlySettingEntity<float>).OnValueChanged += setting.OnValueChanged;
      }
      if (setting.OnTempValueChanged is not null)
      {
        (settingValue as IReadOnlySettingEntity<float>).OnTempValueChanged += setting.OnTempValueChanged;
      }

      var slider = ScriptableObject.CreateInstance<UISettingsEntitySliderFloat>();
      slider.m_Description = setting.Description;
      slider.m_TooltipDescription = setting.DescriptionLong;
      slider.m_MinValue = minValue;
      slider.m_MaxValue = maxValue;
      slider.m_Step = step;
      slider.m_DecimalPlaces = decimalPlaces;
      slider.m_ShowValueText = showValueText;
      slider.LinkSetting(settingValue);
      Settings.Add(slider);

      return this;
    }

    /// <summary>
    /// Adds a slider based on an int.
    /// </summary>
    /// <param name="showValueText">Whether the slider shows the current value on it</param>
    public SettingsGroup AddSliderInt(
      Setting<int> setting,
      int minValue,
      int maxValue,
      bool showValueText = true)
    {
      return AddSliderInt(setting, minValue, maxValue, out _, showValueText);
    }

    /// <inheritdoc cref="AddSliderInt(Setting{int}, int, int, bool)"/>
    /// <param name="settingValue">
    /// Use this parameter to store the resulting <c>SettingsEntityInt</c> which contains the setting value.
    /// </param>
    public SettingsGroup AddSliderInt(
      Setting<int> setting,
      int minValue,
      int maxValue,
      out SettingsEntityInt settingValue,
      bool showValueText = true)
    {
      settingValue =
        new SettingsEntityInt(
          setting.Key,
          setting.DefaultValue,
          saveDependent: setting.SaveDependent,
          requireReboot: setting.RequireReboot);

      if (setting.OnValueChanged is not null)
      {
        (settingValue as IReadOnlySettingEntity<int>).OnValueChanged += setting.OnValueChanged;
      }
      if (setting.OnTempValueChanged is not null)
      {
        (settingValue as IReadOnlySettingEntity<int>).OnTempValueChanged += setting.OnTempValueChanged;
      }

      var slider = ScriptableObject.CreateInstance<UISettingsEntitySliderInt>();
      slider.m_Description = setting.Description;
      slider.m_TooltipDescription = setting.DescriptionLong;
      slider.m_MinValue = minValue;
      slider.m_MaxValue = maxValue;
      slider.m_ShowValueText = showValueText;
      slider.LinkSetting(settingValue);
      Settings.Add(slider);

      return this;
    }

    /// <summary>
    /// Adds a row contained of just an image. There is no setting tied to this, it is just for decoration.
    /// </summary>
    public SettingsGroup AddImage(Sprite sprite)
    {
      var image = ScriptableObject.CreateInstance<UISettingsEntityImage>();
      image.Sprite = sprite;
      Settings.Add(image);

      return this;
    }

    /// <summary>
    /// Adds any arbitrary setting. Use for settings you construct yourself.
    /// </summary>
    public SettingsGroup AddSetting(UISettingsEntityBase setting)
    {
      Settings.Add(setting);
      return this;
    }

    internal UISettingsGroup Build()
    {
      Group.SettingsList = Settings.ToArray();
      return Group;
    }

    internal SettingsGroup AddButton(UISettingsEntityButton button)
    {
      Settings.Add(button);
      return this;
    }

    /// <summary>
    /// Common params for creating (most) settings types.
    /// </summary>
    public class Setting<T>
    {
      internal readonly string Key;
      internal readonly T DefaultValue;
      internal readonly LocalizedString Description;
      internal readonly LocalizedString DescriptionLong;
      internal readonly bool SaveDependent;
      internal readonly bool RequireReboot;
      internal readonly Action<T> OnValueChanged;
      internal readonly Action<T> OnTempValueChanged;

      /// <param name="key">Unique key for the setting. Limited to lowercase letters and periods.</param>
      /// <param name="description">Description displayed on the page for the setting.</param>
      /// <param name="descriptionLong">Long description shown on the right panel when a setting is selected. Defaults to description.</param>
      /// <param name="saveDependent">Whether the setting is tied to the current save or applied globally.</param>
      /// <param name="requireReboot">Whether the setting requires a reboot to take effect.</param>
      /// <param name="onValueChanged">Called when the user confirms a change to the setting, with the new value as the parameter.</param>
      /// <param name="onTempValueChanged">Called when the user changes the settings before confirmation, with the value as the parameter.</param>
      public Setting(
        string key,
        T defaultValue,
        LocalizedString description,
        LocalizedString descriptionLong = null,
        bool saveDependent = false,
        bool requireReboot = false,
        Action<T> onValueChanged = null,
        Action<T> onTempValueChanged = null)
      {
        Key = key.ToLower();
        DefaultValue = defaultValue;
        Description = description;
        DescriptionLong = descriptionLong ?? description;
        SaveDependent = saveDependent;
        RequireReboot = requireReboot;
        OnValueChanged = onValueChanged;
        OnTempValueChanged = onTempValueChanged;
      }
    }
  }
}
