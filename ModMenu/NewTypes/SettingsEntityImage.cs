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
          Height = 40,
          OverrideHeight = true,
        };
        if (set_mOverrideType)
        {
          OverrideType.SetValue(m_LayoutSettings, VirtualListLayoutElementSettings.LayoutOverrideType.UnityLayout);
        }

        if (ViewModel?.Sprite is not null)
        {
          m_LayoutSettings.Height = ViewModel.Sprite.bounds.size.y;
          m_LayoutSettings.Width = ViewModel.Sprite.bounds.size.x;
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

