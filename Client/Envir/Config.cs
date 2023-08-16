using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library;

namespace Client.Envir
{
    [ConfigPath(@".\Zircon.ini")]
    public static class Config
    {
        public static readonly Size IntroSceneSize = new Size(1024, 768);

        public const string DefaultIPAddress = "127.0.0.1";
        public const int DefaultPort = 7000;

        [ConfigSection("Network")]
        public static bool UseNetworkConfig { get; set; } = false;
        public static string IPAddress { get; set; } = DefaultIPAddress;
        public static int Port { get; set; } = DefaultPort;
        public static TimeSpan TimeOutDuration { get; set; } = TimeSpan.FromSeconds(15);

        [ConfigSection("Audit")]
        public static bool SentryEnabled { get; set; } = false;
        public static string SentryDSN { get; set; } = "";


        [ConfigSection("Graphics")]
        public static bool FullScreen { get; set; } = false;
        public static bool VSync { get; set; } = true;
        public static bool LimitFPS { get; set; }
        public static Size GameSize { get; set; } = IntroSceneSize;
        public static TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(30);
        public static string FontName { get; set; } = "MS Sans Serif";
        public static string MapPath { get; set; } = @".\Map\";
        public static bool ClipMouse { get; set; } = false;
        public static bool DebugLabel { get; set; } = false;
        public static float FontSizeMod { get; set; } = 0.0F;
        public static string Language { get; set; } = "English";
        public static bool Borderless { get; set; } = false;


        [ConfigSection("Sound")]
        public static bool SoundInBackground { get; set; } = true;
        public static int SoundOverLap { get; set; } = 5;
        public static int SystemVolume { get; set; } = 25;
        public static int MusicVolume { get; set; } = 25;
        public static int PlayerVolume { get; set; } = 25;
        public static int MonsterVolume { get; set; } = 25;
        public static int MagicVolume { get; set; } = 25;

        [ConfigSection("Login")]
        public static bool RememberDetails { get; set; } = false;
        public static string RememberedEMail { get; set; } = string.Empty;
        public static string RememberedPassword { get; set; } = string.Empty;

        [ConfigSection("Game")]
        public static bool DrawEffects { get; set; } = true;
        public static bool DrawParticles { get; set; } = false;
        public static bool DrawWeather { get; set; } = true;
        public static bool ShowItemNames { get; set; } = true;
        public static bool ShowMonsterNames { get; set; } = true;
        public static bool ShowPlayerNames { get; set; } = true;
        public static bool ShowUserHealth { get; set; } = true;
        public static bool ShowMonsterHealth { get; set; } = true;
        public static bool ShowDamageNumbers { get; set; } = true;
        public static bool EscapeCloseAll { get; set; } = false;
        public static bool ShiftOpenChat { get; set; } = true;
        public static bool SpecialRepair { get; set; } = true;
        public static bool RightClickDeTarget { get; set; } = true;

        public static bool MonsterBoxExpanded { get; set; } = true;
        public static bool MonsterBoxVisible { get; set; } = true;
        public static bool QuestTrackerVisible { get; set; } = true;

        public static bool LogChat { get; set; } = true;

        public static int RankingClass { get; set; } = (int)RequiredClass.All;
        public static bool RankingOnline { get; set; } = true;
        public static string HighlightedItems { get; set; } = string.Empty;

        [ConfigSection("Colours")]
        public static Color LocalTextColour { get; set; } = Color.White;
        public static Color GMWhisperInTextColour { get; set; } = Color.Red;
        public static Color WhisperInTextColour { get; set; } = Color.Cyan;
        public static Color WhisperOutTextColour { get; set; } = Color.Aquamarine;
        public static Color GroupTextColour { get; set; } = Color.Plum;
        public static Color GuildTextColour { get; set; } = Color.LightPink;
        public static Color ShoutTextColour { get; set; } = Color.Yellow;
        public static Color GlobalTextColour { get; set; } = Color.Lime;
        public static Color ObserverTextColour { get; set; } = Color.Silver;
        public static Color HintTextColour { get; set; } = Color.AntiqueWhite;
        public static Color SystemTextColour { get; set; } = Color.Red;
        public static Color GainsTextColour { get; set; } = Color.GreenYellow;
        public static Color AnnouncementTextColour { get; set; } = Color.DarkBlue;
    }
}
