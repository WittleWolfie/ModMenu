using HarmonyLib;
using Kingmaker.Localization;
using Kingmaker.UI.MVVM._VM.Settings;
using Kingmaker.UI.SettingsUI;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ModMenu
{
  /// <summary>
  /// Class containing patches necessary to inject an additional settings screen into the menu.
  /// </summary>
  internal class ModsMenuEntity
  {
    /// Random magic number representing our fake enum for UiSettingsManager.SettingsScreen
    private const int SettingsScreenValue = 17;
    private static readonly UISettingsManager.SettingsScreen SettingsScreenId =
      (UISettingsManager.SettingsScreen)SettingsScreenValue;

    private static LocalizedString _menuTitleString;
    private static LocalizedString MenuTitleString
    {
      get
      {
        if (_menuTitleString is null)
        {
          _menuTitleString = Helpers.CreateString("ModsMenu.Title", "Mods");
        }
        return _menuTitleString;
      }
    }

    internal static readonly List<UISettingsGroup> ModSettings = new();

    /// <summary>
    /// Patch to create the Mods menu screen.
    /// </summary>
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
        Main.Logger.NativeLog("Adding mod settings menu.");
        settings.CreateMenuEntity(MenuTitleString, SettingsScreenId);
      }
    }

    /// <summary>
    /// Patch to return the Mods settings list
    /// </summary>
    [HarmonyPatch(typeof(UISettingsManager))]
    static class UISettingsManager_GetSettingsList
    {
      [HarmonyPatch(nameof(UISettingsManager.GetSettingsList)), HarmonyPostfix]
      static void Postfix(UISettingsManager.SettingsScreen? screenId, ref List<UISettingsGroup> __result)
      {
        if (screenId is not null && screenId == SettingsScreenId)
        {
          Main.Logger.NativeLog("Returning mod settings.");
          __result = ModSettings;
        }
      }
    }
  }
}
