using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using BepInEx.Configuration;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace arrowShotgun
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.erwer.arrowShotgun";
        public const string PLUGIN_NAME = "arrowShotgun";
        public const string PLUGIN_VERSION = "1.0.0";
        internal static ConfigFile config;
        internal static ConfigEntry<float> arrowSpread;
        internal static ConfigEntry<int> arrowCount;

        public void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            config = Config;

            
        arrowSpread = config.Bind("Settings", "Arrow Spread / Extents", 0.3f, new ConfigDescription(
            "Minimum is 0.0 (Negatives default to 0).\n MAX: 1\n Suggested Value: 0.3f", 
            new AcceptableValueRange<float>(0f, 1f)
        ));
        
        arrowCount = config.Bind("Settings", "Arrow Count", 6, new ConfigDescription(
            "Minimum is 0 (Negatives default to 0).\n MAX: 100\n Suggested Value: 6", 
            new AcceptableValueRange<int>(0, 100)
        ));

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(typeof(Patches));
        }
    }
    class Patches
    {
        [HarmonyPatch(typeof(BowTransform), nameof(BowTransform.Shoot))]
        [HarmonyPrefix]
        public static bool Shotgun(Vec2 dir, BowTransform __instance, ref Fix ___ArrowSpeed, ref PlayerBody ___body, ref BoplBody ___Arrow, ref bool ___hasFired, ref Fix ___TimeBeforeArrowsHurtOwner, ref PlayerInfo ___playerInfo, ref int ___maxNumberOfArrows)
        {
            int amount_to_spawn = Plugin.arrowCount.Value;
            Fix extents = (Fix)Plugin.arrowSpread.Value;//(Fix).3f;

            for (int i = 0; i < amount_to_spawn; i++)
            {
                Vec2 spawnPos = ___body.position;
                Fix arrowVelocity = Fix.One + (___body.fixtrans.Scale - Fix.One) / (Fix)2L;
                BoplBody arrow = FixTransform.InstantiateFixed<BoplBody>(___Arrow, spawnPos, ___body.rotation); // create arrow
                arrow.Scale = ___body.fixtrans.Scale;
                arrow.rotation = ___body.rotation;
                arrow.position = spawnPos;

                Vec2 randomOffset = new Vec2(Updater.RandomFix(-extents, extents), Updater.RandomFix(-extents, extents));
                arrow.StartVelocity = (dir + randomOffset)
                    * ((Fix)4L + Fix.One) *
                    ___ArrowSpeed *
                    arrowVelocity +
                    ___body.selfImposedVelocity;
                arrow.GetComponent<SpriteRenderer>().material = ___playerInfo.playerMaterial;
                arrow.GetComponent<IPlayerIdHolder>().SetPlayerId(___playerInfo.playerId); // makes the arrow belong to shooting player
                arrow.GetComponent<Projectile>().DelayedEnableHurtOwner(___TimeBeforeArrowsHurtOwner
                    * arrowVelocity / Vec2.Magnitude(arrow.StartVelocity)
                    ); // enable delayed hurt owner
            }

            ___hasFired = true;
            ___maxNumberOfArrows = 7;
            return false;
        }

    }
}
