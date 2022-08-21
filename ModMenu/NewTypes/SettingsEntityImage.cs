using Kingmaker.UI.SettingsUI;
using Owlcat.Runtime.UI.MVVM;
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
    protected override void BindViewImplementation()
    {
      var image = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
      image.sprite = ViewModel.Sprite;
      image.preserveAspect = true;
    }

    protected override void DestroyViewImplementation()
    {
    }
  }
}
