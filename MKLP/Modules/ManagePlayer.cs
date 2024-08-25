//Microsoft
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

        #region [ Account Link Value ]

        public static ulong GetUserIDFromPlayerName(string playername)
        {
            using var reader = MKLP.MAINKLP_DB.QueryReader("SELECT * FROM Accounts WHERE Name = @0", playername);

            while (reader.Read())
            {
                if (reader.Get<string>("Discord") == "0")
                {
                    throw new NullReferenceException();
                }
                else
                {
                    return ulong.Parse(reader.Get<string>("Discord"));
                }
            }

            throw new NullReferenceException();
        }

        public static string GetPlayerNameFromUserID(ulong userid)
        {
            using var reader = MKLP.MAINKLP_DB.QueryReader("SELECT * FROM Accounts WHERE Discord = @0", userid);

            while (reader.Read())
            {
                return reader.Get<string>("Name");
            }

            throw new NullReferenceException();
        }

        #endregion

        public static void CheckIllegalItemInventory(TSPlayer player)
        {

            Dictionary<int, string> illegalitems = SurvivalManager.GetIllegalItem();

            int maxvalue = 10;

            if (Main.hardMode) maxvalue = 100;

            foreach (Item check in player.Inventory)
            {
                if (illegalitems.ContainsKey(check.netID))
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
                
                if ((check.value * check.stack) / 5000000 >= maxvalue && check.netID != 74 && !player.HasPermission(MKLP.Config.Permissions.IgnoreCode_1))
                {
                    DisablePlayer(player, $"Abnormal Item [i/s{check.stack}:{check.netID}]", ServerReason: $"Main,code,1|{check.netID}|{check.stack}");
                    return;
                }
            }

            foreach (Item check in player.TPlayer.miscEquips)
            {
                if (illegalitems.ContainsKey(check.netID))
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
            }
            foreach (Item check in player.TPlayer.miscDyes)
            {
                if (illegalitems.ContainsKey(check.netID))
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
            }

            foreach (Item check in player.TPlayer.armor)
            {
                if (illegalitems.ContainsKey(check.netID))
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
            }

            foreach (Item check in player.TPlayer.dye)
            {
                if (illegalitems.ContainsKey(check.netID))
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                foreach (Item check in player.TPlayer.Loadouts[i].Armor)
                {
                    if (illegalitems.ContainsKey(check.netID))
                    {
                        DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                        return;
                    }
                }

                foreach (Item check in player.TPlayer.Loadouts[i].Dye)
                {
                    if (illegalitems.ContainsKey(check.netID))
                    {
                        DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                        return;
                    }
                }
            }

            foreach (Item check in player.TPlayer.bank.item)
            {
                if (illegalitems.ContainsKey(check.netID))
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
            }
            foreach (Item check in player.TPlayer.bank2.item)
            {
                if (illegalitems.ContainsKey(check.netID))
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
            }
            foreach (Item check in player.TPlayer.bank3.item)
            {
                if (illegalitems.ContainsKey(check.netID))
                {
                    DisablePlayer(player, $"{illegalitems[check.netID]} Item Progression", ServerReason: $"Survival,code,1|{check.netID}|{illegalitems[check.netID]}");
                    return;
                }
            }
            foreach (Item check in player.TPlayer.bank4.item)
            {
                if (illegalitems.ContainsKey(check.netID))
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

                    }
                    if (playerinv[i].stack == 500)
                    {
                        if (prevplayerinv[i].stack > 485) return;
                        if (prevplayerinv[i].netID == 0 || prevplayerinv[i].stack == 0) return;
                        if (tsplayer.ActiveChest != -1) return;

                        MKLP.Discordklp.KLPBotSendMessage_Warning($"**Warning!** Player named **{tsplayer.Account.Name}** has suspicious activity for **Split Dupe** `( {prevplayerinv[i].stack} ) {prevplayerinv[i].Name}` to `( {playerinv[i].stack} ) {playerinv[i].Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                            $"\n- Please Check this player if they are duping", tsplayer.Account.Name, "Split Duplicating");

                    }
                    if (playerinv[i].stack == 1000)
                    {
                        if (prevplayerinv[i].stack > 985) return;
                        if (prevplayerinv[i].netID == 0 || prevplayerinv[i].stack == 0) return;
                        if (tsplayer.ActiveChest != -1) return;

                        MKLP.Discordklp.KLPBotSendMessage_Warning($"**Warning!** Player named **{tsplayer.Account.Name}** has suspicious activity for **Split Dupe** `( {prevplayerinv[i].stack} ) {prevplayerinv[i].Name}` to `( {playerinv[i].stack} ) {playerinv[i].Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                            $"\n- Please Check this player if they are duping", tsplayer.Account.Name, "Split Duplicating");

                    }
                    if (playerinv[i].stack == 5000)
                    {
                        if (prevplayerinv[i].stack > 4985) return;
                        if (prevplayerinv[i].netID == 0 || prevplayerinv[i].stack == 0) return;
                        if (tsplayer.ActiveChest != -1) return;

                        MKLP.Discordklp.KLPBotSendMessage_Warning($"**Warning!** Player named **{tsplayer.Account.Name}** has suspicious activity for **Split Dupe** `( {prevplayerinv[i].stack} ) {prevplayerinv[i].Name}` to `( {playerinv[i].stack} ) {playerinv[i].Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                            $"\n- Please Check this player if they are duping", tsplayer.Account.Name, "Split Duplicating");

                    }
                    if (playerinv[i].stack == 9999)
                    {
                        if (prevplayerinv[i].stack > 9985) return;
                        if (prevplayerinv[i].netID == 0 || prevplayerinv[i].stack == 0) return;
                        if (tsplayer.ActiveChest != -1) return;

                        MKLP.Discordklp.KLPBotSendMessage_Warning($"**Warning!** Player named **{tsplayer.Account.Name}** has suspicious activity for **Split Dupe** `( {prevplayerinv[i].stack} ) {prevplayerinv[i].Name}` to `( {playerinv[i].stack} ) {playerinv[i].Name}` `ActiveChest: {tsplayer.ActiveChest}`" +
                            $"\n- Please Check this player if they are duping", tsplayer.Account.Name, "Split Duplicating");

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
            if (MKLP.DisabledKey.Contains(player.IP) ||
                MKLP.DisabledKey.Contains(player.UUID))
            {
                return false;
            }
            else
            {
                MKLP.DisabledKey.Add(player.IP);
                MKLP.DisabledKey.Add(player.UUID);

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
                                                $"Player {player.Name} Disabled for high Value Item Stack `Item: ({ServerReason.Split("|")[2]}) {Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}`",
                                                player.Account.Name,
                                                "High Value Item Stack");
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
                                                $"Player {player.Name} Disabled for TileKill Threshold `itemheld: {Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}`",
                                                player.Account.Name,
                                                "Breaking blocks too fast");
                                            break;
                                        }
                                    #endregion
                                    case 2:
                                        #region ( code 2 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player {player.Name} Disabled for TilePlace Threshold `itemheld: {Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}`",
                                                player.Account.Name,
                                                "Placing blocks too fast");
                                            break;
                                        }
                                        #endregion
                                    case 3:
                                        #region ( code 3 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player {player.Name} Disabled for TilePaint Threshold `itemheld: {Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}`",
                                                player.Account.Name,
                                                "Paint too fast");
                                            break;
                                        }
                                    #endregion
                                    case 4:
                                        #region ( code 4 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player {player.Name} Disabled for LiquidTile Threshold `itemheld: {Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}`",
                                                player.Account.Name,
                                                "Flooding to fast");
                                            break;
                                        }
                                    #endregion
                                    case 5:
                                        #region ( code 5 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player {player.Name} Disabled for Projectile Threshold `itemheld: {Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}` `projectile name: {ServerReason.Split("|")[2]}`",
                                                player.Account.Name,
                                                "Spawning projectile too fast at onces");
                                            break;
                                        }
                                        #endregion
                                    case 6:
                                        #region ( code 6 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player {player.Name} Disabled for HealOther Threshold `itemheld: {Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}`",
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
                                                $"Player {player.Name} Disabled for illegal item progression `{Lang.GetItemNameValue(int.Parse(ServerReason.Split("|")[1]))}` **{ServerReason.Split("|")[2]}**",
                                                player.Account.Name,
                                                "Illegal item progression");
                                            break;
                                        }
                                        #endregion
                                    case 2:
                                        #region ( code 2 )
                                        {

                                            MKLP.Discordklp.KLPBotSendMessage_Disabled(
                                                $"Player {player.Name} Disabled for illegal Projectile progression `{Lang.GetProjectileName(int.Parse(ServerReason.Split("|")[1]))}` **{ServerReason.Split("|")[2]}**",
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

                player.SendMessage("You have been Disable reason : " + Reason, Color.Red);
                if (ServerReason == "")
                {
                    MKLP.Discordklp.KLPBotSendMessageLog(MKLP.Config.Discord.MainChannelLog, $"Player {player.Name} was Disabled by {executername}");
                }
                

                return true;
            }


        }

        public static bool UnDisablePlayer(TSPlayer player, string executername = "Unknown")
        {
            if (!MKLP.DisabledKey.Contains(player.IP) ||
                !MKLP.DisabledKey.Contains(player.UUID))
            {

                return false;
            }
            else
            {
                if (MKLP.DisabledKey.Contains(player.IP)) MKLP.DisabledKey.Remove(player.IP);
                if (MKLP.DisabledKey.Contains(player.UUID)) MKLP.DisabledKey.Remove(player.UUID);

                player.SendMessage("You're now enabled", Color.Lime);

                MKLP.Discordklp.KLPBotSendMessageLog(MKLP.Config.Discord.MainChannelLog, $"Player **{player.Name}** was Enabled by **{executername}**");

                return true;
            }



        }

        #endregion

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

            TShock.Bans.InsertBan(Identifier.Account + Account.Name, Reason, Executer, DateTime.UtcNow, Duration);
            if (IP)
            {
                if (!Account.KnownIps.Contains(","))
                {
                    TShock.Bans.InsertBan(Identifier.IP + Account.KnownIps.Replace("\"", "").Replace("[", "").Replace("]", "").Replace("\n", "").Replace(" ", ""), Reason, Executer, DateTime.UtcNow, Duration);
                } else
                {
                    string[] IPs = Account.KnownIps.Replace("\"", "").Replace("[", "").Replace("]", "").Replace("\n", "").Replace(" ", "").Split(",");
                    TShock.Bans.InsertBan(Identifier.IP + IPs[IPs.Count() - 1], Reason, Executer, DateTime.UtcNow, Duration);
                }
                
            }
            if (UUID) TShock.Bans.InsertBan(Identifier.UUID + Account.UUID, Reason, Executer, DateTime.UtcNow, Duration);
            return true;
        }

    }
}
