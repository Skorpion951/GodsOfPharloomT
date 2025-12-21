using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gods_Of_Pharloom
{
    public class BossInfo
    {
        public static Dictionary<string, BossInfo> bosses;
        public static float waitForBossDeathAnim = 1.5f;
        public string sceneName;
        public string entryGate;
        public string bossName;
        public Dictionary<string, float>[] bossesHpMul;
        public BossInfo ascendedVersion;
        public bool noInputOnStart;
        public bool is3ActBoss;

        public BossInfo(string sceneName, string entryGate, string bossName, float[][] bossesHpMul, BossInfo ascendedVersion = null, bool noInputOnStart = true, bool is3ActBoss = false)
        {
            this.sceneName = sceneName;
            this.entryGate = entryGate;
            this.bossName = bossName;
            this.ascendedVersion = ascendedVersion;
            this.noInputOnStart = noInputOnStart;
            this.is3ActBoss = is3ActBoss;

            var dictionary = new Dictionary<string, float>[bossesHpMul.Length];
            for(int i = 0; i < bossesHpMul.Length; i++)
            {
                dictionary[i] = new Dictionary<string, float>{{"OrigHp", bossesHpMul[i][0]},
                                                              {"Attuned", bossesHpMul[i][1]},
                                                              {"Ascended", bossesHpMul[i][2]},
                                                              {"Radiant", bossesHpMul[i][2]}
                                                            };
            };
            this.bossesHpMul = dictionary;
        }
        public static void InitBossesInfo()
        {
            var bossesInfo = new BossInfo[]
            {
                new BossInfo("Tut_03", "start_battle_entry", "Moss Mother", new float[][]{new float[]{120f, 2f, 1.5f}}, 
                                        ascendedVersion : new BossInfo("Weave_03", "start_battle_entry", "Moss Mother", new float[][]{new float[]{350f, 2f, 1.5f}})),
                new BossInfo("Bone_05", "start_battle_entry", "Bell Beast", new float[][]{new float[]{150f, 2f, 1.5f}}),
                new BossInfo("Bone_East_08", "start_battle_entry", "Fourth Chorus", new float[][]{new float[]{500f, 2f, 1.5f}}),
                new BossInfo("Coral_11", "start_battle_entry", "Great Conchflies", new float[][]{new float[]{400f, 2f, 1.5f}}),
                new BossInfo("Bone_East_12", "start_battle_entry", "Lace in Deep Docks", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Coral_Judge_Arena", "start_battle_entry", "The Last Judge", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Greymoor_08", "start_battle_entry", "Moorwing", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Organ_01", "start_battle_entry", "Phantom", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Ant_19", "start_battle_entry", "Savage Beastfly in Chapel of The Beast", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Shellwood_18", "start_battle_entry", "Sister Splinter", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Bone_15", "start_battle_entry", "Skull Tyrant", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Belltown_Shrine", "start_battle_entry", "Widow", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Slab_16b", "start_battle_entry", "Broodmother", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Cog_Dancers", "start_battle_entry", "Cogwork Dancers", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Dust_Chef", "start_battle_entry", "Disgraced Chef Lugoli", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Belltown_08", "start_battle_entry", "Father of the Flame", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Slab_10b", "start_battle_entry", "First Sinner", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Dock_09", "start_battle_entry", "Forebrothers Signis & Gron", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Library_09", "start_battle_entry", "Garmond & Zaza", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Cradle_03", "start_battle_entry", "Grand Mother Silk", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Shadow_18", "start_battle_entry", "Groal the Great", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Song_Tower_01", "start_battle_entry", "Lace in the Cradle", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Coral_27", "start_battle_entry", "Raging Conchfly", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Bone_East_08", "start_battle_entry", "Savage Beastfly in Far Fields", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Hang_17b", "start_battle_entry", "Second Sentiel", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Greymoor_08", "start_battle_entry", "Shakra", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Ward_02", "start_battle_entry", "The Unravelled", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Library_13", "start_battle_entry", "Trobbio", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Coral_29", "start_battle_entry", "Voltvyrm", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Bellway_Centipede_Arena", "start_battle_entry", "Bell Eater", new float[][]{new float[]{250f, 2f, 1.5f}}, is3ActBoss: true),
                new BossInfo("Clover_10", "start_battle_entry", "Clover Dancers", new float[][]{new float[]{250f, 2f, 1.5f}}, is3ActBoss: true),
                new BossInfo("Room_CrowCourt_02", "start_battle_entry", "Crawfather", new float[][]{new float[]{250f, 2f, 1.5f}}, is3ActBoss: true),
                new BossInfo("Memory_Coral_Tower", "start_battle_entry", "Crust King Khann", new float[][]{new float[]{250f, 2f, 1.5f}}, is3ActBoss: true),
                new BossInfo("Bone_East_18b", "start_battle_entry", "Gurr the Outcast", new float[][]{new float[]{250f, 2f, 1.5f}}, is3ActBoss: true),
                new BossInfo("Coral_33", "start_battle_entry", "Lost Garmond", new float[][]{new float[]{250f, 2f, 1.5f}}, is3ActBoss: true),
                new BossInfo("Abyss_Cocoon", "start_battle_entry", "Lost Lace", new float[][]{new float[]{1800f, 1.5f, 2f}}, is3ActBoss: true),
                new BossInfo("Shellwood_11b_Memory", "start_battle_entry", "Nyleth", new float[][]{new float[]{250f, 2f, 1.5f}}, is3ActBoss: true),
                new BossInfo("Clover_19", "start_battle_entry", "Palestag", new float[][]{new float[]{250f, 2f, 1.5f}}, is3ActBoss: true),
                new BossInfo("Peak_07", "start_battle_entry", "Pinstress", new float[][]{new float[]{250f, 2f, 1.5f}}, is3ActBoss: true),
                new BossInfo("Crawl_10", "start_battle_entry", "Plasmified Zango", new float[][]{new float[]{250f, 2f, 1.5f}}, is3ActBoss: true),
                new BossInfo("Shellwood_22", "start_battle_entry", "Shrine Guardian Seth", new float[][]{new float[]{250f, 2f, 1.5f}}, is3ActBoss: true),
                new BossInfo("Memory_Ant_Queen", "start_battle_entry", "Skarrsinger Karmelita", new float[][]{new float[]{250f, 2f, 1.5f}}, is3ActBoss: true),
                new BossInfo("Library_13", "start_battle_entry", "Tormented Trobbio", new float[][]{new float[]{250f, 2f, 1.5f}}, is3ActBoss: true),
                new BossInfo("Coral_39", "start_battle_entry", "Watcher at the Edge", new float[][]{new float[]{250f, 2f, 1.5f}}, is3ActBoss: true),
            };

            var dict = new Dictionary<string, BossInfo>();
            foreach(var bossInfo in bossesInfo)
            {
                dict[bossInfo.bossName] = bossInfo;
            }
            bosses = dict;
        }
    }
    public static class BossSequence
    {
        public static GameObject sequenceGO;
        public static bool isInSequence = false;
        public static BossInfo[] bossSequence;
        public static BossInfo currentBoss;
        public static int currentBossIndex = 0;
        public static string backEntry;
        public static string backScene;
        public static PlayMakerFSM sequenceController;
        public static string difficultMode;
        public static bool isPantheon;
        public static bool isHoG;

        public static void Reset()
        {
            isInSequence = false;
            bossSequence = null;
            currentBoss = null;
            currentBossIndex = 0;
            difficultMode = "";
            isPantheon = false;
            isHoG = false;

            sequenceController.SetState("Dormant");
        }
        public static void Start()
        {
            isInSequence = true;
            sequenceController.SendEvent("START SEQUENCE");
        }

        public static void CreateSequenceController()
        {
            var go = new GameObject("BossSequence");
            GameObject.DontDestroyOnLoad(go);

            var fsmComponent = go.gameObject.AddComponent<PlayMakerFSM>();
            fsmComponent.enabled = false;

            var fsm = fsmComponent.Fsm;

            sequenceController = fsmComponent;

            var dormant = new FsmState(fsm);
            dormant.Name = "Dormant";

            var startSequence = new FsmState(fsm);
            startSequence.Name = "Start Sequence";

            var idle = new FsmState(fsm);
            idle.Name = "Idle";

            var next = new FsmState(fsm);
            next.Name = "Next";

            var endSequence = new FsmState(fsm);
            endSequence.Name = "End Sequence";

            fsm.StartState = "Dormant";

            var startSequenceAction = new PatchedFsm.CustomLogicFsm(fsm);
            startSequenceAction.action = (Fsm fsm) =>
            {
                StartSequence();

                startSequenceAction.Finish();
            };

            var nextAction = new PatchedFsm.CustomLogicFsm(fsm);
            nextAction.action += (Fsm fsm) =>
            {
                currentBossIndex++;
                if(!(currentBossIndex < bossSequence.Length))
                {
                    sequenceController.SendEvent("END SEQUENCE");
                    return;
                }

                NextBoss();
                sequenceController.SendEvent("NEXT");
            };

            var endSequenceAction = new PatchedFsm.CustomLogicFsm(fsm);
            endSequenceAction.action += (Fsm fsm) =>
            {
                EndSequence();
                endSequenceAction.Finish();
            };

            dormant.Transitions = new FsmTransition[]
            {
                new FsmTransition
                {
                    FsmEvent = FsmEvent.GetFsmEvent("START SEQUENCE"),
                    ToFsmState = startSequence
                }
            };

            startSequence.Transitions = new FsmTransition[]
            {
                new FsmTransition
                {
                    FsmEvent = FsmEvent.GetFsmEvent("FINISHED"),
                    ToFsmState = idle
                }
            };

            idle.Transitions = new FsmTransition[]
            {
                new FsmTransition
                {
                    FsmEvent = FsmEvent.GetFsmEvent(PatchedFsm.bossDeadEvent), //boss dead event
                    ToFsmState = next
                },
                new FsmTransition
                {
                    FsmEvent = FsmEvent.GetFsmEvent("HORNET DEFEATED"),
                    ToFsmState = endSequence
                }
            };

            next.Transitions = new FsmTransition[]
            {
                new FsmTransition
                {
                    FsmEvent = FsmEvent.GetFsmEvent("NEXT"),
                    ToFsmState = idle
                },
                new FsmTransition
                {
                    FsmEvent = FsmEvent.GetFsmEvent("END SEQUENCE"),
                    ToFsmState = endSequence
                }
            };

            startSequence.Actions = new FsmStateAction[]{startSequenceAction};
            next.Actions = new FsmStateAction[]{nextAction};
            endSequence.Actions = new FsmStateAction[]{endSequenceAction};

            fsm.States = new FsmState[]{dormant, startSequence, idle, next, endSequence};

            fsm.FsmComponent.enabled = true;
        }
        public static void StartSequence()
        {
            if (currentBoss.is3ActBoss)
            {
                PlayerData.instance.blackThreadWorld = true;
            }
            else
            {
                PlayerData.instance.blackThreadWorld = false;
            }

            GameManager.SceneLoadInfo sceneLoadInfo;
            if(isHoG && currentBoss.ascendedVersion != null && (BossStatueInfo.currentDifficultMode == "Ascended" || BossStatueInfo.currentDifficultMode == "Radiant"))
            {
                sceneLoadInfo = new GameManager.SceneLoadInfo
                {
                    SceneName = currentBoss.ascendedVersion.sceneName,
                    EntryGateName = currentBoss.ascendedVersion.entryGate,
                    EntrySkip = true,
                    Visualization = GameManager.SceneLoadVisualizations.Default,
                };
            }
            else
            {
                sceneLoadInfo = new GameManager.SceneLoadInfo
                {
                    SceneName = currentBoss.sceneName,
                    EntryGateName = currentBoss.entryGate,
                    EntrySkip = true,
                    Visualization = GameManager.SceneLoadVisualizations.Default,
                };
            }

            GameManager.instance.BeginSceneTransition(sceneLoadInfo);
        }
        public static void NextBoss()
        {
            if (currentBoss.is3ActBoss)
            {
                PlayerData.instance.blackThreadWorld = true;
            }
            else
            {
                PlayerData.instance.blackThreadWorld = false;
            }

            var sceneLoadInfo = new GameManager.SceneLoadInfo
            {
                SceneName = currentBoss.sceneName,
                EntryGateName = currentBoss.entryGate,
                EntrySkip = true,
                Visualization = GameManager.SceneLoadVisualizations.Default
            };

            GameManager.instance.BeginSceneTransition(sceneLoadInfo);
        }
        public static void EndSequence()
        {
            if(isHoG)
                PlayerDataMod.instance.badges[currentBoss.bossName].badges[BossStatueInfo.currentDifficultMode] = true;
            
            var sceneLoadInfo = new GameManager.SceneLoadInfo
            {
                SceneName = backScene,
                EntryGateName = backEntry,
                EntrySkip = true,
                Visualization = GameManager.SceneLoadVisualizations.Default
            };

            GameManager.instance.BeginSceneTransition(sceneLoadInfo);

            Reset();
        }
        public static void SetSequence(BossInfo[] bossSequence, string backEntry, string backScene, bool isPantheon = false,
                bool isHoG = false, string difficultMode = "", bool startImmediately = true)
        {
            Reset();

            BossSequence.bossSequence = bossSequence;
            BossSequence.currentBoss = bossSequence[0];
            BossSequence.backEntry = backEntry;
            BossSequence.backScene = backScene;

            BossSequence.isPantheon = isPantheon;
            BossSequence.isHoG = isHoG;
            BossSequence.difficultMode = difficultMode;

            if (startImmediately)
            {
                Start();
            }
        }
    }
}