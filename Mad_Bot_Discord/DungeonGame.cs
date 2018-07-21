using Discord.Commands;
using Discord.WebSocket;
using Mad_Bot_Discord.Core.UserAccounts;
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
    /// To Do Next: Add Dictionaries for everything.
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
        public string tutorialMessage = $"Reply with `{Config.bot.cmdPrefix}dg <direction>` to move, `{Config.bot.cmdPrefix}dg map` to view the map" +
            $", `{Config.bot.cmdPrefix}dg inv` to view your inventory, `{Config.bot.cmdPrefix}dg types` to view what room types do/appear like on the map, " +
            $"or `{Config.bot.cmdPrefix}dg leave` to leave the dungeon and finalize your score.";

        private static List<SaveData> saves;
        static string savePath = "Resources/dgsaves.json";

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
            saves = JsonConvert.DeserializeObject<List<SaveData>>(json);
        }
        SaveData CreateSaveData(ulong id)
        {
            SocketGuildUser user = (SocketGuildUser)Context.User;

            var newSave = new SaveData() { User = UserAccounts.GetAccount(user) };
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
        // --- Data saving and loading.

        Random r = new Random();

        [Command("Dungeon"), Alias("dg")]
        public async Task Dungeon([Remainder] string options = "")
        {

            // Tests if the save file exists.
            if (!File.Exists(savePath))
            {
                // If it doesn't, create it.
                UserAccount bot = UserAccounts.GetAccount(Context.Client.CurrentUser);

                saves = new List<SaveData> { new SaveData { User = bot } };
                string json = JsonConvert.SerializeObject(saves, Formatting.Indented);
                File.WriteAllText(savePath, json);
            }
            else
            {
                // If it does, read it and save it to a variable.
                string json = File.ReadAllText(savePath);
                
                saves = JsonConvert.DeserializeObject<List<SaveData>>(json);
            }

            // Gets any arguments provided.
            string[] args = { };
            if (!string.IsNullOrWhiteSpace(options)) args = options.Split(' ');

            // Gets the users save data.
            SaveData data = GetData(Context.User.Id);

            // Starts a variable that stores what mb will say to you.
            string sentence = "";

            // If the user doesn't have any save data, set up the game.
            if (!data.InGame)
            {

                // Creates a starting sword.
                Sword startingSword = new Sword()
                {
                    BladeLength = Sword_BladeLength[0],
                    BladeThickness = Sword_BladeThickness[0],
                    BladeType = Sword_BladeType[1],
                    HiltGrip = Sword_HiltGrip[0],
                    HiltSize = Sword_HiltSize[0],
                    HiltType = Sword_HiltType[1],
                    WeightType = Sword_WeightType[0],
                    Name = Sword_BladeType[1].NamePart + Sword_BladeThickness[0].NamePart + Sword_HiltType[1].NamePart + Sword_HiltSize[0].NamePart
                };

                // Sets the equipped weapon to the starting sword.
                data.EquippedWeapon = startingSword;

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
                data.Inventory = new Weapons { Swords = new List<Sword> { startingSword }, Guns = new List<Gun> { } };
                
                // Makes sure they arent in a fight.
                data.InAFight = false;

                // Saves all that new data.
                SaveData();
            }

            // Tests if the player is in a fight.
            if (!data.InAFight)
            {
                if (args.Length > 0)
                {
                    switch (args[0].ToLower())
                    {
                        case "left":
                            if (data.CurrentRoom.RoomDoors.Contains(Direction.Left))
                            {
                                

                                data.CurrentRoom = RoomGenerator(new Coordinates { X = data.CurrentRoom.Coordinates.X - 1, Y = data.CurrentRoom.Coordinates.Y }, data);
                                sentence = sentence + $"Current Room Type: [{data.CurrentRoom.Type}] \n\n";

                                if (data.CurrentRoom.Type == RoomType.Enemy)
                                {
                                    data.CurrentEnemy = EnemyGenerator();
                                    data.InAFight = true;

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

                        case "right":
                            if (data.CurrentRoom.RoomDoors.Contains(Direction.Right))
                            {
                                data.CurrentRoom = RoomGenerator(new Coordinates { X = data.CurrentRoom.Coordinates.X + 1, Y = data.CurrentRoom.Coordinates.Y }, data);
                                sentence = sentence + $"Current Room Type: [{data.CurrentRoom.Type}] \n\n";

                                if (data.CurrentRoom.Type == RoomType.Enemy)
                                {
                                    data.CurrentEnemy = EnemyGenerator();
                                    data.InAFight = true;

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

                        case "front":
                        case "up":
                        case "forward":
                        case "forwards":
                            if (data.CurrentRoom.RoomDoors.Contains(Direction.Front))
                            {
                                data.CurrentRoom = RoomGenerator(new Coordinates { X = data.CurrentRoom.Coordinates.X, Y = data.CurrentRoom.Coordinates.Y + 1 }, data);
                                sentence = sentence + $"Current Room Type: [{data.CurrentRoom.Type}] \n\n";

                                if (data.CurrentRoom.Type == RoomType.Enemy)
                                {
                                    data.CurrentEnemy = EnemyGenerator();
                                    data.InAFight = true;

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

                        case "back":
                        case "down":
                        case "backward":
                        case "backwards":
                            if (data.CurrentRoom.RoomDoors.Contains(Direction.Back))
                            {
                                data.CurrentRoom = RoomGenerator(new Coordinates { X = data.CurrentRoom.Coordinates.X, Y = data.CurrentRoom.Coordinates.Y - 1}, data);
                                sentence = sentence + $"Current Room Type: [{data.CurrentRoom.Type}] \n\n";

                                if (data.CurrentRoom.Type == RoomType.Enemy)
                                {
                                    if (!data.Rooms.ContainsKey(Coordinates.CoordsToString(data.CurrentRoom.Coordinates)))
                                    {
                                        data.CurrentEnemy = EnemyGenerator();
                                        data.InAFight = true;

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

                        case "map":
                            sentence = sentence + $"Current Room Type: [{data.CurrentRoom.Type}] \n\n";

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
                        case "inv":
                        case "inventory":
                            sentence = sentence + $"Current Room Type: [{data.CurrentRoom.Type}] \n\n";

                            string tba = "Your current inventory:\n\n**Swords:** \n\n";
                            Weapons w = data.Inventory;

                            if (w.Swords.Count > 0)
                            {
                                foreach (Sword s in w.Swords)
                                {
                                    tba = tba + s.Name + $" [Damage: {s.Damage} | Accuracy: {s.Accuracy} | Stun Chance: {s.StunChance}0% | Stun Amount: {s.StunAmount} Turns] \n";
                                }
                            }
                            else tba = tba + "None!\n";
                            tba = tba + "\n**Guns:** \n\n";


                            if (w.Guns.Count > 0)
                            {
                                foreach (Gun g in w.Guns)
                                {
                                    tba = tba + g.Name + $" [Damage: {g.Damage} | Accuracy: {g.Accuracy} | Stun Chance: {g.StunChance}0% | Stun Amount: {g.StunAmount} Turns] \n";
                                }
                            }
                            else tba = tba + "None!\n";

                            sentence = sentence + tba + "\n\n";

                            break;

                        case "leave":
                        case "exit":
                        case "quit":
                            string[] encouragingWords = { "Wow, you", "Incredible! You", "Good job, you", "Nice work, you", "That's crazy! You", "Meh, you", "**yawn** you" };
                            string word = encouragingWords[r.Next(encouragingWords.Length)];

                            sentence = $"You left the dungeon. {word} got [0] score.";
                            await Context.Channel.SendMessageAsync(sentence);
                            data.InGame = false;
                            SaveData();
                            return;

                        case "types":
                            sentence = sentence + $"Current Room Type: [{data.CurrentRoom.Type}] \n\n";

                            sentence = sentence + "Room Types:\n\nEmpty: An empty room. Appears as `[ ]` in the map.\nEnemy: A room that contains enemies. Appears as `[#]` in the map.\n" +
                                "Loot: A room that gives you random loot. Appears as `[$]` in the map.\nBoss: A room that contains a boss. Appears as `[!]` in the map.\n" +
                                "You: You appear as `<@>` on the map.\n\n";

                            await Context.Channel.SendMessageAsync(sentence);
                            return;
                    }
                }



                if (data.CurrentRoom.RoomCount > 1)
                {

                    string one = "";
                    for (int i = 0; i < data.CurrentRoom.RoomCount; i++)
                    {

                        if (i == data.CurrentRoom.RoomCount - 1)
                        {
                            one = one + ", and one to your " + data.CurrentRoom.RoomDoors[i].ToString().ToLower() + ".";

                        }
                        else
                        {
                            one = one + ", one to your " + data.CurrentRoom.RoomDoors[i].ToString().ToLower();

                        }
                    }
                    sentence = sentence + $"There are {data.CurrentRoom.RoomCount} rooms{one} {tutorialMessage}";

                }
                else
                {
                    sentence = sentence = $"There is one room to your {data.CurrentRoom.RoomDoors[0].ToString().ToLower()}. {tutorialMessage}";
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

                            double doingDamage = (double)data.EquippedWeapon.Damage + r.Next(-1, 5); ;
                            sentence = sentence + "You attack the " + data.CurrentEnemy.Name + " with your " + data.EquippedWeapon.Name + " dealing " + doingDamage + " damage!";
                            data.CurrentEnemy.Health -= doingDamage;

                            if (data.EquippedWeapon.StunChance > 0)
                            {
                                if (r.Next(0, 10) <= data.EquippedWeapon.StunChance)
                                {
                                    data.CurrentEnemy.StunLeft = data.EquippedWeapon.StunAmount + 1;
                                    sentence = sentence + " It was stunned for " + data.EquippedWeapon.StunAmount + " turns!";
                                }

                            }

                            if (data.CurrentEnemy.Health < 0) data.CurrentEnemy.Health = 0;

                            sentence = sentence + "\n\n" + data.CurrentEnemy.Name + " has " + data.CurrentEnemy.Health + " HP left!";

                            int eTurns = (data.StunLeft == 0) ? 1 : data.StunLeft + 1;

                            if (data.CurrentEnemy.Health <= 0)
                            {
                                sentence = sentence + $"\n\nYou defeated the {data.CurrentEnemy.Name} and gained [0] gold!";
                                data.CurrentRoom.Type = RoomType.Empty;
                                data.Rooms[Coordinates.CoordsToString(data.CurrentRoom.Coordinates)].Type = RoomType.Empty;
                                data.InAFight = false;

                                await Context.Channel.SendMessageAsync(sentence);

                                SaveData();
                                return;
                            }

                            if (data.CurrentEnemy.StunLeft < 1)
                            {
                                for (int i = 0; i < eTurns; i++)
                                {
                                    double damageDone = data.CurrentEnemy.Damage + r.Next(-3, 4);
                                   
                                    if (damageDone < 1)
                                    {
                                        damageDone = 0;
                                        data.Health -= damageDone;
                                        sentence = sentence + $"\n\n ... \n\n { data.CurrentEnemy.Name} missed! You have {data.Health} HP!";
                                    }
                                    else
                                    {
                                        data.Health -= damageDone;
                                        sentence = sentence + $"\n\n ... \n\n {data.CurrentEnemy.Name} hit you and dealt {damageDone} damage! You now have {data.Health} HP!";
                                    }

                                    if (r.Next(0, 10) <= data.CurrentEnemy.StunChance && damageDone != 0)
                                    {
                                        sentence = sentence + $" You were stunned for {data.CurrentEnemy.StunAmount} turns!";
                                        data.StunLeft = data.CurrentEnemy.StunAmount;
                                    }

                                    if (data.StunLeft > 0)
                                        sentence = sentence + $"\n\n{data.StunLeft} turns until stun wears off!";

                                    if (data.Health <= 0)
                                    {
                                        sentence = sentence + $"You died! You got [0] score!";
                                        data.InGame = false;
                                        SaveData();
                                        return;
                                    }
                                }
                            }
                            else
                                data.CurrentEnemy.StunLeft--;

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
                            sentence = sentence + $"You currently have `{data.Health}` HP and have the {data.EquippedWeapon.Name} [Damage: {data.EquippedWeapon.Damage}] equipped.";
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
        }

        /*            string weapon = $"You find a {rGun.FireType.Name} {rGun.BodyType.Name} that fires {rGun.Projectile.Name}. It is labelled *\"{rGun.Name}\"*\n" +
                $"Total Stats: [Damage: {rGun.FireType.Damage + rGun.BodyType.Damage + rGun.Projectile.Damage} | Accuracy: {rGun.FireType.Accuracy + rGun.BodyType.Accuracy + rGun.Projectile.Accuracy}" +
                $" | Stun Chance: {rGun.Projectile.StunChance}0% | Stun Amount: {rGun.Projectile.StunAmount} Rounds]";
                */

        // --- Gun
        public enum Gun_ProjectileType { Bullet, Explosive, Primitive, Special };
        public Dictionary<int, Attributes> Gun_FireType = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "Bolt-Action", Accuracy = 3, Damage = 3 } },
            { 1, new Attributes { Name = "Pump-Action", Accuracy = -1, Damage = 2 } },
            { 2, new Attributes { Name = "Semi-Auto", Accuracy = 1, Damage = 4 } },
            { 3, new Attributes { Name = "Full-Auto", Accuracy = -2, Damage = 6} }
        };
        public Dictionary<int, Attributes> Gun_BodyType = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "Pistol", NamePart = "Handi", Accuracy = -1 } },
            { 1, new Attributes { Name = "Revolver", NamePart = "Revol", Accuracy = 2 } },
            { 2, new Attributes { Name = "SMG", NamePart = "Subma", Accuracy = -2 } },
            { 3, new Attributes { Name = "Rifle", NamePart = "Ri", Accuracy = 5} },
            { 4, new Attributes { Name = "Shotgun", NamePart = "Shot", Accuracy = 3} },
            { 5, new Attributes { Name = "Minigun", NamePart = "Mini", Accuracy = -5} },
            { 6, new Attributes { Name = "RPG", NamePart = "Ro", Accuracy = 7 } }
        };
        public Dictionary<int, Attributes> Gun_BulletCaliber = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "9mm bullets", NamePart = "nind", Damage = 2, Accuracy = 2} },
            { 1, new Attributes { Name = ".45 ACP bullets", NamePart = "llet", Damage = 3, Accuracy = 1} },
            { 2, new Attributes { Name = "7.62 x 39mm bullets", NamePart = "47", Damage = 4, Accuracy = 4} },
            { 3, new Attributes { Name = "12 Gauge Birdshot shells", NamePart = "shot", Damage = 3, Accuracy = -2} },
            { 4, new Attributes { Name = "12 Gauge Buckshot shells", NamePart = "uck", Damage = 5, Accuracy = -3} },
            { 5, new Attributes { Name = "12 Gauge Slug shells", NamePart = "snail", Damage = 7, Accuracy = -5 } },
            { 6, new Attributes { Name = ".50 BMG bullets", NamePart = "barr", Damage = 10, Accuracy = -2} }
        };
        public Dictionary<int, Attributes> Gun_ExplosiveType = new Dictionary<int, Attributes>
        {
            {0, new Attributes { Name = "Dynamite", NamePart = "nyte", Damage = 10, Accuracy = 10} },
            {1, new Attributes { Name = "Frag Grenades", NamePart = "napple", Damage = 5, Accuracy = 7 } },
            {2, new Attributes { Name = "Stun Grenades", NamePart = "nade", StunChance = 8, StunAmount = 2, Accuracy = 7 } },
            {3, new Attributes { Name = "Smoke Grenades", NamePart = "dust", StunChance = 6, Accuracy = 10} },
            {4, new Attributes { Name = "Impact Grenades", NamePart = "pack", Damage = 6, Accuracy = 4} },
            {5, new Attributes { Name = "Random Fireworks", NamePart = "sparkle", Damage = 6} }
        };
        public Dictionary<int, Attributes> Gun_PrimitiveType = new Dictionary<int, Attributes>
        {
            {0, new Attributes { Name = "Rocks", NamePart = "stone", Damage = 2, StunChance = 3, StunAmount = 1, Accuracy = 7} },
            {1, new Attributes { Name = "Crystals", NamePart = "shine", Damage = 6, StunChance = 1, StunAmount = 1, Accuracy = 6} },
            {2, new Attributes { Name = "Wood", NamePart = "ode", Damage = 1, StunChance = 4, StunAmount = 1, Accuracy = 6} },
            {3, new Attributes { Name = "Dirt", NamePart = "own", Damage = 0, StunChance = 6, StunAmount = 2, Accuracy = 4} }
        };
        public Dictionary<int, Attributes> Gun_SpecialType = new Dictionary<int, Attributes>
        {
            {0, new Attributes { Name = "Lasers", NamePart = "inator5000", Damage = 2, Accuracy = 10} },
            {1, new Attributes { Name = "Cats", NamePart = "line", Damage = 6, Accuracy = 2, StunChance = 8, StunAmount = 1 } }
        };
        // --- Gun

        // --- Sword
        public Dictionary<int, Attributes> Sword_WeightType = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "Light", Damage = -2, Accuracy = 3} },
            { 1, new Attributes { Name = "Medium", Damage = 1, Accuracy = 1 } },
            { 2, new Attributes { Name = "Heavy", Damage = 3, Accuracy = -2 } }
        };
        public Dictionary<int, Attributes> Sword_BladeType = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "Stab", NamePart = "Stabi", Damage = -2, Accuracy = 4} },
            { 1, new Attributes { Name = "Slice", NamePart = "Slici", Damage = 2, Accuracy = -2} }
        };
        public Dictionary<int, Attributes> Sword_BladeThickness = new Dictionary<int, Attributes>
        {
            { 0, new Attributes {Name = "Thin", NamePart = "xno", Damage = -2, Accuracy = 3} },
            { 1, new Attributes {Name = "Normal", NamePart = "de", Accuracy = 1 } },
            { 2, new Attributes {Name = "Thick", NamePart = "thi", Damage = 3, Accuracy = -2} }
        };
        public Dictionary<int, Attributes> Sword_BladeLength = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "Short", Damage = 0, Accuracy = -2} },
            { 1, new Attributes { Name = "Normal", Damage = 1} },
            { 2, new Attributes { Name = "Long", Damage = 2, Accuracy = 1} }
        };
        public Dictionary<int, Attributes> Sword_HiltType = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "Covered", Accuracy = 2 } },
            { 1, new Attributes { Name = "Exposed", Damage = 2 } }
        };
        public Dictionary<int, Attributes> Sword_HiltSize = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "One Handed", NamePart = "sin", Damage = -2, Accuracy = 3 } },
            { 1, new Attributes { Name = "Two Handed", NamePart = "duo", Damage = 2, Accuracy = -2} },
            { 2, new Attributes { Name = "Three Handed", NamePart = "tric", Damage = 4, Accuracy = -3 } }
        };
        public Dictionary<int, Attributes> Sword_HiltGrip = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "None", Accuracy = -3, Damage = -2 } },
            { 1, new Attributes { Name = "Bad", Accuracy = -1} },
            { 2, new Attributes { Name = "Normal", Accuracy = 2} },
            { 3, new Attributes { Name = "Great", Accuracy = 4, Damage = 2} }
        };

        // --- Sword

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
            Array values = Enum.GetValues(typeof(Gun_ProjectileType));
            Gun_ProjectileType randomProj = (Gun_ProjectileType)values.GetValue(r.Next(0, values.Length));


            Attributes projectile = new Attributes();

            // Selects different projectiles based on which enum was selected earlier.
            switch (randomProj)
            {
                case Gun_ProjectileType.Bullet:
                    int proj1 = r.Next(0, Gun_BulletCaliber.Count);
                    projectile = Gun_BulletCaliber[proj1];
                    break;

                case Gun_ProjectileType.Explosive:
                    int proj2 = r.Next(0, Gun_ExplosiveType.Count);
                    projectile = Gun_ExplosiveType[proj2];
                    break;

                case Gun_ProjectileType.Primitive:
                    int proj3 = r.Next(0, Gun_PrimitiveType.Count);
                    projectile = Gun_PrimitiveType[proj3];
                    break;

                case Gun_ProjectileType.Special:
                    int proj4 = r.Next(0, Gun_SpecialType.Count);
                    projectile = Gun_SpecialType[proj4];
                    break;
            }

            // Creates the gun.
            Gun rGun = new Gun
            {
                FireType = Gun_FireType[r.Next(0, Gun_FireType.Count)],
                BodyType = Gun_BodyType[r.Next(0, Gun_BodyType.Count)],
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
                BladeLength = Sword_BladeLength[r.Next(Sword_BladeLength.Count)],
                BladeThickness = Sword_BladeThickness[r.Next(Sword_BladeThickness.Count)],
                BladeType = Sword_BladeType[r.Next(Sword_BladeType.Count)],
                HiltGrip = Sword_HiltGrip[r.Next(Sword_HiltGrip.Count)],
                HiltSize = Sword_HiltSize[r.Next(Sword_HiltSize.Count)],
                HiltType = Sword_HiltType[r.Next(Sword_HiltType.Count)],
                WeightType = Sword_WeightType[r.Next(Sword_WeightType.Count)]
            };

            return sword;
        }

        // Randomly generates an enemy then return it.
        public Enemy EnemyGenerator()
        {
            bool willStun = false;
            if (r.Next(0, 3) == 0)
                willStun = true;

            Enemy enemy = new Enemy()
            {
                Accuracy = r.Next(7) + r.Next(-4, 6),
                Damage = r.Next(15) + r.Next(-10, 11),
                Health = r.Next(2,11),
                StunAmount = r.Next(0, 4),
                StunChance = 0,
                StunLeft = 0,
                Name = "Placeholder Man"
            };

            if (willStun) enemy.StunChance = r.Next(0, 8);

            return enemy;
        }
    }
    public enum RoomType { Empty, Loot, Enemy, Boss };

    public enum Direction { Front, Back, Left, Right };
    // Add staff and bow later.
    public enum WeaponType { Sword, Gun, /*Staff, Bow*/
    };

    public class Gun
    {
        public string Name { get; set; } = "Gun";
        public Attributes FireType { get; set; }
        public Attributes BodyType { get; set; }
        public Enum ProjectileType { get; set; }
        public Attributes Projectile { get; set; }

        public double Damage
        {
            get
            {
                if (Name != "Gun")
                {
                    double d = FireType.Damage + BodyType.Damage + Projectile.Damage;

                    if (d <= 0) d = 1;

                    return d;
                }
                else return 0;
            }
        }

        public double Accuracy
        {
            get
            {
                if (Name != "Gun")
                {
                    double a = FireType.Accuracy + BodyType.Accuracy + Projectile.Accuracy;

                    return a;
                }
                else return 0;
            }
        }

        public double StunChance
        {
            get
            {
                if (Name != "Gun") return Projectile.StunChance;
                else return 0;
            }
        }

        public double StunAmount
        {
            get
            {
                if (Name != "Gun") return Projectile.StunAmount;
                else return 0;
            }
        }
    }

    public class Sword
    {
        public string Name { get; set; } = "Sword";
        public Attributes WeightType { get; set; } = null;
        public Attributes BladeType { get; set; } = null;
        public Attributes BladeThickness { get; set; } = null;
        public Attributes BladeLength { get; set; } = null;
        public Attributes HiltType { get; set; } = null;
        public Attributes HiltGrip { get; set; } = null;
        public Attributes HiltSize { get; set; } = null;

        public double Damage {
            get
            {


                if (Name != "Sword")
                {
                    double d = WeightType.Damage + BladeType.Damage + BladeThickness.Damage + BladeLength.Damage + HiltType
                        .Damage + HiltGrip.Damage + HiltSize.Damage;

                    if (d <= 0) d = 1;
                        return d;
                }
                else return 0;
            }
        }

        public double Accuracy
        {
            get
            {
                if (Name != "Sword")
                    return WeightType.Accuracy + BladeType.Accuracy + BladeThickness.Accuracy + BladeLength.Accuracy + HiltType
                        .Accuracy + HiltGrip.Accuracy + HiltSize.Accuracy;
                else return 0;
            }
        }

        public double StunAmount
        {
            get
            {
                if (Name != "Sword")
                    return WeightType.StunAmount + BladeType.StunAmount + BladeThickness.StunAmount + BladeLength.StunAmount + HiltType.StunAmount
                        + HiltGrip.StunAmount + HiltSize.StunAmount;
                else return 0;
            }
        }

        public double StunChance
        {
            get
            {
                if (Name != "Sword")
                    return WeightType.StunChance + BladeType.StunChance + BladeThickness.StunChance + BladeLength.StunChance + HiltType.StunChance
                        + HiltGrip.StunChance + HiltSize.StunChance;
                else return 0;
            }
        }

        
        
    }

    public class Attributes
    {
        public string Name { get; set; } = null;
        public string NamePart { get; set; } = null;
        public double Damage { get; set; } = 0;
        public int StunChance { get; set; } = 0;
        public int StunAmount { get; set; } = 0;
        public int Accuracy { get; set; } = 0;
    }

    public class Coordinates
    {
        internal static string CoordsToString(double X1, double Y1)
        {
            return X1.ToString() + "|" + Y1.ToString();
        }

        internal static string CoordsToString(Coordinates coords)
        {
            return coords.X.ToString() + "|" + coords.Y.ToString();
        }

        internal static Coordinates StringToCoords(string coords)
        {
            string[] splits = coords.Split('|');


            if (!double.TryParse(splits[0], out double X1))
                return null;

            if (!double.TryParse(splits[1], out double Y1))
                return null;

            return new Coordinates { X = X1, Y = Y1 };
        }

        public double X { get; set; }
        public double Y { get; set; }
    }

    public class DungeonRoom
    {
        public Coordinates Coordinates { get; set; }
        public List<Direction> RoomDoors { get; set; }
        public int RoomCount
        {
            get
            {
                return RoomDoors.Count;
            }
        }
        public RoomType Type { get; set; }
    }

    public class Enemy
    {
        public string Name { get; set; } = "Enemy";
        public double Health { get; set; } = 0;
        public double Damage { get; set; } = 0;
        public double Accuracy { get; set; } = 0;
        public double StunChance { get; set; } = 0;
        public int StunAmount { get; set; } = 0;

        // Effects

        public int StunLeft { get; set; } = 0;
    }

    public class Weapons
    {
        public List<Sword> Swords { get; set; }
        public List<Gun> Guns { get; set; }
    }

    public class SaveData
    {
        public bool InGame { get; set; } = false;
        public double Health { get; set; } = 100;
        public UserAccount User { get; set; } = null;
        public DungeonRoom CurrentRoom { get; set; } = null;
        public Coordinates Coordinates {
            get
            {
                if (CurrentRoom != null)
                    return CurrentRoom.Coordinates;
                else
                    return new Coordinates { X = 0, Y = 0 };
            }
        }
        public Dictionary<string, DungeonRoom> Rooms { get; set; } = null;
        public Weapons Inventory { get; set; } = null;
        public dynamic EquippedWeapon { get; set; } = null;
        public bool InAFight { get; set; } = false;
        public Enemy CurrentEnemy { get; set; } = null;

        // Effects

        public int StunLeft { get; set; } = 0;

        // 0 = players turn -- 1 = enemy's turn
        public int WhoseTurn { get; set; } = 0;
    }

    public class CanDirection
    {
        public bool CanLeft { get; set; } = false;
        public bool CanRight { get; set; } = false;
        public bool CanFront { get; set; } = false;
        public bool CanBack { get; set; } = false;
    }
}
