using Mad_Bot_Discord.Core.UserAccounts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mad_Bot_Discord.Modules.DungeonGame
{
    public enum WeaponType
    {
        Gun,
        Sword,
        Staff,
        Bow
    }

    public abstract class Weapon
    {
        public abstract WeaponType WeaponType { get; }
        public abstract string Name { get; set; }

        public abstract double GetBaseDamage();
        public abstract double GetAccuracy();
        public abstract double GetStunChance();
        public abstract double GetStunAmount();

        public abstract KeyValuePair<string, double> AttackResult(bool critsEnabled);
    }

    public class Gun : Weapon
    {
        public override WeaponType WeaponType { get; } = WeaponType.Gun;
        public override string Name { get; set; } = "Gun";

        public Attributes FireType { get; set; }
        public Attributes BodyType { get; set; }
        public Enum ProjectileType { get; set; }
        public Attributes Projectile { get; set; }

        public override double GetBaseDamage()
        {
            double d = FireType.Damage + BodyType.Damage + Projectile.Damage;

            if (d <= 0) d = 1;

            return d;
        }

        public override double GetAccuracy()
        {
            double a = FireType.Accuracy + BodyType.Accuracy + Projectile.Accuracy;

            return a;
        }

        public override double GetStunChance()
        {
            return Projectile.StunChance;
        }

        public override double GetStunAmount()
        {
            return Projectile.StunAmount;
        }

        /// <summary> Returns the damage to be done and the string that represents the damage in a KeyValuePair. </summary>
        /// <param name="critsEnabled"> If enabled, it will have a chance to give critical hits. </param>
        public override KeyValuePair<string, double> AttackResult(bool critsEnabled)
        {
            double damage = GetBaseDamage();
            string s = $"You fire a {Projectile.Name} using your {Name}!";


            return new KeyValuePair<string, double>(s, 2);
        }
    }

    public class Sword : Weapon
    {
        public override WeaponType WeaponType { get; } = WeaponType.Sword;
        public override string Name { get; set; } = "Sword";

        public Attributes WeightType { get; set; } = null;
        public Attributes BladeType { get; set; } = null;
        public Attributes BladeThickness { get; set; } = null;
        public Attributes BladeLength { get; set; } = null;
        public Attributes HiltType { get; set; } = null;
        public Attributes HiltGrip { get; set; } = null;
        public Attributes HiltSize { get; set; } = null;

        public override double GetBaseDamage()
        {
            double d = WeightType.Damage + BladeType.Damage + BladeThickness.Damage + BladeLength.Damage + HiltType
                .Damage + HiltGrip.Damage + HiltSize.Damage;

            if (d <= 0) d = 1;
            return d;
        }

        public override double GetAccuracy()
        {
            return WeightType.Accuracy + BladeType.Accuracy + BladeThickness.Accuracy + BladeLength.Accuracy + HiltType
                .Accuracy + HiltGrip.Accuracy + HiltSize.Accuracy;
        }

        public override double GetStunAmount()
        {
            return WeightType.StunAmount + BladeType.StunAmount + BladeThickness.StunAmount + BladeLength.StunAmount + HiltType.StunAmount
                + HiltGrip.StunAmount + HiltSize.StunAmount;
        }

        public override double GetStunChance()
        {
            return WeightType.StunChance + BladeType.StunChance + BladeThickness.StunChance + BladeLength.StunChance + HiltType.StunChance
                + HiltGrip.StunChance + HiltSize.StunChance;
        }

        /// <summary> Returns the damage to be done and the string that represents the damage in a KeyValuePair. </summary>
        /// <param name="critsEnabled"> If enabled, it will have a chance to give critical hits. </param>
        public override KeyValuePair<string, double> AttackResult(bool critsEnabled)
        {
            double damage = GetBaseDamage();
            string s = $"You slice";


            return new KeyValuePair<string, double>(s, 2);
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
        /// <summary>
        /// The (X, Y) coordinates of the room.
        /// </summary>
        public Coordinates Coordinates { get; set; }

        /// <summary>
        /// List of doors in the room and which side they are on.
        /// </summary>
        public List<Direction> RoomDoors { get; set; }

        /// <summary>
        /// Amount of doors in one room.
        /// </summary>
        public int RoomCount
        {
            get
            {
                return RoomDoors.Count;
            }
        }

        /// <summary>
        /// The type of room.
        /// </summary>
        public RoomType Type { get; set; }
    }

    public class Enemy : Entity
    {
        public override string Name { get; set; } = "Enemy";
        public override float Health { get; set; } = 0;
        public override Stats Stats { get; set; } = new Stats();

        public Weapon Weapon { get; set; } = null;

        // Debuffs

        public override int StunLeft { get; set; } = 0;
        public override int PoisonLeft { get; set; } = 0;
    }

    public class Player : Entity
    {
        public override string Name { get; set; } = "Player";
        public override float Health { get; set; } = 100;
        public override Stats Stats { get; set; } = new Stats();

        public Weapon EquippedWeapon { get; set; } = null;
        public List<Weapon> Inventory { get; set; } = null;

        public SaveData SaveData { get; set; } = null;

        // Debuffs

        public override int StunLeft { get; set; } = 0;
        public override int PoisonLeft { get; set; } = 0;

        // Methods

        public float GetDamage()
        {
            return (float)Math.Round((float)EquippedWeapon.GetBaseDamage() * ((100 + (float)Stats.DamageIncrease) / 100));
        }

        public float GetAccuracy()
        {
            return (float)Math.Round((float)EquippedWeapon.GetAccuracy() * ((100 + (float)Stats.AccuracyIncrease) / 100));
        }
    }

    public abstract class Entity
    {
        public abstract string Name { get; set; }
        public abstract float Health { get; set; }
        public abstract Stats Stats { get; set; }
        
        // Debuffs

        public abstract int StunLeft { get; set; }
        public abstract int PoisonLeft { get; set; }
    }

    public class CanDirection
    {
        public bool CanLeft { get; set; } = false;
        public bool CanRight { get; set; } = false;
        public bool CanFront { get; set; } = false;
        public bool CanBack { get; set; } = false;
    }

    public class Stats
    {
        public double CriticalChanceIncrease { get; set; } = 0;

        public double DamageIncrease { get; set; } = 0;

        public double AccuracyIncrease { get; set; } = 0;

        public double StunChanceIncrease { get; set; } = 0;

        public double StunAmountIncrease { get; set; } = 0;
    }

    public class SaveData
    {
        public Player Player { get; set; } = null;
        public Enemy CurrentEnemy { get; set; } = null;

        public bool InGame { get; set; } = false;
        public bool InFight { get; set; } = false;

        public UserAccount User { get; set; } = null;
        public Dictionary<string, DungeonRoom> Rooms { get; set; } = null;
        public DungeonRoom CurrentRoom { get; set; } = null;
        public Coordinates Coordinates
        {
            get
            {
                if (CurrentRoom != null)
                    return CurrentRoom.Coordinates;
                else
                    return new Coordinates { X = 0, Y = 0 };
            }
        }
    }

    public class WeaponConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Weapon));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            if (reader.TokenType != JsonToken.Null)
            {
                if (reader.TokenType == JsonToken.StartArray || reader.TokenType == JsonToken.StartObject)
                {
                    try
                    {
                        JToken token = JToken.Load(reader);

                        switch (token["WeaponType"].Value<int>())
                        {
                            case 0:
                                return token.ToObject<Gun>(serializer);

                            case 1:
                                return token.ToObject<Sword>(serializer);

                            default:
                                return null;
                        }
                    }
                    catch (Exception) { }
                }
            }

            return null;

        }

        protected static bool FieldExists(JObject jObject, string name, JTokenType type)
        {
            JToken token;
            return jObject.TryGetValue(name, out token) && token.Type == type;
        }

        public override bool CanWrite
        {
            get { return false; } 
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

}
