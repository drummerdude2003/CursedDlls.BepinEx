using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;


namespace MyPlugin
{
[BepInPlugin("com.drummerdude2003.curseddll", "MyPlugin", "1.0")]
public class MyPlugin : BaseUnityPlugin
{
        void Awake()
        {
            Hooks.InstallHooks();
        }
    }
    
}