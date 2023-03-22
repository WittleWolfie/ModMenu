using Kingmaker.Localization;
using ModMenu.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UnityEngine;

namespace ModMenu.Window.Layout
{
  internal enum ElementType
  {
    Text,
    Button,
  }

  internal abstract class BaseElement
  {
    internal readonly ElementType Type;
    protected readonly LayoutParams LayoutParams;

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
  /// Element for displaying text
  /// </summary>
  internal class TextElement : BaseElement
  {
    private readonly LocalizedString Text;
    private readonly TextStyle Style;

    internal TextElement(
      LocalizedString text, TextStyle style, LayoutParams layoutParams) : base(layoutParams, ElementType.Text)
    {
      Text = text;
      Style = style;
    }

    protected override Transform InstantiateInternal()
    {
      var transform = Object.Instantiate(Prefabs.Text).transform;
      var text = transform.GetComponent<TextMeshProUGUI>();
      text.SetText(Text);
      Style?.Apply(text);
      return transform;
    }
  }

  /// <summary>
  /// Called when the button is clicked
  /// </summary>
  /// <param name="id">The ID of the button clicked</param>
  /// <param name="enabled">Whether the button state is currently interactable</param>
  /// <param name="doubleClick">True when it is a double click</param>
  public delegate void OnClick(string id, bool enabled = true, bool doubleClick = false);

  internal class ButtonElement : BaseElement
  {
    private readonly LocalizedString Text;
    private readonly ButtonStyle Style;

    private readonly OnClick OnLeftClick;
    // Can be null
    private readonly OnClick OnRightClick;

    internal ButtonElement(
        LocalizedString text,
        OnClick onLeftClick,
        ButtonStyle style,
        LayoutParams layoutParams,
        OnClick onRightClick)
      : base(layoutParams, ElementType.Button)
    {
      Text = text;
      OnLeftClick = onLeftClick;
      Style = style;
      OnRightClick = onRightClick;
    }

    protected override Transform InstantiateInternal()
    {
      var transform = Object.Instantiate(Prefabs.Button).transform;
      var button = transform.GetComponent<OwlcatButton>();
      button.gameObject.ChildObject("Text").GetComponent<TextMeshProUGUI>().SetText(Text);

      button.OnLeftClick.AddListener(() => OnLeftClick.Invoke(LayoutParams.ID));
      button.OnLeftClickNotInteractable.AddListener(() => OnLeftClick.Invoke(LayoutParams.ID, enabled: false));
      button.OnLeftDoubleClick.AddListener(() => OnLeftClick.Invoke(LayoutParams.ID, doubleClick: true));
      button.OnLeftDoubleClickNotInteractable.AddListener(() => OnLeftClick.Invoke(LayoutParams.ID, doubleClick: true, enabled: false));

      button.OnRightClick.AddListener(() => OnRightClick?.Invoke(LayoutParams.ID));
      button.OnRightClickNotInteractable.AddListener(() => OnRightClick?.Invoke(LayoutParams.ID, enabled: false));
      button.OnRightDoubleClick.AddListener(() => OnRightClick?.Invoke(LayoutParams.ID, doubleClick: true));
      button.OnRightDoubleClickNotInteractable.AddListener(() => OnRightClick?.Invoke(LayoutParams.ID, doubleClick: true, enabled: false));

      Style?.Apply(button);
      return transform;
    }
  }
}
