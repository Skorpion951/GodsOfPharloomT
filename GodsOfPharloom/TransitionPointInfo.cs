using UnityEngine;
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