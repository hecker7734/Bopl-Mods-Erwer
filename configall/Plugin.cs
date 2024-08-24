using System;
using System.Collections;
using System.Collections.Generic;
using AGoodNameLib;
using BepInEx;
using BepInEx.Configuration;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace configall
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.configall";
        public const string PLUGIN_NAME = "configall";
        public const string PLUGIN_VERSION = "4.2.0";
        internal static ConfigFile config;

        //internal static ConfigEntry<float> gust_strength;
        internal static ConfigEntry<float> gust_radius;

        internal static ConfigEntry<float> BeamSize;
        internal static ConfigEntry<float> BeamMax_Time;
        
        internal static ConfigEntry<int> RevivalCap;

        internal static ConfigEntry<int> dupe_weapon_count;
        internal static ConfigEntry<int> dupe_projectile_count;
        internal static ConfigEntry<int> dupe_item_n_smoke;
        internal static ConfigEntry<int> dupe_players;
        internal static ConfigEntry<int> dupe_lifespan;

        internal static ConfigEntry<bool> dupe_keep_abilities;
        internal static ConfigEntry<bool> dupe_remember_platforms;


        Dictionary<string,string> config_set = new Dictionary<string, string>
        {
            { "beam", "Beam" },
            { "revive", "Revival" },
            { "dupe", "Duplicator Ray" },
            { "gust", "Gust" }
        };

        /*
         * player       X
         * grenade      X
         * arrow        X 
         * drill        X 
         * mine         X
         * 
         * 
         * Beam         X
         * revival      -
         * Duplicator   X 
         * Gust         X
         */


        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
            config = Config;

            AGoodNameLib.auto_config.Slider<float>(ref BeamSize, config, config_set["beam"], "Changes beams base size", 1f, 1f, 100f);   
            AGoodNameLib.auto_config.Slider<float>(ref BeamMax_Time, config, config_set["beam"], "Changes beams max time", 1f, 1f, 100f);
            //AGoodNameLib.auto_config.Slider<int>(ref RevivalCap, config, config_set["revive"], "Changes cap for revives", 3, 1, 100);

            AGoodNameLib.auto_config.Slider<int>(ref dupe_weapon_count, config, config_set["dupe"], "Changes Amount of duplicated objects that are WEAPONS..", 3, 3, 10);
            AGoodNameLib.auto_config.Slider<int>(ref dupe_projectile_count, config, config_set["dupe"], "Changes Amount of duplicated objects that are PROJECTILES.", 4, 4, 10);
            AGoodNameLib.auto_config.Slider<int>(ref dupe_item_n_smoke, config, config_set["dupe"], "Changes Amount of duplicated objects that are ITEMS/SMOKE.", 1, 1, 10);
            AGoodNameLib.auto_config.Slider<int>(ref dupe_lifespan, config, config_set["dupe"], "Changes Lifetime of duplicated objects", 1800, 0, 10000);
            
            AGoodNameLib.auto_config.Slider<int>(ref dupe_players, config, config_set["dupe"], "Changes Amount of duplicated objects that are PLAYERS.", 1, 1, 16);

            AGoodNameLib.auto_config.Slider<float>(ref gust_radius, config, config_set["gust"], "Changes the radius of gust.", 5f, 1f, 100f);

            util.CheckBox(ref dupe_keep_abilities, config, config_set["dupe"], "Keep your abilities after getting duped?", false);
            util.CheckBox(ref dupe_remember_platforms, config, config_set["dupe"], "Remembered platforms are deleted after re-using dupe.", true);




            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }
    }

    public partial class Patches
    {
        /*[HarmonyPatch(typeof(Revive), nameof(Revive.SetReviveFlag))]
        [HarmonyPrefix]
        public static bool SetReviveFlag(Revive __instance, ref RevivePositionIndicator reviveIndicator, ref Player ___player)
        {
            if (reviveIndicator != null && !reviveIndicator.IsDestroyed || ___player.RespawnPositions.Count > Plugin.RevivalCap.Value)
            {
                reviveIndicator.End();
                int num = ___player.RespawnPositions.Count - 1;
                while (num >= 0 && num < ___player.RespawnPositions.Count)
                {
                    if (___player.RespawnPositions.Count > Plugin.RevivalCap.Value  || ___player.RespawnPositions[num] == null || (___player.RespawnPositions[num] != null && ___player.RespawnPositions[num].IsDestroyed))
                    {
                        ___player.RespawnPositions.RemoveAt(num);
                    }
                    else
                    {
                        num--;
                    }
                }
            }

            reviveIndicator = reviveIndicator.GetComponent<RevivePositionIndicator>();
            ___player = PlayerHandler.Get().GetPlayer(__instance.GetComponent<IPlayerIdHolder>().GetPlayerId());
            ___player.ExtraLife = ReviveStatus.willReviveOnDeath;
            ___player.ReviveInstance = __instance;
            ___player.RespawnPositions.Add(reviveIndicator);
            return true;
        }*/

        [HarmonyPatch(typeof(Shockwave), nameof(Shockwave.Awake))]
        [HarmonyPrefix]
        public static void DuplicateObjectPatch(Shockwave __instance)
        {
            if (__instance == __instance.isGustAbility)
            {
                __instance.radius = (Fix)Plugin.gust_radius.Value;
            }

        }


        [HarmonyPatch(typeof(ShootDuplicator), nameof(ShootDuplicator.DuplicateObject))]
        [HarmonyPrefix]
        public static void DuplicateObjectPatch(ShootDuplicator __instance, ref int lifespan)
        {
            lifespan = Plugin.dupe_lifespan.Value;
        }



        [HarmonyPatch(typeof(Beam), "UpdateBeam")]
        [HarmonyPrefix]
        public static void UpdateBeam(Beam __instance, Fix simDeltaTime)
        {
            Vec2 vec = __instance.FirePos();
            Fix y = Fix.One;
            Fix fix = __instance.inAir ? __instance.maxTimeAir : __instance.maxTime;
            Fix beamWidthMultiplier = (Fix)Plugin.BeamSize.Value;

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
            
            //custom time based on the width + beam time max.
            __instance.timeSinceBeamStart += simDeltaTime /  (beamWidthMultiplier + (Fix)Plugin.BeamMax_Time.Value);
        }



    }
    public static class util
    {
        public static void CheckBox(ref ConfigEntry<bool> configVar, ConfigFile config, string sectionName, string description, bool defaultValue)
        {
            configVar = config.Bind(sectionName, description, defaultValue, new ConfigDescription(
                description
            ));
        }
    }
}