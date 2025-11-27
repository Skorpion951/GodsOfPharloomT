using BepInEx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEngine.AddressableAssets;
using HarmonyLib;

namespace Gods_Of_Pharloom
{
    public partial class GodsOfPharloomMod : BaseUnityPlugin
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayMakerFSM), "Awake")]
        private static void PlayMakerPatch_Postfix(PlayMakerFSM __instance)
        {
            Log.LogInfo(__instance.gameObject.name);
            var orig = __instance?.gameObject;
            if(orig == null) return;

            int index1 = -1;
            int index2 = -1;
            bool found = false;
            for(int i = 0; i < BossesInfo.bossesGameObjName.Length; i++)
            {
                for(int j = 0; j < BossesInfo.bossesGameObjName[i].Length; j++)
                {
                    if(orig.name.Equals(BossesInfo.bossesGameObjName[i][j]) &&
                        String.Equals(__instance.FsmName, BossesInfo.bossesFsmName[i][j]))
                    {
                        found = true;
                        index1 = i;
                        index2 = j;
                        break;
                    }
                }
                if(found) break;
            }
            if(!found) return;
            
            (BossesInfo.bossesPatchedFsm[index1][index2])(__instance);
        }
    }
}