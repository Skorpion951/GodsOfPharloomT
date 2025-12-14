using System.Collections;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;
using Gods_Of_Pharloom;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class CustomScene
{
    public string sceneName {get; private set;}
    private List<TransitionGateInfo> TransitionGates = new List<TransitionGateInfo>();
    public Action AfterSceneActivated;
    public bool isSceneActive = false;
    public bool isPreloading = false;
    public bool isSkongScene = false;

    public CustomScene(string sceneName, bool isSkongScene = false)
    {
        this.sceneName = sceneName;
        this.isSkongScene = isSkongScene;
    }
    
    public void AddTransitionPoint(string gateName, Vector3 position, TransitionPointInfo TransitionPointInfo)
    {
        TransitionGates.Add(new TransitionGateInfo(gateName, position, TransitionPointInfo));
        SceneTeleportMap.AddTransitionGate(this.sceneName, gateName);
    }
    public bool Remove(string entryPoint)
    {
        var itemToRemove = TransitionGates.Find(item =>
        {
            return item.tp.entryPoint == entryPoint;
        });

        if(TransitionGates.IndexOf(itemToRemove) == -1) return false;
        else
        {
            TransitionGates.Remove(itemToRemove);
            return true;
        }
    }
    public IEnumerator PreloadScene()
    {
        this.isPreloading = true;
        var op = SceneManager.LoadSceneAsync(this.sceneName, LoadSceneMode.Additive);
        yield return op;
        this.Activate();
    }
    private void CreateGate(TransitionGateInfo item)
    {
        //create\\
        var gm = new GameObject(item.gateName);
        var collider = gm.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        //setup gate's position
        gm.transform.position = item.pos;
        
        //setup collider size
        if(item.gateName.Contains("top") || item.gateName.Contains("bot")) collider.size = new Vector2(4, 1);
        else collider.size = new Vector2(1, 4);
        if(item.tp.isOneTimeTransition) collider.enabled = false;

        //adding transition point
        var tp = gm.AddComponent<TransitionPoint>();
        tp.targetScene = item.tp.targetScene;
        tp.entryPoint = item.tp.entryPoint;
        tp.InteractLabel = item.tp.InteractLabel;
        tp.isADoor = item.tp.isADoor;
        tp.OnDoorEnter = new UnityEngine.Events.UnityEvent();
        tp.respawnMarker = gm.AddComponent<HazardRespawnMarker>();
        tp.Activate();
        SceneManager.MoveGameObjectToScene(gm, SceneManager.GetSceneByName(sceneName));
    }

    public System.Collections.IEnumerator GetObjectFromSilkScene(string[] path, string sceneName, Action<GameObject> func)
    {
        string scenePath = "Scenes/" + sceneName;
        var op = UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(scenePath, UnityEngine.SceneManagement.LoadSceneMode.Additive, activateOnLoad: false);
        GodsOfPharloomMod.Log.LogInfo("000000000000000000");
        yield return op;
        GodsOfPharloomMod.Log.LogInfo("000000000000000000.1");
        var handle = op.Result;
        var wait = handle.ActivateAsync();
        yield return wait;
        var scene = handle.Scene;
        GodsOfPharloomMod.Log.LogInfo("111111111111111111");
        GodsOfPharloomMod.Log.LogInfo(scene.name);
        var objects = scene.GetRootGameObjects();
        var obj = GetObjectByPath(ref objects, path);
        GodsOfPharloomMod.Log.LogInfo("222222222222222222");
        // obj.SetActive(false);
        // GodsOfPharloomMod.Log.LogInfo("333333333333333333");
        // SceneManager.MoveGameObjectToScene(obj, SceneManager.GetSceneByName(this.sceneName));
        // func(obj);
        Addressables.UnloadSceneAsync(handle, true);
    }

    public static GameObject GetObjectByPath(ref GameObject[] rootObjects, string[] path)
    {
        int j = 0;
        Transform parent = null;

        foreach(var obj in rootObjects)
        {
            obj.SetActive(false);
            if(obj.name == path[j])
            {
                parent = obj.transform;
            }
        }

        if(j == path.Length - 1 && parent != null)
        {
            return parent.gameObject;
        }

        j++;
        int i = 0;
        for(; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if(child.gameObject.name == path[j])
            {
                if(j == path.Length - 1)
                {
                    return child.gameObject;
                }
                parent = child;
                i = 0;
                j++;
            }
        }
        return null;
    }

    public void Activate()
    {
        var scene = SceneManager.GetSceneByName(sceneName);
        var rootObjects = scene.GetRootGameObjects();
        foreach(var obj in rootObjects)
        {
            if (obj.name.StartsWith("CameraLock")){
                var area = obj.AddComponent<CameraLockArea>();
                var collider = obj.GetComponent<BoxCollider2D>();
            }
        }
        foreach(var item in TransitionGates)
        {
            CreateGate(item);
        }
        var gm = new GameObject("TileMap");
        gm.tag = "TileMap";
        gm.AddComponent<tk2dTileMap>();
        // var sm = new GameObject("_SceneManager");
        // sm.tag = "SceneManager";
        SceneManager.MoveGameObjectToScene(gm, SceneManager.GetSceneByName(this.sceneName));
        // SceneManager.MoveGameObjectToScene(sm, SceneManager.GetSceneByName("GG_Pharloom_Atrium"));
        // var csm = sm.AddComponent<CustomSceneManager>();
        // csm.scenePools = new SceneObjectPool[0];
        // sm.AddComponent<PlayerDataTestResponse>();
        AfterSceneActivated?.Invoke();
    }
}