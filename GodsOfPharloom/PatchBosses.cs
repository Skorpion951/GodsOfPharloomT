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
            var orig = __instance?.gameObject;
            if(orig == null) return;

            int index1 = 0;
            for(; index1 < PatchedFsm.patchedFsms.Length; index1++)
            {
                if(String.Equals(__instance.gameObject.scene.name, PatchedFsm.patchedFsms[index1].sceneName, StringComparison.OrdinalIgnoreCase)) break;
                if(index1 == PatchedFsm.patchedFsms.Length - 1) return;
            }
            Log.LogInfo(__instance.gameObject.name);


            var patchedFsm = PatchedFsm.patchedFsms[index1];

            foreach(var item in patchedFsm.fsms)
            {
                if(orig.name.Equals(item.objName) && String.Equals(__instance.FsmName, item.fsmName))
                {
                    Log.LogInfo(__instance.FsmName + "YAAAAAAAAAAAAAAAAAY");
                    item.method(__instance.Fsm);
                    return;
                }
            }
        }
    }
}