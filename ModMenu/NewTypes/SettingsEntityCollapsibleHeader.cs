using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._PCView.ServiceWindows.Journal;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Decorative;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using TMPro;
using UniRx;

namespace ModMenu.NewTypes
{
  internal class SettingsEntityCollapsibleHeaderVM : SettingsEntityHeaderVM
  {
    internal readonly List<VirtualListElementVMBase> SettingsInGroup = new();

    public SettingsEntityCollapsibleHeaderVM(string title) : base(title) { }

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

  internal class SettingsEntityCollapsibleHeaderView : VirtualListElementViewBase<SettingsEntityCollapsibleHeaderVM>
  {
    public override VirtualListLayoutElementSettings LayoutSettings
    {
      get
      {
        bool set_mOverrideType = m_LayoutSettings == null;
        m_LayoutSettings ??=
          new()
          {
            // This is the typical header height
            Height = 55,
            OverrideHeight = true,
          };
        if (set_mOverrideType)
        {
          SettingsEntityPatches.OverrideType.SetValue(
            m_LayoutSettings, VirtualListLayoutElementSettings.LayoutOverrideType.Custom);
        }

        return m_LayoutSettings;
      }
    }
    private VirtualListLayoutElementSettings m_LayoutSettings;

    public TextMeshProUGUI Title;
    public OwlcatMultiButton Button;
    public ExpandableCollapseMultiButtonPC ButtonPC;

    private bool Enabled = false;

    protected override void BindViewImplementation()
    {
      Title.text = UIUtility.GetSaberBookFormat(ViewModel.Tittle, default, 140, null, 0f);
      Button.OnLeftClick.RemoveAllListeners();
      Button.OnLeftClick.AddListener(
        () =>
        {
          Enabled = !Enabled;
          ButtonPC.SetValue(Enabled, true);
          if (Enabled)
            ViewModel.Expand();
          else
            ViewModel.Collapse();
        });
    }

    protected override void DestroyViewImplementation() { }
  }
}
