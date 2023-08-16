using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Library.Network;
using Library.Network.ServerPackets;
using Library.SystemModels;
using MirDB;

namespace Library
{
    public static class Globals
    {
        public static ItemInfo GoldInfo;

        public static DBCollection<ItemInfo> ItemInfoList;
        public static DBCollection<MagicInfo> MagicInfoList;
        public static DBCollection<MapInfo> MapInfoList;
        public static DBCollection<InstanceInfo> InstanceInfoList;
        public static DBCollection<NPCPage> NPCPageList;
        public static DBCollection<MonsterInfo> MonsterInfoList;
        public static DBCollection<FishingInfo> FishingInfoList;
        public static DBCollection<StoreInfo> StoreInfoList;
        public static DBCollection<NPCInfo> NPCInfoList;
        public static DBCollection<MovementInfo> MovementInfoList;
        public static DBCollection<QuestInfo> QuestInfoList;
        public static DBCollection<QuestTask> QuestTaskList;
        public static DBCollection<CompanionInfo> CompanionInfoList;
        public static DBCollection<CompanionLevelInfo> CompanionLevelInfoList;
        public static DBCollection<CurrencyInfo> CurrencyInfoList;
        public static DBCollection<DisciplineInfo> DisciplineInfoList;

        public static Random Random = new Random();

        public static readonly Regex EMailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.Compiled);
        public static readonly Regex PasswordRegex = new Regex(@"^[\S]{" + MinPasswordLength + "," + MaxPasswordLength + "}$", RegexOptions.Compiled);
        public static readonly Regex CharacterReg = new Regex(@"^[A-Za-z0-9]{" + MinCharacterNameLength + "," + MaxCharacterNameLength + @"}$", RegexOptions.Compiled);
        public static readonly Regex GuildNameRegex = new Regex(@"^[A-Za-z0-9]{" + MinGuildNameLength + "," + MaxGuildNameLength + "}$", RegexOptions.Compiled);
        public static readonly Regex CaptionReg = new Regex(@"^[A-Za-z0-9]{" + MinCaptionLength + "," + MaxCaptionLength + @"}$", RegexOptions.Compiled);

        public static Color NoneColour = Color.White,
                            FireColour = Color.OrangeRed,
                            IceColour = Color.PaleTurquoise,
                            LightningColour = Color.LightSkyBlue,
                            WindColour = Color.LightSeaGreen,
                            HolyColour = Color.DarkKhaki,
                            DarkColour = Color.SaddleBrown,
                            PhantomColour = Color.Purple,

                            BrownNameColour = Color.Brown,
                            RedNameColour = Color.Red;

        public const string ClientName = "Legend of Mir 3";

        public const int
            MinPasswordLength = 5,
            MaxPasswordLength = 15,

            MinRealNameLength = 3,
            MaxRealNameLength = 20,


            MinCaptionLength = 3,
            MaxCaptionLength = 25,

            MaxEMailLength = 50,

            MinCharacterNameLength = 3,
            MaxCharacterNameLength = 15,
            MaxCharacterCount = 4,

            MinGuildNameLength = 2,
            MaxGuildNameLength = 15,

            MaxChatLength = 120,
            MaxGuildNoticeLength = 4000,

            MaxBeltCount = 10,
            MaxAutoPotionCount = 8,

            MagicRange = 10,
            MagicMaxLevel = 5,

            DuraLossRate = 15,

            GroupLimit = 15,

            MaxGrowthLevel = 3,

            MaxMailStorage = 50,

            CloakRange = 3,
            MarketPlaceFee = 0,
            AccessoryLevelCost = 0,
            AccessoryResetCost = 1000000,

            CraftWeaponPercentCost = 1000000,

            CommonCraftWeaponPercentCost = 30000000,
            SuperiorCraftWeaponPercentCost = 60000000,
            EliteCraftWeaponPercentCost = 80000000;

        public static decimal MarketPlaceTax = 0.07M;  //2.5x Item cost

        public static Regex LinkedItemRegex = new Regex(@"\[(?<Text>.*?):(?<ID>.+?)\]", RegexOptions.Compiled);

        public static long
            GuildCreationCost = 7500000,
            GuildMemberCost = 1000000,
            GuildStorageCost = 350000,
            GuildWarCost = 200000;

        public static long
            MasterRefineCost = 50000,
            MasterRefineEvaluateCost = 250000;

        public static List<string> Languages = new List<string>
        {
            "English",
            "Chinese",
        };

        public static List<decimal> ExperienceList = new List<decimal>
        {
            0,
            100,
            200,
            300,
            400,
            600,
            900,
            1200,
            1700,
            2500,
            6000,
            8000,
            10000,
            15000,
            30000,
            40000,
            50000,
            70000,
            100000,
            120000,
            140000,
            250000,
            300000,
            350000,
            400000,
            500000,
            700000,
            1000000,
            1400000,
            1800000,
            2000000,
            2400000,
            2800000,
            3200000,
            3600000,
            4000000,
            4800000,
            5600000,
            8200000,
            9000000,
            11000000,
            14000000,
            18000000,
            22000000,
            25000000,
            30000000,
            35000000,
            40000000,
            50000000,
            60000000,
            70000000,
            85000000,
            110000000,
            135000000,
            145000000,
            150000000,
            175000000,
            180000000,
            200000000,
            220000000,
            230000000,
            240000000,
            250000000,
            260000000,
            270000000,
            280000000,
            300000000,
            320000000,
            340000000,
            360000000,
            380000000,
            400000000,
            800000000,
            1400000000,
            2200000000,
            6530000000,
            12000000000,
            30000000000,
            75000000000,
            150000000000,
            175000000000,
            300000000000,
            430000000000,
            570000000000,
            700000000000,
            800000000000,
            900000000000,
            3000000000000,
            6000000000000,
            9000000000000,
            13000000000000,
            17000000000000,
            1440000000000,
            1460000000000,
            1490000000000,
            1620000000000,
            1660000000000,
            1720000000000,
            1800000000000,
            1880000000000,
            2000000000000,
        };

        public static List<decimal> OldExperienceList = new List<decimal>
        {
            0, // Lv 0
            100,
            200,
            300,
            400,
            600,
            900,
            1200,
            1700,
            2500,
            6000,
            8000,
            10000,
            15000,
            30000,
            40000,
            50000,
            70000,
            100000,
            120000,
            140000,
            250000,
            300000,
            350000,
            400000,
            500000,
            700000,
            1000000,
            1400000,
            1800000,
            2000000,
            2400000,
            2800000,
            3200000,
            3600000,
            4000000,
            4800000,
            5600000,
            8200000,
            9000000,
            11000000,
            14000000,
            25000000,
            45000000,
            70000000,
            90000000,
            110000000,
            130000000,
            150000000,
            170000000,
            210000000,
            230000000,
            250000000,
            270000000,
            310000000,
            330000000,
            350000000,
            370000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            400000000,
            800000000,
            1400000000,
            2200000000,
            3200000000,
            3600000000,
            4000000000,
            4500000000,
            5000000000,
            15000000000,
            45000000000,
            50000000000,
            55000000000,
            60000000000,
            100000000000,
            120000000000,
            135000000000,
            150000000000,
            170000000000,
            300000000000,
            400000000000,
            440000000000,
            460000000000,
            490000000000,
            620000000000,
            660000000000,
            720000000000,
            800000000000,
            880000000000,
            1000000000000,
        };

        public static List<decimal> WeaponExperienceList = new List<decimal>
        {
            0, //0

            300000,
            350000,
            400000,
            450000,
            500000,
            550000,
            600000,
            650000,
            700000,
            750000, //10

            800000,
            850000,
            900000,
            1000000,
            1300000,
            2000000,
        };

        public static List<decimal> AccessoryExperienceList = new List<decimal>
        {
            0,

            5,
            20,
            80,
            350,
            1500,
            6200,
            26500,
            114000,
            490000,
            2090000,
        };

        public const int InventorySize = 48,
                         EquipmentSize = 22,
                         CompanionInventorySize = 30,
                         CompanionEquipmentSize = 4,
                         EquipmentOffSet = 1000,
                         StorageSize = 100,
                         PartsStorageOffset = 2000;

        public const int AttackDelay = 1500,
                         ASpeedRate = 47,
                         ProjectileSpeed = 48;

        public static TimeSpan TurnTime = TimeSpan.FromMilliseconds(300),
                               HarvestTime = TimeSpan.FromMilliseconds(600),
                               MoveTime = TimeSpan.FromMilliseconds(600),
                               AttackTime = TimeSpan.FromMilliseconds(600),
                               CastTime = TimeSpan.FromMilliseconds(600),
                               MagicDelay = TimeSpan.FromMilliseconds(2000);

        public static bool RealNameRequired = false,
                           BirthDateRequired = false;

        public static Dictionary<RefineQuality, TimeSpan> RefineTimes = new Dictionary<RefineQuality, TimeSpan>
        {
            [RefineQuality.Rush] = TimeSpan.FromMinutes(1),
            [RefineQuality.Quick] = TimeSpan.FromMinutes(30),
            [RefineQuality.Standard] = TimeSpan.FromHours(1),
            [RefineQuality.Careful] = TimeSpan.FromHours(6),
            [RefineQuality.Precise] = TimeSpan.FromDays(1),
        };

        public const int WooolMiddleFileStartIndex = 605, WooolBackFileStartIndex = 600;

    }

    public sealed class SelectInfo
    {
        public int CharacterIndex { get; set; }
        public string CharacterName { get; set; }
        public string Caption { get; set; }
        public int Level { get; set; }
        public MirGender Gender { get; set; }
        public MirClass Class { get; set; }
        public int Location { get; set; }
        public DateTime LastLogin { get; set; }
    }

    public sealed class StartInformation
    {
        public int Index { get; set; }
        public uint ObjectID { get; set; }
        public string Name { get; set; }

        public string Caption { get; set; }
        public Color NameColour { get; set; }
        public string GuildName { get; set; }
        public string GuildRank { get; set; }

        public MirClass Class { get; set; }
        public MirGender Gender { get; set; }
        public Point Location { get; set; }
        public MirDirection Direction { get; set; }

        public int MapIndex { get; set; }
        public int InstanceIndex { get; set; }

        public int Level { get; set; }
        public int HairType { get; set; }
        public Color HairColour { get; set; }
        public int Weapon { get; set; }
        public int Armour { get; set; }
        public int Costume { get; set; }
        public int Shield { get; set; }
        public Color ArmourColour { get; set; }

        public ExteriorEffect ArmourEffect { get; set; }
        public ExteriorEffect EmblemEffect { get; set; }
        public ExteriorEffect WeaponEffect { get; set; }
        public ExteriorEffect ShieldEffect { get; set; }

        public decimal Experience { get; set; }

        public int CurrentHP { get; set; }
        public int CurrentMP { get; set; }
        public int CurrentFP { get; set; }

        public AttackMode AttackMode { get; set; }
        public PetMode PetMode { get; set; }

        public OnlineState OnlineState { get; set; }

        public ClientUserDiscipline Discipline { get; set; }

        public int HermitPoints { get; set; }

        public float DayTime { get; set; }
        public bool AllowGroup { get; set; }

        public List<ClientFriendInfo> Friends { get; set; }

        public List<ClientUserItem> Items { get; set; }
        public List<ClientBeltLink> BeltLinks { get; set; }
        public List<ClientAutoPotionLink> AutoPotionLinks { get; set; }

        public List<ClientUserMagic> Magics { get; set; }
        public List<ClientBuffInfo> Buffs { get; set; }

        public List<ClientUserCurrency> Currencies { get; set; }

        public PoisonType Poison { get; set; }

        public bool InSafeZone { get; set; }
        public bool Observable { get; set; }

        public bool Dead { get; set; }

        public HorseType Horse { get; set; } //Horse Armour too

        public int HelmetShape { get; set; }
        public int HorseShape { get; set; }

        public bool HideHead { get; set; }

        public List<ClientUserQuest> Quests { get; set; }

        public List<int> CompanionUnlocks { get; set; }
        public List<CompanionInfo> AvailableCompanions = new List<CompanionInfo>();

        public List<ClientUserCompanion> Companions { get; set; }

        public int Companion { get; set; }

        public int StorageSize { get; set; }

        public string FiltersClass { get; set; }
        public string FiltersRarity { get; set; }
        public string FiltersItemType { get; set; }

        [CompleteObject]
        public void OnComplete()
        {
            foreach (int index in CompanionUnlocks)
                AvailableCompanions.Add(Globals.CompanionInfoList.Binding.First(x => x.Index == index));
        }
    }

    public sealed class ClientUserItem
    {
        public ItemInfo Info;

        public int Index { get; set; } //ItemID
        public int InfoIndex { get; set; }

        public int CurrentDurability { get; set; }
        public int MaxDurability { get; set; }

        public long Count { get; set; }

        public int Slot { get; set; }

        public int Level { get; set; }
        public decimal Experience { get; set; }

        public Color Colour { get; set; }

        public TimeSpan SpecialRepairCoolDown { get; set; }
        public TimeSpan ResetCoolDown { get; set; }

        public bool New;
        public DateTime NextSpecialRepair, NextReset;

        public Stats AddedStats { get; set; }

        public UserItemFlags Flags { get; set; }
        public TimeSpan ExpireTime { get; set; }


        [IgnorePropertyPacket]
        public int Weight
        {
            get
            {
                switch (Info.ItemType)
                {
                    case ItemType.Poison:
                    case ItemType.Amulet:
                        return Info.Weight;
                    default:
                        return (int)Math.Min(int.MaxValue, Info.Weight * Count);
                }
            }
        }

        [CompleteObject]
        public void Complete()
        {
            Info = Globals.ItemInfoList.Binding.FirstOrDefault(x => x.Index == InfoIndex);

            NextSpecialRepair = Time.Now + SpecialRepairCoolDown;
            NextReset = Time.Now + ResetCoolDown;
        }

        public ClientUserItem()
        { }
        public ClientUserItem(ItemInfo info, long count)
        {
            Info = info;
            Count = count;
            MaxDurability = info.Durability;
            CurrentDurability = info.Durability;
            Level = 1;
            AddedStats = new Stats();
        }
        public ClientUserItem(ClientUserItem item, long count)
        {
            Info = item.Info;

            Index = item.Index;
            InfoIndex = item.InfoIndex;

            CurrentDurability = item.CurrentDurability;
            MaxDurability = item.MaxDurability;

            Count = count;

            Slot = item.Slot;

            Level = item.Level;
            Experience = item.Experience;

            Colour = item.Colour;

            SpecialRepairCoolDown = item.SpecialRepairCoolDown;

            Flags = item.Flags;
            ExpireTime = item.ExpireTime;

            New = item.New;
            NextSpecialRepair = item.NextSpecialRepair;

            AddedStats = new Stats(item.AddedStats);
        }


        public long Price(long count)
        {
            if ((Flags & UserItemFlags.Worthless) == UserItemFlags.Worthless) return 0;

            decimal p = Info.Price;

            if (Info.Durability > 0)
            {
                decimal r = Info.Price / 2M / Info.Durability;

                p = MaxDurability * r;

                r = MaxDurability > 0 ? CurrentDurability / (decimal)MaxDurability : 0;

                p = Math.Floor(p / 2M + p / 2M * r + Info.Price / 2M);
            }

            p = p * (AddedStats.Count * 0.1M + 1M);

            if (Info.Stats[Stat.SaleBonus20] > 0 && Info.Stats[Stat.SaleBonus20] <= count)
                p *= 1.2M;
            else if (Info.Stats[Stat.SaleBonus15] > 0 && Info.Stats[Stat.SaleBonus15] <= count)
                p *= 1.15M;
            else if (Info.Stats[Stat.SaleBonus10] > 0 && Info.Stats[Stat.SaleBonus10] <= count)
                p *= 1.1M;
            else if (Info.Stats[Stat.SaleBonus5] > 0 && Info.Stats[Stat.SaleBonus5] <= count)
                p *= 1.05M;

            return (long)(p * count * Info.SellRate);
        }

        public int RepairCost(bool special)
        {
            if (Info.Durability == 0 || CurrentDurability >= MaxDurability) return 0;

            int rate = special ? 2 : 1;

            decimal p = Math.Floor(MaxDurability * (Info.Price / 2M / Info.Durability) + Info.Price / 2M);
            p = p * (AddedStats.Count * 0.1M + 1M);

            return (int)(p * Count - Price(Count)) * rate;


        }
        public bool CanAccessoryUpgrade()
        {
            switch (Info.ItemType)
            {
                case ItemType.Ring:
                case ItemType.Bracelet:
                case ItemType.Necklace:
                    break;
                default: return false;

            }

            return (Flags & UserItemFlags.NonRefinable) != UserItemFlags.NonRefinable && (Flags & UserItemFlags.Refinable) == UserItemFlags.Refinable;
        }
        public bool CanFragment()
        {
            if ((Flags & UserItemFlags.NonRefinable) == UserItemFlags.NonRefinable || (Flags & UserItemFlags.Worthless) == UserItemFlags.Worthless) return false;

            switch (Info.Rarity)
            {
                case Rarity.Common:
                    if (Info.RequiredAmount <= 15) return false;
                    break;
                case Rarity.Superior:
                    break;
                case Rarity.Elite:
                    break;
            }

            switch (Info.ItemType)
            {
                case ItemType.Weapon:
                case ItemType.Armour:
                case ItemType.Helmet:
                case ItemType.Necklace:
                case ItemType.Bracelet:
                case ItemType.Ring:
                case ItemType.Shoes:
                    break;
                default:
                    return false;
            }

            return true;
        }
        public int FragmentCost()
        {
            switch (Info.Rarity)
            {
                case Rarity.Common:
                    switch (Info.ItemType)
                    {
                        case ItemType.Armour:
                        case ItemType.Weapon:
                        case ItemType.Helmet:
                        case ItemType.Necklace:
                        case ItemType.Bracelet:
                        case ItemType.Ring:
                        case ItemType.Shoes:
                            return Info.RequiredAmount * 10000 / 9;
                        /* case ItemType.Helmet:
                         case ItemType.Necklace:
                         case ItemType.Bracelet:
                         case ItemType.Ring:
                         case ItemType.Shoes:
                             return Info.RequiredAmount * 7000 / 9;*/
                        default:
                            return 0;
                    }
                case Rarity.Superior:
                    switch (Info.ItemType)
                    {
                        case ItemType.Weapon:
                        case ItemType.Armour:
                        case ItemType.Helmet:
                        case ItemType.Necklace:
                        case ItemType.Bracelet:
                        case ItemType.Ring:
                        case ItemType.Shoes:
                            return Info.RequiredAmount * 10000 / 2;
                        /*  case ItemType.Helmet:
                          case ItemType.Necklace:
                          case ItemType.Bracelet:
                          case ItemType.Ring:
                          case ItemType.Shoes:
                              return Info.RequiredAmount * 10000 / 10;*/
                        default:
                            return 0;
                    }
                case Rarity.Elite:
                    switch (Info.ItemType)
                    {
                        case ItemType.Weapon:
                        case ItemType.Armour:
                            return 250000;
                        case ItemType.Helmet:
                            return 50000;
                        case ItemType.Necklace:
                        case ItemType.Bracelet:
                        case ItemType.Ring:
                            return 150000;
                        case ItemType.Shoes:
                            return 30000;
                        default:
                            return 0;
                    }
                default:
                    return 0;
            }
        }
        public int FragmentCount()
        {
            switch (Info.Rarity)
            {
                case Rarity.Common:
                    switch (Info.ItemType)
                    {
                        case ItemType.Armour:
                        case ItemType.Weapon:
                        case ItemType.Helmet:
                        case ItemType.Necklace:
                        case ItemType.Bracelet:
                        case ItemType.Ring:
                        case ItemType.Shoes:
                            return Math.Max(1, Info.RequiredAmount / 2 + 5);
                        /*  case ItemType.Helmet:
                              return Math.Max(1, (Info.RequiredAmount - 30) / 6);
                          case ItemType.Necklace:
                              return Math.Max(1, Info.RequiredAmount / 8);
                          case ItemType.Bracelet:
                              return Math.Max(1, Info.RequiredAmount / 15);
                          case ItemType.Ring:
                              return Math.Max(1, Info.RequiredAmount / 9);
                          case ItemType.Shoes:
                              return Math.Max(1, (Info.RequiredAmount - 35) / 6);*/
                        default:
                            return 0;
                    }
                case Rarity.Superior:
                    switch (Info.ItemType)
                    {
                        case ItemType.Armour:
                        case ItemType.Weapon:
                        case ItemType.Helmet:
                        case ItemType.Necklace:
                        case ItemType.Bracelet:
                        case ItemType.Ring:
                        case ItemType.Shoes:
                            return Math.Max(1, Info.RequiredAmount / 2 + 5);
                        /*  case ItemType.Helmet:
                              return Math.Max(1, (Info.RequiredAmount - 30) / 6);
                          case ItemType.Necklace:
                              return Math.Max(1, Info.RequiredAmount / 10);
                          case ItemType.Bracelet:
                              return Math.Max(1, Info.RequiredAmount / 15);
                          case ItemType.Ring:
                              return Math.Max(1, Info.RequiredAmount / 10);
                          case ItemType.Shoes:
                              return Math.Max(1, (Info.RequiredAmount - 35) / 6);*/
                        default:
                            return 0;
                    }
                case Rarity.Elite:
                    switch (Info.ItemType)
                    {
                        case ItemType.Armour:
                        case ItemType.Weapon:
                            return 50;
                        case ItemType.Helmet:
                            return 5;
                        case ItemType.Necklace:
                        case ItemType.Bracelet:
                        case ItemType.Ring:
                            return 10;
                        case ItemType.Shoes:
                            return 3;
                        default:
                            return 0;
                    }
                default:
                    return 0;
            }
        }
    }

    public sealed class ClientBeltLink
    {
        public int Slot { get; set; }
        public int LinkInfoIndex { get; set; }
        public int LinkItemIndex { get; set; }
    }

    public sealed class ClientAutoPotionLink
    {
        public int Slot { get; set; }
        public int LinkInfoIndex { get; set; }
        public int Health { get; set; }
        public int Mana { get; set; }
        public bool Enabled { get; set; }
    }

    public class ClientUserMagic
    {
        public int Index { get; set; }
        public int InfoIndex { get; set; }
        public MagicInfo Info;

        public SpellKey Set1Key { get; set; }
        public SpellKey Set2Key { get; set; }
        public SpellKey Set3Key { get; set; }
        public SpellKey Set4Key { get; set; }

        public int Level { get; set; }
        public long Experience { get; set; }

        public TimeSpan Cooldown { get; set; }

        public DateTime NextCast;


        [IgnorePropertyPacket]
        public int Cost => Info.BaseCost + Level * Info.LevelCost / 3;

        [CompleteObject]
        public void Complete()
        {
            NextCast = Time.Now + Cooldown;
            Info = Globals.MagicInfoList.Binding.FirstOrDefault(x => x.Index == InfoIndex);
        }
    }

    public class ClientNPCValues
    {
        public int ID { get; set; }
        public string Value { get; set; }
    }

    public class CellLinkInfo
    {
        public GridType GridType { get; set; }
        public int Slot { get; set; }
        public long Count { get; set; }
    }

    public class ClientBuffInfo
    {
        public int Index { get; set; }
        public BuffType Type { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public TimeSpan TickFrequency { get; set; }
        public Stats Stats { get; set; }
        public bool Pause { get; set; }
        public int ItemIndex { get; set; }
    }

    public class ClientRefineInfo
    {
        public int Index { get; set; }
        public ClientUserItem Weapon { get; set; }
        public RefineType Type { get; set; }
        public RefineQuality Quality { get; set; }
        public int Chance { get; set; }
        public int MaxChance { get; set; }
        public TimeSpan ReadyDuration { get; set; }

        public DateTime RetrieveTime;

        [CompleteObject]
        public void Complete()
        {
            RetrieveTime = Time.Now + ReadyDuration;
        }
    }


    public sealed class RankInfo
    {
        public int Rank { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public MirClass Class { get; set; }
        public int Level { get; set; }
        public decimal Experience { get; set; }
        public decimal MaxExperience { get; set; }
        public bool Online { get; set; }
        public bool Observable { get; set; }
        public int Rebirth { get; set; }
        public int RankChange { get; set; }
    }

    public class ClientMarketPlaceInfo
    {
        public int Index { get; set; }
        public ClientUserItem Item { get; set; }

        public int Price { get; set; }

        public string Seller { get; set; }
        public string Message { get; set; }
        public bool IsOwner { get; set; }

        public bool Loading;
    }

    public class ClientMailInfo
    {
        public int Index { get; set; }
        public bool Opened { get; set; }
        public bool HasItem { get; set; }
        public DateTime Date { get; set; }

        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }

        public int Gold { get; set; }
        public List<ClientUserItem> Items { get; set; }
    }

    public class ClientGuildInfo
    {
        public string GuildName { get; set; }

        public string Notice { get; set; }

        public int MemberLimit { get; set; }

        public long GuildFunds { get; set; }
        public long DailyGrowth { get; set; }

        public long TotalContribution { get; set; }
        public long DailyContribution { get; set; }

        public int UserIndex { get; set; }

        public int StorageLimit { get; set; }
        public int Tax { get; set; }

        public string DefaultRank { get; set; }
        public GuildPermission DefaultPermission { get; set; }

        public List<ClientGuildMemberInfo> Members { get; set; }

        public List<ClientUserItem> Storage { get; set; }

        [IgnorePropertyPacket]
        public GuildPermission Permission => Members.FirstOrDefault(x => x.Index == UserIndex)?.Permission ?? GuildPermission.None;
    }

    public class ClientGuildMemberInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public long TotalContribution { get; set; }
        public long DailyContribution { get; set; }
        public TimeSpan Online { get; set; }

        public GuildPermission Permission { get; set; }

        public DateTime LastOnline;
        public uint ObjectID { get; set; }

        [CompleteObject]
        public void Complete()
        {
            if (Online == TimeSpan.MinValue)
                LastOnline = DateTime.MaxValue;
            else
                LastOnline = Time.Now - Online;
        }

    }

    public class ClientUserQuest
    {
        public int Index { get; set; }

        [IgnorePropertyPacket]
        public QuestInfo Quest { get; set; }

        public int QuestIndex { get; set; }

        public bool Track { get; set; }

        public bool Completed { get; set; }

        public int SelectedReward { get; set; }

        public DateTime DateTaken { get; set; }
        public DateTime DateCompleted { get; set; }

        [IgnorePropertyPacket]
        public bool IsComplete => Tasks.Count == Quest.Tasks.Count && Tasks.All(x => x.Completed);

        public List<ClientUserQuestTask> Tasks { get; set; }

        [CompleteObject]
        public void Complete()
        {
            Quest = Globals.QuestInfoList.Binding.First(x => x.Index == QuestIndex);
        }
    }

    public class ClientUserQuestTask
    {
        public int Index { get; set; }

        [IgnorePropertyPacket]
        public QuestTask Task { get; set; }

        public int TaskIndex { get; set; }

        public long Amount { get; set; }

        [IgnorePropertyPacket]
        public bool Completed => Amount >= Task.Amount;

        [CompleteObject]
        public void Complete()
        {
            Task = Globals.QuestTaskList.Binding.First(x => x.Index == TaskIndex);
        }
    }

    public class ClientCompanionObject
    {
        public string Name { get; set; }

        public int HeadShape { get; set; }
        public int BackShape { get; set; }
    }

    public class ClientUserCompanion
    {
        public int Index { get; set; }
        public string Name { get; set; }

        public int CompanionIndex { get; set; }
        public CompanionInfo CompanionInfo;

        public int Level { get; set; }
        public int Hunger { get; set; }
        public int Experience { get; set; }

        public Stats Level3 { get; set; }
        public Stats Level5 { get; set; }
        public Stats Level7 { get; set; }
        public Stats Level10 { get; set; }
        public Stats Level11 { get; set; }
        public Stats Level13 { get; set; }
        public Stats Level15 { get; set; }

        public string CharacterName { get; set; }

        public List<ClientUserItem> Items { get; set; }

        public ClientUserItem[] EquipmentArray = new ClientUserItem[Globals.CompanionEquipmentSize], InventoryArray = new ClientUserItem[Globals.CompanionInventorySize];


        [CompleteObject]
        public void OnComplete()
        {
            CompanionInfo = Globals.CompanionInfoList.Binding.First(x => x.Index == CompanionIndex);

            foreach (ClientUserItem item in Items)
            {
                if (item.Slot < Globals.EquipmentOffSet)
                    InventoryArray[item.Slot] = item;
                else
                    EquipmentArray[item.Slot - Globals.EquipmentOffSet] = item;
            }
        }

    }

    public class ClientPlayerInfo
    {
        public uint ObjectID { get; set; }

        public string Name { get; set; }
    }
    public class ClientObjectData
    {
        public uint ObjectID;

        public int MapIndex;
        public Point Location;

        public string Name;

        //Guild/Group
        public MonsterInfo MonsterInfo;
        public ItemInfo ItemInfo;

        public string PetOwner;

        public int Health;
        public int MaxHealth;

        public int Mana;
        public int MaxMana;
        public Stats Stats { get; set; }

        public bool Dead;
    }

    public class ClientBlockInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }
    }

    public class ClientFriendInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public OnlineState State { get; set; }
    }

    public class ClientFortuneInfo
    {
        public int ItemIndex { get; set; }
        public ItemInfo ItemInfo;

        public TimeSpan CheckTime { get; set; }
        public long DropCount { get; set; }
        public decimal Progress { get; set; }

        public DateTime CheckDate;

        [CompleteObject]
        public void OnComplete()
        {
            ItemInfo = Globals.ItemInfoList.Binding.First(x => x.Index == ItemIndex);

            CheckDate = Time.Now - CheckTime;
        }
    }

    public class CompanionFiltersInfo
    {
        public string FilterClass { get; set; }
        public string FilterRarity { get; set; }
        public string FilterItemType { get; set; }
    }

    public class ClientUserCurrency
    {
        public int CurrencyIndex { get; set; }
        public CurrencyInfo Info;
        public long Amount { get; set; }

        [IgnorePropertyPacket]
        public bool CanPickup
        {
            get { return Info != null && Info.DropItem != null; }
        }
    }

    public class ClientUserDiscipline
    {
        public int InfoIndex { get; set; }
        public DisciplineInfo DisciplineInfo;
        public int Level { get; set; }
        public long Experience { get; set; }
        public List<ClientUserMagic> Magics { get; set; }
    }
}


