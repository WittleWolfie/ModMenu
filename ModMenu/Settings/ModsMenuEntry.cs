using JetBrains.Annotations;
using Kingmaker.Localization;
using Kingmaker.Modding;
using Kingmaker.UI.SettingsUI;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace ModMenu.Settings
{
  /// <summary>
  /// Wrapper class used to display mod's settings in the ModMenu dropdown. Contains a list of UI Setting Groups and
  /// modification's info such as name.
  /// </summary>
  public class ModsMenuEntry
  {
#pragma warning disable CS1591 // stupid documentation requests
#pragma warning disable CS0618 // Method is obolete. I know! I made it obsolete!
    internal static ModsMenuEntry EmptyInstance =
      new(new UISettingsGroup() { Title = new() { m_Key = "" }, SettingsList = Array.Empty<UISettingsEntityBase>() });

    internal readonly IEnumerable<UISettingsGroup> ModSettings;
    internal readonly Info ModInfo;

    internal ModsMenuEntry() => ModInfo = new(Helpers.EmptyString, Helpers.EmptyString);

    /// <summary>
    /// Creates a simpliest ModEntry out of a single UI setting group. 
    /// Mod entry will use the group's title as displayed name or  will be called anonymous if title is empty
    /// </summary>
    /// <exception cref="ArgumentException">
    /// You can't create a ModEntry out of a null settings group  ¯\_(ツ)_/¯
    /// </exception>
    [Obsolete("Please, use ModsMenuEntry(Info modInfo, UISettingsGroup settingGroup) instead. You will not be able to change ModInfo later.")]
    public ModsMenuEntry([NotNull]UISettingsGroup settingGroup)
    {
      if (settingGroup is null)
      {
        throw new ArgumentException("Cannot create ModsMenuEntry with a null settingsGroup.");
      }

      if (settingGroup.SettingsList.Length == 0)
      {
        string name = ("titled " + settingGroup.Title) ?? "without a title";
        throw new ArgumentException($"UISettingsGroup {name} is trying to create a ModsMenuEntry without mod info.");
      }

      ModSettings  = new UISettingsGroup[1] {settingGroup};
       ModInfo = new(settingGroup.Title);
    }

    /// <summary>
    /// Creates a ModEntry out of a several UI setting group. 
    /// Mod entry will use the first group's title as displayed name or will be called anonymous if title is empty
    /// </summary>
    /// <exception cref="ArgumentException">
    /// settingGroups argument must not be null, have at least 1 group in it and none can be null
    /// </exception> 
    [Obsolete("Use ModsMenuEntry(Info modInfo, IEnumerable<UISettingsGroup> settingGroups). You will not be able to change ModInfo later.")]
    public ModsMenuEntry(IEnumerable<UISettingsGroup> settingGroups)
    {
      if (settingGroups is null || settingGroups.Count() == 0)
      {
        throw new ArgumentException("Cannot create ModsMenuEntry without any settingsGroups.");
      }
      if (settingGroups.Any(g => g is null))
      {
        throw new ArgumentException("Cannot create ModsMenuEntry with a null settingsGroup.");
      }

      UISettingsGroup firstGroup = settingGroups.ElementAt(0);
      if (firstGroup.SettingsList.Length == 0)
      {
        string name = ("titled " + firstGroup.Title) ?? "without a title";
        throw new ArgumentException($"UISettingsGroup {name} is trying to create a ModsMenuEntry without mod info.");
      }

      ModSettings = new UISettingsGroup[1] { firstGroup };
      ModInfo = new(firstGroup.Title);
    }

    public ModsMenuEntry(Info modInfo, UISettingsGroup settingGroup) : this(settingGroup)
    {
      ModInfo = modInfo;
    }

    public ModsMenuEntry(Info modInfo, IEnumerable<UISettingsGroup> settingGroups) : this(settingGroups)
    {
      ModInfo = modInfo;
    }

    /// <summary>
    /// Creates a simple ModEntry out of a single UI setting group. 
    /// </summary>
    /// <param name="modName">
    /// Name of your mod displayed in the ModMenu dropdown. If you don't provide any, it will be Anonymous
    /// </param>
    /// <exception cref="ArgumentException">
    /// You can't create a ModEntry out of a null settings group  ¯\_(ツ)_/¯
    /// </exception>
    public ModsMenuEntry(LocalizedString modName, [NotNull]UISettingsGroup settingGroup) : this(settingGroup)
    {
      ModInfo = new(modName);
    }

    /// <summary>
    /// Structure containing information required to display the ModEntry in the ModMenu dropdown (such as mod's info)
    /// </summary>
    public struct Info
    {
      private static readonly LocalizedString AnonymousMod =
        Helpers.CreateString("ModsMenu.AnonymousMod", "Anonymous Mod", ruRU: "Безымянный мод");
      private static readonly LocalizedString stringAuthor =
        Helpers.CreateString("ModsMenu.stringAuthor", "Author", ruRU: "Создатель");
      private static readonly LocalizedString stringVer =
        Helpers.CreateString("ModsMenu.stringVer", "Version", ruRU: "Версия");
      private static int AnonymousCounter = 0;

      public Sprite ModImage { get; private set; }
      public LocalizedString ModName { get; private set; }
      public string VersionNumber { get; private set; }
      public string AuthorName { get; private set; }
      public LocalizedString LocalizedModDescription { get; private set; }
      public string NonLocalizedModDescription { get; private set; }
      private string ModDescription { get { return LocalizedModDescription ?? NonLocalizedModDescription; } }
      internal bool AllowModDisabling { get; set; }
      internal OwlcatModification OwlMod { get; }
      internal UnityModManager.ModEntry UMMMod { get; }

      /// <param name="name">
      /// Name of your mod displayed in the ModMenu dropdown. If you don't provide any, it will be Anonymous
      /// </param>
      /// <param name="description">
      /// Description which will be displayed when user selects your mod in ModMenu dropdown
      /// </param>
      /// <param name="version">
      /// Mod version to be displayed alongside the description
      /// </param>
      /// <param name="author">
      /// Mod author's name to be displayed alongside the description
      /// </param>
      /// <param name="image">
      /// Mod's icon to be displayed alongside the description
      /// </param>
      public Info(
        LocalizedString name,
        LocalizedString description = null,
        string version = "",
        string author = "",
        Sprite image = null)
      {
        if (name is null) ModName = AnonymousMod;
        else ModName = name;
        VersionNumber = version;
        AuthorName = author;
        ModImage = image;
        LocalizedModDescription = description;
      }

      /// <param name="name">
      /// Name of your mod which will be displayed in the ModMenu dropdown. If you don't provide any, it will displayed
      /// as Anonymous
      /// </param> 
      /// <param name="description">
      /// Description which will be displayed when user selects your mod in ModMenu dropdown
      /// </param>
      /// <param name="version">Mod version to be displayed alongside the description</param>
      /// <param name="author">Mod author's name to be displayed alongside the description</param>
      /// <param name="image">Mod's icon to be displayed alongside the description</param>
      public Info(
          string name, string description = "", string version = "", string author = "", Sprite image = null) :
        this(Helpers.CreateString($"ModsMenu.{name}.Name", name), null, version, author, image) 
      {
        if (name.IsNullOrEmpty())
          AnonymousCounter++;

        NonLocalizedModDescription = description; 
      }

      /// <param name="owlcatModification">
      /// If your mod is an OwlcatModification, you may provide a link to it in this case the constructor will use the
      /// information from manifest to set mod's name, description, author and version.
      /// </param>
      /// <param name="allowModDisabling">
      /// Set to true if you want to have a button in the ModMenu to disable your mod
      /// </param>
      /// <param name="localizedModName">
      /// Localized name for your mod if you are not satisfied with the non localized name taken from the mod manifest
      /// </param>
      /// <param name="localizedModDescription">
      /// Localized description for your mod if you are mot satisfied with the non localized description taken from the
      /// mod manifest
      /// </param>
      /// <param name="image">Mod's icon to be displayed alongside the description</param>
      /// <exception cref="ArgumentException">
      /// You are not allowed to provide a null OwlcatModification when using this constructor
      /// </exception>
      public Info(
        OwlcatModification owlcatModification,
        bool allowModDisabling,
        LocalizedString localizedModName = null,
        LocalizedString localizedModDescription = null,
        Sprite image = null)
      {
        if (owlcatModification is null)
          throw new ArgumentException("Attempt to create ModInfo out of a null OwlcatModification");

        OwlMod = owlcatModification;
        AllowModDisabling = allowModDisabling;
        OwlcatModificationManifest manifest = owlcatModification.Manifest;
        AuthorName = manifest.Author;
        VersionNumber = manifest.Version;

        string uniqueName = manifest.UniqueName;
        if (uniqueName.IsNullOrEmpty())
        {
          uniqueName = "AnonymousMod" + AnonymousCounter;
          AnonymousCounter++;
        }

        if (localizedModName is not null && localizedModName.ToString().IsNullOrEmpty() is false) 
          ModName = localizedModName;
        else
        {
          ModName = manifest.DisplayName.IsNullOrEmpty() is false
            ? Helpers.CreateString($"ModsMenu.{uniqueName}.ModName", manifest.DisplayName)
            : AnonymousMod;
        }

        if (localizedModDescription is not null && localizedModDescription.ToString().IsNullOrEmpty() is false)
          LocalizedModDescription = localizedModDescription;
        else if (manifest.Description.IsNullOrEmpty() is false)
          NonLocalizedModDescription = Helpers.CreateString($"ModsMenu.{uniqueName}.Description", manifest.DisplayName);

        ModImage = image;
      }

      /// <param name="ummMod"></param>if your mod is an UMM mod, you may provide a link to it
      /// in this case the constructor will use the information from manifest to set mod's name, author and version.
      /// <param name="allowModDisabling"></param> Set to true if you want to have a button in the ModMenu to disable your mod
      /// <param name="localizedModName"></param> Localized name for your mod if you are mot satisfied with the non localized name taken from the mod manifest
      /// <param name="localizedModDescription"></param> Localized description for your mod
      /// <param name="image"></param> Mod's icon to be displayed alongside the description
      /// <exception cref="ArgumentException"></exception> You are not allowed to provide a null UnityModManager.ModEntry when using this constructor
      public Info(
        UnityModManager.ModEntry ummMod,
        bool allowModDisabling,
        LocalizedString localizedModName = null,
        LocalizedString localizedModDescription = null,
        Sprite image = null)
      {
        if (ummMod is null)
          throw new ArgumentException("Attempt to create ModInfo out of a null UMM mod");

        UMMMod = ummMod;
        AllowModDisabling = allowModDisabling;
        UnityModManager.ModInfo info = ummMod.Info;
        AuthorName = info.Author;
        VersionNumber = info.Version;

        string uniqueName = info.Id;
        if (uniqueName.IsNullOrEmpty())
        {
          uniqueName = "AnonymousMod" + AnonymousCounter;
          AnonymousCounter++;
        }
        AuthorName = info.Author;

        if (localizedModName is not null && localizedModName.ToString().IsNullOrEmpty() is false)
          ModName = localizedModName;
        else if (info.DisplayName.IsNullOrEmpty() is false)
          ModName = Helpers.CreateString($"ModsMenu.{uniqueName}.ModName", info.DisplayName);
        else
          ModName = AnonymousMod;

        if (localizedModDescription is not null && localizedModDescription.ToString().IsNullOrEmpty() is false)
          LocalizedModDescription = localizedModDescription;

        ModImage = image;
      }

      internal string GenerateDescription()
      {
        string result = ModDescription;
        if (!string.IsNullOrEmpty(VersionNumber))
          result = $"{result} ({stringVer}: {VersionNumber})";

        result = $"<align=\"center\"><color=#{Color.black}><size=140%><b>{result}</b></size></color></align>\n";
        if (!string.IsNullOrEmpty(AuthorName))
          result = string.Concat(result, $"<align=\"center\"><color=#{Color.black}><size=120%>{stringAuthor}: {AuthorName}</size></color></align>\n");
        
        string.Concat(result, LocalizedModDescription);
        return result;
      }
    }
  }
}
