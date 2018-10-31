using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mad_Bot_Discord.Modules.DungeonGame
{
    static class DungeonWeaponParts
    {
        // --- Gun
        public enum Gun_ProjectileType { Bullet, Explosive, Primitive, Special };
        public static Dictionary<int, Attributes> Gun_FireType = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "Bolt-Action", Accuracy = 3, Damage = 3 } },
            { 1, new Attributes { Name = "Pump-Action", Accuracy = -1, Damage = 2 } },
            { 2, new Attributes { Name = "Semi-Auto", Accuracy = 1, Damage = 4 } },
            { 3, new Attributes { Name = "Full-Auto", Accuracy = -2, Damage = 6} }
        };
        public static Dictionary<int, Attributes> Gun_BodyType = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "Pistol", NamePart = "Handi", Accuracy = -1 } },
            { 1, new Attributes { Name = "Revolver", NamePart = "Revol", Accuracy = 2 } },
            { 2, new Attributes { Name = "SMG", NamePart = "Subma", Accuracy = -2 } },
            { 3, new Attributes { Name = "Rifle", NamePart = "Ri", Accuracy = 5} },
            { 4, new Attributes { Name = "Shotgun", NamePart = "Shot", Accuracy = 3} },
            { 5, new Attributes { Name = "Minigun", NamePart = "Mini", Accuracy = -5} },
            { 6, new Attributes { Name = "RPG", NamePart = "Ro", Accuracy = 7 } }
        };
        public static Dictionary<int, Attributes> Gun_BulletCaliber = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "9mm bullets", NamePart = "nind", Damage = 2, Accuracy = 2} },
            { 1, new Attributes { Name = ".45 ACP bullets", NamePart = "llet", Damage = 3, Accuracy = 1} },
            { 2, new Attributes { Name = "7.62 x 39mm bullets", NamePart = "47", Damage = 4, Accuracy = 4} },
            { 3, new Attributes { Name = "12 Gauge Birdshot shells", NamePart = "shot", Damage = 3, Accuracy = -2} },
            { 4, new Attributes { Name = "12 Gauge Buckshot shells", NamePart = "uck", Damage = 5, Accuracy = -3} },
            { 5, new Attributes { Name = "12 Gauge Slug shells", NamePart = "snail", Damage = 7, Accuracy = -5 } },
            { 6, new Attributes { Name = ".50 BMG bullets", NamePart = "barr", Damage = 10, Accuracy = -2} }
        };
        public static Dictionary<int, Attributes> Gun_ExplosiveType = new Dictionary<int, Attributes>
        {
            {0, new Attributes { Name = "Dynamite", NamePart = "nyte", Damage = 10, Accuracy = 10} },
            {1, new Attributes { Name = "Frag Grenades", NamePart = "napple", Damage = 5, Accuracy = 7 } },
            {2, new Attributes { Name = "Stun Grenades", NamePart = "nade", StunChance = 8, StunAmount = 2, Accuracy = 7 } },
            {3, new Attributes { Name = "Smoke Grenades", NamePart = "dust", StunChance = 6, Accuracy = 10} },
            {4, new Attributes { Name = "Impact Grenades", NamePart = "pack", Damage = 6, Accuracy = 4} },
            {5, new Attributes { Name = "Random Fireworks", NamePart = "sparkle", Damage = 6} }
        };
        public static Dictionary<int, Attributes> Gun_PrimitiveType = new Dictionary<int, Attributes>
        {
            {0, new Attributes { Name = "Rocks", NamePart = "stone", Damage = 2, StunChance = 3, StunAmount = 1, Accuracy = 7} },
            {1, new Attributes { Name = "Crystals", NamePart = "shine", Damage = 6, StunChance = 1, StunAmount = 1, Accuracy = 6} },
            {2, new Attributes { Name = "Wood", NamePart = "ode", Damage = 1, StunChance = 4, StunAmount = 1, Accuracy = 6} },
            {3, new Attributes { Name = "Dirt", NamePart = "own", Damage = 0, StunChance = 6, StunAmount = 2, Accuracy = 4} }
        };
        public static Dictionary<int, Attributes> Gun_SpecialType = new Dictionary<int, Attributes>
        {
            {0, new Attributes { Name = "Lasers", NamePart = "inator5000", Damage = 2, Accuracy = 10} },
            {1, new Attributes { Name = "Cats", NamePart = "line", Damage = 6, Accuracy = 2, StunChance = 8, StunAmount = 1 } }
        };
        // --- Gun

        // --- Sword
        public static Dictionary<int, Attributes> Sword_WeightType = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "Light", Damage = -2, Accuracy = 3} },
            { 1, new Attributes { Name = "Medium", Damage = 1, Accuracy = 1 } },
            { 2, new Attributes { Name = "Heavy", Damage = 3, Accuracy = -2 } }
        };
        public static Dictionary<int, Attributes> Sword_BladeType = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "Stab", NamePart = "Stabi", Damage = -2, Accuracy = 4} },
            { 1, new Attributes { Name = "Slice", NamePart = "Slici", Damage = 2, Accuracy = -2} }
        };
        public static Dictionary<int, Attributes> Sword_BladeThickness = new Dictionary<int, Attributes>
        {
            { 0, new Attributes {Name = "Thin", NamePart = "xno", Damage = -2, Accuracy = 3} },
            { 1, new Attributes {Name = "Normal", NamePart = "de", Accuracy = 1 } },
            { 2, new Attributes {Name = "Thick", NamePart = "thi", Damage = 3, Accuracy = -2} }
        };
        public static Dictionary<int, Attributes> Sword_BladeLength = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "Short", Damage = 0, Accuracy = -2} },
            { 1, new Attributes { Name = "Normal", Damage = 1} },
            { 2, new Attributes { Name = "Long", Damage = 2, Accuracy = 1} }
        };
        public static Dictionary<int, Attributes> Sword_HiltType = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "Covered", Accuracy = 2 } },
            { 1, new Attributes { Name = "Exposed", Damage = 2 } }
        };
        public static Dictionary<int, Attributes> Sword_HiltSize = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "One Handed", NamePart = "sin", Damage = -2, Accuracy = 3 } },
            { 1, new Attributes { Name = "Two Handed", NamePart = "duo", Damage = 2, Accuracy = -2} },
            { 2, new Attributes { Name = "Three Handed", NamePart = "tric", Damage = 4, Accuracy = -3 } }
        };
        public static Dictionary<int, Attributes> Sword_HiltGrip = new Dictionary<int, Attributes>
        {
            { 0, new Attributes { Name = "None", Accuracy = -3, Damage = -2 } },
            { 1, new Attributes { Name = "Bad", Accuracy = -1} },
            { 2, new Attributes { Name = "Normal", Accuracy = 2} },
            { 3, new Attributes { Name = "Great", Accuracy = 4, Damage = 2} }
        };
        // --- Sword
    }
}
