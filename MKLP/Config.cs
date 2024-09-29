using Newtonsoft.Json;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace MKLP
{
    public class Config
    {
        public CONFIG_HELPTEXT Guide;
        public CONFIG_MAIN Main;
        public CONFIG_PROGRESSION Progression;
        public CONFIG_PERMISSIONS Permissions;
        public CONFIG_DISCORD Discord;
        public CONFIG_BOSSES BossManager;
        public CONFIG_DATABASE DataBase;

        static string path = Path.Combine(TShock.SavePath, "MKLP.json");

        public static Config Read()
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(Default(), Formatting.Indented));
                return Default();
            }


            var args = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));

            if (args == null) return Default();

            args.Guide = new(); //help text should not be modified!

            if (args.Main == null) args.Main = new();
            args.Main.FixNull();

            if (args.Progression == null) args.Progression = new();
            args.Progression.FixNull();

            if (args.Discord == null) args.Discord = new();
            args.Discord.FixNull();

            if (args.Permissions == null) args.Permissions = new();
            args.Permissions.FixNull();

            if (args.BossManager == null) args.BossManager = new();
            args.BossManager.FixNull();

            if (args.DataBase == null) args.DataBase = new();
            args.DataBase.FixNull();

            File.WriteAllText(path, JsonConvert.SerializeObject(args, Formatting.Indented));
            return args;
        }

        /// <summary>
        /// changes config file
        /// </summary>
        /// <param name="config"></param>
        public void Changeall(Config config)
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(Default(), Formatting.Indented));
            }
            else
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
            }
        }

        private static Config Default()
        {
            return new Config()
            {
                Guide = new(),
                Main = new(),
                Progression = new(),
                Permissions = new(),
                Discord = new(),
                BossManager = new(),
                DataBase = new(),
            };
        }

    }


    #region [ Config Objects ]
    
    public class CONFIG_HELPTEXT
    {
        public string HelpText_StaffChat0a = "■▶▶▶▶ StaffChat ◀◀◀◀■";
        public string HelpText_StaffChat1a = "▮▬▬▬ MessageRecieved_Discord ▬▬▮";
        public string HelpText_StaffChat1b = "- %discordname% : Discord username";
        public string HelpText_StaffChat1c = "- %discordingame% : Shows Ingame name of that discord user if they already linked their account";
        public string HelpText_StaffChat1d = "- %discordoringame% : Shows discord username or if they linked their account it will show their ingame name";
        public string HelpText_StaffChat1e = "- %discordacclinkedicon% : Icon shows up when that discorduser already linked their account";
        public string HelpText_StaffChat2a = "▮▬▬▬▬ MessageSend_Discord ▬▬▬▬▬▮";
        public string HelpText_StaffChat2b = "- %ingamename% : Ingame player name";
        public string HelpText_StaffChat2c = "- %ingameaccountname% : Ingame Account name";
        public string HelpText_StaffChat2d = "- %ingameaccountnameifname% : Ingame account name shows up when accountname and playername did not match";
        public string HelpText_StaffChat2e = "- %ingamelinkedusername% : Shows discord username if that player linked their account";
        public string HelpText_StaffChat2f = "- %discordacclinkedicon% : Icon shows up when that player already linked their account";
        public string HelpText_StaffChat3a = "▮▬▬▬ MessageRecieved_InGame ▬▬▬▮";
        public string HelpText_StaffChat3b = "- %ingamename% : Ingame player name";
        public string HelpText_StaffChat3c = "- %ingameaccountname% : Ingame Account name";
        public string HelpText_StaffChat3d = "- %ingameaccountnameifname% : Ingame account name shows up when accountname and playername did not match";
        public string HelpText_StaffChat3e = "- %ingamelinkedusername% : Shows discord username if that player linked their account";
        public string HelpText_StaffChat3f = "- %ingamelinkedicon% : Icon shows up when that player already linked their account";
        public string HelpText_StaffChat4a = "▮▬▬▬ MessageRecieved_InGame ▬▬▬▮";
        public string HelpText_StaffChat4b = "- %groupname% : player's group name";
        public string HelpText_StaffChat4c = "- %groupprefix% : player's group prefix";
        public string HelpText_StaffChat4d = "- %groupsuffix% : player's group suffix";
        public string HelpText_StaffChat4e = "- %tempgroupname% : player's temp group name";
        public string HelpText_StaffChat4f = "- %tempgroupprefix% : player's temp group prefix";
        public string HelpText_StaffChat4g = "- %tempgroupsuffix% : player's temp group suffix";
        public string HelpText_StaffChat5a = "▮▬▬▬▬▬▬▬▬▬▬▬▬▬ Any ▬▬▬▬▬▬▬▬▬▬▬▬▮";
        public string HelpText_StaffChat5b = "- %message% : Message of player/discorduser";
        public string HelpText_Restart1 = "■▶▶▶▶ Reminder ◀◀◀◀■";
        public string HelpText_Restart2 = "- modifying database/command require's server restart";
    }

    public struct CONFIG_COLOR_RBG
    {
        public float R;
        public float G;
        public float B;
        public CONFIG_COLOR_RBG(float r = 255, float g = 255, float b = 255)
        {
            R = r;
            G = g;
            B = b;
        }
    }
    public class CONFIG_MAIN
    {
        public byte? Minimum_CharacterName = 1;
        public byte? Maximum_CharacterName = 255;
        public string[] IllegalNames = { "ServerConsole", "Server" };
        public bool? Allow_PlayerName_Symbols = true;
        public bool? Allow_PlayerName_InappropriateWords = false;

        public string StaffChat_Message_discordacclinkedicon = "[i:3124]";
        public string StaffChat_Message_ingamelinkedicon = "💳";

        public string StaffChat_MessageRecieved_Discord = "[c/0aa3ef:[StaffChat][c/0aa3ef:]] %discordname% : %message%";
        public string StaffChat_HexColor_Discord_Mention_User = "f28f0c";
        public string StaffChat_HexColor_Discord_Mention_Channel = "00FFFF";
        public string StaffChat_Message_Discord_HasAttachment = "[c/34e718:[Attachment][c/34e718:]]";

        public string StaffChat_MessageSend_Discord = "[c/0aa3ef:[StaffChat][c/0aa3ef:]] %ingamename% : %message%";
        public string StaffChat_MessageRecieved_InGame = "⚒️ **%ingamename%** : %message%";
        public CONFIG_COLOR_RBG? StaffChat_MessageRecieved_InGame_RBG = new();

        public bool? Replace_Ban_TShockCommand = false;
        public bool? Replace_Mute_TShockCommand = false;

        public bool? Using_AntiGrief_Surface_Break = false;
        public bool? Using_AntiGrief_Surface_Explosive = false;
        public bool? Using_AntiGrief_Surface_Place = false;
        public bool? Using_AntiGrief_Surface_PlaceLiquid = false;
        public string Message_AntiGrief_Surface_Break = "You Cannot Break Blocks on Surface!";
        public string Message_AntiGrief_Surface_Explosive = "You Cannot Use Explosives on Surface!";
        public string Message_AntiGrief_Surface_Place = "You Cannot Place Blocks on Surface!";
        public string Message_AntiGrief_Surface_PlaceLiquid = "You Cannot use Liquids on Surface!";

        public bool? RecievedWarning_SuspiciousDupe = true;
        public bool? Prevent_IllegalWire_Progression = false;

        public bool? Prevent_Place_BastStatueNearDoor = true;

        public bool? Using_Main_Code1 = false;
        public bool? Using_Main_Code2 = false;

        public bool? Using_Survival_Code1 = false;
        public bool? Using_Survival_Code2 = false;
        public bool? Using_Survival_Code3 = false;
        public bool? Using_Survival_Code4 = false;

        public bool? Using_Default_Code1 = false;
        public int? default_code1_maxdefault = 140;
        public int? default_code1_maxboost = 180;
        public int? default_code1_maxbomb = 250;
        public int? default_code1_maxdynamite = 450;

        public bool? Using_Default_Code2 = false;
        public int? default_code2_maxdefault = 20;
        public int? default_code2_maxboost = 40;
        public int? default_code2_maxbomb = 160;

        public bool? Using_Default_Code3 = false;
        public int? default_code3_maxdefault = 20;
        public int? default_code3_maxboost = 40;

        public bool? Using_Default_Code4 = false;
        public int? default_code4_maxdefault = 20;
        public int? default_code4_maxboost = 40;
        public int? default_code4_maxbomb = 160;

        public bool? Using_Default_Code5 = false;
        public int? default_code5_maxdefault = 60;
        public int? default_code5_maxHM = 90;

        public bool? Using_Default_Code6 = false;
        public int? default_code6_maxdefault = 30;
        public int? default_code6_maxPlant = 50;
        public int? default_code6_addmax_spectrehood = 15;

        public int? Ignore_Value_ClearLag = 12000;
        public bool? Save_Inventory_Log = false;

        public CONFIG_MAIN() { }

        public void FixNull()
        {
            CONFIG_MAIN getdefault = new();

            if (Minimum_CharacterName == null) Minimum_CharacterName = getdefault.Minimum_CharacterName;
            if (Maximum_CharacterName == null) Maximum_CharacterName = getdefault.Maximum_CharacterName;
            if (IllegalNames == null) IllegalNames = getdefault.IllegalNames;
            if (Allow_PlayerName_Symbols == null) Allow_PlayerName_Symbols = getdefault.Allow_PlayerName_Symbols;
            if (Allow_PlayerName_InappropriateWords == null) Allow_PlayerName_InappropriateWords = getdefault.Allow_PlayerName_InappropriateWords;

            if (StaffChat_Message_discordacclinkedicon == null) StaffChat_Message_discordacclinkedicon = getdefault.StaffChat_Message_discordacclinkedicon;
            if (StaffChat_Message_ingamelinkedicon == null) StaffChat_Message_ingamelinkedicon = getdefault.StaffChat_Message_ingamelinkedicon;

            if (StaffChat_MessageRecieved_Discord == null) StaffChat_MessageRecieved_Discord = getdefault.StaffChat_MessageRecieved_Discord;
            if (StaffChat_HexColor_Discord_Mention_User == null) StaffChat_HexColor_Discord_Mention_User = getdefault.StaffChat_HexColor_Discord_Mention_User;
            if (StaffChat_HexColor_Discord_Mention_Channel == null) StaffChat_HexColor_Discord_Mention_Channel = getdefault.StaffChat_HexColor_Discord_Mention_Channel;
            if (StaffChat_Message_Discord_HasAttachment == null) StaffChat_Message_Discord_HasAttachment = getdefault.StaffChat_Message_Discord_HasAttachment;
            
            if (StaffChat_MessageSend_Discord == null) StaffChat_MessageSend_Discord = getdefault.StaffChat_MessageSend_Discord;
            if (StaffChat_MessageRecieved_InGame == null) StaffChat_MessageRecieved_InGame = getdefault.StaffChat_MessageRecieved_InGame;
            if (StaffChat_MessageRecieved_InGame_RBG == null) StaffChat_MessageRecieved_InGame_RBG = getdefault.StaffChat_MessageRecieved_InGame_RBG;
            CONFIG_COLOR_RBG? is0 = new(0.0f, 0.0f, 0.0f);
            if (StaffChat_MessageRecieved_InGame_RBG.Equals(is0)) StaffChat_MessageRecieved_InGame_RBG = new(255f, 255f, 255f);

            if (Replace_Ban_TShockCommand == null) Replace_Ban_TShockCommand = getdefault.Replace_Ban_TShockCommand;
            if (Replace_Mute_TShockCommand == null) Replace_Mute_TShockCommand = getdefault.Replace_Mute_TShockCommand;

            if (Using_AntiGrief_Surface_Break == null) Using_AntiGrief_Surface_Break = getdefault.Using_AntiGrief_Surface_Break;
            if (Using_AntiGrief_Surface_Explosive == null) Using_AntiGrief_Surface_Explosive = getdefault.Using_AntiGrief_Surface_Explosive;
            if (Using_AntiGrief_Surface_Place == null) Using_AntiGrief_Surface_Place = getdefault.Using_AntiGrief_Surface_Place;
            if (Using_AntiGrief_Surface_PlaceLiquid == null) Using_AntiGrief_Surface_PlaceLiquid = getdefault.Using_AntiGrief_Surface_PlaceLiquid;
            if (Message_AntiGrief_Surface_Break == null) Message_AntiGrief_Surface_Break = getdefault.Message_AntiGrief_Surface_Break;
            if (Message_AntiGrief_Surface_Explosive == null) Message_AntiGrief_Surface_Explosive = getdefault.Message_AntiGrief_Surface_Explosive;
            if (Message_AntiGrief_Surface_Place == null) Message_AntiGrief_Surface_Place = getdefault.Message_AntiGrief_Surface_Place;
            if (Message_AntiGrief_Surface_PlaceLiquid == null) Message_AntiGrief_Surface_PlaceLiquid = getdefault.Message_AntiGrief_Surface_PlaceLiquid;

            if (RecievedWarning_SuspiciousDupe == null) RecievedWarning_SuspiciousDupe = getdefault.RecievedWarning_SuspiciousDupe;
            if (Prevent_IllegalWire_Progression == null) Prevent_IllegalWire_Progression = getdefault.Prevent_IllegalWire_Progression;

            if (Prevent_Place_BastStatueNearDoor == null) Prevent_Place_BastStatueNearDoor = getdefault.Prevent_Place_BastStatueNearDoor;

            if (Using_Main_Code1 == null) Using_Main_Code1 = getdefault.Using_Main_Code1;
            if (Using_Main_Code2 == null) Using_Main_Code2 = getdefault.Using_Main_Code2;

            if (Using_Survival_Code1 == null) Using_Survival_Code1 = getdefault.Using_Survival_Code1;
            if (Using_Survival_Code2 == null) Using_Survival_Code2 = getdefault.Using_Survival_Code2;
            if (Using_Survival_Code3 == null) Using_Survival_Code3 = getdefault.Using_Survival_Code3;
            if (Using_Survival_Code4 == null) Using_Survival_Code4 = getdefault.Using_Survival_Code4;

            if (Using_Default_Code1 == null) Using_Default_Code1 = getdefault.Using_Default_Code1;
            if (default_code1_maxdefault == null) default_code1_maxdefault = getdefault.default_code1_maxdefault;
            if (default_code1_maxboost == null) default_code1_maxboost = getdefault.default_code1_maxboost;
            if (default_code1_maxbomb == null) default_code1_maxbomb = getdefault.default_code1_maxbomb;
            if (default_code1_maxdynamite == null) default_code1_maxdynamite = getdefault.default_code1_maxdynamite;

            if (Using_Default_Code2 == null) Using_Default_Code2 = getdefault.Using_Default_Code1;
            if (default_code2_maxdefault == null) default_code2_maxdefault = getdefault.default_code2_maxdefault;
            if (default_code2_maxboost == null) default_code2_maxboost = getdefault.default_code2_maxboost;
            if (default_code2_maxbomb == null) default_code2_maxbomb = getdefault.default_code2_maxbomb;

            if (Using_Default_Code3 == null) Using_Default_Code3 = getdefault.Using_Default_Code1;
            if (default_code3_maxdefault == null) default_code3_maxdefault = getdefault.default_code3_maxdefault;
            if (default_code3_maxboost == null) default_code3_maxboost = getdefault.default_code3_maxboost;

            if (Using_Default_Code4 == null) Using_Default_Code4 = getdefault.Using_Default_Code1;
            if (default_code4_maxdefault == null) default_code4_maxdefault = getdefault.default_code4_maxdefault;
            if (default_code4_maxboost == null) default_code4_maxboost = getdefault.default_code4_maxboost;
            if (default_code4_maxbomb == null) default_code4_maxbomb = getdefault.default_code4_maxbomb;

            if (Using_Default_Code5 == null) Using_Default_Code5 = getdefault.Using_Default_Code1;
            if (default_code5_maxdefault == null) default_code5_maxdefault = getdefault.default_code5_maxdefault;
            if (default_code5_maxHM == null) default_code5_maxHM = getdefault.default_code5_maxHM;

            if (Using_Default_Code6 == null) Using_Default_Code6 = getdefault.Using_Default_Code1;
            if (default_code6_maxdefault == null) default_code6_maxdefault = getdefault.default_code6_maxdefault;
            if (default_code6_maxPlant == null) default_code6_maxPlant = getdefault.default_code6_maxPlant;
            if (default_code6_addmax_spectrehood == null) default_code6_addmax_spectrehood = getdefault.default_code6_addmax_spectrehood;

            if (Save_Inventory_Log == null) Save_Inventory_Log = getdefault.Save_Inventory_Log;

            if (Ignore_Value_ClearLag == null) Ignore_Value_ClearLag = getdefault.Ignore_Value_ClearLag;
        }
    }

    public class CONFIG_PROGRESSION
    {
        public bool? AllowVanityCloth = true;
        public bool? AllowDungeonRush = false;
        public bool? AllowTempleRush = true;
        public bool? AllowBanners = false;
        public bool? AllowMusicBox = true;
        public CONFIG_PROGRESSION() { }

        public void FixNull()
        {
            CONFIG_PROGRESSION getdefault = new();

            if (AllowVanityCloth == null) AllowVanityCloth = getdefault.AllowVanityCloth;
            if (AllowDungeonRush == null) AllowDungeonRush = getdefault.AllowDungeonRush;
            if (AllowTempleRush == null) AllowTempleRush = getdefault.AllowTempleRush;
            if (AllowBanners == null) AllowBanners = getdefault.AllowBanners;
            if (AllowMusicBox == null) AllowMusicBox = getdefault.AllowMusicBox;
            return;
        }
    }
    public class CONFIG_DISCORD
    {
        public string BotToken = "NONE";
        public ulong? MainGuildID = 0;
        public ulong? MainChannelID = 0;
        public ulong? MainChannelLog = 0;
        public ulong? ReportChannel = 0;
        public ulong? CommandLogChannel = 0;

        public bool? AllowUser_UseIngame_ModPermission = false;
        public CONFIG_DISCORD( ) { }

        public void FixNull()
        {
            CONFIG_DISCORD getdefault = new();

            if (BotToken == null) BotToken = getdefault.BotToken;
            if (MainGuildID == null) MainGuildID = getdefault.MainGuildID;
            if (MainChannelID == null) MainChannelID = getdefault.MainChannelID;
            if (MainChannelLog == null) MainChannelLog = getdefault.MainChannelLog;
            if (ReportChannel == null) ReportChannel = getdefault.ReportChannel;
            if (CommandLogChannel == null) CommandLogChannel = getdefault.CommandLogChannel;
            if (AllowUser_UseIngame_ModPermission == null) AllowUser_UseIngame_ModPermission = getdefault.AllowUser_UseIngame_ModPermission;
        }
    }

    public class CONFIG_PERMISSIONS
    {
        //default
        public string Default_CMD_Ping = "MKLP.default.ping";
        public string Default_CMD_Progression = "MKLP.default.progression";
        public string Default_CMD_Report = "MKLP.default.report";

        //staff
        public string Staff = "MKLP.staff";

        //admin
        public string CMD_MapPingTP = "MKLP.mapping.tp";
        public string CMD_ClearLag = "MKLP.clearlag";

        //moderator
        public string CMD_ManageReport = "MKLP.moderator.report";
        public string CMD_Ban = "MKLP.moderator.ban";
        public string CMD_OfflineBan = "MKLP.moderator.offline.ban";
        public string CMD_UnBan = "MKLP.moderator.unban";
        public string CMD_Disable = "MKLP.moderator.disable";
        public string CMD_Mute = "MKLP.moderator.mute";
        public string CMD_OfflineMute = "MKLP.moderator.offline.mute";
        public string CMD_UnMute = "MKLP.moderator.unmute";
        public string CMD_OfflineUnMute = "MKLP.moderator.offline.unmute";

        //inspect
        public string CMD_InventoryView = "MKLP.staff.Inventory.view";

        public string IgnoreAntiGrief_protectsurface_break = "MKLP.antigrief.protect.surface.break";
        public string IgnoreAntiGrief_protectsurface_explosive = "MKLP.antigrief.protect.surface.explosive";
        public string IgnoreAntiGrief_protectsurface_place = "MKLP.antigrief.protect.surface.place";
        public string IgnoreAntiGrief_protectsurface_placeliquid = "MKLP.antigrief.protect.surface.placeliquid";

        public string Ignore_IllegalWireProgression = "MKLP.bypass.progression.wire";

        public string IgnoreMainCode_1 = "MKLP.bypass.Main.code1";
        public string IgnoreMainCode_2 = "MKLP.bypass.Main.code2";

        public string IgnoreDefaultCode_1 = "MKLP.bypass.Default.code1";
        public string IgnoreDefaultCode_2 = "MKLP.bypass.Default.code2";
        public string IgnoreDefaultCode_3 = "MKLP.bypass.Default.code3";
        public string IgnoreDefaultCode_4 = "MKLP.bypass.Default.code4";
        public string IgnoreDefaultCode_5 = "MKLP.bypass.Default.code5";
        public string IgnoreDefaultCode_6 = "MKLP.bypass.Default.code6";

        public string IgnoreSurvivalCode_1 = "MKLP.bypass.Survival.code1";
        public string IgnoreSurvivalCode_2 = "MKLP.bypass.Survival.code2";
        public string IgnoreSurvivalCode_3 = "MKLP.bypass.Survival.code3";
        public string IgnoreSurvivalCode_4 = "MKLP.bypass.Survival.code4";

        public CONFIG_PERMISSIONS() { }

        public void FixNull()
        {
            CONFIG_PERMISSIONS getdefault = new();

            if (Default_CMD_Ping == null) Default_CMD_Ping = getdefault.Default_CMD_Ping;
            if (Default_CMD_Progression == null) Default_CMD_Progression = getdefault.Default_CMD_Progression;
            if (Default_CMD_Report == null) Default_CMD_Report = getdefault.Default_CMD_Report;

            if (Staff == null) Staff = getdefault.Staff;

            if (CMD_MapPingTP == null) CMD_MapPingTP = getdefault.CMD_MapPingTP;
            if (CMD_ClearLag == null) CMD_ClearLag = getdefault.CMD_ClearLag;

            if (CMD_ManageReport == null) CMD_ManageReport = getdefault.CMD_ManageReport;
            if (CMD_Ban == null) CMD_Ban = getdefault.CMD_Ban;
            if (CMD_OfflineBan == null) CMD_OfflineBan = getdefault.CMD_OfflineBan;
            if (CMD_UnBan == null) CMD_UnBan = getdefault.CMD_UnBan;
            if (CMD_Disable == null) CMD_Disable = getdefault.CMD_Disable;
            if (CMD_Mute == null) CMD_Mute = getdefault.CMD_Mute;
            if (CMD_OfflineMute == null) CMD_OfflineMute = getdefault.CMD_OfflineMute;
            if (CMD_UnMute == null) CMD_UnMute = getdefault.CMD_UnMute;
            if (CMD_OfflineUnMute == null) CMD_OfflineUnMute = getdefault.CMD_OfflineUnMute;
            if (CMD_InventoryView == null) CMD_InventoryView = getdefault.CMD_InventoryView;

            if (IgnoreAntiGrief_protectsurface_break == null) IgnoreAntiGrief_protectsurface_break = getdefault.IgnoreAntiGrief_protectsurface_break;
            if (IgnoreAntiGrief_protectsurface_explosive == null) IgnoreAntiGrief_protectsurface_explosive = getdefault.IgnoreAntiGrief_protectsurface_explosive;
            if (IgnoreAntiGrief_protectsurface_place == null) IgnoreAntiGrief_protectsurface_place = getdefault.IgnoreAntiGrief_protectsurface_place;
            if (IgnoreAntiGrief_protectsurface_placeliquid == null) IgnoreAntiGrief_protectsurface_placeliquid = getdefault.IgnoreAntiGrief_protectsurface_placeliquid;

            if (Ignore_IllegalWireProgression == null) Ignore_IllegalWireProgression = getdefault.Ignore_IllegalWireProgression;

            if (IgnoreMainCode_1 == null) IgnoreMainCode_1 = getdefault.IgnoreMainCode_1;
            if (IgnoreMainCode_2 == null) IgnoreMainCode_2 = getdefault.IgnoreMainCode_2;

            if (IgnoreDefaultCode_1 == null) IgnoreDefaultCode_1 = getdefault.IgnoreDefaultCode_1;
            if (IgnoreDefaultCode_2 == null) IgnoreDefaultCode_2 = getdefault.IgnoreDefaultCode_2;
            if (IgnoreDefaultCode_3 == null) IgnoreDefaultCode_3 = getdefault.IgnoreDefaultCode_3;
            if (IgnoreDefaultCode_4 == null) IgnoreDefaultCode_4 = getdefault.IgnoreDefaultCode_4;
            if (IgnoreDefaultCode_5 == null) IgnoreDefaultCode_5 = getdefault.IgnoreDefaultCode_5;
            if (IgnoreDefaultCode_6 == null) IgnoreDefaultCode_6 = getdefault.IgnoreDefaultCode_6;

            if (IgnoreSurvivalCode_1 == null) IgnoreSurvivalCode_1 = getdefault.IgnoreSurvivalCode_1;
            if (IgnoreSurvivalCode_2 == null) IgnoreSurvivalCode_2 = getdefault.IgnoreSurvivalCode_2;
            if (IgnoreSurvivalCode_3 == null) IgnoreSurvivalCode_3 = getdefault.IgnoreSurvivalCode_3;
            if (IgnoreSurvivalCode_4 == null) IgnoreSurvivalCode_4 = getdefault.IgnoreSurvivalCode_4;


        }
    }

    public class CONFIG_BOSSES
    {
        public bool? AllowKingSlime = true;
        public bool? AllowEyeOfCthulhu = true;
        public bool? AllowEaterOfWorlds = true;
        public bool? AllowBrainOfCthulhu = true;
        public bool? AllowQueenBee = true;
        public bool? AllowSkeletron = true;
        public bool? AllowDeerclops = true;
        public bool? AllowWallOfFlesh = true;
        public bool? AllowQueenSlime = true;
        public bool? AllowTheTwins = true;
        public bool? AllowTheDestroyer = true;
        public bool? AllowSkeletronPrime = true;
        public bool? AllowPlantera = true;
        public bool? AllowGolem = true;
        public bool? AllowDukeFishron = true;
        public bool? AllowEmpressOfLight = true;
        public bool? AllowLunaticCultist = true;
        public bool? AllowMoonLord = true;

        public bool? AllowJoinDuringBoss = true;
        public bool? PreventIllegalBoss = true;
        public int? RequiredPlayersforBoss = 2;
        
        public CONFIG_BOSSES() { }

        public void FixNull()
        {
            CONFIG_BOSSES getdefault = new();

            if (AllowJoinDuringBoss == null) AllowJoinDuringBoss = getdefault.AllowJoinDuringBoss;
            if (PreventIllegalBoss == null) PreventIllegalBoss = getdefault.PreventIllegalBoss;
            if (RequiredPlayersforBoss == null) RequiredPlayersforBoss = getdefault.RequiredPlayersforBoss;

            if (AllowKingSlime == null) AllowKingSlime = getdefault.AllowKingSlime;
            if (AllowEyeOfCthulhu == null) AllowEyeOfCthulhu = getdefault.AllowEyeOfCthulhu;
            if (AllowEaterOfWorlds == null) AllowEaterOfWorlds = getdefault.AllowEaterOfWorlds;
            if (AllowBrainOfCthulhu == null) AllowBrainOfCthulhu = getdefault.AllowBrainOfCthulhu;
            if (AllowQueenBee == null) AllowQueenBee = getdefault.AllowQueenBee;
            if (AllowSkeletron == null) AllowSkeletron = getdefault.AllowSkeletron;
            if (AllowDeerclops == null) AllowDeerclops = getdefault.AllowDeerclops;
            if (AllowWallOfFlesh == null) AllowWallOfFlesh = getdefault.AllowWallOfFlesh;
            if (AllowQueenSlime == null) AllowQueenSlime = getdefault.AllowQueenSlime;
            if (AllowTheTwins == null) AllowTheTwins = getdefault.AllowTheTwins;
            if (AllowTheDestroyer == null) AllowTheDestroyer = getdefault.AllowTheDestroyer;
            if (AllowSkeletronPrime == null) AllowSkeletronPrime = getdefault.AllowSkeletronPrime;
            if (AllowPlantera == null) AllowPlantera = getdefault.AllowPlantera;
            if (AllowGolem == null) AllowGolem = getdefault.AllowGolem;
            if (AllowDukeFishron == null) AllowDukeFishron = getdefault.AllowDukeFishron;
            if (AllowEmpressOfLight == null) AllowEmpressOfLight = getdefault.AllowEmpressOfLight;
            if (AllowLunaticCultist == null) AllowLunaticCultist = getdefault.AllowLunaticCultist;
            if (AllowMoonLord == null) AllowMoonLord = getdefault.AllowMoonLord;
        }
    }

    public class CONFIG_DATABASE
    {
        public bool? UsingDB = false;

        public bool? UsingGSKLP = false;

        public bool? UseTShockFilePath = true;
        public string Path = "file1/file2/";
        public string File = "example.sqlite";

        public string TableName = "LinkedAccounts";
        public string Get_AccountName_DB = "Name";
        public string Get_UserID_DB = "DiscordUserID";

        public bool? UsingCustom = false;

        public string Custom_Get_AccountName_From_UserID = "";
        public string Custom_Get_UserID_From_AccountName = "";
        
        public CONFIG_DATABASE() { }

        public void FixNull()
        {
            CONFIG_DATABASE getdefault = new();

            if (UsingDB == null) UsingDB = getdefault.UsingDB;

            if (UsingGSKLP == null) UsingGSKLP = getdefault.UsingGSKLP;

            if (UseTShockFilePath == null) UseTShockFilePath = getdefault.UseTShockFilePath;
            if (Path == null) Path = getdefault.Path;
            if (File == null) File = getdefault.File;

            if (TableName == null) TableName = getdefault.TableName;
            if (Get_AccountName_DB == null) Get_AccountName_DB = getdefault.Get_AccountName_DB;
            if (Get_UserID_DB == null) Get_UserID_DB = getdefault.Get_UserID_DB;

            if (UsingCustom == null) UsingCustom = getdefault.UsingCustom;

            if (Custom_Get_AccountName_From_UserID == null) Custom_Get_AccountName_From_UserID = getdefault.Custom_Get_AccountName_From_UserID;
            if (Custom_Get_UserID_From_AccountName == null) Custom_Get_UserID_From_AccountName = getdefault.Custom_Get_UserID_From_AccountName;
        }
    }
    #endregion
}
