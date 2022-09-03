using Kingmaker.Localization;
using Kingmaker.Settings;
using Kingmaker.UI.SettingsUI;
using ModMenu.NewTypes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModMenu.Settings
{
  /// <summary>
  /// Builder API for constructing <see cref="SettingsGroup"/>.
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
  public class SettingsBuilder
  {
    private readonly UISettingsGroup Group = ScriptableObject.CreateInstance<UISettingsGroup>();
    private readonly List<UISettingsEntityBase> Settings = new();
    private readonly Dictionary<string, ISettingsEntity> SettingsEntities = new();

    /// <param name="key">
    /// Globally unique key / name for the settings group. Use only lowercase letters, numbers, '-', and '.'
    /// </param>
    /// <param name="title">Title of the settings group, displayed on the settings page</param>
    public static SettingsBuilder New(string key, LocalizedString title)
    {
      return new(key, title);
    }

    /// <summary>
    /// Adds a row containing an image. There is no setting tied to this, it is just for decoration.
    /// </summary>
    /// 
    /// <param name="height">
    /// Sets the row height. Keep in mind the scaling is relative to resolution; a standard row has a height of 40. The
    /// image width will be scaled to preserve the aspect ratio.
    /// </param>
    public SettingsBuilder AddImage(Sprite sprite, int height)
    {
      return AddImageInternal(sprite, height);
    }

    /// <inheritdoc cref="AddImage(Sprite, int)"/>
    public SettingsBuilder AddImage(Sprite sprite)
    {
      return AddImageInternal(sprite);
    }

    private SettingsBuilder AddImageInternal(Sprite sprite, int height = -1)
    {
      var image = new UISettingsEntityImage(sprite, height);
      Settings.Add(image);
      return this;
    }

    /// <summary>
    /// Adds a row containing a button. There is no setting tied to this, only an event handler.
    /// </summary>
    public SettingsBuilder AddButton(Button button)
    {
      var uiEntity = button.Build();
      Settings.Add(uiEntity);
      return this;
    }

    /// <summary>
    /// Adds an On / Off setting toggle.
    /// </summary>
    public SettingsBuilder AddToggle(Toggle toggle)
    {
      var (entity, uiEntity) = toggle.Build();
      return Add(entity.Key, entity, uiEntity);
    }

    /// <summary>
    /// Adds a dropdown setting populated using an enum.
    /// </summary>
    public SettingsBuilder AddDropdown<T>(Dropdown<T> dropdown) where T : Enum
    {
      var (entity, uiEntity) = dropdown.Build();
      return Add(entity.Key, entity, uiEntity);
    }

    /// <summary>
    /// Adds a slider based on a float.
    /// </summary>
    public SettingsBuilder AddSliderFloat(SliderFloat sliderFloat)
    {
      var (entity, uiEntity) = sliderFloat.Build();
      return Add(entity.Key, entity, uiEntity);
    }

    /// <summary>
    /// Adds a slider based on an int.
    /// </summary>
    public SettingsBuilder AddSliderInt(SliderInt sliderInt)
    {
      var (entity, uiEntity) = sliderInt.Build();
      return Add(entity.Key, entity, uiEntity);
    }

    /// <summary>
    /// Use for settings you construct on your own.
    /// </summary>
    /// 
    /// <remarks>
    /// Note that settings added this way cannot be retrieved using <see cref="ModMenu.GetSetting{T, TValue}(string)"/>
    /// or <see cref="ModMenu.GetSettingValue{T}(string)"/>.
    /// </remarks>
    public SettingsBuilder AddSetting(UISettingsEntityBase setting)
    {
      Settings.Add(setting);
      return this;
    }

    internal (UISettingsGroup group, Dictionary<string, ISettingsEntity> settings) Build()
    {
      Group.SettingsList = Settings.ToArray();
      return (Group, SettingsEntities);
    }

    private SettingsBuilder Add(string key, ISettingsEntity entity, UISettingsEntityBase uiEntity)
    {
      SettingsEntities.Add(key, entity);
      Settings.Add(uiEntity);
      return this;
    }

    public SettingsBuilder(string key, LocalizedString title)
    {
      Group.name = key.ToLower();
      Group.Title = title;
      Settings.Add(new UISettingsEntityHeaderButton());
    }
  }

  public abstract class BaseSetting<TUIEntity, TBuilder>
    where TUIEntity : UISettingsEntityBase
    where TBuilder : BaseSetting<TUIEntity, TBuilder>
  {
    protected readonly TBuilder Self;

    protected readonly LocalizedString Description;

    protected LocalizedString LongDescription;
    protected bool VisualConnection = false;

    /// <param name="description">Short description displayed on the setting row.</param>
    protected BaseSetting(LocalizedString description)
    {
      Self = (TBuilder)this;
      Description = description;
      LongDescription = description;
    }

    /// <summary>
    /// Changes the setting bullet point to a visual connection line. See the game's visual perception settings for an
    /// example.
    /// </summary>
    public TBuilder ShowVisualConnection()
    {
      VisualConnection = true;
      return Self;
    }

    /// <summary>
    /// Sets the long description displayed on the right side of the menu when the setting is highlighted.
    /// </summary>
    /// 
    /// <remarks>
    /// This sets <c>UISettingsEntityBase.TooltipDescription</c>. When not specified, Description is used.
    /// </remarks>
    public TBuilder WithLongDescription(LocalizedString longDescription)
    {
      LongDescription = longDescription;
      return Self;
    }
  }

  public abstract class BaseSettingBuilder<TUIEntity, TBuilder>
    : BaseSetting<TUIEntity, TBuilder>
    where TUIEntity : UISettingsEntityBase
    where TBuilder : BaseSettingBuilder<TUIEntity, TBuilder>
  {
    private TUIEntity UIEntity;

    /// <inheritdoc cref="BaseSetting{TUIEntity, TBuilder}.BaseSetting(LocalizedString)"/>
    protected BaseSettingBuilder(LocalizedString description) : base(description) { }

    public TUIEntity Build()
    {
      UIEntity ??= CreateUIEntity();
      UIEntity.m_ShowVisualConnection = VisualConnection;
      return UIEntity;
    }
    protected abstract TUIEntity CreateUIEntity();
  }

  public class Button : BaseSettingBuilder<UISettingsEntityButton, Button>
  {
    private readonly LocalizedString ButtonText;
    private readonly Action OnClick;

    /// <inheritdoc cref="Button(LocalizedString, LocalizedString, Action)"/>
    public static Button New(LocalizedString description, LocalizedString buttonText, Action onClick)
    {
      return new(description, buttonText, onClick);
    }

    protected override UISettingsEntityButton CreateUIEntity()
    {
      return new(Description, LongDescription, ButtonText, OnClick);
    }

    /// <inheritdoc cref="BaseSettingBuilder{TUIEntity, TBuilder}.BaseSettingBuilder(LocalizedString)"/>
    /// <param name="buttonText">Text displayed on the button</param>
    /// <param name="onClick">Action invoked when the button is clicked</param>
    public Button(LocalizedString description, LocalizedString buttonText, Action onClick) : base(description)
    {
      ButtonText = buttonText;
      OnClick = onClick;
    }
  }

  public abstract class BaseSettingWithValue<T, TEntity, TUIEntity, TBuilder>
    : BaseSetting<TUIEntity, TBuilder>
    where TEntity : SettingsEntity<T>
    where TUIEntity : UISettingsEntityWithValueBase<T>
    where TBuilder : BaseSettingWithValue<T, TEntity, TUIEntity, TBuilder>
  {
    private TEntity Entity;
    private TUIEntity UIEntity;

    protected readonly string Key;
    protected readonly T DefaultValue;
    /// <summary>
    /// Currently this is unused but I might add some kind of special handling later so the code is here.
    /// </summary>
    protected readonly bool RebootRequired = false;

    protected bool SaveDependent;
    protected Action<T> ValueChanged;
    protected Action<T> TempValueChanged;
    protected Func<bool> ModificationAllowed;

    /// <inheritdoc cref="BaseSetting{TUIEntity, TBuilder}.BaseSetting(LocalizedString)"/>
    /// <param name="key">
    /// Globally unique key / name for the setting. Use only lowercase letters, numbers, '-', and '.'
    /// </param>
    /// <param name="defaultValue">Default value for the setting.</param>
    protected BaseSettingWithValue(string key, T defaultValue, LocalizedString description) : base(description)
    {
      Key = key;
      DefaultValue = defaultValue;
    }

    /// <summary>
    /// Causes the setting to be associated with the current save. By default settings apply globally.
    /// </summary>
    public TBuilder DependsOnSave()
    {
      SaveDependent = true;
      return Self;
    }

    /// <summary>
    /// Invokes the provided action when the value is changed and applied.
    /// </summary>
    public TBuilder OnValueChanged(Action<T> onValueChanged)
    {
      ValueChanged = onValueChanged;
      return Self;
    }

    /// <summary>
    /// Invokes the provided action when the value is changed, before the change is applied.
    /// </summary>
    public TBuilder OnTempValueChanged(Action<T> onTempValueChanged)
    {
      TempValueChanged = onTempValueChanged;
      return Self;
    }

    /// <summary>
    /// When the menu is displayed, the provided function is checked to determine if the setting can be changed.
    /// </summary>
    /// 
    /// <remarks>
    /// This is only checked when the Mods menu page is opened. As a result you cannot use this to create dependencies
    /// between settings.
    /// </remarks>
    public TBuilder IsModificationAllowed(Func<bool> isModificationAllowed)
    {
      ModificationAllowed = isModificationAllowed;
      return Self;
    }

    internal (TEntity entity, TUIEntity uiEntity) Build()
    {
      if (Entity is null || UIEntity is null)
      {
        Entity ??= CreateEntity();
        if (ValueChanged is not null)
        {
          (Entity as IReadOnlySettingEntity<T>).OnValueChanged += ValueChanged;
        }
        if (TempValueChanged is not null)
        {
          (Entity as IReadOnlySettingEntity<T>).OnTempValueChanged += TempValueChanged;
        }

        UIEntity ??= CreateUIEntity();
        UIEntity.m_Description = Description;
        UIEntity.m_TooltipDescription = LongDescription;
        UIEntity.ModificationAllowedCheck = ModificationAllowed;
        UIEntity.m_ShowVisualConnection = VisualConnection;
        UIEntity.LinkSetting(Entity);
      }
      return (Entity, UIEntity);
    }
    protected abstract TEntity CreateEntity();
    protected abstract TUIEntity CreateUIEntity();
  }

  public class Toggle : BaseSettingWithValue<bool, SettingsEntityBool, UISettingsEntityBool, Toggle>
  {
    /// <inheritdoc cref="Toggle(string, bool, LocalizedString)"/>
    public static Toggle New(string key, bool defaultValue, LocalizedString description)
    {
      return new(key, defaultValue, description);
    }

    protected override SettingsEntityBool CreateEntity()
    {
      return new SettingsEntityBool(Key, DefaultValue, SaveDependent, RebootRequired);
    }

    protected override UISettingsEntityBool CreateUIEntity()
    {
      var uiEntity = ScriptableObject.CreateInstance<UISettingsEntityBool>();
      uiEntity.DefaultValue = DefaultValue;
      return uiEntity;
    }

    /// <inheritdoc cref="BaseSettingWithValue{T, TEntity, TUIEntity, TBuilder}.BaseSettingWithValue(string, T, LocalizedString)"/>
    public Toggle(string key, bool defaultValue, LocalizedString description)
      : base(key, defaultValue, description) { }
  }

  public class Dropdown<T>
    : BaseSettingWithValue<T, SettingsEntityEnum<T>, UISettingsEntityDropdownEnum<T>, Dropdown<T>>
    where T : Enum
  {
    private readonly UISettingsEntityDropdownEnum<T> DropdownEntity;

    /// <inheritdoc cref="Dropdown{T}.Dropdown(string, T, LocalizedString, UISettingsEntityDropdownEnum{T})"/>
    public static Dropdown<T> New(
      string key, T defaultValue, LocalizedString description, UISettingsEntityDropdownEnum<T> dropdown)
    {
      return new(key, defaultValue, description, dropdown);
    }

    protected override SettingsEntityEnum<T> CreateEntity()
    {
      return new SettingsEntityEnum<T>(Key, DefaultValue, SaveDependent, RebootRequired);
    }

    protected override UISettingsEntityDropdownEnum<T> CreateUIEntity()
    {
      var values = new List<string>();
      foreach (var value in Enum.GetValues(typeof(T)))
      {
        values.Add(value.ToString());
      }
      DropdownEntity.m_CashedLocalizedValues = values;
      return DropdownEntity;
    }

    /// <inheritdoc cref="BaseSettingWithValue{T, TEntity, TUIEntity, TBuilder}.BaseSettingWithValue(string, T, LocalizedString)"/>
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
    /// new(
    ///   "mymod.feature.enum",
    ///   MySettingsEnum.SomeValue,
    ///   MyEnumFeatureDescription,
    ///   ScriptableObject.CreateInstance&lt;UISettingsEntityDropdownMySettingsEnum&gt;());
    /// </code>
    /// </example>
    /// </remarks>
    /// 
    /// <param name="dropdown">
    /// Instance of class inheriting from <c>UISettingsEntityDropdownEnum&lt;TEnum&gt;</c>, created by calling
    /// <c>ScriptableObject.CreateInstance&lt;T&gt;()</c>
    /// </param>
    public Dropdown(
      string key, T defaultValue, LocalizedString description, UISettingsEntityDropdownEnum<T> dropdown)
      : base(key, defaultValue, description)
    {
      DropdownEntity = dropdown;
    }
  }

  public class SliderFloat
    : BaseSettingWithValue<float, SettingsEntityFloat, UISettingsEntitySliderFloat, SliderFloat>
  {
    private readonly float MinValue;
    private readonly float MaxValue;

    private float Step = 0.1f;
    private int DecimalPlaces = 1;
    private bool ShowValueText = true;

    /// <inheritdoc cref="SliderFloat(string, float, LocalizedString, float, float)"/>
    public static SliderFloat New(
      string key, float defaultValue, LocalizedString description, float minValue, float maxValue)
    {
      return new(key, defaultValue, description, minValue, maxValue);
    }

    /// <summary>
    /// Sets the size of a single step on the slider.
    /// </summary>
    public SliderFloat WithStep(float step)
    {
      Step = step;
      return this;
    }

    /// <summary>
    /// Sets the number of decimal places tracked on the slider.
    /// </summary>
    public SliderFloat WithDecimalPlaces(int decimalPlaces)
    {
      DecimalPlaces = decimalPlaces;
      return this;
    }

    /// <summary>
    /// Hides the text showing the slider value.
    /// </summary>
    public SliderFloat HideValueText()
    {
      ShowValueText = false;
      return this;
    }

    protected override SettingsEntityFloat CreateEntity()
    {
      return new SettingsEntityFloat(Key, DefaultValue, SaveDependent, RebootRequired);
    }

    protected override UISettingsEntitySliderFloat CreateUIEntity()
    {
      var uiEntity = ScriptableObject.CreateInstance<UISettingsEntitySliderFloat>();
      uiEntity.m_MinValue = MinValue;
      uiEntity.m_MaxValue = MaxValue;
      uiEntity.m_Step = Step;
      uiEntity.m_DecimalPlaces = DecimalPlaces;
      uiEntity.m_ShowValueText = ShowValueText;
      return uiEntity;
    }

    /// <inheritdoc cref="BaseSettingWithValue{T, TEntity, TUIEntity, TBuilder}.BaseSettingWithValue(string, T, LocalizedString)"/>
    /// 
    /// <remarks>
    /// <c>UISettingsEntitySliderFloat</c> Defaults:
    /// <list type="bullet">
    /// <item>
    ///   <term><c>m_Step</c></term>
    ///   <description><c>0.1f</c></description>
    /// </item>
    /// <item>
    ///   <term><c>m_DecimalPlaces</c></term>
    ///   <description><c>1</c></description>
    /// </item>
    /// <item>
    ///   <term><c>m_ShowValueText</c></term>
    ///   <description><c>true</c></description>
    /// </item>
    /// </list>
    /// </remarks>
    public SliderFloat(
      string key, float defaultValue, LocalizedString description, float minValue, float maxValue)
      : base(key, defaultValue, description)
    {
      MinValue = minValue;
      MaxValue = maxValue;
    }
  }

  public class SliderInt
    : BaseSettingWithValue<int, SettingsEntityInt, UISettingsEntitySliderInt, SliderInt>
  {
    private readonly int MinValue;
    private readonly int MaxValue;

    private bool ShowValueText = true;

    /// <inheritdoc cref="SliderInt(string, int, LocalizedString, int, int)"/>
    public static SliderInt New(
      string key, int defaultValue, LocalizedString description, int minValue, int maxValue)
    {
      return new(key, defaultValue, description, minValue, maxValue);
    }

    /// <summary>
    /// Hides the text showing the slider value.
    /// </summary>
    public SliderInt HideValueText()
    {
      ShowValueText = false;
      return this;
    }

    protected override SettingsEntityInt CreateEntity()
    {
      return new SettingsEntityInt(Key, DefaultValue, SaveDependent, RebootRequired);
    }

    protected override UISettingsEntitySliderInt CreateUIEntity()
    {
      var uiEntity = ScriptableObject.CreateInstance<UISettingsEntitySliderInt>();
      uiEntity.m_MinValue = MinValue;
      uiEntity.m_MaxValue = MaxValue;
      uiEntity.m_ShowValueText = ShowValueText;
      return uiEntity;
    }

    /// <inheritdoc cref="BaseSettingWithValue{T, TEntity, TUIEntity, TBuilder}.BaseSettingWithValue(string, T, LocalizedString)"/>
    /// 
    /// <remarks>
    /// <c>UISettingsEntitySliderInt</c> Defaults:
    /// <list type="bullet">
    /// <item>
    ///   <term><c>m_ShowValueText</c></term>
    ///   <description><c>true</c></description>
    /// </item>
    /// </list>
    /// </remarks>
    public SliderInt(
      string key, int defaultValue, LocalizedString description, int minValue, int maxValue)
      : base(key, defaultValue, description)
    {
      MinValue = minValue;
      MaxValue = maxValue;
    }
  }
}
