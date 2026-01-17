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

namespace Gods_Of_Pharloom;

public class PantheonMenu : MonoBehaviour
{
    public static GameObject pantheonMenu;
    public static List<GameObject> buttons;
    public static GameObject selectArrow;
    public static Text pantheonName;
    public static GameObject currentButton;
    public static GameObject needleButton;
    public static GameObject silkButton;
    public static GameObject toolsButton;
    public static GameObject maskButton;
    public static GameObject beginButton;
    public static int pantheonsCount;
    public int pantheonIndex;
    public Pantheon pantheon;

    void Awake()
    {
        if(pantheonMenu.IsNullOrDestroyed())
        {
            var rootObjects = this.gameObject.scene.GetRootGameObjects();
            
            pantheonMenu = Preload.FindObjectByPath(rootObjects, "PantheonMenuCanvas");
            selectArrow = pantheonMenu.transform.Find("select_arrow").gameObject;
            pantheonName = pantheonMenu.transform.Find("PantheonNameText").gameObject.GetComponent<Text>();

            var buttonsChildren = pantheonMenu.transform.Find("Buttons");
            buttons = new List<GameObject>();

            needleButton = buttonsChildren.Find("Nail Binding").gameObject;
            silkButton = buttonsChildren.Find("Silk Binding").gameObject;
            toolsButton = buttonsChildren.Find("Tools Binding").gameObject;
            maskButton = buttonsChildren.Find("Mask Binding").gameObject;
            beginButton = buttonsChildren.Find("Begin").gameObject;

            currentButton = needleButton;

            buttons = new List<GameObject>{needleButton, silkButton, toolsButton, maskButton, beginButton};

            Reset();

            pantheonsCount = -1;
        }

        pantheonIndex = ++pantheonsCount;
    }

    void Reset()
    {
        foreach(var button in buttons)
        {
            button.SetActive(true);
            foreach(Transform child in button.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    void InitButtons()
    {
        var pd = PlayerDataMod.instance;

        if (pd.bindings["Needle Binding"] && pd.bindings["Silk Binding"] &&
            pd.bindings["Tools Binding"] && pd.bindings["Mask Binding"])
        {
            foreach(var binding in new GameObject[]{needleButton, silkButton, toolsButton, maskButton})
            {
                binding.transform.Find("AllActivated").gameObject.SetActive(true);
            }
        }
        else
        {
            needleButton.transform.Find("Activated").gameObject.SetActive(pd.bindings["Needle Binding"]);
            silkButton.transform.Find("Activated").gameObject.SetActive(pd.bindings["Silk Binding"]);
            toolsButton.transform.Find("Activated").gameObject.SetActive(pd.bindings["Tools Binding"]);
            maskButton.transform.Find("Activated").gameObject.SetActive(pd.bindings["Mask Binding"]);

            needleButton.transform.Find("Deactivated").gameObject.SetActive(!pd.bindings["Needle Binding"]);
            silkButton.transform.Find("Deactivated").gameObject.SetActive(!pd.bindings["Silk Binding"]);
            toolsButton.transform.Find("Deactivated").gameObject.SetActive(!pd.bindings["Tools Binding"]);
            maskButton.transform.Find("Deactivated").gameObject.SetActive(!pd.bindings["Mask Binding"]);
        }
    }

    void InitChallengeRegion()
    {
        //arrow on current button
        var currentButtonPos = currentButton.transform.position;
        var arrowPos = selectArrow.transform.position;

        selectArrow.transform.position = new Vector3(arrowPos.x, currentButtonPos.y, arrowPos.z);

        var go = this.gameObject;
        var pos = go.transform.position;
        bool isOnLeft = pos.x < 44.5f;

        var go_backEntry = new GameObject($"back_entry{pantheonIndex}");
        SceneManager.MoveGameObjectToScene(go_backEntry, this.gameObject.scene);
        go_backEntry.transform.position = new Vector3(pos.x + 1.711f, pos.y, pos.z);
        var inputHandler = InputHandler.Instance.inputActions;

        var tp = CustomScene.CreateTransitionPoint(new TransitionPointInfo($"back_entry{pantheonIndex}", new Vector3(), "", "", 
                                                    dontWalkOutOfDoor: true, isADoor: true, noInputOnStart: false), go_backEntry, this.gameObject.scene.name);

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

        var startPantheon = new FsmState(fsm);
        startPantheon.Name = "Start Pantheon Sequence";

        var exitMenu = new FsmState(fsm);
        exitMenu.Name = "Exit Menu";

        
        var interactAction = new PatchedFsm.CustomLogicFsm(fsm);
        interactAction.action += (Fsm fsm) =>
        {
            InitButtons();
            pantheonName.text = pantheon.pantheonDisplayName;
            pantheonMenu.SetActive(true);
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
                fsm.FsmComponent.SendEvent("START BOSS SEQUENCE");
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

        // var startPantheonSequence = new PatchedFsm.CustomLogicFsm(fsm);
        // startPantheonSequence.action += (Fsm fsm) =>
        // {
        //     BossSequence.SetSequence(new BossInfo[]{instance.boss}, $"back_entry{instance.statueIndex}", BossStatueInfo.hog_sceneName, isHoG: true);
        // };


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
                ToFsmState = startPantheon,
                FsmEvent = FsmEvent.GetFsmEvent("START BOSS SEQUENCE")
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

        // startPantheon.Actions = new FsmStateAction[]{startPantheonSequence};


        fsm.States = new FsmState[]{init, idle, interact, startPantheon, exitMenu};
        fsm.StartState = "Init";
        fsm.SetState("Init");
    }
}