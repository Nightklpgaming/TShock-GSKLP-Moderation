//Microsoft
using Google.Protobuf.WellKnownTypes;
using IL.Terraria.DataStructures;
using IL.Terraria.Graphics;
using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework;
using MKLP.Functions;
using MKLP.Modules;
using MySqlX.XDevAPI.Relational;
using Newtonsoft.Json;
using NuGet.Protocol;
using NuGet.Protocol.Plugins;
using Org.BouncyCastle.Asn1.X509;
using Steamworks;
using System;
using System.Collections.Generic;





//System
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO.Streams;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using System.Threading.Channels;



//Terraria
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TerrariaApi.Server;
//TShock
using TShockAPI;
using TShockAPI.Configuration;
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
        public override string Description => "Makes Moderating a bit easy";
        public override string Name => "MKLP";
        public override System.Version Version => new System.Version(1, 5);
        #endregion

        #region [ Variables ]

        public static Config Config = Config.Read(); //CONFIG

        public static MKLP_DB DBManager = new();

        public static DiscordKLP Discordklp = new();

        public static AccountDLinked LinkAccountManager = new();

        public static Dictionary<string, string> DisabledKey = new();

        //illegal things list
        public static Dictionary<int, string> IllegalItemProgression = new();

        public static Dictionary<short, string> IllegalProjectileProgression = new();

        public static Dictionary<SurvivalManager.MKLP_Tile, string> IllegalTileProgression = new();

        public static Dictionary<ushort, string> IllegalWallProgression = new();

        public static bool HasBanGuardPlugin = false;
        #endregion

        public MKLP(Main game) : base(game)
        {
            //amogus
            HasBanGuardPlugin = File.Exists(Path.Combine("ServerPlugins", "BanGuard.dll"));
        }

        #region [ Initialize ]
        public override void Initialize()
        {
            if (!HasBanGuardPlugin && ((bool)Config.BanGuard.UsingBanGuard && (bool)Config.BanGuard.UsingPlugin))
            {
                Config.BanGuard.UsingBanGuard = false;
                MKLP_Console.SendLog_Warning("Warning: BanGuard plugin doesn't Exist on \"ServerPlugins\" Folder!");
            }
            BanGuardAPI.Initialize();

            //=====================Player===================
            GetDataHandlers.PlayerUpdate += OnPlayerUpdate;

            //GetDataHandlers.player

            ServerApi.Hooks.ServerJoin.Register(this, OnPlayerJoin);

            ServerApi.Hooks.ServerLeave.Register(this, OnPlayerLeave);

            PlayerHooks.PlayerCommand += OnPlayerCommand;

            //PlayerHooks.PlayerChat += OnPlayerChat;
            ServerApi.Hooks.ServerChat.Register(this, OnChatReceived);

            //=====================game=====================
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);

            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);

            GetDataHandlers.TileEdit += OnTileEdit;

            GetDataHandlers.PlaceObject += OnPlaceObject;

            GetDataHandlers.PaintTile += OnPaintTile;

            GetDataHandlers.PaintWall += OnPaintWall;

            GetDataHandlers.MassWireOperation += OnMassWireOperation;

            GetDataHandlers.LiquidSet += HandleLiquidInteraction;

            GetDataHandlers.NewProjectile += OnNewProjectile;

            GetDataHandlers.HealOtherPlayer += OnHealOtherPlayer;

            //GetDataHandlers.ItemDrop

            ServerApi.Hooks.NpcSpawn.Register(this, OnNPCSpawn);

            ServerApi.Hooks.NpcKilled.Register(this, OnNPCKilled);

            ServerApi.Hooks.NpcAIUpdate.Register(this, OnNPCAIUpdate);

            ServerApi.Hooks.ProjectileAIUpdate.Register(this, OnProjectileAIUpdate);

            //ServerApi.Hooks.WireTriggerAnnouncementBox.Register(this, OnProjectileAIUpdate);

            //=====================Server===================
            //ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);

            ServerApi.Hooks.ServerBroadcast.Register(this, OnServerBroadcast);

            ServerApi.Hooks.WorldSave.Register(this, OnWorldSave);

            ServerApi.Hooks.GamePostInitialize.Register(this, OnServerStart);

            GeneralHooks.ReloadEvent += OnReload;

            #region [ Commands Initialize ]

            #region { default }
            
            Commands.ChatCommands.Add(new Command(Config.Permissions.Default_CMD_Ping, CMD_ping, "ping")
            {
                HelpText = "Get Players Latency"
            });
            
            Commands.ChatCommands.Add(new Command(Config.Permissions.Default_CMD_Progression, CMD_BE, "progression", "prog")
            {
                HelpText = "displays defeated bosses and events"
            });

            Commands.ChatCommands.Add(new Command(Config.Permissions.Default_CMD_Report, CMD_Report, "report")
            {
                HelpText = "Report any suspicious activity by doing /report <message>",
                AllowServer = false
            });

            if ((bool)Config.Main.Replace_Who_TShockCommand)
            {
                Command VarMCMD_Who = new(MCMD_Playing, "playing", "online", "who") { HelpText = "Shows the currently connected players." };
                Commands.ChatCommands.RemoveAll(cmd => cmd.Names.Exists(alias => VarMCMD_Who.Names.Contains(alias)));
                Commands.ChatCommands.Add(VarMCMD_Who);
            }

            #endregion

            #region { Staff }

            Commands.ChatCommands.Add(new Command(Config.Permissions.Staff, CMD_StaffChat, "staffchat", "staff", "#")
            {
                HelpText = "Sends a message in staff chat"
            });

            if ((bool)Config.Main.Replace_AccountInfo_TShockCommand)
            {
                Command VarMCMD_AccountInfo = new(Permissions.checkaccountinfo, MCMD_AccountInfo, "accountinfo", "ai") { HelpText = "Shows information about a user." };
                Commands.ChatCommands.RemoveAll(cmd => cmd.Names.Exists(alias => VarMCMD_AccountInfo.Names.Contains(alias)));
                Commands.ChatCommands.Add(VarMCMD_AccountInfo);
            } else
            {
                Commands.ChatCommands.Add(new Command(Permissions.checkaccountinfo, MCMD_AccountInfo, "klpaccountinfo")
                {
                    HelpText = "Shows information about a user."
                });
            }

            #endregion

            #region { Admin }

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_ClearMessage, CMD_ClearMessage, "clearmessage", "messageclear", "purgemessage")
            {
                HelpText = "Clears the whole message chat"
            });

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_LockDown, CMD_LockDown, "lockdown")
            {
                HelpText = "Prevents Players from joining the server"
            });

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_LockDownRegister, CMD_LockDownRegister, "lockdownregister", "lockdownreg")
            {
                HelpText = "Prevents Players to register their account"
            });

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_MapPingTP, CMD_MapPingTP, "tpmap", "pingmap", "maptp")
            {
                AllowServer = false,
                HelpText = "Allows you to teleporter anywhere using map ping"
            });

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_ClearLag, CMD_ClearLag, "clearlag")
            {
                HelpText = "Deletes low value npc/items"
            });

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_ManageBoss, CMD_ManageBoss, "manageboss", "mboss")
            {
                HelpText = "Manage it by enable/disable boss or schedule it"
            });

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_Vanish, CMD_Vanish, "vanish", "ghost")
            {
                AllowServer = false,
                HelpText = "allows you to become completely invisible to players."
            });

            #endregion

            #region { moderation }

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_ManageReport, CMD_ManageReport, "managereport", "mreport")
            {
                HelpText = "View/Delete any reports"
            });

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_Ban, CMD_BanInfo, "baninfo")
            {
                HelpText = "Displays ban information using ban ticket number"
            });

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
                if (HasBanGuardPlugin && ((bool)Config.BanGuard.UsingPlugin && (bool)Config.BanGuard.UsingBanGuard))
                {
                    Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_Ban, CMD_Ban, "qban")
                    {
                        HelpText = "Bans a player"
                    });
                    MKLP_Console.SendLog_Warning("Cannot Replace TShock Ban Command When \"UsingPlugin\" on \"BanGuard\" are enabled on Config");
                    Config.BanGuard.APIKey = BanGuard.BanGuard.Config.APIKey;
                    Config.Changeall();
                } else
                {
                    Command VarCMD_Ban = new(Config.Permissions.CMD_Ban, CMD_Ban, "ban") { HelpText = "Bans a player" };
                    Commands.ChatCommands.RemoveAll(cmd => cmd.Names.Exists(alias => VarCMD_Ban.Names.Contains(alias)));
                    Commands.ChatCommands.Add(VarCMD_Ban);
                }
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

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_Spy, CMD_Spy, "spy")
            {
                HelpText = "allows you to stalk a player"
            });

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_UUIDMatch, CMD_uuidmatch, "uuidmatch")
            {
                HelpText = "check useraccounts with following match uuid accounts"
            });

            #endregion

            #region [ Manager ]

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_MKLPDiscord, CMD_MKLPDiscord, "mklpdiscord")
            {
                HelpText = "Manage Linked Account Players"
            });

            #endregion

            #endregion

            if (Config.Discord.BotToken != "NONE")
            {
                Discordklp.Initialize();
            } else
            {
                MKLP_Console.SendLog_Message_DiscordBot("Discord bot token has not been set!", " {Setup} ");
            }

            for (int i = 0; i < Main.item.Length; i++)
            {
                prevItemDrops[i] = new Item();
                prevItemDrops[i].SetDefaults(0); // Initialize as 'empty' item
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

                ServerApi.Hooks.ServerLeave.Deregister(this, OnPlayerLeave);

                PlayerHooks.PlayerCommand -= OnPlayerCommand;

                //PlayerHooks.PlayerChat -= OnPlayerChat;
                ServerApi.Hooks.ServerChat.Deregister(this, OnChatReceived);

                //=====================game=====================
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);

                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);

                GetDataHandlers.TileEdit -= OnTileEdit;

                GetDataHandlers.PlaceObject += OnPlaceObject;

                GetDataHandlers.PaintTile += OnPaintTile;

                GetDataHandlers.PaintWall += OnPaintWall;

                GetDataHandlers.MassWireOperation += OnMassWireOperation;

                GetDataHandlers.LiquidSet -= HandleLiquidInteraction;

                GetDataHandlers.NewProjectile -= OnNewProjectile;

                GetDataHandlers.HealOtherPlayer -= OnHealOtherPlayer;

                ServerApi.Hooks.NpcSpawn.Deregister(this, OnNPCSpawn);

                ServerApi.Hooks.NpcKilled.Deregister(this, OnNPCKilled);

                ServerApi.Hooks.NpcAIUpdate.Deregister(this, OnNPCAIUpdate);

                ServerApi.Hooks.ProjectileAIUpdate.Deregister(this, OnProjectileAIUpdate);

                //=====================Server===================
                //ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);

                ServerApi.Hooks.ServerBroadcast.Deregister(this, OnServerBroadcast);

                ServerApi.Hooks.WorldSave.Deregister(this, OnWorldSave);

                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnServerStart);

                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region [ Get Latest Version ]

        public async Task InformLatestVersion()
        {
            var http = HttpWebRequest.CreateHttp("https://raw.githubusercontent.com/Nightklpgaming/TShock-GSKLP-Moderation/master/version.txt");

            WebResponse res = await http.GetResponseAsync();

            using (StreamReader sr = new StreamReader(res.GetResponseStream()))
            {
                System.Version latestversion = new(sr.ReadToEnd());

                if (latestversion > Version)
                {
                    MKLP_Console.SendLog_LatestVersion(Version.ToString(), latestversion.ToString());
                }

                return;
            }
        }

        #endregion

        #region [ Events ]

        #region =={[ On Get Data ]}==

        Item[] prevItemDrops = new Item[401];
        public static Dictionary<int, Item> newitemDrops = new();
        //static Item[] lostitemDrops = null;
        private async void OnGetData(GetDataEventArgs args)
        {
            if (args.Handled)
                return;

            var player = TShock.Players[args.Msg.whoAmI];

            if (args.MsgID == PacketTypes.ForceItemIntoNearestChest)
            {
                if ((bool)Config.ManagePackets.Disable_Packet85_QuickStackChest)
                {
                    args.Handled = true;
                }
            }

            if (args.MsgID == PacketTypes.SyncExtraValue)
            {
                if ((bool)Config.ManagePackets.Disable_Packet92_MobPickupCoin)
                {
                    args.Handled = true;
                }
            }

            #region [ ContinueConnecting2 ]

            if (args.MsgID != PacketTypes.ContinueConnecting2)
            {
                if ((bool)Config.BanGuard.UsingBanGuard)
                {
                    if (player == null || player.State > 1) return;

                    args.Handled = true;
                    int prevState = player.State;
                    player.State = 0;

                    bool isBanned = await BanGuardAPI.CheckPlayerBan(player.UUID, player.Name, player.IP) ?? false;

                    if (isBanned)
                    {
                        player.Disconnect("You are banned on the BanGuard network.\nVisit https://banguard.uk for more details.");
                    }
                    else
                    {
                        args.Handled = false;
                        player.State = prevState + 1;
                        player.SendData(PacketTypes.WorldInfo);
                    }
                }
            }

            #endregion

            #region [ latency ]

            if (args.MsgID == PacketTypes.ItemOwner)
            {
                
                if (player.ContainsData("MKLP_StartGetLatency"))
                {
                    player.SetData("MKLP_GetLatency", (DateTime.UtcNow - player.GetData<DateTime>("MKLP_StartGetLatency")).TotalMilliseconds);
                    player.RemoveData("MKLP_StartGetLatency");
                }
                
                /*
                var user = TShock.Players[args.Msg.whoAmI];
                if (user == null) return;
                using (BinaryReader date = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
                {
                    int iid = date.ReadInt16();
                    int pid = date.ReadByte();
                    if (pid != 255) return;
                    var pingresponse = PlayerPing[args.Msg.whoAmI];
                    var ping = pingresponse?.RecentPings[iid];
                    if (ping != null)
                    {
                        ping.End = DateTime.Now;
                        ping.Channel!.Writer.TryWrite(iid);
                    }
                }
                */
            }
            
            #endregion

            #region [ Disable ]
            if (player.ContainsData("MKLP_IsDisabled"))
            {
                if (!player.GetData<bool>("MKLP_IsDisabled"))
                    return;

                if (player == null || !player.Active || player.Dead)
                    return;

                if (args.MsgID == PacketTypes.PlayerSlot ||
                    args.MsgID == PacketTypes.PlayerUpdate ||
                    args.MsgID == PacketTypes.ItemOwner ||
                    args.MsgID == PacketTypes.ClientSyncedInventory)
                    return;
                
                if (TShockAPI.Utils.Distance(value2: new Vector2((int)player.TPlayer.position.X / 16, (int)player.TPlayer.position.Y / 16), value1: new Vector2(Main.spawnTileX, Main.spawnTileY)) >= 3f)
                {
                    player.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);
                }
                player.SetBuff(149, 330, true);

                // Prevent the packet from being processed
                args.Handled = true;
                return;
            }
            #endregion

            #region [ Vanish ]

            if (args.MsgID == PacketTypes.PlayerActive)
            {
                if (player.ContainsData("MKLP_Vanish"))
                {
                    if (player.GetData<bool>("MKLP_Vanish"))
                    {
                        foreach (TSPlayer gplayer in TShock.Players)
                        {
                            if (gplayer == null) continue;
                            if (gplayer == player) continue;
                            gplayer.SendData(PacketTypes.PlayerActive, null, gplayer.Index, false.GetHashCode());
                        }
                    }
                }
            }

            if (args.MsgID == PacketTypes.PlayerDeathV2)
            {
                try
                {
                    exe1();
                    async void exe1()
                    {
                        await exe2();

                        async Task exe2()
                        {
                            if (!player.ContainsData("MKLP_Vanish")) return;

                            if (!player.GetData<bool>("MKLP_Vanish")) return;

                            while (player.Dead) { }
                            while (player.TPlayer.dead) { }
                            while (!player.Active) { }
                            while (!player.TPlayer.active) { }

                            if ((bool)Config.Main.Use_VanishCMD_TPlayer_Active_Var)
                            {
                                player.TPlayer.active = false;
                            }

                            for (int i = 0; i < 10; i++)
                            {
                                foreach (TSPlayer gplayer in TShock.Players)
                                {
                                    if (gplayer == null) continue;
                                    if (gplayer == player) continue;

                                    gplayer.SendData(PacketTypes.PlayerActive, null, player.Index, false.GetHashCode());
                                }
                                await Task.Delay(1000);
                            }

                        }
                    }
                } catch { }
            }

            #endregion

            #region ( Inventory Checking )
            if (args.MsgID == PacketTypes.PlayerSlot)
            {

                if (!player.IsLoggedIn) return;

                if ((bool)Config.Main.Use_OnUpdate_Func) return;

                if ((bool)Config.Main.DetectAllPlayerInv)
                {
                    if (player.ContainsData("MKLP_PrevInventory") &&
                    player.ContainsData("MKLP_PrevPiggyBank") &&
                    player.ContainsData("MKLP_PrevSafe") &&
                    player.ContainsData("MKLP_PrevDefenderForge") &&
                    player.ContainsData("MKLP_PrevVoidVault"))
                    {
                        ManagePlayer.CheckPlayerInventory(player,
                            player.GetData<Item[]>("MKLP_PrevInventory"),
                            player.GetData<Item[]>("MKLP_PrevPiggyBank"),
                            player.GetData<Item[]>("MKLP_PrevSafe"),
                            player.GetData<Item[]>("MKLP_PrevDefenderForge"),
                            player.GetData<Item[]>("MKLP_PrevVoidVault"));
                        /*
                        if (Main.chest[player.ActiveChest] != null)
                        {
                            if (player.ActiveChest != -1) player.SetData("MKLP_PrevChestOpen", Main.chest[player.ActiveChest].item.Clone());
                        }
                        */
                        player.SetData("MKLP_PrevInventory", player.TPlayer.inventory.Clone());
                        player.SetData("MKLP_PrevPiggyBank", player.TPlayer.bank.item.Clone());
                        player.SetData("MKLP_PrevSafe", player.TPlayer.bank2.item.Clone());
                        player.SetData("MKLP_PrevDefenderForge", player.TPlayer.bank3.item.Clone());
                        player.SetData("MKLP_PrevVoidVault", player.TPlayer.bank4.item.Clone());
                    }
                    else
                    {
                        player.SetData("MKLP_PrevInventory", player.TPlayer.inventory.Clone());
                        player.SetData("MKLP_PrevPiggyBank", player.TPlayer.bank.item.Clone());
                        player.SetData("MKLP_PrevSafe", player.TPlayer.bank2.item.Clone());
                        player.SetData("MKLP_PrevDefenderForge", player.TPlayer.bank3.item.Clone());
                        player.SetData("MKLP_PrevVoidVault", player.TPlayer.bank4.item.Clone());
                    }
                } else
                {
                    if (player.ContainsData("MKLP_PrevInventory"))
                    {

                        ManagePlayer.CheckPlayerInventory(player,
                            player.GetData<Item[]>("MKLP_PrevInventory"));

                        player.SetData("MKLP_PrevInventory", player.TPlayer.inventory.Clone());
                    }
                    else
                    {
                        player.SetData("MKLP_PrevInventory", player.TPlayer.inventory.Clone());
                    }
                }
                
            }
            #endregion

            #region { spawn boss/invasion }
            if (args.MsgID == PacketTypes.SpawnBossorInvasion)
            {
                using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length - 1))
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
            #endregion

            #region { Ping Map }

            if (args.MsgID == PacketTypes.LoadNetModule)
            {
                using (var stream = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
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

        #region [ OnGameUpdate ]

        private void OnGameUpdate(EventArgs args)
        {
            if (!(bool)Config.Main.Use_OnUpdate_Func) return;
            /*
            //List<Item> newitemget = new();

            for (int i = 0; i < Main.item.Count(); i++)
            {
                if (Main.item[i] == null) continue;
                if (prevItemDrops[i] == null)
                {
                    AddDItemList(i, Main.item[i]);
                    continue;
                }

                if (!Main.item[i].active && !prevItemDrops[i].active) continue;

                if (Main.item[i].active && !prevItemDrops[i].active)
                {
                    AddDItemList(i, Main.item[i]);
                    continue;
                }
                if (Main.item[i].stack > prevItemDrops[i].stack)
                {
                    Item item = Main.item[i];
                    item.stack -= prevItemDrops[i].stack;
                    AddDItemList(i, Main.item[i]);
                    continue;
                }
            }
            prevItemDrops = (Item[])Main.item.Clone();

            void AddDItemList(int index, Item TItem)
            {
                if (!newitemDrops.ContainsKey(index))
                {
                    newitemDrops.Add(index, TItem);
                } else
                {
                    newitemDrops[index] = TItem;
                }
            }
            */

            int maxvalue = 10;

            if (Main.hardMode) maxvalue = 100;

            if ((bool)Config.Main.AutoClear_IllegalItemDrops_SurvivalCode1 || (bool)Config.Main.AutoClear_IllegalItemDrops_MainCode1)
            {
                for (int i = 0; i < Main.maxItems; i++)
                {
                    if (IllegalItemProgression.ContainsKey(Main.item[i].netID) &&
                        (bool)MKLP.Config.Main.Using_Survival_Code1 && (bool)Config.Main.AutoClear_IllegalItemDrops_SurvivalCode1 && Main.item[i].active)
                    {
                        Main.item[i].active = false;
                        TSPlayer.All.SendData(PacketTypes.ItemDrop, "", i);
                    }
                    if ((Main.item[i].value * Main.item[i].stack) / 5000000 >= maxvalue &&
                        Main.item[i].netID != 74
                        && (bool)MKLP.Config.Main.Using_Main_Code1 && (bool)Config.Main.AutoClear_IllegalItemDrops_MainCode1 && Main.item[i].active)
                    {
                        Main.item[i].active = false;
                        TSPlayer.All.SendData(PacketTypes.ItemDrop, "", i);
                    }
                }
            }

            if (checklatency_interval < DateTime.UtcNow)
            {
                checklatency_interval.AddSeconds(5);
                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null) continue;
                    DBManager.CheckPlayerMute(player, true);
                    player.SetData("MKLP_StartGetLatency", DateTime.UtcNow);
                    NetMessage.SendData((int)PacketTypes.ItemOwner, player.Index, -1, null, 0, player.Index);
                }
            }

            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;
                if (player.ContainsData("MKLP_TargetSpy"))
                {
                    player.TPlayer.position = player.GetData<TSPlayer>("MKLP_TargetSpy").TPlayer.position;
                    player.SendData(PacketTypes.PlayerUpdate, "", player.Index);

                    player.SetBuff(BuffID.Invisibility, 15 * 60);
                    player.SetBuff(BuffID.ObsidianSkin, 20 * 60);
                    player.SetBuff(BuffID.Webbed, 10 * 60);

                }
            }

            #region inventory checking
            foreach (var player in TShock.Players)
            {
                if (player == null) continue;
                if ((bool)Config.Main.DetectAllPlayerInv)
                {
                    if (player.ContainsData("MKLP_PrevInventory") &&
                    player.ContainsData("MKLP_PrevPiggyBank") &&
                    player.ContainsData("MKLP_PrevSafe") &&
                    player.ContainsData("MKLP_PrevDefenderForge") &&
                    player.ContainsData("MKLP_PrevVoidVault"))
                    {
                        ManagePlayer.CheckPlayerInventory(player,
                            player.GetData<Item[]>("MKLP_PrevInventory"),
                            player.GetData<Item[]>("MKLP_PrevPiggyBank"),
                            player.GetData<Item[]>("MKLP_PrevSafe"),
                            player.GetData<Item[]>("MKLP_PrevDefenderForge"),
                            player.GetData<Item[]>("MKLP_PrevVoidVault"));
                        /*
                        if (player.ActiveChest != -1)
                        {
                            try
                            {
                                List<Item> chestitem = new();

                                foreach (Item item in Main.chest[player.ActiveChest].item)
                                {
                                    chestitem.Add(item);
                                }
                                //player.SetData("MKLP_PrevChestOpen", Main.chest[player.ActiveChest].item);
                                player.SetData("MKLP_PrevChestOpen", chestitem.ToArray());
                            }
                            catch { }
                        }
                        */
                        player.SetData("MKLP_PrevInventory", player.TPlayer.inventory.Clone());
                        player.SetData("MKLP_PrevPiggyBank", player.TPlayer.bank.item.Clone());
                        player.SetData("MKLP_PrevSafe", player.TPlayer.bank2.item.Clone());
                        player.SetData("MKLP_PrevDefenderForge", player.TPlayer.bank3.item.Clone());
                        player.SetData("MKLP_PrevVoidVault", player.TPlayer.bank4.item.Clone());
                    }
                    else
                    {
                        /*
                        if (player.ActiveChest != -1)
                        {
                            try
                            {
                                List<Item> chestitem = new();

                                foreach (Item item in Main.chest[player.ActiveChest].item)
                                {
                                    chestitem.Add(item);
                                }
                                //player.SetData("MKLP_PrevChestOpen", Main.chest[player.ActiveChest].item);
                                player.SetData("MKLP_PrevChestOpen", chestitem.ToArray());
                            } catch { }
                        }
                        */
                        player.SetData("MKLP_PrevInventory", player.TPlayer.inventory.Clone());
                        player.SetData("MKLP_PrevPiggyBank", player.TPlayer.bank.item.Clone());
                        player.SetData("MKLP_PrevSafe", player.TPlayer.bank2.item.Clone());
                        player.SetData("MKLP_PrevDefenderForge", player.TPlayer.bank3.item.Clone());
                        player.SetData("MKLP_PrevVoidVault", player.TPlayer.bank4.item.Clone());
                    }
                }
                else
                {
                    if (player.ContainsData("MKLP_PrevInventory"))
                    {

                        ManagePlayer.CheckPlayerInventory(player,
                            player.GetData<Item[]>("MKLP_PrevInventory"),
                            null,
                            null,
                            null,
                            null);

                        player.SetData("MKLP_PrevInventory", player.TPlayer.inventory.Clone());
                    }
                    else
                    {
                        player.SetData("MKLP_PrevInventory", player.TPlayer.inventory.Clone());
                    }
                }
            }

            #endregion
        }

        #endregion

        #region { Player }

        bool LockDown = false;
        string LockDownReason = "";

        /*
        private PingData[] PlayerPing { get; set; }
        public class PingData
        {
            public TimeSpan? LastPing;
            internal PingDetails?[] RecentPings = new PingDetails?[Terraria.Main.item.Length];
        }
        internal class PingDetails
        {
            internal Channel<int>? Channel;
            internal DateTime Start = DateTime.Now;
            internal DateTime? End = null;
        }

        public async Task<TimeSpan> Ping(TSPlayer player)
        {
            return await Ping(player, new CancellationTokenSource(1000).Token);
        }

        public async Task<TimeSpan> Ping(TSPlayer player, CancellationToken token)
        {
            var pingdata = PlayerPing[player.Index];
            if (pingdata == null) return TimeSpan.MaxValue;

            var inv = -1;
            for (var i = 0; i < Terraria.Main.item.Length; i++)
                if (Terraria.Main.item[i] != null)
                    if (!Terraria.Main.item[i].active || Terraria.Main.item[i].playerIndexTheItemIsReservedFor == 255)
                    {
                        if (pingdata.RecentPings[i]?.Channel == null)
                        {
                            inv = i;
                            break;
                        }
                    }

            if (inv == -1) return TimeSpan.MaxValue;

            var pd = pingdata.RecentPings[inv] ??= new PingDetails();

            pd.Channel ??= Channel.CreateBounded<int>(new BoundedChannelOptions(30)
            {
                SingleReader = true,
                SingleWriter = true
            });


            Terraria.NetMessage.TrySendData((int)PacketTypes.RemoveItemOwner, player.Index, -1, null, inv);

            await pd.Channel.Reader.ReadAsync(token);
            pd.Channel = null;

            return (pingdata.LastPing = pd.End!.Value - pd.Start).Value;
        }
        */

        /*
        private void Hook_Ping_GetData(GetDataEventArgs args)
        {
            if (args.Handled) return;
            if (args.MsgID != PacketTypes.ItemOwner) return;
            var user = TShock.Players[args.Msg.whoAmI];
            if (user == null) return;
            using (BinaryReader date = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
            {
                int iid = date.ReadInt16();
                int pid = date.ReadByte();
                if (pid != 255) return;
                var pingresponse = PlayerPing[args.Msg.whoAmI];
                var ping = pingresponse?.RecentPings[iid];
                if (ping != null)
                {
                    ping.End = DateTime.Now;
                    ping.Channel!.Writer.TryWrite(iid);
                }
            }
        }
        */

        DateTime checklatency_interval = DateTime.MinValue;

        private void OnPlayerUpdate(object? sender, GetDataHandlers.PlayerUpdateEventArgs args)
        {
            #region code
            if ((bool)Config.Main.Use_OnUpdate_Func) return;

            if ((DateTime.UtcNow - checklatency_interval).TotalSeconds >= 5)
            {
                checklatency_interval = DateTime.UtcNow;

                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null) continue;
                    player.SetData("MKLP_StartGetLatency", DateTime.UtcNow);
                    NetMessage.SendData((int)PacketTypes.ItemOwner, player.Index, -1, null, 0, player.Index);
                    //player.SetData("MKLP_GetLatency", Ping(player).Result.TotalMilliseconds);
                }
            }

            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;
                if (player.ContainsData("MKLP_TargetSpy"))
                {
                    if (player.GetData<TSPlayer>("MKLP_TargetSpy") == args.Player)
                    {
                        player.TPlayer.position = args.Player.TPlayer.position;
                        player.SendData(PacketTypes.PlayerUpdate, "", player.Index);

                        player.SetBuff(BuffID.Invisibility, 15 * 60);
                        player.SetBuff(BuffID.ObsidianSkin, 20 * 60);
                        player.SetBuff(BuffID.Webbed, 10 * 60);

                    }
                }
            }

            #endregion
        }

        private void OnPlayerJoin(JoinEventArgs args)
        {
            #region code
            
            IllegalItemProgression = SurvivalManager.GetIllegalItem();

            IllegalProjectileProgression = SurvivalManager.GetIllegalProjectile();

            IllegalTileProgression = SurvivalManager.GetIllegalTile();

            IllegalWallProgression = SurvivalManager.GetIllegalWall();

            var player = TShock.Players[args.Who];
            if (player != null)
            {
                #region lockdown

                if (LockDown)
                {
                    if (LockDownReason == "")
                    {
                        player.Disconnect("You cannot join the server yet!");
                    } else
                    {
                        player.Disconnect("You cannot join the server by the reason of " + LockDownReason);
                    }
                    return;
                }

                #endregion

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
                foreach (string contains in Config.Main.Ban_NameContains)
                {
                    if (player.Name.Contains(contains))
                    {
                        player.Disconnect($"You Can't use {contains} in your Name!");
                        return;
                    }
                }
                if (player.Name.Length < (byte)Config.Main.Minimum_CharacterName)
                {
                    player.Disconnect($"You're Character Name has less than {(byte)Config.Main.Minimum_CharacterName}");
                    return;
                }
                if (player.Name.Length > (byte)Config.Main.Maximum_CharacterName)
                {
                    player.Disconnect($"You're Character Name has more than {(byte)Config.Main.Maximum_CharacterName}");
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

                #region Boss Is Present

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (!(bool)Config.BossManager.AllowJoinDuringBoss && Main.npc[i].active && Main.npc[i].boss)
                    {
                        player.Disconnect("The current in-game players must defeat the current boss\nBefore you can join.");
                        return;
                    }
                }

                #endregion

                #region UUID Match [ alt-acc prevention ]

                var getuuidmatch = GetMatchUUID_UserAccount(player.Name, player.UUID);
                UserAccount useraccount = TShock.UserAccounts.GetUserAccountByName(player.Name);
                if (getuuidmatch.Count != 0 && !(bool)Config.Main.Allow_User_JoinMatchUUID && useraccount == null)
                {
                    bool whitelisted = false;
                    try
                    {
                        foreach (var check in (WhiteListAlt[])Config.Main.WhiteList_User_JoinMatchUUID)
                        {
                            if (check.MainName == player.Name || check.AltNames.Contains(player.Name))
                            {
                                whitelisted = true;
                            }
                        }
                    } catch { }

                    if ((bool)Config.Main.Target_UserMatchUUIDAndIP && !whitelisted)
                    {
                        foreach (UserAccount get in getuuidmatch)
                        {
                            if (JsonConvert.DeserializeObject<List<string>>(get.KnownIps).Contains(player.IP))
                            {
                                player.Disconnect("( UUID|IP ) making 2 or more accounts is forbidden!");
                                return;
                            }
                        }
                    } else if (!whitelisted)
                    {
                        player.Disconnect("( UUID ) making 2 or more accounts is forbidden!");
                        return;
                    }
                }

                #endregion

                foreach (var check in DisabledKey)
                {
                    if (check.Key == Identifier.Name + player.Name ||
                        check.Key == Identifier.IP + player.IP ||
                        check.Key == Identifier.UUID + player.UUID)
                    {
                        player.SetData("MKLP_IsDisabled", true);
                        player.SendErrorMessage("Your still disabled" +
                            "\nReason : " + check.Value);
                    }
                }

                if (DBManager.CheckPlayerMute(player, true))
                {
                    player.SendErrorMessage("You're still muted!");
                }

                foreach (TSPlayer gplayer in TShock.Players)
                {
                    if (gplayer == null) continue;
                    if (gplayer == player) continue;
                    if (gplayer.ContainsData("MKLP_Vanish"))
                    {
                        if (gplayer.GetData<bool>("MKLP_Vanish"))
                        {
                            player.SendData(PacketTypes.PlayerActive, null, gplayer.Index, false.GetHashCode());
                        }
                    }
                }
            }

            bool HasSymbols(string Name)
            {
                foreach (char remove in Config.Main.WhiteList_PlayerName_Symbols)
                {
                    Name.Replace($"{remove}", "");
                }
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

        private void OnPlayerLeave(LeaveEventArgs args)
        {
            #region code
            var godPower = Terraria.GameContent.Creative.CreativePowerManager.Instance.GetPower<Terraria.GameContent.Creative.CreativePowers.GodmodePower>();

            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;
                if (player.ContainsData("MKLP_TargetSpy"))
                {
                    if (player.GetData<TSPlayer>("MKLP_TargetSpy") == TShock.Players[args.Who])
                    {
                        player.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);

                        godPower.SetEnabledState(player.Index, false);

                        TogglePlayerVanish(player, false);

                        player.RemoveData("MKLP_TargetSpy");

                        player.SendInfoMessage($"You're no longer spying on someone");
                    }
                }
            }

            #endregion
        }

        bool LockDownRegister = false;
        private void OnPlayerCommand(PlayerCommandEventArgs args)
        {
            #region code
            if (args.Handled || args.Player == null)
                return;

            Command command = args.CommandList.FirstOrDefault();

            if (command == null)
                return;

            if (!args.Player.RealPlayer)
            {
                if (command.Name == "register" ||
                command.Name == "login" ||
                command.Name == "password")
                {
                    return;
                }

                if (Config.Discord.CommandLogChannel == null) return;
                if (command.CanRun(args.Player))
                {
                    /*
                    Discordklp.KLPBotSendMessageLog((ulong)Config.Discord.CommandLogChannel, $"(Not in a Server) Named **{args.Player.Name}** ✅Executed" +
                        $" `/{command.Name}{((string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count) == "") ? "" : " ")}{string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count)}`");
                    */
                    Discordklp.KLPBotSendMessageLog((ulong)Config.Discord.CommandLogChannel, $"(Not in a Server) Named **{args.Player.Name}** ✅Executed" +
                        $" `{args.CommandPrefix}{args.CommandText}`");
                }
                else
                {
                    /*
                    Discordklp.KLPBotSendMessageLog((ulong)Config.Discord.CommandLogChannel, $"(Not in a Server) Named **{args.Player.Name}** ⛔Tried" +
                        $" `/{command.Name}{((string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count) == "") ? "" : " ")}{string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count)}`");
                    */
                    Discordklp.KLPBotSendMessageLog((ulong)Config.Discord.CommandLogChannel, $"(Not in a Server) Named **{args.Player.Name}** ⛔Tried" +
                        $" `{args.CommandPrefix}{args.CommandText}`");
                }
                return;
            }


            if (DisabledKey.ContainsKey(Identifier.Name + args.Player.Name) ||
                DisabledKey.ContainsKey(Identifier.IP + args.Player.IP) ||
                DisabledKey.ContainsKey(Identifier.UUID + args.Player.UUID))
            {
                if (command.Name == "register" ||
                    command.Name == "login")
                {
                    args.Player.SendErrorMessage("You're currently Disabled!" +
                    "\n you cannot perform this command");
                    args.Handled = true;
                    return;
                }
            }

            if (command.Name == "register" && LockDownRegister)
            {
                args.Player.SendErrorMessage("You do not have permission to register at the moment");
                args.Handled = true;
                return;
            }

            if (command.Name == "register" ||
                command.Name == "login" ||
                command.Name == "password")
            {
                return;
            }
            
            if (Config.Discord.CommandLogChannel == null) return;
            if (args.Player.IsLoggedIn)
            {
                if (command.CanRun(args.Player))
                {
                    /*
                    Discordklp.KLPBotSendMessageLog((ulong)Config.Discord.CommandLogChannel, $"Player **{args.Player.Account.Name}** ✅Executed" +
                        $" `/{command.Name}{((string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count) == "") ? "" : " ")}{string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count)}`");
                    */
                    Discordklp.KLPBotSendMessageLog((ulong)Config.Discord.CommandLogChannel, $"Player **{args.Player.Account.Name}** ✅Executed" +
                        $" `{args.CommandPrefix}{args.CommandText}`");
                }
                else
                {
                    /*
                    Discordklp.KLPBotSendMessageLog((ulong)Config.Discord.CommandLogChannel, $"Player **{args.Player.Account.Name}** ⛔Tried" +
                        $" `/{command.Name}{((string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count) == "") ? "" : " ")}{string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count)}`");
                    */
                    Discordklp.KLPBotSendMessageLog((ulong)Config.Discord.CommandLogChannel, $"Player **{args.Player.Account.Name}** ⛔Tried" +
                        $" `{args.CommandPrefix}{args.CommandText}`");
                }
            } else
            {

                if (command.CanRun(args.Player))
                {
                    /*
                    Discordklp.KLPBotSendMessageLog((ulong)Config.Discord.CommandLogChannel, $"Player **{args.Player.Account.Name}** ✅Executed" +
                        $" `/{command.Name}{((string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count) == "") ? "" : " ")}{string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count)}`");
                    */
                    Discordklp.KLPBotSendMessageLog((ulong)Config.Discord.CommandLogChannel, $"(Not Logged In) Player **{args.Player.Name}** ✅Executed" +
                        $" `{args.CommandPrefix}{args.CommandText}`");
                }
                else
                {
                    /*
                    Discordklp.KLPBotSendMessageLog((ulong)Config.Discord.CommandLogChannel, $"Player **{args.Player.Account.Name}** ⛔Tried" +
                        $" `/{command.Name}{((string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count) == "") ? "" : " ")}{string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count)}`");
                    */
                    Discordklp.KLPBotSendMessageLog((ulong)Config.Discord.CommandLogChannel, $"(Not Logged In) Player **{args.Player.Name}** ⛔Tried" +
                        $" `{args.CommandPrefix}{args.CommandText}`");
                }
            }
            #endregion
        }

        int NumberOfMutedPlayers = 0;
        public struct PlayerMessageThreshold
        {
            public int Threshold;
            public DateTime Since;

            public PlayerMessageThreshold(int Threshold, DateTime Since)
            {
                this.Threshold = Threshold;
                this.Since = Since;
            }
        }
        private async void OnChatReceived(ServerChatEventArgs args)
        {
            #region code

            if (!(bool)Config.ChatMod.Using_Chat_AutoMod) return;

            TSPlayer player = TShock.Players[args.Who];

            foreach (string banned in Config.ChatMod.Ban_MessageContains)
            {
                if (args.Text.ToLower().Contains(banned.ToLower()))
                {
                    player.SendErrorMessage("You can not send that message!");
                    args.Handled = true;
                    return;
                }
            }
            if (args.Text.Length >= (int)Config.ChatMod.Maximum__MessageLength_NoSpace && !args.Text.Contains(" "))
            {
                player.SendErrorMessage("You can not send that message!");
                args.Handled = true;
                return;
            }

            if (args.Text.Length >= (int)Config.ChatMod.Maximum__MessageLength_WithSpace)
            {
                player.SendErrorMessage("You can not send that message!");
                args.Handled = true;
                return;
            }

            if (args.Text.Length >= (int)Config.ChatMod.Maximum_Spammed_MessageLength_NoSpace && !args.Text.Contains(" "))
            {
                if (player.ContainsData("MKLP_Chat_Spam_message1"))
                {
                    if ((DateTime.UtcNow - player.GetData<PlayerMessageThreshold>("MKLP_Chat_Spam_message1").Since).TotalMilliseconds < (int)Config.ChatMod.Millisecond_Threshold)
                    {
                        if (player.GetData<PlayerMessageThreshold>("MKLP_Chat_Spam_message1").Threshold >= (int)Config.ChatMod.Threshold_Spammed_MessageLength_NoSpace)
                        {
                            SendWarning();
                            return;
                        }

                        player.SetData("MKLP_Chat_Spam_message1", new PlayerMessageThreshold(player.GetData<PlayerMessageThreshold>("MKLP_Chat_Spam_message1").Threshold + 1, DateTime.UtcNow));
                        
                    } else
                    {
                        player.SetData("MKLP_Chat_Spam_message1", new PlayerMessageThreshold(0, DateTime.UtcNow));
                    }
                } else
                {
                    player.SetData("MKLP_Chat_Spam_message1", new PlayerMessageThreshold(1, DateTime.UtcNow));
                }
            }

            if (args.Text.Length >= (int)Config.ChatMod.Maximum_Spammed_MessageLength_WithSpace)
            {
                if (player.ContainsData("MKLP_Chat_Spam_message2"))
                {
                    if ((DateTime.UtcNow - player.GetData<PlayerMessageThreshold>("MKLP_Chat_Spam_message2").Since).TotalMilliseconds < (int)Config.ChatMod.Millisecond_Threshold)
                    {
                        if (player.GetData<PlayerMessageThreshold>("MKLP_Chat_Spam_message2").Threshold >= (int)Config.ChatMod.Threshold_Spammed_MessageLength_WithSpace)
                        {
                            SendWarning();
                            return;
                        }

                        player.SetData("MKLP_Chat_Spam_message2", new PlayerMessageThreshold(player.GetData<PlayerMessageThreshold>("MKLP_Chat_Spam_message2").Threshold + 1, DateTime.UtcNow));

                    }
                    else
                    {
                        player.SetData("MKLP_Chat_Spam_message2", new PlayerMessageThreshold(0, DateTime.UtcNow));
                    }
                }
                else
                {
                    player.SetData("MKLP_Chat_Spam_message2", new PlayerMessageThreshold(1, DateTime.UtcNow));
                }
            }

            void SendWarning()
            {
                if (player.ContainsData("MKLP_Chat_Warning_message"))
                {
                    player.SetData("MKLP_Chat_Warning_message", player.GetData<int>("MKLP_Chat_Warning_message") + 1);
                } else
                {
                    player.SetData("MKLP_Chat_Warning_message", 1);
                }

                if (player.GetData<int>("MKLP_Chat_Warning_message") >= (int)Config.ChatMod.MutePlayer_AtWarning)
                {
                    if ((bool)Config.ChatMod.PermanentDuration)
                    {
                        ManagePlayer.OnlineMute(false, player, "Spamming/Flooding Messages", "(Auto Chat Mod)", DateTime.MaxValue);
                    } else
                    {
                        ManagePlayer.OnlineMute(false, player, "Spamming/Flooding Messages", "(Auto Chat Mod)", DateTime.UtcNow.AddSeconds((int)Config.ChatMod.MuteDuration_Seconds));
                    }
                    NumberOfMutedPlayers++;
                    args.Handled = true;

                    if ((bool)Config.ChatMod.EnableLockDown_When_MultipleMutes)
                    {
                        if (NumberOfMutedPlayers >= (int)Config.ChatMod.NumberOFPlayersAutoMute_Lockdown)
                        {
                            Discordklp.KLPBotSendMessageMainLog("Server On 🔒LockDown🔒 Due to Multiple Player Mutes🔇!");
                            LockDown = true;
                            LockDownReason = Config.ChatMod.AutoLockDown_Reason;
                        }
                    }

                    return;
                }

                player.SendWarningMessage("Warning! please do not spam/flood the messages!");
                args.Handled = true;
                return;
            }
            #endregion
        }

        #endregion

        #region { Game }

        private void OnTileEdit(object? sender, GetDataHandlers.TileEditEventArgs args)
        {
            #region code
            int tileX = args.X;
            int tileY = args.Y;

            if (KillThreshold()) return;
            if (PlaceThreshold()) return;

            #region [ Breakable Tiles ]

            ushort[] breakableTiles =
            {
                //vines
                TileID.Plants,
                TileID.Plants2,
                TileID.AshPlants,
                TileID.CorruptPlants,
                TileID.CrimsonPlants,
                TileID.HallowedPlants,
                TileID.HallowedPlants2,
                TileID.JunglePlants,
                TileID.JunglePlants2,
                TileID.MushroomPlants,
                TileID.OasisPlants,

                //vines
                TileID.VineFlowers,
                TileID.Vines,
                TileID.AshVines,
                TileID.CorruptVines,
                TileID.CrimsonVines,
                TileID.HallowedVines,
                TileID.JungleVines,
                TileID.MushroomVines,

                //pots
                

                //misc
                TileID.Cobweb,
                TileID.Pigronata,

            };

            #endregion

            if (args.Action == GetDataHandlers.EditAction.PlaceTile || args.Action == GetDataHandlers.EditAction.ReplaceTile)
            {
                if (IllegalTileProgression.ContainsKey(new(Main.tile[tileX, tileY].type, args.Style)) && !args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_3) && (bool)Config.Main.Using_Survival_Code3)
                {
                    if (PunishPlayer(MKLP_CodeType.Survival, 3, args.Player, $"{IllegalTileProgression[new(Main.tile[tileX, tileY].type, args.Style)]} Block Place", $"Player **{args.Player.Name}** has placed illegal tile progression `tile id: {Main.tile[tileX, tileY].type} style: {args.Style}` **{IllegalTileProgression[new(Main.tile[tileX, tileY].type, args.Style)]}**"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return;
                    }
                }
                if (IllegalTileProgression.ContainsKey(new(Main.tile[tileX, tileY].type, 0, true)) && !args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_3) && (bool)Config.Main.Using_Survival_Code3)
                {
                    if (PunishPlayer(MKLP_CodeType.Survival, 3, args.Player, $"{IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]} Block Place", $"Player **{args.Player.Name}** has placed illegal tile progression `tile id: {Main.tile[tileX, tileY].type} style: {args.Style}` **{IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]}**"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return;
                    }
                }
            }

            if (args.Action == GetDataHandlers.EditAction.PlaceWall)
            {
                if (IllegalWallProgression.ContainsKey(Main.tile[tileX, tileY].wall) &&
                    !args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_4) &&
                    (bool)Config.Main.Using_Survival_Code4)
                {
                    if (PunishPlayer(MKLP_CodeType.Survival, 4, args.Player, $"{IllegalWallProgression[Main.tile[tileX, tileY].wall]} Wall Place", $"Player **{args.Player.Name}** has placed illegal wall progression `wall id:{Main.tile[tileX, tileY].wall}` **{IllegalWallProgression[Main.tile[tileX, tileY].wall]}**"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return;
                    }
                }
            }

            if (args.Action == GetDataHandlers.EditAction.KillTile ||
                args.Action == GetDataHandlers.EditAction.KillWall ||
                args.Action == GetDataHandlers.EditAction.TryKillTile)
            {
                if (tileY < (int)Main.worldSurface)
                {
                    if (args.Action == GetDataHandlers.EditAction.KillTile &&
                        breakableTiles.Contains(Main.tile[tileX, tileY].type)) return;


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
                    if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_protectsurface_place) && (bool)Config.Main.Using_AntiGrief_Surface_Place)
                    {
                        args.Player.SendErrorMessage(Config.Main.Message_AntiGrief_Surface_Place);
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return;
                    }
                }

                ushort[] infectionb =
                {
                    TileID.CorruptGrass,
                    TileID.CrimsonGrass,
                    TileID.HallowedGrass,
                    TileID.CorruptJungleGrass,
                    TileID.CrimsonJungleGrass,

                    TileID.CorruptPlants,
                    TileID.CrimsonPlants,
                    TileID.HallowedPlants,
                    TileID.HallowedPlants2,

                    TileID.CorruptVines,
                    TileID.CrimsonVines,
                    TileID.HallowedVines,

                    TileID.CorruptThorns,
                    TileID.CrimsonThorns,

                    TileID.Ebonstone,
                    TileID.Crimstone,
                    TileID.Pearlstone,

                    TileID.Ebonsand,
                    TileID.Crimsand,
                    TileID.Pearlsand,
                    TileID.CorruptHardenedSand,
                    TileID.CrimsonHardenedSand,
                    TileID.HallowHardenedSand,
                    TileID.CorruptSandstone,
                    TileID.CrimsonSandstone,
                    TileID.HallowSandstone,

                    TileID.CorruptIce,
                    TileID.FleshIce,
                    TileID.HallowedIce
                };

                if (infectionb.Contains(Main.tile[tileX, tileY].type) && !args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_infection) && (bool)Config.Main.Using_AntiGrief_Infection)
                {
                    args.Player.SendErrorMessage(Config.Main.Message_AntiGrief_Infection);
                    args.Player.SendTileSquareCentered(tileX, tileY, 4);
                    args.Handled = true;
                    return;
                }
            }

            if (!NPC.downedBoss3)
            {
                if ((args.Action == GetDataHandlers.EditAction.KillActuator ||
                    args.Action == GetDataHandlers.EditAction.PlaceActuator ||
                    args.Action == GetDataHandlers.EditAction.KillWire ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire ||
                    args.Action == GetDataHandlers.EditAction.KillWire2 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire2 ||
                    args.Action == GetDataHandlers.EditAction.KillWire3 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire3 ||
                    args.Action == GetDataHandlers.EditAction.KillWire4 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire4) &&
                    (
                    !args.Player.HasPermission(Config.Permissions.Ignore_IllegalWireProgression) &&
                    !args.Player.HasPermission(TShockAPI.Permissions.item) &&
                    !args.Player.HasPermission(TShockAPI.Permissions.give) &&
                    !args.Player.HasPermission(TShockAPI.Permissions.manageitem)
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

            if ((args.Action == GetDataHandlers.EditAction.KillActuator ||
                    args.Action == GetDataHandlers.EditAction.PlaceActuator ||
                    args.Action == GetDataHandlers.EditAction.KillWire ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire ||
                    args.Action == GetDataHandlers.EditAction.KillWire2 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire2 ||
                    args.Action == GetDataHandlers.EditAction.KillWire3 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire3 ||
                    args.Action == GetDataHandlers.EditAction.KillWire4 ||
                    args.Action == GetDataHandlers.EditAction.PlaceWire4 ) &&
                    tileY >= (int)Main.worldSurface &&
                    (bool)Config.Main.ReceivedWarning_WirePlaceUnderground
                    )
            {
                Discordklp.KLPBotSendMessageMainLog($"Player **{args.Player.Name}** Used Wire/Actuator below surface `{tileX}, {tileY}`");
                
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
                    if (PunishPlayer(MKLP_CodeType.Default, 1, args.Player, $"Breaking blocks to fast", $"Player **{args.Player.Name}** has exceeded TileKill Threshold `itemheld: {args.Player.SelectedItem.netID}` `Threshold: {max}`"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return true;
                    }
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
                    if (PunishPlayer(MKLP_CodeType.Default, 2, args.Player, $"Placing blocks too fast", $"Player **{args.Player.Name}** has exceeded TilePlace Threshold `itemheld: {args.Player.SelectedItem.netID}` `Threshold: {max}`"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return true;
                    }
                }

                return false;
            }
            #endregion

            

            #endregion
        }

        private void OnPlaceObject(object? sender, GetDataHandlers.PlaceObjectEventArgs args)
        {
            #region code

            int tileX = args.X;
            int tileY = args.Y;

            ushort Type = (ushort)args.Type;

            if (PlaceThreshold()) return;

            ushort[] SetupBastTile =
            {
                Terraria.ID.TileID.OpenDoor,
                Terraria.ID.TileID.ClosedDoor,
                Terraria.ID.TileID.TrapdoorOpen,
                Terraria.ID.TileID.TrapdoorClosed,
                Terraria.ID.TileID.Campfire
            };

            if ((bool)Config.Main.Prevent_Place_BastStatueNearDoor && SetupBastTile.Contains(Type))
            {
                if (PossibleTransmutationGlitch1())
                {
                    args.Player.SendErrorMessage("You cannot place 'Bast_Statue & Door/Campfire' near each other!");
                    args.Player.SendTileSquareCentered(tileX, tileY, 10);
                    args.Handled = true;
                    return;
                }
            }
            if ((bool)Config.Main.Prevent_Place_BastStatueNearDoor && Type == Terraria.ID.TileID.CatBast)
            {
                if (PossibleTransmutationGlitch2())
                {
                    args.Player.SendErrorMessage("You cannot place 'Bast_Statue & Door/Campfire' near each other!");
                    args.Player.SendTileSquareCentered(tileX, tileY, 10);
                    args.Handled = true;
                    return;
                }
            }

            if (IllegalTileProgression.ContainsKey(new(Main.tile[tileX, tileY].type, args.Style)) && !args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_3) && (bool)Config.Main.Using_Survival_Code3)
            {
                if (PunishPlayer(MKLP_CodeType.Survival, 3, args.Player, $"{IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]} Block Place", $"Player **{args.Player.Name}** has placed illegal tile progression `tile id: {Main.tile[tileX, tileY].type} style: {args.Style}` **{IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]}**"))
                {
                    args.Player.SendTileSquareCentered(tileX, tileY, 10);
                    args.Handled = true;
                    return;
                }
            }
            if (IllegalTileProgression.ContainsKey(new(Main.tile[tileX, tileY].type, 0, true)) && !args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_3) && (bool)Config.Main.Using_Survival_Code3)
            {
                if (PunishPlayer(MKLP_CodeType.Survival, 3, args.Player, $"{IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]} Block Place", $"Player **{args.Player.Name}** has placed illegal tile progression `tile id: {Main.tile[tileX, tileY].type} style: {args.Style}` **{IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]}**"))
                {
                    args.Player.SendTileSquareCentered(tileX, tileY, 10);
                    args.Handled = true;
                    return;
                }
            }

            #region ( door near at bast statue )

            bool PossibleTransmutationGlitch1()
            {
                for (int x = tileX - 8; x <= tileX + 8; x++)
                {
                    for (int y = tileY - 8; y <= tileY + 8; y++)
                    {
                        if (x == tileX && y == tileY)
                            continue;
                        if (Main.tile[x, y].type == Terraria.ID.TileID.CatBast) return true;
                    }
                }
                return false;
            }

            bool PossibleTransmutationGlitch2()
            {
                for (int x = tileX - 8; x <= tileX + 8; x++)
                {
                    for (int y = tileY - 8; y <= tileY + 8; y++)
                    {
                        if (x == tileX && y == tileY)
                            continue;
                        if (SetupBastTile.Contains(Main.tile[x, y].type))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            #endregion

            #region ( Threshold )
            
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
                    if (PunishPlayer(MKLP_CodeType.Default, 2, args.Player, $"Placing blocks to fast", $"Player **{args.Player.Name}** has exceeded TilePlace Threshold `itemheld: {args.Player.SelectedItem.netID}` `Threshold: {max}`"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return true;
                    }
                }

                return false;
            }

            #endregion

            #endregion
        }

        private void OnPaintTile(object? sender, GetDataHandlers.PaintTileEventArgs args)
        {
            #region code

            int tileX = args.X;
            int tileY = args.Y;

            if (PaintThreshold()) return;

            #region ( Threshold )

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
                    if (PunishPlayer(MKLP_CodeType.Default, 3, args.Player, $"Painting too fast", $"Player **{args.Player.Name}** has exceeded Paint Threshold `itemheld: {args.Player.SelectedItem.netID}` `Threshold {max}`"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return true;
                    }
                }

                return false;
            }

            #endregion

            #endregion
        }

        private void OnPaintWall(object? sender, GetDataHandlers.PaintWallEventArgs args)
        {
            #region code

            int tileX = args.X;
            int tileY = args.Y;

            if (PaintThreshold()) return;

            #region ( Threshold )

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
                    if (PunishPlayer(MKLP_CodeType.Default, 3, args.Player, $"Painting too fast", $"Player **{args.Player.Name}** has exceeded Paint Threshold `itemheld: {args.Player.SelectedItem.netID}` `Threshold {max}`"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return true;
                    }
                }

                return false;
            }

            #endregion

            #endregion
        }

        public void OnMassWireOperation(object? sender, GetDataHandlers.MassWireOperationEventArgs args)
        {
            #region code

            if (!NPC.downedBoss3)
            {
                if ((
                    !args.Player.HasPermission(Config.Permissions.Ignore_IllegalWireProgression) &&
                    !args.Player.HasPermission(TShockAPI.Permissions.item) &&
                    !args.Player.HasPermission(TShockAPI.Permissions.give) &&
                    !args.Player.HasPermission(TShockAPI.Permissions.manageitem)
                    ) && (bool)Config.Main.Prevent_IllegalWire_Progression
                    )
                {
                    Discordklp.KLPBotSendMessage_Warning($"Player **{args.Player.Name}** was Able to use Wire/Actuator on pre skeletron! `start: {args.StartX}, {args.StartY} end: {args.EndX}, {args.EndY}`", args.Player.Account.Name, "Illegal Wire/Actuator Progression");
                    args.Player.SendErrorMessage("This is Illegal on this progression!");
                    args.Handled = true;
                    return;
                }
            }

            if ((args.StartY >= (int)Main.worldSurface || args.EndY >= (int)Main.worldSurface) && (bool)Config.Main.ReceivedWarning_WirePlaceUnderground)
            {
                Discordklp.KLPBotSendMessageMainLog($"Player **{args.Player.Name}** Used mass Wire/Actuator below surface `start: {args.StartX}, {args.StartY} end: {args.EndX}, {args.EndY}`");

            }

            #endregion
        }

        private void HandleLiquidInteraction(object? sender, GetDataHandlers.LiquidSetEventArgs args)
        {
            #region code
            int TileX = args.TileX;
            int TileY = args.TileY;

            // Log the interaction details
            string liquidName = args.Type switch
            {
                GetDataHandlers.LiquidType.Removal => "Removal",
                GetDataHandlers.LiquidType.Water => "Water",
                GetDataHandlers.LiquidType.Lava => "Lava",
                GetDataHandlers.LiquidType.Honey => "Honey",
                GetDataHandlers.LiquidType.Shimmer => "Shimmer"
            };
            if (LiquidThreshold()) return;


            if (TileY < (int)Main.worldSurface && args.Type != GetDataHandlers.LiquidType.Removal)
            {
                // Log liquid placed
                if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_protectsurface_placeliquid) && (bool)Config.Main.Using_AntiGrief_Surface_PlaceLiquid)
                {
                    args.Player.SendErrorMessage(Config.Main.Message_AntiGrief_Surface_PlaceLiquid);
                    args.Player.SendTileSquareCentered(TileX, TileY, 4);
                    args.Handled = true;
                    return;
                }
            }

            #region ( Threshold )

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
                    if (PunishPlayer(MKLP_CodeType.Default, 4, args.Player, $"Exceeded Liquid place", $"Player **{args.Player.Name}** has exceeded TileLiquid Threshold `itemheld: {args.Player.SelectedItem.netID}` `Threshold: {max}`"))
                    {
                        args.Player.SendTileSquareCentered(TileX, TileY, 4);
                        args.Handled = true;
                        return true;
                    }
                }

                return false;
            }
            #endregion

            #endregion
        }

        private void OnNewProjectile(object sender, GetDataHandlers.NewProjectileEventArgs args)
        {
            #region code
            try
            {
                short ident = args.Identity;
                //Vector2 pos = args.Position;
                //Vector2 vel = args.Velocity;
                //float knockback = args.Knockback;
                //short damage = args.Damage;
                byte owner = args.Owner;
                short type = args.Type;
                //int index = args.Index;
                //float[] ai = args.Ai;

                if (ProjectileThreshold()) return;

                Dictionary<short, string> GetIllegalProj = SurvivalManager.GetIllegalProjectile();


                if (args.Player.IsLoggedIn && IllegalProjectileProgression.ContainsKey(type) &&
                    !args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_2) &&
                    (bool)Config.Main.Using_Survival_Code2)
                {
                    if (PunishPlayer(MKLP_CodeType.Survival, 2, args.Player, $"{GetIllegalProj[type]} Projectile", $"Player **{args.Player.Name}** spawned illegal Projectile progression `itemheld: {args.Player.SelectedItem.netID} projectile: {Lang.GetProjectileName(type)}` **{GetIllegalProj[type]}**"))
                    {
                        args.Player.RemoveProjectile(ident, owner);
                        args.Handled = true;
                        return;
                    }
                }
                short[] InfectionProj =
                {
                    ProjectileID.ViciousPowder,
                    ProjectileID.VilePowder,

                    ProjectileID.CrimsonSpray,
                    ProjectileID.CorruptSpray,
                    ProjectileID.HallowSpray,

                    ProjectileID.BloodWater,
                    ProjectileID.UnholyWater,
                    ProjectileID.HolyWater
                };

                if (InfectionProj.Contains(type))
                {
                    if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_infection) && (bool)Config.Main.Using_AntiGrief_Infection)
                    {
                        args.Player.SendErrorMessage(Config.Main.Message_AntiGrief_Infection);
                        args.Player.RemoveProjectile(ident, owner);
                        args.Handled = true;
                        return;
                    }
                }

                short[] SprayProj =
                {
                    ProjectileID.CorruptSpray,
                    ProjectileID.CrimsonSpray,
                    ProjectileID.DirtSpray,
                    ProjectileID.HallowSpray,
                    ProjectileID.MushroomSpray,
                    ProjectileID.PureSpray,
                    ProjectileID.SandSpray,
                    ProjectileID.SnowSpray
                };

                if (SprayProj.Contains(type))
                {
                    if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_spray) && (bool)Config.Main.Using_AntiGrief_Spray)
                    {
                        args.Player.SendErrorMessage(Config.Main.Message_AntiGrief_Spray);
                        args.Player.RemoveProjectile(ident, owner);
                        args.Handled = true;
                        return;
                    }
                }

                if (args.Player.TileY <= (int)Main.worldSurface)
                {
                    short[] explosives =
                    {
                        //misc
                        ProjectileID.DirtBomb,
                        ProjectileID.DirtStickyBomb,

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

                    if (!explosives.Contains(type)) return;

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
                        if (PunishPlayer(MKLP_CodeType.Default, 5, args.Player, $"Spawning too many projectiles at onces!", $"Player **{args.Player.Name}** Spawned to many projectile at onces! `itemheld: {args.Player.SelectedItem.netID} projectile id: {type}` `Threshold: {max}`"))
                        {
                            args.Player.RemoveProjectile(ident, owner);
                            args.Handled = true;
                            return true;
                        }
                    }

                    return false;
                }
            } catch (OutOfMemoryException e)
            {
                MKLP_Console.SendLog_Exception(e);
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
                    if (PunishPlayer(MKLP_CodeType.Default, 6, args.Player, $"Healing others to fast!", $"Player **{args.Player.Name}** has exceeded HealOther Threshold `itemheld: {args.Player.SelectedItem.netID}` `Threshold: {max}`"))
                    {
                        args.Handled = true;
                        return true;
                    }
                }

                return false;
            }
            #endregion
        }

        private async void OnNPCSpawn(NpcSpawnEventArgs args)
        {
            #region code

            if (!Main.npc[args.NpcId].boss) return;

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
                        DespawnNPC();
                    }

                    if (!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3 && (npc.type == NPCID.Plantera))
                    {
                        DespawnNPC();
                    }
                    if (!NPC.downedPlantBoss && (npc.type == NPCID.HallowBoss || npc.type == NPCID.EmpressButterfly || npc.type == NPCID.Golem))
                    {
                        DespawnNPC();
                    }
                    if (!NPC.downedGolemBoss && (npc.type == NPCID.CultistBoss || npc.type == NPCID.MoonLordCore))
                    {
                        DespawnNPC();
                    }
                }


                if (!NPC.downedSlimeKing && npc.type == NPCID.KingSlime) // King Slime
                {
                    if (!(bool)Config.BossManager.AllowKingSlime)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("King Slime isn't allowed yet!", Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.KingSlime_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("There aren't enough players to fight King Slime!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.KingSlime_RequiredPlayersforBoss}", Color.MediumPurple);
                    }
                }

                if (!NPC.downedBoss1 && npc.type == NPCID.EyeofCthulhu) // Eye of Cthulhu
                {
                    if (!(bool)Config.BossManager.AllowEyeOfCthulhu)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("Eye of Cthulhu isn't allowed yet!", Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.EyeOfCthulhu_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("There aren't enough players to fight Eye of Cthulhu!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.EyeOfCthulhu_RequiredPlayersforBoss}", Color.MediumPurple);
                    }
                }

                if (!NPC.downedBoss2 && npc.type == NPCID.EaterofWorldsHead) // Eater of Worlds
                {
                    if (!(bool)Config.BossManager.AllowEaterOfWorlds)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("Eater of Worlds isn't allowed yet!", Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.EaterOfWorlds_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("There aren't enough players to fight Eater of Worlds!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.EaterOfWorlds_RequiredPlayersforBoss}", Color.MediumPurple);
                    }
                }

                if (!NPC.downedBoss2 && npc.type == NPCID.BrainofCthulhu) // Brain of Cthulhu
                {
                    if (!(bool)Config.BossManager.AllowBrainOfCthulhu)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("Brain of Cthulhu isn't allowed yet!", Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.BrainOfCthulhu_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("There aren't enough players to fight Brain of Cthulhu!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.BrainOfCthulhu_RequiredPlayersforBoss}", Color.MediumPurple);
                    }
                }

                if (!NPC.downedQueenBee && npc.type == NPCID.QueenBee) // Queen Bee
                {
                    if (!(bool)Config.BossManager.AllowQueenBee)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("Queen Bee isn't allowed yet!", Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.QueenBee_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("There aren't enough players to fight Queen Bee!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.QueenBee_RequiredPlayersforBoss}", Color.MediumPurple);
                    }
                }

                if (!NPC.downedBoss3 && npc.type == NPCID.SkeletronHead) // Skeletron
                {
                    if (!(bool)Config.BossManager.AllowSkeletron)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("Skeletron isn't allowed yet!", Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Skeletron_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("There aren't enough players to fight Skeletron!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.Skeletron_RequiredPlayersforBoss}", Color.MediumPurple);
                    }
                }

                if (!NPC.downedDeerclops && npc.type == NPCID.Deerclops) // Deerclops
                {
                    if (!(bool)Config.BossManager.AllowDeerclops)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("Deerclops isn't allowed yet!", Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Deerclops_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("There aren't enough players to fight Deerclops!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.Deerclops_RequiredPlayersforBoss}", Color.MediumPurple);
                    }
                }

                if (!Main.hardMode && npc.type == NPCID.WallofFlesh) // Wall of Flesh
                {
                    if (!(bool)Config.BossManager.AllowWallOfFlesh)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("Wall of Flesh isn't allowed yet!", Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.WallOfFlesh_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("There aren't enough players to fight Wall of Flesh!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.WallOfFlesh_RequiredPlayersforBoss}", Color.MediumPurple);
                    }
                }

                if (!NPC.downedQueenSlime && npc.type == NPCID.QueenSlimeBoss) // Queen Slime
                {
                    if (!(bool)Config.BossManager.AllowQueenSlime)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("Queen Slime isn't allowed yet!", Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.QueenSlime_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("There aren't enough players to fight Queen Slime!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.QueenSlime_RequiredPlayersforBoss}", Color.MediumPurple);
                    }
                }

                if (Main.zenithWorld)
                {
                    if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss1) &&
                        (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism || (npc.type == NPCID.TheDestroyer || npc.type == NPCID.TheDestroyerBody || npc.type == NPCID.TheDestroyerTail) || npc.type == NPCID.SkeletronPrime)
                        )
                    {
                        if (!(bool)Config.BossManager.AllowMechdusa)
                        {
                            await Task.Delay(700);
                            DespawnNPC();
                            TShock.Utils.Broadcast("Mechdusa isn't allowed yet!", Color.MediumPurple);
                        }
                        if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Mechdusa_RequiredPlayersforBoss)
                        {
                            await Task.Delay(700);
                            DespawnNPC();
                            TShock.Utils.Broadcast("There aren't enough players to fight Mechdusa!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.Mechdusa_RequiredPlayersforBoss}", Color.MediumPurple);
                        }
                    }
                } else
                {
                    if (!NPC.downedMechBoss2 && (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism)) // The Twins
                    {
                        if (!(bool)Config.BossManager.AllowTheTwins)
                        {
                            DespawnNPC();
                            TShock.Utils.Broadcast("The Twins isn't allowed yet!", Color.MediumPurple);
                        }
                        if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.TheTwins_RequiredPlayersforBoss)
                        {
                            DespawnNPC();
                            TShock.Utils.Broadcast("There aren't enough players to fight The Twins!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.TheTwins_RequiredPlayersforBoss}", Color.MediumPurple);
                        }
                    }

                    if (!NPC.downedMechBoss1 && (npc.type == NPCID.TheDestroyer || npc.type == NPCID.TheDestroyerBody || npc.type == NPCID.TheDestroyerTail)) // The Destroyer
                    {
                        if (!(bool)Config.BossManager.AllowTheDestroyer)
                        {
                            DespawnNPC();
                            TShock.Utils.Broadcast("The Destroyer isn't allowed yet!", Color.MediumPurple);
                        }
                        if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.TheDestroyer_RequiredPlayersforBoss)
                        {
                            DespawnNPC();
                            TShock.Utils.Broadcast("There aren't enough players to fight The Destroyer!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.TheDestroyer_RequiredPlayersforBoss}", Color.MediumPurple);
                        }
                    }

                    if (!NPC.downedMechBoss3 && npc.type == NPCID.SkeletronPrime) // Skeletron Prime
                    {
                        if (!(bool)Config.BossManager.AllowSkeletronPrime)
                        {
                            DespawnNPC();
                            TShock.Utils.Broadcast("Skeletron Prime isn't allowed yet!", Color.MediumPurple);
                        }
                        if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.SkeletronPrime_RequiredPlayersforBoss)
                        {
                            DespawnNPC();
                            TShock.Utils.Broadcast("There aren't enough players to fight Skeletron Prime!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.SkeletronPrime_RequiredPlayersforBoss}", Color.MediumPurple);
                        }
                    }
                }

                

                if (!NPC.downedPlantBoss && npc.type == NPCID.Plantera) // Plantera
                {
                    if (!(bool)Config.BossManager.AllowPlantera)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("Plantera isn't allowed yet!", Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Plantera_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("There aren't enough players to fight Plantera!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.Plantera_RequiredPlayersforBoss}", Color.MediumPurple);
                    }
                }

                if (!NPC.downedGolemBoss && npc.type == NPCID.Golem) // Golem
                {
                    if (!(bool)Config.BossManager.AllowGolem)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("Golem isn't allowed yet!", Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Golem_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("There aren't enough players to fight Golem!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.Golem_RequiredPlayersforBoss}", Color.MediumPurple);
                    }
                }

                if (!NPC.downedFishron && npc.type == NPCID.DukeFishron) // Duke Fishron
                {
                    if (!(bool)Config.BossManager.AllowDukeFishron)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("Duke Fishron isn't allowed yet!", Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.DukeFishron_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("There aren't enough players to fight Duke Fishron!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.DukeFishron_RequiredPlayersforBoss}", Color.MediumPurple);
                    }
                }

                if (!NPC.downedEmpressOfLight && npc.type == NPCID.HallowBoss) // Empress of Light
                {
                    if (!(bool)Config.BossManager.AllowEmpressOfLight)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("Empress of Light isn't allowed yet!", Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.EmpressOfLight_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("There aren't enough players to fight Empress of Light!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.EmpressOfLight_RequiredPlayersforBoss}", Color.MediumPurple);
                    }
                }

                if (!NPC.downedAncientCultist && npc.type == NPCID.CultistBoss) // Lunatic Cultist
                {
                    if (!(bool)Config.BossManager.AllowLunaticCultist)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("Lunatic Cultist isn't allowed yet!", Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.LunaticCultist_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("There aren't enough players to fight Lunatic Cultist!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.LunaticCultist_RequiredPlayersforBoss}", Color.MediumPurple);
                    }
                }

                if (!NPC.downedMoonlord && npc.type == NPCID.MoonLordCore) // Moon Lord
                {
                    if (!(bool)Config.BossManager.AllowMoonLord)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("Moon Lord isn't allowed yet!", Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.MoonLord_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast("There aren't enough players to fight Moon Lord!" +
                            $"\nPlayers Needed: {(int)Config.BossManager.MoonLord_RequiredPlayersforBoss}", Color.MediumPurple);
                    }
                }

                void DespawnNPC()
                {
                    args.Handled = true;
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

                IllegalWallProgression = SurvivalManager.GetIllegalWall();
            }
            

            #endregion
        }

        private static bool nullboss_Confirmed_Twins = false;
        private bool HandleSpawnBoss(GetDataHandlerArgs args)
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
            int npcid = 0;

            if (isKnownBoss)
            {
                getnpc.SetDefaults(thingType);
                npcid = getnpc.type;
            }

            
            if (npcid == -16)
            {
                if (!(bool)Config.BossManager.AllowMechdusa)
                {
                    TShock.Utils.Broadcast("Mechdusa isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Mechdusa_RequiredPlayersforBoss)
                {
                    TShock.Utils.Broadcast("There aren't enough players to fight Mechdusa!" +
                    $"\nPlayers Needed: {(int)Config.BossManager.Mechdusa_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }
            if (npcid == -8)
            {
                if (!(bool)Config.BossManager.AllowMoonLord)
                {
                    TShock.Utils.Broadcast("Moon Lord isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.MoonLord_RequiredPlayersforBoss)
                {
                    TShock.Utils.Broadcast("There aren't enough players to fight Moon Lord!" +
                        $"\nPlayers Needed: {(int)Config.BossManager.MoonLord_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }
            #region bossmanager
            
            if ((bool)Config.BossManager.PreventIllegalBoss)
            {
                if (!Main.hardMode && (
                    npcid == NPCID.QueenSlimeBoss ||
                    npcid == NPCID.TheDestroyer ||
                    npcid == NPCID.Retinazer ||
                    npcid == NPCID.Spazmatism ||
                    npcid == NPCID.SkeletronPrime ||
                    npcid == NPCID.DukeFishron))
                {
                    //args.Player.SendErrorMessage("Illegal Boss Spawn!");
                    return false;
                }

                if (!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3 && (npcid == NPCID.Plantera))
                {
                    //args.Player.SendErrorMessage("Illegal Boss Spawn!");
                    return false;
                }
                if (!NPC.downedPlantBoss && (npcid == NPCID.HallowBoss || npcid == NPCID.EmpressButterfly || npcid == NPCID.Golem))
                {
                    //args.Player.SendErrorMessage("Illegal Boss Spawn!");
                    return false;
                }
                if (!NPC.downedGolemBoss && (npcid == NPCID.CultistBoss || npcid == NPCID.MoonLordCore || npcid == -8))
                {
                    //args.Player.SendErrorMessage("Illegal Boss Spawn!");
                    return false;
                }
            }


            if (!NPC.downedSlimeKing && npcid == NPCID.KingSlime) // King Slime
            {
                if (!(bool)Config.BossManager.AllowKingSlime)
                {
                    //TShock.Utils.Broadcast("King Slime isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.KingSlime_RequiredPlayersforBoss)
                {
                    //TShock.Utils.Broadcast("There aren't enough players to fight King Slime!" +
                    //    $"\nPlayers Needed: {(int)Config.BossManager.KingSlime_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }

            if (!NPC.downedBoss1 && npcid == NPCID.EyeofCthulhu) // Eye of Cthulhu
            {
                if (!(bool)Config.BossManager.AllowEyeOfCthulhu)
                {
                    //TShock.Utils.Broadcast("Eye of Cthulhu isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.EyeOfCthulhu_RequiredPlayersforBoss)
                {
                    //TShock.Utils.Broadcast("There aren't enough players to fight Eye of Cthulhu!" +
                    //    $"\nPlayers Needed: {(int)Config.BossManager.EyeOfCthulhu_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }

            if (!NPC.downedBoss2 && npcid == NPCID.EaterofWorldsHead) // Eater of Worlds
            {
                if (!(bool)Config.BossManager.AllowEaterOfWorlds)
                {
                    //TShock.Utils.Broadcast("Eater of Worlds isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.EaterOfWorlds_RequiredPlayersforBoss)
                {
                    //TShock.Utils.Broadcast("There aren't enough players to fight Eater of Worlds!" +
                    //    $"\nPlayers Needed: {(int)Config.BossManager.EaterOfWorlds_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }

            if (!NPC.downedBoss2 && npcid == NPCID.BrainofCthulhu) // Brain of Cthulhu
            {
                if (!(bool)Config.BossManager.AllowBrainOfCthulhu)
                {
                    //TShock.Utils.Broadcast("Brain of Cthulhu isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.BrainOfCthulhu_RequiredPlayersforBoss)
                {
                    //TShock.Utils.Broadcast("There aren't enough players to fight Brain of Cthulhu!" +
                    //    $"\nPlayers Needed: {(int)Config.BossManager.BrainOfCthulhu_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }

            if (!NPC.downedQueenBee && npcid == NPCID.QueenBee) // Queen Bee
            {
                if (!(bool)Config.BossManager.AllowQueenBee)
                {
                    //TShock.Utils.Broadcast("Queen Bee isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.QueenBee_RequiredPlayersforBoss)
                {
                    //TShock.Utils.Broadcast("There aren't enough players to fight Queen Bee!" +
                    //    $"\nPlayers Needed: {(int)Config.BossManager.QueenBee_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }

            if (!NPC.downedBoss3 && npcid == NPCID.SkeletronHead) // Skeletron
            {
                if (!(bool)Config.BossManager.AllowSkeletron)
                {
                    //TShock.Utils.Broadcast("Skeletron isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Skeletron_RequiredPlayersforBoss)
                {
                    //TShock.Utils.Broadcast("There aren't enough players to fight Skeletron!" +
                    //    $"\nPlayers Needed: {(int)Config.BossManager.Skeletron_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }

            if (!NPC.downedDeerclops && npcid == NPCID.Deerclops) // Deerclops
            {
                if (!(bool)Config.BossManager.AllowDeerclops)
                {
                    //TShock.Utils.Broadcast("Deerclops isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Deerclops_RequiredPlayersforBoss)
                {
                    //TShock.Utils.Broadcast("There aren't enough players to fight Deerclops!" +
                    //    $"\nPlayers Needed: {(int)Config.BossManager.Deerclops_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }

            if (!Main.hardMode && npcid == NPCID.WallofFlesh) // Wall of Flesh
            {
                if (!(bool)Config.BossManager.AllowWallOfFlesh)
                {
                    //TShock.Utils.Broadcast("Wall of Flesh isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.WallOfFlesh_RequiredPlayersforBoss)
                {
                    //TShock.Utils.Broadcast("There aren't enough players to fight Wall of Flesh!" +
                    //    $"\nPlayers Needed: {(int)Config.BossManager.WallOfFlesh_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }

            if (!NPC.downedQueenSlime && npcid == NPCID.QueenSlimeBoss) // Queen Slime
            {
                if (!(bool)Config.BossManager.AllowQueenSlime)
                {
                    //TShock.Utils.Broadcast("Queen Slime isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.QueenSlime_RequiredPlayersforBoss)
                {
                    //TShock.Utils.Broadcast("There aren't enough players to fight Queen Slime!" +
                    //    $"\nPlayers Needed: {(int)Config.BossManager.QueenSlime_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }

            if (Main.zenithWorld)
            {
                if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss1) &&
                    (npcid == NPCID.Retinazer || npcid == NPCID.Spazmatism || npcid == NPCID.TheDestroyer || npcid == NPCID.SkeletronPrime)
                    )
                {
                    if (!(bool)Config.BossManager.AllowMechdusa)
                    {
                        //TShock.Utils.Broadcast("Mechdusa isn't allowed yet!", Color.MediumPurple);
                        return false;
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Mechdusa_RequiredPlayersforBoss)
                    {
                        //TShock.Utils.Broadcast("There aren't enough players to fight Mechdusa!" +
                        //  $"\nPlayers Needed: {(int)Config.BossManager.Mechdusa_RequiredPlayersforBoss}", Color.MediumPurple);
                        return false;
                    }
                }
            }
            else
            {
                if (!NPC.downedMechBoss2 && (npcid == NPCID.Retinazer || npcid == NPCID.Spazmatism)) // The Twins
                {
                    if (!(bool)Config.BossManager.AllowTheTwins)
                    {
                        //TShock.Utils.Broadcast("The Twins isn't allowed yet!", Color.MediumPurple);
                        return false;
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.TheTwins_RequiredPlayersforBoss)
                    {
                        //TShock.Utils.Broadcast("There aren't enough players to fight The Twins!" +
                        //  $"\nPlayers Needed: {(int)Config.BossManager.TheTwins_RequiredPlayersforBoss}", Color.MediumPurple);
                        return false;
                    }
                }

                if (!NPC.downedMechBoss1 && npcid == NPCID.TheDestroyer) // The Destroyer
                {
                    if (!(bool)Config.BossManager.AllowTheDestroyer)
                    {
                        //TShock.Utils.Broadcast("The Destroyer isn't allowed yet!", Color.MediumPurple);
                        return false;
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.TheDestroyer_RequiredPlayersforBoss)
                    {
                        //TShock.Utils.Broadcast("There aren't enough players to fight The Destroyer!" +
                        //  $"\nPlayers Needed: {(int)Config.BossManager.TheDestroyer_RequiredPlayersforBoss}", Color.MediumPurple);
                        return false;
                    }
                }

                if (!NPC.downedMechBoss3 && npcid == NPCID.SkeletronPrime) // Skeletron Prime
                {
                    if (!(bool)Config.BossManager.AllowSkeletronPrime)
                    {
                        //TShock.Utils.Broadcast("Skeletron Prime isn't allowed yet!", Color.MediumPurple);
                        return false;
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.SkeletronPrime_RequiredPlayersforBoss)
                    {
                        //TShock.Utils.Broadcast("There aren't enough players to fight Skeletron Prime!" +
                        //  $"\nPlayers Needed: {(int)Config.BossManager.SkeletronPrime_RequiredPlayersforBoss}", Color.MediumPurple);
                        return false;
                    }
                }
            }



            if (!NPC.downedPlantBoss && npcid == NPCID.Plantera) // Plantera
            {
                if (!(bool)Config.BossManager.AllowPlantera)
                {
                    //TShock.Utils.Broadcast("Plantera isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Plantera_RequiredPlayersforBoss)
                {
                    //TShock.Utils.Broadcast("There aren't enough players to fight Plantera!" +
                    //    $"\nPlayers Needed: {(int)Config.BossManager.Plantera_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }

            if (!NPC.downedGolemBoss && npcid == NPCID.Golem) // Golem
            {
                if (!(bool)Config.BossManager.AllowGolem)
                {
                    //TShock.Utils.Broadcast("Golem isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Golem_RequiredPlayersforBoss)
                {
                    //TShock.Utils.Broadcast("There aren't enough players to fight Golem!" +
                    //    $"\nPlayers Needed: {(int)Config.BossManager.Golem_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }

            if (!NPC.downedFishron && npcid == NPCID.DukeFishron) // Duke Fishron
            {
                if (!(bool)Config.BossManager.AllowDukeFishron)
                {
                    //TShock.Utils.Broadcast("Duke Fishron isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.DukeFishron_RequiredPlayersforBoss)
                {
                    //TShock.Utils.Broadcast("There aren't enough players to fight Duke Fishron!" +
                    //    $"\nPlayers Needed: {(int)Config.BossManager.DukeFishron_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }

            if (!NPC.downedEmpressOfLight && npcid == NPCID.HallowBoss) // Empress of Light
            {
                if (!(bool)Config.BossManager.AllowEmpressOfLight)
                {
                    //TShock.Utils.Broadcast("Empress of Light isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.EmpressOfLight_RequiredPlayersforBoss)
                {
                    //TShock.Utils.Broadcast("There aren't enough players to fight Empress of Light!" +
                    //    $"\nPlayers Needed: {(int)Config.BossManager.EmpressOfLight_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }

            if (!NPC.downedAncientCultist && npcid == NPCID.CultistBoss) // Lunatic Cultist
            {
                if (!(bool)Config.BossManager.AllowLunaticCultist)
                {
                    //TShock.Utils.Broadcast("Lunatic Cultist isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.LunaticCultist_RequiredPlayersforBoss)
                {
                    //TShock.Utils.Broadcast("There aren't enough players to fight Lunatic Cultist!" +
                    //    $"\nPlayers Needed: {(int)Config.BossManager.LunaticCultist_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }

            if (!NPC.downedMoonlord && npcid == NPCID.MoonLordCore) // Moon Lord
            {
                if (!(bool)Config.BossManager.AllowMoonLord)
                {
                    //TShock.Utils.Broadcast("Moon Lord isn't allowed yet!", Color.MediumPurple);
                    return false;
                }
                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.MoonLord_RequiredPlayersforBoss)
                {
                    //TShock.Utils.Broadcast("There aren't enough players to fight Moon Lord!" +
                    //    $"\nPlayers Needed: {(int)Config.BossManager.MoonLord_RequiredPlayersforBoss}", Color.MediumPurple);
                    return false;
                }
            }
            
            #endregion

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
                            return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Mechdusa`");
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
                            return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `invasion: Blood Moon`");
                        }

                        break;
                    }
                case -8: // Impending doom approaches... ( Moon Lord )
                    {
                        if (args.Player.SelectedItem.type != 3601)
                        {
                            return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss/invasion: Impending doom approaches... (Moon Lord)`");
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
                            return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `event: Solar Eclipse`");
                        }

                        return false;
                    }
                case -5: // Frost Moon
                    {
                        if (args.Player.SelectedItem.type != 1958)
                        {
                            return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `event: Frost Moon`");
                        }

                        return false;
                    }
                case -4: //  Pumpkin Moon
                    {
                        if (args.Player.SelectedItem.type != 1844)
                        {
                            return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `event: Pumpkin Moon`");
                        }

                        return false;
                    }
                case -3: // Pirate Invasion
                    {
                        if (args.Player.SelectedItem.type != 1315)
                        {
                            return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `invasion: Pirate Invasion`");
                        }

                        return false;
                    }
                case -2: // frost legion
                    {
                        if (args.Player.SelectedItem.type != 602)
                        {
                            return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `invasion: Legion`");
                        }

                        return false;
                    }
                case -1: // goblin army
                    {
                        if (args.Player.SelectedItem.type != 361)
                        {
                            return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `invasion: Goblin Army`");
                        }

                        return false;
                    }
                
                default:
                    NPC npc = new NPC();
                    npc.SetDefaults(thingType);
                    
                    switch (npc.netID)
                    {
                        case 50: //king slime
                            {
                                if (args.Player.SelectedItem.type != 560)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: King Slime`");
                                }

                                break;
                            }
                        case 4: // Eye Of Cthulhu
                            {
                                if (args.Player.SelectedItem.type != 43)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Eye Of Cthulhu`");
                                }

                                break;
                            }
                        case 13: // Eater Of Worlds
                        case 14:
                        case 15:
                            {
                                if (!args.Player.TPlayer.ZoneCorrupt)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Eater Of Worlds` **not in corruption zone**");
                                }
                                if (args.Player.SelectedItem.type != 70)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn",$"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Eater Of Worlds`");
                                }

                                break;
                            }
                        case 266: // Brain Of Cthulhu
                            {
                                if (!args.Player.TPlayer.ZoneCrimson)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Brain Of Cthulhu` **not in crimson zone**");
                                }
                                if (args.Player.SelectedItem.type != 1331)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Brain Of Cthulhu`");
                                }

                                break;
                            }
                        case 222: // Queen Bee
                            {
                                if (!args.Player.TPlayer.ZoneJungle)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Queen Bee` **not in jungle zone**");
                                }
                                if (args.Player.SelectedItem.type != 1133)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Queen Bee`");
                                }

                                break;
                            }
                        case 668: // Deerclops
                            {
                                if (!args.Player.TPlayer.ZoneSnow)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Deerclops` **not in snow zone**");
                                }

                                if (args.Player.SelectedItem.type != 5120)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Deerclops`");
                                }

                                break;
                            }
                        case 35: // Skeletron
                        case 36:
                            {
                                if (NPC.downedBoss3)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Skeletron`");
                                }
                                break;
                            }
                        case 113: // Wall of Flesh
                            {
                                return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Wall Of Flesh`");
                            }
                        case 657: // Queen Slime
                            {
                                if (!args.Player.TPlayer.ZoneHallow)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Queen Slime` **not in hallow zone**");
                                }
                                if (args.Player.SelectedItem.type != 4988)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Queen Slime`");
                                }

                                break;
                            }
                        case 125: // The Twins
                            {
                                if (args.Player.SelectedItem.type != 544)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: The Twins`");
                                }
                                nullboss_Confirmed_Twins = true;
                                break;
                            }
                        case 126: // The Twins
                            {
                                if (!nullboss_Confirmed_Twins)
                                {
                                    if (args.Player.SelectedItem.type != 544)
                                    {
                                        return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: The Twins`");
                                    }
                                }
                                nullboss_Confirmed_Twins = false;
                                break;
                            }
                        case 134: // Destroyer
                        case 135:
                        case 136:
                            {
                                if (args.Player.SelectedItem.type != 556)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: The Destroyer`");
                                }

                                break;
                            }
                        case 127: // Skeletron Prime
                            {
                                if (args.Player.SelectedItem.type != 557)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Skeletron Prime`");
                                }

                                break;
                            }
                        case 262: // Plantera
                            {
                                return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Plantera`");
                            }
                        case 245: // Golem
                        case 246:
                        case 247:
                        case 248:
                            {
                                if (!NPC.downedPlantBoss)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Golem`");
                                }
                                /*
                                if (!args.Player.Inventory.All(i => i.type == 1293) && !args.Player.TPlayer.bank4.item.All(i => i.type == 1293))
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }
                                */
                                if (!IsNextGolemSpawn(args.Player))
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Golem` **not near on altar**");
                                }
                                break;
                            }
                        case 370: // Duke Fishron
                            {
                                if (!Main.hardMode)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Duke Fishron` **not hardmode**");
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
                                if (!args.Player.TPlayer.ZoneBeach)
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Duke Fishron` **not in beach zone**");
                                }
                                if (!fishing_rods.Contains(args.Player.SelectedItem.type))
                                {
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Duke Fishron`");
                                }
                                /*
                                if (!args.Player.Inventory.All(i => fishing_rods.Contains(i.type)))
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }
                                if (!args.Player.Inventory.All(i => i.type == ItemID.TruffleWorm))
                                {
                                    ManagePlayer.DisablePlayer(args.Player, $"null item boss spawn", ServerReason: $"Main,code,2|{args.Player.SelectedItem.netID}|{getnpc.FullName}");
                                    return true;
                                }
                                */

                                break;
                            }
                        case 636: // Empress Of Light
                            {
                                return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Empress Of Light`");
                            }
                        case 440: // Lunatic Cultist
                            {
                                return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Lunatic Cultist`");
                            }
                    }

                    break;
                    
            }

            return false;
            /*
            bool HasContainsItemID(TSPlayer player, params int[] itemids)
            {
                foreach (int itemid in  itemids)
                {
                    foreach (Item gettsitem in player.TPlayer.inventory)
                    {
                        if (gettsitem.netID == itemid)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            */

            bool IsNextGolemSpawn(TSPlayer player)
            {

                int playerX = (int)(player.TileX);
                int playerY = (int)(player.TileY);


                for (int x = playerX - 24; x <= playerX + 24; x++)
                {
                    for (int y = playerY - 24; y <= playerY + 24; y++)
                    {
                        if (x == playerX && y == playerY)
                            continue;
                        if (Main.tile[x, y].type == TileID.LihzahrdAltar)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            #endregion
        }

        public struct GetPlayerIG
        {
            public int PreviousHealth;
            public DateTime immunityTill;

            public GetPlayerIG(int prevhp)
            {
                PreviousHealth = prevhp;
                immunityTill = DateTime.MinValue;
            }
            public GetPlayerIG(int prevhp, DateTime immunity)
            {
                PreviousHealth = prevhp;
                immunityTill = immunity;
            }
        }

        private void OnNPCAIUpdate(NpcAiUpdateEventArgs args)
        {
            #region code
            /*
            if ((bool)Config.Main.ServerSideDamage)
            {
                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null) continue;

                    //float getdistance = (float)Math.Sqrt(((args.Npc.Center.X / 16) - (player.TPlayer.Center.X / 16))
                    //    * ((args.Npc.Center.X / 16) - (player.TPlayer.Center.X / 16))
                    //    + ((args.Npc.Center.Y / 16) - (player.TPlayer.Center.Y / 16))
                    //    * ((args.Npc.Center.Y / 16) - (player.TPlayer.Center.Y / 16)));

                    float getdistance = player.TPlayer.Center.Distance(args.Npc.Center);

                    if (getdistance <= 32)
                    {
                        if (player.ContainsData("MKLP_GetPlayerIG"))
                        {
                            if (player.TPlayer.onHitDodge || player.TPlayer.shadowDodge) continue;

                            GetPlayerIG getplrdata = player.GetData<GetPlayerIG>("MKLP_GetPlayerIG");

                            if ((getplrdata.immunityTill - DateTime.UtcNow).TotalSeconds > 0) continue;

                            int totaldmg = args.Npc.damage - player.TPlayer.statDefense;
                            int hpresult = getplrdata.PreviousHealth - totaldmg;

                            if (player.TPlayer.statLife == getplrdata.PreviousHealth)
                            {
                                
                                //player.TPlayer.statLife -= totaldmg;
                                //player.TPlayer.statLife = hpresult;
                                //TSPlayer.All.SendData(PacketTypes.PlayerHp, number: player.Index);
                                //TSPlayer.All.SendData(PacketTypes.PlayerUpdate, number: player.Index);
                                //player.DamagePlayer(totaldmg);
                                
                                player.TPlayer.statLife = hpresult;
                                TSPlayer.All.SendData(PacketTypes.PlayerHp, number: player.Index, number2: hpresult);
                                TSPlayer.All.SendData(PacketTypes.CreateCombatTextExtended,
                                    $"=[{totaldmg}]=",
                                    (int)Color.Red.packedValue, player.X, player.Y);

                                DateTime getcd = DateTime.UtcNow;

                                player.SetData("MKLP_GetPlayerIG", new GetPlayerIG(player.TPlayer.statLife, getcd.AddMilliseconds(300)));
                            }
                            else
                            {
                                player.SetData("MKLP_GetPlayerIG", new GetPlayerIG(player.TPlayer.statLife));
                            }
                        } else
                        {
                            player.SetData("MKLP_GetPlayerIG", new GetPlayerIG(player.TPlayer.statLife));
                        }
                    }
                }
            }
            */
            #endregion
        }


        private void OnProjectileAIUpdate(ProjectileAiUpdateEventArgs args)
        {
            #region code
            /*
            if (!args.Projectile.hostile) return;

            if ((bool)Config.Main.ServerSideDamage)
            {
                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null) continue;

                    //float getdistance = (float)Math.Sqrt(((args.Projectile.Center.X / 16) - (player.TPlayer.Center.X / 16))
                    //    * ((args.Projectile.Center.X / 16) - (player.TPlayer.Center.X / 16))
                    //    + ((args.Projectile.Center.Y / 16) - (player.TPlayer.Center.Y / 16))
                    //    * ((args.Projectile.Center.Y / 16) - (player.TPlayer.Center.Y / 16)));

                    float getdistance = player.TPlayer.Center.Distance(args.Projectile.Center);

                    if (getdistance <= 32)
                    {
                        if (player.ContainsData("MKLP_GetPlayerIG"))
                        {
                            if (player.TPlayer.onHitDodge || player.TPlayer.shadowDodge) continue;

                            GetPlayerIG getplrdata = player.GetData<GetPlayerIG>("MKLP_GetPlayerIG");

                            if ((getplrdata.immunityTill - DateTime.UtcNow).TotalSeconds > 0) continue;

                            int totaldmg = args.Projectile.damage - player.TPlayer.statDefense;
                            int hpresult = getplrdata.PreviousHealth - totaldmg;

                            if (player.TPlayer.statLife == getplrdata.PreviousHealth)
                            {
                                //player.TPlayer.statLife -= totaldmg;
                                player.TPlayer.statLife = hpresult;
                                TSPlayer.All.SendData(PacketTypes.PlayerHp, number: player.Index);
                                TSPlayer.All.SendData(PacketTypes.PlayerUpdate, number: player.Index);
                                player.DamagePlayer(totaldmg);
                                TSPlayer.All.SendData(PacketTypes.CreateCombatTextExtended,
                                    $"=[{totaldmg}]=",
                                    (int)Color.Red.packedValue, player.X, player.Y);

                                DateTime getcd = DateTime.UtcNow;

                                player.SetData("MKLP_GetPlayerIG", new GetPlayerIG(player.TPlayer.statLife, getcd.AddMilliseconds(300)));
                            }
                            else
                            {
                                player.SetData("MKLP_GetPlayerIG", new GetPlayerIG(player.TPlayer.statLife));
                            }
                        }
                        else
                        {
                            player.SetData("MKLP_GetPlayerIG", new GetPlayerIG(player.TPlayer.statLife));
                        }
                    }
                }
            }
            */
            #endregion
        }

        #endregion

        #region { Server }

        private void OnServerBroadcast(ServerBroadcastEventArgs args)
        {
            #region code
            var literalText = Terraria.Localization.Language.GetText(args.Message._text).Value;

            if (args.Message._substitutions?.Length > 0)
                literalText = string.Format(literalText, args.Message._substitutions);

            
            if (
                literalText.EndsWith(" has joined.") ||
                literalText.EndsWith(" has left.")
                )
            {
                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null) continue;
                    foreach (TSPlayer gplayer in TShock.Players)
                    {
                        if (gplayer == null) continue;
                        if (gplayer == player) continue;
                        if (gplayer.ContainsData("MKLP_Vanish"))
                        {
                            if (gplayer.GetData<bool>("MKLP_Vanish"))
                            {
                                player.SendData(PacketTypes.PlayerActive, null, gplayer.Index, false.GetHashCode());
                            }
                        }
                    }
                }
            }

            if (literalText.EndsWith(" has awoken!"))
            {
                args.Message._mode = NetworkText.Mode.LocalizationKey;
                literalText = args.Message.ToString();

                string bossName = literalText[..literalText.IndexOf(" has awoken!")];

                foreach (NPC npc in Main.npc)
                {
                    if (npc.FullName.StartsWith(bossName) && npc.type == 0 && !npc.active)
                    {
                        args.Handled = true;
                    }
                }
            }

            #endregion
        }

        private void OnReload(ReloadEventArgs args)
        {
            Config = Config.Read();
            LinkAccountManager.ReloadConfig();
            args.Player.SendMessage("MKLP config reloaded!", Microsoft.Xna.Framework.Color.Purple);

            if (!HasBanGuardPlugin && ((bool)Config.BanGuard.UsingBanGuard && (bool)Config.BanGuard.UsingPlugin))
            {
                Config.BanGuard.UsingBanGuard = false;
                args.Player.SendWarningMessage("Warning: BanGuard plugin doesn't Exist on 'ServerPlugins' Folder!");
                MKLP_Console.SendLog_Warning("Warning: BanGuard plugin doesn't Exist on \"ServerPlugins\" Folder!");
            }
        }

        private void OnWorldSave(WorldSaveEventArgs args)
        {
            checkplayers();

            if ((bool)Config.BossManager.UseBossSchedule)
            {
                check_bosssched();
            }

            try
            {
                InformLatestVersion();
            }
            catch { }
        }

        #endregion

        #region { Auto Check }
        
        private void OnServerStart(EventArgs args)
        {
            #region code

            IllegalItemProgression = SurvivalManager.GetIllegalItem();

            IllegalProjectileProgression = SurvivalManager.GetIllegalProjectile();

            IllegalTileProgression = SurvivalManager.GetIllegalTile();

            IllegalWallProgression = SurvivalManager.GetIllegalWall();

            //Slow_Checking();

            #endregion
        }

        /*
        private static int interval_informlatestversion = 2;
        private async void Slow_Checking()
        {
            interval_informlatestversion++;

            checkplayermute();

            if ((bool)Config.BossManager.UseBossSchedule)
            {
                check_bosssched();
            }

            if (interval_informlatestversion >= 2)
            {
                try
                {
                    InformLatestVersion();
                } catch { }
                interval_informlatestversion = 0;
            }
            await Task.Delay(120000);
            Slow_Checking();
            
        }
        */

        private void check_bosssched()
        {
            #region code
            bool changed = false;
            if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowKingSlime && !(bool)Config.BossManager.AllowKingSlime)
            {
                Config.BossManager.AllowKingSlime = true;
                changed = true;
                Discordklp.KLPBotSendMessage_BossEnabled("King Slime");
            }
            if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowEyeOfCthulhu && !(bool)Config.BossManager.AllowEyeOfCthulhu)
            {
                Config.BossManager.AllowEyeOfCthulhu = true;
                changed = true;
                Discordklp.KLPBotSendMessage_BossEnabled("Eye of Cthulhu");
            }
            if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowEaterOfWorlds && !(bool)Config.BossManager.AllowEaterOfWorlds)
            {
                Config.BossManager.AllowEaterOfWorlds = true;
                changed = true;
                Discordklp.KLPBotSendMessage_BossEnabled("Eater of Worlds");
            }
            if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowBrainOfCthulhu && !(bool)Config.BossManager.AllowBrainOfCthulhu)
            {
                Config.BossManager.AllowBrainOfCthulhu = true;
                changed = true;
                Discordklp.KLPBotSendMessage_BossEnabled("Brain of Cthulhu");
            }
            if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowQueenBee && !(bool)Config.BossManager.AllowQueenBee)
            {
                Config.BossManager.AllowQueenBee = true;
                changed = true;
                Discordklp.KLPBotSendMessage_BossEnabled("Queen Bee");
            }
            if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowSkeletron && !(bool)Config.BossManager.AllowSkeletron)
            {
                Config.BossManager.AllowSkeletron = true;
                changed = true;
                Discordklp.KLPBotSendMessage_BossEnabled("Skeletron");
            }
            if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowDeerclops && !(bool)Config.BossManager.AllowDeerclops)
            {
                Config.BossManager.AllowDeerclops = true;
                changed = true;
                Discordklp.KLPBotSendMessage_BossEnabled("Deerclops");
            }
            if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowWallOfFlesh && !(bool)Config.BossManager.AllowWallOfFlesh)
            {
                Config.BossManager.AllowWallOfFlesh = true;
                changed = true;
                Discordklp.KLPBotSendMessage_BossEnabled("Wall of Flesh");
            }
            if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowQueenSlime && !(bool)Config.BossManager.AllowQueenSlime)
            {
                Config.BossManager.AllowQueenSlime = true;
                changed = true;
                Discordklp.KLPBotSendMessage_BossEnabled("Queen Slime");
            }

            //mechanical boss
            if (Main.zenithWorld)
            {
                if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowMechdusa &&
                    (
                    !(bool)Config.BossManager.AllowTheTwins &&
                    !(bool)Config.BossManager.AllowTheDestroyer &&
                    !(bool)Config.BossManager.AllowSkeletronPrime
                    )
                    )
                {
                    Config.BossManager.AllowTheTwins = true;
                    Config.BossManager.AllowTheDestroyer = true;
                    Config.BossManager.AllowSkeletronPrime = true;
                    Config.BossManager.AllowMechdusa = true;
                    changed = true;
                    Discordklp.KLPBotSendMessage_BossEnabled("Mechdusa");
                }
            } else
            {
                if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowTheTwins && !(bool)Config.BossManager.AllowTheTwins)
                {
                    Config.BossManager.AllowTheTwins = true;
                    changed = true;
                    Discordklp.KLPBotSendMessage_BossEnabled("The Twins");
                }
                if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowTheDestroyer && !(bool)Config.BossManager.AllowTheDestroyer)
                {
                    Config.BossManager.AllowTheDestroyer = true;
                    changed = true;
                    Discordklp.KLPBotSendMessage_BossEnabled("The Destroyer");
                }
                if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowSkeletronPrime && !(bool)Config.BossManager.AllowSkeletronPrime)
                {
                    Config.BossManager.AllowSkeletronPrime = true;
                    changed = true;
                    Discordklp.KLPBotSendMessage_BossEnabled("Skeletron Prime");
                }
            }

            if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowPlantera && !(bool)Config.BossManager.AllowPlantera)
            {
                Config.BossManager.AllowPlantera = true;
                changed = true;
                Discordklp.KLPBotSendMessage_BossEnabled("Plantera");
            }
            if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowGolem && !(bool)Config.BossManager.AllowGolem)
            {
                Config.BossManager.AllowGolem = true;
                changed = true;
                Discordklp.KLPBotSendMessage_BossEnabled("Golem");
            }

            if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowDukeFishron && !(bool)Config.BossManager.AllowDukeFishron)
            {
                Config.BossManager.AllowDukeFishron = true;
                changed = true;
                Discordklp.KLPBotSendMessage_BossEnabled("Duke Fishron");
            }
            if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowEmpressOfLight && !(bool)Config.BossManager.AllowEmpressOfLight)
            {
                Config.BossManager.AllowEmpressOfLight = true;
                changed = true;
                Discordklp.KLPBotSendMessage_BossEnabled("Empress of Light");
            }

            if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowLunaticCultist && !(bool)Config.BossManager.AllowLunaticCultist)
            {
                Config.BossManager.AllowLunaticCultist = true;
                changed = true;
                Discordklp.KLPBotSendMessage_BossEnabled("Lunatic Cultist");
            }
            if (DateTime.UtcNow > (DateTime)Config.BossManager.ScheduleAllowMoonLord && !(bool)Config.BossManager.AllowMoonLord)
            {
                Config.BossManager.AllowMoonLord = true;
                changed = true;
                Discordklp.KLPBotSendMessage_BossEnabled("MoonLord");
            }

            if (changed)
            {
                Config.Changeall();
                Config.Read();
            }
            #endregion
        }

        private void checkplayers()
        {
            #region code
            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;
                try
                {
                    DBManager.CheckPlayerMute(player, true);
                } catch { }
            }
            #endregion
        }

        
        #endregion

        #endregion


        #region [ Commands ]

        #region { Default }

        private void CMD_ping(CommandArgs args)
        {
            #region code
            string result = "";

            
            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;
                result += $"{player.Name} : {player.GetData<double>("MKLP_GetLatency")}ms\n";
            }
            

            if (result == "") result = "No Latency has been check...";

            args.Player.SendMessage("List Of Players Latency:\n\n" +
                result, Color.Yellow);

            #endregion
        }

        private void CMD_BE(CommandArgs args)
        {
            #region code

            check_bosssched();

            #region { stringdefeatedbosses }
            /*
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
            */
            #endregion

            #region { stringdefeatedbosses2 }
            string GetListDefeatedBoss2()
            {
                CONFIG_BOSSES getenabledboss = Config.BossManager;
                Dictionary<string, bool> defeatedbosses = new();
                if ((bool)getenabledboss.AllowKingSlime)
                {
                    if (NPC.downedSlimeKing)
                    {
                        defeatedbosses.Add("[i:2493]", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:2493]", false);
                    }
                }
                else if (NPC.downedSlimeKing)
                {
                    defeatedbosses.Add("[i:2493]", true);
                }
                if ((bool)getenabledboss.AllowEyeOfCthulhu)
                {
                    if (NPC.downedBoss1)
                    {
                        defeatedbosses.Add("[i:2112]", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:2112]", false);
                    }
                }
                else if (NPC.downedBoss1)
                {
                    defeatedbosses.Add("[i:2112]", true);
                }
                if ((bool)getenabledboss.AllowEaterOfWorlds || (bool)getenabledboss.AllowBrainOfCthulhu)
                {
                    if (NPC.downedBoss2)
                    {
                        defeatedbosses.Add($"{(WorldGen.crimson ? "[i:2104]" : "[i:2111]")}", true);
                    }
                    else
                    {
                        defeatedbosses.Add($"{(WorldGen.crimson ? "[i:2104]" : "[i:2111]")}", false);
                    }
                }
                else if (NPC.downedBoss2)
                {
                    defeatedbosses.Add($"{(WorldGen.crimson ? "[i:2104]" : "[i:2111]")}", true);
                }
                if ((bool)getenabledboss.AllowDeerclops)
                {
                    if (NPC.downedDeerclops)
                    {
                        defeatedbosses.Add("[i:5109]", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:5109]", false);
                    }
                }
                else if (NPC.downedDeerclops)
                {
                    defeatedbosses.Add("[i:5109]", true);
                }
                if ((bool)getenabledboss.AllowQueenBee)
                {
                    if (NPC.downedQueenBee)
                    {
                        defeatedbosses.Add("[i:2108]", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:2108]", false);
                    }
                }
                else if (NPC.downedQueenBee)
                {
                    defeatedbosses.Add("[i:2108]", true);
                }
                if ((bool)getenabledboss.AllowSkeletron)
                {
                    if (NPC.downedBoss3)
                    {
                        defeatedbosses.Add("[i:1281]", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:1281]", false);
                    }
                }
                else if (NPC.downedBoss3)
                {
                    defeatedbosses.Add("[i:1281]", true);
                }
                if ((bool)getenabledboss.AllowWallOfFlesh)
                {
                    if (Main.hardMode)
                    {
                        defeatedbosses.Add("[i:2105]", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:2105]", false);
                    }
                }
                else if (Main.hardMode)
                {
                    defeatedbosses.Add("[i:2105]", true);
                }
                if ((bool)getenabledboss.AllowQueenSlime)
                {
                    if (NPC.downedQueenSlime)
                    {
                        defeatedbosses.Add("[i:4959]", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:4959]", false);
                    }
                }
                else if (NPC.downedQueenSlime)
                {
                    defeatedbosses.Add("[i:4959]", true);
                }
                if (Main.zenithWorld)
                {
                    if ((bool)getenabledboss.AllowTheDestroyer && (bool)getenabledboss.AllowTheTwins && (bool)getenabledboss.AllowSkeletronPrime)
                    {
                        if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
                        {
                            defeatedbosses.Add("[i:2113]", true);
                        }
                        else
                        {
                            defeatedbosses.Add("[i:2113]", false);
                        }
                    }
                    else if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
                    {
                        defeatedbosses.Add("[i:2113]", true);
                    }
                }
                else
                {
                    if ((bool)getenabledboss.AllowTheDestroyer)
                    {
                        if (NPC.downedMechBoss1)
                        {
                            defeatedbosses.Add("[i:2113]", true);
                        }
                        else
                        {
                            defeatedbosses.Add("[i:2113]", false);
                        }
                    }
                    else if (NPC.downedMechBoss1)
                    {
                        defeatedbosses.Add("[i:2113]", true);
                    }
                    if ((bool)getenabledboss.AllowTheTwins)
                    {
                        if (NPC.downedMechBoss2)
                        {
                            defeatedbosses.Add("[i:2106]", true);
                        }
                        else
                        {
                            defeatedbosses.Add("[i:2106]", false);
                        }
                    }
                    else if (NPC.downedMechBoss2)
                    {
                        defeatedbosses.Add("[i:2106]", true);
                    }
                    if ((bool)getenabledboss.AllowSkeletronPrime)
                    {
                        if (NPC.downedMechBoss3)
                        {
                            defeatedbosses.Add("[i:2107]", true);
                        }
                        else
                        {
                            defeatedbosses.Add("[i:2107]", false);
                        }
                    }
                    else if (NPC.downedMechBoss3)
                    {
                        defeatedbosses.Add("[i:2107]", true);
                    }
                }

                if ((bool)getenabledboss.AllowDukeFishron)
                {
                    if (NPC.downedFishron)
                    {
                        defeatedbosses.Add("[i:2588]", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:2588]", false);
                    }
                }
                else if (NPC.downedFishron)
                {
                    defeatedbosses.Add("[i:2588]", true);
                }
                if ((bool)getenabledboss.AllowPlantera)
                {
                    if (NPC.downedPlantBoss)
                    {
                        defeatedbosses.Add("[i:2109]", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:2109]", false);
                    }
                }
                else if (NPC.downedPlantBoss)
                {
                    defeatedbosses.Add("[i:2109]", true);
                }
                if ((bool)getenabledboss.AllowEmpressOfLight)
                {
                    if (NPC.downedEmpressOfLight)
                    {
                        defeatedbosses.Add("[i:4784]", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:4784]", false);
                    }
                }
                else if (NPC.downedEmpressOfLight)
                {
                    defeatedbosses.Add("[i:4784]", true);
                }
                if ((bool)getenabledboss.AllowGolem)
                {
                    if (NPC.downedGolemBoss)
                    {
                        defeatedbosses.Add("[i:2110]", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:2110]", false);
                    }
                }
                else if (NPC.downedGolemBoss)
                {
                    defeatedbosses.Add("[i:2110]", true);
                }
                if ((bool)getenabledboss.AllowLunaticCultist)
                {
                    if (NPC.downedAncientCultist)
                    {
                        defeatedbosses.Add("[i:3372]", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:3372]", false);
                    }
                }
                else if (NPC.downedAncientCultist)
                {
                    defeatedbosses.Add("[i:3372]", true);
                }
                if ((bool)getenabledboss.AllowMoonLord)
                {
                    if (NPC.downedMoonlord)
                    {
                        defeatedbosses.Add("[i:3373]", true);
                    }
                    else
                    {
                        defeatedbosses.Add("[i:3373]", false);
                    }
                }
                else if (NPC.downedMoonlord)
                {
                    defeatedbosses.Add("[i:3373]", true);
                }
                string getdefeatedboss = "";
                string getenableboss = "";
                foreach (var boss in defeatedbosses)
                {
                    if (boss.Value)
                    {
                        getdefeatedboss += $"{boss.Key},";
                    } else
                    {
                        getenableboss += $"{boss.Key},";
                    }
                }

                string result =
                    $"[c/25ba14:Defeated Bosses:] {getdefeatedboss}\n" +
                    $"[c/e0e50f:Enabled Bosses:] {getenableboss}";

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

            #region { GetNextBossSchedule }

            string GetNextBossSchedule()
            {
                string result = "";

                DateTime nextsched = DateTime.MaxValue;

                if (!(bool)Config.BossManager.AllowKingSlime && !NPC.downedSlimeKing)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowKingSlime)
                    {
                        result = "\n\nNext Boss is King Slime in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowKingSlime;
                    }
                }

                if (!(bool)Config.BossManager.AllowEyeOfCthulhu && !NPC.downedBoss1)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowEyeOfCthulhu)
                    {
                        result = "\n\nNext Boss is Eye Of Cthulhu in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowEyeOfCthulhu;
                    }
                }

                if (!(bool)Config.BossManager.AllowEaterOfWorlds && !NPC.downedBoss2)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowEaterOfWorlds)
                    {
                        result = "\n\nNext Boss is Eater Of Worlds in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowEaterOfWorlds;
                    }
                }

                if (!(bool)Config.BossManager.AllowDeerclops && !NPC.downedDeerclops)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowDeerclops)
                    {
                        result = "\n\nNext Boss is Deerclops in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowDeerclops;
                    }
                }

                if (!(bool)Config.BossManager.AllowQueenBee && !NPC.downedQueenBee)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowQueenBee)
                    {
                        result = "\n\nNext Boss is Queen Bee in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowQueenBee;
                    }
                }

                if (!(bool)Config.BossManager.AllowSkeletron && !NPC.downedBoss3)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowSkeletron)
                    {
                        result = "\n\nNext Boss is Skeletron in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowSkeletron;
                    }
                }

                if (!(bool)Config.BossManager.AllowWallOfFlesh && !Main.hardMode)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowWallOfFlesh)
                    {
                        result = "\n\nNext Boss is Wall of Flesh in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowWallOfFlesh;
                    }
                }

                if (!(bool)Config.BossManager.AllowQueenSlime && !NPC.downedQueenSlime)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowQueenSlime)
                    {
                        result = "\n\nNext Boss is Queen Slime in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowQueenSlime;
                    }
                }

                if (!(bool)Config.BossManager.AllowTheDestroyer && !NPC.downedMechBoss1)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowTheDestroyer)
                    {
                        result = "\n\nNext Boss is The Destroyer in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowTheDestroyer;
                    }
                }

                if (!(bool)Config.BossManager.AllowTheTwins && !NPC.downedMechBoss2)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowTheTwins)
                    {
                        result = "\n\nNext Boss is The Twins in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowTheTwins;
                    }
                }

                if (!(bool)Config.BossManager.AllowSkeletronPrime && !NPC.downedMechBoss3)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowSkeletronPrime)
                    {
                        result = "\n\nNext Boss is Skeletron Prime in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowSkeletronPrime;
                    }
                }

                if (!(bool)Config.BossManager.AllowDukeFishron && !NPC.downedFishron)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowDukeFishron)
                    {
                        result = "\n\nNext Boss is Duke Fishron in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowDukeFishron;
                    }
                }

                if (!(bool)Config.BossManager.AllowPlantera && !NPC.downedPlantBoss)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowPlantera)
                    {
                        result = "\n\nNext Boss is Plantera in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowPlantera;
                    }
                }

                if (!(bool)Config.BossManager.AllowEmpressOfLight && !NPC.downedEmpressOfLight)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowEmpressOfLight)
                    {
                        result = "\n\nNext Boss is Empress Of Light in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowEmpressOfLight;
                    }
                }

                if (!(bool)Config.BossManager.AllowGolem && !NPC.downedGolemBoss)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowGolem)
                    {
                        result = "\n\nNext Boss is Golem in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowGolem;
                    }
                }

                if (!(bool)Config.BossManager.AllowLunaticCultist && !NPC.downedAncientCultist)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowLunaticCultist)
                    {
                        result = "\n\nNext Boss is Lunatic Cultist in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowLunaticCultist;
                    }
                }

                if (!(bool)Config.BossManager.AllowMoonLord && !NPC.downedMoonlord)
                {
                    if (nextsched > (DateTime)Config.BossManager.ScheduleAllowMoonLord)
                    {
                        result = "\n\nNext Boss is Moon Lord in ";
                        nextsched = (DateTime)Config.BossManager.ScheduleAllowMoonLord;
                    }
                }

                if (nextsched == DateTime.MaxValue) return "";

                string GetTimeString(DateTime datetime)
                {
                    TimeSpan getresult = (datetime - DateTime.UtcNow);

                    if (getresult.TotalDays >= 1)
                    {
                        return $"{Math.Floor(getresult.TotalDays)}{(getresult.TotalDays >= 2 ? "Days" : "Day")}";
                    }
                    if (getresult.TotalHours >= 1)
                    {
                        return $"{Math.Floor(getresult.TotalHours)}{(getresult.TotalHours >= 2 ? "Hours" : "Hour")}";
                    }
                    if (getresult.TotalMinutes >= 1)
                    {
                        return $"{Math.Floor(getresult.TotalMinutes)}{(getresult.TotalMinutes >= 2 ? "Minutes" : "Minute")}";
                    }
                    if (getresult.TotalSeconds >= 1)
                    {
                        return $"{Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds >= 2 ? "Seconds" : "Second")}";
                    }
                    if (getresult.TotalMilliseconds >= 1)
                    {
                        return $"{Math.Floor(getresult.TotalMilliseconds)}{(getresult.TotalMilliseconds >= 2 ? "Milliseconds" : "Millisecond")}";
                    }
                    return $"Time {Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds >= 2 ? "Seconds" : "Second")}";
                }

                return result + GetTimeString(nextsched);
            }

            #endregion

            args.Player.SendMessage(
                $"List Of Bosses:" +
                $"\n{GetListDefeatedBoss2()}{GetNextBossSchedule()}",
                Color.Gray);

            #endregion
        }

        private static Dictionary<string, DateTime> ReportCD = new();
        private void CMD_Report(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage($"Usage: {Commands.Specifier}report <message>" +
                    $"\nor you can use '{Commands.Specifier}report <player> <message>'" +
                    $"\nexample: {Commands.Specifier}report {args.Player.Name} is cheating");
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
            #endregion
        }

        #endregion

        #region { Staff }

        private void CMD_StaffChat(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage($"Usage: {Commands.Specifier}staffchat <message>" +
                    $"\nshortcuts: {Commands.Specifier}staff, {Commands.Specifier}#");
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
                        ulong getuserid = (bool)Config.DataBaseDLink.Target_UserAccount_ID ? LinkAccountManager.GetUserIDByAccountID(tsplayer.Account.ID) : LinkAccountManager.GetUserIDByAccountName(tsplayer.Account.Name);

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
                        ulong getuserid = (bool)Config.DataBaseDLink.Target_UserAccount_ID ? LinkAccountManager.GetUserIDByAccountID(tsplayer.Account.ID) : LinkAccountManager.GetUserIDByAccountName(tsplayer.Account.Name);

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
                        ulong getuserid = (bool)Config.DataBaseDLink.Target_UserAccount_ID ? LinkAccountManager.GetUserIDByAccountID(tsplayer.Account.ID) : LinkAccountManager.GetUserIDByAccountName(tsplayer.Account.Name);

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
                        ulong getuserid = getuserid = (bool)Config.DataBaseDLink.Target_UserAccount_ID ? LinkAccountManager.GetUserIDByAccountID(tsplayer.Account.ID) : LinkAccountManager.GetUserIDByAccountName(tsplayer.Account.Name);

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

        private void CMD_ClearMessage(CommandArgs args)
        {
            #region code

            for (int i = 0; i < 130; i++)
            {
                TSPlayer.All.SendMessage("\n\n\n\n", Color.Black);
            }

            args.Player.SendSuccessMessage("Message Cleared!");
            
            #endregion
        }

        private void CMD_LockDown(CommandArgs args)
        {
            #region code
            if (!LockDown)
            {
                if (args.Parameters.Count == 0)
                {
                    LockDown = true;
                    TShock.Utils.Broadcast("Server is on LockDown!", Color.OrangeRed);
                    Discordklp.KLPBotSendMessageMainLog($"**🔒{args.Player.Name}🔒** Server is on lockdown!");
                }
                else
                {
                    LockDown = true;
                    LockDownReason = string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count);
                    TShock.Utils.Broadcast("Server is on LockDown by the reason of " + LockDownReason, Color.OrangeRed);
                    Discordklp.KLPBotSendMessageMainLog($"**🔒{args.Player.Name}🔒** Server is on lockdown! `reason: {LockDownReason}`");
                }
            } else
            {
                LockDown = false;
                TShock.Utils.Broadcast("Server is no longer on LockDown!", Color.LightGreen);
                Discordklp.KLPBotSendMessageMainLog($"**🔓{args.Player.Name}🔓** Server is no longer on lockdown!");
            }
            

            #endregion
        }

        private void CMD_LockDownRegister(CommandArgs args)
        {
            #region code
            if (!LockDownRegister)
            {
                LockDownRegister = true;
                args.Player.SendSuccessMessage("Guest can no longer resgister!");
                Discordklp.KLPBotSendMessageMainLog($"**🔒{args.Player.Name}🔒** Guest can no longer register!");
            }
            else
            {
                LockDownRegister = false;
                args.Player.SendSuccessMessage("Guest can now resgister!");
                Discordklp.KLPBotSendMessageMainLog($"**🔓{args.Player.Name}🔓** Guest can now register!");
            }


            #endregion
        }

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

        private void CMD_ClearLag(CommandArgs args)
        {
            #region code
            int ClearedItems = 0;
            for (int i = 0; i < Main.maxItems; i++)
            {
                if (Main.item[i].value < Config.Main.Ignore_Value_ClearLag && Main.item[i].active)
                {
                    Main.item[i].active = false;
                    TSPlayer.All.SendData(PacketTypes.ItemDrop, "", i);
                    ClearedItems++;
                }
            }
            int ClearedNPC = 0;
            int[] IgnoreNPCIDs = {
                NPCID.DungeonGuardian,

                NPCID.DD2EterniaCrystal,
                NPCID.DD2LanePortal,

                NPCID.DD2DarkMageT1,
                NPCID.DD2DarkMageT3,
                NPCID.DD2OgreT2,
                NPCID.DD2OgreT3,
                NPCID.DD2Betsy,

                NPCID.PirateShip,
                NPCID.PirateShipCannon,

                NPCID.Everscream,
                NPCID.SantaNK1,
                NPCID.IceQueen,

                NPCID.MourningWood,
                NPCID.Pumpking,

                NPCID.MartianSaucer,
                NPCID.MartianSaucerCannon,
                NPCID.MartianSaucerCore,
                NPCID.MartianSaucerTurret,

                NPCID.LunarTowerSolar,
                NPCID.LunarTowerNebula,
                NPCID.LunarTowerStardust,
                NPCID.LunarTowerVortex,
            };
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].boss) continue;
                if (Main.npc[i].townNPC) continue;
                if (IgnoreNPCIDs.Contains(Main.npc[i].type)) continue;
                if (Main.npc[i].rarity > 0) continue;
                if (Main.npc[i].active)
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                    ClearedNPC++;
                }
            }
            int ClearedProjectile = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].minion) continue;
                if (Main.projectile[i].active)
                {
                    Main.projectile[i].active = false;
                    Main.projectile[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", i);
                    ClearedProjectile++;
                }
            }

            TShock.Utils.Broadcast($"[MKLP] ClearLag - {args.Player.Name} Removed" +
                $" {(ClearedItems >= 2 ? $"{ClearedItems} Items" : $"{ClearedItems} Item")}" +
                $" {(ClearedProjectile >= 2 ? $"{ClearedProjectile} Projectiles" : $"{ClearedProjectile} Projectile")}" +
                $" {(ClearedNPC >= 2 ? $"{ClearedNPC} Entities" : $"{ClearedNPC} Entity")}", Color.Yellow);
            #endregion
        }

        private void CMD_ManageBoss(CommandArgs args)
        {
            #region code

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage($"Proper usage: {Commands.Specifier}manageboss <type> <args...>" +
                    $"\ndo '{Commands.Specifier}manageboss help' for more details");
                return;
            }

            switch (args.Parameters[0].ToLower())
            {
                #region [ Help Text ]
                case "help":
                    {
                        args.Player.SendMessage($"Proper Usage: [c/31ff77:{Commands.Specifier}manageboss <type> <args...>]" +
                            "\n[c/ffd531:== Available Sub-Command ==]" +
                            "\n'enable <boss name>' : Enable a boss" +
                            "\n[c/b6b6b6:'disable <boss name>' : Disable a boss to prevent it from spawning]" +
                            "\n'enableall' : Enables all bosses" +
                            "\n[c/b6b6b6:'disableall' : Disables all bosses]" +
                            $"{(args.Player.HasPermission(Config.Permissions.CMD_ManageBoss_SetKilled) ? "'setkilled <boss name>' : set boss killed or not\n" : "")}\n" +
                            "\n[c/96b85f:== Boss Schedule Sub-Command ==]" +
                            "\n'enablesched <boss name> <MM/DD/YY>' : Enable a boss in specific time" +
                            "\n[c/b6b6b6:'disablesched <boss name> : cancel a specific boss schedule]" +
                            "\n'disableschedall' : cancel all boss schedule" +
                            "\n[c/b6b6b6:'usingschedule <yes/no>' : activate or deactivate boss schedule]" +
                            "\n'resetschedule' : Restart the whole boss schedule to day 0 and assign the bosses each days on config file"
                            , Color.WhiteSmoke);
                        return;
                    }
                #endregion
                #region ( Type : Enable )
                case "enable":
                    {
                        switch (args.Parameters[1].ToLower())
                        {
                            case "kingslime":
                            case "king slime":
                            case "king":
                            case "ks":
                                {
                                    if ((bool)Config.BossManager.AllowKingSlime)
                                    {
                                        args.Player.SendErrorMessage("King Slime is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowKingSlime = true;
                                    args.Player.SendInfoMessage("King Slime is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("King Slime", args.Player.Name);
                                    break;
                                }
                            case "eyeofcthulhu":
                            case "eye of cthulhu":
                            case "eye":
                            case "eoc":
                                {
                                    if ((bool)Config.BossManager.AllowEyeOfCthulhu)
                                    {
                                        args.Player.SendErrorMessage("Eye Of Cthulhu is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowEyeOfCthulhu = true;
                                    args.Player.SendInfoMessage("Eye Of Cthulhu is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Eye of Cthulhu", args.Player.Name);
                                    break;
                                }
                            case "evilboss":
                            case "evil boss":
                                {
                                    if ((bool)Config.BossManager.AllowEaterOfWorlds && (bool)Config.BossManager.AllowBrainOfCthulhu)
                                    {
                                        args.Player.SendErrorMessage("Eater Of Worlds & Brain Of Cthulhu is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowEaterOfWorlds = true;
                                    Config.BossManager.AllowBrainOfCthulhu = true;
                                    args.Player.SendInfoMessage("Eater Of Worlds & Brain Of Cthulhu is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Eater of Worlds & Brain of Cthulhu", args.Player.Name);
                                    break;
                                }
                            case "eow":
                            case "eaterofworlds":
                            case "eater of worlds":
                            case "eater":
                                {
                                    if ((bool)Config.BossManager.AllowEaterOfWorlds)
                                    {
                                        args.Player.SendErrorMessage("Eater Of Worlds is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowEaterOfWorlds = true;
                                    args.Player.SendInfoMessage("Eater Of Worlds is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Eater of Worlds", args.Player.Name);
                                    break;
                                }
                            case "boc":
                            case "brainofcthulhu":
                            case "brain of cthulhu":
                            case "brain":
                                {
                                    if ((bool)Config.BossManager.AllowBrainOfCthulhu)
                                    {
                                        args.Player.SendErrorMessage("Brain Of Cthulhu is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowBrainOfCthulhu = true;
                                    args.Player.SendInfoMessage("Brain Of Cthulhu is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Brain of Cthulhu", args.Player.Name);
                                    break;
                                }
                            case "skeletron":
                            case "sans":
                                {
                                    if ((bool)Config.BossManager.AllowSkeletron)
                                    {
                                        args.Player.SendErrorMessage("Skeletron is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowSkeletron = true;
                                    args.Player.SendInfoMessage("Skeletron is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Skeletron", args.Player.Name);
                                    break;
                                }
                            case "queenbee":
                            case "queen bee":
                            case "qb":
                                {
                                    if ((bool)Config.BossManager.AllowQueenBee)
                                    {
                                        args.Player.SendErrorMessage("Queen Bee is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowQueenBee = true;
                                    args.Player.SendInfoMessage("Queen Bee is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Queen Bee", args.Player.Name);
                                    break;
                                }
                            case "deerclops":
                            case "deer clops":
                            case "deer":
                                {
                                    if ((bool)Config.BossManager.AllowDeerclops)
                                    {
                                        args.Player.SendErrorMessage("Deerclops is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowDeerclops = true;
                                    args.Player.SendInfoMessage("Deerclops is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Deerclops", args.Player.Name);
                                    break;
                                }
                            case "wall of flesh":
                            case "wallofflesh":
                            case "wof":
                                {
                                    if ((bool)Config.BossManager.AllowWallOfFlesh)
                                    {
                                        args.Player.SendErrorMessage("Wall Of Flesh is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowWallOfFlesh = true;
                                    args.Player.SendInfoMessage("Wall Of Flesh is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Wall of Flesh", args.Player.Name);
                                    break;
                                }
                            case "queenslime":
                            case "queen slime":
                            case "qs":
                                {
                                    if ((bool)Config.BossManager.AllowQueenSlime)
                                    {
                                        args.Player.SendErrorMessage("Queen Slime is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowQueenSlime = true;
                                    args.Player.SendInfoMessage("Queen Slime is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Queen Slime", args.Player.Name);
                                    break;
                                }
                            case "mech1":
                            case "thedestroyer":
                            case "the destroyer":
                            case "destroyer":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        if ((bool)Config.BossManager.AllowMechdusa)
                                        {
                                            args.Player.SendErrorMessage("Mechdusa is already Enabled!");
                                            return;
                                        }

                                        Config.BossManager.AllowMechdusa = false;
                                        args.Player.SendInfoMessage("Mechdusa is now enabled");
                                        Discordklp.KLPBotSendMessage_BossEnabled("Mechdusa", args.Player.Name);
                                        break;
                                    }
                                    if ((bool)Config.BossManager.AllowTheDestroyer)
                                    {
                                        args.Player.SendErrorMessage("The Destroyer is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowTheDestroyer = true;
                                    args.Player.SendInfoMessage("The Destroyer is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Destroyer", args.Player.Name);
                                    break;
                                }
                            case "mech2":
                            case "thetwins":
                            case "the twins":
                            case "twins":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        if ((bool)Config.BossManager.AllowMechdusa)
                                        {
                                            args.Player.SendErrorMessage("Mechdusa is already Enabled!");
                                            return;
                                        }

                                        Config.BossManager.AllowMechdusa = false;
                                        args.Player.SendInfoMessage("Mechdusa is now enabled");
                                        Discordklp.KLPBotSendMessage_BossEnabled("Mechdusa", args.Player.Name);
                                        break;
                                    }
                                    if ((bool)Config.BossManager.AllowTheTwins)
                                    {
                                        args.Player.SendErrorMessage("The Twins is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowTheTwins = true;
                                    args.Player.SendInfoMessage("The Twins is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("The Twins", args.Player.Name);
                                    break;
                                }
                            case "mech3":
                            case "skeletronprime":
                            case "skeletron prime":
                            case "prime":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        if ((bool)Config.BossManager.AllowMechdusa)
                                        {
                                            args.Player.SendErrorMessage("Mechdusa is already Enabled!");
                                            return;
                                        }

                                        Config.BossManager.AllowMechdusa = false;
                                        args.Player.SendInfoMessage("Mechdusa is now enabled");
                                        Discordklp.KLPBotSendMessage_BossEnabled("Mechdusa", args.Player.Name);
                                        break;
                                    }
                                    if ((bool)Config.BossManager.AllowSkeletronPrime)
                                    {
                                        args.Player.SendErrorMessage("Skeletron Prime is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowSkeletronPrime = true;
                                    args.Player.SendInfoMessage("Skeletron Prime is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Skeletron Prime", args.Player.Name);
                                    break;
                                }
                            case "mechdusa":
                                {
                                    if (!Main.zenithWorld)
                                    {
                                        args.Player.SendErrorMessage("Mechdusa is only available on zenith seed!");
                                        return;
                                    }

                                    if ((bool)Config.BossManager.AllowMechdusa)
                                    {
                                        args.Player.SendErrorMessage("Mechdusa is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowMechdusa = false;
                                    args.Player.SendInfoMessage("Mechdusa is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Mechdusa", args.Player.Name);
                                    break;
                                }
                            case "plantera":
                                {
                                    if ((bool)Config.BossManager.AllowPlantera)
                                    {
                                        args.Player.SendErrorMessage("Plantera is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowPlantera = true;
                                    args.Player.SendInfoMessage("Plantera is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Plantera", args.Player.Name);
                                    break;
                                }
                            case "golem":
                                {
                                    if ((bool)Config.BossManager.AllowGolem)
                                    {
                                        args.Player.SendErrorMessage("Golem is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowGolem = true;
                                    args.Player.SendInfoMessage("Golem is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Golem", args.Player.Name);
                                    break;
                                }
                            case "duke":
                            case "fishron":
                            case "dukefishron":
                            case "duke fishron":
                                {
                                    if ((bool)Config.BossManager.AllowDukeFishron)
                                    {
                                        args.Player.SendErrorMessage("Duke Fishron is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowDukeFishron = true;
                                    args.Player.SendInfoMessage("Duke Fishron is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Duke Fishron", args.Player.Name);
                                    break;
                                }
                            case "cultist":
                            case "lunatic":
                            case "lunaticcultist":
                            case "lunatic cultist":
                                {
                                    if ((bool)Config.BossManager.AllowLunaticCultist)
                                    {
                                        args.Player.SendErrorMessage("Lunatic Cultist is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowLunaticCultist = true;
                                    args.Player.SendInfoMessage("Lunatic Cultist is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Lunatic Cultist", args.Player.Name);
                                    break;
                                }

                            case "empress":
                            case "eol":
                            case "empressoflight":
                            case "empress of light":
                                {
                                    if ((bool)Config.BossManager.AllowEmpressOfLight)
                                    {
                                        args.Player.SendErrorMessage("Empress Of Light is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowEmpressOfLight = true;
                                    args.Player.SendInfoMessage("Empress Of Light is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Empress of Light", args.Player.Name);
                                    break;
                                }
                            case "moonlord":
                            case "moon lord":
                            case "ml":
                                {
                                    if ((bool)Config.BossManager.AllowMoonLord)
                                    {
                                        args.Player.SendErrorMessage("Moon Lord is already Enabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowMoonLord = true;
                                    args.Player.SendInfoMessage("Moon Lord is now enabled");
                                    Discordklp.KLPBotSendMessage_BossEnabled("Moon Lord", args.Player.Name);
                                    break;
                                }
                            default:
                                {
                                    args.Player.SendErrorMessage("Please specify the boss!");
                                    return;
                                }
                        }
                        Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : Disabled )
                case "disable":
                    {
                        switch (args.Parameters[1].ToLower())
                        {
                            case "kingslime":
                            case "king slime":
                            case "king":
                            case "ks":
                                {
                                    if (!(bool)Config.BossManager.AllowKingSlime)
                                    {
                                        args.Player.SendErrorMessage("King Slime is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowKingSlime = false;
                                    args.Player.SendInfoMessage("King Slime is now disabled");
                                    break;
                                }
                            case "eyeofcthulhu":
                            case "eye of cthulhu":
                            case "eye":
                            case "eoc":
                                {
                                    if (!(bool)Config.BossManager.AllowEyeOfCthulhu)
                                    {
                                        args.Player.SendErrorMessage("Eye Of Cthulhu is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowEyeOfCthulhu = false;
                                    args.Player.SendInfoMessage("Eye Of Cthulhu is now disabled");
                                    break;
                                }
                            case "evilboss":
                            case "evil boss":
                                {
                                    if (!(bool)Config.BossManager.AllowEaterOfWorlds && !(bool)Config.BossManager.AllowBrainOfCthulhu)
                                    {
                                        args.Player.SendErrorMessage("Eater Of Worlds & Brain Of Cthulhu is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowEaterOfWorlds = false;
                                    Config.BossManager.AllowBrainOfCthulhu = false;
                                    args.Player.SendInfoMessage("Eater Of Worlds & Brain Of Cthulhu is now disabled");
                                    break;
                                }
                            case "eow":
                            case "eaterofworlds":
                            case "eater of worlds":
                            case "eater":
                                {
                                    if (!(bool)Config.BossManager.AllowEaterOfWorlds)
                                    {
                                        args.Player.SendErrorMessage("Eater Of Worlds is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowEaterOfWorlds = false;
                                    args.Player.SendInfoMessage("Eater Of Worlds is now disabled");
                                    break;
                                }
                            case "boc":
                            case "brainofcthulhu":
                            case "brain of cthulhu":
                            case "brain":
                                {
                                    if (!(bool)Config.BossManager.AllowBrainOfCthulhu)
                                    {
                                        args.Player.SendErrorMessage("Brain Of Cthulhu is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowBrainOfCthulhu = false;
                                    args.Player.SendInfoMessage("Brain Of Cthulhu is now disabled");
                                    break;
                                }
                            case "skeletron":
                            case "sans":
                                {
                                    if (!(bool)Config.BossManager.AllowSkeletron)
                                    {
                                        args.Player.SendErrorMessage("Skeletron is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowSkeletron = false;
                                    args.Player.SendInfoMessage("Skeletron is now disabled");
                                    break;
                                }
                            case "queenbee":
                            case "queen bee":
                            case "qb":
                                {
                                    if (!(bool)Config.BossManager.AllowQueenBee)
                                    {
                                        args.Player.SendErrorMessage("Queen Bee is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowQueenBee = false;
                                    args.Player.SendInfoMessage("Queen Bee is now disabled");
                                    break;
                                }
                            case "deerclops":
                            case "deer clops":
                            case "deer":
                                {
                                    if (!(bool)Config.BossManager.AllowDeerclops)
                                    {
                                        args.Player.SendErrorMessage("Deerclops is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowDeerclops = false;
                                    args.Player.SendInfoMessage("Deerclops is now disabled");
                                    break;
                                }
                            case "wall of flesh":
                            case "wallofflesh":
                            case "wof":
                                {
                                    if (!(bool)Config.BossManager.AllowWallOfFlesh)
                                    {
                                        args.Player.SendErrorMessage("Wall Of Flesh is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowWallOfFlesh = false;
                                    args.Player.SendInfoMessage("Wall Of Flesh is now disabled");
                                    break;
                                }
                            case "queenslime":
                            case "queen slime":
                            case "qs":
                                {
                                    if (!(bool)Config.BossManager.AllowQueenSlime)
                                    {
                                        args.Player.SendErrorMessage("Queen Slime is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowQueenSlime = false;
                                    args.Player.SendInfoMessage("Queen Slime is now disabled");
                                    break;
                                }
                            case "mech1":
                            case "thedestroyer":
                            case "the destroyer":
                            case "destroyer":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        if (!(bool)Config.BossManager.AllowMechdusa)
                                        {
                                            args.Player.SendErrorMessage("Mechdusa is already Disabled!");
                                            return;
                                        }

                                        Config.BossManager.AllowMechdusa = false;
                                        args.Player.SendInfoMessage("Mechdusa is now disabled");
                                        break;
                                    }
                                    if (!(bool)Config.BossManager.AllowTheDestroyer)
                                    {
                                        args.Player.SendErrorMessage("The Destroyer is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowTheDestroyer = false;
                                    args.Player.SendInfoMessage("The Destroyer is now disabled");
                                    break;
                                }
                            case "mech2":
                            case "thetwins":
                            case "the twins":
                            case "twins":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        if (!(bool)Config.BossManager.AllowMechdusa)
                                        {
                                            args.Player.SendErrorMessage("Mechdusa is already Disabled!");
                                            return;
                                        }

                                        Config.BossManager.AllowMechdusa = false;
                                        args.Player.SendInfoMessage("Mechdusa is now disabled");
                                        break;
                                    }
                                    if (!(bool)Config.BossManager.AllowTheTwins)
                                    {
                                        args.Player.SendErrorMessage("The Twins is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowTheTwins = false;
                                    args.Player.SendInfoMessage("The Twins is now disabled");
                                    break;
                                }
                            case "mech3":
                            case "skeletronprime":
                            case "skeletron prime":
                            case "prime":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        if (!(bool)Config.BossManager.AllowMechdusa)
                                        {
                                            args.Player.SendErrorMessage("Mechdusa is already Disabled!");
                                            return;
                                        }

                                        Config.BossManager.AllowMechdusa = false;
                                        args.Player.SendInfoMessage("Mechdusa is now disabled");
                                        break;
                                    }
                                    if (!(bool)Config.BossManager.AllowSkeletronPrime)
                                    {
                                        args.Player.SendErrorMessage("Skeletron Prime is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowSkeletronPrime = false;
                                    args.Player.SendInfoMessage("Skeletron Prime is now disabled");
                                    break;
                                }
                            case "mechdusa":
                                {
                                    if (!Main.zenithWorld)
                                    {
                                        args.Player.SendErrorMessage("Mechdusa is only available on zenith seed!");
                                        return;
                                    }

                                    if (!(bool)Config.BossManager.AllowMechdusa)
                                    {
                                        args.Player.SendErrorMessage("Mechdusa is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowMechdusa = false;
                                    args.Player.SendInfoMessage("Mechdusa is now disabled");
                                    break;
                                }
                            case "plantera":
                                {
                                    if (!(bool)Config.BossManager.AllowPlantera)
                                    {
                                        args.Player.SendErrorMessage("Plantera is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowPlantera = false;
                                    args.Player.SendInfoMessage("Plantera is now disabled");
                                    break;
                                }
                            case "golem":
                                {
                                    if (!(bool)Config.BossManager.AllowGolem)
                                    {
                                        args.Player.SendErrorMessage("Golem is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowGolem = false;
                                    args.Player.SendInfoMessage("Golem is now disabled");
                                    break;
                                }
                            case "duke":
                            case "fishron":
                            case "dukefishron":
                            case "duke fishron":
                                {
                                    if (!(bool)Config.BossManager.AllowDukeFishron)
                                    {
                                        args.Player.SendErrorMessage("Duke Fish is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowDukeFishron = false;
                                    args.Player.SendInfoMessage("Duke Fish is now disable");
                                    break;
                                }
                            case "cultist":
                            case "lunatic":
                            case "lunaticcultist":
                            case "lunatic cultist":
                                {
                                    if (!(bool)Config.BossManager.AllowLunaticCultist)
                                    {
                                        args.Player.SendErrorMessage("Lunatic Cultist is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowLunaticCultist = false;
                                    args.Player.SendInfoMessage("Lunatic Cultist is now disabled");
                                    break;
                                }

                            case "empress":
                            case "eol":
                            case "empressoflight":
                            case "empress of light":
                                {
                                    if (!(bool)Config.BossManager.AllowEmpressOfLight)
                                    {
                                        args.Player.SendErrorMessage("Empress Of Light is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowEmpressOfLight = false;
                                    args.Player.SendInfoMessage("Empress Of Light is now disabled");
                                    break;
                                }
                            case "moonlord":
                            case "moon lord":
                            case "ml":
                                {
                                    if (!(bool)Config.BossManager.AllowMoonLord)
                                    {
                                        args.Player.SendErrorMessage("Moon Lord is already Disabled!");
                                        return;
                                    }

                                    Config.BossManager.AllowMoonLord = false;
                                    args.Player.SendInfoMessage("Moon Lord is now disabled");
                                    break;
                                }
                            default:
                                {
                                    args.Player.SendErrorMessage("Please specify the boss!");
                                    return;
                                }
                        }
                        Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : EnableAll )
                case "enableall":
                    {
                        Config.BossManager.AllowKingSlime = true;
                        Config.BossManager.AllowEyeOfCthulhu = true;
                        Config.BossManager.AllowEaterOfWorlds = true;
                        Config.BossManager.AllowBrainOfCthulhu = true;
                        Config.BossManager.AllowQueenBee = true;
                        Config.BossManager.AllowDeerclops = true;
                        Config.BossManager.AllowSkeletron = true;
                        Config.BossManager.AllowWallOfFlesh = true;
                        Config.BossManager.AllowQueenSlime = true;
                        Config.BossManager.AllowTheTwins = true;
                        Config.BossManager.AllowTheDestroyer = true;
                        Config.BossManager.AllowSkeletronPrime = true;
                        Config.BossManager.AllowMechdusa = true;
                        Config.BossManager.AllowDukeFishron = true;
                        Config.BossManager.AllowPlantera = true;
                        Config.BossManager.AllowEmpressOfLight = true;
                        Config.BossManager.AllowGolem = true;
                        Config.BossManager.AllowLunaticCultist = true;
                        Config.BossManager.AllowMoonLord = true;

                        args.Player.SendInfoMessage("All Bosses are enabled");
                        Discordklp.KLPBotSendMessage_BossEnabled("All Bosses", args.Player.Name);
                        Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : DisableAll )
                case "disableall":
                    {
                        Config.BossManager.AllowKingSlime = false;
                        Config.BossManager.AllowEyeOfCthulhu = false;
                        Config.BossManager.AllowEaterOfWorlds = false;
                        Config.BossManager.AllowBrainOfCthulhu = false;
                        Config.BossManager.AllowQueenBee = false;
                        Config.BossManager.AllowDeerclops = false;
                        Config.BossManager.AllowSkeletron = false;
                        Config.BossManager.AllowWallOfFlesh = false;
                        Config.BossManager.AllowQueenSlime = false;
                        Config.BossManager.AllowTheTwins = false;
                        Config.BossManager.AllowTheDestroyer = false;
                        Config.BossManager.AllowSkeletronPrime = false;
                        Config.BossManager.AllowMechdusa = false;
                        Config.BossManager.AllowDukeFishron = false;
                        Config.BossManager.AllowPlantera = false;
                        Config.BossManager.AllowEmpressOfLight = false;
                        Config.BossManager.AllowGolem = false;
                        Config.BossManager.AllowLunaticCultist = false;
                        Config.BossManager.AllowMoonLord = false;

                        args.Player.SendInfoMessage("All Bosses are disabled!");
                        Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : SetKilled )
                case "setkilled":
                case "setkill":
                    {
                        if (!args.Player.HasPermission(Config.Permissions.CMD_ManageBoss_SetKilled))
                        {
                            args.Player.SendErrorMessage("You do not have permission to set bosses as kill or not!");
                            return;
                        }
                        switch (args.Parameters[1].ToLower())
                        {
                            case "kingslime":
                            case "king slime":
                            case "king":
                            case "ks":
                                {
                                    NPC.downedSlimeKing = !NPC.downedSlimeKing;
                                    args.Player.SendInfoMessage($"Set King Slime as {(NPC.downedSlimeKing ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }
                            case "eyeofcthulhu":
                            case "eye of cthulhu":
                            case "eye":
                            case "eoc":
                                {
                                    NPC.downedBoss1 = !NPC.downedBoss1;
                                    args.Player.SendInfoMessage($"Set Eye of Cthulhu as {(NPC.downedBoss1 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }
                            case "evilboss":
                            case "evil boss":
                            case "boc":
                            case "eow":
                            case "eaterofworlds":
                            case "eater of worlds":
                            case "brainofcthulhu":
                            case "brain of cthulhu":
                            case "brain":
                            case "eater":
                                {
                                    NPC.downedBoss2 = !NPC.downedBoss2;
                                    args.Player.SendInfoMessage($"Set {(WorldGen.crimson ? "Brain of Cthulhu" : "Eater of Worlds")} as {(NPC.downedBoss2 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }
                            case "skeletron":
                            case "sans":
                                {
                                    NPC.downedBoss3 = !NPC.downedBoss3;
                                    args.Player.SendInfoMessage($"Set Skeletron as {(NPC.downedBoss3 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }
                            case "queenbee":
                            case "queen bee":
                            case "qb":
                                {
                                    NPC.downedQueenBee = !NPC.downedQueenBee;
                                    args.Player.SendInfoMessage($"Set Queen Bee as {(NPC.downedQueenBee ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }
                            case "deerclops":
                            case "deer clops":
                            case "deer":
                                {
                                    NPC.downedDeerclops = !NPC.downedDeerclops;
                                    args.Player.SendInfoMessage($"Set Deerclops as {(NPC.downedDeerclops ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }
                            case "hardmode":
                            case "wallofflesh":
                            case "wall of flesh":
                            case "wof":
                                {
                                    Main.hardMode = !Main.hardMode;
                                    args.Player.SendInfoMessage($"Set Wall of Flesh (Hardmode) as {(Main.hardMode ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    args.Player.SendInfoMessage("Note: This is the same as the '/hardmode' command.");
                                    return;
                                }
                            case "queenslime":
                            case "queen slime":
                            case "qs":
                                {
                                    NPC.downedQueenSlime = !NPC.downedQueenSlime;
                                    args.Player.SendInfoMessage($"Set Queen Slime as {(NPC.downedQueenSlime ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }
                            case "mech1":
                            case "thedestroyer":
                            case "the destroyer":
                            case "destroyer":
                                {
                                    NPC.downedMechBoss1 = !NPC.downedMechBoss1;
                                    args.Player.SendInfoMessage($"Set The Destroyer as {(NPC.downedMechBoss1 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }
                            case "mech2":
                            case "thetwins":
                            case "the twins":
                            case "twins":
                                {
                                    NPC.downedMechBoss2 = !NPC.downedMechBoss2;
                                    args.Player.SendInfoMessage($"Set The Twins as {(NPC.downedMechBoss2 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }
                            case "mech3":
                            case "skeletronprime":
                            case "skeletron prime":
                            case "prime":
                                {
                                    NPC.downedMechBoss3 = !NPC.downedMechBoss3;
                                    args.Player.SendInfoMessage($"Set Skeletron Prime as {(NPC.downedMechBoss3 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }
                            case "plantera":
                                {
                                    NPC.downedPlantBoss = !NPC.downedPlantBoss;
                                    args.Player.SendInfoMessage($"Set Plantera as {(NPC.downedPlantBoss ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }
                            case "golem":
                                {
                                    NPC.downedGolemBoss = !NPC.downedGolemBoss;
                                    args.Player.SendInfoMessage($"Set Golem as {(NPC.downedGolemBoss ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }
                            case "duke":
                            case "fishron":
                            case "dukefishron":
                            case "duke fishron":
                                {
                                    NPC.downedFishron = !NPC.downedFishron;
                                    args.Player.SendInfoMessage($"Set Duke Fishron as {(NPC.downedFishron ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }
                            case "cultist":
                            case "lunatic":
                            case "lunaticcultist":
                            case "lunatic cultist":
                                {
                                    NPC.downedAncientCultist = !NPC.downedAncientCultist;
                                    args.Player.SendInfoMessage($"Set Lunatic Cultist as {(NPC.downedAncientCultist ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }

                            case "empress":
                            case "eol":
                            case "empressoflight":
                            case "empress of light":
                                {
                                    NPC.downedEmpressOfLight = !NPC.downedEmpressOfLight;
                                    args.Player.SendInfoMessage($"Set Empress of Light as {(NPC.downedEmpressOfLight ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }
                            case "moonlord":
                            case "moon lord":
                            case "ml":
                                {
                                    NPC.downedMoonlord = !NPC.downedMoonlord;
                                    args.Player.SendInfoMessage($"Set Moonlord as {(NPC.downedMoonlord ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                                    return;
                                }
                            default:
                                {
                                    args.Player.SendErrorMessage("Please specify which boss to toggle!");
                                    args.Player.SendInfoMessage("eg. /undoboss king - toggle king slime");
                                    return;
                                }
                        }
                        return;
                    }
                #endregion

                //schedule
                #region ( Type : EnableSched )
                case "enableschedule":
                case "enablesched":
                    {
                        DateTime schedule;
                        if (!DateTime.TryParse(args.Parameters[2], out schedule))
                        {
                            args.Player.SendErrorMessage("Invalid DateTime Schedule!");
                            return;
                        }
                        string[] getmonth_str = { 
                            "Jan",
                            "feb",
                            "Mar",
                            "Apr",
                            "May",
                            "Jun",
                            "Jul",
                            "Aug",
                            "Sep",
                            "Oct",
                            "Nov",
                            "Dec"
                        };

                        string stringsched = 
                            $"> target: {schedule.Hour}:{schedule.Minute} {getmonth_str[schedule.Month - 1]} {schedule.Day} {schedule.Year}" +
                            $"\n> server time: {DateTime.UtcNow.Hour}:{DateTime.UtcNow.Minute} {getmonth_str[DateTime.UtcNow.Month - 1]} {DateTime.UtcNow.Day} {DateTime.UtcNow.Year}";
                        //TShock.Utils.TryParseTime()
                        switch (args.Parameters[1].ToLower())
                        {
                            case "kingslime":
                            case "king slime":
                            case "king":
                            case "ks":
                                {
                                    Config.BossManager.ScheduleAllowKingSlime = schedule;
                                    args.Player.SendInfoMessage("set King Slime Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "eyeofcthulhu":
                            case "eye of cthulhu":
                            case "eye":
                            case "eoc":
                                {
                                    Config.BossManager.ScheduleAllowEyeOfCthulhu = schedule;
                                    args.Player.SendInfoMessage("set Eye Of Cthulhu Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "evilboss":
                            case "evil boss":
                                {
                                    Config.BossManager.ScheduleAllowEaterOfWorlds = schedule;
                                    Config.BossManager.ScheduleAllowBrainOfCthulhu = schedule;
                                    args.Player.SendInfoMessage("set Eater Of Worlds & Brain Of Cthulhu Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "eow":
                            case "eaterofworlds":
                            case "eater of worlds":
                            case "eater":
                                {
                                    Config.BossManager.ScheduleAllowEaterOfWorlds = schedule;
                                    args.Player.SendInfoMessage("set Eater Of Worlds Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "boc":
                            case "brainofcthulhu":
                            case "brain of cthulhu":
                            case "brain":
                                {
                                    Config.BossManager.ScheduleAllowBrainOfCthulhu = schedule;
                                    args.Player.SendInfoMessage("set Brain Of Cthulhu Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "skeletron":
                            case "sans":
                                {
                                    Config.BossManager.ScheduleAllowSkeletron = schedule;
                                    args.Player.SendInfoMessage("set Skeletron Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "queenbee":
                            case "queen bee":
                            case "qb":
                                {
                                    Config.BossManager.ScheduleAllowQueenBee = schedule;
                                    args.Player.SendInfoMessage("set Queen Bee Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "deerclops":
                            case "deer clops":
                            case "deer":
                                {
                                    Config.BossManager.ScheduleAllowDeerclops = schedule;
                                    args.Player.SendInfoMessage("set Deerclops Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "wall of flesh":
                            case "wallofflesh":
                            case "wof":
                                {
                                    Config.BossManager.ScheduleAllowSkeletron = schedule;
                                    args.Player.SendInfoMessage("set Wall Of Flesh Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "queenslime":
                            case "queen slime":
                            case "qs":
                                {
                                    Config.BossManager.ScheduleAllowQueenSlime = schedule;
                                    args.Player.SendInfoMessage("set Queen Slime Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "mech1":
                            case "thedestroyer":
                            case "the destroyer":
                            case "destroyer":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        Config.BossManager.ScheduleAllowMechdusa = schedule;
                                        args.Player.SendInfoMessage("set Mechdusa Schedule" +
                                            $"\n{stringsched}");
                                        break;
                                    }
                                    Config.BossManager.ScheduleAllowTheDestroyer = schedule;
                                    args.Player.SendInfoMessage("set the Destroyer Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "mech2":
                            case "thetwins":
                            case "the twins":
                            case "twins":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        Config.BossManager.ScheduleAllowMechdusa = schedule;
                                        args.Player.SendInfoMessage("set Mechdusa Schedule" +
                                            $"\n{stringsched}");
                                        break;
                                    }
                                    Config.BossManager.ScheduleAllowTheTwins = schedule;
                                    args.Player.SendInfoMessage("set The Twins Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "mech3":
                            case "skeletronprime":
                            case "skeletron prime":
                            case "prime":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        Config.BossManager.ScheduleAllowMechdusa = schedule;
                                        args.Player.SendInfoMessage("set Mechdusa Schedule" +
                                            $"\n{stringsched}");
                                        break;
                                    }
                                    Config.BossManager.ScheduleAllowSkeletronPrime = schedule;
                                    args.Player.SendInfoMessage("set Skeletron Prime Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "mechdusa":
                                {
                                    if (!Main.zenithWorld)
                                    {
                                        args.Player.SendErrorMessage("Mechdusa is only available on zenith seed!");
                                        return;
                                    }
                                    Config.BossManager.ScheduleAllowMechdusa = schedule;
                                    args.Player.SendInfoMessage("set Mechdusa Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "plantera":
                                {
                                    Config.BossManager.ScheduleAllowPlantera = schedule;
                                    args.Player.SendInfoMessage("set Plantera Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "golem":
                                {
                                    Config.BossManager.ScheduleAllowGolem = schedule;
                                    args.Player.SendInfoMessage("set Golem Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "duke":
                            case "fishron":
                            case "dukefishron":
                            case "duke fishron":
                                {
                                    Config.BossManager.ScheduleAllowDukeFishron = schedule;
                                    args.Player.SendInfoMessage("set Duke Fishron Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "cultist":
                            case "lunatic":
                            case "lunaticcultist":
                            case "lunatic cultist":
                                {
                                    Config.BossManager.ScheduleAllowLunaticCultist = schedule;
                                    args.Player.SendInfoMessage("set Lunatic Cultist Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "empress":
                            case "eol":
                            case "empressoflight":
                            case "empress of light":
                                {
                                    Config.BossManager.ScheduleAllowEmpressOfLight = schedule;
                                    args.Player.SendInfoMessage("set Empress Of Light Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            case "moonlord":
                            case "moon lord":
                            case "ml":
                                {
                                    Config.BossManager.ScheduleAllowMoonLord = schedule;
                                    args.Player.SendInfoMessage("set Moon Lord Schedule" +
                                        $"\n{stringsched}");
                                    break;
                                }
                            default:
                                {
                                    args.Player.SendErrorMessage("Please specify the boss!");
                                    return;
                                }
                        }
                        Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : DisableSched )
                case "disableenableschedule":
                case "disablesched":
                    {
                        switch (args.Parameters[1].ToLower())
                        {
                            case "kingslime":
                            case "king slime":
                            case "king":
                            case "ks":
                                {
                                    Config.BossManager.ScheduleAllowKingSlime = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Cancled King Slime Schedule");
                                    break;
                                }
                            case "eyeofcthulhu":
                            case "eye of cthulhu":
                            case "eye":
                            case "eoc":
                                {
                                    Config.BossManager.ScheduleAllowEyeOfCthulhu = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Cancled Eye Of Cthulhu Schedule");
                                    break;
                                }
                            case "evilboss":
                            case "evil boss":
                                {
                                    Config.BossManager.ScheduleAllowEaterOfWorlds = DateTime.MaxValue;
                                    Config.BossManager.ScheduleAllowBrainOfCthulhu = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled Eater Of Worlds & Brain Of Cthulhu Schedule");
                                    break;
                                }
                            case "eow":
                            case "eaterofworlds":
                            case "eater of worlds":
                            case "eater":
                                {
                                    Config.BossManager.ScheduleAllowEaterOfWorlds = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled Eater Of Worlds Schedule");
                                    break;
                                }
                            case "boc":
                            case "brainofcthulhu":
                            case "brain of cthulhu":
                            case "brain":
                                {
                                    Config.BossManager.ScheduleAllowBrainOfCthulhu = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled Brain Of Cthulhu Schedule");
                                    break;
                                }
                            case "skeletron":
                            case "sans":
                                {
                                    Config.BossManager.ScheduleAllowSkeletron = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled Skeletron Schedule");
                                    break;
                                }
                            case "queenbee":
                            case "queen bee":
                            case "qb":
                                {
                                    Config.BossManager.ScheduleAllowQueenBee = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled Queen Bee Schedule");
                                    break;
                                }
                            case "deerclops":
                            case "deer clops":
                            case "deer":
                                {
                                    Config.BossManager.ScheduleAllowDeerclops = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled Deerclops Schedule");
                                    break;
                                }
                            case "wall of flesh":
                            case "wallofflesh":
                            case "wof":
                                {
                                    Config.BossManager.ScheduleAllowSkeletron = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled Wall Of Flesh Schedule");
                                    break;
                                }
                            case "queenslime":
                            case "queen slime":
                            case "qs":
                                {
                                    Config.BossManager.ScheduleAllowQueenSlime = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("set Queen Slime Schedule");
                                    break;
                                }
                            case "mech1":
                            case "thedestroyer":
                            case "the destroyer":
                            case "destroyer":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        Config.BossManager.ScheduleAllowMechdusa = DateTime.MaxValue;
                                        args.Player.SendInfoMessage("Canceled Mechdusa Schedule");
                                        break;
                                    }
                                    Config.BossManager.ScheduleAllowTheDestroyer = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled the Destroyer Schedule");
                                    break;
                                }
                            case "mech2":
                            case "thetwins":
                            case "the twins":
                            case "twins":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        Config.BossManager.ScheduleAllowMechdusa = DateTime.MaxValue;
                                        args.Player.SendInfoMessage("Canceled Mechdusa Schedule");
                                        break;
                                    }
                                    Config.BossManager.ScheduleAllowTheTwins = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled The Twins Schedule");
                                    break;
                                }
                            case "mech3":
                            case "skeletronprime":
                            case "skeletron prime":
                            case "prime":
                                {
                                    if (Main.zenithWorld)
                                    {
                                        Config.BossManager.ScheduleAllowMechdusa = DateTime.MaxValue;
                                        args.Player.SendInfoMessage("Canceled Mechdusa Schedule");
                                        break;
                                    }
                                    Config.BossManager.ScheduleAllowSkeletronPrime = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled Skeletron Prime Schedule");
                                    break;
                                }
                            case "mechdusa":
                                {
                                    if (!Main.zenithWorld)
                                    {
                                        args.Player.SendErrorMessage("Mechdusa is only available on zenith seed!");
                                        return;
                                    }

                                    Config.BossManager.ScheduleAllowMechdusa = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled Mechdusa Schedule");
                                    break;
                                }
                            case "plantera":
                                {
                                    Config.BossManager.ScheduleAllowPlantera = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled Plantera Schedule");
                                    break;
                                }
                            case "golem":
                                {
                                    Config.BossManager.ScheduleAllowGolem = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled Golem Schedule");
                                    break;
                                }
                            case "duke":
                            case "fishron":
                            case "dukefishron":
                            case "duke fishron":
                                {
                                    Config.BossManager.ScheduleAllowDukeFishron = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled Duke Fishron Schedule");
                                    break;
                                }
                            case "cultist":
                            case "lunatic":
                            case "lunaticcultist":
                            case "lunatic cultist":
                                {
                                    Config.BossManager.ScheduleAllowLunaticCultist = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled Lunatic Cultist Schedule");
                                    break;
                                }
                            case "empress":
                            case "eol":
                            case "empressoflight":
                            case "empress of light":
                                {
                                    Config.BossManager.ScheduleAllowEmpressOfLight = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled Empress Of Light Schedule");
                                    break;
                                }
                            case "moonlord":
                            case "moon lord":
                            case "ml":
                                {
                                    Config.BossManager.ScheduleAllowMoonLord = DateTime.MaxValue;
                                    args.Player.SendInfoMessage("Canceled Moon Lord Schedule");
                                    break;
                                }
                            default:
                                {
                                    args.Player.SendErrorMessage("Please specify the boss!");
                                    return;
                                }
                        }
                        Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : DisableSchedall )
                case "disableenablescheduleall":
                case "disableschedall":
                    {

                        Config.BossManager.ScheduleAllowKingSlime = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowEyeOfCthulhu = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowEaterOfWorlds = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowBrainOfCthulhu = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowQueenBee = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowSkeletron = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowDeerclops = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowWallOfFlesh = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowQueenSlime = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowTheDestroyer = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowTheTwins = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowSkeletronPrime = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowMechdusa = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowPlantera = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowGolem = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowDukeFishron = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowEmpressOfLight = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowLunaticCultist = DateTime.MaxValue;
                        Config.BossManager.ScheduleAllowMoonLord = DateTime.MaxValue;

                        args.Player.SendInfoMessage("Cancled All Boss Schedule");

                        Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : usingschedule )
                case "usingschedule":
                case "usingsched":
                    {


                        switch (args.Parameters[1].ToLower())
                        {
                            case "true":
                            case "yes":
                                {
                                    Config.BossManager.UseBossSchedule = true;
                                    args.Player.SendInfoMessage("Set Using Boss Schedule to true");
                                    break;
                                }
                            case "false":
                            case "no":
                                {
                                    Config.BossManager.UseBossSchedule = false;
                                    args.Player.SendInfoMessage("Set Using Boss Schedule to false");
                                    break;
                                }
                            default:
                                {
                                    if ((bool)Config.BossManager.UseBossSchedule)
                                    {
                                        Config.BossManager.UseBossSchedule = false;
                                        args.Player.SendInfoMessage("Set Using Boss Schedule to false");
                                        break;
                                    } else
                                    {
                                        Config.BossManager.UseBossSchedule = true;
                                        args.Player.SendInfoMessage("Set Using Boss Schedule to true");
                                        break;
                                    }
                                }
                        }

                        Config.Changeall();
                        Config.Read();
                        return;
                    }
                #endregion
                #region ( Type : ResetSchedule )
                case "resetschedule":
                case "resetsched":
                    {
                        if (args.Parameters.Count != 1)
                        {
                            DateTime gettoday;
                            if (!DateTime.TryParse(args.Parameters[1], out gettoday))
                            {
                                args.Player.SendErrorMessage("Invalid DateTime Schedule!");
                                return;
                            }

                            DateTime today = new(gettoday.Year, gettoday.Month, gettoday.Day, (int)Config.BossManager.Default_ScheduleDay_Hour, 0, 0);

                            string[] getmonth_str = {
                                "Jan",
                                "feb",
                                "Mar",
                                "Apr",
                                "May",
                                "Jun",
                                "Jul",
                                "Aug",
                                "Sep",
                                "Oct",
                                "Nov",
                                "Dec"
                            };

                            string resultstr = $"{getmonth_str[today.Month - 1]} {today.Day}, {today.Year}";

                            //DateTime today = DateTime.Parse($"{DateTime.UtcNow.Month}/{DateTime.UtcNow.Day}/{DateTime.UtcNow.Year}");

                            Config.BossManager.ScheduleAllowKingSlime = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowKingSlime);
                            Config.BossManager.ScheduleAllowEyeOfCthulhu = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowEyeOfCthulhu);
                            Config.BossManager.ScheduleAllowEaterOfWorlds = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowEaterOfWorlds);
                            Config.BossManager.ScheduleAllowBrainOfCthulhu = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowBrainOfCthulhu);
                            Config.BossManager.ScheduleAllowQueenBee = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowQueenBee);
                            Config.BossManager.ScheduleAllowSkeletron = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowSkeletron);
                            Config.BossManager.ScheduleAllowDeerclops = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowDeerclops);
                            Config.BossManager.ScheduleAllowWallOfFlesh = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowWallOfFlesh);
                            Config.BossManager.ScheduleAllowQueenSlime = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowQueenSlime);
                            Config.BossManager.ScheduleAllowTheDestroyer = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowTheDestroyer);
                            Config.BossManager.ScheduleAllowTheTwins = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowTheTwins);
                            Config.BossManager.ScheduleAllowSkeletronPrime = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowSkeletronPrime);
                            Config.BossManager.ScheduleAllowMechdusa = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowMechdusa);
                            Config.BossManager.ScheduleAllowPlantera = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowPlantera);
                            Config.BossManager.ScheduleAllowGolem = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowGolem);
                            Config.BossManager.ScheduleAllowDukeFishron = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowDukeFishron);
                            Config.BossManager.ScheduleAllowEmpressOfLight = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowEmpressOfLight);
                            Config.BossManager.ScheduleAllowLunaticCultist = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowLunaticCultist);
                            Config.BossManager.ScheduleAllowMoonLord = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowMoonLord);

                            

                            args.Player.SendInfoMessage($"Reload Boss Schedule" +
                                $"\nStarting Point: {resultstr}");

                            Config.Changeall();
                            Config.Read();
                            return;
                        } else
                        {
                            DateTime today = new(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, (int)Config.BossManager.Default_ScheduleDay_Hour, 0, 0);

                            //DateTime today = DateTime.Parse($"{DateTime.UtcNow.Month}/{DateTime.UtcNow.Day}/{DateTime.UtcNow.Year}");

                            Config.BossManager.ScheduleAllowKingSlime = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowKingSlime);
                            Config.BossManager.ScheduleAllowEyeOfCthulhu = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowEyeOfCthulhu);
                            Config.BossManager.ScheduleAllowEaterOfWorlds = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowEaterOfWorlds);
                            Config.BossManager.ScheduleAllowBrainOfCthulhu = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowBrainOfCthulhu);
                            Config.BossManager.ScheduleAllowQueenBee = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowQueenBee);
                            Config.BossManager.ScheduleAllowSkeletron = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowSkeletron);
                            Config.BossManager.ScheduleAllowDeerclops = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowDeerclops);
                            Config.BossManager.ScheduleAllowWallOfFlesh = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowWallOfFlesh);
                            Config.BossManager.ScheduleAllowQueenSlime = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowQueenSlime);
                            Config.BossManager.ScheduleAllowTheDestroyer = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowTheDestroyer);
                            Config.BossManager.ScheduleAllowTheTwins = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowTheTwins);
                            Config.BossManager.ScheduleAllowSkeletronPrime = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowSkeletronPrime);
                            Config.BossManager.ScheduleAllowMechdusa = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowMechdusa);
                            Config.BossManager.ScheduleAllowPlantera = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowPlantera);
                            Config.BossManager.ScheduleAllowGolem = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowGolem);
                            Config.BossManager.ScheduleAllowDukeFishron = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowDukeFishron);
                            Config.BossManager.ScheduleAllowEmpressOfLight = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowEmpressOfLight);
                            Config.BossManager.ScheduleAllowLunaticCultist = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowLunaticCultist);
                            Config.BossManager.ScheduleAllowMoonLord = today.AddDays((double)Config.BossManager.Default_ScheduleDay_AllowMoonLord);

                            args.Player.SendInfoMessage("Reset Boss Schedule");

                            Config.Changeall();
                            Config.Read();
                            return;
                        }
                        

                    }
                #endregion
                default:
                    {
                        args.Player.SendErrorMessage("Invalid Sub-Command!" +
                            $"\ndo '{Commands.Specifier}manageboss help' for more info");
                        return;
                    }
            }

            #endregion
        }

        private void CMD_Vanish(CommandArgs args)
        {
            #region code
            //args.TPlayer.active = !args.TPlayer.active;

            if (args.Player.ContainsData("MKLP_Vanish"))
            {
                if (!args.Player.GetData<bool>("MKLP_Vanish"))
                {
                    TogglePlayerVanish(args.Player, true);

                    TShock.Utils.Broadcast($"{args.Player.Name} has left.", Color.Yellow);
                    args.Player.SendSuccessMessage("You're on Vanish");
                }
                else
                {
                    TogglePlayerVanish(args.Player, false);

                    TShock.Utils.Broadcast($"{args.Player.Name} has joined.", Color.Yellow);
                    args.Player.SendSuccessMessage("You're no longer on Vanish");
                }
            }
            else
            {
                TogglePlayerVanish(args.Player, true);

                TShock.Utils.Broadcast($"{args.Player.Name} has left.", Color.Yellow);
                args.Player.SendSuccessMessage("You're on Vanish");
            }

            #endregion
        }

        #endregion

        #region { Moderator }

        private void CMD_ManageReport(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage($"Proper Usage: {Commands.Specifier}managereport <info/delete>");
                args.Player.SendMessage(
                    $"'{Commands.Specifier}report info <id>': get report info by id" +
                    $"\n'{Commands.Specifier}report info fromlist <accountname>' : get this user list of his reports" +
                    $"\n'{Commands.Specifier}report info targetlist <targetname>' : get list of report from this player" +
                    "\n" +
                    $"\n'{Commands.Specifier}report delete <id>': delete any reports by id", Color.WhiteSmoke);
                return;
            }

            switch (args.Parameters[0].ToLower())
            {
                case "info":
                    {
                        if (args.Parameters.Count == 1)
                        {
                            return;
                        }

                        int getid = -1;

                        if (int.TryParse(args.Parameters[1], out getid))
                        {
                            try
                            {
                                MKLP_Report getreport = DBManager.GetReportByID(getid);

                                args.Player.SendMessage($"Report id: {getreport.ID} Info" +
                                    $"\nFrom: [c/ff7c34:{getreport.From}]" +
                                    $"\nTarget: [c/ff7c34:{(getreport.Target == "" ? $"{DiscordKLP.S_}none{DiscordKLP.S_}" : $"{getreport.Target}")}]" +
                                    $"\nLocation: [c/ff7c34:{getreport.Location}]" +
                                    $"\nPlayers Online: [c/ff7c34:{getreport.Players.Replace($"{DiscordKLP.S_}", ", ")}]" +
                                    $"\n" +
                                    $"\nMessage: [c/ff7c34:{getreport.Message}]" +
                                    $"\n" +
                                    $"\nReported Since: [c/ff7c34:{getreport.Since}]"
                                    , Color.OrangeRed);
                                return;
                            } catch (NullReferenceException)
                            {
                                args.Player.SendErrorMessage("No reports found with this id");
                                return;
                            }
                        } else
                        {
                            switch (args.Parameters[1])
                            {
                                case "fromlist":
                                    {
                                        if (args.Parameters.Count == 2)
                                        {
                                            args.Player.SendErrorMessage($"Proper Usage: {Commands.Specifier}managereport info fromlist <accountname>");
                                            return;
                                        }

                                        MKLP_Report[] getreport = DBManager.GetReport(from: args.Parameters[2]).ToArray();

                                        if (getreport.Count() <= 0)
                                        {
                                            args.Player.SendErrorMessage($"No reports from {args.Parameters[2]}");
                                            return;
                                        }

                                        string result = "";

                                        foreach (MKLP_Report r in getreport)
                                        {
                                            result += $"[c/ff7c34:<{r.ID}> target {r.Target}]\n";
                                        }

                                        args.Player.SendMessage($"Report List from [ {args.Parameters[2]} ]" +
                                            $"\n{result}", Color.OrangeRed);

                                        return;
                                    }
                                case "targetlist":
                                    {
                                        if (args.Parameters.Count == 2)
                                        {
                                            args.Player.SendErrorMessage($"Proper Usage: {Commands.Specifier}managereport info target <accountname>");
                                            return;
                                        }

                                        MKLP_Report[] getreport = DBManager.GetReport(target: args.Parameters[2]).ToArray();

                                        if (getreport.Count() <= 0)
                                        {
                                            args.Player.SendErrorMessage($"{args.Parameters[2]} has no reports from someone");
                                            return;
                                        }

                                        string result = "";

                                        foreach (MKLP_Report r in getreport)
                                        {
                                            result += $"[c/ff7c34:<{r.ID}> from {r.From}]\n";
                                        }

                                        args.Player.SendMessage($"Players Report List from [ {args.Parameters[2]} ]" +
                                            $"\n{result}", Color.OrangeRed);

                                        return;
                                    }
                                default:
                                    {
                                        args.Player.SendErrorMessage("Invalid Report ID");
                                        return;
                                    }
                            }
                        }
                    }
                case "delete":
                    {
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendErrorMessage($"Proper Usage: {Commands.Specifier}managereport delete <reportID>");
                            return;
                        }

                        int getid = -1;

                        if (int.TryParse(args.Parameters[1], out getid))
                        {
                            if (DBManager.DeleteReport(getid))
                            {
                                args.Player.SendSuccessMessage($"Successfully deleted report no. {getid}");
                                return;
                            } else
                            {
                                args.Player.SendSuccessMessage($"Unable to delete report no. {getid}");
                                return;
                            }
                        }

                        return;
                    }
                default:
                    {
                        args.Player.SendSuccessMessage($"Invalid Sub-Command");
                        args.Player.SendMessage($"Do '{Commands.Specifier}report help' for more info", Color.WhiteSmoke);
                        return;
                    }
            }
            #endregion
        }

        private void CMD_BanInfo(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage($"Usage: {Commands.Specifier}baninfo <ticketban>");
                return;
            }

            if (args.Parameters[0].ToLower() == "list")
            {
                string PickColorForBan(Ban ban)
                {
                    double hoursRemaining = (ban.ExpirationDateTime - DateTime.UtcNow).TotalHours;
                    double hoursTotal = (ban.ExpirationDateTime - ban.BanDateTime).TotalHours;
                    double percentRemaining = TShock.Utils.Clamp(hoursRemaining / hoursTotal, 100, 0);

                    int red = TShock.Utils.Clamp((int)(255 * 2.0f * percentRemaining), 255, 0);
                    int green = TShock.Utils.Clamp((int)(255 * (2.0f * (1 - percentRemaining))), 255, 0);

                    return $"{red:X2}{green:X2}{0:X2}";
                }

                if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out int pageNumber))
                {
                    args.Player.SendMessage($"Invalid Ban page", Color.White);
                    return;
                }

                var bans = from bban in TShock.Bans.Bans
                           where bban.Value.ExpirationDateTime > DateTime.UtcNow
                           orderby bban.Value.ExpirationDateTime ascending
                           select $"[{bban.Key.Color(TShockAPI.Utils.GreenHighlight)}] {bban.Value.Identifier.Color(PickColorForBan(bban.Value))}";

                PaginationTools.SendPage(args.Player, pageNumber, bans.ToList(),
                    new PaginationTools.Settings
                    {
                        HeaderFormat = "Bans ({{0}}/{{1}}):",
                        FooterFormat = "Type " + Commands.Specifier + "ban list {{0}} for more.",
                        NothingToDisplayString = "There are currently no active bans."
                    });
                return;
            }

            int targetid;
            if (!int.TryParse(args.Parameters[0], out targetid))
            {
                args.Player.SendErrorMessage("Invalid ticket number");
                return;
            }

            Ban ban = TShock.Bans.GetBanById(targetid);

            if (ban == null)
            {
                args.Player.SendErrorMessage("No bans found");
                return;
            }

            args.Player.SendMessage("[c/3bf000:Ban Info]" +
                $"\nTicket No. : [c/84ff5b:{ban.TicketNumber}]" +
                $"\nIdentifier : [c/84ff5b:{ban.Identifier}]" +
                $"\nBanned by : [c/84ff5b:{ban.BanningUser}]" +
                $"\nReason : [c/84ff5b:{ban.Reason}]" +
                $"\n{(ban.ExpirationDateTime < DateTime.UtcNow ? $":Ban expired : [c/84ff5b:{ban.ExpirationDateTime.ToString("yyyy/MM/dd")} ({ban.GetPrettyExpirationString()} ago)]" : "" )}"
                , Color.Green);

            /*
            switch (args.Parameters[0])
            {
                default:
                    {
                        return;
                    }
            }
            */
            #endregion
        }
        private void CMD_Ban(CommandArgs args)
        {
            #region code

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage($"Usage: {Commands.Specifier}{((bool)Config.Main.Replace_Ban_TShockCommand ? "ban" : "qban")} <player> <reason> <duration> <args...>");
                args.Player.SendMessage($"[c/8fbfd4:Example:] [c/a5d063:{Commands.Specifier}{((bool)Config.Main.Replace_Ban_TShockCommand ? "ban" : "qban")} {args.Player.Name} \"cheating\" \"1d 1m\" -offline]" +
                    $"\n[c/8fbfd4:duration:] 1d = 1day (d,h,m,s = day,hour,minute,second)" +
                    $"\n" +
                    $"\n[c/8fbfd4:args:] " +
                    $"\n( -alt = bans only name )" +
                    $"\n( -account = only used when banning a offline player )" +
                    $"\n( -accountid = only used when banning a offline player account id )" +
                    ((bool)Config.BanGuard.UsingBanGuard ? $"\n( -banguard = must have a category to use banguard ex.'/ban \"{args.Player.Name}\" -banguard hacks' )\n( -banguardauto = automatically assign banguard category from your reason must be accurate )" : ""),
                    Color.Gray);
                return;
            }

            bool altban = args.Parameters.Any(p => p == "-alt");
            bool accountban = args.Parameters.Any(p => p == "-account");
            bool accountidban = args.Parameters.Any(p => p == "-accountid");
            bool usingbanguardauto = args.Parameters.Any(p => p == "-banguardauto");
            int usingbanguardi = args.Parameters.FindIndex(p => p == "-banguard") + 1;
            string usingbanguardcat = "N/A";

            if (usingbanguardi > 0 && usingbanguardi < args.Parameters.Count)
            {
                usingbanguardcat = args.Parameters[usingbanguardi];
            }

            bool uuidip = true;

            if (altban == true) uuidip = false;


            List<string> flags = new List<string>() { "-alt", "-account", "-accountid", "-banguardauto", "-banguard" };

            string reason = "No Reason Specified";
            string duration = null;
            DateTime expiration = DateTime.MaxValue;

            for (int i = 1; i < args.Parameters.Count; i++)
            {
                var param = args.Parameters[i];
                if (!flags.Contains(param) || (usingbanguardcat != "N/A" && i == usingbanguardi))
                {
                    reason = param;
                    break;
                }
            }
            for (int i = 2; i < args.Parameters.Count; i++)
            {
                var param = args.Parameters[i];
                if (!flags.Contains(param) || (usingbanguardcat != "N/A" && i == usingbanguardi))
                {
                    duration = param;
                    break;
                }
            }

            if (TShock.Utils.TryParseTime(duration, out ulong seconds))
            {
                expiration = DateTime.UtcNow.AddSeconds(seconds);
            }

            if (!(bool)Config.BanGuard.UsingBanGuard)
            {
                if (usingbanguardauto)
                {
                    usingbanguardcat = BanGuardAPI.GetCategoryFromReason(reason);
                }
                if (usingbanguardcat != "N/A")
                {
                    if (!BanGuardAPI.IsCategory(usingbanguardcat))
                    {
                        args.Player.SendErrorMessage("Invalid BanGuard Category!");
                        return;
                    }
                }
            }

            if (accountidban)
            {
                if (!args.Player.HasPermission(Config.Permissions.CMD_OfflineBan))
                {
                    args.Player.SendErrorMessage("You do not have permission to ban offline players!");
                    return;
                }

                int accountidtarget = 0;

                if (!int.TryParse(args.Parameters[0], out accountidtarget))
                {
                    args.Player.SendErrorMessage("Invalid Number!");
                    return;
                }

                UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByID(accountidtarget);

                if (targetaccount == null)
                {
                    args.Player.SendErrorMessage($"Account ID {accountidtarget} doesn't exist");
                    return;
                }

                var players = TSPlayer.FindByNameOrID(targetaccount.Name);

                TSPlayer? targetplayer = null;

                foreach (TSPlayer player in players)
                {
                    if (player == null) continue;
                    if (player.Account.Name == targetaccount.Name)
                    {
                        targetplayer = player;
                    }
                }

                if (usingbanguardcat != "N/A" && BanGuardAPI._isApiKeyValid)
                {
                    args.Player.SendInfoMessage("Using BanGuard Ban...");
                }

                if (targetplayer != null)
                {
                    if (ManagePlayer.OnlineBan(args.Silent, targetplayer, reason, args.Player.Account.Name, expiration, uuidip, uuidip, usingbanguardcat))
                    {
                        args.Player.SendSuccessMessage($"Successfully banned {targetplayer.Name} for {reason}");
                    }
                    else
                    {
                        args.Player.SendErrorMessage($"Error occur banning {targetplayer.Name}");
                    }
                }
                else
                {
                    if (ManagePlayer.OfflineBan(targetaccount, reason, args.Player.Account.Name, expiration, uuidip, uuidip, usingbanguardcat))
                    {
                        args.Player.SendSuccessMessage($"Successfully banned Acc: {targetaccount.Name} for {reason}");
                    }
                    else
                    {
                        args.Player.SendErrorMessage($"Error occur banning Acc: {targetaccount.Name}");
                    }
                }

            } else if (accountban)
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
                    return;
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


                if (usingbanguardcat != "N/A" && BanGuardAPI._isApiKeyValid)
                {
                    args.Player.SendInfoMessage("Using BanGuard Ban...");
                }

                if (targetplayer != null)
                {
                    if (ManagePlayer.OnlineBan(args.Silent, targetplayer, reason, args.Player.Account.Name, expiration, uuidip, uuidip, usingbanguardcat))
                    {
                        args.Player.SendSuccessMessage($"Successfully banned {targetplayer.Name} for {reason}");
                    }
                    else
                    {
                        args.Player.SendErrorMessage($"Error occur banning {targetplayer.Name}");
                    }
                } else
                {
                    if (ManagePlayer.OfflineBan(targetaccount, reason, args.Player.Account.Name, expiration, uuidip, uuidip, usingbanguardcat))
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


                if (usingbanguardcat != "N/A" && BanGuardAPI._isApiKeyValid)
                {
                    args.Player.SendInfoMessage("Using BanGuard Ban...");
                }

                if (ManagePlayer.OnlineBan(args.Silent, targetplayer, reason, args.Player.Account.Name, expiration, uuidip, uuidip, usingbanguardcat))
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
                args.Player.SendErrorMessage($"Usage: {Commands.Specifier}unban <ticket number>" +
                    $"\nor use '{Commands.Specifier}unban <account name> -account' to unban a account" +
                    $"\nor use '{Commands.Specifier}unban <account id> -accountid' to unban a account");
                return;
            }

            bool accountunban = args.Parameters.Any(p => p == "-account");
            bool accountidunban = args.Parameters.Any(p => p == "-accountid");

            if (accountidunban)
            {
                int accountid = 0;

                if (!int.TryParse(args.Parameters[0], out accountid))
                {
                    args.Player.SendErrorMessage("Invalid Number!");
                    return;
                }

                UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByID(accountid);

                if (targetaccount == null)
                {
                    args.Player.SendErrorMessage("Invalid Account");
                    return;
                }

                if (ManagePlayer.UnBanAccount(targetaccount, args.Player.Name))
                {
                    args.Player.SendSuccessMessage($"Removing Ban Tickets from account: {targetaccount.Name}");
                    return;
                }
                else
                {
                    args.Player.SendErrorMessage($"AccountID: '{accountid}' could not be found...");
                    return;
                }

            } else if (accountunban)
            {
                string targetname = string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count);
                targetname = targetname.Replace(" -account", "");
                targetname = targetname.Replace("-account", "");

                UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByName(targetname);

                if (targetaccount == null)
                {
                    args.Player.SendErrorMessage("Invalid Account");
                    return;
                }

                if (ManagePlayer.UnBanAccount(targetaccount, args.Player.Name))
                {
                    args.Player.SendSuccessMessage($"Removing Ban Tickets from account: {targetaccount.Name}");
                    return;
                }
                else
                {
                    args.Player.SendErrorMessage($"Account: '{targetname}' could not be found...");
                    return;
                }

            } else
            {
                int ticketnumber = -1;

                if (int.TryParse(args.Parameters[0], out ticketnumber))
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

            #endregion
        }

        private void CMD_Mute(CommandArgs args)
        {
            #region code

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage($"Proper Usage: {Commands.Specifier}{((bool)Config.Main.Replace_Mute_TShockCommand ? "mute" : "qmute")} <player> <duration> <reason>" +
                    (args.Player.HasPermission(Config.Permissions.CMD_OfflineUnMute) ? $"\nMuting Offline Player: {Commands.Specifier}{((bool)Config.Main.Replace_Mute_TShockCommand ? "mute" : "qmute")} <accountname> <duration> <reason> -offline" : ""));
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
                args.Player.SendErrorMessage($"Proper Usage: {Commands.Specifier}unmute <player>" +
                    (args.Player.HasPermission(Config.Permissions.CMD_OfflineUnMute) ? $"\nUnmuting Offline Player: {Commands.Specifier}unmute <accountname> -offline" : ""));
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
                args.Player.SendErrorMessage($"Proper Usage: {Commands.Specifier}disable <player> <reason>");
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
                args.Player.SendErrorMessage($"Proper Usage: {Commands.Specifier}enable <player>");
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

            if (ManagePlayer.UnDisablePlayer(targetplayer, args.Player.Name))
            {
                args.Player.SendSuccessMessage($"Player {targetplayer.Name} enabled");
            } else
            {
                args.Player.SendErrorMessage($"Player {targetplayer.Name} isn't disabled");
            }


            #endregion
        }

        #endregion

        #region { Inspect }

        private void CMD_Spy(CommandArgs args)
        {
            #region code
            var godPower = Terraria.GameContent.Creative.CreativePowerManager.Instance.GetPower<Terraria.GameContent.Creative.CreativePowers.GodmodePower>();

            if (args.Parameters.Count == 0)
            {
                if (args.Player.ContainsData("MKLP_TargetSpy"))
                {
                    TogglePlayerVanish(args.Player, false);

                    args.Player.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);

                    godPower.SetEnabledState(args.Player.Index, false);

                    args.Player.RemoveData("MKLP_TargetSpy");

                    args.Player.SendInfoMessage($"You're no longer spying on someone");
                    return;
                }
                args.Player.SendErrorMessage($"Usage: {Commands.Specifier}spy <player>");
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

            TSPlayer player = players[0];

            TogglePlayerVanish(args.Player, true);

            args.Player.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);

            godPower.SetEnabledState(args.Player.Index, true);

            args.Player.SetData("MKLP_TargetSpy", player);

            args.Player.SendInfoMessage($"Spying {player.Name}");
            return;

            #endregion
        }

        private void CMD_uuidmatch(CommandArgs args)
        {
            #region code
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage($"Proper Usage: {Commands.Specifier}uuidmatch <accountname>");
                return;
            }

            UserAccount getuser = TShock.UserAccounts.GetUserAccountByName(string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count));

            if (getuser == null)
            {
                args.Player.SendErrorMessage("Invalid Player");
                return;
            }

            var getresult = GetMatchUUID_UserAccount(getuser.Name, getuser.UUID);

            if (getresult.Count == 0)
            {
                args.Player.SendWarningMessage("No Accounts Match");
            } else
            {
                string result = "";

                foreach (var get in getresult)
                {
                    result += get.ID + " : " + get.Name + "\n";
                }

                args.Player.SendMessage("Following Users match their UUID:" +
                    "\n" + result, Color.Gray);
            }
            #endregion
        }

        #endregion

        #region { Manager }

        private void CMD_MKLPDiscord(CommandArgs args)
        {
            #region code

            if ((Config.DataBaseDLink.StorageType != "sqlite" && Config.DataBaseDLink.SqliteDBPath != Path.Combine(TShock.SavePath, "MKLP.sqlite")) || !(bool)Config.DataBaseDLink.UsingDB)
            {
                args.Player.SendErrorMessage("You cannot use this command" +
                    "\nas if 'UsingDB' and 'UsingMKLPDatabase' in Config file is set to false...");
                return;
            }

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage($"Usage: {Commands.Specifier}mklpdiscord <type> <args...>" +
                    $"\nDo '{Commands.Specifier}mklpdiscord help' for more details");
                return;
            }

            switch (args.Parameters[0].ToLower())
            {
                #region [ helptext ]
                case "help":
                    {
                        args.Player.SendMessage(
                            $"Proper Usage: {Commands.Specifier}mklpdiscord <sub-command> <args...>" +
                            $"\n\n[ Sub Commands ]" +
                            $"\n[c/4089ff:'list <page>' :] list of accountlinked users" +
                            $"\n" +
                            $"\n[c/4089ff:'set <player> <userid>' :] sets or adds a accountlink user" +
                            $"\n" +
                            $"\n[c/4089ff:'remove <player>' :] removes a accountlink user"
                            , Color.WhiteSmoke);
                        return;
                    }
                #endregion
                case "list":
                    #region ( Type: List )
                    {
                        try
                        {

                            Dictionary<string, string> DLinkList = DBManager.AccountDLinkingList();

                            decimal maxpage = Math.Ceiling((decimal)DLinkList.Count() / 10);

                            if (maxpage == 0)
                            {
                                args.Player.SendInfoMessage("No Link Accounts Assigned...");
                                return;
                            }

                            if (args.Parameters.Count == 2)
                            {
                                args.Player.SendInfoMessage($"Linked Accounts 1/{maxpage}" +
                                    $"\n{valuepage()}");
                                return;
                            }
                            else
                            {
                                try
                                {
                                    int page = int.Parse(args.Parameters[2]);
                                    args.Player.SendInfoMessage($"Linked Accounts {page}/{maxpage}" +
                                        $"\n{valuepage(page)}");
                                    return;
                                }
                                catch (Exception)
                                {
                                    args.Player.SendInfoMessage($"Linked Accounts 1/{maxpage}" +
                                        $"\n{valuepage()}");
                                    return;
                                }
                            }

                            string valuepage(int page = 1)
                            {
                                string result = "";
                                if (page <= 0) page = 1;

                                int i = 0;
                                int Index = (page - 1) * 6;
                                int max = Index + 6;
                                if (max > DLinkList.Count()) max = DLinkList.Count();
                                foreach (var Tvalue in DLinkList)
                                {
                                    i++;
                                    if (i < Index || i > max) continue;
                                    result += $"\n[ {Tvalue.Key} ]" +
                                        $"\nUserID: {Tvalue.Value}\n";
                                }

                                if (result == "") result = "Empty...";

                                return result;
                            }

                        } catch { }
                        
                        return;
                    }
                    #endregion
                case "set":
                    #region ( Type: Set )
                    {
                        if (args.Parameters.Count < 3)
                        {
                            args.Player.SendErrorMessage($"Usage: {Commands.Specifier}mklpdiscord set <player> <userid>");
                            return;
                        }

                        string targetname = "";

                        var getplayers = TSPlayer.FindByNameOrID(args.Parameters[1]);
                        if (getplayers.Count > 1)
                        {
                            args.Player.SendMultipleMatchError(getplayers.Select(p => p.Name));
                            return;
                        }
                        if (getplayers.Count < 1)
                        {
                            UserAccount getuseraccount = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);
                            if (getuseraccount == null)
                            {
                                args.Player.SendErrorMessage("Invalid Player User");
                                return;
                            }
                            targetname = getuseraccount.Name;
                        }
                        if (getplayers.Count == 1)
                        {
                            targetname = getplayers[0].Name;
                        }
                        if (targetname == "")
                        {
                            args.Player.SendErrorMessage("Invalid Player User");
                            return;
                        }

                        ulong useridtarget = 0;

                        if (!ulong.TryParse(args.Parameters[2], out useridtarget))
                        {
                            args.Player.SendErrorMessage("Invalid UserID");
                            return;
                        }

                        if (DBManager.ChangeAccountDLinkingUserID(targetname, useridtarget.ToString()))
                        {
                            args.Player.SendSuccessMessage($"Change {targetname} UserID to {useridtarget}");
                            return;
                        } else
                        {
                            if (DBManager.AddAccountDLinkingUserID(targetname, useridtarget.ToString()))
                            {
                                args.Player.SendSuccessMessage($"Added new linked account {targetname} UserID: {useridtarget}");
                                return;
                            } else
                            {
                                args.Player.SendSuccessMessage($"Unable to add new link account");
                                return;
                            }
                        }
                    }
                    #endregion
                case "remove":
                    #region ( Type: Removed )
                    {
                        if (args.Parameters.Count == 1)
                        {
                            args.Player.SendErrorMessage($"Usage: {Commands.Specifier}mklpdiscord remove <player>");
                            return;
                        }

                        string targetname = "";

                        var getplayers = TSPlayer.FindByNameOrID(args.Parameters[1]);
                        if (getplayers.Count > 1)
                        {
                            args.Player.SendMultipleMatchError(getplayers.Select(p => p.Name));
                            return;
                        }
                        if (getplayers.Count < 1)
                        {
                            UserAccount getuseraccount = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);
                            if (getuseraccount == null)
                            {
                                args.Player.SendErrorMessage("Invalid Player User");
                                return;
                            }
                            targetname = getuseraccount.Name;
                        }
                        if (getplayers.Count == 0)
                        {
                            targetname = getplayers[0].Name;
                        }
                        if (targetname == "")
                        {
                            args.Player.SendErrorMessage("Invalid Player User");
                            return;
                        }

                        if (DBManager.DeleteAccountDLinkingUserID(targetname))
                        {
                            args.Player.SendSuccessMessage($"Removed {targetname} accountlink");
                        } else
                        {
                            args.Player.SendErrorMessage($"Unable to remove {targetname} accountlink" +
                                $"\nplease check '{Commands.Specifier}mklpdiscord list' if that name exist");
                        }

                        return;
                    }
                    #endregion
                default:
                    {
                        args.Player.SendErrorMessage("Invalid Sub-Command" +
                            $"\nDo '{Commands.Specifier}mklpdiscord help' for more info");
                        return;
                    }
            }

            #endregion
        }

        #endregion

        #endregion

        #region [ Modified Commands ]

        private void MCMD_AccountInfo(CommandArgs args)
        {
            #region code
            
            if (args.Parameters.Count == 0)
            {
                if ((bool)Config.Main.Replace_AccountInfo_TShockCommand)
                {
                    args.Player.SendErrorMessage($"Proper Usage: {Commands.Specifier}accountinfo <account name>");
                } else
                {
                    args.Player.SendErrorMessage($"Proper Usage: {Commands.Specifier}klpaccountinfo <account name>");
                }
                return;
            }

            UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByName(args.Parameters[0]);

            if (targetaccount == null)
            {
                args.Player.SendErrorMessage("Invalid Account!");
                return;
            }

            sendinfo(targetaccount);

            /*
            int targetaccid = -1;

            if (int.TryParse(args.Parameters[0], out targetaccid))
            {
                UserAccount targetaccount = TShock.UserAccounts.GetUserAccountByID(targetaccid);

                if (targetaccount == null)
                {
                    args.Player.SendErrorMessage("Invalid Account!");
                    return;
                }

                sendinfo(targetaccount);
            } else
            {
                
            }

            */

            void sendinfo(UserAccount account)
            {
                ulong userid = 0;

                try
                {
                    userid = (bool)Config.DataBaseDLink.Target_UserAccount_ID ? LinkAccountManager.GetUserIDByAccountID(account.ID) : LinkAccountManager.GetUserIDByAccountName(account.Name);
                } catch { }


                List<string> iplist = JsonConvert.DeserializeObject<List<string>>(account.KnownIps?.ToString() ?? string.Empty);
                string lastknownip = iplist?[iplist.Count - 1] ?? "N/A";

                string UTC = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString("+#;-#");

                args.Player.SendMessage(
                    $"== Account Info ==\n" +
                    $"Name: [c/ffffff:{account.Name}]\n" +
                    $"Account ID: [c/ffffff:{account.ID}]\n" +
                    $"group: [c/ffffff:{account.Group}]\n" +
                    $"\n{(userid != 0 ? $"Discord UserID: {userid}\n\n" : "")}" +
                    $"Last  known IP: [c/ffffff:{lastknownip}]\n" +
                    $"Last accessed: [c/ffffff:{account.LastAccessed} UTC{UTC}] [c/82ff91:{GetSince(DateTime.Parse(account.LastAccessed))}]\n" +
                    $"Registered Since: [c/ffffff:{account.Registered} UTC{UTC}] [c/82ff91:{GetSince(DateTime.Parse(account.Registered))}]\n",
                    Color.Gray);
            }

            string GetSince(DateTime Since)
            {
                TimeSpan getresult = (DateTime.UtcNow - Since);

                if (getresult.TotalDays >= 1)
                {
                    return $"{Math.Floor(getresult.TotalDays)}{(getresult.TotalDays >= 2 ? "Days" : "Day")} ago";
                }
                if (getresult.TotalHours >= 1)
                {
                    return $"{Math.Floor(getresult.TotalHours)}{(getresult.TotalHours >= 2 ? "Hours" : "Hour")} ago";
                }
                if (getresult.TotalMinutes >= 1)
                {
                    return $"{Math.Floor(getresult.TotalMinutes)}{(getresult.TotalMinutes >= 2 ? "Minutes" : "Minute")} ago";
                }
                if (getresult.TotalSeconds >= 1)
                {
                    return $"{Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds >= 2 ? "Seconds" : "Second")} ago";
                }
                if (getresult.TotalMilliseconds >= 1)
                {
                    return $"{Math.Floor(getresult.TotalMilliseconds)}{(getresult.TotalMilliseconds >= 2 ? "Milliseconds" : "Millisecond")} ago";
                }
                return $"Time {Math.Floor(getresult.TotalSeconds)}{(getresult.TotalSeconds >= 2 ? "Seconds" : "Second")}";
            }

            #endregion
        }

        private void MCMD_Playing(CommandArgs args)
        {
            #region code

            bool invalidUsage = (args.Parameters.Count > 2);

            bool displayIdsRequested = false;
            int pageNumber = 1;
            if (!invalidUsage)
            {
                foreach (string parameter in args.Parameters)
                {
                    if (parameter.Equals("-i", StringComparison.InvariantCultureIgnoreCase))
                    {
                        displayIdsRequested = true;
                        continue;
                    }

                    if (!int.TryParse(parameter, out pageNumber))
                    {
                        invalidUsage = true;
                        break;
                    }
                }
            }
            if (invalidUsage)
            {
                args.Player.SendMessage($"List Online Players Syntax", Color.White);
                args.Player.SendMessage($"{"playing".Color(TShockAPI.Utils.BoldHighlight)} {"[-i]".Color(TShockAPI.Utils.RedHighlight)} {"[page]".Color(TShockAPI.Utils.GreenHighlight)}", Color.White);
                args.Player.SendMessage($"Command aliases: {"playing".Color(TShockAPI.Utils.GreenHighlight)}, {"online".Color(TShockAPI.Utils.GreenHighlight)}, {"who".Color(TShockAPI.Utils.GreenHighlight)}", Color.White);
                args.Player.SendMessage($"Example usage: {"who".Color(TShockAPI.Utils.BoldHighlight)} {"-i".Color(TShockAPI.Utils.RedHighlight)}", Color.White);
                return;
            }
            
            if (displayIdsRequested && !args.Player.HasPermission(Permissions.seeids))
            {
                args.Player.SendErrorMessage("You do not have permission to see player IDs.");
                return;
            }

            if (TShock.Utils.GetActivePlayerCount() == 0)
            {
                args.Player.SendMessage("There are currently no players online.", Color.White);
                return;
            }
            args.Player.SendMessage($"Online Players ({TShock.Utils.GetActivePlayerCount().Color(TShockAPI.Utils.GreenHighlight)}/{TShock.Config.Settings.MaxSlots})", Color.White);

            var players = new List<string>();

            foreach (TSPlayer ply in TShock.Players)
            {
                if (ply != null && ply.Active)
                {
                    if (ply.ContainsData("MKLP_Vanish"))
                    {
                        if (ply.GetData<bool>("MKLP_Vanish")) continue;
                    }

                    if (displayIdsRequested)
                        if (ply.Account != null)
                            players.Add($"{ply.Name} (Index: {ply.Index}, Account ID: {ply.Account.ID})");
                        else
                            players.Add($"{ply.Name} (Index: {ply.Index})");
                    else
                        players.Add(ply.Name);
                }
            }

            PaginationTools.SendPage(
                args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(players),
                new PaginationTools.Settings
                {
                    IncludeHeader = false,
                    FooterFormat = $"Type {Commands.Specifier}who {(displayIdsRequested ? "-i" : string.Empty)} for more."
                }
            );

            #endregion
        }

        #endregion

        #region [ Function ]

        public static void SendStaffMessage(string message, Microsoft.Xna.Framework.Color messagecolor)
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;
                if (!player.HasPermission(Config.Permissions.Staff)) continue;
                player.SendMessage(message, messagecolor);
            }
        }
        
        public static void TogglePlayerVanish(TSPlayer executer, bool vanish)
        {
            #region code
            //PacketTypes.player
            // set player null? ( completely invisible ) in future

            if (vanish)
            {
                if ((bool)Config.Main.Use_VanishCMD_TPlayer_Active_Var)
                {
                    executer.TPlayer.active = false;
                }
                executer.SetData("MKLP_Vanish", true);

                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null) continue;
                    if (player == executer) continue;
                    player.SendData(PacketTypes.PlayerActive, null, executer.Index, false.GetHashCode());
                }
            } else
            {
                if ((bool)Config.Main.Use_VanishCMD_TPlayer_Active_Var)
                {
                    executer.TPlayer.active = true;
                }
                executer.SetData("MKLP_Vanish", false);

                foreach (TSPlayer player in TShock.Players)
                {
                    if (player == null) continue;
                    if (player == executer) continue;
                    player.SendData(PacketTypes.PlayerActive, null, executer.Index, true.GetHashCode());

                    for (int k = 0; k < NetItem.MaxInventory - (NetItem.SafeSlots + NetItem.PiggySlots + NetItem.ForgeSlots); k++)
                    {
                        try
                        {
                            executer.SendData(PacketTypes.PlayerSlot, null, executer.Index, (float)k);
                        }
                        catch (Exception e) { MKLP_Console.SendLog_Exception(e); }
                    }

                    player.SendData(PacketTypes.PlayerInfo, null, executer.Index);
                    player.SendData(PacketTypes.PlayerUpdate, null, executer.Index);
                    player.SendData(PacketTypes.PlayerMana, null, executer.Index);
                    player.SendData(PacketTypes.PlayerHp, null, executer.Index);
                    player.SendData(PacketTypes.PlayerBuff, null, executer.Index);

                    var trashSlot = NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots;

                    for (int k = 0; k < NetItem.MaxInventory - (NetItem.SafeSlots + NetItem.PiggySlots); k++)
                    {
                        player.SendData(PacketTypes.PlayerSlot, null, executer.Index, (float)k);
                    }
                    player.SendData(PacketTypes.PlayerSlot, null, executer.Index, (float)trashSlot);
                }
            }

            #endregion
        }

        public static bool PunishPlayer(MKLP_CodeType CodeType, byte CodeNumber, TSPlayer player, string Reason, string WarningMessage, bool RevertInventory = false)
        {
            #region code

            if (CodeType == MKLP_CodeType.Main)
            {
                switch ((PunishmentType)Config.Main.Main_Code_PunishmentType)
                {
                    case PunishmentType.Ban:
                        {

                            ManagePlayer.OnlineBan(false, player, Reason, "MKLP-AntiCheat", DateTime.MaxValue);
                            return true;
                        }
                    case PunishmentType.Disable:
                        {
                            ManagePlayer.DisablePlayer(player, Reason, "MKLP-AntiCheat", WarningMessage + $"\n-# {CodeType} Code {CodeNumber}");
                            return true;
                        }
                    case PunishmentType.KickAndLog:
                        {
                            Discordklp.KLPBotSendMessage_Warning(WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", player.Name, Reason);
                            player.Kick(Reason, true, false, "MKLP-AntiCheat");
                            return true;
                        }
                    case PunishmentType.Kick:
                        {
                            player.Kick(Reason, true, false, "MKLP-AntiCheat");
                            return true;
                        }
                    case PunishmentType.RevertAndLog:
                        {
                            if (RevertInventory) RevertPlayerInv();
                            Discordklp.KLPBotSendMessage_Warning(WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", player.Name, Reason);
                            player.SendWarningMessage(Reason);
                            return true;
                        }
                    case PunishmentType.Revert:
                        {
                            if (RevertInventory) RevertPlayerInv();
                            player.SendWarningMessage(Reason);
                            return true;
                        }
                    case PunishmentType.Log:
                        {
                            Discordklp.KLPBotSendMessage_Warning(WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", player.Name, Reason);
                            return false;
                        }
                }
                return false;
            }
            if (CodeType == MKLP_CodeType.Survival)
            {
                switch ((PunishmentType)Config.Main.Survival_Code_PunishmentType)
                {
                    case PunishmentType.Ban:
                        {
                            ManagePlayer.OnlineBan(false, player, Reason, "MKLP-AntiCheat", DateTime.MaxValue);
                            return true;
                        }
                    case PunishmentType.Disable:
                        {
                            ManagePlayer.DisablePlayer(player, Reason, "MKLP-AntiCheat", WarningMessage + $"\n-# {CodeType} Code {CodeNumber}");
                            return true;
                        }
                    case PunishmentType.KickAndLog:
                        {
                            Discordklp.KLPBotSendMessage_Warning(WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", player.Name, Reason);
                            player.Kick(Reason, true, false, "MKLP-AntiCheat");
                            return true;
                        }
                    case PunishmentType.Kick:
                        {
                            player.Kick(Reason, true, false, "MKLP-AntiCheat");
                            return true;
                        }
                    case PunishmentType.RevertAndLog:
                        {
                            if (RevertInventory) RevertPlayerInv();
                            Discordklp.KLPBotSendMessage_Warning(WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", player.Name, Reason);
                            player.SendWarningMessage(Reason);
                            return true;
                        }
                    case PunishmentType.Revert:
                        {
                            if (RevertInventory) RevertPlayerInv();
                            player.SendWarningMessage(Reason);
                            return true;
                        }
                    case PunishmentType.Log:
                        {
                            Discordklp.KLPBotSendMessage_Warning(WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", player.Name, Reason);
                            return false;
                        }
                }
                return false;
            }
            if (CodeType == MKLP_CodeType.Default)
            {
                switch ((PunishmentType)Config.Main.Default_Code_PunishmentType)
                {
                    case PunishmentType.Ban:
                        {
                            ManagePlayer.OnlineBan(false, player, Reason, "MKLP-AntiCheat", DateTime.MaxValue);
                            return true;
                        }
                    case PunishmentType.Disable:
                        {
                            ManagePlayer.DisablePlayer(player, Reason, "MKLP-AntiCheat", WarningMessage + $"\n-# {CodeType} Code {CodeNumber}");
                            return true;
                        }
                    case PunishmentType.KickAndLog:
                        {
                            Discordklp.KLPBotSendMessage_Warning(WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", player.Name, Reason);
                            player.Kick(Reason, true, false, "MKLP-AntiCheat");
                            return true;
                        }
                    case PunishmentType.Kick:
                        {
                            player.Kick(Reason, true, false, "MKLP-AntiCheat");
                            return true;
                        }
                    case PunishmentType.RevertAndLog:
                        {
                            if (RevertInventory) RevertPlayerInv();
                            Discordklp.KLPBotSendMessage_Warning(WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", player.Name, Reason);
                            player.SendWarningMessage(Reason);
                            return true;
                        }
                    case PunishmentType.Revert:
                        {
                            if (RevertInventory) RevertPlayerInv();
                            player.SendWarningMessage(Reason);
                            return true;
                        }
                    case PunishmentType.Log:
                        {
                            Discordklp.KLPBotSendMessage_Warning(WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", player.Name, Reason);
                            return false;
                        }
                }
            }
            if (CodeType == MKLP_CodeType.Dupe)
            {
                switch ((PunishmentType)Config.Main.SuspiciousDupe_PunishmentType)
                {
                    case PunishmentType.Ban:
                        {
                            ManagePlayer.OnlineBan(false, player, Reason, "MKLP-AntiCheat", DateTime.MaxValue);
                            return true;
                        }
                    case PunishmentType.Disable:
                        {
                            ManagePlayer.DisablePlayer(player, Reason, "MKLP-AntiCheat", WarningMessage + $"\n-# {CodeType} Code {CodeNumber}");
                            return true;
                        }
                    case PunishmentType.KickAndLog:
                        {
                            Discordklp.KLPBotSendMessage_Warning(WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", player.Name, Reason);
                            player.Kick(Reason, true, false, "MKLP-AntiCheat");
                            return true;
                        }
                    case PunishmentType.Kick:
                        {
                            player.Kick(Reason, true, false, "MKLP-AntiCheat");
                            return true;
                        }
                    case PunishmentType.RevertAndLog:
                        {
                            if (RevertInventory) RevertPlayerInv();
                            Discordklp.KLPBotSendMessage_Warning(WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", player.Name, Reason);
                            player.SendWarningMessage(Reason);
                            return true;
                        }
                    case PunishmentType.Revert:
                        {
                            if (RevertInventory) RevertPlayerInv();
                            player.SendWarningMessage(Reason);
                            return true;
                        }
                    case PunishmentType.Log:
                        {
                            Discordklp.KLPBotSendMessage_Warning(WarningMessage + $"\n-# {CodeType} Code {CodeNumber}", player.Name, Reason);
                            return false;
                        }
                }
                return false;
            }
            return false;

            void RevertPlayerInv()
            {
                Item[] previnv = player.GetData<Item[]>("MKLP_PrevInventory");
                Item[] prevpig = player.GetData<Item[]>("MKLP_PrevPiggyBank");
                Item[] prevsafe = player.GetData<Item[]>("MKLP_PrevSafe");
                Item[] prevforge = player.GetData<Item[]>("MKLP_PrevDefenderForge");
                Item[] prevvault = player.GetData<Item[]>("MKLP_PrevVoidVault");

                player.SetData("MKLP_Confirmed_InvRev", 1);

                // Clear Main Inventory (slots 0–49)
                for (int i = 0; i < NetItem.InventorySlots; i++)
                    player.TPlayer.inventory[i] = previnv[i];

                // Clear Armor and Accessories (slots 50–79)
                //for (int i = 0; i < player.TPlayer.armor.Length; i++)
                //player.TPlayer.armor[i].SetDefaults(0);
                if ((bool)Config.Main.DetectAllPlayerInv)
                {
                    // Clear Piggy Bank
                    for (int i = 0; i < player.TPlayer.bank.item.Length; i++)
                        player.TPlayer.bank.item[i] = prevpig[i];

                    // Clear Safe
                    for (int i = 0; i < player.TPlayer.bank2.item.Length; i++)
                        player.TPlayer.bank2.item[i] = prevsafe[i];

                    // Clear Void Vault (Forge)
                    for (int i = 0; i < player.TPlayer.bank3.item.Length; i++)
                        player.TPlayer.bank3.item[i] = prevforge[i];


                    for (int i = 0; i < player.TPlayer.bank4.item.Length; i++)
                        player.TPlayer.bank4.item[i] = prevvault[i];
                }

                // Send the updated inventory to the client
                for (int k = 0; k < NetItem.MaxInventory - (NetItem.SafeSlots + NetItem.PiggySlots + NetItem.ForgeSlots + NetItem.VoidSlots); k++) //clear all slots excluding bank slots, bank slots cleared in ResetBanks method
                {
                    try
                    {
                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, player.Index, (float)k, 0f, 0f, 0);
                    }
                    catch (Exception e)
                    {
                        MKLP_Console.SendLog_Exception(e);
                    }
                }
            }
            #endregion
        }

        public void Check_SentryAndSummons()
        {
            #region code
            /*
            short[] SentryID =
            {
                ProjectileID.FrostHydra,
                ProjectileID.SpiderHiver,
                ProjectileID.HoundiusShootius,
            };

            short[] SummonID =
            {
                ProjectileID.Pygmy,
                ProjectileID.Pygmy2,
                ProjectileID.Pygmy3,
                ProjectileID.Pygmy4,
                ProjectileID.BabySlime,
                ProjectileID.Raven,
                ProjectileID.Hornet,
                ProjectileID.FlyingImp,
                ProjectileID.Retanimini,
                ProjectileID.Spazmamini,
                ProjectileID.VenomSpider,
                ProjectileID.JumperSpider,
                ProjectileID.DangerousSpider,
                ProjectileID.OneEyedPirate,
                ProjectileID.SoulscourgePirate,
                ProjectileID.PirateCaptain,
                ProjectileID.UFOMinion,
                ProjectileID.DeadlySphere,
                ProjectileID.StardustDragon2,
                ProjectileID.StardustDragon3,
                ProjectileID.BatOfLight,
                ProjectileID.VampireFrog,
                ProjectileID.BabyBird,
                ProjectileID.FlinxMinion,

            };

            short[] OOASentryID =
            {
                ProjectileID.DD2FlameBurstTowerT1,
                ProjectileID.DD2FlameBurstTowerT2,
                ProjectileID.DD2FlameBurstTowerT3,
                ProjectileID.DD2BallistraTowerT1,
                ProjectileID.DD2BallistraTowerT2,
                ProjectileID.DD2BallistraTowerT3,
                ProjectileID.DD2LightningAuraT1,
                ProjectileID.DD2LightningAuraT2,
                ProjectileID.DD2LightningAuraT3,
                ProjectileID.DD2ExplosiveTrapT1,
                ProjectileID.DD2ExplosiveTrapT2,
                ProjectileID.DD2ExplosiveTrapT3,
            };
            */
            //desert tiger & Abigail stack
            /*
            foreach (var player in TShock.Players)
            {
                player.TPlayer.numMinions
            }
            */
            #endregion
        }

        public List<UserAccount> GetMatchUUID_UserAccount(string playername, string UUID)
        {
            #region code
            using var reader = TShock.DB.QueryReader("SELECT * FROM Users WHERE UUID = @0", UUID);

            List<UserAccount> result = new();

            while (reader.Read())
            {
                if (reader.Get<string>("Username") == playername) continue;
                result.Add(new UserAccount{
                    ID = reader.Get<int>("ID"),
                    Group = reader.Get<string>("Usergroup"),
                    UUID = reader.Get<string>("UUID"),
                    Name = reader.Get<string>("Username"),
                    Registered = reader.Get<string>("Registered"),
                    LastAccessed = reader.Get<string>("LastAccessed"),
                    KnownIps = reader.Get<string>("KnownIps")
                });
            }

            return result;
            #endregion
        }

        #endregion
    }
    
    public enum MKLP_CodeType
    {
        Main,
        Survival,
        Default,
        Dupe
    }
    public enum PunishmentType
    {
        Ban,
        Disable,
        KickAndLog,
        Kick,
        RevertAndLog,
        Revert,
        Log
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

        public static void SendLog_LatestVersion(string oldversion, string newversion)
        {
            SendTitle();
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" Warning: ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"MKLP has updated to v{oldversion} to v{newversion}");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("> You can download the latest version at");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"> https://github.com/Nightklpgaming/TShock-GSKLP-Moderation/releases/tag/{newversion}");
            Console.ResetColor();
        }

        public static void SendLog_Warning(object? value)
        {
            SendTitle();
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" Warning: ");
            Console.ResetColor();

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(value);
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