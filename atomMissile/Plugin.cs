using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.ParticleSystem.PlaybackState;


namespace atomMissile
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.atomMissile";
        public const string PLUGIN_NAME = "atomMissile";
        public const string PLUGIN_VERSION = "1.0.0";
        internal static Sprite atomMissileSprite;
        internal static ConfigFile config;
        internal static ConfigEntry<double> missilePower;
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
            config = Config;

            missilePower = config.Bind("Settings", "Atom Missile (Multiplier)", 12d, "Minimum is 0.0 (Negitives default to 1).\n MAX : 20\n Suggested Value : 12");
            if (missilePower.Value < 1d) missilePower.Value = 1d;
            if (missilePower.Value > 20d) missilePower.Value = 20d;


            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));

        }
    }

    public class Patches
    {
        [HarmonyPatch(typeof(Missile), nameof(Missile.OnCollide))]
        [HarmonyPrefix]
        public static bool OnCollide(Missile __instance, ref CollisionInformation collision)
        {
            if (collision.layer != LayerMask.NameToLayer("RigidBodyAffector") && collision.layer != LayerMask.NameToLayer("Rope"))
            {
                // Instantiate the explosion at the position of the missile
                Explosion explosion = FixTransform.InstantiateFixed(__instance.onHitExplosionPrefab, __instance.body.position);

                // Ensure the explosion has a component that implements IPhysicsCollider
                var physicsCollider = explosion.GetComponent<IPhysicsCollider>();
                if (physicsCollider != null)
                {
                    // Apply scaling
                    physicsCollider.Scale = __instance.fixTrans.Scale * (Fix)Plugin.missilePower.Value;
                }
                else
                {
                    Debug.LogWarning("Explosion prefab does not have an IPhysicsCollider component.");
                }

                // Play the sound effect if specified
                if (!string.IsNullOrEmpty(__instance.soundEffectOnCol))
                {
                    AudioManager.Get().Play(__instance.soundEffectOnCol);
                }

                // Destroy the missile
                Updater.DestroyFix(__instance.gameObject);
            }
            return false; // Skip original method
        }

    }
}