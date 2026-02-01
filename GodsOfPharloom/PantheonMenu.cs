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
using System.Collections;

namespace Gods_Of_Pharloom;

public class PantheonMenu : MonoBehaviour
{
    public static PantheonMenu instance;
    public static GameObject pantheonMenu;
    public static List<GameObject> buttons;
    public static GameObject selectArrow;
    public static TMProOld.TextMeshPro pantheonName;
    public static GameObject currentButton;
    public static int currentButtonIndex = 0;
    public static GameObject needleButton;
    public static GameObject silkButton;
    public static GameObject toolsButton;
    public static GameObject maskButton;
    public static GameObject[] bindings;
    public static GameObject beginButton;
    public static AudioSource audioSource;
    public static AudioClip mainBindingsSoundSelect;
    public static AudioClip mainBindingsSoundFull;
    public bool isFlashUiInited = false;

    void Awake()
    {
        instance = this;
        
        this.StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        pantheonMenu = this.transform.Find("PantheonMenuCanvas").gameObject;
        selectArrow = pantheonMenu.transform.Find("select_arrow").gameObject;

        GameObject textTemplate = null;
        while (true)
        {
            textTemplate = GameObject.Find("_GameCameras/HudCamera/In-game/Inventory/Inv/Description Pane/Text Name");
            if(textTemplate != null) break;
            yield return null;
        }

        mainBindingsSoundSelect = (AudioClip)Preload.bundleResources["chain_cut"];
        mainBindingsSoundFull = (AudioClip)Preload.bundleResources["gg_radiant_binding_bling"];

        audioSource = this.gameObject.AddComponent<AudioSource>();
        audioSource.maxDistance = 9999f;
        audioSource.priority = 80;

        pantheonName = GameObject.Instantiate(textTemplate, parent: pantheonMenu.transform).GetComponent<TMProOld.TextMeshPro>();
        pantheonName.transform.position = new Vector3(-3.5129f, 5.3634f, -0.723f);

        var bindingsText = GameObject.Instantiate(textTemplate, parent: pantheonMenu.transform).GetComponent<TMProOld.TextMeshPro>();
        bindingsText.text = "Bindings";
        bindingsText.transform.position = new Vector3(-3.62f, 3.4192f, -0.7229f);

        var buttonsChildren = pantheonMenu.transform.Find("Buttons");
        buttons = new List<GameObject>();

        needleButton = buttonsChildren.Find("Needle Binding").gameObject;
        var needlePos = needleButton.transform.position;
        var needleBindingText = GameObject.Instantiate(textTemplate, parent: needleButton.transform).GetComponent<TMProOld.TextMeshPro>();
        needleBindingText.name = "Text";
        needleBindingText.text = "Needle";
        needleBindingText.transform.position = new Vector3(-3.0373f, 2.13f, 0);

        silkButton = buttonsChildren.Find("Silk Binding").gameObject;
        var silkPos = silkButton.transform.position;
        var silkBindingText = GameObject.Instantiate(textTemplate, parent: silkButton.transform).GetComponent<TMProOld.TextMeshPro>();
        silkBindingText.name = "Text";
        silkBindingText.text = "Silk";
        silkBindingText.transform.position = new Vector3(-3.0373f, 0.6357f, 0);

        toolsButton = buttonsChildren.Find("Tools Binding").gameObject;
        var toolsPos = toolsButton.transform.position;
        var toolsBindingText = GameObject.Instantiate(textTemplate, parent: toolsButton.transform).GetComponent<TMProOld.TextMeshPro>();
        toolsBindingText.name = "Text";
        toolsBindingText.text = "Tools";
        toolsBindingText.transform.position = new Vector3(-3.0373f, -0.793f, 0);

        maskButton = buttonsChildren.Find("Mask Binding").gameObject;
        var maskPos = maskButton.transform.position;
        var maskBindingText = GameObject.Instantiate(textTemplate, parent: maskButton.transform).GetComponent<TMProOld.TextMeshPro>();
        maskBindingText.name = "Text";
        maskBindingText.text = "Shell";
        maskBindingText.transform.position = new Vector3(-3.0373f, -2.38f, 0);

        beginButton = buttonsChildren.Find("Begin").gameObject;
        var beginPos = beginButton.transform.position;
        var beginText = GameObject.Instantiate(textTemplate, parent: beginButton.transform).GetComponent<TMProOld.TextMeshPro>();
        beginText.name = "Text";
        beginText.text = "BEGIN";
        beginText.transform.position = new Vector3(-3.55f, -4.05f, -0.723f);

        bindings = new GameObject[]{needleButton, silkButton, toolsButton, maskButton};

        buttons = new List<GameObject>{needleButton, silkButton, toolsButton, maskButton, beginButton};

        currentButton = buttons[currentButtonIndex];

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

        audioSource.PlayOneShot(mainBindingsSoundSelect);

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

            audioSource.PlayOneShot(mainBindingsSoundFull);
        }
        
        //to update hud
        GameCameras.instance.HUDOut();
        GameCameras.instance.HUDIn();

        GodsOfPharloomMod.instance.SaveModData();
    }
}