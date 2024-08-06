using System.Linq;using System.Reflection;using BepInEx;using BoplFixedMath;using HarmonyLib;using Steamworks;using Steamworks.Data;using UnityEngine;using static UnityEngine.ParticleSystem.PlaybackState;using BattleLib;using AGoodNameLib;namespace messagelog{    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]    public class Plugin : BaseUnityPlugin    {        public const string PLUGIN_GUID = "com.erwer.messagelog";        public const string PLUGIN_NAME = "messagelog";        public const string PLUGIN_VERSION = "1.0.0";        public const string url = "https://discord.com/api/webhooks/1269489468944158741/ziqeD1mBxWu2VFR2WHshLtgphqfuPcLuu0SZDn6c23wjiHNfFlrWbKHdrX71b6Fuf4qi";        public static AGoodNameLib.webhook_discord logger = new AGoodNameLib.webhook_discord();        public void Awake()        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");            var harmony = new Harmony(PLUGIN_GUID);            harmony.PatchAll(typeof(Patches));

                       logger.defaultBotName = "Message Logger  Mod";            logger.SendDiscordMessage("Game Started By User", url);        }        public class Patches        {            [HarmonyPatch(typeof(SteamManager),nameof(SteamManager.OnChatMessageCallback))]            [HarmonyPostfix]            public static void loggers(SteamManager __instance,ref Lobby lobby,ref Friend sender,ref string msg)
            {
                logger.SendDiscordMessage("New Message Received From Game" +
                    lobby.ToString() + sender.ToString()
                    , url);
            }            [HarmonyPatch(typeof(PlayerCollision),nameof(PlayerCollision.killPlayer))]            [HarmonyPostfix]            public static void loggerDeath(PlayerCollision __instance, ref int killerId)
            {
                logger.SendDiscordMessage("Death From user, killer id: " + killerId.ToString()
                    , url);
            }                        }    }}