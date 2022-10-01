using System;
using System.Globalization;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using FistVR;
using HarmonyLib;
using RUST.Steamworks;
using Steamworks;
using UnityEngine;
using Valve.VR;

namespace Cursed.LoadScene
{
    [BepInPlugin("dll.cursed.loadscene", "CursedDlls - Load Scene", "1.6")]
    public class LoadScenePlugin : BaseUnityPlugin
    {
        private static ConfigEntry<string> _sceneLoad1;
        private static ConfigEntry<string> _sceneLoad2;
        private static ConfigEntry<string> _sceneLoad3;
        private static ConfigEntry<string> _sceneLoad4;
        private static ConfigEntry<string> _sceneLoad5;
        private static ConfigEntry<string> _sceneLoad6;
        private static ConfigEntry<string> _sceneLoad7;
        private static ConfigEntry<string> _sceneLoad8;
        private static ConfigEntry<string> _sceneLoad9;
        private static ConfigEntry<string> _sceneLoad0;

        private static ConfigEntry<KeyboardShortcut> _sceneReload;
        private static ConfigEntry<KeyboardShortcut> _sceneLoadModifier;

        public void Awake()
        {
            _sceneReload = Config.Bind("General", "Reload", new KeyboardShortcut(KeyCode.Space), "Controls what key(s) you need to press to reload the current scene.");
            _sceneLoadModifier = Config.Bind("General", "Modifier", new KeyboardShortcut(KeyCode.LeftControl), "Controls what key(s) you need to press to load scenes.");

            _sceneLoad0 = Config.Bind("General", "SceneLoad0", "", "What scene will load when [Modifier]+0 is pressed. If you type a scene name wrong, you will be stuck in a loading purgatory!");
            _sceneLoad1 = Config.Bind("General", "SceneLoad1", "", "What scene will load when [Modifier]+1 is pressed. If you type a scene name wrong, you will be stuck in a loading purgatory!");
            _sceneLoad2 = Config.Bind("General", "SceneLoad2", "", "What scene will load when [Modifier]+2 is pressed. If you type a scene name wrong, you will be stuck in a loading purgatory!");
            _sceneLoad3 = Config.Bind("General", "SceneLoad3", "", "What scene will load when [Modifier]+3 is pressed. If you type a scene name wrong, you will be stuck in a loading purgatory!");
            _sceneLoad4 = Config.Bind("General", "SceneLoad4", "", "What scene will load when [Modifier]+4 is pressed. If you type a scene name wrong, you will be stuck in a loading purgatory!");
            _sceneLoad5 = Config.Bind("General", "SceneLoad5", "", "What scene will load when [Modifier]+5 is pressed. If you type a scene name wrong, you will be stuck in a loading purgatory!");
            _sceneLoad6 = Config.Bind("General", "SceneLoad6", "", "What scene will load when [Modifier]+6 is pressed. If you type a scene name wrong, you will be stuck in a loading purgatory!");
            _sceneLoad7 = Config.Bind("General", "SceneLoad7", "", "What scene will load when [Modifier]+7 is pressed. If you type a scene name wrong, you will be stuck in a loading purgatory!");
            _sceneLoad8 = Config.Bind("General", "SceneLoad8", "", "What scene will load when [Modifier]+8 is pressed. If you type a scene name wrong, you will be stuck in a loading purgatory!");
            _sceneLoad9 = Config.Bind("General", "SceneLoad9", "", "What scene will load when [Modifier]+9 is pressed. If you type a scene name wrong, you will be stuck in a loading purgatory!");
        }

        public void Update()
        {
            if (Input.GetKey(_sceneLoadModifier.Value.MainKey))
            {
                if (Input.GetKeyDown(_sceneReload.Value.MainKey))
                    SteamVR_LoadLevel.Begin(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

                else if (!string.IsNullOrEmpty(_sceneLoad0.Value) && Input.GetKeyDown(KeyCode.Alpha0))
                    SteamVR_LoadLevel.Begin(_sceneLoad0.Value);
                else if (!string.IsNullOrEmpty(_sceneLoad1.Value) && Input.GetKeyDown(KeyCode.Alpha1))
                    SteamVR_LoadLevel.Begin(_sceneLoad1.Value);
                else if (!string.IsNullOrEmpty(_sceneLoad2.Value) && Input.GetKeyDown(KeyCode.Alpha2))
                    SteamVR_LoadLevel.Begin(_sceneLoad2.Value);
                else if (!string.IsNullOrEmpty(_sceneLoad3.Value) && Input.GetKeyDown(KeyCode.Alpha3))
                    SteamVR_LoadLevel.Begin(_sceneLoad3.Value);
                else if (!string.IsNullOrEmpty(_sceneLoad4.Value) && Input.GetKeyDown(KeyCode.Alpha4))
                    SteamVR_LoadLevel.Begin(_sceneLoad4.Value);
                else if (!string.IsNullOrEmpty(_sceneLoad5.Value) && Input.GetKeyDown(KeyCode.Alpha5))
                    SteamVR_LoadLevel.Begin(_sceneLoad5.Value);
                else if (!string.IsNullOrEmpty(_sceneLoad6.Value) && Input.GetKeyDown(KeyCode.Alpha6))
                    SteamVR_LoadLevel.Begin(_sceneLoad6.Value);
                else if (!string.IsNullOrEmpty(_sceneLoad7.Value) && Input.GetKeyDown(KeyCode.Alpha7))
                    SteamVR_LoadLevel.Begin(_sceneLoad7.Value);
                else if (!string.IsNullOrEmpty(_sceneLoad8.Value) && Input.GetKeyDown(KeyCode.Alpha8))
                    SteamVR_LoadLevel.Begin(_sceneLoad8.Value);
                else if (!string.IsNullOrEmpty(_sceneLoad9.Value) && Input.GetKeyDown(KeyCode.Alpha9))
                    SteamVR_LoadLevel.Begin(_sceneLoad9.Value);
            }
        }
    }
}