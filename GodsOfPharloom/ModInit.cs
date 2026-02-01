using BepInEx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Reflection;
using HarmonyLib;
using Unity.Mathematics;
using Newtonsoft.Json;
using GlobalEnums;
using System.Collections;
using HutongGames.PlayMaker.Actions;
using GenericVariableExtension;
using HutongGames.PlayMaker;
using Unity.Burst.Intrinsics;

namespace Gods_Of_Pharloom
{
    [BepInPlugin("bepinex.plugin.test", "Test", "0.0.0.1")]
    public partial class GodsOfPharloomMod : BaseUnityPlugin
    {
        public static GodsOfPharloomMod instance;
        public static string currentSceneName;
        public static Scene currentScene;
        string pathToModData = BepInEx.Paths.ConfigPath + "/" +"GodsOfPharloomData.dat";
        public static object obj;
        private static string[] assetBundleNames =
        {
            "gg_pharloom_atrium",
            "gg_pharloom_hall_of_gods",
            "gg_rest_scene",
            "gg_resources",
        };
        public static List<AssetBundle> assetBundles = new List<AssetBundle>();
        public static List<CustomScene> customScenes = new List<CustomScene>();
        public static BepInEx.Logging.ManualLogSource Log;
        public static MethodInfo HeroController_SetState = AccessTools.Method(typeof(HeroController), "SetState");
        public AssetBundle LoadBundle(string bundleName)
        {
            AssetBundle bundle;
            Assembly asm = Assembly.GetExecutingAssembly();
            foreach (string res in asm.GetManifestResourceNames())
            {
                string name = Path.GetExtension(res).Substring(1);
                Logger.LogInfo(name);
                if (name != bundleName) continue;

                using (Stream s = asm.GetManifestResourceStream(res))
                {
                    if (s == null) continue;
                    byte[] buffer = new byte[s.Length];
                    s.Read(buffer, 0, buffer.Length);
                    s.Dispose();
                    Logger.LogInfo("Loading bundle " + bundleName);
                    bundle = AssetBundle.LoadFromMemory(buffer);
                }
                return bundle;
            }
            return null;
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

            Harmony.CreateAndPatchAll(typeof(GodsOfPharloomMod));

            try{
            BossScene.InitBossesInfo();
            BossStatueInfo.InitBossesStatue();
            LoadModData();
            BossStatueInfo.GetBadges();
            }
            catch(Exception ex)
            {
                Logger.LogInfo(ex.Message);
            }

            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => lastLoadedScene = scene;

            BossSequence.CreateSequenceController();

            CustomScene.InitModRespawnMarkers();

            afterSceneLoaded += CustomMenu.Reset;
            InitCustomScenes();

            foreach(string name in assetBundleNames)
            {
                var bundle = LoadBundle(name);
                assetBundles.Add(bundle);

                if(name == "gg_resources")
                {
                    var assets = bundle.LoadAllAssets();
                    foreach(var asset in assets)
                    {
                        Preload.bundleResources[asset.name] = asset;
                    }
                }
            }

            TransitionSequence.Init();

            afterSceneLoaded += () => BossSequence.isHeroDead = false;

            Logger.LogInfo($"Plugin is loaded!");
        }

        void Update()
        {
            if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                if(BindingsMenu.menuBindingsFsm != null && BindingsMenu.menuBindingsFsm.ActiveStateName == "Opened") BindingsMenu.menuBindingsFsm.FsmComponent.SendEvent("CLOSE");
                else if(BindingsMenu.menuBindingsFsm.ActiveStateName == "Closed") BindingsMenu.menuBindingsFsm.SetState("Can Open Inventory?");
            }
            if (Keyboard.current.f6Key.wasPressedThisFrame)
            {
                var sceneLoadInfo = new GameManager.SceneLoadInfo
                {
                    SceneName = "GG_Pharloom_Atrium",
                    EntryGateName = "door_wakeInMemory_AntQueen(Clone)",
                    EntrySkip = true,
                    Visualization = GameManager.SceneLoadVisualizations.Default
                };

                GameManager.instance.BeginSceneTransition(sceneLoadInfo);
            }
        }

        public static void SetHeroState(ActorStates state)
        {
            Func<MethodInfo, object[], object> InvokeMethod = (method, obj) => method.Invoke(HeroController.instance, obj);
            InvokeMethod(HeroController_SetState, new object[] {state});
        }

        void InitCustomScenes()
        {
            var GG_Pharloom_Atrium = new CustomScene("GG_Pharloom_Atrium", isFastSuperJump: true, isSkongScene: false);
            // GG_Pharloom_Atrium.AddTransitionPoint(new TransitionPointInfo("door1", new Vector3(131.08f, 73.9f, 0), "Belltown", "door1", isADoor: true, noInputOnStart: false));
            GG_Pharloom_Atrium.AddTransitionPoint(new TransitionPointInfo("door2", new Vector3(156.4901f, 36.18f, 0), BossStatueInfo.hog_sceneName, "door1", isADoor: true, noInputOnStart: false));
            GG_Pharloom_Atrium.AfterSceneLoaded += (Scene scene) => {
                var audioManager = GameManager.instance.AudioManager;
                audioManager.StopAndClearAtmos();
                audioManager.StopAndClearMusic();

                var rootObjects = scene.GetRootGameObjects();

                //add a bench
                GameObject gg_bench_sprite = null;
                foreach(var obj in rootObjects)
                {
                    if(obj.name == "GG_Bench") gg_bench_sprite = obj;
                }
                var bench = Instantiate(Preload.preloads["RestBench"], parameters: new InstantiateParameters
                {
                    parent = gg_bench_sprite.transform,
                    scene = scene,
                });
                bench.transform.position = gg_bench_sprite.transform.position;
                var benchFsm = bench.LocateMyFSM("Bench Control").Fsm;
                var origVec = benchFsm.GetFsmVector3("Adjust Vector").Value;
                benchFsm.GetFsmVector3("Adjust Vector").Value = new Vector3(origVec.x, 0.7f, origVec.z);
                bench.GetComponent<SpriteRenderer>().enabled = false;
                var size = bench.GetComponent<BoxCollider2D>().size;
                bench.GetComponent<BoxCollider2D>().size = new Vector2(size.x, 1);
                /////////////
                
                //add camera lock
                var cameraLock1 = CustomScene.CreateCameraLock(scene);
                var go1 = cameraLock1.gameObject;
                go1.transform.position = new Vector3(105.2262f, 60.9892f, 0f); 
                cameraLock1.cameraYMin = 50f;
                cameraLock1.cameraYMax = 65f;
                cameraLock1.cameraXMin = 0f;
                cameraLock1.cameraXMax = 1000f;
                cameraLock1.preventLookDown = true;
                // cameraLock1.preventLookUp = true;
                // cameraLock1.lookYMax = 0;
                go1.GetComponent<BoxCollider2D>().size = new Vector2(300f, 18f);
                go1.AddComponent<ActiveCameraLockOnEnter>();
                cameraLock1.enabled = false;
                var cameraLock1_1 = ((GameObject)Instantiate(go1, scene : scene)).GetComponent<BoxCollider2D>();
                cameraLock1_1.transform.position = new Vector3(58.8772f, 42.7895f, 0f);
                cameraLock1_1.size = new Vector2(80, 18);
                var cameraLock1_2 = ((GameObject)Instantiate(go1, scene : scene)).GetComponent<BoxCollider2D>();
                cameraLock1_2.transform.position = new Vector3(379.4977f, 42.5895f, 0f);

                var cameraLock2 = CustomScene.CreateCameraLock(scene);
                var go2 = cameraLock2.gameObject;
                go2.transform.position = new Vector3(156.5713f, 40.4745f, 0f);
                cameraLock2.cameraYMin = 40f;
                cameraLock2.cameraYMax = 50f;
                cameraLock2.cameraXMin = 157f;
                cameraLock2.cameraXMax = 157f;
                cameraLock2.preventLookDown = true;
                // cameraLock2.preventLookUp = true;
                // cameraLock2.lookYMax = 0;
                go2.GetComponent<BoxCollider2D>().size = new Vector2(12f, 20f);
                go2.AddComponent<ActiveCameraLockOnEnter>();
                cameraLock2.enabled = false;

                var cameraLock3 = CustomScene.CreateCameraLock(scene);
                var go3 = cameraLock3.gameObject;
                go3.transform.position = new Vector3(156.7058f, 76.6994f, 0f);
                cameraLock3.cameraYMin = 79f;
                cameraLock3.cameraYMax = 500f;
                cameraLock3.cameraXMin = 0f;
                cameraLock3.cameraXMax = 1000f;
                // cameraLock3.preventLookDown = true;
                // cameraLock3.preventLookUp = true;
                // cameraLock3.lookYMax = 0;
                go3.GetComponent<BoxCollider2D>().size = new Vector2(300f, 8f);
                go3.AddComponent<ActiveCameraLockOnEnter>();
                cameraLock3.enabled = false;

                var cameraLock4 = CustomScene.CreateCameraLock(scene);
                var go4 = cameraLock4.gameObject;
                go4.transform.position = new Vector3(156.7058f, 86.9f, 0f);
                cameraLock4.cameraYMin = 88f;
                cameraLock4.cameraYMax = 500f;
                cameraLock4.cameraXMin = 156.4254f;
                cameraLock4.cameraXMax = 156.4254f;
                cameraLock4.preventLookDown = true;
                cameraLock4.preventLookUp = true;
                cameraLock4.lookYMax = 0;
                go4.GetComponent<BoxCollider2D>().size = new Vector2(11f, 12f);
                go4.AddComponent<ActiveCameraLockOnEnter>();
                cameraLock4.enabled = false;
                /////////////
                
                // //add memory entry
                var wakeInMemory = (GameObject)Instantiate(Preload.preloads["door_wakeInMemory_AntQueen"], scene: scene);
                wakeInMemory.transform.position = new Vector3(14f, 54f, 0);
                var wakeInMemoryFSM = wakeInMemory.GetComponent<PlayMakerFSM>().Fsm;
                var pause = wakeInMemoryFSM.GetState("Pause");
                var setRespawn = wakeInMemoryFSM.GetState("Set Respawn?");
                var blankScreen = wakeInMemoryFSM.GetState("Blank Screen");
                var saveGame = wakeInMemoryFSM.GetState("Save?");
                wakeInMemoryFSM.GetFsmBool("Save Game").Value = true;
                var customActionSetRespawn = new PatchedFsm.CustomLogicFsm(wakeInMemoryFSM);
                customActionSetRespawn.action += (Fsm fsm) =>
                {
                    PlayerData.instance.respawnMarkerName = "Death Respawn Marker";
                    PlayerData.instance.respawnScene = "GG_Pharloom_Atrium";
                };
                blankScreen.Actions = PatchedFsm.InsertInArray(blankScreen.Actions, customActionSetRespawn, 0);
                pause.Transitions = new FsmTransition[]{pause.Transitions[1]};

                //add exit edge
                var exitEdge = (GameObject)Instantiate(Preload.preloads["Exit Edge Trigger_AntQueen"], scene: scene);
                exitEdge.transform.position = new Vector3(0, 54, 0);
                var exitEdgeComp = exitEdge.GetComponent<SceneTransitionZone>();
                exitEdgeComp.GetType().GetField("targetGate", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(exitEdgeComp, "door_wakeOnGround_FlowerQueen(Clone)");
                exitEdgeComp.GetType().GetField("targetScene", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(exitEdgeComp, "Abyss_05");

                var exitEdge2 = (GameObject)GameObject.Instantiate(exitEdge, scene);
                exitEdge2.transform.position = new Vector3(62f, 28f, 0);
                exitEdge2.transform.Rotate(new Vector3(0, 0, 90));
                exitEdge2.transform.localScale = new Vector3(1, 7, 1);

                //init pantheon menu
                var pantheonMenuHandler = Preload.FindObjectByPath(rootObjects, "PantheonMenuCanvasHandler");
                var pantheonMenuComp = pantheonMenuHandler.AddComponent<PantheonMenu>();

                //init pantheons
                Pantheon.pantheonsCount = 0;
                Pantheon pComp;
                var bosses = BossScene.bosses;
                var pantheon1 = Preload.FindObjectByPath(rootObjects, "Half1/Pantheon1");
                pComp = pantheon1.AddComponent<Pantheon>();
                pComp.pantheonName = "Pantheon 1";
                pComp.pantheonDisplayName = "Pantheon of the Devoted";
                pComp.sequence = new BossScene[]{
                    bosses["Moss Mother"],
                    bosses["Bell Beast"],
                    bosses["Skull Tyrant"],
                    bosses["Fourth Chorus"],
                    bosses["Lace in Deep Docks"],
                    bosses["RestScene"],
                    bosses["Moorwing"],
                    bosses["Sister Splinter"],
                    bosses["Great Conchflies"],
                    bosses["Savage Beastfly in Chapel of The Beast"],
                    bosses["Widow"],
                };
                pComp.Init();

                var pantheon2 = Preload.FindObjectByPath(rootObjects, "Half1/Pantheon2");
                pComp = pantheon2.AddComponent<Pantheon>();
                pComp.pantheonName = "Pantheon 2";
                pComp.pantheonDisplayName = "Pantheon of the Failed Child";
                pComp.sequence = new BossScene[]{
                    bosses["Garmond & Zaza"],
                    bosses["Disgraced Chef Lugoli"],
                    bosses["Father of the Flame"],
                    bosses["Raging Conchfly"],
                    bosses["Forebrothers Signis & Gron"],
                    bosses["RestScene"],
                    bosses["Shakra"],
                    bosses["Voltvyrm"],
                    bosses["Cogwork Dancers"],
                    bosses["The Last Judge"],
                    bosses["Phantom"],
                };
                pComp.Init();

                var pantheon3 = Preload.FindObjectByPath(rootObjects, "Half2/Pantheon3");
                pComp = pantheon3.AddComponent<Pantheon>();
                pComp.pantheonName = "Pantheon 3";
                pComp.pantheonDisplayName = "Pantheon of the First Sinner";
                pComp.sequence = new BossScene[]{
                    bosses["Broodmother"],
                    bosses["Savage Beastfly in Far Fields"],
                    bosses["Trobbio"],
                    bosses["Second Sentiel"],
                    bosses["Lace in the Cradle"],
                    bosses["RestScene"],
                    bosses["The Unravelled"],
                    bosses["Palestag"],
                    bosses["Plasmified Zango"],
                    bosses["Groal the Great"],
                    bosses["First Sinner"],
                };
                pComp.Init();

                var pantheon4 = Preload.FindObjectByPath(rootObjects, "Half2/Pantheon4");
                pComp = pantheon4.AddComponent<Pantheon>();
                pComp.pantheonName = "Pantheon 4";
                pComp.pantheonDisplayName = "Pantheon of the Lost Child";
                pComp.sequence = new BossScene[]{
                    bosses["Clover Dancers"],
                    bosses["Gurr the Outcast"],
                    bosses["Lost Garmond"],
                    bosses["Tormented Trobbio"],
                    bosses["Crawfather"],
                    bosses["RestScene"],
                    bosses["Crust King Khann"],
                    bosses["Pinstress"],
                    bosses["Nyleth"],
                    bosses["Skarrsinger Karmelita"],
                    bosses["Lost Lace"],
                };
                pComp.Init();

                var pantheon5 = Preload.FindObjectByPath(rootObjects, "Pantheon5");
                pComp = pantheon5.AddComponent<Pantheon>();
                pComp.pantheonName = "Pantheon 5";
                pComp.pantheonDisplayName = "Pantheon Of Pharloom";
                pComp.sequence = new BossScene[]{
                    bosses["Moss Mother"].ascendedVersion, //pantheon 1
                    bosses["Bell Beast"],
                    bosses["Skull Tyrant"],
                    bosses["Fourth Chorus"],
                    bosses["Lace in Deep Docks"],
                    bosses["RestScene"], //////
                    bosses["Moorwing"],
                    bosses["Sister Splinter"],
                    bosses["Great Conchflies"],
                    bosses["Garmond & Zaza"],
                    bosses["Widow"],
                    bosses["RestScene"], //////
                    bosses["Shakra"], //pantheon 2
                    bosses["Disgraced Chef Lugoli"],
                    bosses["Father of the Flame"],
                    bosses["Raging Conchfly"],
                    bosses["Forebrothers Signis & Gron"],
                    bosses["RestScene"], //////
                    bosses["The Unravelled"],
                    bosses["Voltvyrm"],
                    bosses["Cogwork Dancers"],
                    bosses["The Last Judge"],
                    bosses["Phantom"],
                    bosses["RestScene"], //////
                    bosses["Broodmother"], //pantheon 3
                    bosses["Savage Beastfly in Far Fields"],
                    bosses["Trobbio"],
                    bosses["Second Sentiel"],
                    bosses["Lace in the Cradle"],
                    bosses["RestScene"], //////
                    bosses["Pinstress"],
                    bosses["Palestag"],
                    bosses["Plasmified Zango"],
                    bosses["Groal the Great"],
                    bosses["First Sinner"],
                    bosses["RestScene"], //////
                    bosses["Clover Dancers"], //pantheon 4
                    bosses["Gurr the Outcast"],
                    bosses["Bell Eater"],
                    bosses["Tormented Trobbio"],
                    bosses["Crawfather"],
                    bosses["RestScene"], //////
                    bosses["Crust King Khann"],
                    bosses["Lost Garmond"],
                    bosses["Shrine Guardian Seth"],
                    bosses["Nyleth"],
                    bosses["Skarrsinger Karmelita"],
                    bosses["RestScene"], //////
                    bosses["Watcher at the Edge"], //last pantheon
                    bosses["Lost Lace"],
                    bosses["Grand Mother Silk"],
                };
                pComp.Init();

                HeroController.instance.MaxHealth();
                HeroController.instance.MaxRegenSilkInstant();

                //add a scene manager
                var sceneManager = (GameObject)Instantiate(Preload.preloads["_SceneManager_Abyss_05"], scene: scene);
                var sceneManagerComp = sceneManager.GetComponent<CustomSceneManager>();
                sceneManager.SetActive(true);
                IEnumerator enumerator()
                {
                    yield return null;

                    sceneManagerComp.darknessLevel = 0;
                    sceneManagerComp.saturation = 1.2f;
                    sceneManagerComp.UpdateScene();
                }
                this.StartCoroutine(enumerator());
                /////////////////////

                BossSequence.Reset();

                ToolItemManager.TryReplenishTools(true, ToolItemManager.ReplenishMethod.BenchSilent);

                GG_Pharloom_Atrium.isSceneActive = true;
            };
            GG_Pharloom_Atrium.AfterSceneActivated += (Scene scene) =>
            {
                HeroController.instance.MaxHealth();
                HeroController.instance.MaxRegenSilkInstant();
            };
            customScenes.Add(GG_Pharloom_Atrium);

            var GG_Pharloom_HoG = new CustomScene(BossStatueInfo.hog_sceneName, isFastSuperJump: true, isSkongScene: false);
            GG_Pharloom_HoG.AddTransitionPoint(new TransitionPointInfo("door1", new Vector3(44.64f, 52.58f, 0), "GG_Pharloom_Atrium", "door2", isADoor: true, noInputOnStart: false));
            GG_Pharloom_HoG.AfterSceneLoaded += (Scene scene) => {
                var gm = GameManager.instance;
                var audioManager = gm.AudioManager;
                audioManager.StopAndClearAtmos();
                audioManager.StopAndClearMusic();

                var rootObjects = scene.GetRootGameObjects();
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

                var cameraLock1 = CustomScene.CreateCameraLock(scene);
                var go1 = cameraLock1.gameObject;
                go1.transform.position = new Vector3(44.64f, 90f, 0);
                cameraLock1.cameraYMin = 57f;
                cameraLock1.cameraYMax = 400f;
                cameraLock1.cameraXMin = 44.64f;
                cameraLock1.cameraXMax = 44.64f;
                cameraLock1.preventLookDown = true;
                go1.GetComponent<BoxCollider2D>().size = new Vector2(8f, 100f);

                //add a bench
                GameObject gg_bench_sprite = null;
                foreach(var obj in rootObjects)
                {
                    if(obj.name == "GG_Bench") gg_bench_sprite = obj;
                }
                var bench = Instantiate(Preload.preloads["RestBench"], parameters: new InstantiateParameters
                {
                    parent = gg_bench_sprite.transform,
                    scene = scene,
                });
                bench.transform.position = gg_bench_sprite.transform.position;
                var benchFsm = bench.LocateMyFSM("Bench Control").Fsm;
                var origVec = benchFsm.GetFsmVector3("Adjust Vector").Value;
                benchFsm.GetFsmVector3("Adjust Vector").Value = new Vector3(origVec.x, 0.5f, origVec.z);
                bench.GetComponent<SpriteRenderer>().enabled = false;
                /////////////
                
                //add a scene manager
                var sceneManager = (GameObject)Instantiate(Preload.preloads["_SceneManager_Abyss_05"], scene: scene);
                var sceneManagerComp = sceneManager.GetComponent<CustomSceneManager>();
                sceneManager.SetActive(true);
                IEnumerator enumerator()
                {
                    yield return null;

                    sceneManagerComp.darknessLevel = 0;
                    sceneManagerComp.saturation = 1.2f;
                    sceneManagerComp.UpdateScene();
                }
                this.StartCoroutine(enumerator());
                /////////////////////

                BossSequence.Reset();

                ToolItemManager.TryReplenishTools(true, ToolItemManager.ReplenishMethod.BenchSilent);

                GG_Pharloom_HoG.isSceneActive = true;
            };
            GG_Pharloom_HoG.AfterSceneActivated += (Scene scene) =>
            {
                HeroController.instance.MaxHealth();
                HeroController.instance.MaxRegenSilkInstant();
            };
            customScenes.Add(GG_Pharloom_HoG);

            var GG_Rest_Scene = new CustomScene("GG_Rest_Scene", isFastSuperJump: true, isSkongScene: false);
            GG_Rest_Scene.AddTransitionPoint(new TransitionPointInfo("rest_scene_entry", new Vector3(59.1f, 59f, 0), "", "",
                                                                    isADoor: true, noInputOnStart: false, isOneTimeTransition: true));
            GG_Rest_Scene.AddTransitionPoint(new TransitionPointInfo("right1", new Vector3(123.08f, 54f, 0), "GG_Pharloom_Atrium", "door2",
                                                                    isADoor: false, noInputOnStart: false));
            GG_Rest_Scene.AfterSceneLoaded += (Scene scene) => {
                var gm = GameManager.instance;
                var audioManager = gm.AudioManager;
                audioManager.StopAndClearAtmos();
                audioManager.StopAndClearMusic();

                var rootObjects = scene.GetRootGameObjects();
                var nextSequenceScene = BossSequence.nextSequenceScene;

                //add a scene manager
                var sceneManager = (GameObject)Instantiate(Preload.preloads["_SceneManager_Abyss_05"], scene: scene);
                var sceneManagerComp = sceneManager.GetComponent<CustomSceneManager>();
                sceneManager.SetActive(true);
                /////////////////////
                
                //add a bench
                GameObject gg_bench_sprite = Preload.FindObjectByPath(rootObjects, "GG_Bench");
                var bench = Instantiate(Preload.preloads["RestBench"], parent: gg_bench_sprite.transform);
                bench.transform.position = gg_bench_sprite.transform.position;
                var benchFsm = bench.LocateMyFSM("Bench Control").Fsm;
                var origVec = benchFsm.GetFsmVector3("Adjust Vector").Value;
                benchFsm.GetFsmVector3("Adjust Vector").Value = new Vector3(origVec.x, 0.5f, origVec.z);
                bench.GetComponent<SpriteRenderer>().enabled = false;
                /////////////
                
                //add camera lock
                var cameraLock1 = CustomScene.CreateCameraLock(scene);
                var go1 = cameraLock1.gameObject;
                go1.transform.position = new Vector3(74.0264f, 77.2036f, 0f);
                cameraLock1.cameraYMin = 57f;
                cameraLock1.cameraYMax = 60f;
                cameraLock1.cameraXMin = 60f;
                cameraLock1.cameraXMax = 104f;
                cameraLock1.preventLookDown = true;
                cameraLock1.preventLookUp = true;
                cameraLock1.lookYMax = 0;
                go1.GetComponent<BoxCollider2D>().size = new Vector2(100f, 100f);
                /////////////
                
                //add spa
                var water = ((GameObject)Instantiate(Preload.preloads["Surface Water Region"], scene: scene)).GetComponent<SurfaceWaterRegion>();
                var spaRegion = (GameObject)Instantiate(Preload.preloads["Spa Region"], parent: water.transform);
                var spaWaterSmall = (GameObject)Instantiate(Preload.preloads["spa_water_small"], parent: water.transform);
                var stillWater = (GameObject)Instantiate(Preload.preloads["StillWater"], parent: water.transform);

                water.transform.position = new Vector3(80f, 53f, 0f);
                water.GetComponent<BoxCollider2D>().offset = new Vector2(-14.6142f, -4f);
                water.GetComponent<BoxCollider2D>().size = new Vector2(280f, 4.7626f);
                var splashSurface = water.transform.Find("Splash Surface").GetComponent<BoxCollider2D>();
                splashSurface.size = new Vector2(280.5812f, 5.6057f);
                splashSurface.transform.localPosition = new Vector3(0f, -1.15f, 0f);
                
                spaWaterSmall.transform.localPosition = new Vector3(0f, 3.2164f, 0.1f);
                spaWaterSmall.transform.localScale = new Vector3(24.2527f, 1f, 1f);
                spaWaterSmall.transform.Find("water_fog").gameObject.SetActive(false);

                stillWater.transform.localPosition = new Vector3(0f, -3.7f, 0);
                stillWater.transform.localScale = new Vector3(348.0877f, 4.4392f, 1f);

                spaRegion.transform.localPosition = new Vector3(-10.527f, -2f, 0f);
                spaRegion.transform.localScale = new Vector3(25.2328f, 1f, 1f);
                /////////////
                
                IEnumerator enumerator()
                {
                    yield return null;

                    sceneManagerComp.darknessLevel = 0;
                    sceneManagerComp.saturation = 1.2f;
                    sceneManagerComp.UpdateScene();

                    water.GetType().GetField("heroSurfaceY", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(water, 52f);
                }
                this.StartCoroutine(enumerator());
                
                var transition = Preload.FindObjectByPath(rootObjects, "right1").GetComponent<TransitionPoint>();
                transition.targetScene = nextSequenceScene.sceneName;
                transition.entryPoint = nextSequenceScene.entryGate;

                var customScene1 = customScenes.Find(i => i.sceneName == nextSequenceScene.sceneName);

                var transitionPointInfo1 = customScene1.TransitionGates.Find(i => i.gateName == nextSequenceScene.entryGate);
                var transitionPointInfo2 = GG_Rest_Scene.TransitionGates.Find(i => i.gateName == "right1");

                transitionPointInfo2.noInputOnStart = transitionPointInfo1.noInputOnStart;

                Action tmpAction1 = null;
                Action tmpAction2 = null;
                Action<Scene> tmpAction3 = null;

                tmpAction1 = () =>
                {
                    BossSequence.currentSequenceSceneIndex++;

                    BossSequence.currentSequenceScene = BossSequence.bossSequence[BossSequence.currentSequenceSceneIndex];
                    if(BossSequence.currentSequenceSceneIndex + 1 < BossSequence.bossSequence.Length) nextSequenceScene = BossSequence.bossSequence[BossSequence.currentSequenceSceneIndex + 1];
                    else nextSequenceScene = null;

                    Log.LogInfo(nextSequenceScene.sceneName);

                    customScene1.BeforeSceneLoaded -= tmpAction1;
                };

                tmpAction2 = () =>
                {
                    if(nextSequenceScene.sceneType == BossScene.SceneType.Rest) TransitionSequence.SetVisible(false);
                    else TransitionSequence.SetVisible(true);

                    var endAudio = TransitionSequence.transitionEndAudio;
                    if(endAudio != null) endAudio.Play();

                    customScene1.AfterHeroEnteredScene -= tmpAction2;
                };

                tmpAction3 = (_) =>
                {
                    if(nextSequenceScene.sceneType == BossScene.SceneType.Rest) TransitionSequence.SetVisible(false);
                    else TransitionSequence.SetVisible(true);

                    customScene1.AfterSceneActivated -= tmpAction3;
                };

                customScene1.BeforeSceneLoaded += tmpAction1;
                customScene1.AfterHeroEnteredScene += tmpAction2;
                customScene1.AfterSceneActivated += tmpAction3;

                Log.LogInfo(customScene1.sceneName);

                PlayMakerFSM.BroadcastEvent("REST SCENE MOD");

                GG_Rest_Scene.isSceneActive = true;
            };
            customScenes.Add(GG_Rest_Scene);

            var Abyss_05 = new CustomScene("Abyss_05", isSkongScene: true);
            Abyss_05.AfterSceneLoaded += (Scene scene) => {
                var bundle = assetBundles.Find(i => i.name == "gg_resources");
                var tunnerTex = bundle.LoadAsset<Texture2D>("gg_tuner_0001_2");
                var tunnerSprite = Sprite.Create(tunnerTex, new Rect(0, 0, tunnerTex.width, tunnerTex.height), new Vector2(0.5f, 0.5f));

                var tunner = new GameObject("Tunner_Mod");
                SceneManager.MoveGameObjectToScene(tunner, scene);
                tunner.transform.position = new Vector3(145.5f, 12.4f, 0.013f);
                tunner.transform.localScale = new Vector3(1.5f, 1.5f, 1);

                var tunnerSpriteRenderer = tunner.AddComponent<SpriteRenderer>();
                tunnerSpriteRenderer.sprite = tunnerSprite;



                var respawnPoint = (GameObject)Instantiate(Preload.preloads["door_wakeOnGround_FlowerQueen"], scene: scene);
                respawnPoint.transform.position = new Vector3(142, 12.4f, 0);
                var respawnComp = Preload.FindObjectByPath(respawnPoint, "Death Respawn Marker").GetComponent<RespawnMarker>();
                respawnComp.name = "Death Respawn Marker_Mod";
                var respawnFSM = respawnPoint.GetComponent<PlayMakerFSM>().Fsm;
                var doorEntry = respawnFSM.GetState("Door Entry");
                respawnFSM.GetFsmBool("Save Game").Value = true;
                var customActionSetRespawn = new PatchedFsm.CustomLogicFsm(respawnFSM);
                customActionSetRespawn.action += (Fsm fsm) =>
                {
                    PlayerData.instance.respawnMarkerName = respawnComp.gameObject.name;
                    PlayerData.instance.respawnScene = "Abyss_05";
                };
                doorEntry.Actions = PatchedFsm.InsertInArray(doorEntry.Actions, customActionSetRespawn, 0);
            };
            Abyss_05.AfterSceneActivated += (Scene scene) => {

                var memGroup = (GameObject)Instantiate(Preload.preloads["Memory Group"], scene: scene);
                memGroup.transform.position = new Vector3(144.2f, 13f, 0.013f);
                var memGroupFSM = memGroup.GetComponent<PlayMakerFSM>().Fsm;
                var transitionScene = memGroupFSM.GetState("Transition Scene");
                ((BeginSceneTransition)transitionScene.Actions[4]).sceneName = "GG_Pharloom_Atrium";
                ((BeginSceneTransition)transitionScene.Actions[4]).entryGateName = "door_wakeInMemory_AntQueen(Clone)";

                Abyss_05.isSceneActive = true;
            };
            customScenes.Add(Abyss_05);


            var Tut_03 = new CustomScene("Tut_03");
            Tut_03.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(58.41f, 17.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Tut_03.isSkongScene = true;
            Tut_03.AfterSceneLoaded += (Scene scene) => {Tut_03.isSceneActive = true;};
            customScenes.Add(Tut_03);

            var Weave_03 = new CustomScene("Weave_03");
            Weave_03.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(13.41f, 20.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Weave_03.isSkongScene = true;
            Weave_03.AfterSceneLoaded += (Scene scene) => {Weave_03.isSceneActive = true;};
            customScenes.Add(Weave_03);

            var Bone_05 = new CustomScene("Bone_05");
            Bone_05.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(86.48f, 3.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Bone_05.isSkongScene = true;
            Bone_05.AfterSceneLoaded += (Scene scene) => {Bone_05.isSceneActive = true;};
            customScenes.Add(Bone_05);

            var Bone_East_08 = new CustomScene("Bone_East_08");
            Bone_East_08.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(76.89f, 8.08f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false, afterTransition: () =>
            {
                GameObject.Find("Temp plat").SetActive(false);

                PlayerData.instance.hazardRespawnLocation = new Vector3(80.32f, 8.49f, 0);
            }));
            Bone_East_08.isSkongScene = true;
            Bone_East_08.AfterSceneLoaded += (Scene scene) => {
                var go = new GameObject("Temp plat");
                SceneManager.MoveGameObjectToScene(go, scene);
                go.AddComponent<BoxCollider2D>();
                go.layer = 8;
                go.transform.position = new Vector3(76.89f, 4.9f, 0);

                Bone_East_08.isSceneActive = true;
            };
            customScenes.Add(Bone_East_08);

            var Coral_11 = new CustomScene("Coral_11");
            Coral_11.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(52.6f, 14.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: true));
            Coral_11.isSkongScene = true;
            Coral_11.AfterSceneLoaded += (Scene scene) => {Coral_11.isSceneActive = true;};
            customScenes.Add(Coral_11);

            var Bone_East_12 = new CustomScene("Bone_East_12");
            Bone_East_12.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(92f, 7.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Bone_East_12.isSkongScene = true;
            Bone_East_12.AfterSceneLoaded += (Scene scene) => {Bone_East_12.isSceneActive = true;};
            customScenes.Add(Bone_East_12);

            var Coral_Judge_Arena = new CustomScene("Coral_Judge_Arena");
            Coral_Judge_Arena.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(34.1932f, 24.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: true));
            Coral_Judge_Arena.isSkongScene = true;
            Coral_Judge_Arena.AfterSceneLoaded += (Scene scene) => {Coral_Judge_Arena.isSceneActive = true;};
            customScenes.Add(Coral_Judge_Arena);

            var Greymoor_08 = new CustomScene("Greymoor_08");
            Greymoor_08.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(27.3f, 4.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Greymoor_08.isSkongScene = true;
            Greymoor_08.AfterSceneLoaded += (Scene scene) => {Greymoor_08.isSceneActive = true;};
            customScenes.Add(Greymoor_08);

            var Organ_01 = new CustomScene("Organ_01");
            Organ_01.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(84.36f, 104.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Organ_01.isSkongScene = true;
            Organ_01.AfterSceneLoaded += (Scene scene) => {Organ_01.isSceneActive = true;};
            customScenes.Add(Organ_01);

            var Ant_19 = new CustomScene("Ant_19");
            Ant_19.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(54.8f, 34.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Ant_19.isSkongScene = true;
            Ant_19.AfterSceneLoaded += (Scene scene) => {Ant_19.isSceneActive = true;};
            customScenes.Add(Ant_19);

            var Shellwood_18 = new CustomScene("Shellwood_18");
            Shellwood_18.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(42.49f, 8.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Shellwood_18.isSkongScene = true;
            Shellwood_18.AfterSceneLoaded += (Scene scene) => {Shellwood_18.isSceneActive = true;};
            customScenes.Add(Shellwood_18);

            var Bone_15 = new CustomScene("Bone_15");
            Bone_15.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(83.75f, 14.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Bone_15.isSkongScene = true;
            Bone_15.AfterSceneLoaded += (Scene scene) => {Bone_15.isSceneActive = true;};
            customScenes.Add(Bone_15);

            var Belltown_Shrine = new CustomScene("Belltown_Shrine");
            Belltown_Shrine.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(52.86f, 8.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: true));
            Belltown_Shrine.isSkongScene = true;
            Belltown_Shrine.AfterSceneLoaded += (Scene scene) => {Belltown_Shrine.isSceneActive = true;};
            customScenes.Add(Belltown_Shrine);

            var Slab_16b = new CustomScene("Slab_16b");
            Slab_16b.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(56.95f, 5.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Slab_16b.isSkongScene = true;
            Slab_16b.AfterSceneLoaded += (Scene scene) => {Slab_16b.isSceneActive = true;};
            customScenes.Add(Slab_16b);

            var Cog_Dancers = new CustomScene("Cog_Dancers");
            Cog_Dancers.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(39.96f, 4.6f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Cog_Dancers.isSkongScene = true;
            Cog_Dancers.AfterSceneLoaded += (Scene scene) => {Cog_Dancers.isSceneActive = true;};
            customScenes.Add(Cog_Dancers);

            var Dust_Chef = new CustomScene("Dust_Chef");
            Dust_Chef.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(36.67f, 35.59f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Dust_Chef.isSkongScene = true;
            Dust_Chef.AfterSceneLoaded += (Scene scene) => {Dust_Chef.isSceneActive = true;};
            customScenes.Add(Dust_Chef);

            var Belltown_08 = new CustomScene("Belltown_08");
            Belltown_08.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(53.11f, 11.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Belltown_08.isSkongScene = true;
            Belltown_08.AfterSceneLoaded += (Scene scene) => {Belltown_08.isSceneActive = true;};
            customScenes.Add(Belltown_08);

            var Slab_10b = new CustomScene("Slab_10b");
            Slab_10b.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(41f, 9.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: true));
            Slab_10b.isSkongScene = true;
            Slab_10b.AfterSceneLoaded += (Scene scene) => {Slab_10b.isSceneActive = true;};
            customScenes.Add(Slab_10b);

            var Dock_09 = new CustomScene("Dock_09");
            Dock_09.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(30f, 7.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Dock_09.isSkongScene = true;
            Dock_09.AfterSceneLoaded += (Scene scene) => {Dock_09.isSceneActive = true;};
            customScenes.Add(Dock_09);

            var Library_09 = new CustomScene("Library_09");
            Library_09.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(75.39f, 15.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: true));
            Library_09.isSkongScene = true;
            Library_09.AfterSceneLoaded += (Scene scene) => {Library_09.isSceneActive = true;};
            customScenes.Add(Library_09);

            var Cradle_03 = new CustomScene("Cradle_03");
            Cradle_03.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(39.7f, 133.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false, afterTransition: () =>
            {
                PlayMakerFSM.BroadcastEvent("START CHALLENGE MOD");
            }));
            Cradle_03.isSkongScene = true;
            Cradle_03.AfterSceneLoaded += (Scene scene) => {Cradle_03.isSceneActive = true;};
            customScenes.Add(Cradle_03);

            var Shadow_18 = new CustomScene("Shadow_18");
            Shadow_18.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(56.5236f, 11.443f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Shadow_18.isSkongScene = true;
            Shadow_18.AfterSceneLoaded += (Scene scene) => {Shadow_18.isSceneActive = true;};
            customScenes.Add(Shadow_18);

            var Song_Tower_01 = new CustomScene("Song_Tower_01");
            Song_Tower_01.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(49.27f, 100.01f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Song_Tower_01.isSkongScene = true;
            Song_Tower_01.AfterSceneLoaded += (Scene scene) => {Song_Tower_01.isSceneActive = true;};
            customScenes.Add(Song_Tower_01);

            var Coral_27 = new CustomScene("Coral_27");
            Coral_27.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(12f, 33.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Coral_27.isSkongScene = true;
            Coral_27.AfterSceneLoaded += (Scene scene) => {Coral_27.isSceneActive = true;};
            customScenes.Add(Coral_27);

            var Hang_17b = new CustomScene("Hang_17b");
            Hang_17b.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(36.4f, 4.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Hang_17b.isSkongScene = true;
            Hang_17b.AfterSceneLoaded += (Scene scene) => {Hang_17b.isSceneActive = true;};
            customScenes.Add(Hang_17b);

            var Ward_02 = new CustomScene("Ward_02");
            Ward_02.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(51.26f, 6.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Ward_02.isSkongScene = true;
            Ward_02.AfterSceneLoaded += (Scene scene) => {Ward_02.isSceneActive = true;};
            customScenes.Add(Ward_02);

            var Library_13 = new CustomScene("Library_13");
            Library_13.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(69.5f, 14.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Library_13.isSkongScene = true;
            Library_13.AfterSceneLoaded += (Scene scene) => {Library_13.isSceneActive = true;};
            customScenes.Add(Library_13);

            var Coral_29 = new CustomScene("Coral_29");
            Coral_29.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(172.76f, 24.573f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
            Coral_29.isSkongScene = true;
            Coral_29.AfterSceneLoaded += (Scene scene) => {Coral_29.isSceneActive = true;};
            customScenes.Add(Coral_29);

            var Bellway_Centipede_Arena = new CustomScene("Bellway_Centipede_Arena");
            Bellway_Centipede_Arena.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(136.35f, 7.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Bellway_Centipede_Arena.isSkongScene = true;
            Bellway_Centipede_Arena.AfterSceneLoaded += (Scene scene) => {Bellway_Centipede_Arena.isSceneActive = true;};
            customScenes.Add(Bellway_Centipede_Arena);

            var Clover_10 = new CustomScene("Clover_10");
            Clover_10.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(88.55f, 37.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Clover_10.isSkongScene = true;
            Clover_10.AfterSceneLoaded += (Scene scene) => {Clover_10.isSceneActive = true;};
            customScenes.Add(Clover_10);

            var Room_CrowCourt_02 = new CustomScene("Room_CrowCourt_02");
            Room_CrowCourt_02.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(33.50f, 21.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Room_CrowCourt_02.isSkongScene = true;
            Room_CrowCourt_02.AfterSceneLoaded += (Scene scene) => {Room_CrowCourt_02.isSceneActive = true;};
            customScenes.Add(Room_CrowCourt_02);

            var Memory_Coral_Tower = new CustomScene("Memory_Coral_Tower");
            Memory_Coral_Tower.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(54.93f, 550.7f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, afterTransition: () =>
            {
                GameObject.Find("Temp plat").SetActive(false);
            }));
            Memory_Coral_Tower.isSkongScene = true;
            Memory_Coral_Tower.AfterSceneLoaded += (Scene scene) => {
                var go = new GameObject("Temp plat");
                SceneManager.MoveGameObjectToScene(go, scene);
                go.AddComponent<BoxCollider2D>();
                go.layer = 8;
                go.transform.position = new Vector3(54.93f, 548.7f, 0);

                Memory_Coral_Tower.isSceneActive = true;
                };
            customScenes.Add(Memory_Coral_Tower);

            var Bone_East_18b = new CustomScene("Bone_East_18b");
            Bone_East_18b.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(172.7f, 6.33f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Bone_East_18b.isSkongScene = true;
            Bone_East_18b.AfterSceneLoaded += (Scene scene) => {Bone_East_18b.isSceneActive = true;};
            customScenes.Add(Bone_East_18b);

            var Coral_33 = new CustomScene("Coral_33");
            Coral_33.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(30f, 61.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Coral_33.isSkongScene = true;
            Coral_33.AfterSceneLoaded += (Scene scene) => {Coral_33.isSceneActive = true;};
            customScenes.Add(Coral_33);

            var AbyssCocoon = new CustomScene("Abyss_Cocoon");
            AbyssCocoon.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(29.16f, 5.65f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: true));
            AbyssCocoon.isSkongScene = true;
            AbyssCocoon.AfterSceneLoaded += (Scene scene) => {AbyssCocoon.isSceneActive = true;};
            customScenes.Add(AbyssCocoon);

            var Shellwood_11b_Memory = new CustomScene("Shellwood_11b_Memory");
            Shellwood_11b_Memory.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(18f, 96f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Shellwood_11b_Memory.isSkongScene = true;
            Shellwood_11b_Memory.AfterSceneLoaded += (Scene scene) => {Shellwood_11b_Memory.isSceneActive = true;};
            customScenes.Add(Shellwood_11b_Memory);

            var Clover_19 = new CustomScene("Clover_19");
            Clover_19.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(25.64f, 12.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: true));
            Clover_19.isSkongScene = true;
            Clover_19.AfterSceneLoaded += (Scene scene) => {Clover_19.isSceneActive = true;};
            customScenes.Add(Clover_19);

            var Peak_07 = new CustomScene("Peak_07");
            Peak_07.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(34.65f, 88.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: true));
            Peak_07.isSkongScene = true;
            Peak_07.AfterSceneLoaded += (Scene scene) => {Peak_07.isSceneActive = true;};
            customScenes.Add(Peak_07);

            var Crawl_10 = new CustomScene("Crawl_10");
            Crawl_10.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(19.66f, 7.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Crawl_10.isSkongScene = true;
            Crawl_10.AfterSceneLoaded += (Scene scene) => {Crawl_10.isSceneActive = true;};
            customScenes.Add(Crawl_10);

            var Shellwood_22 = new CustomScene("Shellwood_22");
            Shellwood_22.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(101.35f, 6.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Shellwood_22.isSkongScene = true;
            Shellwood_22.AfterSceneLoaded += (Scene scene) => {Shellwood_22.isSceneActive = true;};
            customScenes.Add(Shellwood_22);

            var Memory_Ant_Queen = new CustomScene("Memory_Ant_Queen");
            Memory_Ant_Queen.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(148.4f, 19.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: true));
            Memory_Ant_Queen.isSkongScene = true;
            Memory_Ant_Queen.AfterSceneLoaded += (Scene scene) => {Memory_Ant_Queen.isSceneActive = true;};
            customScenes.Add(Memory_Ant_Queen);

            var Coral_39 = new CustomScene("Coral_39");
            Coral_39.AddTransitionPoint(new TransitionPointInfo("start_battle_entry", new Vector3(131f, 7.57f, 0), "", "", isADoor: true, 
            isOneTimeTransition: true, dontWalkOutOfDoor : true));
            Coral_39.isSkongScene = true;
            Coral_39.AfterSceneLoaded += (Scene scene) => {Coral_39.isSceneActive = true;};
            customScenes.Add(Coral_39);
        }
    }
}