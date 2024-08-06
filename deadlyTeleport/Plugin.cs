using BepInEx;using BoplFixedMath;using HarmonyLib;using Mono.Cecil.Cil;using UnityEngine;using static UnityEngine.ParticleSystem.PlaybackState;namespace deadlyTeleport{    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]    public class Plugin : BaseUnityPlugin    {        public const string PLUGIN_GUID = "com.erwer.deadlyTeleport";        public const string PLUGIN_NAME = "deadlyTeleport";        public const string PLUGIN_VERSION = "1.0.0";        public void Awake()        {            // Plugin startup logic            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");            var harmony = new Harmony(PLUGIN_GUID);            harmony.PatchAll(typeof(Patches));        }    }    public class Patches() {
        [HarmonyPatch(typeof(TeleportIndicator), "Update")]        [HarmonyPrefix]        public static void TpPatch(TeleportIndicator __instance)
        {
            PhysicsParent[] playerObjectsBuffer = new PhysicsParent[100];

            int num = DetPhysics.Get().CircleCastAll(__instance.playerTeleportPosition, __instance.playerTeleportRadius * __instance.scale, __instance.teleportableThings, ref playerObjectsBuffer);
            for (int i = 0; i < num; i++)
            {
                GameObject teleportedObject = playerObjectsBuffer[i].fixTrans.gameObject;
                if (teleportedObject.layer == LayerMask.NameToLayer("Player"))
                {
                    PlayerPhysics component = teleportedObject.GetComponent<PlayerPhysics>();
                    PlayerBody playerBody = component?.GetPlayerBody();
                    AGoodNameLib.player.do_kill_player(playerBody,256,CauseOfDeath.Beamed);
                }
            }
        }    }}