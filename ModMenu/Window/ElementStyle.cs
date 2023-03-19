using ModMenu.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UnityEngine;

namespace ModMenu.Window
{
  public class TextStyle
  {
    private readonly TextAlignmentOptions Alignment;
    private readonly Color? Color;

    public TextStyle(TextAlignmentOptions alignment = TextAlignmentOptions.Center, Color? color = null)
    {
      Alignment = alignment;
      Color = color;
    }

    internal void Apply(TextMeshProUGUI text)
    {
      text.alignment = Alignment;
      if (Color is not null)
        text.color = Color.Value;
    }
  }

  public class ButtonStyle
  {
    private readonly TextStyle TextStyle;

    public ButtonStyle(TextStyle textStyle)
    {
      TextStyle = textStyle;
    }

    internal void Apply(OwlcatButton button)
    {
      var text = button.gameObject.ChildObject("Text").GetComponent<TextMeshProUGUI>();
      TextStyle.Apply(text);
    }
  }
}
