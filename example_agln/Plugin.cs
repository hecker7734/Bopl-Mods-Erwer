using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace example_agln
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer-no-publish.example_agln";
        public const string PLUGIN_NAME = "example_agln";
        public const string PLUGIN_VERSION = "1.0.0";

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");


            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }
    } 
    public class Patches()
    {
        //write a patch that clears ropes from the player, in this postfix, if that player does not own that rope.
        [HarmonyPatch(typeof(Dash), "performDash")]
        [HarmonyPostfix]
        public static void PatchDash(Dash __instance)
        {


            PlayerBody body = __instance.body;
            __instance.player.CanUseAbilities = false;


            if (!AGoodNameLib.player.does_own_rope(body))
            {
                AGoodNameLib.player.clear_ropes(body);
            }
        }

    }
}