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

    public override SettingsListItemType? Type
    {
      get
      {
        return new SettingsListItemType?(SettingsListItemType.Custom);
      }
    }
  }

  internal class SettingsEntityImageVM : VirtualListElementVMBase
  {
    internal Sprite Sprite;

    internal SettingsEntityImageVM(UISettingsEntityImage imageEntity)
    {
      Sprite = imageEntity.Sprite;
    }

    protected override void DisposeImplementation()
    {
    }
  }

  internal class SettingsEntityImageView : VirtualListElementViewBase<SettingsEntityImageVM>
  {
    public override VirtualListLayoutElementSettings LayoutSettings
    {
      get
      {
        return m_LayoutSettings;
      }
    }
    private VirtualListLayoutElementSettings m_LayoutSettings;

    private static readonly FieldInfo OverrideType = AccessTools.Field(typeof(VirtualListLayoutElementSettings), "m_OverrideType");

    protected override void BindViewImplementation()
    {
      var image = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
      image.sprite = ViewModel.Sprite;
      image.preserveAspect = true;

      // Experimental
      gameObject.AddComponent<LayoutElement>();
      gameObject.AddComponent<VerticalLayoutGroup>();
      m_LayoutSettings = new();
      m_LayoutSettings.Height = image.preferredHeight;
      m_LayoutSettings.Width = image.preferredWidth;
      m_LayoutSettings.OverrideHeight = true;
      m_LayoutSettings.OverrideWidth = true;
      OverrideType.SetValue(m_LayoutSettings, VirtualListLayoutElementSettings.LayoutOverrideType.UnityLayout);
    }

    protected override void DestroyViewImplementation()
    {
    }
  }
}
