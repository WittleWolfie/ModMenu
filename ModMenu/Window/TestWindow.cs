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
using System.Linq;
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
        WindowBuilder.New(Key, CreateString("window.title", "Armor Enchantments"))
          .AddButton(
            CreateString(GetKey("finalize"), "Finalize Attunement"),
            onLeftClick: OnLeftClick,
            layoutParams: new("finalize-button", position: new(600, -400)))
          .AddButton(
            CreateString(GetKey("primary-weapon"), "Primary Weapon"),
            onLeftClick: OnLeftClick,
            layoutParams: new("primary-weapon-button", position: new(-600, -265)))
          .AddButton(
            CreateString(GetKey("secondary-weapon"), "Secondary Weapon"),
            onLeftClick: OnLeftClick,
            layoutParams: new("secondary-weapon-button", position: new(-600, -310)))
          .AddButton(
            CreateString(GetKey("armor"), "Armor"),
            onLeftClick: OnLeftClick,
            layoutParams: new("armor-button", position: new(-600, -355)))
          .AddButton(
            CreateString(GetKey("shield"), "Shield"),
            onLeftClick: OnLeftClick,
            layoutParams: new("shield-button", position: new(-600, -400)))
          .AddCharInfoFeatureGrid(
            GetFeatures,
            style: GridStyle.FixedColumns(4),
            layoutParams: new("enchantments", anchorMin: new(0.12f, 0.3f), anchorMax: new(0.89f, 0.85f))));
    }

    private static IEnumerable<UIFeature> GetFeatures(UnitDescriptor unit)
    {
      return
        UIUtilityUnit.ClearFromDublicatedFeatures(
            UIUtilityUnit.CollectAbilityFeatures(unit),
            UIUtilityUnit.CollectAbilities(unit),
            UIUtilityUnit.CollectActivatableAbilities(unit))
          .Select(
            feature =>
            {
              Main.Logger.Log($"Feature: {feature}");
              feature.Rank = 2;
              return feature;
            });
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
