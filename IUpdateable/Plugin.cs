using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using System;
using static UnityEngine.ParticleSystem.PlaybackState;



namespace IUpdateable
{
    public interface IUpdatable
    {
        void Init();
        void UpdateSim(Fix SimDeltaTime);
        void LateUpdateSim(Fix SimDeltaTime);
        void OnDestroyUpdatable();
        bool IsEnabled();
        bool IsDestroyed { get; set; }
        int HierarchyNumber { get; set; }
    }

    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.IUpdateable";
        public const string PLUGIN_NAME = "IUpdateable";
        public const string PLUGIN_VERSION = "2.0.0";
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded! Added IUpdateable to game..");

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }
    }

    public class Patches
    {
        [HarmonyPatch(typeof(Updater), nameof(Updater.Update))]
        [HarmonyPrefix]
        public static bool Update(Updater __instance)
        {
            if (__instance.autoUpdate && !GameLobby.isOnlineGame && !GameLobby.isPlayingAReplay)
            {
                __instance.autoUpdateAccum += Time.deltaTime;
                while (__instance.autoUpdateAccum > 0f)
                {
                    __instance.autoUpdateAccum -= (float)GameTime.FixedTimeStep;
                    if (Updater.hitStopFrames > 0)
                    {
                        Updater.hitStopFrames--;
                        Updater.SimTimePassed += GameTime.FixedTimeStep;
                        Updater.SimTimeSinceLevelLoaded += GameTime.FixedTimeStep;
                        Updater.SimulationTicks++;
                    }
                    else
                    {
                        Updater.TickSimulation(GameTime.FixedTimeStep);
                    }
                }
            }
            return false;
        }
    }
}