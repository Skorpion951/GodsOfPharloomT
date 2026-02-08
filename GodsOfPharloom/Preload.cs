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
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Drawing.Drawing2D;
using HutongGames.PlayMaker;
using GenericVariableExtension;
using TMPro;

namespace Gods_Of_Pharloom;

public class Preload
{
    public struct ObjectPreloadInfo
    {
        public string objectName;
        public string path;
        public bool isActive;
        public Action<GameObject> afterObjectPreloaded;
    }
    public struct ScenePreloadInfo
    {
        public string sceneName;
        public ObjectPreloadInfo[] objectsInfo;
    }
    public static Action afterAllPreloaded;
    public static bool startedInitialization = false;
    public static bool isInitialized = false;
    public static int preloadedCount = 0;
    public static GameObject handler;
    public static Dictionary<string, GameObject> preloads = new Dictionary<string, GameObject>();
    public static ScenePreloadInfo[] preloadsInfo = new ScenePreloadInfo[]
    {
        new ScenePreloadInfo{sceneName = "Ant_17", objectsInfo = new ObjectPreloadInfo[]{
            new ObjectPreloadInfo{objectName = "_SceneManager_Ant_17", path = "_SceneManager", isActive = false},
        }},
        new ScenePreloadInfo{sceneName = "Song_10", objectsInfo = new ObjectPreloadInfo[]{
            new ObjectPreloadInfo{objectName = "Surface Water Region", path = "Surface Water Region", isActive = true},
            new ObjectPreloadInfo{objectName = "Spa Region", path = "Spa Region", isActive = true},
            new ObjectPreloadInfo{objectName = "spa_water_small", path = "spa_water_small (1)", isActive = true},
            new ObjectPreloadInfo{objectName = "StillWater", path = "StillWater", isActive = true},
        }},
        new ScenePreloadInfo{sceneName = "Abyss_05", objectsInfo = new ObjectPreloadInfo[]{
            new ObjectPreloadInfo{objectName = "_SceneManager_Abyss_05", path = "_SceneManager", isActive = false},
        }},
        new ScenePreloadInfo{sceneName = "Peak_12", objectsInfo = new ObjectPreloadInfo[]{
            new ObjectPreloadInfo{objectName = "RestBench", path = "RestBench (1)", isActive = true},
        }},
        new ScenePreloadInfo{sceneName = "Memory_Ant_Queen", objectsInfo = new ObjectPreloadInfo[]{
            new ObjectPreloadInfo{objectName = "Exit Edge Trigger_AntQueen", path = "Exit Edge Trigger", isActive = true},
            new ObjectPreloadInfo{objectName = "door_wakeInMemory_AntQueen", path = "door_wakeInMemory", isActive = true},
        }},
        new ScenePreloadInfo{sceneName = "Shellwood_11b", objectsInfo = new ObjectPreloadInfo[]{
            new ObjectPreloadInfo{objectName = "door_wakeOnGround_FlowerQueen", path = "door_wakeOnGround", isActive = true},
            new ObjectPreloadInfo{objectName = "Memory Group", path = "memory_font/Uncompleted/Memory Group", isActive = true,
                afterObjectPreloaded = (GameObject go) =>
                {
                    go.transform.position = new Vector3(45, 53, 0);
                    var children = go.transform;
                    foreach(Transform child in children)
                    {
                        if(child.name == "thread_memory")
                        {
                            go = child.gameObject;

                            GameObject.Destroy(Preload.FindObjectByPath(new GameObject[]{go}, "thread_memory/fade/ghosts/glow (2)"));

                            GameObject.Destroy(go.GetComponent<PersistentBoolItem>());

                            var fsms = go.GetComponents<PlayMakerFSM>();
                            foreach(var fsm in fsms)
                            {
                                if(fsm.FsmName == "Deep Memory Pre Enter Effect")
                                {
                                    var init = fsm.Fsm.GetState("Init");

                                    var preEnterEffect = GameObject.Instantiate(((CreateObject)init.Actions[2]).gameObject.Value);
                                    preEnterEffect.transform.SetParent(handler.transform);
                                    preEnterEffect.name = "Deep Memory Pre Enter Effect";
                                    preloads[preEnterEffect.name] = preEnterEffect;

                                    break;
                                }
                            }
                        }
                    }
                }
            },
        }},
    };

    public static Dictionary<string, object> bundleResources = new Dictionary<string, object>();

    public static void Init()
    {
        GodsOfPharloomMod.instance.StartCoroutine(EInit());
    }

    private static IEnumerator EInit()
    {
        startedInitialization = true;

        handler = new GameObject("PreloadHandler_GodsOfPharloom");
        handler.SetActive(false);
        GameObject.DontDestroyOnLoad(handler);

        foreach(var preloadInfo in preloadsInfo)
        {
            var coroutine = GodsOfPharloomMod.instance.StartCoroutine(PreloadObjects(preloadInfo));
            yield return coroutine;
        }
        
        while(!(preloadedCount >= preloadsInfo.Length)) yield return null;

        afterAllPreloaded?.Invoke();

        isInitialized = true;
    }
    public static void AddToPreloads(GameObject obj, ObjectPreloadInfo objectInfo)
    {
        GameObject.DontDestroyOnLoad(obj);
        obj.transform.SetParent(handler.transform);
        obj.name = objectInfo.objectName;
        obj.SetActive(objectInfo.isActive);

        preloads[obj.name] = obj;

        objectInfo.afterObjectPreloaded?.Invoke(obj);
    }

    public static IEnumerator PreloadObjects(ScenePreloadInfo preloadInfo)
    {
        var op = Addressables.LoadSceneAsync("Scenes/" + preloadInfo.sceneName, LoadSceneMode.Additive);
        var sceneLoad = GodsOfPharloomMod.instance.StartCoroutine(PreloadScene(op));

        yield return sceneLoad;

        var rootObjects = op.Result.Scene.GetRootGameObjects();

        foreach(var objInfo in preloadInfo.objectsInfo)
        {
            AddToPreloads(FindObjectByPath(rootObjects, objInfo.path), objInfo);
        }

        Addressables.UnloadSceneAsync(op);

        preloadedCount++;
    }
    public static GameObject FindObjectByPath(GameObject[] rootObjects, string path)
    {
        string[] subPaths = path.Split(separator: new char[]{'/'});
        GameObject parent = null;

        foreach(var obj in rootObjects)
        {
            if(obj.name == subPaths[0])
            {
                parent = obj;
                break;
            }
        }

        if(subPaths.Length == 1){
            return parent;
        }

        for(int i = 0; i < subPaths.Length; i++)
        {
            foreach(Transform child in parent.transform)
            {
                if(child.name == subPaths[i])
                {
                    if(i == subPaths.Length - 1)
                    {
                        return child.gameObject;
                    }
                    parent = child.gameObject;
                    break;
                }
            }
        }

        return null;
    }
    public static GameObject FindObjectByPath(GameObject parentObject, string path)
    {
        string[] subPaths = path.Split(separator: new char[]{'/'});
        GameObject parent = null;

        foreach(Transform obj in parentObject.transform)
        {
            if(obj.name == subPaths[0])
            {
                parent = obj.gameObject;
                break;
            }
        }

        if(subPaths.Length == 1){
            return parent;
        }

        for(int i = 0; i < subPaths.Length; i++)
        {
            foreach(Transform child in parent.transform)
            {
                if(child.name == subPaths[i])
                {
                    if(i == subPaths.Length - 1)
                    {
                        return child.gameObject;
                    }
                    parent = child.gameObject;
                    break;
                }
            }
        }

        return null;
    }
    private static IEnumerator PreloadScene(AsyncOperationHandle<SceneInstance> op)
    {
        yield return op;

        var scene = op.Result.Scene;
        var rootObjects = scene.GetRootGameObjects();
        foreach(var obj in rootObjects)
        {
            obj.SetActive(false);
        }

        yield break;
    }
}