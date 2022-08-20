using Kingmaker.UI.SettingsUI;
using ModMenu.Settings;
using Owlcat.Runtime.UI.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace ModMenu.NewTypes
{
  internal class UISettingsEntityImage : UISettingsEntityBase
  {
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
    protected override void DisposeImplementation()
    {
    }
  }

  internal class SettingsEntityImageView : VirtualListElementViewBase<SettingsEntityImageVM>
  {
    protected override void BindViewImplementation()
    {
      var image = gameObject.AddComponent<Image>();
      image.sprite = TestSettings.Create();
    }

    protected override void DestroyViewImplementation()
    {
    }
  }
}
