using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mad_Bot_Discord
{
    public class BattleStats
    {
        public static BattleInfo stats;

        static string battleFolder = "Resources";
        static string battleFile = "battleInfo.json";

        static BattleStats()
        {
            if (!Directory.Exists(battleFolder))
                Directory.CreateDirectory(battleFolder);

            if (!File.Exists(battleFolder + "/" + battleFile))
            {
                stats = new BattleInfo()
                {
                    Name = "M.A.D Bot",
                    Items = new List<string>()
                    {
                        "Fists"
                    },
                    Enemies = new List<string>()
                    {
                        "Air"
                    }
                };

                string json = JsonConvert.SerializeObject(stats, Formatting.Indented);
                File.WriteAllText(battleFolder + "/" + battleFile, json);
            }
            else
            {
                string json = File.ReadAllText(battleFolder + "/" + battleFile);
                stats = JsonConvert.DeserializeObject<BattleInfo>(json);
            }
        }

        public static void SaveStats()
        {
            string json = JsonConvert.SerializeObject(stats, Formatting.Indented);
            File.WriteAllText(battleFolder + "/" + battleFile, json);
        }

        public static bool HasItem(string item)
        {
            foreach (string i in stats.Items)
            {
                if (i.ToLower() == item.ToLower())
                {
                    return true;
                } 
            }
            return false;
        }

        public static bool HasEnemy(string enemy)
        {
            foreach (string e in stats.Enemies)
            {
                if (e.ToLower() == enemy.ToLower())
                {
                    return true;
                }
            }
            return false;
        }
        
    }

    public struct BattleInfo
    {
        public List<string> Items { get; set; }

        public List<string> Enemies { get; set; }

        public string Name { get; set; }
    }
}
