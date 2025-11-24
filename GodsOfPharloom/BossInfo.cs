using UnityEngine;

public static class BossesInfo
{
    public enum BossName {MossMother};
    public static Dictionary<BossName, string> bossesSceneName = new Dictionary<BossName, string>
    {
        {BossName.MossMother, "Tut_03"}
    };
    public static Dictionary<BossName, string[][]> bossObjectsPath = new Dictionary<BossName, string[][]>
    {
        {BossName.MossMother, new string[][] { new string[] {"Black Thread States"} } } //Battle Scene "Normal World"
    };
}