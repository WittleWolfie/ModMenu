using Kingmaker.Localization;
using System.Reflection;
using UnityEngine;

namespace ModMenu
{
  /// <summary>
  /// Generic utils for simple operations.
  /// </summary>
  internal static class Helpers
  {
    internal static LocalizedString CreateString(string key, string value)
    {
      var localizedString = new LocalizedString() { m_Key = key };
      LocalizationManager.CurrentPack.PutString(key, value);
      return localizedString;
    }

    internal static Sprite CreateSprite(string embeddedImage)
    {
      var assembly = Assembly.GetExecutingAssembly();
      using var stream = assembly.GetManifestResourceStream(embeddedImage);
      byte[] bytes = new byte[stream.Length];
      stream.Read(bytes, 0, bytes.Length);
      var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
      _ = texture.LoadImage(bytes);
      var sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), Vector2.zero);
      return sprite;
    }
  }
}
