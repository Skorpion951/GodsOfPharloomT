using System.Collections;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;
using System.Windows.Forms.VisualStyles;
using Gods_Of_Pharloom;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class CustomScene
{
    public string sceneName {get; private set;}
    private List<TransitionPointInfo> TransitionGates = new List<TransitionPointInfo>();
    public Action AfterSceneActivated;
    public Vector2 tileMapVector = new Vector2(512, 512);
    public bool isSceneActive = false;
    public bool isPreloading = false;
    public bool isSkongScene = false;

    public CustomScene(string sceneName, bool isSkongScene = false)
    {
        this.sceneName = sceneName;
        this.isSkongScene = isSkongScene;
    }
    
    public void AddTransitionPoint(TransitionPointInfo TransitionPointInfo)
    {
        TransitionGates.Add(TransitionPointInfo);
        SceneTeleportMap.AddTransitionGate(this.sceneName, TransitionPointInfo.gateName);
    }
    public bool Remove(string entryPoint)
    {
        var itemToRemove = TransitionGates.Find(item =>
        {
            return item.entryPoint == entryPoint;
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
    private void CreateGate(TransitionPointInfo item)
    {
        //create\\
        var gm = new GameObject(item.gateName);
        var collider = gm.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        //setup gate's position
        gm.transform.position = item.position;
        
        //setup collider size
        if(item.gateName.Contains("top") || item.gateName.Contains("bot")) collider.size = new Vector2(4, 1);
        else collider.size = new Vector2(1, 4);
        if(item.isOneTimeTransition) collider.enabled = false;

        //adding transition point
        CreateTransitionPoint(item, gm);
        SceneManager.MoveGameObjectToScene(gm, SceneManager.GetSceneByName(sceneName));
    }
    public static TransitionPoint CreateTransitionPoint(TransitionPointInfo item, GameObject go)
    {
        var tp = go.AddComponent<TransitionPoint>();

        var fsmComponent = go.AddComponent<PlayMakerFSM>();
        tp.customEntryFSM = fsmComponent;
        fsmComponent.enabled = false;

        var fsm = fsmComponent.Fsm;

        var init = new FsmState(fsm);
        init.Name = "Init";

        var afterEntry = new FsmState(fsm);
        afterEntry.Name = "After Entry";

        fsm.StartState = "Init";

        var actionAfter = new PatchedFsm.CustomLogicFsm(fsm);
        actionAfter.action = (Fsm fsm) =>
        {
            if(item.forceMemoryZone) GameManager.instance.ForceCurrentSceneIsMemory(true);
            if(item.noInputOnStart) HeroController.instance.hero_state = GlobalEnums.ActorStates.no_input;
            item.afterTransition?.Invoke();
        };

        init.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent("FINISH ENTRY"),
                ToFsmState = afterEntry
            }
        };

        afterEntry.Actions = new FsmStateAction[]{actionAfter};

        fsm.States = new FsmState[]{init, afterEntry};

        fsm.FsmComponent.enabled = true;
        if (item.alwaysEnterRight)
        {
            tp.alwaysEnterRight = true;
            tp.alwaysEnterLeft = false;
        }
        else
        {
            tp.alwaysEnterLeft = true;
            tp.alwaysEnterRight = false;
        }
        
        tp.targetScene = item.targetScene;
        tp.dontWalkOutOfDoor = item.dontWalkOutOfDoor;
        tp.hardLandOnExit = item.hardLandOnExit;
        tp.entryPoint = item.entryPoint;
        tp.InteractLabel = item.InteractLabel;
        tp.isADoor = item.isADoor;
        tp.OnDoorEnter = new UnityEngine.Events.UnityEvent();
        tp.respawnMarker = go.AddComponent<HazardRespawnMarker>();
        tp.Activate();

        return tp;
    }

    public static CameraLockArea CreateCameraLock(string sceneName)
    {
        var go = new GameObject("CameraLockArea");
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName(sceneName));
        var collider = go.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(10f, 10f);
        var cameraLockArea = go.AddComponent<CameraLockArea>();

        return cameraLockArea;
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

        if (!isSkongScene)
        {
            // tilemap needs for max heigh and width of scene
            var gm = new GameObject("TileMap");
            gm.tag = "TileMap";
            var tileMap = gm.AddComponent<tk2dTileMap>();
            tileMap.width = (int)tileMapVector.x;
            tileMap.height = (int)tileMapVector.y;
            SceneManager.MoveGameObjectToScene(gm, SceneManager.GetSceneByName(this.sceneName));
        }
        // var sm = new GameObject("_SceneManager");
        // sm.tag = "SceneManager";
        // SceneManager.MoveGameObjectToScene(sm, SceneManager.GetSceneByName("GG_Pharloom_Atrium"));
        // var csm = sm.AddComponent<CustomSceneManager>();
        // csm.scenePools = new SceneObjectPool[0];
        // sm.AddComponent<PlayerDataTestResponse>();
        AfterSceneActivated?.Invoke();
    }
}