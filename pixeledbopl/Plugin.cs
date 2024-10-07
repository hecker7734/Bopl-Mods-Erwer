using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Reflection;

namespace pixeledbopl
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.pixeledbopl";
        public const string PLUGIN_NAME = "pixeledbopl";
        public const string PLUGIN_VERSION = "1.0.0";

        private static Shader pixelArtShader;
        public static Material pixelArtMaterial;
        private AssetBundle pixelArtAssetBundle;

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();

            LoadAssetBundle();
            LoadPixelArtShader();
        }

        private void Update() {
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
            foreach (Camera camera in cameras)
            {
                if (camera != null && camera.gameObject.GetComponent<CameraEffect>() == null)
                {
                    camera.gameObject.AddComponent<CameraEffect>();
                    Debug.Log($"Added Camera Effect to {camera.gameObject.name}");
                }
            }
        }

        private void LoadAssetBundle()
        {
            // Specify the path to your AssetBundle
            string assetBundlePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "shaderpixel"); // Adjust this path if necessary
            Debug.Log(assetBundlePath);

            pixelArtAssetBundle = AssetBundle.LoadFromFile(assetBundlePath);

            if (pixelArtAssetBundle == null)
            {
                Logger.LogError("Failed to load AssetBundle!");
                return;
            }
            Logger.LogInfo("AssetBundle loaded successfully!");
        }

        private void LoadPixelArtShader()
        {
            if (pixelArtAssetBundle != null)
            {
                // Load the shader from the AssetBundle
                pixelArtShader = pixelArtAssetBundle.LoadAsset<Shader>("Pixelation"); // Replace "ShaderName" with the actual name of the shader

                if (pixelArtShader != null)
                {
                    pixelArtMaterial = new Material(pixelArtShader);
                    pixelArtMaterial.SetFloat("_PixelSize", 0.5f); // Set the pixel size if needed
                    Logger.LogInfo("Pixel Art Shader loaded successfully!");
                }
                else
                {
                    Logger.LogError("Pixel Art Shader not found in AssetBundle!");
                }
            }
        }


        private void OnDestroy()
        {
            // Unload the AssetBundle when the plugin is destroyed
            if (pixelArtAssetBundle != null)
            {
                pixelArtAssetBundle.Unload(false);
                Logger.LogInfo("AssetBundle unloaded successfully!");
            }
        }
    } 

    public class CameraEffect : MonoBehaviour
    {
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (Plugin.pixelArtMaterial != null)
            {
                Graphics.Blit(source,destination);
                return;
            }
            Graphics.Blit(source, destination, Plugin.pixelArtMaterial);
        }
    }


}
