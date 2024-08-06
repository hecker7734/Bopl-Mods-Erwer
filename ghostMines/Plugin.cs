using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using Steamworks.Ugc;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace ghostMines
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.ghostMines";
        public const string PLUGIN_NAME = "ghostMines";
        public const string PLUGIN_VERSION = "1.0.0";

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }
    }

    public class Patches
    {
        [HarmonyPatch(typeof(Mine), "Update")]
        [HarmonyPrefix]
        public static void MinePatch(Mine __instance)
        {
            var ren = __instance.gameObject.GetComponent<SpriteRenderer>();
            Color color2 = ren.color;
            __instance.maxAlpha = .2f;

            if (__instance.state == Mine.MineState.armed)
            {
                color2.a = 0f;
            }
            else if (__instance.state == Mine.MineState.off)
            {
                color2.a = 1.0f - ((float)__instance.timeSinceSpawned * 0.5f); // Math :bhappy:
                color2.a = Mathf.Clamp(color2.a, 0.0f, 1.0f); // Ensure alpha stays within the valid range

            }
            else
            { 
                color2.a = 0.2f;
            }

            ren.color = color2;
        }

        [HarmonyPatch(typeof(Mine), nameof(Mine.ScanForPlayers))]
        [HarmonyPrefix]
        public static bool ScanForPlayers(Mine __instance, ref FixTransform __result)
        {
            PhysicsParent[] scanHitsBuffer = new PhysicsParent[4];
            int num = 0;
            num = DetPhysics.Get().CircleCastAll(__instance.body.position, __instance.scanRadius, __instance.ScansFor, ref scanHitsBuffer);
            int num2 = 0;
            while (num2 < num)
            {
                IPlayerIdHolder component = scanHitsBuffer[num2].fixTrans.GetComponent<IPlayerIdHolder>();
                if (component != null)
                {
                    Player player = PlayerHandler.Get().GetPlayer(component.GetPlayerId());
                    if (player != null)
                    {
                        __result = scanHitsBuffer[0].fixTrans;
                        return false;
                    }
                }

                num2++;
            }

            __result = null;
            return false;
        }
    }

}