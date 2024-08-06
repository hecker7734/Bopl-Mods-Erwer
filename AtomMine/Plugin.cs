using System;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using Steamworks.Ugc;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace AtomMine
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.AtomMine";
        public const string PLUGIN_NAME = "AtomMine";
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
        [HarmonyPatch(typeof(Mine), nameof(Mine.Detonate))]
        [HarmonyPrefix]
        public static bool Detonate(Mine __instance, ref Explosion ___Explosion, ref BoplBody ___body, ref IPhysicsCollider ___physicsCollider)
        {
            Explosion explosion = FixTransform.InstantiateFixed(___Explosion, ___body.position);
            explosion.GetComponent<IPhysicsCollider>().Scale = ___physicsCollider.Scale * (Fix)12f;
            explosion.PlayerOwnerId = __instance.GetComponent<Item>().OwnerId;
            if (__instance.SoundToPlayOnDetonation != null)
            {
                AudioManager.Get().Play(__instance.SoundToPlayOnDetonation);
            }

            Updater.DestroyFix(__instance.gameObject);
            return false; //skips orignal function
        }
    }
}