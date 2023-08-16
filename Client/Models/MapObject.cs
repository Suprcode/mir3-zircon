using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.Controls;
using Client.Envir;
using Client.Models.Particles;
using Client.Scenes;
using Client.Scenes.Views;
using Library;
using Library.SystemModels;
using SlimDX;
using SlimDX.Direct3D9;
using Frame = Library.Frame;

namespace Client.Models
{
    public abstract class MapObject
    {
        public static SortedDictionary<string, List<DXLabel>> NameLabels = new SortedDictionary<string, List<DXLabel>>();
        public static List<DXLabel> ChatLabels = new List<DXLabel>();

        public static UserObject User => GameScene.Game.User;

        public static MapObject MouseObject
        {
            get { return GameScene.Game.MouseObject; }
            set
            {
                if (GameScene.Game.MouseObject == value) return;

                GameScene.Game.MouseObject = value;
                //GameScene.Game.HuntingLogPanel.MouseMonsterIndex = value?.MonsterIndex ?? 0;
                GameScene.Game.MapControl.TextureValid = false;
            }
        }
        public static MapObject TargetObject
        {
            get { return GameScene.Game.TargetObject; }
            set
            {
                if (GameScene.Game.TargetObject == value) return;

                GameScene.Game.TargetObject = value;
                //if (value != null) GameScene.Game.OldTargetObjectID = value.ObjectID;
                GameScene.Game.MapControl.TextureValid = false;
            }
        }
        public static MapObject MagicObject
        {
            get { return GameScene.Game.MagicObject; }
            set
            {
                if (GameScene.Game.MagicObject == value) return;

                GameScene.Game.MagicObject = value;
                //if (value != null) GameScene.Game.OldTargetObjectID = value.ObjectID;
                GameScene.Game.MapControl.TextureValid = false;
            }
        }

        public static Texture ShadowTexture;

        public abstract ObjectType Race { get; }
        public virtual bool Blocking => Visible && !Dead;
        public bool Visible = true;

        public uint ObjectID;

        public virtual int Level { get; set; }
        public virtual int CurrentHP { get; set; }
        public virtual int CurrentMP { get; set; }
        public virtual int CurrentFP { get; set; }

        public uint AttackerID;

        public MagicType MagicType;
        public Element AttackElement;
        public List<MapObject> AttackTargets;
        public List<Point> MagicLocations;
        public bool MagicCast;
        public MirGender Gender;
        public bool MiningEffect;

        public FishingState FishingState;
        public bool FishFound;
        public Point FloatLocation;

        public Point CurrentLocation
        {
            get { return _CurrentLocation; }
            set
            {
                if (_CurrentLocation == value) return;

                _CurrentLocation = value;

                LocationChanged();
            }
        }
        private Point _CurrentLocation;
        public Cell CurrentCell;

        public MirDirection Direction;

        public virtual string Name
        {
            get { return _Name; }
            set
            {
                if (_Name == value) return;

                _Name = value;

                NameChanged();
            }
        }
        private string _Name;

        public virtual string Caption
        {
            get { return _Caption; }
            set
            {
                if (_Caption == value) return;

                _Caption = value;

                NameChanged();
            }
        }
        private string _Caption;
        public virtual string Title
        {
            get { return _Title; }
            set
            {
                if (_Title == value) return;

                _Title = value;

                NameChanged();
            }
        }
        private string _Title;

        public string PetOwner
        {
            get { return _PetOwner; }
            set
            {
                if (_PetOwner == value) return;

                _PetOwner = value;

                NameChanged();
            }
        }
        private string _PetOwner;


        public Color NameColour
        {
            get
            {
                if (Race != ObjectType.Player) return _NameColour;

                foreach (CastleInfo castle in GameScene.Game.ConquestWars)
                {
                    if (castle.Map != GameScene.Game.MapControl.MapInfo) continue;

                    if (User.Title == Title) return Color.DarkCyan;

                    return Color.OrangeRed;
                }

                if (GameScene.Game.GuildWars.Count == 0) return _NameColour;

                if (User.Title == Title) return Color.DarkCyan;

                if (GameScene.Game.GuildWars.Contains(Title)) return Color.OrangeRed;

                return _NameColour;
            }
            set
            {
                if (_NameColour == value) return;

                _NameColour = value;

                NameChanged();
            }
        }
        private Color _NameColour;

        public DateTime ChatTime;
        public DXLabel NameLabel, ChatLabel, TitleNameLabel, CaptionLabel;
        public List<DamageInfo> DamageList = new List<DamageInfo>();
        public List<MirEffect> Effects = new List<MirEffect>();

        public List<BuffType> VisibleBuffs = new List<BuffType>();
        public PoisonType Poison;

        public int MoveDistance;
        public int DrawFrame
        {
            get { return _DrawFrame; }
            set
            {
                if (_DrawFrame == value) return;

                _DrawFrame = value;
                DrawFrameChanged();
            }
        }
        private int _DrawFrame;

        public int FrameIndex
        {
            get { return _FrameIndex; }
            set
            {
                if (_FrameIndex == value) return;

                _FrameIndex = value;
                FrameIndexChanged();
            }
        }
        private int _FrameIndex;

        public int DrawX;
        public int DrawY;

        public DateTime DrawHealthTime, StanceTime;

        private Point _MovingOffSet;
        public Point MovingOffSet
        {
            get
            {
                return _MovingOffSet;
            }
            set
            {
                if (_MovingOffSet == value) return;

                _MovingOffSet = value;
                MovingOffSetChanged();
            }
        }

        public int Light;
        public float Opacity = 1F;
        public Color LightColour = Globals.NoneColour;

        public MirEffect MagicShieldEffect, WraithGripEffect, WraithGripEffect2, AssaultEffect, CelestialLightEffect, LifeStealEffect, SilenceEffect, BlindEffect, AbyssEffect, DragonRepulseEffect, DragonRepulseEffect1,
                         RankingEffect, DeveloperEffect, FrostBiteEffect, InfectionEffect;

        public bool CanShowWraithGrip = true;


        public bool Skeleton;

        public bool Dead
        {
            get { return _Dead; }
            set
            {
                if (_Dead == value) return;

                _Dead = value;

                DeadChanged();
            }
        }
        private bool _Dead;

        public virtual Stats Stats { get; set; }

        public bool Interupt;
        public MirAction CurrentAction;
        public MirAnimation CurrentAnimation;
        public Frame CurrentFrame;
        public DateTime FrameStart;
        public Dictionary<MirAnimation, Frame> Frames;
        public Color DrawColour;

        public Color DefaultColour = Color.White;

        public virtual int RenderY
        {
            get
            {
                if (MovingOffSet.IsEmpty)
                    return CurrentLocation.Y;

                switch (Direction)
                {
                    case MirDirection.Up:
                    case MirDirection.UpRight:
                    case MirDirection.UpLeft:
                        return CurrentLocation.Y + MoveDistance;
                    default:
                        return CurrentLocation.Y;
                }
            }

        }

        public static int CellWidth => GameScene.Game.MapControl.CellWidth;
        public static int CellHeight => GameScene.Game.MapControl.CellHeight;
        public static int OffSetX => MapControl.OffSetX;
        public static int OffSetY => MapControl.OffSetY;

        public List<ObjectAction> ActionQueue = new List<ObjectAction>();


        public virtual void Process()
        {
            DamageInfo previous = null;
            for (int index = 0; index < DamageList.Count; index++)
            {
                DamageInfo damageInfo = DamageList[index];
                if (DamageList.Count - index > 3 && CEnvir.Now - damageInfo.StartTime > damageInfo.AppearDelay && CEnvir.Now - damageInfo.StartTime < damageInfo.AppearDelay + damageInfo.ShowDelay)
                    damageInfo.StartTime = CEnvir.Now - damageInfo.AppearDelay - damageInfo.ShowDelay;


                damageInfo.Process(previous);

                previous = damageInfo;
            }

            for (int i = DamageList.Count - 1; i >= 0; i--)
            {
                if (!DamageList[i].Visible)
                    DamageList.RemoveAt(i);
            }
            UpdateFrame();

            DrawX = CurrentLocation.X - User.CurrentLocation.X + MapControl.OffSetX;
            DrawY = CurrentLocation.Y - User.CurrentLocation.Y + MapControl.OffSetY;

            DrawX *= GameScene.Game.MapControl.CellWidth;
            DrawY *= GameScene.Game.MapControl.CellHeight;

            if (this != User)
            {
                DrawX += MovingOffSet.X - User.MovingOffSet.X;
                DrawY += MovingOffSet.Y - User.MovingOffSet.Y;
            }

            DrawColour = DefaultColour;

            if ((Poison & PoisonType.Red) == PoisonType.Red)
                DrawColour = Color.IndianRed;

            if ((Poison & PoisonType.Green) == PoisonType.Green)
                DrawColour = Color.SeaGreen;

            if ((Poison & PoisonType.Slow) == PoisonType.Slow)
                DrawColour = Color.CornflowerBlue;

            if ((Poison & PoisonType.Paralysis) == PoisonType.Paralysis)
                DrawColour = Color.DimGray;

            if ((Poison & PoisonType.WraithGrip) == PoisonType.WraithGrip)
            {
                if (CanShowWraithGrip && WraithGripEffect == null)
                    WraithGripCreate();
            }
            else
            {
                if (WraithGripEffect != null)
                    WraithGripEnd();
            }

            if ((Poison & PoisonType.Silenced) == PoisonType.Silenced)
            {
                if (SilenceEffect == null)
                    SilenceCreate();
            }
            else
            {
                if (SilenceEffect != null)
                    SilenceEnd();
            }


            if ((Poison & PoisonType.Abyss) == PoisonType.Abyss)
            {
                if (BlindEffect == null)
                    BlindCreate();
            }
            else
            {
                if (BlindEffect != null)
                    BlindEnd();
            }

            if ((Poison & PoisonType.Infection) == PoisonType.Infection)
            {
                if (InfectionEffect == null)
                    InfectionCreate();
            }
            else
            {
                if (InfectionEffect != null)
                    InfectionEnd();
            }

            if (Stats?[Stat.ClearRing] > 0 || VisibleBuffs.Contains(BuffType.Invisibility) || VisibleBuffs.Contains(BuffType.Cloak) || VisibleBuffs.Contains(BuffType.Transparency))
                Opacity = 0.5f;
            else
                Opacity = 1f;

            if (VisibleBuffs.Contains(BuffType.MagicShield))
            {
                if (MagicShieldEffect == null)
                    MagicShieldCreate();
            }
            else if (MagicShieldEffect != null)
                MagicShieldEnd();


            if (VisibleBuffs.Contains(BuffType.Developer))
            {
                if (RankingEffect != null)
                    RankingEnd();

                if (DeveloperEffect == null)
                    DeveloperCreate();
            }
            else
            {
                if (DeveloperEffect != null)
                    DeveloperEnd();

                if (VisibleBuffs.Contains(BuffType.Ranking))
                {
                    if (RankingEffect == null)
                        RankingCreate();
                }
                else if (RankingEffect != null)
                    RankingEnd();
            }


            if (VisibleBuffs.Contains(BuffType.LifeSteal))
            {
                if (LifeStealEffect == null)
                    LifeStealCreate();
            }
            else if (LifeStealEffect != null)
                LifeStealEnd();

            if (VisibleBuffs.Contains(BuffType.CelestialLight))
            {
                if (CelestialLightEffect == null)
                    CelestialLightCreate();
            }
            else if (CelestialLightEffect != null)
                CelestialLightEnd();

            if (VisibleBuffs.Contains(BuffType.FrostBite))
            {
                if (FrostBiteEffect == null)
                    FrostBiteCreate();
            }
            else if (FrostBiteEffect != null)
                FrostBiteEnd();
        }

        public virtual void UpdateFrame()
        {
            if (Frames == null || CurrentFrame == null) return;


            switch (CurrentAction)
            {
                case MirAction.Moving:
                case MirAction.Pushed:
                    if (!GameScene.Game.MoveFrame) return;
                    break;
            }

            int frame = CurrentFrame.GetFrame(FrameStart, CEnvir.Now, (this != User || GameScene.Game.Observer) && ActionQueue.Count > 1);

            if (frame == CurrentFrame.FrameCount || (Interupt && ActionQueue.Count > 0))
            {
                DoNextAction();
                frame = CurrentFrame.GetFrame(FrameStart, CEnvir.Now, (this != User || GameScene.Game.Observer) && ActionQueue.Count > 1);
                if (frame == CurrentFrame.FrameCount)
                    frame -= 1;
            }

            int x = 0, y = 0;
            switch (CurrentAction)
            {
                case MirAction.Moving:
                case MirAction.Pushed:
                    switch (Direction)
                    {
                        case MirDirection.Up:
                            x = 0;
                            y = (int)(CellHeight * MoveDistance / (float)CurrentFrame.FrameCount * (CurrentFrame.FrameCount - (frame + 1)));
                            break;
                        case MirDirection.UpRight:
                            x = -(int)(CellWidth * MoveDistance / (float)CurrentFrame.FrameCount * (CurrentFrame.FrameCount - (frame + 1)));
                            y = (int)(CellHeight * MoveDistance / (float)CurrentFrame.FrameCount * (CurrentFrame.FrameCount - (frame + 1)));
                            break;
                        case MirDirection.Right:
                            x = -(int)(CellWidth * MoveDistance / (float)CurrentFrame.FrameCount * (CurrentFrame.FrameCount - (frame + 1)));
                            y = 0;
                            break;
                        case MirDirection.DownRight:
                            x = -(int)(CellWidth * MoveDistance / (float)CurrentFrame.FrameCount * (CurrentFrame.FrameCount - (frame + 1)));
                            y = -(int)(CellHeight * MoveDistance / (float)CurrentFrame.FrameCount * (CurrentFrame.FrameCount - (frame + 1)));
                            break;
                        case MirDirection.Down:
                            x = 0;
                            y = -(int)(CellHeight * MoveDistance / (float)CurrentFrame.FrameCount * (CurrentFrame.FrameCount - (frame + 1)));
                            break;
                        case MirDirection.DownLeft:
                            x = (int)(CellWidth * MoveDistance / (float)CurrentFrame.FrameCount * (CurrentFrame.FrameCount - (frame + 1)));
                            y = -(int)(CellHeight * MoveDistance / (float)CurrentFrame.FrameCount * (CurrentFrame.FrameCount - (frame + 1)));
                            break;
                        case MirDirection.Left:
                            x = (int)(CellWidth * MoveDistance / (float)CurrentFrame.FrameCount * (CurrentFrame.FrameCount - (frame + 1)));
                            y = 0;
                            break;
                        case MirDirection.UpLeft:
                            x = (int)(CellWidth * MoveDistance / (float)CurrentFrame.FrameCount * (CurrentFrame.FrameCount - (frame + 1)));
                            y = (int)(CellHeight * MoveDistance / (float)CurrentFrame.FrameCount * (CurrentFrame.FrameCount - (frame + 1)));
                            break;
                    }
                    break;
            }
            x -= x % 2;
            y -= y % 2;

            if (CurrentFrame.Reversed)
            {
                frame = CurrentFrame.FrameCount - frame - 1;
                x *= -1;
                y *= -1;
            }

            MovingOffSet = new Point(x, y);

            if (Race == ObjectType.Player && CurrentAction == MirAction.Pushed)
                frame = 0;

            FrameIndex = frame;
            DrawFrame = FrameIndex + CurrentFrame.StartIndex + CurrentFrame.OffSet * (int)Direction;

        }

        public abstract void SetAnimation(ObjectAction action);
        public virtual void SetFrame(ObjectAction action)
        {
            SetAnimation(action);

            FrameIndex = -1;
            CurrentAction = action.Action;
            FrameStart = CEnvir.Now;

            switch (action.Action)
            {
                case MirAction.Standing:
                case MirAction.Dead:
                    Interupt = true;
                    break;
                case MirAction.Fishing:
                    Interupt = CurrentAnimation == MirAnimation.FishingWait;
                    break;
                default:
                    Interupt = false;
                    break;
            }
        }
        public virtual void SetAction(ObjectAction action)
        {
            MirEffect spell;

            switch (CurrentAction)
            {
                case MirAction.Mining:
                    if (!MiningEffect) break;

                    Effects.Add(new MirEffect(3470, 3, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 20, 70, Globals.NoneColour) //Element style?
                    {
                        Blend = true,
                        MapTarget = CurrentLocation,
                        Direction = Direction,
                        Skip = 10,
                    });
                    break;
                case MirAction.Spell:
                    if (!MagicCast) break;

                    switch (MagicType)
                    {
                        #region Warrior

                        //Swordsmanship

                        //Potion Mastery 

                        //Slaying

                        //Thrusting

                        //Half Moon

                        //Shoulder Dash

                        //Flaming Sword

                        //Dragon Rise

                        //Blade Storm

                        //Destructive Surge

                        //Interchange

                        //Defiance

                        //Beckon

                        //Might


                        #region Swift Blade

                        case MagicType.SwiftBlade:
                            //todo element colour
                            foreach (Point point in MagicLocations)
                            {
                                spell = new MirEffect(2330, 16, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 10, 35, Globals.NoneColour)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                };
                                spell.Process();
                            }

                            DXSoundManager.Play(SoundIndex.SwiftBladeEnd);
                            break;

                        #endregion

                        //Assault

                        //Endurance

                        //Reflect Damage

                        //Fetter

                        #endregion

                        #region Wizard

                        #region Fire Ball

                        case MagicType.FireBall:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(420, 5, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.FireColour, CurrentLocation, typeof(Client.Models.Particles.FireballTrail))
                                {
                                    Blend = true,
                                    MapTarget = point,
                                });
                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                Effects.Add(spell = new MirProjectile(420, 5, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.FireColour, CurrentLocation, typeof(Client.Models.Particles.FireballTrail))
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });

                                spell.CompleteAction += () =>
                                {
                                    attackTarget.Effects.Add(spell = new MirEffect(580, 10, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 10, 35, Globals.FireColour)
                                    {
                                        Blend = true,
                                        Target = attackTarget,
                                    });
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.FireBallEnd);
                                };

                                spell.Process();
                            }

                            if (MagicLocations.Count > 0 || AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.FireBallTravel);
                            break;

                        #endregion

                        #region Lightning Ball

                        case MagicType.LightningBall:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(3070, 6, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.LightningColour, CurrentLocation)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                });
                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                Effects.Add(spell = new MirProjectile(3070, 6, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.LightningColour, CurrentLocation)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });

                                spell.CompleteAction = () =>
                                {
                                    attackTarget.Effects.Add(spell = new MirEffect(3230, 10, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 10, 35, Globals.LightningColour)
                                    {
                                        Blend = true,
                                        Target = attackTarget,
                                    });
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.ThunderBoltEnd);
                                };
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0 || AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.ThunderBoltTravel);

                            break;

                        #endregion

                        #region Ice Bolt

                        case MagicType.IceBolt:

                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(2700, 3, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.IceColour, CurrentLocation, typeof(IceBoltTrail))
                                {
                                    Blend = true,
                                    MapTarget = point,
                                });
                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                Effects.Add(spell = new MirProjectile(2700, 3, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.IceColour, CurrentLocation, typeof(IceBoltTrail))
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });

                                spell.CompleteAction += () =>
                                {
                                    attackTarget.Effects.Add(spell = new MirEffect(2860, 10, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 10, 35, Globals.IceColour)
                                    {
                                        Blend = true,
                                        Target = attackTarget,
                                    });
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.IceBoltEnd);
                                };
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0 || AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.IceBoltTravel);

                            break;

                        #endregion

                        #region Gust Blast

                        case MagicType.GustBlast:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(430, 5, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 35, 35, Globals.WindColour, CurrentLocation, typeof(GustTrail))
                                {
                                    Blend = true,
                                    MapTarget = point,
                                });
                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                Effects.Add(spell = new MirProjectile(430, 5, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 35, 35, Globals.WindColour, CurrentLocation, typeof(GustTrail))
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });

                                spell.CompleteAction += () =>
                                {
                                    attackTarget.Effects.Add(spell = new MirEffect(590, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 35, Globals.WindColour)
                                    {
                                        Blend = true,
                                        Target = attackTarget,
                                    });
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.GustBlastEnd);
                                };
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0 || AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.GustBlastTravel);

                            break;

                        #endregion

                        #region Electric Shock

                        case MagicType.ElectricShock:
                            foreach (Point point in MagicLocations)
                            {
                                spell = new MirEffect(10, 10, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 10, 35, Globals.LightningColour)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                };
                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                attackTarget.Effects.Add(spell = new MirEffect(10, 10, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 10, 35, Globals.LightningColour)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0 || AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.ElectricShockEnd);
                            break;

                        #endregion

                        //Teleportation

                        #region AdamantineFireBall & MeteorShower & FireBounce

                        case MagicType.AdamantineFireBall:
                        case MagicType.MeteorShower:
                        case MagicType.FireBounce:

                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(1640, 6, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.FireColour, CurrentLocation, typeof(Client.Models.Particles.FireballTrail))
                                {
                                    Blend = true,
                                    MapTarget = point,
                                });
                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                Effects.Add(spell = new MirProjectile(1640, 6, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.FireColour, CurrentLocation, typeof(Client.Models.Particles.FireballTrail))
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });

                                spell.CompleteAction += () =>
                                {
                                    attackTarget.Effects.Add(spell = new MirEffect(1800, 10, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 10, 35, Globals.FireColour)
                                    {
                                        Blend = true,
                                        Target = attackTarget,
                                    });
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.GreaterFireBallEnd);
                                };
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0 || AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.GreaterFireBallTravel);
                            break;

                        #endregion

                        #region Thunder Bolt & Thunder Strike

                        case MagicType.ThunderBolt:
                        case MagicType.ThunderStrike:
                            foreach (Point point in MagicLocations)
                            {
                                spell = new MirEffect(1450, 3, TimeSpan.FromMilliseconds(150), LibraryFile.Magic, 150, 50, Globals.LightningColour)
                                {
                                    Blend = true,
                                    MapTarget = point
                                };
                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                spell = new MirEffect(1450, 3, TimeSpan.FromMilliseconds(150), LibraryFile.Magic, 150, 50, Globals.LightningColour)
                                {
                                    Blend = true,
                                    Target = attackTarget
                                };
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0 || AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.LightningStrikeEnd);
                            break;

                        #endregion

                        #region Ice Blades

                        case MagicType.IceBlades:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(2960, 6, TimeSpan.FromMilliseconds(50), LibraryFile.Magic, 35, 35, Globals.IceColour, CurrentLocation, typeof(IceBladesTrail))
                                {
                                    Blend = true,
                                    MapTarget = point,
                                    Skip = 0,
                                    BlendRate = 1F,
                                });
                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                Effects.Add(spell = new MirProjectile(2960, 6, TimeSpan.FromMilliseconds(50), LibraryFile.Magic, 35, 35, Globals.IceColour, CurrentLocation, typeof(IceBladesTrail))
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                    Skip = 0,
                                    BlendRate = 1F,
                                });

                                spell.CompleteAction += () =>
                                {
                                    attackTarget.Effects.Add(spell = new MirEffect(2970, 10, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 10, 35, Globals.IceColour)
                                    {
                                        Blend = true,
                                        Target = attackTarget
                                    });
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.GreaterIceBoltEnd);
                                };
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0 || AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.GreaterIceBoltTravel);
                            break;

                        #endregion

                        #region Cyclone

                        case MagicType.Cyclone:
                            foreach (Point point in MagicLocations)
                            {
                                spell = new MirEffect(1990, 5, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 50, 80, Globals.WindColour)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                };

                                spell.CompleteAction = () =>
                                {
                                    spell = new MirEffect(2000, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 50, 80, Globals.WindColour)
                                    {
                                        Blend = true,
                                        MapTarget = point
                                    };
                                    spell.Process();
                                };

                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                spell = new MirEffect(1990, 5, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 50, 80, Globals.WindColour)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                };

                                spell.CompleteAction = () =>
                                {
                                    spell = new MirEffect(2000, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 50, 80, Globals.WindColour)
                                    {
                                        Blend = true,
                                        Target = attackTarget
                                    };
                                    spell.Process();
                                };

                                spell.Process();
                            }

                            DXSoundManager.Play(SoundIndex.CycloneEnd);
                            break;

                        #endregion

                        #region Scortched Earth

                        case MagicType.ScortchedEarth:
                            if (Config.DrawEffects && Race != ObjectType.Monster)
                                foreach (Point point in MagicLocations)
                                {
                                    Effects.Add(new MirEffect(220, 1, TimeSpan.FromMilliseconds(2500), LibraryFile.ProgUse, 0, 0, Globals.NoneColour)
                                    {
                                        MapTarget = point,
                                        StartTime = CEnvir.Now.AddMilliseconds(500 + Functions.Distance(point, CurrentLocation) * 50),
                                        Opacity = 0.8F,
                                        DrawType = DrawType.Floor,
                                    });

                                    Effects.Add(new MirEffect(2450 + CEnvir.Random.Next(5) * 10, 10, TimeSpan.FromMilliseconds(250), LibraryFile.Magic, 0, 0, Globals.NoneColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                        StartTime = CEnvir.Now.AddMilliseconds(500 + Functions.Distance(point, CurrentLocation) * 50),
                                        DrawType = DrawType.Floor,
                                    });

                                    Effects.Add(new MirEffect(1900, 30, TimeSpan.FromMilliseconds(50), LibraryFile.Magic, 20, 70, Globals.FireColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                        StartTime = CEnvir.Now.AddMilliseconds(Functions.Distance(point, CurrentLocation) * 50),
                                        BlendRate = 1F,
                                    });

                                }
                            break;

                        #endregion

                        #region Lightning Beam

                        case MagicType.LightningBeam:
                            if (Config.DrawEffects && Race != ObjectType.Monster)
                                foreach (Point point in MagicLocations)
                                {
                                    Effects.Add(spell = new MirEffect(1180, 4, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 150, 150, Globals.LightningColour)
                                    {
                                        Blend = true,
                                        Target = this,
                                        Direction = Functions.DirectionFromPoint(CurrentLocation, point)
                                    });
                                    spell.Process();
                                }

                            DXSoundManager.Play(SoundIndex.LightningBeamEnd);
                            break;

                        #endregion

                        #region Frozen Earth

                        case MagicType.FrozenEarth:
                            if (Config.DrawEffects && Race != ObjectType.Monster)
                                foreach (Point point in MagicLocations)
                                {
                                    Effects.Add(spell = new MirEffect(90, 20, TimeSpan.FromMilliseconds(50), LibraryFile.MagicEx, 20, 70, Globals.IceColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                        StartTime = CEnvir.Now.AddMilliseconds(Functions.Distance(point, CurrentLocation) * 50),
                                        Opacity = 0.5F,
                                    });

                                    spell.CompleteAction = () =>
                                    {
                                        Effects.Add(spell = new MirEffect(260, 1, TimeSpan.FromMilliseconds(2500), LibraryFile.ProgUse, 0, 0, Globals.IceColour)
                                        {
                                            MapTarget = point,
                                            Opacity = 0.8F,
                                            DrawType = DrawType.Floor,
                                        });
                                        spell.Process();
                                    };
                                    spell.Process();
                                }
                            if (MagicLocations.Count > 0)
                                DXSoundManager.Play(SoundIndex.FrozenEarthEnd);
                            break;

                        #endregion

                        #region Blow Earth

                        case MagicType.BlowEarth:
                            if (Config.DrawEffects && Race != ObjectType.Monster)
                                foreach (Point point in MagicLocations)
                                {
                                    Effects.Add(spell = new MirProjectile(1990, 5, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 50, 80, Globals.WindColour, CurrentLocation)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                        Skip = 0,
                                        Explode = true,
                                    });

                                    spell.CompleteAction = () =>
                                    {
                                        spell = new MirEffect(2000, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 50, 80, Globals.WindColour)
                                        {
                                            Blend = true,
                                            MapTarget = point
                                        };
                                        spell.Process();
                                        DXSoundManager.Play(SoundIndex.BlowEarthEnd);
                                    };

                                    spell.Process();
                                }

                            if (MagicLocations.Count > 0)
                                DXSoundManager.Play(SoundIndex.BlowEarthTravel);
                            break;

                        #endregion

                        //Fire Wall

                        #region Expel Undead

                        case MagicType.ExpelUndead:
                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                spell = new MirEffect(140, 5, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 50, 80, Globals.PhantomColour)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                };
                                spell.Process();
                            }

                            DXSoundManager.Play(SoundIndex.ExpelUndeadEnd);
                            break;

                        #endregion

                        //Geo Manipulation

                        //Magic Shield

                        #region Fire Storm

                        case MagicType.FireStorm:
                            foreach (Point point in MagicLocations)
                            {
                                spell = new MirEffect(950, 7, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 10, 35, Globals.FireColour)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                };
                                spell.Process();
                            }

                            DXSoundManager.Play(SoundIndex.FireStormEnd);
                            break;

                        #endregion

                        #region Lightning Wave

                        case MagicType.LightningWave:
                            foreach (Point point in MagicLocations)
                            {
                                spell = new MirEffect(980, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 50, 80, Globals.LightningColour)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                };
                                spell.Process();
                            }

                            DXSoundManager.Play(SoundIndex.LightningWaveEnd);
                            break;

                        #endregion

                        #region Ice Storm

                        case MagicType.IceStorm:
                            foreach (Point point in MagicLocations)
                            {
                                spell = new MirEffect(780, 7, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 10, 35, Globals.IceColour)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                };
                                spell.Process();
                            }

                            DXSoundManager.Play(SoundIndex.IceStormEnd);
                            break;

                        #endregion

                        #region DragonTornado

                        case MagicType.DragonTornado:
                            foreach (Point point in MagicLocations)
                            {
                                spell = new MirEffect(1040, 16, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 35, Globals.WindColour)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                };
                                spell.Process();
                            }

                            DXSoundManager.Play(SoundIndex.DragonTornadoEnd);
                            break;

                        #endregion

                        #region Greater Frozen Earth

                        case MagicType.GreaterFrozenEarth:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirEffect(90, 20, TimeSpan.FromMilliseconds(50), LibraryFile.MagicEx, 20, 70, Globals.IceColour)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                    StartTime = CEnvir.Now.AddMilliseconds(Functions.Distance(point, CurrentLocation) * 50),
                                    Opacity = 0.5F,
                                });

                                spell.CompleteAction = () =>
                                {
                                    Effects.Add(spell = new MirEffect(260, 1, TimeSpan.FromMilliseconds(2500), LibraryFile.ProgUse, 0, 0, Globals.NoneColour)
                                    {
                                        MapTarget = point,
                                        Opacity = 0.8F,
                                        DrawType = DrawType.Floor,
                                    });
                                    spell.Process();
                                };
                                spell.Process();
                            }
                            if (MagicLocations.Count > 0)
                                DXSoundManager.Play(SoundIndex.GreaterFrozenEarthEnd);
                            break;

                        #endregion

                        #region Chain Lightning

                        case MagicType.ChainLightning:
                            foreach (Point point in MagicLocations)
                            {
                                spell = new MirEffect(830, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 50, 80, Globals.LightningColour)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                };
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0)
                                DXSoundManager.Play(SoundIndex.ChainLightningEnd);
                            break;

                        #endregion

                        #region Asteroid

                        case MagicType.Asteroid:

                            foreach (Point point in MagicLocations)
                            {
                                MirProjectile eff;
                                Point p = new Point(point.X + 4, point.Y - 10);
                                Effects.Add(eff = new MirProjectile(1300, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx5, 50, 80, Globals.FireColour, p)
                                {
                                    MapTarget = point,
                                    Skip = 0,
                                    Explode = true,
                                    Blend = true,
                                });

                                eff.CompleteAction = () =>
                                {
                                    Effects.Add(new MirEffect(1320, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx5, 100, 100, Globals.NoneColour)
                                    {
                                        MapTarget = eff.MapTarget,
                                        Blend = true,
                                    });
                                };
                            }
                            break;

                        #endregion

                        //Meteor Shower -> Adam Fire Ball

                        //Renounce

                        //Tempest

                        //Judgement Of Heaven

                        //Thunder Strike -> Thunder Bolt

                        //Mirror Image



                        #endregion

                        #region Taoist

                        #region Heal

                        case MagicType.Heal:

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                attackTarget.Effects.Add(spell = new MirEffect(610, 10, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 10, 35, Globals.HolyColour)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });
                                spell.Process();
                            }

                            if (AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.HealEnd);
                            break;

                        #endregion

                        //SpiritSword

                        #region Poison Dust & Greater Poison Dust

                        case MagicType.PoisonDust:
                        case MagicType.GreaterPoisonDust:
                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                attackTarget.Effects.Add(spell = new MirEffect(70, 10, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 10, 35, Globals.DarkColour)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });
                                spell.Process();
                            }

                            if (AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.PoisonDustEnd);
                            break;

                        #endregion

                        #region Explosive Talisman

                        case MagicType.ExplosiveTalisman:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(980, 3, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.DarkColour, CurrentLocation)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                });
                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                Effects.Add(spell = new MirProjectile(980, 3, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.DarkColour, CurrentLocation)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });

                                spell.CompleteAction = () =>
                                {
                                    attackTarget.Effects.Add(spell = new MirEffect(1140, 10, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 20, 50, Globals.DarkColour)
                                    {
                                        Blend = true,
                                        Target = attackTarget,
                                    });
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.ExplosiveTalismanEnd);
                                };
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0 || AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.ExplosiveTalismanTravel);

                            break;

                        #endregion

                        #region Evil Slayer

                        case MagicType.EvilSlayer:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(3330, 6, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.HolyColour, CurrentLocation)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                    Skip = 0,
                                });
                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                Effects.Add(spell = new MirProjectile(3330, 6, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.HolyColour, CurrentLocation)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                    Skip = 0,
                                });

                                spell.CompleteAction = () =>
                                {
                                    attackTarget.Effects.Add(spell = new MirEffect(3340, 10, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 20, 50, Globals.HolyColour)
                                    {
                                        Blend = true,
                                        Target = attackTarget,
                                    });
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.HolyStrikeEnd);
                                };
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0 || AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.HolyStrikeTravel);

                            break;

                        #endregion

                        //SummonSkeleton

                        //Invisibility

                        #region Magic Resistance

                        case MagicType.MagicResistance:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(980, 3, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.NoneColour, CurrentLocation)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                    Explode = true,
                                });

                                spell.CompleteAction = () =>
                                {
                                    spell = new MirEffect(200, 8, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 20, 80, Globals.NoneColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                    };
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.MagicResistanceEnd);
                                };
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0)
                                DXSoundManager.Play(SoundIndex.MagicResistanceTravel);

                            break;

                        #endregion

                        #region Mass Invisibility

                        case MagicType.MassInvisibility:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(980, 3, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.PhantomColour, CurrentLocation)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                    Explode = true,
                                });

                                spell.CompleteAction = () =>
                                {
                                    spell = new MirEffect(820, 7, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 20, 80, Globals.PhantomColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                    };
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.MassInvisibilityEnd);
                                };
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0)
                                DXSoundManager.Play(SoundIndex.MassInvisibilityTravel);
                            break;

                        #endregion

                        #region Greater Evil Slayer

                        case MagicType.GreaterEvilSlayer:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(3440, 6, TimeSpan.FromMilliseconds(50), LibraryFile.Magic, 35, 35, Globals.HolyColour, CurrentLocation)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                    Skip = 0,
                                });
                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                Effects.Add(spell = new MirProjectile(3440, 6, TimeSpan.FromMilliseconds(50), LibraryFile.Magic, 35, 35, Globals.HolyColour, CurrentLocation)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                    Skip = 0,
                                });

                                spell.CompleteAction = () =>
                                {
                                    attackTarget.Effects.Add(spell = new MirEffect(3450, 10, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 20, 50, Globals.HolyColour)
                                    {
                                        Blend = true,
                                        Target = attackTarget,
                                    });
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.ImprovedHolyStrikeEnd);
                                };
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0 || AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.ImprovedHolyStrikeTravel);

                            break;

                        #endregion

                        #region Resilience

                        case MagicType.Resilience:

                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(980, 3, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.NoneColour, CurrentLocation)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                    Explode = true,
                                });

                                spell.CompleteAction = () =>
                                {
                                    spell = new MirEffect(170, 8, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 20, 80, Globals.NoneColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                    };
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.ResilienceEnd);
                                };

                                spell.Process();
                            }

                            if (MagicLocations.Count > 0)
                                DXSoundManager.Play(SoundIndex.ResilienceTravel);

                            break;

                        #endregion

                        #region Trap Octagon

                        case MagicType.TrapOctagon:
                            DXSoundManager.Play(SoundIndex.ShacklingTalismanEnd);
                            break;

                        #endregion

                        //Taoist Combat Kick

                        #region Elemental Superiority

                        case MagicType.ElementalSuperiority:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(980, 3, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.NoneColour, CurrentLocation)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                    Explode = true,
                                });

                                spell.CompleteAction = () =>
                                {
                                    spell = new MirEffect(1870, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 20, 80, Globals.NoneColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                    };
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.BloodLustTravel);
                                };
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0)
                                DXSoundManager.Play(SoundIndex.BloodLustEnd);

                            break;

                        #endregion

                        //Shinsu

                        #region Mass Heal

                        case MagicType.MassHeal:
                            foreach (Point point in MagicLocations)
                            {
                                spell = new MirEffect(670, 7, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 40, 60, Globals.HolyColour)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                };
                                spell.Process();
                            }

                            DXSoundManager.Play(SoundIndex.MassHealEnd);

                            break;

                        #endregion

                        //Summon Jin Skeleton

                        #region Blood Lust

                        case MagicType.BloodLust:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(980, 3, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 35, 35, Globals.DarkColour, CurrentLocation)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                    Explode = true,
                                });

                                spell.CompleteAction = () =>
                                {
                                    spell = new MirEffect(140, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 20, 80, Globals.DarkColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                    };
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.BloodLustEnd);
                                };
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0)
                                DXSoundManager.Play(SoundIndex.BloodLustTravel);
                            break;

                        #endregion

                        #region Resurrection

                        case MagicType.Resurrection:
                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                attackTarget.Effects.Add(spell = new MirEffect(320, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 60, 60, Globals.HolyColour)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });
                                spell.Process();
                            }

                            break;

                        #endregion

                        #region Purification

                        case MagicType.Purification:
                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                attackTarget.Effects.Add(spell = new MirEffect(230, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 20, 40, Globals.HolyColour)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });
                                spell.Process();
                            }

                            if (AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.PurificationEnd);
                            break;

                        #endregion

                        #region Strength Of Faith

                        case MagicType.StrengthOfFaith:
                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                attackTarget.Effects.Add(spell = new MirEffect(370, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 20, 40, Globals.PhantomColour)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });
                                spell.Process();
                            }

                            if (AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.StrengthOfFaithEnd);
                            break;

                        #endregion

                        //Transparency

                        #region Celestial Light

                        case MagicType.CelestialLight:
                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                attackTarget.Effects.Add(spell = new MirEffect(290, 9, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 20, 40, Globals.HolyColour)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });
                                spell.Process();
                            }
                            break;

                        #endregion

                        //Empowered Healing

                        #region LifeSteal

                        case MagicType.LifeSteal:

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                attackTarget.Effects.Add(spell = new MirEffect(2500, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 10, 35, Globals.DarkColour)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });
                                spell.Process();
                            }

                            if (AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.HolyStrikeEnd);
                            break;

                        #endregion

                        #region Improved Explosive Talisman

                        case MagicType.ImprovedExplosiveTalisman:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(980, 3, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 35, 35, Globals.DarkColour, CurrentLocation)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                    Has16Directions = false
                                });
                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                Effects.Add(spell = new MirProjectile(980, 3, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 35, 35, Globals.DarkColour, CurrentLocation)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                    Has16Directions = false
                                });

                                spell.CompleteAction = () =>
                                {
                                    attackTarget.Effects.Add(spell = new MirEffect(1160, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 20, 50, Globals.DarkColour)
                                    {
                                        Blend = true,
                                        Target = attackTarget,
                                    });
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.FireStormEnd);
                                };
                                spell.Process();
                            }

                            if (MagicLocations.Count > 0 || AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.ExplosiveTalismanTravel);

                            break;

                        #endregion

                        //Greater Poison Dust -> Poison Dust

                        //Summon Demon

                        //Scarecrow

                        //Demon Explosion

                        #region Thunder Kick

                        case MagicType.ThunderKick:
                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                attackTarget.Effects.Add(spell = new MirEffect(1190, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 20, 40, Globals.NoneColour)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });
                                spell.Process();
                            }

                            if (AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.FireStormEnd);
                            break;

                        #endregion

                        #region Infection

                        case MagicType.Infection:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(800, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx5, 35, 35, Globals.NoneColour, CurrentLocation)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                    Direction = Direction,
                                    Skip = 10
                                    //  DrawColour = Color.FromArgb(76, 34, 4),
                                });
                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                Effects.Add(spell = new MirProjectile(800, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx5, 35, 35, Globals.NoneColour, CurrentLocation)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                    Direction = Direction,
                                    Skip = 10
                                    //   DrawColour = Color.FromArgb(76, 34, 4),
                                });

                                spell.Process();
                            }

                            if (MagicLocations.Count > 0 || AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.FireBallTravel);
                            break;

                        #endregion

                        #endregion

                        #region Assassin

                        //Willow Dance

                        //Vine Tree Dance

                        //Discipline

                        //Poisonous Cloud

                        //Full Bloom

                        //Cloak

                        //White Lotus

                        //Calamity Of Full Moon

                        #region Wraith Grip

                        case MagicType.WraithGrip:

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                attackTarget.CanShowWraithGrip = false;
                                attackTarget.Effects.Add(spell = new MirEffect(1420, 14, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx4, 60, 60, Globals.NoneColour)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                    BlendRate = 0.4f,
                                });
                                spell.CompleteAction = () => attackTarget.CanShowWraithGrip = true;

                                spell.Process();

                                attackTarget.Effects.Add(spell = new MirEffect(1440, 14, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx4, 60, 60, Globals.NoneColour)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                    BlendRate = 0.4f,
                                });
                                spell.Process();
                            }

                            if (AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.WraithGripEnd);
                            break;

                        #endregion

                        #region Hell Fire

                        case MagicType.HellFire:
                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                attackTarget.Effects.Add(spell = new MirEffect(1500, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx4, 60, 60, Globals.FireColour)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                });

                                spell.Process();
                            }

                            if (AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.WraithGripEnd);
                            break;

                        #endregion

                        //Pledge Of Blood

                        //Rake

                        //Sweet Brier

                        //Summon Puppet

                        //Karma - Removed

                        //Touch Of Departed 

                        //Waning Moon

                        //Ghost Walk

                        //Elemental Puppet

                        //Rejuvenation

                        //Resolution

                        //Change Of Seasons

                        //Release

                        //Flame Splash

                        //Bloody Flower

                        //The New Beginning

                        //Dance Of Swallows

                        //Dark Conversion

                        //Dragon Repulsion

                        //Advent Of Demon

                        //Advent Of Devil

                        //Abyss

                        //Flash Of Light

                        //Stealth

                        //Evasion

                        //Raging Wind

                        #endregion

                        #region Pink Fire Ball

                        case MagicType.PinkFireBall:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(1500, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 35, 35, Color.Purple, CurrentLocation)
                                {
                                    Blend = true,
                                    Direction = action.Direction,
                                    MapTarget = point,
                                });
                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                Effects.Add(spell = new MirProjectile(1600, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 35, 35, Color.Purple, CurrentLocation)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                    Has16Directions = false,
                                });

                                spell.CompleteAction = () =>
                                {
                                    attackTarget.Effects.Add(spell = new MirEffect(1700, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 35, 35, Color.Purple)
                                    {
                                        Blend = true,
                                        Target = attackTarget,
                                        Direction = action.Direction
                                    });
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.FireBallEnd);
                                };

                                spell.Process();
                            }

                            if (MagicLocations.Count > 0 || AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.FireBallTravel);
                            break;

                        #endregion

                        #region Green Sludge Ball

                        case MagicType.GreenSludgeBall:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(spell = new MirProjectile(2600, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx23, 35, 35, Color.GreenYellow, CurrentLocation)
                                {
                                    Blend = true,
                                    Direction = action.Direction,
                                    MapTarget = point,
                                });
                                spell.Process();
                            }

                            foreach (MapObject attackTarget in AttackTargets)
                            {
                                Effects.Add(spell = new MirProjectile(2600, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx23, 35, 35, Color.GreenYellow, CurrentLocation)
                                {
                                    Blend = true,
                                    Target = attackTarget,
                                    Has16Directions = false,
                                });

                                spell.CompleteAction = () =>
                                {
                                    attackTarget.Effects.Add(spell = new MirEffect(2780, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx23, 35, 35, Color.GreenYellow)
                                    {
                                        Blend = true,
                                        Target = attackTarget,
                                        Direction = action.Direction
                                    });
                                    spell.Process();

                                    DXSoundManager.Play(SoundIndex.FireBallEnd);
                                };

                                spell.Process();
                            }

                            if (MagicLocations.Count > 0 || AttackTargets.Count > 0)
                                DXSoundManager.Play(SoundIndex.FireBallTravel);
                            break;

                        #endregion

                        #region Monster Scortched Earth

                        case MagicType.MonsterScortchedEarth:
                            if (Config.DrawEffects && Race != ObjectType.Monster)
                                foreach (Point point in MagicLocations)
                                {
                                    Effects.Add(new MirEffect(220, 1, TimeSpan.FromMilliseconds(2500), LibraryFile.ProgUse, 0, 0, Globals.NoneColour)
                                    {
                                        MapTarget = point,
                                        StartTime = CEnvir.Now.AddMilliseconds(500 + Functions.Distance(point, CurrentLocation) * 50),
                                        Opacity = 0.8F,
                                        DrawType = DrawType.Floor,
                                    });

                                    Effects.Add(new MirEffect(2450 + CEnvir.Random.Next(5) * 10, 10, TimeSpan.FromMilliseconds(250), LibraryFile.Magic, 0, 0, Globals.NoneColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                        StartTime = CEnvir.Now.AddMilliseconds(500 + Functions.Distance(point, CurrentLocation) * 50),
                                        DrawType = DrawType.Floor,
                                    });

                                    Effects.Add(new MirEffect(1930, 30, TimeSpan.FromMilliseconds(50), LibraryFile.Magic, 20, 70, Globals.FireColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                        StartTime = CEnvir.Now.AddMilliseconds(Functions.Distance(point, CurrentLocation) * 50),
                                        BlendRate = 1F,
                                    });
                                }

                            // if (MagicLocations.Count > 0)
                            //   DXSoundManager.Play(SoundIndex.LavaStrikeEnd);

                            break;

                        #endregion

                        #region MonsterIceStorm

                        case MagicType.MonsterIceStorm:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(new MirEffect(6230, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx3, 20, 70, Globals.IceColour)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                    BlendRate = 1F,
                                });
                            }
                            break;


                        case MagicType.MonsterThunderStorm:
                            foreach (Point point in MagicLocations)
                            {
                                Effects.Add(new MirEffect(650, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx5, 20, 70, Globals.LightningColour)
                                {
                                    Blend = true,
                                    MapTarget = point,
                                    BlendRate = 1F,
                                });
                            }
                            break;

                        #endregion

                        #region Sama

                        case MagicType.SamaGuardianFire:
                            if (Config.DrawEffects)
                                foreach (Point point in MagicLocations)
                                {
                                    spell = new MirEffect(4000, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx9, 30, 80, Globals.FireColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                    };
                                    spell.Process();
                                }
                            break;
                        case MagicType.SamaGuardianIce:
                            if (Config.DrawEffects)
                                foreach (Point point in MagicLocations)
                                {
                                    spell = new MirEffect(4100, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx9, 30, 80, Globals.IceColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                    };
                                    spell.Process();
                                }
                            break;
                        case MagicType.SamaGuardianLightning:
                            if (Config.DrawEffects)
                                foreach (Point point in MagicLocations)
                                {
                                    spell = new MirEffect(4200, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx9, 30, 80, Globals.LightningColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                    };
                                    spell.Process();
                                }
                            break;
                        case MagicType.SamaGuardianWind:
                            if (Config.DrawEffects)
                                foreach (Point point in MagicLocations)
                                {
                                    spell = new MirEffect(4300, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx9, 30, 80, Globals.WindColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                    };
                                    spell.Process();
                                }
                            break;

                        case MagicType.SamaPhoenixFire:
                            if (Config.DrawEffects)
                                foreach (Point point in MagicLocations)
                                {
                                    spell = new MirEffect(4500, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx9, 30, 80, Globals.FireColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                    };
                                    spell.Process();
                                }
                            break;
                        case MagicType.SamaBlackIce:
                            if (Config.DrawEffects)
                                foreach (Point point in MagicLocations)
                                {
                                    spell = new MirEffect(4600, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx9, 30, 80, Globals.IceColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                    };
                                    spell.Process();
                                }
                            break;
                        case MagicType.SamaBlueLightning:
                            if (Config.DrawEffects)
                                foreach (Point point in MagicLocations)
                                {
                                    spell = new MirEffect(4700, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx9, 30, 80, Globals.LightningColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                    };
                                    spell.Process();
                                }
                            break;
                        case MagicType.SamaWhiteWind:
                            if (Config.DrawEffects)
                                foreach (Point point in MagicLocations)
                                {
                                    spell = new MirEffect(4800, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx9, 30, 80, Globals.WindColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                    };
                                    spell.Process();
                                }
                            break;
                        case MagicType.SamaProphetFire:
                            //   foreach (Point point in MagicLocations)

                            spell = new MirEffect(5600, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx9, 30, 80, Globals.FireColour)
                            {
                                Blend = true,
                                MapTarget = CurrentLocation,
                            };
                            spell.Process();

                            break;
                        case MagicType.SamaProphetLightning:
                            //    foreach (Point point in MagicLocations)

                            spell = new MirEffect(5200, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx9, 30, 80, Globals.LightningColour)
                            {
                                Blend = true,
                                MapTarget = CurrentLocation,
                            };
                            spell.Process();

                            break;
                        case MagicType.SamaProphetWind:
                            //     foreach (Point point in MagicLocations)

                            spell = new MirEffect(5400, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx9, 30, 80, Globals.WindColour)
                            {
                                Blend = true,
                                MapTarget = CurrentLocation,
                            };
                            spell.Process();

                            break;

                        #endregion

                    }

                    break;
            }

            MoveDistance = 0;

            SetFrame(action);

            Direction = action.Direction;
            CurrentLocation = action.Location;

            AssaultEnd();
            List<uint> targets;

            if (action.Action != MirAction.Standing)
                DragonRepulseEnd();

            switch (action.Action)
            {
                case MirAction.Fishing:
                    FishingState = (FishingState)action.Extra[0];
                    FloatLocation = (Point)action.Extra[1];
                    FishFound = (bool)action.Extra[2];

                    if (FishingState == FishingState.Reel)
                        FishingState = FishingState.None;
                    break;
                case MirAction.Mining:
                    MiningEffect = (bool)action.Extra[0];
                    break;
                case MirAction.Moving:
                    MoveDistance = (int)action.Extra[0];
                    MagicType = (MagicType)action.Extra[1];

                    switch (MagicType)
                    {
                        case MagicType.Assault:
                            DXSoundManager.Play(SoundIndex.AssaultStart);
                            AssaultCreate();
                            break;
                    }

                    break;
                case MirAction.Standing:

                    if (!VisibleBuffs.Contains(BuffType.DragonRepulse))
                    {
                        if (DragonRepulseEffect != null)
                            DragonRepulseEnd();
                        break;
                    }

                    if (DragonRepulseEffect == null)
                        DragonRepulseCreate();

                    break;
                case MirAction.Pushed:
                    MoveDistance = 1;
                    break;
                case MirAction.RangeAttack:

                    targets = (List<uint>)action.Extra[0];
                    AttackTargets = new List<MapObject>();
                    foreach (uint target in targets)
                    {
                        MapObject attackTarget = GameScene.Game.MapControl.Objects.FirstOrDefault(x => x.ObjectID == target);
                        if (attackTarget == null) continue;
                        AttackTargets.Add(attackTarget);
                    }
                    break;
                /*case MirAction.Struck:
									if (VisibleBuffs.Contains(BuffType.MagicShield))
										MagicShieldStruck();

									if (VisibleBuffs.Contains(BuffType.CelestialLight))
										CelestialLightStruck();


									AttackerID = (uint)action.Extra[0];

									Element element = (Element)action.Extra[1];
									switch (element)
									{
										case Element.None:
											Effects.Add(new MirEffect(930, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.NoneColour)
											{
												Blend = true,
												Target = this,
											});
											break;
										case Element.Fire:
											Effects.Add(new MirEffect(790, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.FireColour)
											{
												Blend = true,
												Target = this,
											});
											break;
										case Element.Ice:
											Effects.Add(new MirEffect(810, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.IceColour)
											{
												Blend = true,
												Target = this,
											});
											break;
										case Element.Lightning:
											Effects.Add(new MirEffect(830, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.LightningColour)
											{
												Blend = true,
												Target = this,
											});
											break;
										case Element.Wind:
											Effects.Add(new MirEffect(850, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.WindColour)
											{
												Blend = true,
												Target = this,
											});
											break;
										case Element.Holy:
											Effects.Add(new MirEffect(870, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.HolyColour)
											{
												Blend = true,
												Target = this,
											});
											break;
										case Element.Dark:
											Effects.Add(new MirEffect(890, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.DarkColour)
											{
												Blend = true,
												Target = this,
											});
											break;
										case Element.Phantom:
											Effects.Add(new MirEffect(910, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.PhantomColour)
											{
												Blend = true,
												Target = this,
											});
											break;
									}


									break;*/
                case MirAction.Attack:
                    MagicType = (MagicType)action.Extra[1];
                    AttackElement = (Element)action.Extra[2];

                    Color attackColour = Globals.NoneColour;
                    switch (AttackElement)
                    {
                        case Element.Fire:
                            attackColour = Globals.FireColour;
                            break;
                        case Element.Ice:
                            attackColour = Globals.IceColour;
                            break;
                        case Element.Lightning:
                            attackColour = Globals.LightningColour;
                            break;
                        case Element.Wind:
                            attackColour = Globals.WindColour;
                            break;
                        case Element.Holy:
                            attackColour = Globals.HolyColour;
                            break;
                        case Element.Dark:
                            attackColour = Globals.DarkColour;
                            break;
                        case Element.Phantom:
                            attackColour = Globals.PhantomColour;
                            break;
                    }

                    switch (MagicType)
                    {
                        case MagicType.None:
                            if (Race != ObjectType.Player || CurrentAnimation != MirAnimation.Combat3 || AttackElement == Element.None) break;

                            Effects.Add(new MirEffect(1090, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 25, attackColour) //Element style?
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction,
                                DrawColour = attackColour
                            });
                            break;

                        #region Slaying

                        case MagicType.Slaying:
                            Effects.Add(new MirEffect(1350, 6, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 10, 50, attackColour) //Element style?
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction,
                                DrawColour = attackColour
                            });
                            if (Gender == MirGender.Male)
                                DXSoundManager.Play(SoundIndex.SlayingMale);
                            else
                                DXSoundManager.Play(SoundIndex.SlayingFemale);
                            break;

                        #endregion

                        #region Thrusting

                        case MagicType.Thrusting:
                            Effects.Add(new MirEffect(0, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx3, 20, 70, attackColour) //Element style?
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction,
                                DrawColour = attackColour,
                            });
                            DXSoundManager.Play(SoundIndex.EnergyBlast);
                            break;

                        #endregion

                        #region Half Moon

                        case MagicType.HalfMoon:

                            Effects.Add(new MirEffect(230, 6, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 20, 70, attackColour) //Element style?
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction,
                                DrawColour = attackColour
                            });
                            DXSoundManager.Play(SoundIndex.HalfMoon);
                            break;

                        #endregion

                        #region Destructive Surge

                        case MagicType.DestructiveSurge:

                            Effects.Add(new MirEffect(1420, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 20, 70, attackColour) //Element style?
                            {
                                Blend = true,
                                Target = this,
                                DrawColour = attackColour
                            });
                            DXSoundManager.Play(SoundIndex.DestructiveBlow);
                            break;

                        #endregion

                        #region Flaming Sword

                        case MagicType.FlamingSword:
                            Effects.Add(new MirEffect(1470, 6, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 10, 50, Globals.FireColour) //Element style?
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction,
                            });

                            DXSoundManager.Play(SoundIndex.FlamingSword);
                            break;

                        #endregion

                        #region Dragon Rise

                        case MagicType.DragonRise:
                            Effects.Add(new MirEffect(2180, 10, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 20, 70, attackColour) //Element style?
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction,
                                DrawColour = attackColour,
                                //StartTime = CEnvir.Now.AddMilliseconds(500)
                            });

                            DXSoundManager.Play(SoundIndex.DragonRise);
                            break;

                        #endregion

                        #region Blade Storm

                        case MagicType.BladeStorm:
                            Effects.Add(new MirEffect(1780, 10, TimeSpan.FromMilliseconds(60), LibraryFile.MagicEx, 20, 70, attackColour) //Element style?
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction,
                                DrawColour = attackColour,
                            });

                            DXSoundManager.Play(SoundIndex.BladeStorm);
                            break;

                        #endregion

                        #region Flame Splash

                        case MagicType.FlameSplash:
                            Effects.Add(new MirEffect(900, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx4, 20, 70, Globals.FireColour) //Element style?
                            {
                                Blend = true,
                                Target = this,
                            });

                            DXSoundManager.Play(SoundIndex.BladeStorm);
                            break;

                        #endregion

                        #region Dance Of Swallow

                        case MagicType.DanceOfSwallow:
                            break;

                            #endregion

                    }
                    break;
                case MirAction.Spell:
                    MagicType = (MagicType)action.Extra[0];

                    targets = (List<uint>)action.Extra[1];
                    AttackTargets = new List<MapObject>();
                    foreach (uint target in targets)
                    {
                        MapObject attackTarget = GameScene.Game.MapControl.Objects.FirstOrDefault(x => x.ObjectID == target);
                        if (attackTarget == null) continue;
                        AttackTargets.Add(attackTarget);
                    }
                    MagicLocations = (List<Point>)action.Extra[2];
                    MagicCast = (bool)action.Extra[3];

                    Point location;
                    switch (MagicType)
                    {

                        #region Warrior
                        //Swordsmanship

                        //Potion Mastery 

                        //Slaying

                        //Thrusting

                        //Half Moon

                        //Shoulder Dash

                        //Flaming Sword

                        //Dragon Rise

                        //Blade Storm

                        //Destructive Surge

                        #region Interchange

                        case MagicType.Interchange:
                            Effects.Add(new MirEffect(0, 9, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 60, 60, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.TeleportationStart);
                            break;

                        #endregion

                        #region Defiance

                        case MagicType.Defiance:
                            Effects.Add(new MirEffect(40, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 60, 60, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.DefianceStart);
                            break;

                        #endregion

                        #region Beckon

                        case MagicType.Beckon:
                        case MagicType.MassBeckon:
                            Effects.Add(new MirEffect(580, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 60, 60, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });
                            DXSoundManager.Play(SoundIndex.TeleportationStart);
                            break;

                        #endregion

                        #region Might

                        case MagicType.Might:
                            Effects.Add(new MirEffect(60, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 60, 60, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.DragonRise); //Same file as Beckon
                            break;

                        #endregion

                        #region Lightning Beam

                        case MagicType.SeismicSlam:
                            Effects.Add(spell = new MirEffect(4900, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx5, 10, 35, Globals.LightningColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction,
                            });
                            break;

                        #endregion

                        //Swift Blade

                        //Assault - will be passive?

                        //Endurance

                        #region Reflect Damage

                        case MagicType.ReflectDamage:
                            Effects.Add(new MirEffect(1220, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 60, 60, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = this
                            });
                            DXSoundManager.Play(SoundIndex.DefianceStart);
                            break;

                        #endregion

                        #region Fetter

                        case MagicType.Fetter:
                            Effects.Add(new MirEffect(2370, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 60, 60, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = this
                            });
                            break;

                        #endregion

                        #endregion

                        #region Wizard

                        #region Fire Ball

                        case MagicType.FireBall:
                            Effects.Add(spell = new MirEffect(1820, 8, TimeSpan.FromMilliseconds(70), LibraryFile.Magic, 10, 35, Globals.FireColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });

                            DXSoundManager.Play(SoundIndex.FireBallStart);
                            break;

                        #endregion

                        #region Lightning Ball

                        case MagicType.LightningBall:
                            Effects.Add(spell = new MirEffect(2990, 6, TimeSpan.FromMilliseconds(80), LibraryFile.Magic, 10, 35, Globals.LightningColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });
                            DXSoundManager.Play(SoundIndex.ThunderBoltStart);
                            break;

                        #endregion

                        #region Ice Bolt

                        case MagicType.IceBolt:
                            Effects.Add(spell = new MirEffect(2620, 6, TimeSpan.FromMilliseconds(80), LibraryFile.Magic, 10, 35, Globals.IceColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });

                            DXSoundManager.Play(SoundIndex.IceBoltStart);
                            break;

                        #endregion

                        #region Gust Blast

                        case MagicType.GustBlast:
                            Effects.Add(spell = new MirEffect(350, 7, TimeSpan.FromMilliseconds(50), LibraryFile.MagicEx, 10, 35, Globals.WindColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });
                            DXSoundManager.Play(SoundIndex.GustBlastStart);
                            break;

                        #endregion

                        #region Repulsion

                        case MagicType.Repulsion:
                            Effects.Add(new MirEffect(90, 10, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 10, 35, Globals.WindColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.RepulsionEnd);
                            break;

                        #endregion

                        #region Electric Shock

                        case MagicType.ElectricShock:
                            Effects.Add(spell = new MirEffect(0, 10, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.LightningColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.ElectricShockStart);
                            break;

                        #endregion

                        #region Teleportation

                        case MagicType.Teleportation:
                            Effects.Add(new MirEffect(110, 10, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.PhantomColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.TeleportationStart);
                            break;

                        #endregion

                        #region AdamantineFireBall & MeteorShower & FireBounce

                        case MagicType.AdamantineFireBall:
                        case MagicType.MeteorShower:
                        case MagicType.FireBounce:
                            Effects.Add(spell = new MirEffect(1560, 9, TimeSpan.FromMilliseconds(65), LibraryFile.Magic, 10, 35, Globals.FireColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });

                            DXSoundManager.Play(SoundIndex.GreaterFireBallStart);
                            break;

                        #endregion

                        #region Thunder Bolt

                        case MagicType.ThunderBolt:
                            Effects.Add(spell = new MirEffect(1430, 12, TimeSpan.FromMilliseconds(50), LibraryFile.Magic, 10, 35, Globals.LightningColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.LightningStrikeStart);
                            break;

                        #endregion

                        #region Ice Blades

                        case MagicType.IceBlades:
                            Effects.Add(spell = new MirEffect(2880, 6, TimeSpan.FromMilliseconds(115), LibraryFile.Magic, 10, 35, Globals.IceColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });

                            DXSoundManager.Play(SoundIndex.GreaterIceBoltStart);
                            break;

                        #endregion

                        #region Cyclone

                        case MagicType.Cyclone:
                            Effects.Add(spell = new MirEffect(1970, 10, TimeSpan.FromMilliseconds(60), LibraryFile.MagicEx, 10, 35, Globals.WindColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.CycloneStart);
                            break;

                        #endregion

                        #region Scortched Earth

                        case MagicType.ScortchedEarth:
                            if (Config.DrawEffects && Race != ObjectType.Monster)
                            {
                                Effects.Add(spell = new MirEffect(1820, 8, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.FireColour)
                                {
                                    Blend = true,
                                    Target = this,
                                    Direction = action.Direction,
                                });
                                DXSoundManager.Play(SoundIndex.LavaStrikeStart);
                            }

                            break;

                        #endregion

                        #region Lightning Beam

                        case MagicType.LightningBeam:
                            Effects.Add(spell = new MirEffect(1970, 10, TimeSpan.FromMilliseconds(30), LibraryFile.Magic, 10, 35, Globals.LightningColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });
                            break;

                        #endregion

                        #region Frozen Earth

                        case MagicType.FrozenEarth:
                            Effects.Add(spell = new MirEffect(0, 10, TimeSpan.FromMilliseconds(50), LibraryFile.MagicEx, 10, 35, Globals.IceColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });
                            DXSoundManager.Play(SoundIndex.FrozenEarthStart);
                            break;

                        #endregion

                        #region Blow Earth

                        case MagicType.BlowEarth:
                            Effects.Add(spell = new MirEffect(1970, 10, TimeSpan.FromMilliseconds(60), LibraryFile.MagicEx, 10, 35, Globals.WindColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.BlowEarthStart);
                            break;

                        #endregion

                        #region Fire Wall

                        case MagicType.FireWall:
                            Effects.Add(new MirEffect(910, 10, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.FireColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.FireWallStart);
                            break;

                        #endregion

                        #region Expel Undead

                        case MagicType.ExpelUndead:
                            Effects.Add(spell = new MirEffect(130, 10, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.PhantomColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.ExpelUndeadStart);
                            break;

                        #endregion

                        #region GeoManipulation

                        case MagicType.GeoManipulation:
                            Effects.Add(new MirEffect(110, 10, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.PhantomColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.TeleportationStart);
                            break;

                        #endregion

                        #region Magic Shield

                        case MagicType.MagicShield:
                            Effects.Add(new MirEffect(830, 19, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.PhantomColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.MagicShieldStart);
                            break;

                        #endregion

                        #region Fire Storm

                        case MagicType.FireStorm:
                            Effects.Add(spell = new MirEffect(940, 10, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.FireColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.FireStormStart);
                            break;

                        #endregion

                        #region Lightning Wave

                        case MagicType.LightningWave:
                            Effects.Add(spell = new MirEffect(1430, 12, TimeSpan.FromMilliseconds(50), LibraryFile.Magic, 10, 35, Globals.LightningColour)
                            {
                                Blend = true,
                                Target = this
                            });
                            DXSoundManager.Play(SoundIndex.LightningWaveStart);
                            break;

                        #endregion

                        #region Ice Storm

                        case MagicType.IceStorm:
                            Effects.Add(spell = new MirEffect(770, 10, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.IceColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.IceStormStart);
                            break;

                        #endregion

                        #region DragonTornado

                        case MagicType.DragonTornado:
                            Effects.Add(spell = new MirEffect(1030, 10, TimeSpan.FromMilliseconds(60), LibraryFile.MagicEx, 10, 35, Globals.WindColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.DragonTornadoStart);
                            break;

                        #endregion

                        #region Greater Frozen Earth

                        case MagicType.GreaterFrozenEarth:
                            Effects.Add(spell = new MirEffect(0, 10, TimeSpan.FromMilliseconds(50), LibraryFile.MagicEx, 10, 35, Globals.IceColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });
                            DXSoundManager.Play(SoundIndex.GreaterFrozenEarthStart);
                            break;

                        #endregion

                        #region Chain Lightning

                        case MagicType.ChainLightning:
                            Effects.Add(spell = new MirEffect(1430, 12, TimeSpan.FromMilliseconds(50), LibraryFile.Magic, 10, 35, Globals.LightningColour)
                            {
                                Blend = true,
                                Target = this
                            });
                            DXSoundManager.Play(SoundIndex.ChainLightningStart);
                            break;

                        #endregion

                        //Meteor Strike -> Great Fire Ball

                        #region Renounce

                        case MagicType.Renounce:
                            Effects.Add(new MirEffect(80, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 10, 35, Globals.PhantomColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.DefianceStart);
                            break;

                        #endregion

                        #region Tempest

                        case MagicType.Tempest:
                            Effects.Add(new MirEffect(910, 10, TimeSpan.FromMilliseconds(60), LibraryFile.MagicEx2, 10, 35, Globals.WindColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.BlowEarthStart);
                            break;

                        #endregion

                        #region Judgement Of Heaven

                        case MagicType.JudgementOfHeaven:
                            DXSoundManager.Play(SoundIndex.LightningStrikeEnd);
                            break;

                        #endregion

                        #region Thunder Strike

                        case MagicType.ThunderStrike:
                            DXSoundManager.Play(SoundIndex.LightningStrikeStart);
                            break;

                        #endregion

                        #region Mirror Image

                        case MagicType.MirrorImage:
                            DXSoundManager.Play(SoundIndex.ShacklingTalismanStart);
                            break;

                        #endregion

                        #region Frost Bite

                        case MagicType.FrostBite:
                            Effects.Add(new MirEffect(500, 16, TimeSpan.FromMilliseconds(60), LibraryFile.MagicEx5, 10, 35, Globals.IceColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.FrostBiteStart);
                            break;

                        #endregion


                        //Ice Sonic


                        #endregion

                        #region Taoist

                        #region Heal

                        case MagicType.Heal:
                            Effects.Add(spell = new MirEffect(660, 10, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.HolyColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.HealStart);
                            break;

                        #endregion

                        //Spirit Sword

                        #region Poison Dust

                        case MagicType.PoisonDust:
                            Effects.Add(spell = new MirEffect(60, 10, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.PoisonDustStart);
                            break;

                        #endregion

                        #region Explosive Talisman

                        case MagicType.ExplosiveTalisman:
                            Effects.Add(spell = new MirEffect(2080, 6, TimeSpan.FromMilliseconds(80), LibraryFile.Magic, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });

                            DXSoundManager.Play(SoundIndex.ExplosiveTalismanStart);
                            break;

                        #endregion

                        #region Evil Slayer

                        case MagicType.EvilSlayer:
                            Effects.Add(spell = new MirEffect(3250, 6, TimeSpan.FromMilliseconds(80), LibraryFile.Magic, 10, 35, Globals.HolyColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });

                            DXSoundManager.Play(SoundIndex.HolyStrikeStart);
                            break;

                        #endregion

                        #region Summon Skeleton & Summon Jin Skeleton

                        case MagicType.SummonSkeleton:
                        case MagicType.SummonJinSkeleton:
                        case MagicType.Scarecrow:
                            Effects.Add(new MirEffect(740, 10, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.PhantomColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.SummonSkeletonStart);
                            break;

                        #endregion

                        #region Invisibility

                        case MagicType.Invisibility:
                            Effects.Add(new MirEffect(810, 10, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.PhantomColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.InvisibilityEnd);
                            break;

                        #endregion

                        #region Magic Resistance

                        case MagicType.MagicResistance:
                            Effects.Add(spell = new MirEffect(2080, 6, TimeSpan.FromMilliseconds(80), LibraryFile.Magic, 10, 35, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });
                            break;

                        #endregion

                        #region Mass Invisibility

                        case MagicType.MassInvisibility:
                            Effects.Add(spell = new MirEffect(2080, 6, TimeSpan.FromMilliseconds(80), LibraryFile.Magic, 10, 35, Globals.PhantomColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });

                            break;

                        #endregion

                        #region Greater Evil Slayer

                        case MagicType.GreaterEvilSlayer:
                            Effects.Add(spell = new MirEffect(3360, 6, TimeSpan.FromMilliseconds(80), LibraryFile.Magic, 10, 35, Globals.HolyColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });

                            DXSoundManager.Play(SoundIndex.ImprovedHolyStrikeStart);
                            break;

                        #endregion

                        #region Resilience

                        case MagicType.Resilience:
                            Effects.Add(spell = new MirEffect(2080, 6, TimeSpan.FromMilliseconds(80), LibraryFile.Magic, 10, 35, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });
                            break;

                        #endregion

                        #region Trap Octagon

                        case MagicType.TrapOctagon:
                            Effects.Add(spell = new MirEffect(630, 10, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                            });

                            DXSoundManager.Play(SoundIndex.ShacklingTalismanStart);
                            break;

                        #endregion

                        #region Taoist Combat Kick

                        case MagicType.TaoistCombatKick:
                            DXSoundManager.Play(SoundIndex.TaoistCombatKickStart);
                            break;

                        #endregion

                        #region Elemental Superiority

                        case MagicType.ElementalSuperiority:
                            Effects.Add(spell = new MirEffect(2080, 6, TimeSpan.FromMilliseconds(80), LibraryFile.Magic, 10, 35, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });

                            break;

                        #endregion

                        #region Summon Shinsu
                        case MagicType.SummonShinsu:
                            Effects.Add(new MirEffect(2590, 19, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.PhantomColour)
                            {
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.SummonShinsuStart);
                            break;
                        #endregion

                        #region Mass Heal

                        case MagicType.MassHeal:
                            Effects.Add(spell = new MirEffect(660, 10, TimeSpan.FromMilliseconds(60), LibraryFile.Magic, 10, 35, Globals.HolyColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.MassHealStart);
                            break;

                        #endregion

                        //Summon Jin Skeleton -> Summon Skeleton

                        #region Blood Lust

                        case MagicType.BloodLust:
                            Effects.Add(spell = new MirEffect(2080, 6, TimeSpan.FromMilliseconds(80), LibraryFile.Magic, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });
                            break;

                        #endregion

                        #region Resurrection

                        case MagicType.Resurrection:
                            Effects.Add(spell = new MirEffect(310, 10, TimeSpan.FromMilliseconds(60), LibraryFile.MagicEx, 60, 60, Globals.HolyColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.ResurrectionStart);
                            break;

                        #endregion

                        #region Purification

                        case MagicType.Purification:
                            Effects.Add(spell = new MirEffect(220, 10, TimeSpan.FromMilliseconds(60), LibraryFile.MagicEx2, 20, 40, Globals.HolyColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.PurificationStart);
                            break;

                        #endregion

                        #region Strength Of Faith

                        case MagicType.StrengthOfFaith:
                            Effects.Add(spell = new MirEffect(360, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 20, 40, Globals.PhantomColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.StrengthOfFaithStart);
                            break;

                        #endregion

                        #region Transparency

                        case MagicType.Transparency:
                            Effects.Add(new MirEffect(430, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 10, 35, Globals.PhantomColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.InvisibilityEnd);
                            break;

                        #endregion

                        #region Celestial Light

                        case MagicType.CelestialLight:
                            Effects.Add(new MirEffect(280, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 10, 35, Globals.HolyColour)
                            {
                                Blend = true,
                                Target = this,
                                DrawColour = Color.Yellow,
                            });
                            DXSoundManager.Play(SoundIndex.MagicShieldStart);
                            break;

                        #endregion

                        #region Life Steal

                        case MagicType.LifeSteal:
                            Effects.Add(new MirEffect(2410, 9, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction,
                            });
                            DXSoundManager.Play(SoundIndex.HolyStrikeStart);
                            break;

                        #endregion

                        #region Improved Explosive Talisman

                        case MagicType.ImprovedExplosiveTalisman:
                            Effects.Add(spell = new MirEffect(980, 6, TimeSpan.FromMilliseconds(80), LibraryFile.MagicEx2, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction
                            });

                            DXSoundManager.Play(SoundIndex.ExplosiveTalismanStart);
                            break;

                        #endregion

                        //Greater Poison Dust -> Poison Dust

                        //Scarecrow -> Summon Skeleton

                        #region Thunder Kick

                        case MagicType.ThunderKick:
                            DXSoundManager.Play(SoundIndex.TaoistCombatKickStart);
                            break;

                        #endregion

                        #endregion

                        #region Assassin

                        //Willow Dance

                        //Vine Tree Dance

                        //Discipline

                        //Poisonous Cloud

                        //Full Bloom

                        #region Cloak

                        case MagicType.Cloak:
                            Effects.Add(new MirEffect(600, 10, TimeSpan.FromMilliseconds(60), LibraryFile.MagicEx4, 10, 35, Globals.PhantomColour)
                            {
                                Blend = true,
                                MapTarget = CurrentLocation,
                            });
                            DXSoundManager.Play(SoundIndex.CloakStart);
                            break;

                        #endregion

                        //White Lotus

                        //Calamity Of Full Moon

                        #region Wraith Grip

                        case MagicType.WraithGrip:
                            Effects.Add(spell = new MirEffect(1460, 15, TimeSpan.FromMilliseconds(60), LibraryFile.MagicEx4, 60, 60, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = this,
                                BlendRate = 0.4f,
                            });
                            DXSoundManager.Play(SoundIndex.WraithGripStart);
                            break;

                        #endregion

                        #region Hell Fire

                        case MagicType.HellFire:
                            Effects.Add(spell = new MirEffect(1520, 15, TimeSpan.FromMilliseconds(60), LibraryFile.MagicEx4, 60, 60, Globals.FireColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            DXSoundManager.Play(SoundIndex.WraithGripStart);
                            break;

                        #endregion

                        //Pledge Of Blood

                        #region Rake

                        case MagicType.Rake:
                            Effects.Add(spell = new MirEffect(1200, 9, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx4, 60, 60, Globals.IceColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = action.Direction,
                                Skip = 10,
                            });
                            DXSoundManager.Play(SoundIndex.RakeStart);
                            break;

                        #endregion

                        //Sweet Brier

                        #region Summon Puppet

                        case MagicType.SummonPuppet:
                            Effects.Add(new MirEffect(800, 16, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx4, 80, 50, Globals.PhantomColour)
                            {
                                Blend = true,
                                MapTarget = CurrentLocation,
                            });
                            DXSoundManager.Play(SoundIndex.SummonPuppet);
                            break;

                        #endregion

                        //Karma - Removed

                        //Touch Of Departed 

                        //Waning Moon

                        //Ghost Walk

                        //Elemental Puppet

                        //Rejuvenation

                        //Resolution

                        //Change Of Seasons

                        //Release

                        //Flame Splash

                        //Bloody Flower

                        #region The New Beginning

                        case MagicType.TheNewBeginning:
                            Effects.Add(spell = new MirEffect(2200, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx4, 60, 60, Globals.NoneColour)
                            {
                                Blend = true,
                                MapTarget = CurrentLocation              
                            }) ;
                            DXSoundManager.Play(SoundIndex.TheNewBeginning);
                            break;

                        #endregion

                        //Dance Of Swallows

                        //Dark Conversion

                        #region Dragon Repulse

                        case MagicType.DragonRepulse:
                            Effects.Add(new MirEffect(1000, 10, TimeSpan.FromMilliseconds(60), LibraryFile.MagicEx4, 0, 0, Globals.NoneColour)
                            {
                                MapTarget = CurrentLocation,
                            });
                            Effects.Add(new MirEffect(1020, 10, TimeSpan.FromMilliseconds(60), LibraryFile.MagicEx4, 80, 50, Globals.LightningColour)
                            {
                                Blend = true,
                                MapTarget = CurrentLocation,
                            });
                            DXSoundManager.Play(SoundIndex.DragonRepulseStart);
                            break;

                        #endregion

                        //Advent Of Demon

                        //Advent Of Devil

                        #region Abyss

                        case MagicType.Abyss:
                            Effects.Add(new MirEffect(2000, 14, TimeSpan.FromMilliseconds(70), LibraryFile.MagicEx4, 80, 50, Globals.PhantomColour)
                            {
                                Blend = true,
                                MapTarget = CurrentLocation,
                            });
                            DXSoundManager.Play(SoundIndex.AbyssStart);
                            break;

                        #endregion

                        #region Flash Of Light

                        case MagicType.FlashOfLight:
                            break;

                        #endregion

                        //Stealth

                        #region Evasion

                        case MagicType.Evasion:
                            Effects.Add(new MirEffect(2500, 12, TimeSpan.FromMilliseconds(70), LibraryFile.MagicEx4, 80, 50, Globals.NoneColour)
                            {
                                Blend = true,
                                MapTarget = CurrentLocation,
                            });
                            DXSoundManager.Play(SoundIndex.EvasionStart);
                            break;

                        #endregion

                        #region RagingWind

                        case MagicType.RagingWind:
                            Effects.Add(new MirEffect(2600, 12, TimeSpan.FromMilliseconds(70), LibraryFile.MagicEx4, 80, 50, Globals.NoneColour)
                            {
                                Blend = true,
                                MapTarget = CurrentLocation,
                            });
                            DXSoundManager.Play(SoundIndex.RagingWindStart);
                            break;

                        #endregion

                        #endregion

                        #region Monster Scortched Earth

                        case MagicType.MonsterScortchedEarth:

                            location = CurrentLocation;

                            if (Config.DrawEffects && Race != ObjectType.Monster)
                                foreach (Point point in MagicLocations)
                                {
                                    Effects.Add(new MirEffect(220, 1, TimeSpan.FromMilliseconds(2500), LibraryFile.ProgUse, 0, 0, Globals.NoneColour)
                                    {
                                        MapTarget = point,
                                        StartTime = CEnvir.Now.AddMilliseconds(500 + Functions.Distance(point, location) * 50),
                                        Opacity = 0.8F,
                                        DrawType = DrawType.Floor,
                                    });

                                    Effects.Add(new MirEffect(2450 + CEnvir.Random.Next(5) * 10, 10, TimeSpan.FromMilliseconds(250), LibraryFile.Magic, 0, 0, Globals.NoneColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                        StartTime = CEnvir.Now.AddMilliseconds(500 + Functions.Distance(point, location) * 50),
                                        DrawType = DrawType.Floor,
                                    });

                                    Effects.Add(new MirEffect(1930, 30, TimeSpan.FromMilliseconds(50), LibraryFile.Magic, 20, 70, Globals.FireColour)
                                    {
                                        Blend = true,
                                        MapTarget = point,
                                        StartTime = CEnvir.Now.AddMilliseconds(Functions.Distance(point, location) * 50),
                                        BlendRate = 1F,
                                    });
                                }

                            // if (MagicLocations.Count > 0)
                            //   DXSoundManager.Play(SoundIndex.LavaStrikeEnd);

                            break;
                        case MagicType.DoomClawRightPinch:

                            spell = new MirEffect(2640, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx19, 0, 0, Globals.NoneColour)
                            {
                                Blend = true,
                                MapTarget = CurrentLocation,
                            };
                            spell.Process();

                            spell.CompleteAction = () =>
                            {
                                spell = new MirEffect(2680, 9, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx19, 0, 0, Globals.NoneColour)
                                {
                                    Blend = true,
                                    MapTarget = Functions.Move(Functions.Move(CurrentLocation, MirDirection.Down, 0), MirDirection.Right, 5),
                                };
                                spell.Process();
                            };

                            break;
                        case MagicType.DoomClawLeftPinch:

                            spell = new MirEffect(2660, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx19, 0, 0, Globals.NoneColour)
                            {
                                Blend = true,
                                MapTarget = CurrentLocation,
                            };
                            spell.Process();

                            spell.CompleteAction = () =>
                            {
                                spell = new MirEffect(2680, 9, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx19, 0, 0, Globals.NoneColour)
                                {
                                    Blend = true,
                                    MapTarget = Functions.Move(CurrentLocation, MirDirection.Right, 5),
                                };
                                spell.Process();
                            };
                            break;
                        case MagicType.DoomClawRightSwipe:

                            spell = new MirEffect(2700, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx19, 0, 0, Globals.NoneColour)
                            {
                                Blend = true,
                                MapTarget = CurrentLocation,
                            };
                            spell.Process();
                            break;
                        case MagicType.DoomClawLeftSwipe:

                            spell = new MirEffect(2720, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx19, 0, 0, Globals.NoneColour)
                            {
                                Blend = true,
                                MapTarget = CurrentLocation,
                            };
                            spell.Process();
                            break;
                        case MagicType.DoomClawSpit:
                            foreach (Point point in MagicLocations)
                            {
                                MirProjectile eff;
                                Point p = new Point(point.X, point.Y - 10);
                                Effects.Add(eff = new MirProjectile(2500, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx19, 0, 0, Globals.NoneColour, p)
                                {
                                    MapTarget = point,
                                    Skip = 0,
                                    Explode = true,
                                    Blend = true,
                                });

                                eff.CompleteAction = () =>
                                {
                                    Effects.Add(new MirEffect(2520, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx19, 0, 0, Globals.NoneColour)
                                    {
                                        MapTarget = eff.MapTarget,
                                        Blend = true,
                                    });
                                };

                            }
                            break;

                            #endregion


                    }


                    break;
            }
        }

        public virtual void DoNextAction()
        {
            if (ActionQueue.Count == 0)
            {
                switch (CurrentAction)
                {
                    //Die, Attack,..
                    case MirAction.Die:
                    case MirAction.Dead:
                        ActionQueue.Add(new ObjectAction(MirAction.Dead, Direction, CurrentLocation));
                        break;
                    default:
                        ActionQueue.Add(new ObjectAction(MirAction.Standing, Direction, CurrentLocation));
                        break;
                }
            }

            switch (ActionQueue[0].Action)
            {
                case MirAction.Moving:
                // case MirAction.DashL:
                // case MirAction.DashR:
                case MirAction.Pushed:
                    if (!GameScene.Game.MoveFrame) return;
                    break;
            }

            SetAction(ActionQueue[0]);
            ActionQueue.RemoveAt(0);
        }


        public virtual void DrawFrameChanged()
        {
            GameScene.Game.MapControl.TextureValid = false;

        }
        public virtual void FrameIndexChanged()
        {
            switch (CurrentAction)
            {
                case MirAction.Attack:
                    if (FrameIndex != 1) return;
                    PlayAttackSound();
                    break;
                case MirAction.RangeAttack:
                    if (FrameIndex != 4) return;
                    CreateProjectile();
                    PlayAttackSound();
                    break;
                /*  case MirAction.Struck:
					  if (FrameIndex == 0)
						  PlayStruckSound();
					  break;*/
                case MirAction.Die:
                    if (FrameIndex == 0)
                        PlayDieSound();
                    break;
            }
        }

        public virtual void CreateProjectile()
        {
        }
        public virtual void MovingOffSetChanged()
        {
            GameScene.Game.MapControl.TextureValid = false;
        }
        public virtual void LocationChanged()
        {
            if (CurrentCell == null) return;

            CurrentCell.RemoveObject(this);

            if (CurrentLocation.X < GameScene.Game.MapControl.Width && CurrentLocation.Y < GameScene.Game.MapControl.Height)
                GameScene.Game.MapControl.Cells[CurrentLocation.X, CurrentLocation.Y].AddObject(this);

        }
        public virtual void DeadChanged()
        {
            ;//      GameScene.Game.BigMapBox.Update(this);
            ;//      GameScene.Game.MiniMapBox.Update(this);
        }

        public void Struck(uint attackerID, Element element)
        {
            AttackerID = attackerID;

            PlayStruckSound();

            if (VisibleBuffs.Contains(BuffType.MagicShield))
                MagicShieldStruck();

            if (VisibleBuffs.Contains(BuffType.CelestialLight))
                CelestialLightStruck();

            switch (element)
            {
                case Element.None:
                    Effects.Add(new MirEffect(930, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.NoneColour)
                    {
                        Blend = true,
                        Target = this,
                    });
                    break;
                case Element.Fire:
                    Effects.Add(new MirEffect(790, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.FireColour)
                    {
                        Blend = true,
                        Target = this,
                    });
                    break;
                case Element.Ice:
                    Effects.Add(new MirEffect(810, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.IceColour)
                    {
                        Blend = true,
                        Target = this,
                    });
                    break;
                case Element.Lightning:
                    Effects.Add(new MirEffect(830, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.LightningColour)
                    {
                        Blend = true,
                        Target = this,
                    });
                    break;
                case Element.Wind:
                    Effects.Add(new MirEffect(850, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.WindColour)
                    {
                        Blend = true,
                        Target = this,
                    });
                    break;
                case Element.Holy:
                    Effects.Add(new MirEffect(870, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.HolyColour)
                    {
                        Blend = true,
                        Target = this,
                    });
                    break;
                case Element.Dark:
                    Effects.Add(new MirEffect(890, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.DarkColour)
                    {
                        Blend = true,
                        Target = this,
                    });
                    break;
                case Element.Phantom:
                    Effects.Add(new MirEffect(910, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx, 10, 30, Globals.PhantomColour)
                    {
                        Blend = true,
                        Target = this,
                    });
                    break;
            }
        }

        public virtual void Draw()
        {



        }
        public virtual void DrawBlend()
        {

        }

        public void Chat(string text)
        {
            const int chatWidth = 200;

            Color colour = Dead ? Color.Gray : Color.White;
            ChatLabel = ChatLabels.FirstOrDefault(x => x.Text == text && x.ForeColour == colour);

            ChatTime = CEnvir.Now.AddSeconds(5);

            if (ChatLabel != null) return;

            ChatLabel = new DXLabel
            {
                AutoSize = false,
                Outline = true,
                OutlineColour = Color.Black,
                ForeColour = colour,
                Text = text,
                IsVisible = true,
                DrawFormat = TextFormatFlags.WordBreak | TextFormatFlags.WordEllipsis,
            };
            ChatLabel.Size = DXLabel.GetHeight(ChatLabel, chatWidth);
            ChatLabel.Disposing += (o, e) => ChatLabels.Remove(ChatLabel);
            ChatLabels.Add(ChatLabel);

        }

        public virtual void NameChanged()
        {


            if (Race is ObjectType.Player && Caption is not null)
            {
                CaptionLabel = new DXLabel
                {
                    BackColour = Color.Empty,
                    ForeColour = NameColour,
                    Outline = true,
                    OutlineColour = Color.Black,
                    Text = Caption,
                    IsControl = false,
                    IsVisible = true,

                };
            }
                if (string.IsNullOrEmpty(Name))
            {
                NameLabel = null;
            }
            else
            {
                if (!NameLabels.TryGetValue(Name, out List<DXLabel> names))
                    NameLabels[Name] = names = new List<DXLabel>();

                NameLabel = names.FirstOrDefault(x => x.ForeColour == NameColour && x.BackColour == Color.Empty);

                if (NameLabel == null)
                {
                    NameLabel = new DXLabel
                    {
                        BackColour = Color.Empty,
                        ForeColour = NameColour,
                        Outline = true,
                        OutlineColour = Color.Black,
                        Text = Name,
                        IsControl = false,
                        IsVisible = true,
                    };

                    NameLabel.Disposing += (o, e) => names.Remove(NameLabel);
                    names.Add(NameLabel);
                }
            }

            if (string.IsNullOrEmpty(Title))
            {
                TitleNameLabel = null;
            }
            else
            {
                string title = Title;

                if (Race == ObjectType.Player)
                {
                    foreach (KeyValuePair<CastleInfo, string> pair in GameScene.Game.CastleOwners)
                    {
                        if (pair.Value != Title) continue;

                        title += $" ({pair.Key.Name})";
                    }
                }

                if (!NameLabels.TryGetValue(title, out List<DXLabel> titles))
                    NameLabels[title] = titles = new List<DXLabel>();

                TitleNameLabel = titles.FirstOrDefault(x => x.ForeColour == NameColour && x.BackColour == Color.Empty);

                if (TitleNameLabel != null) return;

                TitleNameLabel = new DXLabel
                {
                    BackColour = Color.Empty,
                    ForeColour = Race != ObjectType.Player ? Color.Orange : NameColour,
                    Outline = true,
                    OutlineColour = Color.Black,
                    Text = title,
                    IsControl = false,
                    IsVisible = true,
                };

                TitleNameLabel.Disposing += (o, e) => titles.Remove(TitleNameLabel);
                titles.Add(TitleNameLabel);
            }
        }
        public virtual void DrawName()
        {
            if (NameLabel != null)
            {
                int x = DrawX + (48 - NameLabel.Size.Width) / 2;
                int y = DrawY - (32 - NameLabel.Size.Height) / 2;

                if (Dead)
                    y += 21;
                else
                    y -= 6;

                if (TitleNameLabel != null)
                    y -= 13;

                NameLabel.Location = new Point(x, y);
                NameLabel.ForeColour = NameColour;
                if (Config.HighlightedItems != string.Empty)
                {
                    string[] items = Config.HighlightedItems.Split(',');
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (string.Equals(items[i].Replace(" ", ""), Name.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
                        {
                            NameLabel.ForeColour = Color.OrangeRed;
                            break;
                        }
                    }
                }
                NameLabel.Draw();
            }

         

            if (TitleNameLabel != null)
            {
                int x = DrawX + (48 - TitleNameLabel.Size.Width) / 2;
                int y = DrawY - (32 - TitleNameLabel.Size.Height) / 2;

                if (Dead)
                    y += 21;
                else
                    y -= 6;

                TitleNameLabel.Location = new Point(x, y);
                TitleNameLabel.Draw();
            }

            if (CaptionLabel is not null)
            {

                int x = DrawX + (48 - CaptionLabel.Size.Width) / 2;
                int y = (DrawY - (32 - CaptionLabel.Size.Height) / 2) - 13;

                if (Dead)
                    y += 21;
                else
                    y -= 6;

                if (TitleNameLabel != null)
                {
                    y -= 13;
                }

                CaptionLabel.Location = new Point(x, y);
                CaptionLabel.Draw();
            }
        }
        public virtual void DrawDamage()
        {

            foreach (DamageInfo damageInfo in DamageList)
                damageInfo.Draw(DrawX, DrawY);
        }
        public void DrawChat()
        {
            if (ChatLabel == null || ChatLabel.IsDisposed) return;

            if (CEnvir.Now > ChatTime) return;

            int x = DrawX + (48 - ChatLabel.Size.Width) / 2;
            int y = DrawY - (60 + ChatLabel.Size.Height);

            if (CEnvir.Now < DrawHealthTime)
                y -= 20;

            if (Dead)
                y += 35;

            ChatLabel.ForeColour = Dead ? Color.Gray : Color.White;
            ChatLabel.Location = new Point(x, y);
            ChatLabel.Draw();
        }

        public virtual void PlayAttackSound()
        {

        }
        public virtual void PlayStruckSound()
        {

        }
        public virtual void PlayDieSound()
        {


        }

        public void DrawPoison()
        {
            //   if (this is SpellObject || Dead) return;
            if (Dead) return;

            int count = 0;

            if ((Poison & PoisonType.Paralysis) == PoisonType.Paralysis)
            {
                DXManager.Sprite.Draw(DXManager.PoisonTexture, Vector3.Zero, new Vector3(DrawX + count * 5, DrawY - 50, 0), Color.DimGray);
                count++;
            }
            if ((Poison & PoisonType.Slow) == PoisonType.Slow)
            {
                DXManager.Sprite.Draw(DXManager.PoisonTexture, Vector3.Zero, new Vector3(DrawX + count * 5, DrawY - 50, 0), Color.CornflowerBlue);
                count++;
            }

            if ((Poison & PoisonType.Red) == PoisonType.Red)
            {
                DXManager.Sprite.Draw(DXManager.PoisonTexture, Vector3.Zero, new Vector3(DrawX + count * 5, DrawY - 50, 0), Color.IndianRed);
                count++;
            }

            if ((Poison & PoisonType.Green) == PoisonType.Green)
                DXManager.Sprite.Draw(DXManager.PoisonTexture, Vector3.Zero, new Vector3(DrawX + count * 5, DrawY - 50, 0), Color.SeaGreen);
        }
        public virtual void DrawHealth()
        {

        }

        public abstract bool MouseOver(Point p);

        public virtual void UpdateQuests()
        {

        }

        public void WraithGripCreate()
        {
            WraithGripEffect = new MirEffect(1424, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx4, 40, 40, Globals.NoneColour)
            {
                Blend = true,
                Target = this,
                Loop = true,
                BlendRate = 0.4f,
            };
            WraithGripEffect2 = new MirEffect(1444, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx4, 40, 40, Globals.NoneColour)
            {
                Blend = true,
                Target = this,
                Loop = true,
                BlendRate = 0.4f,
            };
        }
        public void WraithGripEnd()
        {
            WraithGripEffect?.Remove();
            WraithGripEffect = null;
            WraithGripEffect2?.Remove();
            WraithGripEffect2 = null;
        }
        public void MagicShieldCreate()
        {
            MagicShieldEffect = new MirEffect(850, 3, TimeSpan.FromMilliseconds(200), LibraryFile.Magic, 40, 40, Globals.WindColour)
            {
                Blend = true,
                Target = this,
                Loop = true,
            };
            MagicShieldEffect.Process();
        }
        public void MagicShieldStruck()
        {
            MagicShieldEnd();

            MagicShieldEffect = new MirEffect(853, 3, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 40, 40, Globals.WindColour)
            {
                Blend = true,
                Target = this,
                CompleteAction = MagicShieldCreate,
            };
            MagicShieldEffect.Process();

        }
        public void MagicShieldEnd()
        {
            MagicShieldEffect?.Remove();
            MagicShieldEffect = null;
        }
        public void AssaultCreate()
        {
            AssaultEffect = new MirEffect(740, 3, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 40, 40, Globals.NoneColour)
            {
                Blend = true,
                Target = this,
                Loop = true,
                Direction = Direction,
            };
            AssaultEffect.Process();
        }
        public void AssaultEnd()
        {
            AssaultEffect?.Remove();
            AssaultEffect = null;
        }
        public void CelestialLightCreate()
        {
            CelestialLightEffect = new MirEffect(300, 3, TimeSpan.FromMilliseconds(200), LibraryFile.MagicEx2, 40, 40, Globals.HolyColour)
            {
                Blend = true,
                Target = this,
                Loop = true,
            };
            CelestialLightEffect.Process();
        }
        public void CelestialLightStruck()
        {
            CelestialLightEnd();

            CelestialLightEffect = new MirEffect(303, 3, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx2, 40, 40, Globals.HolyColour)
            {
                Blend = true,
                Target = this,
                CompleteAction = CelestialLightCreate,
            };
            CelestialLightEffect.Process();

        }
        public void CelestialLightEnd()
        {
            CelestialLightEffect?.Remove();
            CelestialLightEffect = null;
        }
        public void LifeStealCreate()
        {
            LifeStealEffect = new MirEffect(1260, 6, TimeSpan.FromMilliseconds(150), LibraryFile.MagicEx2, 40, 40, Globals.DarkColour)
            {
                Blend = true,
                Target = this,
                Loop = true,
            };
        }
        public void LifeStealEnd()
        {
            LifeStealEffect?.Remove();
            LifeStealEffect = null;
        }

        public void FrostBiteCreate()
        {
            FrostBiteEffect = new MirEffect(600, 7, TimeSpan.FromMilliseconds(150), LibraryFile.MagicEx5, 40, 40, Globals.IceColour)
            {
                Blend = true,
                Target = this,
                Loop = true,
            };
        }
        public void FrostBiteEnd()
        {
            FrostBiteEffect?.Remove();
            FrostBiteEffect = null;
        }
        public void SilenceCreate()
        {
            SilenceEffect = new MirEffect(680, 6, TimeSpan.FromMilliseconds(150), LibraryFile.ProgUse, 0, 0, Globals.NoneColour)
            {
                Blend = true,
                Target = this,
                Loop = true,
            };
        }
        public void SilenceEnd()
        {
            SilenceEffect?.Remove();
            SilenceEffect = null;
        }
        public void BlindCreate()
        {
            BlindEffect = new MirEffect(680, 6, TimeSpan.FromMilliseconds(150), LibraryFile.ProgUse, 0, 0, Globals.NoneColour)
            {
                //Blend = true,
                Target = this,
                Loop = true,
                DrawColour = Color.Black,
                Opacity = 0.8F
            };

            if (this != User) return;

            AbyssEffect = new MirEffect(2100, 19, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx4, 0, 0, Globals.NoneColour)
            {
                Blend = true,
                Target = this,
                Loop = true,
                AdditionalOffSet = new Point(0, -64)
            };
        }
        public void BlindEnd()
        {
            BlindEffect?.Remove();
            BlindEffect = null;
            AbyssEffect?.Remove();
            AbyssEffect = null;
        }
        public void InfectionCreate()
        {
            InfectionEffect = new MirEffect(900, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MagicEx5, 0, 0, Globals.NoneColour)
            {
                Blend = true,
                Target = this,
                Loop = true,
                //  DrawColour = Color.SaddleBrown,
                Opacity = 0.8F
            };
        }
        public void InfectionEnd()
        {
            InfectionEffect?.Remove();
            InfectionEffect = null;
        }


        public void DragonRepulseCreate()
        {
            DragonRepulseEffect = new MirEffect(1011, 4, TimeSpan.FromMilliseconds(150), LibraryFile.MagicEx4, 0, 0, Globals.NoneColour)
            {
                Target = this,
                Loop = true,
            };
            DragonRepulseEffect1 = new MirEffect(1031, 4, TimeSpan.FromMilliseconds(150), LibraryFile.MagicEx4, 80, 80, Globals.LightningColour)
            {
                Blend = true,
                Target = this,
                Loop = true,
            };
        }
        public void DragonRepulseEnd()
        {
            DragonRepulseEffect?.Remove();
            DragonRepulseEffect = null;
            DragonRepulseEffect1?.Remove();
            DragonRepulseEffect1 = null;
        }

        public void RankingCreate()
        {
            RankingEffect = new MirEffect(3420, 7, TimeSpan.FromMilliseconds(150), LibraryFile.GameInter, 0, 0, Globals.NoneColour)
            {
                Blend = true,
                Target = this,
                Loop = true,
                AdditionalOffSet = new Point(0, -25)
            };
            RankingEffect.Process();
        }
        public void RankingEnd()
        {
            RankingEffect?.Remove();
            RankingEffect = null;
        }

        public void DeveloperCreate()
        {
            DeveloperEffect = new MirEffect(3410, 7, TimeSpan.FromMilliseconds(150), LibraryFile.GameInter, 0, 0, Globals.NoneColour)
            {
                Blend = true,
                Target = this,
                Loop = true,
                AdditionalOffSet = new Point(10, -25)
            };
            DeveloperEffect.Process();
        }
        public void DeveloperEnd()
        {
            DeveloperEffect?.Remove();
            DeveloperEffect = null;
        }

        public virtual void Remove()
        {
            GameScene.Game.MapControl.RemoveObject(this);

            MagicShieldEnd();
            CelestialLightEnd();
            WraithGripEnd();
            LifeStealEnd();
            SilenceEnd();
            BlindEnd();
            DragonRepulseEnd();
            RankingEnd();
            DeveloperEnd();
            AssaultEnd();
            FrostBiteEnd();
            InfectionEnd();

            for (int i = Effects.Count - 1; i >= 0; i--)
            {
                MirEffect effect = Effects[i];
                effect.Remove();
            }
        }
    }
}
