# This mod is now maintained here: https://github.com/CasDragon/ModMenu/

# ModMenu

Adds a new page to the game options for mods. This allows mods to easily implement settings using native UI instead of UMM. This does not create a save dependency.

![Test settings screenshot](https://github.com/WittleWolfie/ModMenu/blob/main/test_settings.png)

![More settings screenshot](https://github.com/WittleWolfie/ModMenu/blob/main/more_settings.png)

## Installation

1. Install [Unity Mod Manager](https://github.com/newman55/unity-mod-manager) (UMM), minimum version 0.23.0, and configure for use with Wrath
2. Install [ModFinder](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder) and use it to search for Mewsifer Console
3. Click "Install"

If you don't want to use ModFinder you can download the [latest release](https://github.com/WittleWolfie/ModMenu/releases/latest) and install normally using UMM.

## Problems or Suggestions

File an [issue on GitHub](https://github.com/WittleWolfie/ModMenu/issues/new) or reach out to me (@WittleWolfie) on [Discord](https://discord.com/invite/wotr) in #mod-dev-technical or #mod-user-general channel.

### Controller Support

**This does not support controllers**. It's a lot of work to support, but let me know if you need this. If there is enough demand I will add it.

## Mods Using ModMenu

This is a non-exhaustive list, let me know if you want your mod added here!

* [Added Feats](https://github.com/Telyl/AddedFeats)
* [BOAT BOAT BOAT](https://github.com/Balkoth-dev/WOTR_BOAT_BOAT_BOAT)
* [Character Options+](https://github.com/WittleWolfie/CharacterOptionsPlus)
* [MewsiferConsole](https://github.com/Pathfinder-WOTR-Modding-Community/MewsiferConsole)

## Mod Developers

### Why should you use it?

* It looks nice!
* Automatically persists your settings
* Handles restoring defaults
* Automatically persists per-save settings
* Super easy to use

### How to use it

The screenshot above was generated using [TestSettings](https://github.com/WittleWolfie/ModMenu/blob/main/ModMenu/Settings/TestSettings.cs). That exercises every function supported. The API is documented and generally self-explanatory.

In your mod's `Info.json` add `ModMenu` as a requirement:

```json
"Requirements": ["ModMenu"]
```

You should specify a minimum version:

```json
"Requirements": ["ModMenu-1.1.0"]
```

It's safest to just specify the version you build against as the minimum version, but methods added after 1.0 do specify the version in their remarks.

Install ModMenu then in your mod's project add `%WrathPath%/Mods/ModMenu/ModMenu.dll` as an assembly reference.

### Basic Usage

Create a setting:

```C#
ModMenu.AddSettings(
  SettingsBuilder.New("mymod-settings, SettingsTitle)
    .AddToggle(Toggle.New("mymod-settings-toggle", defaultValue: true, MyToggleTitle)
      .OnValueChanged(OnToggle)));
      
private static void OnToggle(bool toggleValue) {
  // The user just changed the toggle, toggleValue is the new setting.
  // If you need to react to it changing then you can do that here.
  // If you don't need to do something whenever the value changes, you can skip OnValueChanged()
}
```

Get the setting value:

```C#
ModMenu.GetSettingValue<bool>("mymod-settings-toggle");
```

**The game handles the setting value for you.** You do not need to save the setting, or set the setting to a specific value. You *can* set it if necessary but most of the time it isn't necessary. This includes saving settings that you flag as per-save using `DependsOnSave()`.

For more examples see [TestSettings](https://github.com/WittleWolfie/ModMenu/blob/main/ModMenu/Settings/TestSettings.cs).

### Best Practices

* **Do not add settings during mod load,** without additional handling you cannot create a `LocalizedString`. I recommend adding settings before, during, or after `BlueprintsCache.Init()`.
* Don't use `IsModificationAllowed` to enable/disable a setting based on another setting. This is checked when the page is opened so it won't apply immediately.
* Indicate settings which require reboot using `WithLongDescription()`. The game's setting boolean `RequireReboot` does nothing.

Define a "root" key unique to your mod to make sure there are no key conflicts:

```C#
private const string RootKey = "mymod-settings";
```

You can then prepend this to all of your settings keys:

```C#

// Results in a settings key "mymod-settings-key"
var toggle = Toggle.New(GetKey("toggle"), MyToggleTitle);

private static string GetKey(string key)
{
  return $"{RootKey}-{key}";
}
```

Just make sure you always get the key the same way when getting a setting value.

### Settings Behavior

* Settings with `DependsOnSave()` are associated with a save slot, but do not create save dependencies
    * You do not need to handle saving or restoring settings at all, though save dependent settings may be lost if the mod is disabled
* `OnValueChanged()` is called after the user clicks "Apply" and confirms
* `OnTempValueChanged()` is called immediately after the user changes the value, but before it is applied
* A setting's value can be checked at any time by calling [GetSettingValue()](https://github.com/WittleWolfie/ModMenu/blob/main/ModMenu/ModMenu.cs#L85)

## Acknowledgements

* A shout out to Bubbles (factsubio) who essentially wrote the new image and button settings types when I was about to give up.
* The modding community on [Discord](https://discord.com/invite/wotr), an invaluable and supportive resource for help modding.
* All the Owlcat modders who came before me, wrote documents, and open sourced their code.

## Interested in modding?

* Check out the [OwlcatModdingWiki](https://github.com/WittleWolfie/OwlcatModdingWiki/wiki).
* Join us on [Discord](https://discord.com/invite/wotr).
