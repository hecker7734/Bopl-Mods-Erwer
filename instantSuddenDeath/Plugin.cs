using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace instantSuddenDeath
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.instantSuddenDeath";
        public const string PLUGIN_NAME = "instantSuddenDeath";
        public const string PLUGIN_VERSION = "1.0.0";

        public void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }
    }
    public class Patches()
    {
        [HarmonyPatch(typeof(GameSessionHandler), "Awake")]
        [HarmonyPostfix]
        public static void Postfix(GameSessionHandler __instance)
        {
            __instance.TimeBeforeSuddenDeath = (Fix)4f;
        }
    }
}
