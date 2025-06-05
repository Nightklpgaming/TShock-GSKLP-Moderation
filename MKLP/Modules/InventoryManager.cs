using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using Microsoft.Xna.Framework;

namespace MKLP.Modules
{
    public static class InventoryManager
    {

        public static List<string> InventoryLogs = new();
        static int InvLog_index = 0;

        public static void TryAddInvLog(TSPlayer tsplayer, Item prevplayerinv, Item playerinv, int slot, string Type)
        {
            if (!(bool)MKLP.Config.Main.Logging.Save_Inventory_Log) return;

            InvLog_index++;

            if (InventoryLogs.Count >= (int)MKLP.Config.Main.Logging.Save_InvLog_Max)
            {
                InventoryLogs.RemoveRange(0, (int)MKLP.Config.Main.Logging.Remove_InvLog_IfMax);
            }
            //OutOfMemoryException

            string log = $"{tsplayer.Account.Name}{DiscordKLP.S_}{Type}{DiscordKLP.S_}{slot}{DiscordKLP.S_}" +
                $"{prevplayerinv.netID},{prevplayerinv.stack},{prevplayerinv.prefix}" +
                DiscordKLP.S_ +
                $"{playerinv.netID},{playerinv.stack},{playerinv.prefix}|{InvLog_index}";

            if (!InventoryLogs.Contains(log))
            {
                InventoryLogs.Add(log);
            }
        }
        public static void TryAddInvLog(TSPlayer tsplayer, NetItem prevplayerinv, Item playerinv, int slot, string Type)
        {
            if (!(bool)MKLP.Config.Main.Logging.Save_Inventory_Log) return;

            InvLog_index++;

            if (InventoryLogs.Count >= (int)MKLP.Config.Main.Logging.Save_InvLog_Max)
            {
                InventoryLogs.RemoveRange(0, (int)MKLP.Config.Main.Logging.Remove_InvLog_IfMax);
            }
            //OutOfMemoryException

            string log = $"{tsplayer.Account.Name}{DiscordKLP.S_}{Type}{DiscordKLP.S_}{slot}{DiscordKLP.S_}" +
                $"{prevplayerinv.NetId},{prevplayerinv.Stack},{prevplayerinv.PrefixId}" +
                DiscordKLP.S_ +
                $"{playerinv.netID},{playerinv.stack},{playerinv.prefix}|{InvLog_index}";

            if (!InventoryLogs.Contains(log))
            {
                InventoryLogs.Add(log);
            }
        }

        public static void InventoryView(CommandArgs args)
        {
            TSPlayer Player = args.Player;
            if (args.Parameters.Count != 1 && args.Parameters.Count != 2)
            {
                Player.SendErrorMessage(MKLP.GetText($"Invalid syntax. Proper syntax: {Commands.Specifier}inventoryview <player> <type>" +
                    $"\nDo '{Commands.Specifier}inventoryview help' for more info"));
                return;
            }

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Specify a Player!\nDo '{Commands.Specifier}inventoryview help' for more info"));
                return;
            }

            //help text
            string helptext = MKLP.GetText("[i:3619] [c/00f412:Inventory Viewer Info] [i:3619]" +
                    "\nYou can view player contents using this command\n" +
                    $"Example: {Commands.Specifier}inventoryview [c/abff96:{Player.Name}] [c/96ffdc:inv] (View inventory contents)\n" +
                    "[c/a4ff4e:[List of Types][c/a4ff4e:]]\n" +
                    "[c/ffffff:'inventory/inv'] [c/71b45a:'equipment/equip'] [c/f268ff:'piggy/pig'] [c/6f6f6f:'safe'] [c/e3fa00:'defenderforge/forge'] [c/c600fa:'voidvault/vault'] [c/fa2b00:'all']" +
                    "\n------------------------------" +
                    "\n[c/fab200:about 'track' type]\ninfo: get logged when a player inventory changes... \nturnoff: to turn it off repeat the command again\n[c/f40000:warning: this can flood your chat message]" +
                    "\n------------------------------");

            if (args.Parameters[0] == "help")
            {
                Player.SendMessage(helptext, Color.WhiteSmoke);
                return;
            }

            if (args.Parameters.Count == 1)
            {
                args.Player.SendErrorMessage(MKLP.GetText($"Specify a type!\nDo [ {Commands.Specifier}inventoryview help ] for more info"));
                return;
            }

            var foundPlr = TSPlayer.FindByNameOrID(args.Parameters[0]);
            if (foundPlr.Count == 0)
            {
                args.Player.SendErrorMessage(MKLP.GetText("Invalid player!"));
                return;
            }

            var targetplayer = foundPlr[0];
            string targetplayername = targetplayer.Name;

            //makes a variable to check if this player is logged in or not ( usefull to avoid false ban )
            string targetplayerlogin = MKLP.GetText("[c/5c5c5c:status: ][c/f40000:This player hasn't been logged in!]");
            if (targetplayer.IsLoggedIn)
            {
                targetplayerlogin = MKLP.GetText("[c/5c5c5c:status: ][c/05f400:this player is logged in.]");
            }

            #region Types
            switch (args.Parameters[1])
            {
                case "inventory":
                case "inv":
                    {
                        InvView_InventoryString get = new(Player.RealPlayer, targetplayer, "inventory");
                        Player.SendMessage($"( [c/ffffff:{targetplayer.Name}] ) inventory:\n{get.Inventory}\n\n{targetplayerlogin}", Color.WhiteSmoke);
                        return;
                    }
                case "equipment":
                case "equip":
                    {
                        InvView_InventoryString get = new(Player.RealPlayer, targetplayer, "equipment");
                        Player.SendMessage($"( [c/ffffff:{targetplayer.Name}] ) Equipment:\n{get.Equipment}\n\n{targetplayerlogin}", Color.Green);
                        return;
                    }
                case "piggybank":
                case "piggy":
                case "pig":
                    {
                        InvView_InventoryString get = new(Player.RealPlayer, targetplayer, "piggybank");
                        Player.SendMessage($"( [c/ffffff:{targetplayername}] ) Piggy Bank:\n{get.PiggyBank}\n\n{targetplayerlogin}", Color.Pink);
                        return;
                    }
                case "safe":
                    {
                        InvView_InventoryString get = new(Player.RealPlayer, targetplayer, "safe");
                        Player.SendMessage($"( [c/ffffff:{targetplayername}] ) Safe:\n{get.Safe}\n\n{targetplayerlogin}", Color.Gray);
                        return;
                    }
                case "defenderforge":
                case "forge":
                    {
                        InvView_InventoryString get = new(Player.RealPlayer, targetplayer, "defenderforge");
                        Player.SendMessage($"( [c/ffffff:{targetplayername}] ) defender's forge:\n{get.DefenderForge}\n\n{targetplayerlogin}", Color.Yellow);
                        return;
                    }
                case "voidvault":
                case "void":
                case "vault":
                    {
                        InvView_InventoryString get = new(Player.RealPlayer, targetplayer, "voidvault");
                        Player.SendMessage($"( [c/ffffff:{targetplayername}] ) Void vault\n{get.VoidVault}\n\n{targetplayerlogin}", Color.Purple);
                        return;
                    }
                case "all":
                    {
                        InvView_InventoryString get = new(Player.RealPlayer, targetplayer, "all");
                        Player.SendMessage($"( [c/ffffff:{targetplayername}] ) Inventory:\n{get.Inventory}\n\n" +
                            $"Equipment:\n{get.Equipment}\n" +
                            $"piggy bank:\n{get.PiggyBank}\n" +
                            $"safe:\n{get.Safe}\n" +
                            $"defender's forge:\n{get.DefenderForge}\n" +
                            $"void vault:\n{get.VoidVault}\n" +
                            $"{targetplayerlogin}", Color.Gray);
                        return;
                    }
                default:
                    {
                        Player.SendErrorMessage($"Invalid type!\nDo [ {Commands.Specifier}inventoryview help ] for more info");
                        return;
                    }

            }
            #endregion
        }

        #region Functions


        #endregion
    }

    #region OBJECTS
    public class InvView_InventoryString
    {
        public string Inventory = "|";
        public string Equipment = "|";
        public string PiggyBank = "|";
        public string Safe = "|";
        public string DefenderForge = "|";
        public string VoidVault = "|";

        /*
        public InvView_InventoryString(string inventory, string equipment, string piggybank, string safe, string defenderforge, string voidvault)
        {
            Inventory = inventory;
            Equipment = equipment;
            PiggyBank = piggybank;
            Safe = safe;
            DefenderForge = defenderforge;
            VoidVault = voidvault;
        }
        */

        /// <summary>
        /// functions of /inventoryview 
        /// insert a type to get a value [inventory, equipment, piggybank, safe, voidvault, defenderforge]
        /// </summary>
        /// <param name="executer"></param>
        /// <param name="target"></param>
        /// <param name="type"></param>
        public InvView_InventoryString(bool InGame, TSPlayer target, string type)
        {

            if (InGame)
            {
                #region inventory
                if (type == "inventory" || type == "all")
                {
                    int d2 = 10;
                    for (int i = 0; i < target.TPlayer.inventory.Length; i++)
                    {

                        string sp = $"/s{target.TPlayer.inventory[i].stack}";
                        if (target.TPlayer.inventory[i].prefix != 0) sp = $"/p{target.TPlayer.inventory[i].prefix}";

                        if (i == d2)
                        {
                            Inventory += $"\n|[i{sp}:{target.TPlayer.inventory[i].netID}]|";
                            d2 += 10;
                        }
                        else
                        {
                            Inventory += $"[i{sp}:{target.TPlayer.inventory[i].netID}]|";
                        }
                    }
                    Inventory += $"\n[ [i/s{target.TPlayer.trashItem.stack}:{target.TPlayer.trashItem.netID}] ]";
                }
                #endregion


                #region Equipment
                if (type == "equipment" || type == "all")
                {
                    string loadoutused = $"( in use )loadout {target.TPlayer.CurrentLoadoutIndex + 1}:\n";
                    string loadout1 = "";
                    string loadout2 = "";
                    string loadout3 = "";
                    string misclist = "";
                    for (int i = 0; i < 10; i++)
                    {
                        int ii = i + 10;
                        if (i < 5)
                        {
                            misclist += $"|[i/s{target.TPlayer.miscDyes[i].stack}:{target.TPlayer.miscDyes[i].netID}]|[i/s{target.TPlayer.miscEquips[i].stack}:{target.TPlayer.miscEquips[i].netID}]|\n";
                        }
                        if (i < 3)
                        {
                            loadoutused += $"|[i/s{target.TPlayer.dye[i + 3].stack}:{target.TPlayer.dye[i + 3].netID}]|[i/s{target.TPlayer.armor[ii + 3].stack}:{target.TPlayer.armor[ii + 3].netID}]|[i/s{target.TPlayer.armor[i + 3].stack}:{target.TPlayer.armor[i + 3].netID}]|====|[i/s{target.TPlayer.dye[i].stack}:{target.TPlayer.dye[i].netID}]|[i/s{target.TPlayer.armor[ii].stack}:{target.TPlayer.armor[ii].netID}]|[i/s{target.TPlayer.armor[i].stack}:{target.TPlayer.armor[i].netID}]|\n";
                        }
                        if (i >= 6 && i <= 9)
                        {
                            loadoutused += $"|[i/s{target.TPlayer.dye[i].stack}:{target.TPlayer.dye[i].netID}]|[i/s{target.TPlayer.armor[ii].stack}:{target.TPlayer.armor[ii].netID}]|[i/s{target.TPlayer.armor[i].stack}:{target.TPlayer.armor[i].netID}]|\n";
                        }
                    }
                    for (int il = 0; il < 3; il++)
                    {
                        string loadoutget = $"loadout {il + 1}:";
                        for (int i = 0; i < 10; i++)
                        {
                            int ii = i + 10;
                            if (i < 3)
                            {
                                loadoutget += $"|[i/s{target.TPlayer.Loadouts[il].Dye[i + 3].stack}:{target.TPlayer.Loadouts[il].Dye[i + 3].netID}]|[i/s{target.TPlayer.Loadouts[il].Armor[ii + 3].stack}:{target.TPlayer.Loadouts[il].Armor[ii + 3].netID}]|[i/s{target.TPlayer.Loadouts[il].Armor[i + 3].stack}:{target.TPlayer.Loadouts[il].Armor[i + 3].netID}]|====|[i/s{target.TPlayer.Loadouts[il].Dye[i].stack}:{target.TPlayer.Loadouts[il].Dye[i].netID}]|[i/s{target.TPlayer.Loadouts[il].Armor[ii].stack}:{target.TPlayer.Loadouts[il].Armor[ii].netID}]|[i/s{target.TPlayer.Loadouts[il].Armor[i].stack}:{target.TPlayer.Loadouts[il].Armor[i].netID}]|\n";
                            }
                            if (i >= 6 && i <= 9)
                            {
                                loadoutget += $"|[i/s{target.TPlayer.Loadouts[il].Dye[i].stack}:{target.TPlayer.Loadouts[il].Dye[i].netID}]|[i/s{target.TPlayer.Loadouts[il].Armor[ii].stack}:{target.TPlayer.Loadouts[il].Armor[ii].netID}]|[i/s{target.TPlayer.Loadouts[il].Armor[i].stack}:{target.TPlayer.Loadouts[il].Armor[i].netID}]|\n";
                            }
                        }
                        switch (il)
                        {
                            case 0:
                                loadout1 = loadoutget;
                                break;
                            case 1:
                                loadout2 = loadoutget;
                                break;
                            case 2:
                                loadout3 = loadoutget;
                                break;
                        }
                    }
                    switch (target.TPlayer.CurrentLoadoutIndex)
                    {
                        case 0:
                            loadout1 = loadoutused;
                            break;
                        case 1:
                            loadout2 = loadoutused;
                            break;
                        case 2:
                            loadout3 = loadoutused;
                            break;
                    }
                    Equipment = $"{loadout1}\n{loadout2}\n{loadout3}\nmisc:\n{misclist}";
                }
                #endregion


                #region  PiggyBank
                if (type == "piggybank" || type == "all")
                {
                    int d2 = 10;
                    for (int i = 0; i < target.TPlayer.bank.item.Length; i++)
                    {

                        string sp = $"/s{target.TPlayer.bank.item[i].stack}";
                        if (target.TPlayer.bank.item[i].prefix != 0) sp = $"/p{target.TPlayer.bank.item[i].prefix}";

                        if (i == d2)
                        {
                            PiggyBank += $"\n|[i{sp}:{target.TPlayer.bank.item[i].netID}]|";
                            d2 += 10;
                        }
                        else
                        {
                            PiggyBank += $"[i{sp}:{target.TPlayer.bank.item[i].netID}]|";
                        }
                    }
                }
                #endregion


                #region Safe
                if (type == "safe" || type == "all")
                {
                    int d2 = 10;
                    for (int i = 0; i < target.TPlayer.bank2.item.Length; i++)
                    {

                        string sp = $"/s{target.TPlayer.bank2.item[i].stack}";
                        if (target.TPlayer.bank2.item[i].prefix != 0) sp = $"/p{target.TPlayer.bank2.item[i].prefix}";

                        if (i == d2)
                        {
                            Safe += $"\n|[i{sp}:{target.TPlayer.bank2.item[i].netID}]|";
                            d2 += 10;
                        }
                        else
                        {
                            Safe += $"[i{sp}:{target.TPlayer.bank2.item[i].netID}]|";
                        }
                    }
                }
                #endregion


                #region Defender Forge
                if (type == "defenderforge" || type == "all")
                {
                    int d2 = 10;
                    for (int i = 0; i < target.TPlayer.bank3.item.Length; i++)
                    {

                        string sp = $"/s{target.TPlayer.bank3.item[i].stack}";
                        if (target.TPlayer.bank3.item[i].prefix != 0) sp = $"/p{target.TPlayer.bank3.item[i].prefix}";

                        if (i == d2)
                        {
                            DefenderForge += $"\n|[i{sp}:{target.TPlayer.bank3.item[i].netID}]|";
                            d2 += 10;
                        }
                        else
                        {
                            DefenderForge += $"[i{sp}:{target.TPlayer.bank3.item[i].netID}]|";
                        }
                    }
                }
                #endregion


                #region Void Vault
                if (type == "voidvault" || type == "all")
                {
                    int d2 = 10;
                    for (int i = 0; i < target.TPlayer.bank4.item.Length; i++)
                    {

                        string sp = $"/s{target.TPlayer.bank4.item[i].stack}";
                        if (target.TPlayer.bank4.item[i].prefix != 0) sp = $"/p{target.TPlayer.bank4.item[i].prefix}";

                        if (i == d2)
                        {
                            VoidVault += $"\n|[i{sp}:{target.TPlayer.bank4.item[i].netID}]|";
                            d2 += 10;
                        }
                        else
                        {
                            VoidVault += $"[i{sp}:{target.TPlayer.bank4.item[i].netID}]|";
                        }
                    }
                }
                #endregion

                Inventory = Inventory.Replace("[i/s0:0]", "   ");
                Equipment = Equipment.Replace("[i/s0:0]", "   ");
                PiggyBank = PiggyBank.Replace("[i/s0:0]", "   ");
                Safe = Safe.Replace("[i/s0:0]", "   ");
                DefenderForge = DefenderForge.Replace("[i/s0:0]", "   ");
                VoidVault = VoidVault.Replace("[i/s0:0]", "   ");
            }
            else
            {


                #region prefix list
                string[] prefixlist = { "" ,"Large", "Massive", "Dangerous", "Savage", "Sharp", "Pointy", "Tiny",
                    "Terrible", "Small", "Dull", "Unhappy", "Bulky", "Shameful", "Heavy", "Light", "Sighted", "Sighted",
                    "Sighted", "Intimidating", "Deadly", "Staunch", "Awful", "Lethargic", "Awkward", "Powerful", "Mystic",
                   "Adept", "Masterful", "Inept", "Ignorant", "Deranged", "Intense", "Taboo", "Celestial", "Furious", "Keen",
                    "Superior", "Forceful", "Broken", "Damaged", "Shoddy", "Quick", "Deadly", "Agile", "Nimble", "Murderous",
                    "Slow", "Sluggish", "Lazy", "Annoying", "Nasty", "Manic", "Hurtful", "Strong", "Unpleasant", "Weak",
                    "Ruthless", "Frenzying", "Godly", "Demonic", "Zealous", "Hard", "Guarding", "Armored", "Warding",
                    "Arcane", "Precise", "Lucky", "Jagged", "Spiked", "Angry", "Menacing", "Brisk", "Fleeting", "Hasty",
                    "Quick", "Wild", "Rash", "Intrepid", "Violent", "Legendary", "Unreal", "Mythical", "Legendary", "Piercing" };
                #endregion


                #region inventory
                if (type == "inventory" || type == "all")
                {
                    int d2 = 10;
                    for (int i = 0; i < target.TPlayer.inventory.Length; i++)
                    {

                        string p = "";
                        if (target.TPlayer.inventory[i].prefix != 0) p = $"{prefixlist[target.TPlayer.inventory[i].prefix]}";

                        string s = "";
                        if (target.TPlayer.inventory[i].stack > 1) s = $"({target.TPlayer.inventory[i].stack}) ";

                        if (i == d2)
                        {
                            Inventory += $"\n|[{p} {target.TPlayer.inventory[i].Name} {s}]|";
                            d2 += 10;
                        }
                        else
                        {
                            Inventory += $"[{p} {target.TPlayer.inventory[i].Name} {s}]|";
                        }
                    }
                    Inventory += $"\n[ {prefixlist[target.TPlayer.trashItem.prefix]} {target.TPlayer.trashItem.Name} ({target.TPlayer.trashItem.stack}) ]";
                }
                #endregion


                #region Equipment
                if (type == "equipment" || type == "all")
                {
                    string loadoutused = $"( in use )loadout {target.TPlayer.CurrentLoadoutIndex + 1}:\n";
                    string loadout1 = "";
                    string loadout2 = "";
                    string loadout3 = "";
                    string misclist = "";
                    for (int i = 0; i < 10; i++)
                    {
                        int ii = i + 10;

                        if (i < 5)
                        {
                            misclist += $"|[{target.TPlayer.miscDyes[i].Name}]|[{target.TPlayer.miscEquips[i].Name}]|\n";
                        }
                        if (i < 3)
                        {
                            loadoutused += $"|[{target.TPlayer.dye[i + 3].Name}]|[{prefixlist[target.TPlayer.armor[ii + 3].prefix]} {target.TPlayer.armor[ii + 3].Name}]|[{prefixlist[target.TPlayer.armor[i + 3].prefix]} {target.TPlayer.armor[i + 3].Name} ]|====|[ {target.TPlayer.dye[i].Name} ]|[ {target.TPlayer.armor[ii].Name} ]|[ {target.TPlayer.armor[i].Name} ]|\n";
                        }
                        if (i >= 6 && i <= 9)
                        {
                            loadoutused += $"|[ {target.TPlayer.dye[i].Name} ]|[ {prefixlist[target.TPlayer.armor[ii].prefix]} {target.TPlayer.armor[ii].Name} ]|[ {prefixlist[target.TPlayer.armor[i].prefix]} {target.TPlayer.armor[i].Name} ]|\n";
                        }
                    }
                    for (int il = 0; il < 3; il++)
                    {
                        string loadoutget = $"loadout {il}:";
                        for (int i = 0; i < 10; i++)
                        {
                            int ii = i + 10;
                            if (i < 3)
                            {
                                loadoutget += $"|[ {target.TPlayer.Loadouts[il].Dye[i + 3].Name} ]|[ {prefixlist[target.TPlayer.Loadouts[il].Armor[ii + 3].prefix]} {target.TPlayer.Loadouts[il].Armor[ii + 3].Name} ]|[ {prefixlist[target.TPlayer.Loadouts[il].Armor[i + 3].prefix]} {target.TPlayer.Loadouts[il].Armor[i + 3].Name}]|====|[ {target.TPlayer.Loadouts[il].Dye[i].Name}]|[ {target.TPlayer.Loadouts[il].Armor[ii].Name} ]|[ {target.TPlayer.Loadouts[il].Armor[i].Name} ]|\n";
                            }
                            if (i >= 6 && i <= 9)
                            {
                                loadoutget += $"|[ {target.TPlayer.Loadouts[il].Dye[i].Name} ]|[ {prefixlist[target.TPlayer.Loadouts[il].Armor[ii].prefix]} {target.TPlayer.Loadouts[il].Armor[ii].Name} ]|[ {prefixlist[target.TPlayer.Loadouts[il].Armor[i].prefix]} {target.TPlayer.Loadouts[il].Armor[i].Name}]|\n";
                            }
                        }
                        switch (il)
                        {
                            case 0:
                                loadout1 = loadoutget;
                                break;
                            case 1:
                                loadout2 = loadoutget;
                                break;
                            case 2:
                                loadout3 = loadoutget;
                                break;
                        }
                    }
                    switch (target.TPlayer.CurrentLoadoutIndex)
                    {
                        case 0:
                            loadout1 = loadoutused;
                            break;
                        case 1:
                            loadout2 = loadoutused;
                            break;
                        case 2:
                            loadout3 = loadoutused;
                            break;
                    }
                    Equipment = $"{loadout1}\n{loadout2}\n{loadout3}\nmisc:\n{misclist}";
                }
                #endregion


                #region  PiggyBank
                if (type == "piggybank" || type == "all")
                {
                    int d2 = 10;
                    for (int i = 0; i < target.TPlayer.bank.item.Length; i++)
                    {

                        string p = "";
                        if (target.TPlayer.bank.item[i].prefix != 0) p = $"{prefixlist[target.TPlayer.bank.item[i].prefix]}";

                        string s = $"({target.TPlayer.bank.item[i].stack}) ";
                        if (target.TPlayer.bank.item[i].stack == 1) s = "";

                        if (i == d2)
                        {
                            PiggyBank += $"\n|[{p} {target.TPlayer.bank.item[i].Name} {s}]|";
                            d2 += 10;
                        }
                        else
                        {
                            PiggyBank += $"[{p}{target.TPlayer.bank.item[i].Name} {s}]|";
                        }
                    }
                }
                #endregion


                #region Safe
                if (type == "safe" || type == "all")
                {
                    int d2 = 10;
                    for (int i = 0; i < target.TPlayer.bank2.item.Length; i++)
                    {

                        string p = "";
                        if (target.TPlayer.bank2.item[i].prefix != 0) p = $"{prefixlist[target.TPlayer.bank2.item[i].prefix]}";

                        string s = $"({target.TPlayer.bank2.item[i].stack}) ";
                        if (target.TPlayer.bank2.item[i].stack == 1) s = "";

                        if (i == d2)
                        {
                            Safe += $"\n|[{p} {target.TPlayer.bank2.item[i].Name} {s}]|";
                            d2 += 10;
                        }
                        else
                        {
                            Safe += $"[{p}{target.TPlayer.bank2.item[i].Name} {s}]|";
                        }
                    }
                }
                #endregion


                #region Defender Forge
                if (type == "defenderforge" || type == "all")
                {
                    int d2 = 10;
                    for (int i = 0; i < target.TPlayer.bank3.item.Length; i++)
                    {

                        string p = "";
                        if (target.TPlayer.bank3.item[i].prefix != 0) p = $"{prefixlist[target.TPlayer.bank3.item[i].prefix]}";

                        string s = $"({target.TPlayer.bank3.item[i].stack}) ";
                        if (target.TPlayer.bank3.item[i].stack == 1) s = "";

                        if (i == d2)
                        {
                            DefenderForge += $"\n|[{p} {target.TPlayer.bank3.item[i].Name} {s}]|";
                            d2 += 10;
                        }
                        else
                        {
                            DefenderForge += $"[{p}{target.TPlayer.bank3.item[i].Name} {s}]|";
                        }
                    }
                }
                #endregion


                #region Void Vault
                if (type == "voidvault" || type == "all")
                {
                    int d2 = 10;
                    for (int i = 0; i < target.TPlayer.bank4.item.Length; i++)
                    {

                        string p = "";
                        if (target.TPlayer.bank4.item[i].prefix != 0) p = $"{prefixlist[target.TPlayer.bank4.item[i].prefix]}";

                        string s = $"({target.TPlayer.bank4.item[i].stack}) ";
                        if (target.TPlayer.bank4.item[i].stack == 1) s = "";

                        if (i == d2)
                        {
                            VoidVault += $"\n|[{p} {target.TPlayer.bank4.item[i].Name} {s}]|";
                            d2 += 10;
                        }
                        else
                        {
                            VoidVault += $"[{p}{target.TPlayer.bank4.item[i].Name} {s}]|";
                        }
                    }
                }
                #endregion
            }

        }

    }
    #endregion
}
