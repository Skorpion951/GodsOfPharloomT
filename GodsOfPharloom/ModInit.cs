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
        public static GodsOfPharloomMod instance;
        string pathToModData = BepInEx.Paths.ConfigPath + "/" +"GodsOfPharloomData.bin";
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
            if (File.Exists(pathToModData))
            {
                var json = File.ReadAllText(pathToModData);
                PlayerDataMod.instance = JsonConvert.DeserializeObject<PlayerDataMod>(json);
            }
            else
            {
                PlayerDataMod.instance = new PlayerDataMod();
                SaveModData();
            }
        }
        public void SaveModData()
        {
            var jsonString = JsonConvert.SerializeObject(PlayerDataMod.instance, Formatting.Indented);
            File.WriteAllText(pathToModData, jsonString);
        }
        private void Awake()
        {
            instance = this;
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

            BossSequence.CreateSequenceController();

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
            if (keyboard.backquoteKey.wasPressedThisFrame)
            {
                FastTeleport.Start();
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

                var cameraLock1 = CustomScene.CreateCameraLock(BossStatueInfo.hog_sceneName);
                var go1 = cameraLock1.gameObject;
                go1.transform.position = new Vector3(44.64f, 90f, 0);
                cameraLock1.cameraYMin = 57f;
                cameraLock1.cameraYMax = 400f;
                cameraLock1.cameraXMin = 44.64f;
                cameraLock1.cameraXMax = 44.64f;
                go1.GetComponent<BoxCollider2D>().size = new Vector2(8f, 100f);

                GG_Pharloom_HoG.isSceneActive = true;
            };
            customScenes.Add(GG_Pharloom_HoG);


            var Tut_03 = new CustomScene("Tut_03");
            Tut_03.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(58.41f, 17.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Tut_03.isSkongScene = true;
            Tut_03.AfterSceneActivated += () => {Tut_03.isSceneActive = true;};
            customScenes.Add(Tut_03);

            var Weave_03 = new CustomScene("Weave_03");
            Weave_03.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(13.41f, 20.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Weave_03.isSkongScene = true;
            Weave_03.AfterSceneActivated += () => {Weave_03.isSceneActive = true;};
            customScenes.Add(Weave_03);

            var Bone_05 = new CustomScene("Bone_05");
            Bone_05.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(86.48f, 3.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Bone_05.isSkongScene = true;
            Bone_05.AfterSceneActivated += () => {Bone_05.isSceneActive = true;};
            customScenes.Add(Bone_05);

            var Bone_East_08 = new CustomScene("Bone_East_08");
            Bone_East_08.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(76.89f, 8.08f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false, afterTransition: () =>
            {
                GameObject.Find("Temp plat").SetActive(false);
            }));
            Bone_East_08.isSkongScene = true;
            Bone_East_08.AfterSceneActivated += () => {
                var go = new GameObject("Temp plat");
                SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName("Bone_East_08"));
                go.AddComponent<BoxCollider2D>();
                go.layer = 8;
                go.transform.position = new Vector3(76.89f, 4.9f, 0);

                Bone_East_08.isSceneActive = true;
            };
            customScenes.Add(Bone_East_08);

            var Coral_11 = new CustomScene("Coral_11");
            Coral_11.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(52.6f, 14.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Coral_11.isSkongScene = true;
            Coral_11.AfterSceneActivated += () => {Coral_11.isSceneActive = true;};
            customScenes.Add(Coral_11);

            var Bone_East_12 = new CustomScene("Bone_East_12");
            Bone_East_12.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(92f, 7.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Bone_East_12.isSkongScene = true;
            Bone_East_12.AfterSceneActivated += () => {Bone_East_12.isSceneActive = true;};
            customScenes.Add(Bone_East_12);

            var Coral_Judge_Arena = new CustomScene("Coral_Judge_Arena");
            Coral_Judge_Arena.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(34.1932f, 24.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Coral_Judge_Arena.isSkongScene = true;
            Coral_Judge_Arena.AfterSceneActivated += () => {Coral_Judge_Arena.isSceneActive = true;};
            customScenes.Add(Coral_Judge_Arena);

            var Greymoor_08 = new CustomScene("Greymoor_08");
            Greymoor_08.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(27.3f, 4.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Greymoor_08.isSkongScene = true;
            Greymoor_08.AfterSceneActivated += () => {Greymoor_08.isSceneActive = true;};
            customScenes.Add(Greymoor_08);

            var Organ_01 = new CustomScene("Organ_01");
            Organ_01.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(84.36f, 104.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Organ_01.isSkongScene = true;
            Organ_01.AfterSceneActivated += () => {Organ_01.isSceneActive = true;};
            customScenes.Add(Organ_01);

            var Ant_19 = new CustomScene("Ant_19");
            Ant_19.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(54.8f, 34.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Ant_19.isSkongScene = true;
            Ant_19.AfterSceneActivated += () => {Ant_19.isSceneActive = true;};
            customScenes.Add(Ant_19);

            var Shellwood_18 = new CustomScene("Shellwood_18");
            Shellwood_18.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(42.49f, 8.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Shellwood_18.isSkongScene = true;
            Shellwood_18.AfterSceneActivated += () => {Shellwood_18.isSceneActive = true;};
            customScenes.Add(Shellwood_18);

            var Bone_15 = new CustomScene("Bone_15");
            Bone_15.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(83.75f, 14.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Bone_15.isSkongScene = true;
            Bone_15.AfterSceneActivated += () => {Bone_15.isSceneActive = true;};
            customScenes.Add(Bone_15);

            var Belltown_Shrine = new CustomScene("Belltown_Shrine");
            Belltown_Shrine.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(52.86f, 8.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Belltown_Shrine.isSkongScene = true;
            Belltown_Shrine.AfterSceneActivated += () => {Belltown_Shrine.isSceneActive = true;};
            customScenes.Add(Belltown_Shrine);

            var Slab_16b = new CustomScene("Slab_16b");
            Slab_16b.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(56.95f, 5.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Slab_16b.isSkongScene = true;
            Slab_16b.AfterSceneActivated += () => {Slab_16b.isSceneActive = true;};
            customScenes.Add(Slab_16b);

            var Cog_Dancers = new CustomScene("Cog_Dancers");
            Cog_Dancers.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(39.96f, 4.6f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Cog_Dancers.isSkongScene = true;
            Cog_Dancers.AfterSceneActivated += () => {Cog_Dancers.isSceneActive = true;};
            customScenes.Add(Cog_Dancers);

            var Dust_Chef = new CustomScene("Dust_Chef");
            Dust_Chef.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(36.67f, 35.59f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Dust_Chef.isSkongScene = true;
            Dust_Chef.AfterSceneActivated += () => {Dust_Chef.isSceneActive = true;};
            customScenes.Add(Dust_Chef);

            var Belltown_08 = new CustomScene("Belltown_08");
            Belltown_08.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(53.11f, 11.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Belltown_08.isSkongScene = true;
            Belltown_08.AfterSceneActivated += () => {Belltown_08.isSceneActive = true;};
            customScenes.Add(Belltown_08);

            var Slab_10b = new CustomScene("Slab_10b");
            Slab_10b.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(41f, 9.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Slab_10b.isSkongScene = true;
            Slab_10b.AfterSceneActivated += () => {Slab_10b.isSceneActive = true;};
            customScenes.Add(Slab_10b);

            var Dock_09 = new CustomScene("Dock_09");
            Dock_09.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(24.766f, 7.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Dock_09.isSkongScene = true;
            Dock_09.AfterSceneActivated += () => {Dock_09.isSceneActive = true;};
            customScenes.Add(Dock_09);

            var Library_09 = new CustomScene("Library_09");
            Library_09.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(75.39f, 15.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Library_09.isSkongScene = true;
            Library_09.AfterSceneActivated += () => {Library_09.isSceneActive = true;};
            customScenes.Add(Library_09);

            var Cradle_03 = new CustomScene("Cradle_03");
            Cradle_03.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(39.7f, 133.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false, afterTransition: () =>
            {
                PlayMakerFSM.BroadcastEvent("START CHALLENGE MOD");
            }));
            Cradle_03.isSkongScene = true;
            Cradle_03.AfterSceneActivated += () => {Cradle_03.isSceneActive = true;};
            customScenes.Add(Cradle_03);

            var Shadow_18 = new CustomScene("Shadow_18");
            Shadow_18.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(56.5236f, 11.443f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Shadow_18.isSkongScene = true;
            Shadow_18.AfterSceneActivated += () => {Shadow_18.isSceneActive = true;};
            customScenes.Add(Shadow_18);

            var Song_Tower_01 = new CustomScene("Song_Tower_01");
            Song_Tower_01.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(49.27f, 100.01f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Song_Tower_01.isSkongScene = true;
            Song_Tower_01.AfterSceneActivated += () => {Song_Tower_01.isSceneActive = true;};
            customScenes.Add(Song_Tower_01);

            var Coral_27 = new CustomScene("Coral_27");
            Coral_27.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(12f, 33.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Coral_27.isSkongScene = true;
            Coral_27.AfterSceneActivated += () => {Coral_27.isSceneActive = true;};
            customScenes.Add(Coral_27);

            var Hang_17b = new CustomScene("Hang_17b");
            Hang_17b.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(36.4f, 4.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Hang_17b.isSkongScene = true;
            Hang_17b.AfterSceneActivated += () => {Hang_17b.isSceneActive = true;};
            customScenes.Add(Hang_17b);

            var Ward_02 = new CustomScene("Ward_02");
            Ward_02.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(51.26f, 6.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Ward_02.isSkongScene = true;
            Ward_02.AfterSceneActivated += () => {Ward_02.isSceneActive = true;};
            customScenes.Add(Ward_02);

            var Library_13 = new CustomScene("Library_13");
            Library_13.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(69.5f, 14.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Library_13.isSkongScene = true;
            Library_13.AfterSceneActivated += () => {Library_13.isSceneActive = true;};
            customScenes.Add(Library_13);

            var Coral_29 = new CustomScene("Coral_29");
            Coral_29.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(172.76f, 24.573f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Coral_29.isSkongScene = true;
            Coral_29.AfterSceneActivated += () => {Coral_29.isSceneActive = true;};
            customScenes.Add(Coral_29);

            var Bellway_Centipede_Arena = new CustomScene("Bellway_Centipede_Arena");
            Bellway_Centipede_Arena.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(136.35f, 7.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Bellway_Centipede_Arena.isSkongScene = true;
            Bellway_Centipede_Arena.AfterSceneActivated += () => {Bellway_Centipede_Arena.isSceneActive = true;};
            customScenes.Add(Bellway_Centipede_Arena);

            var Clover_10 = new CustomScene("Clover_10");
            Clover_10.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(88.55f, 37.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Clover_10.isSkongScene = true;
            Clover_10.AfterSceneActivated += () => {Clover_10.isSceneActive = true;};
            customScenes.Add(Clover_10);

            var Room_CrowCourt_02 = new CustomScene("Room_CrowCourt_02");
            Room_CrowCourt_02.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(33.50f, 21.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Room_CrowCourt_02.isSkongScene = true;
            Room_CrowCourt_02.AfterSceneActivated += () => {Room_CrowCourt_02.isSceneActive = true;};
            customScenes.Add(Room_CrowCourt_02);

            var Memory_Coral_Tower = new CustomScene("Memory_Coral_Tower");
            Memory_Coral_Tower.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(54.93f, 550.7f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, afterTransition: () =>
            {
                GameObject.Find("Temp plat").SetActive(false);
            }));
            Memory_Coral_Tower.isSkongScene = true;
            Memory_Coral_Tower.AfterSceneActivated += () => {
                var go = new GameObject("Temp plat");
                SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName("Memory_Coral_Tower"));
                go.AddComponent<BoxCollider2D>();
                go.layer = 8;
                go.transform.position = new Vector3(54.93f, 547.7f, 0);

                Memory_Coral_Tower.isSceneActive = true;
                };
            customScenes.Add(Memory_Coral_Tower);

            var Bone_East_18b = new CustomScene("Bone_East_18b");
            Bone_East_18b.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(172.7f, 6.33f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Bone_East_18b.isSkongScene = true;
            Bone_East_18b.AfterSceneActivated += () => {Bone_East_18b.isSceneActive = true;};
            customScenes.Add(Bone_East_18b);

            var Coral_33 = new CustomScene("Coral_33");
            Coral_33.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(30f, 61.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Coral_33.isSkongScene = true;
            Coral_33.AfterSceneActivated += () => {Coral_33.isSceneActive = true;};
            customScenes.Add(Coral_33);

            var AbyssCocoon = new CustomScene("Abyss_Cocoon");
            AbyssCocoon.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(29.16f, 5.65f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            AbyssCocoon.isSkongScene = true;
            AbyssCocoon.AfterSceneActivated += () => {AbyssCocoon.isSceneActive = true;};
            customScenes.Add(AbyssCocoon);

            var Shellwood_11b_Memory = new CustomScene("Shellwood_11b_Memory");
            Shellwood_11b_Memory.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(14f, 113f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Shellwood_11b_Memory.isSkongScene = true;
            Shellwood_11b_Memory.AfterSceneActivated += () => {Shellwood_11b_Memory.isSceneActive = true;};
            customScenes.Add(Shellwood_11b_Memory);

            var Clover_19 = new CustomScene("Clover_19");
            Clover_19.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(25.64f, 12.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Clover_19.isSkongScene = true;
            Clover_19.AfterSceneActivated += () => {Clover_19.isSceneActive = true;};
            customScenes.Add(Clover_19);

            var Peak_07 = new CustomScene("Peak_07");
            Peak_07.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(34.65f, 88.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Peak_07.isSkongScene = true;
            Peak_07.AfterSceneActivated += () => {Peak_07.isSceneActive = true;};
            customScenes.Add(Peak_07);

            var Crawl_10 = new CustomScene("Crawl_10");
            Crawl_10.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(19.66f, 7.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Crawl_10.isSkongScene = true;
            Crawl_10.AfterSceneActivated += () => {Crawl_10.isSceneActive = true;};
            customScenes.Add(Crawl_10);

            var Shellwood_22 = new CustomScene("Shellwood_22");
            Shellwood_22.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(101.35f, 6.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Shellwood_22.isSkongScene = true;
            Shellwood_22.AfterSceneActivated += () => {Shellwood_22.isSceneActive = true;};
            customScenes.Add(Shellwood_22);

            var Memory_Ant_Queen = new CustomScene("Memory_Ant_Queen");
            Memory_Ant_Queen.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(148.4f, 19.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Memory_Ant_Queen.isSkongScene = true;
            Memory_Ant_Queen.AfterSceneActivated += () => {Memory_Ant_Queen.isSceneActive = true;};
            customScenes.Add(Memory_Ant_Queen);

            var Coral_39 = new CustomScene("Coral_39");
            Coral_39.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(131f, 7.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Coral_39.isSkongScene = true;
            Coral_39.AfterSceneActivated += () => {Coral_39.isSceneActive = true;};
            customScenes.Add(Coral_39);
        }
    }
}