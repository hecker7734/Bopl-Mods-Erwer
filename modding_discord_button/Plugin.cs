using System.IO;
using System.Security.Policy;
using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.ParticleSystem.PlaybackState;
using static UnityEngine.UI.GridLayoutGroup;
using Image = UnityEngine.UI.Image; // Alias for the Image class

namespace modding_discord_button
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string AUTHOR = "erwer";
        public const string PLUGIN_GUID = "com.erwer.modding_discord_button";
        public const string PLUGIN_NAME = "modding_discord_button";
        public const string PLUGIN_VERSION = "1.0.0";
        private GameObject modding_discord_button;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }
        


        private void Update()
        {

            if(modding_discord_button == null)
            {
                GameObject canvas = GameObject.Find("Canvas (1)");
                Sprite bg = texTils.LoadTexture(Path.Combine(BepInEx.Paths.BepInExRootPath, "plugins" , AUTHOR+"-"+PLUGIN_NAME , "mds_erwer_discordCircle.png"));

                GameObject button_discord = new GameObject("Discord-Link-Modding");
                RectTransform butn_pos = button_discord.AddComponent<RectTransform>();


                float commonPos =  .053f + 0f; // 0.045f;
                float commonPosY = .03f + 0f; // 0.030f;
                butn_pos.anchoredPosition = new Vector2(0f, 0f);
                butn_pos.anchorMin = new Vector2(commonPos, commonPosY); 
                butn_pos.anchorMax = new Vector2(commonPos, commonPosY); 
                butn_pos.pivot = new Vector2(commonPos, commonPosY);
                butn_pos.sizeDelta = new Vector2(50f, 50f) * 3f;

                Image discordBtn = button_discord.AddComponent<Image>();
                button_discord.AddComponent<UnityEngine.UI.Button>();
                
                discordBtn.sprite = bg;
                button_discord.SetActive(true);
                button_discord.transform.SetParent(canvas.transform, false);

                discordBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
                    Application.OpenURL("https://discord.gg/official-bopl-battle-modding-comunity-1175164882388275310");
                });

                // Get the ColorBlock of the Button to modify state colors
                ColorBlock colors = discordBtn.GetComponent<UnityEngine.UI.Button>().colors;
                colors.normalColor = Color.white; // The default color of the button
                colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f); // Brighten the button on hover (slightly more than white)
                colors.pressedColor = new Color(0.9f, 0.9f, 0.9f); // Darker when pressed
                colors.selectedColor = Color.white; // Selected state
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f); // Dim color when disabled
                discordBtn.GetComponent<UnityEngine.UI.Button>().colors = colors; // Apply the modified ColorBlock

                modding_discord_button = button_discord;
            }
        }
    }
    public static class texTils
    {
        public static Sprite LoadTexture(string path)
        {
            byte[] array = File.ReadAllBytes(path);
            Texture2D val = new Texture2D(2, 2);
            ImageConversion.LoadImage(val, array);
            return Sprite.Create(val, new Rect(0.0f, 0.0f, val.width, val.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
    }

    public static class Patches
    {
        [HarmonyPatch(typeof(CharacterSelectHandler_online), "Awake")]
        static void Prefix(CharacterSelectBox __instance)
        {
            CharacterSelectHandler_online.clientSideMods_you_can_increment_this_to_enable_matchmaking_for_your_mods__please_dont_use_it_to_cheat_thats_really_cringe_especially_if_its_desyncing_others___you_didnt_even_win_on_your_opponents_screen___I_cannot_imagine_a_sadder_existence++;
        }
    }
}