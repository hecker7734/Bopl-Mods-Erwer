using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;

namespace phaseDash
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.phaseDash";
        public const string PLUGIN_NAME = "phaseDash";
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
        [HarmonyPostfix]
        public static void PatchDash(Dash __instance)
        {
            Fix fix = __instance.smearBaseLength * __instance.body.fixtrans.Scale;

            if (Vec2.Magnitude(__instance.inputVector) < Constants.DEADZONE)
                {
                    __instance.inputVector = Vec2.up;
                }
                __instance.inputVector = Vec2.Normalized(__instance.inputVector);
                Fix rotationFromDirection = Tools.GetRotationFromDirection(__instance.inputVector, Vec2.up);
                __instance.body.rotation = rotationFromDirection;
            __instance.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, (float)rotationFromDirection * 57.29578f));
                __instance.updatesPassedInDash = 0;
                __instance.body.selfImposedVelocity = Vec2.zero;
                __instance.body.externalVelocity = Vec2.zero;
                __instance.SmearFrame.enabled = true;
                __instance.spriteRen.enabled = false;
                __instance.SmearFrame.material.SetColor("_ShadowColor", __instance.spriteRen.material.GetColor("_ShadowColor"));
                __instance.SmearFrame.material.SetColor("_MainColor", __instance.spriteRen.material.GetColor("_MainColor"));
                __instance.SmearFrame.material.SetColor("_ReflectionColour", __instance.spriteRen.material.GetColor("_ReflectionColour"));

                __instance.hitGround = null;
                __instance.SmearFrame.GetComponent<DPhysicsBox>();
                var position = __instance.transform.position;
                position = __instance.transform.position;
                Vec2 vec;
                vec = (Vec2)position + __instance.inputVector * __instance.dashLength;
                if ((Fix)vec.y <= SceneBounds.WaterHeight)
                {
                    vec.y = SceneBounds.WaterHeight;
                }
                float magnitude = (float)((Vector2)(vec - (Vec2)position)).magnitude;
                __instance.SmearFrame.transform.localScale = new Vector3(1f, magnitude / (float)fix, 1f);
                __instance.SmearFrame.material.SetVector("_Scale", new Vector4(1f, __instance.SmearFrame.transform.localScale.y * __instance.smearScaling, 1f));
            }

        
    }
}
