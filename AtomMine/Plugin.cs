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

        internal static Sprite atomMineTex;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));

            Texture2D atomTexture = Utils.LoadDLLTexture("AtomGrenade.atom_grenade.png");
            atomMineTex = Sprite.Create(atomTexture, new Rect(0, 0, atomTexture.width, atomTexture.height), new Vector2(0.5f, 0.5f), 45);
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

        [HarmonyPatch(typeof(Mine), nameof(Mine.Awake))]
        [HarmonyPostfix]
        public static void change_texture(Mine __instance)
        {
            foreach (SpriteRenderer renderer in __instance.GetComponentsInChildren<SpriteRenderer>())
            {
                __instance.GetComponent<SpriteRenderer>().sprite = Plugin.atomMineTex;
            }
        }

    }

    

    internal class Utils
    {
        public static Texture2D LoadDLLTexture(string path)
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            Texture2D texture = new(256, 256);
            texture.LoadImage(buffer);
            return texture;
        }
    }
}