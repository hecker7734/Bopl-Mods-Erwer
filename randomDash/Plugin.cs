using System.Security.Policy;
using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using UnityEngine.TextCore;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace randomDash
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.randomDash";
        public const string PLUGIN_NAME = "randomDash";
        public const string PLUGIN_VERSION = "1.0.0";

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }

        public static int Normalizd(int inz)
        {
            if (inz > 0)
            {
                return 1;
            }
            return 0;
        }
    }
    
    public class Patches()
    {
        [HarmonyPatch(typeof(Dash), "performDash")]
        [HarmonyPostfix]
        public static void PatchDash(Dash __instance)
        {
            Fix Extents = (Fix)180;

            // Calculate the magnitude (length) of the input vector
            Fix magnitude = Fix.Sqrt(__instance.inputVector.x * __instance.inputVector.x + __instance.inputVector.y * __instance.inputVector.y);

            // Calculate the current angle of the vector
            Fix currentAngle = Fix.Atan2(__instance.inputVector.y, __instance.inputVector.x);

            // Get a random angle within the specified range
            Fix randomAngle = Updater.RandomFix(-Extents, Extents);

            // Calculate the new angle by adding the random angle
            Fix newAngle = currentAngle + randomAngle;

            // Convert back to Cartesian coordinates
            Fix newX = magnitude * Fix.Cos(newAngle);
            Fix newY = magnitude * Fix.Sin(newAngle);

            // Set the new input vector
            __instance.inputVector = new Vec2(newX, newY);
            // Rotate the SmearFrame sprite based on the new input 
            __instance.SmearFrame.transform.rotation = Quaternion.Euler(0f, 0f, (float)newAngle*(-1f));
        }


    }
}