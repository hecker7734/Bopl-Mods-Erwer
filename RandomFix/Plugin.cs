using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;
using static UnityEngine.UIElements.StyleVariableResolver;

namespace RandomFix
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.RandomFix";
        public const string PLUGIN_NAME = "RandomFix";
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
        [HarmonyDebug]
        [HarmonyPatch(typeof(Fix), "op_Addition")]
        [HarmonyPrefix]
        public static bool OperatorReturn(Fix x, Fix y, ref Fix __result)
        {
            __result = new Fix(x.m_rawValue + y.m_rawValue) * Updater.RandomFix((Fix).99, (Fix)1.01);
            return false;
        }
    }
}