using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BepInEx;
using BepInEx.Configuration;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;
using Debug = UnityEngine.Debug;


namespace AGoodNameLib
{
    public class player
    {
        /// <summary>
        /// Attempts to kill the specified player, marking the death with a specified cause of death.
        /// </summary>
        /// <param name="player">The player instance to be killed.</param>
        /// <param name="__body">The player's body that will be affected, this needs to be the same as the player being killed.</param>
        /// <param name="id">The ID of the killer. If not provided, defaults to 256.</param>
        /// <param name="cod">The cause of death. Defaults to <see cref="CauseOfDeath.Accident"/>.</param>
        /// <returns>True if the player was successfully killed, otherwise false.</returns>
        public static bool try_kill_player(Player player, PlayerBody __body, int id = 256, CauseOfDeath cod = CauseOfDeath.Accident)
        {
            UnityEngine.Debug.Log("try_kill_player should not be used, try to use do_kill_player;");
            if (player == null) return false;
            __body.GetComponent<PlayerCollision>().killPlayer(id, false, true, cod);
            return true;
        }

        public static bool do_kill_player(PlayerBody __body, int id = 256, CauseOfDeath cod = CauseOfDeath.Accident)
        {
            __body.GetComponent<PlayerCollision>().killPlayer(id, false, true, cod);
            return true;
        }

        /// <summary>
        /// Attempts to kill the specified player, marking the death with a specified cause of death.
        /// </summary>
        /// <param name="player">The player instance to be killed.</param>
        /// <returns>True if the player's spawns were removed, otherwise false</returns>
        public static bool try_destroy_respawns(Player player)
        {
            if (player == null) return false;
            player.RespawnPositions = new List<RevivePositionIndicator>();
            return true;
        }



        //sets player's pos
        public static void setPos(Player player, Vec2 position)
        {
            player.Position = position;
        }

        // gets the player from a player's body
        public static Player player_from_body(PlayerBody body)
        {
            if (body == null) return null;
            Player player = PlayerHandler.Get().GetPlayer(body.idHolder.GetPlayerId());
            return player;
        }


        // gets the player from a player's body
        public static int id_from_player_body(PlayerBody body)
        {
            if (body == null) return 256;
            return body.idHolder.GetPlayerId();
        }

        public static PlayerPhysics Physics_from_body(PlayerBody body)
        {
            if (body == null) return null;
            return body.physics;
        }

        public static void clear_ropes(PlayerBody body)
        {
            if (body == null) return;
            body.EnterAbilityClearRope();
        }
        public static bool does_own_rope(PlayerBody body)
        {
            if (player.id_from_player_body(body) == body.ropeBody.ownerId)
            {
                return true;
            }
            return false;
        }
    }

    public static class abilities
    {
        public static bool can_use_abilities(Player player)
        {
            List<GameObject> abilities = player.Abilities;
            foreach (GameObject ability in abilities)
            {
                if (ability.GetComponent<Ability>().isCastable)
                {
                    return true; // Return true as soon as we find one castable ability
                }
            }
            return false; // Return false if no castable abilities are found
        }

        public static void clear_abilities(Player player)
        {
            player.Abilities.Clear();
        }

        public static void set_ability_at_index(Player player, GameObject ability, int index = 1, String ability_class_name = "BowTransform")
        {
            player.Abilities[index] = ability;
            player.AbilityIcons[index] = AbilityClassToSprite(ability_class_name);
        }


        public static Sprite AbilityClassToSprite(string name)
        {
            RandomAbility ability = new RandomAbility();
            return ability.abilityIcons.FindSprite(name);
        }


        public static NamedSprite GetRandomAbilitySprite()
        {
            RandomAbility rng = new RandomAbility();
            return RandomAbility.GetRandomAbilityPrefab(rng.abilityIcons, rng.abilityIcons);
        }

    }

    public class auto_config
    {
        public static void Slider<T>(ref ConfigEntry<T> configVar, ConfigFile config, string sectionName, string description, T defaultValue, T minValue, T maxValue) where T : IComparable
        {
            configVar = config.Bind(sectionName, description, defaultValue, new ConfigDescription(
                description,
                new AcceptableValueRange<T>(minValue, maxValue)
            ));
        }
    
        public static void NumberBox<T>(ref ConfigEntry<T> configVar, ConfigFile config, string sectionName, string description, T defaultValue) where T : IComparable
        {
            configVar = config.Bind(sectionName, description, defaultValue, new ConfigDescription(
                description
            ));
        }

        public static void CheckBox(ref ConfigEntry<bool> configVar, ConfigFile config, string sectionName, string description, bool defaultValue)
        {
            configVar = config.Bind(sectionName, description, defaultValue, new ConfigDescription(
                description
            ));
        }


    }

    public class math   
    {
        public static int RandomInt(int min, int max)
        {
            return Updater.RandomInt(min, max);
        }
    }

    public class match
    {
        public bool SuddenDeathState
        {
            get { return GameSessionHandler.SuddenDeathInProgress; }
            set { GameSessionHandler.SuddenDeathInProgress = value; }
        }
    }
}
