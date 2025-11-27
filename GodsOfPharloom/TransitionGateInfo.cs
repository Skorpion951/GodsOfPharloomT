using UnityEngine;
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