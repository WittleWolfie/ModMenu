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
    internal int Height;

    internal UISettingsEntityImage(Sprite sprite, int height)
    {
      Sprite = sprite;
      Height = height;
    }

    public override SettingsListItemType? Type => SettingsListItemType.Custom;
  }

  internal class SettingsEntityImageVM : VirtualListElementVMBase
  {
    internal Sprite Sprite;
    internal int Height;

    internal SettingsEntityImageVM(UISettingsEntityImage imageEntity)
    {
      Sprite = imageEntity.Sprite;
      Height = imageEntity.Height;
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
        if (ViewModel is null)
        {
          Main.Logger.NativeLog($"Instantiating layout settings.");
          m_LayoutSettings = new()
          {
            // For some reason it breaks if this isn't set. It doesn't work if you set the height without
            // LayoutOverrideType.Custom, but if this is false things are no good.
            OverrideHeight = true,
          };
          OverrideType.SetValue(m_LayoutSettings, VirtualListLayoutElementSettings.LayoutOverrideType.UnityLayout);
        }
        return m_LayoutSettings;
      }
    }

    private void OverrideHeight()
    {
      Main.Logger.NativeLog($"Overriding layout height: {ViewModel.Height}");
      m_LayoutSettings = new()
      {
        OverrideHeight = true,
        Height = ViewModel.Height,
      };

      // Without setting to custom the height is ignored.
      OverrideType.SetValue(m_LayoutSettings, VirtualListLayoutElementSettings.LayoutOverrideType.Custom);
    }

    private VirtualListLayoutElementSettings m_LayoutSettings;

    public Image Icon;

    protected override void BindViewImplementation()
    {
      Icon.sprite = ViewModel.Sprite;
      if (ViewModel.Height > 0)
      {
        // You can't set height to pixels directly so instead you have to scale.
        var spriteHeight = Icon.sprite.bounds.size.y * Icon.sprite.pixelsPerUnit;
        float scaling = ViewModel.Height / spriteHeight;

        Icon.transform.localScale = new Vector3(scaling, scaling);
        OverrideHeight();
      }
    }

    protected override void DestroyViewImplementation() { }
  }
}

