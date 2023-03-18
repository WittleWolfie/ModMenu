using HarmonyLib;
using Kingmaker.UI;
using ModMenu.Utils;

namespace ModMenu.Window
{
#if DEBUG
  /// <summary>
  /// Test window shown on debug builds to validate usage.
  /// </summary>
  internal class TestWindow
  {
    internal static readonly string Key = "mod-menu.test-window";

    internal static void Initialize()
    {
      ModMenu.AddWindow(WindowBuilder.New(Key, Helpers.CreateString("mod-menu.window.title", "Test Window")));
    }

    [HarmonyPatch(typeof(EscHotkeyManager))]
    static class EscHotkeyManager_Patch
    {
      [HarmonyPatch(nameof(EscHotkeyManager.OnEscPressed)), HarmonyPrefix]
      static bool OnEscPressed()
      {
        Main.Logger.Log("Escape shown (manager)!");
        ModMenu.ShowWindow(Key);
        return false;
      }
    }
  }
#endif
}
