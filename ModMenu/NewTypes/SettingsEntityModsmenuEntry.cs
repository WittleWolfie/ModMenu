using Kingmaker.Settings;
using ModMenu.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModMenu.NewTypes
{
  internal class SettingsEntityModsmenuEntry : SettingsEntity<ModsMenuEntry>
  {
    internal static SettingsEntityModsmenuEntry instance = new("modsmenu.entrystaticinstance", ModsMenuEntry.EmptyInstance);

    public SettingsEntityModsmenuEntry(string key, ModsMenuEntry defaultValue) : base(key, defaultValue, false, false, false) 
      => OnTempValueChanged += new(() => { ModsMenuEntity.settingVM.SwitchSettingsScreen(ModsMenuEntity.SettingsScreenId); return true; });
  }
}
