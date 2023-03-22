using Kingmaker.ElementsSystem;

namespace ModMenu.Window
{
  /// <summary>
  /// Shows a window configured using <see cref="ModMenu.AddWindow(WindowBuilder)"/>.
  /// </summary>
  ///
  /// <remarks>Requires the window <see cref="Key"/></remarks>
  public class ShowWindow : GameAction
  {
    /// <summary>
    /// Key identifying the window to show
    /// </summary>
    public string Key;

    public object WindowParams;

    public override string GetCaption()
    {
      return "Shows a full screen window";
    }

    public override void RunAction()
    {
      ModMenu.ShowWindow(Key);
    }
  }
}
