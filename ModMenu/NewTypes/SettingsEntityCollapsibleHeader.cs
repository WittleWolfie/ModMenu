using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._PCView.ServiceWindows.Journal;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Decorative;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using System.Collections.Generic;
using TMPro;

namespace ModMenu.NewTypes
{
  internal class SettingsEntityCollapsibleHeaderVM : SettingsEntityHeaderVM
  {
    internal readonly List<VirtualListElementVMBase> SettingsInGroup = new();

    internal bool Expanded { get; private set; }

    public SettingsEntityCollapsibleHeaderVM(string title, bool expanded = false) : base(title)
    {
      Expanded = expanded;
    }

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

    internal void Toggle(ExpandableCollapseMultiButtonPC button, bool update = true)
    {
      if (update)
        Expanded = !Expanded;

      button.SetValue(Expanded, true);
      if (Expanded)
        Expand();
      else
        Collapse();
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
            Height = 65,
            OverrideHeight = true,
            Padding = new() { Top = 10 }
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

    protected override void BindViewImplementation()
    {
      Title.text = UIUtility.GetSaberBookFormat(ViewModel.Tittle, size: GetFontSize());
      Button.OnLeftClick.RemoveAllListeners();
      Button.OnLeftClick.AddListener(() => ViewModel.Toggle(ButtonPC));
      ViewModel.Toggle(ButtonPC, update: false);
    }

    protected virtual int GetFontSize() { return 140; }

    protected override void DestroyViewImplementation() { }
  }
}
