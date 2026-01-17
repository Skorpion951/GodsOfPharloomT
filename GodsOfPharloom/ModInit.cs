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
            "gg_resources"
        };
        public static List<AssetBundle> assetBundles = new List<AssetBundle>();
        public static List<CustomScene> customScenes = new List<CustomScene>();
        public static BepInEx.Logging.ManualLogSource Log;
        Keyboard keyboard;
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

            try{
            BossInfo.InitBossesInfo();
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
            // afterSceneActivated += () =>
            // {
            //     if(previousSceneName == "Menu_Title") BindingsMenu.InitBindingsMenu();
            // };

            // BindingsMenu.InitBindingsMenuFsmHistory();

            Harmony.CreateAndPatchAll(typeof(GodsOfPharloomMod));
            InitCustomScenes();

            SceneManager.activeSceneChanged += OnSceneChanged;

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

            Logger.LogInfo($"Plugin is loaded!");
        }

        void Update()
        {
            if (Keyboard.current.backquoteKey.wasPressedThisFrame)
            {
                FastTeleport.Start();
            }
            if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                if(BindingsMenu.menuBindingsFsm != null && BindingsMenu.menuBindingsFsm.ActiveStateName == "Opened") BindingsMenu.menuBindingsFsm.FsmComponent.SendEvent("CLOSE");
                else if(BindingsMenu.menuBindingsFsm.ActiveStateName == "Closed") BindingsMenu.menuBindingsFsm.SetState("Can Open Inventory?");
            }
            if (Keyboard.current.backspaceKey.wasPressedThisFrame)
            {
                PlayerData.instance.GetType().GetProperty("nailDamage", BindingFlags.Instance | BindingFlags.Public).SetValue(PlayerData.instance, 100);
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

        public static void SetHeroState(ActorStates state)
        {
            Func<MethodInfo, object[], object> InvokeMethod = (method, obj) => method.Invoke(HeroController.instance, obj);
            InvokeMethod(HeroController_SetState, new object[] {state});
        }

        void InitCustomScenes()
        {
            var GG_Pharloom_Atrium = new CustomScene("GG_Pharloom_Atrium", isFastSuperJump: true, isSkongScene: false);
            GG_Pharloom_Atrium.AddTransitionPoint(new TransitionPointInfo("door1", new Vector3(131.08f, 73.9f, 0), "Belltown", "door1", isADoor: true, noInputOnStart: false));
            GG_Pharloom_Atrium.AddTransitionPoint(new TransitionPointInfo("door2", new Vector3(108.9f, 54f, 0), BossStatueInfo.hog_sceneName, "door1", isADoor: true, noInputOnStart: false));
            GG_Pharloom_Atrium.AfterSceneLoaded += (Scene scene) => {
                var audioManager = GameManager.instance.AudioManager;
                audioManager.StopAndClearAtmos();
                audioManager.StopAndClearMusic();

                var rootObjects = scene.GetRootGameObjects();

                //add memory entry
                var wakeInMemory = (GameObject)Instantiate(Preload.preloads["door_wakeInMemory_AntQueen"], scene: scene);
                wakeInMemory.transform.position = new Vector3(14f, 54f, 0);
                var wakeInMemoryFSM = wakeInMemory.GetComponent<PlayMakerFSM>().Fsm;
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

                //init pantheons
                Pantheon pComp;
                var pantheon1 = Preload.FindObjectByPath(rootObjects, "Half1/Pantheon1");
                pComp = pantheon1.AddComponent<Pantheon>();
                pComp.pantheonName = "Pantheon 1";
                pComp.Init();

                var pantheon2 = Preload.FindObjectByPath(rootObjects, "Half1/Pantheon2");
                pComp = pantheon2.AddComponent<Pantheon>();
                pComp.pantheonName = "Pantheon 2";
                pComp.Init();

                var pantheon3 = Preload.FindObjectByPath(rootObjects, "Half2/Pantheon3");
                pComp = pantheon3.AddComponent<Pantheon>();
                pComp.pantheonName = "Pantheon 3";
                pComp.Init();

                var pantheon4 = Preload.FindObjectByPath(rootObjects, "Half2/Pantheon4");
                pComp = pantheon4.AddComponent<Pantheon>();
                pComp.pantheonName = "Pantheon 4";
                pComp.Init();

                var pantheon5 = Preload.FindObjectByPath(rootObjects, "Pantheon5");
                pComp = pantheon5.AddComponent<Pantheon>();
                pComp.pantheonName = "Pantheon 5";
                pComp.Init();

                GG_Pharloom_Atrium.isSceneActive = true;
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
                bench.name = "RestBench";
                bench.GetComponent<SpriteRenderer>().enabled = false;
                /////////////
                
                //add a scene manager
                var sceneManager = Instantiate(Preload.preloads["_SceneManager_Ant_17"], parameters: new InstantiateParameters
                {
                    scene = scene,
                });
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

                GG_Pharloom_HoG.isSceneActive = true;
            };
            customScenes.Add(GG_Pharloom_HoG);

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
            isOneTimeTransition: true, dontWalkOutOfDoor : true, noInputOnStart: false));
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