using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using RUST.Steamworks;
using Steamworks;

[assembly: AssemblyVersion("1.2")]
namespace Cursed.SuppressAssemblyLoadErrors
{
    [BepInPlugin("dll.cursed.suppressassemblyloaderrors", "CursedDlls - Suppress Assembly.GetTypes errors", "1.1")]
    public class SuppressAssemblyLoadErrors : BaseUnityPlugin
    {
        internal static ManualLogSource logger; 
            
        private void Awake()
        {
            logger = Logger;
            Harmony.CreateAndPatchAll(typeof(SuppressAssemblyLoadErrors));
        }

        [HarmonyPatch(typeof(Assembly), nameof(Assembly.GetTypes), new Type[0])]
        [HarmonyFinalizer]
        public static void HandleReflectionTypeLoad(ref Exception __exception, ref Type[] __result)
        {
            if (__exception == null)
                return;
            if (__exception is ReflectionTypeLoadException re)
            {
                __exception = null;
                __result = re.Types.Where(t => t != null).ToArray();
                logger.LogDebug($"Encountered ReflectionTypeLoadException which was suppressed. Full error: \n${TypeLoader.TypeLoadExceptionToString(re)}");
            }
        }

        /*
		 * Skiddie prevention
		 */
        [HarmonyPatch(typeof(HighScoreManager), nameof(HighScoreManager.UpdateScore), new Type[] { typeof(string), typeof(int), typeof(Action<int, int>) })]
        [HarmonyPatch(typeof(HighScoreManager), nameof(HighScoreManager.UpdateScore), new Type[] { typeof(SteamLeaderboard_t), typeof(int) })]
        [HarmonyPrefix]
        public static bool HSM_UpdateScore()
        {
            return false;
        }
    }
}