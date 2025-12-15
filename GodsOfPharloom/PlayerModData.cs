using System.Runtime.Serialization.Formatters.Binary;
using HarmonyLib;

namespace Gods_Of_Pharloom
{
    [Serializable]
    public class PlayerDataMod
    {
        public static PlayerDataMod instance;
        string path = "GodsOfPharloomData.bin";
        public Dictionary<string, Badges> badges;

        public PlayerDataMod()
        {
            instance = this;
        }
        public void LoadData()
        {
            if (!File.Exists(BepInEx.Paths.ConfigPath + "/" + path))
            {
                SaveData(firstTime: true);

                return;
            }
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fs = new FileStream(BepInEx.Paths.ConfigPath + "/" + path, FileMode.Open))
            {
                try
                {
                    PlayerDataMod.instance = (PlayerDataMod)formatter.Deserialize(fs);
                }
                catch(Exception ex)
                {
                    SaveData();
                    return;
                }
            }
        }
        public void SaveData(bool firstTime = false)
        {
            if (firstTime)
            {
                var badges = new Dictionary<string, Badges>();
                for(int i = 0; i < BossStatueInfo.bossStatues.Length; i++)
                {
                    string bossName = BossStatueInfo.bossStatues[i].boss.bossName;
                    badges[bossName] = new Badges(bossName);
                }
                this.badges = badges;
            }
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fs = new FileStream(BepInEx.Paths.ConfigPath + "/" + path, FileMode.Create))
            {
                formatter.Serialize(fs, instance);
            }
        }
    }
}