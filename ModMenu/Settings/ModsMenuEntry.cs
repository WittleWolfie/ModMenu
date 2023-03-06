using JetBrains.Annotations;
using Kingmaker.Designers;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Modding;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.Utility;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace ModMenu.Settings
{
  public class ModsMenuEntry
  {
#pragma warning disable CS1291 // stupid documentation requests
#pragma warning disable CS0618 // Method is obolete. I know! I made it obsolete!
    internal static ModsMenuEntry EmptyInstance = new(new UISettingsGroup() { Title = new() { m_Key = "" }, SettingsList = Array.Empty<UISettingsEntityBase>() });

    internal readonly IEnumerable<UISettingsGroup> ModSettings;
    internal readonly Info ModInfo;

    internal ModsMenuEntry() => ModInfo = new(Helpers.EmptyString, Helpers.EmptyString);
    

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
      AddToTheEntityList();
    }

    [Obsolete("Please, use ModsMenuEntry(Info modInfo, IEnumerable<UISettingsGroup> settingGroups) instead. You will not be able to change ModInfo later.")]
    public ModsMenuEntry(IEnumerable<UISettingsGroup> settingGroups)
    {
      if (settingGroups is null || settingGroups.Count() == 0)
      {
        throw new ModMenu_EmptySettingsGroupException(message: $"A null list of UISettingsGroups is trying to create a ModsMenuEntry out of itself without any additional mod info provided.");
      }
      UISettingsGroup firstGroup = settingGroups.ElementAt(0);

      if (firstGroup is null)
      {
        throw new ModMenu_EmptySettingsGroupException(message: $"A null UISettingsGroup is trying to create a ModsMenuEntry out of itself without any additional mod info provided.");
      }

      if (firstGroup.SettingsList.Length == 0)
      {
        string name = ("titled " + firstGroup.Title) ?? "without a title";
        throw new ModMenu_EmptySettingsGroupException(message: $"UISettingsGroup {name} is trying to create a ModsMenuEntry out of itself without any additional mod info provided.");
      }


      ModSettings = new UISettingsGroup[1] { firstGroup };
      ModInfo = new(firstGroup.Title);
      AddToTheEntityList();
    }

    public ModsMenuEntry(Info modInfo, UISettingsGroup settingGroup) : this(settingGroup)
    {
      ModInfo = modInfo;
    }
    public ModsMenuEntry(Info modInfo, IEnumerable<UISettingsGroup> settingGroups) : this(settingGroups)
    {
      ModInfo = modInfo;
    }

    public ModsMenuEntry(LocalizedString ModName, UISettingsGroup settingGroup) : this(settingGroup)
    {
      ModInfo = new(ModName);
    }



    private void AddToTheEntityList()
    {
      ModsMenuEntity.ModEntries.Add(this);
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

    public struct Info
    {
      private static readonly LocalizedString AnonymousMod = Helpers.CreateString("ModsMenu.AnonymousMod", "Anonymous Mod", ruRU: "Безымянный мод");
      private static readonly LocalizedString stringAuthor = Helpers.CreateString("ModsMenu.stringAuthor", "Author", ruRU: "Создатель");
      private static readonly LocalizedString stringVer = Helpers.CreateString("ModsMenu.stringVer", "Version", ruRU: "Версия");
      private const string anonMod = "AnonymousMod";
      private static int AnonymousCounter = 0;

      public Info(LocalizedString Name, LocalizedString Description = null, string version = "", string author = "", Sprite image = null)
      {
        if (Name is null) ModName = AnonymousMod;
        else ModName = Name;
        VersionNumber = version;
        AuthorName = author;
        ModImage = image;
        ModDescription = Description;
      }

      public Info(string Name, string version = "", string author = "", Sprite image = null) : this((Name.IsNullOrEmpty() ? AnonymousMod : Helpers.CreateString($"ModsMenu.{Name}.Name", Name)), null, version, author, image) 
      {

      }
      public Info(string Name, string Description, string version = "", string author = "", Sprite image = null) : this(Name, version, author, image) 
      {
        if (Name.IsNullOrEmpty())
        {
          Name = anonMod + AnonymousCounter;
          AnonymousCounter++;
        }
        ModDescription = Helpers.CreateString($"ModsMenu.{Name}.Description", Description); 
      }

      public Info(OwlcatModification owlcatModification, bool allowModDisabling, LocalizedString LocalizedModName = null, LocalizedString LocalizedModDescription = null, Sprite image = null)
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
        AuthorName = manifest.Author;

        if (LocalizedModName is not null && LocalizedModName.ToString().IsNullOrEmpty() is false) 
          ModName = LocalizedModName;
        else if (manifest.DisplayName.IsNullOrEmpty() is false)
        {
          ModName = Helpers.CreateString($"ModsMenu.{uniqueName}.ModName", manifest.DisplayName);
        }
        else ModName = AnonymousMod;
        if (LocalizedModDescription is not null && LocalizedModDescription.ToString().IsNullOrEmpty() is false)
          ModDescription = LocalizedModDescription;
        else if (manifest.Description.IsNullOrEmpty() is false)
        {
          ModDescription = Helpers.CreateString($"ModsMenu.{uniqueName}.Description", manifest.DisplayName);
        }

        ModImage = image;
      }
      public Info(UnityModManagerNet.UnityModManager.ModEntry UMM_Mod, bool allowModDisabling, LocalizedString LocalizedModName = null, LocalizedString LocalizedModDescription = null, Sprite image = null)
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
          ModDescription = LocalizedModDescription;

        ModImage = image;
      }

      public Sprite ModImage { get; private set; }
      public LocalizedString ModName { get; private set; }
      public string VersionNumber { get; private set; }
      public string AuthorName { get; private set; }
      public LocalizedString ModDescription { get; private set; }
      internal bool AllowModDisabling { get; set; }
      internal OwlcatModification OwlMod { get; }
      internal UnityModManagerNet.UnityModManager.ModEntry UMMMod { get; }

      internal string GenerateName()
      {
        string result = ModName;
        if (!string.IsNullOrEmpty(VersionNumber)) result = $"{result} ({stringVer}: {VersionNumber})";
        result = $"<align=\"center\"><color=#{Color.black}><size=140%><b>{result}</b></size></color></align>\n";
        if (!string.IsNullOrEmpty(AuthorName)) result = string.Concat(result, $"<align=\"center\"><color=#{Color.black}><size=120%>{stringAuthor}: {AuthorName}</size></color></align>\n");
        string.Concat(result, ModDescription);

        return result;
      }


      private class ModMenu_EmptyModException : Exception
      {

        internal ModMenu_EmptyModException()
        {
        }

        internal ModMenu_EmptyModException(string message) : base(message)
        {
        }

        internal ModMenu_EmptyModException(string message, Exception inner) : base(message, inner)
        {
        }

        internal ModMenu_EmptyModException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
      }

    }

  }
}
