using HarmonyLib;
using Kingmaker.UI.SettingsUI;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace ModMenu.NewTypes
{
  internal class UISettingsEntityImage : UISettingsEntityBase
  {
    internal Sprite Sprite;

    internal UISettingsEntityImage(Sprite sprite)
    {
      Sprite = sprite;
    }

    public override SettingsListItemType? Type => SettingsListItemType.Custom;
  }

  internal class SettingsEntityImageVM : VirtualListElementVMBase
  {
    internal Sprite Sprite;

    internal SettingsEntityImageVM(UISettingsEntityImage imageEntity)
    {
      Sprite = imageEntity.Sprite;
    }

    protected override void DisposeImplementation() { }
  }

  internal class SettingsEntityImageView : VirtualListElementViewBase<SettingsEntityImageVM>
  {
    private static readonly FieldInfo OverrideType =
      AccessTools.Field(typeof(VirtualListLayoutElementSettings), "m_OverrideType");

    public override VirtualListLayoutElementSettings LayoutSettings
    {
      get
      {
        bool set_mOverrideType = m_LayoutSettings == null;
        m_LayoutSettings ??= new()
        {
          // If this is set to false the row gets all kinds of wonky. Notably setting height has no impact on anything,
          // the row just sizes based on the image.
          OverrideHeight = true,
        };
        if (set_mOverrideType)
        {
          // Note that m_LayoutSettings is largely ignored by using UnityLayout. If it is set to Custom then the height
          // and width parameters in m_LayoutSettings are used.
          OverrideType.SetValue(m_LayoutSettings, VirtualListLayoutElementSettings.LayoutOverrideType.UnityLayout);
        }

        return m_LayoutSettings;
      }
    }

    private VirtualListLayoutElementSettings m_LayoutSettings;

    public Image Image;

    protected override void BindViewImplementation()
    {
      Image.sprite = ViewModel.Sprite;
    }

    protected override void DestroyViewImplementation() { }
  }
}

