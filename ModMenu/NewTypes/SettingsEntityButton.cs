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
  internal class UISettingsEntityButton : UISettingsEntityBase
  {
    public readonly string ButtonText;
    public readonly Action OnClick;

    public UISettingsEntityButton(LocalizedString description, LocalizedString tooltip, LocalizedString buttonText, Action onClick)
    {
      this.m_Description = description;
      this.m_TooltipDescription = tooltip;
      this.m_IAmSetHandler = false;
      this.m_ShowVisualConnection = false;

      ButtonText = buttonText.ToString();
      OnClick = onClick;
    }
    public override SettingsListItemType? Type => SettingsListItemType.Custom; //Do we want this???
  }
  internal class SettingsEntityButtonVM : SettingsEntityVM
  {
    private readonly UISettingsEntityButton buttonEntity;

    public string Text => buttonEntity.ButtonText;

    internal SettingsEntityButtonVM(UISettingsEntityButton buttonEntity) : base(buttonEntity)
    {
      this.buttonEntity = buttonEntity;
    }
    public void PerformClick()
    {
      buttonEntity.OnClick?.Invoke();
    }

  }
  internal class SettingsEntityButtonView : SettingsEntityView<SettingsEntityButtonVM>, IPointerEnterHandler, IPointerExitHandler
  {
    private static readonly FieldInfo OverrideType = AccessTools.Field(typeof(VirtualListLayoutElementSettings), "m_OverrideType");
    public override VirtualListLayoutElementSettings LayoutSettings
    {
      get
      {
        bool set_mOverrideType = m_LayoutSettings == null;
        m_LayoutSettings ??= new()
        {
          Height = 64,
          Width = 64,
          OverrideHeight = false,
          OverrideWidth = false,
        };
        if (set_mOverrideType)
          OverrideType.SetValue(m_LayoutSettings, VirtualListLayoutElementSettings.LayoutOverrideType.UnityLayout);

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

      SetupColor(false);
    }

    private Color NormalColor = Color.clear;
    private Color OddColor = new Color(0.77f, 0.75f, 0.69f, 0.29f);
    private Color HighlightedColor = new Color(0.52f, 0.52f, 0.52f, 0.29f);
    public Image HighlightedImage;
    public TextMeshProUGUI Title;
    public OwlcatButton Button;
    public TextMeshProUGUI ButtonLabel;

    private void SetupColor(bool isHighlighted)
    {
      Color color = this.NormalColor;
      if (this.HighlightedImage != null)
      {
        this.HighlightedImage.color = (isHighlighted ? this.HighlightedColor : color);
      }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
      EventBus.RaiseEvent<ISettingsDescriptionUIHandler>(delegate (ISettingsDescriptionUIHandler h)
      {
        h.HandleShowSettingsDescription(base.ViewModel.Title, base.ViewModel.Description);
      }, true);
      this.SetupColor(true);
    }

    // Token: 0x06005A86 RID: 23174 RVA: 0x0017D98A File Offset: 0x0017BB8A
    public void OnPointerExit(PointerEventData eventData)
    {
      EventBus.RaiseEvent<ISettingsDescriptionUIHandler>(delegate (ISettingsDescriptionUIHandler h)
      {
        h.HandleHideSettingsDescription();
      }, true);
      this.SetupColor(false);
    }
  }


}

