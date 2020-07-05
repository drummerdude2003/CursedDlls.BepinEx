# Cursed.dll plugin port for H3VR

This is a collection of plugins that port all the functionalities of the original 
Cursed DLLs into Harmony patches.

The main benefits of this collection are

* All patches are applied at runtime without permanently modifying the original game assemblies
    * That means the patches survive game updates much better
* All patches are separated into plugins, so it's possible to choose the only features that you want

## Installation

This plugin collection relies on [BepInEx](https://github.com/BepInEx/BepInEx) -- 
a Unity plugin framework that comes prepackaged with necessary tools for modding.

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
4. Open the downloaded zip. Pick the plugins you want (**check the descriptions below**) and put them into `<H3VR folder>/BepInEx/plugins` folder.
5. Run the game and enjoy the madness


## Plugin descriptions

#### `Cursed.FullAuto` -- make closed-bolt rifles and handguns be full-auto

Makes all closed bolt rifles and handguns act as full-auto.


#### `Cursed.RemoveMagCheck` -- allows any magazine to be used on any gun

This change makes any magazine usable on any gun. Why not?

#### `Cursed.RemoveRoundTypeCheck` -- allows any round to be used in any gun

Carrots in an SMG? Oh my!

#### `Cursed.TimeScale` -- allows to speed up or slow down the game

Allows to slow down time using the snapturning buttons. When time is slowed down, 
sounds are pitched down as well.

You can edit the amount by which time is slowed down with a single click by editing the configuration file at 
`<H3VR folder>/BepInEx/config/dll.cursed.timescale.cfg`.


#### `Cursed.UnlockAll` -- unlocks all item spawners, rewards and whatnot

Should be rather explanatory, have fun.

## Credits

* [drummerdude2003](https://github.com/drummerdude2003) -- original work
* [denikson](https://github.com/denikson) -- help with porting patches over to Harmony