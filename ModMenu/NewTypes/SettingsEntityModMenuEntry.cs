using Kingmaker.Settings;
using ModMenu.Settings;

namespace ModMenu.NewTypes
{
  internal class SettingsEntityModMenuEntry : SettingsEntity<ModsMenuEntry>
  {
    internal static SettingsEntityModMenuEntry instance = new("modsmenu.entrystaticinstance", ModsMenuEntry.EmptyInstance);
    public SettingsEntityModMenuEntry(string key, ModsMenuEntry defaultValue) : base(key, defaultValue, false, false, false) {} 
  }
}
