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
    internal static GameObject Button;
    internal static GameObject WidgetList;

    internal static void Create()
    {
      Main.Logger.Log("Creating prefabs");
      CreateText();
      CreateButton();
      CreateWidgetList();
    }

    private static void CreateText()
    {
      Text = GameObject.Instantiate(UITool.StaticCanvas.ChildObject("ChangeVisualPCView/Window/Header/Header"));
    }

    private static void CreateButton()
    {
      Button = GameObject.Instantiate(UITool.StaticCanvas.ChildObject("ChangeVisualPCView/Window/BackToStashButton/OwlcatButton"));
    }

    private static void CreateWidgetList()
    {

    }
  }
}
