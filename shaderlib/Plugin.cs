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
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            // Create Harmony instance and apply patches
            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));

            
        }

        private void Start()
        {
            // Load the asset bundle and get the shader
            string assetBundlePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "vectorart.assetbundle");
            Shader vectorShader = shaderloader.GetShaderFromBundle(shaderloader.LoadBundle(assetBundlePath), "Vector");
            ApplyShaderToGameObjects(vectorShader);
        }

        private void ApplyShaderToGameObjects(Shader shader)
        {
            // Find all GameObjects in the scene
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                // Exclude GameObjects with Camera component
                if (obj.GetComponent<Camera>() == null)
                {
                    // Check if the GameObject has a Renderer (e.g., MeshRenderer)
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        // Create a new material with the loaded shader
                        Material newMaterial = new Material(shader);

                        // Assign the new material to the existing materials
                        Material[] existingMaterials = renderer.materials;
                        Material[] updatedMaterials = new Material[existingMaterials.Length + 1];
                        existingMaterials.CopyTo(updatedMaterials, 0);
                        updatedMaterials[existingMaterials.Length] = newMaterial;

                        // Apply the updated materials back to the renderer
                        renderer.materials = updatedMaterials;

                        Logger.LogInfo($"Applied shader to {obj.name}");
                    }
                }
            }
        }
    }
}