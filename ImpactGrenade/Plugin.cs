using System.Collections.Generic;
using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace ImpactGrenade
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.ImpactGrenade";
        public const string PLUGIN_NAME = "ImpactGrenade";
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
        [HarmonyPatch(typeof(Grenade), "UpdateSim")]
        [HarmonyPostfix]
        public static void PatchGrenadeSecond(Grenade __instance)
        {
            FixTransform fixTrans = __instance.body.fixTrans;
            Fix Radius = (Fix)1f;
            // Initialize LayerMask with the desired layer
            PhysicsParent[] hits = new PhysicsParent[50];
            HashSet<int> gameObjectIdsHit = new HashSet<int>();


            int hitCount = DetPhysics.Get().circleCastRoundedRect(fixTrans.position, Radius, ref hits);

            for (int i = 0; i < hitCount; i++)
            {
                if (__instance.DetonatesOnOwner)
                {
                    __instance.Detonate();
                }
            }
        }
    }
}