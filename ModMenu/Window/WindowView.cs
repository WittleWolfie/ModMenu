﻿using HarmonyLib;
using Kingmaker;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.FullScreenUITypes;
using Kingmaker.UI.MVVM._PCView.ChangeVisual;
using Kingmaker.UI.MVVM._PCView.InGame;
using ModMenu.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using System;
using TMPro;
using UniRx;
using UnityEngine;

namespace ModMenu.Window
{
  // TODO:
  // - Create basic view element prefabs
  // - Define build structure to create a screen
  //    - Grid + Linear + Absolute?
  // - Define action to launch the screen
  // - Implement layout using build structure
  internal class WindowView : ViewBase<WindowVM>
  {
    #region Static
    private static WindowView BaseView;
    internal static readonly ReactiveProperty<WindowVM> WindowVM = new();

    internal static void ShowWindow(WindowBuilder window)
    {
      WindowVM.Value = new(window, DisposeWindow);
    }

    internal static void DisposeWindow()
    {
      WindowVM.Value?.Dispose();
      WindowVM.Value = null;
    }
    #endregion

    private OwlcatButton CloseButton;
    private TextMeshProUGUI Header;

    public override void BindViewImplementation()
    {
      gameObject.SetActive(true);
      AddDisposable(Game.Instance.UI.EscManager.Subscribe(ViewModel.Close));
      AddDisposable(CloseButton.OnLeftClickAsObservable().Subscribe(_ => ViewModel.Close()));

      Header.text = ViewModel.Header;
    }

    public override void DestroyViewImplementation()
    {
      gameObject.SetActive(false);
    }

    internal void Initialize()
    {
      CloseButton = gameObject.ChildObject("Window/Close").GetComponent<OwlcatButton>();
      Header = gameObject.ChildObject("Window/Header").GetComponentInChildren<TextMeshProUGUI>();
    }

    [HarmonyPatch(typeof(InGameStaticPartPCView))]
    static class InGameStaticPartPCView_Patch
    {

      [HarmonyPatch(nameof(InGameStaticPartPCView.Initialize)), HarmonyPostfix]
      static void Initialize(InGameStaticPartPCView __instance)
      {
        try
        {
          Main.Logger.NativeLog("Initializing WindowView BaseView");
          BaseView = Create(__instance.m_ChangeVisualPCView);
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
          __instance.AddDisposable(WindowVM.Subscribe(BaseView.Bind));
        }
        catch (Exception e)
        {
          Main.Logger.LogException(e);
        }
      }

      internal static WindowView Create(ChangeVisualPCView changeVisualView)
      {
        var obj = GameObject.Instantiate(changeVisualView.gameObject);
        obj.transform.AddTo(changeVisualView.transform.parent);
        obj.MakeSibling("ServiceWindowsPCView");

        obj.DestroyComponents<ChangeVisualPCView>();
        // TODO: Add as components!
        obj.DestroyChildren(
          "Window/InteractionSlot",
          "Window/Inventory",
          "Window/Doll",
          "Window/BackToStashButton",
          "Window/ChangeItemsPool"); 

        var view = obj.AddComponent<WindowView>();
        view.Initialize();
        return view;
      }
    }
  }

  // TODO: Basically the entire implementation :(
  internal class WindowVM : BaseDisposable, IViewModel
  {
    private readonly WindowBuilder Window;
    private readonly Action DisposeAction;

    internal WindowVM(WindowBuilder window, Action disposeAction)
    {
      Window = window;
      DisposeAction = disposeAction;
      EventBus.RaiseEvent<IFullScreenUIHandler>(h => h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Unknown));
    }

    public override void DisposeImplementation()
    {
      DisposeAction();
      EventBus.RaiseEvent<IFullScreenUIHandler>(h => h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Unknown));
    }

    internal void Close()
    {
      DisposeImplementation();
    }

    internal string Header => Window.Title;
  }
}
