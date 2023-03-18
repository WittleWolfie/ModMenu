using HarmonyLib;
using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using Kingmaker.UI.FullScreenUITypes;
using Kingmaker.UI.MVVM._PCView.ChangeVisual;
using Kingmaker.UI.MVVM._PCView.InGame;
using ModMenu.Utils;
using Owlcat.Runtime.UI.MVVM;
using System;
using System.Reflection;
using UniRx;
using UnityEngine;

namespace ModMenu.Window
{
  // TODO:
  // - Remove all the stuff we don't need from ChangeVisualPCView
  // - Create basic view element prefabs
  // - Define build structure to create a screen
  //    - Grid + Linear + Absolute?
  // - Define action to launch the screen
  // - Implement layout using build structure
  internal class WindowView : ViewBase<BookPageVM>
  {
    // TODO: Replace meee
    [HarmonyPatch(typeof(EscHotkeyManager))]
    static class EscHotkeyManager_Patch
    {
      [HarmonyPatch(nameof(EscHotkeyManager.OnEscPressed)), HarmonyPrefix]
      static bool OnEscPressed()
      {
        Main.Logger.Log("Escape shown (manager)!");
        InGameStaticPartPCView_Patch.WindowVM.Value = new(DisposeBookPage);
        return false;
      }

      private static void DisposeBookPage()
      {
        InGameStaticPartPCView_Patch.WindowVM.Value?.Dispose();
        InGameStaticPartPCView_Patch.WindowVM.Value = null;
      }
    }

    public override void BindViewImplementation()
    {
      gameObject.SetActive(true);
    }

    public override void DestroyViewImplementation()
    {
      gameObject.SetActive(false);
    }

    [HarmonyPatch(typeof(InGameStaticPartPCView))]
    static class InGameStaticPartPCView_Patch
    {
      // TODO: Publicize
      internal static readonly MethodInfo AddDisposable =
        AccessTools.Method(typeof(InGameStaticPartPCView), "AddDisposable");

      internal static WindowView Prefab;
      internal static readonly ReactiveProperty<BookPageVM> WindowVM = new();

      [HarmonyPatch(nameof(InGameStaticPartPCView.Initialize)), HarmonyPostfix]
      static void Initialize(InGameStaticPartPCView __instance)
      {
        try
        {
          Main.Logger.NativeLog("Initializing WindowView Prefab");
          Prefab = CreatePrefab(__instance.m_ChangeVisualPCView);
        }
        catch (Exception e)
        {
          Main.Logger.LogException(e);
        }
      }

      [HarmonyPatch(nameof(InGameStaticPartPCView.BindViewImplementation)), HarmonyPostfix]
      static void BindViewImplementation(InGameStaticPartPCView __instance)
      {
        try
        {
          Main.Logger.NativeLog("Binding to WindowVM");
          AddDisposable.Invoke(__instance, new[] { WindowVM.Subscribe(Prefab.Bind) });
        }
        catch (Exception e)
        {
          Main.Logger.LogException(e);
        }
      }

      internal static WindowView CreatePrefab(ChangeVisualPCView changeVisualView)
      {
        var prefab = GameObject.Instantiate(changeVisualView.gameObject);
        prefab.DestroyComponents<ChangeVisualPCView>();
        prefab.transform.AddTo(changeVisualView.transform.parent);
        return prefab.AddComponent<WindowView>();
      }
    }
  }

  // TODO: Basically the entire implementation :(
  internal class BookPageVM : BaseDisposable, IViewModel
  {
    private readonly Action DisposeAction;

    internal BookPageVM(Action disposeAction)
    {
      DisposeAction = disposeAction;
      EventBus.RaiseEvent<IFullScreenUIHandler>(h => h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Unknown));
    }

    public override void DisposeImplementation()
    {
      DisposeAction();
    }

    internal void Close()
    {
      DisposeAction?.Invoke();
    }
  }
}
