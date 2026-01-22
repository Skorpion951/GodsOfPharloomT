using BepInEx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Reflection;
using HarmonyLib;
using HutongGames.PlayMaker;
using UnityEngine.UI;
using HutongGames.PlayMaker.Actions;
using System.Windows.Forms;
using UniverseLib.Utility;

namespace Gods_Of_Pharloom
{
    [Serializable]
    public class Badges
    {
        public string bossStatue;
        public Dictionary<string, bool> badges;
        public Badges(string bossStatue)
        {
            this.bossStatue = bossStatue;
            badges = new Dictionary<string, bool>
            {
                {"Attuned", false},
                {"Ascended", false},
                {"Radiant", false}
            };
        }
    }
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
        public static bool isInfiniteChallenge = false;
        public static GameObject difficultyModeCanvas;
        public static GameObject bossNameGO;
        public static GameObject attunedBadge;
        public static GameObject ascendedBadge;
        public static GameObject radiantBadge;
        public static Dictionary<string, Dictionary<string, GameObject>> menuModesGOs; //attuned, ascended, radiant etc.
        public Dictionary<string, GameObject> statueModeSpriteGOs; //attuned, ascended, radiant etc.
        public static GameObject selectArrow;
        public static BossStatueInfo[] bossStatues;

        public BossScene boss;
        public int statueIndex;
        public Dictionary<string, Dictionary<string, GameObject>> modes;
        Badges badges;

        public BossStatueInfo(BossScene boss)
        {
            this.boss = boss;
        }

        public static void InitBossesStatue()
        {
            var statues = new BossStatueInfo[BossScene.bosses.Count];

            int i = 0;
            foreach(var boss in BossScene.bosses)
            {
                statues[i] = new BossStatueInfo(boss.Value);
                statues[i].statueIndex = i;
                i++;
            }

            bossStatues = statues;
        }

        public static void GetBadges()
        {
            foreach(var bossStatue in bossStatues)
            {
                bossStatue.badges = PlayerDataMod.instance.badges[bossStatue.boss.bossName];
            }
        }
    }
    public class BossStatue : MonoBehaviour
    {
        public BossStatueInfo instance;

        void Awake()
        {
            if(BossStatueInfo.difficultyModeCanvas.IsNullOrDestroyed())
            {
                var rootObjects = this.gameObject.scene.GetRootGameObjects();
                foreach(var obj in rootObjects)
                {
                    if(obj.name == BossStatueInfo.difficultyModeCanvasGOName)
                    {
                        BossStatueInfo.difficultyModeCanvas = obj;
                        var children = obj.transform;

                        foreach(Transform child in children)
                        {
                            if(child.gameObject.name == "Modes")
                            {
                                var modes = new Dictionary<string, Dictionary<string, GameObject>>();

                                var children2 = child;
                                foreach(Transform child2 in children2)
                                {
                                    var children3 = child2;
                                    var dict = new Dictionary<string, GameObject>();
                                    foreach(Transform child3 in children3)
                                    {
                                        dict[child3.name] = child3.gameObject;
                                    }
                                    modes[child2.name] = dict;
                                }

                                BossStatueInfo.menuModesGOs = modes;
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
                if(this.gameObject.name == bossStatue.boss.bossName){
                    instance = bossStatue;
                    break;
                }
            }

            foreach(Transform child1 in this.gameObject.transform)
            {
                if(child1.name == "Orbs")
                {
                    var children = child1.gameObject.transform;
                    var statueSpritesModes = new Dictionary<string, GameObject>();
                    foreach(Transform child2 in children)
                    {
                        statueSpritesModes[child2.name] = child2.gameObject;
                    }
                    instance.statueModeSpriteGOs = statueSpritesModes;
                }
            }

            if(instance != null)
            {
                InitChallengeRegion();
            }
        }

        void InitChallengeRegion()
        {
            //arrow on current difficult mode
            var currentMode = BossStatueInfo.currentDifficultMode;
            var menuMod = BossStatueInfo.menuModesGOs[currentMode];
            var spriteMod = menuMod["SpriteMode"];
            var spritePos = spriteMod.transform.position;
            var arrowPos = BossStatueInfo.selectArrow.transform.position;

            BossStatueInfo.selectArrow.transform.position = new Vector3(arrowPos.x, spritePos.y, arrowPos.z);

            // set statue's hardest badge
            var modes = BossStatueInfo.difficultModes;
            for(int i = modes.Count - 1; i >= 0; i--)
            {
                if (PlayerDataMod.instance.badges[instance.boss.bossName].badges[modes[i]])
                {
                    instance.statueModeSpriteGOs[modes[i]].SetActive(true);
                    break;
                }
            }

            var go = this.gameObject;
            var pos = go.transform.position;
            bool isOnLeft = pos.x < 44.5f;

            var go_backEntry = new GameObject($"back_entry{instance.statueIndex}");
            SceneManager.MoveGameObjectToScene(go_backEntry, this.gameObject.scene);
            go_backEntry.transform.position = new Vector3(pos.x + 1.711f, pos.y, pos.z);
            var inputHandler = InputHandler.Instance.inputActions;

            var tp = CustomScene.CreateTransitionPoint(new TransitionPointInfo($"back_entry{instance.statueIndex}", new Vector3(), "", "", 
                                                        dontWalkOutOfDoor: true, isADoor: true, noInputOnStart: false), go_backEntry, BossStatueInfo.hog_sceneName);

            var interactComponent = go.AddComponent<PlayMakerNPC>();
            var fsmComponent = go.AddComponent<PlayMakerFSM>();
            var fsm = fsmComponent.Fsm;

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
                foreach(var item in BossStatueInfo.menuModesGOs)
                {
                    item.Value["SpriteMode"].SetActive(PlayerDataMod.instance.badges[instance.boss.bossName].badges[item.Key]);
                }
                var text = BossStatueInfo.bossNameGO.GetComponent<Text>();
                text.text = instance.boss.bossName;
                BossStatueInfo.difficultyModeCanvas.SetActive(true);
            };
            interactAction.updateAction += () =>
            {
                if (inputHandler.Up.WasPressed)
                {
                    var tmpIndex = BossStatueInfo.difficultModes.IndexOf(BossStatueInfo.currentDifficultMode);
                    var index = (tmpIndex == 0) ? BossStatueInfo.difficultModes.Count : tmpIndex;
                    BossStatueInfo.currentDifficultMode = BossStatueInfo.difficultModes[index - 1];

                    var currentMode = BossStatueInfo.currentDifficultMode;
                    var menuMod = BossStatueInfo.menuModesGOs[currentMode];
                    var spriteMod = menuMod["SpriteMode"];

                    var spritePos = spriteMod.transform.position;
                    var arrowPos = BossStatueInfo.selectArrow.transform.position;

                    BossStatueInfo.selectArrow.transform.position = new Vector3(arrowPos.x, spritePos.y, arrowPos.z);
                }
                if (inputHandler.Down.WasPressed)
                {
                    var tmpIndex = BossStatueInfo.difficultModes.IndexOf(BossStatueInfo.currentDifficultMode);
                    var index = (tmpIndex == BossStatueInfo.difficultModes.Count - 1) ? -1 : tmpIndex;
                    BossStatueInfo.currentDifficultMode = BossStatueInfo.difficultModes[index + 1];

                    var currentMode = BossStatueInfo.currentDifficultMode;
                    var menuMod = BossStatueInfo.menuModesGOs[currentMode];
                    var spriteMod = menuMod["SpriteMode"];

                    var spritePos = spriteMod.transform.position;
                    var arrowPos = BossStatueInfo.selectArrow.transform.position;

                    BossStatueInfo.selectArrow.transform.position = new Vector3(arrowPos.x, spritePos.y, arrowPos.z);
                }
                if (inputHandler.Jump.WasPressed)
                {
                    fsm.FsmComponent.SendEvent("START BOSS FIGHT");
                }
                if (inputHandler.QuickCast.WasPressed || inputHandler.MenuCancel.WasPressed || inputHandler.Cast.WasPressed)
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
                BossSequence.SetSequence(new BossScene[]{instance.boss}, $"back_entry{instance.statueIndex}", BossStatueInfo.hog_sceneName, isHoG: true);
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