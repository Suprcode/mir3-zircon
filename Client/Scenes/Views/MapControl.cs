using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Client.Controls;
using Client.Envir;
using Client.Models;
using Client.Models.Particles;
using Library;
using Library.SystemModels;
using LibraryCore.Maps;
using SlimDX;
using SlimDX.Direct3D9;
using C = Library.Network.ClientPackets;

//Cleaned
namespace Client.Scenes.Views
{
    public sealed class MapControl : DXControl
    {
        #region Properties

        public static UserObject User => GameScene.Game.User;

        #region MapInformation

        public MapFormat.MapTypeInfo MapFormat { get; private set; }

        public MapInfo MapInfo
        {
            get => _MapInfo;
            set
            {
                if (_MapInfo == value) return;

                MapInfo oldValue = _MapInfo;
                _MapInfo = value;

                OnMapInfoChanged(oldValue, value);
            }
        }
        private MapInfo _MapInfo;

        public InstanceInfo InstanceInfo
        {
            get => _InstanceInfo;
            set
            {
                if (_InstanceInfo == value) return;

                InstanceInfo oldValue = _InstanceInfo;
                _InstanceInfo = value;
                OnInstanceInfoChanged(oldValue, value);
            }
        }
        private InstanceInfo _InstanceInfo;
        public void OnInstanceInfoChanged(InstanceInfo oValue, InstanceInfo nValue)
        {
            MapInfoChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> MapInfoChanged;
        public void OnMapInfoChanged(MapInfo oValue, MapInfo nValue)
        {
            TextureValid = false;
            LoadMap();

            if (oValue != null)
            {
                if (nValue == null || nValue.Music != oValue.Music)
                    DXSoundManager.Stop(oValue.Music);
            }

            if (nValue != null)
                DXSoundManager.Play(nValue.Music);

            UpdateWeather();
            LLayer.UpdateLights();
            MapInfoChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Animation

        public int Animation
        {
            get => _Animation;
            set
            {
                if (_Animation == value) return;

                int oldValue = _Animation;
                _Animation = value;

                OnAnimationChanged(oldValue, value);
            }
        }
        private int _Animation;
        public event EventHandler<EventArgs> AnimationChanged;
        public void OnAnimationChanged(int oValue, int nValue)
        {
            AnimationChanged?.Invoke(this, EventArgs.Empty);
            TextureValid = false;
        }



        #endregion
        
        #region MouseLocation

        public Point MouseLocation
        {
            get => _MouseLocation;
            set
            {
                if (_MouseLocation == value) return;

                Point oldValue = _MouseLocation;
                _MouseLocation = value;

                OnMouseLocationChanged(oldValue, value);
            }
        }
        private Point _MouseLocation;
        public event EventHandler<EventArgs> MouseLocationChanged;
        public void OnMouseLocationChanged(Point oValue, Point nValue)
        {
            MouseLocationChanged?.Invoke(this, EventArgs.Empty);
            UpdateMapLocation();
        }

        #endregion

        public override void OnSizeChanged(Size oValue, Size nValue)
        {
            base.OnSizeChanged(oValue, nValue);

            if (FLayer != null)
                FLayer.Size = Size;

            if (LLayer != null)
                LLayer.Size = Size;


            OffSetX = Size.Width/2/CellWidth;
            OffSetY = Size.Height/2/CellHeight;
        }

        public MouseButtons MapButtons;
        public Point MapLocation;

        public bool Mining;
        public Point MiningPoint;
        public MirDirection MiningDirection;

        public FishingState FishingState;
        public Point FloatLocation;
        public MirDirection FishingDirection;
        public bool AutoCast;

        public Floor FLayer;
        public Light LLayer;

        public Cell[,] Cells;
        public int Width, Height;

        public List<DXControl> MapInfoObjects = new List<DXControl>();
        public List<MapObject> Objects = new List<MapObject>();
        public List<MirEffect> Effects = new List<MirEffect>();
        public List<Models.Particles.ParticleEmitter> ParticleEffects = new List<Models.Particles.ParticleEmitter>();

        //public const int CellWidth = 48, CellHeight = 32;

        //48
        public int CellWidth => MapFormat.mapType == MapType.Woool ? 64 : 48;

        //32
        public int CellHeight => 32;

        public int ViewRangeX = 12, ViewRangeY = 24;

        public static int OffSetX;
        public static int OffSetY;


        #endregion

        public MapControl()
        {
            DrawTexture = true;

            BackColour = Color.Empty;
            
            FLayer = new Floor { Parent = this, Size = Size };
            LLayer = new Light { Parent = this, Location = new Point(-GameScene.Game.Location.X, -GameScene.Game.Location.Y), Size = Size };
        }

        #region Methods

        protected override void OnClearTexture()
        {
            base.OnClearTexture();

            if (!Visible) return;

            if (FLayer.TextureValid)
                DXManager.Sprite.Draw(FLayer.ControlTexture, Color.White);

            if (Config.DrawEffects)
            {
                foreach (MirEffect ob in Effects)
                {
                    if (ob.DrawType != DrawType.Floor) continue;

                    ob.Draw();
                }
            }

            DrawObjects();

            if (MapObject.MouseObject != null) // && MapObject.MouseObject != MapObject.TargetObject)
                MapObject.MouseObject.DrawBlend();

            foreach (MapObject ob in Objects)
            {
                if (ob.Dead) continue;

                switch (ob.Race)
                {
                    case ObjectType.Player:
                        if (!Config.ShowPlayerNames) continue;

                        break;
                    case ObjectType.Item:
                        if (!Config.ShowItemNames || ob.CurrentLocation == MapLocation) continue;
                        break;
                    case ObjectType.NPC:
                        break;
                    case ObjectType.Spell:
                        break;
                    case ObjectType.Monster:
                        if (!Config.ShowMonsterNames) continue;
                        break;
                }

                ob.DrawName();
            }

            if (Config.DrawEffects)
            {
                foreach (MirEffect ob in Effects)
                {
                    if (ob.DrawType != DrawType.Final) continue;

                     ob.Draw();
                }
            }

            if (MapObject.MouseObject != null && MapObject.MouseObject.Race != ObjectType.Item)
                MapObject.MouseObject.DrawName();



            foreach (MapObject ob in Objects)
            {
                ob.DrawChat();
                ob.DrawPoison();
                ob.DrawHealth();
            }

            if (Config.ShowDamageNumbers)
                foreach (MapObject ob in Objects)
                    ob.DrawDamage();

            if (MapLocation.X >= 0 && MapLocation.X < Width && MapLocation.Y >= 0 && MapLocation.Y < Height)
            {
                Cell cell = Cells[MapLocation.X, MapLocation.Y];
                int layer = 0;
                if (cell.Objects != null)
                    for (int i = cell.Objects.Count - 1; i >= 0; i--)
                    {
                        ItemObject ob = cell.Objects[i] as ItemObject;

                        ob?.DrawFocus(layer++);
                    }
            }

            DXManager.Sprite.Flush();

            DXManager.Device.SetRenderState(RenderState.SourceBlend, Blend.Zero);
            DXManager.Device.SetRenderState(RenderState.DestinationBlend, Blend.SourceColor);

            DXManager.Sprite.Draw(LLayer.ControlTexture, Color.White);

            DXManager.Sprite.End();
            DXManager.Sprite.Begin(SpriteFlags.AlphaBlend);
            
        }
        public override void Draw()
        {
            if (!IsVisible || Size.Width == 0 || Size.Height == 0) return;

            FLayer.CheckTexture();
            LLayer.CheckTexture();
            
            //CreateTexture();
            OnBeforeDraw();

            DrawControl();
            
            DrawBorder();
            OnAfterDraw();
        }

        private void DrawObjects()
        {

            if (User == null)
                return;

            if (GameScene.Game.MapControl.MapInfo == null)
                return;

            if (Cells == null) return;

            byte animation = (byte)0;
            bool blend = false;

            MirLibrary library = null;
            LibraryFile file;

            int minX = Math.Max(0, User.CurrentLocation.X - OffSetX - 4), maxX = Math.Min(Width - 1, User.CurrentLocation.X + OffSetX + 4);
            int minY = Math.Max(0, User.CurrentLocation.Y - OffSetY - 4), maxY = Math.Min(Height - 1, User.CurrentLocation.Y + OffSetY + 40);//25

            maxY = Math.Min(GameScene.Game.MapControl.Cells.GetUpperBound(1), maxY);
            maxX = Math.Min(GameScene.Game.MapControl.Cells.GetUpperBound(0), maxX);

            int drawX = 0;
            int drawY = 0;
            Cell cell = new Cell();

            for (int y = minY; y <= maxY; y++)
            {
                drawY = (y - User.CurrentLocation.Y + OffSetY + 1) * CellHeight - User.MovingOffSet.Y;

                for (int x = minX; x <= maxX; x++)
                {
                    drawX = (x - User.CurrentLocation.X + OffSetX) * CellWidth - User.MovingOffSet.X;

                    cell = Cells[x, y];

                    animation = (byte)0;
                    blend = false;

                    switch (GameScene.Game.MapControl.MapFormat.mapType)
                    {
                        case MapType.Other:
                            animation = 0;

                            #region Mir3 Layer
                            if (cell.Mir3Layer)
                            {
                                library = CrystalMaps.MapLibs[cell.MiddleFile];

                                if (library != null)
                                {
                                    int index = cell.MiddleImage - 1;

                                    blend = false;

                                    Size s = library.GetSize(index);

                                    if (!blend)
                                    {
                                        

                                        library.Draw(index, drawX, drawY - s.Height, Color.White, false, 1F, ImageType.Image);
                                    }
                                    else
                                        library.DrawBlend(index, drawX, drawY - s.Height, Color.White, false, 0.5F, ImageType.Image);

                                }

                            }
                            #endregion

                            if (cell.FrontFile == -1) continue;

                            animation = 0;
                            library = CrystalMaps.MapLibs[cell.FrontFile];

                            if (library != null)
                            {
                                int index = (cell.FrontImage & short.MaxValue) - 1;

                                if (index < 0) continue;

                                animation = (byte)cell.FrontAnimationFrame;

                                if ((animation & 0x80) > 0)
                                {
                                    blend = true;
                                    animation &= 0x7F;
                                }
                                else
                                    blend = false;

                                if (animation > 0)
                                {
                                    byte animationTick = (byte)cell.FrontAnimationTick;
                                    index += (Animation % (animation + (animation * animationTick))) / (1 + animationTick);
                                }

                                Size s = library.GetSize(index);

                                #region revised & optimized

                                if (s.Width == CellWidth && s.Height == CellHeight && animation == 0) continue;
                                if ((s.Width == CellWidth * 2) && (s.Height == CellHeight * 2) && (animation == 0)) continue;

                                if (s != Size.Empty)
                                {
                                    if ((s.Width != CellWidth || s.Height != CellHeight) && (s.Width != CellWidth * 2 || s.Height != CellHeight * 2))
                                    {
                                        if (blend)
                                        {
                                            if (cell.FrontFileNegativeDisplacement)
                                                library.DrawBlend(index, drawX, drawY - (3 * CellHeight), Color.White, false, 1F, ImageType.Image);
                                            else
                                                library.DrawBlend(index, drawX, drawY - s.Height, Color.White, (index >= 2723 && index <= 2732), 1F, ImageType.Image);
                                        }
                                        else
                                        {
                                            library.Draw(index, drawX, drawY - s.Height, Color.White, false, 1F, ImageType.Image);
                                        }
                                    }
                                }
                                #endregion

                            }
                            break;
                        case MapType.Woool:
                            animation = (byte)0;
                            blend = false;

                            if (cell.MiddleFile != -1)
                            {
                                library = CrystalMaps.MapLibs[cell.MiddleFile];

                                if (library != null)
                                {
                                    int FirstFrame = cell.MiddleImage;
                                    int index = FirstFrame;
                                    int subIndex = -1;

                                    blend = false;

                                    animation = (byte)(cell.MiddleAnimationFrame);

                                    if (cell.MiddleBlend)
                                    {
                                        blend = true;
                                        animation &= 0x7F;
                                    }
                                    else
                                        blend = false;

                                    if (animation > 0)
                                    {
                                        byte animationTick = (byte)cell.MiddleAnimationTick;
                                        index += (Animation % (animation + (animation * animationTick))) / (1 + animationTick);
                                        if (index != FirstFrame)
                                            subIndex = index - 1 - FirstFrame;
                                        else
                                            subIndex = -1;
                                    }

                                    Size s = library.GetSize(FirstFrame);

                                    int CustomCellHeight = cell.CustomCellHeight;

                                    if (cell.MiddleSpecialBlend)
                                        //some black gradients fixes (libs are ok)
                                        library.DrawSpecialBlend(FirstFrame, drawX, drawY - s.Height + CustomCellHeight, Color.White, false, 1F, ImageType.Image);
                                    else if (!blend)
                                        library.Draw(FirstFrame, drawX, drawY - s.Height + CustomCellHeight, Color.White, false, 1F, ImageType.Image, 1, subIndex);
                                    else
                                        library.DrawBlend(FirstFrame, drawX, drawY - s.Height + CustomCellHeight, Color.White, false, 0.5F, ImageType.Image, 0, subIndex);

                                }

                            }

                            if (cell.FrontFile == -1) continue;

                            animation = (byte)0;

                            library = CrystalMaps.MapLibs[cell.FrontFile];

                            if (library != null)
                            {
                                int FirstFrame = cell.FrontImage;
                                int index = FirstFrame;
                                int subIndex = -1;

                                if (index < 0) continue;

                                blend = false;

                                animation = (byte)(cell.FrontAnimationFrame);

                                if (cell.FrontBlend)
                                {
                                    blend = true;
                                    animation &= 0x7F;
                                }
                                else
                                    blend = false;

                                if (animation > 0)
                                {
                                    byte animationTick = (byte)cell.FrontAnimationTick;
                                    index += (Animation % (animation + (animation * animationTick))) / (1 + animationTick);
                                    if (index != FirstFrame)
                                        subIndex = index - 1 - FirstFrame;
                                    else
                                        subIndex = -1;
                                }

                                Size s = library.GetSize(FirstFrame);

                                if (blend)
                                    library.DrawBlend(FirstFrame, drawX, drawY - s.Height, Color.White, false, 1F, ImageType.Image, 0, subIndex);
                                else
                                    library.Draw(FirstFrame, drawX, drawY - s.Height, Color.White, false, 1F, ImageType.Image, 1, subIndex);

                            }
                            break;
                        case MapType.Mir3:
                        default:
                            if (Libraries.KROrder.TryGetValue(cell.MiddleFile, out file) && file != LibraryFile.Tilesc && CEnvir.LibraryList.TryGetValue(file, out library))
                            {
                                int index = cell.MiddleImage - 1;

                                blend = false;
                                if (cell.MiddleAnimationFrame > 1 && cell.MiddleAnimationFrame < 255)
                                {
                                    index += Animation % (cell.MiddleAnimationFrame & 0x4F);
                                    blend = (cell.MiddleAnimationFrame & 0x50) > 0;
                                }

                                Size s = library.GetSize(index);

                                if ((s.Width != CellWidth || s.Height != CellHeight) && (s.Width != CellWidth * 2 || s.Height != CellHeight * 2))
                                {
                                    if (!blend)
                                        library.Draw(index, drawX, drawY - s.Height, Color.White, false, 1F, ImageType.Image);
                                    else
                                        library.DrawBlend(index, drawX, drawY - s.Height, Color.White, false, 0.5F, ImageType.Image);
                                }
                            }
                            if (Libraries.KROrder.TryGetValue(cell.FrontFile, out file) && file != LibraryFile.Tilesc && CEnvir.LibraryList.TryGetValue(file, out library))
                            {
                                int index = cell.FrontImage - 1;

                                blend = false;
                                if (cell.FrontAnimationFrame > 1 && cell.FrontAnimationFrame < 255)
                                {
                                    index += Animation % (cell.FrontAnimationFrame & 0x7F);
                                    blend = (cell.FrontAnimationFrame & 0x80) > 0;
                                }

                                Size s = library.GetSize(index);

                                if ((s.Width != CellWidth || s.Height != CellHeight) && (s.Width != CellWidth * 2 || s.Height != CellHeight * 2))
                                {
                                    if (!blend)
                                        library.Draw(index, drawX, drawY - s.Height, Color.White, false, 1F, ImageType.Image);
                                    else
                                        library.DrawBlend(index, drawX, drawY - s.Height, Color.White, false, 0.5F, ImageType.Image);
                                }
                            }
                            break;
                    }

                }

                foreach (MapObject ob in Objects)
                {
                    if (ob.RenderY == y)
                        ob.Draw();
                }

                if (Config.DrawEffects)
                {
                    foreach (MirEffect ob in Effects)
                    {
                        if (ob.DrawType != DrawType.Object) continue;

                        if (ob.MapTarget.IsEmpty && ob.Target != null)
                        {
                            if (ob.Target.RenderY == y && ob.Target != User)
                                ob.Draw();
                        }
                        else if (ob.MapTarget.Y == y)
                            ob.Draw();
                    }
                }

            }

            if (User.Opacity != 1f) return;
            float oldOpacity = MapObject.User.Opacity;
            MapObject.User.Opacity = 0.65F;

            MapObject.User.DrawPlayer(false);

            MapObject.User.Opacity = oldOpacity;

            if (Config.DrawEffects)
            {
                if (Config.DrawParticles)
                {
                    foreach (var ob in ParticleEffects)
                    {
                        ob.Draw();
                    }
                }

                foreach (MirEffect ob in Effects)
                {
                    if (ob.DrawType != DrawType.Object || !ob.MapTarget.IsEmpty || ob.Target != User) continue;

                    ob.Draw();
                }
            }

        }

        private void LoadMap()
        {
            try
            {
                var fileName = Config.MapPath + MapInfo.FileName + ".map";

                if (!File.Exists(fileName))
                {
                    throw new Exception($"[LoadMap] File Missing: {fileName}");
                }

                var map = File.ReadAllBytes(fileName);

                MapFormat = LibraryCore.Maps.MapFormat.FindType(map);

                switch (MapFormat.mapType)
                {
                    case MapType.Mir3:
                        //zircon mir3
                        using (MemoryStream mStream = new MemoryStream(map))
                        using (BinaryReader reader = new BinaryReader(mStream))
                        {

                            mStream.Seek(22, SeekOrigin.Begin);
                            Width = reader.ReadInt16();
                            Height = reader.ReadInt16();

                            mStream.Seek(28, SeekOrigin.Begin);

                            Cells = new Cell[Width, Height];
                            for (int x = 0; x < Width; x++)
                                for (int y = 0; y < Height; y++)
                                    Cells[x, y] = new Cell();

                            for (int x = 0; x < Width / 2; x++)
                                for (int y = 0; y < Height / 2; y++)
                                {
                                    Cells[(x * 2), (y * 2)].BackFile = reader.ReadByte();
                                    Cells[(x * 2), (y * 2)].BackImage = reader.ReadUInt16();
                                }

                            for (int x = 0; x < Width; x++)
                                for (int y = 0; y < Height; y++)
                                {
                                    byte flag = reader.ReadByte();
                                    Cells[x, y].MiddleAnimationFrame = reader.ReadByte();

                                    byte value = reader.ReadByte();
                                    Cells[x, y].FrontAnimationFrame = value == 255 ? 0 : value;
                                    Cells[x, y].FrontAnimationFrame &= 0x8F; //Probably a Blend Flag

                                    Cells[x, y].FrontFile = reader.ReadByte();
                                    Cells[x, y].MiddleFile = reader.ReadByte();

                                    Cells[x, y].MiddleImage = reader.ReadUInt16() + 1;
                                    Cells[x, y].FrontImage = reader.ReadUInt16() + 1;

                                    mStream.Seek(3, SeekOrigin.Current);

                                    Cells[x, y].Light = (byte)(reader.ReadByte() & 0x0F) * 2;

                                    mStream.Seek(1, SeekOrigin.Current);

                                    Cells[x, y].Flag = ((flag & 0x01) != 1) || ((flag & 0x02) != 2);
                                }
                        }
                        break;
                    case MapType.Other:
                        LoadOtherMapFormats(map);
                        break;
                    case MapType.Woool:
                        LoadMapType150(map);
                        break;
                    case MapType.None:
                    default:
                        break;
                }

                if (Width > 1500 || Height > 1500)
                {
                    throw new Exception($"[LoadMap] Error loading the file: {fileName}");
                }

                #region old code
                //using (MemoryStream mStream = new MemoryStream(File.ReadAllBytes(fileName)))
                //using (BinaryReader reader = new BinaryReader(mStream))
                //{

                //    mStream.Seek(22, SeekOrigin.Begin);
                //    Width = reader.ReadInt16();
                //    Height = reader.ReadInt16();

                //    mStream.Seek(28, SeekOrigin.Begin);

                //    Cells = new Cell[Width, Height];
                //    for (int x = 0; x < Width; x++)
                //        for (int y = 0; y < Height; y++)
                //            Cells[x, y] = new Cell();

                //    for (int x = 0; x < Width/2; x++)
                //        for (int y = 0; y < Height/2; y++)
                //        {
                //            Cells[(x*2), (y*2)].BackFile = reader.ReadByte();
                //            Cells[(x*2), (y*2)].BackImage = reader.ReadUInt16();
                //        }

                //    for (int x = 0; x < Width; x++)
                //        for (int y = 0; y < Height; y++)
                //        {
                //            byte flag = reader.ReadByte();
                //            Cells[x, y].MiddleAnimationFrame = reader.ReadByte();

                //            byte value = reader.ReadByte();
                //            Cells[x, y].FrontAnimationFrame = value == 255 ? 0 : value;
                //            Cells[x, y].FrontAnimationFrame &= 0x8F; //Probably a Blend Flag

                //            Cells[x, y].FrontFile = reader.ReadByte();
                //            Cells[x, y].MiddleFile = reader.ReadByte();

                //            Cells[x, y].MiddleImage = reader.ReadUInt16() + 1;
                //            Cells[x, y].FrontImage = reader.ReadUInt16() + 1;

                //            mStream.Seek(3, SeekOrigin.Current);

                //            Cells[x, y].Light = (byte) (reader.ReadByte() & 0x0F)*2;

                //            mStream.Seek(1, SeekOrigin.Current);

                //            Cells[x, y].Flag = ((flag & 0x01) != 1) || ((flag & 0x02) != 2);
                //        }
                //}
                #endregion

            }
            catch (Exception ex)
            {
                CEnvir.SaveException(ex);
            }

            OffSetX = Size.Width / 2 / CellWidth;
            OffSetY = Size.Height / 2 / CellHeight;

            if (FLayer != null)
                FLayer.TextureValid = false;

            if (LLayer != null)
                LLayer.TextureValid = false;

            foreach (MapObject ob in Objects)
                if (ob.CurrentLocation.X < Width && ob.CurrentLocation.Y < Height)
                    Cells[ob.CurrentLocation.X, ob.CurrentLocation.Y].AddObject(ob);
        }

        #region MapLoaders
        private void LoadOtherMapFormats(byte[] Bytes)
        {

            //c# custom map format
            if ((Bytes[2] == 0x43) && (Bytes[3] == 0x23))
            {
                LoadMapType100(Bytes);
                goto Optimizations;
            }

            //wemade mir3 maps have no title they just start with blank bytes
            if (Bytes[0] == 0)
            {
                LoadMapType5(Bytes);
                goto Optimizations;
            }

            //shanda mir3 maps start with title: (C) SNDA, MIR3.
            if ((Bytes[0] == 0x0F) && (Bytes[5] == 0x53) && (Bytes[14] == 0x33))
            {
                LoadMapType6(Bytes);
                return;
            }

            //shanda mir3 maps start with title: (C) MIR3, MIR3. (NEW)
            if ((Bytes[0] == 0x0F) && (Bytes[5] == 0x4D) && (Bytes[14] == 0x33))
            {
                LoadMapType8(Bytes);
                goto Optimizations;
            }

            //wemades antihack map (laby maps) title start with: Mir2 AntiHack
            if ((Bytes[0] == 0x15) && (Bytes[4] == 0x32) && (Bytes[6] == 0x41) && (Bytes[19] == 0x31))
            {
                LoadMapType4(Bytes);
                goto Optimizations;
            }

            //wemades 2010 map format i guess title starts with: Map 2010 Ver 1.0
            if ((Bytes[0] == 0x10) && (Bytes[2] == 0x61) && (Bytes[7] == 0x31) && (Bytes[14] == 0x31))
            {
                LoadMapType1(Bytes);
                goto Optimizations;
            }

            //shanda's 2012 format and one of shandas(wemades) older formats share same header info, only difference is the filesize
            if ((Bytes[4] == 0x0F) || (Bytes[4] == 0x03) && (Bytes[18] == 0x0D) && (Bytes[19] == 0x0A))
            {
                int W = Bytes[0] + (Bytes[1] << 8);
                int H = Bytes[2] + (Bytes[3] << 8);
                if (Bytes.Length > (52 + (W * H * 14)))
                {
                    LoadMapType3(Bytes);
                    goto Optimizations;
                }
                else
                {
                    LoadMapType2(Bytes);
                    goto Optimizations;
                }
            }

            //3/4 heroes map format (myth/lifcos i guess)
            if ((Bytes[0] == 0x0D) && (Bytes[1] == 0x4C) && (Bytes[7] == 0x20) && (Bytes[11] == 0x6D))
            {
                LoadMapType7(Bytes);
                goto Optimizations;
            }

            //if it's none of the above load the default old school format
            LoadMapType0(Bytes);

        Optimizations:
            #region Tweak Optimizations
            if (MapFormat.mapType == MapType.Other)
            {
                //usually mir2 and mir3 generic maps

                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                    {
                        var cell = Cells[x, y];

                        if (!((cell.MiddleFile > 199) && (cell.MiddleFile < 300)))
                            cell.Mir3Layer = true;
                        else
                        {
                            //cell.MiddleFile = -1;//only mir3 has middle layer
                            cell.Mir3Layer = false;
                        }

                        if (cell.Mir3Layer) continue;

                        if (cell.FrontFile == -1) continue;

                        //under testing
                        //if (CrystalMaps.MapLibs[cell.FrontFile] == null)
                        //{
                        //    //disable because we dont have the libs
                        //    cell.FrontFile = -1;
                        //    continue;
                        //}

                        if ((cell.FrontFile > 99) & (cell.FrontFile < 199))
                            cell.FrontFileNegativeDisplacement = true;

                    }
            }
            #endregion

        }

        private void LoadMapType0(byte[] Bytes)
        {
            int offset = 0;
            Width = BitConverter.ToInt16(Bytes, offset);
            offset += 2;
            Height = BitConverter.ToInt16(Bytes, offset);
            Cells = new Cell[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Cells[x, y] = new Cell();
            offset = 52;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {//12
                    Cells[x, y] = new Cell();
                    Cells[x, y].BackFile = 0;
                    Cells[x, y].MiddleFile = 1;
                    Cells[x, y].BackImage = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    Cells[x, y].MiddleImage = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    Cells[x, y].FrontImage = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    Cells[x, y].DoorIndex = (byte)(Bytes[offset++] & 0x7F);
                    Cells[x, y].DoorOffset = Bytes[offset++];
                    Cells[x, y].FrontAnimationFrame = Bytes[offset++];
                    Cells[x, y].FrontAnimationTick = Bytes[offset++];
                    Cells[x, y].FrontFile = (short)(Bytes[offset++] + 2);
                    Cells[x, y].Light = Bytes[offset++];

                    if ((Cells[x, y].BackImage & 0x8000) != 0)
                        Cells[x, y].BackImage = (Cells[x, y].BackImage & 0x7FFF) | 0x20000000;

                    Cells[x, y].Flag = (Cells[x, y].BackImage & 0x20000000) != 0 || (Cells[x, y].FrontImage & 0x8000) != 0;

                }
        }
        private void LoadMapType1(byte[] Bytes)
        {
            int offSet = 21;

            int w = BitConverter.ToInt16(Bytes, offSet);
            offSet += 2;
            int xor = BitConverter.ToInt16(Bytes, offSet);
            offSet += 2;
            int h = BitConverter.ToInt16(Bytes, offSet);
            Width = w ^ xor;
            Height = h ^ xor;
            Cells = new Cell[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Cells[x, y] = new Cell();
            offSet = 54;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    Cells[x, y] = new Cell
                    {
                        BackFile = 0,
                        BackImage = (int)(BitConverter.ToInt32(Bytes, offSet) ^ 0xAA38AA38),
                        MiddleFile = 1,
                        MiddleImage = (short)(BitConverter.ToInt16(Bytes, offSet += 4) ^ xor),
                        FrontImage = (short)(BitConverter.ToInt16(Bytes, offSet += 2) ^ xor),
                        DoorIndex = (byte)(Bytes[offSet += 2] & 0x7F),
                        DoorOffset = Bytes[++offSet],
                        FrontAnimationFrame = Bytes[++offSet],
                        FrontAnimationTick = Bytes[++offSet],
                        FrontFile = (short)(Bytes[++offSet] + 2),
                        Light = Bytes[++offSet],
                        Unknown = Bytes[++offSet]
                    };

                    Cells[x, y].Flag = (Cells[x, y].BackImage & 0x20000000) != 0 || (Cells[x, y].FrontImage & 0x8000) != 0;

                    offSet++;

                }
        }
        private void LoadMapType2(byte[] Bytes)
        {
            int offset = 0;
            Width = BitConverter.ToInt16(Bytes, offset);
            offset += 2;
            Height = BitConverter.ToInt16(Bytes, offset);
            Cells = new Cell[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Cells[x, y] = new Cell();
            offset = 52;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {//14
                    Cells[x, y] = new Cell();
                    Cells[x, y].BackImage = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    Cells[x, y].MiddleImage = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    Cells[x, y].FrontImage = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    Cells[x, y].DoorIndex = (byte)(Bytes[offset++] & 0x7F);
                    Cells[x, y].DoorOffset = Bytes[offset++];
                    Cells[x, y].FrontAnimationFrame = Bytes[offset++];
                    Cells[x, y].FrontAnimationTick = Bytes[offset++];
                    Cells[x, y].FrontFile = (short)(Bytes[offset++] + 120);
                    Cells[x, y].Light = Bytes[offset++];
                    Cells[x, y].BackFile = (short)(Bytes[offset++] + 100);
                    Cells[x, y].MiddleFile = (short)(Bytes[offset++] + 110);
                    if ((Cells[x, y].BackImage & 0x8000) != 0)
                        Cells[x, y].BackImage = (Cells[x, y].BackImage & 0x7FFF) | 0x20000000;

                    Cells[x, y].Flag = (Cells[x, y].BackImage & 0x20000000) != 0 || (Cells[x, y].FrontImage & 0x8000) != 0;

                }
        }
        private void LoadMapType3(byte[] Bytes)
        {
            int offset = 0;
            Width = BitConverter.ToInt16(Bytes, offset);
            offset += 2;
            Height = BitConverter.ToInt16(Bytes, offset);
            Cells = new Cell[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Cells[x, y] = new Cell();
            offset = 52;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {//36
                    Cells[x, y] = new Cell();
                    Cells[x, y].BackImage = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    Cells[x, y].MiddleImage = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    Cells[x, y].FrontImage = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    Cells[x, y].DoorIndex = (byte)(Bytes[offset++] & 0x7F);
                    Cells[x, y].DoorOffset = Bytes[offset++];
                    Cells[x, y].FrontAnimationFrame = Bytes[offset++];
                    Cells[x, y].FrontAnimationTick = Bytes[offset++];
                    Cells[x, y].FrontFile = (short)(Bytes[offset++] + 120);
                    Cells[x, y].Light = Bytes[offset++];
                    Cells[x, y].BackFile = (short)(Bytes[offset++] + 100);
                    Cells[x, y].MiddleFile = (short)(Bytes[offset++] + 110);
                    Cells[x, y].TileAnimationImage = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 7;//2bytes from tileanimframe, 2 bytes always blank?, 2bytes potentialy 'backtiles index', 1byte fileindex for the backtiles?
                    Cells[x, y].TileAnimationFrames = Bytes[offset++];
                    Cells[x, y].TileAnimationOffset = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 14; //tons of light, blending, .. related options i hope
                    if ((Cells[x, y].BackImage & 0x8000) != 0)
                        Cells[x, y].BackImage = (Cells[x, y].BackImage & 0x7FFF) | 0x20000000;

                    Cells[x, y].Flag = (Cells[x, y].BackImage & 0x20000000) != 0 || (Cells[x, y].FrontImage & 0x8000) != 0;

                }
        }
        private void LoadMapType4(byte[] Bytes)
        {
            int offset = 31;
            int w = BitConverter.ToInt16(Bytes, offset);
            offset += 2;
            int xor = BitConverter.ToInt16(Bytes, offset);
            offset += 2;
            int h = BitConverter.ToInt16(Bytes, offset);
            Width = w ^ xor;
            Height = h ^ xor;
            Cells = new Cell[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Cells[x, y] = new Cell();
            offset = 64;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {//12
                    Cells[x, y] = new Cell();
                    Cells[x, y].BackFile = 0;
                    Cells[x, y].MiddleFile = 1;
                    Cells[x, y].BackImage = (short)(BitConverter.ToInt16(Bytes, offset) ^ xor);
                    offset += 2;
                    Cells[x, y].MiddleImage = (short)(BitConverter.ToInt16(Bytes, offset) ^ xor);
                    offset += 2;
                    Cells[x, y].FrontImage = (short)(BitConverter.ToInt16(Bytes, offset) ^ xor);
                    offset += 2;
                    Cells[x, y].DoorIndex = (byte)(Bytes[offset++] & 0x7F);
                    Cells[x, y].DoorOffset = Bytes[offset++];
                    Cells[x, y].FrontAnimationFrame = Bytes[offset++];
                    Cells[x, y].FrontAnimationTick = Bytes[offset++];
                    Cells[x, y].FrontFile = (short)(Bytes[offset++] + 2);
                    Cells[x, y].Light = Bytes[offset++];
                    if ((Cells[x, y].BackImage & 0x8000) != 0)
                        Cells[x, y].BackImage = (Cells[x, y].BackImage & 0x7FFF) | 0x20000000;

                    Cells[x, y].Flag = (Cells[x, y].BackImage & 0x20000000) != 0 || (Cells[x, y].FrontImage & 0x8000) != 0;

                }
        }
        private void LoadMapType5(byte[] Bytes)
        {
            //zircon mir3
            byte flag = 0;
            int offset = 20;
            short Attribute = (short)(BitConverter.ToInt16(Bytes, offset));
            Width = (int)(BitConverter.ToInt16(Bytes, offset += 2));
            Height = (int)(BitConverter.ToInt16(Bytes, offset += 2));
            Cells = new Cell[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Cells[x, y] = new Cell();
            //ignoring eventfile and fogcolor for now (seems unused in maps i checked)
            offset = 28;
            //initiate all cells

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Cells[x, y] = new Cell();
            //read all back tiles
            for (int x = 0; x < (Width / 2); x++)
                for (int y = 0; y < (Height / 2); y++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Cells[(x * 2) + (i % 2), (y * 2) + (i / 2)].BackFile = (short)(Bytes[offset] != 255 ? Bytes[offset] + 200 : -1);
                        Cells[(x * 2) + (i % 2), (y * 2) + (i / 2)].BackImage = (int)(BitConverter.ToUInt16(Bytes, offset + 1) + 1);
                    }
                    offset += 3;
                }
            //read rest of data
            offset = 28 + (3 * ((Width / 2) + (Width % 2)) * (Height / 2));
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {

                    flag = Bytes[offset++];
                    Cells[x, y].MiddleAnimationFrame = Bytes[offset++];

                    Cells[x, y].FrontAnimationFrame = Bytes[offset] == 255 ? (byte)0 : Bytes[offset];
                    Cells[x, y].FrontAnimationFrame &= 0x8F;
                    offset++;
                    Cells[x, y].MiddleAnimationTick = 0;
                    Cells[x, y].FrontAnimationTick = 0;
                    Cells[x, y].FrontFile = (short)(Bytes[offset] != 255 ? Bytes[offset] + 200 : -1);
                    offset++;
                    Cells[x, y].MiddleFile = (short)(Bytes[offset] != 255 ? Bytes[offset] + 200 : -1);
                    offset++;
                    Cells[x, y].MiddleImage = (ushort)(BitConverter.ToUInt16(Bytes, offset) + 1);
                    offset += 2;
                    Cells[x, y].FrontImage = (ushort)(BitConverter.ToUInt16(Bytes, offset) + 1);
                    if ((Cells[x, y].FrontImage == 1) && (Cells[x, y].FrontFile == 200))
                        Cells[x, y].FrontFile = -1;
                    offset += 2;
                    offset += 3;//mir3 maps dont have doors so dont bother reading the info
                    Cells[x, y].Light = (byte)(Bytes[offset] & 0x0F);
                    offset += 2;
                    if ((flag & 0x01) != 1) Cells[x, y].BackImage |= 0x20000000;
                    if ((flag & 0x02) != 2) Cells[x, y].FrontImage = (ushort)((UInt16)Cells[x, y].FrontImage | 0x8000);

                    Cells[x, y].Flag = (Cells[x, y].BackImage & 0x20000000) != 0 || (Cells[x, y].FrontImage & 0x8000) != 0;


                    if (!(Cells[x, y].Light >= 100 && Cells[x, y].Light <= 119))
                        Cells[x, y].Light *= 2;//expand general mir3 lighting as default range is small. Might break new colour lights.

                }
        }
        private void LoadMapType6(byte[] Bytes)
        {
            byte flag = 0;
            int offset = 16;
            Width = BitConverter.ToInt16(Bytes, offset);
            offset += 2;
            Height = BitConverter.ToInt16(Bytes, offset);
            Cells = new Cell[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Cells[x, y] = new Cell();
            offset = 40;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    Cells[x, y] = new Cell();
                    flag = Bytes[offset++];
                    Cells[x, y].BackFile = (short)(Bytes[offset] != 255 ? Bytes[offset] + 300 : -1);
                    offset++;
                    Cells[x, y].MiddleFile = (short)(Bytes[offset] != 255 ? Bytes[offset] + 300 : -1);
                    offset++;
                    Cells[x, y].FrontFile = (short)(Bytes[offset] != 255 ? Bytes[offset] + 300 : -1);
                    offset++;
                    Cells[x, y].BackImage = (short)(BitConverter.ToInt16(Bytes, offset) + 1);
                    offset += 2;
                    Cells[x, y].MiddleImage = (short)(BitConverter.ToInt16(Bytes, offset) + 1);
                    offset += 2;
                    Cells[x, y].FrontImage = (short)(BitConverter.ToInt16(Bytes, offset) + 1);
                    offset += 2;
                    if ((Cells[x, y].FrontImage == 1) && (Cells[x, y].FrontFile == 200))
                        Cells[x, y].FrontFile = -1;
                    Cells[x, y].MiddleAnimationFrame = Bytes[offset++];
                    Cells[x, y].FrontAnimationFrame = Bytes[offset] == 255 ? (byte)0 : Bytes[offset];
                    if (Cells[x, y].FrontAnimationFrame > 0x0F)//assuming shanda used same value not sure
                        Cells[x, y].FrontAnimationFrame = (byte)(/*0x80 ^*/ (Cells[x, y].FrontAnimationFrame & 0x0F));
                    offset++;
                    Cells[x, y].MiddleAnimationTick = 1;
                    Cells[x, y].FrontAnimationTick = 1;
                    Cells[x, y].Light = (byte)(Bytes[offset] & 0x0F);
                    Cells[x, y].Light *= 4;//far wants all light on mir3 maps to be maxed :p
                    offset += 8;
                    if ((flag & 0x01) != 1) Cells[x, y].BackImage |= 0x20000000;
                    if ((flag & 0x02) != 2) Cells[x, y].FrontImage = (short)((UInt16)Cells[x, y].FrontImage | 0x8000);

                    Cells[x, y].Flag = (Cells[x, y].BackImage & 0x20000000) != 0 || (Cells[x, y].FrontImage & 0x8000) != 0;

                }
        }
        private void LoadMapType7(byte[] Bytes)
        {
            int offset = 21;
            Width = BitConverter.ToInt16(Bytes, offset);
            offset += 4;
            Height = BitConverter.ToInt16(Bytes, offset);
            Cells = new Cell[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Cells[x, y] = new Cell();
            offset = 54;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {//total 15
                    Cells[x, y] = new Cell
                    {
                        BackFile = 0,
                        BackImage = (int)BitConverter.ToInt32(Bytes, offset),
                        MiddleFile = 1,
                        MiddleImage = (short)BitConverter.ToInt16(Bytes, offset += 4),
                        FrontImage = (short)BitConverter.ToInt16(Bytes, offset += 2),
                        DoorIndex = (byte)(Bytes[offset += 2] & 0x7F),
                        DoorOffset = Bytes[++offset],
                        FrontAnimationFrame = Bytes[++offset],
                        FrontAnimationTick = Bytes[++offset],
                        FrontFile = (short)(Bytes[++offset] + 2),
                        Light = Bytes[++offset],
                        Unknown = Bytes[++offset],
                    };
                    if ((Cells[x, y].BackImage & 0x8000) != 0)
                        Cells[x, y].BackImage = (Cells[x, y].BackImage & 0x7FFF) | 0x20000000;
                    offset++;

                    Cells[x, y].Flag = (Cells[x, y].BackImage & 0x20000000) != 0 || (Cells[x, y].FrontImage & 0x8000) != 0;

                }

        }
        private void LoadMapType8(byte[] Bytes)
        {

            //NEW MIR3 SHANDA map format (small changes)

            byte walk = 0;
            int offset = 16;
            Width = BitConverter.ToInt16(Bytes, offset);
            offset += 2;
            Height = BitConverter.ToInt16(Bytes, offset);
            offset += 22;
            Cells = new Cell[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Cells[x, y] = new Cell();

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    Cells[x, y] = new Cell();
                    walk = Bytes[offset++];
                    Cells[x, y].BackFile = (short)(Bytes[offset] != 0xFF ? Bytes[offset] + 400 : -1);
                    offset++;
                    Cells[x, y].MiddleFile = (short)(Bytes[offset] != 0xFF ? Bytes[offset] + 400 : -1);
                    offset++;
                    Cells[x, y].FrontFile = (short)(Bytes[offset] != 0xFF ? Bytes[offset] + 400 : -1);
                    offset++;
                    Cells[x, y].BackImage = (short)(BitConverter.ToInt16(Bytes, offset) + 1);
                    offset += 2;
                    Cells[x, y].MiddleImage = (short)(BitConverter.ToInt16(Bytes, offset) + 1);
                    offset += 2;
                    Cells[x, y].FrontImage = (short)(BitConverter.ToInt16(Bytes, offset) + 1);
                    offset += 2;

                    if ((Cells[x, y].FrontImage == 1) && (Cells[x, y].FrontFile == 200))
                        Cells[x, y].FrontFile = -1;

                    Cells[x, y].MiddleAnimationFrame = Bytes[offset++];
                    Cells[x, y].FrontAnimationFrame = Bytes[offset] == 0xFF ? (byte)0 : Bytes[offset];
                    if (Cells[x, y].FrontAnimationFrame > 0x0F)
                        Cells[x, y].FrontAnimationFrame = (byte)(Cells[x, y].FrontAnimationFrame & 0x0F);
                    offset++;
                    Cells[x, y].MiddleAnimationTick = 1;
                    Cells[x, y].FrontAnimationTick = 1;
                    Cells[x, y].Light = (byte)(Bytes[offset] & 0x0F);
                    Cells[x, y].Light *= 4;
                    offset += 8;
                    if ((walk & 0x01) != 1) Cells[x, y].BackImage |= 0x20000000;
                    if ((walk & 0x02) != 2) Cells[x, y].FrontImage = (short)((UInt16)Cells[x, y].FrontImage | 0x8000);

                    Cells[x, y].Flag = (Cells[x, y].BackImage & 0x20000000) != 0 || (Cells[x, y].FrontImage & 0x8000) != 0;

                }

        }
        private void LoadMapType100(byte[] Bytes)
        {
            int offset = 4;
            if ((Bytes[0] != 1) || (Bytes[1] != 0)) return;//only support version 1 atm
            Width = BitConverter.ToInt16(Bytes, offset);
            offset += 2;
            Height = BitConverter.ToInt16(Bytes, offset);
            offset = 8;
            Cells = new Cell[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Cells[x, y] = new Cell();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    Cells[x, y] = new Cell();
                    Cells[x, y].BackFile = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    Cells[x, y].BackImage = (int)BitConverter.ToInt32(Bytes, offset);
                    offset += 4;
                    Cells[x, y].MiddleFile = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    Cells[x, y].MiddleImage = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    Cells[x, y].FrontFile = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    Cells[x, y].FrontImage = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    Cells[x, y].DoorIndex = (byte)(Bytes[offset++] & 0x7F);
                    Cells[x, y].DoorOffset = Bytes[offset++];

                    Cells[x, y].FrontAnimationFrame = Bytes[offset++];
                    Cells[x, y].FrontAnimationTick = Bytes[offset++];
                    Cells[x, y].MiddleAnimationFrame = Bytes[offset++];
                    Cells[x, y].MiddleAnimationTick = Bytes[offset++];

                    Cells[x, y].TileAnimationImage = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;

                    Cells[x, y].TileAnimationOffset = (short)BitConverter.ToInt16(Bytes, offset);
                    offset += 2;

                    Cells[x, y].TileAnimationFrames = Bytes[offset++];

                    Cells[x, y].Light = Bytes[offset++];

                    Cells[x, y].Flag = (Cells[x, y].BackImage & 0x20000000) != 0 || (Cells[x, y].FrontImage & 0x8000) != 0;

                }

        }
        private void LoadMapType150(byte[] Bytes)
        {
            var x = 0;
            var y = 0;

            int offset = 0;

            offset += 70;

            var startOffset = BitConverter.ToInt32(Bytes, offset);
            offset += 4;

            //width 4 bytes (int)
            Width = BitConverter.ToInt32(Bytes, offset);
            offset += 4;
            //height 4 bytes (int)
            Height = BitConverter.ToInt32(Bytes, offset);
            offset += 4;

            if (Width > 1500 || Height > 1500)
            {
                return;
            }

            Cells = new Cell[Width, Height];
            for (int x1 = 0; x1 < Width; x1++)
                for (int y1 = 0; y1 < Height; y1++)
                    Cells[x1, y1] = new Cell();

            //precalc
            int pSize = ((Width - 1) * (Height - 1)) * 13;

            if (Bytes.Length - offset < pSize)
                return;

            var MapData = new byte[Bytes.Length - offset];

            Array.ConstrainedCopy(Bytes, offset, MapData, 0, MapData.Length);

            offset = 0;

            #region new format
            for (int x1 = 0; x1 < Width - 1; x1++)
            {
                for (int y1 = 0; y1 < Height - 1; y1++)
                {
                    x = x1;
                    y = y1;
                    Cells[x, y] = new Cell();

                    if ((MapData[offset++] & 1) == 1)
                    {
                        Cells[x, y].Flag = true;
                    }
                    else
                    {
                        Cells[x, y].Flag = false;
                    }

                    Cells[x, y].MiddleImage = BitConverter.ToInt16(MapData, offset);
                    offset++;
                    offset++;

                    Cells[x, y].BackImage = BitConverter.ToInt16(MapData, offset);
                    offset++;
                    offset++;

                    Cells[x, y].FrontImage = BitConverter.ToInt16(MapData, offset);
                    offset++;
                    offset++;

                    Cells[x, y].FrontFile = MapData[offset++];

                    Cells[x, y].BackFile = MapData[offset++];
                    Cells[x, y].MiddleFile = MapData[offset++];

                    Cells[x, y].MiddleAnimationFrame = MapData[offset++];
                    Cells[x, y].MiddleAnimationTick = MapData[offset++];

                    Cells[x, y].Light = MapData[offset++];

                    #region Fixes & Tricks (Gradients & Animations fixes)

                    if (Cells[x, y].BackFile < Globals.WooolBackFileStartIndex && Cells[x, y].BackImage > 0)
                        Cells[x, y].BackFile = Globals.WooolBackFileStartIndex;

                    if (Cells[x, y].MiddleFile < Globals.WooolMiddleFileStartIndex && Cells[x, y].MiddleImage > 0)
                        Cells[x, y].MiddleFile = Globals.WooolMiddleFileStartIndex;

                    //lib fixer :D
                    switch (Cells[x, y].FrontFile)
                    {
                        case 5:
                            Cells[x, y].FrontFile = 610 + 1;
                            break;
                        case 6:
                            Cells[x, y].FrontFile = 610 + 2;
                            break;
                        case 7:
                            Cells[x, y].FrontFile = 610 + 3;
                            break;
                        case 8:
                            Cells[x, y].FrontFile = 610 + 4;
                            break;
                        case 33:
                            Cells[x, y].FrontFile = 610 + 5;
                            break;
                        case 34:
                            Cells[x, y].FrontFile = 610 + 6;
                            break;
                        case 35:
                            Cells[x, y].FrontFile = 610 + 7;
                            break;
                        case 36:
                            Cells[x, y].FrontFile = 610 + 8;
                            break;
                        case 37:
                            Cells[x, y].FrontFile = 610 + 9;
                            break;
                        case 38:
                            Cells[x, y].FrontFile = 610 + 10;
                            break;
                        case 39:
                            Cells[x, y].FrontFile = 610 + 11;
                            break;
                        case 40:
                            Cells[x, y].FrontFile = 610 + 12;
                            break;
                        case 41:
                            Cells[x, y].FrontFile = 610 + 13;
                            break;
                        case 42:
                            Cells[x, y].FrontFile = 610 + 14;
                            break;
                        case 43:
                            Cells[x, y].FrontFile = 610 + 15;
                            break;
                        case 44:
                            Cells[x, y].FrontFile = 610 + 16;
                            break;
                        default:
                            Cells[x, y].FrontFile = -1;
                            break;
                    }

                    //Blend + Animation + calc fixes

                    #region Back Layer
                    if (Cells[x, y].BackFile <= 0)
                        Cells[x, y].BackFile = -1;

                    if (Cells[x, y].BackFile > 0)
                    {
                        //put back anims to other layer to not saturate engine

                        //BackFile = 600; <- remember file idx

                        //under dev

                        //fountain 1
                        if (Cells[x, y].BackImage >= 10007 && Cells[x, y].BackImage <= 10019)
                        {

                            //put to animated layer - 18 frames
                            Cells[x, y].MiddleFile = Globals.WooolBackFileStartIndex;
                            Cells[x, y].MiddleImage = Cells[x, y].BackImage;
                            Cells[x, y].MiddleAnimationFrame = 18;
                            Cells[x, y].MiddleAnimationTick = 1;
                            Cells[x, y].MiddleBlend = false;

                        }

                        //water 1
                        //if (Cells[x, y].BackImage == 23857)
                        //{

                        //    //put to animated layer - 18 frames
                        //    Cells[x, y].MiddleFile = 600;
                        //    Cells[x, y].MiddleImage = Cells[x, y].BackImage;
                        //    Cells[x, y].MiddleAnimationFrame = 21;
                        //    Cells[x, y].MiddleAnimationTick = 1;
                        //    Cells[x, y].MiddleBlend = false;

                        //}

                        //strange floor 1
                        if (Cells[x, y].BackImage == 24064)
                        {

                            //put to animated layer - 18 frames
                            Cells[x, y].MiddleFile = Globals.WooolBackFileStartIndex;
                            Cells[x, y].MiddleImage = Cells[x, y].BackImage;
                            Cells[x, y].MiddleAnimationFrame = 14;
                            Cells[x, y].MiddleAnimationTick = 1;
                            Cells[x, y].MiddleBlend = false;

                        }

                        //Blossom type 1
                        if (Cells[x, y].BackImage >= 24185 && Cells[x, y].BackImage <= 24190)
                        {

                            //put to animated layer - 18 frames
                            Cells[x, y].MiddleFile = Globals.WooolBackFileStartIndex;
                            Cells[x, y].MiddleImage = Cells[x, y].BackImage;
                            Cells[x, y].MiddleAnimationFrame = 19;
                            Cells[x, y].MiddleAnimationTick = 1;
                            Cells[x, y].MiddleBlend = false;

                        }

                    }

                    #endregion

                    #region Middle Layer

                    #region NON ANIMATED FIXES - BLENDING

                    if (Cells[x, y].MiddleFile == Globals.WooolMiddleFileStartIndex)
                    {
                        //sand blend fixes 1
                        if (Cells[x, y].MiddleImage >= 9002 && Cells[x, y].MiddleImage <= 9138)
                        {
                            Cells[x, y].MiddleSpecialBlend = true;
                        }

                        //sand blend fixes 2
                        if (Cells[x, y].MiddleImage >= 9516 && Cells[x, y].MiddleImage <= 11614)
                        {
                            Cells[x, y].MiddleSpecialBlend = true;
                        }
                    }
                    #endregion

                    //rocks and buildings
                    if (Cells[x, y].MiddleImage >= 20318 && Cells[x, y].MiddleImage <= 20351)
                    {
                        Cells[x, y].MiddleAnimationFrame = 19;
                        Cells[x, y].MiddleBlend = false;
                        Cells[x, y].Light = 1;
                    }
                    //portal floor water
                    if (Cells[x, y].MiddleImage >= 20352 && Cells[x, y].MiddleImage <= 20369)
                    {
                        Cells[x, y].MiddleAnimationFrame = 8;
                        Cells[x, y].MiddleBlend = false;
                    }
                    //floor rock bright
                    if (Cells[x, y].MiddleImage == 24125)
                    {
                        Cells[x, y].MiddleAnimationFrame = 14;
                        Cells[x, y].MiddleBlend = false;
                        Cells[x, y].Light = 1;
                    }
                    //floor vortex-portal big
                    if (Cells[x, y].MiddleImage >= 24126 && Cells[x, y].MiddleImage <= 24131)
                    {
                        Cells[x, y].MiddleAnimationFrame = 19;
                        Cells[x, y].MiddleBlend = false;
                        Cells[x, y].Light = 1;
                    }
                    //floor vortex-portal small
                    if (Cells[x, y].MiddleImage >= 24385 && Cells[x, y].MiddleImage <= 24386)
                    {
                        Cells[x, y].MiddleAnimationFrame = 19;
                        Cells[x, y].MiddleBlend = false;
                        Cells[x, y].Light = 1;
                    }

                    if (Cells[x, y].MiddleAnimationFrame > 0)
                        Cells[x, y].MiddleAnimationFrame += 1;

                    if (Cells[x, y].MiddleAnimationTick == 0 && Cells[x, y].MiddleAnimationFrame > 0)
                        Cells[x, y].MiddleAnimationTick = 1;

                    if ((Cells[x, y].MiddleAnimationFrame & 0x80) > 0)
                        Cells[x, y].MiddleBlend = true;

                    if (Cells[x, y].MiddleFile <= 0)
                        Cells[x, y].MiddleFile = -1;

                    #endregion

                    #region Front Layer

                    #region Objects7
                    if (Cells[x, y].FrontFile == 617 && (Cells[x, y].FrontImage >= 605 && Cells[x, y].FrontImage <= 612))
                    {
                        Cells[x, y].FrontAnimationFrame = 14;
                        Cells[x, y].FrontBlend = true;
                    }

                    if (Cells[x, y].FrontFile == 617 && (Cells[x, y].FrontImage >= 784 && Cells[x, y].FrontImage <= 804))
                    {
                        Cells[x, y].FrontAnimationFrame = 19;
                        Cells[x, y].FrontBlend = true;
                    }

                    if (Cells[x, y].FrontFile == 617 && (Cells[x, y].FrontImage >= 805 && Cells[x, y].FrontImage <= 814))
                    {
                        Cells[x, y].FrontAnimationFrame = 29;
                        Cells[x, y].FrontBlend = true;
                    }

                    if (Cells[x, y].FrontFile == 617 && (Cells[x, y].FrontImage >= 815 && Cells[x, y].FrontImage <= 829))
                    {
                        Cells[x, y].FrontAnimationFrame = 19;
                        Cells[x, y].FrontBlend = true;
                    }

                    if (Cells[x, y].FrontFile == 617 && (Cells[x, y].FrontImage >= 830 && Cells[x, y].FrontImage <= 835))
                    {
                        Cells[x, y].FrontAnimationFrame = 35;
                        Cells[x, y].FrontBlend = true;
                    }

                    #endregion

                    #region objects8
                    //fire
                    if (Cells[x, y].FrontFile == 618 && (Cells[x, y].FrontImage >= 2860 && Cells[x, y].FrontImage <= 2871))
                    {
                        Cells[x, y].FrontAnimationFrame = 6;
                        Cells[x, y].FrontBlend = true;

                        if (Cells.GetUpperBound(1) > y - 3)
                            Cells[x, y - 3].Light = 1;
                    }
                    //portal fx
                    if (Cells[x, y].FrontFile == 618 && (Cells[x, y].FrontImage >= 2872 && Cells[x, y].FrontImage <= 2873))
                    {
                        Cells[x, y].FrontAnimationFrame = 14;
                        Cells[x, y].FrontBlend = true;

                        if (Cells.GetUpperBound(1) > y - 3)
                            Cells[x, y - 3].Light = 1;
                    }
                    //unk fx - bright?
                    if (Cells[x, y].FrontFile == 618 && (Cells[x, y].FrontImage >= 2874 && Cells[x, y].FrontImage <= 2880))
                    {
                        Cells[x, y].FrontAnimationFrame = 8;
                        Cells[x, y].FrontBlend = true;
                        Cells[x, y].Light = 1;
                    }
                    //fire small
                    if (Cells[x, y].FrontFile == 618 && (Cells[x, y].FrontImage >= 2882 && Cells[x, y].FrontImage <= 2885))
                    {
                        Cells[x, y].FrontAnimationFrame = 6;
                        Cells[x, y].FrontBlend = true;

                        if (Cells.GetUpperBound(1) > y - 3)
                            Cells[x, y - 3].Light = 1;

                    }
                    //unk fx - shadow?
                    if (Cells[x, y].FrontFile == 618 && (Cells[x, y].FrontImage >= 2886 && Cells[x, y].FrontImage <= 2890))
                    {
                        Cells[x, y].FrontAnimationFrame = 19;
                        Cells[x, y].FrontBlend = true;
                    }
                    //fire big
                    if (Cells[x, y].FrontFile == 618 && (Cells[x, y].FrontImage >= 2891 && Cells[x, y].FrontImage <= 2895))
                    {
                        Cells[x, y].FrontAnimationFrame = 6;
                        Cells[x, y].FrontBlend = true;

                        if (Cells.GetUpperBound(1) > y - 4)
                            Cells[x, y - 4].Light = 1;

                    }
                    //stone
                    if (Cells[x, y].FrontFile == 618 && (Cells[x, y].FrontImage >= 3011 && Cells[x, y].FrontImage <= 3016))
                    {
                        Cells[x, y].FrontAnimationFrame = 19;
                        Cells[x, y].FrontBlend = false;
                    }
                    //stone fx?
                    if (Cells[x, y].FrontFile == 618 && (Cells[x, y].FrontImage >= 3017 && Cells[x, y].FrontImage <= 3020))
                    {
                        Cells[x, y].FrontAnimationFrame = 19;
                        Cells[x, y].FrontBlend = true;

                        if (Cells.GetUpperBound(1) > y - 5)
                            Cells[x, y - 5].Light = 1;
                    }
                    #endregion

                    #region objects14
                    if (Cells[x, y].FrontFile == 624 && (Cells[x, y].FrontImage >= 521 && Cells[x, y].FrontImage <= 526))
                    {
                        Cells[x, y].FrontAnimationFrame = 29;
                        Cells[x, y].FrontBlend = true;
                        Cells[x, y].Light = 1;
                    }
                    #endregion

                    if ((Cells[x, y].FrontAnimationFrame & 0x80) > 0)
                        Cells[x, y].FrontBlend = true;

                    if (Cells[x, y].FrontAnimationFrame > 0)
                        Cells[x, y].FrontAnimationFrame += 1;

                    if (Cells[x, y].FrontAnimationTick == 0 && Cells[x, y].FrontAnimationFrame > 0)
                        Cells[x, y].FrontAnimationTick = 1;

                    #endregion

                    #endregion

                    #region Lights
                    //if (Cells[x, y].FrontBlend || Cells[x, y].MiddleBlend)
                    //    Cells[x, y].Light = 1;
                    #endregion

                    #region Images PreCalcs - Objects Draw

                    #region Base Middle layer static location calcs
                    if (Cells[x, y].MiddleFile == Globals.WooolBackFileStartIndex)
                        Cells[x, y].CustomCellHeight = CellHeight * 2;
                    else
                        Cells[x, y].CustomCellHeight = CellHeight;
                    #endregion

                    if (Cells[x, y].MiddleFile != -1)
                        Cells[x, y].MiddleImage = Cells[x, y].MiddleImage & 0x1FFFF;

                    if (Cells[x, y].FrontFile != -1)
                        Cells[x, y].FrontImage = Cells[x, y].FrontImage & 0x1FFFF;

                    #endregion

                }
            }
            #endregion

        }

        #endregion

        private void UpdateWeather()
        {
            GameScene.Game.MapControl.ParticleEffects.Clear();

            if (Config.DrawWeather)
            {
                var point = new Point((Size.Width) / 2, (Size.Height) / 2);

                if (GameScene.Game.MapControl.MapInfo.Weather.HasFlag(Weather.Rain))
                {
                    var emitter = (ParticleEmitter)Activator.CreateInstance(typeof(Rain), point);
                    GameScene.Game.MapControl.ParticleEffects.Add(emitter);
                }

                if (GameScene.Game.MapControl.MapInfo.Weather.HasFlag(Weather.Snow))
                {
                    var emitter = (ParticleEmitter)Activator.CreateInstance(typeof(Snow), point);
                    GameScene.Game.MapControl.ParticleEffects.Add(emitter);
                }

                if (GameScene.Game.MapControl.MapInfo.Weather.HasFlag(Weather.Fog))
                {
                    var emitter = (ParticleEmitter)Activator.CreateInstance(typeof(Fog), point);
                    GameScene.Game.MapControl.ParticleEffects.Add(emitter);
                }

                if (GameScene.Game.MapControl.MapInfo.Weather.HasFlag(Weather.Lightning))
                {
                    var emitter = (ParticleEmitter)Activator.CreateInstance(typeof(Lightning), point);
                    GameScene.Game.MapControl.ParticleEffects.Add(emitter);
                }
            }
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            MouseLocation = e.Location;
        }
        public override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (GameScene.Game.Observer) return;

            MapButtons |= e.Button;

            if (e.Button == MouseButtons.Right)
            {
                if (Config.RightClickDeTarget && MapObject.TargetObject?.Race == ObjectType.Monster)
                    MapObject.TargetObject = null;
            }
            
            if (e.Button != MouseButtons.Left) return;

            DXItemCell cell = DXItemCell.SelectedCell;
            if (cell != null)
            {
                MapButtons &= ~e.Button;

                if (cell.GridType == GridType.Belt)
                {
                    cell.QuickInfo = null;
                    cell.QuickItem = null;
                    DXItemCell.SelectedCell = null;

                    ClientBeltLink link = GameScene.Game.BeltBox.Links[cell.Slot];
                    CEnvir.Enqueue(new C.BeltLinkChanged { Slot = link.Slot, LinkIndex = link.LinkInfoIndex, LinkItemIndex = link.LinkItemIndex }); //Update serve
                    return;
                }

                if (cell.GridType == GridType.AutoPotion)
                {
                    cell.QuickInfo = null;
                    cell.QuickItem = null;
                    DXItemCell.SelectedCell = null;

                    GameScene.Game.AutoPotionBox.Rows[cell.Slot].SendUpdate();
                    return;
                }

                if ((cell.Item.Flags & UserItemFlags.Locked) == UserItemFlags.Locked || (cell.GridType != GridType.Inventory && cell.GridType != GridType.CompanionInventory))
                {
                    DXItemCell.SelectedCell = null;
                    return;
                }
                
                DXItemAmountWindow window = new DXItemAmountWindow("Drop Item", cell.Item);

                window.ConfirmButton.MouseClick += (o, a) =>
                {
                    if (window.Amount <= 0) return;

                    CEnvir.Enqueue(new C.ItemDrop
                    {
                        Link = new CellLinkInfo { GridType = cell.GridType, Slot = cell.Slot, Count = window.Amount }
                    });

                    cell.Locked = true;
                };

                DXItemCell.SelectedCell = null;
                return;
            }

            if (GameScene.Game.CurrencyPickedUp != null)
            {
                MapButtons &= ~e.Button;

                int index = GameScene.Game.CurrencyPickedUp.Info.Index;

                DXItemAmountWindow window = new DXItemAmountWindow("Drop Item", new ClientUserItem(GameScene.Game.CurrencyPickedUp.Info.DropItem, GameScene.Game.CurrencyPickedUp.Amount));

                window.ConfirmButton.MouseClick += (o, a) =>
                {
                    if (window.Amount <= 0) return;

                    CEnvir.Enqueue(new C.CurrencyDrop
                    {
                        CurrencyIndex = index,
                        Amount = window.Amount
                    });
                };

                GameScene.Game.CurrencyPickedUp = null;
                return;
            }
            
            if (CanAttack(MapObject.MouseObject))
            {
                MapObject.TargetObject = MapObject.MouseObject;

                if (MapObject.MouseObject.Race == ObjectType.Monster && ((MonsterObject) MapObject.MouseObject).MonsterInfo.AI >= 0) //Check if AI is guard
                {
                    MapObject.MagicObject = MapObject.TargetObject;
                    GameScene.Game.FocusObject = MapObject.MouseObject;
                }
                return;
            }

            MapObject.TargetObject = null;
            GameScene.Game.FocusObject = null;
            //GameScene.Game.OldTargetObjectID = 0;
        }
        public override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (GameScene.Game.Observer) return;

                    GameScene.Game.AutoRun = false;
                    if (MapObject.MouseObject == null) return;
                    NPCObject npc = MapObject.MouseObject as NPCObject;
                    if (npc != null)
                    {
                        if (CEnvir.Now <= GameScene.Game.NPCTime) return;

                        GameScene.Game.NPCTime = CEnvir.Now.AddSeconds(1);

                        CEnvir.Enqueue(new C.NPCCall { ObjectID = npc.ObjectID });
                    }
                    break;
                case MouseButtons.Right:
                    GameScene.Game.AutoRun = false;

                    if (User.CurrentAction == MirAction.Standing)
                        GameScene.Game.CanRun = false;

                    if (!CEnvir.Ctrl) return;

                    PlayerObject player = MapObject.MouseObject as PlayerObject;

                    if (player == null || player == MapObject.User) return;
                    if (CEnvir.Now <= GameScene.Game.InspectTime && player.ObjectID == GameScene.Game.InspectID) return;

                    GameScene.Game.InspectTime = CEnvir.Now.AddMilliseconds(2500);
                    GameScene.Game.InspectID = player.ObjectID;
                    CEnvir.Enqueue(new C.Inspect { Index = player.CharacterIndex });
                    break;
            }
        }

        public void CheckCursor()
        {
            MapObject deadObject = null, itemObject = null;
            
            for (int d = 0; d < 4; d++)
            {
                for (int y = MapLocation.Y - d; y <= MapLocation.Y + d; y++)
                {
                    if (y >= Height) continue;
                    if (y < 0) break;
                    for (int x = MapLocation.X - d; x <= MapLocation.X + d; x++)
                    {
                        if (x >= Width) continue;
                        if (x < 0) break;

                        List<MapObject> list = Cells[x, y].Objects;
                        if (list == null) continue;

                        MapObject cellSelect = null;
                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                            MapObject ob = list[i];

                            if (ob == MapObject.User || ob.Race == ObjectType.Spell || ((x != MapLocation.X || y != MapLocation.Y) && !ob.MouseOver(MouseLocation))) continue;

                            if (ob.Dead || (ob.Race == ObjectType.Monster && ((MonsterObject)ob).CompanionObject != null))
                            {
                                if (deadObject == null)
                                    deadObject = ob;
                                continue;
                            }
                            if (ob.Race == ObjectType.Item)
                            {
                                if (itemObject == null)
                                    itemObject = ob;
                                continue;
                            }
                            if (x == MapLocation.X && y == MapLocation.Y && !ob.MouseOver(MouseLocation))
                            {
                                if (cellSelect == null)
                                    cellSelect = ob;
                            }
                            else
                            {
                                MapObject.MouseObject = ob;
                                return;
                            }
                        }

                        if (cellSelect != null)
                        {
                            MapObject.MouseObject = cellSelect;
                            return;
                        }
                    }
                }
            }

            MapObject mouseOb = deadObject ?? itemObject;

            if (mouseOb == null)
            {
                if ((User.CurrentLocation.X == MapLocation.X && User.CurrentLocation.Y == MapLocation.Y) || User.MouseOver(MouseLocation))
                    mouseOb = User;
            }


            MapObject.MouseObject = mouseOb;
        }

        public void ProcessInput()
        {
            if (GameScene.Game.Observer) return;

            if (User.Dead || (User.Poison & PoisonType.Paralysis) == PoisonType.Paralysis || User.Buffs.Any(x => x.Type == BuffType.DragonRepulse || x.Type == BuffType.FrostBite)) return; //Para or Frozen??

            if (User.MagicAction != null)
            {
                if (CEnvir.Now < MapObject.User.NextActionTime || MapObject.User.ActionQueue.Count != 0) return;

                //  if (QueuedMagic.Action == MirAction.Magic && (Spell)QueuedMagic.Extra[1] == Magic.ShoulderDash && !GameScene.Game.MoveFrame) return;

                MapObject.User.AttemptAction(User.MagicAction);
                User.MagicAction = null;
                Mining = false;
            }
            
            if (MapObject.TargetObject != null && !MapObject.TargetObject.Dead && ((MapObject.TargetObject.Race == ObjectType.Monster && string.IsNullOrEmpty(MapObject.TargetObject.PetOwner)) || CEnvir.Shift))
            {
                if (Functions.Distance(MapObject.TargetObject.CurrentLocation, MapObject.User.CurrentLocation) ==  1 && CEnvir.Now > User.AttackTime && User.Horse == HorseType.None)
                {
                    MapObject.User.AttemptAction(new ObjectAction(
                        MirAction.Attack,
                        Functions.DirectionFromPoint(MapObject.User.CurrentLocation, MapObject.TargetObject.CurrentLocation),
                        MapObject.User.CurrentLocation,
                        0, //Ranged Attack Target ID
                        MagicType.None,
                        Element.None));
                    return;
                }
            }

            //If  auto run

            MirDirection direction = MouseDirection(), best;

            if (GameScene.Game.AutoRun)
            {
                if (!GameScene.Game.MoveFrame || (User.Poison & PoisonType.WraithGrip) == PoisonType.WraithGrip) return;
                Run(direction);
                return;
            }

            if (MouseControl == this)
            {
                switch (MapButtons)
                {
                    case MouseButtons.Left:
                        Mining = false;

                        if (CEnvir.Shift && MapObject.TargetObject == null)
                        {
                            if (FishingState != FishingState.None) return;

                            if (CEnvir.Now > User.AttackTime && User.Horse == HorseType.None)
                                MapObject.User.AttemptAction(new ObjectAction(MirAction.Attack, direction, MapObject.User.CurrentLocation, 0, MagicType.None, Element.None));

                            return;
                        }

                        ClientUserItem weap = GameScene.Game.Equipment[(int)EquipmentSlot.Weapon];
                        ClientUserItem armour = GameScene.Game.Equipment[(int)EquipmentSlot.Armour];

                        if (CEnvir.Alt)
                        {
                            if (User.Horse != HorseType.None) return;
                            if (FishingState != FishingState.None) return;

                            if (weap?.Info.ItemEffect == ItemEffect.FishingRod && armour?.Info.ItemEffect == ItemEffect.FishingRobe)
                            {
                                FloatLocation = MapLocation;
                                FishingDirection = Functions.DirectionFromPoint(MapObject.User.CurrentLocation, FloatLocation);

                                var distance = Functions.Distance(MapObject.User.CurrentLocation, FloatLocation);

                                if (Functions.FishingZone(Globals.FishingInfoList, MapInfo, Width, Height, FloatLocation) != null && Functions.ValidFishingDistance(distance, User.Stats[Stat.ThrowDistance]))
                                {
                                    FishingState = FishingState.Cast;
                                    break;
                                }
                            }
                            else
                            {
                                MapObject.User.AttemptAction(new ObjectAction(MirAction.Harvest, direction, MapObject.User.CurrentLocation));
                                return;
                            }
                        }

                        if (FishingState != FishingState.None)
                        {
                            FishingState = FishingState.Cancel;
                            break;
                        }

                        if (MapLocation == MapObject.User.CurrentLocation)
                        {
                            if (CEnvir.Now <= GameScene.Game.PickUpTime) return;

                            CEnvir.Enqueue(new C.PickUp());
                            GameScene.Game.PickUpTime = CEnvir.Now.AddMilliseconds(250);

                            return;
                        }

                        if (MapObject.MouseObject != null && MapObject.MouseObject.Race != ObjectType.Item && !MapObject.MouseObject.Dead) break;

                        if (MapInfo.CanMine && weap != null && weap.Info.ItemEffect == ItemEffect.PickAxe)
                        {
                            MiningPoint = Functions.Move(User.CurrentLocation, direction);

                            if (MiningPoint.X >= 0 && MiningPoint.Y >= 0 && MiningPoint.X < Width && MiningPoint.Y < Height && Cells[MiningPoint.X, MiningPoint.Y].Flag)
                            {
                                Mining = true;
                                break;
                            }
                        }

                        if (!CanMove(direction, 1))
                        {
                            best = MouseDirectionBest(direction, 1);

                            if (best == direction)
                            {
                                if (direction != User.Direction)
                                    MapObject.User.AttemptAction(new ObjectAction(MirAction.Standing, direction, MapObject.User.CurrentLocation));
                                return;
                            }

                            direction = best;
                        }
                        if (GameScene.Game.MoveFrame && (User.Poison & PoisonType.WraithGrip) != PoisonType.WraithGrip)
                            MapObject.User.AttemptAction(new ObjectAction(MirAction.Moving, direction, Functions.Move(MapObject.User.CurrentLocation, direction), 1, MagicType.None));
                        return;
                    case MouseButtons.Right:
                        Mining = false;

                        if (FishingState != FishingState.None)
                        {
                            FishingState = FishingState.Cancel;
                            break;
                        }

                        if (MapObject.MouseObject is PlayerObject && MapObject.MouseObject != MapObject.User && CEnvir.Ctrl) break;

                        if (!GameScene.Game.MoveFrame || (User.Poison & PoisonType.WraithGrip) == PoisonType.WraithGrip) break;
                        
                        if (Functions.InRange(MapLocation, MapObject.User.CurrentLocation, 2))
                        {
                            if (direction != User.Direction)
                                MapObject.User.AttemptAction(new ObjectAction(MirAction.Standing, direction, MapObject.User.CurrentLocation));
                            return;
                        }

                        Run(direction);

                        return;
                }
            }

            if (Mining)
            {
                ClientUserItem weap = GameScene.Game.Equipment[(int)EquipmentSlot.Weapon];

                if (MapInfo.CanMine && weap != null && (weap.CurrentDurability > 0 || weap.Info.Durability == 0) && weap.Info.ItemEffect == ItemEffect.PickAxe &&
                    MiningPoint.X >= 0 && MiningPoint.Y >= 0 && MiningPoint.X < Width && MiningPoint.Y < Height && Cells[MiningPoint.X, MiningPoint.Y].Flag &&
                    Functions.Distance(MiningPoint, MapObject.User.CurrentLocation) == 1 && User.Horse == HorseType.None)
                {
                    if (CEnvir.Now > User.AttackTime)
                    {
                        MapObject.User.AttemptAction(new ObjectAction(
                            MirAction.Mining,
                            Functions.DirectionFromPoint(MapObject.User.CurrentLocation, MiningPoint),
                            MapObject.User.CurrentLocation,
                            false));
                    }
                }
                else
                {
                    Mining = false;
                }
            }

            if (FishingState != FishingState.None || AutoCast)
            {
                if (CEnvir.Now > User.AttackTime)
                {
                    if (FishingState == FishingState.Reel)
                    {
                        FishingState = FishingState.None;
                    }
                    else
                    {
                        if (AutoCast)
                        {
                            FishingState = FishingState.Cast;
                            AutoCast = false;
                        }

                        MapObject.User.AttemptAction(new ObjectAction(MirAction.Fishing, FishingDirection, MapObject.User.CurrentLocation, FishingState, FloatLocation, MapObject.User.FishFound));
                    }
                }
            }

            if (MapObject.TargetObject == null || MapObject.TargetObject.Dead) return;
            if ((MapObject.TargetObject.Race == ObjectType.Player || !string.IsNullOrEmpty(MapObject.TargetObject.PetOwner)) && !CEnvir.Shift) return;
            if (Functions.InRange(MapObject.TargetObject.CurrentLocation, MapObject.User.CurrentLocation, 1)) return;

            direction = Functions.DirectionFromPoint(MapObject.User.CurrentLocation, MapObject.TargetObject.CurrentLocation);

            if (!CanMove(direction, 1))
            {
                best = DirectionBest(direction, 1, MapObject.TargetObject.CurrentLocation);

                if (best == direction)
                {
                    if (direction != User.Direction)
                        MapObject.User.AttemptAction(new ObjectAction(MirAction.Standing, direction, MapObject.User.CurrentLocation));
                    return;
                }
                direction = best;
            }

            if (GameScene.Game.MoveFrame && (User.Poison & PoisonType.WraithGrip) != PoisonType.WraithGrip)
                MapObject.User.AttemptAction(new ObjectAction(MirAction.Moving, direction, Functions.Move(MapObject.User.CurrentLocation, direction), 1, MagicType.None));
        }

        public void Run(MirDirection direction)
        {
            int steps = 1;

            if (GameScene.Game.CanRun && CEnvir.Now >= User.NextRunTime && User.BagWeight <= User.Stats[Stat.BagWeight] && User.WearWeight <= User.Stats[Stat.WearWeight])
            {
                steps++;
                if (User.Horse != HorseType.None)
                    steps++;
            }

            for (int i = 1; i <= steps; i++)
            {
                if (CanMove(direction, i)) continue;

                MirDirection best = MouseDirectionBest(direction, 1);

                if (best == direction)
                {
                    if (i == 1)
                    {
                        if (direction != User.Direction)
                            MapObject.User.AttemptAction(new ObjectAction(MirAction.Standing, direction, MapObject.User.CurrentLocation));
                        return;
                    }

                    steps = i - 1;
                }
                else
                {
                    steps = 1;
                }
                direction = best;
                break;
            }

            MapObject.User.AttemptAction(new ObjectAction(MirAction.Moving, direction, Functions.Move(MapObject.User.CurrentLocation, direction, steps), steps, MagicType.None));
        }

        public MirDirection MouseDirectionBest(MirDirection dir, int distance) //22.5 = 16
        {

            Point loc = Functions.Move(MapObject.User.CurrentLocation, dir, distance);

            if (loc.X >= 0 && loc.Y >= 0 && loc.X < Width && loc.Y < Height && !Cells[loc.X, loc.Y].Blocking()) return dir;
            

            PointF c = new PointF(OffSetX * CellWidth + CellWidth / 2F, OffSetY * CellHeight + CellHeight / 2F);
            PointF a = new PointF(c.X, 0);
            PointF b = MouseLocation;
            float bc = (float)Functions.Distance(c, b);
            float ac = bc;
            b.Y -= c.Y;
            c.Y += bc;
            b.Y += bc;
            double ab = (float)Functions.Distance(b, a);
            double x = (ac * ac + bc * bc - ab * ab) / (2 * ac * bc);
            double angle = Math.Acos(x);

            angle *= 180 / Math.PI;

            if (MouseLocation.X < c.X) angle = 360 - angle;

            MirDirection best = (MirDirection)(angle / 45F);

            if (best == dir)
                best = Functions.ShiftDirection(dir, 1);

            MirDirection next = Functions.ShiftDirection(dir, -((int)best - (int)dir));

            if (CanMove(best, distance))
                return best;

            if (CanMove(next, distance))
                return next;

            return dir;
        }
        public MirDirection DirectionBest(MirDirection dir, int distance, Point targetLocation) //22.5 = 16
        {
            Point loc = Functions.Move(MapObject.User.CurrentLocation, dir, distance);

            if (loc.X >= 0 && loc.Y >= 0 && loc.X < Width && loc.Y < Height && !Cells[loc.X, loc.Y].Blocking()) return dir;
            

            PointF c = new PointF(MapObject.OffSetX * MapObject.CellWidth + MapObject.CellWidth / 2F, MapObject.OffSetY * MapObject.CellHeight + MapObject.CellHeight / 2F);
            PointF a = new PointF(c.X, 0);
            PointF b = new PointF((targetLocation.X - MapObject.User.CurrentLocation.X + MapObject.OffSetX) * MapObject.CellWidth + MapObject.CellWidth / 2F,
                (targetLocation.Y - MapObject.User.CurrentLocation.Y + MapObject.OffSetY) * MapObject.CellHeight + MapObject.CellHeight / 2F);
            float bc = (float)Functions.Distance(c, b);
            float ac = bc;
            b.Y -= c.Y;
            c.Y += bc;
            b.Y += bc;
            double ab = (float)Functions.Distance(b, a);
            double x = (ac * ac + bc * bc - ab * ab) / (2 * ac * bc);
            double angle = Math.Acos(x);

            angle *= 180 / Math.PI;

            if (b.X < c.X) angle = 360 - angle;

            MirDirection best = (MirDirection)(angle / 45F);

            if (best == dir)
                best = Functions.ShiftDirection(dir, 1);

            MirDirection next = Functions.ShiftDirection(dir, -((int)best - (int)dir));

            if (CanMove(best, distance))
                return best;

            return CanMove(next, distance) ? next : dir;
        }

        private bool CanMove(MirDirection dir, int distance)
        {
            for (int i = 1; i <= distance; i++)
            {
                Point loc = Functions.Move(User.CurrentLocation, dir, i);

                if (loc.X < 0 || loc.Y < 0 || loc.X >= Width || loc.Y > Height) return false;

                if (Cells[loc.X, loc.Y].Blocking())
                    return false;
            }
            return true;
        }

        public MirDirection MouseDirection() //22.5 = 16
        {
            PointF p = new PointF(MouseLocation.X  / CellWidth, MouseLocation.Y / CellHeight);

            //If close proximity then co by co ords 
            if (Functions.InRange(new Point(OffSetX, OffSetY), Point.Truncate(p), 2))
                return Functions.DirectionFromPoint(new Point(OffSetX, OffSetY), Point.Truncate(p));

            PointF c = new PointF(OffSetX * CellWidth + CellWidth / 2F, OffSetY * CellHeight + CellHeight / 2F);
            PointF a = new PointF(c.X, 0);
            PointF b = new PointF(MouseLocation.X, MouseLocation.Y);
            float bc = (float)Functions.Distance(c, b);
            float ac = bc;
            b.Y -= c.Y;
            c.Y += bc;
            b.Y += bc;
            float ab = (float)Functions.Distance(b, a);
            double x = (ac * ac + bc * bc - ab * ab) / (2 * ac * bc);
            double angle = Math.Acos(x);

            angle *= 180 / Math.PI;

            if (MouseLocation.X < c.X) angle = 360 - angle;
            angle += 22.5F;
            if (angle > 360) angle -= 360;


            return (MirDirection)(angle / 45F);
        }

        public void AddObject(MapObject ob)
        {
            Objects.Add(ob);

            if (ob.CurrentLocation.X < Width && ob.CurrentLocation.Y < Height)
                Cells[ob.CurrentLocation.X, ob.CurrentLocation.Y].AddObject(ob);
        }

        public void RemoveObject(MapObject ob)
        {
            Objects.Remove(ob);

            if (ob.CurrentLocation.X < Width && ob.CurrentLocation.Y < Height)
                Cells[ob.CurrentLocation.X, ob.CurrentLocation.Y].RemoveObject(ob);
        }

        public bool CanAttack(MapObject ob)
        {
            if (ob == null || ob == User) return false;

            switch (ob.Race)
            {
                case ObjectType.Player:
                    break;
                case ObjectType.Monster:
                    MonsterObject mob = (MonsterObject) ob;

                    if (mob.MonsterInfo.AI < 0) return false;

                    break;
                default:
                    return false;
            }

            return !ob.Dead;
        }

        public void UpdateMapLocation()
        {
            if (User == null) return;

            GameScene.Game.MapControl.MapLocation = new Point((GameScene.Game.MapControl.MouseLocation.X - GameScene.Game.Location.X) / CellWidth - OffSetX + User.CurrentLocation.X,
                                                              (GameScene.Game.MapControl.MouseLocation.Y - GameScene.Game.Location.Y) / CellHeight - OffSetY + User.CurrentLocation.Y);
        }

        public bool HasTarget(Point loc)
        {
            if (loc.X < 0 || loc.Y < 0 || loc.X >= Width || loc.Y > Height) return false;

            Cell cell = Cells[loc.X, loc.Y];

            if (cell.Objects == null) return false;

            foreach (MapObject ob in cell.Objects)
                if (ob.Blocking) return true;

            return false;
        }

        public bool CanEnergyBlast(MirDirection direction)
        {
            return HasTarget(Functions.Move(MapObject.User.CurrentLocation, direction, 2));
        }

        public bool CanHalfMoon(MirDirection direction)
        {
            if (HasTarget(Functions.Move(MapObject.User.CurrentLocation, Functions.ShiftDirection(direction, -1)))) return true;
            if (HasTarget(Functions.Move(MapObject.User.CurrentLocation, Functions.ShiftDirection(direction, 1)))) return true;
            if (HasTarget(Functions.Move(MapObject.User.CurrentLocation, Functions.ShiftDirection(direction, 2)))) return true;

            return false;
        }

        public bool CanDestructiveBlow(MirDirection direction)
        {
            for (int i = 1; i < 8; i++)
                if (HasTarget(Functions.Move(MapObject.User.CurrentLocation, Functions.ShiftDirection(direction, i)))) return true;

            return false;
        }

        public bool ValidCell(Point location)
        {
            if (location.X < 0 || location.Y < 0 || location.X >= Width || location.Y >= Height) return false;

            return !Cells[location.X, location.Y].Flag;
        }
        #endregion
        
        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _MapInfo = null;
                MapInfoChanged = null;

                _InstanceInfo = null;

                _Animation = 0;
                AnimationChanged = null;

                MapButtons = 0;
                MapLocation = Point.Empty;
                Mining = false;
                MiningPoint = Point.Empty;
                MiningDirection = 0;

                FishingState = FishingState.None;
                FloatLocation = Point.Empty;
                FishingDirection = 0;
                AutoCast = false;

                if (FLayer != null)
                {
                    if (!FLayer.IsDisposed)
                        FLayer.Dispose();

                    FLayer = null;
                }

                if (LLayer != null)
                {
                    if (!LLayer.IsDisposed)
                        LLayer.Dispose();

                    LLayer = null;
                }

                Cells = null;

                Width = 0;
                Height = 0;

                MapInfoObjects.Clear();
                MapInfoObjects = null;

                Objects.Clear();
                Objects = null;

                Effects.Clear();
                Effects = null;
                ViewRangeX = 0;
                ViewRangeY = 0;
                OffSetX = 0;
                OffSetY = 0;

                if (ParticleEffects != null)
                {
                    foreach (var effect in ParticleEffects)
                    {
                        effect.Dispose();
                    }

                    ParticleEffects.Clear();
                    ParticleEffects = null;
                }
            }
        }

        #endregion

        public sealed class Floor : DXControl
        {
            public Floor()
            {
                IsControl = false;
            }

            #region Methods
            public void CheckTexture()
            {
                if (!TextureValid)
                    CreateTexture();
            }
            
            protected override void OnClearTexture()
            {
                base.OnClearTexture();

                int minX = Math.Max(0, User.CurrentLocation.X - OffSetX - 4), maxX = Math.Min(GameScene.Game.MapControl.Width - 1, User.CurrentLocation.X + OffSetX + 4);
                int minY = Math.Max(0, User.CurrentLocation.Y - OffSetY - 4), maxY = Math.Min(GameScene.Game.MapControl.Height - 1, User.CurrentLocation.Y + OffSetY + 4);

                switch (GameScene.Game.MapControl.MapFormat.mapType)
                {
                    case MapType.Other:
                        //Mir2 and other mir3 maps
                        for (int y = minY; y <= maxY; y++)
                        {

                            int drawY = (y - User.CurrentLocation.Y + OffSetY) * GameScene.Game.MapControl.CellHeight - User.MovingOffSet.Y;

                            for (int x = minX; x <= maxX; x++)
                            {
                                int drawX = (x - User.CurrentLocation.X + OffSetX) * GameScene.Game.MapControl.CellWidth - User.MovingOffSet.X;

                                Cell cell = GameScene.Game.MapControl.Cells[x, y];
                                MirLibrary library = null;
                                int index = -1;

                                //back layer
                                if (y % 2 == 0 && x % 2 == 0)
                                {
                                    index = cell.BackImage - 1;

                                    if (cell.BackImage > -1)
                                    {
                                        library = CrystalMaps.MapLibs[cell.BackFile];
                                        index = (cell.BackImage & 0x1FFFF) - 1;

                                        if (index > -1)
                                            library?.Draw(index, drawX, drawY, Color.White, false, 1F, ImageType.Image);
                                    }
                                }

                                //middle layer
                                if (cell.MiddleFile > -1)
                                {
                                    library = null;

                                    library = CrystalMaps.MapLibs[cell.MiddleFile];

                                    if (library != null)
                                    {
                                        index = cell.MiddleImage - 1;

                                        if (index > -1)
                                        {

                                            if (cell.MiddleFile > 199)
                                            {

                                                Size s = library.GetSize(index);

                                                if (s.Width != GameScene.Game.MapControl.CellWidth || s.Height != GameScene.Game.MapControl.CellHeight) continue;

                                            }

                                            library.Draw(index, drawX, drawY - GameScene.Game.MapControl.CellHeight, Color.White, false, 1F, ImageType.Image);
                                        }
                                    }
                                }

                                #region Optimized from DrawObjects to Floor
                                //front layer
                                if (cell.FrontFile > -1)
                                {
                                    library = null;

                                    library = CrystalMaps.MapLibs[cell.FrontFile];

                                    if (library != null)
                                    {
                                        index = (cell.FrontImage & 0x7FFF) - 1;
                                        if (index == -1) continue;

                                        int fileIndex = cell.FrontFile;
                                        if (fileIndex == -1) continue;

                                        Size s = library.GetSize(index);

                                        if ((s.Width != GameScene.Game.MapControl.CellWidth || s.Height != GameScene.Game.MapControl.CellHeight) && ((s.Width != GameScene.Game.MapControl.CellWidth * 2) || (s.Height != GameScene.Game.MapControl.CellHeight * 2))) continue;

                                        library.Draw(index, drawX, drawY, Color.White, false, 1F, ImageType.Image);
                                    }

                                }
                                #endregion

                            }
                        }


                        break;
                    case MapType.Mir3:
                        //original mir3
                        for (int y = minY; y <= maxY; y++)
                        {
                            if (y < 0) continue;
                            if (y >= GameScene.Game.MapControl.Height) break;

                            int drawY = (y - User.CurrentLocation.Y + OffSetY) * GameScene.Game.MapControl.CellHeight - User.MovingOffSet.Y;

                            for (int x = minX; x <= maxX; x++)
                            {
                                if (x < 0) continue;
                                if (x >= GameScene.Game.MapControl.Width) break;

                                int drawX = (x - User.CurrentLocation.X + OffSetX) * GameScene.Game.MapControl.CellWidth - User.MovingOffSet.X;

                                Cell tile = GameScene.Game.MapControl.Cells[x, y];

                                if (y % 2 == 0 && x % 2 == 0)
                                {
                                    MirLibrary library;
                                    LibraryFile file;

                                    if (!Libraries.KROrder.TryGetValue(tile.BackFile, out file)) continue;

                                    if (!CEnvir.LibraryList.TryGetValue(file, out library)) continue;

                                    library.Draw(tile.BackImage, drawX, drawY, Color.White, false, 1F, ImageType.Image);
                                }
                            }
                        }

                        for (int y = minY; y <= maxY; y++)
                        {
                            int drawY = (y - User.CurrentLocation.Y + OffSetY + 1) * GameScene.Game.MapControl.CellHeight - User.MovingOffSet.Y;

                            for (int x = minX; x <= maxX; x++)
                            {
                                int drawX = (x - User.CurrentLocation.X + OffSetX) * GameScene.Game.MapControl.CellWidth - User.MovingOffSet.X;

                                Cell cell = GameScene.Game.MapControl.Cells[x, y];

                                MirLibrary library;
                                LibraryFile file;

                                if (Libraries.KROrder.TryGetValue(cell.MiddleFile, out file) && file != LibraryFile.Tilesc && CEnvir.LibraryList.TryGetValue(file, out library))
                                {
                                    int index = cell.MiddleImage - 1;

                                    if (cell.MiddleAnimationFrame > 1 && cell.MiddleAnimationFrame < 255)
                                        continue;//   index += GameScene.Game.MapControl.Animation % cell.MiddleAnimationFrame;

                                    Size s = library.GetSize(index);

                                    if ((s.Width == GameScene.Game.MapControl.CellWidth && s.Height == GameScene.Game.MapControl.CellHeight) || (s.Width == GameScene.Game.MapControl.CellWidth * 2 && s.Height == GameScene.Game.MapControl.CellHeight * 2))
                                        library.Draw(index, drawX, drawY - GameScene.Game.MapControl.CellHeight, Color.White, false, 1F, ImageType.Image);
                                }


                                if (Libraries.KROrder.TryGetValue(cell.FrontFile, out file) && file != LibraryFile.Tilesc && CEnvir.LibraryList.TryGetValue(file, out library))
                                {
                                    int index = cell.FrontImage - 1;

                                    if (cell.FrontAnimationFrame > 1 && cell.FrontAnimationFrame < 255)
                                        continue;//  index += GameScene.Game.MapControl.Animation % cell.FrontAnimationFrame;

                                    Size s = library.GetSize(index);

                                    if ((s.Width == GameScene.Game.MapControl.CellWidth && s.Height == GameScene.Game.MapControl.CellHeight) || (s.Width == GameScene.Game.MapControl.CellWidth * 2 && s.Height == GameScene.Game.MapControl.CellHeight * 2))
                                        library.Draw(index, drawX, drawY - GameScene.Game.MapControl.CellHeight, Color.White, false, 1F, ImageType.Image);
                                }
                            }
                        }
                        break;
                    case MapType.Woool:
                        //Woool maps converted
                        for (int y = minY; y <= maxY; y++)
                        {

                            int drawY = (y - User.CurrentLocation.Y + OffSetY) * GameScene.Game.MapControl.CellHeight - User.MovingOffSet.Y;

                            for (int x = minX; x <= maxX; x++)
                            {

                                if (y % 2 == 0 && x % 2 == 0)
                                {
                                    Cell tile = GameScene.Game.MapControl.Cells[x, y];

                                    if (tile.BackFile == -1) continue;

                                    MirLibrary library = null;

                                    var index = tile.BackImage & 0x1FFFF;

                                    if (tile.BackImage == -1) continue;

                                    library = CrystalMaps.MapLibs[tile.BackFile];

                                    if (index < 0) continue;

                                    int drawX = (x - User.CurrentLocation.X + OffSetX) * GameScene.Game.MapControl.CellWidth - User.MovingOffSet.X;

                                    library?.Draw(index, drawX, drawY, Color.White, false, 1F, ImageType.Image);

                                }
                            }
                        }
                        break;
                    case MapType.None:
                    default:
                        break;
                }

            }

            public override void Draw()
            {
            }
            protected override void DrawControl()
            {
            }

            #endregion
        }

        public sealed class Light : DXControl
        {
            public Light()
            {
                IsControl = false;
                BackColour = Color.FromArgb(15, 15, 15);
            }

            #region Methods

            public void CheckTexture()
            {
                CreateTexture();
            }

            protected override void OnClearTexture()
            {
                base.OnClearTexture();

                if (MapObject.User.Dead)
                {
                    DXManager.Device.Clear(ClearFlags.Target, Color.IndianRed, 0, 0);
                    return;
                }

                DXManager.SetBlend(true);
                DXManager.Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                DXManager.Device.SetRenderState(RenderState.DestinationBlend, Blend.One);


                const float lightScale = 0.02F; //Players/Monsters
                const float baseSize = 0.1F;
                
                float fX;
                float fY;

                if ((MapObject.User.Poison & PoisonType.Abyss) == PoisonType.Abyss)
                {
                    DXManager.Device.Clear(ClearFlags.Target, Color.Black, 0, 0);

                    float scale = baseSize + 4  * lightScale;

                    fX = (OffSetX + MapObject.User.CurrentLocation.X - User.CurrentLocation.X) * GameScene.Game.MapControl.CellWidth  + GameScene.Game.MapControl.CellWidth / 2;
                    fY = (OffSetY + MapObject.User.CurrentLocation.Y - User.CurrentLocation.Y) * GameScene.Game.MapControl.CellHeight;

                    fX -= (DXManager.LightWidth * scale) / 2;
                    fY -= (DXManager.LightHeight * scale) / 2;

                    fX /= scale;
                    fY /= scale;

                    DXManager.Sprite.Transform = Matrix.Scaling(scale, scale, 1);

                    DXManager.Sprite.Draw(DXManager.LightTexture, Vector3.Zero, new Vector3(fX, fY, 0), Color.White);

                    DXManager.Sprite.Transform = Matrix.Identity;

                    DXManager.SetBlend(false);
                    
                    MapObject.User.AbyssEffect.Draw();
                    return;
                }


                foreach (MapObject ob in GameScene.Game.MapControl.Objects)
                {
                    if (ob.Light > 0 && (!ob.Dead || ob == MapObject.User || ob.Race == ObjectType.Spell))
                    {
                        float scale = baseSize + ob.Light * 2 * lightScale;

                        fX = (OffSetX + ob.CurrentLocation.X - User.CurrentLocation.X) * GameScene.Game.MapControl.CellWidth + ob.MovingOffSet.X - User.MovingOffSet.X + GameScene.Game.MapControl.CellWidth / 2;
                        fY = (OffSetY + ob.CurrentLocation.Y - User.CurrentLocation.Y) * GameScene.Game.MapControl.CellHeight + ob.MovingOffSet.Y - User.MovingOffSet.Y;

                        fX -= (DXManager.LightWidth * scale) / 2;
                        fY -= (DXManager.LightHeight * scale) / 2;

                        fX /= scale;
                        fY /= scale;

                        DXManager.Sprite.Transform = Matrix.Scaling(scale, scale, 1);

                        DXManager.Sprite.Draw(DXManager.LightTexture, Vector3.Zero, new Vector3(fX, fY, 0), ob.LightColour);

                        DXManager.Sprite.Transform = Matrix.Identity;
                    }
                }

                foreach (MirEffect ob in GameScene.Game.MapControl.Effects)
                {
                    float frameLight = ob.FrameLight;

                    if (frameLight > 0)
                    {
                        float scale = baseSize + frameLight * 2 * lightScale / 5;

                        fX = ob.DrawX + GameScene.Game.MapControl.CellWidth / 2;
                        fY = ob.DrawY + GameScene.Game.MapControl.CellHeight / 2;

                        fX -= (DXManager.LightWidth * scale) / 2;
                        fY -= (DXManager.LightHeight * scale) / 2;

                        fX /= scale;
                        fY /= scale;

                        DXManager.Sprite.Transform = Matrix.Scaling(scale, scale, 1);

                        DXManager.Sprite.Draw(DXManager.LightTexture, Vector3.Zero, new Vector3(fX, fY, 0), ob.FrameLightColour);

                        DXManager.Sprite.Transform = Matrix.Identity;
                    }
                }

                int minX = Math.Max(0, User.CurrentLocation.X - OffSetX - 15), maxX = Math.Min(GameScene.Game.MapControl.Width - 1, User.CurrentLocation.X + OffSetX + 15);
                int minY = Math.Max(0, User.CurrentLocation.Y - OffSetY - 15), maxY = Math.Min(GameScene.Game.MapControl.Height - 1, User.CurrentLocation.Y + OffSetY + 15);

                for (int y = minY; y <= maxY; y++)
                {
                    if (y < 0) continue;
                    if (y >= GameScene.Game.MapControl.Height) break;

                    int drawY = (y - User.CurrentLocation.Y + OffSetY)*GameScene.Game.MapControl.CellHeight - User.MovingOffSet.Y;

                    for (int x = minX; x <= maxX; x++)
                    {
                        if (x < 0) continue;
                        if (x >= GameScene.Game.MapControl.Width) break;

                        int drawX = (x - User.CurrentLocation.X + OffSetX)*GameScene.Game.MapControl.CellWidth - User.MovingOffSet.X;

                        Cell tile = GameScene.Game.MapControl.Cells[x, y];

                        if (tile.Light == 0) continue;

                        float scale = baseSize + tile.Light * 30 * lightScale;

                        fX = drawX + GameScene.Game.MapControl.CellWidth / 2;
                        fY = drawY  + GameScene.Game.MapControl.CellHeight / 2;

                        fX -= DXManager.LightWidth * scale / 2;
                        fY -= DXManager.LightHeight * scale / 2;

                        fX /= scale;
                        fY /= scale;

                        DXManager.Sprite.Transform = Matrix.Scaling(scale, scale, 1);

                        DXManager.Sprite.Draw(DXManager.LightTexture, Vector3.Zero, new Vector3(fX, fY, 0), Color.White);

                        DXManager.Sprite.Transform = Matrix.Identity;
                    }
                }


                DXManager.SetBlend(false);
            }

            public void UpdateLights()
            {
                switch (GameScene.Game.MapControl.MapInfo.Light)
                {
                    case LightSetting.Default:
                        byte shading = (byte) (255 * GameScene.Game.DayTime);
                        BackColour = Color.FromArgb(shading, shading, shading);
                        Visible = true;
                        break;
                    case LightSetting.Night:
                        BackColour = Color.FromArgb(15, 15, 15);
                        Visible = true;
                        break;
                    case LightSetting.Twilight:
                        BackColour = Color.FromArgb(100, 100, 100);
                        Visible = true;
                        break;
                    case LightSetting.Light:
                        BackColour = Color.FromArgb(200, 200, 200);
                        Visible = true;
                        break;
                }

                if (MapObject.User != null && (MapObject.User.Poison & PoisonType.Abyss) == PoisonType.Abyss)
                {
                    BackColour = Color.FromArgb(15, 15, 15);
                    Visible = false;
                }
            }
            protected override void DrawControl()
            {
            }

            public override void Draw()
            {
            }
            #endregion
        }
    }

    public sealed class Cell
    {
        public int BackFile;
        public int BackImage;

        public int MiddleFile;
        public int MiddleImage;

        public int FrontFile;
        public int FrontImage;

        public int FrontAnimationFrame;
        public int FrontAnimationTick;

        public int MiddleAnimationFrame;
        public int MiddleAnimationTick;

        public int Light;

        public bool Flag;

        public int DoorIndex;
        public byte DoorOffset;
        public short TileAnimationImage;
        public short TileAnimationOffset;
        public byte TileAnimationFrames;
        public byte Unknown;

        public List<MapObject> Objects;

        //generic mir2 & mir3 tweaks
        public bool Mir3Layer;
        public bool FrontFileNegativeDisplacement;
        //woool
        public bool MiddleBlend;

        public bool MiddleSpecialBlend;

        public bool FrontBlend;

        public int CustomCellHeight;

        public bool Blocking()
        {
            if (Objects != null)
            {
                foreach (MapObject ob in Objects)
                    if (ob.Blocking) return true;
            }

            return Flag;
        }

        public void AddObject(MapObject ob)
        {
            if (Objects == null)
                Objects = new List<MapObject>();

            if (ob.Race == ObjectType.Spell)
                Objects.Insert(0, ob);
            else
                Objects.Add(ob);

            ob.CurrentCell = this;
        }

        public void RemoveObject(MapObject ob)
        {
            Objects.Remove(ob);

            if (Objects.Count == 0)
                Objects = null;

            ob.CurrentCell = null;
        }
    }

}
