using ModMenu.Utils;
using UnityEngine;

namespace ModMenu.Window
{
  /// <summary>
  /// Class storing prefab objects used to instantiate elements and containers.
  /// </summary>
  /// 
  /// <remarks>
  /// Each Prefab is a copy of some original prefab. This allows edit any components on the object once to create
  /// a prefab, rather than using a base game prefab and editing components each time it is used. 
  /// </remarks>
  internal static class Prefabs
  {
    internal static GameObject Text;

    // Basic idea: create a copy of our original prefabs, removing anything not needed. This way when we spawn from
    // prefab we don't have to remove a bunch of things.
    internal static void Create()
    {
      Main.Logger.Log("Creating prefabs");
      //var canvas = view.gameObject.ChildObject("StaticCanvas");
      CreateText();
    }

    private static void CreateText()
    {
      Text = GameObject.Instantiate(UITool.StaticCanvas.gameObject.ChildObject("ChangeVisualPCView/Window/Header/Header"));
    }
  }
}
