using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace MKLP
{
    public class Config
    {

        public CONFIG_PERMISSIONS Permissions;
        public CONFIG_DISCORD Discord;
        public CONFIG_BOSSES BossManager;

        static string path = Path.Combine(TShock.SavePath, "MKLP.json");

        public static Config Read()
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(Default(), Formatting.Indented));
                return Default();
            }
            try
            {
                var args = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
                return args;
            }
            catch
            {
                return Default();
            }
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
                Permissions = new(),
                Discord = new(),
                BossManager = new(),
            };
        }

    }


    #region [ Config Objects ]
    /*
    class CONFIG_MAIN
    {

    }
    */
    public class CONFIG_DISCORD
    {
        public string BotToken;
        public ulong MainGuildID;
        public ulong MainChannelID;
        public ulong MainChannelLog;
        public ulong CommandLogChannel;
        public CONFIG_DISCORD(string BotToken = "NONE",
            ulong MainGuildID = 0,
            ulong MainChannelID = 0,
            ulong MainChannelLog = 0,
            ulong CommandLogChannel = 0
            )
        {
            this.BotToken = BotToken;
            this.MainGuildID = MainGuildID;
            this.MainChannelID = MainChannelID;
            this.MainChannelLog = MainChannelLog;
            this.CommandLogChannel = CommandLogChannel;
        }
    }

    public class CONFIG_PERMISSIONS
    {
        public string Staff;

        public string CMD_Disable;
        public string CMD_InventoryView;

        public string IgnoreAntiGrief_protectsurface_break;
        public string IgnoreAntiGrief_protectsurface_explosive;

        public string IgnoreCode_1;

        public string IgnoreDefaultCode_1;
        public string IgnoreDefaultCode_2;
        public string IgnoreDefaultCode_3;
        public string IgnoreDefaultCode_4;
        public string IgnoreDefaultCode_5;
        public string IgnoreDefaultCode_6;

        public string IgnoreSurvivalCode_1;
        public string IgnoreSurvivalCode_2;
        public string IgnoreSurvivalCode_3;

        public CONFIG_PERMISSIONS(
            string Staff = "MKLP.staff",

            string CMD_Disable = "MKLP.staff.disable",
            string CMD_InventoryView = "MKLP.staff.Inventory.view",

            string IgnoreAntiGrief_protectsurface_break = "MKLP.antigrief.protect.surface.break",
            string IgnoreAntiGrief_protectsurface_explosive = "MKLP.antigrief.protect.surface.explosive",

            string IgnoreCode_1 = "MKLP.Main.code1",

            string IgnoreDefaultCode_1 = "MKLP.Default.code1",
            string IgnoreDefaultCode_2 = "MKLP.Default.code2",
            string IgnoreDefaultCode_3 = "MKLP.Default.code3",
            string IgnoreDefaultCode_4 = "MKLP.Default.code4",
            string IgnoreDefaultCode_5 = "MKLP.Default.code5",
            string IgnoreDefaultCode_6 = "MKLP.Default.code6",

            string IgnoreSurvivalCode_1 = "MKLP.Survival.code1",
            string IgnoreSurvivalCode_2 = "MKLP.Survival.code2",
            string IgnoreSurvivalCode_3 = "MKLP.Survival.code3"
            )
        {
            this.Staff = Staff;

            this.CMD_Disable = CMD_Disable;
            this.CMD_InventoryView = CMD_InventoryView;

            this.IgnoreAntiGrief_protectsurface_break = IgnoreAntiGrief_protectsurface_break;
            this.IgnoreAntiGrief_protectsurface_explosive = IgnoreAntiGrief_protectsurface_explosive;

            this.IgnoreCode_1 = IgnoreCode_1;

            this.IgnoreDefaultCode_1 = IgnoreDefaultCode_1;
            this.IgnoreDefaultCode_2 = IgnoreDefaultCode_2;
            this.IgnoreDefaultCode_3 = IgnoreDefaultCode_3;
            this.IgnoreDefaultCode_4 = IgnoreDefaultCode_4;
            this.IgnoreDefaultCode_5 = IgnoreDefaultCode_5;
            this.IgnoreDefaultCode_6 = IgnoreDefaultCode_6;

            this.IgnoreSurvivalCode_1 = IgnoreSurvivalCode_1;
            this.IgnoreSurvivalCode_2 = IgnoreSurvivalCode_2;
            this.IgnoreSurvivalCode_3 = IgnoreSurvivalCode_3;
        }
    }

    public class CONFIG_BOSSES
    {
        public bool AllowKingSlime;
        public bool AllowEyeOfCthulhu;
        public bool AllowEaterOfWorlds;
        public bool AllowBrainOfCthulhu;
        public bool AllowQueenBee;
        public bool AllowSkeletron;
        public bool AllowDeerclops;
        public bool AllowWallOfFlesh;
        public bool AllowQueenSlime;
        public bool AllowTheTwins;
        public bool AllowTheDestroyer;
        public bool AllowSkeletronPrime;
        public bool AllowPlantera;
        public bool AllowGolem;
        public bool AllowDukeFishron;
        public bool AllowEmpressOfLight;
        public bool AllowLunaticCultist;
        public bool AllowMoonLord;
        public bool AllowJoinDuringBoss;

        public bool PreventIllegalBoss;
        public int RequiredPlayersforBoss;

        public CONFIG_BOSSES(
            bool KS = true,
            bool EOC = true,
            bool EOW = true,
            bool BOC = true,
            bool QB = true,
            bool SANS = true,
            bool DEER = true,
            bool WOF = true,
            bool QS = true,
            bool MECH1 = true,
            bool MECH2 = true,
            bool MECH3 = true,
            bool PLANT = true,
            bool GOLEM = true,
            bool DUKE = true,
            bool EOL = true,
            bool CULTIST = true,
            bool ML = true,

            bool AllowJoinDuringBoss = true,
            bool PreventIllegalBoss = true,
            int RequiredPlayersforBoss = 2)
        {
            this.AllowJoinDuringBoss = AllowJoinDuringBoss;
            this.PreventIllegalBoss = PreventIllegalBoss;
            this.RequiredPlayersforBoss = RequiredPlayersforBoss;

            AllowKingSlime = KS;
            AllowEyeOfCthulhu = EOC;
            AllowEaterOfWorlds = EOW;
            AllowBrainOfCthulhu = BOC;
            AllowQueenBee = QB;
            AllowSkeletron = SANS;
            AllowDeerclops = DEER;
            AllowWallOfFlesh = WOF;
            AllowQueenSlime = QS;
            AllowTheTwins = MECH1;
            AllowTheDestroyer = MECH2;
            AllowSkeletronPrime = MECH3;
            AllowPlantera = PLANT;
            AllowGolem = GOLEM;
            AllowDukeFishron = DUKE;
            AllowEmpressOfLight = EOL;
            AllowLunaticCultist = CULTIST;
            AllowMoonLord = ML;
        }
    }


    class CONFIG_OBJ1
    {

    }

    #endregion



}
