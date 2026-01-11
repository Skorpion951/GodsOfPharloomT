using System.Runtime.Serialization.Formatters.Binary;

namespace Gods_Of_Pharloom
{
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

        public int previousNeedleUpgrade;
        public int previousHealthCount = 10;

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