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

namespace Gods_Of_Pharloom;

public class Preload
{
    public struct ObjectPreloadInfo
    {
        public string objectName;
        public string path;
        public bool isActive;
    }
    public struct ScenePreloadInfo
    {
        public string sceneName;
        public ObjectPreloadInfo[] objectsInfo;
    }
    public static bool startedInitialization = false;
    public static bool isInitialized = false;
    public static int preloadedCount = 0;
    public static GameObject handler;
    public static Dictionary<string, GameObject> preloads = new Dictionary<string, GameObject>();
    public static ScenePreloadInfo[] preloadsInfo = new ScenePreloadInfo[]
    {
        new ScenePreloadInfo{sceneName = "Ant_17", objectsInfo = new ObjectPreloadInfo[]{
            new ObjectPreloadInfo{objectName = "RestBench", path = "Trap Scene/RestBench", isActive = true},
            new ObjectPreloadInfo{objectName = "_SceneManager_Ant_17", path = "_SceneManager", isActive = false},
        }},
    };

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
            GodsOfPharloomMod.instance.StartCoroutine(PreloadObjects(preloadInfo));
        }
        
        while(!(preloadedCount >= preloadsInfo.Length)) yield return null;

        isInitialized = true;
    }
    public static void AddToPreloads(GameObject obj, ObjectPreloadInfo objectInfo)
    {
        var copy = (GameObject)GameObject.Instantiate(obj, parameters: new InstantiateParameters
        {
            scene = SceneManager.GetSceneByName("DontDestroyOnLoad"),
            parent = handler.transform
        });
        copy.name = objectInfo.objectName;
        copy.SetActive(objectInfo.isActive);

        preloads[copy.name] = copy;
    }

    public static IEnumerator PreloadObjects(ScenePreloadInfo preloadInfo)
    {
        var op = Addressables.LoadSceneAsync("Scenes/" + preloadInfo.sceneName, LoadSceneMode.Additive);
        var sceneLoad = GodsOfPharloomMod.instance.StartCoroutine(PreloadScene(op));

        yield return sceneLoad;

        var rootObjects = op.Result.Scene.GetRootGameObjects();

        foreach(var objInfo in preloadInfo.objectsInfo)
        {
            string[] subPaths = objInfo.path.Split(separator: new char[]{'/'});
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
                AddToPreloads(parent, objInfo);
                break;
            }

            for(int i = 0; i < subPaths.Length; i++)
            {
                foreach(Transform child in parent.transform)
                {
                    if(child.name == subPaths[i])
                    {
                        if(i == subPaths.Length - 1)
                        {
                            AddToPreloads(child.gameObject, objInfo);
                            break;
                        }
                        parent = child.gameObject;
                        break;
                    }
                }
            }
        }

        Addressables.UnloadSceneAsync(op);

        preloadedCount++;
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