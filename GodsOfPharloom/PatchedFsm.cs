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
using GenericVariableExtension;
using InControl.NativeDeviceProfiles;
using System.Collections;

namespace Gods_Of_Pharloom;

public class PatchedFsm
{
    public static Func<object, MethodInfo, object[], object> InvokeMethod = (instance, method, obj) => method.Invoke(instance, obj);
    public static MethodInfo activateChildOnTrigger = AccessTools.Method(typeof(ActivateChildrenOnContact), "OnTriggerEnter2D");
    public class CustomLogicFsm : FsmStateAction
    {
        public Action<Fsm> action;
        public Action updateAction;
        public float time;
        public bool finishOnEnter;
        public Fsm fsm;
        public override void OnEnter()
        {
            if(time == 0f) action?.Invoke(fsm);
            else Gods_Of_Pharloom.BossSequence.sequenceController.StartCoroutine(DoActionWithDelay());
            if(updateAction == null && time == 0f) Finish();
            if(finishOnEnter) Finish();
        }
        public override void OnUpdate()
        {
            updateAction?.Invoke();
        }
        public IEnumerator DoActionWithDelay()
        {
            yield return new WaitForSeconds(time);
            action?.Invoke(fsm);
            Finish();
        }
        public CustomLogicFsm(Fsm fsm, float time = 0f, bool finishOnEnter = false)
        {
            this.fsm = fsm;
            this.time = time;
            this.finishOnEnter = finishOnEnter;
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
    public static string bossDeadEvent = "BOSS DEAD EVENT MOD";
    public string sceneName;
    public FsmPatch[] fsms;

    PatchedFsm(string sceneName, FsmPatch[] fsms)
    {
        this.sceneName = sceneName;
        this.fsms = fsms;
    }
    public static PatchedFsm[] patchedFsms = new PatchedFsm[]{
        new PatchedFsm("Menu_Title", new FsmPatch[]
        {
            new FsmPatch("Hero_Hornet(Clone)", "Superjump", PatchFsm_SuperJump),
            // new FsmPatch("Start Blanker", "Blanker Control", PatchFsm_ForInitModPreloads),
        }),
        new PatchedFsm(BossStatueInfo.hog_sceneName, new FsmPatch[]
        {
            new FsmPatch("Detect Range", "Detect Hero", PatchFsm_DetectRangeBenchControl),
            new FsmPatch("RestBench", "Trap Bench", PatchFsm_TrapBenchDestroy),
            new FsmPatch("thread_memory", "Deep Memory Pre Enter Effect", PatchFsm_ThreadMemoryPreEnterEffect),
            new FsmPatch("thread_memory", "FSM", PatchFsm_ThreadMemoryFSM),
        }),
        new PatchedFsm("Abyss_05", new FsmPatch[]
        {
            new FsmPatch("thread_memory", "Deep Memory Pre Enter Effect", PatchFsm_ThreadMemoryPreEnterEffect),
            new FsmPatch("thread_memory", "FSM", PatchFsm_ThreadMemoryFSM),
        }),

        new PatchedFsm("Tut_03", new FsmPatch[]
        {
            new FsmPatch("Mossbone Mother", "Control", PatchFsm_MossMother),
            new FsmPatch("Moss Vine Cluster", "Control", PatchFsm_MossMotherMossVineCluster),
            new FsmPatch("Moss Vine Cluster (1)", "Control", PatchFsm_MossMotherMossVineCluster),
            new FsmPatch("Mossbone Mother Corpse(Clone)", "Death", PatchFsm_MossMotherCorpseControl),
        }),
        new PatchedFsm("Weave_03", new FsmPatch[]
        {
            new FsmPatch("Mossbone Mother A", "Control", PatchFsm_MossMotherDoubleA),
            new FsmPatch("Mossbone Mother B", "Control", PatchFsm_MossMotherDoubleB),
            new FsmPatch("Moss Vine Cluster (2)", "Control", PatchFsm_MossMotherMossVineCluster),
            new FsmPatch("Mossbone Mother Ambient Corpse(Clone)", "Death", PatchFsm_MossMotherDoubleCorpseControl),
            new FsmPatch("Mossbone Mother B Ambient Corpse(Clone)", "Death", PatchFsm_MossMotherDoubleCorpseControl),
        }),
        new PatchedFsm("Bone_05_Boss", new FsmPatch[]
        {
            new FsmPatch("Bone Beast", "Control", PatchFsm_BellBeast),
            new FsmPatch("Boss Scene", "Return State", PatchFsm_BellBeastReturnState),
            new FsmPatch("Return Battle", "Start Return Battle", PatchFsm_BellBeastStartReturnBattle),
            new FsmPatch("Bone Beast Corpse(Clone)", "Death", PatchFsm_BellBeastCorpseControl),
        }),
        new PatchedFsm("Bone_East_08_Boss_Golem", new FsmPatch[]
        {
            new FsmPatch("song_golem", "Control", PatchFsm_FourthChorus)
        }),
        new PatchedFsm("Bone_East_08", new FsmPatch[]
        {
            new FsmPatch("Boss Scene", "Control", PatchFsm_BossScene)
        }),
        new PatchedFsm("Coral_11", new FsmPatch[]
        {
            new FsmPatch("Driller A", "Control", PatchFsm_GreatConchfliesDriller),
            new FsmPatch("Driller B", "Control", PatchFsm_GreatConchfliesDriller),
            new FsmPatch("Boss Scene", "Control", PatchFsm_GreatConchfliesBattleScene),
            new FsmPatch("Corpse Coral Conch Driller Giant(Clone)", "Death", PatchFsm_GreatConchfliesCorpseControl)
        }),
        new PatchedFsm("Bone_East_12", new FsmPatch[]
        {
            new FsmPatch("Lace Boss1", "Control", PatchFsm_Lace1),
            new FsmPatch("Corpse Lace1(Clone)", "Control", PatchFsm_Lace1CorpseControl),
        }),
        new PatchedFsm("Coral_Judge_Arena", new FsmPatch[]
        {
            new FsmPatch("Last Judge", "Control", PatchFsm_LastJudge),
            new FsmPatch("Boss Scene", "Control", PatchFsm_LastJudgeBattleScene),
            new FsmPatch("Corpse Last Judge(Clone)", "Control", PatchFsm_LastJudgeCorpseControl)
        }),
        new PatchedFsm("Greymoor_08_boss", new FsmPatch[]
        {
            new FsmPatch("Vampire Gnat", "Control", PatchFsm_Moorwing),
            new FsmPatch("Tension Range", "Control", PatchFsm_MoorwingTensionAudio),
            new FsmPatch("Vampire Gnat Corpse(Clone)", "Death", PatchFsm_MoorwingCorpseControl),
        }),
        new PatchedFsm("Organ_01", new FsmPatch[]
        {
            new FsmPatch("Phantom", "Control", PatchFsm_Phantom),
            new FsmPatch("Boss Scene", "Control", PatchFsm_PhantomBossScene)
        }),
        new PatchedFsm("Ant_19", new FsmPatch[]
        {
            new FsmPatch("Bone Flyer Giant", "Control", PatchFsm_SavageBeastfly1),
            new FsmPatch("Boss Scene", "Control", PatchFsm_SavageBeastfly1BossScene),
            new FsmPatch("Corpse Giant Bone Flyer(Clone)", "Death", PatchFsm_SavageBeastfly1CorpseControl),
        }),
        new PatchedFsm("Shellwood_18", new FsmPatch[]
        {
            new FsmPatch("Splinter Queen", "Control", PatchFsm_SisterSplinter),
            new FsmPatch("Boss Scene", "Battle Control", PatchFsm_SisterSplinterBossScene),
            new FsmPatch("Approaches", "Control", PatchFsm_SisterSplinterApproaches),
            new FsmPatch("Boss Return Scene", "Bud Control", PatchFsm_SisterSplinterBossReturnScene),
            new FsmPatch("Corpse Splinter Queen(Clone)", "Death", PatchFsm_SisterSplinterCorpseControl),
        }),
        new PatchedFsm("Bone_15", new FsmPatch[]
        {
            new FsmPatch("Skull King", "Behaviour", PatchFsm_SkullTyrant),
            new FsmPatch("Corpse Skull King SkullFragment(Clone)", "Death", PatchFsm_SkullTyrantCorpseControl),
            new FsmPatch("Audio Loop Tension", "FSM", PatchFsm_SkullTyrantAudioTension),
        }),
        new PatchedFsm("Belltown_Shrine", new FsmPatch[]
        {
            new FsmPatch("Spinner Boss", "Control", PatchFsm_Widow),
            new FsmPatch("Boss Scene", "Control", PatchFsm_WidowBossScene),
            new FsmPatch("Bell Shrine Lever", "Activate Delayed", PatchFsm_WidowLever),
        }),
        new PatchedFsm("Slab_16b", new FsmPatch[]
        {
            new FsmPatch("Slab Fly Broodmother", "Control", PatchFsm_Broodmother),
            new FsmPatch("Corpse Slab Fly Broodmaster(Clone)", "Death", PatchFsm_BroodmotherCorpseControl),
            new FsmPatch("Battle Gate Slab (2)", "BG Control", PatchFsm_BroodmotherBGControl),
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
            new FsmPatch("Corpse Roachkeeper Chef(Clone)", "Death", PatchFsm_DustChefCorpseControl),
            new FsmPatch("Kitchen Pipe Gong", "Gong Hit Reaction", PatchFsm_DustChefGongHitReaction),
        }),
        new PatchedFsm("Belltown_08", new FsmPatch[]
        {
            new FsmPatch("Wisp Pyre Effigy", "Summon Control", PatchFsm_FatherOfFlame),
            new FsmPatch("Battle Gate Swamp", "BG Control", PatchFsm_FatherOfFlameGateControl),
        }),
        new PatchedFsm("Slab_10b", new FsmPatch[]
        {
            new FsmPatch("First Weaver", "Control", PatchFsm_FirstSinner),
            new FsmPatch("Shrine First Weaver", "Inspection", PatchFsm_FirstSinnerInspection),
            new FsmPatch("Boss Scene", "Outro", PatchFsm_FirstSinnerBossSceneOutro),
            new FsmPatch("Corpse First Weaver(Clone)", "Death", PatchFsm_FirstSinnerCorpseControl),
        }),
        new PatchedFsm("Dock_09", new FsmPatch[]
        {
            new FsmPatch("Boss Scene", "Control", PatchFsm_ForebrothersSignisAndGronBossScene),
            new FsmPatch("Dock Guard Slasher", "Control", PatchFsm_ForebrothersSignisAndGronSlasher),
            new FsmPatch("Dock Guard Thrower", "Control", PatchFsm_ForebrothersSignisAndGronThrower),
        }),
        new PatchedFsm("Library_09", new FsmPatch[]
        {
            new FsmPatch("Garmond Scene", "Control", PatchFsm_GarmondAndZazaSceneControl),
            new FsmPatch("Garmond Fighter", "Control", PatchFsm_GarmondAndZaza),
            new FsmPatch("Citadel Library NPC", "Dialogue", PatchFsm_GarmondAndZazaDestroyNPCComponent),
        }),
        new PatchedFsm("Cradle_03", new FsmPatch[]
        {
            new FsmPatch("Silk Boss", "Control", PatchFsm_SilkBoss),
            new FsmPatch("Intro Sequence", "First Challenge", PatchFsm_SilkBossIntroSequence),
            new FsmPatch("Boss Title", "Title Control", PatchFsm_SilkBossTitleControl),
            new FsmPatch("Silk Boss", "Phase Control", PatchFsm_SilkBossPhaseControl),
            new FsmPatch("Challenge Region", "Challenge", PatchFsm_SilkBossChallengeControl),
            new FsmPatch("Death Sequence", "Control", PatchFsm_SilkBossDeathSequence),
        }),
        new PatchedFsm("Shadow_18", new FsmPatch[]
        {
            new FsmPatch("Swamp Shaman", "Control", PatchFsm_GroalTheGreat),
            new FsmPatch("Battle Gate Swamp", "BG Control", PatchFsm_GroalTheGreatCloseGate),
            new FsmPatch("Battle Gate Swamp (1)", "BG Control", PatchFsm_GroalTheGreatCloseGate),
            new FsmPatch("Battle Gate Swamp (2)", "BG Control", PatchFsm_GroalTheGreatCloseGate),
            new FsmPatch("Battle Gate Swamp (3)", "BG Control", PatchFsm_GroalTheGreatCloseGate),
        }),
        new PatchedFsm("Song_Tower_01", new FsmPatch[]
        {
            new FsmPatch("door_cutsceneEndLaceTower", "Travel Control", PatchFsm_Lace2door_cutsceneEndLaceTower),
            new FsmPatch("Lace Boss2 New", "Control", PatchFsm_Lace2BossControl),
            new FsmPatch("Corpse Lace2(Clone)", "Control", PatchFsm_Lace2CorpseControl),
            new FsmPatch("Lace Return Corpse", "Position", PatchFsm_Lace2ReturnCorpseDeactivate),
            new FsmPatch("song_tower_right_gate", "Control", PatchFsm_Lace2RightGate),
        }),
        new PatchedFsm("Coral_27", new FsmPatch[]
        {
            new FsmPatch("Coral Conch Driller Giant Solo", "Control", PatchFsm_RagingConchfly),
            new FsmPatch("Corpse Coral Conch Driller Giant Solo(Clone)", "Death", PatchFsm_RagingConchflyCorpseControl),
        }),
        new PatchedFsm("Bone_East_08_Boss_Beastfly", new FsmPatch[]
        {
            new FsmPatch("Bone Flyer Giant", "Control", PatchFsm_SavageBeastfly2),
            new FsmPatch("Corpse Giant Bone Flyer Quest(Clone)", "Death", PatchFsm_SavageBeastfly2CorpseControl),
        }),
        new PatchedFsm("Hang_17b", new FsmPatch[]
        {
            new FsmPatch("Song Knight", "Control", PatchFsm_SecondSentielControl),
            new FsmPatch("Boss Scene - To Additive Load", "Control", PatchFsm_SecondSentielBossSceneControl),
            new FsmPatch("Corpse Song Knight(Clone)", "Death", PatchFsm_SecondSentielCorpseControl),
        }),
        new PatchedFsm("Greymoor_08_Mapper", new FsmPatch[]
        {
            new FsmPatch("Mapper Spar NPC", "Attack Enemies", PatchFsm_ShakraAttackEnemies),
            new FsmPatch("Mapper Call Pole", "Control", PatchFsm_ShakraCallPole),
        }),
        new PatchedFsm("Ward_02", new FsmPatch[]
        {
            new FsmPatch("Pipe_Vent_Hatch", "Open At Battle End", PatchFsm_TheUnravelledPipeControl),
        }),
        new PatchedFsm("Ward_02_Boss", new FsmPatch[]
        {
            new FsmPatch("Boss Scene", "Control", PatchFsm_TheUnravelledBossScene),
            new FsmPatch("Conductor Boss", "Control", PatchFsm_TheUnravelledControl),
        }),
        new PatchedFsm("Library_13", new FsmPatch[]
        {
            new FsmPatch("Trobbio", "Control", PatchFsm_TrobbioControl),///////////////////////////
            new FsmPatch("Tormented Trobbio", "Control", PatchFsm_TormentedTrobbioControl),
            new FsmPatch("Corpse Tormented Trobbio(Clone)", "Control", PatchFsm_TormentedTrobbioCorpseControl),
            new FsmPatch("Grand Stage Scene", "Control", PatchFsm_TrobbioGrandStageSceneControl),
        }),
        new PatchedFsm("Coral_29", new FsmPatch[]
        {
            new FsmPatch("Zap Core Enemy", "Control", PatchFsm_VoltvyrmControl),
        }),
        new PatchedFsm("Bellway_Centipede_Arena", new FsmPatch[]
        {
            new FsmPatch("Centipede Control", "Control", PatchFsm_BellEaterControl),
        }),
        new PatchedFsm("Clover_10", new FsmPatch[]
        {
            new FsmPatch("Dancer A", "Control", PatchFsm_CloverDancersDancerAB),
            new FsmPatch("Dancer B", "Control", PatchFsm_CloverDancersDancerAB),
            new FsmPatch("Green Prince Boss NPC", "Dialogue", PatchFsm_CloverDancersGreenPrinceBossNPC),
            new FsmPatch("Dancer Control", "Control", PatchFsm_CloverDancersDancerControl),
            new FsmPatch("Corpse Green Prince(Clone)", "Death", PatchFsm_CloverDancersCorpseControl),
        }),
        new PatchedFsm("Room_CrowCourt_02", new FsmPatch[]
        {
            new FsmPatch("Crawfather", "Control", PatchFsm_CrawfatherControl),
            new FsmPatch("Battle Start", "Battle Start", PatchFsm_CrawfatherBattleStart),
            new FsmPatch("Corpse Crawfather(Clone)", "Death", PatchFsm_CrawfatherCorpseControl),
        }),
        new PatchedFsm("Memory_Coral_Tower", new FsmPatch[]
        {
            new FsmPatch("Coral King", "Control", PatchFsm_CrustKingKhanControl),
            new FsmPatch("Boss Scene", "Control", PatchFsm_CrustKingKhanBossSceneControl),
        }),
        new PatchedFsm("Bone_East_18b", new FsmPatch[]
        {
            new FsmPatch("Bone Hunter Trapper", "Control", PatchFsm_GurrTheOutcastControl),
            new FsmPatch("TrapBench", "Control", PatchFsm_GurrTheOutcastTrapBenchControl),
            new FsmPatch("Boss Scene", "Control", PatchFsm_GurrTheOutcastBossSceneControl),
            new FsmPatch("Corpse Bone Hunter Trapper(Clone)", "Death", PatchFsm_GurrTheOutcastCorpseControl),
        }),
        new PatchedFsm("Coral_33", new FsmPatch[]
        {
            new FsmPatch("Garmond Black Threaded Fighter", "Control", PatchFsm_LostGarmondControl),
            new FsmPatch("Corpse Garmond BlackThreaded(Clone)", "Control", PatchFsm_LostGarmondCorpseControl),
        }),
        new PatchedFsm("Abyss_Cocoon", new FsmPatch[]
        {
            new FsmPatch("Intro Control", "Control", PatchFsm_LostLaceIntroControl),
            new FsmPatch("Boss Title", "Title Control", PatchFsm_LostLaceBossTitle),
            new FsmPatch("Abyss_Cocoon_Silk", "Animate during lace death", PatchFsm_LostLaceGrandMother),
            new FsmPatch("Lost Lace Boss", "Control", PatchFsm_LostLaceBossControl),
            new FsmPatch("Lost Lace Boss", "Death Control", PatchFsm_LostLaceDeathControl),
            new FsmPatch("door_entry", "Control", PatchFsm_LostLaceDoorEntryControl),
            new FsmPatch("Corpse Lost Lace(Clone)", "Control", PatchFsm_LostLaceCorpseControl),
        }),
        new PatchedFsm("Shellwood_11b_Memory", new FsmPatch[]
        {
            new FsmPatch("Boss Scene", "Control", PatchFsm_NylethBossSceneControl),
            new FsmPatch("Corpse Flower Queen(Clone)", "Death", PatchFsm_NylethCorpseControl),
        }),
        new PatchedFsm("Clover_19", new FsmPatch[]
        {
            new FsmPatch("Cloverstag White Boss", "Control", PatchFsm_PalestagControl),
            new FsmPatch("Corpse White Cloverstag(Clone)", "Disappear", PatchFsm_PalestagCorpseControl),
        }),
        new PatchedFsm("Peak_07", new FsmPatch[]
        {
            new FsmPatch("Pinstress Boss", "Control", PatchFsm_PinstressBossControl),
            new FsmPatch("Pinstress Control", "Control", PatchFsm_PinstressControl),
            new FsmPatch("NPC", "NPC Control", PatchFsm_PinstressNPCControl),
        }),
        new PatchedFsm("Crawl_10", new FsmPatch[]
        {
            new FsmPatch("Blue Assistant", "Control", PatchFsm_PlasmifiedZango),
        }),
        new PatchedFsm("Shellwood_22", new FsmPatch[]
        {
            new FsmPatch("Seth", "Control", PatchFsm_SethControl),
            new FsmPatch("Corpse Seth(Clone)", "Death", PatchFsm_SethCorpseControl),
        }),
        new PatchedFsm("Memory_Ant_Queen", new FsmPatch[]
        {
            new FsmPatch("Hunter Queen Boss", "Control", PatchFsm_SkarrsingerKarmelitaBossControl),
            new FsmPatch("Challenge Region", "Challenge", PatchFsm_SkarrsingerKarmelitaChallengeRegion),
            new FsmPatch("Corpse Hunter Queen(Clone)", "Death", PatchFsm_SkarrsingerKarmelitaCorpseControl),
        }),
        new PatchedFsm("Coral_39", new FsmPatch[]
        {
            new FsmPatch("Coral Warrior Grey", "Control", PatchFsm_WatcherAtTheEdgeControl),
            new FsmPatch("Coral Warrior Grey", "Battle Music", PatchFsm_WatcherAtTheEdgeBattleMusic),
            new FsmPatch("Corpse Coral Warrior Grey(Clone)", "Control", PatchFsm_WatcherAtTheEdgeCorpseControl),
        }),

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
        "Ward_02_Boss", //TheUnravelled
        "Library_13", //Trobbio
        "Coral_29", //Voltvyrm
        "Bellway_Centipede_Arena", //BellEater
        "Clover_10", //CloverDancers
        "Room_CrowCourt_02", //Crawfather
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

    public static bool PatchFsm_SuperJump(Fsm fsm)
    {
        var startDelay = fsm.GetState("Start Delay");
        var throwNeedle = fsm.GetState("Throw Needle");
        var throwWait = fsm.GetState("Throw Wait");
        var groundCharge = fsm.GetState("Ground Charge");

        float origSuperJumpSpeed = fsm.GetFsmFloat("Jump Speed").Value;
        float origWaitSuperJump = fsm.GetFsmFloat("Charge Time").Value;
        var origThrowNeedleWait = throwNeedle.Actions[9];

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.SendEvent("FINISHED");
        };

        throwNeedle.Actions = InsertInArray(throwNeedle.Actions, customActionSendEvent, throwNeedle.Actions.Length);
        throwWait.Actions = InsertInArray(throwWait.Actions, customActionSendEvent, throwWait.Actions.Length);
        startDelay.Actions = InsertInArray(startDelay.Actions, customActionSendEvent, startDelay.Actions.Length);

        var sendEventAction = throwNeedle.Actions[throwNeedle.Actions.Length - 1];

        var customActionSetCustomValues = new CustomLogicFsm(fsm);
        customActionSetCustomValues.action += (Fsm fsm) =>
        {
            string activeScene = SceneManager.GetActiveScene().name;
            var scene = GodsOfPharloomMod.customScenes.Find(item => item.sceneName == activeScene);

            if(scene != null && scene.isFastSuperJump)
            {
                fsm.GetFsmFloat("Jump Speed").Value = CustomScene.customSuperJumpSpeed;
                fsm.GetFsmFloat("Charge Time").Value = CustomScene.customWaitForSuperJump;

                origThrowNeedleWait.Enabled = false;
                sendEventAction.Enabled = true;
            }
            else
            {
                fsm.GetFsmFloat("Jump Speed").Value = origSuperJumpSpeed;
                fsm.GetFsmFloat("Charge Time").Value = origWaitSuperJump;

                origThrowNeedleWait.Enabled = true;
                sendEventAction.Enabled = false;
            }
        };

        startDelay.Actions = InsertInArray(startDelay.Actions, customActionSetCustomValues, 0);

        groundCharge.Actions = InsertInArray(groundCharge.Actions, groundCharge.Actions[19], groundCharge.Actions.Length);
        groundCharge.Actions = RemoveFromArray(groundCharge.Actions, 19);

        return true;
    }
    public static bool PatchFsm_ForInitModPreloads(Fsm fsm)
    {
        if(Preload.isInitialized) return false;

        void afterPreloaded()
        {
            var scene = SceneManager.GetSceneByName("Menu_Title");
            foreach(var go in scene.GetRootGameObjects())
            {
                if(go.name == "_SceneManager")
                {
                    go.GetComponent<CustomSceneManager>().UpdateScene();
                    Preload.afterAllPreloaded -= afterPreloaded;
                    return;
                }
            }
        }

        Preload.afterAllPreloaded += afterPreloaded;
        Preload.Init();

        return true;
    }
    public static bool PatchFsm_DetectRangeBenchControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var idle = fsm.GetState("Idle");
        var close = fsm.GetState("Close");

        var color = new FsmColor();
        color.Value = new UnityEngine.Color(0, 0, 0, 0);

        ((SetMaterialColor)idle.Actions[6]).color = color;

        idle.Actions = RemoveFromArray(idle.Actions, 5);

        close.Actions = RemoveFromArray(close.Actions, 6);
        close.Actions = RemoveFromArray(close.Actions, 5);
        close.Actions = RemoveFromArray(close.Actions, 3);
        close.Actions = RemoveFromArray(close.Actions, 2);

        return true;
    }
    public static bool PatchFsm_TrapBenchDestroy(Fsm fsm)
    {
        GameObject.Destroy(fsm.FsmComponent);

        return true;
    }
    public static bool PatchFsm_ThreadMemoryFSM(Fsm fsm)
    {
        var collapse = fsm.GetState("Collapse");

        var wait = new Wait
        {
            time = 0.5f,
            finishEvent = FsmEvent.GetFsmEvent("FINISHED")
        };

        collapse.Actions = PatchedFsm.InsertInArray(collapse.Actions, wait, collapse.Actions.Length-1);

        return true;
    }
    public static bool PatchFsm_ThreadMemoryPreEnterEffect(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var idle = fsm.GetState("Idle");
        var inZone = fsm.GetState("In Zone");
        var preEnterEffect = fsm.GetState("Pre Enter Effect");

        ((CreateObject)init.Actions[2]).gameObject = Preload.preloads["Deep Memory Pre Enter Effect"];

        GameObject effectObj = null;

        ParticleSystem roarEmitter = null;
        ParticleSystem burst2Particles = null;
        ParticleSystem burst3Particles = null;
        ParticleSystem burst4Particles = null;
        ParticleSystem burst5Particles = null;

        int origBurst2MaxParticles = 0;
        int origBurst4MaxParticles = 0;

        float timer = 0f;

        var customActionStopRoarEmitter = new CustomLogicFsm(fsm);
        customActionStopRoarEmitter.action += (Fsm fsm) =>
        {
            roarEmitter.Stop();
        };
        var customActionStartRoarEmitter = new CustomLogicFsm(fsm);
        customActionStartRoarEmitter.action += (Fsm fsm) =>
        {
            roarEmitter.Play();
        };
        var customActionReset = new CustomLogicFsm(fsm);
        customActionReset.action += (Fsm fsm) =>
        {
            timer = 0f;

            var mainBurst2 = burst2Particles.main;
            var mainBurst4 = burst4Particles.main;
            mainBurst2.maxParticles = origBurst2MaxParticles;
            mainBurst4.maxParticles = origBurst4MaxParticles;
        };

        IEnumerator enumerator()
        {
            effectObj = fsm.GetFsmGameObject("Deep Memory Pre Enter Effect").Value;
            var pos = effectObj.transform.position;
            effectObj.transform.position = new Vector3(10000, 10000, pos.z);
            roarEmitter = Preload.FindObjectByPath(new GameObject[]{effectObj}, $"{effectObj.name}/Roar Wave Emitter (2)").GetComponent<ParticleSystem>();
            burst2Particles = Preload.FindObjectByPath(new GameObject[]{effectObj}, $"{effectObj.name}/Burst (2)").GetComponent<ParticleSystem>();
            burst3Particles = Preload.FindObjectByPath(new GameObject[]{effectObj}, $"{effectObj.name}/Burst (3)").GetComponent<ParticleSystem>();
            burst4Particles = Preload.FindObjectByPath(new GameObject[]{effectObj}, $"{effectObj.name}/Burst (4)").GetComponent<ParticleSystem>();
            burst5Particles = Preload.FindObjectByPath(new GameObject[]{effectObj}, $"{effectObj.name}/Burst (5)").GetComponent<ParticleSystem>();

            var mainBurst2 = burst2Particles.main;
            var mainBurst4 = burst4Particles.main;

            origBurst2MaxParticles = mainBurst2.maxParticles;
            origBurst4MaxParticles = mainBurst4.maxParticles;

            while (true)
            {
                if(timer > 0f){
                    burst2Particles.Emit(1000);
                    if(timer > 3f){
                        if(mainBurst2.maxParticles < 500) mainBurst2.maxParticles += 5;
                    }
                }
                if(timer > 1.5f) burst3Particles.Emit(1000);
                if(timer > 1.5f){
                    burst4Particles.Emit(1000);
                    if(timer > 5f){
                        mainBurst4.maxParticles += 5;
                    }
                }
                if(timer > 3.5f) burst5Particles.Emit(1000);

                if(fsm.ActiveState == preEnterEffect) timer += Time.deltaTime;
                
                yield return null;
            }
        }
        var customActionStartEnumerator = new CustomLogicFsm(fsm);
        customActionStartEnumerator.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.StartCoroutine(enumerator());
        };

        init.Actions = InsertInArray(init.Actions, customActionStartEnumerator, init.Actions.Length);
        inZone.Actions = InsertInArray(inZone.Actions, customActionStopRoarEmitter, 0);
        inZone.Actions = InsertInArray(inZone.Actions, customActionReset, 0);
        preEnterEffect.Actions = InsertInArray(preEnterEffect.Actions, customActionStartRoarEmitter, 0);

        return true;
    }

    public static bool PatchFsm_MossMother(Fsm fsm)
    {

        var init = fsm.GetState("Init");
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

        var customActionCreateTriggerForStart = new CustomLogicFsm(fsm);
        customActionCreateTriggerForStart.action += (Fsm fsm) =>
        {
            var pos = fsm.GameObject.transform.position;

            var bossSceneChild = ((GetParent)init.Actions[4]).storeResult.Value;
            var bossScene = bossSceneChild.transform.parent;
            var battleSceneComponent = bossScene.GetComponent<BattleScene>();
            battleSceneComponent.battleStartPause = 0;

            var customTrigger = CreateTrigger("Tut_03");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(pos.x, 13f, pos.z);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                battleSceneComponent.StartBattle();
            };
        };

        
        var initToDormant = init.Transitions.FirstOrDefault(i => {
            return i.ToState == dormantState.Name;
        });
        initToDormant.ToState = returnReadyState.Name;
        initToDormant.ToFsmState = returnReadyState;

        returnReadyState.Actions = InsertInArray(returnReadyState.Actions, customActionCreateTriggerForStart, returnReadyState.Actions.Length);

        return true;
    }
    public static bool PatchFsm_MossMotherDoubleA(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var dormantState = fsm.GetState("Dormant");
        var returnReadyState = fsm.GetState("Return Ready");
        var returnReady2 = fsm.GetState("Return Ready 2");
        var returnPause2 = fsm.GetState("Return Pause 2");
        var returnAntic2 = fsm.GetState("Return Antic 2");
        var returnIn2 = fsm.GetState("Return In 2");
        var returnAntic = fsm.GetState("Return Antic");
        var roarState = fsm.GetState("Roar");
        var returnIn = fsm.GetState("Return In");
        var returnPause = fsm.GetState("Return Pause");
        
        ((Wait)(roarState.Actions[6])).time = 0.01f;
        ((Wait)(returnAntic2.Actions[0])).time = 0;
        ((Wait)(returnPause2.Actions[0])).time = 0;

        var customActionInitDisableCocoon = new CustomLogicFsm(fsm);
        customActionInitDisableCocoon.action = (Fsm fsm) =>
        {
            var cocoon = ((FindChild)init.Actions[8]).storeResult.Value;
            cocoon.SetActive(false);
        };

        var customActionSetPosition = new CustomLogicFsm(fsm);
        customActionSetPosition.action = (Fsm fsm) =>
        {
            var posPlanned = ((SetPosition2D)returnIn2.Actions[3]).Vector.Value;
            var posGO = fsm.GameObject.transform.position;
            fsm.GameObject.transform.position = new Vector3(21.87f, posPlanned.y, posGO.z);

            var scale = fsm.GameObject.transform.localScale;
            fsm.GameObject.transform.localScale = new Vector3(-1, scale.y, scale.z);
        };

        var customActionCreateTriggerForStart = new CustomLogicFsm(fsm);
        customActionCreateTriggerForStart.action += (Fsm fsm) =>
        {
            var pos = fsm.GameObject.transform.position;

            var bossSceneChild = ((GetParent)init.Actions[4]).storeResult.Value;
            var bossScene = bossSceneChild.transform.parent;
            var battleSceneComponent = bossScene.GetComponent<BattleScene>();
            battleSceneComponent.battleStartPause = 0;

            var customTrigger = CreateTrigger("Weave_03");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(pos.x, 20f, pos.z);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                battleSceneComponent.StartBattle();
            };
        };

        init.Actions = InsertInArray(init.Actions, customActionInitDisableCocoon, init.Actions.Length - 1);
        returnReady2.Actions = InsertInArray(returnReady2.Actions, customActionCreateTriggerForStart, returnReady2.Actions.Length);
        returnIn2.Actions = InsertInArray(returnIn2.Actions, customActionSetPosition, 4);

        
        SetTransitionToState(init, returnReady2, 0);

        return true;
    }
    public static bool PatchFsm_MossMotherDoubleB(Fsm fsm)
    {
        var init = fsm.GetState("Init");

        var customActionInit = new CustomLogicFsm(fsm);
        customActionInit.action += (Fsm fsm) =>
        {
            var children = fsm.GameObject.transform;
            foreach(Transform child in children)
            {
                if(child.name == "Mossbone Mother Ambient Corpse(Clone)")
                {
                    child.name = "Mossbone Mother B Ambient Corpse(Clone)";
                    break;
                }
            }
        };

        init.Actions = InsertInArray(init.Actions, customActionInit, init.Actions.Length - 1);

        return true;
    }
    public static bool PatchFsm_MossMotherMossVineCluster(Fsm fsm)
    {
        fsm.GameObject.SetActive(false);

        return true;
    }
    public static bool PatchFsm_MossMotherCorpseControl(Fsm fsm)
    {
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var waitFrame = fsm.GetState("Wait Frame");
        var stagger = fsm.GetState("Stagger");

        ((Wait)steam.Actions[1]).time = 0.01f;

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };
        var waitAction = new Wait
        {
            time = BossInfo.waitForBossDeathAnim,
            finishEvent = FsmEvent.GetFsmEvent("FINISHED")
        };

        var customState2 = new FsmState(fsm);
        customState2.Actions = new FsmStateAction[]{customActionSendEvent};

        var customState1 = new FsmState(fsm);
        customState1.Actions = new FsmStateAction[]{waitAction};
        customState1.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent("FINISHED"),
                ToFsmState = customState2
            }
        };

        blow.Transitions[0].ToFsmState = customState1;

        SetTransitionToState(stagger, blow, 0);

        return true;
    }
    public static bool PatchFsm_MossMotherDoubleCorpseControl(Fsm fsm)
    {
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var waitFrame = fsm.GetState("Wait Frame");
        var stagger = fsm.GetState("Stagger");

        var children = GameObject.Find("Bosses").transform;
        var countOfActiveBosses = 0;
        foreach(Transform child in children)
        {
            if(child.gameObject.activeSelf) countOfActiveBosses++;
        }
        if(countOfActiveBosses > 1)
        {
            return false;
        }

        var customActionSendEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        blow.Actions = InsertInArray(blow.Actions, customActionSendEvent, 0);

        SetTransitionToState(stagger, blow, 0);

        return true;
    }
    public static bool PatchFsm_MossMotherBCorpseControl(Fsm fsm)
    {
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var state1 = fsm.GetState("State 1");
        var blackThread = fsm.GetState("Black Thread?");

        ((Wait)steam.Actions[1]).time = 0.01f;

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };
        var waitAction = new Wait
        {
            time = BossInfo.waitForBossDeathAnim,
            finishEvent = FsmEvent.GetFsmEvent("FINISHED")
        };

        var customState2 = new FsmState(fsm);
        customState2.Actions = new FsmStateAction[]{customActionSendEvent};

        var customState1 = new FsmState(fsm);
        customState1.Actions = new FsmStateAction[]{waitAction};
        customState1.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent("FINISHED"),
                ToFsmState = customState2
            }
        };

        blow.Transitions[0].ToFsmState = customState1;
        blow.Transitions[0].FsmEvent = FsmEvent.GetFsmEvent("FINISHED");

        SetTransitionToState(blackThread, blow, 1);

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
    public static bool PatchFsm_BellBeastReturnState(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var setReturnState = fsm.GetState("Set Return State");

        SetTransitionToState(init, setReturnState, 0);

        return true;
    }
    public static bool PatchFsm_BellBeastStartReturnBattle(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var sceneSetup = fsm.GetState("Scene Setup");
        var startBattle = fsm.GetState("Start Battle");

        ((Wait)startBattle.Actions[1]).time = 0.5f;

        sceneSetup.Actions = RemoveFromArray(sceneSetup.Actions, 4); //Wait


        return true;
    }
    public static bool PatchFsm_BellBeastCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var state1 = fsm.GetState("State 1");

        ((Wait)blow.Actions[1]).time = 0.1f;

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };
        var waitAction = new Wait
        {
            time = BossInfo.waitForBossDeathAnim,
            finishEvent = FsmEvent.GetFsmEvent("FINISHED")
        };

        var customState2 = new FsmState(fsm);
        customState2.Actions = new FsmStateAction[]{customActionSendEvent};

        var customState1 = new FsmState(fsm);
        customState1.Actions = new FsmStateAction[]{waitAction};
        customState1.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent("FINISHED"),
                ToFsmState = customState2
            }
        };

        blow.Transitions[0].ToFsmState = customState1;
        blow.Transitions[0].FsmEvent = FsmEvent.GetFsmEvent("FINISHED");

        SetTransitionToState(stagger, blow, 0);

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
        var deathAnim = fsm.GetState("Death Anim");
        var explode = fsm.GetState("Explode");
        var deathFall = fsm.GetState("Death Fall");
        var deathLand = fsm.GetState("Death Land");

        ((Wait)(roarNoClamp.Actions[11])).time = 0.1f;
        ((Wait)(roarClamp.Actions[11])).time = 0.1f;
        // ((Wait)deathFall.Actions[1]).time = 1.25f;

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        deathLand.Actions = InsertInArray(deathLand.Actions, customActionSendEvent, 0);

        SetTransitionToState(init, remeetRoar, 0);
        SetTransitionToState(remeetRoar, roarNoClamp, 0);
        SetTransitionToState(deathAnim, explode, 0);

        return true;
    }
    public static bool PatchFsm_BossScene(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var meetReady = fsm.GetState("Meet Ready");
        var remeet1 = fsm.GetState("Remeet 1");
        var remeet2 = fsm.GetState("Remeet 2");
        var remeetReady = fsm.GetState("Remeet Ready");
        var beastfly = fsm.GetState("Beastfly?");

        if(Gods_Of_Pharloom.BossSequence.currentBoss == BossInfo.bosses["Savage Beastfly in Far Fields"])
        {
            var customAction = new CustomLogicFsm(fsm);
            customAction.action += (Fsm fsm) =>
            {
                var go = fsm.GameObject;

                var children = go.transform;

                foreach(Transform child in children)
                {
                    if(child.name == "Lava Plats" ||
                    child.name == "Pre Activation Floor" ||
                    child.name == "Battle End Floor"
                    ) child.gameObject.SetActive(false);
                }
            };

            init.Actions = InsertInArray(init.Actions, customAction, 16);
        }

        ((Wait)(remeet1.Actions[7])).time = 0.01f;
        ((Wait)(remeet2.Actions[2])).time = 0f;

        var wait = new Wait
        {
            time = 0.5f,
            finishEvent = FsmEvent.GetFsmEvent("FINISHED")
        };

        var customState = new FsmState(fsm);
        customState.Actions = new FsmStateAction[]{wait};
        customState.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent("FINISHED"),
                ToFsmState = remeetReady
            }
        };

        var customActionCreateTriggerForStart = new CustomLogicFsm(fsm);
        customActionCreateTriggerForStart.action += (Fsm fsm) =>
        {
            var pos = fsm.GameObject.transform.position;

            var customTrigger = CreateTrigger("Bone_East_08");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(80f, 7f, pos.z);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                fsm.FsmComponent.SendEvent("ENTER");
            };
        };

        var customActionStartBattle = new CustomLogicFsm(fsm);
        customActionStartBattle.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.SendEvent("REMEET READY");
        };

        remeetReady.Actions = InsertInArray(remeetReady.Actions,
                              customActionCreateTriggerForStart, remeetReady.Actions.Length);
        
        init.Actions = InsertInArray(init.Actions, customActionStartBattle, init.Actions.Length);

        if(Gods_Of_Pharloom.BossSequence.currentBoss == BossInfo.bosses["Fourth Chorus"])
        {
            SetTransitionToState(init, customState, 0);
            SetTransitionToState(init, customState, 1);
            SetTransitionToState(init, customState, 2);
        }
        if(Gods_Of_Pharloom.BossSequence.currentBoss == BossInfo.bosses["Savage Beastfly in Far Fields"])
        {
            SetTransitionToState(init, beastfly, 0);
            SetTransitionToState(init, beastfly, 1);
            SetTransitionToState(init, beastfly, 2);
        }

        return true;
    }
    public static bool PatchFsm_GreatConchfliesBattleScene(Fsm fsm)
    {

        var arenaStart = fsm.GetState("Arena Start");
        var startPauseS = fsm.GetState("Start Pause S");
        var state = fsm.GetState("State");
        var restartReady = fsm.GetState("Restart Ready");


        ((Wait)(startPauseS.Actions[0])).time = 0f;

        SetTransitionToState(arenaStart, startPauseS, 1);

        SetTransitionToState(state, restartReady, 1);
        SetTransitionToState(state, restartReady, 2);
        SetTransitionToState(state, restartReady, 3);

        // restartReady.Transitions[0].ToState = startPauseS.Name;
        // restartReady.Transitions[0].ToFsmState = startPauseS;

        var customActionCreateTriggerForStart = new CustomLogicFsm(fsm);
        customActionCreateTriggerForStart.action += (Fsm fsm) =>
        {
            var pos = fsm.GameObject.transform.position;

            var customTrigger = CreateTrigger("Coral_11");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(52.6f, 14.5f, 0);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                fsm.FsmComponent.SendEvent("ENTER");
            };
        };

        restartReady.Actions = InsertInArray(restartReady.Actions, customActionCreateTriggerForStart, restartReady.Actions.Length - 1);

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
    public static bool PatchFsm_GreatConchfliesCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };
        var waitAction = new Wait
        {
            time = BossInfo.waitForBossDeathAnim,
            finishEvent = FsmEvent.GetFsmEvent("FINISHED")
        };

        var customState2 = new FsmState(fsm);
        customState2.Actions = new FsmStateAction[]{customActionSendEvent};

        var customState1 = new FsmState(fsm);
        customState1.Actions = new FsmStateAction[]{waitAction};
        customState1.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent("FINISHED"),
                ToFsmState = customState2
            }
        };

        land.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                ToFsmState = customState1,
                FsmEvent = FsmEvent.GetFsmEvent("FINISHED")
            }
        };

        SetTransitionToState(stagger, blow, 0);

        return true;
    }
    public static bool PatchFsm_Lace1(Fsm fsm)
    {
        var enctountred = fsm.GetState("Encountered?");
        var refight = fsm.GetState("Refight");
        var dormant = fsm.GetState("Dormant");

        var customActionCreateTriggerForStart = new CustomLogicFsm(fsm);
        customActionCreateTriggerForStart.action += (Fsm fsm) =>
        {
            var pos = fsm.GameObject.transform.position;

            var customTrigger = CreateTrigger("Bone_East_12");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(pos.x, pos.y, pos.z);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                fsm.FsmComponent.SendEvent("ENTER");
            };
        };

        SetTransitionToState(enctountred, refight, 0);

        dormant.Actions = InsertInArray(dormant.Actions, customActionCreateTriggerForStart, dormant.Actions.Length);

        return true;
    }
    public static bool PatchFsm_Lace1CorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");
        var jumpAntic = fsm.GetState("Jump Antic");

        ((Wait)land.Actions[3]).time = 1f;

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        SetTransitionToState(stagger, blow, 0);

        jumpAntic.Actions = InsertInArray(jumpAntic.Actions, customActionSendEvent, 0);

        return true;
    }
    public static bool PatchFsm_LastJudge(Fsm fsm)
    {
        var introRoar = fsm.GetState("Intro Roar");
        var introFallAnticQ = fsm.GetState("Intro Fall Antic Q");
        var firstIdle = fsm.GetState("First Idle");
        var idle = fsm.GetState("Idle");

        ((Wait)(introRoar.Actions[1])).time = 0.1f;
        ((Wait)(introFallAnticQ.Actions[4])).time = 0f;

        var customActionSendFinishEvent = new CustomLogicFsm(fsm);
        customActionSendFinishEvent.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.SendEvent("FINISHED");
        };

        firstIdle.Actions = InsertInArray(firstIdle.Actions, customActionSendFinishEvent, 4);

        SetTransitionToState(firstIdle, idle, 0);
        SetTransitionToState(firstIdle, idle, 1);

        return true;
    }
    public static bool PatchFsm_LastJudgeBattleScene(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var encountered = fsm.GetState("Encountered");

        SetTransitionToState(init, encountered, 0);
        SetTransitionToState(init, encountered, 2);
        SetTransitionToState(init, encountered, 3);

        return true;
    }
    public static bool PatchFsm_LastJudgeCorpseControl(Fsm fsm)
    {
        var steam1 = fsm.GetState("Steam 1");
        var steam2 = fsm.GetState("Steam 2");
        var explode = fsm.GetState("Explode");
        var finalBlow = fsm.GetState("Final Blow");
        var land = fsm.GetState("Land");
        var break1 = fsm.GetState("Break 1");
        var break2 = fsm.GetState("Break 2");
        var break3 = fsm.GetState("Break 3");

        ((Wait)steam1.Actions[3]).time = 0.01f;
        ((Wait)steam2.Actions[3]).time = 0.01f;

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };
        var waitAction = new Wait
        {
            time = BossInfo.waitForBossDeathAnim,
            finishEvent = FsmEvent.GetFsmEvent("FINISHED")
        };

        var customState2 = new FsmState(fsm);
        customState2.Actions = new FsmStateAction[]{customActionSendEvent};

        var customState1 = new FsmState(fsm);
        customState1.Actions = new FsmStateAction[]{waitAction};
        customState1.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent("FINISHED"),
                ToFsmState = customState2
            }
        };

        break3.Actions = InsertInArray(break3.Actions, customActionSendEvent, 0);

        SetTransitionToState(explode, finalBlow, 0);

        land.Actions = RemoveFromArray(land.Actions, 6);

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
    public static bool PatchFsm_MoorwingTensionAudio(Fsm fsm)
    {
        GameObject.Destroy(fsm.FsmComponent);

        return true;
    }
    public static bool PatchFsm_MoorwingCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");
        var landCheck = fsm.GetState("Land Check");
        var fall = fsm.GetState("Fall");

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };
        var waitAction = new Wait
        {
            time = BossInfo.waitForBossDeathAnim,
            finishEvent = FsmEvent.GetFsmEvent("FINISHED")
        };

        var customState2 = new FsmState(fsm);
        customState2.Actions = new FsmStateAction[]{customActionSendEvent};

        var customState1 = new FsmState(fsm);
        customState1.Actions = new FsmStateAction[]{waitAction};
        customState1.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent("FINISHED"),
                ToFsmState = customState2
            }
        };

        land.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                ToFsmState = customState1,
                FsmEvent = FsmEvent.GetFsmEvent("FINISHED")
            }
        };

        SetTransitionToState(stagger, blow, 0);
        SetTransitionToState(blow, fall, 0);
        SetTransitionToState(land, customState1, 0);

        return true;
    }
    public static bool PatchFsm_Phantom(Fsm fsm)
    {
        var hornetL = fsm.GetState("Hornet L");
        var hornetR = fsm.GetState("Hornet R");
        var parryReady = fsm.GetState("Parry Ready");
        var parryFacing = fsm.GetState("Parry Facing");
        var hornetFaceL = fsm.GetState("Hornet Face L");
        var hornetFaceR = fsm.GetState("Hornet Face R");
        var clashCutscene = fsm.GetState("Clash Cutscene");
        var timeFreeze = fsm.GetState("Time Freeze");
        var DCLand = fsm.GetState("DC Land");
        var crossSlashEnd = fsm.GetState("Cross Slash End");
        var bloodStream = fsm.GetState("Blood Stream");
        var deathSteam = fsm.GetState("Death Steam");
        var deathExplode = fsm.GetState("Death Explode");
        var fadeToBlack = fsm.GetState("Fade To Black");
        var endPause = fsm.GetState("End Pause");

        ((Wait)clashCutscene.Actions[14]).time = 0.1f;
        ((Wait)timeFreeze.Actions[4]).time = 0.1f;
        ((Wait)crossSlashEnd.Actions[4]).time = 0.1f;

        var bossDeathSound = ((AudioPlayRandomVoiceFromTableV2)deathSteam.Actions[1]);

        var customActionDoParry = new CustomLogicFsm(fsm);
        customActionDoParry.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.SendEvent("PARRY");
        };

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        parryReady.Actions = InsertInArray(parryReady.Actions, customActionDoParry, 0);
        bloodStream.Actions = InsertInArray(bloodStream.Actions, bossDeathSound, bloodStream.Actions.Length);
        fadeToBlack.Actions = new FsmStateAction[]{customActionSendEvent};

        DCLand.Actions = RemoveFromArray(DCLand.Actions, 2);
        deathSteam.Actions = RemoveFromArray(deathSteam.Actions, 4);

        SetTransitionToState(bloodStream, deathExplode, 0);

        fadeToBlack.Transitions = new FsmTransition[0];

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
        var organHit = fsm.GetState("Organ Hit");

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

            // if(HeroController.instance.transform.position.x > fogColumnBGGO.transform.position.x) return;

            fogDamagerGO.transform.position = new Vector3(fogDamagerGO.transform.position.x + xPos, fogDamagerGO.transform.position.y, fogDamagerGO.transform.position.z);
            fogColumnAnticGO.transform.position = new Vector3(fogColumnAnticGO.transform.position.x + xPos, fogColumnAnticGO.transform.position.y, fogColumnAnticGO.transform.position.z);
            fogColumnFGGO.transform.position = new Vector3(fogColumnFGGO.transform.position.x + xPos, fogColumnFGGO.transform.position.y, fogColumnFGGO.transform.position.z);
            phantomBossGO.transform.position = new Vector3(phantomBossGO.transform.position.x + xPos, phantomBossGO.transform.position.y, phantomBossGO.transform.position.z);
        };

        var customActionSkipAnimation = new CustomLogicFsm(fsm);
        customActionSkipAnimation.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.SendEvent("FINISHED");
        };

        var list = BGFog.Actions.ToList();
        list.Insert(list.Count - 1, customAction);
        BGFog.Actions = list.ToArray();

        init.Actions = RemoveFromArray(init.Actions, 9); //player data bool test

        // organHit.Actions = InsertInArray(organHit.Actions, customActionSkipAnimation, organHit.Actions.Length);

        return true;
    }
    public static bool PatchFsm_SavageBeastfly1(Fsm fsm)
    {
        var pos = fsm.GameObject.transform.position;
        fsm.GameObject.transform.position = new Vector3(61.76f, 39.5f, pos.z);


        var init = fsm.GetState("Init");
        var choice = fsm.GetState("Choice");
        var idlyFlyAudio = fsm.GetState("Idly Fly Audio?");
        var rematch = fsm.GetState("Rematch?");
        var introLook = fsm.GetState("Intro Look");
        var introRoar = fsm.GetState("Intro Roar");

        choice.Transitions = RemoveFromArray(choice.Transitions, 3);

        ((WaitRandom)(introLook.Actions[5])).timeMin = 0f;
        ((WaitRandom)(introLook.Actions[5])).timeMax = 0f;
        ((Wait)(introRoar.Actions[0])).time = 0.1f;

        var customActionCreateTriggerForStart = new CustomLogicFsm(fsm);
        customActionCreateTriggerForStart.action += (Fsm fsm) =>
        {
            var pos = fsm.GameObject.transform.position;

            var customTrigger = CreateTrigger("Ant_19");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(43.45f, 39.28f, pos.z);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                fsm.FsmComponent.SendEvent("TO INTRO LOOK");
            };
        };

        rematch.Actions = new FsmStateAction[]{customActionCreateTriggerForStart};
        rematch.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent("TO INTRO LOOK"),
                ToFsmState = introLook
            }
        };


        return true;
    }
    public static bool PatchFsm_SavageBeastfly1BossScene(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var idle = fsm.GetState("Idle");

        idle.Transitions[1].ToFsmState = null;

        return true;
    }
    public static bool PatchFsm_SavageBeastfly1CorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };
        var waitAction = new Wait
        {
            time = BossInfo.waitForBossDeathAnim,
            finishEvent = FsmEvent.GetFsmEvent("FINISHED")
        };

        var customState2 = new FsmState(fsm);
        customState2.Actions = new FsmStateAction[]{customActionSendEvent};

        var customState1 = new FsmState(fsm);
        customState1.Actions = new FsmStateAction[]{waitAction};
        customState1.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent("FINISHED"),
                ToFsmState = customState2
            }
        };

        // SetTransitionToState(stagger, blow, 0);
        SetTransitionToState(blow, customState1, 0);

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
        var idle = fsm.GetState("Idle");

        // ((SendEventByName)(battleStart.Actions[2])).delay = 0f;

        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) =>
        {
            var init = fsm.GetState("Init");

            var battleStartRange = ((FindNamedChild)(init.Actions[2])).storeResult.Value;

            var battleStartRangeCollider = battleStartRange.GetComponent<BoxCollider2D>();
            battleStartRangeCollider.size = new Vector2(26.6f, battleStartRangeCollider.size.y);
        };

        init.Actions = InsertInArray(init.Actions, customAction, 3);

        SetTransitionToState(init, idle, 1);
        SetTransitionToState(idle, battleStart, 1);

        return true;
    }
    public static bool PatchFsm_SisterSplinterBossReturnScene(Fsm fsm)
    {
        GameObject.Destroy(fsm.GameObject);

        return true;
    }
    public static bool PatchFsm_SisterSplinterCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };
        var waitAction = new Wait
        {
            time = BossInfo.waitForBossDeathAnim,
            finishEvent = FsmEvent.GetFsmEvent("FINISHED")
        };

        var customState2 = new FsmState(fsm);
        customState2.Actions = new FsmStateAction[]{customActionSendEvent};

        var customState1 = new FsmState(fsm);
        customState1.Actions = new FsmStateAction[]{waitAction};
        customState1.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent("FINISHED"),
                ToFsmState = customState2
            }
        };

        SetTransitionToState(stagger, blow, 0);
        SetTransitionToState(blow, customState1, 0);

        return true;
    }
    public static bool PatchFsm_SisterSplinterApproaches(Fsm fsm)
    {
        var pause = fsm.GetState("Pause");
        var r = fsm.GetState("R");
        var l = fsm.GetState("L");

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

        r.Actions = InsertInArray(r.Actions, customAction, r.Actions.Length);
        l.Actions = InsertInArray(l.Actions, customAction, l.Actions.Length);
        return true;
    }
    public static bool PatchFsm_SkullTyrant(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var stateCheck = fsm.GetState("State Check");
        var inRoof = fsm.GetState("In Roof");
        var wakeRoar = fsm.GetState("Wake Roar");
        var rewakePause = fsm.GetState("Rewake Pause");
        var rewakeAntic = fsm.GetState("Rewake Antic");
        
        ((Wait)(wakeRoar.Actions[4])).time = 0.1f;
        ((Wait)(rewakePause.Actions[1])).time = 0f;
        ((Wait)(rewakeAntic.Actions[1])).time = 0.1f;

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.SendEvent("WOKEN");
        };

        SetTransitionToState(stateCheck, inRoof, 0);
        SetTransitionToState(stateCheck, inRoof, 2);
        SetTransitionToState(stateCheck, inRoof, 3);

        stateCheck.Actions = InsertInArray(stateCheck.Actions, customActionSendEvent, 4);

        return true;
    }
    public static bool PatchFsm_SkullTyrantAudioTension(Fsm fsm)
    {
        fsm.FsmComponent.gameObject.SetActive(false);

        return true;
    }
    public static bool PatchFsm_SkullTyrantCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");

        var customActionSendEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        SetTransitionToState(stagger, blow, 0);

        land.Actions = InsertInArray(land.Actions, customActionSendEvent, land.Actions.Length - 1);

        return true;
    }
    public static bool PatchFsm_Widow(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var introScream = fsm.GetState("Intro Scream");
        var setRage = fsm.GetState("Set Rage");
        var deathStaggerF = fsm.GetState("Death Stagger F");
        var rageScream2 = fsm.GetState("Rage Scream 2");
        var away = fsm.GetState("Away");
        var hornetConnect = fsm.GetState("Hornet Connect");
        var canBind = fsm.GetState("Can Bind");
        var finalBindBurst = fsm.GetState("Final Bind Burst");
        var fade = fsm.GetState("Fade");

        ((Wait)(introScream.Actions[3])).time = 0.1f;
        // ((Wait)(deathStaggerF.Actions[16])).time = 0.1f;
        ((Wait)(rageScream2.Actions[1])).time = 0.1f;
        ((Wait)(away.Actions[1])).time = 0.01f;
        ((Wait)(setRage.Actions[3])).time = 0.5f;

        var customActionSkipBind = new CustomLogicFsm(fsm);
        customActionSkipBind.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.SendEvent("BIND");
        };

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            GameCameras.instance.HUDIn();
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        hornetConnect.Actions = RemoveFromArray(hornetConnect.Actions, 10);
        canBind.Actions = RemoveFromArray(canBind.Actions, 1);

        SetTransitionToState(canBind, finalBindBurst, 0);

        canBind.Actions = InsertInArray(canBind.Actions, customActionSkipBind, canBind.Actions.Length);
        fade.Actions = InsertInArray(fade.Actions, customActionSendBossDeadEvent, fade.Actions.Length);


        return true;
    }
    public static bool PatchFsm_WidowBossScene(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var spinnerLook = fsm.GetState("Spinner Look");
        var spinnerAway = fsm.GetState("Spinner Away");
        var checkState = fsm.GetState("Check State");
        var state1 = fsm.GetState("State 1");

        ((Wait)(spinnerLook.Actions[2])).time = 0f;
        ((Wait)(spinnerAway.Actions[0])).time = 0.01f;

        SetTransitionToState(checkState, state1, 0);
        SetTransitionToState(checkState, state1, 2);

        return true;
    }
    public static bool PatchFsm_WidowLever(Fsm fsm)
    {
        fsm.GameObject.SetActive(false);

        return true;
    }
    public static bool PatchFsm_Broodmother(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var entryAntic = fsm.GetState("Entry Antic");
        var roar = fsm.GetState("Roar");
        var dormant = fsm.GetState("Dormant");

        ((Wait)(entryAntic.Actions[5])).time = 0.01f;
        ((Wait)(roar.Actions[8])).time = 0.1f;

        return true;
    }
    public static bool PatchFsm_BroodmotherCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");

        var customActionSendEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        SetTransitionToState(stagger, blow, 0);

        land.Actions = InsertInArray(land.Actions, customActionSendEvent, land.Actions.Length);
        land.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_BroodmotherBGControl(Fsm fsm)
    {
        var opened = fsm.GetState("Opened");

        var customActionCreateTrigger = new CustomLogicFsm(fsm);
        customActionCreateTrigger.action += (Fsm fsm) =>
        {
            var battleScene = fsm.GameObject.transform.parent.parent.gameObject;

            var battleSceneComponent = battleScene.GetComponent<BattleScene>();
            battleSceneComponent.battleStartPause = 0f;
            battleSceneComponent.waves.RemoveRange(0, 3);

            var wave04 = battleSceneComponent.waves[0];
            wave04.startDelay = 0;


            var pos = fsm.GameObject.transform.position;

            var customTrigger = CreateTrigger("Slab_16b");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(pos.x, pos.y, pos.z);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                var battleScene = fsm.GameObject.transform.parent.parent.gameObject;
                var battleSceneComponent = battleScene.GetComponent<BattleScene>();
                battleSceneComponent.StartBattle();
            };
        };

        opened.Actions = InsertInArray(opened.Actions, customActionCreateTrigger, opened.Actions.Length);

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
        var end = fsm.GetState("End");
        var windup4 = fsm.GetState("Windup 4");

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

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        end.Actions = new FsmStateAction[]{customActionSendBossDeadEvent};

        gateClose.Actions = RemoveFromArray(gateClose.Actions, 2);
        gateClose.Actions = RemoveFromArray(gateClose.Actions, 2);

        var list = windup4.Transitions.ToList();
        list.Add(new FsmTransition
        {
            FsmEvent = FsmEvent.GetFsmEvent("FINISHED"),
            ToFsmState = windup4
        });
        windup4.Transitions = list.ToArray();

        var customActionFinished = new CustomLogicFsm(fsm, time: 0.05f);
        customActionFinished.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.SendEvent("FINISHED");
        };

        var customActionCreateTrigger = new CustomLogicFsm(fsm);
        customActionCreateTrigger.action += (Fsm fsm) =>
        {
            var customTrigger = CreateTrigger("Cog_Dancers_boss");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(37.14f, 4.6f, 0f);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                fsm.FsmComponent.SendEvent("ENTER");
            };
        };

        dormant.Actions = new FsmStateAction[]{customActionCreateTrigger};

        windup4.Actions = InsertInArray(windup4.Actions, customActionFinished, windup4.Actions.Length);

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
        var deathBlow = fsm.GetState("Death Blow");

        ((IntCompare)(firstWindup.Actions[0])).integer2 = 1;
        ((IntCompare)(firstWindupOB.Actions[0])).integer2 = 1;

        ((SendEventByName)deathSteam.Actions[5]).delay = 0f;
        ((SendEventByName)deathStagger.Actions[3]).delay = 0f;

        ((SetRandomAudioClipFromTable)deathSteam.Actions[7]).delay = 0f;

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

        deathSteam.Actions = RemoveFromArray(deathSteam.Actions, 4);
        deathSteam.Actions = RemoveFromArray(deathSteam.Actions, 3);
        deathSteam.Actions = RemoveFromArray(deathSteam.Actions, 2);

        return true;
    }
    public static bool PatchFsm_CogDancersBossScene(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var gatesClose = fsm.GetState("Gates Close");
        var wait = fsm.GetState("Wait");
        var rotationSequence = fsm.GetState("Rotation Sequence");
        var check = fsm.GetState("Check");
        var undefeated = fsm.GetState("Undefeated");
        
        ((Wait)(wait.Actions[4])).time = 0.1f;
        ((Wait)(wait.Actions[7])).time = 0.1f;
        ((Wait)(gatesClose.Actions[2])).time = 0.01f;

        SetTransitionToState(check, undefeated, 0);

        return true;
    }
    public static bool PatchFsm_DustChef(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var entryRoar = fsm.GetState("Entry Roar");
        var entryAntic = fsm.GetState("Entry Antic");
        
        // ((Wait)(entryRoar.Actions[5])).time = 0.1f;
        ((Wait)(entryAntic.Actions[3])).time = 0.1f;
        
        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) =>
        {
            var init = fsm.GetState("Init");
            var battleScene = ((GetGrandParent)(init.Actions[0])).storeResult.Value.GetComponent<BattleScene>();
            battleScene.battleStartPause = 0.25f;

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

                PlayMakerFSM.BroadcastEvent("BG CLOSE");
                battleScene.StartBattle();
            };
        };

        init.Actions = InsertInArray(init.Actions, customAction, init.Actions.Length - 1);

        return true;
    }
    public static bool PatchFsm_DustChefCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");
        var splashIn = fsm.GetState("Splash In");

        var customActionSendEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        SetTransitionToState(stagger, blow, 0);

        splashIn.Actions = InsertInArray(splashIn.Actions, customActionSendEvent, 0);
        // land.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_DustChefGongHitReaction(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var waitForHit = fsm.GetState("Wait For Hit");

        SetTransitionToState(init, waitForHit, 1);

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
        var bodyBurn = fsm.GetState("Body Burn");
        var coreLand = fsm.GetState("Core Land");
        var coreSteam = fsm.GetState("Core Steam");
        var coreExplode = fsm.GetState("Core Explode");
        var award = fsm.GetState("Award");
        

        // ((Wait)(init.Actions[40])).time = 0.01f;
        ((Wait)(setHp.Actions[9])).time = 0f;
        ((Wait)(intro.Actions[3])).time = 0.5f;
        ((Wait)(intro.Actions[6])).time = 0.1f;
        ((Wait)(intro.Actions[17])).time = 0.1f;
        ((Wait)(intro.Actions[20])).time = 0.1f;
        // ((Wait)(brokenPause.Actions[0])).time = 0.01f;
        // ((Wait)(brokenPause.Actions[3])).time = 0.01f;
        ((Wait)(flareUp.Actions[11])).time = 0.5f;

        var customActionSendEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        init.Actions = RemoveFromArray(init.Actions, 40);
        bodyBurn.Actions = RemoveFromArray(bodyBurn.Actions, 9);
        coreLand.Actions = RemoveFromArray(coreLand.Actions, 3);
        coreSteam.Actions = RemoveFromArray(coreSteam.Actions, 4);
        coreExplode.Actions = RemoveFromArray(coreExplode.Actions, 13);

        award.Actions = new FsmStateAction[]{customActionSendEvent};
        award.Transitions = new FsmTransition[0];

        SetTransitionToState(init, setHp, 1);

        return true;
    }
    public static bool PatchFsm_FatherOfFlameGateControl(Fsm fsm)
    {
        var pause = fsm.GetState("Pause");
        var close1 = fsm.GetState("Close 1");

        SetTransitionToState(pause, close1, 0);

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

        if(init == null) return true;

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
        var setRespawn = fsm.GetState("Set Respawn");

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
        setRespawn.Actions = RemoveFromArray(setRespawn.Actions, 1);

        // SetTransitionToState(init, breakOut, 1);

        init.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent("FINISHED"),
                ToFsmState = breakOut
            }
        };

        GameObject.DestroyImmediate(fsm.FsmComponent.gameObject.GetComponent<PlayMakerNPC>());

        return true;
    }
    public static bool PatchFsm_FirstSinnerCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var activateSpireNPC = fsm.GetState("Activate Spire NPC");

        var customActionSendEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        SetTransitionToState(stagger, blow, 0);

        blow.Actions = RemoveFromArray(blow.Actions, 6); // screen fader

        activateSpireNPC.Actions = new FsmStateAction[]{customActionSendEvent};
        activateSpireNPC.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_ForebrothersSignisAndGronBossScene(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var battleReady = fsm.GetState("Battle Ready");

        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) =>
        {
            var introMinions = ((FindNamedChild)init.Actions[11]).storeResult.Value;
            introMinions.SetActive(false);
        };

        init.Actions = InsertInArray(init.Actions, customAction, init.Actions.Length - 1);

        SetTransitionToState(init, battleReady, 1);

        return true;
    }
    public static bool PatchFsm_ForebrothersSignisAndGronSlasher(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var startRangeCheck = fsm.GetState("Start Range Check");
        var roar = fsm.GetState("Roar");

        var size = fsm.GameObject.transform.localScale;
        fsm.GameObject.transform.localScale = new Vector3(1, size.y, size.z);

        var customAction = new CustomLogicFsm(fsm);
        customAction.action += (Fsm fsm) =>
        {
            var gates = ((FindNamedChild)init.Actions[2]).storeResult.Value;
            var gate2 = gates.transform.GetChild(1).gameObject;

            if(HeroController.instance.transform.position.x < gate2.transform.position.x)
            {
                var pos = fsm.GameObject.transform.position;
                fsm.GameObject.transform.position = new Vector3(39f, pos.y, pos.z);

                var children = fsm.GameObject.transform;
                foreach(Transform child in children)
                {
                    if(child.gameObject.name == "Start Range")
                    {
                        var childPos = child.gameObject.transform.position;
                        child.gameObject.transform.position = new Vector3(27f, childPos.y, childPos.z);
                        break;
                    }
                }
            }
        };

        ((Wait)(roar.Actions[4])).time = 0.1f;

        SetTransitionToState(init, startRangeCheck, 0);

        init.Actions = InsertInArray(init.Actions, customAction, init.Actions.Length - 1);


        var deathStagger = fsm.GetState("Death Stagger");
        var deathFly = fsm.GetState("Death Fly");
        var lavaBurst = fsm.GetState("Lava Burst");

        var customActionSendEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            var children = fsm.GameObject.transform.parent;
            foreach(Transform child in children)
            {
                if(child.name == "Dock Guard Thrower")
                {
                    var thrower = child.gameObject.GetComponent<HealthManager>();
                    if(thrower.hp < 1) PlayMakerFSM.BroadcastEvent(bossDeadEvent);
                    return;
                }
            }
        };

        deathFly.Actions = InsertInArray(deathFly.Actions, customActionSendEvent, 0);

        // lavaBurst.Actions = RemoveFromArray(lavaBurst.Actions, 2);

        return true;
    }
    public static bool PatchFsm_ForebrothersSignisAndGronThrower(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var deathStagger = fsm.GetState("Death Stagger");
        var deathFly = fsm.GetState("Death Fly");
        var lavaBurst = fsm.GetState("Lava Burst");

        var customActionSendEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            var children = fsm.GameObject.transform.parent;
            foreach(Transform child in children)
            {
                if(child.name == "Dock Guard Slasher")
                {
                    var slasher = child.gameObject.GetComponent<HealthManager>();
                    if(slasher.hp < 1) PlayMakerFSM.BroadcastEvent(bossDeadEvent);
                    return;
                }
            }
        };

        deathFly.Actions = InsertInArray(deathFly.Actions, customActionSendEvent, 0);

        // lavaBurst.Actions = RemoveFromArray(lavaBurst.Actions, 2);

        return true;
    }
    public static bool PatchFsm_GarmondAndZaza(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var roarAntic = fsm.GetState("Roar Antic");
        var autoTarget = fsm.GetState("Auto Target?");
        var appearRange = fsm.GetState("Appear Range");
        var citNPC = fsm.GetState("Cit NPC");
        var citadelRemeet = fsm.GetState("Citadel Remeet");
        var enemyRoar = fsm.GetState("Enemy Roar");
        var setup1 = fsm.GetState("Setup 1");
        var setup2 = fsm.GetState("Setup 2");
        var deathLand = fsm.GetState("Death Land");

        // ((Wait)(setup1.Actions[15])).time = 0.001f;
        ((Wait)(setup2.Actions[3])).time = 0.001f;
        ((Wait)(enemyRoar.Actions[3])).time = 0.1f;

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

        var customActionSendEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        citNPC.Actions = InsertInArray(citNPC.Actions, customAction, citNPC.Actions.Length - 1);

        deathLand.Actions = InsertInArray(deathLand.Actions, customActionSendEvent, 0);

        setup1.Actions = RemoveFromArray(setup1.Actions, 2);

        return true;
    }
    public static bool PatchFsm_GarmondAndZazaSceneControl(Fsm fsm)
    {
        var idle = fsm.GetState("Idle");

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.SendEvent("SEEN");
        };

        idle.Actions = new FsmStateAction[]{customActionSendEvent};

        return true;
    }
    public static bool PatchFsm_GarmondAndZazaDestroyNPCComponent(Fsm fsm)
    {
        GameObject.Destroy(fsm.GameObject);

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
        
        ((SendEventByName)(waitForBeatEnd.Actions[1])).delay = 0f;
        ((Wait)(readyWait.Actions[1])).time = 0.1f;
        ((Wait)(introShake.Actions[1])).time = 0.1f;

        var customActionSpeedUpCocoonAnimation = new CustomLogicFsm(fsm);
        customActionSpeedUpCocoonAnimation.action += (Fsm fsm) =>
        {
            var animatorGO = ((FindNamedChild)init.Actions[5]).storeResult.Value;
            var animatorComponent = animatorGO.GetComponent<Animator>();
            animatorComponent.speed = 10;
            animatorGO.SetActive(false);
        };

        init.Actions = InsertInArray(init.Actions, customActionSpeedUpCocoonAnimation, init.Actions.Length);

        waitForBeatEnd.Actions = RemoveFromArray(waitForBeatEnd.Actions, 2);
        
        SetTransitionToState(waitForBeatEnd, quickStart, 0);

        // readyWait.Actions = RemoveFromArray(readyWait.Actions, 2);
        // readyWait.Actions = RemoveFromArray(readyWait.Actions, 1);
        
        return true;
    }
    public static bool PatchFsm_SilkBossDeathSequence(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var deathSlashesUp = fsm.GetState("Death Slashes Up");
        var deathStart = fsm.GetState("Death Start");
        var bindOrNeedolin = fsm.GetState("Bind Or Needolin");
        var ready1 = fsm.GetState("Ready 1");
        var ready2 = fsm.GetState("Ready 2");
        var ready3 = fsm.GetState("Ready 3");
        var ready4 = fsm.GetState("Ready 4");
        var bind1 = fsm.GetState("Bind 1");
        var bind2 = fsm.GetState("Bind 2");
        var bind3 = fsm.GetState("Bind 3");
        var bind4 = fsm.GetState("Bind 4");
        var bindBurst1 = fsm.GetState("Bind Burst 1");
        var bindBurst2 = fsm.GetState("Bind Burst 2");
        var bindBurst3 = fsm.GetState("Bind Burst 3");
        var bindBurst4 = fsm.GetState("Bind Burst 4");
        var finalBind = fsm.GetState("Final Bind");
        var toBind2 = fsm.GetState("To Bind 2");
        var hornetAttach = fsm.GetState("Hornet Attach");
        var preBindable = fsm.GetState("Pre Bindable");

        // {
        //     // ((IntCompare)bind1.Actions[5]).integer2 = 1;
        //     // ((IntCompare)bind2.Actions[1]).integer2 = 3;
        //     ((IntCompare)bind3.Actions[1]).integer2 = 3;
        //     ((IntCompare)bind4.Actions[1]).integer2 = 5;
        // }

        ((Wait)hornetAttach.Actions[1]).time = 0f;

        ((SendEventToRegister)bindBurst2.Actions[12]).eventName = "";
        ((SendEventToRegister)bindBurst3.Actions[10]).eventName = "";
        ((SendEventToRegister)bindBurst4.Actions[12]).eventName = "";

        ((SendEventToRegister)bindBurst2.Actions[14]).eventName = "";
        ((SendEventToRegister)finalBind.Actions[9]).eventName = "";

        var customActionSendEventBind = new CustomLogicFsm(fsm);
        customActionSendEventBind.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.SendEvent("BIND");
        };
        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        bindOrNeedolin.Actions = InsertInArray(bindOrNeedolin.Actions, customActionSendEventBind, 
                                                bindOrNeedolin.Actions.Length);
        ready2.Actions = InsertInArray(ready2.Actions, customActionSendEventBind, 
                                        ready2.Actions.Length);
        ready3.Actions = InsertInArray(ready3.Actions, customActionSendEventBind, 
                                        ready3.Actions.Length);
        ready4.Actions = InsertInArray(ready4.Actions, customActionSendEventBind, 
                                        ready4.Actions.Length);
        
        finalBind.Actions = InsertInArray(finalBind.Actions, customActionSendBossDeadEvent, 0);

        ///remove add silk
        bindBurst1.Actions = RemoveFromArray(bindBurst1.Actions, 10);
        bindBurst2.Actions = RemoveFromArray(bindBurst2.Actions, 11);
        bindBurst3.Actions = RemoveFromArray(bindBurst3.Actions, 9);
        bindBurst4.Actions = RemoveFromArray(bindBurst4.Actions, 11);
        
        //remove waits
        preBindable.Actions = RemoveFromArray(preBindable.Actions, 1);
        bindBurst1.Actions = RemoveFromArray(bindBurst1.Actions, 5);
        bindBurst2.Actions = RemoveFromArray(bindBurst2.Actions, 6);
        bindBurst3.Actions = RemoveFromArray(bindBurst3.Actions, 5);
        bindBurst4.Actions = RemoveFromArray(bindBurst4.Actions, 7);

        deathSlashesUp.Actions = RemoveFromArray(deathSlashesUp.Actions, 20);

        toBind2.Actions = RemoveFromArray(toBind2.Actions, 10); //remove set UnlockSilkFinalCutscene = true

        finalBind.Transitions = new FsmTransition[0];

        var customActionSetToolItemManagerActive = new CustomLogicFsm(fsm);
        customActionSetToolItemManagerActive.action += (Fsm fsm) =>
        {
            ToolItemManager.SetActiveState(ToolsActiveStates.Active);
        };
        finalBind.Actions = InsertInArray(finalBind.Actions, customActionSetToolItemManagerActive, 0);
        
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
        var straightBack = fsm.GetState("Straight Back?");

        idle.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent("START CHALLENGE MOD"),
                ToFsmState = inRegion
            }
        };

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.SendEvent("SPECIAL CHALLENGE");
        };

        inRegion.Actions[1] = customActionSendEvent;

        return true;
    }
    public static bool PatchFsm_GroalTheGreat(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var fakeBattleEnd = fsm.GetState("Fake Battle End");
        var entryAntic = fsm.GetState("Entry Antic");
        var entryRoar = fsm.GetState("Entry Roar");
        var dormant = fsm.GetState("Dormant");
        var deathHit = fsm.GetState("Death Hit");
        var vomitHornet = fsm.GetState("Vomit Hornet");
        var blow = fsm.GetState("Blow");
        
        ((Wait)(entryRoar.Actions[2])).time = 0.1f;
        ((Wait)(entryAntic.Actions[1])).time = 0.01f;

        var customActionCreateTriggerAndSetupBattleScene = new CustomLogicFsm(fsm);
        customActionCreateTriggerAndSetupBattleScene.action += (Fsm fsm) =>
        {
            var battleScene = ((GetGrandparent)init.Actions[5]).storeResult.Value;
            var battleSceneComponent = battleScene.GetComponent<BattleScene>();
            var collider = battleScene.GetComponent<BoxCollider2D>();

            battleSceneComponent.battleStartPause = 0.25f;
            battleSceneComponent.waves.RemoveRange(0, 5);

            collider.size = new Vector2(29.5f, collider.size.y);


            var pos = fsm.GameObject.transform.position;

            var customTrigger = CreateTrigger("Shadow_18");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(55.8f, 10.8f, 0);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                var battleScene = fsm.GameObject.transform.parent.parent.gameObject;
                var battleSceneComponent = battleScene.GetComponent<BattleScene>();
                battleSceneComponent.StartBattle();
            };
        };

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        init.Actions = InsertInArray(init.Actions, customActionCreateTriggerAndSetupBattleScene, 
            init.Actions.Length - 1);
        
        blow.Actions = InsertInArray(blow.Actions, customActionSendBossDeadEvent, 0);

        SetTransitionToState(dormant, entryAntic, 0);
        SetTransitionToState(deathHit, vomitHornet, 0);

        blow.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_GroalTheGreatCloseGate(Fsm fsm)
    {
        var pause = fsm.GetState("Pause");
        var close1 = fsm.GetState("Close 1");
        
        SetTransitionToState(pause, close1, 0);

        return true;
    }
    public static bool PatchFsm_Lace2door_cutsceneEndLaceTower(Fsm fsm)
    {
        fsm.GameObject.SetActive(false);
        // var init = fsm.GetState("Init");
        // var liftArrive = fsm.GetState("Lift Arrive?");
        // var liftAlreadyHere = fsm.GetState("Lift Already Here");
        // var setHeroPos = fsm.GetState("Set Hero Pos");
        // var startCinematic = fsm.GetState("Start Cinematic");
        // var doorEntry = fsm.GetState("Door Entry");

        // startCinematic.Actions = RemoveFromArray(startCinematic.Actions, 0);

        // var customAction = new CustomLogicFsm(fsm);
        // customAction.action += (Fsm fsm) =>
        // {
        //     fsm.FsmComponent.SendEvent("CINEMATIC END");
        // };

        // startCinematic.Actions = InsertInArray(startCinematic.Actions, customAction, startCinematic.Actions.Length);


        return true;
    }
    public static bool PatchFsm_Lace2BossControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var startBattleWait = fsm.GetState("Start Battle Wait");
        var startBattle = fsm.GetState("Start Battle");

        startBattleWait.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(TransitionPointInfo.eventName),
                ToFsmState = startBattle
            }
        };

        SetTransitionToState(init, startBattleWait, 1);

        return true;
    }
    public static bool PatchFsm_Lace2CorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var setTalkPos = fsm.GetState("Set Talk Pos");
        var land = fsm.GetState("Land");

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        SetTransitionToState(stagger, blow, 0);

        land.Actions = InsertInArray(land.Actions, customActionSendBossDeadEvent, 0);

        land.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_Lace2ReturnCorpseDeactivate(Fsm fsm)
    {
        fsm.GameObject.SetActive(false);

        return true;
    }
    public static bool PatchFsm_Lace2RightGate(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        
        init.Transitions = new FsmTransition[0];

        fsm.GameObject.GetComponent<Gate>().ForceClose();

        return true;
    }
    public static bool PatchFsm_RagingConchfly(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var dormant = fsm.GetState("Dormant");
        var startPause = fsm.GetState("Start Pause");
        var introL = fsm.GetState("Intro L");
        var intro2 = fsm.GetState("Intro 2");
        var roar = fsm.GetState("Roar");

        ((Wait)(startPause.Actions[0])).time = 0f;
        ((Wait)(introL.Actions[11])).time = 0.2f;
        ((AnimatePositionBy)(intro2.Actions[4])).time = 0.01f;
        ((AnimatePositionBy)(introL.Actions[9])).time = 0.01f;
        ((Wait)(roar.Actions[1])).time = 0.1f;

        var customActionCreateTriggerAndSetupBattleScene = new CustomLogicFsm(fsm);
        customActionCreateTriggerAndSetupBattleScene.action += (Fsm fsm) =>
        {
            var battleScene = fsm.GameObject.transform.parent.parent.gameObject;
            var battleSceneComponent = battleScene.GetComponent<BattleScene>();
            var collider = battleScene.GetComponent<BoxCollider2D>();

            battleSceneComponent.battleStartPause = 0f;


            var customTrigger = CreateTrigger("Coral_27");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(18.09f, 39f, 0);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                battleSceneComponent.StartBattle();
            };
        };

        dormant.Actions = InsertInArray(dormant.Actions, customActionCreateTriggerAndSetupBattleScene, dormant.Actions.Length);

        init.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(TransitionPointInfo.eventName),
                ToFsmState = dormant
            }
        };

        var pos = fsm.GameObject.transform.position;
        fsm.GameObject.transform.position = new Vector3(pos.x + 3, pos.y, pos.z);

        return true;
    }
    public static bool PatchFsm_RagingConchflyCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");

        var customActionSendEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        SetTransitionToState(stagger, blow, 0);

        land.Actions = InsertInArray(land.Actions, customActionSendEvent, 0);

        return true;
    }
    public static bool PatchFsm_SavageBeastfly2(Fsm fsm)
    {
        var go = fsm.GameObject;
        var children = go.transform;
        foreach(Transform child in children)
        {
            if(child.name == "Wake Range")
            {
                var pos = child.position;
                child.position = new Vector3(77.5f, pos.y, pos.z);
                break;
            }
        }

        var init = fsm.GetState("Init");
        var rematchPause = fsm.GetState("Rematch Pause");
        var entryAntic = fsm.GetState("Entry Antic");
        var rematchRoar = fsm.GetState("Rematch Roar");

        ((Wait)(init.Actions[12])).time = 0f;
        ((Wait)(rematchPause.Actions[2])).time = 0f;
        ((Wait)(entryAntic.Actions[6])).time = 0.1f;
        // ((Wait)(rematchRoar.Actions[3])).time = 0.1f;

        return true;
    }
    public static bool PatchFsm_SavageBeastfly2CorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        blow.Actions = InsertInArray(blow.Actions, customActionSendBossDeadEvent, blow.Actions.Length);

        // SetTransitionToState(stagger, blow, 0);

        return true;
    }
    public static bool PatchFsm_SecondSentielControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var encountered = fsm.GetState("Encountered");
        var encWake = fsm.GetState("Enc Wake");
        var becomeActive = fsm.GetState("Become Active");

        ((Wait)(encWake.Actions[3])).time = 0f;
        ((Wait)(becomeActive.Actions[2])).time = 0.01f;

        SetTransitionToState(init, encountered, 1);
        SetTransitionToState(init, encountered, 2);

        return true;
    }
    public static bool PatchFsm_SecondSentielBossSceneControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var idle = fsm.GetState("Idle");
        var arenaStart = fsm.GetState("Arena Start");
        var skipArena = fsm.GetState("Skip Arena");

        idle.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(TransitionPointInfo.eventName),
                ToFsmState = skipArena
            }
        };

        return true;
    }
    public static bool PatchFsm_SecondSentielCorpseControl(Fsm fsm)
    {
        var deathHit = fsm.GetState("Death Hit");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        land.Actions = InsertInArray(land.Actions, customActionSendBossDeadEvent, land.Actions.Length);

        SetTransitionToState(deathHit, blow, 0);

        land.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_ShakraAttackEnemies(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var idle = fsm.GetState("Idle");
        var reset = fsm.GetState("Reset");
        var startAwayPause = fsm.GetState("Start Away Pause");
        var callPause = fsm.GetState("Call Pause");
        var startAway = fsm.GetState("Start Away");
        var mapperEnter = fsm.GetState("Mapper Enter");
        var battleCryStart = fsm.GetState("Battle Cry Start");
        var battleCry1 = fsm.GetState("Battle Cry 1");
        var setFightingHero = fsm.GetState("Set Fighting Hero");
        var endBattle = fsm.GetState("End Battle");
        var defeatStart = fsm.GetState("Defeat Start");
        var defeatLand = fsm.GetState("Defeat Land");
        var defeatShout1 = fsm.GetState("Defeat Shout 1");
        var defeatShout2 = fsm.GetState("Defeat Shout 2");

        var customActionEndState = new CustomLogicFsm(fsm);
        customActionEndState.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.SendEvent("FINISHED");
        };

        var customActionCreateTrigger = new CustomLogicFsm(fsm);
        customActionCreateTrigger.action += (Fsm fsm) =>
        {
            var pos = fsm.GameObject.transform.position;

            var customTrigger = CreateTrigger("Greymoor_08_Mapper");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(pos.x, pos.y, pos.z);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                fsm.FsmComponent.SendEvent("MAPPER CALL");
            };
        };

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        // defeatStart.Actions = RemoveFromArray(defeatStart.Actions, 26);
        defeatLand.Actions = RemoveFromArray(defeatLand.Actions, 5);
        defeatShout2.Actions = RemoveFromArray(defeatShout2.Actions, 3);

        mapperEnter.Actions = InsertInArray(mapperEnter.Actions, customActionEndState, 1);
        defeatLand.Actions = InsertInArray(defeatLand.Actions, customActionSendBossDeadEvent, 0);
        startAway.Actions = InsertInArray(startAway.Actions, customActionCreateTrigger, startAway.Actions.Length);

        ((Wait)(init.Actions[38])).time = 0f;
        ((Wait)(startAwayPause.Actions[0])).time = 0f;
        ((Wait)(callPause.Actions[0])).time = 0f;

        SetTransitionToState(startAway, mapperEnter, 0);
        SetTransitionToState(mapperEnter, reset, 0);
        SetTransitionToState(reset, setFightingHero, 0);
        // SetTransitionToState(endBattle, startAwayPause, 0);
        SetTransitionToState(battleCryStart, battleCry1, 1);

        startAwayPause.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(TransitionPointInfo.eventName),
                ToFsmState = startAway
            }
        };

        defeatShout2.Transitions = new FsmTransition[0];

        endBattle.Actions = new FsmStateAction[0];
        endBattle.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_ShakraCallPole(Fsm fsm)
    {
        fsm.GameObject.SetActive(false);

        return true;
    }
    public static bool PatchFsm_TheUnravelledBossScene(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var encounteredStart = fsm.GetState("Encountered Start");
        var arenaStart = fsm.GetState("Arena Start");
        var bossPhase1 = fsm.GetState("Boss Phase 1");
        var spearSuckPause = fsm.GetState("Spear Suck Pause");
        var suckSpears = fsm.GetState("Suck Spears");
        var p3Shake2 = fsm.GetState("P3 Shake 2");
        var posSuckSpearsPause = fsm.GetState("Pos Suck Spears Pause");
        var p3Shake = fsm.GetState("P3 Shake");
        var headpieceAntic = fsm.GetState("Headpiece Antic");
        var headpiecePause = fsm.GetState("Headpiece Pause");
        var headpieceSuck = fsm.GetState("Headpiece Suck");
        var deathExplode = fsm.GetState("Death Explode");

        ((Wait)arenaStart.Actions[4]).time = 0f;
        ((Wait)encounteredStart.Actions[1]).time = 0f;
        ((Wait)spearSuckPause.Actions[0]).time = 0f;
        ((Wait)p3Shake2.Actions[2]).time = 0.1f;
        ((Wait)suckSpears.Actions[1]).time = 0.1f;
        ((Wait)posSuckSpearsPause.Actions[0]).time = 0.1f;
        ((Wait)p3Shake.Actions[3]).time = 0.01f;
        ((Wait)headpieceAntic.Actions[3]).time = 0.01f;
        ((Wait)headpiecePause.Actions[0]).time = 0f;
        ((Wait)headpieceSuck.Actions[0]).time = 0.01f;
        ((Translate)headpieceSuck.Actions[3]).y = -1000f;

        SetTransitionToState(arenaStart, encounteredStart, 0);
        SetTransitionToState(encounteredStart, p3Shake, 0);

        deathExplode.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_TheUnravelledControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var introRoar = fsm.GetState("Intro Roar");
        var teleAnticIntro = fsm.GetState("Tele Antic Intro");
        var die = fsm.GetState("Die");
        var deathBlow = fsm.GetState("Death Blow");

        ((Wait)introRoar.Actions[2]).time = 0.1f;
        ((Wait)teleAnticIntro.Actions[3]).time = 0f;

        SetTransitionToState(die, deathBlow, 0);

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        deathBlow.Actions = InsertInArray(deathBlow.Actions, customActionSendBossDeadEvent, 0);

        return true;
    }
    public static bool PatchFsm_TheUnravelledPipeControl(Fsm fsm)
    {
        var state3 = fsm.GetState("State 3");

        state3.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_TrobbioControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var state = fsm.GetState("State");
        var waitRefight = fsm.GetState("Wait Refight");
        var startPause = fsm.GetState("Start Pause");
        var quickEntrance1 = fsm.GetState("Quick Entrance 1");
        var quickEntrance2 = fsm.GetState("Quick Entrance 2");
        var quickEntrance3 = fsm.GetState("Quick Entrance 3");
        var stopStream = fsm.GetState("Stop Stream");
        var deathAir = fsm.GetState("Death Air");
        var deathPose = fsm.GetState("Death Pose");
        var finalPose1 = fsm.GetState("Final Pose 1");
        var finalPose2 = fsm.GetState("Final Pose 2");
        var finalFireworks = fsm.GetState("Final Fireworks");
        var collapse = fsm.GetState("Collapse");

        ((Wait)quickEntrance1.Actions[0]).time = 0f;
        ((Wait)quickEntrance2.Actions[3]).time = 0.1f;
        ((Wait)quickEntrance3.Actions[2]).time = 0.1f;
        ((Wait)finalFireworks.Actions[1]).time = 0.5f;

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.SendEvent("END");
        };

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        SetTransitionToState(state, waitRefight, 0);
        SetTransitionToState(startPause, quickEntrance1, 0);
        SetTransitionToState(deathPose, finalPose1, 0);

        deathPose.Actions = InsertInArray(deathPose.Actions, customActionSendEvent, deathPose.Actions.Length);
        
        collapse.Actions = InsertInArray(collapse.Actions, customActionSendBossDeadEvent, 0);

        startPause.Actions = RemoveFromArray(startPause.Actions, 0);
        finalPose1.Actions = RemoveFromArray(finalPose1.Actions, 8);
        finalPose2.Actions = RemoveFromArray(finalPose2.Actions, 1);

        collapse.Transitions = new FsmTransition[0];

        waitRefight.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(TransitionPointInfo.eventName),
                ToFsmState = startPause
            }
        };

        return true;
    }
    public static bool PatchFsm_TrobbioGrandStageSceneControl(Fsm fsm)
    {
        var act2 = fsm.GetState("Act 2");
        var act3 = fsm.GetState("Act 3");
        var trobbioReady = fsm.GetState("Trobbio Ready");
        var trobbioReady2 = fsm.GetState("Trobbio Ready 2");

        SetTransitionToState(act2, trobbioReady, 1);
        SetTransitionToState(act3, trobbioReady2, 1);
        SetTransitionToState(act3, trobbioReady2, 2);

        trobbioReady2.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_VoltvyrmControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var dormant = fsm.GetState("Dormant");
        var introPause = fsm.GetState("Intro Pause");
        var introAntic = fsm.GetState("Intro Antic");
        var roar = fsm.GetState("Roar");
        var deathHit = fsm.GetState("Death Hit");
        var blow = fsm.GetState("Blow");

        ((Wait)introPause.Actions[0]).time = 0f;
        ((Wait)introAntic.Actions[1]).time = 0f;
        ((Wait)roar.Actions[13]).time = 0.1f;

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        SetTransitionToState(deathHit, blow, 0);

        blow.Actions = InsertInArray(blow.Actions, customActionSendBossDeadEvent, 0);

        blow.Transitions = new FsmTransition[0];

        dormant.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(TransitionPointInfo.eventName),
                ToFsmState = introPause
            }
        };

        return true;
    }
    public static bool PatchFsm_BellEaterControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var dormant = fsm.GetState("Dormant");
        var setHp = fsm.GetState("Set HP");
        var bodyRUp = fsm.GetState("Body R Up");
        var introCam = fsm.GetState("Intro Cam");
        var bodyLDown = fsm.GetState("Body L Down");
        var introRoar = fsm.GetState("Intro Roar");
        var deathPause = fsm.GetState("Death Pause");
        var deathHeadAntic = fsm.GetState("Death Head Antic");
        var deathHeadRoar = fsm.GetState("Death Head Roar");
        var bellbeastJumpsIn = fsm.GetState("Bellbeast Jumps In");
        var bellBeastConnect = fsm.GetState("Bell Beast Connect");
        var yankWallL = fsm.GetState("Yank Wall L");
        var shakeSequence = fsm.GetState("Shake Sequence");
        var spitHeadOut = fsm.GetState("Spit Head Out");

        ((Wait)bodyRUp.Actions[11]).time = 0.01f;
        ((Wait)introCam.Actions[0]).time = 0.01f;
        ((Wait)bodyLDown.Actions[6]).time = 0.01f;
        ((Wait)introRoar.Actions[3]).time = 0.1f;
        ((Wait)introRoar.Actions[3]).time = 0.1f;
        ((Wait)deathPause.Actions[0]).time = 0f;
        ((Wait)deathHeadAntic.Actions[5]).time = 0f;
        ((Wait)deathHeadRoar.Actions[1]).time = 0.1f;
        ((Wait)bellbeastJumpsIn.Actions[8]).time = 0.5f;
        ((Wait)bellBeastConnect.Actions[9]).time = 0.1f;
        ((Wait)yankWallL.Actions[1]).time = 0.1f;

        // var customActionReplaceStartRage = new CustomLogicFsm(fsm);
        // customActionReplaceStartRage.action += (Fsm fsm) =>
        // {
        //     var startRange = ((FindNamedChild)init.Actions[8]).storeResult.Value;
        //     var pos = startRange.transform.position;
        //     startRange.transform.position = new Vector3(pos.x - 10f, pos.y, pos.z);
        // };

        // init.Actions = InsertInArray(init.Actions, customActionReplaceStartRage, init.Actions.Length - 1);
        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        spitHeadOut.Actions = InsertInArray(spitHeadOut.Actions, customActionSendBossDeadEvent, 0);

        shakeSequence.Actions = RemoveFromArray(shakeSequence.Actions, 33);
        shakeSequence.Actions = RemoveFromArray(shakeSequence.Actions, 23);
        shakeSequence.Actions = RemoveFromArray(shakeSequence.Actions, 13);
        shakeSequence.Actions = RemoveFromArray(shakeSequence.Actions, 3);

        dormant.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(TransitionPointInfo.eventName),
                ToFsmState = setHp
            }
        };

        spitHeadOut.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_CloverDancersGreenPrinceBossNPC(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var encountered = fsm.GetState("Encountered?");
        var encounteredStart = fsm.GetState("Encountered Start");
        

        ((Wait)encounteredStart.Actions[2]).time = 0.01f;

        SetTransitionToState(encountered, encounteredStart, 0);

        return true;
    }
    public static bool PatchFsm_CloverDancersDancerControl(Fsm fsm)
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
        
        ((Wait)(returnDancers.Actions[4])).time = 0.3f;
        ((Wait)(dancersStunned.Actions[6])).time = 0.3f;

        return true;
    }
    public static bool PatchFsm_CloverDancersDancerAB(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var cloverRoar = fsm.GetState("Clover Roar");
        var cloverSubRoar = fsm.GetState("Clover Sub Roar");
        var cRoar = fsm.GetState("C Roar");
        var cRoar2 = fsm.GetState("C Roar 2");

        ((Wait)cloverRoar.Actions[0]).time = 0.1f;
        ((Wait)cloverSubRoar.Actions[2]).time = 0.05f;
        ((Wait)cRoar.Actions[2]).time = ((Wait)cRoar.Actions[2]).time.Value / 5f;
        ((Wait)cRoar2.Actions[4]).time = ((Wait)cRoar2.Actions[4]).time.Value / 5f;

        return true;
    }
    public static bool PatchFsm_CloverDancersCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        land.Actions = InsertInArray(land.Actions, customActionSendBossDeadEvent, 0);

        SetTransitionToState(stagger, blow, 0);

        land.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_CrawfatherControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var emergeAnnounce = fsm.GetState("Emerge Announce");
        var emerge = fsm.GetState("Emerge");
        var flapDown = fsm.GetState("Flap Down");
        var BGIdle = fsm.GetState("BG Idle");
        var BGRoar = fsm.GetState("BG Roar");
        var BGPeck1 = fsm.GetState("BG Peck 1");
        var BGPeakEnd = fsm.GetState("BG Peck End");
        var roar = fsm.GetState("Roar");

        ((Wait)emergeAnnounce.Actions[2]).time = 0f;
        ((Wait)roar.Actions[3]).time = 0.1f;

        var AnimateXPosition = new AnimateXPositionTo
        {
            GameObject = new FsmOwnerDefault(){GameObject = fsm.GameObject},
            ToValue = fsm.GameObject.transform.position.x + 6.5f,
            localSpace = false,
            time = 0.4f,
            easeType = (EaseFsmAction.EaseType)4,
            // finishEvent = FsmEvent.GetFsmEvent(""),
            delay = 0,
            reverse = false,
            speed = 1,
            realTime = false,
            BlocksFinish = true
        };

        var customActionRotate = new CustomLogicFsm(fsm);
        customActionRotate.action += (Fsm fsm) =>
        {
            if(fsm.GameObject.transform.position.x > HeroController.instance.gameObject.transform.position.x)
                fsm.GameObject.transform.localScale = new Vector3(-1, 1, 1);
            else fsm.GameObject.transform.localScale = new Vector3(1, 1, 1);
        };

        BGRoar.Transitions[0].FsmEvent.Name = "FINISHED";

        emerge.Actions = InsertInArray(emerge.Actions, AnimateXPosition, 5);
        flapDown.Actions = InsertInArray(flapDown.Actions, customActionRotate, 0);

        SetTransitionToState(BGPeakEnd, emergeAnnounce, 0);

        BGIdle.Transitions = RemoveFromArray(BGIdle.Transitions, 1);

        return true;
    }
    public static bool PatchFsm_CrawfatherBattleStart(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var idle = fsm.GetState("Idle");
        var enter = fsm.GetState("Enter");
        var startWipeAudio = fsm.GetState("Start Wipe Audio");
        var lightsUp = fsm.GetState("Lights Up");
        var crowdRoar = fsm.GetState("Crowd Roar");
        var crowdIdle = fsm.GetState("Crowd Idle");

        ((Wait)enter.Actions[3]).time = 0f;
        ((Wait)startWipeAudio.Actions[1]).time = 0f;
        ((SendEventToRegisterDelay)lightsUp.Actions[8]).delay = 0f;
        ((EaseSpriteColor)lightsUp.Actions[2]).time = 0.001f;
        ((EaseFloat)lightsUp.Actions[4]).time = 0.001f;
        ((Wait)lightsUp.Actions[6]).time = 0.01f;
        ((Wait)crowdRoar.Actions[4]).time = 0f;
        ((Wait)crowdIdle.Actions[1]).time = 0.05f;

        var customActionSetupBattle = new CustomLogicFsm(fsm);
        customActionSetupBattle.action += (Fsm fsm) =>
        {
            var battleSceneComponent = fsm.GameObject.transform.parent.gameObject.GetComponent<BattleScene>();
            battleSceneComponent.battleStartPause = 0.25f;
            var waves = battleSceneComponent.waves;
            var wave6 = waves[waves.Count - 1];

            battleSceneComponent.waves = new List<BattleWave>{waves[waves.Count - 1]};
            wave6.startDelay = 0f;
            fsm.FsmComponent.SendEvent("ENTER");
        };

        idle.Actions = InsertInArray(idle.Actions, customActionSetupBattle, 0);

        init.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(TransitionPointInfo.eventName),
                ToFsmState = idle
            }
        };

        SetTransitionToState(idle, enter, 1);

        lightsUp.Actions = RemoveFromArray(lightsUp.Actions, 8);
        lightsUp.Actions = RemoveFromArray(lightsUp.Actions, 7);
        crowdIdle.Actions = RemoveFromArray(crowdIdle.Actions, 1);

        return true;
    }
    public static bool PatchFsm_CrawfatherCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        land.Actions = InsertInArray(land.Actions, customActionSendBossDeadEvent, 0);

        SetTransitionToState(stagger, blow, 0);

        return true;
    }
    public static bool PatchFsm_CrustKingKhanControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var dormant = fsm.GetState("Dormant");
        var startPos = fsm.GetState("Start Pos");
        var introRoar = fsm.GetState("Intro Roar");
        var refightPos = fsm.GetState("Refight Pos");
        var refightAntic = fsm.GetState("Refight Antic");
        var airRoar = fsm.GetState("Air Roar");
        var deathStagger = fsm.GetState("Death Stagger");
        var deathFall = fsm.GetState("Death Fall");
        var getItem = fsm.GetState("Get Item");
        var grabIdle = fsm.GetState("Grab Idle");
        var yank = fsm.GetState("Yank");
        var hornetLand = fsm.GetState("Hornet Land");
        var finalRumble = fsm.GetState("Final Rumble");

        ((Wait)introRoar.Actions[8]).time = 0.1f;
        ((Wait)airRoar.Actions[8]).time = 0.1f;

        ((IntCompare)yank.Actions[2]).integer2 = 2;

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            fsm.FsmComponent.SendEvent("YANK");
        };

        SetTransitionToState(dormant, startPos, 1);
        SetTransitionToState(deathStagger, deathFall, 0);

        hornetLand.Actions = InsertInArray(hornetLand.Actions, customActionSendBossDeadEvent, 0);
        grabIdle.Actions = InsertInArray(grabIdle.Actions, customActionSendEvent, 0);

        getItem.Actions = RemoveFromArray(getItem.Actions, 1);

        return true;
    }
    public static bool PatchFsm_CrustKingKhanBossSceneControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var enter = fsm.GetState("Enter");
        var preIntroRoar = fsm.GetState("Pre Intro Roar");
        var encounteredPause = fsm.GetState("Encountered Pause");
        var throneRumble = fsm.GetState("Throne Rumble");
        var spearsUp = fsm.GetState("Spears Up");

        ((Wait)enter.Actions[3]).time = 0.01f;
        ((Wait)preIntroRoar.Actions[2]).time = 0.01f;
        ((Wait)throneRumble.Actions[1]).time = 0.1f;
        ((Wait)spearsUp.Actions[1]).time = 0.1f;
        ((AudioPlayerOneShotSingle)throneRumble.Actions[3]).delay = 0f;

        var customActionDisableThrone = new CustomLogicFsm(fsm);
        customActionDisableThrone.action += (Fsm fsm) =>
        {
            var throne = ((FindNamedChild)init.Actions[3]).storeResult.Value;
            throne.SetActive(false);
        };

        init.Actions = InsertInArray(init.Actions, customActionDisableThrone, init.Actions.Length - 1);

        SetTransitionToState(enter, throneRumble, 0);
        SetTransitionToState(enter, throneRumble, 1);

        return true;
    }
    public static bool PatchFsm_GurrTheOutcastControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var posChoice = fsm.GetState("Pos Choice");
        var pos2 = fsm.GetState("Pos 2");
        var ambushAntic = fsm.GetState("Ambush Antic");
        var burstOut = fsm.GetState("Burst Out");
        var introRoar = fsm.GetState("Intro Roar");
        var hiding = fsm.GetState("Hiding");
        
        ((Wait)ambushAntic.Actions[2]).time = 0.01f;
        ((Wait)introRoar.Actions[3]).time = 0.1f;

        var customActionStartBattle = new CustomLogicFsm(fsm);
        customActionStartBattle.action += (Fsm fsm) =>
        {
            var pos = fsm.GameObject.transform.position;

            var customTrigger = CreateTrigger("Bone_East_18b");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(pos.x - 10, pos.y - 5, pos.z);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                PlayMakerFSM.BroadcastEvent("BG CLOSE");
                fsm.FsmComponent.SendEvent("START");
            };
        };

        hiding.Actions = InsertInArray(hiding.Actions, customActionStartBattle, 0);

        init.Actions = RemoveFromArray(init.Actions, 4);

        SetTransitionToState(posChoice, pos2, 0);
        SetTransitionToState(posChoice, pos2, 2);

        return true;
    }
    public static bool PatchFsm_GurrTheOutcastTrapBenchControl(Fsm fsm)
    {
        GameObject.Destroy(fsm.GameObject);

        return true;
    }
    public static bool PatchFsm_GurrTheOutcastBossSceneControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var encountered = fsm.GetState("Encountered");
        var idle = fsm.GetState("Idle");
        var gateSfx =fsm.GetState("Gate Sfx");

        SetTransitionToState(init, encountered, 0);
        SetTransitionToState(init, encountered, 2);

        idle.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(TransitionPointInfo.eventName),
                ToFsmState = gateSfx
            }
        };

        return true;
    }
    public static bool PatchFsm_GurrTheOutcastCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        land.Actions = InsertInArray(land.Actions, customActionSendBossDeadEvent, 0);

        SetTransitionToState(stagger, blow, 0);

        land.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_LostGarmondControl(Fsm fsm)
    {
        var go = fsm.GameObject;
        var pos = go.transform.position;
        go.transform.position = new Vector3(pos.x + 12f, pos.y, pos.z);
        go.transform.localScale = new Vector3(1f, 1f, 1f);

        var init = fsm.GetState("Init");
        var introRoar = fsm.GetState("Intro Roar");
        var sting = fsm.GetState("Sting");

        ((Wait)sting.Actions[2]).time = 0.01f;

        return true;
    }
    public static bool PatchFsm_LostGarmondCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");
        var activateNPC = fsm.GetState("Activate NPC");

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        land.Actions = InsertInArray(land.Actions, customActionSendBossDeadEvent, 0);

        activateNPC.Actions = new FsmStateAction[0];

        SetTransitionToState(stagger, blow, 0);

        activateNPC.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_LostLaceIntroControl(Fsm fsm)
    {
        PlayerData.instance.EncounteredLostLace = true;

        var init = fsm.GetState("Init");
        var encountered = fsm.GetState("Encountered");
        var laceRe_emerge = fsm.GetState("Lace Re-emerge");
        var checkEncountered = fsm.GetState("Check Encountered");
        var laceRoar = fsm.GetState("Lace Roar");
        var silkScream = fsm.GetState("Silk Scream");
        var titleUp = fsm.GetState("Title Up");

        var laceAppearSpeed = 17;

        ((Wait)laceRe_emerge.Actions[11]).time = ((Wait)laceRe_emerge.Actions[11]).time.Value / laceAppearSpeed;
        ((Wait)laceRoar.Actions[4]).time = 0.1f;
        // ((Wait)silkScream.Actions[1]).time = 0.01f;
        ((Wait)titleUp.Actions[0]).time = 0.01f;

        SetTransitionToState(laceRoar, titleUp, 0);

        var customActionSpeedUpLaceAppearAnimation = new CustomLogicFsm(fsm);
        customActionSpeedUpLaceAppearAnimation.action += (Fsm fsm) =>
        {
            var animationObj = ((FindNamedChild)init.Actions[5]).storeResult.Value;
            var animationComponent = animationObj.GetComponent<Animator>();
            animationComponent.speed = laceAppearSpeed;
        };

        init.Actions = InsertInArray(init.Actions, customActionSpeedUpLaceAppearAnimation, init.Actions.Length - 1);

        return true;
    }
    public static bool PatchFsm_LostLaceBossTitle(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var titleUp = fsm.GetState("Title Up");

        ((Wait)titleUp.Actions[1]).time = 0.5f;

        return true;
    }
    public static bool PatchFsm_LostLaceGrandMother(Fsm fsm)
    {
        fsm.GameObject.SetActive(false);

        return true;
    }
    public static bool PatchFsm_LostLaceBossControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var stop = fsm.GetState("Stop");
        var midCocoonBreak = fsm.GetState("Mid Cocoon Break");
        var lockEnd = fsm.GetState("Lock End");
        var silkScream = fsm.GetState("Silk Scream");
        var silkFall = fsm.GetState("Silk Fall");
        var abyssWaveStartInit = fsm.GetState("Abyss Wave Start Init");
        var setRoarPos = fsm.GetState("Set Roar Pos");
        var wavePause = fsm.GetState("Wave Pause");
        var anticWave = fsm.GetState("Antic Wave");
        var p3Roar = fsm.GetState("P3 Roar");

        var laceAppearSpeed = 9;

        ((Wait)setRoarPos.Actions[5]).time = 0f;
        ((Wait)wavePause.Actions[3]).time = 0f;
        ((Wait)anticWave.Actions[5]).time = ((Wait)anticWave.Actions[5]).time.Value / laceAppearSpeed;
        ((Wait)p3Roar.Actions[5]).time = 0.5f;

        SetTransitionToState(stop, silkFall, 0);

        var customActionSpeedUpAnimation = new CustomLogicFsm(fsm);
        customActionSpeedUpAnimation.action += (Fsm fsm) =>
        {
            var animationObj = ((FindNamedChild)init.Actions[42]).storeResult.Value.transform.GetChild(0).gameObject;
            var animationComponent = animationObj.GetComponent<Animator>();
            animationComponent.speed = laceAppearSpeed;
            customActionSpeedUpAnimation.Finish();
        };

        init.Actions = InsertInArray(init.Actions, customActionSpeedUpAnimation, init.Actions.Length - 1);

        silkFall.Actions = new FsmStateAction[]{silkFall.Actions[0]};

        //remove silk mother scream
        wavePause.Actions = RemoveFromArray(wavePause.Actions, 2);

        return true;
    }
    public static bool PatchFsm_LostLaceDeathControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var midDeathSplash = fsm.GetState("Mid Death Splash");

        ((Wait)midDeathSplash.Actions[8]).time = 0.2f;

        return true;
    }
    public static bool PatchFsm_LostLaceDoorEntryControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var fallIn = fsm.GetState("Fall In");
        var land = fsm.GetState("Land");

        ((Wait)init.Actions[19]).time = 0f;

        fallIn.Actions = RemoveFromArray(fallIn.Actions, 7);

        land.Actions = RemoveFromArray(land.Actions, 1);
        land.Actions = RemoveFromArray(land.Actions, 1);
        land.Actions = RemoveFromArray(land.Actions, 2);

        return true;
    }
    public static bool PatchFsm_LostLaceCorpseControl(Fsm fsm)
    {
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var air = fsm.GetState("Air");
        var splashIn = fsm.GetState("Splash In");
        var end = fsm.GetState("End");
        var stagger = fsm.GetState("Stagger");

        ((Wait)steam.Actions[1]).time = 0.01f;

        var customActionSendEvent = new CustomLogicFsm(fsm);
        customActionSendEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
            customActionSendEvent.Finish();
        };

        end.Actions = InsertInArray(end.Actions, customActionSendEvent, 0);

        SetTransitionToState(stagger, blow, 0);

        return true;
    }
    public static bool PatchFsm_NylethBossSceneControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var unencountered = fsm.GetState("Unencountered");
        var scream = fsm.GetState("Scream");
        var floorBreak = fsm.GetState("Floor Break");
        var roofUp = fsm.GetState("Roof Up");

        ((Wait)scream.Actions[9]).time = 0.1f;
        ((Wait)floorBreak.Actions[8]).time = 0.3f;
        ((Wait)roofUp.Actions[6]).time = 0f;

        SetTransitionToState(init, unencountered, 2);

        unencountered.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(TransitionPointInfo.eventName),
                ToFsmState = scream
            }
        };

        var customActionDisableTerrainIntro = new CustomLogicFsm(fsm);
        customActionDisableTerrainIntro.action += (Fsm fsm) =>
        {
            var terrainIntroObj = fsm.Variables.FindFsmGameObject("Terrain Intro").Value;
            terrainIntroObj.SetActive(false);
        };

        init.Actions = InsertInArray(init.Actions, customActionDisableTerrainIntro, 5);

        return true;
    }
    public static bool PatchFsm_NylethCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        blow.Actions = InsertInArray(blow.Actions, customActionSendBossDeadEvent, 0);

        SetTransitionToState(stagger, blow, 0);

        land.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_PalestagControl(Fsm fsm)
    {
        fsm.GameObject.transform.localScale = new Vector3(1, 1, 1);

        var init = fsm.GetState("Init");
        var roar = fsm.GetState("Roar");
        var rest = fsm.GetState("Rest");

        rest.Transitions[0].FsmEvent = FsmEvent.GetFsmEvent(TransitionPointInfo.eventName);

        ((Wait)roar.Actions[11]).time = 0.1f;

        return true;
    }
    public static bool PatchFsm_PalestagCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var disappear = fsm.GetState("Disappear");

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        blow.Actions = InsertInArray(blow.Actions, customActionSendBossDeadEvent, 0);

        SetTransitionToState(stagger, blow, 0);

        disappear.Actions = RemoveFromArray(disappear.Actions, 6);
        disappear.Actions = RemoveFromArray(disappear.Actions, 5);

        return true;
    }
    public static bool PatchFsm_PinstressBossControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var roarAntic = fsm.GetState("Roar Antic");
        var roar = fsm.GetState("Roar");
        var recover = fsm.GetState("Recover");
        var recoverEnd = fsm.GetState("Recover End");
        var groundTeleReturn = fsm.GetState("Ground Tele Return");

        ((Wait)roar.Actions[3]).time = 0.1f;
        ((Wait)recover.Actions[1]).time = 0f;

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        recoverEnd.Actions = InsertInArray(recoverEnd.Actions, customActionSendBossDeadEvent, 0);

        recoverEnd.Actions = RemoveFromArray(recoverEnd.Actions, 5);
        recoverEnd.Actions = RemoveFromArray(recoverEnd.Actions, 4);
        roarAntic.Actions = RemoveFromArray(roarAntic.Actions, 3);

        recoverEnd.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_PinstressControl(Fsm fsm)
    {
        var pause = fsm.GetState("Pause");
        var check = fsm.GetState("Check");
        var pinstress = fsm.GetState("Pinstress");

        SetTransitionToState(check, pinstress, 1);

        return true;
    }
    public static bool PatchFsm_PinstressNPCControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var snowSleep = fsm.GetState("Snow Sleep");
        var wake3 = fsm.GetState("Wake 3");
        var battleStart = fsm.GetState("Battle Start");

        ((Wait)wake3.Actions[3]).time = 0;

        var customActionCreateTriggerForStart = new CustomLogicFsm(fsm);
        customActionCreateTriggerForStart.action += (Fsm fsm) =>
        {
            var pos = fsm.GameObject.transform.position;

            var customTrigger = CreateTrigger("Peak_07");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(pos.x, pos.y, pos.z);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                fsm.FsmComponent.SendEvent("TEST");
            };
        };

        GameObject.Destroy(fsm.FsmComponent.gameObject.GetComponent<PlayMakerNPC>());

        snowSleep.Actions = InsertInArray(snowSleep.Actions, customActionCreateTriggerForStart, snowSleep.Actions.Length);

        SetTransitionToState(wake3, battleStart, 0);

        return true;
    }
    public static bool PatchFsm_PlasmifiedZango(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var roar = fsm.GetState("Roar");
        var rest = fsm.GetState("Rest");
        var walkSlow = fsm.GetState("Walk Slow");
        var recordJournalKill = fsm.GetState("Record Journal Kill");

        ((Wait)roar.Actions[5]).time = 0.1f;

        ((SetScale)rest.Actions[1]).x = -1;
        ((WalkLeftRight)walkSlow.Actions[0]).startLeft = true;
        // fsm.GameObject.transform.localScale = new Vector3(-1, 1, 1);

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        SetTransitionToState(init, rest, 1);

        recordJournalKill.Actions = InsertInArray(recordJournalKill.Actions, customActionSendBossDeadEvent, 0);

        var pos = fsm.GameObject.transform.position;
        fsm.GameObject.transform.position = new Vector3(pos.x + 10, pos.y, pos.z);

        return true;
    }
    public static bool PatchFsm_SethControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var dormant = fsm.GetState("Dormant");
        var wakeAntic = fsm.GetState("Wake Antic");
        var roar = fsm.GetState("Roar");

        ((Wait)roar.Actions[6]).time = 0.1f;

        dormant.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(TransitionPointInfo.eventName),
                ToFsmState = wakeAntic
            }
        };

        return true;
    }
    public static bool PatchFsm_SethCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var splashIn = fsm.GetState("Splash In");

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        splashIn.Actions = InsertInArray(splashIn.Actions, customActionSendBossDeadEvent, 0);

        SetTransitionToState(stagger, blow, 0);

        splashIn.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_SkarrsingerKarmelitaBossControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var challengePause = fsm.GetState("Challenge Pause");
        var launchInAntic = fsm.GetState("Launch In Antic");
        var roar = fsm.GetState("Roar");

        ((Wait)challengePause.Actions[1]).time = 0;
        ((Wait)roar.Actions[5]).time = 0.1f;

        SetTransitionToState(challengePause, launchInAntic, 0);

        return true;
    }
    public static bool PatchFsm_SkarrsingerKarmelitaChallengeRegion(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var idle = fsm.GetState("Idle");
        var inRegion = fsm.GetState("In Region");
        var hornetVoice = fsm.GetState("Hornet Voice");
        var challenge2 = fsm.GetState("Challenge 2");

        ((Wait)challenge2.Actions[1]).time = 0;

        SetTransitionToState(idle, hornetVoice, 0);

        return true;
    }
    public static bool PatchFsm_SkarrsingerKarmelitaCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var steam = fsm.GetState("Steam");
        var blow = fsm.GetState("Blow");
        var land = fsm.GetState("Land");

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        land.Actions = InsertInArray(land.Actions, customActionSendBossDeadEvent, 0);

        SetTransitionToState(stagger, blow, 0);

        land.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_TormentedTrobbioControl(Fsm fsm)
    {
        var init = fsm.GetState("Init");
        var wait = fsm.GetState("Wait");
        var state = fsm.GetState("State");
        var startPause = fsm.GetState("Start Pause");
        var fogStart = fsm.GetState("Fog Start");
        var drumFade = fsm.GetState("Drum Fade");
        var trobbioRise = fsm.GetState("Trobbio Rise");
        var riseEnd = fsm.GetState("Rise End");

        ((ConvertBoolToFloat)trobbioRise.Actions[6]).falseValue = 0.3f;
        ((ConvertBoolToFloat)trobbioRise.Actions[6]).trueValue = 0.3f;
        ((ConvertBoolToFloat)fogStart.Actions[6]).falseValue = 0f;
        ((ConvertBoolToFloat)fogStart.Actions[6]).trueValue = 0f;
        ((FadeAudio)fogStart.Actions[1]).time = 0.2f;
        ((ConvertBoolToFloat)riseEnd.Actions[1]).trueValue = 0f;
        ((ConvertBoolToFloat)riseEnd.Actions[1]).falseValue = 0f;

        startPause.Actions = RemoveFromArray(startPause.Actions, 0);
        riseEnd.Actions = RemoveFromArray(riseEnd.Actions, 3);

        SetTransitionToState(state, startPause, 0);

        wait.Transitions = new FsmTransition[]
        {
            new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(TransitionPointInfo.eventName),
                ToFsmState = state
            }
        };

        return true;
    }
    public static bool PatchFsm_TormentedTrobbioCorpseControl(Fsm fsm)
    {
        var stagger = fsm.GetState("Stagger");
        var blow = fsm.GetState("Blow");
        var doSpin = fsm.GetState("Do Spin");
        var interactable = fsm.GetState("Interactable");
        var deathPose2 = fsm.GetState("Death Pose 2");
        var deathStream = fsm.GetState("Death Stream");
        var leaveShake = fsm.GetState("Leave Shake");
        var leave = fsm.GetState("Leave");
        var leaveEnd = fsm.GetState("Leave End");

        float speed = 3f;

        ((Wait)deathStream.Actions[4]).time = ((Wait)deathStream.Actions[4]).time.Value / speed;
        ((Wait)leaveShake.Actions[1]).time = ((Wait)leaveShake.Actions[1]).time.Value / speed;
        ((AnimatePositionBy)leave.Actions[0]).time = ((AnimatePositionBy)leave.Actions[0]).time.Value / speed;
        ((Wait)leaveEnd.Actions[5]).time = 0.1f;

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        leaveEnd.Actions = InsertInArray(leaveEnd.Actions, customActionSendBossDeadEvent, 0);

        leaveEnd.Actions = RemoveFromArray(leaveEnd.Actions, 7);
        leaveEnd.Actions = RemoveFromArray(leaveEnd.Actions, 6);
        interactable.Actions = RemoveFromArray(interactable.Actions, 3);

        SetTransitionToState(stagger, blow, 0);

        leaveEnd.Transitions = new FsmTransition[0];

        return true;
    }
    public static bool PatchFsm_WatcherAtTheEdgeControl(Fsm fsm)
    {
        var pos = fsm.GameObject.transform.position;
        fsm.GameObject.transform.position = new Vector3(pos.x + 10, pos.y, pos.z);


        var init = fsm.GetState("Init");
        var startState = fsm.GetState("Start State");
        var sleep = fsm.GetState("Sleep");
        var wakeAntic = fsm.GetState("Wake Antic");
        var wakeRoar1 = fsm.GetState("Wake Roar 1");
        var wakeRoar2 = fsm.GetState("Wake Roar 2");

        ((Wait)wakeAntic.Actions[2]).time = 0.1f;
        ((Wait)wakeRoar2.Actions[5]).time = 0.1f;

        SetTransitionToState(startState, sleep, 1);
        SetTransitionToState(startState, sleep, 2);

        var customActionStartTrigger = new CustomLogicFsm(fsm);
        customActionStartTrigger.action += (Fsm fsm) =>
        {
            var pos = fsm.GameObject.transform.position;

            var customTrigger = CreateTrigger("Coral_39");
            var triggerPos = customTrigger.transform.position;
            customTrigger.transform.position = new Vector3(pos.x, pos.y, pos.z);

            var triggerComponent = customTrigger.AddComponent<CustomTrigger>();
            triggerComponent.fsm = fsm;

            triggerComponent.action += (Fsm fsm, FsmStateAction fsmAction) =>
            {
                fsm.FsmComponent.SendEvent("WAKE");
            };
        };

        sleep.Actions = InsertInArray(sleep.Actions, customActionStartTrigger, sleep.Actions.Length);

        return true;
    }
    public static bool PatchFsm_WatcherAtTheEdgeBattleMusic(Fsm fsm)
    {
        var state1 = fsm.GetState("State 1");
        var musicPause = fsm.GetState("Music Pause");
        var music = fsm.GetState("Music");

        ((TransitionToAudioSnapshot)music.Actions[1]).transitionTime = 0.5f;

        SetTransitionToState(state1, music, 0);

        return true;
    }
    public static bool PatchFsm_WatcherAtTheEdgeCorpseControl(Fsm fsm)
    {
        var land = fsm.GetState("Land");
        var eaten = fsm.GetState("Eaten");
        var roarAntic = fsm.GetState("Roar Antic");
        var roar = fsm.GetState("Roar");
        var crustPause = fsm.GetState("Crust Pause");
        var crustUp = fsm.GetState("Crust Up");
        var fadeAway = fsm.GetState("Fade Away");
        var dropSword = fsm.GetState("Drop Sword");

        ((Wait)roar.Actions[0]).time = 0.1f;
        ((Wait)fadeAway.Actions[2]).time = 0.1f;
        ((Wait)land.Actions[7]).time = 0.1f;

        var customActionSendBossDeadEvent = new CustomLogicFsm(fsm, BossInfo.waitForBossDeathAnim, true);
        customActionSendBossDeadEvent.action += (Fsm fsm) =>
        {
            PlayMakerFSM.BroadcastEvent(bossDeadEvent);
        };

        dropSword.Actions = InsertInArray(dropSword.Actions, customActionSendBossDeadEvent, 0);
        eaten.Actions = InsertInArray(eaten.Actions, customActionSendBossDeadEvent, 0);

        crustPause.Actions = RemoveFromArray(crustPause.Actions, 0);

        return true;
    }
}