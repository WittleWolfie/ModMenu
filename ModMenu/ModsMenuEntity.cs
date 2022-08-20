using HarmonyLib;
using Kingmaker.Localization;
using Kingmaker.UI.MVVM._VM.Settings;
using Kingmaker.UI.SettingsUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ModMenu
{
  /// <summary>
  /// Class containing patches necessary to inject an additional settings screen into the menu.
  /// </summary>
  internal class ModsMenuEntity
  {
    /// Random number representing our fake enum for UiSettingsManager.SettingsScreen
    private const int SettingsScreenValue = 17;
    private static readonly UISettingsManager.SettingsScreen SettingsScreen =
      (UISettingsManager.SettingsScreen)SettingsScreenValue;

    private static LocalizedString _menuTitleString;
    private static LocalizedString MenuTitleString
    {
      get
      {
        if (_menuTitleString is null)
        {
          _menuTitleString = new LocalizedString() { m_Key = "MenuTitleString" };
          LocalizationManager.CurrentPack.PutString(_menuTitleString.m_Key, "Mods");
        }
        return _menuTitleString;
      }
    }

    [HarmonyPatch]
    static class SettingsVM_Constructor
    {
      static MethodBase TargetMethod()
      {
        // There's only a single constructor so grab the first one and ignore the arguments. Maybe I'll try adding
        // back the args version later but right now this works.
        return AccessTools.FirstConstructor(typeof(SettingsVM), c => true);
      }

      private static readonly MethodInfo CreateMenuEntity =
        AccessTools.Method(typeof(SettingsVM), nameof(SettingsVM.CreateMenuEntity));
      static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
      {
        var code = new List<CodeInstruction>(instructions);

        // Look for the last usage of CreateMenuEntity, we want to insert just after that.
        var insertionIndex = 0;
        for (int i = code.Count - 1; i > 0; i--)
        {
          if (code[i].Calls(CreateMenuEntity))
          {
            insertionIndex = i + 1; // increment since inserting at i would actually be before the insertion point
            break;
          }
        }

        var newCode =
          new List<CodeInstruction>()
          {
            new CodeInstruction(OpCodes.Ldarg_0), // Loads this
            CodeInstruction.Call(typeof(SettingsVM_Constructor), nameof(SettingsVM_Constructor.AddMenuEntity)),
          };

        code.InsertRange(insertionIndex, newCode);
        return code;
      }

      private static void AddMenuEntity(SettingsVM settings)
      {
        Main.Logger.Log("Adding mod settings menu.");
        settings.CreateMenuEntity(MenuTitleString, SettingsScreen);
      }
    }
  }
}
