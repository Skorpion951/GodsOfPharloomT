using HutongGames.PlayMaker.Actions;
using UnityEngine;
public class TransitionPointInfo
{
    public string gateName;
    public Vector3 position;
    public string targetScene;
    public string entryPoint;
    public InteractableBase.PromptLabels InteractLabel;
    public bool isADoor;
    public bool isOneTimeTransition;
    public bool dontWalkOutOfDoor;
    public bool hardLandOnExit;
    public bool noInputOnStart;
    public bool alwaysEnterRight;
    public bool forceMemoryZone;
    public Action afterTransition;
    public bool doSendEventAfterTransition;
    public bool doCreateRespawnMarker;
    public static string eventName = "HORNET TRANSITION DONE MOD";

    public TransitionPointInfo(string gateName, Vector3 position, string targetScene, string entryPoint,
                InteractableBase.PromptLabels InteractLabel = InteractableBase.PromptLabels.Enter, bool isADoor = false,
                bool isOneTimeTransition = false, bool dontWalkOutOfDoor = false, bool hardLandOnExit = false,
                bool noInputOnStart = false, bool alwaysEnterRight = true, bool forceMemoryZone = true,
                Action afterTransition = null, bool doSendEventAfterTransition = true, bool doCreateRespawnMarker = true,
                bool doCreateHazardRespawnMarker = true)
    {
        this.gateName = gateName;
        this.position = position;
        this.targetScene = targetScene;
        this.entryPoint = entryPoint;
        this.InteractLabel = InteractLabel;
        this.isADoor = isADoor;
        this.isOneTimeTransition = isOneTimeTransition;
        this.dontWalkOutOfDoor = dontWalkOutOfDoor;
        this.hardLandOnExit = hardLandOnExit;
        this.noInputOnStart = noInputOnStart;
        this.alwaysEnterRight = alwaysEnterRight;
        this.forceMemoryZone = forceMemoryZone;
        this.afterTransition = afterTransition;
        this.doSendEventAfterTransition = doSendEventAfterTransition;
        this.doCreateRespawnMarker = doCreateRespawnMarker;
    }
}