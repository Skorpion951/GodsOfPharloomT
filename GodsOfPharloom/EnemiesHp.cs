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

public class EnemyHp
{
    public class PhaseHp
    {
        public Dictionary<string, int> hpDict = new Dictionary<string, int>();
        public int hp
        {
            get
            {
                return hpDict[Gods_Of_Pharloom.BossSequence.currentDifficultMode];
            }
        }

        public PhaseHp(int attuned, int ascended, int radiant)
        {
            hpDict["Attuned"] = attuned;
            hpDict["Ascended"] = ascended;
            hpDict["Radiant"] = radiant;
        }
    }
    public static Dictionary<string, EnemyHp> enemies = new Dictionary<string, EnemyHp>()
    {
        {"Moss Mother", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(50, 60, 60),
            new PhaseHp(50, 60, 60),
            new PhaseHp(150, 300, 300),
            new PhaseHp(300, 500, 500),
        })},
        {"Bell Beast", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(50, 100, 100),
            new PhaseHp(100, 200, 200),
            new PhaseHp(600, 900, 900),
        })},
        {"Fourth Chorus", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(320, 440, 440),
            new PhaseHp(600, 700, 700),
            new PhaseHp(600, 800, 800),
            new PhaseHp(400, 500, 500),
        })},
        {"Great Conchflies", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(150, 100, 100),
            new PhaseHp(800, 1400, 1400),
        })},
        {"Lace in Deep Docks", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(300, 300, 300),
            new PhaseHp(950, 1200, 1200),
        })},
        {"The Last Judge", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(300, 400, 400),
            new PhaseHp(600, 900, 900),
            new PhaseHp(500, 600, 600),
        })},
        {"Phantom", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(200, 300, 300),
            new PhaseHp(400, 500, 500),
            new PhaseHp(700, 900, 900),
        })},
        {"Moorwing", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(300, 400, 400),
            new PhaseHp(900, 1100, 1100),
        })},
        {"Savage Beastfly in Chapel of The Beast", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(200, 300, 300),
            new PhaseHp(700, 850, 850),
            new PhaseHp(400, 500, 500),
        })},
        {"Beastfly", new EnemyHp(new PhaseHp[] //savage beastly1 summon
        {
            new PhaseHp(18, 60, 60),
        })},
        {"Kilik", new EnemyHp(new PhaseHp[] //savage beastly1 summon
        {
            new PhaseHp(26, 70, 70),
        })},
        {"Vicious Caranid", new EnemyHp(new PhaseHp[] //savage beastly1 summon
        {
            new PhaseHp(50, 100, 100),
        })},
        {"Sister Splinter", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(250, 300, 300),
            new PhaseHp(600, 700, 700),
            new PhaseHp(200, 400, 400),
        })},
        {"Splinterbark", new EnemyHp(new PhaseHp[] //sister slinter summon
        {
            new PhaseHp(18, 40, 40),
        })},
        {"Skull Tyrant", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(1000, 1400, 1400),
        })},
        {"Widow", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(300, 400, 400),
            new PhaseHp(500, 600, 600),
            new PhaseHp(800, 900, 900),
        })},
        {"Cogwork Dancers", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(400, 600, 600),
            new PhaseHp(650, 750, 750),
            new PhaseHp(650, 750, 750),
            new PhaseHp(250, 300, 300),
        })},
        {"Disgraced Chef Lugoli", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(600, 700, 700),
            new PhaseHp(900, 1100, 1100),
        })},
        {"Father of the Flame", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(400, 600, 600), //brazier BL
            new PhaseHp(400, 600, 600), //brazier TL
            new PhaseHp(400, 600, 600), //brazier TR
            new PhaseHp(400, 600, 600), //brazier BR
            new PhaseHp(800, 1200, 1200), //phase 2
            new PhaseHp(600, 800, 800), //core
        })},
        {"First Sinner", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(400, 500, 500),
            new PhaseHp(850, 1000, 1000),
            new PhaseHp(850, 1000, 1000),
        })},
        {"Forebrothers_Sigins", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(200, 300, 300),
            new PhaseHp(400, 500, 500),
            new PhaseHp(400, 600, 600),
            new PhaseHp(600, 700, 700),
        })},
        {"Forebrothers_Gron", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(1400, 1600, 1600),
        })},
        {"Flintstone Flyer", new EnemyHp(new PhaseHp[] //forebrothers' summon
        {
            new PhaseHp(36, 70, 70),
        })},
        {"Smokerock Sifter", new EnemyHp(new PhaseHp[] //forebrothers' summon
        {
            new PhaseHp(47, 90, 90),
        })},
        {"Garmond & Zaza", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(1000, 1300, 1300),
        })},
        {"Voltvyrm", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(200, 200, 200),
            new PhaseHp(600, 700, 700),
            new PhaseHp(900, 1100, 1100),
        })},
        {"Groal the Great", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(500, 500, 500),
            new PhaseHp(1500, 1900, 1900),
        })},
        {"Lace in the Cradle", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(500, 500, 500),
            new PhaseHp(500, 700, 700),
            new PhaseHp(800, 1000, 1000),
        })},
        {"Raging Conchfly", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(500, 700, 700),
            new PhaseHp(1100, 1300, 1300),
        })},
        {"Savage Beastfly in Far Fields", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(300, 400, 400),
            new PhaseHp(400, 600, 600),
            new PhaseHp(900, 1000, 1000),
        })},
        {"Tarmite", new EnemyHp(new PhaseHp[] //savage beastly2 summon
        {
            new PhaseHp(34, 70, 70),
        })},
        {"Second Sentiel", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(800, 900, 900),
            new PhaseHp(800, 1000, 1000),
        })},
        {"Shakra", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(1400, 1800, 1800),
        })},
        {"The Unravelled", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(300, 400, 400),
            new PhaseHp(850, 1000, 1000),
            new PhaseHp(850, 1000, 1000),
        })},
        {"Trobbio", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(700, 700, 700),
            new PhaseHp(700, 1100, 1100),
        })},
        {"Palestag", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(700, 700, 700),
            new PhaseHp(400, 700, 700),
        })},
        {"Bell Eater", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(900, 1000, 1000), //head hp
            new PhaseHp(900, 1000, 1000), //butt hp
            new PhaseHp(400, 400, 400), //phase 1 hp
            new PhaseHp(700, 700, 700), //phase 2 hp
        })},
        {"Crawfather", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(1100, 1000, 1000),
            new PhaseHp(1200, 1600, 1600),
        })},
        {"Pin Wielder Craw", new EnemyHp(new PhaseHp[] //crawfather summon
        {
            new PhaseHp(66, 120, 120),
        })},
        {"Dagger Craw", new EnemyHp(new PhaseHp[] //crawfather summon
        {
            new PhaseHp(66, 120, 120),
        })},
        {"Tinie Craw", new EnemyHp(new PhaseHp[] //crawfather summon
        {
            new PhaseHp(60, 100, 100),
        })},
        {"Crust King Khann", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(700, 700, 700),
            new PhaseHp(700, 700, 700),
            new PhaseHp(1100, 1100, 1100),
        })},
        {"Gurr the Outcast", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(300, 400, 400),
            new PhaseHp(700, 800, 800),
            new PhaseHp(600, 700, 700),
        })},
        {"Lost Garmond", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(1300, 1600, 1600),
        })},
        {"Nyleth", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(700, 700, 700),
            new PhaseHp(1000, 1000, 1000),
        })},
        {"Watcher at the Edge", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(750, 750, 750),
            new PhaseHp(750, 950, 950),
        })},
        {"Pinstress", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(200, 400, 400),
            new PhaseHp(1250, 1250, 1250),
        })},
        {"Plasmified Zango", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(100, 100, 100),
            new PhaseHp(300, 350, 350),
            new PhaseHp(300, 400, 400),
            new PhaseHp(300, 400, 400),
            new PhaseHp(400, 500, 500),
        })},
        {"Shrine Guardian Seth", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(200, 350, 350),
            new PhaseHp(700, 850, 850),
            new PhaseHp(650, 700, 700),
        })},
        {"Skarrsinger Karmelita", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(450, 450, 450),
            new PhaseHp(700, 700, 700),
            new PhaseHp(900, 900, 900),
        })},
        {"Clover Dancers", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(800, 1000, 1000),
            new PhaseHp(800, 1000, 1000),
        })},
        {"Tormented Trobbio", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(100, 200, 200),
            new PhaseHp(600, 800, 800),
            new PhaseHp(800, 800, 800),
        })},
        {"Grand Mother Silk", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(200, 200, 200),
            new PhaseHp(300, 300, 300),
            new PhaseHp(400, 400, 400),
            new PhaseHp(430, 350, 350),
            new PhaseHp(450, 400, 400),
            new PhaseHp(500, 500, 500),
        })},
        {"Lost Lace", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(500, 500, 500),
            new PhaseHp(700, 700, 700),
            new PhaseHp(650, 650, 650),
            new PhaseHp(700, 700, 700),
        })},
    };
    public Dictionary<string, int> hpFullDict = new Dictionary<string, int>()
    {
        {"Attuned", 0},
        {"Ascended", 0},
        {"Radiant", 0},
    };
    public PhaseHp[] phases;
    public EnemyHp(PhaseHp[] phases)
    {
        this.phases = phases;
        foreach(var phase in phases)
        {
            hpFullDict["Attuned"] += phase.hpDict["Attuned"];
            hpFullDict["Ascended"] += phase.hpDict["Ascended"];
            hpFullDict["Radiant"] += phase.hpDict["Radiant"];
        }
    }
}