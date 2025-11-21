using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class TransitionPointInfo
{
    public string targetScene;
    public string entryPoint;
    public InteractableBase.PromptLabels InteractLabel;
    public bool isADoor;

    public TransitionPointInfo(string targetScene, string entryPoint,
                InteractableBase.PromptLabels InteractLabel = InteractableBase.PromptLabels.Enter, bool isADoor = false)
    {
        this.targetScene = targetScene;
        this.entryPoint = entryPoint;
        this.InteractLabel = InteractLabel;
        this.isADoor = isADoor;
    }
}

class TransitionGateInfo
{
    public string gateName;
    public Vector3 pos;
    public TransitionPointInfo tp;
    public TransitionGateInfo(string gateName, Vector3 position, TransitionPointInfo TransitionPointInfo)
    {
       this.gateName = gateName;
       this.pos = position;
       this.tp = TransitionPointInfo;
    }
}
public class CustomScene
{
    public string sceneName {get; private set;}
    private List<TransitionGateInfo> TransitionGates = new List<TransitionGateInfo>();
    public Action CustomTask;

    public CustomScene(string sceneName)
    {
        this.sceneName = sceneName;
    }
    
    public void Add(string gateName, Vector3 position, TransitionPointInfo TransitionPointInfo)
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

    public void Init()
    {
        foreach(var item in TransitionGates)
        {
            CreateGate(item);
        }
        var gm = new GameObject("TileMap");
        gm.tag = "TileMap";
        // var sm = new GameObject("_SceneManager");
        // sm.tag = "SceneManager";
        SceneManager.MoveGameObjectToScene(gm, SceneManager.GetSceneByName(this.sceneName));
        // SceneManager.MoveGameObjectToScene(sm, SceneManager.GetSceneByName("GG_Pharloom_Atrium"));
        gm.AddComponent<tk2dTileMap>();
        // var csm = sm.AddComponent<CustomSceneManager>();
        // csm.scenePools = new SceneObjectPool[0];
        // sm.AddComponent<PlayerDataTestResponse>();
    }
}