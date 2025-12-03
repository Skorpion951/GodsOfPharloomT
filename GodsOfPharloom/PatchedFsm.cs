using System.Resources;
using GlobalEnums;
using Gods_Of_Pharloom;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEngine.Events;
using HarmonyLib;
using System.Drawing;

public class PatchedFsm
{
    public static Func<object, MethodInfo, object[], object> InvokeMethod = (instance, method, obj) => method.Invoke(instance, obj);
    public static MethodInfo activateChildOnTrigger = AccessTools.Method(typeof(ActivateChildrenOnContact), "OnTriggerEnter2D");
    private class CustomLogicFsm : FsmStateAction
    {
        public Action<Fsm> action;
        public Fsm fsm;
        public override void OnEnter()
        {
            action?.Invoke(fsm);
            Finish();
        }
        public CustomLogicFsm(Fsm fsm)
        {
            this.fsm = fsm;
        }
    }
    private class CustomWaitConditionFsm : FsmStateAction
    {
    }
    private class CustomTrigger : MonoBehaviour
    {
        public Action<Fsm, FsmStateAction> action;
        public FsmStateAction fsmAction;
        public Fsm fsm;
        private void OnTriggerStay2D(Collider2D collider)
        {
            action?.Invoke(fsm, fsmAction);
            Destroy(this.gameObject);
        }
        private void OnTriggerEnter2D(Collider2D collider)
        {
            action?.Invoke(fsm, fsmAction);
            Destroy(this.gameObject);
        }
    }
    public class FsmPatch
    {
        public string objName;
        public string fsmName;
        public Func<Fsm, bool> method;
        public FsmPatch(string objName, string fsmName, Func<Fsm, bool> method)
        {
            this.objName = objName;
            this.fsmName = fsmName;
            this.method = method;
        }
    }
    public string sceneName;
    public FsmPatch[] fsms;

    PatchedFsm(string sceneName, FsmPatch[] fsms)
    {
        this.sceneName = sceneName;
        this.fsms = fsms;
    }
    public static PatchedFsm[] patchedFsms = new PatchedFsm[]{
        new PatchedFsm("Tut_03", new FsmPatch[]
        {
            new FsmPatch("Mossbone Mother", "Control", PatchFsm_MossMother)
        }),
        new PatchedFsm("Weave_03", new FsmPatch[]
        {
            new FsmPatch("Mossbone Mother A", "Control", PatchFsm_MossMother)
        }),
        new PatchedFsm("Bone_05", new FsmPatch[]
        {
            new FsmPatch("Bone Beast", "Control", PatchFsm_BellBeast)
        }),
        new PatchedFsm("Bone_East_08_Boss_Golem", new FsmPatch[]
        {
            new FsmPatch("song_golem", "Control", PatchFsm_FourthChorus)
        }),
        new PatchedFsm("Bone_East_08", new FsmPatch[]
        {
            new FsmPatch("Boss Scene", "Control", PatchFsm_FourthChorus_Awake)
        }),
        new PatchedFsm("Coral_11", new FsmPatch[]
        {
            new FsmPatch("Driller A", "Control", PatchFsm_GreatConchfliesDriller),
            new FsmPatch("Driller B", "Control", PatchFsm_GreatConchfliesDriller),
            new FsmPatch("Boss Scene", "Control", PatchFsm_GreatConchfliesBattleScene)
        }),
        new PatchedFsm("Bone_East_12", new FsmPatch[]
        {
            new FsmPatch("Lace Boss1", "Control", PatchFsm_Lace1)
        }),
        new PatchedFsm("Coral_Judge_Arena", new FsmPatch[]
        {
            new FsmPatch("Last Judge", "Control", PatchFsm_LastJudge),
            new FsmPatch("Boss Scene", "Control", PatchFsm_LastJudgeBattleScene)
        }),
        new PatchedFsm("Greymoor_08_boss", new FsmPatch[]
        {
            new FsmPatch("Vampire Gnat", "Control", PatchFsm_Moorwing)
        }),
        new PatchedFsm("Organ_01", new FsmPatch[]
        {
            new FsmPatch("Phantom", "Control", PatchFsm_Phantom),
            new FsmPatch("Boss Scene", "Control", PatchFsm_PhantomBossScene)
        }),
        new PatchedFsm("Ant_19", new FsmPatch[]
        {
            new FsmPatch("Bone Flyer Giant", "Control", PatchFsm_SavageBeastfly1),
        }),
        new PatchedFsm("Shellwood_18", new FsmPatch[]
        {
            new FsmPatch("Splinter Queen", "Control", PatchFsm_SisterSplinter),
            new FsmPatch("Boss Scene", "Battle Control", PatchFsm_SisterSplinterBossScene),
            new FsmPatch("Approaches", "Control", PatchFsm_SisterSplinterApproaches),
            
        }),
        new PatchedFsm("Bone_15", new FsmPatch[]
        {
            new FsmPatch("Skull King", "Behaviour", PatchFsm_SkullTyrant),
            
        }),
        new PatchedFsm("Belltown_Shrine", new FsmPatch[]
        {
            new FsmPatch("Spinner Boss", "Control", PatchFsm_Widow),
            new FsmPatch("Boss Scene", "Control", PatchFsm_WidowBossScene),
        }),
        new PatchedFsm("Slab_16b", new FsmPatch[]
        {
            new FsmPatch("Slab Fly Broodmother", "Control", PatchFsm_Broodmother),
        }),
        new PatchedFsm("Cog_Dancers", new FsmPatch[]
        {
            new FsmPatch("Boss Scene", "Sequence", PatchFsm_CogDancersBossScene),
        }),
        new PatchedFsm("Cog_Dancers_Boss", new FsmPatch[]
        {
            new FsmPatch("Dancer Control", "Control", PatchFsm_CogDancersDancerControl),
            new FsmPatch("Dancer A", "Control", PatchFsm_CogDancersDancerAB),
            new FsmPatch("Dancer B", "Control", PatchFsm_CogDancersDancerAB),
        }),
        new PatchedFsm("Dust_Chef", new FsmPatch[]
        {
            new FsmPatch("Roachkeeper Chef (1)", "Control", PatchFsm_DustChef),
            new FsmPatch("kitchen_gong", "Tink Hit Force", PatchFsm_DustChefKitchenGong),
        }),
        new PatchedFsm("Belltown_08", new FsmPatch[]
        {
            new FsmPatch("Wisp Pyre Effigy", "Summon Control", PatchFsm_FatherOfFlame),
        }),
        new PatchedFsm("Slab_10b", new FsmPatch[]
        {
            new FsmPatch("First Weaver", "Control", PatchFsm_FirstSinner),
            new FsmPatch("Shrine First Weaver", "Inspection", PatchFsm_FirstSinnerInspection),
            new FsmPatch("Boss Scene", "Outro", PatchFsm_FirstSinnerBossSceneOutro),
        }),
        new PatchedFsm("Dock_09", new FsmPatch[]
        {
            new FsmPatch("Boss Scene", "Control", PatchFsm_ForebrothersSignisAndGronBossScene),
            new FsmPatch("Dock Guard Slasher", "Control", PatchFsm_ForebrothersSignisAndGronSlasher),
            new FsmPatch("Dock Guard Thrower", "Control", PatchFsm_ForebrothersSignisAndGronThrower),
        }),
        new PatchedFsm("Library_09", new FsmPatch[]
        {
            new FsmPatch("Garmond Fighter", "Control", PatchFsm_GarmondAndZaza),
        }),
        new PatchedFsm("Cradle_03", new FsmPatch[]
        {
            new FsmPatch("Silk Boss", "Control", PatchFsm_SilkBoss),
            new FsmPatch("Intro Sequence", "First Challenge", PatchFsm_SilkBossIntroSequence),
            new FsmPatch("Boss Title", "Title Control", PatchFsm_SilkBossTitleControl),
            new FsmPatch("Silk Boss", "Phase Control", PatchFsm_SilkBossPhaseControl),
            new FsmPatch("Challenge Region", "Challenge", PatchFsm_SilkBossChallengeControl),
        }),
        new PatchedFsm("Shadow_18", new FsmPatch[]
        {
            new FsmPatch("Swamp Shaman", "Control", PatchFsm_GroalTheGreat),
        }),

    };
    public enum BossName
    {
        MossMother = 0,
        DoubleMossMother,
        BellBeast,
        FourthChorus,
        GreatConchflies,
        Lace1,
        LastJudge,
        Moorwing,
        Phantom,
        SavageBeastfly1,
        SisterSplinter,
        SkullTyrant,
        Window,
        Broodmother,
        CogworkDancers,
        DisgracedChefLugoli,
        FatherOfTheFlame,
        FirstSinner,
        ForebrothersSignisAndGron,
        GarmondAndZaza,
        GrandMotherSilk,
        GroalTheGreat,
        Lace2,
        RagingConchfly,
        SavageBeastfly2,
        SecondSentiel,
        Shakra,
        TheUnravelled,
        Trobbio,
        Voltvyrm,
        BellEater,
        CloverDancers,
        Crawfather,
        CrustKingKhann,
        GurrTheOutcast,
        LostGarmond,
        LostLace,
        Nyleth,
        Palestag,
        Pinstress,
        PlasmifiedZango,
        ShrineGuardianSeth,
        SkarrsingerKarmelita,
        TormentedTrobbio,
        WatherAtTheEdge
    };
    public static string[] bossesSceneName = new string[]
    {
        "Tut_03", //MossMother
        "Weave_03", //DoubleMossMother
        "Bone_05", //BellBeast
        "Bone_East_08_Boss_Golem", //FourthChorus
        "Coral_11", //GreatConchflies
        "Bone_East_12", //Lace1
        "Coral_Judge_Arena", //LastJudge
        "Greymoor_08_boss", //Moorwing
        "Organ_01", //Phantom
        "Ant_19", //SavageBeastfly1
        "Shellwood_18", //SisterSplinter
        "Bone_15", //Skull Tyrant
        "Belltown_Shrine", //Widow
        "Slab_16b", //Broodmother
        "Cog_Dancers", //CogworkDancers
        "Dust_Chef", //DisgracedChefLugoli
        "Belltown_08", //FatherOfTheFlame
        "Slab_10b", //FirstSinner
        "Dock_09", //ForebrothersSignisAndGron
        "Library_09", //GarmondAndZaza
        "Cradle_03", //GrandMotherSilk
        "Shadow_18", //GroalTheGreat
        "Song_Tower", //Lace2
        "Coral_27", //RagingConchfly
        "Bone_East_08", //SavageBeastfly2
        "Hang_17b", //SecondSentiel
        "Greymoor_08", //Shakra
        "Under_08", //TheUnravelled
        "Library_13", //Trobbio
        "Coral_29", //Voltvyrm
        "Bellway_Centipede_Arena", //BellEater
        "Clover_10", //CloverDancers
        "Room_CrowCourt", //Crawfather
        "Memory_Coral_Tower", //CrustKingKhann
        "Bone_East_18b", //GurrTheOutcast
        "Coral_33", //LostGarmond
        "Abyss_Cocoon", //LostLace
        "Shellwood_11b_Memory", //Nyleth
        "Clover_19", //Palestag
        "Peak_07", //Pinstress
        "Crawl_10", //PlasmifiedZango
        "Shellwood_22", //ShrineGuardianSeth
        "Memory_Ant_Queen", //SkarrsingerKarmelita
        "Library_13", //TormentedTrobbio
        "Coral_39", //WatherAtTheEdge
    };
    public static string[][] bossesGameObjName = new string[][]
    {
        new []{"Mossbone Mother"},
        new []{"Mossbone Mother A"},
        new []{"Bone Beast"},
        new []{"song_golem"},
        new []{"Boss Scene", }

    };
    public static string[][] bossesFsmName = new string[][]
    {
        new[] {"Control"},
        new[] {"Control"},
        new[] {"Control"},
        new[] {"Control"},

    };
    
    public static void SetTransitionToState(FsmState state, FsmState to, int transitionIndex)
    {
        state.Transitions[transitionIndex].ToState = to.Name;
        state.Transitions[transitionIndex].ToFsmState = to;
    }
    public static T[] InsertInArray<T>(T[] array, T elem, int index)
    {
        var list = array.ToList();
        list.Insert(index, elem);
        return list.ToArray();
    }
    public static T[] RemoveFromArray<T>(T[] array, int index)
    {
        var list = array.ToList();
        list.RemoveAt(index);
        return list.ToArray();
    }
    public static GameObject CreateTrigger(string sceneName)
    {
        var customTrigger = new GameObject("CustomTrigger");
        SceneManager.MoveGameObjectToScene(customTrigger, SceneManager.GetSceneByName(sceneName));
        customTrigger.layer = (int)PhysLayers.HERO_DETECTOR;

        var customCollider = customTrigger.AddComponent<BoxCollider2D>();

        customCollider.isTrigger = true;
        customCollider.size = new Vector2(25, 18);
        return customTrigger;
    }

    public static bool PatchFsm_MossMother(Fsm fsm)
    {

        var initState = fsm.GetState("Init");
        var dormantState = fsm.GetState("Dormant");
        var returnReadyState = fsm.GetState("Return Ready");
        var returnAntic = fsm.GetState("Return Antic");
        var roarState = fsm.GetState("Roar");
        var returnIn = fsm.GetState("Return In");
        var returnPause = fsm.GetState("Return Pause");
        
        ((Wait)(roarState.Actions[6])).time = 0.01f;
        ((Wait)(returnAntic.Actions[0])).time = 0;
        ((Wait)(returnPause.Actions[0])).time = 0;
        // ((Wait)(returnIn.Actions[13])).time = 0f;

        
        var initToDormant = initState.Transitions.FirstOrDefault(i => {
            return i.ToState == dormantState.Name;
        });
        initToDormant.ToState = returnReadyState.Name;
        initToDormant.ToFsmState = returnReadyState;

        return true;
    }
    public static bool PatchFsm_BellBeast(Fsm fsm)
    {

        var submergedInit = fsm.GetState("Submerged Init");
        var emergeAnticC = fsm.GetState("Emerge Antic C");
        var init = fsm.GetState("Init");

        ((Wait)(submergedInit.Actions[5])).time = 0.1f;

        // init.Transitions[0].ToState = emergeAnticC.Name;
        // init.Transitions[0].ToFsmState = emergeAnticC;

        return true;
    }
    public static bool PatchFsm_FourthChorus(Fsm fsm)
    {

        var init = fsm.GetState("Init");
        var meetRoar1 = fsm.GetState("Meet Roar 1");
        var remeetRoar = fsm.GetState("Remeet Roar");
        var roarClamp = fsm.GetState("Roar Clamp");
        var roarNoClamp = fsm.GetState("Roar No Clamp");
        var meet = fsm.GetState("Meet?");

        ((Wait)(roarNoClamp.Actions[11])).time = 0.1f;
        ((Wait)(roarClamp.Actions[11])).time = 0.1f;

        init.Transitions[0].ToState = remeetRoar.Name;
        init.Transitions[0].ToFsmState = remeetRoar;

        remeetRoar.Transitions[0].ToState = roarNoClamp.Name;
        remeetRoar.Transitions[0].ToFsmState = roarNoClamp;

        return true;
    }
    public static bool PatchFsm_FourthChorus_Awake(Fsm fsm)
    {

        var remeet1 = fsm.GetState("Remeet 1");
        var remeet2 = fsm.GetState("Remeet 2");

        ((Wait)(remeet1.Actions[7])).time = 0f;
        ((Wait)(remeet2.Actions[2])).time = 0f;

        return true;
    }
    public static bool PatchFsm_GreatConchfliesBattleScene(Fsm fsm)
    {

        var arenaStart = fsm.GetState("Arena Start");
        var startPauseS = fsm.GetState("Start Pause S");
        var state = fsm.GetState("State");
        var restartReady = fsm.GetState("Restart Ready");


        ((Wait)(startPauseS.Actions[0])).time = 0f;

        arenaStart.Transitions[1].ToState = startPauseS.Name;
        arenaStart.Transitions[1].ToFsmState = startPauseS;

        state.Transitions[1].ToState = restartReady.Name;
        state.Transitions[1].ToFsmState = restartReady;

        // restartReady.Transitions[0].ToState = startPauseS.Name;
        // restartReady.Transitions[0].ToFsmState = startPauseS;

        return true;
    }
    public static bool PatchFsm_GreatConchfliesDriller(Fsm fsm)
    {

        var dormant = fsm.GetState("Dormant");
        var introR1 = fsm.GetState("Intro R 1");
        var introG1 = fsm.GetState("Intro G 1");
        var introR2 = fsm.GetState("Intro R 2");
        var introG2 = fsm.GetState("Intro G 2");
        var introR3 = fsm.GetState("Intro R 3");
        var introG3 = fsm.GetState("Intro G 3");

        var roarG = fsm.GetState("Roar G");
        var roarR = fsm.GetState("Roar R");

        // ((Wait)(introR1.Actions[1])).time = ((Wait)(introR1.Actions[1])).time.Value / 2;
        // ((Wait)(introG1.Actions[3])).time = ((Wait)(introG1.Actions[3])).time.Value / 2;
        ((Wait)(introR2.Actions[1])).time = 0.1f;//((Wait)(introR2.Actions[1])).time.Value / 2;
        ((Wait)(introG2.Actions[1])).time = 0.1f;//((Wait)(introG2.Actions[1])).time.Value / 2;
        ((Wait)(introR3.Actions[2])).time = 0;//((Wait)(introR3.Actions[2])).time.Value / 2;
        ((Wait)(introG3.Actions[4])).time = 0;//((Wait)(introG3.Actions[4])).time.Value / 2;

        ((AnimatePositionTo)(introG2.Actions[0])).time = 0.1f;
        ((AnimatePositionTo)(introR2.Actions[0])).time = 0.1f;

        ((Wait)(roarR.Actions[0])).time = 0.1f;
        ((Wait)(roarG.Actions[1])).time = 0.1f;

        SetTransitionToState(dormant, introG2, 0);
        SetTransitionToState(dormant, introR2, 1);

        return true;
    }
    public static bool PatchFsm_Lace1(Fsm fsm)
    {

        var enctountred = fsm.GetState("Encountered?");
        var refight = fsm.GetState("Refight");

        SetTransitionToState(enctountred, refight, 0);

        return true;
    }
    public static bool PatchFsm_LastJudge(Fsm fsm)
    {
        var introRoar = fsm.GetState("Intro Roar");
        var introFallAnticQ = fsm.GetState("Intro Fall Antic Q");

        ((Wait)(introRoar.Actions[1])).time = 0.1f;
        ((Wait)(introFallAnticQ.Actions[4])).time = 0f;

        return true;
    }
    public static bool PatchFsm_LastJudgeBattleScene(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var encountered = fsm.GetState("Encountered");

        SetTransitionToState(init, encountered, 0);

        return true;
    }
    public static bool PatchFsm_Moorwing(Fsm fsm)
    {
        var roar = fsm.GetState("Roar");
        var quickRoar = fsm.GetState("Quick Roar");

        ((Wait)(roar.Actions[2])).time = 0.1f;
        ((Wait)(quickRoar.Actions[0])).time = 0.1f;

        return true;
    }
    public static bool PatchFsm_Phantom(Fsm fsm)
    {

        return true;
    }
    public static bool PatchFsm_PhantomBossScene(Fsm fsm)
    {
        var BGFog = fsm.GetState("BG Fog");
        var FGAntic = fsm.GetState("FG Antic");
        var init = fsm.GetState("Init");
        var enter = fsm.GetState("Enter");
        var FGColumn = fsm.GetState("FG Column");
        var organNote = fsm.GetState("Organ Note");

        ((Wait)(BGFog.Actions[0])).time = 0.1f;
        ((Wait)(enter.Actions[3])).time = 0f;
        ((Wait)(FGColumn.Actions[5])).time = 0.1f;
        ((Wait)(organNote.Actions[3])).time = 0.1f;

        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) =>
        {
            var init = fsm.GetState("Init");

            var fogColumnAnticAction = (FindNamedChild)(init.Actions[6]);
            var fogColumnFG = (FindChild)(init.Actions[5]);
            var fogColumnBG = (FindChild)(init.Actions[4]);
            var fogDamagerAction = (FindNamedChild)(init.Actions[7]);
            var phantomBoss = (FindChild)(init.Actions[0]);

            var fogColumnAnticGO = fogColumnAnticAction.storeResult.Value;
            var fogColumnBGGO = fogColumnBG.storeResult.Value;
            var fogDamagerGO = fogDamagerAction.storeResult.Value;
            var fogColumnFGGO = fogColumnFG.storeResult.Value;
            var phantomBossGO = phantomBoss.storeResult.Value;

            int xPos = 15;

            if(HeroController.instance.transform.position.x > fogColumnBGGO.transform.position.x) return;

            fogDamagerGO.transform.position = new Vector3(fogDamagerGO.transform.position.x + xPos, fogDamagerGO.transform.position.y, fogDamagerGO.transform.position.z);
            fogColumnAnticGO.transform.position = new Vector3(fogColumnAnticGO.transform.position.x + xPos, fogColumnAnticGO.transform.position.y, fogColumnAnticGO.transform.position.z);
            fogColumnFGGO.transform.position = new Vector3(fogColumnFGGO.transform.position.x + xPos, fogColumnFGGO.transform.position.y, fogColumnFGGO.transform.position.z);
            phantomBossGO.transform.position = new Vector3(phantomBossGO.transform.position.x + xPos, phantomBossGO.transform.position.y, phantomBossGO.transform.position.z);
        };

        var list = BGFog.Actions.ToList();
        list.Insert(list.Count - 1, customAction);
        BGFog.Actions = list.ToArray();

        return true;
    }
    public static bool PatchFsm_SavageBeastfly1(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var choice = fsm.GetState("Choice");
        var idlyFlyAudio = fsm.GetState("Idly Fly Audio?");
        var rematch = fsm.GetState("Rematch?");
        var introLook = fsm.GetState("Intro Look");
        var introRoar = fsm.GetState("Intro Roar");

        var waitForCondition = new CustomWaitConditionFsm();

        choice.Transitions = RemoveFromArray(choice.Transitions, 3);
        rematch.Actions = InsertInArray(rematch.Actions, waitForCondition, 0);

        ((WaitRandom)(introLook.Actions[5])).timeMin = 0f;
        ((WaitRandom)(introLook.Actions[5])).timeMax = 0f;
        ((Wait)(introRoar.Actions[0])).time = 0.1f;
        
        SetTransitionToState(rematch, introLook, 0);

        var customTrigger = CreateTrigger("Ant_19");
        var triggerPos = customTrigger.transform.position;
        customTrigger.transform.position = new Vector3(43.45f, 39.28f, triggerPos.z);

        var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
        triggerComponent.fsmAction = waitForCondition;
        triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
        {
            fsmAction.Finish();
        };


        return true;
    }
    public static bool PatchFsm_SisterSplinter(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var introShake = fsm.GetState("Intro Shake");
        var emergeAntic = fsm.GetState("Emerge Antic");
        var roar4 = fsm.GetState("Roar 4");

        ((Wait)(introShake.Actions[1])).time = 0f;
        ((Wait)(emergeAntic.Actions[1])).time = 0f;
        ((Wait)(roar4.Actions[4])).time = 0.1f;

        return true;
    }
    public static bool PatchFsm_SisterSplinterBossScene(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var battleStart = fsm.GetState("Battle Start");

        ((SendEventByName)(battleStart.Actions[2])).delay = 0f;

        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) =>
        {
            var init = fsm.GetState("Init");

            var battleStartRange = ((FindNamedChild)(init.Actions[2])).storeResult.Value;

            var battleStartRangeCollider = battleStartRange.GetComponent<BoxCollider2D>();
            battleStartRangeCollider.size = new Vector2(26.6f, battleStartRangeCollider.size.y);
        };

        init.Actions = InsertInArray(init.Actions, customAction, 3);

        return true;
    }
    public static bool PatchFsm_SisterSplinterApproaches(Fsm fsm)
    {
        var pause = fsm.GetState("Pause");

        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) => {
            var customTrigger = new GameObject("TriggerForStartBoss");
            SceneManager.MoveGameObjectToScene(customTrigger, SceneManager.GetSceneByName("Shellwood_18"));
            customTrigger.transform.position = new Vector3(45f, 11f, 0);
            customTrigger.layer = (int)PhysLayers.HERO_DETECTOR;

            var customCollider = customTrigger.AddComponent<BoxCollider2D>();
            var doWork = customTrigger.AddComponent<CustomTrigger>();

            doWork.fsm = fsm;

            customCollider.isTrigger = true;
            customCollider.size = new Vector2(28.3f, 18);

            doWork.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                var pause = fsm.GetState("Pause");

                var approachR = ((FindNamedChild)(pause.Actions[0])).storeResult.Value;
                var approachL = ((FindNamedChild)(pause.Actions[1])).storeResult.Value;


                foreach(Transform child in approachR.transform)
                {
                    var go = child.gameObject;
                    var collider = go.GetComponent<BoxCollider2D>();
                    if(collider == null) continue;
                    var activateChild = go.GetComponent<ActivateChildrenOnContact>();
                    ((UnityEvent)(activateChild.GetType().GetField("onContact", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(activateChild))).Invoke();
                    InvokeMethod(activateChild, activateChildOnTrigger, new object[]{null});
                }
                foreach(Transform child in approachL.transform)
                {
                    var go = child.gameObject;
                    var collider = go.GetComponent<BoxCollider2D>();
                    if(collider == null) continue;
                    var activateChild = go.GetComponent<ActivateChildrenOnContact>();
                    ((UnityEvent)(activateChild.GetType().GetField("onContact", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(activateChild))).Invoke();
                    InvokeMethod(activateChild, activateChildOnTrigger, new object[]{null});
                }
            };
        };

        pause.Actions = InsertInArray(pause.Actions, customAction, pause.Actions.Length - 1);
        return true;
    }
    public static bool PatchFsm_SkullTyrant(Fsm fsm)
    {
        PlayerData.instance.skullKingAwake = true;
        var init = fsm.GetState("Init");
        var stateCheck = fsm.GetState("State Check");
        var inRoof = fsm.GetState("In Roof");
        var wakeRoar = fsm.GetState("Wake Roar");
        var rewakePause = fsm.GetState("Rewake Pause");
        var rewakeAntic = fsm.GetState("Rewake Antic");
        
        ((Wait)(wakeRoar.Actions[4])).time = 0.1f;
        ((Wait)(rewakePause.Actions[1])).time = 0f;
        ((Wait)(rewakeAntic.Actions[1])).time = 0.1f;

        return true;
    }
    public static bool PatchFsm_Widow(Fsm fsm)
    {
        PlayerData.instance.encounteredSpinner = true;
        var init = fsm.GetState("Init");
        var introScream = fsm.GetState("Intro Scream");
        var setRage = fsm.GetState("Set Rage");
        var deathStaggerF = fsm.GetState("Death Stagger F");
        var rageScream2 = fsm.GetState("Rage Scream 2");
        var away = fsm.GetState("Away");

        ((Wait)(introScream.Actions[3])).time = 0.1f;
        ((Wait)(deathStaggerF.Actions[16])).time = 0.1f;
        ((Wait)(rageScream2.Actions[1])).time = 0.1f;
        ((Wait)(away.Actions[1])).time = 0.01f;
        ((Wait)(setRage.Actions[3])).time = 0.5f;


        return true;
    }
    public static bool PatchFsm_WidowBossScene(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var spinnerLook = fsm.GetState("Spinner Look");
        var spinnerAway = fsm.GetState("Spinner Away");

        ((Wait)(spinnerLook.Actions[2])).time = 0f;
        ((Wait)(spinnerAway.Actions[0])).time = 0.01f;

        return true;
    }
    public static bool PatchFsm_Broodmother(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var entryAntic = fsm.GetState("Entry Antic");
        var roar = fsm.GetState("Roar");

        ((Wait)(entryAntic.Actions[5])).time = 0.01f;
        ((Wait)(roar.Actions[8])).time = 0.1f;

        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) =>
        {
            var init = fsm.GetState("Init");

            var battleScene = ((GetGrandParent)(init.Actions[0])).storeResult.Value;

            var battleSceneComponent = battleScene.GetComponent<BattleScene>();
            battleSceneComponent.battleStartPause = 0f;
            battleSceneComponent.waves.RemoveRange(0, 3);

            var wave04 = battleSceneComponent.waves[0];
            wave04.startDelay = 0;
        };

        init.Actions = InsertInArray(init.Actions, customAction, init.Actions.Length - 1);

        return true;
    }
    public static bool PatchFsm_CogDancersDancerControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var dormant = fsm.GetState("Dormant");
        var gateClose = fsm.GetState("Gate Close");
        var beatStartPause = fsm.GetState("Beat Start Pause");
        var pendulumPrepare = fsm.GetState("Pendulum Prepare");
        var beatStart = fsm.GetState("Beat Start");
        var deathPause = fsm.GetState("Death Pause");
        var returnDancers = fsm.GetState("Return Dancers");
        var dancersStunned = fsm.GetState("Dancers Stunned");
        var lightOpen = fsm.GetState("Light Open");

        ((EaseFloat)(lightOpen.Actions[3])).time = 0.01f;

        ((SendEventByName)(dormant.Actions[1])).delay = 0.01f;
        ((Wait)(gateClose.Actions[2])).time = 0.01f;
        ((WaitBool)(gateClose.Actions[3])).time = 0.01f;
        ((Wait)(beatStartPause.Actions[4])).time = 0.01f;
        ((Wait)(pendulumPrepare.Actions[3])).time = 0.01f;
        ((Wait)(beatStart.Actions[1])).time = 0.01f;
        ((WaitBool)(beatStart.Actions[2])).time = 0.01f;
        ((Wait)(deathPause.Actions[2])).time = 0.01f;
        ((Wait)(returnDancers.Actions[4])).time = 0.3f;
        ((Wait)(dancersStunned.Actions[6])).time = 0.3f;

        

        return true;
    }
    public static bool PatchFsm_CogDancersDancerAB(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var doRoar = fsm.GetState("Do Roar");
        var subRoar = fsm.GetState("Sub Roar");
        var deathSteam = fsm.GetState("Death Steam");
        var stunStagger = fsm.GetState("Stun Stagger");
        var stunOutOfCombo = fsm.GetState("Stun Out of Combo");
        var deathStagger = fsm.GetState("Death Stagger");
        var windup = fsm.GetState("Windup");
        var windupOB = fsm.GetState("Windup OB");
        var OBPause = fsm.GetState("OB Pause");
        var emerge = fsm.GetState("Emerge");
        var rest = fsm.GetState("Rest");
        var returnToRest = fsm.GetState("Return To Rest");
        var return2 = fsm.GetState("Return 2");
        var firstWindup = fsm.GetState("First Windup?");
        var firstWindupOB = fsm.GetState("First Windup? OB");

        ((IntCompare)(firstWindup.Actions[0])).integer2 = 1;
        ((IntCompare)(firstWindupOB.Actions[0])).integer2 = 1;


        ((AnimatePositionTo)(returnToRest.Actions[7])).speed = 10f;
        ((AnimatePositionTo)(return2.Actions[0])).speed = 10f;
        ((AnimatePositionTo)(emerge.Actions[11])).speed = 2f;

        
        ((Wait)(doRoar.Actions[3])).time = 0.1f;
        ((Wait)(subRoar.Actions[2])).time = 0.1f;
        ((Wait)(deathSteam.Actions[2])).time = 0.4f;
        ((Wait)(windup.Actions[3])).time = 0.1f;
        ((Wait)(windupOB.Actions[3])).time = 0.1f;
        ((Wait)(OBPause.Actions[0])).time = 0f;

        // ((Wait)(stunStagger.Actions.OfType<Wait>().FirstOrDefault())).time = 0.5f;

        // ((stunStagger.Actions.OfType<SendEventByName>().FirstOrDefault(item => item.sendEvent.Value == "SURPRISE"))).delay = 0f;
        // ((Wait)(stunOutOfCombo.Actions[5])).time = 0.1f;
        // ((SendEventByName)(deathStagger.Actions[3])).delay = 0f;
        // ((Wait)(deathStagger.Actions[18])).time = 0.1f;
        // ((AudioPlayerOneShotSingle)(emerge.Actions[2])).delay = 0.1f;
        // ((AudioPlayerOneShotSingle)(emerge.Actions[3])).delay = 0.1f;

        // SetTransitionToState(rest, emerge, 0);
        // SetTransitionToState(rest, emerge, 1);

        return true;
    }
    public static bool PatchFsm_CogDancersBossScene(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var gatesClose = fsm.GetState("Gates Close");
        var wait = fsm.GetState("Wait");
        var rotationSequence = fsm.GetState("Rotation Sequence");
        
        ((Wait)(wait.Actions[4])).time = 0.1f;
        ((Wait)(wait.Actions[7])).time = 0.1f;
        ((Wait)(gatesClose.Actions[2])).time = 0.01f;

        return true;
    }
    public static bool PatchFsm_DustChef(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var entryRoar = fsm.GetState("Entry Roar");
        var entryAntic = fsm.GetState("Entry Antic");
        
        ((Wait)(entryRoar.Actions[5])).time = 0.1f;
        ((Wait)(entryAntic.Actions[3])).time = 0.1f;
        
        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) =>
        {
            var init = fsm.GetState("Init");
            var battleScene = ((GetGrandParent)(init.Actions[0])).storeResult.Value.GetComponent<BattleScene>();
            battleScene.battleStartPause = 0f;

            battleScene.battleStartEventRegister = "BATTLE LOCK";

            battleScene.waves.RemoveAt(0);
            battleScene.waves[0].startDelay = 0f;

            var children = battleScene.gameObject.transform;
            foreach(Transform child in children)
            {
                if(child.gameObject.name == "Wave 1") child.gameObject.SetActive(false);
                if(child.gameObject.name == "Roachkeeper Chef Tiny (2)") child.gameObject.SetActive(false);
            }
            
            //Create trigger for start battle
            var customTrigger = CreateTrigger("Dust_Chef");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(42.45f, 39.28f, triggerPos.z);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                var init = fsm.GetState("Init");
                var battleScene = ((GetGrandParent)(init.Actions[0])).storeResult.Value.GetComponent<BattleScene>();

                battleScene.StartBattle();
            };
        };

        init.Actions = InsertInArray(init.Actions, customAction, init.Actions.Length - 1);

        return true;
    }
    public static bool PatchFsm_DustChefKitchenGong(Fsm fsm)
    {
        var kitchenGong = fsm.GameObject;
        var kitchenString = kitchenGong.transform.parent.gameObject;
        kitchenString.SetActive(false);

        return true;
    }
    public static bool PatchFsm_FatherOfFlame(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var setHp = fsm.GetState("Set HP");
        var intro = fsm.GetState("Intro");
        var brokenPause = fsm.GetState("Broken Pause");
        var flareUp = fsm.GetState("Flare Up");
        

        ((Wait)(init.Actions[40])).time = 0.01f;
        ((Wait)(setHp.Actions[9])).time = 0f;
        ((Wait)(intro.Actions[3])).time = 0f;
        ((Wait)(intro.Actions[6])).time = 0f;
        ((Wait)(intro.Actions[17])).time = 0f;
        ((Wait)(intro.Actions[20])).time = 0.1f;
        ((Wait)(brokenPause.Actions[0])).time = 0.01f;
        ((Wait)(brokenPause.Actions[3])).time = 0.01f;
        ((Wait)(flareUp.Actions[11])).time = 0.5f;


        return true;
    }
    public static bool PatchFsm_FirstSinner(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var dormant = fsm.GetState("Dormant");
        var introWave = fsm.GetState("Intro Wave");
        var introStand = fsm.GetState("Intro Stand");
        var roar = fsm.GetState("Roar");
        var p2TelePause = fsm.GetState("P2 Tele Pause");
        var p2Roar = fsm.GetState("P2 Roar");

        if(init == null) return false;

        ((Wait)(introWave.Actions[7])).time = 0.01f;
        ((Wait)(introStand.Actions[2])).time = 0.01f;
        ((Wait)(roar.Actions[4])).time = 0.1f;
        ((Wait)(p2TelePause.Actions[1])).time = 0f;
        ((Wait)(p2Roar.Actions[5])).time = 1f;

        return true;
    }
    public static bool PatchFsm_FirstSinnerBossSceneOutro(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var pause = fsm.GetState("Pause");

        ((Wait)(pause.Actions[0])).time = 0.01f;

        return true;
    }
    public static bool PatchFsm_FirstSinnerInspection(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var bindStart = fsm.GetState("Bind Start");
        var blowStart = fsm.GetState("Blow Start");
        var bind = fsm.GetState("Bind");
        var breakOut = fsm.GetState("Break Out");
        var introLand = fsm.GetState("Intro Land");

        ((Wait)(breakOut.Actions[16])).time = 0.01f;

        var waitEvent = new CustomWaitConditionFsm();

        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) =>
        {
            var walkArea = ((FindChild)(init.Actions[13])).storeResult.Value;
            walkArea.SetActive(false);

            var customTrigger = CreateTrigger("Slab_10b");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(48.7f, 11f, triggerPos.z);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                waitEvent.Finish();
            };
        };
        
        init.Actions = InsertInArray(init.Actions, customAction, init.Actions.Length - 1);
        init.Actions = InsertInArray(init.Actions, waitEvent, init.Actions.Length - 1);

        breakOut.Actions = RemoveFromArray(breakOut.Actions, 12);

        SetTransitionToState(init, breakOut, 1);

        GameObject.DestroyImmediate(fsm.FsmComponent.gameObject.GetComponent<PlayMakerNPC>());

        return true;
    }
    public static bool PatchFsm_ForebrothersSignisAndGronBossScene(Fsm fsm)
    {
        var init = fsm.GetState("Init");

        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) =>
        {
            var introMinions = ((FindNamedChild)init.Actions[11]).storeResult.Value;
            introMinions.SetActive(false);
        };

        init.Actions = InsertInArray(init.Actions, customAction, init.Actions.Length - 1);

        return true;
    }
    public static bool PatchFsm_ForebrothersSignisAndGronSlasher(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var startRangeCheck = fsm.GetState("Start Range Check");
        var roar = fsm.GetState("Roar");

        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) =>
        {
            var gates = ((FindNamedChild)init.Actions[2]).storeResult.Value;
            var gate2 = gates.transform.GetChild(1).gameObject;

            if(HeroController.instance.transform.position.x < gate2.transform.position.x)
            {
                var pos = fsm.GameObject.transform.position;
                fsm.GameObject.transform.position = new Vector3(41.5f, pos.y, pos.z);

                var children = fsm.GameObject.transform;
                foreach(Transform child in children)
                {
                    if(child.gameObject.name == "Start Range")
                    {
                        var childPos = child.gameObject.transform.position;
                        child.gameObject.transform.position = new Vector3(18f, childPos.y, childPos.z);
                        break;
                    }
                }
            }
        };

        ((Wait)(roar.Actions[4])).time = 0.1f;

        SetTransitionToState(init, startRangeCheck, 0);

        init.Actions = InsertInArray(init.Actions, customAction, init.Actions.Length - 1);

        return true;
    }
    public static bool PatchFsm_ForebrothersSignisAndGronThrower(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        

        

        return true;
    }
    public static bool PatchFsm_GarmondAndZaza(Fsm fsm)
    {
        PlayerData.instance.garmondInLibrary = true;
        var init = fsm.GetState("Init");
        var roarAntic = fsm.GetState("Roar Antic");
        var autoTarget = fsm.GetState("Auto Target?");
        var appearRange = fsm.GetState("Appear Range");
        var citNPC = fsm.GetState("Cit NPC");
        var citadelRemeet = fsm.GetState("Citadel Remeet");
        var enemyRoar = fsm.GetState("Enemy Roar");
        var setup1 = fsm.GetState("Setup 1");
        var setup2 = fsm.GetState("Setup 2");

        ((Wait)(setup1.Actions[15])).time = 0.001f;
        ((Wait)(setup2.Actions[3])).time = 0.001f;
        ((Wait)(enemyRoar.Actions[3])).time = 0.1f;
        
        var customInit = new CustomLogicFsm(fsm);
        customInit.action += (Fsm fsm) =>
        {
            var citadelLibraryNPC = ((FindNamedChild)init.Actions[13]).storeResult.Value;
            citadelLibraryNPC.SetActive(false);
        };

        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) =>
        {
            var pos = fsm.GameObject.transform.position;
            fsm.GameObject.transform.position = new Vector3(80.4f, pos.y, pos.z);

            var customTrigger = CreateTrigger("Library_09");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(80.7f, 15f, triggerPos.z);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                fsm.FsmComponent.SendEvent("BATTLE START");
            };
        };

        citNPC.Actions = InsertInArray(citNPC.Actions, customAction, citNPC.Actions.Length - 1);
        init.Actions = InsertInArray(init.Actions, customAction, init.Actions.Length - 1);

        return true;
    }
    public static bool PatchFsm_SilkBoss(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var introUp = fsm.GetState("Intro Up");
        var introRoar = fsm.GetState("Intro Roar");
        var titleUp = fsm.GetState("Title Up");
        var moveStop = fsm.GetState("Move Stop");
        
        ((Wait)(init.Actions[8])).time = 0f;
        ((AnimatePositionBy)(introUp.Actions[7])).time = 0.1f;
        ((Wait)(introUp.Actions[8])).time = 0.1f;
        ((Wait)(introRoar.Actions[4])).time = 0.1f;
        ((Wait)(titleUp.Actions[2])).time = 0.1f;
        ((Wait)(moveStop.Actions[1])).time = 0f;
        

        return true;
    }
    public static bool PatchFsm_SilkBossIntroSequence(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var waitForBeatEnd = fsm.GetState("Wait For Beat End");
        var burstAnim = fsm.GetState("Burst Anim");
        var readyWait = fsm.GetState("Ready Wait");
        var introShake = fsm.GetState("Intro Shake");
        var quickStart = fsm.GetState("Quick Start");
        
        ((Wait)(waitForBeatEnd.Actions[2])).time = 0.002f;
        ((SendEventByName)(waitForBeatEnd.Actions[1])).delay = 0.001f;
        ((Wait)(readyWait.Actions[1])).time = 0.1f;
        ((Wait)(introShake.Actions[1])).time = 0.1f;

        // var pos = fsm.GameObject.transform.position;
        // fsm.GameObject.transform.position = new Vector3(pos.x + 3, pos.y, pos.z);

        burstAnim.Actions = RemoveFromArray(burstAnim.Actions, 0);
        burstAnim.Actions = RemoveFromArray(burstAnim.Actions, 0);

        SetTransitionToState(waitForBeatEnd, quickStart, 0);
        
        return true;
    }
    public static bool PatchFsm_SilkBossTitleControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var titleUp = fsm.GetState("Title Up");
        
        ((Wait)(titleUp.Actions[1])).time = 0.1f;

        return true;
    }
    public static bool PatchFsm_SilkBossPhaseControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var staggerPause = fsm.GetState("Stagger Pause");
        var staggerFall = fsm.GetState("Stagger Fall");
        var staggerHit = fsm.GetState("Stagger Hit");
        
        ((Wait)(staggerPause.Actions[1])).time = 0.1f;
        // ((Wait)(staggerFall.Actions[4])).time = 0.1f;
        ((Wait)(staggerHit.Actions[14])).time = 0f;
        // ((AccelerateToY)(staggerFall.Actions[3])).targetSpeed = -90f;

        return true;
    }
    public static bool PatchFsm_SilkBossChallengeControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var idle = fsm.GetState("Idle");
        var hornetVoice = fsm.GetState("Hornet Voice");
        var inRegion = fsm.GetState("In Region");

        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.SendEvent("SPECIAL CHALLENGE");
        };

        inRegion.Actions = InsertInArray(inRegion.Actions, customAction, inRegion.Actions.Length - 1);

        return true;
    }
    public static bool PatchFsm_GroalTheGreat(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var fakeBattleEnd = fsm.GetState("Fake Battle End");
        var entryAntic = fsm.GetState("Entry Antic");
        var entryRoar = fsm.GetState("Entry Roar");
        var dormant = fsm.GetState("Dormant");
        
        ((Wait)(entryRoar.Actions[2])).time = 0.1f;
        ((Wait)(entryAntic.Actions[1])).time = 0.01f;

        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) =>
        {
            if(PlayerData.instance.DefeatedSwampShaman) return;
            var battleScene = ((GetGrandparent)init.Actions[5]).storeResult.Value;
            var battleSceneComponent = battleScene.GetComponent<BattleScene>();
            var collider = battleScene.GetComponent<BoxCollider2D>();

            battleSceneComponent.battleStartPause = 0f;
            battleSceneComponent.waves.RemoveRange(0, 5);

            collider.size = new Vector2(29.5f, collider.size.y);
        };

        init.Actions = InsertInArray(init.Actions, customAction, init.Actions.Length - 1);

        SetTransitionToState(dormant, entryAntic, 0);
        return true;
    }
}