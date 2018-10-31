using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mad_Bot_Discord.Modules.DungeonGame
{
    class DungeonMethods
    {
        static string[] attacks;
        static Random rnd = new Random();

        static DungeonMethods()
        {
            string attacksJson = File.ReadAllText("SystemLang/dgattacks.json");
            attacks = JsonConvert.DeserializeObject<string[]>(attacksJson);
        }

        static string enemyWeapon = "a knife";
        // make this better later

        /// <summary>
        /// Attacks the given entity.
        /// </summary>
        /// <param name="entity">The entity to attack.</param>
        /// <param name="damage">The damage to be dealt.</param>
        public static void DealDamage(Entity entity, float damage)
        {
            entity.Health -= damage;
        }

    }
}
