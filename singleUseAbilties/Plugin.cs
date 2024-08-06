using System.Collections.Generic;
using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace singleUseAbilties
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.singleUseAbilties";
        public const string PLUGIN_NAME = "singleUseAbilties";
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
        [HarmonyPatch(typeof(Ability), nameof(Ability.ExitAbility))]
        [HarmonyPostfix]
        public static void oneTimeUse(Ability __instance)
        {
            __instance.isCastable = false;
        }

    }
}