using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Mad_Bot_Discord
{
    class Config
    {
        private const string configFolder = "Resources";
        private const string configFile = "config.json";

        public static BotConfig bot;

        static Config()
        {
            
            // Checks if the configFolder folder exists, and if it doesn't, creates it.
            if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder);
            
            // Checks if the configFile file exists.
            if (!File.Exists(configFolder + "/" + configFile))
            {
                // If it doesn't, create it and put the default information in.
                bot = new BotConfig();
                string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
                File.WriteAllText(configFolder + "/" + configFile, json);
            }
            else
            {
                // If it does, take the information from it and store it in the bot variable.
                string json = File.ReadAllText(configFolder + "/" + configFile);
                bot = JsonConvert.DeserializeObject<BotConfig>(json);

            }


        }

        public static void Refresh()
        {
            if (!File.Exists(configFolder + "/" + configFile))
            {
                return;
            }

            string json = File.ReadAllText(configFolder + "/" + configFile);
            bot = JsonConvert.DeserializeObject<BotConfig>(json);
        }
    }



    public struct BotConfig
    {
        public string token;
        public string cmdPrefix;
        public string game;
    }

}
