using System.Resources;
using Gods_Of_Pharloom;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

public class PatchedFsm
{
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
        public Action<Fsm, CustomWaitConditionFsm> action;
        public bool condition = false;
        public Fsm fsm;
        public override void OnUpdate()
        {
            action?.Invoke(fsm, this);
            if(condition) Finish();
        }
        public CustomWaitConditionFsm(Fsm fsm)
        {
            this.fsm = fsm;
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
        "Belltown_Shrine", //Window
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

        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) =>
        {
            var init = fsm.GetState("Init");
            var choice = fsm.GetState("Choice");
            var idlyFlyAudio = fsm.GetState("Idly Fly Audio?");

            var arenaRange = ((FindNamedChild)(init.Actions[6])).storeResult.Value;
            if(arenaRange == null) GodsOfPharloomMod.Log.LogInfo("WEEEEEEEEEEEEEEEEEEEEEEELLLLL");
            GodsOfPharloomMod.Log.LogInfo(arenaRange.name + "YOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
        };

        rematch.Actions = InsertInArray(rematch.Actions, customAction, 0);
        
        SetTransitionToState(choice, choice, 3);
        SetTransitionToState(choice, choice, 7);

        return true;
    }
}