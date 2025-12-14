using BepInEx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Reflection;
using HarmonyLib;
using HutongGames.PlayMaker;
using UnityEngine.UI;

namespace Gods_Of_Pharloom
{
    public class BossStatueInfo
    {
        public static string hog_sceneName = "GG_Pharloom_Hall_Of_Gods";
        public static List<string> difficultModes = new List<string>{
            "Attuned",
            "Ascended",
            "Radiant"
        };
        public static string currentDifficultMode = difficultModes[0];
        public static string difficultyModeCanvasGOName = "DifficultyModeCanvas";
        public static GameObject difficultyModeCanvas;
        public static GameObject bossNameGO;
        public static GameObject attunedBadge;
        public static GameObject ascendedBadge;
        public static GameObject radiantBadge;
        public static Dictionary<string, GameObject> menuBadgesGOs = new Dictionary<string, GameObject>();
        public Dictionary<string, GameObject> statueBadgesGOs = new Dictionary<string, GameObject>();
        public static GameObject selectArrow;
        public static BossStatueInfo[] bossStatues = new BossStatueInfo[]
        {
            new BossStatueInfo(BossInfo.bosses["Lost Lace"], "Abyss_Cocoon", "LostLace_Statue", "Lost Lace"),
        };
        [Serializable]
        private class Badges
        {
            public string boss;
            public bool hasAttunedBadge = false;
            public bool hasAscendedBadge = false;
            public bool hasRadiantBadge = false;
            public Badges(string boss)
            {
                this.boss = boss;
            }
        }

        public BossInfo boss;
        string sceneName;
        public string statueObjectName;
        public string bossName;
        Badges badges;

        public BossStatueInfo(BossInfo boss, string sceneName, string statueObjectName, string bossName)
        {
            this.boss = boss;
            this.sceneName = sceneName;
            this.statueObjectName = statueObjectName;
            this.bossName = bossName;
        }
    }
    public class BossStatue : MonoBehaviour
    {
        public BossStatueInfo instance;

        void Awake()
        {
            if(BossStatueInfo.difficultyModeCanvas == null)
            {
                var rootObjects = SceneManager.GetSceneByName(BossStatueInfo.hog_sceneName).GetRootGameObjects();
                foreach(var obj in rootObjects)
                {
                    if(obj.name == BossStatueInfo.difficultyModeCanvasGOName)
                    {
                        BossStatueInfo.difficultyModeCanvas = obj;
                        var children = obj.transform;

                        foreach(Transform child in children)
                        {
                            if(child.gameObject.name == "AttunedBadge"){
                                BossStatueInfo.menuBadgesGOs["Attuned"] = child.gameObject;
                                continue;
                            }
                            if(child.gameObject.name == "AscendedBadge"){
                                BossStatueInfo.menuBadgesGOs["Ascended"] = child.gameObject;
                                continue;
                            }
                            if(child.gameObject.name == "RadiantBadge"){
                                BossStatueInfo.menuBadgesGOs["Radiant"] = child.gameObject;
                                continue;
                            }
                            if(child.gameObject.name == "BossNameText"){
                                BossStatueInfo.bossNameGO = child.gameObject;
                                continue;
                            }
                            if(child.gameObject.name == "select_arrow") BossStatueInfo.selectArrow = child.gameObject;
                        }
                        break;
                    }
                }
            }

            foreach(var bossStatue in BossStatueInfo.bossStatues)
            {
                if(this.gameObject.name == bossStatue.statueObjectName){
                    instance = bossStatue;
                    break;
                }
            }

            foreach(Transform child1 in this.gameObject.transform)
            {
                if(child1.name == "Orbs")
                {
                    var children = child1.gameObject.transform;
                    foreach(Transform child2 in children)
                    {
                        if(child2.gameObject.name == "Attuned_Orb"){
                            instance.statueBadgesGOs["Attuned"] = child2.gameObject;
                            continue;
                        }
                        if(child2.gameObject.name == "Ascended_Orb"){
                            instance.statueBadgesGOs["Ascended"] = child2.gameObject;
                            continue;
                        }
                        if(child2.gameObject.name == "Radiant_Orb"){
                            instance.statueBadgesGOs["Radiant"] = child2.gameObject;
                            continue;
                        }
                    }
                }
            }

            if(instance != null)
            {
                InitChallangeRegion();
            }
        }

        void InitChallangeRegion()
        {
            var go = this.gameObject;
            var inputHandler = InputHandler.Instance.inputActions;

            var collider = go.AddComponent<BoxCollider2D>();
            var interactComponent = go.AddComponent<PlayMakerNPC>();
            var fsmComponent = go.AddComponent<PlayMakerFSM>();
            var fsm = fsmComponent.Fsm;

            collider.size = new Vector2(3f, 0.3f);
            collider.offset = new Vector2(0f, -1.35f);
            collider.isTrigger = true;

            interactComponent.InteractLabel = InteractableBase.PromptLabels.Challenge;
            interactComponent.CustomEventTarget = fsmComponent;

            var init = new FsmState(fsm);
            init.Name = "Init";

            var idle = new FsmState(fsm);
            idle.Name = "Idle";

            var interact = new FsmState(fsm);
            interact.Name = "Interact";

            var startBossFight = new FsmState(fsm);
            startBossFight.Name = "Start Boss Fight";

            var exitMenu = new FsmState(fsm);
            exitMenu.Name = "Exit Menu";

            
            var interactAction = new PatchedFsm.CustomLogicFsm(fsm);
            interactAction.action += (Fsm fsm) =>
            {
                var text = BossStatueInfo.bossNameGO.GetComponent<Text>();
                text.text = instance.bossName;
                BossStatueInfo.difficultyModeCanvas.SetActive(true);
            };
            interactAction.updateAction += () =>
            {
                if (inputHandler.Up.WasPressed)
                {
                    var tmpIndex = BossStatueInfo.difficultModes.IndexOf(BossStatueInfo.currentDifficultMode);
                    var index = (tmpIndex == 0) ? BossStatueInfo.difficultModes.Count : tmpIndex;
                    BossStatueInfo.currentDifficultMode = BossStatueInfo.difficultModes[index - 1];

                    var mode = BossStatueInfo.currentDifficultMode;
                    var badge = BossStatueInfo.menuBadgesGOs[mode];

                    var badgePos = badge.transform.position;
                    var arrowPos = BossStatueInfo.selectArrow.transform.position;

                    BossStatueInfo.selectArrow.transform.position = new Vector3(arrowPos.x, badgePos.y, arrowPos.z);
                }
                if (inputHandler.Down.WasPressed)
                {
                    var tmpIndex = BossStatueInfo.difficultModes.IndexOf(BossStatueInfo.currentDifficultMode);
                    var index = (tmpIndex == BossStatueInfo.difficultModes.Count - 1) ? -1 : tmpIndex;
                    BossStatueInfo.currentDifficultMode = BossStatueInfo.difficultModes[index + 1];

                    var mode = BossStatueInfo.currentDifficultMode;
                    var badge = BossStatueInfo.menuBadgesGOs[mode];

                    var badgePos = badge.transform.position;
                    var arrowPos = BossStatueInfo.selectArrow.transform.position;

                    BossStatueInfo.selectArrow.transform.position = new Vector3(arrowPos.x, badgePos.y, arrowPos.z);
                }
                if (inputHandler.Jump.WasPressed)
                {
                    fsm.FsmComponent.SendEvent("START BOSS FIGHT");
                }
                if (inputHandler.QuickCast.WasPressed)
                {
                    fsm.FsmComponent.SendEvent("EXIT MENU");
                }
            };
            interact.Actions = new FsmStateAction[]{interactAction};

            var exitAction = new PatchedFsm.CustomLogicFsm(fsm);
            exitAction.action += (Fsm fsm) =>
            {
                BossStatueInfo.difficultyModeCanvas.SetActive(false);
                inputHandler.ClearInputState();
                interactComponent.ForceEndDialogue();
                exitAction.Finish();
            };
            exitMenu.Actions = new FsmStateAction[]{new HutongGames.PlayMaker.Actions.NextFrameEvent(), exitAction};

            var startBossFightAction = new PatchedFsm.CustomLogicFsm(fsm);
            startBossFightAction.action += (Fsm fsm) =>
            {
                BossSequence.CreateSequence(new BossInfo[]{instance.boss}, "back_entry1", BossStatueInfo.hog_sceneName);
            };


            init.Transitions = new FsmTransition[]
            {
                new FsmTransition
                {
                    ToFsmState = idle,
                    FsmEvent = FsmEvent.GetFsmEvent("FINISHED")
                },
            };

            idle.Transitions = new FsmTransition[]
            {
                new FsmTransition
                {
                    ToFsmState = interact,
                    FsmEvent = FsmEvent.GetFsmEvent("INTERACT")
                },
            };

            interact.Transitions = new FsmTransition[]
            {
                new FsmTransition
                {
                    ToFsmState = startBossFight,
                    FsmEvent = FsmEvent.GetFsmEvent("START BOSS FIGHT")
                },
                new FsmTransition
                {
                    ToFsmState = exitMenu,
                    FsmEvent = FsmEvent.GetFsmEvent("EXIT MENU")
                },
            };

            exitMenu.Transitions = new FsmTransition[]
            {
                new FsmTransition
                {
                    ToFsmState = idle,
                    FsmEvent = FsmEvent.GetFsmEvent("FINISHED")
                },  
            };

            startBossFight.Actions = new FsmStateAction[]{startBossFightAction};


            fsm.States = new FsmState[]{init, idle, interact, startBossFight, exitMenu};
            fsm.StartState = "Init";
            fsm.SetState("Init");
        }
    }
}