using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
        public const string PLUGIN_VERSION = "4.3.3";
        internal static ConfigFile config;

        internal static ConfigEntry<float> gust_strength;
        internal static ConfigEntry<float> gust_radius;

        internal static ConfigEntry<float> BeamSize;
        internal static ConfigEntry<float> BeamMax_Time;

        //internal static ConfigEntry<int> RevivalCap;

        internal static ConfigEntry<int> dupe_weapon_count;
        internal static ConfigEntry<int> dupe_projectile_count;
        internal static ConfigEntry<int> dupe_item_n_smoke;
        internal static ConfigEntry<int> dupe_players;
        internal static ConfigEntry<int> dupe_lifespan;

        internal static ConfigEntry<bool> dupe_keep_abilities;
        internal static ConfigEntry<bool> dupe_remember_platforms;

        internal static ConfigEntry<bool> tapbow;
        internal static ConfigEntry<bool> tapnade;

        // Assuming Fix is a class or struct that has a constructor accepting a float
        public static Dictionary<string, float> allAbilities = new Dictionary<string, float>
{
    { "Dash", 4f },
    { "Bow", 2.5f },
    { "Grenade", 2.25f },
    { "Engine", 3.5f },
    { "Gust", 5f },
    { "Blink gun", 2.5f },
    { "Growth ray", 2.5f },
    { "Missile", 4f },
    { "Rock", 2f },
    { "Spike", 3f },
    { "Smoke", 3f },
    { "Time stop", 3f },
    { "Platform", 6f },
    { "Roll", 3.5f },
    { "Revival", 10f },
    { "Shrink ray", 2.5f },
    { "Black hole", 4.5f },
    { "Meteor", 4f },
    { "Push", 1f },
    { "Throw", 1f },
    { "Tesla coil", 1f },
    { "Mine", 4.5f },
    { "Beam", 5.5f },
    { "Drill", 4f },
    { "Duplicator", 4f }
};
        public static Dictionary<string, ConfigEntry<float>> configEntries = new Dictionary<string, ConfigEntry<float>>();


        Dictionary<string, string> config_set = new Dictionary<string, string>
        {
            { "beam", "Beam" },
            { "revive", "Revival" },
            { "dupe", "Duplicator Ray" },
            { "gust", "Gust" },
            { "tap", "tap items"}
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
            AGoodNameLib.auto_config.Slider<float>(ref gust_strength, config, config_set["gust"], "Changes the strength (multiplier) of gust.", 1f, 1f, 100f);

            // Assuming 'config' is an instance of a configuration manager or similar
            foreach (var cfg in allAbilities)
            {
                // Bind or load the config entry
                ConfigEntry<float> configEntry = config.Bind(
                    "Cooldowns",
                    cfg.Key,
                    cfg.Value,
                    new ConfigDescription($"Cooldown for {cfg.Key} ability")
                );

                // Optionally, you might want to store these entries if needed later
                // For example, in a dictionary to easily access them by ability name
                configEntries[cfg.Key] = configEntry;
            }


            util.CheckBox(ref dupe_keep_abilities, config, config_set["dupe"], "Keep your abilities after getting duped?", false);
            util.CheckBox(ref dupe_remember_platforms, config, config_set["dupe"], "Remembered platforms are deleted after re-using dupe.", true);

            util.CheckBox(ref tapbow, config, config_set["tap"], "Instantly Charge Bows", false);
            util.CheckBox(ref tapnade, config, config_set["tap"], "Instantly Charge Grenades", false);

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }

    }

    public partial class Patches
    {
        [HarmonyPatch(typeof(BowTransform), nameof(BowTransform.OnEnterAbility))]
        [HarmonyPostfix]
        public static void patch_tap(BowTransform __instance)
        {
            if (Plugin.tapbow.Value)
            {
                __instance.loadingFrame = 4;
            }
        }

        [HarmonyPatch(typeof(ThrowItem2), nameof(ThrowItem2.Awake))]
        [HarmonyPostfix]
        public static void patch_tap_grenade(ThrowItem2 __instance)
        {
           Debug.Log($"patch_tap_grenade called. tapnade.Value: {Plugin.tapnade.Value}");

            if (Plugin.tapnade.Value)
            {
                __instance.ThrowForceGainSpeed = (Fix)900L;
                Debug.Log("Tapnade is enabled.");
            }
            else
            {
                Debug.Log("Tapnade is disabled.");
            }
        }





        [HarmonyPatch(typeof(Shockwave), nameof(Shockwave.Awake))]
        [HarmonyPrefix]
        public static void DuplicateObjectPatch(Shockwave __instance)
        {
            if (__instance == __instance.isGustAbility)
            {
                __instance.radius = (Fix)Plugin.gust_radius.Value;
                __instance.defaultForce = (Fix)Plugin.gust_strength.Value;
                __instance.platformForce = (Fix)Plugin.gust_strength.Value;
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
            __instance.timeSinceBeamStart += simDeltaTime / (beamWidthMultiplier + (Fix)Plugin.BeamMax_Time.Value);
        }

        [HarmonyPatch(typeof(Ability), nameof(Ability.Awake))]
        [HarmonyPostfix]
        public static void AbiltiyPatch(Ability __instance)
        {
            // Clean up the ability name by removing "(Clone)" if it exists
            string abilityName = __instance.name;
            string suffix = "(Clone)";
            if (abilityName.EndsWith(suffix))
            {
                abilityName = abilityName.Substring(0, abilityName.Length - suffix.Length).Trim();
            }

            // Check if configEntries is initialized
            if (Plugin.configEntries == null)
            {
                Debug.LogError("Plugin.configEntries is null.");
                return;
            }

            // Attempt to retrieve the config entry for the ability
            if (Plugin.configEntries.TryGetValue(abilityName, out var configEntry))
            {
                // Check if configEntry is null
                if (configEntry == null)
                {
                    Debug.LogError($"Config entry for '{abilityName}' is null.");
                    return;
                }

                // Apply the cooldown
                __instance.Cooldown = (Fix)configEntry.Value;
            }
            else
            {
                Debug.LogWarning($"No config entry found for ability '{abilityName}'.");
            }
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