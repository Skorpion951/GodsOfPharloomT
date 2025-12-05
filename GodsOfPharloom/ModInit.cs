using BepInEx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Reflection;
using HarmonyLib;

namespace Gods_Of_Pharloom
{
    [BepInPlugin("bepinex.plugin.test", "Test", "0.0.0.1")]
    public partial class GodsOfPharloomMod : BaseUnityPlugin
    {
        public static object obj;
        private static string[] assetBundleNames =
        {
            "gg_pharloom_atrium", "gg_moss_mother"
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
        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(GodsOfPharloomMod), null);
            InitCustomScenes();
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");
            SceneManager.activeSceneChanged += OnSceneChanged;
            Log = this.Logger;

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
                TransitionPoint.TransitionPoints[0].targetScene = "Coral_29";
                TransitionPoint.TransitionPoints[0].entryPoint = "left1";
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
                return;
                if(tp1 != null)
                {
                    tp1.targetScene = "Belltown";
                    tp1.entryPoint = "door1";
                    tp1.entryOffset = new Vector2(0, 0);
                    // tp1.GetType().GetField("ignoredInput", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(tp1, true);
                    tp1.GetType().GetField("isADoor", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(tp1, true);
                    // tp1.GetType().GetField("activated", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(tp1, true);
                }
            }
        }

        void InitCustomScenes()
        {
            var GG_Pharloom_Atrium = new CustomScene("GG_Pharloom_Atrium");
            GG_Pharloom_Atrium.Add("door1", new Vector3(79.68f, 73.9f, 0), new TransitionPointInfo("Belltown", "door1", isADoor: true));
            GG_Pharloom_Atrium.Add("door2", new Vector3(57.5f, 54f, 0), new TransitionPointInfo("GG_Moss_Mother", "door1", isADoor: true));
            GG_Pharloom_Atrium.AfterSceneActivated += () => {GG_Pharloom_Atrium.isSceneActive = true;};
            customScenes.Add(GG_Pharloom_Atrium);
        }
    }
}