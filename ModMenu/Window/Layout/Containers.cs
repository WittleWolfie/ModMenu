using Kingmaker.Localization;
using Kingmaker.UI;
using ModMenu.Utils;
using ModMenu.Window.Views;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UnityEngine;

namespace ModMenu.Window.Layout
{
  internal enum ContainerType
  {
    Grid,
  }

  internal abstract class BaseContainer
  {
    internal readonly ContainerType Type;
    protected readonly LayoutParams LayoutParams;

    protected BaseContainer(LayoutParams layoutParams, ContainerType type)
    {
      LayoutParams = layoutParams;
      Type = type;
    }

    internal Transform Instantiate(Transform parent)
    {
      var transform = InstantiateInternal();
      transform.AddTo(parent);
      LayoutParams?.Apply(transform);
      BindVM(transform);
      return transform;
    }

    protected abstract Transform InstantiateInternal();

    protected abstract void BindVM(Transform transform);
  }

  /// <summary>
  /// Grid layout container
  /// </summary>
  internal class GridContainer : BaseContainer
  {
    private readonly GridBuilder Grid;
    private readonly GridStyle Style;

    internal GridContainer(GridBuilder grid, GridStyle style, AbsoluteLayoutParams layoutParams) : base(layoutParams, ContainerType.Grid)
    {
      Grid = grid;
      Style = style;
    }

    protected override Transform InstantiateInternal()
    {
      var transform = Object.Instantiate(Prefabs.Grid).transform;
      var gridLayout = transform.GetComponentInChildren<GridLayoutGroupWorkaround>();
      Style?.Apply(gridLayout);
      transform.gameObject.CreateComponent<GridView>(view => view.Grid = gridLayout.transform);
      return transform;
    }

    protected override void BindVM(Transform transform)
    {
      var view = transform.gameObject.GetComponent<GridView>();
      view.Bind(new(Grid));
    }
  }
}
