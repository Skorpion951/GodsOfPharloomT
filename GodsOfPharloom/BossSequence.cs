using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gods_Of_Pharloom
{
    public class BossInfo
    {
        public static Dictionary<string, BossInfo> bosses = CreateBossesInfo();
        public string sceneName;
        public string entryGate;
        public string bossName;
        public Dictionary<string, float>[] bossesHpMul;
        public BossInfo ascendedVersion;
        public bool noInputOnStart;

        public BossInfo(string sceneName, string entryGate, string bossName, float[][] bossesHpMul, BossInfo ascendedVersion = null, bool noInputOnStart = true)
        {
            this.sceneName = sceneName;
            this.entryGate = entryGate;
            this.bossName = bossName;
            this.ascendedVersion = ascendedVersion;
            this.noInputOnStart = noInputOnStart;

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
        public static Dictionary<string, BossInfo> CreateBossesInfo()
        {
            var bossesInfo = new BossInfo[]
            {
                new BossInfo("Tut_03", "start_battle_entry", "Moss Mother", new float[][]{new float[]{120f, 2f, 1.5f}}, 
                                        ascendedVersion : new BossInfo("Weave_03", "start_battle_entry", "Moss Mother", new float[][]{new float[]{350f, 2f, 1.5f}})),
                new BossInfo("Bone_05", "start_battle_entry", "Bell Beast", new float[][]{new float[]{150f, 2f, 1.5f}}),
                new BossInfo("Bone_East_08", "start_battle_entry", "Fourth Chorus", new float[][]{new float[]{500f, 2f, 1.5f}}),
                new BossInfo("Coral_11", "start_battle_entry", "Great Conchflies", new float[][]{new float[]{400f, 2f, 1.5f}}),
                new BossInfo("Bone_East_12", "start_battle_entry", "Lace In Deep Docks", new float[][]{new float[]{250f, 2f, 1.5f}}),
                new BossInfo("Bone_East_12", "start_battle_entry", "Lost Garmond", new float[][]{new float[]{250f, 2f, 1.5f}}),

                new BossInfo("Abyss_Cocoon", "start_battle_entry", "Lost Lace", new float[][]{new float[]{1800f, 1.5f, 2f}}),
            };

            var dict = new Dictionary<string, BossInfo>();
            foreach(var bossInfo in bossesInfo)
            {
                dict[bossInfo.bossName] = bossInfo;
            }
            return dict;
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
        public bool isPantheon = false;
        public bool isHoG = false;

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
                var index = ++instance.currentBossIndex;
                if(index !< instance.bossSequence.Length)
                {
                    instance.sequenceController.SendEvent("END SEQUENCE");
                    return;
                }
            };

            var endSequenceAction = new PatchedFsm.CustomLogicFsm(fsm);
            endSequenceAction.action += (Fsm fsm) =>
            {
                instance.EndSequence();
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
                    FsmEvent = FsmEvent.GetFsmEvent("BOSS DEAD"),
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
                    FsmEvent = FsmEvent.GetFsmEvent("REPEAT"),
                    ToFsmState = idle
                },
                new FsmTransition
                {
                    FsmEvent = FsmEvent.GetFsmEvent("END SEQUENCE"),
                    ToFsmState = endSequence
                }
            };

            startSequence.Actions = new FsmStateAction[]{startSequenceAction};

            fsm.States = new FsmState[]{startSequence, idle, next, endSequence};

            fsm.FsmComponent.enabled = true;
        }
        public void StartSequence()
        {
            var sceneLoadInfo = new GameManager.SceneLoadInfo
            {
                SceneName = currentBoss.sceneName,
                EntryGateName = currentBoss.entryGate,
                EntrySkip = true,
                Visualization = GameManager.SceneLoadVisualizations.Default,
            };

            GameManager.instance.BeginSceneTransition(sceneLoadInfo);
        }
        public void NextBoss()
        {
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
            PlayerDataMod.instance.badges[currentBoss.bossName].badges[BossStatueInfo.currentDifficultMode] = true;
            var sceneLoadInfo = new GameManager.SceneLoadInfo
            {
                SceneName = backScene,
                EntryGateName = backEntry,
                EntrySkip = true,
                Visualization = GameManager.SceneLoadVisualizations.Default
            };

            GameManager.instance.BeginSceneTransition(sceneLoadInfo);
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