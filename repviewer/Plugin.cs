using System;
using System.Text;
using System.Reflection;
using System.IO;
using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.ParticleSystem.PlaybackState;
using Object = System.Object;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Mono.Unix.Native;
using System.Linq;



namespace repviewer
{
    [HarmonyDebug]
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.repviewer";
        public const string PLUGIN_NAME = "repviewer";
        public const string PLUGIN_VERSION = "1.0.0";
        private static bool created_warning = false;
        private static bool show_replays = false;
        private static long click_count = 0;
        private static GameObject warningBox;
        private static GameObject replay_btn;
        private static String[] replays;
        private static GameObject[] buttons_file_replays;
        private static string game_path = BepInEx.Paths.GameRootPath;
        private static readonly string replays_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), "Johan Gronvall", "BoplBattle", "replays");
        //erwer-repviewer
        public static string debug_path = Path.Combine(BepInEx.Paths.BepInExRootPath, "plugins", "erwer-repviewer");
        //replay menus
        private static string current_replay = "None";
        private static GameObject replaymenu_text;
        private static GameObject replaymenu_screen;
        private static GameObject favorite_replay_btn_gameplay;
        private static TextMeshProUGUI textComp;
        private static RectTransform location;
        public static bool replaymenu_toggled_fast = false;
        public static bool replaymenu_toggled_pause = false;
        private Harmony harmony = new Harmony(PLUGIN_GUID);

        // persistent data between two games.
        public static List<String> favorited = new List<string>();
        public static string gdpath()
        {
            return debug_path;
        }

        private void Awake()
        {
            // Plugin startup logic
            // oh boy, here we go again.
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
            Logger.LogInfo(debug_path);
            harmony.PatchAll(typeof(Patches));

            // read favorites list.
            try
            {
                using StreamReader reader = new(Path.Combine(gdpath(), "favorited.repview"));
                string text = reader.ReadToEnd(); // Read the entire content as a string
                favorited = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList(); // Split by new line
                foreach (string fav in favorited)
                    Debug.Log(fav);
            } catch (Exception e) {
                Debug.Log(e + "\n ErrrNoFind");
            }

            replays = scan_replays();
        }

        private void OnApplicationQuit()
        {
            harmony.UnpatchSelf();
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(gdpath(), "favorited.repview")))
            {
                foreach (string fav in favorited)
                    outputFile.WriteLine(fav);
            }
        }

        private void Update()
        {
            if (!created_warning)
            {
                guistuff();
            }
        }
        

        private static String[] scan_replays()
        {
            Debug.Log(replays_path);
            string[] files = Directory.GetFiles(replays_path);
            foreach (string file in files)
            {
                Debug.Log(Path.GetFileName(file));
            }

            // Sort mainList, moving elements in priorityList to the top
            List<String> sortedList = files
                .OrderByDescending(x => favorited.Contains(x))
                .ThenBy(x => x)
                .ToList();

            return sortedList.ToArray();
        }

        private static GameObject newtx(string name)
        {
            GameObject txt = new GameObject(name, new Type[2]
            {
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            });

            return txt;
        }

        private static void guistuff()
        {
            GameObject canvas = null;
            Canvas canv = null;

            // Check if the active scene is "Replay"
            if (SceneManager.GetActiveScene().name.Contains("Level"))
            {
                // Create a new canvas
                if(replaymenu_screen == null)
                {
                    canvas = GameObject.Find("AbilitySelectCanvas");
                    canv = canvas.GetComponent<Canvas>();
                }
            } else
            {
                GameObject.Destroy(replaymenu_text);
            }
            //btnText.transform.position = new Vector3(-246.50f, 128.92f,0f);
            if (canvas != null && GameLobby.isPlayingAReplay)
            {
                // Ensure replaymenu_text is not null
                if (replaymenu_text == null)
                {

                    replaymenu_text = new GameObject("ReplayMenuText", new Type[2]
                    {
                         typeof(RectTransform),
                         typeof(TextMeshProUGUI)
                    });
                    replaymenu_text.transform.SetParent(((Component)canv).transform);
                    textComp = replaymenu_text.GetComponent<TextMeshProUGUI>();
                    ((Graphic)textComp).raycastTarget = false;
                    ((TMP_Text)textComp).fontSize = 20f;
                    ((TMP_Text)textComp).alignment = TextAlignmentOptions.TopRight;
                    ((TMP_Text)textComp).font = LocalizedText.localizationTable.GetFont(Settings.Get().Language, false);
                    location = replaymenu_text.GetComponent<RectTransform>();
                    location.pivot = new Vector2(0f, 1f);
                    replaymenu_text.SetActive(true);
                }
                else
                {
                    StringBuilder textBuilder = new StringBuilder();

                    textBuilder.AppendLine($"<#FF0000>Viewing Replay {current_replay} <#FFFFFF>");
                    textBuilder.AppendLine("<#FFF000>Tilda(~) to leave replay.<#FFFFFF>");
                    textBuilder.AppendLine(replaymenu_toggled_fast ? "<#00FF00>2xSpeed(F1)<#FFFFFF>" : "<#FF0000>2x Speed(F1)<#FFFFFF>");
                    textBuilder.AppendLine(replaymenu_toggled_pause ? "<#00FF00>Pause(F3)<#FFFFFF>" : "<#FF0000>Pause(F3)<#FFFFFF>");


                    ((TMP_Text)textComp).text = textBuilder.ToString();


                    Rect rect = ((Component)canv).GetComponent<RectTransform>().rect;
                    float height = ((Rect)(rect)).height;
                    rect = ((Component)canv).GetComponent<RectTransform>().rect;
                    float width = ((Rect)(rect)).width;
                    int num = 20;
                    location.anchoredPosition = new Vector2(width / 2f - 325f, height / 2f - 100f - (float)num);
                }
                return;
            }
            else if (!GameLobby.isPlayingAReplay)
            {
                GameObject.Destroy(replaymenu_text);
                Plugin.replaymenu_toggled_pause = false;
                Plugin.replaymenu_toggled_fast = false;
            }

            
            if (!GameLobby.isPlayingAReplay && canvas != null && SceneManager.GetActiveScene().name.Contains("Level") && favorite_replay_btn_gameplay==null)
            {
                GameObject btn = newtx("Favorite");
                Button btnComponent = btn.AddComponent<Button>();

                btn.transform.SetParent(((Component)canv).transform);
                textComp = btn.GetComponent<TextMeshProUGUI>();
                ((Graphic)textComp).raycastTarget = true; 
                ((TMP_Text)textComp).fontSize = 20f;
                ((TMP_Text)textComp).alignment = TextAlignmentOptions.TopRight;
                ((TMP_Text)textComp).font = LocalizedText.localizationTable.GetFont(Settings.Get().Language, false);
                ((TMP_Text)textComp).text = "Favorite";

                ColorBlock buttonColors = btnComponent.colors;
                buttonColors.normalColor = new Color(1, 1, 1, 1f); // Button color
                buttonColors.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f); // Highlight color
                buttonColors.pressedColor = new Color(0.1f, 0.5f, 0.8f, 1f); // Pressed color
                buttonColors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Disabled color
                btnComponent.colors = buttonColors;

                btnComponent.targetGraphic = textComp;

                location = btn.GetComponent<RectTransform>();
                location.pivot = new Vector2(0f, 1f);
                btn.SetActive(true);

                Rect rect = ((Component)canv).GetComponent<RectTransform>().rect;
                float height = ((Rect)(rect)).height;
                rect = ((Component)canv).GetComponent<RectTransform>().rect;
                float width = ((Rect)(rect)).width;
                int num = 20;
                location.anchoredPosition = new Vector2(width / 2f - 325f, height / 2f - 100f - (float)num);

                // Run replay on click
                btnComponent.onClick.AddListener(() => {
                    Debug.Log($"Favorited : {Host.replaysSaved}, next one is {Host.replaysSaved + 1}");
                    favorited.Add($"{Host.replaysSaved + 1}");
                });
                favorite_replay_btn_gameplay = btn;
            }

            else if (!SceneManager.GetActiveScene().name.Contains("Level") && favorite_replay_btn_gameplay != null)
            {
                GameObject.Destroy(favorite_replay_btn_gameplay);
            }

            canvas = GameObject.Find("Canvas (1)");
            if (canvas == null)
            {
                return;
            }
            else
            {
                Canvas component = canvas.GetComponent<Canvas>();
                Rect parentRect = component.GetComponent<RectTransform>().rect;
                float height = parentRect.height;
                float width = parentRect.width;

                if (replay_btn == null)
                {
                    // Create button object
                    replay_btn = new GameObject("ReplayButton");

                    // Add RectTransform and Button components
                    RectTransform btnRectTransform = replay_btn.AddComponent<RectTransform>();
                    Button btnComponent = replay_btn.AddComponent<Button>();

                    // Add TextMeshProUGUI for button label
                    TextMeshProUGUI btnText = replay_btn.AddComponent<TextMeshProUGUI>();
                    btnText.text = "Open Replay Viewer";
                    btnText.color = Color.red;
                    btnText.alignment = TextAlignmentOptions.Center;
                    btnText.font = LocalizedText.localizationTable.GetFont(Settings.Get().Language, false);
                    btnText.fontSize = width / 80f; // Adjust font size relative to canvas width

                    // Set button dimensions and position (Wider button)
                    btnRectTransform.sizeDelta = new Vector2(400f, 75f); // Adjusted button size for a wider look

                    // Anchor the button to the top-right corner
                    btnRectTransform.anchorMin = new Vector2(1f, 1f); // Top-right corner (Min and Max both set to 1)
                    btnRectTransform.anchorMax = new Vector2(1f, 1f); // Anchored top-right
                    btnRectTransform.pivot = new Vector2(1f, 1f); // Pivot at the top-right of the button

                    // Adjust the position relative to the top-right corner
                    btnRectTransform.anchoredPosition = new Vector2(-20f, -20f); // Slight offset to keep it within the canvas boundaries

                    // Set button visual properties
                    btnComponent.targetGraphic = btnText; // Button uses the TextMeshPro as its graphic
                    btnComponent.colors = ColorBlock.defaultColorBlock; // Default color settings, can be customized

                    // Add a click event listener to the button
                    btnComponent.onClick.AddListener(OpenReplayViewer);

                    // Set parent to canvas
                    btnRectTransform.SetParent(component.transform, false);
                    replay_btn.SetActive(true);
                }

                if (!show_replays)
                {
                    return;
                }

                // Initialize buttons_file_replays to the size of replays
                buttons_file_replays = new GameObject[replays.Length];

                if (GameObject.Find("scroller_replays") == null)
                {
                    // create the scrolling content first
                    GameObject scroller_content = new GameObject("scroller_content");
                    RectTransform rect_content = scroller_content.AddComponent<RectTransform>();
                    rect_content.sizeDelta = new Vector2(400f, 75f);
                    rect_content.anchorMin = new Vector2(1f, 1f);
                    rect_content.anchorMax = new Vector2(1f, 1f);
                    rect_content.pivot = new Vector2(1f, 1f);
                    rect_content.anchoredPosition = new Vector2(-20f, -20f);
                    // Set parent to canvas  
                    scroller_content.SetActive(true);

                    GameObject scroller_replays = new GameObject("scroller_replays");
                    RectTransform rect = scroller_replays.AddComponent<RectTransform>();
                    ScrollRect scroll = scroller_replays.AddComponent<ScrollRect>();
                    // scroll settings
                    scroll.horizontal = false;
                    scroll.vertical = true;
                    scroll.scrollSensitivity = 20;
                    scroll.content = rect_content;

                    // positioning
                    rect.sizeDelta = new Vector2(400f, 75f);
                    rect.anchorMin = new Vector2(1f, 1f); 
                    rect.anchorMax = new Vector2(1f, 1f); 
                    rect.pivot = new Vector2(1f, 1f); 
                    rect.anchoredPosition = new Vector2(-20f, -20f);
                    // Set parent to canvas
                    rect.SetParent(component.transform, false);
                    scroller_replays.SetActive(true);
                    

                    rect_content.SetParent(scroller_replays.transform, false);
                }

                for (int i = 0; i < replays.Length; i++)
                {
                    string replay = replays[i];
                    string rep = Path.GetFileNameWithoutExtension(replay);

                    // Create a new button for each replay
                    GameObject rep_button = new GameObject(rep + "_btn_load");
                    RectTransform btnRectTransform = rep_button.AddComponent<RectTransform>();
                    Button btnComponent = rep_button.AddComponent<Button>();

                    // Set button color (e.g., light blue) for better contrast
                    ColorBlock buttonColors = btnComponent.colors;
                    buttonColors.normalColor = new Color(1, 1, 1, 1f); // Button color
                    buttonColors.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f); // Highlight color
                    buttonColors.pressedColor = new Color(0.1f, 0.5f, 0.8f, 1f); // Pressed color
                    buttonColors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Disabled color
                    btnComponent.colors = buttonColors;

                    // Set button dimensions and position (Wider button)
                    btnRectTransform.sizeDelta = new Vector2(550f, 75f); // Adjusted button size for a wider look

                    // Anchor the button to the top-right corner
                    btnRectTransform.anchorMin = new Vector2(1f, .9f); // Top-right corner (Min and Max both set to 1)
                    btnRectTransform.anchorMax = new Vector2(1f, .9f); // Anchored top-right
                    btnRectTransform.pivot = new Vector2(1f, 1f); // Pivot at the top-right of the button

                    // Adjust the position relative to the top-right corner
                    btnRectTransform.anchoredPosition = new Vector2(-20f, (-85f * i) - 75f); // Slight offset to keep it within the canvas boundaries

                    // Add TextMeshProUGUI for replay button label
                    TextMeshProUGUI btnText = rep_button.AddComponent<TextMeshProUGUI>();
                    btnText.text = rep;
                    if (favorited.Contains(rep))
                    {
                        btnText.color = new Color(1f,0.5f,0f);
                    }
                    else
                    {
                        btnText.color = Color.black;
                    }

                    btnText.alignment = TextAlignmentOptions.Center;
                    btnText.font = LocalizedText.localizationTable.GetFont(Settings.Get().Language, false);
                    btnText.fontSize = width / 80f;

                    // Create background object and set as child of the button
                    GameObject bgRect = new GameObject(rep + "_bg");
                    RectTransform bgRectTransform = bgRect.AddComponent<RectTransform>();
                    Image bgImage = bgRect.AddComponent<Image>();

                    // Set background color (e.g., light gray with transparency)
                    bgImage.color = new Color(0.8f, 0.8f, 0.8f, 0.5f); // Adjust RGBA as needed

                    // Set background dimensions (slightly larger than button for padding)
                    bgRectTransform.sizeDelta = new Vector2(550f, 90f); // Slightly larger than the button

                    // Anchor the background to the same corner as the button (matching the button)
                    bgRectTransform.anchorMin = btnRectTransform.anchorMin;
                    bgRectTransform.anchorMax = btnRectTransform.anchorMax;
                    bgRectTransform.pivot = btnRectTransform.pivot;

                    // Position the background slightly behind the button
                    bgRectTransform.anchoredPosition = new Vector2(0, 0); // Align it directly under the button

                    // Set parent to canvas (ensure the background is behind the button)
                    bgRectTransform.SetParent(btnRectTransform.transform, false); // Child of button to stay aligned
                    btnRectTransform.SetParent(GameObject.Find("scroller_content").transform, false); // Set button to canvas parent

                    // Run replay on click
                    btnComponent.onClick.AddListener(() => run_replay(rep));

                    // Store the button in the array
                    buttons_file_replays[i] = rep_button;
                }


                show_replays = false;
            }
        }
        private static void OpenReplayViewer()
        {
            if (buttons_file_replays != null && buttons_file_replays.Length > 0)
            {
                foreach (GameObject btn in buttons_file_replays)
                {
                    GameObject.Destroy(btn);
                }
            }
            click_count++;
            if(click_count % 2 ==0 )
            { 
                Debug.Log("Replay Viewer Closed");
                replays = scan_replays();
                return;
            }
            Debug.Log("Replay Viewer Opened");
            show_replays = true;
        }
        private void Dispose()
        {
            GameObject.Destroy(replay_btn);
            created_warning = false;
        }

        private static void run_replay(string replayName)
        {
            
            // Set the replay path for the Host class
            Host.recordReplay = false; //don't attempt to clone replays.
            Host.replayPath = Path.Combine(replays_path, replayName+".rep");
            Debug.Log($"Loading replay from: {Host.replayPath}");
            Host curhost = GameObject.Find("networkClient").GetComponent<Host>();
            current_replay = replayName;

            // Ensure the replay file exists
            if (File.Exists(Host.replayPath))
            {
                //try to add the clients..
                string text = Host.replayPath;
                if (text != null)
                {
                    // create players.
                    StartRequestPacket startRequestPacket;

                    curhost.replay = NetworkTools.ReadCompressedReplay(File.ReadAllBytes(text), out startRequestPacket);
                    SteamManager.startParameters = startRequestPacket;
                    GameLobby.isPlayingAReplay = true;
                    curhost.clients = new List<Client>();
                    for (int j = 0; j < (int)(startRequestPacket.nrOfPlayers - 1); j++)
                    {
                        curhost.clients.Add(new Client(1, new SteamConnection()));
                    }

                    float duration = duration_of_replay(File.ReadAllBytes(text));
                    Debug.Log( "Raw Duration : " + duration   );
                    Debug.Log( "Second Guess Duration : " + Math.Round(duration / 79));
                }


                SceneManager.LoadScene("Replay"); // Load the replay scene
                GameLobby.isPlayingAReplay = true;

            }
            else
            {
                Debug.LogError($"Replay file does not exist: {Host.replayPath}");

            }
        }
        public static float duration_of_replay(byte[] compressedReplay)
        {
            StartRequestPacket outreq;
            List<InputPacketQuad> decompressed = new List<InputPacketQuad>(NetworkTools.ReadCompressedReplay(compressedReplay, out outreq));
            return decompressed.Count;
        }

    }

    public class Patches
    {

        [HarmonyPatch(typeof(CharacterSelectHandler_online), "Awake")]
        static void Prefix(CharacterSelectBox __instance)
        {
           CharacterSelectHandler_online.clientSideMods_you_can_increment_this_to_enable_matchmaking_for_your_mods__please_dont_use_it_to_cheat_thats_really_cringe_especially_if_its_desyncing_others___you_didnt_even_win_on_your_opponents_screen___I_cannot_imagine_a_sadder_existence+=2;
        }
         
        [HarmonyPatch(typeof(Host), nameof(Host.PlayReplayUpdate))]
        [HarmonyPrefix]
        public static bool PlayReplayUpdate(Host __instance)
        {
            /*if (GameLobby.isPlayingAReplay)
            {
                return false;
            }*/

            if(Keyboard.current.backquoteKey.wasPressedThisFrame)
            {
                __instance.clients.Clear();
                GameLobby.isPlayingAReplay = false;
                Host.replayPath = "";

                Debug.Log("A client was disconnected, abandoning lobby");
                GameSessionHandler.LeaveGame(abandonLobbyEntirely: true);
                Host.recordReplay = true;
                Host.replayLevelsLoaded++;
            }

            __instance.timePassed += Time.deltaTime;
            double num = (double)Updater.SimulationTicks * (double)GameTime.FixedTimeStep;
            double num2 = (double)Mathf.Max((float)((double)__instance.timePassed - num) / (float)GameTime.FixedTimeStep, 0f);
            num2 = ((num2 > 4) ? 4 : num2);
            // speed modifer keys.
            if (Keyboard.current.f1Key.wasPressedThisFrame)
            {
                Plugin.replaymenu_toggled_fast = !Plugin.replaymenu_toggled_fast;
            }else if (Keyboard.current.f3Key.wasPressedThisFrame)
            {
                Plugin.replaymenu_toggled_pause = !Plugin.replaymenu_toggled_pause;
            }
            
            // apply speed modifer
            if (Plugin.replaymenu_toggled_pause)
            {
                num2 = 0;
                __instance.timePassed = (float)(num + (double)GameTime.FixedTimeStep * (double)num2);
            } else if(Plugin.replaymenu_toggled_fast)
            {
                num2 = 2;
                __instance.timePassed = (float)(num + (double)GameTime.FixedTimeStep * (double)num2);
            }

            for (int i = 0; i < num2; i++)
            {
                InputPacketQuad inputPacketQuad;
                if (__instance.replay.Count > 0)
                {
                    inputPacketQuad = __instance.replay.Dequeue();
                }
                else
                {
                    inputPacketQuad = default(InputPacketQuad);
                }
                Player player = PlayerHandler.Get().GetPlayer(1);
                Player player2 = PlayerHandler.Get().GetPlayer(2);
                Player player3 = PlayerHandler.Get().GetPlayer(3);
                Player player4 = PlayerHandler.Get().GetPlayer(4);
                if (GameTime.IsTimeStopped())
                {
                    if (player != null && player.isProtectedFromTimeStop)
                    {
                        player.OverrideInputWithNetworkInput(inputPacketQuad.p1);
                    }
                    if (player2 != null && player2.isProtectedFromTimeStop)
                    {
                        player2.OverrideInputWithNetworkInput(inputPacketQuad.p2);
                    }
                    if (player3 != null && player3.isProtectedFromTimeStop)
                    {
                        player3.OverrideInputWithNetworkInput(inputPacketQuad.p3);
                    }
                    if (player4 != null && player4.isProtectedFromTimeStop)
                    {
                        player4.OverrideInputWithNetworkInput(inputPacketQuad.p4);
                    }
                }
                else
                {
                    if (player != null)
                    {
                        player.OverrideInputWithNetworkInput(inputPacketQuad.p1);
                    }
                    if (player2 != null)
                    {
                        player2.OverrideInputWithNetworkInput(inputPacketQuad.p2);
                    }
                    if (player3 != null)
                    {
                        player3.OverrideInputWithNetworkInput(inputPacketQuad.p3);
                    }
                    if (player4 != null)
                    {
                        player4.OverrideInputWithNetworkInput(inputPacketQuad.p4);
                    }
                }
                Updater.TickSimulation(GameTime.FixedTimeStep);
                if (Updater.LoadedALevelThisUpdate)
                {
                    Updater.LoadedALevelThisUpdate = false;
                    break;
                }
            }
            return false;
        }
    }
}