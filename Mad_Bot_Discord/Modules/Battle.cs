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

        // Items -----------------------------------

        [Command("Give")]
        public async Task Give(string item)
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

            BattleStats.stats.Items.Add(item);
            BattleStats.SaveStats();

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Give for " + Context.User.Username, "You have given " + BattleStats.stats.Name + " " + item + "!", Context));

        }

        [Command("Drop")]
        public async Task Drop(string item)
        {
            foreach (string i in BattleStats.stats.Items)
            {
                if (i.ToLower() == item.ToLower())
                {
                    BattleStats.stats.Items.Remove(i);
                    BattleStats.SaveStats();

                    await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Drop for " + Context.User.Username, item + " has been removed from " + BattleStats.stats.Name + "'s inventory.", Context));
                    return;
                }
            }

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Drop Failed", BattleStats.stats.Name + " does not have " + item + "!", Context));
        }

        // Items -----------------------------------
        


        // Name ------------------------------------

        [Command("Rename")]
        public async Task Rename(string name)
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

        // Name ------------------------------------



        // Enemies ---------------------------------

        [Command("Spawn")]
        public async Task Spawn(string name)
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

            BattleStats.stats.Enemies.Add(name);
            BattleStats.SaveStats();

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Spawn for " + Context.User.Username, "You have spawned " + name + ". " + BattleStats.stats.Name + " can now fight it!", Context));
            return;
        }

        [Command("Kill")]
        public async Task Kill(string name)
        {
            foreach (string e in BattleStats.stats.Enemies)
            {
                if (e.ToLower() == name.ToLower())
                {
                    BattleStats.stats.Enemies.Remove(e);
                    BattleStats.SaveStats();

                    await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Kill for " + Context.User.Username, name + " has been killed.", Context));
                    return;
                }
            }

            await Context.Channel.SendMessageAsync("", embed: Utilities.EasyEmbed("Kill Failed", name + " does not exist!", Context));
        }

        // Enemies ---------------------------------
    }
}
