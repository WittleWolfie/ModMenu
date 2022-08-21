using Kingmaker.Localization;
using Kingmaker.Settings;
using Kingmaker.UI.SettingsUI;
using ModMenu.NewTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace ModMenu.Settings
{
  /// <summary>
  /// Represents a group of settings on the Mods menu page.
  /// </summary>
  public class SettingsGroup
  {
    private readonly UISettingsGroup Group = ScriptableObject.CreateInstance<UISettingsGroup>();
    private readonly List<UISettingsEntityBase> Settings = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="title"></param>
    public SettingsGroup(string key, LocalizedString title)
    {
      Group.name = key.ToLower();
      Group.Title = title;
    }

    public SettingsGroup AddToggle(Setting<bool> setting)
    {
      return AddToggle(setting, out _);
    }

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

    public SettingsGroup AddDropdown<T>(
      Setting<T> setting,
      UISettingsEntityDropdownEnum<T> dropdown) where T : Enum
    {
      return AddDropdown<T>(setting, dropdown, out _);
    }

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

    public SettingsGroup AddSliderInt(
      Setting<int> setting,
      int minValue,
      int maxValue,
      bool showValueText = true)
    {
      return AddSliderInt(setting, minValue, maxValue, out _, showValueText);
    }

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
