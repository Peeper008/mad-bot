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

            // Checks if the battleFile file exists, and if it doesn't, runs this block of code.
            if (!File.Exists(battleFolder + "/" + battleFile))
            {
                // Creates a new variable and fills it with the default information for the new file.
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

                // Converts the variable into json information and puts it in a file.
                string json = JsonConvert.SerializeObject(stats, Formatting.Indented);
                File.WriteAllText(battleFolder + "/" + battleFile, json);
            }
            else
            {
                // If the battleFile file does exist, it sets the stats variable to the information in the file.
                string json = File.ReadAllText(battleFolder + "/" + battleFile);
                stats = JsonConvert.DeserializeObject<BattleInfo>(json);
            }
        }

        public static void SaveStats()
        {
            // Converts the stats variable to json information and saves it in the battleFile file.
            string json = JsonConvert.SerializeObject(stats, Formatting.Indented);
            File.WriteAllText(battleFolder + "/" + battleFile, json);
        }

        public static bool HasItem(string item)
        {
            // Loops through each item in the Items list and checks for string provided.
            foreach (string i in stats.Items)
            {
                if (i.ToLower() == item.ToLower())
                {
                    // If found, return true.
                    return true;
                } 
            }
            // If not found, return false.
            return false;
        }

        public static bool HasEnemy(string enemy)
        {
            // Loops through each enemy in the Enemies list and checks for string provided.
            foreach (string e in stats.Enemies)
            {
                if (e.ToLower() == enemy.ToLower())
                {
                    // If found, return true.
                    return true;
                }
            }
            // If not found, return false.
            return false;
        }
        
    }

    public struct BattleInfo
    {
        public List<string> Items { get; set; }

        public List<string> Enemies { get; set; }

        public string CurrentEnemy { get; set; }

        public string Name { get; set; }

        public int EnemyHP { get; set; }

        public bool InFight { get; set; }
    }
}
