using Kingmaker.Localization;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic;
using ModMenu.Window.Layout;
using System.Collections.Generic;

namespace ModMenu.Window
{
  // TODO: Add real docs
  /// <summary>
  /// Builder API for constructing a full screen window.
  /// </summary>
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


    public delegate IEnumerable<UIFeature> GetFeatures(UnitDescriptor unit);

    // TODO: DataGrid needs to be its own thing not a builder. Only use builders for horizontal / vertical containers.
    // TODO: Vertical / Horizontal containers can nest
    // TODO: Grids just pull from a data source and scroll
    public WindowBuilder AddCharInfoFeatureGrid(
      GetFeatures featureProvider,
      GridStyle style = null,
      RelativeLayoutParams layoutParams = null)
    {
      Containers.Add(new CharInfoFeatureGrid(featureProvider, style, layoutParams));
      return this;
    }
    
    // TODO:
    //  - To support customized stuff, just provide an add function accepting a callback which returns a list of
    //    IViewModel objects + callback to intantiate + binder
  }
}
