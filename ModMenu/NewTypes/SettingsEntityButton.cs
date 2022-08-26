using HarmonyLib;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.MVVM._PCView.Settings.Entities;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Kingmaker.UI.SettingsUI;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModMenu.NewTypes
{
  public class UISettingsEntityButton : UISettingsEntityBase
  {
    internal readonly LocalizedString ButtonText;
    internal readonly Action OnClick;

    public UISettingsEntityButton(
      LocalizedString description, LocalizedString longDescription, LocalizedString buttonText, Action onClick)
    {
      m_Description = description;
      m_TooltipDescription = longDescription;

      ButtonText = buttonText;
      OnClick = onClick;
    }

    public override SettingsListItemType? Type => SettingsListItemType.Custom;
  }

  internal class SettingsEntityButtonVM : SettingsEntityVM
  {
    private readonly UISettingsEntityButton buttonEntity;

    public string Text =>
      buttonEntity.ButtonText.LoadString(LocalizationManager.CurrentPack, LocalizationManager.CurrentLocale);

    internal SettingsEntityButtonVM(UISettingsEntityButton buttonEntity) : base(buttonEntity)
    {
      this.buttonEntity = buttonEntity;
    }

    public void PerformClick()
    {
      buttonEntity.OnClick?.Invoke();
    }
  }

  internal class SettingsEntityButtonView
    : SettingsEntityView<SettingsEntityButtonVM>, IPointerEnterHandler, IPointerExitHandler
  {
    private static readonly FieldInfo OverrideType =
      AccessTools.Field(typeof(VirtualListLayoutElementSettings), "m_OverrideType");

    public override VirtualListLayoutElementSettings LayoutSettings
    {
      get
      {
        bool set_mOverrideType = m_LayoutSettings == null;
        m_LayoutSettings ??= new();
        if (set_mOverrideType)
        {
          OverrideType.SetValue(m_LayoutSettings, VirtualListLayoutElementSettings.LayoutOverrideType.UnityLayout);
        }

        return m_LayoutSettings;
      }
    }

    private VirtualListLayoutElementSettings m_LayoutSettings;

    public override void BindViewImplementation()
    {
      Title.text = ViewModel.Title;
      ButtonLabel.text = ViewModel.Text;
      Button.OnLeftClick.RemoveAllListeners();
      Button.OnLeftClick.AddListener(() =>
      {
        ViewModel.PerformClick();
      });

      SetupColor(isHighlighted: false);
    }

    private Color NormalColor = Color.clear;
    private Color HighlightedColor = new(0.52f, 0.52f, 0.52f, 0.29f);

    internal Image HighlightedImage;
    internal TextMeshProUGUI Title;
    internal OwlcatButton Button;
    internal TextMeshProUGUI ButtonLabel;

    private void SetupColor(bool isHighlighted)
    {
      if (HighlightedImage != null)
      {
        HighlightedImage.color = isHighlighted ? HighlightedColor : NormalColor;
      }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
      EventBus.RaiseEvent(delegate (ISettingsDescriptionUIHandler h)
      {
        h.HandleShowSettingsDescription(ViewModel.Title, ViewModel.Description);
      },
      true);
      SetupColor(isHighlighted: true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
      EventBus.RaiseEvent(delegate (ISettingsDescriptionUIHandler h)
      {
        h.HandleHideSettingsDescription();
      },
      true);
      SetupColor(isHighlighted: false);
    }
  }
}

