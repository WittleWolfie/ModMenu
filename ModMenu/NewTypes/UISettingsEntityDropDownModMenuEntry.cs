using HarmonyLib;
using Kingmaker.Localization;
using Kingmaker.Settings;
using Kingmaker.UI.MVVM._VM.Settings;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Kingmaker.UI.SettingsUI;
using ModMenu.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModMenu.NewTypes
{
  [HarmonyLib.HarmonyPatch]
  internal class UISettingsEntityDropdownModMenuEntry : UISettingsEntityDropdown<ModsMenuEntry>
  {
    static UISettingsEntityDropdownModMenuEntry()
    {
      instance = new()
      {
        m_Description = Helpers.CreateString("UISettingsEntityDropdownModMenuEntry.Description", "Choose your mod", ruRU: "Выберите мод"),
        m_TooltipDescription = Helpers.EmptyString,
      };
      instance.LinkSetting(SettingsEntityModMenuEntry.instance);

      ((IUISettingsEntityDropdown) instance).OnTempIndexValueChanged +=
        new (ModIndex => ModsMenuEntity.settingVM.SwitchSettingsScreen(ModsMenuEntity.SettingsScreenId));

      ((IUISettingsEntityDropdown) instance).OnTempIndexValueChanged +=
        new (_ =>
        {
          SettingsController.RemoveFromConfirmationList(instance.SettingsEntity, false);
          SettingsEntityModMenuEntry.instance.TempValueIsConfirmed = true;
        });

    }

    internal static UISettingsEntityDropdownModMenuEntry instance;

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
