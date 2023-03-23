using ModMenu.Window.Layout;
using Owlcat.Runtime.UI.MVVM;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModMenu.Window.Views
{
  internal class GridView : ViewBase<GridViewVM>
  {
    private readonly List<Transform> Children = new();

    public override void BindViewImplementation()
    {
      gameObject.SetActive(true);

      ViewModel.Elements.ForEach(BindElement);
    }

    public override void DestroyViewImplementation()
    {
      gameObject.SetActive(false);

      Children.ForEach(GameObject.DestroyImmediate);
    }

    private void BindElement(BaseElement element)
    {
      Children.Add(element.Instantiate(Grid));
    }

    internal Transform Grid; 
  }

  internal class GridViewVM : BaseDisposable, IViewModel
  {
    private readonly GridBuilder Grid;

    internal GridViewVM(GridBuilder grid)
    {
      Grid = grid;
    }

    public override void DisposeImplementation() { }

    internal List<BaseElement> Elements => Grid.Elements;
  }
}
