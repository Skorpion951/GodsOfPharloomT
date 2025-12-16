using BepInEx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Reflection;
using HarmonyLib;
using Unity.Mathematics;
using Newtonsoft.Json;

namespace Gods_Of_Pharloom
{
    [BepInPlugin("bepinex.plugin.test", "Test", "0.0.0.1")]
    public partial class GodsOfPharloomMod : BaseUnityPlugin
    {
        string pathToModData = BepInEx.Paths.ConfigPath + "/" +"GodsOfPharloomData.bin";
        public static PlayerDataMod playerData;
        public static object obj;
        private static string[] assetBundleNames =
        {
            "gg_pharloom_atrium", "gg_pharloom_hall_of_gods"
        };
        public static List<AssetBundle> assetBundles = new List<AssetBundle>();
        public static List<CustomScene> customScenes = new List<CustomScene>();
        public static BepInEx.Logging.ManualLogSource Log;
        Keyboard keyboard;
        public void LoadBundle(string bundleName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            foreach (string res in asm.GetManifestResourceNames())
            {
                string name = Path.GetExtension(res).Substring(1);
                if (name != bundleName) continue;

                AssetBundle bundle;
                using (Stream s = asm.GetManifestResourceStream(res))
                {
                    if (s == null) continue;
                    byte[] buffer = new byte[s.Length];
                    s.Read(buffer, 0, buffer.Length);
                    s.Dispose();
                    Logger.LogInfo("Loading bundle " + bundleName);
                    bundle = AssetBundle.LoadFromMemory(buffer);
                    assetBundles.Add(bundle);
                }
                Logger.LogInfo(bundle.GetAllScenePaths()[0]);
            }
        }
        public void LoadModData()
        {
            if (!File.Exists(pathToModData))
            {
                var newData = new PlayerDataMod();
                var jsonString = JsonUtility.ToJson(newData, true);
                File.WriteAllText(pathToModData, jsonString);
            }

            var json = File.ReadAllText(pathToModData);
            playerData = JsonUtility.FromJson<PlayerDataMod>(json);
        }
        public void SaveModData()
        {
            var jsonString = JsonUtility.ToJson(playerData, true);
            File.WriteAllText(pathToModData, jsonString);
        }
        private void Awake()
        {
            Log = this.Logger;
            try{
            Logger.LogInfo($"Plugin is loaded!");
            BossInfo.InitBossesInfo();
            BossStatueInfo.InitBossesStatue();
            LoadModData();
            BossStatueInfo.GetBadges();

            }
            catch(Exception ex)
            {
                Logger.LogInfo(ex.Message);
            }



            Harmony.CreateAndPatchAll(typeof(GodsOfPharloomMod), null);
            InitCustomScenes();
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");
            SceneManager.activeSceneChanged += OnSceneChanged;

            foreach(string name in assetBundleNames)
            {
                LoadBundle(name);
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
        public System.Collections.IEnumerator LoadScene()
        {
            var op = UnityEngine.AddressableAssets.Addressables.LoadSceneAsync("Scenes/Bone_East_08_Boss_Beastfly", UnityEngine.SceneManagement.LoadSceneMode.Additive);
            GodsOfPharloomMod.Log.LogInfo("000000000000000000");
            yield return op;
        }

        void Update()
        {
            keyboard = Keyboard.current;
            if (keyboard.nKey.wasPressedThisFrame)
            {
                EventRegister.SendEvent("BATTLE LOCK");
                SceneData.instance.PersistentBools.SetValue(new PersistentItemData<bool>
                {
                    SceneName = "GG_Moss_Mother",
                    ID = "Battle Scene",
                    Value = false
                });
                PlayerData.instance.defeatedMossMother = false;
            }
            if (keyboard.mKey.wasPressedThisFrame)
            {
                StartCoroutine(LoadScene());
            }
        }

        void OnSceneChanged(Scene from, Scene to)
        {
            if(to.name == "Ant_17")
            {
                TransitionPoint.TransitionPoints[0].targetScene = "GG_Pharloom_Hall_Of_Gods";
                TransitionPoint.TransitionPoints[0].entryPoint = "door1";
            }
            if(to.name == "Belltown")
            {
                foreach(var item in AssetBundle.GetAllLoadedAssetBundles().ToArray())
                {
                    Log.LogInfo(item.name);
                }
                var allObjects = to.GetRootGameObjects();
                TransitionPoint tp1;

                foreach(GameObject obj in allObjects)
                {
                    if(obj.name == "right2") {
                        tp1 = obj.GetComponent<TransitionPoint>();
                        Logger.LogInfo("left1 is here");
                        obj.name = "door1";
                        obj.gameObject.GetComponent<Transform>().position = new Vector3(obj.gameObject.GetComponent<Transform>().position.x - 10, obj.gameObject.GetComponent<Transform>().position.y, obj.gameObject.GetComponent<Transform>().position.z);
                        obj.gameObject.GetComponent<BoxCollider2D>().size = new Vector2(4, 0.1f);
                        obj.gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(0, -1.95f);
                        tp1.SetTargetScene("GG_Pharloom_Atrium");
                        tp1.entryPoint = "door1";
                        tp1.InteractLabel = InteractableBase.PromptLabels.Enter;
                        tp1.isADoor = true;
                        tp1.GetType().GetField("AudioTransitionTime", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tp1, 2.5f);
                        tp1.OnDoorEnter = new UnityEngine.Events.UnityEvent();
                        //tp1.GetType().GetField("skipSceneMapCheck", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tp1, true);
                        tp1.Activate();
                        return;
                    }
                }
            }
        }

        void InitCustomScenes()
        {
            var GG_Pharloom_Atrium = new CustomScene("GG_Pharloom_Atrium");
            GG_Pharloom_Atrium.AddTransitionPoint(new TransitionPointInfo("door1", new Vector3(79.68f, 73.9f, 0), "Belltown", "door1", isADoor: true, noInputOnStart: false));
            GG_Pharloom_Atrium.AddTransitionPoint(new TransitionPointInfo("door2", new Vector3(57.5f, 54f, 0), "GG_Pharloom_Hall_Of_Gods", "door1", isADoor: true, noInputOnStart: false));
            GG_Pharloom_Atrium.AfterSceneActivated += () => {GG_Pharloom_Atrium.isSceneActive = true;};
            customScenes.Add(GG_Pharloom_Atrium);

            var GG_Pharloom_HoG = new CustomScene("GG_Pharloom_Hall_Of_Gods");
            GG_Pharloom_HoG.AddTransitionPoint(new TransitionPointInfo("door1", new Vector3(44.64f, 52.58f, 0), "GG_Pharloom_Atrium", "door2", isADoor: true, noInputOnStart: false));
            GG_Pharloom_HoG.AfterSceneActivated += () => {
                var rootObjects = SceneManager.GetSceneByName("GG_Pharloom_Hall_Of_Gods").GetRootGameObjects();
                foreach(var obj in rootObjects)
                {
                    if(obj.name == "BossStatues")
                    {
                        var children = obj.transform;
                        foreach(Transform child in children)
                        {
                            child.gameObject.AddComponent<BossStatue>();
                        }
                        break;
                    }
                }

                GG_Pharloom_HoG.isSceneActive = true;
            };
            customScenes.Add(GG_Pharloom_HoG);

            var AbyssCocoon = new CustomScene("Abyss_Cocoon");
            AbyssCocoon.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(29.16f, 5.65f, 0), "Belltown", "door1", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            AbyssCocoon.isSkongScene = true;
            AbyssCocoon.AfterSceneActivated += () => {AbyssCocoon.isSceneActive = true;};
            customScenes.Add(AbyssCocoon);
        }
    }
}