using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Mad_Bot_Discord.Modules
{
    public class Battle : ModuleBase<SocketCommandContext>
    {

        Random r = new Random();

        [Command("Give")]
        public async Task Give([Remainder] string item)
        {
            if (item.Length > 50)
            {
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Give Failed", $"The item name is {item.Length} characters long, the max is 50.", Context));
                return;
            }

            if (BattleStats.HasItem(item))
            {
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Give Failed", BattleStats.stats.Name + " already has " + item + "!", Context));
                return;
            }

            if (BattleStats.HasItem("Fists"))
                BattleStats.stats.Items.Remove("Fists");

            BattleStats.stats.Items.Add(item);
            BattleStats.SaveStats();

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Give for " + Context.User.Username, "You have given " + BattleStats.stats.Name + " " + item + "!", Context));

        }

        [Command("Drop")]
        public async Task Drop([Remainder] string item)
        {
            foreach (string i in BattleStats.stats.Items)
            {
                if (i.ToLower() == item.ToLower())
                {
                    BattleStats.stats.Items.Remove(i);
                    if (BattleStats.stats.Items.Count == 0)
                        BattleStats.stats.Items.Add("Fists");
                    BattleStats.SaveStats();

                    await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Drop for " + Context.User.Username, item + " has been removed from " + BattleStats.stats.Name + "'s inventory.", Context));
                    return;
                }
            }

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Drop Failed", BattleStats.stats.Name + " does not have " + item + "!", Context));
        }

        [Command("Rename")]
        public async Task Rename([Remainder] string name)
        {
            if (name.Length > 50)
            {
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Rename Failed" + Context.User.Username, "That name is too long, the maximum length is 50.", Context));
                return;
            }

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Rename for " + Context.User.Username, BattleStats.stats.Name + " has been renamed to " + name + "!", Context));
            BattleStats.stats.Name = name;
            BattleStats.SaveStats();

        }

        [Command("Spawn")]
        public async Task Spawn([Remainder] string name)
        {
            if (name.Length > 50)
            {
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Spawn Failed", $"The enemy name is {name.Length} characters long, the max is 50.", Context));
                return;
            }

            if (BattleStats.HasEnemy(name))
            {
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Spawn Failed", name + " already exists!", Context));
                return;
            }

            if (BattleStats.HasEnemy("Air"))
                BattleStats.stats.Enemies.Remove("Air");

            BattleStats.stats.Enemies.Add(name);
            BattleStats.SaveStats();

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Spawn for " + Context.User.Username, "You have spawned " + name + ". " + BattleStats.stats.Name + " can now fight it!", Context));
            return;
        }

        [Command("Kill")]
        public async Task Kill([Remainder] string name)
        {
            foreach (string e in BattleStats.stats.Enemies)
            {
                if (e.ToLower() == name.ToLower())
                {
                    BattleStats.stats.Enemies.Remove(e);
                    if (BattleStats.stats.Enemies.Count == 0)
                        BattleStats.stats.Enemies.Add("Air");
                    BattleStats.SaveStats();

                    await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Kill for " + Context.User.Username, name + " has been killed.", Context));
                    return;
                }
            }

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Kill Failed", name + " does not exist!", Context));
        }

        [Command("Fight")]
        public async Task Fight()
        {
            string s;

            string json = File.ReadAllText("SystemLang/attacks.json");
            List<string> moves = JsonConvert.DeserializeObject<List<string>>(json);

            if (BattleStats.stats.InFight == false)
            {
                BattleStats.stats.InFight = true;
                BattleStats.stats.CurrentEnemy = BattleStats.stats.Enemies[r.Next(0, BattleStats.stats.Enemies.Count)];
                BattleStats.stats.EnemyHP = r.Next(15, 126);

                int damageDealt = r.Next(5, 26);
                BattleStats.stats.EnemyHP -= damageDealt;
                BattleStats.SaveStats();

                s = $"{BattleStats.stats.Name} has encountered {BattleStats.stats.CurrentEnemy}!";
                s = s + "\n...\n";
                s = s + String.Format(moves[r.Next(0, moves.Count)], BattleStats.stats.Name, BattleStats.stats.CurrentEnemy, BattleStats.stats.Items[r.Next(0, BattleStats.stats.Items.Count)]);
                s = s + "\n...\n";

                if (BattleStats.stats.EnemyHP < 1)
                {
                    s = s + $"{BattleStats.stats.Name} has killed {BattleStats.stats.CurrentEnemy} in one hit!";
                    BattleStats.stats.InFight = false;

                    await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Enemy Killed", s, Context));
                    BattleStats.SaveStats();

                    return;
                }

                s = s + $"{BattleStats.stats.Name} has dealt {damageDealt} damage to {BattleStats.stats.CurrentEnemy}! {BattleStats.stats.CurrentEnemy} has {BattleStats.stats.EnemyHP} HP left!";
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Enemy Damaged", s, Context));
                return;
            }

            int damage = r.Next(5, 26);
            BattleStats.stats.EnemyHP -= damage;
            BattleStats.SaveStats();

            s = String.Format(moves[r.Next(0, moves.Count)], BattleStats.stats.Name, BattleStats.stats.CurrentEnemy, BattleStats.stats.Items[r.Next(0, BattleStats.stats.Items.Count)]);
            s = s + "\n...\n";

            if (BattleStats.stats.EnemyHP < 1)
            {
                s = s + $"{BattleStats.stats.Name} has dealt {damage} damage and killed {BattleStats.stats.CurrentEnemy}!";
                BattleStats.stats.InFight = false;

                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Enemy Killed", s, Context));
                BattleStats.SaveStats();

                return;
            }

            s = s + $"{BattleStats.stats.Name} has dealt {damage} damage to {BattleStats.stats.CurrentEnemy}! {BattleStats.stats.CurrentEnemy} has {BattleStats.stats.EnemyHP} HP left!";
            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Enemy Damaged", s, Context));
        }

        [Command("Flee")]
        public async Task Flee()
        {
            if (BattleStats.stats.InFight)
            {
                BattleStats.stats.InFight = false;
                BattleStats.SaveStats();
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Run Away", $"{BattleStats.stats.Name} has run away from {BattleStats.stats.CurrentEnemy}!", Context));
            }
            else
                await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Flee Failed", $"{BattleStats.stats.Name} is not in a battle!", Context));
        }

    }
}
