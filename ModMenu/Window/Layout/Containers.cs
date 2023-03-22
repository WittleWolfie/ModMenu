using ModMenu.Utils;
using UnityEngine;

namespace ModMenu.Window.Layout
{
  internal enum ContainerType
  {
    WidgetList,
  }

  internal abstract class BaseContainer
  {
    internal readonly ContainerType Type;
    protected readonly LayoutParams LayoutParams;

    protected BaseContainer(ContainerType type, LayoutParams layoutParams)
    {
      Type = type;
      LayoutParams = layoutParams;
    }

    internal Transform Instantiate(Transform parent)
    {
      var transform = InstantiateInternal();
      transform.AddTo(parent);
      LayoutParams?.Apply(transform);
      return transform;
    }

    protected abstract Transform InstantiateInternal();
  }

  /// <summary>
  /// Container for flow layout
  /// </summary>
  internal class FlowContainer : BaseContainer
  {
    internal FlowContainer(LayoutParams layoutParams) : base(ContainerType.WidgetList, layoutParams) { }

    protected override Transform InstantiateInternal()
    {
      var transform = Object.Instantiate(Prefabs.WidgetList).transform;
      return transform;
    }
  }
}
