using Kingmaker.Localization;

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
  }
}
