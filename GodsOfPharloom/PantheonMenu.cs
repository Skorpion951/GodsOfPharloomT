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
using UniverseLib;

namespace Gods_Of_Pharloom;

public class PantheonMenu : MonoBehaviour
{
    public static PantheonMenu instance;
    public static GameObject pantheonMenu;
    public static List<GameObject> buttons;
    public static GameObject selectArrow;
    public static Text pantheonName;
    public static GameObject currentButton;
    public static GameObject needleButton;
    public static GameObject silkButton;
    public static GameObject toolsButton;
    public static GameObject maskButton;
    public static GameObject[] bindings;
    public static GameObject beginButton;
    public bool isFlashUiInited = false;

    void Awake()
    {
        instance = this;
        
        pantheonMenu = this.transform.Find("PantheonMenuCanvas").gameObject;
        selectArrow = pantheonMenu.transform.Find("select_arrow").gameObject;
        pantheonName = pantheonMenu.transform.Find("PantheonNameText").gameObject.GetComponent<Text>();

        var buttonsChildren = pantheonMenu.transform.Find("Buttons");
        buttons = new List<GameObject>();

        needleButton = buttonsChildren.Find("Needle Binding").gameObject;
        silkButton = buttonsChildren.Find("Silk Binding").gameObject;
        toolsButton = buttonsChildren.Find("Tools Binding").gameObject;
        maskButton = buttonsChildren.Find("Mask Binding").gameObject;
        beginButton = buttonsChildren.Find("Begin").gameObject;

        currentButton = needleButton;
        bindings = new GameObject[]{needleButton, silkButton, toolsButton, maskButton};

        buttons = new List<GameObject>{needleButton, silkButton, toolsButton, maskButton, beginButton};

        Reset();
    }

    public static void Reset()
    {
        foreach(var button in buttons)
        {
            button.SetActive(true);
            foreach(Transform child in button.transform)
            {
                if(child.name == "Text")
                {
                    child.gameObject.SetActive(true);
                    continue;
                }
                if(child.name == "States")
                {
                    child.gameObject.SetActive(true);
                    foreach(Transform child1 in child)
                    {
                        child1.gameObject.SetActive(false);
                    }
                    continue;
                }
                child.gameObject.SetActive(false);
            }
        }
    }

    public static void UpdateButtons()
    {
        var pd = PlayerDataMod.instance;

        Reset();

        if (pd.bindings["Needle Binding"] && pd.bindings["Silk Binding"] &&
            pd.bindings["Tools Binding"] && pd.bindings["Mask Binding"])
        {
            foreach(var binding in new GameObject[]{needleButton, silkButton, toolsButton, maskButton})
            {
                binding.transform.Find("States/AllActivated").gameObject.SetActive(true);
            }
        }
        else
        {
            needleButton.transform.Find("States/Activated").gameObject.SetActive(pd.bindings["Needle Binding"]);
            silkButton.transform.Find("States/Activated").gameObject.SetActive(pd.bindings["Silk Binding"]);
            toolsButton.transform.Find("States/Activated").gameObject.SetActive(pd.bindings["Tools Binding"]);
            maskButton.transform.Find("States/Activated").gameObject.SetActive(pd.bindings["Mask Binding"]);

            needleButton.transform.Find("States/Deactivated").gameObject.SetActive(!pd.bindings["Needle Binding"]);
            silkButton.transform.Find("States/Deactivated").gameObject.SetActive(!pd.bindings["Silk Binding"]);
            toolsButton.transform.Find("States/Deactivated").gameObject.SetActive(!pd.bindings["Tools Binding"]);
            maskButton.transform.Find("States/Deactivated").gameObject.SetActive(!pd.bindings["Mask Binding"]);
        }
    }

    public static void ToggleBinding(GameObject bindingObj)
    {
        var playerData = PlayerDataMod.instance;
        var flashEffect = bindingObj.transform.Find("generic_flash_ui").gameObject;
        var states = bindingObj.transform.Find("States");

        if(playerData.bindings["Needle Binding"] && playerData.bindings["Silk Binding"] &&
           playerData.bindings["Tools Binding"] && playerData.bindings["Mask Binding"])
        {
            foreach(var binding in bindings)
            {
                var states1 = binding.transform.Find("States");
                var flashEffect1 = binding.transform.Find("generic_flash_ui").gameObject;

                flashEffect1.SetActive(false);
                flashEffect1.SetActive(true);

                foreach(Transform state in states1)
                {
                    if(state.name == "Activated") state.gameObject.SetActive(true);
                    else state.gameObject.SetActive(false);
                }
            }
        }

        if(bindingObj == needleButton)
        {
            var value = playerData.bindings["Needle Binding"];
            playerData.bindings["Needle Binding"] = !value;

            flashEffect.SetActive(false);
            flashEffect.SetActive(true);

            states.transform.Find("Deactivated").gameObject.SetActive(value);
            states.transform.Find("Activated").gameObject.SetActive(!value);
        }
        if(bindingObj == silkButton)
        {
            var value = playerData.bindings["Silk Binding"];
            playerData.bindings["Silk Binding"] = !value;

            flashEffect.SetActive(false);
            flashEffect.SetActive(true);

            states.transform.Find("Deactivated").gameObject.SetActive(value);
            states.transform.Find("Activated").gameObject.SetActive(!value);
        }
        if(bindingObj == toolsButton)
        {
            var value = playerData.bindings["Tools Binding"];
            playerData.bindings["Tools Binding"] = !value;

            flashEffect.SetActive(false);
            flashEffect.SetActive(true);

            states.transform.Find("Deactivated").gameObject.SetActive(value);
            states.transform.Find("Activated").gameObject.SetActive(!value);
        }
        if(bindingObj == maskButton)
        {
            var value = playerData.bindings["Mask Binding"];
            playerData.bindings["Mask Binding"] = !value;

            flashEffect.SetActive(false);
            flashEffect.SetActive(true);

            states.transform.Find("Deactivated").gameObject.SetActive(value);
            states.transform.Find("Activated").gameObject.SetActive(!value);
        }

        if(playerData.bindings["Needle Binding"] && playerData.bindings["Silk Binding"] &&
           playerData.bindings["Tools Binding"] && playerData.bindings["Mask Binding"])
        {
            foreach(var binding in bindings)
            {
                var states1 = binding.transform.Find("States");
                var flashEffect1 = binding.transform.Find("generic_flash_ui").gameObject;

                flashEffect1.SetActive(false);
                flashEffect1.SetActive(true);

                foreach(Transform state in states1)
                {
                    if(state.name == "AllActivated") state.gameObject.SetActive(true);
                    else state.gameObject.SetActive(false);
                }
            }
        }
        
        //to update hud
        GameCameras.instance.HUDOut();
        GameCameras.instance.HUDIn();
    }
}