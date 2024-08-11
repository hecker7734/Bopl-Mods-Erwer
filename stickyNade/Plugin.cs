using System;
using System.Collections.Generic;
using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace stickyNade
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInIncompatibility("savaGe-NoOwnerExplosionDeaths-1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.stickyNade";
        public const string PLUGIN_NAME = "stickyNade";
        public const string PLUGIN_VERSION = "1.0.0";

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }

    }

    public class GrenadeAdapter : IPlatformAttachable
    {
        private readonly Grenade _grenade;
        public Fix SurfaceOffset { get; set; }

        
        // Token: 0x1700005F RID: 95
        // (get) Token: 0x0600036A RID: 874 RVA: 0x00023043 File Offset: 0x00021243
        // (set) Token: 0x0600036B RID: 875 RVA: 0x0002304B File Offset: 0x0002124B
        public Fix LocalPlatformPosition { get; set; }

        // Token: 0x17000060 RID: 96
        // (get) Token: 0x0600036C RID: 876 RVA: 0x00023054 File Offset: 0x00021254
        // (set) Token: 0x0600036D RID: 877 RVA: 0x0002305C File Offset: 0x0002125C
        public Vec2 HitNormal { get; set; } = Vec2.up;

        // Token: 0x04000388 RID: 904
        public StickyRoundedRectangle stuckToGround;

        // Token: 0x04000387 RID: 903
        public bool hasLanded;

        public GrenadeAdapter(Grenade grenade)
        {
            _grenade = grenade;
        }
        public void RepositionOnParent()
        {
            if (this.stuckToGround)
            {

                Vec2 v = this.stuckToGround.PositionFromLocalPlayerPos(this.LocalPlatformPosition, Fix.Zero);
                _grenade.GetComponent<FixTransform>().position = v;
                
            }
        }

        public void doLand()
        {
            FixTransform fixTrans = _grenade.body.fixTrans;
            Fix Radius = (Fix)1f;
            PhysicsParent[] hits = new PhysicsParent[50];
            HashSet<int> gameObjectIdsHit = new HashSet<int>();

            int hitCount = DetPhysics.Get().circleCastRoundedRect(fixTrans.position, Radius, ref hits);

            Debug.Log($"Hit count: {hitCount}");

            for (int i = 0; i < hitCount; i++)
            {
                StickyRoundedRectangle stickyComponent = hits[i].fixTrans.GetComponent<StickyRoundedRectangle>();
                if (stickyComponent != null)
                {
                    Debug.Log($"Found sticky component on {hits[i].fixTrans.gameObject.name}");
                    this.stuckToGround = stickyComponent;
                    Vec2 position = _grenade.GetComponent<FixTransform>().position;
                    Vec2 position2 = hits[i].fixTrans.position;


                    double angle = mathhs.differ_angel(
                    (float)_grenade.transform.position.x, (float)_grenade.transform.position.y,
                    (float)hits[i].fixTrans.position.x, (float)hits[i].fixTrans.position.y);

                    // Convert the angle to a direction vector
                    Vec2 dir = new Vec2((Fix)Math.Cos(angle * Math.PI / 180.0), (Fix)Math.Sin(angle * Math.PI / 180.0));

                    List<RaycastInformation> output = new List<RaycastInformation>();
                    // Perform a raycast in the calculated direction
                    DetPhysics.Get().RaycastAll((Vec2)_grenade.transform.position, dir, (Fix)250f, LayerMask.GetMask("wall"), output);
                    
                    if (output.Count > 0)
                    {
                        Debug.Log($"Raycast hit at {output[0].nearPos}");
                        _grenade.transform.position = (Vector3)output[0].nearPos;
                    }

                    this.LocalPlatformPosition = this.stuckToGround.LocalPlayerPosFromPos(output[0].nearPos, Fix.Zero);
                    this.hasLanded = true;

                    AudioManager.Get().Play("arrowHit");
                    _grenade.transform.SetParent(hits[i].fixTrans.gameObject.transform);
                    Updater.DestroyFix(_grenade.GetComponent<FixTransform>());

                    for (int j = 0; j < _grenade.transform.childCount; j++)
                    {
                        UnityEngine.Object.Destroy(_grenade.transform.GetChild(j).gameObject);
                    }

                    break; // Exit loop after finding the first valid hit
                }
                else
                {
                    Debug.Log($"No sticky component found on {hits[i].fixTrans.gameObject.name}");
                }
            }
        }

    }

    public class Patches
    {
        public static Dictionary<Grenade, GrenadeAdapter> GrenadeAdapters = new Dictionary<Grenade, GrenadeAdapter>();
        
        [HarmonyPatch(typeof(Grenade), "Awake")]
        [HarmonyPostfix]
        public static void PatchGrenade(Grenade __instance)
        {
            __instance.body.bounciness = (Fix)0f;

            // Attach the adapter
            var adapter = new GrenadeAdapter(__instance);
            GrenadeAdapters[__instance] = adapter;
        }

        [HarmonyPatch(typeof(Grenade), "UpdateSim")]
        [HarmonyPrefix]
        public static bool GrenadePatch2(Grenade __instance, ref Fix simDeltaTime)
        {
            bool _continue = false;
            GrenadeAdapter adapter = GrenadeAdapters[__instance];

            if(adapter != null && __instance.hasBeenThrown && __instance.DetonatesOnOwner)
            {
                if (!adapter.hasLanded)
                {
                    adapter.doLand();
                    _continue = true;
                }
                else
                {
                    adapter.RepositionOnParent();
                    _continue = false;
                }
            }
            if (!_continue) {
                if (!__instance.hasBeenThrown && (__instance.thrower == null || !__instance.thrower.gameObject.activeSelf))
                {
                    __instance.hasBeenThrown = true;
                    __instance.hurtOwnerDelay = -Fix.One;
                    __instance.GetComponent<DPhysicsCircle>().ManualInit();
                }
                if (__instance.hasBeenThrown && __instance.hurtOwnerDelay > 0L)
                {
                    __instance.hurtOwnerDelay -= simDeltaTime * GameTime.PlayerTimeScale;
                    if (__instance.hurtOwnerDelay <= 0L)
                    {
                        __instance.DetonatesOnOwner = true;
                    }
                }
                if (__instance.selfDestructDelay > 0L)
                {
                    __instance.selfDestructDelay -= simDeltaTime * GameTime.PlayerTimeScale;
                    if (__instance.selfDestructDelay <= 0L)
                    {
                        __instance.Detonate();
                    }
                }
            }
            return _continue;
        }
    }

    public class mathhs
    {
        public static double differ_angel(float x1, float y1, float x2, float y2)
        {
            float xDiff = x2 - x1;
            float yDiff = y2 - y1;
            return Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
        }
        
    }

}