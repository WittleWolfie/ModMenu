# ModMenu

Adds a new page to the game options for mods. This allows mods to easily implement settings using native UI instead of UMM. This does not create a save dependency.

![Test settings screenshot](https://github.com/WittleWolfie/ModMenu/blob/main/test_settings.png)

## Installation

1. Install [Unity Mod Manager](https://github.com/newman55/unity-mod-manager) (UMM), minimum version 0.23.0, and configure for use with Wrath
2. Install [ModFinder](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder) and use it to search for Mewsifer Console
3. Click "Install"

If you don't want to use ModFinder you can download the [latest release](https://github.com/WittleWolfie/ModMenu/releases/latest) and install normally using UMM.

## Problems or Suggestions

File an [issue on GitHub](https://github.com/WittleWolfie/ModMenu/issues/new) or reach out to me (@WittleWolfie) on [Discord](https://discord.com/invite/wotr) in #mod-dev-technical or #mod-user-general channel.

## Mods Using ModMenu

This is a non-exhaustive list, let me know if you want your mod added here!

* [MewsiferConsole](https://github.com/Pathfinder-WOTR-Modding-Community/MewsiferConsole)

## Usage for Mod Developers

The screenshot above was generated using [TestSettings](https://github.com/WittleWolfie/ModMenu/blob/main/ModMenu/Settings/TestSettings.cs). That exercises every function supported. The API is documented and generally self-explanatory.

In your mod's `Info.json` add `ModMenu` as a requirement:

```json
"Requirements": ["ModMenu"]
```

You can optionally specify a minimum version:

```json
"Requirements": ["ModMenu-1.1.0"]
```

Install ModMenu then in your mod's project add `%WrathPath%/Mods/ModMenu/ModMenu.dll` as an assembly reference.

### Best Practices

* **Do not add settings during mod load,** without additional handling you cannot create a `LocalizedString`. I recommend adding settings before, during, or after `BlueprintsCache.Init()`.
* Keep the number of settings to a minimum since the page is shared by all mods. If you have a large number of settings, e.g. ToyBox, it may be better to keep them in UMM.
* Consider adding your own "Default" option using the [Button](https://github.com/WittleWolfie/ModMenu/blob/main/ModMenu/Settings/SettingsBuilder.cs#L95) type. The default button at the bottom of the page applies to every setting on the Mods page.
* Don't use `IsModificationAllowed` to enable/disable a setting based on another setting. This is checked when the page is opened so it won't apply immediately.
* Indicate settings which require reboot using `WithLongDescription()`. The game's setting boolean `RequireReboot` does nothing.

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
