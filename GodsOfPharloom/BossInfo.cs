using System.Resources;
using Gods_Of_Pharloom;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

public class PatchedFsm
{
    public class FsmPatch
    {
        public string objName;
        public string fsmName;
        public Func<PlayMakerFSM, bool> method;
        public FsmPatch(string objName, string fsmName, Func<PlayMakerFSM, bool> method)
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
        "Greymoor_08", //Moorwing
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

    public static bool PatchFsm_MossMother(PlayMakerFSM PMfsm)
    {
        var fsm = PMfsm.Fsm;

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
    public static bool PatchFsm_BellBeast(PlayMakerFSM PMfsm)
    {
        var fsm = PMfsm.Fsm;

        var submergedInit = fsm.GetState("Submerged Init");
        var emergeAnticC = fsm.GetState("Emerge Antic C");
        var init = fsm.GetState("Init");

        ((Wait)(submergedInit.Actions[5])).time = 0.1f;

        // init.Transitions[0].ToState = emergeAnticC.Name;
        // init.Transitions[0].ToFsmState = emergeAnticC;

        return true;
    }
    public static bool PatchFsm_FourthChorus(PlayMakerFSM PMfsm)
    {
        var fsm = PMfsm.Fsm;

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
    public static bool PatchFsm_FourthChorus_Awake(PlayMakerFSM PMfsm)
    {
        var fsm = PMfsm.Fsm;

        var remeet1 = fsm.GetState("Remeet 1");
        var remeet2 = fsm.GetState("Remeet 2");

        ((Wait)(remeet1.Actions[7])).time = 0f;
        ((Wait)(remeet2.Actions[2])).time = 0f;

        return true;
    }
    public static bool PatchFsm_GreatConchfliesBattleScene(PlayMakerFSM PMfsm)
    {
        var fsm = PMfsm.Fsm;

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
    public static bool PatchFsm_GreatConchfliesDriller(PlayMakerFSM PMfsm)
    {
        var fsm = PMfsm.Fsm;

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
    public static bool PatchFsm_Lace1(PlayMakerFSM PMfsm)
    {
        var fsm = PMfsm.Fsm;

        var enctountred = fsm.GetState("Encountered?");
        var refight = fsm.GetState("Refight");

        SetTransitionToState(enctountred, refight, 0);

        return true;
    }
}