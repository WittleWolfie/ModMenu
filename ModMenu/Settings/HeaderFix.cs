using Kingmaker.PubSubSystem;
using Kingmaker;
using UnityEngine;
using Kingmaker.Utility;
using HarmonyLib;
using Kingmaker.Blueprints.JsonSystem;

namespace ModMenu.Settings
{
  internal class HeaderFix
  {
    [HarmonyPatch(typeof(BlueprintsCache))]
    static class BlueprintsCache_Patch
    {
      [HarmonyPatch(nameof(BlueprintsCache.Init)), HarmonyPostfix]
      static void Postfix()
      {
        EventBus.Subscribe(new Fix());
      }
    }
  }

  internal class Fix : ISettingsUIHandler
  {
    public void HandleOpenSettings(bool isMainMenu = false)
    {
      var menuSelectorView =
        Game.Instance.RootUiContext.m_CommonView?.transform.Find(
          "Canvas/SettingsView/ContentWrapper/MenuSelectorPCView");
      if (menuSelectorView is null)
        return;

      foreach (RectTransform transform in menuSelectorView)
        transform.ResetScale();
    }
  }
}
