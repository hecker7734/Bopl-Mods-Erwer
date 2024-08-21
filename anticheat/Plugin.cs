using BepInEx;
using BoplFixedMath;
using Entwined;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace anticheat
{
    [BepInDependency("com.entwinedteam.entwined")]
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.anticheat";
        public const string PLUGIN_NAME = "anticheat";
        public const string PLUGIN_VERSION = "1.0.0";

        static EntwinedPacketChannel<string> ping;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }


        private void OnGUI()
        {
            if (GUI.Button(new Rect(15, 120, 100, 40), "Send Packet"))
            {
                ping.SendMessage("Hello World!");
            }
        }

        private static void OnMessage(string payload, PacketSourceInfo sourceInfo)
        {
            Debug.Log($"{sourceInfo.SenderSteamName}: {payload}");
        }
    }
}