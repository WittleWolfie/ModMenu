using HarmonyLib;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints;
using ModMenu.Settings;
using System;
using static UnityModManagerNet.UnityModManager;
using static UnityModManagerNet.UnityModManager.ModEntry;

namespace ModMenu
{
  public static class Main
  {
    internal static ModLogger Logger;
    private static Harmony Harmony;

    public static bool Load(ModEntry modEntry)
    {
      try
      {
        Logger = modEntry.Logger;
        modEntry.OnUnload = OnUnload;

        Harmony = new(modEntry.Info.Id);
        Harmony.PatchAll();

        Logger.Log("Finished loading.");
      }
      catch (Exception e)
      {
        Logger.LogException(e);
        return false;
      }
      return true;
    }

    private static bool OnUnload(ModEntry modEntry)
    {
      Logger.Log("Unloading.");
      Harmony?.UnpatchAll();
      return true;
    }

#if DEBUG
    [HarmonyPatch(typeof(BlueprintsCache))]
    static class BlueprintsCache_Patches
    {
      [HarmonyPriority(Priority.First)]
      [HarmonyPatch(nameof(BlueprintsCache.Init)), HarmonyPostfix]
      static void Postfix()
      {
        new TestSettings().Initialize();
      }
    }
#endif
  }
}
