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
            var orig = __instance.gameObject;

            Log.LogInfo(__instance.gameObject.name + "     " + __instance.FsmName + "        " + __instance.gameObject.scene.name);
            int index1 = 0;
            for(; index1 < PatchedFsm.patchedFsms.Length; index1++)
            {
                if(String.Equals(__instance.gameObject.scene.name, PatchedFsm.patchedFsms[index1].sceneName, StringComparison.OrdinalIgnoreCase)) break;
                if(index1 == PatchedFsm.patchedFsms.Length - 1) return;
            }


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
        [HarmonyPatch(typeof(SceneParticlesController), "OnPositionedAtHero")]
        private static bool Prefix(SceneParticlesController __instance)
        {
            if(customScenes.Find(item => item.sceneName == SceneManager.GetActiveScene().name) == null) return true;
            Log.LogInfo("cleared OnPositionedAtHero");
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameManager), "PlayerDead")]
        public static bool PlayerDead_Prefix(ref float waitTime)
        {
            if(!BossSequence.isInSequence) return true;
            waitTime = 0f;
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ScenePreloader), "SpawnPreloader")]
        public static bool ScenePreloader_Prefix(string sceneName, LoadSceneMode mode)
        {
            if(BossSequence.isInSequence) return false;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HeroController), "Awake")]
        public static void HeroControllerAwake_Postfix()
        {
            if(!Preload.isInitialized) Preload.Init();
        }

        //Set Custom Bosses Hp

        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(HealthManager), "Awake")]
        // private static void HealthManagerPatch_Postfix(HealthManager __instance)
        // {
        //     if(!BossSequence.isInSequence) return;
        //     if(BossSequence.currentBoss == BossInfo.bosses["Father of the Flame"]) return;

        //     bool hasValue;
        //     BossInfo boss;

        //     if(BossStatueInfo.currentDifficultMode == "Ascended" || BossStatueInfo.currentDifficultMode == "Radiant" && BossSequence.currentBoss.ascendedVersion != null) 
        //         boss = BossSequence.currentBoss.ascendedVersion;
        //     else boss = BossSequence.currentBoss;

        //     hasValue = boss.bossesGOsInfo.TryGetValue(__instance.gameObject.name, out var bossObj);
        //     if(hasValue)
        //     {
        //         if (BossSequence.isHoG)
        //         {
        //             var origHp = (int)__instance.GetType().GetField("initHp", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

        //             var newHp = (int)((float)bossObj[BossStatueInfo.currentDifficultMode] * origHp);

        //             __instance.GetType().GetField("initHp", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, newHp);
        //             __instance.hp = newHp;
        //         }
        //         if (BossSequence.isPantheon)
        //         {
        //             var origHp = (int)__instance.GetType().GetField("initHp", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

        //             var newHp = (int)((float)bossObj["Attuned"] * origHp);

        //             __instance.GetType().GetField("initHp", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, newHp);
        //             __instance.hp = newHp;
        //         }
        //     }
        // }

        //if is in sequence - multiply self damage
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), "TakeHealth")]
        private static bool TakeDamagePatch_Prefix(PlayerData __instance, ref int amount)
        {
            if(!BossSequence.isInSequence) return true;

            if(BossStatueInfo.currentDifficultMode == "Ascended")
            {
                amount *= 2;
                return true;
            }
            if(BossStatueInfo.currentDifficultMode == "Radiant")
            {
                amount = int.MaxValue;
                return true;
            }

            return true;
        }

        //get HeroDeathSequence to speed up death animation
        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(HeroDeathSequence), "Awake")]
        // private static void TakeDamagePatch_Postfix(HeroDeathSequence __instance)
        // {
        //     BossSequence.deathSequenceController = __instance;
        // }

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

            if(BossSequence.currentBoss == BossInfo.bosses["Moorwing"] && __instance.gameObject.name.Contains("Mapper Sparring") || __instance.gameObject.name.Contains("Caravan Scene Loader"))
            {
                GameObject.Destroy(__instance.gameObject);
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Moorwing"] && __instance.gameObject.name.Contains("Boss Scene Loader"))
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
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
            if(BossSequence.currentBoss == BossInfo.bosses["Cogwork Dancers"] && 
                __instance.gameObject.name.Contains("Boss Loader"))
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
        private static bool TestGameObjectActivatorPatch_Prefix(TestGameObjectActivator __instance)
        {
            if(BossSequence.currentBoss == BossInfo.bosses["Lost Garmond"] && __instance.gameObject.name == "Garmond Black Threaded Scene")
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("playerDataTest", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);

                return true;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Lost Garmond"] && __instance.gameObject.name == "Pre Garmond")
            {
                __instance.gameObject.SetActive(false);
                return true;
            }
            if(BossSequence.isInSequence && __instance.gameObject.name == "Gnat Corpse Ground")
            {
                Destroy(__instance.gameObject);
                return true;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Broodmother"] && 
            __instance.gameObject.name == "Broodmother Scene Control")
            {
                var children = __instance.gameObject.transform;
                children.GetChild(0).gameObject.SetActive(false);
                children.GetChild(1).gameObject.SetActive(true);
                Destroy(__instance);
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Savage Beastfly in Far Fields"] && 
            __instance.gameObject.name == "Beastfly States")
            {
                var children = __instance.gameObject.transform;
                children.GetChild(0).gameObject.SetActive(true);
                children.GetChild(1).gameObject.SetActive(false);
                Destroy(__instance);
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Gurr the Outcast"] && 
            __instance.gameObject.name == "Boss Scene")
            {
                Destroy(__instance);
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Plasmified Zango"] && 
            __instance.gameObject.name == "Area_States")
            {
                var children = __instance.gameObject.transform;
                children.GetChild(0).gameObject.SetActive(false);
                children.GetChild(1).gameObject.SetActive(true);
                Destroy(__instance);
            }
            return true;
        }
        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(PersistentIntItem), "Awake")]
        // private static bool PersistentIntItemPatch_Postfix(PersistentIntItem __instance)
        // {
        //     if(BossSequence.currentBoss == BossInfo.bosses["Widow"] &&
        //     __instance.gameObject.name == "Bellshrine Sequence Bellhart")
        //     {
        //         __instance.ItemData.Value = -1;
        //     }
        //     return true;
        // }
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
            if(BossSequence.currentBoss == BossInfo.bosses["The Last Judge"] && __instance.gameObject.name == "Last Judge")
            {
                GameObject.Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Plasmified Zango"] && __instance.gameObject.name == "Blue Assistant")
            {
                GameObject.Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Clover Dancers"] && __instance.gameObject.name == "Dancer A")
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
            if(BossSequence.isInSequence && __instance.gameObject.name == "Churchkeeper Basement")
            {
                Destroy(__instance.gameObject);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Lace in Deep Docks"] &&
                __instance.gameObject.name == "Boss Scene")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Skull Tyrant"] &&
                __instance.gameObject.name == "Skull King")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["First Sinner"] &&
                __instance.gameObject.name == "Boss Scene")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Lace in the Cradle"] &&
                __instance.gameObject.name == "Lace Return Corpse")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Second Sentiel"] &&
                __instance.gameObject.name == "Boss Scene - To Additive Load")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["The Unravelled"] &&
                __instance.gameObject.name == "Boss Scene")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Voltvyrm"] &&
                __instance.gameObject.name == "boss_eggshell")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Voltvyrm"] &&
                __instance.gameObject.name == "Zap Core Enemy")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Palestag"] &&
                __instance.gameObject.name == "Cloverstag White Boss")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Shrine Guardian Seth"] &&
                __instance.gameObject.name == "Seth" || __instance.gameObject.name == "Flower Gate")
            {
                Destroy(__instance);
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
            if(BossSequence.currentBoss == BossInfo.bosses["Skull Tyrant"] && __instance.gameObject.name == "Corpse"){
                __instance.gameObject.SetActive(false);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Raging Conchfly"] &&__instance.gameObject.name == "Boss Corpse Scene"){
                __instance.gameObject.SetActive(false);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Watcher at the Edge"] &&__instance.gameObject.name == "Collectable Item Pickup"){
                Destroy(__instance.gameObject);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Disgraced Chef Lugoli"] &&__instance.gameObject.name == "Chef Corpse Prepare Scene"){
                Destroy(__instance.gameObject);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Voltvyrm"] &&__instance.gameObject.name == "Return Aftermath"){
                Destroy(__instance.gameObject);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ActivateIfPlayerdataFalse), "Start")]
        private static bool ActivateIfPlayerdataFalse_Prefix(ActivateIfPlayerdataFalse __instance)
        {
            if(BossSequence.currentBoss == BossInfo.bosses["Savage Beastfly in Chapel of The Beast"] && __instance.gameObject.name == "Boss Control"){
                __instance.objectToActivate.SetActive(true);
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Disgraced Chef Lugoli"] && __instance.gameObject.name == "Battle Scene"){
                __instance.objectToActivate.SetActive(true);
                Destroy(__instance);
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ActivateIfPlayerdataTrue), "OnEnable")]
        private static bool ActivateIfPlayerdataTrue_Prefix(ActivateIfPlayerdataTrue __instance)
        {
            if(BossSequence.currentBoss == BossInfo.bosses["Savage Beastfly in Chapel of The Beast"] && __instance.gameObject.name == "Boss Control"){
                __instance.objectToActivate.SetActive(false);
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Father of the Flame"] && 
                __instance.gameObject.name == "Boss Scene"){
                __instance.objectToActivate.SetActive(false);
                Destroy(__instance);
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerDataTestResponse), "OnEnable")]
        private static bool PlayerDataTestResponse_Prefix(PlayerDataTestResponse __instance)
        {
            if(BossSequence.currentBoss == BossInfo.bosses["Sister Splinter"] && 
                    __instance.gameObject.name == "Boss Scene Parent"){
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Garmond & Zaza"] && 
                    __instance.gameObject.name == "Scene Control"){
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Lace in the Cradle"] && 
                    __instance.gameObject.name == "Boss Scene"){
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Lost Garmond"] && 
                    __instance.gameObject.name == "Garmond Defeated Scene"){
                Destroy(__instance.gameObject);
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StateChangeSequence), "CheckCompleteBool")]
        private static bool StateChangeSequenceCheckCompleteBool_Prefix(PlayerDataTestResponse __instance, ref bool __result)
        {
            if(BossSequence.currentBoss == BossInfo.bosses["Widow"] && __instance.gameObject.name == "Bellshrine Sequence Bellhart"){
                __result = true;
                return false;
            }
            return true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BattleScene), "Awake")]
        private static void BattleSceneAwake_Postfix(BattleScene __instance)
        {
            if(BossSequence.currentBoss == BossInfo.bosses["Broodmother"] && 
                    __instance.gameObject.name == "Battle Scene Broodmother"){
                __instance.setPDBoolOnEnd = null;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Disgraced Chef Lugoli"] && 
                    __instance.gameObject.name == "Battle Scene"){
                __instance.setPDBoolOnEnd = null;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Groal the Great"] && 
                    __instance.gameObject.name == "Battle Scene"){
                __instance.setPDBoolOnEnd = null;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Raging Conchfly"] && 
                    __instance.gameObject.name == "Battle Scene"){
                __instance.setPDBoolOnEnd = null;
            }
            if(BossSequence.currentBoss == BossInfo.bosses["Crawfather"] && 
                    __instance.gameObject.name == "Battle Scene"){
                __instance.setPDBoolOnEnd = null;
                __instance.activeAfterBattle = null;
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomSceneManager), "Awake")]
        private static bool SceneManagerAwake_Prefix(CustomSceneManager __instance)
        {
            var scene = customScenes.Find((item) => item.sceneName == GodsOfPharloomMod.currentScene);
            if(scene != null && !scene.isSkongScene) __instance.scenePools = new SceneObjectPool[0];

            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SceneColorManager), "PairKeyframes")]
        private static bool SceneColorManagerPairKeyframes_Prefix(SceneColorManager __instance)
        {
            var scene = customScenes.Find((item) => item.sceneName == GodsOfPharloomMod.currentScene);
            if(scene != null && !scene.isSkongScene) return false;

            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SceneColorManager), "CreateCurveFromKeyframes")]
        private static bool SceneColorManagerCreateCurveFromKeyframes_Prefix(SceneColorManager __instance)
        {
            var scene = customScenes.Find((item) => item.sceneName == GodsOfPharloomMod.currentScene);
            if(scene != null && !scene.isSkongScene) return false;

            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomSceneManager), "DrawBlackBorders")]
        private static bool SceneManagerDrawBlackBorders_Prefix(CustomSceneManager __instance)
        {
            var scene = customScenes.Find((item) => item.sceneName == GodsOfPharloomMod.currentScene);
            if(scene != null && !scene.isSkongScene) return false;

            return true;
        }
    }
}