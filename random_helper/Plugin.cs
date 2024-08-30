using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace random_helper
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer-nopub.random_helper";
        public const string PLUGIN_NAME = "random_helper";
        public const string PLUGIN_VERSION = "1.0.0";

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }
    }
    public class Patches
    {
        [HarmonyPatch(typeof(TimeStop), nameof(TimeStop.UpdateSim))]
        [HarmonyPrefix]
        public static void UpdateSim(TimeStop __instance)
        {
            GameTime.PlayerTimeScale = (Fix)4f;
            Time.timeScale = .5f;
        }

        [HarmonyPatch(typeof(TimeStop), nameof(TimeStop.End))]
        [HarmonyPrefix]
        public static void End(TimeStop __instance)
        {
            GameTime.PlayerTimeScale = (Fix)1f;
            Time.timeScale = 1f;
        }
    }
}