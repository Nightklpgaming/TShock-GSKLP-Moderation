﻿//Microsoft
using Google.Protobuf.WellKnownTypes;
using IL.Terraria.DataStructures;
using IL.Terraria.Graphics;
using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework;
using MKLP.Modules;
using Org.BouncyCastle.Asn1.X509;
using System;




//System
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO.Streams;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using System.Threading.Channels;



//Terraria
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
//TShock
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;
using static System.Net.Mime.MediaTypeNames;

namespace MKLP
{
    [ApiVersion(2, 1)]

    public class MKLP : TerrariaPlugin
    {

        #region [ Plugin Info ]
        public override string Author => "Nightklp";
        public override string Description => "a plugin that allows users to form clans";
        public override string Name => "MKLP";
        public override Version Version => new Version(1, 0, 0);
        #endregion

        #region [ Variables ]

        public static Config Config = Config.Read(); //CONFIG

        public static MKLP_DB DBManager = new MKLP_DB(new SqliteConnection(("Data Source=" + Path.Combine(TShock.SavePath, "MKLP.sqlite"))));

        public static DiscordKLP Discordklp = new();

        public static List<string> DisabledKey = new();

        public static AccountDLinked LinkAccountManager = new();

        public static Dictionary<int, string> IllegalItemProgression = new();

        public static Dictionary<short, string> IllegalProjectileProgression = new();

        public static Dictionary<SurvivalManager.MKLP_Tile, string> IllegalTileProgression = new();

        #endregion

        public MKLP(Main game) : base(game)
        {
            //amogus
        }

        #region [ Initialize ]
        public override void Initialize()
        {
            //=====================Player===================
            GetDataHandlers.PlayerUpdate += OnPlayerUpdate;

            ServerApi.Hooks.ServerJoin.Register(this, OnPlayerJoin);

            PlayerHooks.PlayerCommand += OnPlayerCommand;

            //=====================game=====================
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);

            GetDataHandlers.TileEdit += OnTileEdit;

            GetDataHandlers.NewProjectile += OnNewProjectile;

            GetDataHandlers.HealOtherPlayer += OnHealOtherPlayer;

            ServerApi.Hooks.NpcSpawn.Register(this, OnNPCSpawn);

            ServerApi.Hooks.NpcKilled.Register(this, OnNPCKilled);

            //GetDataHandlers.get

            //=====================Server===================
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);

            GeneralHooks.ReloadEvent += OnReload;

            #region [ Commands Initialize ]

            #region { default }
            /*
            Commands.ChatCommands.Add(new Command(Config.Permissions.Default_CMD_Ping, CMD_ping, "ping")
            {
                HelpText = "Ping"
            });
            */
            Commands.ChatCommands.Add(new Command(Config.Permissions.Default_CMD_Progression, CMD_BE, "progression", "prog")
            {
                HelpText = "displays defeated bosses and events"
            });

            Commands.ChatCommands.Add(new Command(Config.Permissions.Default_CMD_Report, CMD_Report, "report")
            {
                HelpText = "Report any suspicious activity by doing /report <message>",
                AllowServer = false
            });

            #endregion

            #region { Staff }

            Commands.ChatCommands.Add(new Command(Config.Permissions.Staff, CMD_StaffChat, "staffchat", "staff", "#")
            {
                HelpText = "Sends a message in staff chat"
            });

            #endregion

            #region { Admin }

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_MapPingTP, CMD_MapPingTP, "tpmap", "pingmap", "maptp")
            {
                AllowServer = false,
                HelpText = "Able to TP anywhere using map ping"
            });

            #endregion

            #region { moderation }

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_Disable, CMD_disable, "disable")
            {
                HelpText = "Acts as Ban but prevents players from doing anything \nwarning: disable's are temporary!"
            });

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_Disable, CMD_undisable, "enable", "undisable")
            {
                HelpText = "enable's a player that got disabled"
            });

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_UnBan, CMD_UnBan, "unban")
            {
                HelpText = "Removes ban tickets"
            });
            if ((bool)Config.Main.Replace_Ban_TShockCommand)
            {
                Command VarCMD_Ban = new(Config.Permissions.CMD_Ban, CMD_Ban, "ban") { HelpText = "Bans a player" };
                Commands.ChatCommands.RemoveAll(cmd => cmd.Names.Exists(alias => VarCMD_Ban.Names.Contains(alias)));
                Commands.ChatCommands.Add(VarCMD_Ban);
            } else
            {
                Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_Ban, CMD_Ban, "qban")
                {
                    HelpText = "Bans a player"
                });
            }

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_UnMute, CMD_UnMute, "unmute")
            {
                HelpText = "Unmutes Player"
            });
            if ((bool)Config.Main.Replace_Mute_TShockCommand)
            {
                Command VarCMD_Mute = new(Config.Permissions.CMD_Mute, CMD_Mute, "mute") { HelpText = "Mutes Player" };
                Commands.ChatCommands.RemoveAll(cmd => cmd.Names.Exists(alias => VarCMD_Mute.Names.Contains(alias)));
                Commands.ChatCommands.Add(VarCMD_Mute);
            } else
            {
                Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_Mute, CMD_Mute, "qmute")
                {
                    HelpText = "Mutes Player"
                });
            }
            #endregion

            #region { Inspect }

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_InventoryView, InventoryManager.InventoryView, "inventoryview", "invview", "inview")
            {
                HelpText = "View's inventory of a player"
            });

            #endregion

            #endregion

            if (Config.Discord.BotToken != "NONE")
            {
                Discordklp.Initialize();
            } else
            {
                MKLP_Console.SendLog_Message_DiscordBot("Discord bot token has not been set!", " {Setup}");
            }
            
        }

        #endregion

        #region [ Dispose ]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //=====================Player===================
                GetDataHandlers.PlayerUpdate -= OnPlayerUpdate;

                ServerApi.Hooks.ServerJoin.Deregister(this, OnPlayerJoin);

                PlayerHooks.PlayerCommand -= OnPlayerCommand;

                //=====================game=====================
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);

                GetDataHandlers.TileEdit -= OnTileEdit;

                GetDataHandlers.NewProjectile -= OnNewProjectile;

                GetDataHandlers.HealOtherPlayer -= OnHealOtherPlayer;

                ServerApi.Hooks.NpcSpawn.Deregister(this, OnNPCSpawn);

                ServerApi.Hooks.NpcKilled.Deregister(this, OnNPCKilled);

                //=====================Server===================
                ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);

                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region [ Events ]

        #region =={[ On Get Data ]}==

        private void OnGetData(GetDataEventArgs args)
        {
            if (args.Handled)
                return;

            var player = TShock.Players[args.Msg.whoAmI];

            if (DisabledKey.Contains(Identifier.Name + player.Name) ||
                DisabledKey.Contains(Identifier.IP + player.IP) ||
                DisabledKey.Contains(Identifier.UUID + player.UUID))
            {
                if (player == null || !player.Active || player.Dead)
                    return;

                if (args.MsgID == PacketTypes.PlayerSlot ||
                    args.MsgID == PacketTypes.PlayerUpdate ||
                    args.MsgID == PacketTypes.ItemOwner ||
                    args.MsgID == PacketTypes.ClientSyncedInventory)
                    return;

                // Prevent the packet from being processed
                args.Handled = true;
                return;
            }

            using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length - 1))
            {
                if (args.MsgID == PacketTypes.SpawnBossorInvasion)
                {
                    try
                    {
                        args.Handled = HandleSpawnBoss(new GetDataHandlerArgs(player, data));
                    }
                    catch (Exception ex)
                    {
                        TShock.Log.Error(ex.ToString());
                        MKLP_Console.SendLog_Exception(ex.ToString());
                        return;
                    }
                }
            }

            #region { Ping Map }
            using (var stream = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
            {
                if (args.MsgID == PacketTypes.LoadNetModule)
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        var id = reader.ReadUInt16();
                        var module = Terraria.Net.NetManager.Instance._modules[id];
                        if (module.GetType() == typeof(Terraria.GameContent.NetModules.NetPingModule))
                        {
                            var position = reader.ReadVector2();

                            if (player.ContainsData("MKLP-Map_Ping_TP"))
                            {
                                if (player.GetData<bool>("MKLP-Map_Ping_TP"))
                                {
                                    player.Teleport(position.X * 16, position.Y * 16);
                                }
                                return;
                            }
                        }
                    }
                }
            }
            #endregion

        }

        #endregion

        #region { Player }
        private static void OnPlayerUpdate(object? sender, GetDataHandlers.PlayerUpdateEventArgs args)
        {

        }

        private void OnPlayerJoin(JoinEventArgs args)
        {
            #region code

            IllegalItemProgression = SurvivalManager.GetIllegalItem();

            IllegalProjectileProgression = SurvivalManager.GetIllegalProjectile();

            IllegalTileProgression = SurvivalManager.GetIllegalTile();

            var player = TShock.Players[args.Who];
            if (player != null)
            {
                #region Prevent
                if (Config.Main.IllegalNames.Contains(player.Name))
                {
                    player.Disconnect("Illegal Name");
                    return;
                }
                if (player.Name.Contains(DiscordKLP.S_))
                {
                    player.Disconnect($"You Can't use {DiscordKLP.S_} in your Name!");
                    return;
                }
                if (!HasSymbols(player.Name) && !(bool)Config.Main.Allow_PlayerName_Symbols)
                {
                    player.Disconnect("Your name contains Symbols and is not allowed on this server.");
                    return;
                }
                if (IsIllegalName(player.Name) && !(bool)Config.Main.Allow_PlayerName_InappropriateWords)
                {
                    player.Disconnect("Your name contains inappropriate language and is not allowed on this server.");
                    return;
                }
                #endregion

                foreach (string check in DisabledKey)
                {
                    if (check == Identifier.Name + player.Name ||
                        check == Identifier.IP + player.IP ||
                        check == Identifier.UUID + player.UUID)
                    {
                        player.SendErrorMessage("Your still disabled");
                    }
                }

                if (DBManager.CheckPlayer(player))
                {
                    player.SendErrorMessage("You're still muted!");
                }
            }

            bool HasSymbols(string Name)
            {
                return Regex.IsMatch(Name, @"^[A-Za-z0-9\s@]*$");
            }

            bool IsIllegalName(string Name)
            {
                Regex[] list =
                {
                    new Regex("fuck", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline),
                    new Regex("shit", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline),
                    new Regex("bitch", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline),
                    new Regex("nigga", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline),
                    new Regex("nigger", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline),
                    new Regex("cum", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline),
                    new Regex("dick", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline)
                };

                foreach (Regex check in list)
                {
                    if (check.IsMatch(Name))
                    {
                        return true;
                    }
                }
                return false;
            }
            #endregion
        }

        private void OnPlayerCommand(PlayerCommandEventArgs args)
        {
            if (args.Handled || args.Player == null)
                return;
            if (!args.Player.RealPlayer)
                return;

            Command command = args.CommandList.FirstOrDefault();

            if (command == null)
                return;

            if (DisabledKey.Contains(Identifier.Name + args.Player.Name) ||
                DisabledKey.Contains(Identifier.IP + args.Player.IP) ||
                DisabledKey.Contains(Identifier.UUID + args.Player.UUID))
            {
                if (command.Name == "register" ||
                    command.Name == "login" ||
                    command.Name == "guesttalk")
                {
                    args.Player.SendErrorMessage("You're currently Disabled!" +
                    "\n you cannot perform this command");
                    args.Handled = true;
                    return;
                }
            }

            if (command.Name == "register" ||
                command.Name == "login" ||
                command.Name == "password")
                return;

            if (Config.Discord.CommandLogChannel == null) return;
            if (command.CanRun(args.Player))
            {
                Discordklp.KLPBotSendMessageLog((ulong)Config.Discord.CommandLogChannel, $"Player **{args.Player.Account.Name}** ✅Executed" +
                    $" `/{command.Name}{((string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count) == "") ? "" : " ")}{string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count)}`");
            } else
            {
                Discordklp.KLPBotSendMessageLog((ulong)Config.Discord.CommandLogChannel, $"Player **{args.Player.Account.Name}** ⛔Tried" +
                    $" `/{command.Name}{((string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count) == "") ? "" : " ")}{string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count)}`");
            }
            
        }

        
        #endregion

        #region { Game }
        private void OnUpdate(EventArgs e)
        {
            #region code

            foreach (TSPlayer player in TShock.Players)
            {

                if (player == null || !player.Active) continue;

                if (!player.IsLoggedIn)
                {
                    continue;
                }

                if (DisabledKey.Contains(Identifier.Name + player.Name) ||
                    DisabledKey.Contains(Identifier.IP + player.IP) ||
                    DisabledKey.Contains(Identifier.UUID + player.UUID))
                {
                    if (TShockAPI.Utils.Distance(value2: new Vector2((int)player.TPlayer.position.X / 16, (int)player.TPlayer.position.Y / 16), value1: new Vector2(Main.spawnTileX, Main.spawnTileY)) >= 3f)
                    {
                        player.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);
                        
                    }
                    player.SetBuff(149, 330, true);
                    continue;
                }

                #region ( Inventory Checking )

                if ((!player.HasPermission("tshock.item") || !player.HasPermission("tshock.item.*")) && !player.HasPermission(Config.Permissions.IgnoreSurvivalCode_1)) ManagePlayer.CheckIllegalItemInventory(player);

                if (player.ContainsData("MKLP_PrevInventory"))
                {

                    Item[] prevplayer = player.GetData<Item[]>("MKLP_PrevInventory");

                    ManagePlayer.CheckPreviousInventory(player, player.TPlayer.inventory, prevplayer);

                    player.SetData("MKLP_PrevInventory", player.TPlayer.inventory.Clone());
                }
                else
                {
                    player.SetData("MKLP_PrevInventory", player.TPlayer.inventory.Clone());
                }

                #endregion

            }
            #endregion
        }
        
        private void OnTileEdit(object? sender, GetDataHandlers.TileEditEventArgs args)
        {
            #region code
            int tileX = args.X;
            int tileY = args.Y;


            if (KillThreshold()) return;
            if (PlaceThreshold()) return;
            if (PaintThreshold()) return;
            if (LiquidThreshold()) return;

            if (args.Action == GetDataHandlers.EditAction.PlaceTile ||
                args.Action == GetDataHandlers.EditAction.ReplaceTile)
            {
                if (IllegalTileProgression.ContainsKey(new(Main.tile[tileX, tileY].type, args.Style)) && args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_3))
                {
                    ManagePlayer.DisablePlayer(args.Player, $"{IllegalTileProgression[new(Main.tile[tileX, tileY].type, args.Style)]} Block Place", ServerReason: $"Survival,code,3|{Main.tile[tileX, tileY].type}|{args.Style}|{IllegalTileProgression[new(Main.tile[tileX, tileY].type, args.Style)]}");
                    args.Player.SendTileSquareCentered(tileX, tileY, 4);
                    args.Handled = true;
                    return;
                }
                if (IllegalTileProgression.ContainsKey(new(Main.tile[tileX, tileY].type, 0, true)) && args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_3))
                {
                    ManagePlayer.DisablePlayer(args.Player, $"{IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]} Block Place", ServerReason: $"Survival,code,3|{Main.tile[tileX, tileY].type}|{args.Style}|{IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]}");
                    args.Player.SendTileSquareCentered(tileX, tileY, 4);
                    args.Handled = true;
                    return;
                }
            }
            


            if (args.Action == GetDataHandlers.EditAction.KillTile ||
                args.Action == GetDataHandlers.EditAction.KillWall ||
                args.Action == GetDataHandlers.EditAction.TryKillTile)
            {
                if (tileY < (int)Main.worldSurface)
                {
                    ushort[] whitelist =
                    {
                        TileID.Plants,
                        TileID.Plants2,
                        TileID.AshPlants,
                        TileID.CorruptPlants,
                        TileID.CrimsonPlants,
                        TileID.Pots,
                        TileID.Vines,
                        TileID.CorruptVines,
                        TileID.CrimsonVines,
                        TileID.HallowedVines,
                        TileID.JungleVines,
                        TileID.CorruptPlants,
                        TileID.CrimsonPlants,
                        TileID.HallowedPlants,
                        TileID.HallowedPlants2,
                        TileID.JunglePlants,
                        TileID.JunglePlants2,

                        TileID.Trees
                    };
                    if (args.Action == GetDataHandlers.EditAction.KillTile &&
                        whitelist.Contains(Main.tile[tileX, tileY].type)) return;


                    if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_protectsurface_break) && (bool)Config.Main.Using_AntiGrief_Surface_Break)
                    {
                        args.Player.SendErrorMessage(Config.Main.Message_AntiGrief_Surface_Break);
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return;
                    }
                }

            }

            if (args.Action == GetDataHandlers.EditAction.PlaceTile ||
                args.Action == GetDataHandlers.EditAction.PlaceWall ||
                args.Action == GetDataHandlers.EditAction.ReplaceTile ||
                args.Action == GetDataHandlers.EditAction.ReplaceWall)
            {
                if (tileY < (int)Main.worldSurface)
                {
                    ushort[] whitelist =
                    {
                        TileID.WorkBenches,
                        TileID.Furnaces,
                        TileID.Anvils
                    };
                    if (args.Action == GetDataHandlers.EditAction.PlaceTile &&
                        whitelist.Contains(Main.tile[tileX, tileY].type)) return;
                    

                    if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_protectsurface_place) && (bool)Config.Main.Using_AntiGrief_Surface_Place)
                    {
                        args.Player.SendErrorMessage(Config.Main.Message_AntiGrief_Surface_Place);
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return;
                    }
                }
            }

            /*
            if ()
            {
                if (tileY < (int)Main.worldSurface)
                {
                    ushort[] whitelist =
                    {
                        LiquidID.Water
                    };
                    if (args.Action == GetDataHandlers.EditAction.PlaceTile &&
                        whitelist.Contains(Main.tile[tileX, tileY].type)) return;


                    if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_protectsurface_placeliquid) && (bool)Config.Main.Message_AntiGrief_Surface_PlaceLiquid)
                    {
                        args.Player.SendErrorMessage(Config.Main.Message_AntiGrief_Surface_PlaceLiquid);
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return;
                    }
                }
            }
            */

            if (!NPC.downedBoss3)
            {
                if (args.Action == GetDataHandlers.EditAction.KillActuator ||
                    args.Action == GetDataHandlers.EditAction.PlaceActuator ||
                    args.Action == GetDataHandlers.EditAction.KillWire ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire ||
                    args.Action == GetDataHandlers.EditAction.KillWire2 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire2 ||
                    args.Action == GetDataHandlers.EditAction.KillWire3 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire3 ||
                    args.Action == GetDataHandlers.EditAction.KillWire4 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire4 &&
                    (
                    !args.Player.HasPermission(Config.Permissions.Ignore_IllegalWireProgression) &&
                    !args.Player.HasPermission("tshock.item") &&
                    !args.Player.HasPermission("tshock.item.*")
                    ) && (bool)Config.Main.Prevent_IllegalWire_Progression
                    )
                {
                    Discordklp.KLPBotSendMessage_Warning($"Player **{args.Player.Name}** was Able to use Wire/Actuator on pre skeletron! `{tileX}, {tileY}`", args.Player.Account.Name, "Illegal Wire/Actuator Progression");
                    args.Player.SendErrorMessage("This is Illegal on this progression!");
                    args.Player.SendTileSquareCentered(tileX, tileY, 4);
                    args.Handled = true;
                    return;
                }
            }

            if (args.Action == GetDataHandlers.EditAction.KillActuator ||
                    args.Action == GetDataHandlers.EditAction.PlaceActuator ||
                    args.Action == GetDataHandlers.EditAction.KillWire ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire ||
                    args.Action == GetDataHandlers.EditAction.KillWire2 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire2 ||
                    args.Action == GetDataHandlers.EditAction.KillWire3 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire3 ||
                    args.Action == GetDataHandlers.EditAction.KillWire4 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire4 &&
                    ( tileY <= Main.worldSurface ) && (bool)Config.Main.Prevent_IllegalWire_Progression
                    )
            {
                Discordklp.KLPBotSendMessageMainLog($"Player **{args.Player.Name}** Used wire below surface `{tileX}, {tileY}`");
                
            }

            #region ( Threshold )
            bool KillThreshold()
            {
                if (!(bool)Config.Main.Using_Default_Code1) return false;
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_1)) return false;

                int max = (int)Config.Main.default_code1_maxdefault;

                int[] boost =
                {
                    ItemID.HandOfCreation,
                    ItemID.ArchitectGizmoPack,
                    ItemID.BrickLayer,
                    ItemID.PortableCementMixer,
                    ItemID.AncientChisel,
                    ItemID.MiningPotion
                };
                foreach (Item check in args.Player.TPlayer.armor)
                {
                    if (boost.Contains(check.netID))
                    {
                        max = (int)Config.Main.default_code1_maxboost;
                        break;
                    }
                }

                int[] bomb =
                {
                    ItemID.Bomb,
                    ItemID.StickyBomb,
                    ItemID.BouncyBomb,
                    ItemID.BombFish
                };
                foreach (Item check in args.Player.Inventory)
                {
                    if (bomb.Contains(check.netID))
                    {
                        max = (int)Config.Main.default_code1_maxbomb;
                        break;
                    }
                }
                int[] dynamite =
                {
                    ItemID.Dynamite,
                    ItemID.StickyDynamite,
                    ItemID.BouncyDynamite
                };
                foreach (Item check in args.Player.Inventory)
                {
                    if (dynamite.Contains(check.netID))
                    {
                        max = (int)Config.Main.default_code1_maxdynamite;
                        break;
                    }
                }

                if (args.Player.TileKillThreshold >= max)
                {
                    ManagePlayer.DisablePlayer(args.Player, $"Breaking blocks to fast", ServerReason: $"Default,code,1|{args.Player.SelectedItem.netID}");
                    args.Player.SendTileSquareCentered(tileX, tileY, 4);
                    args.Handled = true;
                    return true;
                }
                return false;
            }

            bool PlaceThreshold()
            {
                if (!(bool)Config.Main.Using_Default_Code2) return false;
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_2)) return false;

                int max = (int)Config.Main.default_code2_maxdefault;


                int[] boost =
                {
                    ItemID.HandOfCreation,
                    ItemID.ArchitectGizmoPack,
                    ItemID.BrickLayer,
                    ItemID.PortableCementMixer
                };
                foreach (Item check in args.Player.TPlayer.armor)
                {
                    if (boost.Contains(check.netID))
                    {
                        max = (int)Config.Main.default_code2_maxboost;
                        break;
                    }
                }

                int[] bomb =
                {
                    ItemID.DirtBomb,
                    ItemID.DirtStickyBomb
                };
                foreach (Item check in args.Player.Inventory)
                {
                    if (bomb.Contains(check.netID))
                    {
                        max = (int)Config.Main.default_code2_maxbomb;
                        break;
                    }
                }


                if (args.Player.TilePlaceThreshold >= max)
                {
                    ManagePlayer.DisablePlayer(args.Player, $"Placing blocks too fast", ServerReason: $"Default,code,2|{args.Player.SelectedItem.netID}");
                    args.Player.SendTileSquareCentered(tileX, tileY, 4);
                    args.Handled = true;
                    return true;
                }

                return false;
            }

            bool PaintThreshold()
            {
                if (!(bool)Config.Main.Using_Default_Code3) return false;
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_3)) return false;

                int max = (int)Config.Main.default_code3_maxdefault;

                int[] boost =
                {
                    ItemID.HandOfCreation,
                    ItemID.ArchitectGizmoPack,
                    ItemID.BrickLayer,
                    ItemID.PortableCementMixer
                };
                foreach (Item check in args.Player.TPlayer.armor)
                {
                    if (boost.Contains(check.netID))
                    {
                        max = (int)Config.Main.default_code3_maxboost;
                        break;
                    }
                }

                if (args.Player.PaintThreshold >= max)
                {
                    ManagePlayer.DisablePlayer(args.Player, $"Painting too fast", ServerReason: $"Default,code,3|{args.Player.SelectedItem.netID}");
                    args.Player.SendTileSquareCentered(tileX, tileY, 4);
                    args.Handled = true;
                    return true;
                }

                return false;
            }

            bool LiquidThreshold()
            {
                if (!(bool)Config.Main.Using_Default_Code4) return false;
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_4)) return false;

                int max = (int)Config.Main.default_code4_maxdefault;

                int[] boost =
                {
                    ItemID.HandOfCreation,
                    ItemID.ArchitectGizmoPack,
                    ItemID.BrickLayer,
                    ItemID.PortableCementMixer
                };
                foreach (Item check in args.Player.TPlayer.armor)
                {
                    if (boost.Contains(check.netID))
                    {
                        max = (int)Config.Main.default_code4_maxboost;
                        break;
                    }
                }

                int[] bomb =
                {
                    ItemID.WetBomb,
                    ItemID.LavaBomb,
                    ItemID.HoneyBomb
                };
                foreach (Item check in args.Player.Inventory)
                {
                    if (bomb.Contains(check.netID))
                    {
                        max = (int)Config.Main.default_code4_maxbomb;
                        break;
                    }
                }

                if (args.Player.TileLiquidThreshold >= max)
                {
                    ManagePlayer.DisablePlayer(args.Player, $"Exceeded Liquid place", ServerReason: $"Default,code,4|{args.Player.SelectedItem.netID}");
                    args.Player.SendTileSquareCentered(tileX, tileY, 4);
                    args.Handled = true;
                    return true;
                }

                return false;
            }

            #endregion

            #region [ Breakable Tiles ]

            ushort[] breakableTiles =
            {
                TileID.Grass,
            };

            #endregion

            #endregion
        }

        private void OnNewProjectile(object sender, GetDataHandlers.NewProjectileEventArgs args)
        {
            #region code
            short ident = args.Identity;
            Vector2 pos = args.Position;
            Vector2 vel = args.Velocity;
            float knockback = args.Knockback;
            short damage = args.Damage;
            byte owner = args.Owner;
            short type = args.Type;
            int index = args.Index;
            float[] ai = args.Ai;

            if (ProjectileThreshold()) return;

            Dictionary<short, string> GetIllegalProj = SurvivalManager.GetIllegalProjectile();

            
            if (args.Player.IsLoggedIn && IllegalProjectileProgression.ContainsKey(type) && !args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_2) && (bool)Config.Main.Using_Survival_Code2)
            {
                ManagePlayer.DisablePlayer(args.Player, $"{GetIllegalProj[type]} Projectile", ServerReason: $"Survival,code,2|{type}|{GetIllegalProj[type]}");
                args.Player.RemoveProjectile(ident, owner);
                args.Handled = true;
                return;
            }
            

            short[] explosives =
            {
                //reg bomb
                ProjectileID.Bomb,
                ProjectileID.StickyBomb,
                ProjectileID.BouncyBomb,

                //reg dynamite
                ProjectileID.Dynamite,
                ProjectileID.StickyDynamite,
                ProjectileID.BouncyDynamite,

                //others
                ProjectileID.BombFish,
                ProjectileID.LavaBomb,
                ProjectileID.WetBomb,
                ProjectileID.HoneyBomb,

                //rocket
                ProjectileID.RocketII,
                ProjectileID.RocketSnowmanII,
                ProjectileID.RocketIV,
                ProjectileID.RocketSnowmanIV,

                ProjectileID.ClusterFragmentsII,
                ProjectileID.ClusterGrenadeII,
                ProjectileID.ClusterMineII,
                ProjectileID.ClusterRocketII,
                ProjectileID.ClusterSnowmanFragmentsII,
                ProjectileID.ClusterSnowmanRocketII,
                ProjectileID.MiniNukeGrenadeII,
                ProjectileID.MiniNukeMineII,
                ProjectileID.MiniNukeRocketII,
                ProjectileID.MiniNukeSnowmanRocketII,
                ProjectileID.LavaGrenade,
                ProjectileID.LavaMine,
                ProjectileID.LavaRocket,
                ProjectileID.LavaSnowmanRocket,

                //celebratiomk
                ProjectileID.Celeb2RocketExplosive,
                ProjectileID.Celeb2RocketExplosiveLarge,
                ProjectileID.Celeb2RocketLarge
            };

            if (explosives.Contains(type))
            {
                if (args.Player.TileY >= (int)Main.worldSurface) return;

                if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_protectsurface_explosive) && (bool)Config.Main.Using_AntiGrief_Surface_Explosive)
                {
                    args.Player.SendErrorMessage(Config.Main.Message_AntiGrief_Surface_Explosive);
                    args.Player.RemoveProjectile(ident, owner);
                    args.Handled = true;
                    return;
                }
            }


            bool ProjectileThreshold()
            {
                if (!(bool)Config.Main.Using_Default_Code5) return false;
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_5)) return false;

                int max = (int)Config.Main.default_code5_maxdefault;

                if (Main.hardMode) max = (int)Config.Main.default_code5_maxHM;

                if (args.Player.ProjectileThreshold >= max)
                {
                    ManagePlayer.DisablePlayer(args.Player, $"Spawning too many projectiles at onces!", ServerReason: $"Default,code,5|{args.Player.SelectedItem.netID}|{type}");
                    args.Player.RemoveProjectile(ident, owner);
                    args.Handled = true;
                    return true;
                }

                return false;
            }

            
            #endregion
        }

        private void OnHealOtherPlayer(object sender, GetDataHandlers.HealOtherPlayerEventArgs args)
        {
            #region code
            if (HealOtherThreshold()) return;

            bool HealOtherThreshold()
            {
                if (!(bool)Config.Main.Using_Default_Code6) return false;
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_6)) return false;

                int max = (int)Config.Main.default_code6_maxdefault;

                if (NPC.downedPlantBoss) max = (int)Config.Main.default_code6_maxPlant;

                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null) continue;

                    bool head = false; bool chestplate = false; bool leggings = false;
                    foreach (Item check in player.TPlayer.armor)
                    {
                        if (check.netID == ItemID.SpectreHood) head = true;
                        if (check.netID == ItemID.SpectreRobe) chestplate = true;
                        if (check.netID == ItemID.SpectrePants) leggings = true;

                    }
                    if (head && chestplate && leggings)
                    {
                        max += (int)Config.Main.default_code6_addmax_spectrehood;
                    }
                }

                if (args.Player.HealOtherThreshold >= max)
                {
                    ManagePlayer.DisablePlayer(args.Player, $"Healing others to fast!", ServerReason: $"Default,code,6|{args.Player.SelectedItem.netID}");
                    args.Handled = true;
                    return true;
                }

                return false;
            }
            #endregion
        }

        private void OnNPCSpawn(NpcSpawnEventArgs args)
        {
            #region code
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                
                if ((bool)Config.BossManager.PreventIllegalBoss)
                {
                    if (!Main.hardMode && (
                        npc.type == NPCID.QueenSlimeBoss ||
                        npc.type == NPCID.TheDestroyer ||
                        npc.type == NPCID.Retinazer ||
                        npc.type == NPCID.Spazmatism ||
                        npc.type == NPCID.SkeletronPrime ||
                        npc.type == NPCID.DukeFishron))
                    {
                        Main.npc[i].active = false;
                        Main.npc[i].type = 0;
                        TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                    }

                    if (!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3 && (npc.type == NPCID.Plantera))
                    {
                        Main.npc[i].active = false;
                        Main.npc[i].type = 0;
                        TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                    }
                    if (!NPC.downedPlantBoss && (npc.type == NPCID.HallowBoss || npc.type == NPCID.EmpressButterfly || npc.type == NPCID.Golem))
                    {
                        Main.npc[i].active = false;
                        Main.npc[i].type = 0;
                        TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                    }
                    if (!NPC.downedGolemBoss && (npc.type == NPCID.CultistBoss || npc.type == NPCID.MoonLordCore))
                    {
                        Main.npc[i].active = false;
                        Main.npc[i].type = 0;
                        TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                    }
                }

                if (TShock.Utils.GetActivePlayerCount() < Config.BossManager.RequiredPlayersforBoss && (
                    !NPC.downedSlimeKing && npc.type == NPCID.KingSlime ||
                    !NPC.downedBoss1 && npc.type == NPCID.EyeofCthulhu ||
                    !NPC.downedBoss2 && !WorldGen.crimson && npc.type == NPCID.EaterofWorldsHead ||
                    !NPC.downedBoss2 && WorldGen.crimson && npc.type == NPCID.BrainofCthulhu ||
                    !NPC.downedDeerclops && npc.type == NPCID.Deerclops ||
                    !NPC.downedBoss3 && npc.type == NPCID.SkeletronHead ||
                    !NPC.downedQueenBee && npc.type == NPCID.QueenBee ||
                    !Main.hardMode && npc.type == NPCID.WallofFlesh ||
                    !NPC.downedQueenSlime && npc.type == NPCID.QueenSlimeBoss ||
                    !NPC.downedMechBoss1 && npc.type == NPCID.TheDestroyer ||
                    !NPC.downedMechBoss2 && npc.type == NPCID.Retinazer ||
                    !NPC.downedMechBoss2 && npc.type == NPCID.Spazmatism ||
                    !NPC.downedMechBoss3 && npc.type == NPCID.SkeletronPrime ||
                    !NPC.downedPlantBoss && npc.type == NPCID.Plantera ||
                    !NPC.downedGolemBoss && npc.type == NPCID.Golem ||
                    !NPC.downedFishron && npc.type == NPCID.DukeFishron ||
                    !NPC.downedMoonlord && npc.type == NPCID.MoonLordCore ||
                    !NPC.downedAncientCultist && npc.type == NPCID.CultistBoss ||
                    !NPC.downedEmpressOfLight && npc.type == NPCID.HallowBoss))
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", 0);
                }

                if (!(bool)Config.BossManager.AllowKingSlime && npc.type == NPCID.KingSlime) // King Slime
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowEyeOfCthulhu && npc.type == NPCID.EyeofCthulhu) // Eye of Cthulhu
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowEaterOfWorlds && npc.type == NPCID.EaterofWorldsHead) // Eater of Worlds
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowBrainOfCthulhu && npc.type == NPCID.BrainofCthulhu) // Brain of Cthulhu
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowQueenBee && npc.type == NPCID.QueenBee) // Queen Bee
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowSkeletron && npc.type == NPCID.SkeletronHead) // Skeletron
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowDeerclops && npc.type == NPCID.Deerclops) // Deerclops
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowWallOfFlesh && npc.type == NPCID.WallofFlesh) // Wall of Flesh
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowQueenSlime && npc.type == NPCID.QueenSlimeBoss) // Queen Slime
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowTheTwins && (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism)) // The Twins
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowTheDestroyer && npc.type == NPCID.TheDestroyer) // The Destroyer
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowSkeletronPrime && npc.type == NPCID.SkeletronPrime) // Skeletron Prime
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowPlantera && npc.type == NPCID.Plantera) // Plantera
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowGolem && npc.type == NPCID.Golem) // Golem
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowDukeFishron && npc.type == NPCID.DukeFishron) // Duke Fishron
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowEmpressOfLight && npc.type == NPCID.HallowBoss) // Empress of Light
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowLunaticCultist && npc.type == NPCID.CultistBoss) // Lunatic Cultist
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!(bool)Config.BossManager.AllowMoonLord && npc.type == NPCID.MoonLordCore) // Moon Lord
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }
            }
            #endregion
        }

        private void OnNPCKilled(NpcKilledEventArgs args)
        {
            #region code
            int[] BossIDs =
            {
                50, // King Slime
			    4, // Eye of Cthulu			
			    222, // Queen Bee
			    13, // Eater of Worlds	
			    266, // Brain of Cthulu
			    35, // Skeletron
			    668, // Deerclops
			    113, // Wall of Flesh
			    657, // Queen Slime
			    125, // Retinazer
			    127, // Skeletron Prime	
			    134, // The Destroyer
			    262, // Plantera
			    245, // Golem
			    636, // Empress Of Light
			    370, // Duke Fishron
			    439, // Lunatic Cultist
			    396 // Moon Lord
		    };

            if (BossIDs.Contains(args.npc.type))
            {
                IllegalItemProgression = SurvivalManager.GetIllegalItem();

                IllegalProjectileProgression = SurvivalManager.GetIllegalProjectile();

                IllegalTileProgression = SurvivalManager.GetIllegalTile();
            }
            

            #endregion
        }

        private static bool HandleSpawnBoss(GetDataHandlerArgs args)
        {
            #region code
            if (args.Player.IsBouncerThrottled())
            {
                return true;
            }

            var plr = args.Data.ReadInt16();
            var thingType = args.Data.ReadInt16();

            var isKnownBoss = thingType > 0 && thingType < Terraria.ID.NPCID.Count && NPCID.Sets.MPAllowedEnemies[thingType];

            NPC getnpc = new NPC();
            

            if (isKnownBoss)
            {
                getnpc.SetDefaults(thingType);
            }

            if (plr != args.Player.Index)
                return true;

            if (args.Player.HasPermission(Config.Permissions.IgnoreMainCode_2) || !(bool)Config.Main.Using_Main_Code2) return false;

            switch (thingType)
            {
                /*
                case -18:
                    thing = GetString("{0} applied traveling merchant's satchel!", args.Player.Name);
                    break;
                case -17:
                    thing = GetString("{0} applied advanced combat techniques volume 2!", args.Player.Name);
                    break;
                */
                case -16: // Mechdusa
                    {
                        if (args.Player.SelectedItem.type != 5334)
                        {
                            ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|Mechdusa");
                            return true;
                        }

                        return false;
                    }
                /*
                case -15:
                    thing = GetString("{0} has sent a request to the slime delivery service!", args.Player.Name);
                    break;
                case -14:
                    thing = GetString("{0} has sent a request to the bunny delivery service!", args.Player.Name);
                    break;
                case -13:
                    thing = GetString("{0} has sent a request to the dog delivery service!", args.Player.Name);
                    break;
                case -12:
                    thing = GetString("{0} has sent a request to the cat delivery service!", args.Player.Name);
                    break;
                case -11:
                    thing = GetString("{0} applied advanced combat techniques!", args.Player.Name);
                    break;
                */
                case -10: // Blood Moon
                    {
                        if (args.Player.SelectedItem.type != 4271)
                        {
                            ManagePlayer.DisablePlayer(args.Player, $"null item invasion spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|Blood Moon");
                            return true;
                        }

                        break;
                    }
                case -8: // Impending doom approaches... ( Moon Lord )
                    {
                        if (args.Player.SelectedItem.type != 3601)
                        {
                            ManagePlayer.DisablePlayer(args.Player, $"null item invasion spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|Impending doom approaches... (Moon Lord)");
                            return true;
                        }

                        break;
                    }
                case -7: // Martians
                    {
                        return true;
                    }
                case -6: // Solar Eclipse
                    {
                        if (args.Player.SelectedItem.type != 2767)
                        {
                            ManagePlayer.DisablePlayer(args.Player, $"null item invasion spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|Solar Eclipse");
                            return true;
                        }

                        return false;
                    }
                case -5: // Frost Moon
                    {
                        if (args.Player.SelectedItem.type != 1958)
                        {
                            ManagePlayer.DisablePlayer(args.Player, $"null item invasion spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|Frost Moon");
                            return true;
                        }

                        return false;
                    }
                case -4: //  Pumpkin Moon
                    {
                        if (args.Player.SelectedItem.type != 1844)
                        {
                            ManagePlayer.DisablePlayer(args.Player, $"null item invasion spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|Pumkin Moon");
                            return true;
                        }

                        return false;
                    }
                case -3: // Pirate Invasion
                    {
                        if (args.Player.SelectedItem.type != 1315)
                        {
                            ManagePlayer.DisablePlayer(args.Player, $"null item invasion spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|Pirate Invasion");
                            return true;
                        }

                        return false;
                    }
                case -2: // frost legion
                    {
                        if (args.Player.SelectedItem.type != 602)
                        {
                            ManagePlayer.DisablePlayer(args.Player, $"null item invasion spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|Frost Legion");
                            return true;
                        }

                        return false;
                    }
                case -1: // goblin army
                    {
                        if (args.Player.SelectedItem.type != 361)
                        {
                            ManagePlayer.DisablePlayer(args.Player, $"null item invasion spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|Goblin Army");
                            return true;
                        }

                        return false;
                    }
                
                default:
                    if (!isKnownBoss)
                        TShock.Log.ConsoleDebug("GetDataHandlers / HandleSpawnBoss unknown boss {0} summoned by {1}", thingType, args.Player.Name);
                    NPC npc = new NPC();
                    npc.SetDefaults(thingType);
                    
                    switch (npc.netID)
                    {
                        case 50: //king slime
                            {
                                if (args.Player.SelectedItem.type != 560)
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }

                                break;
                            }
                        case 4: // Eye Of Cthulhu
                            {
                                if (args.Player.SelectedItem.type != 43)
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }

                                break;
                            }
                        case 13: // Eater Of Worlds
                        case 14:
                        case 15:
                            {
                                if (args.Player.SelectedItem.type != 70)
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }

                                break;
                            }
                        case 266: // Brain Of Cthulhu
                            {
                                if (args.Player.SelectedItem.type != 1331)
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }

                                break;
                            }
                        case 222: // Queen Bee
                            {
                                if (args.Player.SelectedItem.type != 1133)
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }

                                break;
                            }
                        case 668: // Deerclops
                            {
                                if (args.Player.SelectedItem.type != 5120)
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }

                                break;
                            }
                        case 35: // Skeletron
                        case 36:
                            {
                                if (args.Player.Accessories.All(i => i.type == 1307))
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }

                                break;
                            }
                        case 113: // Wall of Flesh
                            {
                                ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                return true;
                            }
                        case 657: // Queen Slime
                            {
                                if (args.Player.SelectedItem.type != 4988)
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }

                                break;
                            }
                        case 125: // The Twins
                        case 126:
                            {
                                if (args.Player.SelectedItem.type != 1133)
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }

                                break;
                            }
                        case 134: // Destroyer
                        case 135:
                        case 136:
                            {
                                if (args.Player.SelectedItem.type != 556)
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }

                                break;
                            }
                        case 127: // Skeletron Prime
                            {
                                if (args.Player.SelectedItem.type != 557)
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }

                                break;
                            }
                        case 262: // Plantera
                            {
                                ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                return true;
                            }
                        case 245: // Golem
                        case 246:
                        case 247:
                        case 248:
                            {
                                if (!NPC.downedPlantBoss)
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }
                                if (!args.Player.Inventory.All(i => i.type == 1293) && !args.Player.TPlayer.bank4.item.All(i => i.type == 1293))
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }

                                break;
                            }
                        case 370: // Duke Fishron
                            {
                                if (!Main.hardMode)
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }
                                int[] fishing_rods =
                                {
                                    ItemID.WoodFishingPole,
                                    ItemID.ReinforcedFishingPole,
                                    ItemID.FisherofSouls,
                                    ItemID.Fleshcatcher,
                                    ItemID.ScarabFishingRod,
                                    4325, //chum caster
                                    ItemID.FiberglassFishingPole,
                                    ItemID.MechanicsRod, 
                                    ItemID.SittingDucksFishingRod, 
                                    ItemID.HotlineFishingHook, 
                                    ItemID.GoldenFishingRod
                                };
                                if (!args.Player.Inventory.All(i => fishing_rods.Contains(i.type)))
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }
                                if (!args.Player.Inventory.All(i => i.type == 2673))
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }

                                break;
                            }
                        case 636: // Empress Of Light
                            {
                                ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                return true;
                            }
                        case 440: // Lunatic Cultist
                            {
                                ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                return true;
                            }
                    }

                    break;
                    
            }

            return false;
            #endregion
        }

        #endregion

        #region { Server }

        private void OnReload(ReloadEventArgs args)
        {
            Config = Config.Read();
            args.Player.SendInfoMessage("MKLP config reloaded!");
        }

        #endregion

        #endregion


        #region [ Commands ]

        #region { Default }

        private void CMD_ping(CommandArgs args)
        {
            #region code
            string result = "";

            /*
            foreach (var PlayerPing in PlayersLatency)
            {
                result += $"{PlayerPing.Key} {PlayerPing.Value}ms\n";
            }
            */

            if (result == "") result = "No Players Online...";

            args.Player.SendMessage("List Of Players Latency:\n\n" +
                result, Color.Yellow);

            #endregion
        }

        private void CMD_BE(CommandArgs args)
        {
            #region { stringdefeatedbosses }
            string GetListDefeatedBoss()
            {
                CONFIG_BOSSES getenabledboss = Config.BossManager;
                Dictionary<string, bool> defeatedbosses = new();
                if ((bool)getenabledboss.AllowKingSlime)
                {
                    if (NPC.downedSlimeKing)
                    {
                        defeatedbosses.Add("[i:2493] King Slime", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:2493] King Slime", false);
                    }
                }
                else if (NPC.downedSlimeKing)
                {
                    defeatedbosses.Add("[i:2493] King Slime", true);
                }
                if ((bool)getenabledboss.AllowEyeOfCthulhu)
                {
                    if (NPC.downedBoss1)
                    {
                        defeatedbosses.Add("[i:2112] Eye of Cthulhu", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:2112] Eye of Cthulhu", false);
                    }
                }
                else if (NPC.downedBoss1)
                {
                    defeatedbosses.Add("[i:2112] Eye of Cthulhu", true);
                }
                if ((bool)getenabledboss.AllowEaterOfWorlds || (bool)getenabledboss.AllowBrainOfCthulhu)
                {
                    if (NPC.downedBoss2)
                    {
                        defeatedbosses.Add($"{(WorldGen.crimson ? "[i:2104]" : "[i:2111]")} Evil Boss", true);
                    }
                    else
                    {
                        defeatedbosses.Add($"{(WorldGen.crimson ? "[i:2104]" : "[i:2111]")} Evil Boss", false);
                    }
                }
                else if (NPC.downedBoss2)
                {
                    defeatedbosses.Add($"{(WorldGen.crimson ? "[i:2104]" : "[i:2111]")} Evil Boss", true);
                }
                if ((bool)getenabledboss.AllowDeerclops)
                {
                    if (NPC.downedDeerclops)
                    {
                        defeatedbosses.Add("[i:5109] Deerclops", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:5109] Deerclops", false);
                    }
                }
                else if (NPC.downedDeerclops)
                {
                    defeatedbosses.Add("[i:5109] Deerclops", true);
                }
                if ((bool)getenabledboss.AllowQueenBee)
                {
                    if (NPC.downedQueenBee)
                    {
                        defeatedbosses.Add("[i:2108] QueenBee", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:2108] QueenBee", false);
                    }
                }
                else if (NPC.downedQueenBee)
                {
                    defeatedbosses.Add("[i:2108] QueenBee", true);
                }
                if ((bool)getenabledboss.AllowSkeletron)
                {
                    if (NPC.downedBoss3)
                    {
                        defeatedbosses.Add("[i:1281] Skeletron", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:1281] Skeletron", false);
                    }
                }
                else if (NPC.downedBoss3)
                {
                    defeatedbosses.Add("[i:1281] Skeletron", true);
                }
                if ((bool)getenabledboss.AllowWallOfFlesh)
                {
                    if (Main.hardMode)
                    {
                        defeatedbosses.Add("[i:2105] Wall of Flesh", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:2105] Wall of Flesh", false);
                    }
                }
                else if (Main.hardMode)
                {
                    defeatedbosses.Add("[i:2105] Wall of Flesh", true);
                }
                if ((bool)getenabledboss.AllowQueenSlime)
                {
                    if (NPC.downedQueenSlime)
                    {
                        defeatedbosses.Add("[i:4959] Queen Slime", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:4959] Queen Slime", false);
                    }
                }
                else if (NPC.downedQueenSlime)
                {
                    defeatedbosses.Add("[i:4959] Queen Slime", true);
                }
                if (Main.zenithWorld)
                {
                    if ((bool)getenabledboss.AllowTheDestroyer && (bool)getenabledboss.AllowTheTwins && (bool)getenabledboss.AllowSkeletronPrime)
                    {
                        if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
                        {
                            defeatedbosses.Add("[i:2113] Mechdusa", true);
                        }
                        else
                        {
                            defeatedbosses.Add("[i:2113] Mechdusa", false);
                        }
                    }
                    else if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
                    {
                        defeatedbosses.Add("[i:2113] Mechdusa", true);
                    }
                }
                else
                {
                    if ((bool)getenabledboss.AllowTheDestroyer)
                    {
                        if (NPC.downedMechBoss1)
                        {
                            defeatedbosses.Add("[i:2113] Destroyer", true);
                        }
                        else
                        {
                            defeatedbosses.Add("[i:2113] Destroyer", false);
                        }
                    }
                    else if (NPC.downedMechBoss1)
                    {
                        defeatedbosses.Add("[i:2113] Destroyer", true);
                    }
                    if ((bool)getenabledboss.AllowTheTwins)
                    {
                        if (NPC.downedMechBoss2)
                        {
                            defeatedbosses.Add("[i:2106] The Twins", true);
                        }
                        else
                        {
                            defeatedbosses.Add("[i:2106] The Twins", false);
                        }
                    }
                    else if (NPC.downedMechBoss2)
                    {
                        defeatedbosses.Add("[i:2106] The Twins", true);
                    }
                    if ((bool)getenabledboss.AllowSkeletronPrime)
                    {
                        if (NPC.downedMechBoss3)
                        {
                            defeatedbosses.Add("[i:2107] Skeletron prime", true);
                        }
                        else
                        {
                            defeatedbosses.Add("[i:2107] Skeletron prime", false);
                        }
                    }
                    else if (NPC.downedMechBoss3)
                    {
                        defeatedbosses.Add("[i:2107] Skeletron prime", true);
                    }
                }

                if ((bool)getenabledboss.AllowDukeFishron)
                {
                    if (NPC.downedFishron)
                    {
                        defeatedbosses.Add("[i:2588] Duke Fishron", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:2588] Duke Fishron", false);
                    }
                }
                else if (NPC.downedFishron)
                {
                    defeatedbosses.Add("[i:2588] Duke Fishron", true);
                }
                if ((bool)getenabledboss.AllowPlantera)
                {
                    if (NPC.downedPlantBoss)
                    {
                        defeatedbosses.Add("[i:2109] Plantera", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:2109] Plantera", false);
                    }
                }
                else if (NPC.downedPlantBoss)
                {
                    defeatedbosses.Add("[i:2109] Plantera", true);
                }
                if ((bool)getenabledboss.AllowEmpressOfLight)
                {
                    if (NPC.downedEmpressOfLight)
                    {
                        defeatedbosses.Add("[i:4784] Empress of Light", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:4784] Empress of Light", false);
                    }
                }
                else if (NPC.downedEmpressOfLight)
                {
                    defeatedbosses.Add("[i:4784] Empress of Light", true);
                }
                if ((bool)getenabledboss.AllowGolem)
                {
                    if (NPC.downedGolemBoss)
                    {
                        defeatedbosses.Add("[i:2110] Golem", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:2110] Golem", false);
                    }
                }
                else if (NPC.downedGolemBoss)
                {
                    defeatedbosses.Add("[i:2110] Golem", true);
                }
                if ((bool)getenabledboss.AllowLunaticCultist)
                {
                    if (NPC.downedAncientCultist)
                    {
                        defeatedbosses.Add("[i:3372] Lunatic Cultist", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:3372] Lunatic Cultist", false);
                    }
                }
                else if (NPC.downedAncientCultist)
                {
                    defeatedbosses.Add("[i:3372] Lunatic Cultist", true);
                }
                if ((bool)getenabledboss.AllowMoonLord)
                {
                    if (NPC.downedMoonlord)
                    {
                        defeatedbosses.Add("[i:3373] MoonLord", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:3373] MoonLord", false);
                    }
                }
                else if (NPC.downedMoonlord)
                {
                    defeatedbosses.Add("[i:3373] MoonLord", true);
                }
                string result = "";
                foreach (var boss in defeatedbosses)
                {
                    result += $"{boss.Key} {(boss.Value ? "[c/00f000:[K][c/00f000:]]" : "[c/ffff00:[E][c/ffff00:]]")}\n";
                }

                return result;
            }
            #endregion

            #region { stringdefeatedinvasion }
            string GetListDefeatedInvasion()
            {
                Dictionary<string, bool> defeatedinvasion = new();
                if (true)
                {
                    if (NPC.downedGoblins)
                    {
                        defeatedinvasion.Add("Goblin Army", true);
                    }
                    else
                    {
                        //defeatedinvasion.Add("Goblin Army", false);
                    }
                }
                if (true)
                {
                    if (NPC.downedFrost)
                    {
                        defeatedinvasion.Add("Frost Legion", true);
                    }
                    else
                    {
                        //defeatedinvasion.Add("Frost Legion", false);
                    }
                }
                if (true)
                {
                    if (NPC.downedPirates)
                    {
                        defeatedinvasion.Add("Pirates", true);
                    }
                    else
                    {
                        //defeatedinvasion.Add("Pirates", false);
                    }
                }
                if (true)
                {
                    if (NPC.downedMartians)
                    {
                        defeatedinvasion.Add("The Martians", true);
                    }
                    else
                    {
                        //defeatedinvasion.Add("The Martians", false);
                    }
                }
                if (true)
                {
                    if (NPC.downedTowers)
                    {
                        defeatedinvasion.Add("Celestial Pillars", true);
                    }
                    else
                    {
                        //defeatedinvasion.Add("Celestial Pillars", false);
                    }
                }
                string result = "";

                foreach (var invasion in defeatedinvasion)
                {
                    result += $"- {invasion.Key}\n";
                }

                return result;
            }
            #endregion


            args.Player.SendMessage(
                $"List Of Bosses:" +
                $"\n{GetListDefeatedBoss()}",
                Color.Gray);
        }

        private static Dictionary<string, DateTime> ReportCD = new();
        private void CMD_Report(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Usage: /report <message>" +
                    "\nor you can use '/report <player> <message>'" +
                    $"\nexample: /report {args.Player.Name} is cheating");
                return;
            }

            if (args.Player.Account != null)
            {
                if (ReportCD.ContainsKey(args.Player.Account.Name))
                {
                    int cdtotal_min = ((int)(DateTime.UtcNow - ReportCD[args.Player.Account.Name]).TotalMinutes - 10) * -1;
                    int cdtotal_sec = ((int)(DateTime.UtcNow - ReportCD[args.Player.Account.Name]).TotalSeconds - 60) * -1;
                    if ((DateTime.UtcNow - ReportCD[args.Player.Name]).TotalMinutes < 10)
                    {
                        args.Player.SendErrorMessage($"You can report again in {(cdtotal_min == 0 ? $"{(cdtotal_sec <= 1 ? $"{cdtotal_sec} second" : $"{cdtotal_sec} seconds" )}" : $"{(cdtotal_min <= 1 ? $"{cdtotal_min} minute" : $"{cdtotal_min} minutes")}" )}");
                        return;
                    }
                }
            }
            else
            {
                if (ReportCD.ContainsKey(args.Player.Name))
                {
                    int cdtotal_min = (int)(DateTime.UtcNow - ReportCD[args.Player.Name]).TotalMinutes;
                    int cdtotal_sec = (int)(DateTime.UtcNow - ReportCD[args.Player.Name]).TotalSeconds;
                    if ((DateTime.UtcNow - ReportCD[args.Player.Name]).TotalMinutes < 10)
                    {
                        args.Player.SendErrorMessage($"You can report again in {(cdtotal_min == 0 ? $"{(cdtotal_sec <= 1 ? $"{cdtotal_sec} second" : $"{cdtotal_sec} seconds")}" : $"{(cdtotal_min <= 1 ? $"{cdtotal_min} minute" : $"{cdtotal_min} minutes")}")}");
                        return;
                    }
                }
            }

            string get_target_name = "";
            bool isAccount = false;
            int get_target_index = -1;

            var targetplayers = TSPlayer.FindByNameOrID(args.Parameters[0]);

            string report_message = string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count);

            if (targetplayers.Count == 1)
            {
                if (targetplayers[0].Account != null)
                {
                    get_target_name = targetplayers[0].Account.Name;
                    isAccount = true;
                } else
                {
                    get_target_name = targetplayers[0].Name;
                }

                get_target_index = targetplayers[0].Index;

                if (args.Parameters.Count == 1)
                {
                    report_message = "no message";
                } else
                {
                    report_message = string.Join(" ", args.Parameters.ToArray(), 1, args.Parameters.Count - 1);
                }
            }

            if (get_target_name == "") get_target_name = $"{DiscordKLP.S_}none{DiscordKLP.S_}";

            /*
            if (get_target_name == "")
            {
                foreach (var targetplayer in TShock.Players)
                {
                    if (report_message.Contains(targetplayer.Name))
                    {
                        get_target_name = targetplayer.Name;
                        break;
                    }
                }
            }
            */

            if (args.Player.Account != null)
            {
                if (ReportCD.ContainsKey(args.Player.Account.Name))
                {
                    ReportCD[args.Player.Account.Name] = DateTime.UtcNow;
                } else { ReportCD.Add(args.Player.Account.Name, DateTime.Now); }

            } else
            {
                if (ReportCD.ContainsKey(args.Player.Name))
                {
                    ReportCD[args.Player.Name] = DateTime.UtcNow;
                }
                else { ReportCD.Add(args.Player.Name, DateTime.Now); }
            }

            if (args.Player.Account != null)
            {
                string playernames = "";
                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null) continue;
                    playernames += player.Name + DiscordKLP.S_;
                }
                playernames.TrimEnd(DiscordKLP.S_);

                int id = DBManager.AddReport(args.Player.Account.Name, get_target_name, report_message, DateTime.UtcNow, $"{args.Player.TileX}, {args.Player.TileY}", playernames);
                Discordklp.KLPBotSendMessage_Report(id, args.Player.Account.Name, get_target_name, report_message, DateTime.UtcNow, $"{args.Player.TileX}, {args.Player.TileY}", playernames);
                SendStaffMessage($"[Report] from {args.Player.Account.Name}" +
                    $"\nTarget: {(get_target_name == "" ? $"{DiscordKLP.S_}none{DiscordKLP.S_}" : $"{get_target_name} {(isAccount ? "(Account) " : "")} {(get_target_index == -1 ? "" : get_target_index)}")}" +
                    $"\nMessage: {report_message}", Color.OrangeRed);
                args.Player.SendSuccessMessage("Reported Sent!" +
                    $"\ntarget: {(get_target_name == "" ? $"{DiscordKLP.S_}none{DiscordKLP.S_}" : $"{get_target_name} {(isAccount ? "(Account) " : "")} {(get_target_index == -1 ? "" : get_target_index)}")}" +
                    $"\nmessage: {report_message}");
                return;
            } else
            {
                SendStaffMessage($"[Temporary-Report] from {args.Player.Name}" +
                    $"\nTarget: {(get_target_name == "" ? $"{DiscordKLP.S_}none{DiscordKLP.S_}" : $"{get_target_name} {(isAccount ? "(Account) " : "")} {(get_target_index == -1 ? "" : get_target_index)}")}" +
                    $"\nMessage: {report_message}", Color.OrangeRed);
                args.Player.SendSuccessMessage("Temporary Reported Sent!" +
                    $"\ntarget: {(get_target_name == "" ? $"{DiscordKLP.S_}none{DiscordKLP.S_}" : $"{get_target_name} {(isAccount ? "(Account) " : "")} {(get_target_index == -1 ? "" : get_target_index)}")}" +
                    $"\nmessage: {report_message}");
                return;
            }

        }

        #endregion

        #region { Staff }

        private void CMD_StaffChat(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage($"Usage: {TShock.Config.Settings.CommandSpecifier}staffchat <message>" +
                    $"\nshortcuts: {TShock.Config.Settings.CommandSpecifier}staff, {TShock.Config.Settings.CommandSpecifier}#");
                return;
            }

            CONFIG_COLOR_RBG Config_messagecolor = (CONFIG_COLOR_RBG)Config.Main.StaffChat_MessageRecieved_InGame_RBG;

            SendStaffMessage(GetSendMessageInGameResult(args.Player, Config.Main.StaffChat_MessageSend_Discord, string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count)), new(Config_messagecolor.R, Config_messagecolor.G, Config_messagecolor.B));
            Discordklp.KLPBotSendMessageMain(GetMessageInGameResult(args.Player, Config.Main.StaffChat_MessageRecieved_InGame, string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count)));
            MKLP_Console.SendLog_Message_StaffChat_InGame(args.Player.Name, string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count));

            #region GetMessageInGameResult
            string GetMessageInGameResult(TSPlayer tsplayer, string Text, string message)
            {
                string Context = Text;
                Context = Context.Replace("%ingamename%", tsplayer.Name);
                if (tsplayer.Account != null && tsplayer != TSPlayer.Server)
                {
                    Context = Context.Replace("%ingameaccountname%", tsplayer.Account.Name);
                    if (tsplayer.Name != tsplayer.Account.Name)
                    {
                        Context = Context.Replace("%ingameaccountnameifname%", tsplayer.Account.Name);
                    } else
                    {
                        Context = Context.Replace("%ingameaccountnameifname%", tsplayer.Name);
                    }

                    try
                    {
                        ulong getuserid = LinkAccountManager.GetUserID(tsplayer.Account.Name);

                        Context = Context.Replace("%ingamelinkedusername%", Discordklp.GetUser(getuserid).Username);
                        Context = Context.Replace("%ingamelinkedicon%", Config.Main.StaffChat_Message_ingamelinkedicon);
                    }
                    catch (NullReferenceException)
                    {
                        Context = Context.Replace("%ingamelinkedusername%", "");
                        Context = Context.Replace("%ingamelinkedicon%", "");
                    }

                }
                else
                {
                    Context = Context.Replace("%ingameaccountname%", "");
                    Context = Context.Replace("%ingameaccountnameifname%", tsplayer.Name);

                    try
                    {
                        ulong getuserid = LinkAccountManager.GetUserID(tsplayer.Name);
                        
                        Context = Context.Replace("%ingamelinkedusername%", Discordklp.GetUser(getuserid).Username);
                        Context = Context.Replace("%ingamelinkedicon%", Config.Main.StaffChat_Message_ingamelinkedicon);
                    }
                    catch (NullReferenceException)
                    {
                        Context = Context.Replace("%ingamelinkedusername%", "");
                        Context = Context.Replace("%ingamelinkedicon%", "");
                    }
                }

                
                if (tsplayer.Group != null)
                {
                    Context = Context.Replace("%groupname%", tsplayer.Group.Name);
                    Context = Context.Replace("%groupprefix%", tsplayer.Group.Prefix);
                    Context = Context.Replace("%groupsuffix%", tsplayer.Group.Suffix);
                } else
                {
                    Context = Context.Replace("%groupname%", "");
                    Context = Context.Replace("%groupprefix%", "");
                    Context = Context.Replace("%groupsuffix%", "");
                }

                if (tsplayer.tempGroup != null)
                {
                    Context = Context.Replace("%tempgroupname%", tsplayer.tempGroup.Name);
                    Context = Context.Replace("%tempgroupprefix%", tsplayer.tempGroup.Prefix);
                    Context = Context.Replace("%tempgroupsuffix%", tsplayer.tempGroup.Suffix);
                } else
                {
                    Context = Context.Replace("%tempgroupname%", "");
                    Context = Context.Replace("%tempgroupprefix%", "");
                    Context = Context.Replace("%tempgroupsuffix%", "");
                }

                Context = Context.Replace("%message%", message);

                return Context;
            }
            #endregion

            #region GetSendMessageInGameResult
            string GetSendMessageInGameResult(TSPlayer tsplayer, string Text, string message)
            {
                string Context = Text;
                Context = Context.Replace("%ingamename%", tsplayer.Name);
                if (tsplayer.Account != null && tsplayer != TSPlayer.Server)
                {
                    Context = Context.Replace("%ingameaccountname%", tsplayer.Account.Name);
                    if (tsplayer.Name != tsplayer.Account.Name)
                    {
                        Context = Context.Replace("%ingameaccountnameifname%", tsplayer.Account.Name);
                    }
                    else
                    {
                        Context = Context.Replace("%ingameaccountnameifname%", tsplayer.Name);
                    }

                    try
                    {
                        ulong getuserid = LinkAccountManager.GetUserID(tsplayer.Account.Name);
                        
                        Context = Context.Replace("%ingamelinkedusername%", Discordklp.GetUser((ulong)getuserid).Username);
                        Context = Context.Replace("%discordacclinkedicon%", Config.Main.StaffChat_Message_discordacclinkedicon);

                    }
                    catch (NullReferenceException)
                    {
                        Context = Context.Replace("%ingamelinkedusername%", "");
                        Context = Context.Replace("%discordacclinkedicon%", "");
                    }
                }
                else
                {
                    Context = Context.Replace("%ingameaccountname%", "");
                    Context = Context.Replace("%ingameaccountnameifname%", tsplayer.Name);

                    try
                    {
                        ulong getuserid = getuserid = LinkAccountManager.GetUserID(tsplayer.Name);
                        
                        Context = Context.Replace("%ingamelinkedusername%", Discordklp.GetUser((ulong)getuserid).Username);
                        Context = Context.Replace("%discordacclinkedicon%", Config.Main.StaffChat_Message_discordacclinkedicon);

                    }
                    catch (NullReferenceException)
                    {
                        Context = Context.Replace("%ingamelinkedusername%", "");
                        Context = Context.Replace("%discordacclinkedicon%", "");
                    }
                }

                if (tsplayer.Group != null)
                {
                    Context = Context.Replace("%groupname%", tsplayer.Group.Name);
                }
                else
                {
                    Context = Context.Replace("%groupname%", "");
                }

                if (tsplayer.tempGroup != null)
                {
                    Context = Context.Replace("%tempgroupname%", tsplayer.tempGroup.Name);
                }
                else
                {
                    Context = Context.Replace("%tempgroupname%", "");
                }

                Context = Context.Replace("%message%", message);

                return Context;
            }
            #endregion

            #endregion
        }

        #endregion

        #region { Admin }

        private void CMD_MapPingTP(CommandArgs args)
        {
            #region code
            if (!args.Player.ContainsData("MKLP-Map_Ping_TP"))
            {
                args.Player.SetData("MKLP-Map_Ping_TP", true);
                args.Player.SendSuccessMessage("you're now able to tp ping");
            }
            else
            {
                if (args.Player.GetData<bool>("MKLP-Map_Ping_TP"))
                {
                    args.Player.SetData("MKLP-Map_Ping_TP", false);
                    args.Player.SendSuccessMessage("you can no longer able to tp ping");
                }
                else
                {
                    args.Player.SetData("MKLP-Map_Ping_TP", true);
                    args.Player.SendSuccessMessage("you're now able to tp ping");
                }
            }
            #endregion
        }

        private void CMD_Boss(CommandArgs args)
        {
            #region code



            #endregion
        }

        #endregion

        #region { Moderator }

        private void CMD_Ban(CommandArgs args)
        {
            #region code

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage($"Usage: /{((bool)Config.Main.Replace_Ban_TShockCommand ? "ban" : "qban")} <player> <reason> <duration> <args...>");
                args.Player.SendMessage($"[c/8fbfd4:Example:] [c/a5d063:/{((bool)Config.Main.Replace_Ban_TShockCommand ? "ban" : "qban")} {args.Player.Name} \"cheating\" \"1d 1m\" -offline]" +
                    $"\n[c/8fbfd4:duration:] 1d = 1day (d,h,m,s = day,hour,minute,second)" +
                    $"\n[c/8fbfd4:args:] ( -alt = bans only name ) ( -offline = only used when banning a offline player )",
                    Color.Gray);
                return;
            }

            bool altban = args.Parameters.Any(p => p == "-alt");
            bool offlineban = args.Parameters.Any(p => p == "-offline");


            bool uuidip = true;

            if (altban == true) uuidip = false;


            List<string> flags = new List<string>() { "-alt", "-offline" };

            string reason = "No Reason Specified";
            string duration = null;
            DateTime expiration = DateTime.MaxValue;

            for (int i = 1; i < args.Parameters.Count; i++)
            {
                var param = args.Parameters[i];
                if (!flags.Contains(param))
                {
                    reason = param;
                    break;
                }
            }
            for (int i = 2; i < args.Parameters.Count; i++)
            {
                var param = args.Parameters[i];
                if (!flags.Contains(param))
                {
                    duration = param;
                    break;
                }
            }

            if (TShock.Utils.TryParseTime(duration, out ulong seconds))
            {
                expiration = DateTime.UtcNow.AddSeconds(seconds);
            }

            if (offlineban)
            {
                if (!args.Player.HasPermission(Config.Permissions.CMD_OfflineBan))
                {
                    args.Player.SendErrorMessage("You do not have permission to ban offline players!");
                    return;
                }

                UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);

                if (targetaccount == null)
                {
                    args.Player.SendErrorMessage($"Account name {args.Parameters[0]} doesn't exist");
                }

                var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

                TSPlayer? targetplayer = null;

                foreach (TSPlayer player in players)
                {
                    if (player == null) continue;
                    if (player.Account.Name == targetaccount.Name)
                    {
                        targetplayer = player;
                    }
                }

                if (targetplayer != null)
                {
                    if (ManagePlayer.OnlineBan(args.Silent, targetplayer, reason, args.Player.Account.Name, expiration, uuidip, uuidip))
                    {
                        args.Player.SendSuccessMessage($"Successfully banned {targetplayer.Name} for {reason}");
                    }
                    else
                    {
                        args.Player.SendErrorMessage($"Error occur banning {targetplayer.Name}");
                    }
                } else
                {
                    if (ManagePlayer.OfflineBan(targetaccount, reason, args.Player.Account.Name, expiration, uuidip, uuidip))
                    {
                        args.Player.SendSuccessMessage($"Successfully banned Acc: {targetaccount.Name} for {reason}");
                    }
                    else
                    {
                        args.Player.SendErrorMessage($"Error occur banning Acc: {targetaccount.Name}");
                    }
                }

            } else
            {
                var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

                if (players.Count > 1)
                {
                    args.Player.SendMultipleMatchError(players.Select(p => p.Name));
                    return;
                }

                if (players.Count < 1)
                {
                    args.Player.SendErrorMessage("Could not find the target specified. Check that you have the correct spelling.");
                    return;
                }

                var targetplayer = players[0];


                if (ManagePlayer.OnlineBan(args.Silent, targetplayer, reason, args.Player.Account.Name, expiration, uuidip, uuidip))
                {
                    args.Player.SendSuccessMessage($"Successfully banned {targetplayer.Name} for {reason}");
                } else
                {
                    args.Player.SendErrorMessage($"Error occur banning {targetplayer.Name}");
                }
            }
            #endregion
        }

        private void CMD_UnBan(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Usage: /unban <ticket/account> <args...>");
                return;
            }

            switch (args.Parameters[0].ToLower())
            {
                case "help":
                    {
                        args.Player.SendMessage("Usage: /unban <ticket/account> <args...>" +
                            "\n'ticket <ticket number>' : Removes a specific ban ticket" +
                            "\n'account <account name>' : Removes all the tickets account has", Color.Gray);
                        return;
                    }
                case "ticket":
                    {
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendErrorMessage("Usage: /unban <ticket> <ticket number>");
                        }

                        int ticketnumber = -1;

                        if (int.TryParse(args.Parameters[1], out ticketnumber))
                        {
                            if (ManagePlayer.UnBanTicketNumber(ticketnumber, args.Player.Account.Name))
                            {
                                args.Player.SendSuccessMessage("Removed Ban Ticket Number: " + ticketnumber);
                                return;
                            }
                            else
                            {
                                args.Player.SendErrorMessage("Invalid Ticket number!");
                                return;
                            }
                        }
                        else
                        {
                            args.Player.SendErrorMessage("Invalid Ticket number!");
                            return;
                        }
                    }
                case "account":
                    {
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendErrorMessage("Usage: /unban <account> <account name>");
                        }

                        UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);

                        if (targetaccount == null)
                        {
                            args.Player.SendErrorMessage("Invalid Account");
                            return;
                        }

                        if (ManagePlayer.UnBanAccount(targetaccount, args.Player.Account.Name))
                        {
                            args.Player.SendSuccessMessage($"Removing Ban Tickets from account: {targetaccount.Name}");
                            return;
                        } else
                        {
                            args.Player.SendErrorMessage($"Account: '{args.Parameters[1]}' could not be found...");
                            return;
                        }
                    }
                default:
                    {
                        args.Player.SendErrorMessage("Invalid Type");
                        return;
                    }
            }
            #endregion
        }

        private void CMD_Mute(CommandArgs args)
        {
            #region code

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage($"Proper Usage: /{((bool)Config.Main.Replace_Mute_TShockCommand ? "mute" : "qmute")} <player> <duration> <reason>" +
                    (args.Player.HasPermission(Config.Permissions.CMD_OfflineUnMute) ? $"\nMuting Offline Player: /{((bool)Config.Main.Replace_Mute_TShockCommand ? "mute" : "qmute")} <accountname> <duration> <reason> -offline" : ""));
                return;
            }

            bool offlineMute = args.Parameters.Any(p => p == "-offline");

            List<string> flags = new List<string>() { "-offline" };

            string reason = "";
            string duration = null;
            DateTime expiration = DateTime.MaxValue;

            for (int i = 2; i < args.Parameters.Count; i++)
            {
                var param = args.Parameters[i];
                if (!flags.Contains(param))
                {
                    reason = param;
                    break;
                }
            }
            for (int i = 1; i < args.Parameters.Count; i++)
            {
                var param = args.Parameters[i];
                if (!flags.Contains(param))
                {
                    duration = param;
                    break;
                }
            }

            if (TShock.Utils.TryParseTime(duration, out ulong seconds))
            {
                expiration = DateTime.UtcNow.AddSeconds(seconds);
            }


            if (offlineMute)
            {
                if (!args.Player.HasPermission(Config.Permissions.CMD_OfflineMute))
                {
                    args.Player.SendErrorMessage("You do not have permission to mute offline players");
                    return;
                }

                UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);

                if (targetaccount == null)
                {
                    args.Player.SendErrorMessage($"Account name {args.Parameters[0]} doesn't exist");
                }

                var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

                TSPlayer? targetplayer = null;

                foreach (TSPlayer player in players)
                {
                    if (player == null) continue;
                    if (player.Account.Name == targetaccount.Name)
                    {
                        targetplayer = player;
                    }
                }

                if (targetplayer != null)
                {
                    if (targetplayer.mute)
                    {
                        args.Player.SendErrorMessage($"{targetplayer.Name} was already muted!");
                        return;
                    }

                    if (ManagePlayer.OnlineMute(args.Silent, targetplayer, reason, args.Player.Account.Name, expiration))
                    {
                        args.Player.SendSuccessMessage($"Muted {targetplayer.Name}");
                    }
                    else
                    {
                        args.Player.SendSuccessMessage($"Error occur Muting {targetplayer.Name}");
                    }

                } else
                {
                    if (ManagePlayer.OfflineMute(targetaccount, reason, args.Player.Account.Name, expiration))
                    {
                        args.Player.SendSuccessMessage($"Muted {targetaccount.Name}");
                    }
                    else
                    {
                        args.Player.SendSuccessMessage($"Error occur Muting {targetaccount.Name}");
                    }
                }

                

            } else
            {
                var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

                if (players.Count > 1)
                {
                    args.Player.SendMultipleMatchError(players.Select(p => p.Name));
                    return;
                }

                if (players.Count < 1)
                {
                    args.Player.SendErrorMessage("Could not find the target specified. Check that you have the correct spelling.");
                    return;
                }

                var targetplayer = players[0];

                if (targetplayer.mute)
                {
                    args.Player.SendErrorMessage($"{targetplayer.Name} was already muted!");
                    return;
                }

                if (ManagePlayer.OnlineMute(args.Silent, targetplayer, reason, args.Player.Account.Name, expiration))
                {
                    args.Player.SendSuccessMessage($"muted {targetplayer.Name}");
                } else
                {
                    args.Player.SendSuccessMessage($"Error occur Muting {targetplayer.Name}");
                }
            }
            //DBManager.CheckPlayer(player, true);
            #endregion
        }

        private void CMD_UnMute(CommandArgs args)
        {
            #region code

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Proper Usage: /unmute <player>" +
                    (args.Player.HasPermission(Config.Permissions.CMD_OfflineUnMute) ? "\nUnmuting Offline Player: /unmute <accountname> -offline" : ""));
                return;
            }

            bool offlineMute = args.Parameters.Any(p => p == "-offline");

            if (offlineMute)
            {
                if (!args.Player.HasPermission(Config.Permissions.CMD_OfflineMute))
                {
                    args.Player.SendErrorMessage("You do not have permission to mute offline players");
                    return;
                }

                UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);

                if (targetaccount == null)
                {
                    args.Player.SendErrorMessage($"Account name {args.Parameters[0]} doesn't exist");
                }

                var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

                TSPlayer? targetplayer = null;

                foreach (TSPlayer player in players)
                {
                    if (player == null) continue;
                    if (player.Account.Name == targetaccount.Name)
                    {
                        targetplayer = player;
                    }
                }

                if (targetplayer != null)
                {
                    if (!targetplayer.mute)
                    {
                        args.Player.SendErrorMessage($"{targetplayer.Name} hasn't been muted!");
                        return;
                    }

                    if (ManagePlayer.OnlineUnMute(args.Silent, targetplayer, args.Player.Account.Name))
                    {
                        args.Player.SendSuccessMessage($"unmuted {targetplayer.Name}");
                    }
                    else
                    {
                        args.Player.SendSuccessMessage($"Error occur unmuting {targetplayer.Name}");
                    }

                }
                else
                {
                    if (ManagePlayer.OfflineUnMute(targetaccount, args.Player.Account.Name))
                    {
                        args.Player.SendSuccessMessage($"unmuted {targetaccount.Name}");
                    }
                    else
                    {
                        args.Player.SendSuccessMessage($"Error occur unmuting {targetaccount.Name}");
                    }
                }



            }
            else
            {
                var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

                if (players.Count > 1)
                {
                    args.Player.SendMultipleMatchError(players.Select(p => p.Name));
                    return;
                }

                if (players.Count < 1)
                {
                    args.Player.SendErrorMessage("Could not find the target specified. Check that you have the correct spelling.");
                    return;
                }

                var targetplayer = players[0];

                if (!targetplayer.mute)
                {
                    args.Player.SendErrorMessage($"{targetplayer.Name} hasn't been muted!");
                    return;
                }

                if (ManagePlayer.OnlineUnMute(args.Silent, targetplayer, args.Player.Account.Name))
                {
                    args.Player.SendSuccessMessage($"Unmuted {targetplayer.Name}");
                }
                else
                {
                    args.Player.SendSuccessMessage($"Error occur unmuting {targetplayer.Name}");
                }
            }
            //DBManager.CheckPlayer(player, true);
            #endregion
        }

        private void CMD_disable(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Proper Usage: /disable <player> <reason>");
                return;
            }

            var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

            if (players.Count > 1)
            {
                args.Player.SendMultipleMatchError(players.Select(p => p.Name));
                return;
            }

            if (players.Count < 1)
            {
                args.Player.SendErrorMessage("Could not find the target specified. Check that you have the correct spelling.");
                return;
            }

            var targetplayer = players[0];

            if (args.Parameters.Count == 1)
            {

                ManagePlayer.DisablePlayer(targetplayer, executername: args.Player.Name);
                args.Player.SendSuccessMessage($"Player {targetplayer.Name} disabled");

            } else
            {
                ManagePlayer.DisablePlayer(targetplayer, args.Parameters[1], executername: args.Player.Name);
                args.Player.SendSuccessMessage($"Player {targetplayer.Name} disabled");

            }

            return;
            #endregion
        }

        private void CMD_undisable(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Proper Usage: /enable <player>");
                return;
            }

            var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

            if (players.Count > 1)
            {
                args.Player.SendMultipleMatchError(players.Select(p => p.Name));
                return;
            }

            if (players.Count < 1)
            {
                args.Player.SendErrorMessage("Could not find the target specified. Check that you have the correct spelling.");
                return;
            }

            var targetplayer = players[0];

            if (ManagePlayer.UnDisablePlayer(targetplayer, args.Player.Account.Name))
            {
                args.Player.SendSuccessMessage($"Player {targetplayer.Name} enabled");
            } else
            {
                args.Player.SendErrorMessage($"Player {targetplayer.Name} isn't disabled");
            }


            #endregion
        }

        #endregion

        #endregion

        #region [ Function ]

        public static void SendStaffMessage(string message, Microsoft.Xna.Framework.Color messagecolor)
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;
                if (!player.HasPermission(Config.Permissions.Staff)) continue;
                TSPlayer.All.SendMessage(message, messagecolor);
            }
        }
        
        #endregion
    }

    #region [ Colored Console ]
    public static class MKLP_Console
    {
        public static void SendTitle()
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("[MKLP]");
            Console.ResetColor();
        }
        public static void SendLog_Exception(object? value)
        {
            SendTitle();
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(" Error: ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void SendLog_Message_StaffChat_InGame(string username, object? value, ConsoleColor consolecolor = ConsoleColor.White)
        {
            SendTitle();
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" [InGame-StaffChat] ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(username + ": ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = consolecolor;
            Console.WriteLine(value);
            Console.ResetColor();
        }
        public static void SendLog_Message_StaffChat_Discord(string username, object? value, ConsoleColor consolecolor = ConsoleColor.White)
        {
            SendTitle();
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" [Discord-StaffChat] ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(username + ": ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = consolecolor;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void SendLog_Message_DiscordBot(object? value, string type, ConsoleColor typeconsolecolor = ConsoleColor.Yellow, ConsoleColor consolecolor = ConsoleColor.Cyan)
        {
            SendTitle();
            Console.ResetColor();
            Console.ForegroundColor = typeconsolecolor;
            Console.Write(type);
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" Discord: ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = consolecolor;
            Console.WriteLine(value);
            Console.ResetColor();
        }

    }
    #endregion

}