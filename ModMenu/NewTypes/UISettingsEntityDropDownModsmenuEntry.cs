using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UI.SettingsUI;
using ModMenu.Settings;
using UnityEngine;

namespace ModMenu.NewTypes
{
  internal class UISettingsEntityDropdownModsmenuEntry : UISettingsEntityDropdown<ModsMenuEntry>
  {
    static UISettingsEntityDropdownModsmenuEntry()
    {
      instance.LinkSetting(SettingsEntityModsmenuEntry.instance);
    }
    internal static UISettingsEntityDropdownModsmenuEntry instance = ScriptableObject.CreateInstance<UISettingsEntityDropdownModsmenuEntry>();
    public override List<string> LocalizedValues
    {
      get
      { 
        return ModsMenuEntity.ModEntries.Select(entry => entry.ModInfo.GenerateName()).ToList();
      }
    }

    public override int GetIndexTempValue()
    {
      return ModsMenuEntity.ModEntries.IndexOf(Setting.GetTempValue());
    }


    public override void SetIndexTempValue(int value)
    {
      if (value is < 0 && value > ModsMenuEntity.ModEntries.Count())
      {
        Main.Logger.Error($"Value {value} is given to UISettingsEntityDropdownModsmenuEntry when there're only {ModsMenuEntity.ModEntries.Count()} entries in the list");
        SetTempValue(ModsMenuEntity.ModEntries[0]);
      }

      SetTempValue(ModsMenuEntity.ModEntries[value]);
    }

  }
}
