using Kingmaker.UI.SettingsUI;
using ModMenu.Settings;

namespace ModMenu
{
  /// <summary>
  /// API mods can use to add settings to the Mods menu page.
  /// </summary>
  public static class ModMenu
  {
    /// <summary>
    /// Adds a new group of settings to the Mods menu page.
    /// </summary>
    public static void AddSettings(SettingsGroup settingsGroup)
    {
      ModsMenuEntity.ModSettings.Add(settingsGroup.Build());
    }

    /// <summary>
    /// Adds a new group of settings to the Mods menu page.
    /// </summary>
    /// 
    /// <remarks>
    /// Using <see cref="AddSettings(SettingsGroup)"/> is recommended. If you prefer to construct the settings on your
    /// own you can use this method.
    /// </remarks>
    public static void AddSettings(UISettingsGroup settingsGroup)
    {
      ModsMenuEntity.ModSettings.Add(settingsGroup);
    }
  }
}
