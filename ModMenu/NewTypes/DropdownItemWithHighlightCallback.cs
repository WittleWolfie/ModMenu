using Owlcat.Runtime.UI.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UniRx.Triggers;
using UniRx;
using HarmonyLib;
using Epic.OnlineServices;
using UnityEngine.EventSystems;
using Steamworks;

namespace ModMenu.NewTypes
{
  [HarmonyPatch]
  internal class DropdownOptionWithHighlightCallback : TMP_Dropdown.OptionData
  {
    public List<Action<TMP_Dropdown.DropdownItem>> OnMouseEnter = new();
    public List<Action<TMP_Dropdown.DropdownItem>> OnMouseExit = new();


    [HarmonyPatch(typeof(TMP_Dropdown), nameof(TMP_Dropdown.AddItem))]
    [HarmonyPostfix]
    static void TMP_Dropdown_AddItem_Patch(TMP_Dropdown.OptionData data, TMP_Dropdown __instance, TMP_Dropdown.DropdownItem __result)
    {
      if (data is not DropdownOptionWithHighlightCallback workaround)
        return;
      else
      {
        var observer = __result.gameObject.AddComponent<PointerObserverWorkaround>();
        foreach (var action in workaround.OnMouseEnter)
          observer.PointerEnter += action;
        foreach (var action in workaround.OnMouseExit)
          observer.PointerExit += action;
      }          
    }


    class PointerObserverWorkaround : UIBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
      internal event Action<TMP_Dropdown.DropdownItem> PointerEnter;
      internal event Action<TMP_Dropdown.DropdownItem> PointerExit;

      public void OnPointerEnter(PointerEventData eventData)
      {
        PointerEnter?.Invoke(GetComponent<TMP_Dropdown.DropdownItem>());
      }
      public void OnPointerExit(PointerEventData eventData)
      {
        PointerExit?.Invoke(GetComponent<TMP_Dropdown.DropdownItem>());
      }
    }
  }
}
