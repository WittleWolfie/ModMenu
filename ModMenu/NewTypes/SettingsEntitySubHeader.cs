using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UI.SettingsUI;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;

namespace ModMenu.NewTypes
{
  internal class UISettingsEntitySubHeader : UISettingsEntityBase
  {
    internal readonly LocalizedString Title;
    internal readonly bool Expanded;

    public UISettingsEntitySubHeader(LocalizedString title, bool expanded)
    {
      Title = title;
      Expanded = expanded;
    }

    public override SettingsListItemType? Type => SettingsListItemType.Custom;
  }

  internal class SettingsEntitySubHeaderVM : SettingsEntityCollapsibleHeaderVM
  {
    public SettingsEntitySubHeaderVM(UISettingsEntitySubHeader headerEntity)
      : base(headerEntity.Title, headerEntity.Expanded) { }
  }

  internal class SettingsEntitySubHeaderView : SettingsEntityCollapsibleHeaderView
  {
    public override VirtualListLayoutElementSettings LayoutSettings
    {
      get
      {
        bool set_mOverrideType = m_LayoutSettings == null;
        m_LayoutSettings ??=
          new()
          {
            Height = 45,
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

    protected override int GetFontSize()
    {
      return 110;
    }
  }
}
