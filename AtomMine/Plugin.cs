using System;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using AGoodNameLib;
using BepInEx;
using BepInEx.Configuration;
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
        internal static ConfigFile config;
        internal static ConfigEntry<float> multiplier;
        private void Awake()
        {
            // Plugin startup logic
            config = Config;
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            multiplier = config.Bind("Settings", "Explosion Multiplier", 12f, new ConfigDescription(
                "Increases the size of Mine Explosions",
                new AcceptableValueRange<float>(1f, 12f)
            ));
            
            if (multiplier.Value < 1f) multiplier.Value = 1f;
            if (multiplier.Value > 12f) multiplier.Value = 12f;


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
            explosion.GetComponent<IPhysicsCollider>().Scale = ___physicsCollider.Scale * (Fix)Plugin.multiplier.Value;
            explosion.PlayerOwnerId = __instance.GetComponent<Item>().OwnerId;
            if (__instance.SoundToPlayOnDetonation != null)
            {
                AudioManager.Get().Play(__instance.SoundToPlayOnDetonation);
            }

            Updater.DestroyFix(__instance.gameObject);
            return false; //skips orignal function
        }
        [HarmonyPatch(typeof(Mine),nameof(Mine.Awake))]
        [HarmonyPostfix]
        public static void d(Mine __instance)
        {
            // Load the texture
            Texture2D tex = texTils.LoadTextureFromResource("AGoodNameLib.Resources.mine_texture.mine_new_tex");
                // Create a sprite from the texture
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

                // Get the SpriteRenderer component and assign the sprite
                SpriteRenderer spriteRenderer = __instance.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;
        }
    }
}