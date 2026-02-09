using System.Runtime.Serialization.Formatters.Binary;

namespace Gods_Of_Pharloom
{
    [Serializable]
    public class PantheonInfo
    {
        public bool completedPantheon;
        public bool completedNeedleBinding = false;
        public bool completedSilkBinding = false;
        public bool completedToolsBinding = false;
        public bool completedMaskBinding = false;
        public bool completedNoHit = false;
        public bool completedAllBindings = false;
        public bool completedAllBindingsNoHit = false;
    }
    [Serializable]
    public class PlayerDataMod
    {
        public static PlayerDataMod instance;
        public Dictionary<string, Badges> badges;

        //bindings
        public Dictionary<string, bool> bindings = new Dictionary<string, bool>
        {
            {"Needle Binding", false},
            {"Silk Binding", false},
            {"Tools Binding", false},
            {"Mask Binding", false},
        };

        public Dictionary<string, PantheonInfo> pantheonsInfo = new Dictionary<string, PantheonInfo>
        {
            {"Pantheon 1", new PantheonInfo()},
            {"Pantheon 2", new PantheonInfo()},
            {"Pantheon 3", new PantheonInfo()},
            {"Pantheon 4", new PantheonInfo()},
            {"Pantheon 5", new PantheonInfo()},
        };

        public int previousHealthCount = 10;
        public int previousSilkSpoolCount = 18;

        public PlayerDataMod()
        {
            var newBadges = new Dictionary<string, Badges>();
            for (int i = 0; i < BossStatueInfo.bossStatues.Length; i++)
            {
                string bossName = BossStatueInfo.bossStatues[i].boss.bossName;
                newBadges[bossName] = new Badges(bossName);
            }
            badges = newBadges;

            instance = this;
        }
    }
}