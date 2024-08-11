using System.Collections;
using System.Collections.Generic;
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
        public const string PLUGIN_VERSION = "1.0.0";
        internal static ConfigFile config;

        internal static ConfigEntry<float> BeamSize;
        internal static ConfigEntry<float> BeamMax_Time;

        Dictionary<string,string> config_set = new Dictionary<string, string>
        {
            { "beam", "Beam" }
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
         * */


        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
            config = Config;

            AGoodNameLib.auto_config.Slider<float>(ref BeamSize, config, config_set["beam"], "Changes beams base size", 1f, 1f, 10f);   
            AGoodNameLib.auto_config.Slider<float>(ref BeamMax_Time, config, config_set["beam"], "Changes beams max time", 1f, 1f, 10f);
            
            
            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }
    }

    public class Patches
    {


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
}