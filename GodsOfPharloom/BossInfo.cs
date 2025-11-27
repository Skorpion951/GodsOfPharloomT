using Gods_Of_Pharloom;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

public static class BossesInfo
{
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
        "Bone_East_08", //FourthChorus
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
        new []{"song_golem"}

    };
    public static string[][] bossesFsmName = new string[][]
    {
        new[] {"Control"},
        new[] {"Control"},
        new[] {"Control"},
        new[] {"Control"},

    };
    public static Func<PlayMakerFSM, bool>[][] bossesPatchedFsm = new Func<PlayMakerFSM, bool>[][]
    {
        new[] {PatchFsm_MossMother},
        new[] {PatchFsm_MossMother},
        new[] {PatchFsm_BellBeast},
        new[] {PatchFsm_FourthChorus},

    };
    public static Dictionary<BossName, string[][]> bossObjectsPath = new Dictionary<BossName, string[][]>
    {
        {BossName.MossMother, new string[][] { new string[] {"Black Thread States"} } } //Battle Scene "Normal World"
    };

    private static bool PatchFsm_MossMother(PlayMakerFSM PMfsm)
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
    private static bool PatchFsm_BellBeast(PlayMakerFSM PMfsm)
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
    private static bool PatchFsm_FourthChorus(PlayMakerFSM PMfsm)
    {
        var fsm = PMfsm.Fsm;

        var init = fsm.GetState("Init");
        var meetRoar1 = fsm.GetState("Meet Roar 1");
        var remeetRoar = fsm.GetState("Remeet Roar");
        var roarClamp = fsm.GetState("Roar Clamp");
        var roarNoClamp = fsm.GetState("Roar No Clamp");
        var meet = fsm.GetState("Meet?");

        ((Wait)(roarNoClamp.Actions[11])).time = 0.01f;
        ((Wait)(roarClamp.Actions[11])).time = 0.01f;

        init.Transitions[0].ToState = roarNoClamp.Name;
        init.Transitions[0].ToFsmState = roarNoClamp;

        // remeetRoar.Transitions[0].ToState = roarNoClamp.Name;
        // remeetRoar.Transitions[0].ToFsmState = roarNoClamp;

        return true;
    }
}