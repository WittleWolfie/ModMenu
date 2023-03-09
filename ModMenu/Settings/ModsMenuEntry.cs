using JetBrains.Annotations;
using Kingmaker.Localization;
using Kingmaker.Modding;
using Kingmaker.UI.SettingsUI;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace ModMenu.Settings
{
  /// <summary>
  /// Wrapper class used to display mod's settings in the ModMenu dropdown. Contains a list of UI Setting Groups and modification's info such as name
  /// </summary>
  public class ModsMenuEntry
  {
#pragma warning disable CS1591 // stupid documentation requests
#pragma warning disable CS0618 // Method is obolete. I know! I made it obsolete!
    internal static ModsMenuEntry EmptyInstance = new(new UISettingsGroup() { Title = new() { m_Key = "" }, SettingsList = Array.Empty<UISettingsEntityBase>() });

    internal readonly IEnumerable<UISettingsGroup> ModSettings;
    internal readonly Info ModInfo;

    internal ModsMenuEntry() => ModInfo = new(Helpers.EmptyString, Helpers.EmptyString);


    /// <summary>
    /// Creates a simpliest ModEntry out of a single UI setting group. 
    /// Mod entry will use the group's title as displayed name or  will be called anonymous if title is empty
    /// </summary>
    /// <exception cref="ModMenu_EmptySettingsGroupException"></exception>You can't create a ModEntry out of a null settings group  ¯\_(ツ)_/¯
    [Obsolete("Please, use ModsMenuEntry(Info modInfo, UISettingsGroup settingGroup) instead. You will not be able to change ModInfo later.")]
    public ModsMenuEntry([NotNull]UISettingsGroup settingGroup)
    {
      if (settingGroup is null)
      {
        throw new ModMenu_EmptySettingsGroupException(message: $"A null UISettingsGroup is trying to create a ModsMenuEntry out of itself without any additional mod info provided.");
      }

      if (settingGroup.SettingsList.Length == 0)
      {
        string name = ("titled " + settingGroup.Title) ?? "without a title";
        throw new ModMenu_EmptySettingsGroupException(message: $"UISettingsGroup {name} is trying to create a ModsMenuEntry out of itself without any additional mod info provided.");
      }


      ModSettings  = new UISettingsGroup[1] {settingGroup};
       ModInfo = new(settingGroup.Title);
    }

    /// <summary>
    /// Creates a ModEntry out of a several UI setting group. 
    /// Mod entry will use the first group's title as displayed name or will be called anonymous if title is empty
    /// </summary>
    /// <exception cref="ModMenu_EmptySettingsGroupException"></exception> settingGroups argument must not be null, have at least 1 group in it and none of groups can be null
    [Obsolete("Please, use ModsMenuEntry(Info modInfo, IEnumerable<UISettingsGroup> settingGroups) instead. You will not be able to change ModInfo later.")]
    public ModsMenuEntry(IEnumerable<UISettingsGroup> settingGroups)
    {
      if (settingGroups is null || settingGroups.Count() == 0)
      {
        throw new ModMenu_EmptySettingsGroupException(message: $"A null list of UISettingsGroups is trying to create a ModsMenuEntry out of itself without any additional mod info provided.");
      }
      if (settingGroups.Any(g => g is null))
      {
        throw new ModMenu_EmptySettingsGroupException(message: $"A list of UISettingsGroups is trying to create a ModsMenuEntry out of itself, but contains a null group.");
      }
      UISettingsGroup firstGroup = settingGroups.ElementAt(0);


      if (firstGroup.SettingsList.Length == 0)
      {
        string name = ("titled " + firstGroup.Title) ?? "without a title";
        throw new ModMenu_EmptySettingsGroupException(message: $"UISettingsGroup {name} is trying to create a ModsMenuEntry out of itself without any additional mod info provided.");
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
    /// </summary>>
    /// <param name="ModName"></param> Name of your mod which will be displayed in the ModMenu dropdown. If you don't provide any, it will displayed as Anonymous
    /// <param name="settingGroup"></param>
    /// <exception cref="ModMenu_EmptySettingsGroupException"></exception>You can't create a ModEntry out of a null settings group  ¯\_(ツ)_/¯
    public ModsMenuEntry( LocalizedString ModName, [NotNull]UISettingsGroup settingGroup) : this(settingGroup)
    {
      ModInfo = new(ModName);
    }

    private class ModMenu_EmptySettingsGroupException : Exception
    {

      internal ModMenu_EmptySettingsGroupException()
      {
      }

      internal ModMenu_EmptySettingsGroupException(string message) : base(message)
      {
      }

      internal ModMenu_EmptySettingsGroupException(string message, Exception inner) : base(message, inner)
      {
      }

      internal ModMenu_EmptySettingsGroupException(SerializationInfo info, StreamingContext context) : base(info, context)
      {
      }
    }

    /// <summary>
    /// Structure containing information required to display the ModEntry in the ModMenu dropdown (such as mod's info)
    /// </summary>
    public struct Info
    {
      private static readonly LocalizedString AnonymousMod = Helpers.CreateString("ModsMenu.AnonymousMod", "Anonymous Mod", ruRU: "Безымянный мод");
      private static readonly LocalizedString stringAuthor = Helpers.CreateString("ModsMenu.stringAuthor", "Author", ruRU: "Создатель");
      private static readonly LocalizedString stringVer = Helpers.CreateString("ModsMenu.stringVer", "Version", ruRU: "Версия");
      private const string anonMod = "AnonymousMod";
      private static int AnonymousCounter = 0;

      public Sprite ModImage { get; private set; }
      public LocalizedString ModName { get; private set; }
      public string VersionNumber { get; private set; }
      public string AuthorName { get; private set; }
      public LocalizedString LocalizedModDescription { get; private set; }
      public string NonLocalizedModDescription { get; private set; }
      internal bool AllowModDisabling { get; set; }
      internal OwlcatModification OwlMod { get; }
      internal UnityModManagerNet.UnityModManager.ModEntry UMMMod { get; }



      /// <summary>
      /// </summary>
      /// <param name="Name"></param> Name of your mod which will be displayed in the ModMenu dropdown. If you don't provide any, it will displayed as Anonymous
      /// <param name="Description"></param> Description which will be displayed when user selects your mod in ModMenu dropdown
      /// <param name="version"></param> Mod version to be displayed alongside the description
      /// <param name="author"></param> Mod author's name to be displayed alongside the description
      /// <param name="image"></param> Mod's icon to be displayed alongside the description
      public Info(LocalizedString Name, LocalizedString Description = null, string version = "", string author = "", Sprite image = null)
      {
        if (Name is null) ModName = AnonymousMod;
        else ModName = Name;
        VersionNumber = version;
        AuthorName = author;
        ModImage = image;
        LocalizedModDescription = Description;
      }

      /// <summary>
      /// </summary>
      /// <param name="Name"></param> Name of your mod which will be displayed in the ModMenu dropdown. If you don't provide any, it will displayed as Anonymous
      /// <param name="Description"></param> Description which will be displayed when user selects your mod in ModMenu dropdown
      /// <param name="version"></param> Mod version to be displayed alongside the description
      /// <param name="author"></param> Mod author's name to be displayed alongside the description
      /// <param name="image"></param> Mod's icon to be displayed alongside the description
      public Info(string Name, string Description = "", string version = "", string author = "", Sprite image = null) : this(Helpers.CreateString($"ModsMenu.{Name}.Name", Name), null, version, author, image) 
      {
        if (Name.IsNullOrEmpty())
        {
          Name = anonMod + AnonymousCounter;
          AnonymousCounter++;
        }
        NonLocalizedModDescription = Description; 
      }

      /// <summary>
      /// </summary>
      /// <param name="owlcatModification"></param> if your mod is an OwlcatModification, you may provide a link to it
      /// in this case the constructor will use the information from manifest to set mod's name, description, author and version.
      /// <param name="allowModDisabling"></param> Set to true if you want to have a button in the ModMenu to disable your mod
      /// <param name="LocalizedModName"></param> Localized name for your mod if you are mot satisfied with the non localized name taken from the mod manifest
      /// <param name="LocalizedModDescription"></param> Localized description for your mod if you are mot satisfied with the non localized description taken from the mod manifest
      /// <param name="image"></param> Mod's icon to be displayed alongside the description
      /// <exception cref="ModMenu_EmptyModException"></exception> You are not allowed to provide a null OwlcatModification when using this constructor
      public Info([NotNull]OwlcatModification owlcatModification, bool allowModDisabling, LocalizedString LocalizedModName = null, LocalizedString LocalizedModDescription = null, Sprite image = null)
      {
        if (owlcatModification is null)
          throw new ModMenu_EmptyModException("Attempt to create ModInfo out of a null OwlcatModification");
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

        if (LocalizedModName is not null && LocalizedModName.ToString().IsNullOrEmpty() is false) 
          ModName = LocalizedModName;
        else
        {
          ModName = manifest.DisplayName.IsNullOrEmpty() is false
            ? Helpers.CreateString($"ModsMenu.{uniqueName}.ModName", manifest.DisplayName)
            : AnonymousMod;
        }
        if (LocalizedModDescription is not null && LocalizedModDescription.ToString().IsNullOrEmpty() is false)
          this.LocalizedModDescription = LocalizedModDescription;
        else if (manifest.Description.IsNullOrEmpty() is false)
        {
          NonLocalizedModDescription = Helpers.CreateString($"ModsMenu.{uniqueName}.Description", manifest.DisplayName);
        }

        ModImage = image;
      }

      /// <summary>
      /// </summary>
      /// <param name="UMM_Mod"></param>if your mod is an UMM mod, you may provide a link to it
      /// in this case the constructor will use the information from manifest to set mod's name, author and version.
      /// <param name="allowModDisabling"></param> Set to true if you want to have a button in the ModMenu to disable your mod
      /// <param name="LocalizedModName"></param> Localized name for your mod if you are mot satisfied with the non localized name taken from the mod manifest
      /// <param name="LocalizedModDescription"></param> Localized description for your mod
      /// <param name="image"></param> Mod's icon to be displayed alongside the description
      /// <exception cref="ModMenu_EmptyModException"></exception> You are not allowed to provide a null UnityModManager.ModEntry when using this constructor
      public Info([NotNull] UnityModManagerNet.UnityModManager.ModEntry UMM_Mod, bool allowModDisabling, LocalizedString LocalizedModName = null, LocalizedString LocalizedModDescription = null, Sprite image = null)
      {
        if (UMM_Mod is null)
          throw new ModMenu_EmptyModException("Attempt to create ModInfo out of a null UMM mod");
        UMMMod = UMM_Mod;
        AllowModDisabling = allowModDisabling;
        UnityModManagerNet.UnityModManager.ModInfo info = UMM_Mod.Info;
        AuthorName = info.Author;
        VersionNumber = info.Version;
        string uniqueName = info.Id;
        if (uniqueName.IsNullOrEmpty())
        {
          uniqueName = "AnonymousMod" + AnonymousCounter;
          AnonymousCounter++;
        }
        AuthorName = info.Author;

        if (LocalizedModName is not null && LocalizedModName.ToString().IsNullOrEmpty() is false)
          ModName = LocalizedModName;
        else if (info.DisplayName.IsNullOrEmpty() is false)
        {
          ModName = Helpers.CreateString($"ModsMenu.{uniqueName}.ModName", info.DisplayName);
        }
        else ModName = AnonymousMod;
        if (LocalizedModDescription is not null && LocalizedModDescription.ToString().IsNullOrEmpty() is false)
          this.LocalizedModDescription = LocalizedModDescription;

        ModImage = image;
      }


      internal string GenerateDescription()
      {
        string result = ModName;
        if (!string.IsNullOrEmpty(VersionNumber)) result = $"{result} ({stringVer}: {VersionNumber})";
        result = $"<align=\"center\"><color=#{Color.black}><size=140%><b>{result}</b></size></color></align>\n";
        if (!string.IsNullOrEmpty(AuthorName)) result = string.Concat(result, $"<align=\"center\"><color=#{Color.black}><size=120%>{stringAuthor}: {AuthorName}</size></color></align>\n");
        string.Concat(result, LocalizedModDescription);

        return result;
      }


      private class ModMenu_EmptyModException : Exception
      {
        internal ModMenu_EmptyModException() { }
        internal ModMenu_EmptyModException(string message) : base(message) { }
        internal ModMenu_EmptyModException(string message, Exception inner) : base(message, inner) { }
        internal ModMenu_EmptyModException(SerializationInfo info, StreamingContext context) : base(info, context) { }
      }

    }

  }
}
