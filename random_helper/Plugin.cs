using BepInEx;
using BepInEx.Configuration;
using BoplFixedMath;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using BattleLib;
using TMPro;
using IUpdateable;

namespace RandomEvents
{
    [BepInPlugin("com.PizzaMan730.RandomEvents", "RandomEvents", "1.0.0")]
    public class Plugin : BaseUnityPlugin, IUpdatable
    {
        private ConfigEntry<int> minTime;
        private ConfigEntry<int> maxTime;

        private List<ConfigEntry<bool>> allowedEvents;
        private int timeSinceEvent;

        private int timeTillNextEvent;

        private ConfigEntry<bool> showAnnouncements;

        private List<EventText> eventTexts = new List<EventText>();

        public static Fix gravityScale;

        private ConfigFile config;

        private Boulder boulderPrefab;

        private MachoThrow2 throwerPrefab;

        private BlackHole blackHolePrefab;

        private BoplBody arrowPrefab;

        private BoplBody grenadePrefab;

        private BoplBody minePrefab;

        private BoplBody smokePrefab;


        private void Awake()
        {
            Logger.LogInfo("RandomEvents has loaded!");

            Harmony harmony = new Harmony("com.PizzaMan730.RandomEvents");
            config = base.Config;
            minTime = config.Bind<int>("Times", "Minimum time", 100, "Minimum time between events");
            maxTime = config.Bind<int>("Times", "Maximum time", 1000, "Maximum time between events");
            showAnnouncements = config.Bind<bool>("Misc", "Show announcements", true, "Show announcement when event is starting");


            allowedEvents = new List<ConfigEntry<bool>>();
            allowedEvents.Add(config.Bind<bool>("Events", "Grenade Rain", true, "Allow the Grenade Rain event?"));
            allowedEvents.Add(config.Bind<bool>("Events", "Arrow Rain", true, "Allow the Arrow Rain event?"));
            allowedEvents.Add(config.Bind<bool>("Events", "Land Mines", true, "Allow the Land Mines event?"));
            allowedEvents.Add(config.Bind<bool>("Events", "Smoke Storm", true, "Allow the Smoke Storm event?"));
            allowedEvents.Add(config.Bind<bool>("Events", "Gravity Change", true, "Allow the Gravity Change event?"));
            allowedEvents.Add(config.Bind<bool>("Events", "Teleport players", true, "Allow the Teleport Players event?"));
            allowedEvents.Add(config.Bind<bool>("Events", "Black Hole Attack", true, "Allow the Black Hole Attack event?"));
            allowedEvents.Add(config.Bind<bool>("Events", "Meteor Strike", true, "Allow the Meteor Strike event?"));
            allowedEvents.Add(config.Bind<bool>("Events", "Meteor Shower", true, "Allow the Meteor Shower event?"));
            //allowedEvents.Add(config.Bind<bool>("Events", "Clone Players", true, "Allow the Clone Players event?"));


            timeSinceEvent = 0;
            timeTillNextEvent = Updater.RandomInt(minTime.Value, maxTime.Value);
            gravityScale = Fix.One;


            //MethodInfo original = AccessTools.Method(typeof(Gravity), "Awake");
            //MethodInfo patch = AccessTools.Method(typeof(myPatches), "Awake_Patch");
            //harmony.Patch(original, new HarmonyMethod(patch));
            harmony.PatchAll(typeof(Patches));
            SceneManager.sceneLoaded += OnSceneLoaded;
            GetPrefabs();
        }

        public void GetPrefabs()
        {
            Boulder prefab = new Boulder();
            MachoThrow2 thrower = new MachoThrow2();
            MachoThrow2[] throwers = Resources.FindObjectsOfTypeAll(typeof(MachoThrow2)) as MachoThrow2[];
            foreach (MachoThrow2 obj in throwers)
            {
                if (obj.name == "Throw")
                {
                    boulderPrefab = (Boulder)AccessTools.Field(typeof(MachoThrow2), "boulderPrefab").GetValue(obj);
                    throwerPrefab = obj;
                }
            }
            BlackHole prefab1 = new BlackHole();
            BlackHole[] blackHoles = Resources.FindObjectsOfTypeAll(typeof(BlackHole)) as BlackHole[];
            foreach (BlackHole obj in blackHoles)
            {
                if (obj.name == "BlackHole2")
                {
                    blackHolePrefab = obj;
                }
            }
            BoplBody prefab2 = new BoplBody();
            BoplBody[] boplBodies = Resources.FindObjectsOfTypeAll(typeof(BoplBody)) as BoplBody[];
            foreach (BoplBody obj in boplBodies)
            {
                if (obj.name == "Arrow")
                {
                    arrowPrefab = obj;
                }
            }
            BoplBody prefab3 = new BoplBody();
            boplBodies = Resources.FindObjectsOfTypeAll(typeof(BoplBody)) as BoplBody[];
            foreach (BoplBody obj in boplBodies)
            {
                if (obj.name == "Grenade")
                {
                    grenadePrefab = obj;
                }
            }
            BoplBody prefab4 = new BoplBody();
            boplBodies = Resources.FindObjectsOfTypeAll(typeof(BoplBody)) as BoplBody[];
            foreach (BoplBody obj in boplBodies)
            {
                if (obj.name == "Mine")
                {
                    minePrefab = obj;
                }
            }
            BoplBody prefab5 = new BoplBody();
            boplBodies = Resources.FindObjectsOfTypeAll(typeof(BoplBody)) as BoplBody[];
            foreach (BoplBody obj in boplBodies)
            {
                if (obj.name == "SmokeCircle")
                {
                    smokePrefab = obj;
                }
            }
        }

        public void UpdateSim(Fix SimDeltaTime)
        {
            int num = 0;
            for (int i = 0; i < eventTexts.Count; i++)
            {
                eventTexts[num].Update();
                if (eventTexts[num].timeAlive > 210) eventTexts.RemoveAt(num);
                else num++;
            }
            if (!GameTime.IsTimeStopped()) timeSinceEvent++;

            if (!BattleUtils.IsGameSessionActive())
            {
                timeSinceEvent = 0;
                ChangeGravity(true);
            }
            if (timeSinceEvent == timeTillNextEvent)
            {
                SpawnEvent();
            }
        }

        private void SpawnEvent()
        {
            timeTillNextEvent = Updater.RandomInt(minTime.Value, maxTime.Value);
            timeSinceEvent = 0;
            int eventType = Updater.RandomInt(0, 9);
            while (!allowedEvents[eventType].Value)
            {
                eventType = Updater.RandomInt(0, 9);     //Inclusive minimum, exclusive maximum
            }
            if (showAnnouncements.Value) eventTexts.Add(new EventText(eventType));

            if (eventType == 0) SpawnGrenades();
            else if (eventType == 1) SpawnArrows();
            else if (eventType == 2) SpawnMines();
            else if (eventType == 3) SpawnSmoke();
            else if (eventType == 4) ChangeGravity();
            else if (eventType == 5) TeleportPlayers();
            else if (eventType == 6) SpawnBlackHole();
            else if (eventType == 7) SpawnMeteor();
            else if (eventType == 8) SpawnMeteorShower();
            else if (eventType == 9) ClonePlayers();
        }

        private void ClonePlayers()
        {
            SlimeController[] slimeControllers = Resources.FindObjectsOfTypeAll(typeof(SlimeController)) as SlimeController[];
            foreach (SlimeController controller in slimeControllers)
            {
                if (controller.name.StartsWith("Player("))
                {
                    PlayerCollision collider = (PlayerCollision)Traverse.Create(controller).Field("playerCollision").GetValue();
                    MethodInfo methodInfo = typeof(PlayerCollision).GetMethod("SpawnClone", BindingFlags.NonPublic | BindingFlags.Instance);
                    object[] parameters = { PlayerHandler.Get().GetPlayer(collider.GetPlayerId()), controller, controller.GetPos() };
                    SlimeController controller2 = ((SlimeController)methodInfo.Invoke(collider, parameters));
                    controller2.name = "Player (Clone)";
                    FixTransform.InstantiateFixed<SlimeController>(controller2, controller.GetPos());
                    typeof(SlimeController).GetMethod("BeforeSpawnInit", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(controller2, null);
                    typeof(SlimeController).GetMethod("Spawn", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(controller2, null);

                    //((PlayerBody)Traverse.Create(controller2).Field("body").GetValue()).fixtrans.gameObject.SetActive(true); 
                }
            }
        }

        private void TeleportPlayers()
        {

            ParticleSystem prefab = new ParticleSystem();
            QuantumTunnel[] quantumTunnels = Resources.FindObjectsOfTypeAll(typeof(QuantumTunnel)) as QuantumTunnel[];
            foreach (QuantumTunnel obj in quantumTunnels)
            {
                if (obj.name == "QuantumTunnel")
                {
                    prefab = obj.onPlayerHitParticlePrefab;
                }
            }


            PlayerBody[] playerBodies = Resources.FindObjectsOfTypeAll(typeof(PlayerBody)) as PlayerBody[];
            List<FixTransform> playerTransforms = new List<FixTransform>();
            foreach (PlayerBody obj in playerBodies)
            {
                if (obj.name.Contains("(") && ((FixTransform)Traverse.Create(obj).Field("fixTransform").GetValue()).gameObject.active) playerTransforms.Add(obj.GetComponent<FixTransform>());
            }
            foreach (FixTransform player in playerTransforms)
            {


                Vec2 from = player.position;
                TeleportChangePos(player);
                Vec2 to = player.position;
                TeleportSpawnEffect(prefab, from, to);
            }
        }

        private void TeleportChangePos(FixTransform player)
        {
            PlayerPhysics component = player.GetComponent<PlayerPhysics>();
            PhysicsBodyList<RoundedRect> roundedRects = DetPhysics.Get().roundedRects;
            DetPhysics detPhysics = DetPhysics.Get();
            if (!(component != null) || !component.IsGrounded())
            {
                Vec2 vec = Vec2.zero;
                for (int i = 0; i < roundedRects.Length; i++)
                {
                    vec += roundedRects.colliders[i].Pos();
                }
                vec /= (Fix)((long)roundedRects.Length);
                Tools.Debug_DrawPos(vec, Color.red, 10f);
                Fix a = vec.y - (Fix)4L;
                Fix fix = SceneBounds.BlastZone_XMin;
                Fix fix2 = SceneBounds.BlastZone_XMax;
                int num = 0;
                for (int j = 0; j < detPhysics.roundedRects.Length; j++)
                {
                    if (DetPhysics.IsActive(detPhysics.roundedRects.physicsParents[j]))
                    {
                        Vec2 vec2 = detPhysics.roundedRects.colliders[j].Pos();
                        if (Fix.Abs(vec2.x - player.position.x) > SceneBounds.BlastZone_YMax)
                        {
                            Tools.Debug_DrawPos(vec2, Color.red, 10f);
                        }
                        else
                        {
                            num++;
                            AABB aabb = PhysTools.ComputeBoundingBox(detPhysics.roundedRects.colliders[j], j);
                            Tools.Debug_DrawAABB(aabb, Color.green, 10f);
                            fix = Fix.Max(fix, aabb.max.x);
                            fix2 = Fix.Min(fix2, aabb.min.x);
                        }
                    }
                }
                if (num == 0 || fix < fix2)
                {
                    fix = player.position.x + (Fix)30L;
                    fix2 = player.position.x - (Fix)30L;
                }
                Vec2 vec3 = new Vec2(fix2, Fix.Max(a, SceneBounds.WaterHeight + Fix.One));
                Vec2 vec4 = new Vec2(fix, SceneBounds.Camera_YMax - (Fix)1.7);
                Tools.Debug_DrawAABB(new AABB
                {
                    min = vec3,
                    max = vec4
                }, Color.red, 10f);
                Vec2 position = player.position;
                if (vec3.x > vec4.x)
                {
                    vec3.x = vec4.x;
                }
                if (vec3.y > vec4.y)
                {
                    vec3.y = vec4.y;
                }
                for (int k = 0; k < 10; k++)
                {
                    Fix x = Updater.RandomFix(vec3.x, vec4.x);
                    Fix y = Updater.RandomFix(vec3.y, vec4.y);
                    position = new Vec2(x, y);
                    if (!detPhysics.PointCheckAllRoundedRects(position))
                    {
                        break;
                    }
                }
                player.position = position;
                PlayerBody component2 = player.GetComponent<PlayerBody>();
                if (component2 != null)
                {
                    component2.selfImposedVelocity = Vec2.zero;
                }
                return;
            }
            Vec2 position2 = player.position;
            List<int> list = new List<int>(detPhysics.roundedRects.Length);
            Fix y2 = SceneBounds.WaterHeight + (Fix)0.5;
            Vec2 p = new Vec2(Fix.Zero, (Fix)1000L);
            for (int l = 0; l < detPhysics.roundedRects.Length; l++)
            {
                if (DetPhysics.IsActive(detPhysics.roundedRects.physicsParents[l]) && SceneBounds.IsInSceneBounds(detPhysics.roundedRects.colliders[l].Pos()) && detPhysics.roundedRects.colliders[l].GetClosestPoint(p).y > y2)
                {
                    Vec2 vec5 = detPhysics.roundedRects.colliders[l].Pos();
                    if (Fix.Abs(vec5.x - player.position.x) > SceneBounds.BlastZone_YMax)
                    {
                        Tools.Debug_DrawLine(vec5, new Vec2((Fix)0L, (Fix)1000L), Color.red, 10f);
                    }
                    else
                    {
                        list.Add(l);
                    }
                }
            }
            if (list.Count == 0)
            {
                return;
            }
            int index = Updater.RandomInt(0, list.Count);
            int num2 = list[index];
            PhysicsParent physicsParent = detPhysics.roundedRects.physicsParents[num2];
            StickyRoundedRectangle component3 = physicsParent.fixTrans.GetComponent<StickyRoundedRectangle>();
            if (component3 == null)
            {
                return;
            }
            for (int m = 0; m < 10; m++)
            {
                Fix localPlayerPos = Updater.RandomFix(Fix.Zero, Fix.One);
                Vec2 vec6 = component3.PositionFromLocalPlayerPos(localPlayerPos, Fix.Zero);
                Vec2 vec7 = vec6 + component3.currentNormal(vec6) * component.radius;
                bool flag = false;
                for (int n = 0; n < detPhysics.roundedRects.Length; n++)
                {
                    Tools.Debug_DrawPos(vec6, Color.blue, 2f);
                    Tools.Debug_DrawLine(vec6, vec7, Color.black, 2f);
                    if (detPhysics.roundedRects.physicsParents[n].hierarchyNumber != physicsParent.hierarchyNumber && detPhysics.roundedRects.colliders[n].OverlapPoint(vec7))
                    {
                        flag = true;
                        break;
                    }
                }
                if (vec6.y > y2 && SceneBounds.IsInSceneBounds(vec6) && !flag)
                {
                    component.AttachToGround(component3, vec6);
                    player.position = component.GetPlayerBody().position;
                    return;
                }
            }
            player.position = detPhysics.roundedRects.colliders[num2].GetClosestPoint(p);
        }

        private void TeleportSpawnEffect(ParticleSystem prefab, Vec2 from, Vec2 to)
        {
            float num3 = 5f;
            Vector3 vector = (Vector3)to - (Vector3)from;
            if (vector.sqrMagnitude > num3)
            {
                ParticleSystem particleSystem = UnityEngine.Object.Instantiate<ParticleSystem>(prefab, (Vector3)from, Quaternion.identity);
                ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleSystem.velocityOverLifetime;
                velocityOverLifetime.orbitalOffsetX = new ParticleSystem.MinMaxCurve
                {
                    constantMin = vector.x,
                    constantMax = vector.x,
                    mode = ParticleSystemCurveMode.TwoConstants
                };
                velocityOverLifetime.orbitalOffsetY = new ParticleSystem.MinMaxCurve
                {
                    constantMin = vector.y - 0.5f,
                    constantMax = vector.y + 0.5f,
                    mode = ParticleSystemCurveMode.TwoConstants
                };
                float num4 = 0.25f;
                float num5 = vector.magnitude / num4;
                if (num5 > 100f)
                {
                    velocityOverLifetime.radial = -num5;
                    return;
                }
                num5 = -100f;
                velocityOverLifetime.radial = num5;
                num4 = -Mathf.Abs(vector.magnitude) / num5;
                //particleSystem.main.startLifetime = num4;
                ParticleSystem.MainModule moduleThing = new ParticleSystem.MainModule(particleSystem);
                moduleThing.startLifetime = num4;
                Debug.Log("This part ran!");
                //particleSystem.MainModule.startLifetime = num4;
            }
        }

        private void SpawnMeteor()
        {
            Boulder boulder = FixTransform.InstantiateFixed<Boulder>(boulderPrefab, new Vec2(Updater.RandomFix((Fix)(-70), (Fix)70), (Fix)50));
            boulder.hitbox.Scale = (Fix)3;
            boulder.hitbox.ManualInit();
            boulder.hitbox.Scale = (Fix)3;
            boulder.hitbox.GetComponent<StickyRoundedRectangle>().platformType = PlatformType.space;
            boulder.hitbox.GetComponent<SpriteRenderer>().sprite = throwerPrefab.boulders.sprites[(int)PlatformType.space].sprite;
        }

        private void SpawnMeteorShower()
        {
            for (int i = 0; i < 10; i++)
            {
                Boulder boulder = FixTransform.InstantiateFixed<Boulder>(boulderPrefab, new Vec2(Updater.RandomFix((Fix)(-70), (Fix)70), Updater.RandomFix((Fix)40, (Fix)60)));
                boulder.hitbox.Scale = (Fix)0.5;
                boulder.hitbox.ManualInit();
                boulder.hitbox.Scale = (Fix)0.5;
                boulder.hitbox.GetComponent<StickyRoundedRectangle>().platformType = PlatformType.space;
                boulder.hitbox.GetComponent<SpriteRenderer>().sprite = throwerPrefab.boulders.sprites[(int)PlatformType.space].sprite;
            }
        }

        private void SpawnBlackHole()
        {
            BlackHole blackHole = FixTransform.InstantiateFixed<BlackHole>(blackHolePrefab, new Vec2(Updater.RandomFix((Fix)(-90), (Fix)90), Updater.RandomFix((Fix)0, (Fix)45)));
        }

        private void SpawnArrows()
        {
            SpawnObjects(arrowPrefab, Updater.RandomInt(10, 30), (Fix)100, (Fix)(-100), (Fix)60, (Fix)40);
        }

        private void SpawnGrenades()
        {
            SpawnObjects(grenadePrefab, Updater.RandomInt(10, 30), (Fix)100, (Fix)(-100), (Fix)60, (Fix)40);
        }

        private void SpawnMines()
        {
            SpawnObjects(minePrefab, Updater.RandomInt(5, 15), (Fix)90, (Fix)(-90), (Fix)45, (Fix)0, true);
        }

        private void SpawnSmoke()
        {
            SpawnObjects(smokePrefab, Updater.RandomInt(10, 20), (Fix)90, (Fix)(-90), (Fix)45, (Fix)0);
        }

        private void SpawnObjects(BoplBody prefab, int amount, Fix xMax, Fix xMin, Fix yMax, Fix yMin, bool isMine = false)
        {
            for (int i = 0; i < amount; i++)
            {
                BoplBody boplBody = FixTransform.InstantiateFixed<BoplBody>(prefab, new Vec2(Updater.RandomFix(xMin, xMax), Updater.RandomFix(yMin, yMax)));
                boplBody.Scale = Fix.One;
                boplBody.StartVelocity = new Vec2(Fix.Zero, Fix.Zero);
                boplBody.rotation = Fix.Zero;
                boplBody.StartAngularVelocity = Fix.Zero;
                if (isMine)
                {
                    Mine mine = boplBody.GetComponent<Mine>();
                    mine.ScansFor = LayerMask.NameToLayer("Player");
                    mine.Light.color = Color.red;
                }
            }
        }

        private void ChangeGravity(bool reset = false)
        {
            gravityScale = Updater.RandomFix((Fix)(0), (Fix)1.5);
            if (reset) gravityScale = Fix.One;
            //foreach (BoplBody obj in (Resources.FindObjectsOfTypeAll(typeof(BoplBody)) as BoplBody[])) if (!obj.name.Contains("(")) obj.gravityScale = newGravity;
            //foreach (PlayerPhysics obj in (Resources.FindObjectsOfTypeAll(typeof(PlayerPhysics)) as PlayerPhysics[])) obj.gravity_modifier = newGravity;
        }



        public class Patches
        {

            [HarmonyPatch(typeof(PlayerPhysics), "UpdateSim")]
            [HarmonyPrefix]
            public static void PatchUpdateSim(ref PlayerPhysics __instance)
            {
                __instance.gravity_modifier = Plugin.gravityScale;
            }


            [HarmonyPatch(typeof(BoplBody), "UpdateSim")]
            [HarmonyPrefix]
            public static void PatchBoplBodyUpdate(ref BoplBody __instance)
            {
                __instance.gravityScale = Plugin.gravityScale;
            }
            /*
	        [HarmonyPatch(typeof(Drill), "UpdateSim")]
	        [HarmonyPrefix]
	        public static void DrillUpdatePatch(ref Drill __instance)
	        {
	        	__instance.strongGravity = Plugin.gravityScale;
	        	__instance.gravityStr = Plugin.gravityScale;
	        }
        
	        [HarmonyPatch(typeof(Boulder), "UpdateSim")]
	        [HarmonyPrefix]
	        public static void BoulderUpdatePatch(ref Boulder __instance)
	        {
	        	__instance.hitbox.SetGravityScale(Plugin.gravityScale);
	        }
            */
            [HarmonyPatch(typeof(DetPhysics), "UpdateRopeMesh_parallell")]
            [HarmonyPrefix]
            public static void RopePatch(ref DetPhysics __instance)
            {
                __instance.playerGravity = Plugin.gravityScale;
                __instance.ropeGravity = Plugin.gravityScale;
            }
        }

        public class EventText
        {
            public EventText(int eventType)
            {
                this.eventType = eventType;
                timeAlive = 0;
                gameObject = new GameObject("EventAnnouncement", new Type[]
                    {
                        typeof(RectTransform),
                        typeof(TextMeshProUGUI)
                    });
                GameObject canvas = GameObject.Find("AbilitySelectCanvas");
                gameObject.transform.SetParent(canvas.transform);

                TextMeshProUGUI textObject = gameObject.GetComponent<TextMeshProUGUI>();
                textObject.font = LocalizedText.localizationTable.GetFont(Settings.Get().Language, false);
                textObject.color = UnityEngine.Color.red;
                textObject.fontSize = 100f;
                textObject.alignment = TextAlignmentOptions.Center;
                textObject.richText = false;
                if (eventType == 0) textObject.text = "Grenade Rain!";
                else if (eventType == 1) textObject.text = "Arrow Rain!";
                else if (eventType == 2) textObject.text = "Land Mines!";
                else if (eventType == 3) textObject.text = "Smoke Storm!";
                else if (eventType == 4) textObject.text = "Gravity Change!";
                else if (eventType == 5) textObject.text = "Teleporting Players!";
                else if (eventType == 6) textObject.text = "Black Hole Attack!";
                else if (eventType == 7) textObject.text = "Meteor Strike!";
                else if (eventType == 8) textObject.text = "Meteor Shower!";
                else if (eventType == 9) textObject.text = "Cloning Players!";
                else textObject.text = "Untitled event!";

                RectTransform textBox = gameObject.GetComponent<RectTransform>();

                float height = canvas.GetComponent<RectTransform>().rect.height;
                float width = canvas.GetComponent<RectTransform>().rect.width;

                textBox.sizeDelta = new Vector2(5000, 100);
                textBox.anchoredPosition = new Vector2(0, (height / 2) - 100);


                gameObject.SetActive(true);
            }


            public void Update()
            {
                timeAlive++;
                if (timeAlive >= 200)
                {
                    UnityEngine.Object.Destroy(gameObject);
                }
            }


            private int eventType;
            public int timeAlive;

            private GameObject gameObject;
        }

        private int hierarchyNumber;

        public bool IsDestroyed { get; set; }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!BattleUtils.IsGameSessionActive())
            {
                Updater.RegisterUpdatable(this);
                timeSinceEvent = 0;
            }
        }

        public int HierarchyNumber
        {
            get
            {
                return hierarchyNumber;
            }
            set
            {
                hierarchyNumber = value;
            }
        }

        public void Init()
        {
        }

        public bool IsEnabled()
        {
            return ((Behaviour)this).isActiveAndEnabled;
        }

        public void LateUpdateSim(Fix SimDeltaTime)
        {
        }

        public void OnDestroyUpdatable()
        {
        }
    }
}

//              dotnet build "C:\Users\ajarc\BoplMods\RandomEvents\RandomEvents.csproj"