using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace modding_discord_button
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.modding_discord_button";
        public const string PLUGIN_NAME = "modding_discord_button";
        public const string PLUGIN_VERSION = "1.0.0";

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }
    }
}