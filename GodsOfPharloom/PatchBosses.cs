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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SceneAdditiveLoadConditional), "OnEnable")]
        private static void SceneAdditiveLoadPatch_Prefix(SceneAdditiveLoadConditional __instance)
        {
            if(BossSequence.currentBoss == BossInfo.bosses["Fourth Chorus"] && __instance.gameObject.name.Contains("Boss Golem Loader"))
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Fourth Chorus"] && __instance.gameObject.name.Contains("Boss Beastfly Loader"))
            {
                Destroy(__instance.gameObject);
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Savage Beastfly in Far Fields"] && __instance.gameObject.name.Contains("Boss Beastfly Loader"))
            {
                PlayerData.instance.defeatedBoneFlyerGiantGolemScene = false;

                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Savage Beastfly in Far Fields"] && __instance.gameObject.name.Contains("Boss Golem Loader"))
            {
                Destroy(__instance.gameObject);
            }
            if(BossSequence.isInSequence && __instance.gameObject.name.Contains("Rest Golem Loader"))
            {
                Destroy(__instance.gameObject);
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Shakra"] && __instance.gameObject.name.Contains("Mapper Sparring"))
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Moorwing"] && __instance.gameObject.name.Contains("Mapper Sparring"))
            {
                GameObject.Destroy(__instance.gameObject);
            }
            if(BossSequence.currentBoss == BossInfo.bosses["The Unravelled"] && __instance.gameObject.name.Contains("Boss Loader"))
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Bell Beast"] && __instance.gameObject.name.Contains("Boss Additive Loader"))
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TestGameObjectActivator), "OnEnable")]
        private static void TestGameObjectActivatorPatch_Prefix(TestGameObjectActivator __instance)
        {
            if(BossSequence.currentBoss == BossInfo.bosses["Lost Garmond"] && __instance.gameObject.name == "Garmond Black Threaded Scene")
            {
                PlayerData.instance.garmondBlackThreadDefeated = false;

                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("playerDataTest", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Lost Garmond"] && __instance.gameObject.name == "Pre Garmond")
            {
                __instance.gameObject.SetActive(false);
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PersistentIntItem), "Awake")]
        private static bool PersistentIntItemPatch_Prefix(PersistentIntItem __instance)
        {
            if(BossSequence.isInSequence && __instance.gameObject.name == "Churchkeeper Basement")
            {
                Destroy(__instance.gameObject);
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PersistentBoolItem), "Awake")]
        private static bool PersistentBoolItemPatch_Prefix(PersistentBoolItem __instance)
        {
            if(BossSequence.currentBoss == BossInfo.bosses["Moss Mother"] && __instance.gameObject.name == "Battle Scene" || __instance.gameObject.name == "Boss Scene")
            {
                GameObject.Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Great Conchflies"] && __instance.gameObject.name == "Driller A" || __instance.gameObject.name == "Driller B")
            {
                GameObject.Destroy(__instance);
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DeactivateIfPlayerdataTrue), "OnEnable")]
        private static bool DeactivateIfPlayerdataTrue_Prefix(DeactivateIfPlayerdataTrue __instance)
        {
            if(BossSequence.currentBoss == BossInfo.bosses["Moss Mother"] && __instance.gameObject.name == "Battle Scene"){
                GameObject.Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Bell Beast"] && __instance.gameObject.name == "Boss Scene"){
                GameObject.Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Fourth Chorus"] && __instance.gameObject.name == "Lava Rocks"){
                GameObject.Destroy(__instance);
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DeactivateIfPlayerdataFalse), "OnEnable")]
        private static bool DeactivateIfPlayerdataFalse_Prefix(DeactivateIfPlayerdataFalse __instance)
        {
            if(BossSequence.currentBoss == BossInfo.bosses["Great Conchflies"] && __instance.gameObject.name == "Coral Driller Return Corpse"){
                __instance.gameObject.SetActive(false);
                return false;
            }
            return true;
        }
    }
}