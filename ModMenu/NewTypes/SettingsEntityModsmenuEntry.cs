using Kingmaker.Settings;
using ModMenu.Settings;

namespace ModMenu.NewTypes
{
  internal class SettingsEntityModsmenuEntry : SettingsEntity<ModsMenuEntry>
  {
    internal static SettingsEntityModsmenuEntry instance = new("modsmenu.entrystaticinstance", ModsMenuEntry.EmptyInstance);
    public SettingsEntityModsmenuEntry(string key, ModsMenuEntry defaultValue) : base(key, defaultValue, false, false, false) {} 
  }
}
