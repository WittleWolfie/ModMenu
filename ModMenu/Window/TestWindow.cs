using HarmonyLib;
using Kingmaker.UI;
using ModMenu.Utils;
using System;

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
        try
        {
          if (WindowView.WindowVM.Value is null)
          {
            Main.Logger.Log("Showing test window.");
            ModMenu.ShowWindow(Key);
          }
          else
          {
            Main.Logger.Log("Hiding test window.");
            WindowView.DisposeWindow();
          }
          return false;
        }
        catch (Exception e)
        {
          Main.Logger.LogException(e);
        }
        return true;
      }
    }
  }
#endif
}
