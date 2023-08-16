using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Controls;
using Client.Envir;
using Client.Scenes;
using Client.Scenes.Views;
using Library;
using Library.SystemModels;
using SlimDX;
using S = Library.Network.ServerPackets;

namespace Client.Models
{
    public sealed class MonsterObject : MapObject
    {
        public override ObjectType Race => ObjectType.Monster;
        public override bool Blocking => base.Blocking && CompanionObject == null;

        public MonsterInfo MonsterInfo;
        
        public MirLibrary BodyLibrary;
        public int BodyOffSet = 1000;
        public int BodyShape;

        public float Scale = 1F;

        public int GrowthLevel;

        public int BodyFrame => DrawFrame + (BodyShape % 10) * BodyOffSet;

        public SoundIndex AttackSound, StruckSound, DieSound;

        public bool Extra, EasterEvent, ChristmasEvent, HalloweenEvent;

        public override int RenderY
        {
            get
            {
                int offset = 0;

                if (Image == MonsterImage.LobsterLord)
                    offset += 5;

                return base.RenderY + offset;
            }
        }

        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(PetOwner))
                    return base.Name;

                return base.Name + $" ({PetOwner})";
            }
            set { base.Name = value; }
        }

        public ClientCompanionObject CompanionObject;

        public MonsterImage Image;

        public MonsterObject(CompanionInfo info)
        {
            MonsterInfo = info.MonsterInfo;

            Stats = new Stats(MonsterInfo.Stats);

            Light = Stats[Stat.Light];

            Name = MonsterInfo.MonsterName;
            
            Direction = MirDirection.DownLeft;

            UpdateLibraries();

            SetAnimation(new ObjectAction(MirAction.Standing, Direction, Point.Empty));
        }
        public MonsterObject(S.ObjectMonster info)
        {
            ObjectID = info.ObjectID;

            MonsterInfo = Globals.MonsterInfoList.Binding.First(x => x.Index == info.MonsterIndex);

            CompanionObject = info.CompanionObject;

            Stats = new Stats(MonsterInfo.Stats);

            Light = Stats[Stat.Light];

            Name = CompanionObject?.Name ?? MonsterInfo.MonsterName;

            PetOwner = info.PetOwner;
            NameColour = info.NameColour;
            Extra = info.Extra;
            
            CurrentLocation = info.Location;
            Direction = info.Direction;
            
            Dead = info.Dead;
            Skeleton = info.Skeleton;

            EasterEvent = info.EasterEvent;
            HalloweenEvent = info.HalloweenEvent;
            ChristmasEvent = info.ChristmasEvent;

            Poison = info.Poison;

            foreach (BuffType type in info.Buffs)
                VisibleBuffs.Add(type);

            UpdateLibraries();
            SetScale();

            SetFrame(new ObjectAction(!Dead ? MirAction.Standing : MirAction.Dead, MirDirection.Up, CurrentLocation));
            
            GameScene.Game.MapControl.AddObject(this);
            
            UpdateQuests();
        }
        public void UpdateLibraries()
        {
            BodyLibrary = null;

            Frames = new Dictionary<MirAnimation, Frame>(FrameSet.DefaultMonster);

            BodyOffSet = 1000;

            AttackSound = SoundIndex.None;
            StruckSound = SoundIndex.None;
            DieSound = SoundIndex.None;
            //OtherSounds

            Image = MonsterInfo.Image;

            switch (Image)
            {
                case MonsterImage.Chicken:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.ChickenAttack;
                    StruckSound = SoundIndex.ChickenStruck;
                    DieSound = SoundIndex.ChickenDie;
                    break;
                case MonsterImage.Pig:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_12, out BodyLibrary);
                    BodyShape = 9;
                    AttackSound = SoundIndex.PigAttack;
                    StruckSound = SoundIndex.PigStruck;
                    DieSound = SoundIndex.PigDie;
                    break;
                case MonsterImage.Deer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.DeerAttack;
                    StruckSound = SoundIndex.DeerStruck;
                    DieSound = SoundIndex.DeerDie;
                    break;
                case MonsterImage.Cow:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_13, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.CowAttack;
                    StruckSound = SoundIndex.CowStruck;
                    DieSound = SoundIndex.CowDie;
                    break;
                case MonsterImage.Sheep:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_6, out BodyLibrary);
                    BodyShape = 8;
                    AttackSound = SoundIndex.SheepAttack;
                    StruckSound = SoundIndex.SheepStruck;
                    DieSound = SoundIndex.SheepDie;
                    break;
                case MonsterImage.ClawCat:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out BodyLibrary);
                    BodyShape = 8;
                    AttackSound = SoundIndex.ClawCatAttack;
                    StruckSound = SoundIndex.ClawCatStruck;
                    DieSound = SoundIndex.ClawCatDie;
                    break;
                case MonsterImage.Wolf:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out BodyLibrary);
                    BodyShape = 5;
                    AttackSound = SoundIndex.WolfAttack;
                    StruckSound = SoundIndex.WolfStruck;
                    DieSound = SoundIndex.WolfDie;
                    break;
                case MonsterImage.ForestYeti:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.ForestYetiAttack;
                    StruckSound = SoundIndex.ForestYetiStruck;
                    DieSound = SoundIndex.ForestYetiDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.ForestYeti)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.ChestnutTree:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_13, out BodyLibrary);
                    BodyShape = 7;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.ChestnutTree)
                        Frames[frame.Key] = frame.Value;

                    break;
                case MonsterImage.CarnivorousPlant:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.CarnivorousPlantAttack;
                    StruckSound = SoundIndex.CarnivorousPlantStruck;
                    DieSound = SoundIndex.CarnivorousPlantDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.CarnivorousPlant)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.Oma:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out BodyLibrary);
                    BodyShape = 3;
                    AttackSound = SoundIndex.OmaAttack;
                    StruckSound = SoundIndex.OmaStruck;
                    DieSound = SoundIndex.OmaDie;
                    break;
                case MonsterImage.TigerSnake:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_6, out BodyLibrary);
                    BodyShape = 7;
                    AttackSound = SoundIndex.TigerSnakeAttack;
                    StruckSound = SoundIndex.TigerSnakeStruck;
                    DieSound = SoundIndex.TigerSnakeDie;
                    break;
                case MonsterImage.SpittingSpider:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out BodyLibrary);
                    BodyShape = 5;
                    AttackSound = SoundIndex.SpittingSpiderAttack;
                    StruckSound = SoundIndex.SpittingSpiderStruck;
                    DieSound = SoundIndex.SpittingSpiderDie;
                    
                    break;
                case MonsterImage.Scarecrow:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.ScarecrowAttack;
                    StruckSound = SoundIndex.ScarecrowStruck;
                    DieSound = SoundIndex.ScarecrowDie;
                    break;
                case MonsterImage.OmaHero:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.OmaHeroAttack;
                    StruckSound = SoundIndex.OmaHeroStruck;
                    DieSound = SoundIndex.OmaHeroDie;
                    break;
                case MonsterImage.Guard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out BodyLibrary);
                    BodyShape = 6;
                    break;
                case MonsterImage.CaveBat:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out BodyLibrary);
                    BodyShape = 9;
                    AttackSound = SoundIndex.CaveBatAttack;
                    StruckSound = SoundIndex.CaveBatStruck;
                    DieSound = SoundIndex.CaveBatDie;
                    break;
                case MonsterImage.Scorpion:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out BodyLibrary);
                    BodyShape = 8;
                    AttackSound = SoundIndex.ScorpionAttack;
                    StruckSound = SoundIndex.ScorpionStruck;
                    DieSound = SoundIndex.ScorpionDie;
                    break;
                case MonsterImage.Skeleton:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out BodyLibrary);
                    BodyShape = 2;
                    AttackSound = SoundIndex.SkeletonAttack;
                    StruckSound = SoundIndex.SkeletonStruck;
                    DieSound = SoundIndex.SkeletonDie;
                    break;
                case MonsterImage.SkeletonAxeMan:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.SkeletonAxeManAttack;
                    StruckSound = SoundIndex.SkeletonAxeManStruck;
                    DieSound = SoundIndex.SkeletonAxeManDie;
                    break;
                case MonsterImage.SkeletonAxeThrower:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out BodyLibrary);
                    BodyShape = 3;
                    AttackSound = SoundIndex.SkeletonAxeThrowerAttack;
                    StruckSound = SoundIndex.SkeletonAxeThrowerStruck;
                    DieSound = SoundIndex.SkeletonAxeThrowerDie;
                    break;
                case MonsterImage.SkeletonWarrior:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out BodyLibrary);
                    BodyShape = 5;
                    AttackSound = SoundIndex.SkeletonWarriorAttack;
                    StruckSound = SoundIndex.SkeletonWarriorStruck;
                    DieSound = SoundIndex.SkeletonWarriorDie;
                    break;
                case MonsterImage.SkeletonLord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out BodyLibrary);
                    BodyShape = 6;
                    AttackSound = SoundIndex.SkeletonLordAttack;
                    StruckSound = SoundIndex.SkeletonLordStruck;
                    DieSound = SoundIndex.SkeletonLordDie;
                    break;
                case MonsterImage.CaveMaggot:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out BodyLibrary);
                    BodyShape = 7;
                    AttackSound = SoundIndex.CaveMaggotAttack;
                    StruckSound = SoundIndex.CaveMaggotStruck;
                    DieSound = SoundIndex.CaveMaggotDie;
                    
                    break;
                case MonsterImage.GhostSorcerer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out BodyLibrary);
                    BodyShape = 8;
                    AttackSound = SoundIndex.GhostSorcererAttack;
                    StruckSound = SoundIndex.GhostSorcererStruck;
                    DieSound = SoundIndex.GhostSorcererDie;
                    break;
                case MonsterImage.GhostMage:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out BodyLibrary);
                    BodyShape = 9;
                    AttackSound = SoundIndex.GhostMageAttack;
                    StruckSound = SoundIndex.GhostMageStruck;
                    DieSound = SoundIndex.GhostMageDie; //Appear SOUND ?
                    break;
                case MonsterImage.VoraciousGhost:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_6, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.VoraciousGhostAttack;
                    StruckSound = SoundIndex.VoraciousGhostStruck;
                    DieSound = SoundIndex.VoraciousGhostDie;
                    break;
                case MonsterImage.DevouringGhost:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_6, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.VoraciousGhostAttack;
                    StruckSound = SoundIndex.VoraciousGhostStruck;
                    DieSound = SoundIndex.VoraciousGhostDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.DevouringGhost)
                        Frames[frame.Key] = frame.Value;


                    break;
                case MonsterImage.CorpseRaisingGhost:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_6, out BodyLibrary);
                    BodyShape = 2;
                    AttackSound = SoundIndex.VoraciousGhostAttack;
                    StruckSound = SoundIndex.VoraciousGhostStruck;
                    DieSound = SoundIndex.VoraciousGhostDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.DevouringGhost)
                        Frames[frame.Key] = frame.Value;

                    break;
                case MonsterImage.GhoulChampion:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_6, out BodyLibrary);
                    BodyShape = 3;
                    AttackSound = SoundIndex.GhoulChampionAttack;
                    StruckSound = SoundIndex.GhoulChampionStruck;
                    DieSound = SoundIndex.GhoulChampionDie;
                    break;
                case MonsterImage.ArmoredAnt:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out BodyLibrary);
                    BodyShape = 8;
                    AttackSound = SoundIndex.ArmoredAntAttack;
                    StruckSound = SoundIndex.ArmoredAntStruck;
                    DieSound = SoundIndex.ArmoredAntDie;
                    break;
                case MonsterImage.AntSoldier:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_2, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.ArmoredAntAttack;
                    StruckSound = SoundIndex.ArmoredAntStruck;
                    DieSound = SoundIndex.ArmoredAntDie;
                    break;
                case MonsterImage.AntHealer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out BodyLibrary);
                    BodyShape = 7;
                    AttackSound = SoundIndex.ArmoredAntAttack;
                    StruckSound = SoundIndex.ArmoredAntStruck;
                    DieSound = SoundIndex.ArmoredAntDie;
                    break;
                case MonsterImage.AntNeedler:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out BodyLibrary);
                    BodyShape = 6;
                    AttackSound = SoundIndex.AntNeedlerAttack;
                    StruckSound = SoundIndex.AntNeedlerStruck;
                    DieSound = SoundIndex.AntNeedlerDie;
                    break;
                case MonsterImage.Beetle:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out BodyLibrary);
                    BodyShape = 3;
                    AttackSound = SoundIndex.KeratoidAttack;
                    StruckSound = SoundIndex.KeratoidStruck;
                    DieSound = SoundIndex.KeratoidDie;
                    break;
                case MonsterImage.ShellNipper:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.ShellNipperAttack;
                    StruckSound = SoundIndex.ShellNipperStruck;
                    DieSound = SoundIndex.ShellNipperDie;
                    break;
                case MonsterImage.VisceralWorm:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.VisceralWormAttack;
                    StruckSound = SoundIndex.VisceralWormStruck;
                    DieSound = SoundIndex.VisceralWormDie;
                    break;
                case MonsterImage.MutantFlea:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out BodyLibrary);
                    BodyShape = 5;
                    AttackSound = SoundIndex.MutantFleaAttack;
                    StruckSound = SoundIndex.MutantFleaStruck;
                    DieSound = SoundIndex.MutantFleaDie;
                    break;
                case MonsterImage.PoisonousMutantFlea:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out BodyLibrary);
                    BodyShape = 9;
                    AttackSound = SoundIndex.PoisonousMutantFleaAttack;
                    StruckSound = SoundIndex.PoisonousMutantFleaStruck;
                    DieSound = SoundIndex.PoisonousMutantFleaDie;
                    break;
                case MonsterImage.BlasterMutantFlea:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out BodyLibrary);
                    BodyShape = 7;
                    AttackSound = SoundIndex.BlasterMutantFleaAttack;
                    StruckSound = SoundIndex.BlasterMutantFleaStruck;
                    DieSound = SoundIndex.BlasterMutantFleaDie;
                    break;

                case MonsterImage.WasHatchling:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.WasHatchlingAttack;
                    StruckSound = SoundIndex.WasHatchlingStruck;
                    DieSound = SoundIndex.WasHatchlingDie;
                    break;

                case MonsterImage.Centipede:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out BodyLibrary);
                    BodyShape = 6;
                    AttackSound = SoundIndex.CentipedeAttack;
                    StruckSound = SoundIndex.CentipedeStruck;
                    DieSound = SoundIndex.CentipedeDie;
                    break;

                case MonsterImage.ButterflyWorm:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out BodyLibrary);
                    BodyShape = 2;
                    AttackSound = SoundIndex.ButterflyWormAttack;
                    StruckSound = SoundIndex.ButterflyWormStruck;
                    DieSound = SoundIndex.ButterflyWormDie;
                    break;

                case MonsterImage.MutantMaggot:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out BodyLibrary);
                    BodyShape = 8;
                    AttackSound = SoundIndex.MutantMaggotAttack;
                    StruckSound = SoundIndex.MutantMaggotStruck;
                    DieSound = SoundIndex.MutantMaggotDie;
                    break;

                case MonsterImage.Earwig:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out BodyLibrary);
                    BodyShape = 9;
                    AttackSound = SoundIndex.EarwigAttack;
                    StruckSound = SoundIndex.EarwigStruck;
                    DieSound = SoundIndex.EarwigDie;
                    break;

                case MonsterImage.IronLance:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.IronLanceAttack;
                    StruckSound = SoundIndex.IronLanceStruck;
                    DieSound = SoundIndex.IronLanceDie;
                    break;

                case MonsterImage.LordNiJae:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out BodyLibrary);
                    BodyShape = 7;
                    AttackSound = SoundIndex.LordNiJaeAttack;
                    StruckSound = SoundIndex.LordNiJaeStruck;
                    DieSound = SoundIndex.LordNiJaeDie;
                    break;

                case MonsterImage.RottingGhoul:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out BodyLibrary);
                    BodyShape = 8;
                    AttackSound = SoundIndex.RottingGhoulAttack;
                    StruckSound = SoundIndex.RottingGhoulStruck;
                    DieSound = SoundIndex.RottingGhoulDie;
                    break;

                case MonsterImage.DecayingGhoul:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out BodyLibrary);
                    BodyShape = 2;
                    AttackSound = SoundIndex.DecayingGhoulAttack;
                    StruckSound = SoundIndex.DecayingGhoulStruck;
                    DieSound = SoundIndex.DecayingGhoulDie;
                    break;

                case MonsterImage.BloodThirstyGhoul:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out BodyLibrary);
                    BodyShape = 2;
                    AttackSound = SoundIndex.BloodThirstyGhoulAttack;
                    StruckSound = SoundIndex.BloodThirstyGhoulStruck;
                    DieSound = SoundIndex.BloodThirstyGhoulDie;
                    break;

                case MonsterImage.SpinedDarkLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out BodyLibrary);
                    BodyShape = 6;
                    AttackSound = SoundIndex.SpinedDarkLizardAttack;
                    StruckSound = SoundIndex.SpinedDarkLizardStruck;
                    DieSound = SoundIndex.SpinedDarkLizardDie;
                    break;
                case MonsterImage.UmaInfidel:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.UmaInfidelAttack;
                    StruckSound = SoundIndex.UmaInfidelStruck;
                    DieSound = SoundIndex.UmaInfidelDie;
                    break;
                case MonsterImage.UmaFlameThrower:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out BodyLibrary);
                    BodyShape = 3;
                    AttackSound = SoundIndex.UmaFlameThrowerAttack;
                    StruckSound = SoundIndex.UmaFlameThrowerStruck;
                    DieSound = SoundIndex.UmaFlameThrowerDie;
                    break;
                case MonsterImage.UmaAnguisher:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.UmaAnguisherAttack;
                    StruckSound = SoundIndex.UmaAnguisherStruck;
                    DieSound = SoundIndex.UmaAnguisherDie;
                    break;
                case MonsterImage.UmaKing:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out BodyLibrary);
                    BodyShape = 5;
                    AttackSound = SoundIndex.UmaKingAttack;
                    StruckSound = SoundIndex.UmaKingStruck;
                    DieSound = SoundIndex.UmaKingDie;
                    break;

                case MonsterImage.SpiderBat:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_11, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.SpiderBatAttack;
                    StruckSound = SoundIndex.SpiderBatStruck;
                    DieSound = SoundIndex.SpiderBatDie;
                    break;
                case MonsterImage.ArachnidGazer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_11, out BodyLibrary);
                    BodyShape = 6;
                    StruckSound = SoundIndex.ArachnidGazerStruck;
                    DieSound = SoundIndex.ArachnidGazerDie;
                    break;

                case MonsterImage.Larva:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_11, out BodyLibrary);
                    BodyShape = 5;
                    AttackSound = SoundIndex.LarvaAttack;
                    StruckSound = SoundIndex.LarvaStruck;
                    
                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.Larva)
                        Frames[frame.Key] = frame.Value;
                    break;

                case MonsterImage.RedMoonGuardian:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_11, out BodyLibrary);
                    BodyShape = 7;
                    AttackSound = SoundIndex.RedMoonGuardianAttack;
                    StruckSound = SoundIndex.RedMoonGuardianStruck;
                    DieSound = SoundIndex.RedMoonGuardianDie;
                    break;
                case MonsterImage.RedMoonProtector:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_11, out BodyLibrary);
                    BodyShape = 8;
                    AttackSound = SoundIndex.RedMoonProtectorAttack;
                    StruckSound = SoundIndex.RedMoonProtectorStruck;
                    DieSound = SoundIndex.RedMoonProtectorDie;
                    break;
                case MonsterImage.VenomousArachnid:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_12, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.VenomousArachnidAttack;
                    StruckSound = SoundIndex.VenomousArachnidStruck;
                    DieSound = SoundIndex.VenomousArachnidDie;
                    break;
                case MonsterImage.DarkArachnid:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_12, out BodyLibrary);
                    BodyShape = 2;
                    AttackSound = SoundIndex.DarkArachnidAttack;
                    StruckSound = SoundIndex.DarkArachnidStruck;
                    DieSound = SoundIndex.DarkArachnidDie;
                    break;
                case MonsterImage.RedMoonTheFallen:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_11, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.RedMoonTheFallenAttack;
                    StruckSound = SoundIndex.RedMoonTheFallenStruck;
                    DieSound = SoundIndex.RedMoonTheFallenDie;
                    break;

                case MonsterImage.ViciousRat:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_9, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.ViciousRatAttack;
                    StruckSound = SoundIndex.ViciousRatStruck;
                    DieSound = SoundIndex.ViciousRatDie;
                    break;

                case MonsterImage.ZumaSharpShooter:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_9, out BodyLibrary);
                    BodyShape = 2;
                    AttackSound = SoundIndex.ZumaSharpShooterAttack;
                    StruckSound = SoundIndex.ZumaSharpShooterStruck;
                    DieSound = SoundIndex.ZumaSharpShooterDie;
                    break;

                case MonsterImage.ZumaFanatic:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_9, out BodyLibrary);
                    BodyShape = 3;
                    AttackSound = SoundIndex.ZumaFanaticAttack;
                    StruckSound = SoundIndex.ZumaFanaticStruck;
                    DieSound = SoundIndex.ZumaFanaticDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.ZumaGuardian)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.ZumaGuardian:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_9, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.ZumaGuardianAttack;
                    StruckSound = SoundIndex.ZumaGuardianStruck;
                    DieSound = SoundIndex.ZumaGuardianDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.ZumaGuardian)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.ZumaKing:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_9, out BodyLibrary);
                    BodyShape = 5;
                    AttackSound = SoundIndex.ZumaKingAttack;
                    StruckSound = SoundIndex.ZumaKingStruck;
                    DieSound = SoundIndex.ZumaKingDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.ZumaKing)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.EvilFanatic:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out BodyLibrary);
                    BodyShape = 7;
                    AttackSound = SoundIndex.EvilFanaticAttack;
                    StruckSound = SoundIndex.EvilFanaticStruck;
                    DieSound = SoundIndex.EvilFanaticDie;
                    break;
                case MonsterImage.Monkey:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.MonkeyAttack;
                    StruckSound = SoundIndex.MonkeyStruck;
                    DieSound = SoundIndex.MonkeyDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.Monkey)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.EvilElephant:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out BodyLibrary);
                    BodyShape = 8;
                    AttackSound = SoundIndex.EvilElephantAttack;
                    StruckSound = SoundIndex.EvilElephantStruck;
                    DieSound = SoundIndex.EvilElephantDie;
                    break;
                case MonsterImage.CannibalFanatic:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out BodyLibrary);
                    BodyShape = 6;
                    AttackSound = SoundIndex.CannibalFanaticAttack;
                    StruckSound = SoundIndex.CannibalFanaticStruck;
                    DieSound = SoundIndex.CannibalFanaticDie;
                    break;

                case MonsterImage.SpikedBeetle:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.SpikedBeetleAttack;
                    StruckSound = SoundIndex.SpikedBeetleStruck;
                    DieSound = SoundIndex.SpikedBeetleDie;
                    break;
                case MonsterImage.NumaGrunt:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_13, out BodyLibrary);
                    BodyShape = 8;
                    AttackSound = SoundIndex.NumaGruntAttack;
                    StruckSound = SoundIndex.NumaGruntStruck;
                    DieSound = SoundIndex.NumaGruntDie;
                    break;
                case MonsterImage.NumaMage:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_2, out BodyLibrary);
                    BodyShape = 3;
                    AttackSound = SoundIndex.NumaMageAttack;
                    StruckSound = SoundIndex.NumaMageStruck;
                    DieSound = SoundIndex.NumaMageDie;
                    
                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.NumaMage)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.NumaElite:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_2, out BodyLibrary);
                    BodyShape = 7;
                    AttackSound = SoundIndex.NumaEliteAttack;
                    StruckSound = SoundIndex.NumaEliteStruck;
                    DieSound = SoundIndex.NumaEliteDie;
                    break;
                case MonsterImage.SandShark:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.SandSharkAttack;
                    StruckSound = SoundIndex.SandSharkStruck;
                    DieSound = SoundIndex.SandSharkDie;
                    break;
                case MonsterImage.StoneGolem:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.StoneGolemAttack;
                    StruckSound = SoundIndex.StoneGolemStruck;
                    DieSound = SoundIndex.StoneGolemDie;
                    break;
                case MonsterImage.WindfurySorceress:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out BodyLibrary);
                    BodyShape = 7;
                    AttackSound = SoundIndex.WindfurySorceressAttack;
                    StruckSound = SoundIndex.WindfurySorceressStruck;
                    DieSound = SoundIndex.WindfurySorceressDie;
                    break;
                case MonsterImage.CursedCactus:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out BodyLibrary);
                    BodyShape = 5;
                    AttackSound = SoundIndex.CursedCactusAttack;
                    StruckSound = SoundIndex.CursedCactusStruck;
                    DieSound = SoundIndex.CursedCactusDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.CursedCactus)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.NetherWorldGate:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out BodyLibrary);
                    BodyShape = 5;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.NetherWorldGate)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.RagingLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.RagingLizardAttack;
                    StruckSound = SoundIndex.RagingLizardStruck;
                    DieSound = SoundIndex.RagingLizardDie;
                    break;
                    
                case MonsterImage.SawToothLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out BodyLibrary);
                    BodyShape = 2;
                    AttackSound = SoundIndex.SawToothLizardAttack;
                    StruckSound = SoundIndex.SawToothLizardStruck;
                    DieSound = SoundIndex.SawToothLizardDie;
                    break;
                case MonsterImage.MutantLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out BodyLibrary);
                    BodyShape = 3;
                    AttackSound = SoundIndex.MutantLizardAttack;
                    StruckSound = SoundIndex.MutantLizardStruck;
                    DieSound = SoundIndex.MutantLizardDie;
                    break;
                case MonsterImage.VenomSpitter:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.VenomSpitterAttack;
                    StruckSound = SoundIndex.VenomSpitterStruck;
                    DieSound = SoundIndex.VenomSpitterDie;
                    break;
                case MonsterImage.SonicLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out BodyLibrary);
                    BodyShape = 5;
                    AttackSound = SoundIndex.SonicLizardAttack;
                    StruckSound = SoundIndex.SonicLizardStruck;
                    DieSound = SoundIndex.SonicLizardDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.WestDesertLizard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.GiantLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out BodyLibrary);
                    BodyShape = 6;
                    AttackSound = SoundIndex.GiantLizardAttack;
                    StruckSound = SoundIndex.GiantLizardStruck;
                    DieSound = SoundIndex.GiantLizardDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.WestDesertLizard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.CrazedLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out BodyLibrary);
                    BodyShape = 9;
                    AttackSound = SoundIndex.CrazedLizardAttack;
                    StruckSound = SoundIndex.CrazedLizardStruck;
                    DieSound = SoundIndex.CrazedLizardDie;
                    break;
                case MonsterImage.TaintedTerror:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out BodyLibrary);
                    BodyShape = 7;
                    AttackSound = SoundIndex.TaintedTerrorAttack;
                    StruckSound = SoundIndex.TaintedTerrorStruck;
                    DieSound = SoundIndex.TaintedTerrorDie;


                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.WestDesertLizard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.DeathLordJichon:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out BodyLibrary);
                    BodyShape = 8;
                    AttackSound = SoundIndex.DeathLordJichonAttack;
                    StruckSound = SoundIndex.DeathLordJichonStruck;
                    DieSound = SoundIndex.DeathLordJichonDie;
                    break;

                case MonsterImage.Minotaur:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out BodyLibrary);
                    BodyShape = 7;
                    AttackSound = SoundIndex.MinotaurAttack;
                    StruckSound = SoundIndex.MinotaurStruck;
                    DieSound = SoundIndex.MinotaurDie;
                    break;
                case MonsterImage.FrostMinotaur:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out BodyLibrary);
                    BodyShape = 3;
                    AttackSound = SoundIndex.FrostMinotaurAttack;
                    StruckSound = SoundIndex.FrostMinotaurStruck;
                    DieSound = SoundIndex.FrostMinotaurDie;
                    break;
                case MonsterImage.ShockMinotaur:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.FrostMinotaurAttack;
                    StruckSound = SoundIndex.FrostMinotaurStruck;
                    DieSound = SoundIndex.FrostMinotaurDie;
                    break;
                case MonsterImage.FlameMinotaur:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out BodyLibrary);
                    BodyShape = 6;
                    AttackSound = SoundIndex.FrostMinotaurAttack;
                    StruckSound = SoundIndex.FrostMinotaurStruck;
                    DieSound = SoundIndex.FrostMinotaurDie;
                    break;
                case MonsterImage.FuryMinotaur:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out BodyLibrary);
                    BodyShape = 5;
                    AttackSound = SoundIndex.FrostMinotaurAttack;
                    StruckSound = SoundIndex.FrostMinotaurStruck;
                    DieSound = SoundIndex.FrostMinotaurDie;
                    break;
                case MonsterImage.BanyaLeftGuard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.BanyaLeftGuardAttack;
                    StruckSound = SoundIndex.BanyaLeftGuardStruck;
                    DieSound = SoundIndex.BanyaLeftGuardDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.BanyaGuard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.BanyaRightGuard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.BanyaLeftGuardAttack;
                    StruckSound = SoundIndex.BanyaLeftGuardStruck;
                    DieSound = SoundIndex.BanyaLeftGuardDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.BanyaGuard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.EmperorSaWoo:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out BodyLibrary);
                    BodyShape = 9;
                    AttackSound = SoundIndex.EmperorSaWooAttack;
                    StruckSound = SoundIndex.EmperorSaWooStruck;
                    DieSound = SoundIndex.EmperorSaWooDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.EmperorSaWoo)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.BoneArcher:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.BoneArcherAttack;
                    StruckSound = SoundIndex.BoneArcherStruck;
                    DieSound = SoundIndex.BoneArcherDie;
                    break;
                case MonsterImage.BoneBladesman:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out BodyLibrary);
                    BodyShape = 3;
                    AttackSound = SoundIndex.BoneArcherAttack;
                    StruckSound = SoundIndex.BoneArcherStruck;
                    DieSound = SoundIndex.BoneArcherDie;
                    break;
                case MonsterImage.BoneCaptain:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.BoneCaptainAttack;
                    StruckSound = SoundIndex.BoneCaptainStruck;
                    DieSound = SoundIndex.BoneCaptainDie;
                    break;
                case MonsterImage.BoneSoldier:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out BodyLibrary);
                    BodyShape = 2;
                    AttackSound = SoundIndex.BoneArcherAttack;
                    StruckSound = SoundIndex.BoneArcherStruck;
                    DieSound = SoundIndex.BoneArcherDie;
                    break;
                case MonsterImage.ArchLichTaedu:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.ArchLichTaeduAttack;
                    StruckSound = SoundIndex.ArchLichTaeduStruck;
                    DieSound = SoundIndex.ArchLichTaeduDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.ArchLichTaeda)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.WedgeMothLarva:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out BodyLibrary);
                    BodyShape = 3;
                    AttackSound = SoundIndex.WedgeMothLarvaAttack;
                    StruckSound = SoundIndex.WedgeMothLarvaStruck;
                    DieSound = SoundIndex.WedgeMothLarvaDie;
                    break;
                case MonsterImage.LesserWedgeMoth:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.LesserWedgeMothAttack;
                    StruckSound = SoundIndex.LesserWedgeMothStruck;
                    DieSound = SoundIndex.LesserWedgeMothDie;
                    break;
                case MonsterImage.WedgeMoth:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out BodyLibrary);
                    BodyShape = 5;
                    AttackSound = SoundIndex.WedgeMothAttack;
                    StruckSound = SoundIndex.WedgeMothStruck;
                    DieSound = SoundIndex.WedgeMothDie;
                    break;
                case MonsterImage.RedBoar:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out BodyLibrary);
                    BodyShape = 6;
                    AttackSound = SoundIndex.RedBoarAttack;
                    StruckSound = SoundIndex.RedBoarStruck;
                    DieSound = SoundIndex.RedBoarDie;
                    break;
                case MonsterImage.ClawSerpent:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out BodyLibrary);
                    BodyShape = 9;
                    AttackSound = SoundIndex.ClawSerpentAttack;
                    StruckSound = SoundIndex.ClawSerpentStruck;
                    DieSound = SoundIndex.ClawSerpentDie;
                    break;
                case MonsterImage.BlackBoar:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out BodyLibrary);
                    BodyShape = 7;
                    AttackSound = SoundIndex.BlackBoarAttack;
                    StruckSound = SoundIndex.BlackBoarStruck;
                    DieSound = SoundIndex.BlackBoarDie;
                    break;
                case MonsterImage.TuskLord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out BodyLibrary);
                    BodyShape = 8;
                    AttackSound = SoundIndex.TuskLordAttack;
                    StruckSound = SoundIndex.TuskLordStruck;
                    DieSound = SoundIndex.TuskLordDie;
                    break;
                case MonsterImage.RazorTusk:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.RazorTuskAttack;
                    StruckSound = SoundIndex.RazorTuskStruck;
                    DieSound = SoundIndex.RazorTuskDie;
                    break;
                    

                case MonsterImage.PinkGoddess:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_17, out BodyLibrary);
                    BodyShape = 2;
                    AttackSound = SoundIndex.PinkGoddessAttack;
                    StruckSound = SoundIndex.PinkGoddessStruck;
                    DieSound = SoundIndex.PinkGoddessDie;
                    break;
                case MonsterImage.GreenGoddess:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_17, out BodyLibrary);
                    BodyShape = 3;
                    AttackSound = SoundIndex.GreenGoddessAttack;
                    StruckSound = SoundIndex.GreenGoddessStruck;
                    DieSound = SoundIndex.GreenGoddessDie;
                    break;
                case MonsterImage.MutantCaptain:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_17, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.MutantCaptainAttack;
                    StruckSound = SoundIndex.MutantCaptainStruck;
                    DieSound = SoundIndex.MutantCaptainDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.WestDesertLizard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.StoneGriffin:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_17, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.StoneGriffinAttack;
                    StruckSound = SoundIndex.StoneGriffinStruck;
                    DieSound = SoundIndex.StoneGriffinDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.BanyaGuard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.FlameGriffin:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out BodyLibrary);
                    BodyShape = 9;
                    AttackSound = SoundIndex.FlameGriffinAttack;
                    StruckSound = SoundIndex.FlameGriffinStruck;
                    DieSound = SoundIndex.FlameGriffinDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.BanyaGuard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.JinchonDevil:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_17, out BodyLibrary);
                    BodyShape = 4;
                    //AttackSound = SoundIndex.JinchonDevilAttack;
                    StruckSound = SoundIndex.JinchonDevilStruck;
                    DieSound = SoundIndex.JinchonDevilDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.JinchonDevil)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.WhiteBone:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_6, out BodyLibrary);
                    BodyShape = 6;
                    AttackSound = SoundIndex.WhiteBoneAttack;
                    StruckSound = SoundIndex.WhiteBoneStruck;
                    DieSound = SoundIndex.WhiteBoneDie;
                    break;
                case MonsterImage.Shinsu:
                    if (Extra)
                    {
                        CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out BodyLibrary);
                        BodyShape = 0;
                        AttackSound = SoundIndex.ShinsuBigAttack;
                        StruckSound = SoundIndex.ShinsuBigStruck;
                        DieSound = SoundIndex.ShinsuBigDie;
                    }
                    else
                    {
                        CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_9, out BodyLibrary);
                        BodyShape = 9;

                        AttackSound = SoundIndex.None;
                        StruckSound = SoundIndex.ShinsuSmallStruck;
                        DieSound = SoundIndex.ShinsuSmallDie;
                    }
                    break;
                    
                case MonsterImage.CorpseStalker:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_2, out BodyLibrary);
                    BodyShape = 2;
                    AttackSound = SoundIndex.CorpseStalkerAttack;
                    StruckSound = SoundIndex.CorpseStalkerStruck;
                    DieSound = SoundIndex.CorpseStalkerDie;
                    break;
                case MonsterImage.LightArmedSoldier:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out BodyLibrary);
                    BodyShape = 6;
                    AttackSound = SoundIndex.LightArmedSoldierAttack;
                    StruckSound = SoundIndex.LightArmedSoldierStruck;
                    DieSound = SoundIndex.LightArmedSoldierDie;
                    break;
                case MonsterImage.CorrosivePoisonSpitter:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out BodyLibrary);
                    BodyShape = 3;
                    AttackSound = SoundIndex.CorrosivePoisonSpitterAttack;
                    StruckSound = SoundIndex.CorrosivePoisonSpitterStruck;
                    DieSound = SoundIndex.CorrosivePoisonSpitterDie;
                    break;
                case MonsterImage.PhantomSoldier:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out BodyLibrary);
                    BodyShape = 9;
                    AttackSound = SoundIndex.PhantomSoldierAttack;
                    StruckSound = SoundIndex.PhantomSoldierStruck;
                    DieSound = SoundIndex.PhantomSoldierDie;
                    break;
                case MonsterImage.MutatedOctopus:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out BodyLibrary);
                    BodyShape = 2;
                    AttackSound = SoundIndex.MutatedOctopusAttack;
                    StruckSound = SoundIndex.MutatedOctopusStruck;
                    DieSound = SoundIndex.MutatedOctopusDie;
                    break;
                case MonsterImage.AquaLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out BodyLibrary);
                    BodyShape = 2;
                    AttackSound = SoundIndex.AquaLizardAttack;
                    StruckSound = SoundIndex.AquaLizardStruck;
                    DieSound = SoundIndex.AquaLizardDie;
                    break;
                case MonsterImage.Stomper:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out BodyLibrary);
                    BodyShape = 9;
                    AttackSound = SoundIndex.AquaLizardAttack;
                    StruckSound = SoundIndex.AquaLizardStruck;
                    DieSound = SoundIndex.AquaLizardDie;
                    break;
                case MonsterImage.CrimsonNecromancer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_2, out BodyLibrary);
                    BodyShape = 9;
                    AttackSound = SoundIndex.CrimsonNecromancerAttack;
                    StruckSound = SoundIndex.CrimsonNecromancerStruck;
                    DieSound = SoundIndex.CrimsonNecromancerDie;
                    break;
                case MonsterImage.ChaosKnight:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_2, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.ChaosKnightAttack;
                  //  StruckSound = SoundIndex.ChaosKnightStruck;
                    DieSound = SoundIndex.ChaosKnightDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.BanyaGuard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.PachonTheChaosBringer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_13, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.PachontheChaosbringerAttack;
                    StruckSound = SoundIndex.PachontheChaosbringerStruck;
                    DieSound = SoundIndex.PachontheChaosbringerDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.PachonTheChaosBringer)
                        Frames[frame.Key] = frame.Value;
                    break;
                    
                case MonsterImage.NumaCavalry:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_19, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.NumaCavalryAttack;
                    StruckSound = SoundIndex.NumaCavalryStruck;
                    DieSound = SoundIndex.NumaCavalryDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.BanyaGuard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.NumaHighMage:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_19, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.NumaHighMageAttack;
                    StruckSound = SoundIndex.NumaHighMageStruck;
                    DieSound = SoundIndex.NumaHighMageDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.BanyaGuard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.NumaStoneThrower:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_19, out BodyLibrary);
                    BodyShape = 3;
                    AttackSound = SoundIndex.NumaStoneThrowerAttack;
                    StruckSound = SoundIndex.NumaStoneThrowerStruck;
                    DieSound = SoundIndex.NumaStoneThrowerDie;
                    break;
                case MonsterImage.NumaRoyalGuard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_19, out BodyLibrary);
                    BodyShape = 5;
                    AttackSound = SoundIndex.NumaRoyalGuardAttack;
                    StruckSound = SoundIndex.NumaRoyalGuardStruck;
                    DieSound = SoundIndex.NumaRoyalGuardDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.EmperorSaWoo)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.NumaArmoredSoldier:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_19, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.NumaArmoredSoldierAttack;
                    StruckSound = SoundIndex.NumaArmoredSoldierStruck;
                    DieSound = SoundIndex.NumaArmoredSoldierDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.BanyaGuard)
                        Frames[frame.Key] = frame.Value;
                    break;
                    
                case MonsterImage.IcyRanger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.IcyRangerAttack;
                    StruckSound = SoundIndex.IcyRangerStruck;
                    DieSound = SoundIndex.IcyRangerDie;
                    break;
                case MonsterImage.IcyGoddess:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_18, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.IcyGoddessAttack;
                    StruckSound = SoundIndex.IcyGoddessStruck;
                    DieSound = SoundIndex.IcyGoddessDie;
                    break;
                case MonsterImage.IcySpiritWarrior:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out BodyLibrary);
                    BodyShape = 2;
                    AttackSound = SoundIndex.IcySpiritWarriorAttack;
                    StruckSound = SoundIndex.IcySpiritWarriorStruck;
                    DieSound = SoundIndex.IcySpiritWarriorDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.NumaMage)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.IcySpiritGeneral:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out BodyLibrary);
                    BodyShape = 3;
                    AttackSound = SoundIndex.IcySpiritWarriorAttack;
                    StruckSound = SoundIndex.IcySpiritWarriorStruck;
                    DieSound = SoundIndex.IcySpiritWarriorDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.IcySpiritGeneral)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.GhostKnight:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out BodyLibrary);
                    BodyShape = 4;
                    AttackSound = SoundIndex.GhostKnightAttack;
                    StruckSound = SoundIndex.GhostKnightStruck;
                    DieSound = SoundIndex.GhostKnightDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.EmperorSaWoo)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.IcySpiritSpearman:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out BodyLibrary);
                    BodyShape = 6;
                    AttackSound = SoundIndex.IcySpiritSpearmanAttack;
                    StruckSound = SoundIndex.IcySpiritSpearmanStruck;
                    DieSound = SoundIndex.IcySpiritSpearmanDie;
                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.BanyaGuard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.Werewolf:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out BodyLibrary);
                    BodyShape = 7;
                    AttackSound = SoundIndex.WerewolfAttack;
                    StruckSound = SoundIndex.WerewolfStruck;
                    DieSound = SoundIndex.WerewolfDie;
                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.BanyaGuard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.Whitefang:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out BodyLibrary);
                    BodyShape = 8;
                    AttackSound = SoundIndex.WhitefangAttack;
                    StruckSound = SoundIndex.WhitefangStruck;
                    DieSound = SoundIndex.WhitefangDie;
                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.BanyaGuard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.IcySpiritSolider:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out BodyLibrary);
                    BodyShape = 9;
                    AttackSound = SoundIndex.IcySpiritSoliderAttack;
                    StruckSound = SoundIndex.IcySpiritSoliderStruck;
                    DieSound = SoundIndex.IcySpiritSoliderDie;
                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.BanyaGuard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.WildBoar:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_18, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.WildBoarAttack;
                    StruckSound = SoundIndex.WildBoarStruck;
                    DieSound = SoundIndex.WildBoarDie;
                    break;
                case MonsterImage.JinamStoneGate:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out BodyLibrary);
                    BodyShape = 9;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.JinamStoneGate)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.FrostLordHwa:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out BodyLibrary);
                    BodyShape = 5;
                    AttackSound = SoundIndex.FrostLordHwaAttack;
                    StruckSound = SoundIndex.FrostLordHwaStruck;
                    DieSound = SoundIndex.FrostLordHwaDie;
                    break;
                case MonsterImage.Companion_Pig:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out BodyLibrary);
                    BodyShape = 0;
                    break;
                case MonsterImage.Companion_TuskLord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out BodyLibrary);
                    BodyShape = 1;
                    break;
                case MonsterImage.Companion_SkeletonLord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out BodyLibrary);
                    BodyShape = 2;
                    break;
                case MonsterImage.Companion_Griffin:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out BodyLibrary);
                    BodyShape = 3;
                    break;
                case MonsterImage.Companion_Dragon:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out BodyLibrary);
                    BodyShape = 4;
                    break;
                case MonsterImage.Companion_Donkey:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out BodyLibrary);
                    BodyShape = 5;
                    break;
                case MonsterImage.Companion_Sheep:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out BodyLibrary);
                    BodyShape = 6;
                    break;
                case MonsterImage.Companion_BanyoLordGuzak:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out BodyLibrary);
                    BodyShape = 7;
                    break;
                case MonsterImage.Companion_Panda:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out BodyLibrary);
                    BodyShape = 8;
                    break;
                case MonsterImage.Companion_Rabbit:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out BodyLibrary);
                    BodyShape = 9;
                    break;
                case MonsterImage.InfernalSoldier:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_26, out BodyLibrary);
                    BodyShape = 2;
                    break;
                case MonsterImage.OmaWarlord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out BodyLibrary);
                    BodyShape = 7;

                    AttackSound = SoundIndex.OmaHeroAttack;
                    StruckSound = SoundIndex.OmaHeroStruck;
                    DieSound = SoundIndex.OmaHeroDie;
                    break;
                case MonsterImage.EscortCommander:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_22, out BodyLibrary);
                    BodyShape = 0;

                    AttackSound = SoundIndex.EscortCommanderAttack;
                    StruckSound = SoundIndex.EscortCommanderStruck;
                    DieSound = SoundIndex.EscortCommanderDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.BanyaGuard)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.FieryDancer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_22, out BodyLibrary);
                    BodyShape = 2;

                    AttackSound = SoundIndex.FieryDancerAttack;
                    StruckSound = SoundIndex.FieryDancerStruck;
                    DieSound = SoundIndex.FieryDancerDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.FieryDancer)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.EmeraldDancer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_22, out BodyLibrary);
                    BodyShape = 3;

                    AttackSound = SoundIndex.EmeraldDancerAttack;
                    StruckSound = SoundIndex.EmeraldDancerStruck;
                    DieSound = SoundIndex.EmeraldDancerDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.EmeraldDancer)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.QueenOfDawn:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_22, out BodyLibrary);
                    BodyShape = 1;

                    AttackSound = SoundIndex.QueenOfDawnAttack;
                    StruckSound = SoundIndex.QueenOfDawnStruck;
                    DieSound = SoundIndex.QueenOfDawnDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.QueenOfDawn)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.OYoungBeast:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out BodyLibrary);
                    BodyShape = 3;

                    AttackSound = SoundIndex.OYoungBeastAttack;
                    StruckSound = SoundIndex.OYoungBeastStruck;
                    DieSound = SoundIndex.OYoungBeastDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.OYoungBeast)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.YumgonWitch:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out BodyLibrary);
                    BodyShape = 6;

                    AttackSound = SoundIndex.YumgonWitchAttack;
                    StruckSound = SoundIndex.YumgonWitchStruck;
                    DieSound = SoundIndex.YumgonWitchDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.YumgonWitch)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.MaWarlord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out BodyLibrary);
                    BodyShape = 4;

                    AttackSound = SoundIndex.MaWarlordAttack;
                    StruckSound = SoundIndex.MaWarlordStruck;
                    DieSound = SoundIndex.MaWarlordDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.OYoungBeast)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.JinhwanSpirit:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out BodyLibrary);
                    BodyShape = 7;

                    AttackSound = SoundIndex.JinhwanSpiritAttack;
                    StruckSound = SoundIndex.JinhwanSpiritStruck;
                    DieSound = SoundIndex.JinhwanSpiritDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.JinhwanSpirit)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.JinhwanGuardian:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out BodyLibrary);
                    BodyShape = 8;

                    AttackSound = SoundIndex.JinhwanGuardianAttack;
                    StruckSound = SoundIndex.JinhwanGuardianStruck;
                    DieSound = SoundIndex.JinhwanGuardianDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.JinhwanSpirit)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.YumgonGeneral:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out BodyLibrary);
                    BodyShape = 5;

                    AttackSound = SoundIndex.YumgonGeneralAttack;
                    StruckSound = SoundIndex.YumgonGeneralStruck;
                    DieSound = SoundIndex.YumgonGeneralDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.OYoungBeast)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.ChiwooGeneral:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out BodyLibrary);
                    BodyShape = 0;

                    AttackSound = SoundIndex.ChiwooGeneralAttack;
                    StruckSound = SoundIndex.ChiwooGeneralStruck;
                    DieSound = SoundIndex.ChiwooGeneralDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.ChiwooGeneral)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.DragonQueen:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out BodyLibrary);
                    BodyShape = 2;

                    AttackSound = SoundIndex.DragonQueenAttack;
                    StruckSound = SoundIndex.DragonQueenStruck;
                    DieSound = SoundIndex.DragonQueenDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.DragonQueen)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.DragonLord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out BodyLibrary);
                    BodyShape = 1;

                    AttackSound = SoundIndex.DragonLordAttack;
                    StruckSound = SoundIndex.DragonLordStruck;
                    DieSound = SoundIndex.DragonLordDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.DragonLord)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.FerociousIceTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out BodyLibrary);
                    BodyShape = 1;

                    AttackSound = SoundIndex.FerociousIceTigerAttack;
                    StruckSound = SoundIndex.FerociousIceTigerStruck;
                    DieSound = SoundIndex.FerociousIceTigerDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.FerociousIceTiger)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SamaFireGuardian:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out BodyLibrary);
                    BodyShape = 0;

                    AttackSound = SoundIndex.SamaFireGuardianAttack;
                    StruckSound = SoundIndex.SamaFireGuardianStruck;
                    DieSound = SoundIndex.SamaFireGuardianDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SamaFireGuardian)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SamaIceGuardian:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out BodyLibrary);
                    BodyShape = 1;

                    AttackSound = SoundIndex.SamaIceGuardianAttack;
                    StruckSound = SoundIndex.SamaIceGuardianStruck;
                    DieSound = SoundIndex.SamaIceGuardianDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SamaFireGuardian)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SamaLightningGuardian:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out BodyLibrary);
                    BodyShape = 2;

                    AttackSound = SoundIndex.SamaLightningGuardianAttack;
                    StruckSound = SoundIndex.SamaLightningGuardianStruck;
                    DieSound = SoundIndex.SamaLightningGuardianDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SamaFireGuardian)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SamaWindGuardian:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out BodyLibrary);
                    BodyShape = 3;

                    AttackSound = SoundIndex.SamaWindGuardianAttack;
                    StruckSound = SoundIndex.SamaWindGuardianStruck;
                    DieSound = SoundIndex.SamaWindGuardianDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SamaFireGuardian)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.Phoenix:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out BodyLibrary);
                    BodyShape = 4;

                    AttackSound = SoundIndex.PhoenixAttack;
                    StruckSound = SoundIndex.PhoenixStruck;
                    DieSound = SoundIndex.PhoenixDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.Phoenix)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.BlackTortoise:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out BodyLibrary);
                    BodyShape = 5;

                    AttackSound = SoundIndex.BlackTortoiseAttack;
                    StruckSound = SoundIndex.BlackTortoiseStruck;
                    DieSound = SoundIndex.BlackTortoiseDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.Phoenix)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.BlueDragon:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out BodyLibrary);
                    BodyShape = 6;

                    AttackSound = SoundIndex.BlueDragonAttack;
                    StruckSound = SoundIndex.BlueDragonStruck;
                    DieSound = SoundIndex.BlueDragonDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.Phoenix)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.WhiteTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out BodyLibrary);
                    BodyShape = 7;

                    AttackSound = SoundIndex.WhiteTigerAttack;
                    StruckSound = SoundIndex.WhiteTigerStruck;
                    DieSound = SoundIndex.WhiteTigerDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.Phoenix)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.EnshrinementBox:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_27, out BodyLibrary);
                    BodyShape = 5;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.EnshrinementBox)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.BloodStone:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_19, out BodyLibrary);
                    BodyShape = 7;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.BloodStone)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SamaCursedBladesman:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_27, out BodyLibrary);
                    BodyShape = 0;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SamaCursedBladesman)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SamaCursedSlave:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_27, out BodyLibrary);
                    BodyShape = 1;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SamaCursedSlave)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SamaCursedFlameMage:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_27, out BodyLibrary);
                    BodyShape = 2;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SamaCursedSlave)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SamaProphet:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_27, out BodyLibrary);
                    BodyShape = 3;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SamaProphet)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SamaSorcerer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_27, out BodyLibrary);
                    BodyShape = 4;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SamaSorcerer)
                        Frames[frame.Key] = frame.Value;
                    break;

                case MonsterImage.OrangeTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out BodyLibrary);
                    BodyShape = 0;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.OrangeTiger)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.RegularTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out BodyLibrary);
                    BodyShape = 1;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.OrangeTiger)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.RedTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out BodyLibrary);
                    BodyShape = 2;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.RedTiger)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SnowTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out BodyLibrary);
                    BodyShape = 3;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.OrangeTiger)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.BlackTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out BodyLibrary);
                    BodyShape = 4;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.OrangeTiger)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.BigBlackTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out BodyLibrary);
                    BodyShape = 5;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.OrangeTiger)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.BigWhiteTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out BodyLibrary);
                    BodyShape = 6;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.OrangeTiger)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.OrangeBossTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out BodyLibrary);
                    BodyShape = 7;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.OrangeBossTiger)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.BigBossTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out BodyLibrary);
                    BodyShape = 8;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.OrangeBossTiger)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.WildMonkey:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_30, out BodyLibrary);
                    BodyShape = 0;
                    AttackSound = SoundIndex.MonkeyAttack;
                    StruckSound = SoundIndex.MonkeyStruck;
                    DieSound = SoundIndex.MonkeyDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.Monkey)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.FrostYeti:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_30, out BodyLibrary);
                    BodyShape = 1;
                    AttackSound = SoundIndex.ForestYetiAttack;
                    StruckSound = SoundIndex.ForestYetiStruck;
                    DieSound = SoundIndex.ForestYetiDie;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.ForestYeti)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.EvilSnake:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_9, out BodyLibrary);
                    BodyShape = 0;

                     AttackSound = SoundIndex.ClawSerpentAttack;
                     StruckSound = SoundIndex.ClawSerpentStruck;
                     DieSound = SoundIndex.ClawSerpentDie;
                    break;
                case MonsterImage.Salamander:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_28, out BodyLibrary);
                    BodyShape = 0;

                    break;
                case MonsterImage.SandGolem:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_28, out BodyLibrary);
                    BodyShape = 1;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SDMob3)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SDMob4:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_29, out BodyLibrary);
                    BodyShape = 0;

                    break;
                case MonsterImage.SDMob5:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_29, out BodyLibrary);
                    BodyShape = 1;

                    break;
                case MonsterImage.SDMob6:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_29, out BodyLibrary);
                    BodyShape = 2;

                    break;
                case MonsterImage.SDMob7:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_29, out BodyLibrary);
                    BodyShape = 8;

                    break;
                case MonsterImage.OmaMage:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_29, out BodyLibrary);
                    BodyShape = 9;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SDMob8)
                        Frames[frame.Key] = frame.Value; 
                    break;
                case MonsterImage.SDMob9:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out BodyLibrary);
                    BodyShape = 1;

                    break;
                case MonsterImage.SDMob10:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out BodyLibrary);
                    BodyShape = 5;

                    break;
                case MonsterImage.SDMob11:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out BodyLibrary);
                    BodyShape = 6;

                    break;
                case MonsterImage.SDMob12:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out BodyLibrary);
                    BodyShape = 7;

                    break;
                case MonsterImage.SDMob13:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out BodyLibrary);
                    BodyShape = 8;

                    break;
                case MonsterImage.SDMob14:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out BodyLibrary);
                    BodyShape = 9;

                    break;
                case MonsterImage.CrystalGolem:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_40, out BodyLibrary);
                    BodyShape = 0;
                     

                     foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SDMob15)
                         Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.DustDevil:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_41, out BodyLibrary);
                    BodyShape = 1;


                     foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SDMob16)
                         Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.TwinTailScorpion:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_41, out BodyLibrary);
                    BodyShape = 2;


                     foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SDMob17)
                         Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.BloodyMole:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_41, out BodyLibrary);
                    BodyShape = 3;
                    

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SDMob18)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SDMob19:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_44, out BodyLibrary); 
                    BodyShape = 3;


                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SDMob19)
                        Frames[frame.Key] = frame.Value;

                    break;
                case MonsterImage.SDMob20:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_44, out BodyLibrary);
                    BodyShape = 4;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SDMob19)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SDMob21:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_44, out BodyLibrary);
                    BodyShape = 5;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SDMob21)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SDMob22:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_44, out BodyLibrary);
                    BodyShape = 6;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SDMob22)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SDMob23:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_44, out BodyLibrary);
                    BodyShape = 7;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SDMob23)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SDMob24:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_44, out BodyLibrary);
                    BodyShape = 8;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SDMob24)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SDMob25:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_44, out BodyLibrary);
                    BodyShape = 9;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SDMob25)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.SDMob26:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_45, out BodyLibrary);
                    BodyShape = 0;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.SDMob26)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.GangSpider:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_28, out BodyLibrary);
                    BodyShape = 8;

                    break;
                case MonsterImage.VenomSpider:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_28, out BodyLibrary);
                    BodyShape = 9;

                    break;
                case MonsterImage.LobsterLord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_45, out BodyLibrary);
                    BodyShape = 3;


                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.LobsterLord)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.NewMob1:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out BodyLibrary);
                    BodyShape = 0;
                    
                    break;
                case MonsterImage.NewMob2:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out BodyLibrary);
                    BodyShape = 1;

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.BobbitWorm)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.NewMob3:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out BodyLibrary);
                    BodyShape = 2;



                    break;
                case MonsterImage.NewMob4:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out BodyLibrary);
                    BodyShape = 3;



                    /*
                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.LobsterLord)
                        Frames[frame.Key] = frame.Value;*/
                    break;
                case MonsterImage.NewMob5:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out BodyLibrary);
                    BodyShape = 4;



                    /*
                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.LobsterLord)
                        Frames[frame.Key] = frame.Value;*/
                    break;
                case MonsterImage.NewMob6:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out BodyLibrary);
                    BodyShape = 5;



                    /*
                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.LobsterLord)
                        Frames[frame.Key] = frame.Value;*/
                    break;
                case MonsterImage.NewMob7:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out BodyLibrary);
                    BodyShape = 6;



                    /*
                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.LobsterLord)
                        Frames[frame.Key] = frame.Value;*/
                    break;
                case MonsterImage.NewMob8:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out BodyLibrary);
                    BodyShape = 7;



                    /*
                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.LobsterLord)
                        Frames[frame.Key] = frame.Value;*/
                    break;
                case MonsterImage.NewMob9:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out BodyLibrary);
                    BodyShape = 8;



                    /*
                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.LobsterLord)
                        Frames[frame.Key] = frame.Value;*/
                    break;
                case MonsterImage.NewMob10:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out BodyLibrary);
                    BodyShape = 9;


                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.DeadTree)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.MonasteryMon0:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_49, out BodyLibrary);
                    BodyShape = 0;

                    break;
                case MonsterImage.MonasteryMon1:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_49, out BodyLibrary);
                    BodyShape = 1;


                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.MonasteryMon1)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.MonasteryMon2:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_49, out BodyLibrary);
                    BodyShape = 2;

                    break;
                case MonsterImage.MonasteryMon3:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_49, out BodyLibrary);
                    BodyShape = 3;

                    //Fucked up mob

                    foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.MonasteryMon3)
                        Frames[frame.Key] = frame.Value;
                    break;
                case MonsterImage.MonasteryMon4:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_49, out BodyLibrary);
                    BodyShape = 4;

                    break;
                case MonsterImage.MonasteryMon5:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_49, out BodyLibrary);
                    BodyShape = 5;


                    break;
                case MonsterImage.MonasteryMon6:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_49, out BodyLibrary);
                    BodyShape = 6;

                    //Fuckjed up Mob

                    break;
                default:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out BodyLibrary);
                    BodyShape = 0;
                    break;
            }

            if (EasterEvent)
            {
                CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_30, out BodyLibrary);
                BodyShape = 4;

                Frames = new Dictionary<MirAnimation, Frame>(FrameSet.DefaultMonster);

                foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.EasterEvent)
                    Frames[frame.Key] = frame.Value;
            }
            else if (HalloweenEvent)
            {
                CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out BodyLibrary);
                BodyShape = 1;

                Frames = new Dictionary<MirAnimation, Frame>(FrameSet.DefaultMonster);
            }
            else if (ChristmasEvent)
            {
                CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out BodyLibrary);
                BodyShape = 0;

                Frames = new Dictionary<MirAnimation, Frame>(FrameSet.DefaultMonster);
            }
        }

        public void SetScale()
        {
            int sizePercent = Stats[Stat.SizePercent];

            GrowthLevel = Math.Min(Globals.MaxGrowthLevel, Math.Max(0, Stats[Stat.GrowthLevel]));

            if (GrowthLevel > 0)
                sizePercent = GrowthLevel * 5;

            Scale = (float)(100 + Math.Min(20, Math.Max(-20, sizePercent))) / 100;
        }

        public override void SetAnimation(ObjectAction action)
        {
            MirAnimation animation;
            MagicType type;

            switch (action.Action)
            {
                case MirAction.Standing:
                    switch (Image)
                    {
                            case MonsterImage.ZumaGuardian:
                            case MonsterImage.ZumaFanatic:
                            case MonsterImage.ZumaKing:
                            animation = !Extra ? MirAnimation.StoneStanding : MirAnimation.Standing;
                            break;
                        default:
                                animation = MirAnimation.Standing;

                            if (VisibleBuffs.Contains(BuffType.DragonRepulse))
                                animation = MirAnimation.DragonRepulseMiddle;
                            else if (CurrentAnimation == MirAnimation.DragonRepulseMiddle)
                                animation = MirAnimation.DragonRepulseEnd;
                            break;
                    }
                    break;
                case MirAction.Moving:
                    animation = MirAnimation.Walking;
                    break;
                case MirAction.Pushed:
                    animation = MirAnimation.Pushed;
                    break;
                case MirAction.Attack:
                    animation = MirAnimation.Combat1;
                    break;
                case MirAction.RangeAttack:
                    animation = MirAnimation.Combat2;
                    break;
                case MirAction.Spell:
                    type = (MagicType)action.Extra[0];
                    
                    animation = MirAnimation.Combat3;

                    if (type == MagicType.DragonRepulse)
                        animation = MirAnimation.DragonRepulseStart;

                    switch (type)
                    {
                        case MagicType.DoomClawRightPinch:
                            animation = MirAnimation.Combat1;
                            break;
                        case MagicType.DoomClawRightSwipe:
                            animation = MirAnimation.Combat2;
                            break;
                        case MagicType.DoomClawSpit:
                            animation = MirAnimation.Combat7;
                            break;
                        case MagicType.DoomClawWave:
                            animation = MirAnimation.Combat6;
                            break;
                        case MagicType.DoomClawLeftPinch:
                            animation = MirAnimation.Combat4;
                            break;
                        case MagicType.DoomClawLeftSwipe:
                            animation = MirAnimation.Combat5;
                            break;
                    }
                    break;
             //   case MirAction.Struck:
             //       animation = MirAnimation.Struck;
             //       break;
                case MirAction.Die:
                    animation = MirAnimation.Die;
                    break;
                case MirAction.Dead:
                    animation = !Skeleton ? MirAnimation.Dead : MirAnimation.Skeleton;
                    break;
                case MirAction.Show:
                    animation = MirAnimation.Show;
                    break;
                case MirAction.Hide:
                    animation = MirAnimation.Hide;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CurrentAnimation = animation;
            if (!Frames.TryGetValue(CurrentAnimation, out CurrentFrame))
                CurrentFrame = Frame.EmptyFrame;
        }

        public override void Draw()
        {
            if (BodyLibrary == null || !Visible) return;

            int y = DrawY;

            switch (Image)
            {
                case MonsterImage.ChestnutTree:
                    y -= GameScene.Game.MapControl.CellHeight;
                    break;
                case MonsterImage.NewMob10:
                    y -= GameScene.Game.MapControl.CellHeight * 4;
                    break;
            }

            DrawShadow(DrawX, y);


            DrawBody(DrawX, y);
        }

        public void DrawShadow(int x, int y)
        {
            switch (Image)
            {
                case MonsterImage.DustDevil:
                    break;
                case MonsterImage.LobsterLord:
                    BodyLibrary.Draw(BodyFrame, x, y, Color.White, true, 0.5f, ImageType.Shadow, Scale);
                    BodyLibrary.Draw(BodyFrame + 1000, x, y, Color.White, true, 0.5f, ImageType.Shadow, Scale);
                    BodyLibrary.Draw(BodyFrame + 2000, x, y, Color.White, true, 0.5f, ImageType.Shadow, Scale);
                    break;
                default:
                    BodyLibrary.Draw(BodyFrame, x, y, Color.White, true, 0.5f, ImageType.Shadow, Scale);
                    break;
            }

        }
        public void DrawBody(int x, int y)
        {
            switch (Image)
            {
                case MonsterImage.DustDevil:
                    BodyLibrary.DrawBlend(BodyFrame, x, y, DrawColour, true, Opacity, ImageType.Image);
                    break;
                case MonsterImage.LobsterLord:
                    BodyLibrary.Draw(BodyFrame, x, y, DrawColour, true, Opacity, ImageType.Image, Scale);
                    BodyLibrary.Draw(BodyFrame + 1000, x, y, DrawColour, true, Opacity, ImageType.Image, Scale);
                    BodyLibrary.Draw(BodyFrame + 2000, x, y, DrawColour, true, Opacity, ImageType.Image, Scale);
                    break;
                default:
                    BodyLibrary.Draw(BodyFrame, x, y, DrawColour, true, Opacity, ImageType.Image, Scale);
                    break;
            }


            MirLibrary library;
            switch (Image)
            {
                case MonsterImage.NewMob1:
                    if (CurrentAction == MirAction.Dead) break;
                    if (!CEnvir.LibraryList.TryGetValue(LibraryFile.MonMagicEx20, out library)) break;
                    library.DrawBlend(DrawFrame + 2000, x, y, Color.White, true, 1f, ImageType.Image);
                    break;
                case MonsterImage.NumaHighMage:
                    if (CurrentAction == MirAction.Dead) break;
                    if (!CEnvir.LibraryList.TryGetValue(LibraryFile.MonMagicEx4, out library)) break;
                    library.DrawBlend(DrawFrame + 500, x, y, Color.White, true, 1f, ImageType.Image);
                    break;
                case MonsterImage.InfernalSoldier:
                    if (CurrentAction == MirAction.Dead) break;
                    if (!CEnvir.LibraryList.TryGetValue(LibraryFile.MonMagicEx8, out library)) break;
                    library.DrawBlend(DrawFrame, x, y, Color.White, true, 1f, ImageType.Image);
                    break;
                case MonsterImage.JinamStoneGate:
                    if (CurrentAction == MirAction.Dead) break;
                    if (!CEnvir.LibraryList.TryGetValue(LibraryFile.MonMagicEx6, out library)) break;
                    library.DrawBlend((GameScene.Game.MapControl.Animation % 30) + 1400, x, y, Color.White, true, 1f, ImageType.Image);
                    break;
            }

            if (CompanionObject != null && CompanionObject.HeadShape > 0)
            {
                switch (Image)
                {
                    case MonsterImage.Companion_Pig:
                        if (!CEnvir.LibraryList.TryGetValue(LibraryFile.PEquipH1, out library)) break;
                        library.Draw(DrawFrame + (CompanionObject.HeadShape * 1000), x, y, Color.White, true, 1f, ImageType.Image, Scale);
                        break;
                }
            }

            if (CompanionObject != null && CompanionObject.BackShape > 0)
            {
                switch (Image)
                {
                    case MonsterImage.Companion_Pig:
                        if (!CEnvir.LibraryList.TryGetValue(LibraryFile.PEquipB1, out library)) break;
                        library.Draw(DrawFrame + (CompanionObject.BackShape * 1000), x, y, Color.White, true, 1f, ImageType.Image, Scale);
                        break;
                }
            }
        }
        public override void DrawHealth()
        {
            if (!Config.ShowMonsterHealth || ((CEnvir.Now > DrawHealthTime || !Visible) && PetOwner != User.Name)) return;

            if (MonsterInfo.AI < 0) return;

            ClientObjectData data;
            if (!GameScene.Game.DataDictionary.TryGetValue(ObjectID, out data)) return;

            if (data.MaxHealth == 0) return;

            MirLibrary library;

            if (!CEnvir.LibraryList.TryGetValue(LibraryFile.Interface, out library)) return;

            float percent = Math.Min(1, Math.Max(0, data.Health / (float)data.MaxHealth));

            if (percent == 0) return;

            Size size = library.GetSize(79);

            Color color = !string.IsNullOrEmpty(PetOwner) ? Color.Yellow : Color.FromArgb(0, 200, 74);

            library.Draw(80, DrawX, DrawY - 55, Color.White, false, 1F, ImageType.Image);
            library.Draw(79, DrawX + 1, DrawY - 55 + 1, color, new Rectangle(0, 0, (int)(size.Width * percent), size.Height), 1F, ImageType.Image);
        }

        public override void DrawBlend()
        {
            if (BodyLibrary == null || !Visible) return;

            int y = DrawY;

            switch (Image)
            {
                case MonsterImage.ChestnutTree:
                    y -= GameScene.Game.MapControl.CellHeight;
                    break;
                    case MonsterImage.JinamStoneGate:
                    return;
            }
            DXManager.SetBlend(true, 0.60F, BlendMode.HIGHLIGHT);
            DrawBody(DrawX, y);
            DXManager.SetBlend(false);
        }
        public override void DrawName()
        {
            if (!Visible) return;

            base.DrawName();
        }

        public override void CreateProjectile()
        {
            base.CreateProjectile();

            switch (Image)
            {
                case MonsterImage.SkeletonAxeThrower:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(800, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                        });
                    }
                    break;

                case MonsterImage.AntNeedler:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(80, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Skip = 0,
                        });
                    }
                    break;

                case MonsterImage.AntHealer:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirEffect(100, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 40, Globals.HolyColour)
                        {
                            Target = attackTarget,
                            Skip = 0,
                            Blend = true,
                        });
                    }
                    break;

                case MonsterImage.SpinedDarkLizard:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(1240, 1, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Skip = 10,
                        });
                    }
                    break;

                case MonsterImage.RedMoonTheFallen:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirEffect(2230, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour)
                        {
                            MapTarget = attackTarget.CurrentLocation,
                            Skip = 0,
                        });
                    }
                    break;

                case MonsterImage.ZumaSharpShooter:
                case MonsterImage.BoneArcher:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(1070, 1, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Skip = 10,
                        });
                    }
                    break;
                case MonsterImage.Monkey:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(900, 1, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx2, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Skip = 10,
                            Has16Directions = false,
                        });
                    }
                    break;
                case MonsterImage.CannibalFanatic:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(0, 1, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx2, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                        });
                    }
                    break;
                case MonsterImage.CursedCactus:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(960, 1, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Skip = 0,
                        });
                    }
                    break;
                case MonsterImage.WindfurySorceress:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirEffect(1570, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 60, Globals.WindColour)
                        {
                            Target = attackTarget,
                            Blend = true,
                        });
                    }
                    break;
                case MonsterImage.SonicLizard:
                    Effects.Add(new MirEffect(1444, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx4, 20, 60, Globals.FireColour)
                    {
                        Target = this,
                        Blend = true,
                        Direction = Direction,

                    });
                    break;
                case MonsterImage.GiantLizard:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirEffect(5930, 4, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx4, 0, 0, Globals.NoneColour)
                        {
                            MapTarget = attackTarget.CurrentLocation,
                        });
                    }
                    break;
                case MonsterImage.CrazedLizard:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(5830, 3, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx4, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                            Blend = true,
                        });
                    }
                    break;
                case MonsterImage.EmperorSaWoo:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirEffect(600, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 60, 60, Globals.WindColour)
                        {
                            MapTarget = attackTarget.CurrentLocation,
                            Blend = true,
                        });
                    }
                    break;
                case MonsterImage.ArchLichTaedu:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(420, 5, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 30, 50, Globals.FireColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Blend = true,
                        });
                    }
                    break;
                case MonsterImage.RazorTusk:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirEffect(1890, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 30, 50, Globals.WindColour)
                        {
                            MapTarget = attackTarget.CurrentLocation,
                            Blend = true,
                            Direction = attackTarget.Direction,
                            BlendRate = 1F,
                        });
                    }
                    break;
                case MonsterImage.MutantCaptain:
                    Effects.Add(new MirEffect(560, 9, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx2, 20, 60, Globals.FireColour)
                    {
                        Target = this,
                        Blend = true,
                        Direction = Direction,
                    });
                    break;
                case MonsterImage.StoneGriffin:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(1080, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx2, 0, 0, Globals.DarkColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Blend = true,
                        });
                    }
                    break;
                case MonsterImage.FlameGriffin:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(1080, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx2, 0, 0, Globals.FireColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Blend = true,
                            DrawColour = Color.Orange,
                        });
                    }
                    break;
                case MonsterImage.NumaCavalry:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(0, 4, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx4, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                            Skip = 10,
                        });
                    }
                    break;
                case MonsterImage.NumaStoneThrower:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        MirEffect effect;
                        Effects.Add(effect = new MirProjectile(0, 4, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx3, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                            Skip = 10,
                        });

                        effect.CompleteAction = () =>
                        {
                            attackTarget.Effects.Add(effect = new MirEffect(80, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx3, 10, 35, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = attackTarget,
                            });
                            effect.Process();

                            DXSoundManager.Play(SoundIndex.FireStormEnd);
                        };
                        effect.Process();
                    }
                    break;
                case MonsterImage.NumaRoyalGuard:
                    Effects.Add(new MirEffect(1440, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx4, 20, 60, Globals.FireColour)
                    {
                        Target = this,
                        Blend = true,
                        Direction = Direction,
                    });
                    break;
                case MonsterImage.IcyGoddess:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(6200, 5, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx3, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                            Skip = 0,
                            Blend = true,
                        });
                    }
                    break;
                case MonsterImage.IcySpiritGeneral:
                case MonsterImage.IcySpiritWarrior:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirEffect(580, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx3, 0, 0, Globals.NoneColour)
                        {
                            Target = attackTarget,
                            Blend = true,
                        });
                    }
                    break;
                case MonsterImage.EvilElephant:
                case MonsterImage.SandShark:
                    Effects.Add(new MirEffect(320, 10, TimeSpan.FromMilliseconds(80), LibraryFile.MonMagic, 10, 35, Globals.DarkColour)
                    {
                        Blend = true,
                        Target = this,
                    });
                    break;
                case MonsterImage.GhostKnight:
                    Effects.Add(new MirEffect(6350, 10, TimeSpan.FromMilliseconds(80), LibraryFile.MonMagicEx3, 10, 35, Globals.DarkColour)
                    {
                        Blend = true,
                        Target = this,
                    });
                    break;
                case MonsterImage.IcyRanger:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(190, 1, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx3, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                        });
                    }
                    break;
                case MonsterImage.YumgonWitch:
                    Effects.Add(new MirEffect(20, 18, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx6, 20, 60, Globals.LightningColour)
                    {
                        Target = this,
                        Blend = true,
                    });
                    break;
                case MonsterImage.ChiwooGeneral:
                    Effects.Add(new MirEffect(1000, 15, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx6, 0, 0, Globals.NoneColour)
                    {
                        Target = this,
                        Blend = true,
                    });
                    break;
                case MonsterImage.DragonLord:
                    foreach (MapObject target in AttackTargets)
                    {
                        MirProjectile eff;
                        Point p = new Point(target.CurrentLocation.X +4, target.CurrentLocation.Y - 10);
                        Effects.Add(eff = new MirProjectile(130, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx6, 0, 0, Globals.NoneColour, p)
                        {
                            MapTarget = target.CurrentLocation,
                            Skip = 0,
                            Explode = true,
                            Blend =  true,
                        });

                        eff.CompleteAction = () =>
                        {
                            Effects.Add(new MirEffect(140, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx6, 0, 0, Globals.NoneColour)
                            {
                                MapTarget = eff.MapTarget,
                                Blend = true,
                            });
                        };
                    }
                    break;
                case MonsterImage.SamaCursedFlameMage:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        MirEffect effect;
                        Effects.Add(effect = new MirProjectile(5000, 9, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx9, 0, 0, Globals.FireColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                            Skip = 10,
                            Blend = true,
                        });

                        effect.CompleteAction = () =>
                        {
                            attackTarget.Effects.Add(effect = new MirEffect(5100, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx9, 10, 35, Globals.FireColour)
                            {
                                Blend = true,
                                Target = attackTarget,
                            });
                            effect.Process();
                        };
                        effect.Process();
                    }
                    break;
            }

        }

        public override bool MouseOver(Point p)
        {
            if (!Visible || BodyLibrary == null) return false;

            switch (Image)
            {
                case MonsterImage.LobsterLord:
                        return BodyLibrary.VisiblePixel(BodyFrame, new Point(p.X - DrawX, p.Y - DrawY), false, true) ||
                               BodyLibrary.VisiblePixel(BodyFrame + 1000, new Point(p.X - DrawX, p.Y - DrawY), false, true) ||
                               BodyLibrary.VisiblePixel(BodyFrame + 2000, new Point(p.X - DrawX, p.Y - DrawY), false, true);
                default:
                    return BodyLibrary.VisiblePixel(BodyFrame, new Point(p.X - DrawX, p.Y - DrawY), false, true);
            }

        }

        public override void SetAction(ObjectAction action)
        {
            switch (Image)
            {
                case MonsterImage.Shinsu:
                    switch (CurrentAction) // Old Action
                    {
                        case MirAction.Hide:
                            Extra = false;
                            UpdateLibraries();
                            break;
                        case MirAction.Dead:
                            Visible = true;
                            break;
                    }
                    switch (action.Action) //Mew Action
                    {
                        case MirAction.Show:
                            Extra = true;
                            DXSoundManager.Play(SoundIndex.ShinsuShow);
                            UpdateLibraries();
                            break;
                        case MirAction.Hide:
                            DXSoundManager.Play(SoundIndex.ShinsuBigAttack);
                            break;
                        case MirAction.Dead:
                            Visible = false;
                            break;
                    }
                    break;
                case MonsterImage.InfernalSoldier:
                    switch (CurrentAction) // Old Action
                    {
                        case MirAction.Dead:
                            Visible = true;
                            break;
                    }
                    switch (action.Action) //Mew Action
                    {
                        case MirAction.Dead:
                            Visible = false;
                            break;
                    }
                    break;
                case MonsterImage.CarnivorousPlant:
                case MonsterImage.LordNiJae:
                    if (CurrentAction == MirAction.Hide)
                        Visible = false;

                    if (action.Action == MirAction.Show)
                        Visible = true;
                    break;
                case MonsterImage.GhostMage:
                    switch (action.Action)
                    {
                        case MirAction.Show:
                            DXSoundManager.Play(SoundIndex.GhostMageAppear);
                            new MirEffect(240, 1, TimeSpan.FromMinutes(1), LibraryFile.ProgUse, 0, 0, Globals.NoneColour)
                            {
                                MapTarget = action.Location,
                                DrawType = DrawType.Floor,
                                Direction = Direction,
                                Skip = 1,
                            };
                            break;
                    }
                    break;
                case MonsterImage.StoneGolem:
                    switch (action.Action)
                    {
                        case MirAction.Show:
                            DXSoundManager.Play(SoundIndex.StoneGolemAppear);
                            new MirEffect(200, 1, TimeSpan.FromMinutes(1), LibraryFile.ProgUse, 0, 0, Globals.NoneColour)
                            {
                                MapTarget = action.Location,
                                DrawType = DrawType.Floor,
                                Direction = Direction,
                                Skip = 1,
                            };
                            break;
                    }
                    break;
                case MonsterImage.ZumaFanatic:
                case MonsterImage.ZumaGuardian:
                    switch (CurrentAction) 
                    {
                        case MirAction.Show:
                            Extra = true;
                            break;
                    }
                    break;
                case MonsterImage.ZumaKing:
                    switch (CurrentAction)
                    {
                        case MirAction.Show:
                            Extra = true;
                            new MirEffect(210, 1, TimeSpan.FromMinutes(1), LibraryFile.ProgUse, 0, 0, Globals.NoneColour)
                            {
                                MapTarget = action.Location,
                                DrawType = DrawType.Floor,
                            };
                            break;
                    }
                    break;
            }

            base.SetAction(action);

            switch (Image)
            {
                case MonsterImage.Scarecrow:
                    switch (action.Action)
                    {
                        case MirAction.Die:
                            Effects.Add(new MirEffect(680, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 40, Globals.FireColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.Skeleton:
                case MonsterImage.SkeletonAxeThrower:
                case MonsterImage.SkeletonWarrior:
                case MonsterImage.SkeletonLord:
                    switch (action.Action)
                    {
                        case MirAction.Die:
                            Effects.Add(new MirEffect(1920, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 40, Globals.FireColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.GhostSorcerer:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(600, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 40, Globals.LightningColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                        case MirAction.Die:
                            Effects.Add(new MirEffect(700, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 40, Globals.LightningColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.CaveMaggot:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(1940, 5, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.DarkColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(200),
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.LordNiJae:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            MirEffect effect;
                            Effects.Add(effect = new MirEffect(361, 9, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.DarkColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(400),
                                Blend = true,
                            });
                            effect.Process();

                            break;
                    }
                    break;
                case MonsterImage.RottingGhoul:
                    switch (action.Action)
                    {
                        case MirAction.Die:
                            Effects.Add(new MirEffect(490, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 20, 40, Globals.LightningColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.DecayingGhoul:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(310, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 20, 40, Globals.LightningColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(400),
                                Blend = true,
                            });
                            break;
                        case MirAction.Die:
                            Effects.Add(new MirEffect(490, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 20, 40, Globals.LightningColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.UmaFlameThrower:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(520, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 40, Globals.FireColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.UmaKing:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(440, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 50, 80, Globals.LightningColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.ZumaKing:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(720, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.FireColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                        case MirAction.Show:
                            DXSoundManager.Play(SoundIndex.ZumaKingAppear);
                            break;
                    }
                    break;
                case MonsterImage.BanyaLeftGuard:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(100, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.FireColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                        case MirAction.Die:
                            Effects.Add(new MirEffect(200, 5, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.FireColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.BanyaRightGuard:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(0, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.LightningColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                        case MirAction.Die:
                            Effects.Add(new MirEffect(90, 5, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.LightningColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.EmperorSaWoo:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(510, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.FireColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.BoneArcher:
                case MonsterImage.BoneSoldier:
                case MonsterImage.BoneBladesman:
                    switch (action.Action)
                    {
                        case MirAction.Die:
                            Effects.Add(new MirEffect(630, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.BoneCaptain:
                    switch (action.Action)
                    {
                        case MirAction.Die:
                            Effects.Add(new MirEffect(650, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.ArchLichTaedu:
                    switch (action.Action)
                    {
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(1470, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                        case MirAction.Show:
                            Effects.Add(new MirEffect(1390, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                        case MirAction.Die:
                            Effects.Add(new MirEffect(1630, 17, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                                Skip = 20,
                            });
                            break;
                    }
                    break;
                case MonsterImage.RazorTusk:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(1800, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 20, 40, Globals.FireColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.Shinsu:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(980, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 40, Globals.PhantomColour)
                            {
                                Target = this,
                                Blend = true,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(400),
                            });
                            break;
                    }
                    break;
                case MonsterImage.Stomper:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(1779, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.PachonTheChaosBringer:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(1800, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                        case MirAction.Die:
                            Effects.Add(new MirEffect(1890, 18, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.JinchonDevil:
                    switch (CurrentAction)
                    {
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(760, 9, TimeSpan.FromMilliseconds(70), LibraryFile.MonMagicEx2, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = Direction,
                            });
                            break;
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(990, 9, TimeSpan.FromMilliseconds(70), LibraryFile.MonMagicEx2, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = Direction,
                            });
                            break;
                    }
                    break;
                case MonsterImage.EmeraldDancer:
                    switch (CurrentAction)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(290, 20, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx5, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = Direction,
                                Skip = 20,
                            });
                            break;
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(540, 20, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx5, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            break;
                    }
                    break;
                case MonsterImage.FieryDancer:
                    switch (CurrentAction)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(570, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx5, 10, 35, Globals.FireColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            break;
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(620, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx5, 10, 35, Globals.FireColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            break;
                    }
                    break;
                case MonsterImage.QueenOfDawn:
                    switch (CurrentAction)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(680, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx5, 10, 35, Globals.HolyColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = Direction,
                            });
                            break;
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(460, 11, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx5, 30, 80, Globals.HolyColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            break;
                    }
                    break;
                case MonsterImage.OYoungBeast:
                    switch (CurrentAction)
                    {
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(600, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx6, 0, 0, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = Direction
                            });
                            break;
                    }
                    break;
                case MonsterImage.MaWarlord:
                    switch (CurrentAction)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(1100, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx6, 0, 0, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = Direction
                            });
                            break;
                    }
                    break;
                case MonsterImage.DragonQueen:
                    switch (CurrentAction)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(500, 20, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx6, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            break;
                    }
                    break;
                case MonsterImage.FerociousIceTiger:
                    switch (CurrentAction)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(700, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx7, 0, 0, Globals.NoneColour)
                            {
                                Blend = true,
                                MapTarget = Functions.Move(CurrentLocation, Direction, 3),
                                StartTime = CEnvir.Now.AddMilliseconds(600)
                            });
                            break;
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(801, 16, TimeSpan.FromMilliseconds(40), LibraryFile.MonMagicEx7, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            Effects.Add(new MirEffect(801, 16, TimeSpan.FromMilliseconds(40), LibraryFile.MonMagicEx7, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                                StartTime = CEnvir.Now.AddMilliseconds(150),
                            });
                            Effects.Add(new MirEffect(801, 16, TimeSpan.FromMilliseconds(40), LibraryFile.MonMagicEx7, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                                StartTime = CEnvir.Now.AddMilliseconds(300),
                            });
                            Effects.Add(new MirEffect(801, 16, TimeSpan.FromMilliseconds(40), LibraryFile.MonMagicEx7, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                                StartTime = CEnvir.Now.AddMilliseconds(450),
                            });
                            break;
                    }
                    break;
                case MonsterImage.NewMob1:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(1500, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 20, 40, Color.Purple)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(200),
                                Blend = true,
                            });
                            break;
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(1500, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 20, 50, Globals.IceColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(200),
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.MonasteryMon4:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(2600, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx23, 20, 40, Color.GreenYellow)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(200),
                                Blend = true,
                            });
                            break;
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(2600, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx23, 20, 50, Color.GreenYellow)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(200),
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.NewMob3:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(2700, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 20, 50, Globals.IceColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(200),
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.NewMob10:
                    switch (action.Action)
                    {
                        case MirAction.Show:
                            Effects.Add(new MirEffect(3100, 18, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 20, 90, Color.Purple)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                                Skip = 0,
                            });
                            break;
                    }
                    break;
                case MonsterImage.NewMob6:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(2900, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.NewMob8:
                    switch (action.Action)
                    {
                        case MirAction.Show:
                            Effects.Add(new MirEffect(3220, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(3200, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.NewMob4:
                case MonsterImage.NewMob5:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(3200, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
            }
        }

        public override void PlayAttackSound()
        {
            DXSoundManager.Play(AttackSound);
        }
        public override void PlayStruckSound()
        {
            DXSoundManager.Play(StruckSound);

            DXSoundManager.Play(SoundIndex.GenericStruckMonster);
        }
        public override void PlayDieSound()
        {
            DXSoundManager.Play(DieSound);
        }

        public override void UpdateQuests()
        {
            if (GameScene.Game.HasQuest(MonsterInfo, GameScene.Game.MapControl.MapInfo)) //Todo Optimize by variable.

                Title = "(Quest)";
            else
                Title = string.Empty;
        }
    }
}

