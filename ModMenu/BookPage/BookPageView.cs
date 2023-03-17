using HarmonyLib;
using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using Kingmaker.UI.FullScreenUITypes;
using Kingmaker.UI.MVVM._PCView.ChangeVisual;
using Kingmaker.UI.MVVM._PCView.InGame;
using Kingmaker.UI.MVVM._VM.EscMenu;
using Owlcat.Runtime.UI.MVVM;
using System;
using System.Reflection;
using UniRx;
using UnityEngine;

namespace ModMenu.BookPage
{
  internal class BookPageView : ViewBase<BookPageVM>
  {
    [HarmonyPatch(typeof(InGameStaticPartPCView))]
    static class InGameStaticPartPCView_Patch
    {
      internal static readonly MethodInfo AddDisposable =
        AccessTools.Method(typeof(InGameStaticPartPCView), "AddDisposable");

      internal static BookPageView BookPageView;
      internal static readonly ReactiveProperty<BookPageVM> BookPageVM = new();

      [HarmonyPatch(nameof(InGameStaticPartPCView.Initialize)), HarmonyPostfix]
      static void Initialize(InGameStaticPartPCView __instance)
      {
        Main.Logger.Log("We initialize!");
        BookPageView = DoInitialize(Instantiate(__instance.m_ChangeVisualPCView.gameObject));
        Main.Logger.Log("We initializeD!");
      }

      [HarmonyPatch(nameof(InGameStaticPartPCView.BindViewImplementation)), HarmonyPostfix]
      static void BindViewImplementation(InGameStaticPartPCView __instance)
      {
        Main.Logger.Log("We bindin!");
        AddDisposable.Invoke(__instance, new[] { BookPageVM.Subscribe(BookPageView.Bind) });
        Main.Logger.Log("We bindinDED!");
      }
    }

    [HarmonyPatch(typeof(EscMenuContextVM))]
    static class EscMenuContextVM_Patch
    {
      [HarmonyPatch(nameof(EscMenuContextVM.HandleOpen)), HarmonyPrefix]
      static bool HandleOpen()
      {
        Main.Logger.Log("Escape shown (VM)!");
        InGameStaticPartPCView_Patch.BookPageVM.Value = new(DisposeBookPage);
        return false;
      }

      private static void DisposeBookPage()
      {
        InGameStaticPartPCView_Patch.BookPageVM.Value?.Dispose();
        InGameStaticPartPCView_Patch.BookPageVM.Value = null;
      }
    }

    [HarmonyPatch(typeof(EscHotkeyManager))]
    static class EscHotkeyManager_Patch
    {
      [HarmonyPatch(nameof(EscHotkeyManager.OnEscPressed)), HarmonyPrefix]
      static bool OnEscPressed()
      {
        Main.Logger.Log("Escape shown (manager)!");
        InGameStaticPartPCView_Patch.BookPageVM.Value = new(DisposeBookPage);
        return false;
      }

      private static void DisposeBookPage()
      {
        InGameStaticPartPCView_Patch.BookPageVM.Value?.Dispose();
        InGameStaticPartPCView_Patch.BookPageVM.Value = null;
      }
    }


    internal static BookPageView DoInitialize(GameObject prefab)
    {
      DestroyImmediate(prefab.GetComponent<ChangeVisualPCView>());
      DontDestroyOnLoad(prefab);

      var view = prefab.AddComponent<BookPageView>();

      Main.Logger.Log("Gotta copy!");
      return view;
    }

    protected override void BindViewImplementation()
    {
      gameObject.SetActive(true);
    }

    protected override void DestroyViewImplementation()
    {
      gameObject.SetActive(false);
    }
  }

  internal class BookPageVM : BaseDisposable, IViewModel
  {
    private readonly Action DisposeAction;

    internal BookPageVM(Action disposeAction)
    {
      DisposeAction = disposeAction;

      EventBus.RaiseEvent<IFullScreenUIHandler>(h => h.HandleFullScreenUiChanged(state: true, FullScreenUIType.ChangeVisual));
    }

    protected override void DisposeImplementation()
    {
      DisposeAction();
    }

    internal void Close()
    {
      DisposeAction?.Invoke();
    }
  }
}
