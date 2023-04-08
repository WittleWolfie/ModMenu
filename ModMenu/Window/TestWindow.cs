using HarmonyLib;
using Kingmaker.Localization;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic;
using ModMenu.Utils;
using ModMenu.Window.Layout;
using ModMenu.Window.Views;
using System;
using System.Collections.Generic;
using TMPro;
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
          .AddText(CreateString("first-text", "First text!"), layoutParams: new("first-text"))
          .AddText(
            CreateString("second-text", "Second text!"),
            style: new(color: Color.green),
            layoutParams: new(
              "second-text",
              position: new(200, 200),
              scale: new(2f, 2f, 1f),
              rotation: Quaternion.Euler(0, 0, 45),
              binder: OnBind))
          .AddButton(
            CreateString(GetKey("first-button"), "Click Me!"),
            onLeftClick: OnLeftClick,
            style: new(new(color: Color.red)),
            layoutParams: new("first-button", position: new(-200, 200)),
            onRightClick: OnRightClick)
          .AddCharInfoFeatureGrid(
            GetFeatures,
            style: GridStyle.FixedColumns(3),
            layoutParams: new("first-grid")));
    }

    private static IEnumerable<UIFeature> GetFeatures(UnitDescriptor unit)
    {
      return
        UIUtilityUnit.ClearFromDublicatedFeatures(
          UIUtilityUnit.CollectAbilityFeatures(unit),
          UIUtilityUnit.CollectAbilities(unit),
          UIUtilityUnit.CollectActivatableAbilities(unit));
    }

    private static LocalizedString CreateString(string key, string text)
    {
      return Helpers.CreateString(GetKey(key), text);
    }

    private static void OnBind(Transform transform, string id)
    {
      Main.Logger.Log($"OnBind called: {id}");
    }

    private static void OnLeftClick(string id, bool enabled, bool doubleClick)
    {
      Main.Logger.Log($"Left Click: id - {id}, enabled - {enabled}, double - {doubleClick}");
    }

    private static void OnRightClick(string id, bool enabled, bool doubleClick)
    {
      Main.Logger.Log($"Right Click: id - {id}, enabled - {enabled}, double - {doubleClick}");
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
