using System;
using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace tex
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.tex";
        public const string PLUGIN_NAME = "tex";
        public const string PLUGIN_VERSION = "1.0.0";

        private static readonly string gameDir = System.Reflection.Assembly.GetExecutingAssembly().Location;
        private static readonly string finalgameDir = gameDir.Substring(0, gameDir.Length - 7); // TEX.DLL = 7
        private static readonly string customTexturePath = "img\\icons.png";
        public static Texture2D customTexture;

        
        

        private void Awake()
        {


            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded: " + finalgameDir + customTexturePath);

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));

            byte[] textureData;
            try
            {
                textureData = System.IO.File.ReadAllBytes(finalgameDir + customTexturePath);
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to load texture: " + e.Message);
                return;
            }

            if (textureData == null || textureData.Length == 0)
            {
                Logger.LogError("Texture data is empty");
                return;
            }

            customTexture = new Texture2D(2048, 2048);
            if (!customTexture.LoadImage(textureData))
            {
                Logger.LogError("Failed to load image into texture");
                return;
            }


        }
    }
    public class Patches
    {   
        [HarmonyPatch(typeof(AbilityGridEntry), nameof(AbilityGridEntry.Awake))]
        [HarmonyPostfix]
        public static void tex(AbilityGridEntry __instance)
        {
            __instance.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>().sprite = TextureToSprite(Plugin.customTexture);

        }
        private static Sprite TextureToSprite(Texture2D texture)
        {
            // Use the entire texture as the sprite, with no pivot offset
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}