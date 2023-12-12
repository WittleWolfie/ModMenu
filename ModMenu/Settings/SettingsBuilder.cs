using Kingmaker;
using Kingmaker.Localization;
using Kingmaker.Modding;
using Kingmaker.PubSubSystem;
using Kingmaker.Settings;
using Kingmaker.UI;
using Kingmaker.UI.SettingsUI;
using ModMenu.NewTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace ModMenu.Settings
{
#pragma warning disable CS1591 // stupid documentation requests
  /// <summary>
  /// Builder API for constructing settings.
  /// </summary>
  /// 
  /// <remarks>
  /// <para>
  /// All the <c>AddX</c> methods return <c>this</c> to support builder style method chaining. Once your SettingsGroup
  /// is configured add it to the Mods menu page by calling <see cref="ModMenu.AddSettings(SettingsBuilder)"/>.
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
    private object Mod;
    private bool AllowDisabling;
    private LocalizedString ModName;
    private LocalizedString ModDescription;
    private string Author = "";
    private string Version = "";
    private Sprite modIllustration;
    private readonly List<UISettingsGroup> GroupList = new();
    private UISettingsGroup Group; // it will be now initialized by the instance constructor through a call to AddAnotherSettingsGroup.
    private readonly List<UISettingsEntityBase> Settings = new();
    private readonly Dictionary<string, ISettingsEntity> SettingsEntities = new();
    private Action OnDefaultsApplied;

    /// <param name="key">
    /// Globally unique key / name for the settings group. Use only lowercase letters, numbers, '-', and '.'
    /// </param>
    /// <param name="title">Title of the settings group, displayed on the settings page</param>
    public static SettingsBuilder New(string key, LocalizedString title)
    {
      return new SettingsBuilder(key, title);
    }

    /// <param name="key">
    /// Globally unique key / name for the settings group. Use only lowercase letters, numbers, '-', and '.'
    /// </param>
    /// <param name="title">Title of the settings group, displayed on the settings page</param>
    public SettingsBuilder(string key, LocalizedString title) : this()
    {
      AddAnotherSettingsGroup(key, title);
    }

    private SettingsBuilder() { }

    /// <summary>
    /// Creates a new group of settings.
    /// </summary>
    /// <param name="key"></param>
    /// Globally unique key / name for the settings group. Use only lowercase letters, numbers, '-', and '.'
    /// <param name="title"></param>
    /// Title of the settings group, displayed on the settings page
    public SettingsBuilder AddAnotherSettingsGroup(string key, LocalizedString title)
    {
      if (GroupList.Any() && GroupList.Any(g => g.name.Equals(key)))
        Main.Logger.Warning("An attempt to create a new Settings group with an existing key");

      var group = ScriptableObject.CreateInstance<UISettingsGroup>();
      group.name = key;
      group.Title= title;
      if (Settings?.Count > 0)
      {
        Group.SettingsList = Settings.ToArray();
        Settings.RemoveRange(0, Settings.Count - 1);
      }
      GroupList.Add(group);
      Group = group;
      return this;
    }

    /// <summary>
    /// Set a source mod for your settings. Information from the manifest / info will be displayed when selecting the
    /// mod in the menu dropdown. If you don't set Mod name and Mod description separately, the information from the
    /// manifest / info will be used instead. Providing the source mod is necessary if you want to allow disabling from
    /// the ModMenu.
    /// </summary>
    /// 
    /// <param name="modEntry">Your mod</param>
    /// <param name="allowDisabling">Set to true if you want to create a button to disable it</param>
    public SettingsBuilder SetMod(OwlcatModification modEntry, bool allowDisabling = false)
    {
      Mod = modEntry;
      AllowDisabling = allowDisabling;
      return this;
    }

    /// <summary>
    /// Set a source mod for your settings. Information from the manifest / info will be displayed when selecting the
    /// mod in the menu dropdown. If you don't set Mod name and Mod description separately, the information from the
    /// manifest / info will be used instead. Providing the source mod is necessary if you want to allow disabling from
    /// the ModMenu.
    /// </summary>
    /// 
    /// <param name="modEntry">Your mod</param>
    /// <param name="allowDisabling">Set to true if you want to create a button to disable it</param>
    public SettingsBuilder SetMod(UnityModManager.ModEntry modEntry, bool allowDisabling = false)
    {
      Mod = modEntry;
      AllowDisabling = allowDisabling;
      return this;
    }

    /// <summary>
    /// The name of your mod to be displayed in the ModMenu dropdown.
    /// </summary>
    /// <param name="name">A Localized string containing the mod name.</param>
    public SettingsBuilder SetModName(LocalizedString name)
    {
      ModName = name;
      return this;
    }

    /// <summary>
    /// Sets the description for your mod which will be visible when hovering the mouse over the mod entry in the ModMenu dropdown.
    /// </summary>
    /// <param name="description">A Localized string containing the description.</param>
    public SettingsBuilder SetModDescription(LocalizedString description)
    {
      ModDescription = description;
      return this;
    }

    /// <summary>
    /// Adds a row containing an image. There is no setting tied to this, it is just for decoration.
    /// </summary>
    /// 
    /// <remarks>Height added in v1.2.1</remarks>
    /// 
    /// <param name="height">
    /// Sets the row height. Keep in mind the scaling is relative to resolution; a standard row has a height of 40. The
    /// image width will be scaled to preserve the aspect ratio.
    /// </param>
    /// <param name="imageScale">
    /// Adjust the size of the image. Use this if the default logic doesn't get the size of the image correct.
    /// </param>
    public SettingsBuilder AddImage(Sprite sprite, int height, float imageScale)
    {
      return AddImageInternal(sprite, height, imageScale);
    }

    /// <summary>
    /// Adds mod's version to the mod description shown when you hover the mouse over the mod entry in the ModMenu dropdown
    /// </summary>
    /// 
    /// <remarks>
    /// Will be displayed only if you at least provided a Localized mod name with <see cref="SettingsBuilder.SetModName(LocalizedString)"/>. 
    /// Mod version from the source mod info (if set with <see cref="SettingsBuilder.SetMod(UnityModManagerNet.UnityModManager.ModEntry, bool)"/>
    /// or with <see cref="SettingsBuilder.SetMod(OwlcatModification, bool)"/>) will have higher precedence</remarks>
    public SettingsBuilder SetModVersion(string version)
    {
      Version = version;
      return this;
    }

    /// <summary>
    /// Adds author's name mod description shown when you hover the mouse over the mod entry in the ModMenu dropdown
    /// </summary>
    /// 
    /// <remarks> Will be displayed only if you at least provided a Localized mod name with <see cref="SettingsBuilder.SetModName(LocalizedString)"/>. 
    /// Author's name from the source mod info (if set with <see cref="SettingsBuilder.SetMod(UnityModManagerNet.UnityModManager.ModEntry, bool)"/>
    /// or with <see cref="SettingsBuilder.SetMod(OwlcatModification, bool)"/>) will have higher precedence.</remarks>
    public SettingsBuilder SetModAuthor(string author)
    {
      Author = author;
      return this;
    }

    /// <summary>
    /// Adds an image to the mod description shown when you hover the mouse over the mod entry in the ModMenu dropdown
    /// </summary>
    /// 
    /// <remarks>
    /// Will be displayed only if you have a source mod info (if set with <see cref="SettingsBuilder.SetMod(UnityModManagerNet.UnityModManager.ModEntry, bool)"/>
    /// or with <see cref="SettingsBuilder.SetMod(OwlcatModification, bool)"/>) or at least provided a Localized mod name with <see cref="SettingsBuilder.SetModName(LocalizedString)"/>.
    /// </remarks>
    public SettingsBuilder SetModIllustration(Sprite image)
    {
      modIllustration = image;
      return this;
    }

    /// <summary>
    /// Adds a row containing an image. There is no setting tied to this, it is just for decoration.
    /// </summary>
    /// 
    /// <remarks>Height added in v1.1.0</remarks>
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

    private SettingsBuilder AddImageInternal(Sprite sprite, int height = -1, float imageScale = 1.0f)
    {
      Settings.Add(UISettingsEntityImage.Create(sprite, height, imageScale));
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
    /// Adds a button which resets the value of each setting in this group to its default. Triggers a confirmation
    /// prompt before executing.
    /// </summary>
    /// 
    /// <remarks>Added in v1.1.0</remarks>
    /// 
    /// <param name="onDefaultsApplied">Invoked after default settings are applied.</param>
    public SettingsBuilder AddDefaultButton(Action onDefaultsApplied = null)
    {
      // Make sure OnDefaultsApplied is not null or the dialog doesn't close
      OnDefaultsApplied = onDefaultsApplied ?? new(() => { });
      Settings.Add(
        Button.New(DefaultDescription(), DefaultButtonLabel, OpenDefaultSettingsDialog)
          .WithLongDescription(DefaultDescriptionLong())
          .Build());
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
    /// Adds a dropdown populated using a list of strings. The value of the setting is the index in the list.
    /// </summary>
    public SettingsBuilder AddDropdownList(DropdownList dropdown)
    {
      var (entity, uiEntity) = dropdown.Build();
      return Add(entity.Key, entity, uiEntity);
    }

    /// <summary>
    /// Similar to DropdownList but includes a button on the setting which fires an event when clicked.
    /// </summary>
    public SettingsBuilder AddDropdownButton(DropdownButton dropdown)
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
    /// Adds a button which can be used to set key bindings.
    /// </summary>
    /// 
    /// <remarks>
    /// Keep in mind:
    /// <list type="bullet">
    /// <item>
    /// The KeyBinding's <c>Key</c> must be a unique identifier for the setting as well as a unique identifier for the
    /// binding. If there's a conflict then <paramref name="onPress"/> will never trigger.
    /// </item>
    /// <item>
    /// If another key binding has the same mapping when the game loads, this key binding will be cleared and the user
    /// must set a new one. This safety check does not happen when resetting to defaults using the default button from
    /// <see cref="AddDefaultButton(Action)"/>. This is an owlcat limitation.
    /// </item>
    /// </list>
    /// </remarks>
    /// 
    /// <param name="onPress">Action invoked when the key binding is activated</param>
    public SettingsBuilder AddKeyBinding(KeyBinding keyBinding, Action onPress)
    {
      var (entity, uiEntity) = keyBinding.Build();
      KeyboardAccess_Patch.RegisterBinding(entity, uiEntity, onPress);
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

    /// <summary>
    /// Adds a sub-header marking the start of collapsible group of settings.
    /// </summary>
    /// 
    /// <remarks>
    /// The sub-header applies to every view following it until another sub-header is added.
    /// </remarks>
    /// 
    /// <param name="startExpanded">If true, the sub-header starts expanded.</param>
    public SettingsBuilder AddSubHeader(LocalizedString title, bool startExpanded = false)
    {
      Settings.Add(UISettingsEntitySubHeader.Create(title, startExpanded));
      return this;
    }

    private SettingsBuilder Add(string key, ISettingsEntity entity, UISettingsEntityBase uiEntity)
    {
      SettingsEntities.Add(key, entity);
      Settings.Add(uiEntity);
      return this;
    }

    internal (List<UISettingsGroup> groups, Dictionary<string, ISettingsEntity> settings, Info info) Build()
    {
      Group.SettingsList = Settings.ToArray();
      Info Info;
      if (Mod is OwlcatModification OwlMod)
        Info = new(OwlMod, AllowDisabling, ModName, ModDescription, modIllustration);
      else if (Mod is UnityModManager.ModEntry UMMmod)
        Info = new(UMMmod, AllowDisabling, ModName, ModDescription, modIllustration);
      else if (ModName is not null)
        Info = new(ModName, ModDescription, Version, Author, modIllustration);
      else
        Info = new(GroupList.ElementAt(0)?.Title ?? "");
      return (GroupList, SettingsEntities, Info);
    }

    private void OpenDefaultSettingsDialog()
    {
      string text =
        string.Format(
          Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SettingsUI.RestoreAllDefaultsMessage,
          Group.Title);
      EventBus.RaiseEvent(delegate (IMessageModalUIHandler w)
      {
        w.HandleOpen(
          text,
          MessageModalBase.ModalType.Dialog,
          new Action<MessageModalBase.ButtonType>(OnDefaultDialogAnswer));
      },
      true);
    }

    public void OnDefaultDialogAnswer(MessageModalBase.ButtonType buttonType)
    {
      if (buttonType != MessageModalBase.ButtonType.Yes)
        return;

      foreach (var setting in SettingsEntities.Values)
        setting.ResetToDefault(true);

      OnDefaultsApplied();
    }

    private LocalizedString DefaultDescription()
    {
      return
        Helpers.CreateString(
          $"mod-menu.default-description.{Group.name}",
          enGB: $"Restore all settings in {Group.Title} to their defaults",
          ruRU: $"Вернуть все настройки в группе {Group.Title} к значениям по умолчанию",
          zhCN: $"还原所有{Group.Title}中的设置到默认值",
          deDE: $"Setze alle Einstellungen in {Group.Title} auf ihre Standardwerte zurück",
          frFR: "Rétablir les valeurs par défaut de tous les paramètres sous {Group.Title}");
    }

    private LocalizedString DefaultDescriptionLong()
    {
      return
        Helpers.CreateString(
          $"mod-menu.default-description-long.{Group.name}",
          enGB: $"Sets each settings under {Group.Title} to its default value. Your current settings will be lost."
          + $" Settings in other groups are not affected. Keep in mind this will apply to sub-groups under"
          + $" {Group.Title} as well (anything that is hidden when the group is collapsed).",
          ruRU: $"При нажатии на кнопку все настройки в группе {Group.Title} примут значения по умолчанию." +
          $" Ваши текущие настройки будут потеряны. Настройки из других групп затронуты не будут. Обратите внимание," +
          $" что изменения коснутся в том числе настроек из подгрупп, вложенных в {Group.Title}" +
          $"  (т.е. все те настройки, которые оказываются скрыты, когда вы сворачиваете группу).",
          zhCN: $"{Group.Title}之中每一项设置的值都会变成各自的默认值。你的当前设置会丢失。其它分组的设置不受影响。" +
          $"注意这也会影响{Group.Title}内部的小分组（只要是折叠之后看不见的都会影响.",
          deDE: "Setzt alle Einstellungen unter {Group.Title} auf ihre Standardwerte zurück. Die aktuellen Einstellungen gehen dabei verloren. " +
          "Einstellungen in anderen Gruppen werden nicht beeinflusst. Beachte, dass dies auch die Untergruppen von {Group.Title} betrifft.",
          frFR: "Rétablit la valeur par défaut pour tous les paramètres sous {Group.Title}. Vos paramètres actuels vont être perdus. " +
          "Les paramètres des autres sections ne seront pas affectés. Gardez en tête que cela va s'appliquer aussi aux sous-sections de {Group.Title} " +
          "(tout ce qui est caché quand la section est réduite)");
    }

    private static LocalizedString _defaultButtonLabel;
    private static LocalizedString DefaultButtonLabel
    {
      get
      {
        _defaultButtonLabel ??= Helpers.CreateString(
          "mod-menu.default-button-label", "Default", ruRU: "По умолчанию", zhCN: "默认值", deDE: "Standard", frFR: "Défaut");
        return _defaultButtonLabel;
      }
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
      return UISettingsEntityButton.Create(Description, LongDescription, ButtonText, OnClick);
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

  public class DropdownList
    : BaseSettingWithValue<int, SettingsEntityInt, UISettingsEntityDropdownInt, DropdownList>
  {
    private readonly List<LocalizedString> DropdownValues;

    /// <inheritdoc cref="DropdownList(string, int, LocalizedString, List{LocalizedString})"/>
    public static DropdownList New(
      string key, int defaultSelected, LocalizedString description, List<LocalizedString> values)
    {
      return new(key, defaultSelected, description, values);
    }

    protected override SettingsEntityInt CreateEntity()
    {
      return new SettingsEntityInt(Key, DefaultValue, SaveDependent, RebootRequired);
    }

    protected override UISettingsEntityDropdownInt CreateUIEntity()
    {
      var dropdown = ScriptableObject.CreateInstance<UISettingsEntityDropdownInt>();
      dropdown.m_LocalizedValues = DropdownValues.Select(value => value.ToString()).ToList();
      return dropdown;
    }

    /// <inheritdoc cref="BaseSettingWithValue{T, TEntity, TUIEntity, TBuilder}.BaseSettingWithValue(string, T, LocalizedString)"/>
    /// 
    /// <param name="defaultSelected">Index of the default selected value in <paramref name="values"/></param>
    /// <param name="values">List of values to display</param>
    public DropdownList(
      string key, int defaultSelected, LocalizedString description, List<LocalizedString> values)
      : base(key, defaultSelected, description)
    {
      DropdownValues = values;
    }
  }

  public class DropdownButton
    : BaseSettingWithValue<int, SettingsEntityInt, UISettingsEntityDropdownButton, DropdownButton>
  {
    private readonly LocalizedString ButtonText;
    private readonly Action<int> OnClick;
    private readonly List<LocalizedString> DropdownValues;

    /// <inheritdoc cref="DropdownButton(string, int, LocalizedString, LocalizedString, Action{int}, List{LocalizedString})"/>
    public static DropdownButton New(
      string key,
      int defaultSelected,
      LocalizedString description,
      LocalizedString buttonText,
      Action<int> onClick,
      List<LocalizedString> values)
    {
      return new(key, defaultSelected, description, buttonText, onClick, values);
    }

    protected override SettingsEntityInt CreateEntity()
    {
      return new SettingsEntityInt(Key, DefaultValue, SaveDependent, RebootRequired);
    }

    protected override UISettingsEntityDropdownButton CreateUIEntity()
    {
      var dropdown = UISettingsEntityDropdownButton.Create(Description, LongDescription, ButtonText, OnClick);
      dropdown.m_LocalizedValues = DropdownValues.Select(value => value.ToString()).ToList();
      return dropdown;
    }

    /// <inheritdoc cref="BaseSettingWithValue{T, TEntity, TUIEntity, TBuilder}.BaseSettingWithValue(string, T, LocalizedString)"/>
    /// 
    /// <param name="defaultSelected">Index of the default selected value in <paramref name="values"/></param>
    /// <param name="values">List of values to display</param>
    public DropdownButton(
        string key,
        int defaultSelected,
        LocalizedString description,
        LocalizedString buttonText,
        Action<int> onClick,
        List<LocalizedString> values)
      : base(key, defaultSelected, description)
    {
      ButtonText = buttonText;
      OnClick = onClick;
      DropdownValues = values;
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
