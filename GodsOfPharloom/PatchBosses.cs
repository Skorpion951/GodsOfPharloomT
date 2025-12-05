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
        public static PatchedFsm.BossName currentBoss = PatchedFsm.BossName.TheUnravelled;

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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SceneAdditiveLoadConditional), "OnEnable")]
        private static void SceneAdditiveLoadPatch_Prefix(SceneAdditiveLoadConditional __instance)
        {
            if(currentBoss == PatchedFsm.BossName.SavageBeastfly2 && __instance.gameObject.name.Contains("Beastfly"))
            {
                PlayerData.instance.defeatedBoneFlyerGiantGolemScene = false;

                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
            }
            if(currentBoss == PatchedFsm.BossName.Shakra && __instance.gameObject.name.Contains("Mapper Sparring"))
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
            }
            if(currentBoss == PatchedFsm.BossName.TheUnravelled && __instance.gameObject.name.Contains("Boss Loader"))
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
            }
        }
    }
}