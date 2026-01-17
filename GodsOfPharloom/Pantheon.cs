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

namespace Gods_Of_Pharloom;

public class Pantheon : MonoBehaviour
{
    public string pantheonName;
    public string pantheonDisplayName;
    public Transform bindings;
    public GameObject needleBinding;
    public GameObject silkBindng;
    public GameObject toolsBinding;
    public GameObject maskBinding;
    public GameObject hitlessHeart;
    public GameObject doorStates;
    public GameObject radiantBackboard;
    public BossInfo[] sequence;
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
    }
}