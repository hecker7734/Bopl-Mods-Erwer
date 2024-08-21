using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;


namespace configall
{
    public static partial class Patches
    {
        [HarmonyPatch(typeof(ShootDuplicator), nameof(ShootDuplicator.Shoot))]
        [HarmonyPrefix]
        public static bool Shoot(ShootDuplicator __instance,  ref Vec2 firepointFIX,ref Vec2 directionFIX, ref bool hasFired,ref int playerId, bool alreadyHitWater = false)
        {
            Vec2 vec = directionFIX;
            AudioManager.Get().PlayWithRandomPitchVariation("fireRaygun");
            AudioManager.Get().PlayRandomWithPrefix("beamSparks");
            Debug.DrawRay((Vector2)firepointFIX, (float)__instance.maxDistance * (Vector2)vec, new Color(255f, 255f, 0f));
            LayerMask mask = __instance.collisionMask;
            if (alreadyHitWater)
            {
                mask = __instance.collisionMaskNoWater;
            }

            RaycastInformation raycastInformation = DetPhysics.Get().PointCheckAllRoundedRects(firepointFIX);
            if (!raycastInformation)
            {
                raycastInformation = DetPhysics.Get().RaycastToClosestTriple(firepointFIX, vec, __instance.maxDistance, mask, __instance.tripleRaycastRadius);
            }

            if ((bool)raycastInformation)
            {
                PlayerHandler.Get().GetPlayer(playerId);
                if (raycastInformation.layer == LayerMask.NameToLayer("Water"))
                {
                    __instance.spawnRayCastEffect((Vector3)firepointFIX, (Vector3)vec, (float)raycastInformation.nearDist, didHit: false, reflected: true);
                    __instance.rayParticle.Stop();
                    vec = new Vec2(vec.x, -vec.y);
                    __instance.Shoot(raycastInformation.nearPos, vec, ref hasFired, playerId, alreadyHitWater: true);
                    Debug.DrawRay((Vector2)raycastInformation.nearPos, (Vector2)(vec * __instance.maxDistance), Color.magenta);
                    GameObject obj = UnityEngine.Object.Instantiate(__instance.WaterRing);
                    AudioManager.Get().Play("waterExplosion");
                    obj.transform.position = new Vector3(__instance.WaterRing.transform.position.x + (float)raycastInformation.nearPos.x, __instance.WaterRing.transform.position.y, __instance.WaterRing.transform.position.z);
                   return false;
                }

                __instance.spawnRayCastEffect((Vector3)firepointFIX, (Vector3)vec, (float)raycastInformation.nearDist, didHit: true);
                hasFired = true;
                Vec2 position = raycastInformation.pp.fixTrans.position;
                Vec2 nearPos = raycastInformation.nearPos;
                GameObject gameObject = raycastInformation.pp.fixTrans.gameObject;
                Vec2 vec2 = vec;
                int layer = raycastInformation.layer;
                if (layer == LayerMask.NameToLayer("Projectile"))
                {
                    BoplBody component = gameObject.GetComponent<BoplBody>();
                    if (!(component != null))
                    {
                        Vec2.MulByI(vec2);
                    }
                    else
                    {
                        Vec2.MulByI(Vec2.NormalizedSafe(component.velocity));
                    }

                    Fix offset = (Fix)7L * component.Scale;
                    Vec2 vec3 = nearPos;
                    ForceArrow forceArrowPrefab = component.ForceArrowPrefab;
                    component.ForceArrowPrefab = null;

                    int num = Plugin.dupe_projectile_count.Value;

                    for (int i = 0; i < num; i++)
                    {
                        vec3 = __instance.FanOut(num, vec, position, offset, (Fix)0.32, i);
                        Vec2 velocity = component.velocity;
                        velocity = Vec2.NormalizedSafe(vec3 - position) * (Vec2.Magnitude(velocity) + (Fix)10L);
                        __instance.duplicateBoplBody(vec3, gameObject, velocity, i, component.Scale);
                    }

                    component.ForceArrowPrefab = forceArrowPrefab;
                   return false;
                }

                if (layer == LayerMask.NameToLayer("item") || layer == LayerMask.NameToLayer("Smoke"))
                {
                    BoplBody component2 = gameObject.GetComponent<BoplBody>();
                    Item component3 = gameObject.GetComponent<Item>();
                    Vec2 vec4 = Vec2.MulByI(Vec2.NormalizedSafe(component2.velocity));
                    Fix fix = component3.duplicate_spacing * component2.Scale;
                    Vec2 vec5 = nearPos;
                    ForceArrow forceArrowPrefab2 = component2.ForceArrowPrefab;
                    component2.ForceArrowPrefab = null;
                    for (int j = 0; j < Plugin.dupe_item_n_smoke.Value; j++)
                    {
                        Vec2 vec6 = component2.velocity;
                        if (component3.duplicatePattern == DuplicatePattern.line)
                        {
                            Fix fix2 = fix * ((Fix)j + Fix.One);
                            if (j % 2 == 0)
                            {
                                vec5 += vec4 * fix2;
                            }
                            else
                            {
                                vec5 -= vec4 * fix2;
                            }
                        }
                        else if (component3.duplicatePattern == DuplicatePattern.circle)
                        {
                            Fix fix3 = (Fix)(j + 1) / (Fix)component3.nrOfClonesWhenDuplicated;
                            Fix angleRadians = Fix.PiTimes2 * fix3;
                            vec5 = raycastInformation.pp.fixTrans.position + new Vec2(angleRadians) * fix;
                        }
                        else if (component3.duplicatePattern == DuplicatePattern.randomCircle)
                        {
                            Vec2 vec7 = Updater.RandomUnitVector();
                            vec5 = raycastInformation.pp.fixTrans.position + vec7 * fix;
                        }
                        else if (component3.duplicatePattern == DuplicatePattern.fan)
                        {
                            vec5 = __instance.FanOut(component3.nrOfClonesWhenDuplicated, vec, position, fix, component3.duplicate_angle, j);
                            vec6 = Vec2.NormalizedSafe(vec5 - position) * (Vec2.Magnitude(vec6) + (Fix)10L);
                        }

                        BoplBody boplBody = __instance.duplicateBoplBody(vec5, gameObject, vec6, j, component2.Scale);
                        boplBody.up = component2.up;
                        Item component4 = boplBody.GetComponent<Item>();
                        Grenade component5 = boplBody.GetComponent<Grenade>();
                        if (component5 != null)
                        {
                            component5.Initialize(null);
                            component5.DelayedEnableHurtOwner(Fix.Zero);
                            component5.hasBeenThrown = false;
                            component5.DetonatesOnOwner = true;
                            component5.timedExplosion = false;
                            component5.selfDestructDelay = component5.detonationTime - (Fix)(j + 1) * (Fix)0.1;
                        }
                        else if (boplBody.GetComponent<Missile>() != null)
                        {
                            boplBody.up = Vec2.NormalizedSafe(vec6);
                        }

                        if (component4 != null)
                        {
                            component4.OwnerId = component3.OwnerId;
                        }
                    }

                    component2.ForceArrowPrefab = forceArrowPrefab2;
                   return false;
                }

                if (layer == LayerMask.NameToLayer("RigidBodyAffector"))
                {
                    BlackHole component6 = gameObject.GetComponent<BlackHole>();
                    if (component6 == null || component6.IsDestroyed)
                    {
                       return false;
                    }

                    AudioManager.Get().Play("blackholeNoClap");
                    Vec2 pos = position + vec * component6.dCircle.radius * (Fix)2L;
                    BlackHole component7 = __instance.duplicateObjectWithEffect(pos, gameObject, 0, component6.dCircle.radius * (Fix)0.75, 480).GetComponent<BlackHole>();
                    component7.SetMass(component6.GetMass());
                    component7.AddForce(component6.GetMass() * vec * (Fix)6L);
                }
                else if (layer == LayerMask.NameToLayer("weapon"))
                {
                    for (int k = 0; k < Plugin.dupe_weapon_count.Value; k++)
                    {
                        BoplBody component8 = gameObject.GetComponent<BoplBody>();
                        Fix offset2 = (Fix)2L * component8.Scale;
                        Fix angle = (Fix)1L;
                        Vec2 vec8 = __instance.FanOut(3, vec, position, offset2, angle, k);
                        Vec2 velocity2 = Vec2.NormalizedSafe(vec8 - position) * (Vec2.Magnitude(component8.velocity) + (Fix)15L);
                        DynamicAbilityPickup component9 = __instance.duplicateBoplBody(vec8, gameObject, velocity2, k, raycastInformation.pp.fixTrans.Scale).GetComponent<DynamicAbilityPickup>();
                        if (component9 != null && !component9.IsDestroyed && !component9.initSelf)
                        {
                            component9.initSelf = true;
                        }
                    }
                }
                else if (layer == LayerMask.NameToLayer("wall"))
                {
                    AudioManager.Get().PlayWithRandomPitchVariation("fireDuplicatorOnPlatform");
                    StickyRoundedRectangle component10 = gameObject.GetComponent<StickyRoundedRectangle>();
                    __instance.SpawnWallEffect((Vector3)nearPos, component10);
                    if (component10.currentDuplicatePlatformEffect != null && !component10.currentDuplicatePlatformEffect.IsDestroyed)
                    {
                        if (Plugin.dupe_remember_platforms.Value)
                        {
                            component10.currentDuplicatePlatformEffect.SelfDestruct();
                        }

                    }

                    if (__instance.useOldPlatformBehaviour)
                    {
                        component10.GetComponent<ShakablePlatform>().AddShake(__instance.PlatformShakeDuration, __instance.platformShake, 30);
                        Vec2 vec9 = nearPos;
                        DPhysicsRoundedRect rr = component10.rr;
                        Vec2 vec10 = rr.CalcExtents();
                        Vec2 right = rr.right;
                        Vec2 up = rr.up;
                        if (vec10.x < (Fix)0.05 && vec10.y < (Fix)0.05)
                        {
                            right = Vec2.right;
                            up = Vec2.up;
                        }

                        Fix value = Vec2.Dot(up, vec);
                        Fix value2 = Vec2.Dot(right, vec);
                        if (Fix.Abs(value) > Fix.Abs(value2))
                        {
                            Fix fix4 = (Fix)Fix.Sign(value);
                            vec9 = rr.position + fix4 * (Fix)2L * up * (rr.radius + vec10.y + rr.Scale);
                        }
                        else
                        {
                            Fix fix5 = (Fix)Fix.Sign(value2);
                            vec9 = rr.position + fix5 * (Fix)2L * right * (rr.radius + vec10.x + rr.Scale);
                        }

                        Tools.Debug_DrawPos(vec9, Color.red, 10f);
                        FixTransform.InstantiateFixed(__instance.platformEffect, vec9).Initialize(component10, vec, __instance.PlatformShakeDuration, __instance.GetComponent<GunTransform>().HierarchyNumber);
                    }
                    else
                    {
                        Sprite sprite = __instance.boulderSprites.GetSprite((int)component10.platformType % __instance.boulderSprites.sprites.Count);
                        Color color = component10.GetComponent<SpriteRenderer>().color;
                        Vec2 vec11 = nearPos;
                        Vec2 vec12 = component10.currentNormal(vec11);
                        RoundedRect r = DetPhysics.Get().roundedRects.colliders[DetPhysics.Get().roundedRects.ColliderIndex(component10.rr.pp.instanceId)];
                        Fix distNear = (Fix)1000000L;
                        Fix distFar = (Fix)2000000L;
                        Vec2 vec13 = firepointFIX - vec2 * (Fix)100L;
                        if (Raycast.RayCastRoundedRect(vec13, vec2, r, out distNear, out distFar))
                        {
                            vec11 = vec13 + vec2 * distNear;
                            vec12 = component10.currentNormal(vec11);
                        }

                        Boulder boulder = FixTransform.InstantiateFixed(__instance.boulderPrefab, vec11);
                        SpriteRenderer component11 = boulder.GetComponent<SpriteRenderer>();
                        component11.sprite = sprite;
                        component11.color = color;
                        FixTransform component12 = boulder.GetComponent<FixTransform>();
                        StickyRoundedRectangle component13 = boulder.GetComponent<StickyRoundedRectangle>();
                        component13.pickupPlayerFrames = 3;
                        component13.platformType = component10.platformType;
                        boulder.GetComponent<BoplBody>().velocity = vec12 * __instance.boulderSpeed;
                        boulder.hitbox.ManualInit();
                        boulder.hitbox.Scale = __instance.fixTrans.Scale;
                        DetPhysics.Get().AddObjectToDeleteList(component12, 1800);
                    }
                }
                else if (layer == LayerMask.NameToLayer("LethalTerrain") || layer == LayerMask.NameToLayer("NonLethalTerrain"))
                {
                    BoplBody boplBody2 = __instance.duplicateBoplBody(nearPos, gameObject, Vec2.zero, -1, raycastInformation.pp.fixTrans.Scale);
                    RocketEngine component14 = boplBody2.GetComponent<RocketEngine>();
                    SpikeAttack spikeAttack = null;
                    SimpleSparkNode simpleSparkNode = null;
                    Fix fix6 = (Fix)7L;
                    Fix fix7 = Fix.One;
                    if (component14 == null)
                    {
                        spikeAttack = boplBody2.GetComponent<SpikeAttack>();
                        fix6 = (Fix)7L;
                        fix7 = -Fix.One;
                        if (spikeAttack == null)
                        {
                            simpleSparkNode = boplBody2.GetComponent<SimpleSparkNode>();
                            fix6 = (Fix)5L;
                            fix7 = Fix.One;
                            if (simpleSparkNode == null)
                            {
                                hasFired = true;
                               return false;
                            }
                        }
                    }

                    StickyRoundedRectangle component15 = gameObject.transform.parent.GetComponent<StickyRoundedRectangle>();
                    FixTransform component16 = gameObject.GetComponent<FixTransform>();
                    boplBody2.GetFixTransform().startScale = component16.startScale;
                    boplBody2.GetFixTransform().offset = component16.offset;
                    Fix scale = raycastInformation.pp.fixTrans.Scale;
                    Fix fix8 = (Fix)Fix.Sign2(Vec2.Dot(Vec2.MulByNegI(component15.currentNormal(position)), vec));
                    Fix fix9 = component15.Circumference();
                    Fix fix10 = component15.LocalPlayerPosFromPos(raycastInformation.pp.fixTrans.position, Fix.Zero);
                    Fix fix11 = fix6 * fix8 / fix9;
                    fix10 = (fix10 + fix11 + (Fix)1000L) % Fix.One;
                    Vec2 vec14 = component15.PositionFromLocalPlayerPos(fix10, Fix.Zero);
                    boplBody2.up = fix7 * component15.currentNormal(vec14);
                    boplBody2.transform.parent = component15.transform;
                    Fix fix12 = Fix.Zero;
                    if (spikeAttack != null)
                    {
                        fix12 = (Fix)0.75;
                        spikeAttack.Initialize(vec14, fix12, component15, scale, updateStartScale: false);
                    }
                    else if (component14 != null)
                    {
                        fix12 = (Fix)0.3;
                        component14.RocketEngineInit(component15, fix12, vec14, scale, playerId, updateStartScale: false);
                    }
                    else if (simpleSparkNode != null)
                    {
                        fix12 = (Fix)2.2;
                        simpleSparkNode.Initialize(playerId, null, fix12, fix10, component15, scale);
                    }

                    __instance.onSpawnParticle[0].transform.localScale = __instance.onSpawnParticlePrefab.transform.localScale * (float)scale;
                    __instance.onSpawnParticle[0].transform.position = (Vector3)(vec14 + boplBody2.up * fix12);
                    __instance.onSpawnParticle[0].Play();
                }
                else if (layer == LayerMask.NameToLayer("Player"))
                {
                    for (int k = 0; k < Plugin.dupe_players.Value; k++)
                    {
                        SlimeController slimeController = gameObject.GetComponent<SlimeController>();
                        AbilityMonoBehaviour component17 = gameObject.GetComponent<AbilityMonoBehaviour>();
                        int playerId2 = gameObject.GetComponent<IPlayerIdHolder>().GetPlayerId();
                        if (component17 != null)
                        {
                            slimeController = component17.GetSlimeController();
                        }

                        if (slimeController == null)
                        {
                            hasFired = true;
                            return false;
                        }

                        PlayerCollision playerCollision = slimeController.GetActivePlayerCollision();
                        if (playerCollision == null)
                        {
                            playerCollision = slimeController.GetPlayerCollision();
                        }

                        if(Plugin.dupe_keep_abilities.Value)
                        {
                            Player player = PlayerHandler.Get().GetPlayer(playerId2);
                            SlimeController slimeController2 = playerCollision.SpawnClone(player, slimeController, position);
                            if (slimeController2 == null)
                            {
                                hasFired = true;
                                return false;
                            }

                            slimeController2.isAbilitylessClone = false;
                            slimeController2.ReSpawn(position);
                            slimeController2.Spawn();
                            Vec2 u = slimeController.CurrentNormal();
                            slimeController2.PopIntoAir(Vec2.Slerp(vec, u, (Fix)0.3));
                        } else
                        {

                        Player player = PlayerHandler.Get().GetPlayer(playerId2);
                        SlimeController slimeController2 = playerCollision.SpawnAbilityLessClone(player, slimeController, position);
                        if (slimeController2 == null)
                        {
                            hasFired = true;
                            return false;
                        }

                        slimeController2.isAbilitylessClone = !Plugin.dupe_keep_abilities.Value;
                        slimeController2.ReSpawn(position);
                        slimeController2.Spawn();
                        Vec2 u = slimeController.CurrentNormal();
                        slimeController2.PopIntoAir(Vec2.Slerp(vec, u, (Fix)0.3));
                        }
                    }

                }

            }
            else
            {
                __instance.spawnRayCastEffect((Vector2)firepointFIX, (Vector2)vec, (float)__instance.maxDistance, didHit: false);
            }

            hasFired = true;
            return false;
        }

    }
}
