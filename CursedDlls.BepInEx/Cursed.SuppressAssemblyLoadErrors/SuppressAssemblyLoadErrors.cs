using System;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;

namespace Cursed.SuppressAssemblyLoadErrors
{
    [BepInPlugin("dll.cursed.suppressassemblyloaderrors", "Suppress assembly load errors", "1.0")]
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
                __result = re.Types;
                logger.LogDebug($"Encountered ReflectionTypeLoadException which was suppressed. Full error: \n${TypeLoader.TypeLoadExceptionToString(re)}");
            }
        }
    }
}