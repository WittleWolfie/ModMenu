using Kingmaker.Localization;
using ModMenu.Utils;
using TMPro;
using UnityEngine;

namespace ModMenu.Window
{
  internal enum ElementType
  {
    Text,
  }

  internal abstract class BaseElement
  {
    internal readonly ElementType Type;
    internal readonly LayoutParams LayoutParams;

    protected BaseElement(LayoutParams layoutParams, ElementType type)
    {
      LayoutParams = layoutParams;
      Type = type;
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
  /// Container for displaying text
  /// </summary>
  internal class TextElement : BaseElement
  {
    internal LocalizedString Text;
    
    internal TextElement(LocalizedString text, LayoutParams layoutParams) : base(layoutParams, ElementType.Text)
    {
      Text = text;
    }

    protected override Transform InstantiateInternal()
    {
      var transform = GameObject.Instantiate(Prefabs.Text).transform;
      transform.GetComponent<TextMeshProUGUI>().SetText(Text);
      return transform;
    }
  }
}
