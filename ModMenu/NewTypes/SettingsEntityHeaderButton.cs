using HarmonyLib;
using Kingmaker.UI.MVVM._PCView.Settings.Entities;
using Kingmaker.UI.MVVM._VM.Settings;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Kingmaker.UI.SettingsUI;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.Controls.SelectableState;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace ModMenu.NewTypes
{
  internal class UISettingsEntityHeaderButton : UISettingsEntityBase
  {
    public override SettingsListItemType? Type => SettingsListItemType.Custom;
  }

  internal class SettingsEntityHeaderButtonVM : SettingsEntityVM
  {
    internal readonly List<VirtualListElementVMBase> SettingsInGroup = new();

    internal SettingsEntityHeaderButtonVM(UISettingsEntityHeaderButton buttonEntity) : base(buttonEntity) { }

    internal void Collapse()
    {
      foreach (var entityVM in SettingsInGroup)
      {
        entityVM.Active.Value = false;
      }
    }

    internal void Expand()
    {
      foreach (var entityVM in SettingsInGroup)
      {
        entityVM.Active.Value = true;
      }
    }
  }

  internal class SettingsEntityHeaderButtonView : SettingsEntityView<SettingsEntityHeaderButtonVM>
  {
    private static readonly FieldInfo OverrideType =
      AccessTools.Field(typeof(VirtualListLayoutElementSettings), "m_OverrideType");

    public override VirtualListLayoutElementSettings LayoutSettings
    {
      get
      {
        bool set_mOverrideType = m_LayoutSettings == null;
        m_LayoutSettings ??=
          new()
          {
            // Set the height to 0 so it doesn't actually take a row.
            Height = 0,
            OverrideHeight = true,
          };
        if (set_mOverrideType)
        {
          OverrideType.SetValue(m_LayoutSettings, VirtualListLayoutElementSettings.LayoutOverrideType.Custom);
        }

        return m_LayoutSettings;
      }
    }
    private VirtualListLayoutElementSettings m_LayoutSettings;

    internal static readonly Sprite ExpandedButton = Helpers.CreateSprite("ModMenu.Assets.ExpandedButton.png");
    internal static readonly Sprite CollapsedButton = Helpers.CreateSprite("ModMenu.Assets.CollapsedButton.png");

    public OwlcatButton Button;
    public Image ButtonImage;

    public override void BindViewImplementation()
    {
      Button.OnLeftClick.RemoveAllListeners();
      Button.OnLeftClick.AddListener(() =>
      {
        if (Expanded)
          Collapse();
        else
          Expand();
        Expanded = !Expanded;
      });

      // Set initial state
      if (Expanded)
        Expand();
      else
        Collapse();
    }

    private bool Expanded = false;

    private void Collapse()
    {
      ViewModel.Collapse();
      ButtonImage.sprite = CollapsedButton;
    }

    private void Expand()
    {
      ViewModel.Expand();
      ButtonImage.sprite = ExpandedButton;
    }
  }
}
