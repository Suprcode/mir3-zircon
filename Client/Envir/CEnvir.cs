using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.Controls;
using Client.Models;
using Client.Scenes;
using Client.Scenes.Views;
using Client.UserModels;
using Library;
using Library.Network;
using Library.SystemModels;
using MirDB;
using SlimDX.Direct3D9;
using System.IO.IsolatedStorage;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Security.Cryptography.X509Certificates;
using Client.Envir.Translations;
using Sentry;
using System.Reflection;

namespace Client.Envir
{
    public static class CEnvir
    {
        public static TargetForm Target;
        public static Random Random = new Random();

        private static DateTime _FPSTime;
        private static int FPSCounter;
        private static int FPSCount;

        public static int DPSCounter;
        private static int DPSCount;

        public static bool Shift, Alt, Ctrl;
        public static DateTime Now;
        public static Point MouseLocation;

        public static CConnection Connection;
        public static bool WrongVersion;

        public static Dictionary<LibraryFile, MirLibrary> LibraryList = new Dictionary<LibraryFile, MirLibrary>();

        public static ClientUserItem[] Storage, MainStorage, PartsStorage, MainPartsStorage;

        public static List<ClientBlockInfo> BlockList = new List<ClientBlockInfo>();
        public static DBCollection<KeyBindInfo> KeyBinds;
        public static DBCollection<WindowSetting> WindowSettings;
        public static DBCollection<CastleInfo> CastleInfoList;
        public static MirDB.Session Session;

        public static ConcurrentQueue<string> ChatLog = new ConcurrentQueue<string>();

        public static bool Loaded;
        public static bool Loading { get; private set; }

        public static string BuyAddress;
        public static string C;

        public static bool TestServer;

        public static StringMessages Language { get; set; }


        static CEnvir()
        {
            LoadLanguage();

            Thread workThread = new Thread(SaveChatLoop) { IsBackground = true };
            workThread.Start();

            try
            { A(); }
            catch { }
        }

        public static void LoadLanguage()
        {
            switch (Config.Language.ToUpper())
            {
                case "ENGLISH":
                    Language = (StringMessages)ConfigReader.ConfigObjects[typeof(EnglishMessages)];
                    break;
                case "CHINESE":
                    Language = (StringMessages)ConfigReader.ConfigObjects[typeof(ChineseMessages)];
                    break;
            }

        }

        private static void A()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Zircon");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (File.Exists(path + "\\CheckSum.bin"))
            {
                using (BinaryReader E = new BinaryReader(File.OpenRead(path + "\\CheckSum.bin")))
                    C = E.ReadString();
            }
            else
            {
                using (BinaryWriter E = new BinaryWriter(File.Create(path + "\\CheckSum.bin")))
                    E.Write(C = Functions.RandomString(Random, 20));
            }

        }

        public static void SaveChatLoop()
        {
            List<string> lines = new List<string>();
            while (true)
            {
                while (ChatLog.IsEmpty)
                    Thread.Sleep(1000);

                while (!ChatLog.IsEmpty)
                {
                    string line;

                    if (!ChatLog.TryDequeue(out line)) continue;

                    lines.Add(line);
                }

                try
                {
                    File.AppendAllLines(@".\Chat Logs.txt", lines);
                    lines.Clear();
                }
                catch
                {
                }
            }
        }

        public static void GameLoop()
        {
            UpdateGame();
            RenderGame();

            if (Config.LimitFPS)
                Thread.Sleep(1);
        }
        private static void UpdateGame()
        {
            Now = Time.Now;
            DXControl.ActiveScene?.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, MouseLocation.X, MouseLocation.Y, 0));

            if (Time.Now >= _FPSTime)
            {
                _FPSTime = Time.Now.AddSeconds(1);
                FPSCount = FPSCounter;
                FPSCounter = 0;
                DPSCount = DPSCounter;
                DPSCounter = 0;
                DXManager.MemoryClear();
            }

            Connection?.Process();
            DXControl.ActiveScene?.Process();

            string debugText = $"FPS: {FPSCount}";

            if (DXControl.MouseControl != null)
                debugText += $", Mouse Control: {DXControl.MouseControl.GetType().Name}";

            if (DXControl.FocusControl != null)
                debugText += $", Focus Control: {DXControl.FocusControl.GetType().Name}";

            debugText += $", Mouse: {CEnvir.MouseLocation}";

            if (GameScene.Game != null)
            {
                if (DXControl.MouseControl is MapControl)
                    debugText += $", Co Ords: {GameScene.Game.MapControl.MapLocation}";

                debugText += $", Objects: {GameScene.Game.MapControl.Objects.Count}";

                if (MapObject.MouseObject != null)
                    debugText += $", Mouse Object: {MapObject.MouseObject.Name}";
            }
            debugText += $", DPS: {DPSCount}";

            string connectionText = string.Empty;

            if (Connection != null)
            {
                const decimal KB = 1024;
                const decimal MB = KB * 1024;

                string sent, received;

                if (Connection.TotalBytesSent > MB)
                    sent = $"{Connection.TotalBytesSent / MB:#,##0.0}MB";
                else if (Connection.TotalBytesSent > KB)
                    sent = $"{Connection.TotalBytesSent / KB:#,##0}KB";
                else
                    sent = $"{Connection.TotalBytesSent:#,##0}B";

                if (Connection.TotalBytesReceived > MB)
                    received = $"{Connection.TotalBytesReceived / MB:#,##0.0}MB";
                else if (Connection.TotalBytesReceived > KB)
                    received = $"{Connection.TotalBytesReceived / KB:#,##0}KB";
                else
                    received = $"{Connection.TotalBytesReceived:#,##0}B";

                connectionText = $"Ping: {Connection.Ping}, Sent: {sent}, Received: {received}";
            }

            if (Config.FullScreen)
            {
                DXControl.DebugLabel.Text = debugText;

                DXControl.DebugLabel.IsVisible = Config.DebugLabel;
                DXControl.PingLabel.IsVisible = Config.DebugLabel;

                if (Connection != null)
                {
                    DXControl.PingLabel.Text = connectionText;
                    DXControl.PingLabel.Location = new Point(DXControl.DebugLabel.DisplayArea.Right + 5, DXControl.DebugLabel.DisplayArea.Y);
                }
            }
            else
            {
                CEnvir.Target.Text = $"{Globals.ClientName} - {debugText} - {connectionText}";

                DXControl.PingLabel.Text = string.Empty;
                DXControl.DebugLabel.Text = string.Empty;

                DXControl.DebugLabel.Visible = false;
                DXControl.PingLabel.Visible = false;
            }

            if (DXControl.MouseControl != null && DXControl.ActiveScene != null)
            {
                DXControl.HintLabel.Text = DXControl.MouseControl.Hint;

                Point location = new Point(MouseLocation.X, MouseLocation.Y + 17);

                if (location.X + DXControl.HintLabel.Size.Width > DXControl.ActiveScene.Size.Width)
                    location.X = DXControl.ActiveScene.Size.Width - DXControl.HintLabel.Size.Width - 1;

                if (location.Y + DXControl.HintLabel.Size.Height > DXControl.ActiveScene.Size.Height)
                    location.Y = DXControl.ActiveScene.Size.Height - DXControl.HintLabel.Size.Height - 1;

                if (location.X < 0) location.X = 0;
                if (location.Y < 0) location.Y = 0;

                DXControl.HintLabel.Location = location;
            }
            else
            {
                DXControl.HintLabel.Text = null;
            }
        }
        private static void RenderGame()
        {
            try
            {
                if (Target.ClientSize.Width == 0 || Target.ClientSize.Height == 0)
                {
                    Thread.Sleep(1);
                    return;
                }

                if (DXManager.DeviceLost)
                {
                    DXManager.AttemptReset();
                    Thread.Sleep(1);
                    return;
                }

                DXManager.Device.Clear(ClearFlags.Target, Color.Black, 1, 0);
                DXManager.Device.BeginScene();
                DXManager.Sprite.Begin(SpriteFlags.AlphaBlend);

                DXControl.ActiveScene?.Draw();

                DXManager.Sprite.End();
                DXManager.Device.EndScene();

                DXManager.Device.Present();
                FPSCounter++;
            }
            catch (Direct3D9Exception)
            {
                DXManager.DeviceLost = true;
            }
            catch (Exception ex)
            {
                SaveException(ex);

                DXManager.AttemptRecovery();
            }
        }

        public static void ReturnToLogin()
        {
            if (DXControl.ActiveScene is LoginScene) return; // handle ?

            DXControl.ActiveScene.Dispose();
            DXSoundManager.StopAllSounds();
            DXControl.ActiveScene = new LoginScene(Config.IntroSceneSize);

            BlockList = new List<ClientBlockInfo>();
        }

        public static void LoadDatabase()
        {
            Loading = true;
            Task.Run(() =>
            {
                try
                {
                    Session = new MirDB.Session(SessionMode.Users, @".\Data\") { BackUp = false };

                    Session.Initialize(
                        Assembly.GetAssembly(typeof(ItemInfo)),
                        Assembly.GetAssembly(typeof(WindowSetting))
                    );

                    Globals.ItemInfoList = Session.GetCollection<ItemInfo>();
                    Globals.MagicInfoList = Session.GetCollection<MagicInfo>();
                    Globals.MapInfoList = Session.GetCollection<MapInfo>();
                    Globals.CurrencyInfoList = Session.GetCollection<CurrencyInfo>();
                    Globals.InstanceInfoList = Session.GetCollection<InstanceInfo>();
                    Globals.NPCPageList = Session.GetCollection<NPCPage>();
                    Globals.MonsterInfoList = Session.GetCollection<MonsterInfo>();
                    Globals.FishingInfoList = Session.GetCollection<FishingInfo>();
                    Globals.StoreInfoList = Session.GetCollection<StoreInfo>();
                    Globals.NPCInfoList = Session.GetCollection<NPCInfo>();
                    Globals.MovementInfoList = Session.GetCollection<MovementInfo>();
                    Globals.QuestInfoList = Session.GetCollection<QuestInfo>();
                    Globals.QuestTaskList = Session.GetCollection<QuestTask>();
                    Globals.CompanionInfoList = Session.GetCollection<CompanionInfo>();
                    Globals.CompanionLevelInfoList = Session.GetCollection<CompanionLevelInfo>();
                    Globals.DisciplineInfoList = Session.GetCollection<DisciplineInfo>();

                    KeyBinds = Session.GetCollection<KeyBindInfo>();
                    WindowSettings = Session.GetCollection<WindowSetting>();
                    CastleInfoList = Session.GetCollection<CastleInfo>();

                    Globals.GoldInfo = Globals.CurrencyInfoList.Binding.Count > 0 ? Globals.CurrencyInfoList.Binding.First(x => x.Type == CurrencyType.Gold).DropItem : null;

                    CheckKeyBinds();

                    if (!DXManager.ValidResolutions.Contains(Config.GameSize))
                        Config.GameSize = DXManager.ValidResolutions[0];

                    Loaded = true;
                }
                finally
                {
                    Loading = false;
                }
            });
        }

        public static IEnumerable<KeyBindAction> GetKeyAction(Keys key)
        {
            if (!Loaded) yield break;

            switch (key)
            {
                case Keys.NumPad0:
                    key = Keys.D0;
                    break;
                case Keys.NumPad1:
                    key = Keys.D1;
                    break;
                case Keys.NumPad2:
                    key = Keys.D2;
                    break;
                case Keys.NumPad3:
                    key = Keys.D3;
                    break;
                case Keys.NumPad4:
                    key = Keys.D4;
                    break;
                case Keys.NumPad5:
                    key = Keys.D5;
                    break;
                case Keys.NumPad6:
                    key = Keys.D6;
                    break;
                case Keys.NumPad7:
                    key = Keys.D7;
                    break;
                case Keys.NumPad8:
                    key = Keys.D8;
                    break;
                case Keys.NumPad9:
                    key = Keys.D9;
                    break;
            }

            foreach (KeyBindInfo bind in KeyBinds.Binding)
            {
                if ((bind.Control1 == Ctrl && bind.Alt1 == Alt && bind.Shift1 == Shift && bind.Key1 == key) ||
                    (bind.Control2 == Ctrl && bind.Alt2 == Alt && bind.Shift2 == Shift && bind.Key2 == key))
                    yield return bind.Action;
            }
        }

        public static void FillStorage(List<ClientUserItem> items, bool observer)
        {
            Storage = new ClientUserItem[1000];
            PartsStorage = new ClientUserItem[1000];

            if (!observer)
            {
                MainStorage = Storage;
                MainPartsStorage = PartsStorage;
            }

            foreach (ClientUserItem item in items)
                if (item.Slot >= 2000)
                    PartsStorage[item.Slot - Globals.PartsStorageOffset] = item;
                else
                    Storage[item.Slot] = item;
        }
        public static void Enqueue(Packet packet)
        {
            Connection?.Enqueue(packet);
        }

        public static void ResetKeyBinds()
        {
            for (int i = KeyBinds.Count - 1; i >= 0; i--)
                KeyBinds[i].Delete();

            CheckKeyBinds();
        }
        public static void CheckKeyBinds()
        {
            foreach (KeyBindAction action in Enum.GetValues(typeof(KeyBindAction)).Cast<KeyBindAction>())
            {
                switch (action)
                {
                    case KeyBindAction.None:
                        break;
                    default:
                        if (KeyBinds.Binding.Any(x => x.Action == action)) continue;

                        ResetKeyBind(action);
                        break;
                }
            }
        }

        public static void ResetKeyBind(KeyBindAction action)
        {
            KeyBindInfo bind = KeyBinds.CreateNewObject();
            bind.Action = action;

            switch (action)
            {
                case KeyBindAction.ConfigWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.O;
                    break;
                case KeyBindAction.CharacterWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.Q;
                    break;
                case KeyBindAction.InventoryWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.W;
                    break;
                case KeyBindAction.MagicWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.E;
                    break;
                case KeyBindAction.MagicBarWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.E;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.RankingWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.R;
                    break;
                case KeyBindAction.GameStoreWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.Y;
                    break;
                case KeyBindAction.DungeonFinderWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.D;
                    bind.Shift1 = true;
                    break;
                case KeyBindAction.CompanionWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.U;
                    break;
                case KeyBindAction.GroupWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.P;
                    break;
                case KeyBindAction.AutoPotionWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.A;
                    break;
                case KeyBindAction.StorageWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.S;
                    break;
                case KeyBindAction.BlockListWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.F;
                    break;
                case KeyBindAction.GuildWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.G;
                    break;
                case KeyBindAction.QuestLogWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.J;
                    break;
                case KeyBindAction.QuestTrackerWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.L;
                    break;
                case KeyBindAction.BeltWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.Z;
                    break;
                case KeyBindAction.MarketPlaceWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.C;
                    break;
                case KeyBindAction.MapMiniWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.V;
                    break;
                case KeyBindAction.MapBigWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.B;
                    break;
                case KeyBindAction.MailBoxWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.Oemcomma;
                    break;
                case KeyBindAction.MailSendWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.OemPeriod;
                    break;
                case KeyBindAction.ChatOptionsWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.O;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.ExitGameWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.Q;
                    bind.Alt1 = true;
                    bind.Key2 = Keys.X;
                    bind.Alt2 = true;
                    break;
                case KeyBindAction.ChangeAttackMode:
                    bind.Category = "Functions";
                    bind.Key1 = Keys.H;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.ChangePetMode:
                    bind.Category = "Functions";
                    bind.Key1 = Keys.A;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.GroupAllowSwitch:
                    bind.Category = "Functions";
                    bind.Key1 = Keys.P;
                    bind.Alt1 = true;
                    break;
                case KeyBindAction.GroupTarget:
                    bind.Category = "Functions";
                    bind.Key1 = Keys.G;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.TradeRequest:
                    bind.Category = "Functions";
                    bind.Key1 = Keys.T;
                    break;
                case KeyBindAction.TradeAllowSwitch:
                    bind.Category = "Functions";
                    bind.Key1 = Keys.T;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.MountToggle:
                    bind.Category = "Functions";
                    bind.Key1 = Keys.M;
                    break;
                case KeyBindAction.AutoRunToggle:
                    bind.Category = "Functions";
                    bind.Key1 = Keys.D;
                    break;
                case KeyBindAction.ChangeChatMode:
                    bind.Category = "Functions";
                    bind.Key1 = Keys.K;
                    break;
                case KeyBindAction.ItemPickUp:
                    bind.Category = "Items";
                    bind.Key1 = Keys.Tab;
                    break;
                case KeyBindAction.PartnerTeleport:
                    bind.Category = "Items";
                    bind.Key1 = Keys.Z;
                    bind.Shift1 = true;
                    break;
                case KeyBindAction.ToggleItemLock:
                    bind.Category = "Items";
                    bind.Key1 = Keys.Scroll;
                    break;
                case KeyBindAction.UseBelt01:
                    bind.Category = "Items";
                    bind.Key1 = Keys.D1;
                    bind.Key2 = Keys.D1;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt02:
                    bind.Category = "Items";
                    bind.Key1 = Keys.D2;
                    bind.Key2 = Keys.D2;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt03:
                    bind.Category = "Items";
                    bind.Key1 = Keys.D3;
                    bind.Key2 = Keys.D3;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt04:
                    bind.Category = "Items";
                    bind.Key1 = Keys.D4;
                    bind.Key2 = Keys.D4;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt05:
                    bind.Category = "Items";
                    bind.Key1 = Keys.D5;
                    bind.Key2 = Keys.D5;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt06:
                    bind.Category = "Items";
                    bind.Key1 = Keys.D6;
                    bind.Key2 = Keys.D6;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt07:
                    bind.Category = "Items";
                    bind.Key1 = Keys.D7;
                    bind.Key2 = Keys.D7;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt08:
                    bind.Category = "Items";
                    bind.Key1 = Keys.D8;
                    bind.Key2 = Keys.D8;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt09:
                    bind.Category = "Items";
                    bind.Key1 = Keys.D9;
                    bind.Key2 = Keys.D9;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.UseBelt10:
                    bind.Category = "Items";
                    bind.Key1 = Keys.D0;
                    bind.Key2 = Keys.D0;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellSet01:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F1;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.SpellSet02:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F2;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.SpellSet03:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F3;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.SpellSet04:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F4;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.SpellUse01:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F1;
                    bind.Key2 = Keys.F1;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse02:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F2;
                    bind.Key2 = Keys.F2;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse03:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F3;
                    bind.Key2 = Keys.F3;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse04:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F4;
                    bind.Key2 = Keys.F4;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse05:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F5;
                    bind.Key2 = Keys.F5;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse06:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F6;
                    bind.Key2 = Keys.F6;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse07:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F7;
                    bind.Key2 = Keys.F7;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse08:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F8;
                    bind.Key2 = Keys.F8;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse09:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F9;
                    bind.Key2 = Keys.F9;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse10:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F10;
                    bind.Key2 = Keys.F10;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse11:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F11;
                    bind.Key2 = Keys.F11;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse12:
                    bind.Category = "Magic";
                    bind.Key1 = Keys.F12;
                    bind.Key2 = Keys.F12;
                    bind.Shift2 = true;
                    break;
                case KeyBindAction.SpellUse13:
                    bind.Category = "Magic";
                    break;
                case KeyBindAction.SpellUse14:
                    bind.Category = "Magic";
                    break;
                case KeyBindAction.SpellUse15:
                    bind.Category = "Magic";
                    break;
                case KeyBindAction.SpellUse16:
                    bind.Category = "Magic";
                    break;
                case KeyBindAction.SpellUse17:
                    bind.Category = "Magic";
                    break;
                case KeyBindAction.SpellUse18:
                    bind.Category = "Magic";
                    break;
                case KeyBindAction.SpellUse19:
                    bind.Category = "Magic";
                    break;
                case KeyBindAction.SpellUse20:
                    bind.Category = "Magic";
                    break;
                case KeyBindAction.SpellUse21:
                    bind.Category = "Magic";
                    break;
                case KeyBindAction.SpellUse22:
                    bind.Category = "Magic";
                    break;
                case KeyBindAction.SpellUse23:
                    bind.Category = "Magic";
                    break;
                case KeyBindAction.SpellUse24:
                    bind.Category = "Magic";
                    break;
                case KeyBindAction.FortuneWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.W;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.CurrencyWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.C;
                    bind.Control1 = true;
                    break;
                case KeyBindAction.FilterDropWindow:
                    bind.Category = "Windows";
                    bind.Key1 = Keys.F;
                    bind.Control1 = true;
                    break;
            }
        }

        public static float FontSize(float size)
        {
            return (size - Config.FontSizeMod) * (96F / DXManager.Graphics.DpiX);
        }

        public static int ErrorCount;
        private static string LastError;
        public static void SaveError(string ex)
        {
            try
            {
                if (++ErrorCount > 20 || String.Compare(ex, LastError, StringComparison.OrdinalIgnoreCase) == 0) return;

                const string LogPath = @".\Errors\";

                LastError = ex;
                string state = $"State = {Target?.DisplayRectangle}";

                if (!Directory.Exists(LogPath))
                    Directory.CreateDirectory(LogPath);

                File.AppendAllText($"{LogPath}{Now.Year}-{Now.Month}-{Now.Day}.txt", LastError + Environment.NewLine + state + Environment.NewLine);
            }
            catch
            { }
        }

        public static void SaveException(Exception ex)
        {
            SaveError(ex.ToString());
            if (Config.SentryEnabled) SentrySdk.CaptureException(ex);
        }

        public static void Unload()
        {
            CConnection con = Connection;

            Connection = null;

            con?.Disconnect();
        }
        public static KeyBindInfo GetKeyBind(KeyBindAction action)
        {
            return KeyBinds.Binding.FirstOrDefault(x => x.Action == action);
        }
        public static string GetText(Keys key)
        {
            switch (key)
            {
                case Keys.None:
                    return string.Empty;
                case Keys.Back:
                    return "Backspace";
                case Keys.Capital:
                    return "Cap Lock";
                case Keys.Scroll:
                    return "Scroll Lock";
                case Keys.NumLock:
                    return "Num Lock";
                case Keys.Prior:
                    return "Page Up";
                case Keys.Next:
                    return "Page Down";
                case Keys.Multiply:
                    return "Num Pad *";
                case Keys.Add:
                    return "Num Pad +";
                case Keys.Subtract:
                    return "Num Pad -";
                case Keys.Decimal:
                    return "Num Pad .";
                case Keys.Divide:
                    return "Num Pad /";
                case Keys.OemSemicolon:
                    return ";";
                case Keys.Oemplus:
                    return "=";
                case Keys.Oemcomma:
                    return ",";
                case Keys.OemMinus:
                    return "-";
                case Keys.OemPeriod:
                    return ".";
                case Keys.OemQuestion:
                    return "/";
                case Keys.Oemtilde:
                    return "'";
                case Keys.OemOpenBrackets:
                    return "[";
                case Keys.OemCloseBrackets:
                    return "]";
                case Keys.OemQuotes:
                    return "#";
                case Keys.Oem8:
                    return "`";
                case Keys.OemBackslash:
                    return "\\";
                case Keys.D1:
                    return "1";
                case Keys.D2:
                    return "2";
                case Keys.D3:
                    return "3";
                case Keys.D4:
                    return "4";
                case Keys.D5:
                    return "5";
                case Keys.D6:
                    return "6";
                case Keys.D7:
                    return "7";
                case Keys.D8:
                    return "8";
                case Keys.D9:
                    return "9";
                case Keys.D0:
                    return "0";
                default:
                    return key.ToString();
            }
        }

        public static bool IsCurrencyItem(ItemInfo info)
        {
            return Globals.CurrencyInfoList.Binding.FirstOrDefault(x => x.DropItem == info) != null;
        }

        public static int CurrencyImage(ItemInfo info, long count)
        {
            var currency = Globals.CurrencyInfoList.Binding.FirstOrDefault(x => x.DropItem == info);

            if (currency == null)
                return 0;

            var image = currency.Images.OrderByDescending(x => x.Amount).FirstOrDefault(x => x.Amount <= count);

            if (image != null)
                return image.Image;
            else
                return currency.DropItem.Image;
        }
    }
}
