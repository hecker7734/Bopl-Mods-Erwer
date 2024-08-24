using System;
using System.Collections.Generic;
using System.Text;
using BoplFixedMath;
using Unity.Mathematics;
using UnityEngine;

namespace IUpdateable
{

    // Token: 0x020000DF RID: 223
    public class Updater_i : MonoBehaviour
    {
        // Token: 0x060006C4 RID: 1732 RVA: 0x000443FC File Offset: 0x000425FC
        public static void PreLevelLoad()
        {
            List<Player> list = PlayerHandler.Get().PlayerList();
            for (int i = 0; i < list.Count; i++)
            {
                int id = list[i].Id;
                list[i].isProtectedFromTimeStop = false;
                list[i].isInvisible = false;
                list[i].timeSpentInvisible = Fix.Zero;
                list[i].timeSinceEnteredAbility = Fix.Zero;
            }
            GameTime.PlayerTimeScale = (Fix)1L;
            Updater_i.toDestroyBuffer.Clear();
            Updater_i.updatables.Clear();
            Updater_i.toBeAddedUpdatables.Clear();
            Updater_i.toBeAddedPrefab.Clear();
            Updater_i.SimTimeSinceLevelLoaded = Fix.Zero;
        }

        // Token: 0x060006C5 RID: 1733 RVA: 0x000444AC File Offset: 0x000426AC
        public static void PostLevelLoad()
        {
            Updater_i.LoadedALevelThisUpdate = true;
        }

        // Token: 0x060006C6 RID: 1734 RVA: 0x000444B4 File Offset: 0x000426B4
        public static void DestroyFix(GameObject go)
        {
            go.GetComponentsInChildren<IUpdatable>(true, Updater_i.toDestroyBuffer);
            FixTransform component = go.GetComponent<FixTransform>();
            for (int i = 0; i < Updater_i.toDestroyBuffer.Count; i++)
            {
                Updater_i.toDestroyBuffer[i].IsDestroyed = true;
            }
            if (component != null)
            {
                component.IsDestroyed = true;
            }
            UnityEngine.Object.Destroy(go);
        }

        // Token: 0x060006C7 RID: 1735 RVA: 0x00044510 File Offset: 0x00042710
        public static void DestroyFix(MonoBehaviour mb)
        {
            if (mb is IUpdatable)
            {
                ((IUpdatable)mb).IsDestroyed = true;
            }
            if (mb is FixTransform)
            {
                ((FixTransform)mb).IsDestroyed = true;
            }
            UnityEngine.Object.Destroy(mb);
        }

        // Token: 0x060006C8 RID: 1736 RVA: 0x00044540 File Offset: 0x00042740
        public static void ReInit()
        {
            Updater_i.SimTimePassed = Fix.Zero;
            Updater_i.SimTimeSinceLevelLoaded = Fix.Zero;
            Updater_i.SimulationTicks = 0;
            Updater_i.updatables.Clear();
            Updater_i.toBeAddedPrefab.Clear();
            Updater_i.toBeAddedUpdatables.Clear();
            Updater_i.addedUpdatables = 0;
            Updater_i.prefabsAdded = 0;
            Updater_i.registeringAPrefab = false;
        }

        // Token: 0x17000088 RID: 136
        // (get) Token: 0x060006C9 RID: 1737 RVA: 0x00044597 File Offset: 0x00042797
        public static int AddedUpdatables
        {
            get
            {
                return Updater_i.addedUpdatables;
            }
        }

        // Token: 0x060006CA RID: 1738 RVA: 0x0004459E File Offset: 0x0004279E
        public static void BeginRegisterPrefab()
        {
            Updater_i.registeringAPrefab = true;
        }

        // Token: 0x060006CB RID: 1739 RVA: 0x000445A8 File Offset: 0x000427A8
        public static void EndRegisterPrefab()
        {
            Updater_i.registeringAPrefab = false;
            for (int i = 0; i < Updater_i.toBeAddedPrefab.Count; i++)
            {
                Updater_i.toBeAddedPrefab[i].HierarchyNumber += Updater_i.prefabsAdded + 100000000;
                Updater_i.toBeAddedUpdatables.Add(Updater_i.toBeAddedPrefab[i]);
            }
            Updater_i.prefabsAdded += Updater_i.toBeAddedPrefab.Count;
            Updater_i.toBeAddedPrefab.Clear();
        }

        // Token: 0x060006CC RID: 1740 RVA: 0x00044626 File Offset: 0x00042826
        public static void RegisterUpdatable(IUpdatable updatable)
        {
            if (Updater_i.registeringAPrefab)
            {
                Updater_i.addedUpdatables++;
                Updater_i.toBeAddedPrefab.Add(updatable);
                return;
            }
            Updater_i.addedUpdatables++;
            Updater_i.toBeAddedUpdatables.Add(updatable);
        }

        // Token: 0x060006CD RID: 1741 RVA: 0x0004465E File Offset: 0x0004285E
        public static void HitStop(int frames)
        {
            Updater_i.hitStopFrames = frames;
        }

        // Token: 0x060006CE RID: 1742 RVA: 0x00044666 File Offset: 0x00042866
        public static bool InHitstop()
        {
            return Updater_i.hitStopFrames > 0;
        }

        // Token: 0x060006CF RID: 1743 RVA: 0x00044670 File Offset: 0x00042870
        public static void InitSeed(uint seed)
        {
            Updater_i.random.InitState(seed);
        }

        // Token: 0x060006D0 RID: 1744 RVA: 0x0004467D File Offset: 0x0004287D
        public static int RandomInt(int from, int to)
        {
            return Updater_i.random.NextInt(from, to);
        }

        // Token: 0x060006D1 RID: 1745 RVA: 0x0004468B File Offset: 0x0004288B
        public static Vec2 RandomInsideUnitCircle()
        {
            return new Vec2(Updater_i.RandomFix(Fix.Zero, Fix.PiTimes2)) * Updater_i.RandomFix(Fix.Zero, Fix.One);
        }

        // Token: 0x060006D2 RID: 1746 RVA: 0x000446B5 File Offset: 0x000428B5
        public static Vec2 RandomUnitVector()
        {
            return new Vec2(Updater_i.RandomFix(Fix.Zero, Fix.PiTimes2));
        }

        // Token: 0x060006D3 RID: 1747 RVA: 0x000446CC File Offset: 0x000428CC
        public static Fix RandomFix(Fix from, Fix to)
        {
            unchecked
            {

            if (Fix.Floor(from) == Fix.Floor(to))
            {
                uint min = (uint)(from.m_rawValue & (long)((ulong)-1));
                uint max = (uint)(to.m_rawValue & (long)((ulong)-1));
                uint num = Updater_i.random.NextUInt(min, max);
                return Fix.FromRaw(Fix.Floor(from).m_rawValue + (long)((ulong)num));
            }
            int min2 = (int)((long)Fix.Floor(from));
            int max2 = (int)((long)Fix.Floor(to));
            long num2 = (long)Updater_i.random.NextInt(min2, max2);
            uint min3 = (uint)(from.m_rawValue & (long)((ulong)-1));
            uint maxValue = uint.MaxValue;
            uint num3 = Updater_i.random.NextUInt(min3, maxValue);
            return Fix.FromRaw((num2 << 32) + (long)((ulong)num3));
            }
        }

        // Token: 0x060006D4 RID: 1748 RVA: 0x00044778 File Offset: 0x00042978
        public static void TickSimulation(Fix deltaTime)
        {
            List<Player> list = PlayerHandler.Get().PlayerList();
            if (Updater_i.LoadedALevelThisUpdate)
            {
                Updater_i.gameHasStopped = false;
            }
            if (!TutorialGameHandler.isInTutorial && !GameLobby.isOnlineGame && !Updater_i.gameHasStopped && Host.recordReplay && !GameLobby.isPlayingAReplay)
            {
                if (Updater_i.LoadedALevelThisUpdate || Updater_i.SimulationTicks == 0)
                {
                    uint tickCount = (uint)Environment.TickCount;
                    Updater_i.InitSeed(tickCount);
                    StartRequestPacket startRequestPacket = new StartRequestPacket
                    {
                        seed = tickCount,
                        seqNum = 1,
                        frameBufferSize = 0,
                        isDemoMask = byte.MaxValue,
                        nrOfAbilites = (byte)Settings.Get().NumberOfAbilities,
                        currentLevel = GameSession.CurrentLevel(),
                        nrOfPlayers = (byte)list.Count
                    };
                    NamedSpriteList abilityIcons = SteamManager.instance.abilityIcons;
                    for (int i = 0; i < list.Count; i++)
                    {
                        byte b = (byte)abilityIcons.IndexOf(list[i].Abilities[0].name);
                        switch (list[i].Id)
                        {
                            case 1:
                                startRequestPacket.p1_color = 1;
                                startRequestPacket.p1_id = 1UL;
                                startRequestPacket.p1_team = (byte)list[i].Team;
                                startRequestPacket.p1_ability1 = b;
                                break;
                            case 2:
                                startRequestPacket.p2_color = 2;
                                startRequestPacket.p2_id = 2UL;
                                startRequestPacket.p2_team = (byte)list[i].Team;
                                startRequestPacket.p2_ability1 = b;
                                break;
                            case 3:
                                startRequestPacket.p3_color = 3;
                                startRequestPacket.p3_id = 3UL;
                                startRequestPacket.p3_team = (byte)list[i].Team;
                                startRequestPacket.p3_ability1 = b;
                                break;
                            case 4:
                                startRequestPacket.p4_color = 4;
                                startRequestPacket.p4_id = 4UL;
                                startRequestPacket.p4_team = (byte)list[i].Team;
                                startRequestPacket.p4_ability1 = b;
                                break;
                        }
                        if (startRequestPacket.nrOfAbilites > 1)
                        {
                            b = (byte)abilityIcons.IndexOf(list[i].Abilities[1].name);
                            switch (list[i].Id)
                            {
                                case 1:
                                    startRequestPacket.p1_ability2 = b;
                                    break;
                                case 2:
                                    startRequestPacket.p2_ability2 = b;
                                    break;
                                case 3:
                                    startRequestPacket.p3_ability2 = b;
                                    break;
                                case 4:
                                    startRequestPacket.p4_ability2 = b;
                                    break;
                            }
                        }
                        if (startRequestPacket.nrOfAbilites > 2)
                        {
                            b = (byte)abilityIcons.IndexOf(list[i].Abilities[2].name);
                            switch (list[i].Id)
                            {
                                case 1:
                                    startRequestPacket.p1_ability3 = b;
                                    break;
                                case 2:
                                    startRequestPacket.p2_ability3 = b;
                                    break;
                                case 3:
                                    startRequestPacket.p3_ability3 = b;
                                    break;
                                case 4:
                                    startRequestPacket.p4_ability3 = b;
                                    break;
                            }
                        }
                    }
                    SteamManager.instance.EncodeCurrentStartParameters_forReplay(ref SteamManager.instance.networkClient.EncodedStartRequest, startRequestPacket, true);
                }
                InputPacketQuad quadToSave = default(InputPacketQuad);
                for (int j = 0; j < list.Count; j++)
                {
                    InputPacket inputPacket = Host.CurrentInputAsPacket(list[j].Id, (uint)(Updater_i.SimulationTicks + 1), null);
                    switch (list[j].Id)
                    {
                        case 1:
                            quadToSave.p1 = inputPacket;
                            break;
                        case 2:
                            quadToSave.p2 = inputPacket;
                            break;
                        case 3:
                            quadToSave.p3 = inputPacket;
                            break;
                        case 4:
                            quadToSave.p4 = inputPacket;
                            break;
                    }
                }
                SteamManager instance = SteamManager.instance;
                if (instance != null)
                {
                    Host networkClient = instance.networkClient;
                    if (networkClient != null)
                    {
                        networkClient.RecordReplayFrame(quadToSave);
                    }
                }
            }
            for (int k = 0; k < list.Count; k++)
            {
                list[k].UpdateFRAMEDependentInputs();
            }
            for (int l = Updater_i.toBeAddedUpdatables.Count - 1; l >= 0; l--)
            {
                if (Updater_i.toBeAddedUpdatables[l] == null || Updater_i.toBeAddedUpdatables[l].IsDestroyed)
                {
                    Updater_i.toBeAddedUpdatables.RemoveAt(l);
                }
            }
            for (int m = 0; m < Updater_i.toBeAddedUpdatables.Count; m++)
            {
                IUpdatable value = Updater_i.toBeAddedUpdatables[m];
                Updater_i.toBeAddedUpdatables[m] = value;
            }
            Updater_i.toBeAddedUpdatables.Sort(Updater_i.updateComparer);
            for (int n = 0; n < Updater_i.toBeAddedUpdatables.Count; n++)
            {
                if (Updater_i.toBeAddedUpdatables[n] != null && !Updater_i.toBeAddedUpdatables[n].IsDestroyed)
                {
                    Updater_i.toBeAddedUpdatables[n].Init();
                }
            }
            for (int num = 0; num < Updater_i.toBeAddedUpdatables.Count; num++)
            {
                Updater_i.updatables.Add(Updater_i.toBeAddedUpdatables[num]);
            }
            Updater_i.toBeAddedUpdatables.Clear();
            for (int num2 = Updater_i.updatables.Count - 1; num2 >= 0; num2--)
            {
                if (Updater_i.updatables[num2].IsDestroyed)
                {
                    Updater_i.updatables[num2].OnDestroyUpdatable();
                    Updater_i.updatables.RemoveAt(num2);
                }
            }
            for (int num3 = 0; num3 < Updater_i.updatables.Count; num3++)
            {
                if (Updater_i.updatables[num3] != null && Updater_i.updatables[num3].IsEnabled())
                {
                    Updater_i.updatables[num3].UpdateSim(deltaTime);
                }
            }
            if (Updater_i.LoadedALevelThisUpdate)
            {
                Updater_i.SimTimePassed += deltaTime;
                Updater_i.SimTimeSinceLevelLoaded += deltaTime;
                Updater_i.SimulationTicks++;
                Updater_i.LoadedALevelThisUpdate = false;
                return;
            }
            DetPhysics.Get().Simulate(deltaTime);
            for (int num4 = 0; num4 < Updater_i.updatables.Count; num4++)
            {
                if (Updater_i.updatables[num4] != null && Updater_i.updatables[num4].IsEnabled())
                {
                    Updater_i.updatables[num4].LateUpdateSim(deltaTime);
                }
            }
            DetPhysics.Get().VeryLateRopeUpdate();
            DetPhysics.Get().UpdateRopeMesh_parallell();
            Updater_i.SimTimePassed += deltaTime;
            Updater_i.SimTimeSinceLevelLoaded += deltaTime;
            Updater_i.SimulationTicks++;
        }

        // Token: 0x060006D5 RID: 1749 RVA: 0x00044DE0 File Offset: 0x00042FE0
        public static ulong ComputeChecksum()
        {
            FixTransform[] array = UnityEngine.Object.FindObjectsOfType<FixTransform>();
            Fix fix = Fix.Zero;
            for (int i = 0; i < array.Length; i++)
            {
                if (!array[i].IsDestroyed)
                {
                    fix = Fix.SlowAdd(fix, array[i].ComputeCheckSum());
                }
            }
            return (ulong)fix.m_rawValue;
        }

        // Token: 0x060006D6 RID: 1750 RVA: 0x00044E28 File Offset: 0x00043028
        public static string SerializeState()
        {
            FixTransform[] array = UnityEngine.Object.FindObjectsOfType<FixTransform>();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("----------------");
            for (int i = 0; i < array.Length; i++)
            {
                if (!array[i].IsDestroyed)
                {
                    IUpdatable[] components = array[i].GetComponents<IUpdatable>();
                    if (components != null)
                    {
                        stringBuilder.Append(array[i].gameObject.name + " hierarchyNumbers = ");
                        for (int j = 0; j < components.Length; j++)
                        {
                            if (components[j].IsDestroyed)
                            {
                                stringBuilder.Append("[DESTROYED]");
                            }
                            stringBuilder.Append(components[j].HierarchyNumber);
                            stringBuilder.Append(", ");
                        }
                    }
                    else
                    {
                        stringBuilder.Append(array[i].gameObject.name);
                    }
                    stringBuilder.Append("\n");
                    stringBuilder.Append("posX = ");
                    stringBuilder.Append(array[i].position.x);
                    stringBuilder.Append(", posY = ");
                    stringBuilder.Append(array[i].position.y);
                    stringBuilder.Append(", rotation = ");
                    stringBuilder.Append(array[i].rotation);
                    stringBuilder.AppendLine("----------------");
                }
            }
            return stringBuilder.ToString();
        }

        // Token: 0x060006D7 RID: 1751 RVA: 0x00044F7C File Offset: 0x0004317C
        public void Update()
        {
            if (this.autoUpdate && !GameLobby.isOnlineGame && !GameLobby.isPlayingAReplay)
            {
                this.autoUpdateAccum += Time.deltaTime;
                while (this.autoUpdateAccum > 0f)
                {
                    this.autoUpdateAccum -= (float)GameTime.FixedTimeStep;
                    if (Updater_i.hitStopFrames > 0)
                    {
                        Updater_i.hitStopFrames--;
                        Updater_i.SimTimePassed += GameTime.FixedTimeStep;
                        Updater_i.SimTimeSinceLevelLoaded += GameTime.FixedTimeStep;
                        Updater_i.SimulationTicks++;
                    }
                    else
                    {
                        Updater_i.TickSimulation(GameTime.FixedTimeStep);
                    }
                }
            }
        }

        // Token: 0x04000913 RID: 2323
        public static bool LoadedALevelThisUpdate = false;

        // Token: 0x04000914 RID: 2324
        public static List<IUpdatable> toDestroyBuffer = new List<IUpdatable>();

        // Token: 0x04000915 RID: 2325
        public static List<IUpdatable> updatables = new List<IUpdatable>();

        // Token: 0x04000916 RID: 2326
        public static List<IUpdatable> toBeAddedUpdatables = new List<IUpdatable>();

        // Token: 0x04000917 RID: 2327
        public static int addedUpdatables = 0;

        // Token: 0x04000918 RID: 2328
        public static Updater_i.UpdatableComparer updateComparer = new Updater_i.UpdatableComparer();

        // Token: 0x04000919 RID: 2329
        public static List<IUpdatable> toBeAddedPrefab = new List<IUpdatable>();

        // Token: 0x0400091A RID: 2330
        public static int prefabsAdded = 0;

        // Token: 0x0400091B RID: 2331
        public static bool registeringAPrefab = false;

        // Token: 0x0400091C RID: 2332
        public static Unity.Mathematics.Random random = new Unity.Mathematics.Random(37U);

        // Token: 0x0400091D RID: 2333
        public static int hitStopFrames;

        // Token: 0x0400091E RID: 2334
        public static Fix standardFixedDeltaTime = (Fix)0.016666668f;

        // Token: 0x0400091F RID: 2335
        public static Fix SimTimePassed = Fix.Zero;

        // Token: 0x04000920 RID: 2336
        public static Fix SimTimeSinceLevelLoaded = Fix.Zero;

        // Token: 0x04000921 RID: 2337
        public static int SimulationTicks = 0;

        // Token: 0x04000922 RID: 2338
        public static bool gameHasStopped = false;

        // Token: 0x04000923 RID: 2339
        public bool autoUpdate;

        // Token: 0x04000924 RID: 2340
        public float autoUpdateAccum;

        // Token: 0x020000E0 RID: 224
        public class UpdatableComparer : IComparer<IUpdatable>
        {
            // Token: 0x060006DA RID: 1754 RVA: 0x000450CA File Offset: 0x000432CA
            public int Compare(IUpdatable x, IUpdatable y)
            {
                if (x.GetHashCode() != y.GetHashCode())
                {
                    int hierarchyNumber = x.HierarchyNumber;
                    int hierarchyNumber2 = y.HierarchyNumber;
                }
                return x.HierarchyNumber - y.HierarchyNumber;
            }
        }
    }

}


