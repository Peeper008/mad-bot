using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mad_Bot_Discord.Modules
{
    public class Gungeon : ModuleBase<SocketCommandContext>
    {
        private Random r = new Random();



        public enum Characters
        {
            Convict,
            Hunter,
            Marine,
            Pilot,
            Bullet,
            Robot,
            Cultist
        };

        public enum EnumEvents
        {
            Chest_Open,
            Chest_Break,
            Boss_Kill,
            Key_Use,
            Heart_Lose,
            Item_Purchased
        };

        public enum Actions
        {
            Drop_Gun,
            Drop_Passive_Item,
            Drop_Active_Item,
            Lose_One_Health,
            Die,
            Give_Random_Passive_Item,
            Give_Random_Active_Item,
            Give_Random_Gun,
            Get_Half_Heart,
            Get_Full_Heart,
            Get_One_Key,
            Get_One_Armor,
           
        }

        public enum ItemType
        {
            Passive,
            Active,
            Gun,
            Key,
            Health,
            Armor,
            All,
            None
        }

        public enum Quality
        {
            D,
            C,
            B,
            A,
            S
        }

        public enum GiveTake
        {
            Give_Item,
            Take_Item,
            None
        }

        public enum Restrictions
        {
            One_Non_Starter_Gun_Only,
            One_Passive_Item_Only,
            No_Passive_Items,
            Must_Kill_Boss_Upon_Finding_Room,
            Bullet_Modifier_Passive_Items_Only,
            Starter_Gun_Only,
            Starter_Items_Only,
            Brown_Chests_Only,
            No_Chests,
            Blue_Chests_Or_Lower_Only,
            Green_Chests_Or_Lower_Only,
            Replace_All_Passive_Items_With_Junk,
            Replace_All_Passive_Items_With_Spice,
            No_Dropping_Items,
            No_Dropping_Guns
        }

        public enum StartWith
        {
            RAND11_RandomPassiveItems,
            RAND12_RandomPassiveItems,
            RAND13_RandomPassiveItems,
            RAND11_RandomActiveItems,
            RAND12_RandomActiveItems,
            RAND13_RandomActiveItems,

            RAND13_Duct_Tape,
            RAND13_Eyepatch,
            Lament_Configurum,
            Ser_Junkan,
            Live_Ammo,
            Disarming_Personality,
            RAND14_Scattershot,
            RAND13_Random_Bullet_Modifier,
            Full_Gunknight_Set,
            Full_Metal_Jacket,
            Gungeon_Blueprint,
            All_Table_Techs,
            Clown_Mask,
            Box,
            Double_Vision,
            RAND12_Ancient_Heros_Bandana,
            Bloodied_Scarf,
        }

        enum RunType
        {
            Normal,
            Cursed,
            Blessed,
            Turbo_Normal,
            Turbo_Blessed,
            Turbo_Cursed,
            Turbo_Blessed_Cursed,
            Turbo_Blessed_Normal
        }

        static string itemsRaw = File.ReadAllText("SystemLang/gungeonItems.json");
        static List<List<string>> str = (List<List<string>>)JsonConvert.DeserializeObject(itemsRaw, typeof(List<List<string>>));

        static List<string> guns = str[0].ToList();
        static List<string> activeItems = str[1].ToList();
        static List<string> passiveItems = str[2].ToList();


        [Command("gungeon"), Alias("etg")]
        public async Task EnterTheGungeon([Remainder] string initialArgs = "")
        {

            if (initialArgs != "")
            {
                string[] args = initialArgs.Split(' ');
                if (args.Length > 0)
                {
                    if (args[0] == "random" || args[0] == "randomize")
                    {
                        if (args.Length > 1)
                        {
                            if (args[1] ==  "character")
                            {
                                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed(
                                    "Random Gungeon Character", "**Random Character: **" + GetCharacters(), Context));
                                return;
                            }

                            if (args[1] == "passive")
                            {
                                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed(
                                    "Random Gungeon Passive Item", "**Random Passive Item: **" + GetPassiveItem(), Context));
                                return;
                            }

                            if (args[1] == "active")
                            {
                                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed(
                                    "Random Gungeon Active Item", "**Random Active Item: **" + GetActiveItem(), Context));
                                return;
                            }

                            if (args[1] == "gun")
                            {
                                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed(
                                    "Random Gungeon Gun", "**Random Gun: **" + GetGun(), Context));
                                return;
                            }
                        }


                    }
                }
            }

            #region Random Challenge
            int eventLimit = 3;
            int restrictionLimit = 3;
            int itemLimit = 3;


            float eventPercent = 0.3333333333333333f;           // 33%
            float restrictionPercent = 0.3333333333333333f;    // 33%
            float startingPercent = 0.8f;                                 // 80%



            // Gets a random character
            string characterString = GetCharacters();




            // Gets a string of random events
            string eventString = String.Empty;

            Array eventArray = Enum.GetValues(typeof(EnumEvents));
            IList<EnumEvents> allEvents = eventArray.OfType<EnumEvents>().ToList();

            Array actionsArray = Enum.GetValues(typeof(Actions));
            IList<Actions> actionsList = actionsArray.OfType<Actions>().ToList();

            for (int i = 0; i < eventLimit; i++)
            {
                int randNumb = r.Next(0, allEvents.Count);
                int secondNumber = (int)Math.Round(allEvents.Count * eventPercent);

                if (randNumb <= secondNumber)
                {
                    int eventToAdd = r.Next(allEvents.Count);
                    eventString = eventString + "On " + Enum.GetName(typeof(EnumEvents), eventToAdd).Replace('_', ' ') + " - " + Enum.GetName(typeof(Actions), r.Next(actionsList.Count)).Replace('_', ' ') + "\n";
                }
            }


            // Gets a string of random restrictions
            string restrictionString = String.Empty;

            Array restValues = Enum.GetValues(typeof(Restrictions));
            IList<Restrictions> restrictionList = restValues.OfType<Restrictions>().ToList();

            for (int i = 0; i < restrictionLimit; i++)
            {
                int randNumb = r.Next(0, restrictionList.Count);
                int secondNumber = (int)Math.Round(restrictionList.Count * restrictionPercent);

                if (randNumb <= secondNumber)
                {
                    int restrictionToAdd = r.Next(restrictionList.Count);
                    restrictionString = restrictionString + Enum.GetName(typeof(Restrictions), restrictionToAdd).Replace('_', ' ') + "\n";
                }
            }

            Array runValues = Enum.GetValues(typeof(RunType));
            IList<RunType> runTypes = runValues.OfType<RunType>().ToList();

            string stringRunType = Enum.GetName(typeof(RunType), r.Next(runTypes.Count)).Replace('_', ' ');


            // Gets a string of random starting items
            string startsWith = String.Empty;

            Array itemValues = Enum.GetValues(typeof(StartWith));
            IList<StartWith> itemList = itemValues.OfType<StartWith>().ToList();

            for (int i = 0; i < itemLimit; i++)
            {
                int randNumb = r.Next(0, itemList.Count);
                int secondNumber = (int)Math.Round(itemList.Count * startingPercent);

                if (randNumb <= secondNumber)
                {
                    int rNumb = r.Next(itemList.Count);
                    string n = Enum.GetName(typeof(StartWith), rNumb);

                    n = TurnStringIntoRandString(n);

                    if (n.Split('_').Length > 1)
                    {
                        if (n.Split('_')[1] == "RandomPassiveItems")
                        {
                            int.TryParse(n.Split('_')[0], out int numb);

                            if (numb > 3) numb = 3;
                            n = String.Empty;

                            for (int h = 0; h < numb; h++)
                            {
                                string newItem = GetPassiveItem();
                                newItem = TurnStringIntoRandString(newItem);

                                n = n + newItem + "\n";
                            }

                            n = n.Substring(0, n.Length - 1);

                        }
                        else if (n.Split('_')[1] == "RandomActiveItems")
                        {
                            int numb = 1;

                            n = String.Empty;

                            for (int h = 0; h < numb; h++)
                            {
                                string newItem = GetActiveItem();
                                newItem = TurnStringIntoRandString(newItem);

                                n = n + newItem + "\n";
                            }

                            n = n.Substring(0, n.Length - 1);
                        }
                    }

                    n = n.Replace('_', ' ');
                    startsWith = startsWith + n + "\n";
                }




            }



            // Sends the message combining everything
            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Mod The Gungeon Challenge", "", "Run:",
                $"(If some things conflict just choose what you want)\n\n**Character:** {characterString}\n**Events:**\n {eventString}\n**Restrictions:**\n{restrictionString}\n" +
                $"**Starting Items:**\n{startsWith}\n**Run Type:** {stringRunType}\n\n(Feel free to change whatever you like to make this a good challenge because it probably isn't to start with)", Context));

            #endregion Random Challenge
            return;

        }

        /// <summary> Gives you a random character that isn't in the excluded list. Returns The Cultist if all are excluded. </summary>
        private string GetCharacters(Characters[] excludedChars)
        {
            // Gets all characters and stores them into an array, then converts it to a list.
            Array values = Enum.GetValues(typeof(Characters));
            List<Characters> chars = values.OfType<Characters>().ToList();

            // Loops through every excluded character and removes characters from the list.
            for (int i = 0; i < excludedChars.Length; i++)
            {
                chars.Remove(excludedChars[i]);
            }

            // Makes sure not every character is excluded, and if they are, return The Cultist.
            if (chars.Count == 0) return "The Cultist <:the_cultist:483065323482906635>";

            // Returns the random character.
            Characters randomCharacter = chars[r.Next(chars.Count)];
            string characterString = "";

            // Sets characterString based off of character chosen
            switch (randomCharacter)
            {
                case Characters.Bullet:
                    characterString = "The Bullet <:the_bullet:483065310136762388>";
                    break;
                case Characters.Convict:
                    characterString = "The Convict <:the_convict:483065317858344970>";
                    break;
                case Characters.Cultist:
                    characterString = "The Cultist <:the_cultist:483065323482906635>";
                    break;
                case Characters.Hunter:
                    characterString = "The Hunter <:the_hunter:483065329665310730>";
                    break;
                case Characters.Marine:
                    characterString = "The Marine <:the_marine:483065334782230549>";
                    break;
                case Characters.Pilot:
                    characterString = "The Pilot <:the_pilot:483065340008333313>";
                    break;
                case Characters.Robot:
                    characterString = "The Robot <:the_robot:483065346866020352>";
                    break;
                default:
                    characterString = "The Cultist <:the_cultist:483065323482906635>";
                    break;
            }

            return characterString;
        }

        /// <summary> Gives you a random character. </summary>
        private string GetCharacters()
        {
            // Gets all characters and stores them into an array, then uses the array to get a random character.
            Array values = Enum.GetValues(typeof(Characters));
            Characters randomCharacter = (Characters)values.GetValue(r.Next(values.Length));

            string characterString = Enum.GetName(typeof(Characters), r.Next(values.Length));

            // Sets characterString based off of character chosen
            switch (randomCharacter)
            {
                case Characters.Bullet:
                    characterString = "The Bullet <:the_bullet:483065310136762388>";
                    break;
                case Characters.Convict:
                    characterString = "The Convict <:the_convict:483065317858344970>";
                    break;
                case Characters.Cultist:
                    characterString = "The Cultist <:the_cultist:483065323482906635>";
                    break;
                case Characters.Hunter:
                    characterString = "The Hunter <:the_hunter:483065329665310730>";
                    break;
                case Characters.Marine:
                    characterString = "The Marine <:the_marine:483065334782230549>";
                    break;
                case Characters.Pilot:
                    characterString = "The Pilot <:the_pilot:483065340008333313>";
                    break;
                case Characters.Robot:
                    characterString = "The Robot <:the_robot:483065346866020352>";
                    break;
                default:
                    characterString = "The Cultist <:the_cultist:483065323482906635>";
                    break;
            }

            // Returns the character
            return characterString;
        }

        private string GetPassiveItem()
        {
            return passiveItems[r.Next(passiveItems.Count)].Replace('_', ' ');
        }

        private string GetActiveItem()
        {
            return activeItems[r.Next(activeItems.Count)].Replace('_', ' ');
        }

        private string GetGun()
        {
            return guns[r.Next(guns.Count)].Replace('_', ' ');
        }

        /// <summary>
        /// Takes strings with "RAND(int)(int)" at the beginning and turns that rand into a number based on the numbers provided.
        /// Example: RAND13_Cool_Thing  ---> 1_Cool_Thing
        /// </summary>
        ///
        /// <param name="str"> The string you want converted </param>
        private string TurnStringIntoRandString(string str)
        {
            if (str.StartsWith("RAND"))
            {
                int firstInt = 0;
                int.TryParse(str.Substring(4, 1), out firstInt);

                int secondInt = 3;
                int.TryParse(str.Substring(5, 1), out secondInt);

                int finalNumb = r.Next(firstInt, secondInt + 1);

                str = finalNumb + "_" + str.Substring(7);

                return str;
            }
            else return str;
        }

        public class Event
        {
            public EnumEvents Name { get; set; }
            public GiveTake GiveOrTake { get; set; }
            public ItemType TypeOfItem { get; set; }
        }

    }
}
