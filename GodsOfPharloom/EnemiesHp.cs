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
            new PhaseHp(100, 100, 100),
            new PhaseHp(100, 100, 100),
            new PhaseHp(150, 150, 150),
            new PhaseHp(100, 100, 100),
        })},
        {"Moss Mother Double 1", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(100, 100, 100),
            new PhaseHp(150, 150, 150),
            new PhaseHp(200, 200, 200),
            new PhaseHp(100, 100, 100),
        })},
        {"Moss Mother Double 2", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(100, 100, 100),
            new PhaseHp(150, 150, 150),
            new PhaseHp(200, 200, 200),
            new PhaseHp(100, 100, 100),
        })},
        {"Bell Beast", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(200, 200, 200),
            new PhaseHp(250, 250, 250),
            new PhaseHp(150, 150, 150),
        })},
        {"Fourth Chorus", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(250, 250, 250),
            new PhaseHp(250, 250, 250),
            new PhaseHp(250, 250, 250),
            new PhaseHp(250, 250, 250),
        })},
        {"Great Conchflies", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(500, 500, 500),
            new PhaseHp(300, 300, 300),
        })},
        {"Lace in Deep Docks", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(600, 600, 600),
            new PhaseHp(300, 300, 300),
        })},
        {"The Last Judge", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(500, 500, 500),
            new PhaseHp(500, 500, 500),
            new PhaseHp(400, 400, 400),
        })},
        {"Phantom", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(300, 300, 300),
            new PhaseHp(400, 400, 400),
            new PhaseHp(300, 300, 300),
        })},
        {"Moorwing", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(500, 500, 500),
            new PhaseHp(300, 300, 300),
        })},
        {"Savage Beastfly in Chapel of The Beast", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(500, 500, 500),
            new PhaseHp(350, 350, 350),
            new PhaseHp(250, 250, 250),
        })},
        {"Beastfly", new EnemyHp(new PhaseHp[] //savage beastfly1 summon
        {
            new PhaseHp(18, 18, 18),
        })},
        {"Kilik", new EnemyHp(new PhaseHp[] //savage beastfly1 summon
        {
            new PhaseHp(26, 26, 26),
        })},
        {"Vicious Caranid", new EnemyHp(new PhaseHp[] //savage beastfly1 summon
        {
            new PhaseHp(50, 50, 50),
        })},
        {"Sister Splinter", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(200, 200, 200),
            new PhaseHp(400, 400, 400),
            new PhaseHp(200, 200, 200),
        })},
        {"Splinterbark", new EnemyHp(new PhaseHp[] //sister slinter summon
        {
            new PhaseHp(18, 18, 18),
        })},
        {"Skull Tyrant", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(800, 800, 800),
        })},
        {"Widow", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(300, 300, 300),
            new PhaseHp(400, 400, 400),
            new PhaseHp(550, 550, 550),
        })},
        {"Cogwork Dancers", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(250, 250, 250),
            new PhaseHp(250, 250, 250),
            new PhaseHp(250, 250, 250),
            new PhaseHp(100, 100, 100),
        })},
        {"Disgraced Chef Lugoli", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(400, 400, 400),
            new PhaseHp(350, 350, 350),
        })},
        {"Father of the Flame", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(100, 100, 100), //brazier BL
            new PhaseHp(100, 100, 100), //brazier TL
            new PhaseHp(100, 100, 100), //brazier TR
            new PhaseHp(100, 100, 100), //brazier BR
            new PhaseHp(200, 200, 200), //phase 2
            new PhaseHp(250, 250, 250), //core
        })},
        {"First Sinner", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(300, 300, 300),
            new PhaseHp(500, 500, 500),
            new PhaseHp(500, 500, 500),
        })},
        {"Forebrothers_Sigins", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(100, 100, 100),
            new PhaseHp(300, 300, 300),
            new PhaseHp(300, 300, 300),
            new PhaseHp(200, 200, 200),
        })},
        {"Forebrothers_Gron", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(650, 650, 650),
        })},
        {"Flintstone Flyer", new EnemyHp(new PhaseHp[] //forebrothers' summon
        {
            new PhaseHp(36, 36, 36),
        })},
        {"Smokerock Sifter", new EnemyHp(new PhaseHp[] //forebrothers' summon
        {
            new PhaseHp(47, 47, 47),
        })},
        {"Garmond & Zaza", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(500, 500, 500),
        })},
        {"Voltvyrm", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(300, 300, 300),
            new PhaseHp(200, 200, 200),
            new PhaseHp(200, 200, 200),
        })},
        {"Groal the Great", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(450, 450, 450),
            new PhaseHp(350, 350, 350),
        })},
        {"Lace in the Cradle", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(500, 500, 500),
            new PhaseHp(400, 400, 400),
            new PhaseHp(200, 200, 200),
        })},
        {"Raging Conchfly", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(700, 700, 700),
            new PhaseHp(400, 400, 400),
        })},
        {"Savage Beastfly in Far Fields", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(500, 500, 500),
            new PhaseHp(500, 500, 500),
            new PhaseHp(300, 300, 300),
        })},
        {"Tarmite", new EnemyHp(new PhaseHp[] //savage beastfly2 summon
        {
            new PhaseHp(34, 34, 34),
        })},
        {"Second Sentiel", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(700, 700, 700),
            new PhaseHp(600, 600, 600),
        })},
        {"Shakra", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(700, 700, 700),
        })},
        {"The Unravelled", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(400, 400, 400),
            new PhaseHp(400, 400, 400),
            new PhaseHp(200, 200, 200),
        })},
        {"Trobbio", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(700, 700, 700),
            new PhaseHp(500, 500, 500),
        })},
        {"Palestag", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(250, 250, 250),
            new PhaseHp(250, 250, 250),
        })},
        {"Bell Eater", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(400, 400, 400), //head hp
            new PhaseHp(400, 400, 400), //butt hp
            new PhaseHp(300, 300, 300), //phase 1 hp
            new PhaseHp(200, 200, 200), //phase 2 hp
        })},
        {"Crawfather", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(900, 900, 900),
            new PhaseHp(700, 700, 700),
        })},
        {"Pin Wielder Craw", new EnemyHp(new PhaseHp[] //crawfather summon
        {
            new PhaseHp(66, 66, 66),
        })},
        {"Dagger Craw", new EnemyHp(new PhaseHp[] //crawfather summon
        {
            new PhaseHp(66, 66, 66),
        })},
        {"Tinie Craw", new EnemyHp(new PhaseHp[] //crawfather summon
        {
            new PhaseHp(55, 55, 55),
        })},
        {"Crust King Khann", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(600, 600, 600),
            new PhaseHp(600, 600, 600),
            new PhaseHp(450, 450, 450),
        })},
        {"Gurr the Outcast", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(400, 400, 400),
            new PhaseHp(350, 350, 350),
            new PhaseHp(250, 250, 250),
        })},
        {"Lost Garmond", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(900, 900, 900),
        })},
        {"Nyleth", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(625, 625, 625),
            new PhaseHp(625, 625, 625),
        })},
        {"Watcher at the Edge", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(450, 450, 450),
            new PhaseHp(450, 450, 450),
        })},
        {"Pinstress", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(500, 500, 500),
            new PhaseHp(500, 500, 500),
        })},
        {"Plasmified Zango", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(200, 200, 200),
            new PhaseHp(200, 200, 200),
            new PhaseHp(200, 200, 200),
            new PhaseHp(200, 200, 200),
            new PhaseHp(200, 200, 200),
        })},
        {"Shrine Guardian Seth", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(200, 350, 350),
            new PhaseHp(700, 850, 850),
            new PhaseHp(650, 700, 700),
        })},
        {"Skarrsinger Karmelita", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(600, 600, 600),
            new PhaseHp(600, 600, 600),
            new PhaseHp(400, 400, 400),
        })},
        {"Clover Dancers", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(600, 600, 600),
            new PhaseHp(600, 600, 600),
        })},
        {"Tormented Trobbio", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(150, 150, 150),
            new PhaseHp(800, 800, 800),
            new PhaseHp(550, 550, 550),
        })},
        {"Grand Mother Silk", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(150, 150, 150),
            new PhaseHp(250, 250, 250),
            new PhaseHp(300, 300, 300),
            new PhaseHp(200, 200, 200),
            new PhaseHp(300, 300, 300),
            new PhaseHp(400, 400, 400),
        })},
        {"Lost Lace", new EnemyHp(new PhaseHp[]
        {
            new PhaseHp(400, 400, 400),
            new PhaseHp(500, 500, 500),
            new PhaseHp(700, 700, 700),
            new PhaseHp(600, 600, 600),
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