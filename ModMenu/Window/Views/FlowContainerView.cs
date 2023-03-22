using Owlcat.Runtime.UI.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModMenu.Window.Views
{
  // TODO: Consider renaming
  // TODO: How the eff do I get this even once I _have_ a prefab?
  // TODO: How do I define the data model?
  // TODO: Maybe for containers I should just do manually instead of prefabbin'
  internal class FlowContainerView : ViewBase<FlowContainerVM>
  {
    public override void BindViewImplementation()
    {
      gameObject.SetActive(true);
    }

    public override void DestroyViewImplementation()
    {
      gameObject.SetActive(false);
    }
  }

  internal class FlowContainerVM : BaseDisposable, IViewModel
  {
    public override void DisposeImplementation() { }
  }
}
