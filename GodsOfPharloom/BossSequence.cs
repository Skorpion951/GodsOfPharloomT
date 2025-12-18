using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gods_Of_Pharloom
{
    public class BossInfo
    {
        public static Dictionary<string, BossInfo> bosses;
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
    public class BossSequence : MonoBehaviour
    {
        public static BossSequence instance;
        public static bool isInSequence = false;
        public BossInfo[] bossSequence;
        public static BossInfo currentBoss;
        public int currentBossIndex = 0;
        public string backEntry;
        public string backScene;
        public PlayMakerFSM sequenceController;
        public string difficultMode;
        public bool isPantheon;
        public bool isHoG;

        void OnEnable()
        {
            instance = this;
            isInSequence = true;
            currentBoss = bossSequence[0];

            var fsmComponent = this.gameObject.AddComponent<PlayMakerFSM>();
            fsmComponent.enabled = false;

            var fsm = fsmComponent.Fsm;

            sequenceController = fsmComponent;

            var startSequence = new FsmState(fsm);
            startSequence.Name = "Start Sequence";

            var idle = new FsmState(fsm);
            idle.Name = "Idle";

            var next = new FsmState(fsm);
            next.Name = "Next";

            var endSequence = new FsmState(fsm);
            endSequence.Name = "End Sequence";

            fsm.StartState = "Start Sequence";

            var startSequenceAction = new PatchedFsm.CustomLogicFsm(fsm);
            startSequenceAction.action = (Fsm fsm) =>
            {
                instance.StartSequence();

                startSequenceAction.Finish();
            };

            var nextAction = new PatchedFsm.CustomLogicFsm(fsm);
            nextAction.action += (Fsm fsm) =>
            {
                instance.currentBossIndex++;
                if(!(currentBossIndex < instance.bossSequence.Length))
                {
                    instance.sequenceController.SendEvent("END SEQUENCE");
                    return;
                }

                instance.NextBoss();
                instance.sequenceController.SendEvent("NEXT");
            };

            var endSequenceAction = new PatchedFsm.CustomLogicFsm(fsm);
            endSequenceAction.action += (Fsm fsm) =>
            {
                instance.EndSequence();
                endSequenceAction.Finish();
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

            fsm.States = new FsmState[]{startSequence, idle, next, endSequence};

            fsm.FsmComponent.enabled = true;
        }
        public void StartSequence()
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
            if(instance.isHoG && currentBoss.ascendedVersion != null && (BossStatueInfo.currentDifficultMode == "Ascended" || BossStatueInfo.currentDifficultMode == "Radiant"))
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
        public void NextBoss()
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
        public void EndSequence()
        {
            if(instance.isHoG)
                PlayerDataMod.instance.badges[currentBoss.bossName].badges[BossStatueInfo.currentDifficultMode] = true;
            
            var sceneLoadInfo = new GameManager.SceneLoadInfo
            {
                SceneName = backScene,
                EntryGateName = backEntry,
                EntrySkip = true,
                Visualization = GameManager.SceneLoadVisualizations.Default
            };

            GameManager.instance.BeginSceneTransition(sceneLoadInfo);

            this.Destroy();
        }
        public static GameObject CreateSequence(BossInfo[] bossSequence, string backEntry, string backScene, bool isPantheon = false,
                bool isHoG = false, string difficultMode = "")
        {
            var go = new GameObject("BossSequence");
            go.SetActive(false);
            DontDestroyOnLoad(go);

            var sequenceComponent = go.AddComponent<BossSequence>();

            sequenceComponent.bossSequence = bossSequence;
            sequenceComponent.backEntry = backEntry;
            sequenceComponent.backScene = backScene;

            sequenceComponent.isPantheon = isPantheon;
            sequenceComponent.isHoG = isHoG;
            sequenceComponent.difficultMode = difficultMode;

            go.SetActive(true);

            return go;
        }
        public void Destroy()
        {
            isInSequence = false;
            currentBoss = null;
            GameObject.Destroy(this.gameObject);
        }
    }
}