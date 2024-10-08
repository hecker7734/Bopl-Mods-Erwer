using System.IO;
using System.Reflection;
using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace shaderlib
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.shaderlib";
        public const string PLUGIN_NAME = "shaderlib";
        public const string PLUGIN_VERSION = "1.0.0";
        public static Material vectuh;


        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            // Create Harmony instance and apply patches
            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));

            string assetBundlePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "vectorart.assetbundle");
            Shader vectorShader = shaderloader.GetShaderFromBundle(shaderloader.LoadBundle(assetBundlePath), "Vector");
            vectuh = shaderloader.MaterialFromShader(vectorShader);
        }
    }
    public class Patches
    {
        [HarmonyPatch(typeof(BoplBody), nameof(BoplBody.Awake))]
        [HarmonyPostfix]
        public static void sh(BoplBody __instance) {
            shaderlib.AddMaterialToGameObject component = __instance.gameObject.AddComponent<shaderlib.AddMaterialToGameObject>();
            component.newMaterial = Plugin.vectuh;
        }
    }
}