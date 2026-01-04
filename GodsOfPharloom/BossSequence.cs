using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;
using UnityEngine.InputSystem;
using System.Reflection;
using UnityEngine.AddressableAssets;
using HarmonyLib;

namespace Gods_Of_Pharloom
{
    public class BossInfo
    {
        public static Dictionary<string, BossInfo> bosses;
        public static float waitForBossDeathAnim = 1.6f;
        public string sceneName;
        public string entryGate;
        public string bossName;
        public Dictionary<string, Dictionary<string, object>> bossesGOsInfo;
        public BossInfo ascendedVersion;
        public bool noInputOnStart;
        public bool is3ActBoss;

        public BossInfo(string sceneName, string entryGate, string bossName, string[] bossesGOsName, float[][] bossesHpMul, BossInfo ascendedVersion = null, bool is3ActBoss = false)
        {
            this.sceneName = sceneName;
            this.entryGate = entryGate;
            this.bossName = bossName;
            this.ascendedVersion = ascendedVersion;
            this.is3ActBoss = is3ActBoss;

            this.bossesGOsInfo = new Dictionary<string, Dictionary<string, object>>();
            
            foreach(var bossGOsName in bossesGOsName){
                var dictionary = new Dictionary<string, object>();
                for(int i = 0; i < bossesHpMul.Length; i++)
                {
                    dictionary = new Dictionary<string, object>{
                                                                {"Attuned", bossesHpMul[i][0]},
                                                                {"Ascended", bossesHpMul[i][1]},
                                                                {"Radiant", bossesHpMul[i][1]}
                                                                };
                };
                bossesGOsInfo.Add(bossGOsName, dictionary);
            }

        }
        public static void InitBossesInfo()
        {
            var bossesInfo = new BossInfo[]
            {
                new BossInfo("Tut_03", "start_battle_entry", "Moss Mother", new string[]{"Mossbone Mother"},new float[][]{new float[]{504f, 504*1.2f}}, 
                        ascendedVersion : new BossInfo("Weave_03", "start_battle_entry", "Moss Mother", new string[]{"Mossbone Mother A", "Mossbone Mother B"}, new float[][]{new float[]{817f, 817*1.2f}, new float[]{817f, 817*1.2f}})),
                new BossInfo("Bone_05", "start_battle_entry", "Bell Beast", new string[]{"Bone Beast"}, new float[][]{new float[]{350f, 350*1.2f}}),
                new BossInfo("Bone_East_08", "start_battle_entry", "Fourth Chorus", new string[]{"song_golem"}, new float[][]{new float[]{500f, 500*1.2f}}),
                new BossInfo("Coral_11", "start_battle_entry", "Great Conchflies", new string[]{"Driller A", "Driller B"}, new float[][]{new float[]{647f, 647*1.2f}, new float[]{647f, 647*1.2f}}),
                new BossInfo("Bone_East_12", "start_battle_entry", "Lace in Deep Docks", new string[]{"Lace Boss1"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Coral_Judge_Arena", "start_battle_entry", "The Last Judge", new string[]{"Last Judge"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Greymoor_08", "start_battle_entry", "Moorwing", new string[]{"Vampire Gnat"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Organ_01", "start_battle_entry", "Phantom", new string[]{"Phantom"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Ant_19", "start_battle_entry", "Savage Beastfly in Chapel of The Beast", new string[]{"Bone Flyer Giant"}, new float[][]{new float[]{680f, 680*1.2f}}),
                new BossInfo("Shellwood_18", "start_battle_entry", "Sister Splinter", new string[]{"Splinter Queen"}, new float[][]{new float[]{724f, 724*1.2f}}),
                new BossInfo("Bone_15", "start_battle_entry", "Skull Tyrant", new string[]{"Skull King"}, new float[][]{new float[]{727f, 727*1.2f}}),
                new BossInfo("Belltown_Shrine", "start_battle_entry", "Widow", new string[]{"Spinner Boss"}, new float[][]{new float[]{1512f, 1512*1.2f}}),
                new BossInfo("Slab_16b", "start_battle_entry", "Broodmother", new string[]{"Slab Fly Broodmother"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Cog_Dancers", "start_battle_entry", "Cogwork Dancers", new string[]{"Dancer A", "Dancer B"}, new float[][]{new float[]{444f, 444*1.2f}, new float[]{444f, 444*1.2f}}),
                new BossInfo("Dust_Chef", "start_battle_entry", "Disgraced Chef Lugoli", new string[]{"Roachkeeper Chef (1)"}, new float[][]{new float[]{970f, 970*1.2f}}),
                new BossInfo("Belltown_08", "start_battle_entry", "Father of the Flame", new string[]{"None"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Slab_10b", "start_battle_entry", "First Sinner", new string[]{"First Weaver"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Dock_09", "start_battle_entry", "Forebrothers Signis & Gron", new string[]{"Dock Guard Slasher", "Dock Guard Thrower"}, new float[][]{new float[]{890f, 890*1.2f}, new float[]{643f, 643*1.2f}}),
                new BossInfo("Library_09", "start_battle_entry", "Garmond & Zaza", new string[]{"Garmond Fighter"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Cradle_03", "start_battle_entry", "Grand Mother Silk", new string[]{"Silk Boss"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Shadow_18", "start_battle_entry", "Groal the Great", new string[]{"Swamp Shaman"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Song_Tower_01", "start_battle_entry", "Lace in the Cradle", new string[]{"Lace Boss2 New"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Coral_27", "start_battle_entry", "Raging Conchfly", new string[]{"Coral Conch Driller Giant Solo"}, new float[][]{new float[]{820f, 820*1.2f}}),
                new BossInfo("Bone_East_08", "start_battle_entry", "Savage Beastfly in Far Fields", new string[]{"Bone Flyer Giant"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Hang_17b", "start_battle_entry", "Second Sentiel", new string[]{"Song Knight"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Greymoor_08", "start_battle_entry", "Shakra", new string[]{"Mapper Spar NPC"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Ward_02", "start_battle_entry", "The Unravelled", new string[]{"Conductor Boss"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Library_13", "start_battle_entry", "Trobbio", new string[]{"Trobbio"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Coral_29", "start_battle_entry", "Voltvyrm", new string[]{"Zap Core Enemy"}, new float[][]{new float[]{2f, 3f}}),
                new BossInfo("Bellway_Centipede_Arena", "start_battle_entry", "Bell Eater", new string[]{"Giant Centipede Head", "Giant Centipede Butt"}, new float[][]{new float[]{2f, 3f}, new float[]{2f, 3f}}, is3ActBoss: true),
                new BossInfo("Clover_10", "start_battle_entry", "Clover Dancers", new string[]{"Dancer A"}, new float[][]{new float[]{2f, 3f}}, is3ActBoss: true),
                new BossInfo("Room_CrowCourt_02", "start_battle_entry", "Crawfather", new string[]{"Crawfather"}, new float[][]{new float[]{1300f, 1300*1.2f}}, is3ActBoss: true),
                new BossInfo("Memory_Coral_Tower", "start_battle_entry", "Crust King Khann", new string[]{"Coral King"}, new float[][]{new float[]{1650f, 1650*1.2f}}, is3ActBoss: true),
                new BossInfo("Bone_East_18b", "start_battle_entry", "Gurr the Outcast", new string[]{"Bone Hunter Trapper"}, new float[][]{new float[]{1000f, 1000*1.2f}}, is3ActBoss: true),
                new BossInfo("Coral_33", "start_battle_entry", "Lost Garmond", new string[]{"Garmond Black Threaded Fighter"}, new float[][]{new float[]{2f, 3f}}, is3ActBoss: true),
                new BossInfo("Abyss_Cocoon", "start_battle_entry", "Lost Lace", new string[]{"Lost Lace Boss"}, new float[][]{new float[]{2f, 3f}}, is3ActBoss: true),
                new BossInfo("Shellwood_11b_Memory", "start_battle_entry", "Nyleth", new string[]{"Flower Queen Boss"}, new float[][]{new float[]{1250f, 1250*1.2f}}, is3ActBoss: true),
                new BossInfo("Clover_19", "start_battle_entry", "Palestag", new string[]{"Cloverstag White Boss"}, new float[][]{new float[]{2f, 3f}}, is3ActBoss: true),
                new BossInfo("Peak_07", "start_battle_entry", "Pinstress", new string[]{"Pinstress Boss"}, new float[][]{new float[]{2f, 3f}}, is3ActBoss: true),
                new BossInfo("Crawl_10", "start_battle_entry", "Plasmified Zango", new string[]{"Blue Assistant"}, new float[][]{new float[]{2f, 3f}}, is3ActBoss: true),
                new BossInfo("Shellwood_22", "start_battle_entry", "Shrine Guardian Seth", new string[]{"Seth"}, new float[][]{new float[]{1185f, 1185*1.2f}}, is3ActBoss: true),
                new BossInfo("Memory_Ant_Queen", "start_battle_entry", "Skarrsinger Karmelita", new string[]{"Hunter Queen Boss"}, new float[][]{new float[]{1500f, 1500*1.2f}}, is3ActBoss: true),
                new BossInfo("Library_13", "start_battle_entry", "Tormented Trobbio", new string[]{"Tormented Trobbio"}, new float[][]{new float[]{2f, 3f}}, is3ActBoss: true),
                new BossInfo("Coral_39", "start_battle_entry", "Watcher at the Edge", new string[]{"Coral Warrior Grey"}, new float[][]{new float[]{2f, 3f}}, is3ActBoss: true),
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
            PlayerData.instance.tempRespawnMarker = backEntry;
            PlayerData.instance.tempRespawnScene = backScene;
            PlayerData.instance.tempRespawnType = 0;

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