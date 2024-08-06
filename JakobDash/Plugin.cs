using BepInEx;
using BoplFixedMath;
using HarmonyLib;

namespace jakobDash
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin 
    {
        public const string PLUGIN_GUID = "com.erwer.jakobDash";
        public const string PLUGIN_NAME = "jakobDash";
        public const string PLUGIN_VERSION = "1.0.0";

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }
    }
    public class Patches() {
        /* COOLDOWN */
        [HarmonyPatch(typeof(Ability), nameof(Ability.Awake))]
        [HarmonyPostfix]
        public static void CustomizeCooldown(ref Ability __instance)
        {
            if (__instance.name.StartsWith("Dash")) __instance.Cooldown = (Fix)3.2f;
        }
    }
}