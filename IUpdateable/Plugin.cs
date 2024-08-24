using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using System;
using static UnityEngine.ParticleSystem.PlaybackState;



namespace IUpdateable
{
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
        [HarmonyPatch(typeof(Updater),nameof(Updater.Update))]
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

                        Updater_i.hitStopFrames--;
                        Updater_i.SimTimePassed += GameTime.FixedTimeStep;
                        Updater_i.SimTimeSinceLevelLoaded += GameTime.FixedTimeStep;
                        Updater_i.SimulationTicks++;
                    }
                    else
                    {
                        Updater.TickSimulation(GameTime.FixedTimeStep);
                        Updater_i.TickSimulation(GameTime.FixedTimeStep);
                    }
                }
            }
            return true; //skip OG.
        }


    }


// Token: 0x020000DE RID: 222
    public interface IUpdatable
    {
           // Token: 0x060006BB RID: 1723
            void Init();
    
            // Token: 0x060006BC RID: 1724
            void UpdateSim(Fix SimDeltaTime);
    
            // Token: 0x060006BD RID: 1725
            void LateUpdateSim(Fix SimDeltaTime);
    
            // Token: 0x060006BE RID: 1726
            void OnDestroyUpdatable();
    
            // Token: 0x060006BF RID: 1727
            bool IsEnabled();
    
            // Token: 0x17000086 RID: 134
            // (get) Token: 0x060006C0 RID: 1728
            // (set) Token: 0x060006C1 RID: 1729
            bool IsDestroyed { get; set; }

            // Token: 0x17000087 RID: 135
            // (get) Token: 0x060006C2 RID: 1730
            // (set) Token: 0x060006C3 RID: 1731
           int HierarchyNumber { get; set; }
    }

}