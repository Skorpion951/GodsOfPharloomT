using UnityEngine;

public class CustomBossSequence
{
    private string[] sequence;
    private bool isBossDefeated = false;
    public IEnumerator<WaitUntil> StartSequence()
    {
        for(int i = 0; i < sequence.Length; i++)
        {
            yield return new WaitUntil(() => isBossDefeated);
        }
    }
}