using BepInEx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Reflection;
using HarmonyLib;
using Unity.Mathematics;
using Newtonsoft.Json;
using GlobalEnums;
using System.Collections;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using GenericVariableExtension;
using TeamCherry.NestedFadeGroup;
using Mono.Posix;

namespace Gods_Of_Pharloom;

public class Pantheon : MonoBehaviour
{
    public string pantheonName;
    public string pantheonDisplayName = "";
    public Transform bindings;
    public GameObject needleBinding;
    public GameObject silkBindng;
    public GameObject toolsBinding;
    public GameObject maskBinding;
    public GameObject hitlessHeart;
    public GameObject doorStates;
    public GameObject radiantBackboard;
    public BossScene[] sequence;
    public static int pantheonsCount = 0;
    public int pantheonIndex;
    void Awake()
    {
        bindings = this.transform.Find("Bindings");
        needleBinding = bindings.Find("Needle Binding").gameObject;
        silkBindng = bindings.Find("Silk Binding").gameObject;
        toolsBinding = bindings.Find("Tools Binding").gameObject;
        maskBinding = bindings.Find("Mask Binding").gameObject;

        doorStates = this.transform.Find("Door_States").gameObject;
        hitlessHeart = doorStates.transform.Find("DoorNoHitHeart").gameObject;
        radiantBackboard = doorStates.transform.Find("gg_radiant_backboard").gameObject;

        pantheonIndex = pantheonsCount;
        pantheonsCount++;
    }
    void Reset()
    {
        foreach(Transform binding in bindings.transform)
        {
            binding.gameObject.SetActive(true);
            foreach(Transform state in binding.transform) state.gameObject.SetActive(false);
        }

        foreach(Transform state in doorStates.transform) state.gameObject.SetActive(false);
    }
    public void Init()
    {
        Reset();

        var pd = PlayerDataMod.instance;
        var pantheon = pd.pantheonsInfo[pantheonName];

        hitlessHeart.SetActive(pantheon.completedNoHit);

        if (pantheon.completedAllBindings)
        {
            foreach(var binding in new GameObject[]{needleBinding, silkBindng, toolsBinding, maskBinding })
            {
                binding.transform.Find("AllActivated").gameObject.SetActive(true);
            }
            radiantBackboard.SetActive(true);
        }
        else
        {
            needleBinding.transform.Find("Activated").gameObject.SetActive(pantheon.completedNeedleBinding);
            silkBindng.transform.Find("Activated").gameObject.SetActive(pantheon.completedSilkBinding);
            toolsBinding.transform.Find("Activated").gameObject.SetActive(pantheon.completedToolsBinding);
            maskBinding.transform.Find("Activated").gameObject.SetActive(pantheon.completedMaskBinding);
        }

        if(pantheon.completedPantheon && !pantheon.completedAllBindings) doorStates.transform.Find("State1").gameObject.SetActive(true);
        else if(pantheon.completedAllBindings && !pantheon.completedAllBindingsNoHit) doorStates.transform.Find("State2").gameObject.SetActive(true);
        else if(pantheon.completedAllBindingsNoHit) doorStates.transform.Find("State3").gameObject.SetActive(true);

        InitChallengeRegion();
    }
    public void InitChallengeRegion()
    {
        var selectArrow = PantheonMenu.selectArrow;
        var buttons = PantheonMenu.buttons;
        var currentButton = PantheonMenu.currentButton;

        //arrow on current button
        var currentButtonPos = currentButton.transform.position;
        var arrowPos = selectArrow.transform.position;

        PantheonMenu.pantheonMenu.SetActive(false);

        selectArrow.transform.position = new Vector3(arrowPos.x, currentButtonPos.y, arrowPos.z);

        var go = this.gameObject;
        var pos = go.transform.position;

        var inputHandler = InputHandler.Instance.inputActions;

        var tp = CustomScene.CreateTransitionPoint(new TransitionPointInfo($"back_entry{pantheonIndex}", new Vector3(), "", "", 
                                                    dontWalkOutOfDoor: true, isADoor: true, noInputOnStart: false), this.gameObject.scene.name);
        var go_backEntry = tp.gameObject;
        go_backEntry.name = $"back_entry{pantheonIndex}";
        SceneManager.MoveGameObjectToScene(go_backEntry, this.gameObject.scene);
        go_backEntry.transform.position = new Vector3(pos.x, pos.y, pos.z);

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
            PantheonMenu.UpdateButtons();
            PantheonMenu.pantheonName.text = pantheonDisplayName;
            
            if (!PantheonMenu.instance.isFlashUiInited)
            {
                if(Preload.preloads.TryGetValue("generic_flash_ui", out GameObject val))
                {
                    var buttons = new GameObject[]{PantheonMenu.needleButton, PantheonMenu.silkButton, PantheonMenu.toolsButton, PantheonMenu.maskButton};
                    foreach(var button in buttons)
                    {
                        var flashUI = GameObject.Instantiate(val, parent: button.transform);
                        flashUI.name = "generic_flash_ui";
                    }
                    PantheonMenu.instance.isFlashUiInited = true;
                }
            }

            foreach(var button in buttons)
            {
                var flash = button.transform.Find("generic_flash_ui");
                if(flash) flash.gameObject.SetActive(false);
            }

            PantheonMenu.pantheonMenu.SetActive(true);
        };
        interactAction.updateAction += () =>
        {
            if (inputHandler.Up.WasPressed)
            {
                var index = (PantheonMenu.currentButtonIndex < 1) ? buttons.Count : PantheonMenu.currentButtonIndex;
                currentButton = buttons[index - 1];
                PantheonMenu.currentButtonIndex = index - 1;

                var buttonPos = currentButton.transform.position;
                var arrowPos = selectArrow.transform.position;

                selectArrow.transform.position = new Vector3(arrowPos.x, buttonPos.y, arrowPos.z);
            }
            if (inputHandler.Down.WasPressed)
            {
                var index = (PantheonMenu.currentButtonIndex >= buttons.Count - 1) ? -1 : PantheonMenu.currentButtonIndex;
                currentButton = buttons[index + 1];
                PantheonMenu.currentButtonIndex = index + 1;

                var buttonPos = currentButton.transform.position;
                var arrowPos = selectArrow.transform.position;

                selectArrow.transform.position = new Vector3(arrowPos.x, buttonPos.y, arrowPos.z);
            }
            if (inputHandler.Jump.WasPressed)
            {
                var pdm = PlayerDataMod.instance;
                var pd = PlayerData.instance;

                if(currentButton == PantheonMenu.beginButton){
                    PantheonMenu.pantheonMenu.SetActive(false);
                    fsm.FsmComponent.SendEvent("START BOSS SEQUENCE");
                }
                else if(currentButton == PantheonMenu.needleButton) PantheonMenu.ToggleBinding(PantheonMenu.needleButton);
                else if(currentButton == PantheonMenu.silkButton) PantheonMenu.ToggleBinding(PantheonMenu.silkButton);
                else if(currentButton == PantheonMenu.toolsButton)
                {
                    PantheonMenu.ToggleBinding(PantheonMenu.toolsButton);
                }
                else if(currentButton == PantheonMenu.maskButton)
                {
                    PantheonMenu.ToggleBinding(PantheonMenu.maskButton);

                    if(pdm.bindings["Mask Binding"])
                    {
                        pdm.previousHealthCount = pd.maxHealth;

                        GodsOfPharloomMod.instance.StartCoroutine(BindingsMenu.TrySetHeroHealth(BindingsMenu.maskBindingCount));
                    }
                    else
                    {
                        GodsOfPharloomMod.instance.StartCoroutine(BindingsMenu.TrySetHeroHealth(pdm.previousHealthCount));
                    }
                }
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
            PantheonMenu.pantheonMenu.SetActive(false);
            inputHandler.ClearInputState();
            interactComponent.ForceEndDialogue();
            exitAction.Finish();
        };
        exitMenu.Actions = new FsmStateAction[]{new HutongGames.PlayMaker.Actions.NextFrameEvent(), exitAction};

        var startPantheonSequence = new PatchedFsm.CustomLogicFsm(fsm);
        startPantheonSequence.action += (Fsm fsm) =>
        {
            BossSequence.SetSequence(sequence, $"back_entry{pantheonIndex}", this.gameObject.scene.name, BossSequence.SequenceType.Pantheon, pantheonName: pantheonName);
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

        startPantheon.Actions = new FsmStateAction[]{startPantheonSequence};


        fsm.States = new FsmState[]{init, idle, interact, startPantheon, exitMenu};
        fsm.StartState = "Init";
        fsm.SetState("Init");
    }
}