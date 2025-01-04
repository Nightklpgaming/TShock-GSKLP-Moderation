using IL.Terraria;
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
        public CONFIG_CHATMODERATION ChatMod;
        public CONFIG_MANAGEPACKET ManagePackets;
        public CONFIG_PROGRESSION Progression;
        public CONFIG_PERMISSIONS Permissions;
        public CONFIG_DISCORD Discord;
        public CONFIG_BOSSES BossManager;
        public CONFIG_DATABASE DataBase;
        public CONFIG_UNRELEASE_FEATURE Unrelease;

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

            if (args.ChatMod == null) args.ChatMod = new();
            args.ChatMod.FixNull();

            if (args.ManagePackets == null) args.ManagePackets = new();
            args.ManagePackets.FixNull();

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

            if (args.Unrelease == null) args.Unrelease = new();
            args.Unrelease.FixNull();

            File.WriteAllText(path, JsonConvert.SerializeObject(args, Formatting.Indented));
            return args;
        }

        /// <summary>
        /// changes config file
        /// </summary>
        /// <param name="config"></param>
        public void Changeall()
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(Default(), Formatting.Indented));
            }
            else
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }

        private static Config Default()
        {
            return new Config()
            {
                Guide = new(),
                Main = new(),
                ChatMod = new(),
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
        public string HelpText_Seperator1 = "■▶▶▶▶ Seperator ◀◀◀◀■";
        public string HelpText_Seperator2 = "- Warning this is used for discord buttons! if a player tried to use this character they woudn't able to join!";

        public string HelpText_Space1 = " ";
        public string HelpText_Space2 = " ";

        public string HelpText_Codes0a = "■▶▶▶▶ Disable Codes ◀◀◀◀■";
        public string HelpText_Codes1a = "▮▬▬▬ Main Code 1 ▬▬▮";
        public string HelpText_Codes1b = "[ High Stack Value ]";
        public string HelpText_Codes1c = "- Checks if that player has a item which sells a bunch of platinum coins like 5000 chlorophyte which sells a ton of platinum coins";
        public string HelpText_Codes2a = "▮▬▬▬ Main Code 2 ▬▬▮";
        public string HelpText_Codes2b = "[ Null Item Boss/Invasion Spawn ]";
        public string HelpText_Codes2c = "- Checks if that player summons a boss with a wrong item they held";
        public string HelpText_Codes3a = "▮▬▬▬ Survival Code 1 ▬▬▮";
        public string HelpText_Codes3b = "[ Illegal Item Progression ]";
        public string HelpText_Codes3c = "- Checks if that player has item isn't right on progression ( unobtainable included )";
        public string HelpText_Codes4a = "▮▬▬▬ Survival Code 2 ▬▬▮";
        public string HelpText_Codes4b = "[ Illegal Projectile Progression ]";
        public string HelpText_Codes4c = "- Checks if that player summon a projectile that isn't right on progression ( unobtainable included )";
        public string HelpText_Codes5a = "▮▬▬▬ Survival Code 3 ▬▬▮";
        public string HelpText_Codes5b = "[ Illegal Tile Progression ]";
        public string HelpText_Codes5c = "- Checks if that player place a tile that isn't right on progression";
        public string HelpText_Codes6a = "▮▬▬▬ Survival Code 4 ▬▬▮";
        public string HelpText_Codes6b = "[ Illegal Wall Progression ]";
        public string HelpText_Codes6c = "- Checks if that player place a wall that isn't right on progression";
        public string HelpText_Codes7a = "▮▬▬▬ Default Codes ▬▬▮";
        public string HelpText_Codes7b = "[ Default Codes are from tshock anticheat ]";
        public string HelpText_Codes7c = "'Code 1' : Tile Kill Threshold";
        public string HelpText_Codes7d = "'Code 2' : Tile Place Threshold";
        public string HelpText_Codes7e = "'Code 3' : Tile Paint Threshold";
        public string HelpText_Codes7f = "'Code 4' : Liquid Threshold";
        public string HelpText_Codes7g = "'Code 5' : Projectile Threshold";
        public string HelpText_Codes7h = "'Code 6' : HealOther Threshold";

        public string HelpText_Space3 = " ";
        public string HelpText_Space4 = " ";

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

        public string HelpText_Space5 = " ";
        public string HelpText_Space6 = " ";

        public string HelpText_PunishType0a = "■▶▶▶▶ Punishment Type 0-6 ◀◀◀◀■";
        public string HelpText_PunishType1a = "▮▬▬▬▬ [ 0 ] Ban ▬▬▬▬▬▮";
        public string HelpText_PunishType1b = "- bans a player immidiately";
        public string HelpText_PunishType2a = "▮▬▬▬▬ [ 1 ] Disable ▬▬▬▬▬▮";
        public string HelpText_PunishType2b = "- acts a ban but it prevents them from sending packets ( disable's are temporary! )";
        public string HelpText_PunishType3a = "▮▬▬▬▬ [ 2 ] KickAndLog ▬▬▬▬▬▮";
        public string HelpText_PunishType3b = "- kicks a player and sends a modlog";
        public string HelpText_PunishType4a = "▮▬▬▬▬ [ 3 ] Kick ▬▬▬▬▬▮";
        public string HelpText_PunishType4b = "- kicks a player";
        public string HelpText_PunishType5a = "▮▬▬▬▬ [ 4 ] RevertAndLog ▬▬▬▬▬▮";
        public string HelpText_PunishType5b = "- revert their action or their inventory and sends a modlog";
        public string HelpText_PunishType6a = "▮▬▬▬▬ [ 5 ] Revert ▬▬▬▬▬▮";
        public string HelpText_PunishType6b = "- revert their action or their inventory";
        public string HelpText_PunishType7a = "▮▬▬▬▬ [ 6 ] Log ▬▬▬▬▬▮";
        public string HelpText_PunishType7b = "- sends a modlog";

        public string HelpText_Space7 = " ";
        public string HelpText_Space8 = " ";

        public string HelpText_Restart1 = "■▶▶▶▶ Reminder ◀◀◀◀■";
        public string HelpText_Restart2 = "- modifying database/command require's server restart";
    }

    static class Variables
    {
        public static string[] var1 = { "Alt Name 1", "Alt Name 2" };
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
    public class WhiteListAlt
    {
        public string MainName;
        public string[] AltNames;


        public WhiteListAlt(string[] AltNames, string MainName = "Main Account Name")
        {
            this.MainName = MainName;
            this.AltNames = AltNames;
        }

    }
    public class CONFIG_MAIN
    {
        public char Seperator = '¤';
        public byte? Minimum_CharacterName = 1;
        public byte? Maximum_CharacterName = 255;
        public string[] Ban_NameContains = { "fuck", "卍" };
        public string[] IllegalNames = { "ServerConsole", "Server" };
        public bool? Allow_PlayerName_Symbols = true;
        public char[] WhiteList_PlayerName_Symbols = { '[', ':', ']' };
        public bool? Allow_PlayerName_InappropriateWords = false;

        public bool? Target_UserMatchUUIDAndIP = false;
        public bool? Allow_User_JoinMatchUUID = true;
        public WhiteListAlt[]? WhiteList_User_JoinMatchUUID = { new WhiteListAlt(AltNames: Variables.var1), new WhiteListAlt(AltNames: Variables.var1) };

        public string StaffChat_Message_discordacclinkedicon = "[i:3124]";
        public string StaffChat_Message_ingamelinkedicon = "💳";

        public string StaffChat_MessageRecieved_Discord = "[c/0aa3ef:[StaffChat][c/0aa3ef:]] %discordname% : %message%";
        public string StaffChat_HexColor_Discord_Mention_User = "f28f0c";
        public string StaffChat_HexColor_Discord_Mention_Role = "f28f0c";
        public string StaffChat_HexColor_Discord_Mention_Channel = "00FFFF";
        public string StaffChat_Message_Discord_HasAttachment = "[c/34e718:[Attachment][c/34e718:]]";

        public string StaffChat_MessageSend_Discord = "[c/0aa3ef:[StaffChat][c/0aa3ef:]] %ingamename% : %message%";
        public string StaffChat_MessageRecieved_InGame = "⚒️ **%ingamename%** : %message%";
        public CONFIG_COLOR_RBG? StaffChat_MessageRecieved_InGame_RBG = new();

        public bool? Replace_Ban_TShockCommand = false;
        public bool? Replace_Mute_TShockCommand = false;
        public bool? Replace_Who_TShockCommand = true;
        public bool? Replace_AccountInfo_TShockCommand = true;

        public bool? Using_AntiGrief_Infection = false;
        public string Message_AntiGrief_Infection = "You Cannot Use those infection blocks!";

        public bool? Using_AntiGrief_Spray = false;
        public string Message_AntiGrief_Spray = "You Cannot Use clentaminator!";

        public bool? Using_AntiGrief_Surface_Break = false;
        public bool? Using_AntiGrief_Surface_Explosive = false;
        public bool? Using_AntiGrief_Surface_Place = false;
        public bool? Using_AntiGrief_Surface_PlaceLiquid = false;
        public string Message_AntiGrief_Surface_Break = "You Cannot Break Blocks on Surface!";
        public string Message_AntiGrief_Surface_Explosive = "You Cannot Use Explosives on Surface!";
        public string Message_AntiGrief_Surface_Place = "You Cannot Place Blocks on Surface!";
        public string Message_AntiGrief_Surface_PlaceLiquid = "You Cannot use Liquids on Surface!";

        public bool? Use_SuspiciousDupe = true;
        public PunishmentType? SuspiciousDupe_PunishmentType = PunishmentType.Log;

        public bool? Prevent_IllegalWire_Progression = false;
        public bool? ReceivedWarning_WirePlaceUnderground = false;

        public bool? Allow_Players_StackSameAccessory = false;

        public bool? Prevent_Place_BastStatueNearDoor = true;

        public PunishmentType? Main_Code_PunishmentType = PunishmentType.Disable;
        public PunishmentType? Survival_Code_PunishmentType = PunishmentType.Disable;
        public PunishmentType? Default_Code_PunishmentType = PunishmentType.Kick;

        public bool? Using_Main_Code1 = false;
        public bool? Using_Main_Code2 = false;

        public bool? Using_Survival_Code1 = false;
        public int[]? WhiteList_Survival_Code1 = { 1 };
        public bool? Using_Survival_Code2 = false;
        public short[]? WhiteList_Survival_Code2 = { 1 };
        public bool? Using_Survival_Code3 = false;
        //public int[,]? WhiteList_Survival_Code3 = { { 0, 0 }, { 1, 0 } };
        public bool? Using_Survival_Code4 = false;
        public ushort[]? WhiteList_Survival_Code4 = { 1 };

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

        public bool? Use_VanishCMD_TPlayer_Active_Var = false;

        public bool? Use_OnUpdate_Func = true;

        public bool? DetectAllPlayerInv = false;

        public bool? Save_Inventory_Log = false;
        public int? Save_InvLog_Max = 70;
        public int? Remove_InvLog_IfMax = 40;

        public CONFIG_MAIN() { }

        public void FixNull()
        {
            CONFIG_MAIN getdefault = new();

            if (Minimum_CharacterName == null) Minimum_CharacterName = getdefault.Minimum_CharacterName;
            if (Maximum_CharacterName == null) Maximum_CharacterName = getdefault.Maximum_CharacterName;
            if (IllegalNames == null) IllegalNames = getdefault.IllegalNames;
            if (Allow_PlayerName_Symbols == null) Allow_PlayerName_Symbols = getdefault.Allow_PlayerName_Symbols;
            if (WhiteList_PlayerName_Symbols == null) WhiteList_PlayerName_Symbols = getdefault.WhiteList_PlayerName_Symbols;
            if (Allow_PlayerName_InappropriateWords == null) Allow_PlayerName_InappropriateWords = getdefault.Allow_PlayerName_InappropriateWords;

            if (Target_UserMatchUUIDAndIP == null) Target_UserMatchUUIDAndIP = getdefault.Target_UserMatchUUIDAndIP;
            if (Allow_User_JoinMatchUUID == null) Allow_User_JoinMatchUUID = getdefault.Allow_User_JoinMatchUUID;
            if (WhiteList_User_JoinMatchUUID == null) WhiteList_User_JoinMatchUUID = getdefault.WhiteList_User_JoinMatchUUID;

            if (StaffChat_Message_discordacclinkedicon == null) StaffChat_Message_discordacclinkedicon = getdefault.StaffChat_Message_discordacclinkedicon;
            if (StaffChat_Message_ingamelinkedicon == null) StaffChat_Message_ingamelinkedicon = getdefault.StaffChat_Message_ingamelinkedicon;

            if (StaffChat_MessageRecieved_Discord == null) StaffChat_MessageRecieved_Discord = getdefault.StaffChat_MessageRecieved_Discord;
            if (StaffChat_HexColor_Discord_Mention_User == null) StaffChat_HexColor_Discord_Mention_User = getdefault.StaffChat_HexColor_Discord_Mention_User;
            if (StaffChat_HexColor_Discord_Mention_Role == null) StaffChat_HexColor_Discord_Mention_Role = getdefault.StaffChat_HexColor_Discord_Mention_Role;
            if (StaffChat_HexColor_Discord_Mention_Channel == null) StaffChat_HexColor_Discord_Mention_Channel = getdefault.StaffChat_HexColor_Discord_Mention_Channel;
            if (StaffChat_Message_Discord_HasAttachment == null) StaffChat_Message_Discord_HasAttachment = getdefault.StaffChat_Message_Discord_HasAttachment;
            
            if (StaffChat_MessageSend_Discord == null) StaffChat_MessageSend_Discord = getdefault.StaffChat_MessageSend_Discord;
            if (StaffChat_MessageRecieved_InGame == null) StaffChat_MessageRecieved_InGame = getdefault.StaffChat_MessageRecieved_InGame;
            if (StaffChat_MessageRecieved_InGame_RBG == null) StaffChat_MessageRecieved_InGame_RBG = getdefault.StaffChat_MessageRecieved_InGame_RBG;
            CONFIG_COLOR_RBG? is0 = new(0.0f, 0.0f, 0.0f);
            if (StaffChat_MessageRecieved_InGame_RBG.Equals(is0)) StaffChat_MessageRecieved_InGame_RBG = new(255f, 255f, 255f);

            if (Replace_Ban_TShockCommand == null) Replace_Ban_TShockCommand = getdefault.Replace_Ban_TShockCommand;
            if (Replace_Mute_TShockCommand == null) Replace_Mute_TShockCommand = getdefault.Replace_Mute_TShockCommand;
            if (Replace_Who_TShockCommand == null) Replace_Who_TShockCommand = getdefault.Replace_Who_TShockCommand;
            if (Replace_AccountInfo_TShockCommand == null) Replace_AccountInfo_TShockCommand = getdefault.Replace_AccountInfo_TShockCommand;

            if (Using_AntiGrief_Infection == null) Using_AntiGrief_Infection = getdefault.Using_AntiGrief_Infection;
            if (Message_AntiGrief_Infection == null) Message_AntiGrief_Infection = getdefault.Message_AntiGrief_Infection;

            if (Using_AntiGrief_Surface_Break == null) Using_AntiGrief_Surface_Break = getdefault.Using_AntiGrief_Surface_Break;
            if (Using_AntiGrief_Surface_Explosive == null) Using_AntiGrief_Surface_Explosive = getdefault.Using_AntiGrief_Surface_Explosive;
            if (Using_AntiGrief_Surface_Place == null) Using_AntiGrief_Surface_Place = getdefault.Using_AntiGrief_Surface_Place;
            if (Using_AntiGrief_Surface_PlaceLiquid == null) Using_AntiGrief_Surface_PlaceLiquid = getdefault.Using_AntiGrief_Surface_PlaceLiquid;
            if (Message_AntiGrief_Surface_Break == null) Message_AntiGrief_Surface_Break = getdefault.Message_AntiGrief_Surface_Break;
            if (Message_AntiGrief_Surface_Explosive == null) Message_AntiGrief_Surface_Explosive = getdefault.Message_AntiGrief_Surface_Explosive;
            if (Message_AntiGrief_Surface_Place == null) Message_AntiGrief_Surface_Place = getdefault.Message_AntiGrief_Surface_Place;
            if (Message_AntiGrief_Surface_PlaceLiquid == null) Message_AntiGrief_Surface_PlaceLiquid = getdefault.Message_AntiGrief_Surface_PlaceLiquid;

            if (Use_SuspiciousDupe == null) Use_SuspiciousDupe = getdefault.Use_SuspiciousDupe;
            if (SuspiciousDupe_PunishmentType == null) SuspiciousDupe_PunishmentType = getdefault.SuspiciousDupe_PunishmentType;

            if (Prevent_IllegalWire_Progression == null) Prevent_IllegalWire_Progression = getdefault.Prevent_IllegalWire_Progression;
            if (ReceivedWarning_WirePlaceUnderground == null) ReceivedWarning_WirePlaceUnderground = getdefault.ReceivedWarning_WirePlaceUnderground;

            if (Allow_Players_StackSameAccessory == null) Allow_Players_StackSameAccessory = getdefault.Allow_Players_StackSameAccessory;

            if (Prevent_Place_BastStatueNearDoor == null) Prevent_Place_BastStatueNearDoor = getdefault.Prevent_Place_BastStatueNearDoor;

            if (Main_Code_PunishmentType == null) Main_Code_PunishmentType = getdefault.Main_Code_PunishmentType;
            if (Survival_Code_PunishmentType == null) Survival_Code_PunishmentType = getdefault.Survival_Code_PunishmentType;
            if (Default_Code_PunishmentType == null) Default_Code_PunishmentType = getdefault.Default_Code_PunishmentType;

            if (Using_Main_Code1 == null) Using_Main_Code1 = getdefault.Using_Main_Code1;
            if (Using_Main_Code2 == null) Using_Main_Code2 = getdefault.Using_Main_Code2;

            if (Using_Survival_Code1 == null) Using_Survival_Code1 = getdefault.Using_Survival_Code1;
            if (WhiteList_Survival_Code1 == null) WhiteList_Survival_Code1 = getdefault.WhiteList_Survival_Code1;
            if (Using_Survival_Code2 == null) Using_Survival_Code2 = getdefault.Using_Survival_Code2;
            if (WhiteList_Survival_Code2 == null) WhiteList_Survival_Code2 = getdefault.WhiteList_Survival_Code2;
            if (Using_Survival_Code3 == null) Using_Survival_Code3 = getdefault.Using_Survival_Code3;
            //wip
            if (Using_Survival_Code4 == null) Using_Survival_Code4 = getdefault.Using_Survival_Code4;
            if (WhiteList_Survival_Code4 == null) WhiteList_Survival_Code4 = getdefault.WhiteList_Survival_Code4;

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

            if (Ignore_Value_ClearLag == null) Ignore_Value_ClearLag = getdefault.Ignore_Value_ClearLag;

            if (Use_VanishCMD_TPlayer_Active_Var == null) Use_VanishCMD_TPlayer_Active_Var = getdefault.Use_VanishCMD_TPlayer_Active_Var;

            if (Use_OnUpdate_Func == null) Use_OnUpdate_Func = getdefault.Use_OnUpdate_Func;

            if (DetectAllPlayerInv == null) DetectAllPlayerInv = getdefault.DetectAllPlayerInv;

            if (Save_Inventory_Log == null) Save_Inventory_Log = getdefault.Save_Inventory_Log;
            if (Save_InvLog_Max == null) Save_InvLog_Max = getdefault.Save_InvLog_Max;
            if (Remove_InvLog_IfMax == null) Remove_InvLog_IfMax = getdefault.Remove_InvLog_IfMax;
        }
    }

    public class CONFIG_CHATMODERATION
    {
        public bool? Using_Chat_AutoMod = true;

        public string[] Ban_MessageContains = { "nigga", "卍" };

        public int? Maximum_Spammed_MessageLength_NoSpace = 10;
        public int? Maximum_Spammed_MessageLength_WithSpace = 25;
        public int? Threshold_Spammed_MessageLength_NoSpace = 4;
        public int? Threshold_Spammed_MessageLength_WithSpace = 6;

        public int? Millisecond_Threshold = 5000;

        public int? MutePlayer_AtWarning = 4;

        public int? MuteDuration_Seconds = 600;
        public bool? PermanentDuration = false;

        public bool? SendLog_SpamWarning = true;

        public int? Maximum__MessageLength_NoSpace = 30;
        public int? Maximum__MessageLength_WithSpace = 200;

        public bool? EnableLockDown_When_MultipleMutes = false;
        public int? NumberOFPlayersAutoMute_Lockdown = 5;
        public string AutoLockDown_Reason = "Multiple Player Mute's occur";
        public CONFIG_CHATMODERATION() { }

        public void FixNull()
        {
            CONFIG_CHATMODERATION getdefault = new();


            if (Using_Chat_AutoMod == null) Using_Chat_AutoMod = getdefault.Using_Chat_AutoMod;

            if (Ban_MessageContains == null) Ban_MessageContains = getdefault.Ban_MessageContains;

            if (Maximum_Spammed_MessageLength_NoSpace == null) Maximum_Spammed_MessageLength_NoSpace = getdefault.Maximum_Spammed_MessageLength_NoSpace;
            if (Maximum_Spammed_MessageLength_WithSpace == null) Maximum_Spammed_MessageLength_WithSpace = getdefault.Maximum_Spammed_MessageLength_WithSpace;
            if (Threshold_Spammed_MessageLength_NoSpace == null) Threshold_Spammed_MessageLength_NoSpace = getdefault.Threshold_Spammed_MessageLength_NoSpace;
            if (Threshold_Spammed_MessageLength_WithSpace == null) Threshold_Spammed_MessageLength_WithSpace = getdefault.Threshold_Spammed_MessageLength_WithSpace;

            if (Millisecond_Threshold == null) Millisecond_Threshold = getdefault.Millisecond_Threshold;

            if (MutePlayer_AtWarning == null) MutePlayer_AtWarning = getdefault.MutePlayer_AtWarning;

            if (MuteDuration_Seconds == null) MuteDuration_Seconds = getdefault.MuteDuration_Seconds;
            if (PermanentDuration == null) PermanentDuration = getdefault.PermanentDuration;

            if (SendLog_SpamWarning == null) SendLog_SpamWarning = getdefault.SendLog_SpamWarning;

            if (Maximum__MessageLength_NoSpace == null) Maximum__MessageLength_NoSpace = getdefault.Maximum__MessageLength_NoSpace;
            if (Maximum__MessageLength_WithSpace == null) Maximum__MessageLength_WithSpace = getdefault.Maximum__MessageLength_WithSpace;

            if (EnableLockDown_When_MultipleMutes == null) EnableLockDown_When_MultipleMutes = getdefault.EnableLockDown_When_MultipleMutes;
            if (NumberOFPlayersAutoMute_Lockdown == null) NumberOFPlayersAutoMute_Lockdown = getdefault.NumberOFPlayersAutoMute_Lockdown;
            if (AutoLockDown_Reason == null) AutoLockDown_Reason = getdefault.AutoLockDown_Reason;

            return;
        }
    }
    public class CONFIG_MANAGEPACKET
    {
        public bool? Disable_Packet85_QuickStackChest = false;
        public bool? Disable_Packet92_MobPickupCoin = false;
        public CONFIG_MANAGEPACKET() { }

        public void FixNull()
        {
            CONFIG_MANAGEPACKET getdefault = new();

            if (Disable_Packet85_QuickStackChest == null) Disable_Packet85_QuickStackChest = getdefault.Disable_Packet85_QuickStackChest;
            if (Disable_Packet92_MobPickupCoin == null) Disable_Packet92_MobPickupCoin = getdefault.Disable_Packet92_MobPickupCoin;

            return;
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
        public ulong? StaffChannel = 0;
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
            if (StaffChannel == null) StaffChannel = getdefault.StaffChannel;
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
        public string CMD_ClearMessage = "MKLP.message.clear";
        public string CMD_LockDown = "MKLP.lockdown.join";
        public string CMD_LockDownRegister = "MKLP.lockdown.register";
        public string CMD_MapPingTP = "MKLP.mapping.tp";
        public string CMD_ClearLag = "MKLP.clearlag";
        public string CMD_ManageBoss = "MKLP.manageBoss";
        public string CMD_ManageBoss_SetKilled = "MKLP.manageBoss.setkilled";
        public string CMD_Vanish = "MKLP.vanish";

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
        public string CMD_Spy = "MKLP.spy";
        public string CMD_UUIDMatch = "MKLP.uuidmatch";

        //manager
        public string CMD_MKLPDiscord = "MKLP.manager.discordlink";


        //immunity
        public string Ignore_ChatMod_Punishment = "MKLP.bypass.chatmod";

        public string IgnoreAntiGrief_infection = "MKLP.antigrief.protect.infection";
        public string IgnoreAntiGrief_spray = "MKLP.antigrief.protect.spray";

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

            if (CMD_ClearMessage == null) CMD_ClearMessage = getdefault.CMD_ClearMessage;
            if (CMD_LockDown == null) CMD_LockDown = getdefault.CMD_LockDown;
            if (CMD_MapPingTP == null) CMD_MapPingTP = getdefault.CMD_MapPingTP;
            if (CMD_ClearLag == null) CMD_ClearLag = getdefault.CMD_ClearLag;
            if (CMD_ManageBoss == null) CMD_ManageBoss = getdefault.CMD_ManageBoss;
            if (CMD_ManageBoss_SetKilled == null) CMD_ManageBoss_SetKilled = getdefault.CMD_ManageBoss_SetKilled;
            if (CMD_Vanish == null) CMD_Vanish = getdefault.CMD_Vanish;

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
            if (CMD_Spy == null) CMD_Spy = getdefault.CMD_Spy;
            if (CMD_UUIDMatch == null) CMD_UUIDMatch = getdefault.CMD_UUIDMatch;

            if (CMD_MKLPDiscord == null) CMD_MKLPDiscord = getdefault.CMD_MKLPDiscord;

            if (Ignore_ChatMod_Punishment == null) Ignore_ChatMod_Punishment = getdefault.Ignore_ChatMod_Punishment;

            if (IgnoreAntiGrief_infection == null) IgnoreAntiGrief_infection = getdefault.IgnoreAntiGrief_infection;
            if (IgnoreAntiGrief_spray == null) IgnoreAntiGrief_spray = getdefault.IgnoreAntiGrief_spray;

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
        public DateTime? ScheduleAllowKingSlime = DateTime.MaxValue;
        public DateTime? ScheduleAllowEyeOfCthulhu = DateTime.MaxValue;
        public DateTime? ScheduleAllowEaterOfWorlds = DateTime.MaxValue;
        public DateTime? ScheduleAllowBrainOfCthulhu = DateTime.MaxValue;
        public DateTime? ScheduleAllowQueenBee = DateTime.MaxValue;
        public DateTime? ScheduleAllowSkeletron = DateTime.MaxValue;
        public DateTime? ScheduleAllowDeerclops = DateTime.MaxValue;
        public DateTime? ScheduleAllowWallOfFlesh = DateTime.MaxValue;
        public DateTime? ScheduleAllowQueenSlime = DateTime.MaxValue;

        public DateTime? ScheduleAllowMechdusa = DateTime.MaxValue;

        public DateTime? ScheduleAllowTheTwins = DateTime.MaxValue;
        public DateTime? ScheduleAllowTheDestroyer = DateTime.MaxValue;
        public DateTime? ScheduleAllowSkeletronPrime = DateTime.MaxValue;

        public DateTime? ScheduleAllowPlantera = DateTime.MaxValue;
        public DateTime? ScheduleAllowGolem = DateTime.MaxValue;
        public DateTime? ScheduleAllowDukeFishron = DateTime.MaxValue;
        public DateTime? ScheduleAllowEmpressOfLight = DateTime.MaxValue;
        public DateTime? ScheduleAllowLunaticCultist = DateTime.MaxValue;
        public DateTime? ScheduleAllowMoonLord = DateTime.MaxValue;

        public int? Default_ScheduleDay_AllowKingSlime = 0;
        public int? Default_ScheduleDay_AllowEyeOfCthulhu = 1;
        public int? Default_ScheduleDay_AllowEaterOfWorlds = 2;
        public int? Default_ScheduleDay_AllowBrainOfCthulhu = 2;
        public int? Default_ScheduleDay_AllowQueenBee = 3;
        public int? Default_ScheduleDay_AllowSkeletron = 4;
        public int? Default_ScheduleDay_AllowDeerclops = 2;
        public int? Default_ScheduleDay_AllowWallOfFlesh = 6;
        public int? Default_ScheduleDay_AllowQueenSlime = 6;

        public int? Default_ScheduleDay_AllowMechdusa = 8;

        public int? Default_ScheduleDay_AllowTheTwins = 7;
        public int? Default_ScheduleDay_AllowTheDestroyer = 8;
        public int? Default_ScheduleDay_AllowSkeletronPrime = 9;

        public int? Default_ScheduleDay_AllowPlantera = 10;
        public int? Default_ScheduleDay_AllowGolem = 12;
        public int? Default_ScheduleDay_AllowDukeFishron = 11;
        public int? Default_ScheduleDay_AllowEmpressOfLight = 13;
        public int? Default_ScheduleDay_AllowLunaticCultist = 14;
        public int? Default_ScheduleDay_AllowMoonLord = 14;

        public int? Default_ScheduleDay_Hour = 0;

        public bool? UseBossSchedule = false;

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
        public bool? AllowMechdusa = true;
        public bool? AllowPlantera = true;
        public bool? AllowGolem = true;
        public bool? AllowDukeFishron = true;
        public bool? AllowEmpressOfLight = true;
        public bool? AllowLunaticCultist = true;
        public bool? AllowMoonLord = true;

        public int? KingSlime_RequiredPlayersforBoss = 1;
        public int? EyeOfCthulhu_RequiredPlayersforBoss = 1;
        public int? BrainOfCthulhu_RequiredPlayersforBoss = 1;
        public int? EaterOfWorlds_RequiredPlayersforBoss = 1;
        public int? QueenBee_RequiredPlayersforBoss = 1;
        public int? Skeletron_RequiredPlayersforBoss = 1;
        public int? Deerclops_RequiredPlayersforBoss = 1;
        public int? WallOfFlesh_RequiredPlayersforBoss = 1;
        public int? QueenSlime_RequiredPlayersforBoss = 1;
        public int? TheTwins_RequiredPlayersforBoss = 1;
        public int? TheDestroyer_RequiredPlayersforBoss = 1;
        public int? SkeletronPrime_RequiredPlayersforBoss = 1;
        public int? Mechdusa_RequiredPlayersforBoss = 1;
        public int? Plantera_RequiredPlayersforBoss = 1;
        public int? Golem_RequiredPlayersforBoss = 1;
        public int? DukeFishron_RequiredPlayersforBoss = 1;
        public int? EmpressOfLight_RequiredPlayersforBoss = 1;
        public int? LunaticCultist_RequiredPlayersforBoss = 1;
        public int? MoonLord_RequiredPlayersforBoss = 1;

        public bool? AllowJoinDuringBoss = true;
        public bool? PreventIllegalBoss = true;

        public ulong? Discord_BossEnableChannel = 0;
        public ulong? Discord_BossEnableRole = 0;
        public string Discord_BossEnableMessage = "%notification% **%bossname%** has been Enabled!";
        public string Discord_BossEnableCMDMessage = "%notification% **%playername%** has Enabled **%bossname%!**";
        //public string Discord_BossDisableMessage = "%notification% **%bossname%** has been Disabled!";
        //public string Discord_BossDisableCMDMessage = "%notification% **%playername%** has Disabled **%bossname%!**";
        public CONFIG_BOSSES() { }

        public void FixNull()
        {
            CONFIG_BOSSES getdefault = new();

            //schedule
            if (ScheduleAllowKingSlime == null) ScheduleAllowKingSlime = getdefault.ScheduleAllowKingSlime;
            if (ScheduleAllowEyeOfCthulhu == null) ScheduleAllowEyeOfCthulhu = getdefault.ScheduleAllowEyeOfCthulhu;
            if (ScheduleAllowEaterOfWorlds == null) ScheduleAllowEaterOfWorlds = getdefault.ScheduleAllowEaterOfWorlds;
            if (ScheduleAllowBrainOfCthulhu == null) ScheduleAllowBrainOfCthulhu = getdefault.ScheduleAllowBrainOfCthulhu;
            if (ScheduleAllowQueenBee == null) ScheduleAllowQueenBee = getdefault.ScheduleAllowQueenBee;
            if (ScheduleAllowSkeletron == null) ScheduleAllowSkeletron = getdefault.ScheduleAllowSkeletron;
            if (ScheduleAllowDeerclops == null) ScheduleAllowDeerclops = getdefault.ScheduleAllowDeerclops;
            if (ScheduleAllowWallOfFlesh == null) ScheduleAllowWallOfFlesh = getdefault.ScheduleAllowWallOfFlesh;
            if (ScheduleAllowQueenSlime == null) ScheduleAllowQueenSlime = getdefault.ScheduleAllowQueenSlime;

            if (ScheduleAllowMechdusa == null) ScheduleAllowMechdusa = getdefault.ScheduleAllowMechdusa;

            if (ScheduleAllowTheTwins == null) ScheduleAllowTheTwins = getdefault.ScheduleAllowTheTwins;
            if (ScheduleAllowTheDestroyer == null) ScheduleAllowTheDestroyer = getdefault.ScheduleAllowTheDestroyer;
            if (ScheduleAllowSkeletronPrime == null) ScheduleAllowSkeletronPrime = getdefault.ScheduleAllowSkeletronPrime;

            if (ScheduleAllowPlantera == null) ScheduleAllowPlantera = getdefault.ScheduleAllowPlantera;
            if (ScheduleAllowGolem == null) ScheduleAllowGolem = getdefault.ScheduleAllowGolem;
            if (ScheduleAllowDukeFishron == null) ScheduleAllowDukeFishron = getdefault.ScheduleAllowDukeFishron;
            if (ScheduleAllowEmpressOfLight == null) ScheduleAllowEmpressOfLight = getdefault.ScheduleAllowEmpressOfLight;
            if (ScheduleAllowLunaticCultist == null) ScheduleAllowLunaticCultist = getdefault.ScheduleAllowLunaticCultist;
            if (ScheduleAllowMoonLord == null) ScheduleAllowMoonLord = getdefault.ScheduleAllowMoonLord;

            //default
            if (Default_ScheduleDay_AllowKingSlime == null) Default_ScheduleDay_AllowKingSlime = getdefault.Default_ScheduleDay_AllowKingSlime;
            if (Default_ScheduleDay_AllowEyeOfCthulhu == null) Default_ScheduleDay_AllowEyeOfCthulhu = getdefault.Default_ScheduleDay_AllowEyeOfCthulhu;
            if (Default_ScheduleDay_AllowEaterOfWorlds == null) Default_ScheduleDay_AllowEaterOfWorlds = getdefault.Default_ScheduleDay_AllowEaterOfWorlds;
            if (Default_ScheduleDay_AllowBrainOfCthulhu == null) Default_ScheduleDay_AllowBrainOfCthulhu = getdefault.Default_ScheduleDay_AllowBrainOfCthulhu;
            if (Default_ScheduleDay_AllowQueenBee == null) Default_ScheduleDay_AllowQueenBee = getdefault.Default_ScheduleDay_AllowQueenBee;
            if (Default_ScheduleDay_AllowSkeletron == null) Default_ScheduleDay_AllowSkeletron = getdefault.Default_ScheduleDay_AllowSkeletron;
            if (Default_ScheduleDay_AllowDeerclops == null) Default_ScheduleDay_AllowDeerclops = getdefault.Default_ScheduleDay_AllowDeerclops;
            if (Default_ScheduleDay_AllowWallOfFlesh == null) Default_ScheduleDay_AllowWallOfFlesh = getdefault.Default_ScheduleDay_AllowWallOfFlesh;
            if (Default_ScheduleDay_AllowQueenSlime == null) Default_ScheduleDay_AllowQueenSlime = getdefault.Default_ScheduleDay_AllowQueenSlime;

            if (Default_ScheduleDay_AllowMechdusa == null) Default_ScheduleDay_AllowMechdusa = getdefault.Default_ScheduleDay_AllowMechdusa;

            if (Default_ScheduleDay_AllowTheTwins == null) Default_ScheduleDay_AllowTheTwins = getdefault.Default_ScheduleDay_AllowTheTwins;
            if (Default_ScheduleDay_AllowTheDestroyer == null) Default_ScheduleDay_AllowTheDestroyer = getdefault.Default_ScheduleDay_AllowTheDestroyer;
            if (Default_ScheduleDay_AllowSkeletronPrime == null) Default_ScheduleDay_AllowSkeletronPrime = getdefault.Default_ScheduleDay_AllowSkeletronPrime;

            if (Default_ScheduleDay_AllowPlantera == null) Default_ScheduleDay_AllowPlantera = getdefault.Default_ScheduleDay_AllowPlantera;
            if (Default_ScheduleDay_AllowGolem == null) Default_ScheduleDay_AllowGolem = getdefault.Default_ScheduleDay_AllowGolem;
            if (Default_ScheduleDay_AllowDukeFishron == null) Default_ScheduleDay_AllowDukeFishron = getdefault.Default_ScheduleDay_AllowDukeFishron;
            if (Default_ScheduleDay_AllowEmpressOfLight == null) Default_ScheduleDay_AllowEmpressOfLight = getdefault.Default_ScheduleDay_AllowEmpressOfLight;
            if (Default_ScheduleDay_AllowLunaticCultist == null) Default_ScheduleDay_AllowLunaticCultist = getdefault.Default_ScheduleDay_AllowLunaticCultist;
            if (Default_ScheduleDay_AllowMoonLord == null) Default_ScheduleDay_AllowMoonLord = getdefault.Default_ScheduleDay_AllowMoonLord;

            if (Default_ScheduleDay_Hour == null) Default_ScheduleDay_Hour = getdefault.Default_ScheduleDay_Hour;

            if (UseBossSchedule == null) UseBossSchedule = getdefault.UseBossSchedule;

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

            if (KingSlime_RequiredPlayersforBoss == null) KingSlime_RequiredPlayersforBoss = getdefault.KingSlime_RequiredPlayersforBoss;
            if (EyeOfCthulhu_RequiredPlayersforBoss == null) EyeOfCthulhu_RequiredPlayersforBoss = getdefault.EyeOfCthulhu_RequiredPlayersforBoss;
            if (EaterOfWorlds_RequiredPlayersforBoss == null) EaterOfWorlds_RequiredPlayersforBoss = getdefault.EaterOfWorlds_RequiredPlayersforBoss;
            if (BrainOfCthulhu_RequiredPlayersforBoss == null) BrainOfCthulhu_RequiredPlayersforBoss = getdefault.BrainOfCthulhu_RequiredPlayersforBoss;
            if (QueenBee_RequiredPlayersforBoss == null) QueenBee_RequiredPlayersforBoss = getdefault.QueenBee_RequiredPlayersforBoss;
            if (Skeletron_RequiredPlayersforBoss == null) Skeletron_RequiredPlayersforBoss = getdefault.Skeletron_RequiredPlayersforBoss;
            if (Deerclops_RequiredPlayersforBoss == null) Deerclops_RequiredPlayersforBoss = getdefault.Deerclops_RequiredPlayersforBoss;
            if (WallOfFlesh_RequiredPlayersforBoss == null) WallOfFlesh_RequiredPlayersforBoss = getdefault.WallOfFlesh_RequiredPlayersforBoss;
            if (QueenSlime_RequiredPlayersforBoss == null) QueenSlime_RequiredPlayersforBoss = getdefault.QueenSlime_RequiredPlayersforBoss;
            if (TheTwins_RequiredPlayersforBoss == null) TheTwins_RequiredPlayersforBoss = getdefault.TheTwins_RequiredPlayersforBoss;
            if (TheDestroyer_RequiredPlayersforBoss == null) TheDestroyer_RequiredPlayersforBoss = getdefault.TheDestroyer_RequiredPlayersforBoss;
            if (SkeletronPrime_RequiredPlayersforBoss == null) SkeletronPrime_RequiredPlayersforBoss = getdefault.SkeletronPrime_RequiredPlayersforBoss;
            if (Plantera_RequiredPlayersforBoss == null) Plantera_RequiredPlayersforBoss = getdefault.Plantera_RequiredPlayersforBoss;
            if (Golem_RequiredPlayersforBoss == null) Golem_RequiredPlayersforBoss = getdefault.Golem_RequiredPlayersforBoss;
            if (DukeFishron_RequiredPlayersforBoss == null) DukeFishron_RequiredPlayersforBoss = getdefault.DukeFishron_RequiredPlayersforBoss;
            if (EmpressOfLight_RequiredPlayersforBoss == null) EmpressOfLight_RequiredPlayersforBoss = getdefault.EmpressOfLight_RequiredPlayersforBoss;
            if (LunaticCultist_RequiredPlayersforBoss == null) LunaticCultist_RequiredPlayersforBoss = getdefault.LunaticCultist_RequiredPlayersforBoss;
            if (MoonLord_RequiredPlayersforBoss == null) MoonLord_RequiredPlayersforBoss = getdefault.MoonLord_RequiredPlayersforBoss;

            if (AllowJoinDuringBoss == null) AllowJoinDuringBoss = getdefault.AllowJoinDuringBoss;
            if (PreventIllegalBoss == null) PreventIllegalBoss = getdefault.PreventIllegalBoss;


            if (Discord_BossEnableChannel == null) Discord_BossEnableChannel = getdefault.Discord_BossEnableChannel;
            if (Discord_BossEnableRole == null) Discord_BossEnableRole = getdefault.Discord_BossEnableRole;
            if (Discord_BossEnableMessage == null) Discord_BossEnableMessage = getdefault.Discord_BossEnableMessage;
            if (Discord_BossEnableCMDMessage == null) Discord_BossEnableCMDMessage = getdefault.Discord_BossEnableCMDMessage;
            //if (Discord_BossDisableMessage == null) Discord_BossDisableMessage = getdefault.Discord_BossDisableMessage;
            //if (Discord_BossDisableCMDMessage == null) Discord_BossDisableCMDMessage = getdefault.Discord_BossDisableCMDMessage;
        }
    }

    public class CONFIG_DATABASE
    {
        public bool? UsingDB = true;

        public bool? UsingMKLPDatabase = true;

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

            if (UsingMKLPDatabase == null) UsingMKLPDatabase = getdefault.UsingMKLPDatabase;

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

    public class CONFIG_UNRELEASE_FEATURE
    {
        public string InfoText = "This is a beta stage! ( do not use it )";
        public bool? Detect_ItemPlayerSpawn = false;

        public CONFIG_UNRELEASE_FEATURE()
        {
            InfoText = "This is a beta stage! ( do not use it )";
        }

        public void FixNull()
        {
            CONFIG_UNRELEASE_FEATURE getdefault = new();

            InfoText = "This is a beta stage! ( do not use it )";

            if (Detect_ItemPlayerSpawn == null) Detect_ItemPlayerSpawn = getdefault.Detect_ItemPlayerSpawn;
        }
    }
    #endregion


}
