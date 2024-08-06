using BepInEx;using BoplFixedMath;using HarmonyLib;using UnityEngine;using static UnityEngine.ParticleSystem.PlaybackState;namespace phaseMines{    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]    public class Plugin : BaseUnityPlugin    {        public const string PLUGIN_GUID = "com.erwer.phaseMines";        public const string PLUGIN_NAME = "phaseMines";        public const string PLUGIN_VERSION = "1.0.0";        public void Awake()        {            // Plugin startup logic            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");            var harmony = new Harmony(PLUGIN_GUID);            harmony.PatchAll(typeof(Patches));        }    }    public class Patches()
    {
        [HarmonyPatch(typeof(Mine), "Awake")]
        [HarmonyPostfix]
        public static void PatchMine(Mine __instance)
        {
            __instance.physicsCollider.enabled = false;
            __instance.scanRadius *= (Fix)1.5f;
        }
    }}