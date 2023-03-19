using HarmonyLib;
using Kingmaker.Localization;
using Kingmaker.UI;
using ModMenu.Utils;
using System;
using UnityEngine;

namespace ModMenu.Window
{
#if DEBUG
  /// <summary>
  /// Test window shown on debug builds to validate usage.
  /// </summary>
  internal class TestWindow
  {
    internal static readonly string Key = GetKey("test-window");

    internal static void Initialize()
    {
      ModMenu.AddWindow(
        WindowBuilder.New(Key, CreateString("window.title", "Test Window"))
          .AddText(CreateString("first-text", "First text!"), new("first-text"))
          .AddText(
            CreateString("second-text", "Second text!"),
            new(
              "second-text",
              position: new(200, 200, 0),
              scale: new(2f, 2f, 1f),
              rotation: Quaternion.Euler(0, 0, 45),
              layoutHandler: OnLayoutCalled)));
    }

    private static LocalizedString CreateString(string key, string text)
    {
      return Helpers.CreateString(GetKey(key), text);
    }

    private static void OnLayoutCalled(Transform transform, string id)
    {
      Main.Logger.Log($"OnLayout called: {id}");
    }

    private static string GetKey(string key)
    {
      return $"mod-menu.{key}";
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
