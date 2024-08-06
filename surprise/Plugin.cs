using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace surprise
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.surprise";
        public const string PLUGIN_NAME = "surprise";
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
    
        [HarmonyPatch(typeof(BowTransform), "Awake")]
        [HarmonyPrefix]
        public static void BowPatch(BowTransform __instance)
        {
            __instance.ArrowSpeed *= (Fix)(-1);
        }

        [HarmonyPatch(typeof(Mine), "Awake")]
        [HarmonyPrefix]
        public static void MinePatch(Mine __instance)
        {
            __instance.chaseStrength *= (Fix)(-10);
        }

        [HarmonyPatch(typeof(Arrow), "OnCollide")]
        [HarmonyPrefix]
        public static bool ArrowPatch(Arrow __instance, ref CollisionInformation collision)
        {

            if (!Tools.LayerMaskContains(__instance.StickTo, collision.layer))
            {
                return false;
            }
            if (collision.layer == LayerMask.NameToLayer("Player") && collision.colliderPP.fixTrans.GetComponent<BoplBody>() == null)
            {
                return false;
            }
            if (collision.colliderPP.fixTrans == null || collision.colliderPP.fixTrans.IsDestroyed)
            {
                return false;
            }

            if (!__instance.hasLanded)
            {
                // Calculate the reflection direction
                Vector2 incomingVector = (Vector2)__instance.body.velocity;
                Vector2 normal = (Vector2)collision.normal;
                Vector2 reflection = Vector3.Reflect(incomingVector, normal);
                Fix speedAmountS = (Fix)(2f);
                Vec2 speedAmount = new Vec2(speedAmountS, speedAmountS);

                Vec2 newVelocity = new Vec2((Fix)reflection.x * speedAmount.x, (Fix)reflection.y * speedAmount.y);

                __instance.body.velocity = newVelocity;

                __instance.transform.position = (Vector3)collision.contactPoint;

                __instance.transform.position = (Vector3)collision.contactPoint;
                __instance.hasLanded = true;
                AudioManager.Get().Play("arrowHit");
                float angle = Mathf.Atan2(reflection.y, reflection.x) * Mathf.Rad2Deg;

                // Rotate the arrow to face the new direction
                __instance.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                __instance.transform.position = (Vector3)collision.contactPoint;
                __instance.transform.SetParent(collision.colliderPP.fixTrans.transform);
                if (collision.layer == LayerMask.NameToLayer("wall"))
                {
                    __instance.stuckToGround = collision.colliderPP.fixTrans.GetComponent<StickyRoundedRectangle>();
                    __instance.LocalPlatformPosition = __instance.stuckToGround.LocalPlayerPosFromPos(collision.contactPoint, Fix.Zero);
                    __instance.RepositionOnParent();
                }
                AudioManager.Get().Play("arrowHit");
                Updater.DestroyFix(__instance.body);
                Updater.DestroyFix(__instance.hitbox);
                Updater.DestroyFix(__instance.fixTrans);
                for (int i = 0; i < __instance.transform.childCount; i++)
                {
                    UnityEngine.Object.Destroy(__instance.transform.GetChild(i).gameObject);
                }
            }

            return false;
        }

    }
}