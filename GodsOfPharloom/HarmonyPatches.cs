using BepInEx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEngine.AddressableAssets;
using HarmonyLib;
using System.Collections;
using HutongGames.PlayMaker;
using GlobalSettings;

namespace Gods_Of_Pharloom
{
    public partial class GodsOfPharloomMod : BaseUnityPlugin
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayMakerFSM), "Awake")]
        private static void PlayMakerPatch_Postfix(PlayMakerFSM __instance)
        {
            var orig = __instance.gameObject;
            int objNameHash = orig.gameObject.name.GetHashCode();
            int fsmNameHash = __instance.FsmName.GetHashCode();
            int sceneNameHash = orig.scene.name.GetHashCode();

            Log.LogInfo(__instance.gameObject.name + "     " + __instance.FsmName + "        " + __instance.gameObject.scene.name);

            int index1 = 0;
            for(; index1 < PatchedFsm.patchedFsms.Length; index1++)
            {
                if(sceneNameHash == PatchedFsm.patchedFsms[index1].sceneNameHash) break;
                if(index1 == PatchedFsm.patchedFsms.Length - 1) return;
            }

            var patchedFsm = PatchedFsm.patchedFsms[index1];

            foreach(var item in patchedFsm.fsms)
            {
                if(objNameHash == item.objNameHash && fsmNameHash == item.fsmNameHash)
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

        // init preloads
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HeroController), "Awake")]
        public static void HeroControllerAwake_Postfix()
        {
            if(!Preload.isInitialized) Preload.Init();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "ResetAllCrestState")]
        public static bool HeroControllerResetAllCrestState_Prefix()
        {
            GodsOfPharloomMod.Log.LogInfo("ResetAllCrestState");
            if(BossSequence.isInSequence && !PlayerData.instance.atBench) return false;
            GodsOfPharloomMod.Log.LogInfo("DoResetAllCrestState");
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "ClearEffects")]
        public static bool HeroControllerClearEffects_Prefix()
        {
            GodsOfPharloomMod.Log.LogInfo("ClearEffects");
            if(BossSequence.isInSequence && !PlayerData.instance.atBench) return false;
            GodsOfPharloomMod.Log.LogInfo("DoClearEffects");
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "ClearEffectsInstant")]
        public static bool HeroControllerClearEffectsInstant_Prefix()
        {
            GodsOfPharloomMod.Log.LogInfo("ClearEffectsInstant");
            if(BossSequence.isInSequence && !PlayerData.instance.atBench) return false;
            GodsOfPharloomMod.Log.LogInfo("DoClearEffectsInstant");
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "ClearEffectsLite")]
        public static bool HeroControllerClearEffectsLite_Prefix()
        {
            GodsOfPharloomMod.Log.LogInfo("ClearEffectsLite");
            if(BossSequence.isInSequence && !PlayerData.instance.atBench) return false;
            GodsOfPharloomMod.Log.LogInfo("DoClearEffectsLite");
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "MaxHealth")]
        public static bool HeroControllerMaxHealth_Prefix()
        {
            GodsOfPharloomMod.Log.LogInfo("MaxHealth");
            if(BossSequence.isInSequence && !PlayerData.instance.atBench) return false;
            GodsOfPharloomMod.Log.LogInfo("DoMaxHealth");
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroController), "RefillHealthToMax")]
        public static bool HeroControllerRefillHealthToMax_Prefix()
        {
            GodsOfPharloomMod.Log.LogInfo("RefillHealthToMax");
            if(BossSequence.isInSequence && !PlayerData.instance.atBench) return false;
            GodsOfPharloomMod.Log.LogInfo("DoRefillHealthToMax");
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), "MaxHealth")]
        public static bool PlayerDataMaxHealth_Prefix()
        {
            GodsOfPharloomMod.Log.LogInfo("MaxHealth");
            if(BossSequence.isInSequence && !PlayerData.instance.atBench) return false;
            GodsOfPharloomMod.Log.LogInfo("DoMaxHealth");
            return true;
        }

        // remove enemies armor
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HealthManager), "ApplyDamageScaling")]
        public static bool HealthManagerApplyDamageScaling_Prefix(HealthManager __instance, HitInstance hitInstance, 
            ref HitInstance __result)
        {
            if (BossSequence.isInSequence)
            {
                __result = hitInstance;
                return false;
            }

            return true;
        }

        //nail binding
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerData), "get_nailDamage")]
        public static void PlayerDataNailDamage_Postfix(ref int __result)
        {
            if (PlayerDataMod.instance.bindings["Needle Binding"])
            {
                var nailDamage = __result;

                if(nailDamage > 13)
                {
                    nailDamage = 13;
                    __result = nailDamage;

                    return;
                }
                else if(nailDamage > 9 && nailDamage < 14) nailDamage = 10;
                else if(nailDamage > 5 && nailDamage < 10) nailDamage = 7;
                else if(nailDamage > 3 && nailDamage < 6) nailDamage = 4;
                else nailDamage = 1;
                
                __result = nailDamage;
            }
        }

        //Set Custom Bosses Hp

        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(HealthManager), "Awake")]
        // private static void HealthManagerPatch_Postfix(HealthManager __instance)
        // {
        //     if(!BossSequence.isInSequence) return;
        //     if(BossSequence.currentSequenceScene == BossScene.bosses["Father of the Flame"]) return;

        //     bool hasValue;
        //     BossScene boss;

        //     if(BossSequence.currentDifficultMode == "Ascended" || BossSequence.currentDifficultMode == "Radiant" && BossSequence.currentSequenceScene.ascendedVersion != null) 
        //         boss = BossSequence.currentSequenceScene.ascendedVersion;
        //     else boss = BossSequence.currentSequenceScene;

        //     hasValue = boss.bossesGOsInfo.TryGetValue(__instance.gameObject.name, out var bossObj);
        //     if(hasValue)
        //     {
        //         if (BossSequence.isHoG)
        //         {
        //             var origHp = (int)__instance.GetType().GetField("initHp", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

        //             var newHp = (int)((float)bossObj[BossSequence.currentDifficultMode] * origHp);

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
        private static bool TakeDamagePatch_Prefix(PlayerData __instance, ref int amount, ref bool hasBlueHealth, ref bool allowFracturedMaskBreak)
        {
            if(!BossSequence.isInSequence) return true;

            BossSequence.hitCounter++;

            if(BossSequence.currentDifficultMode == "Ascended")
            {
                amount *= 2;
                return true;
            }
            if(BossSequence.currentDifficultMode == "Radiant")
            {
                amount = int.MaxValue;
                return true;
            }

            ToolItem fracturedMaskTool = Gameplay.FracturedMaskTool;
            if(amount >= (__instance.health + __instance.healthBlue) &&
               !fracturedMaskTool.IsEquipped || (fracturedMaskTool.SavedData.AmountLeft < 1)
            )
            {
                BossSequence.isHeroDead = true;

                GodsOfPharloomMod.Log.LogInfo("HERO DEAD YOOOOO");
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SceneAdditiveLoadConditional), "OnEnable")]
        private static bool SceneAdditiveLoadPatch_Prefix(SceneAdditiveLoadConditional __instance)
        {
            if(BossSequence.isInSequence && __instance.gameObject.name.Contains("Bellway Additive Loader") ||
               __instance.gameObject.name.Contains("Bell Centipede Loader"))
            {
                Destroy(__instance.gameObject);
                return false;
            }
            if(BossSequence.isInSequence && __instance.gameObject.name.Contains("Boss Loader"))
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo doorBlackList = __instance.GetType().GetField("doorBlackList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo otherLoaderBlacklist = __instance.GetType().GetField("otherLoaderBlacklist", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo loadAlt = __instance.GetType().GetField("loadAlt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _additiveSceneLoads = __instance.GetType().GetField("_additiveSceneLoads", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
                doorBlackList.SetValue(__instance, new string[0]);
                otherLoaderBlacklist.SetValue(__instance, new SceneAdditiveLoadConditional[0]);
                loadAlt.SetValue(__instance, false);

                return true;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Fourth Chorus"] && __instance.gameObject.name.Contains("Boss Golem Loader"))
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo doorBlackList = __instance.GetType().GetField("doorBlackList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo otherLoaderBlacklist = __instance.GetType().GetField("otherLoaderBlacklist", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo loadAlt = __instance.GetType().GetField("loadAlt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _additiveSceneLoads = __instance.GetType().GetField("_additiveSceneLoads", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
                doorBlackList.SetValue(__instance, new string[0]);
                otherLoaderBlacklist.SetValue(__instance, new SceneAdditiveLoadConditional[0]);
                loadAlt.SetValue(__instance, false);

                return true;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Fourth Chorus"] && __instance.gameObject.name.Contains("Boss Beastfly Loader"))
            {
                Destroy(__instance.gameObject);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Savage Beastfly in Far Fields"] && __instance.gameObject.name.Contains("Boss Beastfly Loader"))
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo doorBlackList = __instance.GetType().GetField("doorBlackList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo otherLoaderBlacklist = __instance.GetType().GetField("otherLoaderBlacklist", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo loadAlt = __instance.GetType().GetField("loadAlt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _additiveSceneLoads = __instance.GetType().GetField("_additiveSceneLoads", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
                doorBlackList.SetValue(__instance, new string[0]);
                otherLoaderBlacklist.SetValue(__instance, new SceneAdditiveLoadConditional[0]);
                loadAlt.SetValue(__instance, false);

                return true;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Savage Beastfly in Far Fields"] && __instance.gameObject.name.Contains("Boss Golem Loader"))
            {
                Destroy(__instance.gameObject);
                return false;
            }
            if(BossSequence.isInSequence && __instance.gameObject.name.Contains("Rest Golem Loader"))
            {
                Destroy(__instance.gameObject);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Shakra"] && __instance.gameObject.name.Contains("Mapper Sparring"))
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo doorBlackList = __instance.GetType().GetField("doorBlackList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo otherLoaderBlacklist = __instance.GetType().GetField("otherLoaderBlacklist", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo loadAlt = __instance.GetType().GetField("loadAlt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _additiveSceneLoads = __instance.GetType().GetField("_additiveSceneLoads", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
                doorBlackList.SetValue(__instance, new string[0]);
                otherLoaderBlacklist.SetValue(__instance, new SceneAdditiveLoadConditional[0]);
                loadAlt.SetValue(__instance, false);

                return true;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Shakra"] && __instance.gameObject.name.Contains("Boss Scene Loader") || __instance.gameObject.name.Contains("Caravan Scene Loader"))
            {
                GameObject.Destroy(__instance.gameObject);
                return false;
            }

            if(BossSequence.currentSequenceScene == BossScene.bosses["Moorwing"] && __instance.gameObject.name.Contains("Mapper Sparring") || __instance.gameObject.name.Contains("Caravan Scene Loader"))
            {
                GameObject.Destroy(__instance.gameObject);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Moorwing"] && __instance.gameObject.name.Contains("Boss Scene Loader"))
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo doorBlackList = __instance.GetType().GetField("doorBlackList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo otherLoaderBlacklist = __instance.GetType().GetField("otherLoaderBlacklist", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo loadAlt = __instance.GetType().GetField("loadAlt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _additiveSceneLoads = __instance.GetType().GetField("_additiveSceneLoads", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
                doorBlackList.SetValue(__instance, new string[0]);
                otherLoaderBlacklist.SetValue(__instance, new SceneAdditiveLoadConditional[0]);
                loadAlt.SetValue(__instance, false);
                _additiveSceneLoads.SetValue(__instance, new List<SceneAdditiveLoadConditional>());

                return true;
            }

            if(BossSequence.currentSequenceScene == BossScene.bosses["The Unravelled"] && __instance.gameObject.name.Contains("Boss Loader"))
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);

                return true;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Bell Beast"] && __instance.gameObject.name.Contains("Boss Additive Loader"))
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo doorBlackList = __instance.GetType().GetField("doorBlackList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo otherLoaderBlacklist = __instance.GetType().GetField("otherLoaderBlacklist", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo loadAlt = __instance.GetType().GetField("loadAlt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo _additiveSceneLoads = __instance.GetType().GetField("_additiveSceneLoads", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);
                doorBlackList.SetValue(__instance, new string[0]);
                otherLoaderBlacklist.SetValue(__instance, new SceneAdditiveLoadConditional[0]);
                loadAlt.SetValue(__instance, false);

                return true;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Cogwork Dancers"] && 
                __instance.gameObject.name.Contains("Boss Loader"))
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("tests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);

                return true;
            }

            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TestGameObjectActivator), "OnEnable")]
        private static bool TestGameObjectActivatorPatch_Prefix(TestGameObjectActivator __instance)
        {
            if(BossSequence.currentSequenceScene == BossScene.bosses["Lost Garmond"] && __instance.gameObject.name == "Garmond Black Threaded Scene")
            {
                FieldInfo questTests = __instance.GetType().GetField("questTests", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo tests = __instance.GetType().GetField("playerDataTest", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var playerDataTest = new PlayerDataTest();

                questTests.SetValue(__instance, new QuestTest[0]);
                tests.SetValue(__instance, playerDataTest);

                return true;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Lost Garmond"] && __instance.gameObject.name == "Pre Garmond")
            {
                __instance.gameObject.SetActive(false);
                return true;
            }
            if(BossSequence.isInSequence && __instance.gameObject.name == "Gnat Corpse Ground")
            {
                Destroy(__instance.gameObject);
                return true;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Broodmother"] && 
            __instance.gameObject.name == "Broodmother Scene Control")
            {
                var children = __instance.gameObject.transform;
                children.GetChild(0).gameObject.SetActive(false);
                children.GetChild(1).gameObject.SetActive(true);
                Destroy(__instance);

                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Savage Beastfly in Far Fields"] && 
            __instance.gameObject.name == "Beastfly States")
            {
                var children = __instance.gameObject.transform;
                children.GetChild(0).gameObject.SetActive(true);
                children.GetChild(1).gameObject.SetActive(false);
                Destroy(__instance);

                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Gurr the Outcast"] && 
            __instance.gameObject.name == "Boss Scene")
            {
                Destroy(__instance);

                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Plasmified Zango"] && 
            __instance.gameObject.name == "Area_States")
            {
                var children = __instance.gameObject.transform;
                children.GetChild(0).gameObject.SetActive(false);
                children.GetChild(1).gameObject.SetActive(true);
                Destroy(__instance);

                return false;
            }
            return true;
        }
        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(PersistentIntItem), "Awake")]
        // private static bool PersistentIntItemPatch_Postfix(PersistentIntItem __instance)
        // {
        //     if(BossSequence.currentSequenceScene == BossScene.bosses["Widow"] &&
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
            if(BossSequence.currentSequenceScene == BossScene.bosses["Moss Mother"] && __instance.gameObject.name == "Battle Scene" || __instance.gameObject.name == "Boss Scene")
            {
                GameObject.Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Great Conchflies"] && __instance.gameObject.name == "Driller A" || __instance.gameObject.name == "Driller B")
            {
                GameObject.Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["The Last Judge"] && __instance.gameObject.name == "Last Judge")
            {
                GameObject.Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Plasmified Zango"] && __instance.gameObject.name == "Blue Assistant")
            {
                GameObject.Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Clover Dancers"] && __instance.gameObject.name == "Dancer A")
            {
                GameObject.Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Forum Battle"] && __instance.gameObject.name == "Battle Scene" || __instance.gameObject.name == "Start Range" ||
               __instance.gameObject.name.Contains("Song Handmaiden") || __instance.gameObject.name.Contains("City Merchant Scavenge Generic"))
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
            if(BossSequence.currentSequenceScene == BossScene.bosses["Moss Mother"] && __instance.gameObject.name == "Battle Scene"){
                GameObject.Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Bell Beast"] && __instance.gameObject.name == "Boss Scene"){
                GameObject.Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Fourth Chorus"] && __instance.gameObject.name == "Lava Rocks"){
                GameObject.Destroy(__instance);
                return false;
            }
            if(BossSequence.isInSequence && __instance.gameObject.name == "Churchkeeper Basement")
            {
                Destroy(__instance.gameObject);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Lace in Deep Docks"] &&
                __instance.gameObject.name == "Boss Scene")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Skull Tyrant"] &&
                __instance.gameObject.name == "Skull King")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["First Sinner"] &&
                __instance.gameObject.name == "Boss Scene")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Lace in the Cradle"] &&
                __instance.gameObject.name == "Lace Return Corpse")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Second Sentiel"] &&
                __instance.gameObject.name == "Boss Scene - To Additive Load")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["The Unravelled"] &&
                __instance.gameObject.name == "Boss Scene")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Voltvyrm"] &&
                __instance.gameObject.name == "boss_eggshell")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Voltvyrm"] &&
                __instance.gameObject.name == "Zap Core Enemy")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Voltvyrm"] &&
                __instance.gameObject.name == "Hunter Fan Outside")
            {
                Destroy(__instance.gameObject);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Palestag"] &&
                __instance.gameObject.name == "Cloverstag White Boss")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Shrine Guardian Seth"] &&
                __instance.gameObject.name == "Seth" || __instance.gameObject.name == "Flower Gate")
            {
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Lace in the Cradle"] &&
                __instance.gameObject.name == "Lace Boss2 New")
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
            if(BossSequence.currentSequenceScene == BossScene.bosses["Great Conchflies"] && __instance.gameObject.name == "Coral Driller Return Corpse"){
                __instance.gameObject.SetActive(false);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Skull Tyrant"] && __instance.gameObject.name == "Corpse" || __instance.gameObject.name == "Hunter Fan Outside Rummage (1)"){
                __instance.gameObject.SetActive(false);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Raging Conchfly"] &&__instance.gameObject.name == "Boss Corpse Scene"){
                __instance.gameObject.SetActive(false);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Watcher at the Edge"] &&__instance.gameObject.name == "Collectable Item Pickup"){
                Destroy(__instance.gameObject);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Disgraced Chef Lugoli"] &&__instance.gameObject.name == "Chef Corpse Prepare Scene"){
                Destroy(__instance.gameObject);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Voltvyrm"] &&__instance.gameObject.name == "Return Aftermath"){
                Destroy(__instance.gameObject);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Phantom"] &&__instance.gameObject.name == "Return Mask"){
                Destroy(__instance.gameObject);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ActivateIfPlayerdataFalse), "Start")]
        private static bool ActivateIfPlayerdataFalse_Prefix(ActivateIfPlayerdataFalse __instance)
        {
            if(BossSequence.currentSequenceScene == BossScene.bosses["Savage Beastfly in Chapel of The Beast"] && __instance.gameObject.name == "Boss Control"){
                __instance.objectToActivate.SetActive(true);
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Disgraced Chef Lugoli"] && __instance.gameObject.name == "Battle Scene"){
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
            if(BossSequence.currentSequenceScene == BossScene.bosses["Savage Beastfly in Chapel of The Beast"] && __instance.gameObject.name == "Boss Control"){
                __instance.objectToActivate.SetActive(false);
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Father of the Flame"] && 
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
            if(BossSequence.currentSequenceScene == BossScene.bosses["Sister Splinter"] && 
                    __instance.gameObject.name == "Boss Scene Parent"){
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Garmond & Zaza"] && 
                    __instance.gameObject.name == "Scene Control"){
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Lace in the Cradle"] && 
                    __instance.gameObject.name == "Boss Scene"){
                Destroy(__instance);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Lost Garmond"] && 
                    __instance.gameObject.name == "Garmond Defeated Scene"){
                Destroy(__instance.gameObject);
                return false;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Crawfather"] && 
                    __instance.gameObject.name == "grey_lever_gate"){
                __instance.gameObject.GetComponent<Gate>().ForceClose();
                Destroy(__instance);
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StateChangeSequence), "CheckCompleteBool")]
        private static bool StateChangeSequenceCheckCompleteBool_Prefix(PlayerDataTestResponse __instance, ref bool __result)
        {
            if(BossSequence.currentSequenceScene == BossScene.bosses["Widow"] && __instance.gameObject.name == "Bellshrine Sequence Bellhart"){
                __result = true;
                return false;
            }
            return true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BattleScene), "Awake")]
        private static void BattleSceneAwake_Postfix(BattleScene __instance)
        {
            if(BossSequence.currentSequenceScene == BossScene.bosses["Broodmother"] && 
                    __instance.gameObject.name == "Battle Scene Broodmother"){
                __instance.setPDBoolOnEnd = null;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Disgraced Chef Lugoli"] && 
                    __instance.gameObject.name == "Battle Scene"){
                __instance.setPDBoolOnEnd = null;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Groal the Great"] && 
                    __instance.gameObject.name == "Battle Scene"){
                __instance.setPDBoolOnEnd = null;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Raging Conchfly"] && 
                    __instance.gameObject.name == "Battle Scene"){
                __instance.setPDBoolOnEnd = null;
            }
            if(BossSequence.currentSequenceScene == BossScene.bosses["Crawfather"] && 
                    __instance.gameObject.name == "Battle Scene"){
                __instance.setPDBoolOnEnd = null;
                __instance.activeAfterBattle = null;
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HarpoonRingSlider), "Awake")]
        private static void HarpoonRingSliderAwake_Prefix(HarpoonRingSlider __instance)
        {
            if(BossSequence.currentSequenceScene == BossScene.bosses["Cogwork Dancers"]){
                Destroy(__instance.gameObject);
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomSceneManager), "Awake")]
        private static bool SceneManagerAwake_Prefix(CustomSceneManager __instance)
        {
            var scene = customScenes.Find((item) => item.sceneName == GodsOfPharloomMod.currentSceneName);
            if(scene != null && !scene.isSkongScene) __instance.scenePools = new SceneObjectPool[0];

            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CustomSceneManager), "DrawBlackBorders")]
        private static bool SceneManagerDrawBlackBorders_Prefix(CustomSceneManager __instance)
        {
            var scene = customScenes.Find((item) => item.sceneName == GodsOfPharloomMod.currentSceneName);
            if(scene != null && !scene.isSkongScene) return false;

            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(InventoryItemCollectable), "Submit")]
        private static bool InventoryItemCollectableSubmit_Prefix(InventoryItemCollectable __instance)
        {
            if(!__instance.transform.IsChildOf(BindingsMenu.menuBindings.transform)) return true;

            if(BindingsMenu.TryShowSequenceMsg()) return false;

            if(BindingsMenu.submitActions.TryGetValue(__instance.gameObject.name, out Action<bool> val))
            {
                val?.Invoke(false);
                return false;
            }

            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(InventoryItemSelectable), "Submit")]
        private static bool InventoryItemSelectableSubmit_Prefix(InventoryItemSelectable __instance)
        {
            if(!__instance.transform.IsChildOf(BindingsMenu.menuBindings.transform)) return true;

            if(BindingsMenu.TryShowSequenceMsg()) return false;

            if(BindingsMenu.submitActions.TryGetValue(__instance.gameObject.name, out Action<bool> val))
            {
                val?.Invoke(false);
                return false;
            }

            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(InventoryItemTool), "Submit")]
        private static bool InventoryItemToolSubmit_Prefix(InventoryItemTool __instance)
        {
            if(!PlayerDataMod.instance.bindings["Tools Binding"]) return true;
            if(__instance.name != "Silk Spear" && __instance.name != "Thread Sphere" &&
               __instance.name != "Parry" && __instance.name != "Silk Charge" &&
               __instance.name != "Silk Bomb" && __instance.name != "Silk Boss Needle")
            {
                if(BindingsMenu.submitActions.TryGetValue("Tools Buttons Msg", out Action<bool> val))
                {
                    val?.Invoke(false);
                    return false;
                }
            }

            return true;
        }
        // [HarmonyPrefix] //for unity explorer
        // [HarmonyPatch(typeof(CacheObjectBase), "SetValueState")]
        // public static bool blehhh(CacheObjectBase __instance, CacheObjectCell cell, CacheObjectBase.ValueStateArgs args)
        // {
        //     if (cell is not CacheListEntryCell listEntry)
        //         return true;


        //     void Convert(string name)
        //     {
        //         AccessTools.Property(typeof(CacheObjectBase), "ValueLabelText")
        //             .SetValue(
        //                 __instance, 
        //                 UniverseLib.Utility.ToStringUtility.ToStringWithType(
        //                     __instance.Value, 
        //                     __instance.FallbackType, 
        //                     true
        //                     )
        //                 + $" - <i><color=#b0edff>{name}</color></i>"
        //             );
        //     }

        //     switch (__instance.Value)
        //     {
        //         case FsmState fsm: Convert(fsm.Name); break;
        //         case FsmEvent fsm: Convert(fsm.Name); break;
        //         case FsmStateAction fsm: Convert(fsm.Name); break;
        //         case FsmVar fsm: Convert(fsm.NamedVar.Name); break;
        //         // add additional type handling if u want
        //     }

        //     return true;
        // }
    }
}