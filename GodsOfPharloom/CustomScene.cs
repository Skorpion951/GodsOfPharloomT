using System.Collections;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Drawing2D;
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
    public Action<Scene> AfterSceneActivated;
    public Vector2 tileMapVector = new Vector2(512, 512);
    public bool isSceneActive = false;
    public bool isPreloading = false;
    public bool isSkongScene = true;
    public bool isFastSuperJump = false;
    public static float customSuperJumpSpeed = 48f;
    public static float customWaitForSuperJump = 0.01f;

    public CustomScene(string sceneName, bool isSkongScene = true, bool isFastSuperJump = false)
    {
        this.sceneName = sceneName;
        this.isSkongScene = isSkongScene;
        this.isFastSuperJump = isFastSuperJump;
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
        this.Activate((Scene)GodsOfPharloomMod.lastLoadedScene);
    }
    private void CreateGate(TransitionPointInfo item, Scene scene)
    {
        //create\\
        var go = new GameObject(item.gateName);
        var collider = go.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        //setup gate's position
        go.transform.position = item.position;
        
        //setup collider size
        if(item.gateName.Contains("top") || item.gateName.Contains("bot")) collider.size = new Vector2(4, 1);
        else collider.size = new Vector2(1, 4);
        if(item.isOneTimeTransition) collider.enabled = false;

        //adding transition point
        CreateTransitionPoint(item, go, this.sceneName);
        SceneManager.MoveGameObjectToScene(go, scene);
    }
    public static TransitionPoint CreateTransitionPoint(TransitionPointInfo item, GameObject go, string sceneName)
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
            if(item.noInputOnStart) HeroController.instance.hero_state = GlobalEnums.ActorStates.no_input; //GodsOfPharloomMod.SetHeroState(GlobalEnums.ActorStates.no_input);//HeroController.instance.StartRoarLockNoRecoil();
            if(item.doSendEventAfterTransition) PlayMakerFSM.BroadcastEvent(TransitionPointInfo.eventName);
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




        //do action after hero death event
        var fsmAfterDeathComponent = go.AddComponent<PlayMakerFSM>();
        fsmAfterDeathComponent.enabled = false;

        var fsmAfterDeath = fsmAfterDeathComponent.Fsm;
        fsmAfterDeath.StartState = "Idle";

        var idle = new FsmState(fsmAfterDeath);
        idle.Name = "Idle";

        var afterDeath = new FsmState(fsmAfterDeath);
        afterDeath.Name = "After Death";

        var customActionAfterDeath = new PatchedFsm.CustomLogicFsm(fsmAfterDeath);
        customActionAfterDeath.action += (Fsm fsm) =>
        {
            var color = new Color(0, 0, 0, 0);
            var endColor = new Color(0, 0, 0, 0);
            ScreenFaderUtils.Fade(color, endColor, 0.01f);
            GameCameras.instance.HUDIn();
        };

        afterDeath.Actions = new FsmStateAction[]{customActionAfterDeath};

        idle.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent("HERO RESPAWNING HERE"),
                ToFsmState = afterDeath
            }
        };
        fsmAfterDeath.States = new FsmState[]{idle, afterDeath};
        fsmAfterDeathComponent.enabled = true;
        //////////////////////////

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
        if(item.doCreateRespawnMarker){
            var respawnMarker = go.AddComponent<RespawnMarker>();
            var mapZone = respawnMarker.overrideMapZone = new OverrideMapZone();
            // respawnMarker.customFadeDuration = new TeamCherry.SharedUtils.OverrideFloat();
            respawnMarker.customWakeUp = true;
            tp.gameObject.tag = "RespawnPoint";

            AddRespawnMarkerToTeleportMap(sceneName, item.gateName);
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

    public static CameraLockArea CreateCameraLock(Scene scene)
    {
        var go = new GameObject("CameraLockArea");
        SceneManager.MoveGameObjectToScene(go, scene);
        var collider = go.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(10f, 10f);
        var cameraLockArea = go.AddComponent<CameraLockArea>();

        return cameraLockArea;
    }

    public static void AddRespawnMarkerToTeleportMap(string sceneName, string respawnMarkerName)
    {
        var teleportMap = SceneTeleportMap.GetTeleportMap();
        if (!teleportMap.TryGetValue(sceneName, out SceneTeleportMap.SceneInfo value))
        {
            teleportMap.Add(sceneName, new SceneTeleportMap.SceneInfo());
        }

        teleportMap.TryGetValue(sceneName, out SceneTeleportMap.SceneInfo sceneInfo);
        sceneInfo.RespawnPoints.Add(respawnMarkerName);
    }

    // to make custom benches work
    public static void InitModRespawnMarkers()
    {
        AddRespawnMarkerToTeleportMap("GG_Pharloom_Hall_Of_Gods", "RestBench");
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

    public void Activate(Scene scene)
    {
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
            CreateGate(item, scene);
        }

        if(!isSkongScene)
        {
            // tilemap needs for max heigh and width of scene
            var go = new GameObject("TileMap");
            go.tag = "TileMap";
            var tileMap = go.AddComponent<tk2dTileMap>();
            tileMap.width = (int)tileMapVector.x;
            tileMap.height = (int)tileMapVector.y;
            SceneManager.MoveGameObjectToScene(go, scene);
        }
        // var sm = new GameObject("_SceneManager");
        // sm.tag = "SceneManager";
        // SceneManager.MoveGameObjectToScene(sm, SceneManager.GetSceneByName("GG_Pharloom_Atrium"));
        // var csm = sm.AddComponent<CustomSceneManager>();
        // csm.scenePools = new SceneObjectPool[0];
        // sm.AddComponent<PlayerDataTestResponse>();
        AfterSceneActivated?.Invoke(scene);
    }
}