using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace explosiveDash
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.explosiveDash";
        public const string PLUGIN_NAME = "explosiveDash";
        public const string PLUGIN_VERSION = "1.0.0";

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }
    }
    public class Patches() 
    {
        [HarmonyPatch(typeof(Dash), "performDash")]
        [HarmonyPrefix]
        public static void PatchDash(Dash __instance, ref Player ___player)
        {
            Explosion val = new Explosion();
            Explosion[] array = Resources.FindObjectsOfTypeAll(typeof(Explosion)) as Explosion[];
            Explosion[] array2 = array;
            foreach (Explosion val2 in array2)
            {
                if (((Object)val2).name == "MineExplosion")
                {
                    val = val2;
                }
            }

            // Calculate a safe distance from the player to spawn the mine kaboom, TBH I forgot the input vector was a thing :skull:
            Vec2 safeExplosionPosition = ___player.Position + __instance.inputVector * (Fix)(-7.6f);

            Explosion val3 = FixTransform.InstantiateFixed<Explosion>(val, safeExplosionPosition);
            ((Component)val3).GetComponent<IPhysicsCollider>().Scale = ___player.scale*(Fix).9;
            val3.PlayerOwnerId = ___player.Id;
            AudioManager.Get().Play("explosion");
        }

    }
}