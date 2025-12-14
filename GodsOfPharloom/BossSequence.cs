using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gods_Of_Pharloom
{
    // public enum Bosses
    // {
    //     MossMother = 0,
    //     DoubleMossMother,
    //     BellBeast,
    //     FourthChorus,
    //     GreatConchflies,
    //     Lace1,
    //     LastJudge,
    //     Moorwing,
    //     Phantom,
    //     SavageBeastfly1,
    //     SisterSplinter,
    //     SkullTyrant,
    //     Window,
    //     Broodmother,
    //     CogworkDancers,
    //     DisgracedChefLugoli,
    //     FatherOfTheFlame,
    //     FirstSinner,
    //     ForebrothersSignisAndGron,
    //     GarmondAndZaza,
    //     GrandMotherSilk,
    //     GroalTheGreat,
    //     Lace2,
    //     RagingConchfly,
    //     SavageBeastfly2,
    //     SecondSentiel,
    //     Shakra,
    //     TheUnravelled,
    //     Trobbio,
    //     Voltvyrm,
    //     BellEater,
    //     CloverDancers,
    //     Crawfather,
    //     CrustKingKhann,
    //     GurrTheOutcast,
    //     LostGarmond,
    //     LostLace,
    //     Nyleth,
    //     Palestag,
    //     Pinstress,
    //     PlasmifiedZango,
    //     ShrineGuardianSeth,
    //     SkarrsingerKarmelita,
    //     TormentedTrobbio,
    //     WatherAtTheEdge
    // };
    public class BossInfo
    {
        public static Dictionary<string, BossInfo> bosses = new Dictionary<string, BossInfo>{
            {"Moss Mother", new BossInfo("Tut_03", "start_battle_entry", "Moss Mother", new float[][]{new float[]{120f, 2f, 1.5f}}, 
                                        ascendedVersion : new BossInfo("Weave_03", "start_battle_entry", "Moss Mother", new float[][]{new float[]{350f, 2f, 1.5f},
                                                                                                      new float[]{350f, 2f, 1.5f}}))},
            {"Bell Beast", new BossInfo("Bone_05", "start_battle_entry", "Bell Beast", new float[][]{new float[]{150f, 2f, 1.5f}})},
            {"Fourth Chorus", new BossInfo("Bone_East_08", "start_battle_entry", "Fourth Chorus", new float[][]{new float[]{500f, 2f, 1.5f}})},
            {"Great Conchflies", new BossInfo("Coral_11", "start_battle_entry", "Great Conchflies", new float[][]{new float[]{400f, 2f, 1.5f}})},
            {"Lace1", new BossInfo("Bone_East_12", "start_battle_entry", "Lace In Deep Docks", new float[][]{new float[]{250f, 2f, 1.5f}})},

            {"Lost Lace", new BossInfo("Abyss Cocoon", "start_battle_entry", "Lost Lace", new float[][]{new float[]{1800f, 1.5f, 2f}})},

        };
        public string sceneName;
        public string entryGate;
        public string bossName;
        public float[][] bossesHp;
        public BossInfo ascendedVersion;

        public BossInfo(string sceneName, string entryGate, string bossName, float[][] bossesHp, BossInfo ascendedVersion = null)
        {
            this.sceneName = sceneName;
            this.entryGate = entryGate;
            this.bossName = bossName;
            this.bossesHp = bossesHp;
            this.ascendedVersion = ascendedVersion;
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
        public bool doEndSequence = false;
        public Fsm sequenceController;
        public string difficultMode;
        public bool isPantheon = false;
        public bool isHoG = false;

        void OnEnable()
        {
            instance = this;
            isInSequence = true;
            currentBoss = bossSequence[0];



            var fsm = sequenceController;

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
                    instance.sequenceController.FsmComponent.SendEvent("END SEQUENCE");
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



            fsm.FsmComponent.enabled = true;
        }
        public void StartSequence()
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
        public void SequenceEnd()
        {
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

            var fsmComponent = instance.gameObject.AddComponent<PlayMakerFSM>();
            fsmComponent.enabled = false;
            sequenceComponent.sequenceController = fsmComponent.Fsm;

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