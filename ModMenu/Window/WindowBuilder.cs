using Kingmaker.Localization;
using ModMenu.Window.Layout;
using System.Collections.Generic;

namespace ModMenu.Window
{
  // TODO: Add real docs
  /// <summary>
  /// Builder API for constructing a full screen window.
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
  public class WindowBuilder
  {
    internal readonly string Key;

    internal readonly List<BaseElement> Elements = new();
    internal readonly List<BaseContainer> Containers = new();
    internal LocalizedString Title;

    /// <param name="key">Globally unique key / name for the window. Case insensitive.</param>
    /// <param name="title">Title displayed on the window, or null for none</param>
    public static WindowBuilder New(string key, LocalizedString title = null)
    {
      return new(key, title);
    }

    /// <param name="key">Globally unique key / name for the window. Case insensitive.</param>
    /// <param name="title">Title displayed on the window, or null for none</param>
    public WindowBuilder(string key, LocalizedString title = null)
    {
      Key = key.ToLower();
      Title = title;
    }

    // TODO Layouts:
    //  - CharInfoSectionView
    //  - HorizontalLayout
    //  - VerticalLayout
    //  - GridLayout
    //  - Scrolling List (thing Change Visual RHS)
    // TODO Elements:
    //  - Character Doll & Slots
    //  - Draggable Item "Slot"
    //  - Buttons [DONE]
    //  - Text [DONE]
    //  - Expandable Headers?
    //  - Images?
    // TODO Misc.:
    //  - Sounds?
    //  - Text formatting?
    //
    // Notes:
    //  - Root should be absolute positioning

    public WindowBuilder AddText(
      LocalizedString text, TextStyle style = null, AbsoluteLayoutParams layoutParams = null)
    {
      Elements.Add(new TextElement(text, style, layoutParams));
      return this;
    }

    public WindowBuilder AddButton(
      LocalizedString text,
      OnClick onLeftClick,
      ButtonStyle style = null,
      AbsoluteLayoutParams layoutParams = null,
      OnClick onRightClick = null)
    {
      Elements.Add(new ButtonElement(text, onLeftClick, style, layoutParams, onRightClick));
      return this;
    }

    // TODO: Grid needs to be its own thing not a builder. Only use builders for horizontal / vertical containers.
    // TODO: Vertical / Horizontal containers can nest
    // TODO: Grids just pull from a data source and scroll
    public WindowBuilder AddGrid(GridBuilder grid, GridStyle style = null, AbsoluteLayoutParams layoutParams = null)
    {
      Containers.Add(new GridContainer(grid, style, layoutParams));
      return this;
    }
    
    // TODO:
    //  - To support customized stuff, just provide an add function accepting a callback which returns a list of
    //    IViewModel objects + callback to intantiate + binder
  }
}
