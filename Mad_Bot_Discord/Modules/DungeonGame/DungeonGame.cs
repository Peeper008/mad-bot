using Discord.Commands;
using Discord.WebSocket;
using Mad_Bot_Discord.Core.UserAccounts;
using Mad_Bot_Discord.Modules.DungeonGame;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Mad_Bot_Discord
{
    /// <summary>
    /// You enter an infinite randomly-generated dungeon
    /// attempting to get the best score possible. You
    /// may leave the dungeon at any time, finalizing your score,
    /// or you can continue moving forward. If you die, only half
    /// your total score counts.
    /// 
    /// You start with 0 gold your first run, and killing 
    /// enemies and triggering certain events can give you 
    /// gold. Then, events can give you ways to spend that 
    /// gold. For example, a blacksmith can offer to repair
    /// your weapon for a price, or a shopkeeper can sell you 
    /// weapons or ammo. When your run ends, you get to keep 
    /// half the gold for your next run, and perhaps you can
    /// have a better starter weapon for gold when you start
    /// a run? Another idea for gold is events that can give
    /// or take gold. Like, some dude comes up offering to give
    /// you twice the gold you give him, then there's a 50% chance
    /// you'll actually get the gold or just get scammed. I dunno,
    /// just some random ideas I had. Maybe you shouldn't be able
    /// to keep some of the gold, idk.
    /// 
    /// random events pls
    /// 
    /// To Do Next: 
    /// Damage System for weapons
    /// Random items on ground
    /// Custom Player Icon (map)
    /// Custom Color (map) ini = blue, css = red,
    /// Consumables that you can save between runs.
    /// MB as strongest boss?
    /// Multiple scores on the leaderboard such as most rooms explored in a single run or most weapons gathered in a single run.
    /// Themes and different modes (like wild-west only)
    /// enemy versions of other users whove played before, matching their stats and the current theme.
    /// when you die have it say "You died." along with some random text like "this is so sad alexa play despacito"
    /// 
    /// </summary>


    public class DungeonGame : ModuleBase<SocketCommandContext>
    {
        
        // Tutorial message after each command to tell you what you can do.
        public string tutorialMessage = $"Reply with `{Config.bot.cmdPrefix}dg <direction>` to move, `{Config.bot.cmdPrefix}dg map` to view the map" +
            $", `{Config.bot.cmdPrefix}dg inv` to view your inventory, `{Config.bot.cmdPrefix}dg types` to view what room types do/appear like on the map, " +
            $"or `{Config.bot.cmdPrefix}dg leave` to leave the dungeon and finalize your score.";

        private static List<SaveData> saves;
        static string savePath = "Resources/dgsaves.json";

        #region Data Handling

        void SaveData()
        {
            if (!File.Exists(savePath))
            {
                saves = new List<SaveData>();
                string json = JsonConvert.SerializeObject(saves, Formatting.Indented);
                File.WriteAllText(savePath, json);
            }
            else
            {
                string json = JsonConvert.SerializeObject(saves, Formatting.Indented);
                File.WriteAllText(savePath, json);
            }
        }
        void RefreshData()
        {
            string json = File.ReadAllText(savePath);

            JsonConverter[] converters = { new WeaponConverter() };
            saves = JsonConvert.DeserializeObject<List<SaveData>>(json, new JsonSerializerSettings() { Converters = converters });
        }
        SaveData CreateSaveData(ulong id)
        {
            SocketGuildUser user = (SocketGuildUser)Context.User;

            var newSave = new SaveData() { User = UserAccounts.GetAccount(user), Player = new Player() };
            newSave.Player.Name = user.Username;
            newSave.Player.SaveData = newSave;

            saves.Add(newSave);
            SaveData();
            return newSave;
        }
        SaveData GetOrCreateSaveData(ulong id)
        {
            RefreshData();

            var result = from s in saves
                         where s.User.ID == id
                         select s;

            SaveData save = result.FirstOrDefault();
            if (save == null) save = CreateSaveData(id);
            return save;
        }
        SaveData GetData(ulong id)
        {
            return GetOrCreateSaveData(id);
        }

        #endregion Data Handling

        Random r = new Random();

        [Command("Dungeon"), Alias("dg")]
        public async Task Dungeon([Remainder] string options = "")
        {
            // Currently only works for me.
            if (Context.User.Id != 226223728076390410) return;

            #region Save File Interaction
            if (!File.Exists(savePath))
            {
                // If it doesn't, create it.
                UserAccount user = UserAccounts.GetAccount(Context.User);

                saves = new List<SaveData> { new SaveData { User = user } };
                string json = JsonConvert.SerializeObject(saves, Formatting.Indented);
                File.WriteAllText(savePath, json);
            }
            else
            {
                JsonConverter[] converters = { new WeaponConverter() };

                // If it does, read it and save it to a variable.
                string json = File.ReadAllText(savePath);
                
                saves = JsonConvert.DeserializeObject<List<SaveData>>(json, new JsonSerializerSettings() { Converters = converters });
            }
            #endregion Save File Interaction

            // Gets any arguments provided.
            string[] args = { };
            if (!string.IsNullOrWhiteSpace(options)) args = options.Split(' ');

            // Gets the users save data.
            SaveData data = GetData(Context.User.Id);

            // Gets the player
            Player player = data.Player;

            // Starts a variable that stores what mb will say to you.
            string sentence = "";

            // If the user doesn't have any save data, set up the game.
            if (!data.InGame)
            {

                // Creates a starting sword.
                Sword startingSword = new Sword()
                {
                    BladeLength = DungeonWeaponParts.Sword_BladeLength[0],
                    BladeThickness = DungeonWeaponParts.Sword_BladeThickness[0],
                    BladeType = DungeonWeaponParts.Sword_BladeType[1],
                    HiltGrip = DungeonWeaponParts.Sword_HiltGrip[0],
                    HiltSize = DungeonWeaponParts.Sword_HiltSize[0],
                    HiltType = DungeonWeaponParts.Sword_HiltType[1],
                    WeightType = DungeonWeaponParts.Sword_WeightType[0],
                    Name = DungeonWeaponParts.Sword_BladeType[1].NamePart + DungeonWeaponParts.Sword_BladeThickness[0].NamePart + DungeonWeaponParts.Sword_HiltType[1].NamePart + DungeonWeaponParts.Sword_HiltSize[0].NamePart
                };

                // Sets the equipped weapon to the starting sword.
                player.EquippedWeapon = startingSword;

                // Creates a starting dungeon.
                DungeonRoom startingRoom = new DungeonRoom
                {
                    Coordinates = new Coordinates { X = 0, Y = 0 },
                    RoomDoors = new List<Direction> { Direction.Left, Direction.Right, Direction.Front, Direction.Back },
                    Type = RoomType.Empty
                };

                // Sets up a dictionary that stores every room visited.
                Dictionary<string, DungeonRoom> Rooms = new Dictionary<string, DungeonRoom>
                {
                    { Coordinates.CoordsToString(0, 0), startingRoom }
                };

                // Adds this to the beginning of the sentence to make sure you know you are starting over.
                sentence = sentence + $"**Dungeon Start.**\n\n";

                // Sets the player's current room to the starting room we created earlier.
                data.CurrentRoom = startingRoom;

                // Sets the savedata's room dictionary to the dictionary we created earlier.
                data.Rooms = Rooms;

                // Sets the InGame variable to true, which means the game now knows we have started.
                data.InGame = true;

                // Sets up the player's inventory.
                player.Inventory = new List<Weapon> { startingSword };
                
                // Makes sure they arent in a fight.
                data.InFight = false;

                // Saves all that new data.
                SaveData();
            }

            #region Main
            if (!data.InFight)
            {
                if (args.Length > 0)
                {
                    switch (args[0].ToLower())
                    {
                        #region directions

                        case "left":
                            #region left
                            if (data.CurrentRoom.RoomDoors.Contains(Direction.Left))
                            {
                                

                                data.CurrentRoom = RoomGenerator(new Coordinates { X = data.CurrentRoom.Coordinates.X - 1, Y = data.CurrentRoom.Coordinates.Y }, data);

                                if (data.CurrentRoom.Type == RoomType.Enemy)
                                {
                                    data.CurrentEnemy = EnemyGenerator();
                                    data.InFight = true;

                                    sentence = sentence + "You walk into the room to the left of you and are attacked by a " + data.CurrentEnemy.Name + $"! Type `{Config.bot.cmdPrefix}dg` to start.";
                                    data.Rooms.Add(Coordinates.CoordsToString(data.CurrentRoom.Coordinates), data.CurrentRoom);

                                    SaveData();
                                    await Context.Channel.SendMessageAsync(sentence);
                                    return;
                                }

                                if (!data.Rooms.ContainsKey(Coordinates.CoordsToString(data.CurrentRoom.Coordinates)))
                                {
                                    sentence = sentence + "You walk into the room to your left. ";
                                    data.Rooms.Add(Coordinates.CoordsToString(data.CurrentRoom.Coordinates), data.CurrentRoom);
                                } 
                                else
                                {
                                    sentence = sentence + "You walk back into the room to your left. ";
                                }

                                SaveData();

                                
                            }
                            else
                            {
                                sentence = sentence + "You cannot do that.\n\n";
                            }
                            break;
                            #endregion left

                        case "right":
                            #region right
                            if (data.CurrentRoom.RoomDoors.Contains(Direction.Right))
                            {
                                data.CurrentRoom = RoomGenerator(new Coordinates { X = data.CurrentRoom.Coordinates.X + 1, Y = data.CurrentRoom.Coordinates.Y }, data);

                                if (data.CurrentRoom.Type == RoomType.Enemy)
                                {
                                    data.CurrentEnemy = EnemyGenerator();
                                    data.InFight = true;

                                    sentence = sentence + "You walk into the room to the right of you and are attacked by a " + data.CurrentEnemy.Name + $"! Type `{Config.bot.cmdPrefix}dg` to start.";
                                    data.Rooms.Add(Coordinates.CoordsToString(data.CurrentRoom.Coordinates), data.CurrentRoom);

                                    SaveData();
                                    await Context.Channel.SendMessageAsync(sentence);
                                    return;
                                }

                                if (!data.Rooms.ContainsKey(Coordinates.CoordsToString(data.CurrentRoom.Coordinates)))
                                {
                                    sentence = sentence + "You walk into the room to your right. ";
                                    data.Rooms.Add(Coordinates.CoordsToString(data.CurrentRoom.Coordinates), data.CurrentRoom);
                                }
                                else
                                {
                                    sentence = sentence + "You walk back into the room to your right. ";
                                }

                                SaveData();

                            }
                            else
                            {
                                sentence = sentence + "You cannot do that.\n\n";
                            }
                            break;
                         #endregion right

                        case "front":
                        case "up":
                        case "forward":
                        case "forwards":
                            #region front
                            if (data.CurrentRoom.RoomDoors.Contains(Direction.Front))
                            {
                                data.CurrentRoom = RoomGenerator(new Coordinates { X = data.CurrentRoom.Coordinates.X, Y = data.CurrentRoom.Coordinates.Y + 1 }, data);

                                if (data.CurrentRoom.Type == RoomType.Enemy)
                                {
                                    data.CurrentEnemy = EnemyGenerator();
                                    data.InFight = true;

                                    sentence = sentence + "You walk into the room ahead of you and are attacked by a " + data.CurrentEnemy.Name + $"! Type `{Config.bot.cmdPrefix}dg` to start.";
                                    data.Rooms.Add(Coordinates.CoordsToString(data.CurrentRoom.Coordinates), data.CurrentRoom);

                                    SaveData();
                                    await Context.Channel.SendMessageAsync(sentence);
                                    return;
                                }

                                if (!data.Rooms.ContainsKey(Coordinates.CoordsToString(data.CurrentRoom.Coordinates)))
                                {
                                    sentence = sentence + "You walk into the room ahead. ";
                                    data.Rooms.Add(Coordinates.CoordsToString(data.CurrentRoom.Coordinates), data.CurrentRoom);
                                }
                                else
                                {
                                    sentence = sentence + "You walk back into the room ahead. ";
                                }

                                SaveData();

                            }
                            else
                            {
                                sentence = sentence + "You cannot do that.\n\n";
                            }
                            break;
                            #endregion front

                        case "back":
                        case "down":
                        case "backward":
                        case "backwards":
                            #region back
                            if (data.CurrentRoom.RoomDoors.Contains(Direction.Back))
                            {
                                data.CurrentRoom = RoomGenerator(new Coordinates { X = data.CurrentRoom.Coordinates.X, Y = data.CurrentRoom.Coordinates.Y - 1}, data);

                                if (data.CurrentRoom.Type == RoomType.Enemy)
                                {
                                    if (!data.Rooms.ContainsKey(Coordinates.CoordsToString(data.CurrentRoom.Coordinates)))
                                    {
                                        data.CurrentEnemy = EnemyGenerator();
                                        data.InFight = true;

                                        sentence = sentence + "You walk into the room behind you and are attacked by a " + data.CurrentEnemy.Name + $"! Type `{Config.bot.cmdPrefix}dg` to start.";
                                        data.Rooms.Add(Coordinates.CoordsToString(data.CurrentRoom.Coordinates), data.CurrentRoom);

                                        SaveData();
                                        await Context.Channel.SendMessageAsync(sentence);
                                        return;

                                    }
                                }

                                if (!data.Rooms.ContainsKey(Coordinates.CoordsToString(data.CurrentRoom.Coordinates)))
                                {
                                    sentence = sentence + "You walk into the room behind you. ";
                                    data.Rooms.Add(Coordinates.CoordsToString(data.CurrentRoom.Coordinates), data.CurrentRoom);
                                }
                                else
                                {
                                    sentence = sentence + "You walk back into the room behind you. ";
                                }

                                SaveData();
                            }
                            else
                            {
                                sentence = sentence + "You cannot do that.\n\n ... \n\n";
                            }
                            break;
                        #endregion back

                        #endregion directions

                        case "map":
                            #region map
                            //sentence = sentence + $"Current Room Type: [{data.CurrentRoom.Type}] \n\n";

                            List<double> allX = new List<double>();
                            List<double> allY = new List<double>();

                            foreach (KeyValuePair<string, DungeonRoom> r in data.Rooms)
                            {
                                DungeonRoom v = r.Value;

                                allX.Add(v.Coordinates.X);
                                allY.Add(v.Coordinates.Y);
                            }

                            allX.Sort();
                            allY.Sort();

                            string mp = "```ini\n";

                            for (double y = allY.Max() + 1; y >= allY.Min() - 1; y = y - 0.5)
                            {
                                for (double x = allX[0] - 1; x <= allX.Max() + 1; x = x + 0.5)
                                {
                                    string c = Coordinates.CoordsToString(x, y);
                                    string TBA = "";

                                    if (data.Rooms.ContainsKey(Coordinates.CoordsToString(x + 0.5, y)))
                                    {
                                        if (data.Rooms[Coordinates.CoordsToString(x + 0.5, y)].RoomDoors.Contains(Direction.Left))
                                        {
                                            TBA = "===";
                                        }
                                    }

                                    if (data.Rooms.ContainsKey(Coordinates.CoordsToString(x - 0.5, y)))
                                    {
                                        if (data.Rooms[Coordinates.CoordsToString(x - 0.5, y)].RoomDoors.Contains(Direction.Right))
                                        {
                                            TBA = "==="; 
                                        }
                                    }

                                    if (data.Rooms.ContainsKey(c))
                                    {
                                        if (Coordinates.CoordsToString(data.CurrentRoom.Coordinates) == Coordinates.CoordsToString(x, y))
                                            TBA = "<@>";
                                        else
                                        {
                                            DungeonRoom nr = data.Rooms[c];

                                            switch (nr.Type)
                                            {
                                                case RoomType.Boss:
                                                    TBA = "[!]";
                                                    break;
                                                case RoomType.Empty:
                                                    TBA = "[ ]";
                                                    break;
                                                case RoomType.Enemy:
                                                    TBA = "[#]";
                                                    break;
                                                case RoomType.Loot:
                                                    TBA = "[$]";
                                                    break;
                                            }
                                        }
                                    }
                                    else if (TBA != "===")
                                        TBA = "   ";

                                    if (data.Rooms.ContainsKey(Coordinates.CoordsToString(x, y+0.5)))
                                    {
                                        if (data.Rooms[Coordinates.CoordsToString(x, y+0.5)].RoomDoors.Contains(Direction.Back))
                                        {
                                            TBA = "| |";
                                        }
                                        else
                                        {
                                            TBA = "   ";
                                        }
                                    }

                                    if (data.Rooms.ContainsKey(Coordinates.CoordsToString(x, y-0.5)))
                                    {
                                        if (data.Rooms[Coordinates.CoordsToString(x, y-0.5)].RoomDoors.Contains(Direction.Front))
                                        {
                                            TBA = "| |";
                                        }
                                        else
                                            TBA = "   ";
                                    }

                                    mp = mp + TBA;
                                }

                                mp = mp + "\n";
                            }

                            mp = mp + "``` \n\n";
                            await Context.Channel.SendMessageAsync(mp);

                            break;

                        #endregion map

                        case "inv":
                        case "inventory":
                            #region inventory

                            string tba = "Your current inventory:\n\n**Swords:** \n\n";
                            List<Weapon> w = data.Player.Inventory;

                            List<Sword> swords = new List<Sword>();
                            List<Gun> guns = new List<Gun>();

                            foreach (Weapon weapon in w)
                            {
                                if (weapon.WeaponType == WeaponType.Sword) swords.Add((Sword)weapon);
                                if (weapon.WeaponType == WeaponType.Gun) guns.Add((Gun)weapon);
                            }
                                                 

                            if (swords.Count > 0)
                            {
                                foreach (Sword s in swords)
                                {
                                    tba = tba + s.Name + $" [Damage: {s.GetBaseDamage()} | Accuracy: {s.GetAccuracy()} | Stun Chance: {s.GetStunChance()}0% | Stun Amount: {s.GetStunAmount()} Turns] \n";
                                }
                            }
                            else tba = tba + "None!\n";
                            tba = tba + "\n**Guns:** \n\n";


                            if (guns.Count > 0)
                            {
                                foreach (Gun g in guns)
                                {
                                    tba = tba + g.Name + $" [Damage: {g.GetBaseDamage()} | Accuracy: {g.GetAccuracy()} | Stun Chance: {g.GetStunChance()}0% | Stun Amount: {g.GetStunAmount()} Turns] \n";
                                }
                            }
                            else tba = tba + "None!\n";

                            sentence = sentence + tba + "\n\n";

                            break;
                            #endregion inventory

                        case "leave":
                        case "exit":
                        case "quit":
                            #region quit
                            string[] encouragingWords = { "Wow, you", "Incredible! You", "Good job, you", "Nice work, you", "That's crazy! You", "Meh, you", "**yawn** you" };
                            string word = encouragingWords[r.Next(encouragingWords.Length)];

                            sentence = $"You left the dungeon. {word} got [0] score.";
                            await Context.Channel.SendMessageAsync(sentence);
                            data.InGame = false;
                            SaveData();
                            return;
                            #endregion quit

                        case "types":

                            sentence = sentence + "Room Types:\n\nEmpty: An empty room. Appears as `[ ]` in the map.\nEnemy: A room that contains enemies. Appears as `[#]` in the map.\n" +
                                "Loot: A room that gives you random loot. Appears as `[$]` in the map.\nBoss: A room that contains a boss. Appears as `[!]` in the map.\n" +
                                "You: You appear as `<@>` on the map.\n\n";

                            await Context.Channel.SendMessageAsync(sentence);
                            return;
                    }
                }

                // RoomCount = amount of doors in a room.
                if (data.CurrentRoom.RoomCount > 1)
                {
                    sentence = sentence + $"Current Room Type: [{data.CurrentRoom.Type}] \n\n";
                    string one = "";
                    for (int i = 0; i < data.CurrentRoom.RoomCount; i++)
                    {

                        if (i == data.CurrentRoom.RoomCount - 1)
                        {
                            one = one + ", and one to your **" + data.CurrentRoom.RoomDoors[i].ToString().ToLower() + "**.";

                        }
                        else
                        {
                            one = one + ", one to your **" + data.CurrentRoom.RoomDoors[i].ToString().ToLower() + "**";

                        }
                    }
                    sentence = sentence + $"There are {data.CurrentRoom.RoomCount} rooms{one} {tutorialMessage}";

                }
                else
                {
                    sentence = sentence + $"Current Room Type: [{data.CurrentRoom.Type}] \n\n";
                    sentence = sentence = $"There is one room to your **{data.CurrentRoom.RoomDoors[0].ToString().ToLower()}**. {tutorialMessage}";
                }

                await Context.Channel.SendMessageAsync(sentence);

            }
            else
            {
                if (args.Length > 0)
                {
                    switch (args[0].ToLower())
                    {
                        case "attack":

                            DungeonMethods.DealDamage(data.CurrentEnemy, 5);

                            if (data.CurrentEnemy.Health <= 0)
                            {
                                sentence = sentence + $"\n\nYou defeated the {data.CurrentEnemy.Name} and gained [0] gold!";
                                data.CurrentRoom.Type = RoomType.Empty;
                                data.Rooms[Coordinates.CoordsToString(data.CurrentRoom.Coordinates)].Type = RoomType.Empty;
                                data.InFight = false;

                                await Context.Channel.SendMessageAsync(sentence);

                                SaveData();
                                return;
                            }

                            

                            if (data.Player.Health <= 0)
                            {
                                sentence = sentence + $"You died! You got [0] score!";
                                data.InGame = false;
                                SaveData();
                                return;
                            }

                            SaveData();

                            await Context.Channel.SendMessageAsync(sentence);

                            break;

                        case "item":
                            break;

                        case "check":
                            sentence = sentence + $"The {data.CurrentEnemy.Name} has {data.CurrentEnemy.Health} HP left.";

                            if (data.CurrentEnemy.StunLeft > 0)
                                sentence = sentence + $" It has {data.CurrentEnemy.StunLeft} turns left until the stun wears off.";

                            await Context.Channel.SendMessageAsync(sentence);
                            break;

                        case "status":
                            sentence = sentence + $"You currently have `{player.Health}` HP and have the {player.EquippedWeapon.Name} [Damage: {player.EquippedWeapon.GetBaseDamage()}] equipped.";
                            await Context.Channel.SendMessageAsync(sentence);

                            break;
                    }
                }
                else
                {
                    sentence = sentence + $"You are fighting a `{data.CurrentEnemy.Name}` with `{data.CurrentEnemy.Health}` HP.\n\n**Options:** | Attack | Item | Check | Status |";
                    await Context.Channel.SendMessageAsync(sentence);
                }


            }
            #endregion Main
        }

        /*            string weapon = $"You find a {rGun.FireType.Name} {rGun.BodyType.Name} that fires {rGun.Projectile.Name}. It is labelled *\"{rGun.Name}\"*\n" +
                $"Total Stats: [Damage: {rGun.FireType.Damage + rGun.BodyType.Damage + rGun.Projectile.Damage} | Accuracy: {rGun.FireType.Accuracy + rGun.BodyType.Accuracy + rGun.Projectile.Accuracy}" +
                $" | Stun Chance: {rGun.Projectile.StunChance}0% | Stun Amount: {rGun.Projectile.StunAmount} Rounds]";
                */



        // Random Room
        public DungeonRoom RoomGenerator(Coordinates coords, SaveData data)
        {
            if (data.Rooms.ContainsKey(Coordinates.CoordsToString(coords))) return data.Rooms[Coordinates.CoordsToString(coords)];

            bool BossAvailable = (data.Rooms.Count > 14) ? true : false;

            DungeonRoom currentRoom = data.CurrentRoom;

            Array values = Enum.GetValues(typeof(RoomType));
            RoomType randomType = (RoomType)values.GetValue(r.Next((BossAvailable) ? values.Length : values.Length - 1));

            CanDirection canDir = new CanDirection();

            DungeonRoom roomR = null;
            DungeonRoom roomL = null;
            DungeonRoom roomF = null;
            DungeonRoom roomB = null;

            

            List<Direction> rDoors = new List<Direction>();

            if (data.Rooms.ContainsKey(Coordinates.CoordsToString(new Coordinates { X = coords.X + 1, Y = coords.Y })))
            {
                roomR = data.Rooms[Coordinates.CoordsToString(new Coordinates { X = coords.X + 1, Y = coords.Y })];
                if (roomR.RoomDoors.Contains(Direction.Left))
                {
                    rDoors.Add(Direction.Right); 
                }
            }
            else canDir.CanRight = true;

            if (data.Rooms.ContainsKey(Coordinates.CoordsToString(new Coordinates { X = coords.X - 1, Y = coords.Y })))
            {
                roomL = data.Rooms[Coordinates.CoordsToString(new Coordinates { X = coords.X - 1, Y = coords.Y })];
                if (roomL.RoomDoors.Contains(Direction.Right))
                {
                    rDoors.Add(Direction.Left);
                }
            }
            else canDir.CanLeft = true;

            if (data.Rooms.ContainsKey(Coordinates.CoordsToString(new Coordinates { X = coords.X, Y = coords.Y + 1 })))
            {
                roomF = data.Rooms[Coordinates.CoordsToString(new Coordinates { X = coords.X, Y = coords.Y + 1 })];
                if (roomF.RoomDoors.Contains(Direction.Back))
                {
                    rDoors.Add(Direction.Front);
                }
            }
            else canDir.CanFront = true;

            if (data.Rooms.ContainsKey(Coordinates.CoordsToString(new Coordinates { X = coords.X, Y = coords.Y - 1 })))
            {
                roomB = data.Rooms[Coordinates.CoordsToString(new Coordinates { X = coords.X, Y = coords.Y - 1 })];
                if (roomB.RoomDoors.Contains(Direction.Front))
                {
                    rDoors.Add(Direction.Back);
                }
            }
            else canDir.CanBack = true;
            int totalDoors = 0;

            if (canDir.CanLeft && r.Next(2) == 1)
                if (!rDoors.Contains(Direction.Left))
                {
                    rDoors.Add(Direction.Left);
                    totalDoors++;
                }

            if (canDir.CanRight && r.Next(2) == 1)
                if (!rDoors.Contains(Direction.Right))
                {
                    rDoors.Add(Direction.Right);
                    totalDoors++;
                }

            if (canDir.CanFront && r.Next(2) == 1)
                if (!rDoors.Contains(Direction.Front))
                {
                    rDoors.Add(Direction.Front);
                    totalDoors++;
                }

            if (canDir.CanBack && r.Next(2) == 1)
                if (!rDoors.Contains(Direction.Back))
                {
                    rDoors.Add(Direction.Back);
                    totalDoors++;
                }

            DungeonRoom room = new DungeonRoom
            {
                Coordinates = coords,
                Type = randomType,
                RoomDoors = rDoors,
            };



            return room;
        }

        // Specific Type
        public DungeonRoom RoomGenerator(Coordinates coords, SaveData data, RoomType roomType)
        {
            if (data.Rooms.ContainsKey(Coordinates.CoordsToString(coords))) return data.Rooms[Coordinates.CoordsToString(coords)];

            DungeonRoom currentRoom = data.CurrentRoom;

            CanDirection canDir = new CanDirection();

            DungeonRoom roomR = null;
            DungeonRoom roomL = null;
            DungeonRoom roomF = null;
            DungeonRoom roomB = null;

            List<Direction> rDoors = new List<Direction>();

            if (data.Rooms.ContainsKey(Coordinates.CoordsToString(new Coordinates { X = coords.X + 1, Y = coords.Y })))
            {
                roomR = data.Rooms[Coordinates.CoordsToString(new Coordinates { X = coords.X + 1, Y = coords.Y })];
                if (roomR.RoomDoors.Contains(Direction.Left))
                {
                    canDir.CanRight = true;
                    if (Coordinates.CoordsToString(roomR.Coordinates) == Coordinates.CoordsToString(currentRoom.Coordinates))
                        rDoors.Add(Direction.Right);
                }
            }
            else canDir.CanRight = true;

            if (data.Rooms.ContainsKey(Coordinates.CoordsToString(new Coordinates { X = coords.X - 1, Y = coords.Y })))
            {
                roomL = data.Rooms[Coordinates.CoordsToString(new Coordinates { X = coords.X - 1, Y = coords.Y })];
                if (roomL.RoomDoors.Contains(Direction.Right))
                {
                    canDir.CanLeft = true;
                    if (Coordinates.CoordsToString(roomL.Coordinates) == Coordinates.CoordsToString(currentRoom.Coordinates))
                        rDoors.Add(Direction.Left);
                }
            }
            else canDir.CanLeft = true;

            if (data.Rooms.ContainsKey(Coordinates.CoordsToString(new Coordinates { X = coords.X, Y = coords.Y + 1 })))
            {
                roomF = data.Rooms[Coordinates.CoordsToString(new Coordinates { X = coords.X, Y = coords.Y + 1 })];
                if (roomF.RoomDoors.Contains(Direction.Back))
                {
                    canDir.CanFront = true;
                    if (Coordinates.CoordsToString(roomF.Coordinates) == Coordinates.CoordsToString(currentRoom.Coordinates))
                        rDoors.Add(Direction.Front);
                }
            }
            else canDir.CanFront = true;

            if (data.Rooms.ContainsKey(Coordinates.CoordsToString(new Coordinates { X = coords.X, Y = coords.Y - 1 })))
            {
                roomB = data.Rooms[Coordinates.CoordsToString(new Coordinates { X = coords.X, Y = coords.Y - 1 })];
                if (roomB.RoomDoors.Contains(Direction.Front))
                {
                    canDir.CanBack = true;
                    if (Coordinates.CoordsToString(roomB.Coordinates) == Coordinates.CoordsToString(currentRoom.Coordinates))
                        rDoors.Add(Direction.Back);
                }
            }
            else canDir.CanBack = true;
            int totalDoors = 0;

            if (canDir.CanLeft && r.Next(2) == 1)
                if (!rDoors.Contains(Direction.Left))
                {
                    rDoors.Add(Direction.Left);
                    totalDoors++;
                }

            if (canDir.CanRight && r.Next(2) == 1)
                if (!rDoors.Contains(Direction.Right))
                {
                    rDoors.Add(Direction.Right);
                    totalDoors++;
                }

            if (canDir.CanFront && r.Next(2) == 1)
                if (!rDoors.Contains(Direction.Front))
                {
                    rDoors.Add(Direction.Front);
                    totalDoors++;
                }

            if (canDir.CanBack && r.Next(2) == 1)
                if (!rDoors.Contains(Direction.Back))
                {
                    rDoors.Add(Direction.Back);
                    totalDoors++;
                }

            DungeonRoom room = new DungeonRoom
            {
                Coordinates = coords,
                Type = roomType,
                RoomDoors = rDoors,
            };

            return room;
        }

        // Randomly generates a gun then returns it.
        public Gun GunGenerator()
        {
            // Selects a random value from the Gun_ProjectileType enum.
            Array values = Enum.GetValues(typeof(DungeonWeaponParts.Gun_ProjectileType));
            DungeonWeaponParts.Gun_ProjectileType randomProj = (DungeonWeaponParts.Gun_ProjectileType)values.GetValue(r.Next(0, values.Length));


            Attributes projectile = new Attributes();

            // Selects different projectiles based on which enum was selected earlier.
            switch (randomProj)
            {
                case DungeonWeaponParts.Gun_ProjectileType.Bullet:
                    int proj1 = r.Next(0, DungeonWeaponParts.Gun_BulletCaliber.Count);
                    projectile = DungeonWeaponParts.Gun_BulletCaliber[proj1];
                    break;

                case DungeonWeaponParts.Gun_ProjectileType.Explosive:
                    int proj2 = r.Next(0, DungeonWeaponParts.Gun_ExplosiveType.Count);
                    projectile = DungeonWeaponParts.Gun_ExplosiveType[proj2];
                    break;

                case DungeonWeaponParts.Gun_ProjectileType.Primitive:
                    int proj3 = r.Next(0, DungeonWeaponParts.Gun_PrimitiveType.Count);
                    projectile = DungeonWeaponParts.Gun_PrimitiveType[proj3];
                    break;

                case DungeonWeaponParts.Gun_ProjectileType.Special:
                    int proj4 = r.Next(0, DungeonWeaponParts.Gun_SpecialType.Count);
                    projectile = DungeonWeaponParts.Gun_SpecialType[proj4];
                    break;
            }

            // Creates the gun.
            Gun rGun = new Gun
            {
                FireType = DungeonWeaponParts.Gun_FireType[r.Next(0, DungeonWeaponParts.Gun_FireType.Count)],
                BodyType = DungeonWeaponParts.Gun_BodyType[r.Next(0, DungeonWeaponParts.Gun_BodyType.Count)],
                ProjectileType = randomProj,
                Projectile = projectile,
            };

            rGun.Name = rGun.BodyType.NamePart + rGun.Projectile.NamePart;

            return rGun;
        }

        // Randomly generates a sword then returns it.
        public Sword SwordGenerator()
        {

            // Creates the sword. 
            Sword sword = new Sword
            {
                BladeLength = DungeonWeaponParts.Sword_BladeLength[r.Next(DungeonWeaponParts.Sword_BladeLength.Count)],
                BladeThickness = DungeonWeaponParts.Sword_BladeThickness[r.Next(DungeonWeaponParts.Sword_BladeThickness.Count)],
                BladeType = DungeonWeaponParts.Sword_BladeType[r.Next(DungeonWeaponParts.Sword_BladeType.Count)],
                HiltGrip = DungeonWeaponParts.Sword_HiltGrip[r.Next(DungeonWeaponParts.Sword_HiltGrip.Count)],
                HiltSize = DungeonWeaponParts.Sword_HiltSize[r.Next(DungeonWeaponParts.Sword_HiltSize.Count)],
                HiltType = DungeonWeaponParts.Sword_HiltType[r.Next(DungeonWeaponParts.Sword_HiltType.Count)],
                WeightType = DungeonWeaponParts.Sword_WeightType[r.Next(DungeonWeaponParts.Sword_WeightType.Count)]
            };

            return sword;
        }

        // Randomly generates an enemy then return it.
        public Enemy EnemyGenerator()
        {

            Enemy enemy = new Enemy()
            {
                Name = "Placeholder Man",
                Health = r.Next(2,11), // Have this scale up with difficulty later.
                StunLeft = 0,

                Stats = new Stats()
                {
                    AccuracyIncrease = r.Next(-10, 11),
                    DamageIncrease = r.Next(-10, 16),
                    CriticalChanceIncrease = r.Next(-10, 11),
                    StunChanceIncrease = r.Next(-10, 11),
                    StunAmountIncrease = r.Next(-10, 11)
                },

                
            };

            return enemy;
        }
    }
    public enum RoomType { Empty, Loot, Enemy, Boss };

    public enum Direction { Front, Back, Left, Right };


}
