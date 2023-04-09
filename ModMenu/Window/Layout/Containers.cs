using Kingmaker.UI;
using ModMenu.Utils;
using ModMenu.Window.Views;
using UnityEngine;
using UnityEngine.UI;

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
      //transform.gameObject.AddComponent<Image>().color = Color.blue;
      BindVM(transform);
      return transform;
    }

    protected abstract Transform InstantiateInternal();

    protected abstract void BindVM(Transform transform);
  }

  /// <summary>
  /// DataGrid layout container
  /// </summary>
  internal class CharInfoFeatureGrid : BaseContainer
  {
    private readonly WindowBuilder.GetFeatures FeatureProvider;
    private readonly GridStyle Style;

    internal CharInfoFeatureGrid(
      WindowBuilder.GetFeatures featureProvider,
      GridStyle style,
      RelativeLayoutParams layoutParams) : base(layoutParams, ContainerType.Grid)
    {
      FeatureProvider = featureProvider;
      Style = style;
    }

    protected override Transform InstantiateInternal()
    {
      var transform = Object.Instantiate(Prefabs.DataGrid).transform;
      var gridLayout = transform.GetComponentInChildren<GridLayoutGroupWorkaround>();
      Style?.Apply(gridLayout);
      transform.gameObject.CreateComponent<DataGridView>(view => view.Grid = gridLayout.transform);
      transform.Find("Viewport").Rect().offsetMin = Vector2.zero;
      return transform;
    }

    protected override void BindVM(Transform transform)
    {
      var view = transform.gameObject.GetComponent<DataGridView>();
      view.Bind(new(FeatureProvider));
    }
  }
}
