using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Library;
using Library.Network;
using Library.SystemModels;
using Server.DBModels;
using Server.Envir;
using Server.Models.Monsters;
using S = Library.Network.ServerPackets;

namespace Server.Models
{
    public sealed class Map
    {
        public MapInfo Info { get; }
        public InstanceInfo Instance { get; }
        public byte InstanceSequence { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public bool HasSafeZone { get; set; }

        public Cell[,] Cells { get; private set; }
        public List<Cell> ValidCells { get; } = new List<Cell>();

        public List<MapObject> Objects { get; } = new List<MapObject>();
        public List<PlayerObject> Players { get; } = new List<PlayerObject>();
        public List<MonsterObject> Bosses { get; } = new List<MonsterObject>();
        public List<NPCObject> NPCs { get; } = new List<NPCObject>();
        public HashSet<MapObject>[] OrderedObjects;

        public DateTime LastProcess, LastPlayer, InstanceExpiryDateTime;

        public DateTime HalloweenEventTime, ChristmasEventTime;

        public Map(MapInfo info, InstanceInfo instance = null, byte instanceSequence = 0)
        {
            Info = info;

            if (instance != null)
            {
                Instance = instance;
                InstanceSequence = instanceSequence;
                InstanceExpiryDateTime = instance.TimeLimitInMinutes > 0 ? SEnvir.Now.AddMinutes(instance.TimeLimitInMinutes) : DateTime.MaxValue;
            }
        }

        public void Load()
        {
            string fileName = $"{Config.MapPath}{Info.FileName}.map";

            if (!File.Exists(fileName))
            {
                SEnvir.Log($"Map: {fileName} not found.");
                return;
            }

            byte[] fileBytes = File.ReadAllBytes(fileName);

            switch (LibraryCore.Maps.MapFormat.FindType(fileBytes).mapCodeType)
            {
                case 0://raw
                    LoadMapCellsv0(fileBytes);
                    break;
                case 1://Map 2010
                    LoadMapCellsv1(fileBytes);
                    break;
                case 2://Shanda 2012 old (wemades)
                    LoadMapCellsv2(fileBytes);
                    break;
                case 3://shanda 2012 old
                    LoadMapCellsv3(fileBytes);
                    break;
                case 4://Mir2 AntiHack
                    LoadMapCellsv4(fileBytes);
                    break;
                case 5://Mir3
                    LoadMapCellsv5(fileBytes);
                    break;
                case 6://Shanda Mir3
                    LoadMapCellsv6(fileBytes);
                    break;
                case 7://3/4 heroes map
                    LoadMapCellsv7(fileBytes);
                    break;
                case 8://Shanda Mir3 - NEW
                    LoadMapCellsv8(fileBytes);
                    break;
                case 100://C# custom
                    LoadMapCellsV100(fileBytes);
                    break;
                case 150://C# Woool Map custom format
                    LoadMapCellsV150(fileBytes, fileName);
                    break;
                default:
                    break;
            }

            #region old code
            //old code - only mir3
            //Width = fileBytes[23] << 8 | fileBytes[22];
            //Height = fileBytes[25] << 8 | fileBytes[24];

            //Cells = new Cell[Width, Height];

            //int offSet = 28 + Width * Height / 4 * 3;

            //for (int x = 0; x < Width; x++)
            //    for (int y = 0; y < Height; y++)
            //    {
            //        byte flag = fileBytes[offSet + (x * Height + y) * 14];

            //        if ((flag & 0x02) != 2 || (flag & 0x01) != 1) continue;

            //        ValidCells.Add(Cells[x, y] = new Cell(new Point(x, y)) { Map = this });
            //    }
            #endregion

            if (Width > 1500 || Height > 1500)
            {
                return;
            }

            OrderedObjects = new HashSet<MapObject>[Width];
            for (int i = 0; i < OrderedObjects.Length; i++)
                OrderedObjects[i] = new HashSet<MapObject>();
        }

        #region Maps Functions

        private void LoadMapCellsv0(byte[] fileBytes)
        {

            //raw
            int offSet = 0;
            Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            Height = BitConverter.ToInt16(fileBytes, offSet);

            if (Width > 1500 || Height > 1500) return;

            Cells = new Cell[Width, Height];

            offSet = 52;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    byte flag = 0;

                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        flag = 2; //Can Fire Over.

                    if ((BitConverter.ToInt16(fileBytes, offSet + 2) & 0x8000) != 0)
                        flag = 1; //Can't Fire Over.

                    if ((BitConverter.ToInt16(fileBytes, offSet + 4) & 0x8000) != 0)
                        flag = 2; //No Floor Tile.

                    offSet += 12;

                    if (flag == 1 || flag == 2)
                    {
                        continue;
                    }

                    ValidCells.Add(Cells[x, y] = new Cell(new Point(x, y)) { Map = this });

                }

            }

        }

        private void LoadMapCellsv1(byte[] fileBytes)
        {
            //Map 2010
            int offSet = 21;

            int w = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int xor = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int h = BitConverter.ToInt16(fileBytes, offSet);
            Width = w ^ xor;
            Height = h ^ xor;

            Cells = new Cell[Width, Height];

            offSet = 54;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    byte flag = 0;

                    if (((BitConverter.ToInt32(fileBytes, offSet) ^ 0xAA38AA38) & 0x20000000) != 0)
                        flag = 2; //Can Fire Over.

                    if (((BitConverter.ToInt16(fileBytes, offSet + 6) ^ xor) & 0x8000) != 0)
                        flag = 1; //No Floor Tile.

                    offSet += 15;

                    if (flag == 1 || flag == 2)
                    {
                        continue;
                    }

                    ValidCells.Add(Cells[x, y] = new Cell(new Point(x, y)) { Map = this });

                }

            }
        }

        private void LoadMapCellsv2(byte[] fileBytes)
        {
            //Shanda 2012 old (wemades)
            int offSet = 0;
            Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            Height = BitConverter.ToInt16(fileBytes, offSet);

            Cells = new Cell[Width, Height];

            offSet = 52;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    byte flag = 0;

                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        flag = 2; //Can Fire Over.

                    if ((BitConverter.ToInt16(fileBytes, offSet + 2) & 0x8000) != 0)
                        flag = 1; //Can't Fire Over.

                    if ((BitConverter.ToInt16(fileBytes, offSet + 4) & 0x8000) != 0)
                        flag = 2; //No Floor Tile.

                    offSet += 14;

                    if (flag == 1 || flag == 2)
                    {
                        continue;
                    }

                    ValidCells.Add(Cells[x, y] = new Cell(new Point(x, y)) { Map = this });

                }

            }
        }

        private void LoadMapCellsv3(byte[] fileBytes)
        {
            //Shanda 2012 old
            int offSet = 0;
            Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            Height = BitConverter.ToInt16(fileBytes, offSet);

            Cells = new Cell[Width, Height];

            offSet = 52;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    byte flag = 0;

                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        flag = 2; //Can Fire Over.

                    if ((BitConverter.ToInt16(fileBytes, offSet + 2) & 0x8000) != 0)
                        flag = 1; //Can't Fire Over.

                    if ((BitConverter.ToInt16(fileBytes, offSet + 4) & 0x8000) != 0)
                        flag = 2; //No Floor Tile.

                    offSet += 36;

                    if (flag == 1 || flag == 2)
                    {
                        continue;
                    }

                    ValidCells.Add(Cells[x, y] = new Cell(new Point(x, y)) { Map = this });

                }

            }
        }

        private void LoadMapCellsv4(byte[] fileBytes)
        {
            //Mir2 AntiHack

            int offSet = 31;
            int w = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int xor = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            int h = BitConverter.ToInt16(fileBytes, offSet);
            Width = w ^ xor;
            Height = h ^ xor;

            Cells = new Cell[Width, Height];

            offSet = 64;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    byte flag = 0;

                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        flag = 2; //Can Fire Over.

                    if ((BitConverter.ToInt16(fileBytes, offSet + 2) & 0x8000) != 0)
                        flag = 1; //Can't Fire Over.

                    offSet += 12;

                    if (flag == 1 || flag == 2)
                    {
                        continue;
                    }

                    ValidCells.Add(Cells[x, y] = new Cell(new Point(x, y)) { Map = this });

                }

            }

        }

        private void LoadMapCellsv5(byte[] fileBytes)
        {

            //Mir3
            Width = fileBytes[23] << 8 | fileBytes[22];
            Height = fileBytes[25] << 8 | fileBytes[24];

            Cells = new Cell[Width, Height];

            int offSet = 28 + Width * Height / 4 * 3;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    byte flag = fileBytes[offSet + (x * Height + y) * 14];

                    if ((flag & 0x02) != 2 || (flag & 0x01) != 1)
                    {
                        continue;
                    }

                    ValidCells.Add(Cells[x, y] = new Cell(new Point(x, y)) { Map = this });

                }

            }

        }

        private void LoadMapCellsv6(byte[] fileBytes)
        {
            //Shanda Mir3

            int offSet = 16;
            Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            Height = BitConverter.ToInt16(fileBytes, offSet);

            offSet = 40;

            Cells = new Cell[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {

                    byte flag = 0;

                    if ((fileBytes[offSet] & 0x01) != 1)
                        flag = 2;
                    else if ((fileBytes[offSet] & 0x02) != 2)
                        flag = 1;

                    offSet += 20;

                    if (flag == 1 || flag == 2)
                    {
                        continue;
                    }

                    ValidCells.Add(Cells[x, y] = new Cell(new Point(x, y)) { Map = this });

                }

            }

        }

        private void LoadMapCellsv7(byte[] fileBytes)
        {
            //3/4 heroes map

            int offSet = 21;
            Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 4;
            Height = BitConverter.ToInt16(fileBytes, offSet);

            offSet = 54;

            Cells = new Cell[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {

                    byte flag = 0;

                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        flag = 2; //Can Fire Over.

                    if ((BitConverter.ToInt16(fileBytes, offSet + 6) & 0x8000) != 0)
                        flag = 1; //Can't Fire Over.

                    offSet += 15;

                    if (flag == 1 || flag == 2)
                    {
                        continue;
                    }

                    ValidCells.Add(Cells[x, y] = new Cell(new Point(x, y)) { Map = this });

                }

            }
        }

        private void LoadMapCellsv8(byte[] fileBytes)
        {
            //Shanda Mir3 - new (some stuff match with old format)

            int offSet = 16;
            Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            Height = BitConverter.ToInt16(fileBytes, offSet);

            offSet = 40;

            Cells = new Cell[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {

                    byte flag = 0;

                    if ((fileBytes[offSet] & 0x01) != 1)
                        flag = 2;
                    else if ((fileBytes[offSet] & 0x02) != 2)
                        flag = 1;

                    offSet += 20;

                    if (flag == 1 || flag == 2)
                    {
                        continue;
                    }

                    ValidCells.Add(Cells[x, y] = new Cell(new Point(x, y)) { Map = this });

                }

            }

        }

        private void LoadMapCellsV100(byte[] fileBytes)
        {
            //C# custom

            int offSet = 4;
            if ((fileBytes[0] != 1) || (fileBytes[1] != 0)) return;//only support version 1 atm
            Width = BitConverter.ToInt16(fileBytes, offSet);
            offSet += 2;
            Height = BitConverter.ToInt16(fileBytes, offSet);

            offSet = 8;

            Cells = new Cell[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {

                    byte flag = 0;

                    offSet += 2;
                    if ((BitConverter.ToInt32(fileBytes, offSet) & 0x20000000) != 0)
                        flag = 2; //Can Fire Over.

                    offSet += 10;
                    if ((BitConverter.ToInt16(fileBytes, offSet) & 0x8000) != 0)
                        flag = 1; //Can't Fire Over.

                    offSet += 14;

                    if (flag == 1 || flag == 2)
                    {
                        continue;
                    }

                    ValidCells.Add(Cells[x, y] = new Cell(new Point(x, y)) { Map = this });

                }

            }
        }

        private void LoadMapCellsV150(byte[] fileBytes, string fileName)
        {
            //C# woool custom

            int offset = 0;
            //69 bytes TitleMetaData + XtraData
            offset += 70;
            //startOffset jump 4 bytes (int)
            var startOffset = BitConverter.ToInt32(fileBytes, offset);
            offset += 4;

            //width 4 bytes (int)
            Width = BitConverter.ToInt32(fileBytes, offset);
            offset += 4;
            //height 4 bytes (int)
            Height = BitConverter.ToInt32(fileBytes, offset);
            offset += 4;

            if (Width > 1500 || Height > 1500)
            {
                return;
            }

            Cells = new Cell[Width, Height];

            var MapData = new byte[fileBytes.Length - offset];
            Array.ConstrainedCopy(fileBytes, offset, MapData, 0, MapData.Length);


            var pSize = (Width - 1) * (Height - 1) * 13;

            if (fileBytes.Length - offset < pSize)
                return;

            offset = 0;
            //Fill map info

            for (int x = 0; x < Width - 1; x++)
            {
                for (int y = 0; y < Height - 1; y++)
                {

                    byte flag = 0;

                    if (MapData[offset++] == 1)
                        flag = 1;
                    else
                        flag = 0;

                    offset += 12;

                    if (flag == 1 || flag == 2)
                    {
                        continue;
                    }

                    ValidCells.Add(Cells[x, y] = new Cell(new Point(x, y)) { Map = this });

                }

            }
        }

        #endregion

        public void Setup()
        {
            CreateGuards();

            LastPlayer = DateTime.UtcNow;
        }

        private void CreateGuards()
        {
            foreach (GuardInfo info in Info.Guards)
            {
                MonsterObject mob = MonsterObject.GetMonster(info.Monster);
                mob.Direction = info.Direction;

                if (!mob.Spawn(this, new Point(info.X, info.Y)))
                {
                    SEnvir.Log($"Failed to spawn Guard Map:{Info.Description}, Location: {info.X}, {info.Y}");
                    continue;
                }
            }
        }


        public void Process()
        {
            if (LastPlayer.AddMinutes(1) < DateTime.UtcNow && Players.Any())
            {
                LastPlayer = DateTime.UtcNow;
            }
        }

        public void AddObject(MapObject ob)
        {
            Objects.Add(ob);

            switch (ob.Race)
            {
                case ObjectType.Player:
                    Players.Add((PlayerObject)ob);
                    break;
                case ObjectType.Item:
                    break;
                case ObjectType.NPC:
                    NPCs.Add((NPCObject)ob);
                    break;
                case ObjectType.Spell:
                    break;
                case ObjectType.Monster:
                    MonsterObject mob = (MonsterObject)ob;
                    if (mob.MonsterInfo.IsBoss)
                        Bosses.Add(mob);
                    break;
            }
        }
        public void RemoveObject(MapObject ob)
        {
            Objects.Remove(ob);

            switch (ob.Race)
            {
                case ObjectType.Player:
                    Players.Remove((PlayerObject)ob);
                    break;
                case ObjectType.Item:
                    break;
                case ObjectType.NPC:
                    NPCs.Remove((NPCObject)ob);
                    break;
                case ObjectType.Spell:
                    break;
                case ObjectType.Monster:
                    MonsterObject mob = (MonsterObject)ob;
                    if (mob.MonsterInfo.IsBoss)
                        Bosses.Remove(mob);
                    break;
            }
        }

        public Cell GetCell(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return null;

            return Cells[x, y];
        }
        public Cell GetCell(Point location)
        {
            return GetCell(location.X, location.Y);
        }
        public List<Cell> GetCells(Point location, int minRadius, int maxRadius)
        {
            List<Cell> cells = new List<Cell>();

            for (int d = 0; d <= maxRadius; d++)
            {
                for (int y = location.Y - d; y <= location.Y + d; y++)
                {
                    if (y < 0) continue;
                    if (y >= Height) break;

                    for (int x = location.X - d; x <= location.X + d; x += Math.Abs(y - location.Y) == d ? 1 : d * 2)
                    {
                        if (x < 0) continue;
                        if (x >= Width) break;

                        Cell cell = Cells[x, y]; //Direct Access we've checked the boudaries.

                        if (cell == null) continue;

                        cells.Add(cell);
                    }
                }
            }

            return cells;
        }

        public Point GetRandomLocation()
        {
            return ValidCells.Count > 0 ? ValidCells[SEnvir.Random.Next(ValidCells.Count)].Location : Point.Empty;
        }

        public Point GetRandomLocation(Point location, int range, int attempts = 25)
        {
            int minX = Math.Max(0, location.X - range);
            int maxX = Math.Min(Width, location.X + range);
            int minY = Math.Max(0, location.Y - range);
            int maxY = Math.Min(Height, location.Y + range);

            for (int i = 0; i < attempts; i++)
            {
                Point test = new Point(SEnvir.Random.Next(minX, maxX), SEnvir.Random.Next(minY, maxY));

                if (GetCell(test) != null)
                    return test;
            }

            return Point.Empty;
        }

        public Point GetRandomLocation(int minX, int maxX, int minY, int maxY, int attempts = 25)
        {
            for (int i = 0; i < attempts; i++)
            {
                Point test = new Point(SEnvir.Random.Next(minX, maxX), SEnvir.Random.Next(minY, maxY));

                if (GetCell(test) != null)
                    return test;
            }

            return Point.Empty;
        }

        public void Broadcast(Point location, Packet p)
        {
            foreach (PlayerObject player in Players)
            {
                if (!Functions.InRange(location, player.CurrentLocation, Config.MaxViewRange)) continue;
                player.Enqueue(p);
            }
        }
        public void Broadcast(Packet p)
        {
            foreach (PlayerObject player in Players)
                player.Enqueue(p);
        }
    }

    public class SpawnInfo
    {
        public RespawnInfo Info;
        public Map CurrentMap;

        public DateTime NextSpawn;
        public int AliveCount;

        public DateTime LastCheck;

        public SpawnInfo(RespawnInfo info, InstanceInfo instance, byte index)
        {
            Info = info;
            CurrentMap = SEnvir.GetMap(info.Region.Map, instance, index);
            LastCheck = SEnvir.Now;
        }

        public void DoSpawn(bool eventSpawn)
        {
            if (CurrentMap.Instance != null)
            {

            }

            if (!eventSpawn)
            {
                if (Info.EventSpawn || SEnvir.Now < NextSpawn) return;

                if (Info.Delay >= 1000000)
                {
                    TimeSpan timeofDay = TimeSpan.FromMinutes(Info.Delay - 1000000);

                    if (LastCheck.TimeOfDay >= timeofDay || SEnvir.Now.TimeOfDay < timeofDay)
                    {
                        LastCheck = SEnvir.Now;
                        return;
                    }

                    LastCheck = SEnvir.Now;
                }
                else
                {
                    if (Info.Announce)
                        NextSpawn = SEnvir.Now.AddSeconds(Info.Delay * 60);
                    else
                        NextSpawn = SEnvir.Now.AddSeconds(SEnvir.Random.Next(Info.Delay * 60) + Info.Delay * 30);

                }
            }

            for (int i = AliveCount; i < Info.Count; i++)
            {
                MonsterObject mob = MonsterObject.GetMonster(Info.Monster);

                if (!Info.Monster.IsBoss)
                {
                    if (SEnvir.Now > CurrentMap.HalloweenEventTime && SEnvir.Now <= Config.HalloweenEventEnd)
                    {
                        mob = new HalloweenMonster { MonsterInfo = Info.Monster, HalloweenEventMob = true };
                        CurrentMap.HalloweenEventTime = SEnvir.Now.AddHours(1);
                    }
                    else if (SEnvir.Now > CurrentMap.ChristmasEventTime && SEnvir.Now <= Config.ChristmasEventEnd)
                    {
                        mob = new ChristmasMonster { MonsterInfo = Info.Monster, ChristmasEventMob = true };
                        CurrentMap.ChristmasEventTime = SEnvir.Now.AddMinutes(20);
                    }
                }


                mob.SpawnInfo = this;

                if (!mob.Spawn(Info.Region, CurrentMap.Instance, CurrentMap.InstanceSequence))
                {
                    mob.SpawnInfo = null;
                    continue;
                }

                if (Info.Announce)
                {
                    if (Info.Delay >= 1000000)
                    {
                        foreach (SConnection con in SEnvir.Connections)
                            con.ReceiveChat($"{mob.MonsterInfo.MonsterName} has appeared.", MessageType.System);
                    }
                    else
                    {
                        foreach (SConnection con in SEnvir.Connections)
                            con.ReceiveChat(string.Format(con.Language.BossSpawn, CurrentMap.Info.Description), MessageType.System);
                    }
                }

                mob.DropSet = Info.DropSet;
                AliveCount++;
            }
        }
    }

    public class Cell
    {
        public Point Location;

        public Map Map;

        public List<MapObject> Objects;
        public SafeZoneInfo SafeZone;

        public List<MovementInfo> Movements;

        public List<QuestTask> QuestTasks;

        public Cell(Point location)
        {
            Location = location;
        }


        public void AddObject(MapObject ob)
        {
            if (Objects == null)
                Objects = new List<MapObject>();

            Objects.Add(ob);

            ob.CurrentMap = Map;
            ob.CurrentLocation = Location;

            Map.OrderedObjects[Location.X].Add(ob);
        }
        public void RemoveObject(MapObject ob)
        {
            Objects.Remove(ob);

            if (Objects.Count == 0)
                Objects = null;

            Map.OrderedObjects[Location.X].Remove(ob);
        }
        public bool IsBlocking(MapObject checker, bool cellTime)
        {
            if (Objects == null) return false;

            foreach (MapObject ob in Objects)
            {
                if (!ob.Blocking) continue;
                if (cellTime && SEnvir.Now < ob.CellTime) continue;

                if (ob.Stats == null) return true;

                if (ob.Buffs.Any(x => x.Type == BuffType.Cloak || x.Type == BuffType.Transparency) && ob.Level > checker.Level && !ob.InGroup(checker)) continue;


                return true;
            }

            return false;
        }

        public Cell GetMovement(MapObject ob)
        {
            if (QuestTasks != null && QuestTasks.Count > 0)
            {
                if (ob.Race == ObjectType.Player)
                {
                    PlayerObject player = (PlayerObject)ob;

                    foreach (var task in QuestTasks)
                    {
                        var userQuest = player.Quests.FirstOrDefault(x => x.QuestInfo == task.Quest && !x.Completed);

                        if (userQuest == null) continue;

                        UserQuestTask userTask = userQuest.Tasks.FirstOrDefault(x => x.Task == task);

                        if (userTask == null)
                        {
                            userTask = SEnvir.UserQuestTaskList.CreateNewObject();
                            userTask.Task = task;
                            userTask.Quest = userQuest;
                        }

                        if (userTask.Completed) continue;

                        userTask.Amount = 1;

                        player.Enqueue(new S.QuestChanged { Quest = userQuest.ToClientInfo() });
                    }
                }
            }

            if (Movements == null || Movements.Count == 0)
                return this;

            for (int i = 0; i < 5; i++) //20 Attempts to get movement;
            {
                MovementInfo movement = Movements[SEnvir.Random.Next(Movements.Count)];

                Map map = SEnvir.GetMap(movement.DestinationRegion.Map, Map.Instance, Map.InstanceSequence);

                if (movement.NeedInstance != null)
                {
                    if (ob.Race != ObjectType.Player) break;

                    if (Map.Instance != null) //Moving from instance
                    {
                        map = SEnvir.GetMap(movement.DestinationRegion.Map, null, 0);
                    }
                    else //Moving to instance
                    {
                        var (index, result) = ((PlayerObject)ob).GetInstance(movement.NeedInstance);

                        if (result != InstanceResult.Success)
                        {
                            ((PlayerObject)ob).SendInstanceMessage(movement.NeedInstance, result);
                            break;
                        }

                        map = SEnvir.GetMap(movement.DestinationRegion.Map, movement.NeedInstance, index.Value);
                    }
                }

                if (map == null) break;

                Cell cell = map.GetCell(movement.DestinationRegion.PointList[SEnvir.Random.Next(movement.DestinationRegion.PointList.Count)]);

                if (cell == null) continue;

                if (ob.Race == ObjectType.Player)
                {
                    bool allowed = true;
                    PlayerObject player = (PlayerObject)ob;

                    if (movement.DestinationRegion.Map.MinimumLevel > ob.Level && !player.Character.Account.TempAdmin)
                    {
                        player.Connection.ReceiveChat(string.Format(player.Connection.Language.NeedLevel, movement.DestinationRegion.Map.MinimumLevel), MessageType.System);

                        foreach (SConnection con in player.Connection.Observers)
                            con.ReceiveChat(string.Format(con.Language.NeedLevel, movement.DestinationRegion.Map.MinimumLevel), MessageType.System);

                        break;
                    }
                    if (movement.DestinationRegion.Map.MaximumLevel > 0 && movement.DestinationRegion.Map.MaximumLevel < ob.Level && !player.Character.Account.TempAdmin)
                    {
                        player.Connection.ReceiveChat(string.Format(player.Connection.Language.NeedMaxLevel, movement.DestinationRegion.Map.MaximumLevel), MessageType.System);

                        foreach (SConnection con in player.Connection.Observers)
                            con.ReceiveChat(string.Format(con.Language.NeedMaxLevel, movement.DestinationRegion.Map.MaximumLevel), MessageType.System);

                        break;
                    }

                    if (movement.DestinationRegion.Map.RequiredClass != RequiredClass.None && movement.DestinationRegion.Map.RequiredClass != RequiredClass.All)
                    {
                        switch (player.Class)
                        {
                            case MirClass.Warrior:
                                if ((movement.DestinationRegion.Map.RequiredClass & RequiredClass.Warrior) != RequiredClass.Warrior)
                                    allowed = false;
                                break;
                            case MirClass.Wizard:
                                if ((movement.DestinationRegion.Map.RequiredClass & RequiredClass.Wizard) != RequiredClass.Wizard)
                                    allowed = false;
                                break;
                            case MirClass.Taoist:
                                if ((movement.DestinationRegion.Map.RequiredClass & RequiredClass.Taoist) != RequiredClass.Taoist)
                                    allowed = false;
                                break;
                            case MirClass.Assassin:
                                if ((movement.DestinationRegion.Map.RequiredClass & RequiredClass.Assassin) != RequiredClass.Assassin)
                                    allowed = false;
                                break;
                        }

                        if (!allowed)
                        {
                            var message = string.Format(player.Connection.Language.NeedClass, movement.DestinationRegion.Map.RequiredClass.ToString());
                            player.Connection.ReceiveChat(message, MessageType.System);

                            foreach (SConnection con in player.Connection.Observers)
                                con.ReceiveChat(message, MessageType.System);

                            break;
                        }
                    }

                    if (movement.NeedSpawn != null)
                    {
                        SpawnInfo spawn = SEnvir.Spawns.FirstOrDefault(x => x.Info == movement.NeedSpawn);

                        if (spawn == null)
                            break;

                        if (spawn.AliveCount == 0)
                        {
                            player.Connection.ReceiveChat(player.Connection.Language.NeedMonster, MessageType.System);

                            foreach (SConnection con in player.Connection.Observers)
                                con.ReceiveChat(con.Language.NeedMonster, MessageType.System);

                            break;
                        }
                    }

                    if (movement.NeedItem != null)
                    {
                        if (player.GetItemCount(movement.NeedItem) == 0)
                        {
                            player.Connection.ReceiveChat(string.Format(player.Connection.Language.NeedItem, movement.NeedItem.ItemName), MessageType.System);

                            foreach (SConnection con in player.Connection.Observers)
                                con.ReceiveChat(string.Format(con.Language.NeedItem, movement.NeedItem.ItemName), MessageType.System);
                            break;
                        }

                        player.TakeItem(movement.NeedItem, 1);
                    }

                    switch (movement.Effect)
                    {
                        case MovementEffect.SpecialRepair:
                            player.SpecialRepair(EquipmentSlot.Weapon);
                            player.SpecialRepair(EquipmentSlot.Shield);
                            player.SpecialRepair(EquipmentSlot.Helmet);
                            player.SpecialRepair(EquipmentSlot.Armour);
                            player.SpecialRepair(EquipmentSlot.Necklace);
                            player.SpecialRepair(EquipmentSlot.BraceletL);
                            player.SpecialRepair(EquipmentSlot.BraceletR);
                            player.SpecialRepair(EquipmentSlot.RingL);
                            player.SpecialRepair(EquipmentSlot.RingR);
                            player.SpecialRepair(EquipmentSlot.Shoes);

                            player.RefreshStats();
                            break;
                    }
                }

                return cell.GetMovement(ob);
            }

            return this;
        }
    }
}
