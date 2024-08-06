using BepInEx;using BoplFixedMath;using HarmonyLib;using UnityEngine;using static UnityEngine.ParticleSystem.PlaybackState;namespace drillFly{    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]    public class Plugin : BaseUnityPlugin    {        public const string PLUGIN_GUID = "com.erwer.drillFly";        public const string PLUGIN_NAME = "drillFly";        public const string PLUGIN_VERSION = "1.0.0";        public void Awake()        {            // Plugin startup logic            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");            var harmony = new Harmony(PLUGIN_GUID);            harmony.PatchAll(typeof(Patches));        }    }    public class Patches
    {
        [HarmonyPatch(typeof(Drill), "Update")]
        [HarmonyPostfix]
        public static void drillPatch1(Drill __instance)
        {
            __instance.isUnderground = true;
        }
    }}