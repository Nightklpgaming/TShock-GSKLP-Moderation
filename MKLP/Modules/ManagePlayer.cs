//Microsoft
using Discord;
using IL.Terraria.Graphics;
using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework;
using MKLP.Modules;

//System
using System.ComponentModel;
using System.Data;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;


//Terraria
using Terraria;
using TerrariaApi.Server;
//TShock
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;

namespace MKLP.Modules
{
    public static class ManagePlayer
    {

        public static void CheckIllegalItemInventory(TSPlayer player)
        {
            
            Dictionary<int, string> illegalitems = MKLP.IllegalItemProgression;

            int maxvalue = 10;

            if (Main.hardMode) maxvalue = 100;

            foreach (Item check in player.Inventory)
            {
                if (illegalitems.ContainsKey(check.netID) && (bool)MKLP.Config.Main.Using_Survival_Code1)
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
                
                if ((check.value * check.stack) / 5000000 >= maxvalue && check.netID != 74 && !player.HasPermission(MKLP.Config.Permissions.IgnoreMainCode_1) && (bool)MKLP.Config.Main.Using_Main_Code1)
                {
                    DisablePlayer(player, $"Abnormal Item [i/s{check.stack}:{check.netID}]", ServerReason: $"Main,code,1|{check.netID}|{check.stack}");
                    return;
                }
            }

            foreach (Item check in player.TPlayer.miscEquips)
            {
                if (illegalitems.ContainsKey(check.netID) && (bool)MKLP.Config.Main.Using_Survival_Code1)
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
            }
            foreach (Item check in player.TPlayer.miscDyes)
            {
                if (illegalitems.ContainsKey(check.netID) && (bool)MKLP.Config.Main.Using_Survival_Code1)
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
            }

            foreach (Item check in player.TPlayer.armor)
            {
                if (illegalitems.ContainsKey(check.netID) && (bool)MKLP.Config.Main.Using_Survival_Code1)
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
            }

            foreach (Item check in player.TPlayer.dye)
            {
                if (illegalitems.ContainsKey(check.netID) && (bool)MKLP.Config.Main.Using_Survival_Code1)
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                foreach (Item check in player.TPlayer.Loadouts[i].Armor)
                {
                    if (illegalitems.ContainsKey(check.netID) && (bool)MKLP.Config.Main.Using_Survival_Code1)
                    {
                        DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                        return;
                    }
                }

                foreach (Item check in player.TPlayer.Loadouts[i].Dye)
                {
                    if (illegalitems.ContainsKey(check.netID) && (bool)MKLP.Config.Main.Using_Survival_Code1)
                    {
                        DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                        return;
                    }
                }
            }

            foreach (Item check in player.TPlayer.bank.item)
            {
                if (illegalitems.ContainsKey(check.netID) && (bool)MKLP.Config.Main.Using_Survival_Code1)
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
            }
            foreach (Item check in player.TPlayer.bank2.item)
            {
                if (illegalitems.ContainsKey(check.netID) && (bool)MKLP.Config.Main.Using_Survival_Code1)
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
            }
            foreach (Item check in player.TPlayer.bank3.item)
            {
                if (illegalitems.ContainsKey(check.netID) && (bool)MKLP.Config.Main.Using_Survival_Code1)
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
            }
            foreach (Item check in player.TPlayer.bank4.item)
            {
                if (illegalitems.ContainsKey(check.netID) && (bool)MKLP.Config.Main.Using_Survival_Code1)
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
            }
        }

        public static void CheckPreviousInventory(TSPlayer tsplayer, Item[] playerinv, Item[] prevplayerinv)
        {

            for (int i = 0; i < playerinv.Count(); i++)
            {

                if (playerinv[i].netID != prevplayerinv[i].netID ||
                    playerinv[i].stack != prevplayerinv[i].stack ||
                    playerinv[i].prefix != prevplayerinv[i].prefix)
                {

                    InventoryManager.TryAddInvLog(tsplayer, prevplayerinv[i], playerinv[i], i, "Inv");

                    if (playerinv[i].stack == 255)
                    {
                        if (prevplayerinv[i].stack > 240) return;
                        if (prevplayerinv[i].netID == 0 || prevplayerinv[i].stack == 0) return;
                        if (tsplayer.ActiveChest != -1) return;

                        MKLP.Discordklp.KLPBotSendMessage_Warning($"**Warning!** Player named **{tsplayer.Account.Name}** has suspicious activity for **Split Dupe** `( {prevplayerinv[i].stack} ) {prevplayerinv[i].Name}` to `( {playerinv[i].stack} ) {playerinv[i].Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                            $"\n- Please Check this player if they are duping", tsplayer.Account.Name, "Split Duplicating");
                        MKLP.SendStaffMessage($"[MKLP] Player [c/8911f1:{tsplayer.Account.Name}]  has suspicious activity for Dupe [i/s{prevplayerinv[i].stack}:{prevplayerinv[i].netID}] to [i/s{playerinv[i].stack}:{playerinv[i].netID}]" +
                            $"\nPlease Check this player if they are duping", Microsoft.Xna.Framework.Color.MediumPurple);

                    }
                    if (playerinv[i].stack == 500)
                    {
                        if (prevplayerinv[i].stack > 485) return;
                        if (prevplayerinv[i].netID == 0 || prevplayerinv[i].stack == 0) return;
                        if (tsplayer.ActiveChest != -1) return;

                        MKLP.Discordklp.KLPBotSendMessage_Warning($"**Warning!** Player named **{tsplayer.Account.Name}** has suspicious activity for **Dupe** `( {prevplayerinv[i].stack} ) {prevplayerinv[i].Name}` to `( {playerinv[i].stack} ) {playerinv[i].Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                            $"\n- Please Check this player if they are duping", tsplayer.Account.Name, "Duplicating");
                        MKLP.SendStaffMessage($"[MKLP] Player [c/8911f1:{tsplayer.Account.Name}]  has suspicious activity for Dupe [i/s{prevplayerinv[i].stack}:{prevplayerinv[i].netID}] to [i/s{playerinv[i].stack}:{playerinv[i].netID}]" +
                            $"\nPlease Check this player if they are duping", Microsoft.Xna.Framework.Color.MediumPurple);

                    }
                    if (playerinv[i].stack == 1000)
                    {
                        if (prevplayerinv[i].stack > 950) return;
                        if (prevplayerinv[i].netID == 0 || prevplayerinv[i].stack == 0) return;
                        if (tsplayer.ActiveChest != -1) return;

                        MKLP.Discordklp.KLPBotSendMessage_Warning($"**Warning!** Player named **{tsplayer.Account.Name}** has suspicious activity for **Dupe** `( {prevplayerinv[i].stack} ) {prevplayerinv[i].Name}` to `( {playerinv[i].stack} ) {playerinv[i].Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                            $"\n- Please Check this player if they are duping", tsplayer.Account.Name, "Duplicating");
                        MKLP.SendStaffMessage($"[MKLP] Player [c/8911f1:{tsplayer.Account.Name}]  has suspicious activity for Dupe [i/s{prevplayerinv[i].stack}:{prevplayerinv[i].netID}] to [i/s{playerinv[i].stack}:{playerinv[i].netID}]" +
                            $"\nPlease Check this player if they are duping", Microsoft.Xna.Framework.Color.MediumPurple);

                    }
                    if (playerinv[i].stack == 5000)
                    {
                        if (prevplayerinv[i].stack > 4905) return;
                        if (prevplayerinv[i].netID == 0 || prevplayerinv[i].stack == 0) return;
                        if (tsplayer.ActiveChest != -1) return;

                        MKLP.Discordklp.KLPBotSendMessage_Warning($"**Warning!** Player named **{tsplayer.Account.Name}** has suspicious activity for **Dupe** `( {prevplayerinv[i].stack} ) {prevplayerinv[i].Name}` to `( {playerinv[i].stack} ) {playerinv[i].Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                            $"\n- Please Check this player if they are duping", tsplayer.Account.Name, "Duplicating");
                        MKLP.SendStaffMessage($"[MKLP] Player [c/8911f1:{tsplayer.Account.Name}]  has suspicious activity for Dupe [i/s{prevplayerinv[i].stack}:{prevplayerinv[i].netID}] to [i/s{playerinv[i].stack}:{playerinv[i].netID}]" +
                            $"\nPlease Check this player if they are duping", Microsoft.Xna.Framework.Color.MediumPurple);

                    }
                    if (playerinv[i].stack == 9999)
                    {
                        if (prevplayerinv[i].stack > 9905) return;
                        if (prevplayerinv[i].netID == 0 || prevplayerinv[i].stack == 0) return;
                        if (tsplayer.ActiveChest != -1) return;

                        MKLP.Discordklp.KLPBotSendMessage_Warning($"**Warning!** Player named **{tsplayer.Account.Name}** has suspicious activity for **Dupe** `( {prevplayerinv[i].stack} ) {prevplayerinv[i].Name}` to `( {playerinv[i].stack} ) {playerinv[i].Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                            $"\n- Please Check this player if they are duping", tsplayer.Account.Name, "Duplicating");
                        MKLP.SendStaffMessage($"[MKLP] Player [c/8911f1:{tsplayer.Account.Name}]  has suspicious activity for Dupe [i/s{prevplayerinv[i].stack}:{prevplayerinv[i].netID}] to [i/s{playerinv[i].stack}:{playerinv[i].netID}]" +
                            $"\nPlease Check this player if they are duping", Microsoft.Xna.Framework.Color.MediumPurple);

                    }
                }

            }
            /*
            void CheckIfDupe()
            {
            }
            */
        }



        #region [ Disable Player ]

        public static bool DisablePlayer(TSPlayer player, string Reason = "No Reason Specified", string executername = "Unknown", string ServerReason = "")
        {
            if (MKLP.DisabledKey.Contains(Identifier.Name + player.Name) ||
                MKLP.DisabledKey.Contains(Identifier.IP + player.IP) ||
                MKLP.DisabledKey.Contains(Identifier.UUID + player.UUID))
            {
                return false;
            }
            else
            {
                MKLP.DisabledKey.Add(Identifier.Name + player.Name);
                MKLP.DisabledKey.Add(Identifier.IP + player.IP);
                MKLP.DisabledKey.Add(Identifier.UUID + player.UUID);

                if (player.ActiveChest != -1)
                {
                    player.ActiveChest = -1;

                    player.SendData(PacketTypes.ChestOpen, "", -1);
                }

                

                if (ServerReason != "")
                {

                    string[] AC = ServerReason.Split("|")[0].Split(",");

                    string[] ACValue = ServerReason.Split("|");

                    switch (AC[0])
                    {
                        case "Main":
                            #region ( Type: Main )
                            {

                                switch (int.Parse(AC[2]))
                                {
                                    case 1:
                                        #region ( code 1 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player **{player.Name}** Disabled for high Value Item Stack `Item: ({ServerReason.Split("|")[2]}) {Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}`",
                                                player.Account.Name,
                                                "High Value Item Stack");
                                            break;
                                        }
                                    #endregion
                                    case 2:
                                        #region ( code 2 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player **{player.Name}** Disabled for Item Null Boss/Invasion Spawn `ItemID_Held: ({ServerReason.Split("|")[1]}) boss: {ServerReason.Split("|")[2]}`",
                                                player.Account.Name,
                                                "Item Null Boss Spawn");
                                            break;
                                        }
                                        #endregion
                                }


                                break;
                            }
                            #endregion
                        case "Default":
                            #region ( Type: Default )
                            {
                                switch (int.Parse(AC[2]))
                                {
                                    case 1:
                                        #region ( code 1 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player **{player.Name}** Disabled for TileKill Threshold `itemheld: {Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}`",
                                                player.Account.Name,
                                                "Breaking blocks too fast");
                                            break;
                                        }
                                    #endregion
                                    case 2:
                                        #region ( code 2 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player **{player.Name}** Disabled for TilePlace Threshold `itemheld: {Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}`",
                                                player.Account.Name,
                                                "Placing blocks too fast");
                                            break;
                                        }
                                        #endregion
                                    case 3:
                                        #region ( code 3 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player **{player.Name}** Disabled for TilePaint Threshold `itemheld: {Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}`",
                                                player.Account.Name,
                                                "Paint too fast");
                                            break;
                                        }
                                    #endregion
                                    case 4:
                                        #region ( code 4 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player **{player.Name}** Disabled for LiquidTile Threshold `itemheld: {Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}`",
                                                player.Account.Name,
                                                "Flooding to fast");
                                            break;
                                        }
                                    #endregion
                                    case 5:
                                        #region ( code 5 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player **{player.Name}** Disabled for Projectile Threshold `itemheld: {Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}` `projectile name: {Lang.GetProjectileName(int.Parse(ServerReason.Split("|")[2]))}`",
                                                player.Account.Name,
                                                "Spawning projectile too fast at onces");
                                            break;
                                        }
                                        #endregion
                                    case 6:
                                        #region ( code 6 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player **{player.Name}** Disabled for HealOther Threshold `itemheld: {Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}`",
                                                player.Account.Name,
                                                "Healing others too fast");
                                            break;
                                        }
                                        #endregion
                                }


                                break;
                            }
                            #endregion
                        case "Survival":
                            #region ( Type: Survival )
                            {

                                switch (int.Parse(AC[2]))
                                {
                                    case 1:
                                        #region ( code 1 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player **{player.Name}** Disabled for illegal item progression `{Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}` **{ServerReason.Split("|")[2]}**",
                                                player.Account.Name,
                                                "Illegal item progression");
                                            break;
                                        }
                                        #endregion
                                    case 2:
                                        #region ( code 2 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player **{player.Name}** Disabled for illegal Projectile progression `{Lang.GetProjectileName(int.Parse(ServerReason.Split("|")[1]))}` **{ServerReason.Split("|")[2]}**",
                                                player.Account.Name,
                                                "Illegal item progression");
                                            break;
                                        }
                                        #endregion
                                }


                                break;
                            }
                            #endregion
                    }
                }

                player.SendMessage("You have been Disable reason : " + Reason, Microsoft.Xna.Framework.Color.Red);
                if (ServerReason == "")
                {
                    MKLP.Discordklp.KLPBotSendMessageMainLog($"Player **{player.Name}** was Disabled by **{executername}**");
                    MKLP.SendStaffMessage($"{executername} disabled {player.Name} for: {Reason}", Microsoft.Xna.Framework.Color.DarkRed);
                } else
                {
                    MKLP.SendStaffMessage($"{player.Name} was disabled for: {Reason}", Microsoft.Xna.Framework.Color.DarkRed);
                }
                

                return true;
            }


        }

        public static bool UnDisablePlayer(TSPlayer player, string executername = "Unknown")
        {
            if (!MKLP.DisabledKey.Contains(Identifier.Name + player.Name) ||
                !MKLP.DisabledKey.Contains(Identifier.IP + player.IP) ||
                !MKLP.DisabledKey.Contains(Identifier.UUID + player.UUID))
            {

                return false;
            }
            else
            {
                if (MKLP.DisabledKey.Contains(Identifier.Name + player.Name)) MKLP.DisabledKey.Remove(Identifier.Name + player.Name);
                if (MKLP.DisabledKey.Contains(Identifier.IP + player.IP)) MKLP.DisabledKey.Remove(Identifier.IP + player.IP);
                if (MKLP.DisabledKey.Contains(Identifier.UUID + player.UUID)) MKLP.DisabledKey.Remove(Identifier.UUID + player.UUID);

                player.SendMessage("You're now enabled", Microsoft.Xna.Framework.Color.Lime);

                MKLP.SendStaffMessage($"{player.Name} was enable by {executername}", Microsoft.Xna.Framework.Color.DarkRed);

                MKLP.Discordklp.KLPBotSendMessageMainLog($"Player **{player.Name}** was Enabled by **{executername}**");

                return true;
            }



        }

        #endregion

        #region [ Ban ]

        public static bool OnlineBan(bool Silent, TSPlayer Player, string Reason, string Executer, DateTime Duration, bool IP = false, bool UUID = false)
        {
            var getban = TShock.Bans.RetrieveBansByIdentifier(Identifier.Account + Player.Name);

            foreach (Ban ban in getban)
            {
                if (ban.Identifier == Identifier.Name + Player.Name)
                {
                    return false;
                }
            }
            if (Player.Account != null)
            {
                foreach (Ban ban in getban)
                {
                    if (ban.Identifier == Identifier.Account + Player.Account.Name)
                    {
                        return false;
                    }
                }
            }

            string Tickets = "";

            Tickets += $"- {TShock.Bans.InsertBan(Identifier.Name + Player.Name, Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : PlayerName\n";
            if (Player.Account != null) Tickets += $"- {TShock.Bans.InsertBan(Identifier.Account + Player.Account.Name, Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : Account\n";
            if (IP)
            {
                Tickets += $"- {TShock.Bans.InsertBan(Identifier.IP + Player.IP, Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : IP\n";
            }
            if (UUID) Tickets += $"- {TShock.Bans.InsertBan(Identifier.UUID + Player.UUID, Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : UUID\n";

            MKLP.Discordklp.KLPBotSendMessageMainLog($"**{Executer}** 🔨Banned **{Player.Name}** for `{Reason}`" +
                $"\n### Ban Tickets Numbers:\n" +
                Tickets +
                $"-# Duration: {(Duration == DateTime.MaxValue ? "Permanent" : GetDuration(Duration))}");

            if (!Silent) TShock.Utils.Broadcast($"Player [c/3378f0:{Player.Name}] was banned!", Microsoft.Xna.Framework.Color.Cyan);
            
            MKLP.SendStaffMessage($"[MKLP] [c/008ecf:{Player.Name}] was banned by [c/008ecf:{Executer}]", Microsoft.Xna.Framework.Color.DarkCyan);

            Player.Disconnect("You were Banned By " + Executer +
                "\nReason: " + Reason);

            return true;

            string GetDuration(DateTime Expiration)
            {
                TimeSpan getresult = (Expiration - DateTime.UtcNow);

                if (getresult.TotalDays >= 1)
                {
                    return $"{Math.Floor(getresult.TotalDays)}{(getresult.TotalDays <= 1 ? "Day" : "Days")}";
                }
                if (getresult.TotalHours >= 1)
                {
                    return $"{Math.Floor(getresult.TotalHours)}{(getresult.TotalHours <= 1 ? "Hour" : "Hours")}";
                }
                if (getresult.TotalMinutes >= 1)
                {
                    return $"{Math.Floor(getresult.TotalMinutes)}{(getresult.TotalMinutes <= 1 ? "Minute" : "Minutes")}";
                }
                if (getresult.TotalSeconds >= 1)
                {
                    return $"{Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
                }
                return $"Time {Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
            }
        }

        public static bool OfflineBan(UserAccount Account, string Reason, string Executer, DateTime Duration, bool IP = false, bool UUID = false)
        {
            var getban = TShock.Bans.RetrieveBansByIdentifier(Identifier.Account + Account.Name);
            

            foreach (Ban ban in getban)
            {
                if (ban.Identifier == Identifier.Account + Account.Name)
                {
                    return false;
                }
            }

            string Tickets = "";

            Tickets += $"- {TShock.Bans.InsertBan(Identifier.Name + Account.Name, Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : PlayerName\n";
            Tickets += $"- {TShock.Bans.InsertBan(Identifier.Account + Account.Name, Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : Account\n";
            if (IP)
            {
                string[] GetIPs = GetIPListAccount(Account.KnownIps);
                Tickets += $"- {TShock.Bans.InsertBan(Identifier.IP + GetIPs[GetIPs.Count() - 1], Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : IP\n";
            }
            if (UUID) Tickets += $"- {TShock.Bans.InsertBan(Identifier.UUID + Account.UUID, Reason, Executer, DateTime.UtcNow, Duration).Ban.TicketNumber} : UUID\n";

            MKLP.SendStaffMessage($"[MKLP] Account [c/008ecf:{Account.Name}] was banned by [c/008ecf:{Executer}]", Microsoft.Xna.Framework.Color.DarkCyan);

            MKLP.Discordklp.KLPBotSendMessageMainLog($"**{Executer}** 🔨Banned **{Account.Name}** for `{Reason}`" +
                $"\n### Ban Tickets Numbers:\n" +
                Tickets +
                $"-# Duration: {(Duration == DateTime.MaxValue ? "Permanent" : GetDuration(Duration))}");

            return true;

            string GetDuration(DateTime Expiration)
            {
                TimeSpan getresult = (Expiration - DateTime.UtcNow);

                if (getresult.TotalDays >= 1)
                {
                    return $"{Math.Floor(getresult.TotalDays)}{(getresult.TotalDays <= 1 ? "Day" : "Days")}";
                }
                if (getresult.TotalHours >= 1)
                {
                    return $"{Math.Floor(getresult.TotalHours)}{(getresult.TotalHours <= 1 ? "Hour" : "Hours")}";
                }
                if (getresult.TotalMinutes >= 1)
                {
                    return $"{Math.Floor(getresult.TotalMinutes)}{(getresult.TotalMinutes <= 1 ? "Minute" : "Minutes")}";
                }
                if (getresult.TotalSeconds >= 1)
                {
                    return $"{Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
                }
                return $"Time {Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
            }
        }

        public static bool UnBanAccount(UserAccount Account, string Executer)
        {
            var getban = TShock.Bans.RetrieveBansByIdentifier(Identifier.Account + Account.Name);

            bool unbanned = false;

            string Tickets = "";

            string[] getIPs = GetIPListAccount(Account.KnownIps);

            foreach (Ban ban in getban)
            {
                if (ban.Identifier == Identifier.Name + Account.Name)
                {
                    if (TShock.Bans.RemoveBan(ban.TicketNumber, true))
                    {
                        Tickets += $"- {ban.TicketNumber} : PlayerName\n";
                        unbanned = true;
                    }
                }
                if (ban.Identifier == Identifier.Account + Account.Name)
                {
                    if (TShock.Bans.RemoveBan(ban.TicketNumber, true))
                    {
                        Tickets += $"- {ban.TicketNumber} : Account\n";
                        unbanned = true;
                    }
                }
                if (ban.Identifier == Identifier.IP + getIPs[getIPs.Count() - 1])
                {
                    if (TShock.Bans.RemoveBan(ban.TicketNumber, true))
                    {
                        Tickets += $"- {ban.TicketNumber} : IP\n";
                        unbanned = true;
                    }
                }
                if (ban.Identifier == Identifier.UUID + Account.UUID)
                {
                    if (TShock.Bans.RemoveBan(ban.TicketNumber, true))
                    {
                        Tickets += $"- {ban.TicketNumber} : UUID\n";
                        unbanned = true;
                    }
                }
            }

            MKLP.SendStaffMessage($"[MKLP] Account: [c/008ecf:{Account.Name}] was unbanned by [c/008ecf:{Executer}]", Microsoft.Xna.Framework.Color.DarkCyan);

            MKLP.Discordklp.KLPBotSendMessageMainLog($"**{Executer}** ✅UnBan **{Account.Name}**" +
                $"\n### Ban Tickets Removed:\n" +
                Tickets);
            return unbanned;
        }

        public static bool UnBanTicketNumber(int TicketNumber, string Executer)
        {

            if (TShock.Bans.RemoveBan(TicketNumber, true))
            {
                MKLP.SendStaffMessage($"[MKLP] BanTicket: [c/008ecf:{TicketNumber}] was removed by [c/008ecf:{Executer}]", Microsoft.Xna.Framework.Color.DarkCyan);

                MKLP.Discordklp.KLPBotSendMessageMainLog($"**{Executer}** ✅Remove Ticket Ban No. **{TicketNumber}**");
                return true;
            } else
            {
                return false;
            }
        }

        #endregion

        #region [ Mute ]
        public static bool OnlineMute(bool Silent, TSPlayer Player, string Reason, string Executer, DateTime Duration)
        {
            bool MuteSuccess = false;

            if (MKLP.DBManager.AddMute(Identifier.Name + Player.Name, Duration, Reason)) MuteSuccess = true;
            if (Player.Account != null)
            {
                if (MKLP.DBManager.AddMute(Identifier.Account + Player.Account.Name, Duration, Reason)) MuteSuccess = true;
            }
            if (MKLP.DBManager.AddMute(Identifier.IP + Player.IP, Duration, Reason)) MuteSuccess = true;
            if (MKLP.DBManager.AddMute(Identifier.UUID + Player.UUID, Duration, Reason)) MuteSuccess = true;

            if (MuteSuccess)
            {
                Player.mute = true;
                MKLP.Discordklp.KLPBotSendMessageMainLog($"**{Executer}** 🔇Muted **{Player.Name}** for `{Reason}`" +
                    $"\n-# Duration: {(Duration == DateTime.MaxValue ? "Permanent" : GetDuration(Duration))}");

                if (!Silent)
                {
                    TShock.Utils.Broadcast($"[c/228f25:{Executer}] Muted [c/228f25:{Player.Name}] {(Reason == "" ? "" : $"for {Reason}")}", Microsoft.Xna.Framework.Color.Lime);
                } else
                {
                    MKLP.SendStaffMessage($"[MKLP] [c/09c100:{Player.Name}] was muted by [c/09c100:{Executer}] {(Reason == "" ? "" : $"for {Reason}")}", Microsoft.Xna.Framework.Color.DarkOliveGreen);
                }
            }

            return MuteSuccess;

            string GetDuration(DateTime Expiration)
            {
                TimeSpan getresult = (Expiration - DateTime.UtcNow);

                if (getresult.TotalDays >= 1)
                {
                    return $"{Math.Floor(getresult.TotalDays)}{(getresult.TotalDays <= 1 ? "Day" : "Days")}";
                }
                if (getresult.TotalHours >= 1)
                {
                    return $"{Math.Floor(getresult.TotalHours)}{(getresult.TotalHours <= 1 ? "Hour" : "Hours")}";
                }
                if (getresult.TotalMinutes >= 1)
                {
                    return $"{Math.Floor(getresult.TotalMinutes)}{(getresult.TotalMinutes <= 1 ? "Minute" : "Minutes")}";
                }
                if (getresult.TotalSeconds >= 1)
                {
                    return $"{Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
                }
                return $"Time {Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
            }
        }

        public static bool OnlineUnMute(bool Silent, TSPlayer Player, string Executer)
        {
            bool UnMuteSuccess = false;

            if (MKLP.DBManager.DeleteMute(Identifier.Name + Player.Name)) UnMuteSuccess = true;
            if (Player.Account != null)
            {
                if (MKLP.DBManager.DeleteMute(Identifier.Account + Player.Account.Name)) UnMuteSuccess = true;
            }
            if (MKLP.DBManager.DeleteMute(Identifier.IP + Player.IP)) UnMuteSuccess = true;
            if (MKLP.DBManager.DeleteMute(Identifier.UUID + Player.UUID)) UnMuteSuccess = true;

            if (UnMuteSuccess)
            {
                Player.mute = false;

                MKLP.Discordklp.KLPBotSendMessageMainLog($"**{Executer}** 🔊Unmuted **{Player.Name}**");
                if (!Silent)
                {
                    TShock.Utils.Broadcast($"[c/228f25:{Executer}] Unmuted [c/228f25:{Player.Name}]", Microsoft.Xna.Framework.Color.Lime);
                } else
                {
                    MKLP.SendStaffMessage($"[MKLP] [c/09c100:{Player.Name}] was unmuted by [c/09c100:{Executer}]", Microsoft.Xna.Framework.Color.DarkOliveGreen);
                }
            }

            return UnMuteSuccess;
        }

        public static bool OfflineMute(UserAccount Account, string Reason, string Executer, DateTime Duration)
        {

            bool MuteSuccess = false;

            if (MKLP.DBManager.AddMute(Identifier.Account + Account.Name, Duration, Reason)) MuteSuccess = true;
            string[] GetIPs = GetIPListAccount(Account.KnownIps);
            if (MKLP.DBManager.AddMute(Identifier.IP + GetIPs[GetIPs.Count() - 1], Duration, Reason)) MuteSuccess = true;
            if (MKLP.DBManager.AddMute(Identifier.UUID + Account.UUID, Duration, Reason)) MuteSuccess = true;

            if (MuteSuccess)
            {
                MKLP.Discordklp.KLPBotSendMessageMainLog($"**{Executer}** 🔇Muted **{Account.Name}** for `{Reason}`" +
                    $"\n-# Duration: {(Duration == DateTime.MaxValue ? "Permanent" : GetDuration(Duration))}");

                MKLP.SendStaffMessage($"[MKLP] Account: [c/09c100:{Account.Name}] was muted by [c/09c100:{Executer}] {(Reason == "" ? "" : $"for {Reason}")}", Microsoft.Xna.Framework.Color.DarkOliveGreen);
            }

            return MuteSuccess;

            string GetDuration(DateTime Expiration)
            {
                TimeSpan getresult = (Expiration - DateTime.UtcNow);

                if (getresult.TotalDays >= 1)
                {
                    return $"{Math.Floor(getresult.TotalDays)}{(getresult.TotalDays <= 1 ? "Day" : "Days")}";
                }
                if (getresult.TotalHours >= 1)
                {
                    return $"{Math.Floor(getresult.TotalHours)}{(getresult.TotalHours <= 1 ? "Hour" : "Hours")}";
                }
                if (getresult.TotalMinutes >= 1)
                {
                    return $"{Math.Floor(getresult.TotalMinutes)}{(getresult.TotalMinutes <= 1 ? "Minute" : "Minutes")}";
                }
                if (getresult.TotalSeconds >= 1)
                {
                    return $"{Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
                }
                return $"Time {Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds <= 1 ? "Second" : "Seconds")}";
            }
        }

        public static bool OfflineUnMute(UserAccount Account, string Executer)
        {
            bool UnMuteSuccess = false;

            if (MKLP.DBManager.DeleteMute(Identifier.Account + Account.Name)) UnMuteSuccess = true;
            string[] GetIPs = GetIPListAccount(Account.KnownIps);
            if (MKLP.DBManager.DeleteMute(Identifier.IP + GetIPs[GetIPs.Count() - 1])) UnMuteSuccess = true;
            if (MKLP.DBManager.DeleteMute(Identifier.UUID + Account.UUID)) UnMuteSuccess = true;

            if (UnMuteSuccess)
            {
                MKLP.Discordklp.KLPBotSendMessageMainLog($"**{Executer}** 🔊Unmuted **{Account.Name}**");

                MKLP.SendStaffMessage($"[MKLP] Account: [c/09c100:{Account.Name}] was Unmuted by [c/09c100:{Executer}]", Microsoft.Xna.Framework.Color.DarkOliveGreen);
            }

            return UnMuteSuccess;
        }

        #endregion

        static string[] GetIPListAccount(string KnownIPs)
        {
            if (!KnownIPs.Contains(","))
            {
                string[] e = {
                                $"{KnownIPs.Replace("\"", "").Replace("[", "").Replace("]", "").Replace("\n", "").Replace(" ", "")}"
                            };
                return e;
            }
            else
            {
                return KnownIPs
                    .Replace("\"", "")
                    .Replace("[", "")
                    .Replace("]", "")
                    .Replace("\n", "")
                    .Replace(" ", "")
                    .Split(",");

            }
        }
    }
}
