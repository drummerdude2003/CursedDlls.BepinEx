# Cursed.dll plugin port for H3VR

This is a collection of plugins that port all the functionalities of the original 
Cursed DLLs into Harmony patches.

The main benefits of this collection are

* All patches are applied at runtime without permanently modifying the original game assemblies
    * That means the patches survive game updates much better
* All patches are separated into plugins, so it's possible to only choose the features that you want

## Installation

Requirements are:

* [BepInEx 5.2 or newer](https://github.com/BepInEx/BepInEx/releases)
* [BepInEx.MonoMod.Loader](https://github.com/BepInEx/BepInEx.MonoMod.Loader/releases)
* [BepInEx.ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases)

### For the impatient:

[View installation guide video (check the full guide below too!)](guide.webm)

### Full guide:

To install, do the following:

1. Grab latest version of BepInEx [from releases](https://github.com/BepInEx/BepInEx/releases). Pick the `x64` version.
2. Extract the downloaded zip into H3VR game folder (`<STEAM folder>/steamapps/common/H3VR`) so that `winhttp.dll` is next to `h3vr.exe`
      * It's recommended that you run the game now *at least once*. That way BepInEx initializes all the folders and configuration files.
      * *Optional* Enable the debug console by opening `<H3VR folder>/BepInEx/config/BepInEx.cfg`, finding and setting
         ```toml
         [Logging.Console]

         Enabled = true
         ```
3. Download latest set of Cursed.dll plugins from [releases](https://github.com/drummerdude2003/CursedDlls.BepinEx/releases)
4. Open the downloaded zip. Extract the downloaded zip into your **H3VR** folder. If you did it correctly, you should now have `CursedDlls` folder in the `BepInEx/plugins` folder.
5. Download BepInEx.MonoMod.Loader from its [releases](https://github.com/BepInEx/BepInEx.MonoMod.Loader/releases) and extract the zip into your **H3VR** folder. If you did it correctly, you should now have a `monomod` folder in `BepInEx`.
6. Download BepInEx.ConfigurationManager from its [releases](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases) and extract the zip into your **H3VR** folder. If you did it correctly, you should now have a `ConfigurationManager.dll` file in `BepInEx/plugins`.
7. *Optional* Select plugins you want. Go to `BepInEx/plugins/CursedDlls` and remove plugins you don't want. **Reference the list below for explanation of each plugin**
8. Run the game and enjoy the madness


## Plugin descriptions

#### `Assembly-CSharp.Cursed.RemoveRoundTypeCheck.mm` -- adds a type field to all rounds

Doesn't add any features by itself, but is required for some plugins to work

#### `Cursed.FullAuto` -- make closed-bolt rifles and handguns be full-auto

Makes all closed bolt rifles and handguns have a full auto safety setting. This excludes:
* Handguns without a safety (I.E. Glocks, TT-33, etc)
* Semi Auto Shotguns (full auto with those is handelled differently currently unknown to us, might be with slam fire)

#### `Cursed.BetterBipods` -- improves bipod recoil

Bipods now have balanced recoil, more rearward than upward

#### `Cursed.RemoveAttachmentChecks` -- removes checks related to attachments

* All attachments are bi-directional
* You can apply any attachment on any weapon in any attachment point
* There is no upper bound on the number of attachments per weapon
* If easy mag reloading is enabled, attachments will phase through objects to create yet more cursed guns

#### `Cursed.RemoveMagCheck` -- allows any magazine or clip to be used on any gun

This change makes any magazine or clip usable on any gun. Why not?

#### `Cursed.RemoveRoundTypeCheck` -- allows any round to be used in any gun or any magazine/clip

Potatos in an SMG? Oh my! This also adds some extra features to several other things.

* Load any round into any speedloader
* Load any round into the 6Twelve
* Load any round into belt feds
* The ammo spawner can now fill any gun in the game with any ammo
* If an invalid round is created (ie a JHP shotgun shell) it is replaced with a FMJ .45ACP

#### `Cursed.SuppressAssemblyLoadErrors` -- fix errors when loading certain plugins

Enables proper support for tools like [ConfigManager](https://github.com/BepInEx/BepInEx.ConfigurationManager) and [RuntimeUnityEditor](https://github.com/ManlyMarco/RuntimeUnityEditor).

#### `Cursed.TimeScale` -- allows to speed up or slow down the game

Allows to slow down time using the snapturning buttons in the wrist menu. When time is slowed down, sounds are pitched down as well.

You can edit the amount by which time is slowed down with a single click by editing the configuration file at 
`<H3VR folder>/BepInEx/config/dll.cursed.timescale.cfg` (you need to run the game at least once for it to be generated.)

#### `Cursed.UnlockAll` -- unlocks all Rewards, puts non-item spawner items into the item spawner

Should be rather explanatory, have fun. All objects are found under Misc->Backpacks.

You can choose to overwrite your Rewards.txt contents by editing the configuration file at 
`<H3VR folder>/BepInEx/config/dll.cursed.unlockall.cfg` (again, you need to run the game at least once for it to be generated.)

## Credits

* [drummerdude2003](https://github.com/drummerdude2003) -- original work
* [denikson](https://github.com/denikson) -- help with porting patches over to Harmony
* [BlockBuilder57](https://github.com/BlockBuilder57) -- improvements to existing patches and some new type checking patches
* [modeco80](https://github.com/modeco80) -- developed MSBuild targets to automatically copy the plugins to the proper directories
