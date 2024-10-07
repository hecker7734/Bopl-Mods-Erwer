using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace pixeledbopl
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.pixeledbopl";
        public const string PLUGIN_NAME = "pixeledbopl";
        public const string PLUGIN_VERSION = "1.0.0";

        internal static ConfigFile config;
        internal static ConfigEntry<float> pixelation;
        internal static ConfigEntry<bool> slime_trail;

        private static Shader pixelArtShader;
        public static Material pixelArtMaterial;
        private AssetBundle pixelArtAssetBundle;

        private void Awake()
        {
            config = Config;
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));

            //Note pixelation is the same as the defined config entry
            pixelation = config.Bind("Settings", "Pixelation", 0.0015f, new ConfigDescription(
                "Higher = Less Pixels.",
                new AcceptableValueRange<float>(0.001f, 0.01f)
            ));

            slime_trail = config.Bind("Settings", "SlimeTrail", false, new ConfigDescription(
                "Causes slime trails to blend colors with terrain, don't recommend"
            ));

            LoadAssetBundle();
            LoadPixelArtShader();
        }

        private void Update()
        {
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
            foreach (Camera camera in cameras)
            {
                if(!slime_trail.Value)
                {
                    if (camera.gameObject.name.Contains("SlimeTrailCam"))
                    {
                        continue;
                    }
                }

                if (camera != null && camera.gameObject.GetComponent<CameraEffect>() == null)
                {
                    camera.gameObject.AddComponent<CameraEffect>();
                    Debug.Log($"Added Camera Effect to {camera.gameObject.name}");
                } else if (camera != null && camera.gameObject.GetComponent<CameraEffect>() != null)
                {
                    Scene activeScene = SceneManager.GetActiveScene();
                    if (((Scene)(activeScene)).name == "CharacterSelect" || ((Scene)(activeScene)).name == "ChSelect_online" || GameSessionHandler.HasGameEnded() == true)
                    {
                        camera.gameObject.GetComponent<CameraEffect>().enabled = false;
                    } else
                    {
                        camera.gameObject.GetComponent<CameraEffect>().enabled = true;
                    }
                }
            }
            Debug.Log(SceneManager.GetActiveScene().name);
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
                pixelArtShader = pixelArtAssetBundle.LoadAsset<Shader>("Pixelation"); // Replace "Pixelation" with the actual name of the shader

                if (pixelArtShader != null)
                {
                    pixelArtMaterial = new Material(pixelArtShader);
                    pixelArtMaterial.SetFloat("_PixelSize", pixelation.Value /*0.001f*/); // Set the pixel size if needed
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
                // Apply the pixelation effect
                Debug.Log("Doing pixely art :3");
                Graphics.Blit(source, destination, Plugin.pixelArtMaterial);
            }
            else
            { 
                Graphics.Blit(source, destination);
            }
        }
    }

    public class Patches
    {
        [HarmonyPatch(typeof(CharacterSelectHandler_online), "Awake")]
        static void Prefix(CharacterSelectBox __instance)
        {
            CharacterSelectHandler_online.clientSideMods_you_can_increment_this_to_enable_matchmaking_for_your_mods__please_dont_use_it_to_cheat_thats_really_cringe_especially_if_its_desyncing_others___you_didnt_even_win_on_your_opponents_screen___I_cannot_imagine_a_sadder_existence += 2;
        }
    }
}
