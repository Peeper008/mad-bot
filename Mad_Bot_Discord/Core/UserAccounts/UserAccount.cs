using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mad_Bot_Discord.Core.UserAccounts
{
    public class UserAccount
    {
        // name value for debug purposes
        public string Name { get; set; }

        public ulong ID { get; set; }

        public uint XP { get; set; }

        public DateTime LastMessage { get; set; }

        public uint LevelNumber
        {
            get
            {
                //XP = LEVEL ^ 2 * 50
                return (uint) Math.Sqrt(XP / 50);

            }
        }
    }
}
