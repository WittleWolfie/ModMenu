using Kingmaker.UI.SettingsUI;
using ModMenu.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModMenu.NewTypes
{
  internal class UISettingsEntityDropdownModMenuEntry : UISettingsEntityDropdown<ModsMenuEntry>
  {
    static UISettingsEntityDropdownModMenuEntry()
    {
      ((IUISettingsEntityDropdown) instance).OnTempIndexValueChanged += new Action<int>(ModIndex => ModsMenuEntity.settingVM.SwitchSettingsScreen(ModsMenuEntity.SettingsScreenId));
      instance.LinkSetting(SettingsEntityModsmenuEntry.instance);
    }
    internal static UISettingsEntityDropdownModMenuEntry instance = ScriptableObject.CreateInstance<UISettingsEntityDropdownModMenuEntry>();
    public override List<string> LocalizedValues
    {
      get
      { 
        return ModsMenuEntity.ModEntries.Select(entry => entry.ModInfo.ModName.ToString()).ToList();
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
        Main.Logger.Error($"Value {value} is given to UISettingsEntityDropdownModMenuEntry when there're only {ModsMenuEntity.ModEntries.Count()} entries in the list");
        SetTempValue(ModsMenuEntity.ModEntries[0]);
      }

      SetTempValue(ModsMenuEntity.ModEntries[value]);
    }

  }
}
