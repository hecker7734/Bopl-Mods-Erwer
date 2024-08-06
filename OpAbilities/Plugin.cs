using HarmonyLib;
using BepInEx;
using BoplFixedMath;
using UnityEngine;
using static Mine;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace OpAbilities
{

    [BepInPlugin("com.erewrwerwerwerwerwerwer.OpAbilities", "OpAbilities", PluginInfo.PLUGIN_VERSION)]

    public class Plugin : BaseUnityPlugin
    {
        private static Harmony harmony;
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Logger.LogInfo("Wowie, you found a RNG log file 1/1000 chance; now find the rest."); // it doesn't change LOL   
            harmony = new("me.erewrwerwerwerwerwerwer" + PluginInfo.PLUGIN_GUID); // Idk saw __instance from other mods.
            harmony.PatchAll(typeof(Patches)); // :0
        }
        public class Patches
        {
            [HarmonyPatch(typeof(BowTransform), "Shoot")]
            [HarmonyPrefix]

            public static void bowPatch(BowTransform __instance)
            {
                __instance.ArrowSpeed = (Fix)100;
            }

            [HarmonyPatch(typeof(Drill), "Awake")]
            [HarmonyPrefix]
            public static void drillPatch(Drill __instance)
            {
                __instance.minUndergroundSpeed = (Fix)100f;
                __instance.rotationSpeedAir = (Fix)100f;
            }

            [HarmonyPatch(typeof(Shockwave), "Awake")]
            [HarmonyPrefix]
            public static void ShockwavePatch(Shockwave __instance)
            {
                __instance.defaultForce = (Fix)2f;
                __instance.platformForce = (Fix)10f;
                __instance.groundedPlayerForceMultiplier = (Fix)(-2f);
            }

            [HarmonyPatch(typeof(Mine), "Update")]
            [HarmonyPrefix]
            public static void MinePatch(Mine __instance)
            {
                var ren = __instance.gameObject.GetComponent<SpriteRenderer>();
                Color color2 = ren.color;
                __instance.maxAlpha = .4f;

                if (__instance.state == Mine.MineState.armed)
                {
                    color2.a = 0f;
                }
                else if (__instance.state == Mine.MineState.off)
                {
                    color2.a = 1.0f - ((float)__instance.timeSinceSpawned * 0.5f); // Math :bhappy:
                    color2.a = Mathf.Clamp(color2.a, 0.0f, 1.0f); // Ensure alpha stays within the valid range

                }
                else
                {
                    color2.a = 1f;
                }

                ren.color = color2;

                __instance.scanRadius = (Fix)20f;
                __instance.chaseTime = (Fix)100f;
            }

            [HarmonyPatch(typeof(RocketEngine), "Update")]
            [HarmonyPrefix]
            public static void RocketEnginePatch(RocketEngine __instance)
            {
                __instance.timeSinceEngineStarted = (Fix).7f;
            }

            [HarmonyPatch(typeof(Missile), "Awake")]
            [HarmonyPrefix]
            public static void MissileUpdateSimPatch(Missile __instance)
            {
                __instance.accel = (Fix)6L;
                __instance.minimumForwardsSpeed = (Fix)85f;
            }

            [HarmonyPatch(typeof(BounceBall), "LateUpdateSim")]
            [HarmonyPrefix]
            public static void BoucceBallPatch(BounceBall __instance)
            {
                __instance.IsCancellable = true;
                __instance.shockwaveForce = (Fix)(-100L);
            }

            [HarmonyPatch(typeof(Beam), "UpdateBeam")]
            [HarmonyPrefix]
            public static void UpdateBeam(Beam __instance, Fix simDeltaTime)
            {
                Vec2 vec = __instance.FirePos();
                Fix y = Fix.One;
                Fix fix = __instance.inAir ? __instance.maxTimeAir : __instance.maxTime;
                Fix beamWidthMultiplier = (Fix)4L; // Adjust __instance value to increase the beam width

                if (__instance.timeSinceBeamStart + __instance.exitScaleTime > fix && __instance.state == BeamState.loop && __instance.timeSinceBeamStart + __instance.exitScaleTime > fix)
                {
                    Fix t = (__instance.exitScaleTime - (fix - __instance.timeSinceBeamStart)) / __instance.exitScaleTime;
                    y = __instance.exitScaleAnim.Evaluate(t);
                }

                if (vec.y >= Constants.WATER_HEIGHT || Constants.leveltype == LevelType.space)
                {
                    __instance.beamIndex = DetPhysics.Get().AddBeamBody(new DetPhysics.BeamBody
                    {
                        position = vec + __instance.staffDir * __instance.beamOffset,
                        direction = __instance.staffDir,
                        scale = __instance.body.fixtrans.Scale * y * beamWidthMultiplier, // Apply the multiplier here
                        colors = __instance.playerBeamColor,
                        timePassed = __instance.timeSinceBeamStart,
                        id = __instance.HierarchyNumber,
                        ownerId = __instance.player.Id,
                        ground = __instance.currentGround
                    });
                }
                else
                {
                    __instance.beamIndex = -1;
                }

                __instance.timeSinceBeamStart += simDeltaTime / (Fix)4;
            }

            [HarmonyPatch(typeof(BlackHole), "Update")]
            [HarmonyPrefix]
            private static void BlackholePatch(BlackHole __instance)
            {
                if (__instance.forceArrow == null)
                {
                    return;
                }
                if (GameTime.IsTimeStopped())
                {
                    __instance.forceArrow.gameObject.SetActive(true);
                }
                if (__instance.forceArrow.gameObject.activeSelf)
                {
                    if (GameTime.IsTimeStopped() && __instance.dCircle.initHasBeenCalled && !GameSessionHandler.GameIsPaused)
                    {
                        __instance.forceArrow.SetForce((Vector2)__instance.velocity);
                        return;
                    }
                    __instance.forceArrow.gameObject.SetActive(false);
                }
                if (__instance.mass != 0)   
                {
                    var def = __instance.mass < 0L;
                    var value = (Fix).1f;
                    if (def) {
                        value = (Fix)(-.1f); 
                    }
                    __instance.GrowIncrementally(value);
                }
            }

            /* COOLDOWN */
            [HarmonyPatch(typeof(Ability), nameof(Ability.Awake))]
            [HarmonyPostfix]
            public static void CustomizeCooldown(ref Ability __instance)
            {
                if (__instance.name.StartsWith("Revival")) __instance.Cooldown = (Fix).2f;
                if (__instance.name.StartsWith("Bow")) __instance.Cooldown = (Fix).2f;
                if (__instance.name.StartsWith("Rock")) __instance.Cooldown = (Fix).1f;
                if (__instance.name.StartsWith("Dash")) __instance.Cooldown = (Fix).3f;
                if (__instance.name.StartsWith("Blackhole")) __instance.Cooldown = (Fix).5f;
                if (__instance.name.StartsWith("Shrink")) __instance.Cooldown = (Fix)0.1f;
                if (__instance.name.StartsWith("Grow")) __instance.Cooldown = (Fix)0.1f;
            }
        }


        
    } 
}
