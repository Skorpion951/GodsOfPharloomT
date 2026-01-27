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
using UniverseLib;
using UnityExplorer.CacheObject;
using UnityExplorer.CacheObject.Views;
using TeamCherry.NestedFadeGroup;
using Mono.Posix;
using UniverseLib.Utility;

namespace Gods_Of_Pharloom;

public class BindingsMenu
{
    public static GameObject menuBindings;
    public static Fsm menuBindingsFsm;
    public static Fsm menuBindingsInvProxyFsm;
    public static string[] menuBindingsInvProxyFsmStatesHistory = new string[50];
    public static GameObject customBrokenSpool;
    public static TMProOld.TextMeshPro textName;
    public static TMProOld.TextMeshPro textDesc;
    public static InventoryItemCollectable needleBinding;
    public static InventoryItemCollectable silkBinding;
    public static InventoryItemCollectable toolsBinding;
    public static InventoryItemCollectable maskBinding;
    public static InventoryItemNail needle;
    public static NestedFadeGroup toolsMsgWhileBindingEffect;
    public static NestedFadeGroup bindingsMsgWhileInSequence;
    public static int maskBindingCount = 5;
    public static bool isHealthIncreasing = false;
    public static InventoryItemHeartPieces heartPieces;
    public static InventoryItemSpoolPieces spoolPieces;
    public static InventoryItemConditional sprint;
    public static InventoryItemConditional harpoonDash;
    public static InventoryItemConditional evaHeal;
    public static InventoryItemConditional superJump;
    public static InventoryItemConditional wallJump;
    public static InventoryItemConditional needolin;
    public static InventoryItemSpool silkHeartsSpool;
    public static InventoryItemCollectable cloakStates;
    public static string[] cloakLables = new string[]
    {
        "Hunter's Cloak",
        "Drifter’s Cloak",
        "Faydown Cloak",
        "Faydown Cloak",
    };
    public static string[] cloakDescriptions = new string[]
    {
        "Simple protective garb, expertly woven but showing signs of age.",
        "Simple protective garb, sewn through with flexible spines.",
        "Protective garb lined with the soft down of a Fayforn.",
        "Protective garb lined with the soft down of a Fayforn and sewn through with flexible spines.",
    };
    
    public static Dictionary<string, List<string>> crestsPreviousState = new Dictionary<string, List<string>>();
    public static Dictionary<string, Action<bool>> submitActions = new Dictionary<string, Action<bool>>
    {
        {"Tools Buttons Msg", (_) =>
        {
            GodsOfPharloomMod.instance.StartCoroutine(TryFadeMsg(toolsMsgWhileBindingEffect));
        }},
        {"Bindings Buttons Msg", (_) =>
        {
            GodsOfPharloomMod.instance.StartCoroutine(TryFadeMsg(bindingsMsgWhileInSequence));
        }},
        {"Needle Binding", (_) =>
        {
            ToggleBinding(needleBinding.gameObject);

            UpdateMenuBindingsDisplay();
        }},
        {"Silk Binding", (_) =>
        {
            ToggleBinding(silkBinding.gameObject);

            UpdateMenuBindingsDisplay();
        }},
        {"Tools Binding", (_) =>
        {
            ToggleBinding(toolsBinding.gameObject);

            UpdateMenuBindingsDisplay();
        }},
        {"Mask Binding", (_) =>
        {
            ToggleBinding(maskBinding.gameObject);

            UpdateMenuBindingsDisplay();
        }},
        {"Needle", (_) =>
        {
            var pd = PlayerData.instance;
            if(pd == null) return;
            pd.nailUpgrades = ((pd.nailUpgrades + 1) < 5) ? pd.nailUpgrades + 1 : 0;
            
            UpdateMenuBindingsDisplay();
        }},
        {"Heart Pieces", (_) =>
        {
            var pd = PlayerData.instance;
            var pdm = PlayerDataMod.instance;
            if(pd == null || pdm == null) return;

            var newHeroHealth = pd.maxHealth + 1;
            newHeroHealth = (newHeroHealth > 10) ? 1 : newHeroHealth;

            if(!pdm.bindings["Mask Binding"])
            {
                GodsOfPharloomMod.instance.StartCoroutine(TrySetHeroHealth(newHeroHealth));
            }
            else
            {
                newHeroHealth = (newHeroHealth > maskBindingCount) ? 1 : newHeroHealth;
                GodsOfPharloomMod.instance.StartCoroutine(TrySetHeroHealth(newHeroHealth));
                pdm.previousHealthCount = newHeroHealth;
            }

            UpdateMenuBindingsDisplay();
        }},
        {"Spool Pieces", (_) =>
        {
            var pd = PlayerData.instance;
            var pdm = PlayerDataMod.instance;
            if(pd == null || pdm == null) return;

            var newMaxSilk = (pd.silkMax < 18) ? pd.silkMax + 1 : 0;
            newMaxSilk = (newMaxSilk > 9 && pdm.bindings["Silk Binding"]) ? 0 : newMaxSilk;
            pd.silkMax = newMaxSilk;

            UpdateMenuBindingsDisplay();
        }},
        {"Sprint", (bool onlyUpdateDisplaying) =>
        {
            var pd = PlayerData.instance;
            if(pd == null) return;

            if(!onlyUpdateDisplaying) pd.hasDash = !pd.hasDash;
            
            UpdateMenuBindingsDisplay();
        }},
        {"Harpoon Dash", (bool onlyUpdateDisplaying) =>
        {
            var pd = PlayerData.instance;
            if(pd == null) return;

            if(!onlyUpdateDisplaying) pd.hasHarpoonDash = !pd.hasHarpoonDash;
            
            UpdateMenuBindingsDisplay();
        }},
        {"Eva Heal", (bool onlyUpdateDisplaying) =>
        {
            var pd = PlayerData.instance;
            if(pd == null) return;

            if(!onlyUpdateDisplaying) pd.HasBoundCrestUpgrader = !pd.HasBoundCrestUpgrader;
            
            UpdateMenuBindingsDisplay();
        }},
        {"Super Jump", (bool onlyUpdateDisplaying) =>
        {
            var pd = PlayerData.instance;
            if(pd == null) return;

            if(!onlyUpdateDisplaying) pd.hasSuperJump = !pd.hasSuperJump;
            
            UpdateMenuBindingsDisplay();
        }},
        {"Wall Jump", (bool onlyUpdateDisplaying) =>
        {
            var pd = PlayerData.instance;
            if(pd == null) return;

            if(!onlyUpdateDisplaying) pd.hasWalljump = !pd.hasWalljump;
            
            UpdateMenuBindingsDisplay();
        }},
        {"Needolin", (bool onlyUpdateDisplaying) =>
        {
            var pd = PlayerData.instance;
            if(pd == null) return;

            if(!onlyUpdateDisplaying) pd.hasNeedolin = !pd.hasNeedolin;
            
            UpdateMenuBindingsDisplay();
        }},
        {"Spool", (_) =>
        {
            var pd = PlayerData.instance;
            if(pd == null) return;

            var newSilkRegenCount = (pd.silkRegenMax < 3) ? pd.silkRegenMax + 1 : 0;
            pd.silkRegenMax = newSilkRegenCount;
        }},
        {"Cloak States", (bool onlyUpdateDisplaying) =>
        {
            var pd = PlayerData.instance;
            if(pd == null) return;

            int cloakState = 0;
            if(!pd.hasBrolly && !pd.hasDoubleJump) cloakState = 0;
            else if(pd.hasBrolly && !pd.hasDoubleJump) cloakState = 1;
            else if(!pd.hasBrolly && pd.hasDoubleJump) cloakState = 2;
            else if(pd.hasBrolly && pd.hasDoubleJump) cloakState = 3;

            if (!onlyUpdateDisplaying)
            {
                cloakState = (cloakState < 3) ? cloakState + 1 : 0;
                if(cloakState == 0) {pd.hasBrolly = false; pd.hasDoubleJump = false;}
                else if(cloakState == 1) {pd.hasBrolly = true; pd.hasDoubleJump = false;}
                else if(cloakState == 2) {pd.hasBrolly = false; pd.hasDoubleJump = true;}
                else if(cloakState == 3) {pd.hasBrolly = true; pd.hasDoubleJump = true;}

                textName.text = cloakLables[cloakState];
                textDesc.text = cloakDescriptions[cloakState];
            }

            UpdateMenuBindingsDisplay();
        }},
    };

    public static IEnumerator UpdateSilkSpool()
    {
        GameCameras gc = GameCameras.instance;
        Transform spoolParent = null;
        PlayerData pd = null;
        Coroutine silkUpdater = null;
        int silkAmountStart = 9;

        while (true)
        {
            while (true)
            {
                if(!gc.IsNullOrDestroyed()) break;

                gc = GameCameras.instance;
                if(gc.IsNullOrDestroyed()){
                    yield return null;
                    continue;
                }
                else break;
            }
            while (true)
            {
                if(!pd.IsNullOrDestroyed()) break;

                pd = PlayerData.instance;
                if(pd.IsNullOrDestroyed()){
                    yield return null;
                    continue;
                }
                else break;
            }
            while(true)
            {
                if(!spoolParent.IsNullOrDestroyed()) break;

                try
                {
                    spoolParent = gc.hudCanvasSlideOut.gameObject.transform.Find("Thread/Spool/Thread Spool/Parent");
                }
                catch(Exception ex){GodsOfPharloomMod.Log.LogInfo("UpdateSilkSpool_Method:\n" + ex.Message);}

                if(spoolParent.IsNullOrDestroyed()){
                    yield return null;
                    continue;
                }
                else break;
            }

            if(customBrokenSpool.IsNullOrDestroyed() || customBrokenSpool.transform.parent != spoolParent)
            {
                if(!customBrokenSpool.IsNullOrDestroyed()) GameObject.Destroy(customBrokenSpool);

                var prefab = (GameObject)Preload.bundleResources["CustomBrokenSilkSpool"];
                customBrokenSpool = GameObject.Instantiate(prefab, parent: spoolParent);
                customBrokenSpool.transform.localScale = prefab.transform.localScale;
                customBrokenSpool.transform.position = prefab.transform.position;

                customBrokenSpool.SetActive(false);

                Material brokenSpoolMaterial = customBrokenSpool.GetComponent<MeshRenderer>().material;

                IEnumerator UpdateBrokenSpoolMaterial()
                {
                    // float alpha;
                    while (true)
                    {
                        // alpha = (pd.silkMax <= silkAmountStart) ? (1 - ScreenFaderState.Alpha) : 0;

                        // brokenSpoolMaterial.SetFloat("_Alpha", alpha);
                        brokenSpoolMaterial.SetInt("_SilkSpoolSegAmount", pd.silkMax);

                        yield return null;
                    }
                }

                if(silkUpdater.IsNullOrDestroyed()) silkUpdater = GodsOfPharloomMod.instance.StartCoroutine(UpdateBrokenSpoolMaterial());
            }

            if(pd.silkMax <= silkAmountStart)
            {
                while(pd.silkMax <= silkAmountStart)
                {
                    customBrokenSpool.SetActive(true);

                    spoolParent.Find("Active").gameObject.SetActive(false);
                    spoolParent.Find("Broken").gameObject.SetActive(false);
                    spoolParent.Find("Bind Notch").gameObject.SetActive(false);

                    yield return null;
                }
                customBrokenSpool.SetActive(false);
            }
            

            yield return null;
        }
    }
    public static IEnumerator UpdateSilkBinding()
    {
        while (true)
        {
            if(PlayerDataMod.instance.bindings["Silk Binding"])
            {
                PlayerData pd = null;
                while(pd.IsNullOrDestroyed())
                {
                    pd = PlayerData.instance;
                    if(!pd.IsNullOrDestroyed()) break;
                    yield return null;
                }
                while(PlayerDataMod.instance.bindings["Silk Binding"])
                {
                    pd.silkMax = (pd.silkMax > 9) ? 9 : pd.silkMax;
                    yield return null;
                }
            }

            PlayerData.instance.IsSilkSpoolBroken = false;

            yield return null;
        }
    }
    public static IEnumerator UpdateToolsBinding()
    {
        while (true)
        {
            if(PlayerDataMod.instance.bindings["Tools Binding"])
            {
                if (!TryActivateToolsBinding())
                {
                    yield return null;
                    continue;
                }

                while(PlayerDataMod.instance.bindings["Tools Binding"])
                {
                    yield return null;
                }

                TryActivateToolsBinding();
            }

            yield return null;
        }
    }
    public static IEnumerator UpdateMaskBinding()
    {
        var pd = PlayerData.instance;
        var pdm = PlayerDataMod.instance;

        while (true)
        {
            if(pdm.bindings["Mask Binding"])
            {
                pdm.previousHealthCount = pd.maxHealth;

                var newHealth = (pd.maxHealth > maskBindingCount) ? maskBindingCount : pd.maxHealth;

                GodsOfPharloomMod.instance.StartCoroutine(TrySetHeroHealth(newHealth));

                while(pdm.bindings["Mask Binding"]) yield return null;

                GodsOfPharloomMod.instance.StartCoroutine(TrySetHeroHealth(pdm.previousHealthCount));
            }

            yield return null;
        }
    }

    public static IEnumerator TrySetHeroHealth(int amount)
    {
        isHealthIncreasing = true;

        PlayerData.instance.maxHealth = amount;
        PlayerData.instance.maxHealthBase = amount;
        PlayerData.instance.health = amount;

        var healthFsms = new List<PlayMakerFSM>();
        var healthsToActivate = new List<PlayMakerFSM>();
        var hudCanvas = GameCameras.instance.hudCanvasSlideOut.transform;

        var counter = 0;

        foreach(Transform child in hudCanvas.Find("Health"))
        {
            if(child.name == "Health 1" || child.name == "Health 2+(Clone)")
            {
                var fsm = child.gameObject.LocateMyFSM("health_display");
                healthFsms.Add(fsm);

                if(counter < amount)
                {
                    healthsToActivate.Add(fsm);
                    counter++;
                }

                fsm.enabled = true;
                fsm.Fsm.SetState("Inactive");
            }
        }

        foreach(var health in healthsToActivate)
        {
            health.enabled = true;
            health.Fsm.SetState("Idle Enter");
        }

        while (true)
        {
            bool allActivated = false;

            for(int i = 0; i < healthsToActivate.Count; i++)
            {
                var fsm = healthsToActivate[i].Fsm;
                if(fsm.FsmComponent.enabled || fsm.ActiveStateName != "Idle") break;

                if(i == healthsToActivate.Count - 1) allActivated = true;
            }

            if(allActivated) break;
            else yield return null;
        }

        UpdateMenuBindingsDisplay();

        isHealthIncreasing = false;
    }

    public static IEnumerator TryFadeMsg(NestedFadeGroup fadeComp, float fadeTime = 0.15f)
    {
        GodsOfPharloomMod.Log.LogInfo("Started try fade msg");
        if(fadeComp.AlphaSelf > 0f) yield break;
        GodsOfPharloomMod.Log.LogInfo("Continued try fade msg");

        var timer = 0f;

        while(fadeComp.AlphaSelf < 1f)
        {
            timer += Time.unscaledDeltaTime;
            fadeComp.AlphaSelf = (timer / fadeTime > 1) ? 1 : timer / fadeTime;

            yield return null;
        }
        GodsOfPharloomMod.Log.LogInfo("fade msg 1");

        while (true)
        {
            var inputHandler = InputHandler.Instance.inputActions;

            if(inputHandler.Jump.WasPressed || inputHandler.Up.WasPressed || inputHandler.Down.WasPressed ||
               inputHandler.Left.WasPressed || inputHandler.Right.WasPressed) break;

            yield return null;
        }
        GodsOfPharloomMod.Log.LogInfo("fade msg 2");

        timer = 0;
        while(fadeComp.AlphaSelf > 0f)
        {
            timer += Time.unscaledDeltaTime;
            fadeComp.AlphaSelf = ((1 - timer / fadeTime) < 0) ? 0 : (1 - timer / fadeTime);

            yield return null;
        }
        GodsOfPharloomMod.Log.LogInfo("End fade msg");
    }
    public static bool TryActivateToolsBinding()
    {
        try
        {
            var inventory = GameObject.Find("_GameCameras/HudCamera/In-game/Inventory");
            var additiveDefendSlot = inventory.transform.Find("Tools/Tool Group/Floating Slots/Defend Slot").gameObject.GetComponent<InventoryToolCrestSlot>();
            var additiveExploreSlot = inventory.transform.Find("Tools/Tool Group/Floating Slots/Explore Slot").gameObject.GetComponent<InventoryToolCrestSlot>();

            if(PlayerDataMod.instance.bindings["Tools Binding"])
            {
                foreach(var crest in ToolItemManager.GetAllCrests())
                {
                    if(crest == null) continue;
                    var crestSlots = PlayerData.instance.ToolEquips.GetData(crest.name).Slots;
                    if(crestSlots == null) continue;
                    
                    var crestPreviousState = new List<string>();

                    var newCrestState = new List<string>();

                    foreach(var slot in crestSlots)
                    {
                        var tool = ToolItemManager.GetToolByName(slot.EquippedTool);
                        if(tool == null)
                        {
                            crestPreviousState.Add("");
                            newCrestState.Add("");
                        }
                        else
                        {
                            var name = tool.name != null ? tool.name : "";
                            crestPreviousState.Add(name);
                            if(tool.Type == ToolItemType.Skill)
                            {
                                newCrestState.Add(name);
                            }
                            else newCrestState.Add("");
                        }
                    }

                    crestsPreviousState[crest.name] = crestPreviousState;

                    ToolItemManager.SetEquippedTools(crest.name, newCrestState);
                }

                crestsPreviousState[additiveDefendSlot.name] = new List<string>{additiveDefendSlot.SaveData.EquippedTool};
                crestsPreviousState[additiveExploreSlot.name] = new List<string>{additiveExploreSlot.SaveData.EquippedTool};
                ToolItemManager.SetExtraEquippedTool("Defend1", "");
                ToolItemManager.SetExtraEquippedTool("Explore1", "");

                ToolItemManager.SendEquippedChangedEvent();

                return true;
            }
            else
            {
                foreach(var crestState in crestsPreviousState)
                {
                    if(crestState.Key == additiveDefendSlot.name)
                    {
                        ToolItemManager.SetExtraEquippedTool("Defend1", crestState.Value[0]);
                        continue;
                    }
                    if(crestState.Key == additiveExploreSlot.gameObject.name)
                    {
                        ToolItemManager.SetExtraEquippedTool("Explore1", crestState.Value[0]);
                        continue;
                    }
                    ToolItemManager.SetEquippedTools(crestState.Key, crestState.Value);
                }
                crestsPreviousState = new Dictionary<string, List<string>>();

                ToolItemManager.SendEquippedChangedEvent();

                return true;
            }
        }
        catch(Exception ex)
        {
            return false;
        }
    }
    public static void ToggleBinding(GameObject bindingObj)
    {
        var playerData = PlayerDataMod.instance;
        var icons = bindingObj.transform.Find("Group/Parent");
        var flashEffect = bindingObj.transform.Find("Group/generic_flash_ui").gameObject;
        var bindings = new GameObject[]{needleBinding.gameObject, silkBinding.gameObject,
                                            toolsBinding.gameObject, maskBinding.gameObject};
        

        if(playerData.bindings["Needle Binding"] && playerData.bindings["Silk Binding"] &&
           playerData.bindings["Tools Binding"] && playerData.bindings["Mask Binding"])
        {
            foreach(var binding in bindings)
            {
                var icons1 = binding.transform.Find("Group/Parent");
                var flashEffect1 = binding.transform.Find("Group/generic_flash_ui").gameObject;

                flashEffect1.SetActive(false);
                flashEffect1.SetActive(true);

                foreach(Transform icon in icons1)
                {
                    if(icon.name == "Activated") icon.gameObject.SetActive(true);
                    else icon.gameObject.SetActive(false);
                }
            }
        }

        if(bindingObj.name == "Needle Binding")
        {
            var value = playerData.bindings["Needle Binding"];
            playerData.bindings["Needle Binding"] = !value;

            flashEffect.SetActive(false);
            flashEffect.SetActive(true);

            icons.Find("Deactivated").gameObject.SetActive(value);
            icons.Find("Activated").gameObject.SetActive(!value);
        }
        if(bindingObj.name == "Silk Binding")
        {
            var value = playerData.bindings["Silk Binding"];
            playerData.bindings["Silk Binding"] = !value;

            flashEffect.SetActive(false);
            flashEffect.SetActive(true);

            icons.Find("Deactivated").gameObject.SetActive(value);
            icons.Find("Activated").gameObject.SetActive(!value);
        }
        if(bindingObj.name == "Tools Binding")
        {
            var value = playerData.bindings["Tools Binding"];
            playerData.bindings["Tools Binding"] = !value;

            flashEffect.SetActive(false);
            flashEffect.SetActive(true);

            icons.Find("Deactivated").gameObject.SetActive(value);
            icons.Find("Activated").gameObject.SetActive(!value);
        }
        if(bindingObj.name == "Mask Binding")
        {
            var value = playerData.bindings["Mask Binding"];
            playerData.bindings["Mask Binding"] = !value;

            flashEffect.SetActive(false);
            flashEffect.SetActive(true);

            icons.Find("Deactivated").gameObject.SetActive(value);
            icons.Find("Activated").gameObject.SetActive(!value);
        }

        if(playerData.bindings["Needle Binding"] && playerData.bindings["Silk Binding"] &&
           playerData.bindings["Tools Binding"] && playerData.bindings["Mask Binding"])
        {
            foreach(var binding in bindings)
            {
                var icons1 = binding.transform.Find("Group/Parent");
                var flashEffect1 = binding.transform.Find("Group/generic_flash_ui").gameObject;

                flashEffect1.SetActive(false);
                flashEffect1.SetActive(true);

                binding.GetComponent<InventoryItemCollectable>().PlayConsumeEffect();

                foreach(Transform icon in icons1)
                {
                    if(icon.name == "AllActivated") icon.gameObject.SetActive(true);
                    else icon.gameObject.SetActive(false);
                }
            }
        }
    }

    public static void InitBindingsMenuFsmHistory()
    {
        IEnumerator enumerator()
        {
            while (true)
            {
                while(menuBindingsInvProxyFsm != null && menuBindingsInvProxyFsm.ActiveStateName == menuBindingsInvProxyFsmStatesHistory[0]) yield return null;

                if(menuBindingsInvProxyFsm == null)
                {
                    yield return null;
                    continue;
                }

                for(int i = menuBindingsInvProxyFsmStatesHistory.Length - 1; i > 0; i--)
                {
                    menuBindingsInvProxyFsmStatesHistory[i] = menuBindingsInvProxyFsmStatesHistory[i - 1];
                }

                menuBindingsInvProxyFsmStatesHistory[0] = menuBindingsInvProxyFsm.ActiveStateName;

                yield return null;
            }
        }

        GodsOfPharloomMod.instance.StartCoroutine(enumerator());
    }

    public static void InitBindingsMenu()
    {
        if(menuBindings != null) GameObject.Destroy(menuBindings);
        var inventoryOrig = GameObject.Find("_GameCameras/HudCamera/In-game/Inventory");
        menuBindings = GameObject.Instantiate(inventoryOrig);
        GameObject.DontDestroyOnLoad(menuBindings);
        menuBindings.name = "Bindings Menu";
        menuBindingsFsm = menuBindings.GetComponent<PlayMakerFSM>().Fsm;

        menuBindingsFsm.FsmComponent.StartCoroutine(IInitBindingsMenu());

        menuBindingsFsm.FsmComponent.StartCoroutine(UpdateSilkBinding());

        menuBindingsFsm.FsmComponent.StartCoroutine(UpdateToolsBinding());
        
        menuBindingsFsm.FsmComponent.StartCoroutine(UpdateMaskBinding());
    }
    public static void UpdateMenuBindingsDisplay()
    {
        var pd = PlayerData.instance;
        var pdm = PlayerDataMod.instance;

        //hero health
        if(heartPieces != null) heartPieces.transform.Find("Amount Text").gameObject.GetComponent<TMProOld.TextMeshPro>().text = $"{PlayerData.instance.maxHealth}";

        //spool pieces
        var amountText = spoolPieces.transform.Find("Amount Text").gameObject.GetComponent<TMProOld.TextMeshPro>();
        amountText.text = $"{pd.silkMax}";

        //needle state
        needle.GetType().GetMethod("UpdateState", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(needle, null);
        needle.GetType().GetMethod("UpdateDisplay", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(needle, null);

        //sprint
        float conditionVal = pd.hasDash ? 0f : 0.5f;
        var color = new Color(1f-conditionVal, 1f-conditionVal, 1f-conditionVal, 1);
        sprint.GetComponent<SpriteRenderer>().color = color;

        //harpoon dash
        conditionVal = pd.hasHarpoonDash ? 0f : 0.5f;
        color = new Color(1f-conditionVal, 1f-conditionVal, 1f-conditionVal, 1);
        harpoonDash.GetComponent<SpriteRenderer>().color = color;

        //eva heal
        conditionVal = pd.HasBoundCrestUpgrader ? 0f : 0.5f;
        color = new Color(1f-conditionVal, 1f-conditionVal, 1f-conditionVal, 1);
        evaHeal.GetComponent<SpriteRenderer>().color = color;

        //super jump
        conditionVal = pd.hasSuperJump ? 0f : 0.5f;
        color = new Color(1f-conditionVal, 1f-conditionVal, 1f-conditionVal, 1);
        superJump.GetComponent<SpriteRenderer>().color = color;

        //wall jump
        conditionVal = pd.hasWalljump ? 0f : 0.5f;
        color = new Color(1f-conditionVal, 1f-conditionVal, 1f-conditionVal, 1);
        wallJump.GetComponent<SpriteRenderer>().color = color;

        //needolin
        conditionVal = pd.hasNeedolin ? 0f : 0.5f;
        color = new Color(1f-conditionVal, 1f-conditionVal, 1f-conditionVal, 1);
        needolin.GetComponent<SpriteRenderer>().color = color;

        //cloak
        int cloakState = 0;
        if(!pd.hasBrolly && !pd.hasDoubleJump) cloakState = 0;
        else if(pd.hasBrolly && !pd.hasDoubleJump) cloakState = 1;
        else if(!pd.hasBrolly && pd.hasDoubleJump) cloakState = 2;
        else if(pd.hasBrolly && pd.hasDoubleJump) cloakState = 3;
        foreach(Transform child in cloakStates.transform) child.gameObject.SetActive(false);
        cloakStates.transform.Find($"CloakState_{cloakState}")?.gameObject.SetActive(true);

        //4 main bindings
        var bindings = new InventoryItemCollectable[]{needleBinding, silkBinding, toolsBinding, maskBinding};
        foreach(var binding in bindings)
        {
            foreach(Transform icon in binding.transform.Find("Group/Parent")) icon.gameObject.SetActive(false);
        }

        if(pdm.bindings["Needle Binding"] && pdm.bindings["Silk Binding"] &&
           pdm.bindings["Tools Binding"] && pdm.bindings["Mask Binding"])
        {
            foreach(var binding in bindings)
            {
                binding.transform.Find("Group/Parent/AllActivated").gameObject.SetActive(true);
            }
        }
        else
        {
            foreach(var binding in bindings)
            {
                bool isActive = false;

                if(binding == needleBinding)
                {
                    isActive = pdm.bindings["Needle Binding"];
                }
                else if(binding == silkBinding)
                {
                    isActive = pdm.bindings["Silk Binding"];
                }
                else if(binding == toolsBinding)
                {
                    isActive = pdm.bindings["Tools Binding"];
                }
                else if(binding == maskBinding)
                {
                    isActive = pdm.bindings["Mask Binding"];
                }

                binding.transform.Find("Group/Parent/Activated").gameObject.SetActive(isActive);
                binding.transform.Find("Group/Parent/Deactivated").gameObject.SetActive(!isActive);
            }
        }
    }
    public static InventoryItemCollectable CreateButtonInv(string objName, GameObject template, Vector3 pos, string textLable = "", string textDescription = "")
    {
        var button = GameObject.Instantiate(template, parent: menuBindings.transform);
        button.name = objName;
        button.transform.position = pos;

        button.transform.Find("Group/Amount Text").gameObject.SetActive(false);
        button.transform.Find("Group/New Item Orb").gameObject.SetActive(false);
        button.transform.Find("Group/generic_flash_ui").gameObject.SetActive(false);

        var invItemCollect = button.GetComponent<InventoryItemCollectable>();

        invItemCollect.GetType().GetField("buttonPromptDisplay", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(invItemCollect, null);
        invItemCollect.GetType().GetField("consumePrompt", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(invItemCollect, null);
        
        if(textLable != "")
        {
            invItemCollect.OnSelected += (_) =>
            {
                textName.text = textLable;
            };
        }
        if(textDescription != "")
        {
            invItemCollect.OnSelected += (_) =>
            {
                textDesc.text = textDescription;
            };
        }

        button.SetActive(true);

        return invItemCollect;
    }
    public static IEnumerator IInitBindingsMenu()
    {
        var inventoryOrig = GameObject.Find("_GameCameras/HudCamera/In-game/Inventory");
        
        var closed = menuBindingsFsm.GetState("Closed");
        closed.Transitions = new FsmTransition[0];

        yield return null;
        yield return null;
        
        var border = menuBindings.transform.Find("Border").gameObject;
        foreach(Transform item in border.transform.Find("PaneListDisplay"))
        {
            item.gameObject.SetActive(false);
        }
        foreach(Transform item in border.transform.Find("Arrows"))
        {
            item.gameObject.SetActive(false);
        }
        

        var inv = menuBindings.transform.Find("Inv").gameObject;
        foreach(var fsmComp in inv.GetComponents<PlayMakerFSM>())
        {
            if(fsmComp.FsmName == "Inventory Proxy")
            {
                menuBindingsInvProxyFsm = fsmComp.Fsm;
                break;
            }
        }
        var invPane = inv.GetComponent<InventoryPane>();
        invPane.GetType().BaseType.GetField("OnPaneStart", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(invPane, null);

        var paneList = menuBindings.GetComponent<InventoryPaneList>();
        paneList.GetType().GetField("panes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(paneList, new InventoryPane[]{invPane});

        spoolPieces = inv.transform.Find("Inv_Items/Needle Shift/Spool Pieces").gameObject.GetComponent<InventoryItemSpoolPieces>();
        spoolPieces.gameObject.SetActive(true);
        heartPieces = inv.transform.Find("Inv_Items/Needle Shift/Heart Pieces").gameObject.GetComponent<InventoryItemHeartPieces>();
        heartPieces.gameObject.SetActive(true);

        inv.transform.Find("Inv_Items/Needle Shift/Spool Group").position = new Vector3(-8.42f, -3.479f, 38.18f);

        silkHeartsSpool = inv.transform.Find("Inv_Items/Needle Shift/Spool Group/Spool").gameObject.GetComponent<InventoryItemSpool>();
        silkHeartsSpool.gameObject.SetActive(true);
        silkHeartsSpool.transform.Find("New Item Orb")?.gameObject.SetActive(false);

        foreach(Transform child in inv.transform.Find("Inv_Items/Needle Shift/Spool Group/Radial Layout"))
        {
            child.gameObject.SetActive(true);
            if(child.name == "Sprint") sprint = child.gameObject.GetComponent<InventoryItemConditional>();
            if(child.name == "Harpoon Dash") harpoonDash = child.gameObject.GetComponent<InventoryItemConditional>();
            if(child.name == "Eva Heal") evaHeal = child.gameObject.GetComponent<InventoryItemConditional>();
            if(child.name == "Super Jump") superJump = child.gameObject.GetComponent<InventoryItemConditional>();
            if(child.name == "Wall Jump") wallJump = child.gameObject.GetComponent<InventoryItemConditional>();
            if(child.name == "Needolin") needolin = child.gameObject.GetComponent<InventoryItemConditional>();
            var newItemOrb = child.Find("New Item Orb");
            if(newItemOrb) newItemOrb.gameObject.SetActive(false);
        }

        inv.transform.Find("Inv_Items/Needle Shift/Geo").gameObject.SetActive(false);
        inv.transform.Find("Inv_Items/Needle Shift/Shards").gameObject.SetActive(false);

        textName = inv.transform.Find("Description Pane/Text Name").gameObject.GetComponent<TMProOld.TextMeshPro>();
        textDesc = inv.transform.Find("Description Pane/Text Desc").gameObject.GetComponent<TMProOld.TextMeshPro>();

        var equipment = inv.transform.Find("Equipment").gameObject;
        var template = equipment.transform.Find("Template Collectable Item").gameObject;
        var flashUI_template = template.transform.Find("Group/generic_flash_ui");
        Preload.preloads["generic_flash_ui"] = GameObject.Instantiate(flashUI_template.gameObject, parent: Preload.handler.transform);

        needle = inv.transform.Find("Inv_Items/Needle").GetComponent<InventoryItemNail>();

        foreach(Transform item in equipment.transform)
        {
            item.gameObject.SetActive(false);
        }

        needle.Selectables[2] = needle;

        toolsMsgWhileBindingEffect = GameObject.Instantiate(inv.transform.Find("Memory Use Msg").gameObject, parent: inventoryOrig.transform.Find("Tools")).GetComponent<NestedFadeGroup>();
        toolsMsgWhileBindingEffect.gameObject.SetActive(true);
        toolsMsgWhileBindingEffect.name = "ToolsMsgWhileBindingEffect";
        var toolsMsgWhileBindingEffectText = toolsMsgWhileBindingEffect.transform.Find("Text").gameObject.GetComponent<TMProOld.TextMeshPro>();
        GameObject.Destroy(toolsMsgWhileBindingEffectText.gameObject.GetComponent<SetTextMeshProGameText>());
        toolsMsgWhileBindingEffectText.text = "Can't equip tools while Tools Binding is active.";
        inventoryOrig.transform.Find("Tools").gameObject.GetComponent<InventoryPane>().OnPaneStart += () => {toolsMsgWhileBindingEffect.AlphaSelf = 0;};

        bindingsMsgWhileInSequence = GameObject.Instantiate(inv.transform.Find("Memory Use Msg").gameObject, parent: inv.transform).GetComponent<NestedFadeGroup>();
        bindingsMsgWhileInSequence.gameObject.SetActive(true);
        bindingsMsgWhileInSequence.name = "BindingsMsgWhileInSequence";
        var bindingsMsgWhileInSequenceText = bindingsMsgWhileInSequence.transform.Find("Text").gameObject.GetComponent<TMProOld.TextMeshPro>();
        GameObject.Destroy(bindingsMsgWhileInSequenceText.gameObject.GetComponent<SetTextMeshProGameText>());
        bindingsMsgWhileInSequenceText.text = "Can't toggle bindings while in boss sequence.";
        invPane.OnPaneStart += () => {bindingsMsgWhileInSequence.AlphaSelf = 0;};

        needleBinding = CreateButtonInv("Needle Binding", template, new Vector3(-4.1f, 4.195f, 4.3f), "Needle Binding", "Reduces needle damage.");
        silkBinding = CreateButtonInv("Silk Binding", template, new Vector3(-2f, 4.195f, 4.3f), "Silk Binding", "Makes silk spool broken.");
        toolsBinding = CreateButtonInv("Tools Binding", template, new Vector3(0f, 4.195f, 4.3f), "Tools Binding", "Removes tools.");
        maskBinding = CreateButtonInv("Mask Binding", template, new Vector3(2f, 4.195f, 4.3f), "Mask Binding", "Reduces the number of masks to 5.");

        // 0 - up, 1 - down, 2 - left, 3 - right
        needleBinding.Selectables = new InventoryItemSelectable[4]{null, null, spoolPieces, silkBinding};
        silkBinding.Selectables = new InventoryItemSelectable[4]{null, null, needleBinding, toolsBinding};
        toolsBinding.Selectables = new InventoryItemSelectable[4]{null, null, silkBinding, maskBinding};
        maskBinding.Selectables = new InventoryItemSelectable[4]{null, null, toolsBinding, maskBinding};

        var grid = equipment.GetComponent<InventoryItemGrid>();
        List<InventoryItemSelectableDirectional> gridCollection = new List<InventoryItemSelectableDirectional>();
        var gridCollectionOld = ((List<InventoryItemGrid.GridSection>)grid.GetType().GetField("collections", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(grid));

        gridCollection.Add(needleBinding.GetComponent<InventoryItemCollectable>());
        gridCollection.Add(silkBinding.GetComponent<InventoryItemCollectable>());
        gridCollection.Add(toolsBinding.GetComponent<InventoryItemCollectable>());
        gridCollection.Add(maskBinding.GetComponent<InventoryItemCollectable>());

        gridCollectionOld[0].Items = gridCollection;

        needleBinding.transform.parent = equipment.transform.parent;
        silkBinding.transform.parent = equipment.transform.parent;
        toolsBinding.transform.parent = equipment.transform.parent;
        maskBinding.transform.parent = equipment.transform.parent;
        
        var needleBindingIcons = needleBinding.transform.Find("Group/Parent").gameObject;
        needleBindingIcons.transform.localScale = new Vector3(0.35f, 0.35f, 1);
        var iconTemplate = needleBindingIcons.transform.Find("Icon").gameObject;
        iconTemplate.SetActive(false);
        var needleBindingIconDeactivated = GameObject.Instantiate(iconTemplate, parent: needleBindingIcons.transform);
        needleBindingIconDeactivated.name = "Deactivated";
        needleBindingIconDeactivated.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["GG_UI_pieces_nail_off"];
        needleBindingIconDeactivated.transform.localScale = new Vector3(2.2f, 2.2f, 1);
        var needleBindingIconActivated = GameObject.Instantiate(iconTemplate, parent: needleBindingIcons.transform);
        needleBindingIconActivated.name = "Activated";
        needleBindingIconActivated.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["GG_UI_pieces_nail"];
        needleBindingIconActivated.transform.localScale = new Vector3(2.2f, 2.2f, 1);
        var needleBindingIconActivatedAll = GameObject.Instantiate(iconTemplate, parent: needleBindingIcons.transform);
        needleBindingIconActivatedAll.name = "AllActivated";
        needleBindingIconActivatedAll.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["GG_UI_pieces_nail_r"];
        needleBindingIconActivatedAll.transform.localScale = new Vector3(2.2f, 2.2f, 1);

        var silkBindingIcons = silkBinding.transform.Find("Group/Parent").gameObject;
        silkBindingIcons.transform.localScale = new Vector3(0.35f, 0.35f, 1);
        iconTemplate = silkBindingIcons.transform.Find("Icon").gameObject;
        iconTemplate.SetActive(false);
        var silkBindingIconDeactivated = GameObject.Instantiate(iconTemplate, parent: silkBindingIcons.transform);
        silkBindingIconDeactivated.name = "Deactivated";
        silkBindingIconDeactivated.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["GG_UI_pieces_soul_off"];
        silkBindingIconDeactivated.transform.localScale = new Vector3(2.2f, 2.2f, 1);
        var silkBindingIconActivated = GameObject.Instantiate(iconTemplate, parent: silkBindingIcons.transform);
        silkBindingIconActivated.name = "Activated";
        silkBindingIconActivated.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["GG_UI_pieces_soul"];
        silkBindingIconActivated.transform.localScale = new Vector3(2.2f, 2.2f, 1);
        var silkBindingIconActivatedAll = GameObject.Instantiate(iconTemplate, parent: silkBindingIcons.transform);
        silkBindingIconActivatedAll.name = "AllActivated";
        silkBindingIconActivatedAll.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["GG_UI_pieces_soul_r"];
        silkBindingIconActivatedAll.transform.localScale = new Vector3(2.2f, 2.2f, 1);

        var toolsBindingIcons = toolsBinding.transform.Find("Group/Parent").gameObject;
        toolsBindingIcons.transform.localScale = new Vector3(0.35f, 0.35f, 1);
        iconTemplate = toolsBindingIcons.transform.Find("Icon").gameObject;
        iconTemplate.SetActive(false);
        var toolsBindingIconDeactivated = GameObject.Instantiate(iconTemplate, parent: toolsBindingIcons.transform);
        toolsBindingIconDeactivated.name = "Deactivated";
        toolsBindingIconDeactivated.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["GG_UI_pieces_charm_off"];
        toolsBindingIconDeactivated.transform.localScale = new Vector3(2.2f, 2.2f, 1);
        var toolsBindingIconActivated = GameObject.Instantiate(iconTemplate, parent: toolsBindingIcons.transform);
        toolsBindingIconActivated.name = "Activated";
        toolsBindingIconActivated.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["GG_UI_pieces_charm"];
        toolsBindingIconActivated.transform.localScale = new Vector3(2.2f, 2.2f, 1);
        var toolsBindingIconActivatedAll = GameObject.Instantiate(iconTemplate, parent: toolsBindingIcons.transform);
        toolsBindingIconActivatedAll.name = "AllActivated";
        toolsBindingIconActivatedAll.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["GG_UI_pieces_charm_r"];
        toolsBindingIconActivatedAll.transform.localScale = new Vector3(2.2f, 2.2f, 1);

        var maskBindingIcons = maskBinding.transform.Find("Group/Parent").gameObject;
        maskBindingIcons.transform.localScale = new Vector3(0.35f, 0.35f, 1);
        iconTemplate = maskBindingIcons.transform.Find("Icon").gameObject;
        iconTemplate.SetActive(false);
        var maskBindingIconDeactivated = GameObject.Instantiate(iconTemplate, parent: maskBindingIcons.transform);
        maskBindingIconDeactivated.name = "Deactivated";
        maskBindingIconDeactivated.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["GG_UI_pieces_shell_off"];
        maskBindingIconDeactivated.transform.localScale = new Vector3(2.2f, 2.2f, 1);
        var maskBindingIconActivated = GameObject.Instantiate(iconTemplate, parent: maskBindingIcons.transform);
        maskBindingIconActivated.name = "Activated";
        maskBindingIconActivated.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["GG_UI_pieces_shell"];
        maskBindingIconActivated.transform.localScale = new Vector3(2.2f, 2.2f, 1);
        var maskBindingIconActivatedAll = GameObject.Instantiate(iconTemplate, parent: maskBindingIcons.transform);
        maskBindingIconActivatedAll.name = "AllActivated";
        maskBindingIconActivatedAll.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["GG_UI_pieces_shell_r"];
        maskBindingIconActivatedAll.transform.localScale = new Vector3(2.2f, 2.2f, 1);

        heartPieces.OnSelected += (_) =>
        {
            textName.text = "Ancient Mask";
            textDesc.text = "An ancient mask carved from cold, pale ore. The mask protects the wearer, guarding their shell against damage.";
        };
        foreach(Transform child in heartPieces.transform)
        {
            if(child.name == "Pieces 4") child.gameObject.SetActive(true);
            else child.gameObject.SetActive(false);
        }
        var heartPiecesAmountText = GameObject.Instantiate(template.transform.Find("Group/Amount Text"), parent: heartPieces.transform).gameObject;
        heartPiecesAmountText.transform.position = new Vector3(-9.4864f, 5.8192f, 41.4f);
        heartPiecesAmountText.name = "Amount Text";
        heartPiecesAmountText.SetActive(true);

        spoolPieces.OnSelected += (_) =>
        {
            textName.text = "Silk Spool";
            textDesc.text = "Artefact left behind by the Weavers, designed to collect and hold additional Silk.";
        };
        foreach(Transform child in spoolPieces.transform)
        {
            if(child.name == "Full") child.gameObject.SetActive(true);
            else child.gameObject.SetActive(false);
        }
        var spoolPiecesAmountText = GameObject.Instantiate(template.transform.Find("Group/Amount Text"), parent: spoolPieces.transform).gameObject;
        spoolPiecesAmountText.transform.position = new Vector3(-6.7864f, 5.8192f, 41.4f);
        spoolPiecesAmountText.name = "Amount Text";
        spoolPiecesAmountText.SetActive(true);

        IEnumerator ActivateSilkHeartsEveryFrame()
        {
            while (true)
            {
                silkHeartsSpool.gameObject.SetActive(true);
                foreach(Transform child in silkHeartsSpool.transform)
                {
                    if(child.name == "Heart") child.gameObject.SetActive(true);
                    else child.gameObject.SetActive(false);
                }
                var hearts = silkHeartsSpool.transform.Find("Heart");
                var hearts2 = menuBindings.transform.Find("Inv/Silk Spool Desc Section(Clone)/Silk Hearts/Counter");
                foreach(Transform child in hearts) child.gameObject.SetActive(false);
                if(hearts2 != null)
                {
                    foreach(Transform child in hearts2) child.gameObject.SetActive(false);
                    hearts2.gameObject.SetActive(true);
                }
                var count = (PlayerData.instance.silkRegenMax > 3) ? 3 : PlayerData.instance.silkRegenMax;
                for(int i = 0; i < count; i++)
                {
                    hearts.GetChild(i).gameObject.SetActive(true);
                    if(hearts2 != null) hearts2.GetChild(i).gameObject.SetActive(true);
                }

                yield return null;
            }
        }
        menuBindingsFsm.FsmComponent.StartCoroutine(ActivateSilkHeartsEveryFrame());

        cloakStates = CreateButtonInv("Cloak States", template, new Vector3(-7.1054f, 0.4297f, 4.3f), "", "");
        cloakStates.transform.parent = inv.transform;
        cloakStates.Selectables = new InventoryItemSelectable[4]{spoolPieces, silkHeartsSpool, needle, needleBinding};
        spoolPieces.Selectables[1] = cloakStates; //down
        needle.Selectables[3] = cloakStates; //right
        cloakStates.OnSelected += (_) =>
        {
            var pd = PlayerData.instance;
            if(pd == null) return;

            int cloakState = 0;
            if(!pd.hasBrolly && !pd.hasDoubleJump) cloakState = 0;
            else if(pd.hasBrolly && !pd.hasDoubleJump) cloakState = 1;
            else if(!pd.hasBrolly && pd.hasDoubleJump) cloakState = 2;
            else if(pd.hasBrolly && pd.hasDoubleJump) cloakState = 3;

            textName.text = cloakLables[cloakState];
            textDesc.text = cloakDescriptions[cloakState];
        };
        var cloakState0 = GameObject.Instantiate(iconTemplate, parent: cloakStates.transform);
        cloakState0.name = "CloakState_0";
        cloakState0.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["CloakState_0"];
        var cloakState1 = GameObject.Instantiate(iconTemplate, parent: cloakStates.transform);
        cloakState1.name = "CloakState_1";
        cloakState1.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["CloakState_1"];
        var cloakState2 = GameObject.Instantiate(iconTemplate, parent: cloakStates.transform);
        cloakState2.name = "CloakState_2";
        cloakState2.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["CloakState_2"];
        var cloakState3 = GameObject.Instantiate(iconTemplate, parent: cloakStates.transform);
        cloakState3.name = "CloakState_3";
        cloakState3.GetComponent<SpriteRenderer>().sprite = (Sprite)Preload.bundleResources["CloakState_3"];

        foreach(var item in new InventoryItemCollectable[]{needleBinding, silkBinding, toolsBinding, maskBinding})
        {
            ToggleBinding(item.gameObject);
            ToggleBinding(item.gameObject);
        }

        menuBindingsFsm.FsmComponent.StartCoroutine(UpdateSilkSpool());

        invPane.OnPaneStart += () =>
        {
            border.transform.Find("Pane Name").gameObject.GetComponent<TMProOld.TextMeshPro>().text = "Bindings";
            inv.transform.Find("Text Completion").gameObject.GetComponent<TMProOld.TextMeshPro>().enabled = false;
            inv.transform.Find("Text Completion/Percentage").gameObject.GetComponent<TMProOld.TextMeshPro>().enabled = false;

            foreach(var item in new InventoryItemCollectable[]{needleBinding, silkBinding, toolsBinding, maskBinding})
            {
                if(item == null) continue;
                item.gameObject.transform.Find("Group/generic_flash_ui").gameObject.SetActive(false);
            }

            UpdateMenuBindingsDisplay();
        };
    }
}