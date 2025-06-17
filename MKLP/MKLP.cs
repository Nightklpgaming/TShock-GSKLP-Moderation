﻿//Microsoft
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
using System.Linq;
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
using static Org.BouncyCastle.Math.EC.ECCurve;
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

        internal static MKLP_DB DBManager = new();

        internal static DiscordKLP Discordklp = new();

        internal static AccountDLinked LinkAccountManager = new();

        internal static Dictionary<string, (string, string, string)> DisabledKey = new();

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

        DateTime InitializeSince = DateTime.UtcNow;

        public override void Initialize()
        {
            InitializeSince = DateTime.UtcNow;
            if (!HasBanGuardPlugin && (bool)Config.Main.UsingBanGuardPlugin)
            {
                Config.Main.UsingBanGuardPlugin = false;
                Config.Changeall();
                MKLP_Console.SendLog_Warning("Warning: BanGuard plugin doesn't Exist on \"ServerPlugins\" Folder!");
            }

            //=====================Player===================
            GetDataHandlers.PlayerUpdate += OnPlayerUpdate;

            //GetDataHandlers.player

            ServerApi.Hooks.NetGreetPlayer.Register(this, OnNetGreetPlayer);

            ServerApi.Hooks.ServerJoin.Register(this, OnPlayerJoin);

            ServerApi.Hooks.ServerLeave.Register(this, OnPlayerLeave);

            PlayerHooks.PlayerCommand += OnPlayerCommand;

            PlayerHooks.PlayerPostLogin += OnPlayerPostLogin;

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

            GetDataHandlers.Sign += OnSignChange;

            //ServerApi.Hooks.NpcAIUpdate.Register(this, OnNPCAIUpdate);

            //ServerApi.Hooks.ProjectileAIUpdate.Register(this, OnProjectileAIUpdate);

            //ServerApi.Hooks.WireTriggerAnnouncementBox.Register(this, OnProjectileAIUpdate);

            //=====================Server===================
            //ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);

            ServerApi.Hooks.ServerBroadcast.Register(this, OnServerBroadcast);

            ServerApi.Hooks.WorldSave.Register(this, OnWorldSave);

            ServerApi.Hooks.GamePostInitialize.Register(this, OnServerStart);

            GeneralHooks.ReloadEvent += OnReload;

            CommandsKLP.INIT();

            if (Config.Discord.BotToken != "NONE")
            {
                Discordklp.Initialize();
            }
            else
            {
                MKLP_Console.SendLog_Message_DiscordBot(GetText("Discord bot token has not been set!"), " {Setup} ");
            }

            LogKLP.InitializeLogging();
        }

        #endregion

        #region [ Dispose ]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //=====================Player===================
                GetDataHandlers.PlayerUpdate -= OnPlayerUpdate;

                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnNetGreetPlayer);

                ServerApi.Hooks.ServerJoin.Deregister(this, OnPlayerJoin);

                ServerApi.Hooks.ServerLeave.Deregister(this, OnPlayerLeave);

                PlayerHooks.PlayerCommand -= OnPlayerCommand;

                PlayerHooks.PlayerPostLogin -= OnPlayerPostLogin;

                //PlayerHooks.PlayerChat -= OnPlayerChat;
                ServerApi.Hooks.ServerChat.Deregister(this, OnChatReceived);

                //=====================game=====================
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);

                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);

                GetDataHandlers.TileEdit -= OnTileEdit;

                GetDataHandlers.PlaceObject -= OnPlaceObject;

                GetDataHandlers.PaintTile -= OnPaintTile;

                GetDataHandlers.PaintWall -= OnPaintWall;

                GetDataHandlers.MassWireOperation -= OnMassWireOperation;

                GetDataHandlers.LiquidSet -= HandleLiquidInteraction;

                GetDataHandlers.NewProjectile -= OnNewProjectile;

                GetDataHandlers.HealOtherPlayer -= OnHealOtherPlayer;

                ServerApi.Hooks.NpcSpawn.Deregister(this, OnNPCSpawn);

                ServerApi.Hooks.NpcKilled.Deregister(this, OnNPCKilled);

                GetDataHandlers.Sign -= OnSignChange;

                GetDataHandlers.PlayerDamage -= OnPlayerDamage;

                GetDataHandlers.KillMe -= OnKillMe;

                //ServerApi.Hooks.NpcAIUpdate.Deregister(this, OnNPCAIUpdate);

                //ServerApi.Hooks.ProjectileAIUpdate.Deregister(this, OnProjectileAIUpdate);

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

        private async void OnGetData(GetDataEventArgs args)
        {
            if (args.Handled)
                return;

            var player = TShock.Players[args.Msg.whoAmI];

            if (args.MsgID == PacketTypes.ForceItemIntoNearestChest)
            {
                if ((bool)Config.Main.ManagePackets.Disable_Packet85_QuickStackChest)
                {
                    args.Handled = true;
                    player.SendData(PacketTypes.PlayerUpdate, "", player.Index);
                    for (int i = 0; i < NetItem.InventorySlots; i++)
                    {
                        player.SendData(PacketTypes.PlayerSlot, "", player.Index, i);
                    }
                }
            }

            if (args.MsgID == PacketTypes.SyncExtraValue)
            {
                if ((bool)Config.Main.ManagePackets.Disable_Packet92_MobPickupCoin)
                {
                    args.Handled = true;
                }
            }

            #region [ ItemDrop ]

            if (args.MsgID != PacketTypes.ItemDrop)
            {
                try
                {
                    if (!(bool)Config.Main.Use_OnUpdate_Func)
                    {
                        int maxvalue = 10;

                        if (Main.hardMode) maxvalue = 100;

                        if ((bool)Config.Main.DisableNode.AutoClear_IllegalItemDrops_SurvivalCode1 || (bool)Config.Main.DisableNode.AutoClear_IllegalItemDrops_MainCode1)
                        {
                            for (int i = 0; i < Main.maxItems; i++)
                            {
                                if (IllegalItemProgression.ContainsKey(Main.item[i].netID) &&
                                    (bool)MKLP.Config.Main.DisableNode.Using_Survival_Code1 && (bool)Config.Main.DisableNode.AutoClear_IllegalItemDrops_SurvivalCode1 && Main.item[i].active)
                                {
                                    Main.item[i].active = false;
                                    TSPlayer.All.SendData(PacketTypes.ItemDrop, "", i);
                                }
                                if ((Main.item[i].value * Main.item[i].stack) / 5000000 >= maxvalue &&
                                    Main.item[i].netID != 74
                                    && (bool)MKLP.Config.Main.DisableNode.Using_Main_Code1 && (bool)Config.Main.DisableNode.AutoClear_IllegalItemDrops_MainCode1 && Main.item[i].active)
                                {
                                    Main.item[i].active = false;
                                    TSPlayer.All.SendData(PacketTypes.ItemDrop, "", i);
                                }
                            }
                        }
                    }
                } catch (Exception e)
                {
                    MKLP_Console.SendLog_Exception("Error on ItemDrop");
                    MKLP_Console.SendLog_Exception(e);
                }
            }

            #endregion

            #region [ latency ]

            if (args.MsgID == PacketTypes.ItemOwner)
            {
                try
                {
                    if (player.ContainsData("MKLP_StartGetLatency"))
                    {
                        player.SetData("MKLP_GetLatency", (DateTime.UtcNow - player.GetData<DateTime>("MKLP_StartGetLatency")).TotalMilliseconds);
                        player.RemoveData("MKLP_StartGetLatency");
                    }
                }
                catch (Exception e)
                {
                    MKLP_Console.SendLog_Exception("Error on ItemOwner\n");
                    MKLP_Console.SendLog_Exception(e);
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
            try
            {
                if (player != null)
                {
                    if (player.ContainsData("MKLP_IsDisabled"))
                    {
                        if (!player.Active || player.Dead)
                            return;

                        if (!player.GetData<bool>("MKLP_IsDisabled"))
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
                }
            }
            catch (Exception e)
            {
                MKLP_Console.SendLog_Exception("Error on Disable");
                MKLP_Console.SendLog_Exception(e);
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
                }
                catch { }
            }

            #endregion

            #region ( Inventory Checking )
            if (args.MsgID == PacketTypes.PlayerSlot)
            {
                try
                {
                    if (!player.IsLoggedIn) return;

                    if (player.HasPermission(Config.Permissions.IgnoreDupeCode_0)) return;

                    if ((bool)Config.Main.Use_OnUpdate_Func) return;
                    if (!(bool)Config.Main.DisableNode.Use_SuspiciousDupe) return;

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
                    }
                    else
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
                catch (Exception e)
                {
                    MKLP_Console.SendLog_Exception("Error on PlayerSlot");
                    MKLP_Console.SendLog_Exception(e);
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
                try
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
                catch (Exception e)
                {
                    MKLP_Console.SendLog_Exception("Error on LoadNetModule (Ping Map)");
                    MKLP_Console.SendLog_Exception(e);
                }
            }
            #endregion

        }

        #endregion

        #region [ OnGameUpdate ]

        private void OnGameUpdate(EventArgs args)
        {
            if (!(bool)Config.Main.Use_OnUpdate_Func) return;
            
            int maxvalue = 10;

            if (Main.hardMode) maxvalue = 100;

            if ((bool)Config.Main.DisableNode.AutoClear_IllegalItemDrops_SurvivalCode1 || (bool)Config.Main.DisableNode.AutoClear_IllegalItemDrops_MainCode1)
            {
                for (int i = 0; i < Main.maxItems; i++)
                {
                    if (IllegalItemProgression.ContainsKey(Main.item[i].netID) &&
                        (bool)MKLP.Config.Main.DisableNode.Using_Survival_Code1 && (bool)Config.Main.DisableNode.AutoClear_IllegalItemDrops_SurvivalCode1 && Main.item[i].active)
                    {
                        Main.item[i].active = false;
                        TSPlayer.All.SendData(PacketTypes.ItemDrop, "", i);
                    }
                    if ((Main.item[i].value * Main.item[i].stack) / 5000000 >= maxvalue &&
                        Main.item[i].netID != 74
                        && (bool)MKLP.Config.Main.DisableNode.Using_Main_Code1 && (bool)Config.Main.DisableNode.AutoClear_IllegalItemDrops_MainCode1 && Main.item[i].active)
                    {
                        Main.item[i].active = false;
                        TSPlayer.All.SendData(PacketTypes.ItemDrop, "", i);
                    }
                }
            }

            if ((DateTime.UtcNow - checklatency_interval).TotalSeconds >= 5)
            {
                checklatency_interval = DateTime.UtcNow;
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
            if (!(bool)Config.Main.DisableNode.Use_SuspiciousDupe) return;

            foreach (var player in TShock.Players)
            {
                if (player == null) continue;
                if (player.HasPermission(Config.Permissions.IgnoreDupeCode_0)) continue;

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

        public static bool LockDown = false;
        public static string LockDownReason = "";

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

        private void OnNetGreetPlayer(GreetPlayerEventArgs args)
        {
            #region code
            TSPlayer player = TShock.Players[args.Who];

            if ((bool)Config.Main.AntiRaid.JoinMessage_OnlyToLoginUser)
            {
                player.SilentJoinInProgress = true;
            }

            #endregion
        }


        DateTime playerjoin_temp1_Since = DateTime.MinValue;
        Dictionary<string, List<string>> playerjoin_temp1 = new();
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
                        player.Disconnect(GetText("You cannot join the server yet!"));
                    }
                    else
                    {
                        player.Disconnect(GetText("You cannot join the server by the reason of") + " " + LockDownReason);
                    }
                    return;
                }

                #endregion

                #region Prevent
                if (Config.Main.IllegalNames.Contains(player.Name))
                {
                    player.Disconnect(GetText("Illegal Name"));
                    return;
                }
                if (player.Name.Contains(DiscordKLP.S_))
                {
                    player.Disconnect(GetText($"You Can't use {DiscordKLP.S_} in your Name!"));
                    return;
                }
                foreach (string contains in Config.Main.Ban_NameContains)
                {
                    if (player.Name.Contains(contains))
                    {
                        player.Disconnect(GetText($"You Can't use {contains} in your Name!"));
                        return;
                    }
                }
                if (player.Name.Length < (byte)Config.Main.Minimum_CharacterName)
                {
                    player.Disconnect(GetText($"You're Name has less than {((byte)Config.Main.Minimum_CharacterName <= 1 ? $"{(byte)Config.Main.Minimum_CharacterName} character" : $"{(byte)Config.Main.Minimum_CharacterName} characters")}"));
                    return;
                }
                if (player.Name.Length > (byte)Config.Main.Maximum_CharacterName)
                {
                    player.Disconnect(GetText($"You're Name has more than {((byte)Config.Main.Maximum_CharacterName <= 1 ? $"{(byte)Config.Main.Maximum_CharacterName} character" : $"{(byte)Config.Main.Maximum_CharacterName} characters")}"));
                    return;
                }
                if (!HasSymbols(player.Name) && !(bool)Config.Main.Allow_PlayerName_Symbols)
                {
                    player.Disconnect(GetText("Your name contains Symbols and is not allowed on this server."));
                    return;
                }
                if (IsIllegalName(player.Name) && !(bool)Config.Main.Allow_PlayerName_InappropriateWords)
                {
                    player.Disconnect(GetText("Your name contains inappropriate language and is not allowed on this server."));
                    return;
                }
                #endregion

                #region Boss Is Present
                if ((bool)Config.BossManager.UsingBossManager)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (!(bool)Config.BossManager.AllowJoinDuringBoss && Main.npc[i].active && Main.npc[i].boss)
                        {
                            player.Disconnect(GetText("The current in-game players must defeat the current boss\nBefore you can join."));
                            return;
                        }
                    }
                }

                #endregion

                #region UUID Match [ alt-acc prevention ]

                var getuuidmatch = GetMatchUUID_UserAccount(player.Name, player.UUID);
                UserAccount useraccount = TShock.UserAccounts.GetUserAccountByName(player.Name);
                if (getuuidmatch.Count != 0 && !(bool)Config.Main.Allow_User_JoinMatchUUID && useraccount == null)
                {
                    string message = Config.Main.Reason_User_JoinMatchUUID;
                    string getaccountname = getuuidmatch[0].Name;
                    bool whitelisted = false;
                    try
                    {
                        foreach (var check in (Config.WhiteListAlt[])Config.Main.WhiteList_User_JoinMatchUUID)
                        {
                            if (check.MainName == player.Name || check.AltNames.Contains(player.Name))
                            {
                                whitelisted = true;
                            }
                        }
                    }
                    catch { }

                    if ((bool)Config.Main.Target_UserMatchUUIDAndIP && !whitelisted)
                    {
                        foreach (UserAccount get in getuuidmatch)
                        {
                            if (JsonConvert.DeserializeObject<List<string>>(get.KnownIps).Contains(player.IP))
                            {
                                message = message.Replace("%matchtype%", "UUID & IP");
                                message = message.Replace("%accountname%", get.Name);
                                player.Disconnect(message);
                                return;
                            }
                        }
                    }
                    else if (!whitelisted)
                    {
                        message = message.Replace("%matchtype%", "UUID");
                        message = message.Replace("%accountname%", getaccountname);
                        player.Disconnect(message);
                        return;
                    }
                }

                #endregion

                #region AntiRaid Check

                if ((DateTime.UtcNow - InitializeSince).Minutes >= (int)Config.Main.AntiRaid.Disable_PlayerJoin_ThreshHold_Until_Minutes && (bool)Config.Main.AntiRaid.Using_PlayerJoin_ThreshHold)
                {
                    if ((DateTime.UtcNow - playerjoin_temp1_Since).TotalSeconds >= (int)Config.Main.AntiRaid.PlayerJoin_ThreshHold_Seconds)
                    {
                        playerjoin_temp1_Since = DateTime.UtcNow;
                        playerjoin_temp1.Clear();
                    }

                    bool add_pjt = true;
                    foreach (var get in playerjoin_temp1)
                    {
                        if (get.Key == player.UUID || get.Value.Contains(player.IP))
                        {
                            add_pjt = false;
                            break;
                        }
                    }
                    if (add_pjt)
                    {
                        if (playerjoin_temp1.ContainsKey(player.UUID))
                        {
                            playerjoin_temp1[player.UUID].Add(player.IP);
                        } else
                        {
                            playerjoin_temp1.Add(player.UUID, new() { player.IP });
                        }
                    }

                    if (playerjoin_temp1.Count >= (int)Config.Main.AntiRaid.PlayerJoin_ThreshHold)
                    {
                        LockDown = true;
                        LockDownReason = Config.Main.AntiRaid.PlayerJoin_ThreshHold_LockdownReason;
                        foreach (var getp in TShock.Players)
                        {
                            if (getp == null) continue;
                            if (playerjoin_temp1.ContainsKey(getp.UUID))
                            {
                                getp.Disconnect(GetText("[MKLP] AutoLockDown Due to many player's join at the same time!"));
                            }
                        }
                        return;
                    }
                }

                #endregion

                #region Check Disabled
                string DisableReason;
                if (ManagePlayer.PlayerIsDisable(player.Name, player.IP, player.UUID, out DisableReason))
                {
                    player.SetData("MKLP_IsDisabled", true);
                    player.SendErrorMessage(GetText("Your still disabled Because of") + " " + DisableReason);
                }
                #endregion

                #region check if muted
                if (DBManager.CheckPlayerMute(player, true))
                {
                    player.SendErrorMessage(GetText("You're still muted!"));
                }
                #endregion

                #region check vanish players
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
                #endregion
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
            TSPlayer player = TShock.Players[args.Who];

            if ((bool)Config.Main.AntiRaid.JoinMessage_OnlyToLoginUser && !player.IsLoggedIn)
            {
                player.SilentKickInProgress = true;
            }

            var godPower = Terraria.GameContent.Creative.CreativePowerManager.Instance.GetPower<Terraria.GameContent.Creative.CreativePowers.GodmodePower>();

            foreach (TSPlayer gplayer in TShock.Players)
            {
                if (gplayer == null) continue;
                if (gplayer.ContainsData("MKLP_TargetSpy"))
                {
                    if (gplayer.GetData<TSPlayer>("MKLP_TargetSpy") == player)
                    {
                        gplayer.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);

                        godPower.SetEnabledState(gplayer.Index, false);

                        TogglePlayerVanish(gplayer, false);

                        gplayer.RemoveData("MKLP_TargetSpy");

                        gplayer.SendInfoMessage(GetText($"You're no longer spying on someone"));
                    }
                }
            }

            #endregion
        }

        public static bool LockDownRegister = false;
        private void OnPlayerCommand(PlayerCommandEventArgs args)
        {
            #region code
            if (args.Handled || args.Player == null)
                return;

            Command command = args.CommandList.FirstOrDefault();

            if (command == null)
                return;

            if (DisabledKey.ContainsKey(Identifier.Name + args.Player.Name) ||
                DisabledKey.ContainsKey(Identifier.IP + args.Player.IP) ||
                DisabledKey.ContainsKey(Identifier.UUID + args.Player.UUID))
            {
                if (command.Name == "register" ||
                    command.Name == "login")
                {
                    args.Player.SendErrorMessage(GetText("You're currently Disabled! you cannot perform this command."));
                    args.Handled = true;
                    return;
                }
            }

            if (command.Name == "register" && LockDownRegister)
            {
                args.Player.SendErrorMessage(GetText("You do not have permission to register at the moment"));
                args.Handled = true;
                return;
            }
            SendCommandLog(command, args.Player, args.CommandPrefix, args.CommandName, args.CommandText);

            void SendCommandLog(Command cmd, TSPlayer player, string cmdprefix, string cmdName, string cmdtext)
            {
                if (Config.Main.Logging.CommandLog_Ignore.Contains(cmd.Name)) return;

                string IsNormal = Config.Main.Logging.CommandLog_Normal.Contains(cmd.Name) ? "☑️" : "⚠️NotNormal";
                string TypePlayer = "";
                string getPlayerName = args.Player.Name;
                if (!player.RealPlayer)
                {
                    TypePlayer = "( Not in a Server )";
                } else if (!player.IsLoggedIn)
                {
                    TypePlayer = "( Not Logged In )";
                } else
                {
                    getPlayerName = player.Account.Name;
                }

                string getcmdtext = Config.Main.Logging.CommandLog_IgnoreARGS.Contains(cmd.Name) ? $"{cmdprefix}{cmdName} (args omitted)" : cmdprefix + cmdtext;
                if (cmd.Name is "register" or "login" or "password") getcmdtext = $"{cmdprefix}{cmdName} (args omitted)";

                Discordklp.KLPBotSendMessageLog((ulong)Config.Discord.CommandLogChannel,
                    GetText($"{TypePlayer} Player **{getPlayerName}** {(cmd.CanRun(player) ? "✅Executed" : "⛔Tried")}|{IsNormal} `{getcmdtext}`"));
            }
            #endregion
        }

        private void OnPlayerPostLogin(PlayerPostLoginEventArgs args)
        {
            #region code

            if ((bool)Config.Main.AntiRaid.JoinMessage_OnlyToLoginUser)
            {
                TSPlayer.All.SendInfoMessage($"{args.Player.Name} has joined.");

                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{args.Player.Name} has joined.");
                Console.ResetColor();
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

            if (!(bool)Config.Main.ChatMod.Using_Chat_AutoMod) return;

            TSPlayer player = TShock.Players[args.Who];

            foreach (string banned in Config.Main.ChatMod.Ban_MessageContains)
            {
                if (args.Text.ToLower().Contains(banned.ToLower()))
                {
                    player.SendErrorMessage(GetText("You can not send that message!"));
                    args.Handled = true;
                    return;
                }
            }
            if (args.Text.Length >= (int)Config.Main.ChatMod.Maximum__MessageLength_NoSpace && !args.Text.Contains(" "))
            {
                player.SendErrorMessage(GetText("You can not send that message!"));
                args.Handled = true;
                return;
            }

            if (args.Text.Length >= (int)Config.Main.ChatMod.Maximum__MessageLength_WithSpace)
            {
                player.SendErrorMessage(GetText("You can not send that message!"));
                args.Handled = true;
                return;
            }

            if (args.Text.Length >= (int)Config.Main.ChatMod.Maximum_Spammed_MessageLength_NoSpace && !args.Text.Contains(" "))
            {
                if (player.ContainsData("MKLP_Chat_Spam_message1"))
                {
                    if ((DateTime.UtcNow - player.GetData<PlayerMessageThreshold>("MKLP_Chat_Spam_message1").Since).TotalMilliseconds < (int)Config.Main.ChatMod.Millisecond_Threshold)
                    {
                        if (player.GetData<PlayerMessageThreshold>("MKLP_Chat_Spam_message1").Threshold >= (int)Config.Main.ChatMod.Threshold_Spammed_MessageLength_NoSpace)
                        {
                            SendWarning();
                            return;
                        }

                        player.SetData("MKLP_Chat_Spam_message1", new PlayerMessageThreshold(player.GetData<PlayerMessageThreshold>("MKLP_Chat_Spam_message1").Threshold + 1, DateTime.UtcNow));

                    }
                    else
                    {
                        player.SetData("MKLP_Chat_Spam_message1", new PlayerMessageThreshold(0, DateTime.UtcNow));
                    }
                }
                else
                {
                    player.SetData("MKLP_Chat_Spam_message1", new PlayerMessageThreshold(1, DateTime.UtcNow));
                }
            }

            if (args.Text.Length >= (int)Config.Main.ChatMod.Maximum_Spammed_MessageLength_WithSpace)
            {
                if (player.ContainsData("MKLP_Chat_Spam_message2"))
                {
                    if ((DateTime.UtcNow - player.GetData<PlayerMessageThreshold>("MKLP_Chat_Spam_message2").Since).TotalMilliseconds < (int)Config.Main.ChatMod.Millisecond_Threshold)
                    {
                        if (player.GetData<PlayerMessageThreshold>("MKLP_Chat_Spam_message2").Threshold >= (int)Config.Main.ChatMod.Threshold_Spammed_MessageLength_WithSpace)
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
                }
                else
                {
                    player.SetData("MKLP_Chat_Warning_message", 1);
                }

                if (player.GetData<int>("MKLP_Chat_Warning_message") >= (int)Config.Main.ChatMod.MutePlayer_AtWarning)
                {
                    if ((bool)Config.Main.ChatMod.PermanentDuration)
                    {
                        ManagePlayer.OnlineMute(false, player, "Spamming/Flooding Messages", "(Auto Chat Mod)", DateTime.MaxValue);
                    }
                    else
                    {
                        ManagePlayer.OnlineMute(false, player, "Spamming/Flooding Messages", "(Auto Chat Mod)", DateTime.UtcNow.AddSeconds((int)Config.Main.ChatMod.MuteDuration_Seconds));
                    }
                    NumberOfMutedPlayers++;
                    args.Handled = true;

                    if ((bool)Config.Main.ChatMod.EnableLockDown_When_MultipleMutes)
                    {
                        if (NumberOfMutedPlayers >= (int)Config.Main.ChatMod.NumberOFPlayersAutoMute_Lockdown)
                        {
                            Discordklp.KLPBotSendMessageMainLog(GetText("Server On 🔒LockDown🔒 Due to Multiple Player Mutes🔇!"));
                            LockDown = true;
                            LockDownReason = Config.Main.ChatMod.AutoLockDown_Reason;
                        }
                    }

                    return;
                }

                player.SendWarningMessage(GetText("Warning! please do not spam/flood the messages!"));
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
                if (IllegalTileProgression.ContainsKey(new(Main.tile[tileX, tileY].type, args.Style)) && !SurvivalManager.MKLP_Tile.ObjectIDs[Main.tile[tileX, tileY].type] && !args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_3) && (bool)Config.Main.DisableNode.Using_Survival_Code3)
                {
                    if (PunishPlayer(MKLP_CodeType.Survival, 3, args.Player, $"{IllegalTileProgression[new(Main.tile[tileX, tileY].type, args.Style)]} Block Place", $"Player **{args.Player.Name}** has placed illegal tile progression `tile id: {Main.tile[tileX, tileY].type} style: {args.Style}` **{IllegalTileProgression[new(Main.tile[tileX, tileY].type, args.Style)]}**"))
                    {
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return;

                    }
                }
                if (IllegalTileProgression.ContainsKey(new(Main.tile[tileX, tileY].type, 0, true)) && !SurvivalManager.MKLP_Tile.ObjectIDs[Main.tile[tileX, tileY].type] && !args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_3) && (bool)Config.Main.DisableNode.Using_Survival_Code3)
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
                    (bool)Config.Main.DisableNode.Using_Survival_Code4)
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


                    if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_protectsurface_break) && (bool)Config.Main.AntiGrief.Using_AntiGrief_Surface_Break)
                    {
                        args.Player.SendErrorMessage(Config.Main.AntiGrief.Message_AntiGrief_Surface_Break);
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
                    if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_protectsurface_place) && (bool)Config.Main.AntiGrief.Using_AntiGrief_Surface_Place)
                    {
                        args.Player.SendErrorMessage(Config.Main.AntiGrief.Message_AntiGrief_Surface_Place);
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

                if (infectionb.Contains(Main.tile[tileX, tileY].type) && !args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_infection) && (bool)Config.Main.AntiGrief.Using_AntiGrief_Infection)
                {
                    args.Player.SendErrorMessage(Config.Main.AntiGrief.Message_AntiGrief_Infection);
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
                    args.Action == GetDataHandlers.EditAction.PlaceWire4) &&
                    tileY >= (int)Main.worldSurface &&
                    (bool)Config.Main.ReceivedWarning_WirePlaceUnderground
                    )
            {
                Discordklp.KLPBotSendMessageMainLog($"Player **{args.Player.Name}** Used Wire/Actuator below surface `{tileX}, {tileY}`");

            }

            if ((bool)Config.Main.Logging.LogTile)
            {
                LogKLP.TileLogS += $"<{DateTime.Now.ToString("s")}> {args.Player.Name} | {args.Action.ToString()}|x:{args.X}|y:{args.Y}\n";
            }

            #region ( Threshold )
            bool KillThreshold()
            {
                if (!(bool)Config.Main.DisableNode.Using_Default_Code1) return false;
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_1)) return false;

                int max = (int)Config.Main.DisableNode.default_code1_maxdefault;

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
                        max = (int)Config.Main.DisableNode.default_code1_maxboost;
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
                        max = (int)Config.Main.DisableNode.default_code1_maxbomb;
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
                        max = (int)Config.Main.DisableNode.default_code1_maxdynamite;
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
                if (!(bool)Config.Main.DisableNode.Using_Default_Code2) return false;
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_2)) return false;

                int max = (int)Config.Main.DisableNode.default_code2_maxdefault;


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
                        max = (int)Config.Main.DisableNode.default_code2_maxboost;
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
                        max = (int)Config.Main.DisableNode.default_code2_maxbomb;
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

            if (IllegalTileProgression.ContainsKey(new(Main.tile[tileX, tileY].type, args.Style)) && !args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_3) && (bool)Config.Main.DisableNode.Using_Survival_Code3)
            {
                if (PunishPlayer(MKLP_CodeType.Survival, 3, args.Player, $"{IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]} Block Place", $"Player **{args.Player.Name}** has placed illegal tile progression `tile id: {Main.tile[tileX, tileY].type} style: {args.Style}` **{IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]}**"))
                {
                    args.Player.SendTileSquareCentered(tileX, tileY, 10);
                    args.Handled = true;
                    return;
                }
            }
            if (IllegalTileProgression.ContainsKey(new(Main.tile[tileX, tileY].type, 0, true)) && !args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_3) && (bool)Config.Main.DisableNode.Using_Survival_Code3)
            {
                if (PunishPlayer(MKLP_CodeType.Survival, 3, args.Player, $"{IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]} Block Place", $"Player **{args.Player.Name}** has placed illegal tile progression `tile id: {Main.tile[tileX, tileY].type} style: {args.Style}` **{IllegalTileProgression[new(Main.tile[tileX, tileY].type, 0, true)]}**"))
                {
                    args.Player.SendTileSquareCentered(tileX, tileY, 10);
                    args.Handled = true;
                    return;
                }
            }


            if ((bool)Config.Main.Logging.LogTile)
            {
                LogKLP.TileLogS += $"<{DateTime.Now.ToString("s")}> {args.Player.Name} | PlaceObject|type:{args.Type}|style:{args.Style}|x:{args.X}|y:{args.Y}\n";
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
                if (!(bool)Config.Main.DisableNode.Using_Default_Code2) return false;
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_2)) return false;

                int max = (int)Config.Main.DisableNode.default_code2_maxdefault;


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
                        max = (int)Config.Main.DisableNode.default_code2_maxboost;
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
                        max = (int)Config.Main.DisableNode.default_code2_maxbomb;
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


            if ((bool)Config.Main.Logging.LogTile)
            {
                LogKLP.TileLogS += $"<{DateTime.Now.ToString("s")}> {args.Player.Name} | PaintTile|type:{args.type}|x:{args.X}|y:{args.Y}\n";
            }

            #region ( Threshold )

            bool PaintThreshold()
            {
                if (!(bool)Config.Main.DisableNode.Using_Default_Code3) return false;
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_3)) return false;

                int max = (int)Config.Main.DisableNode.default_code3_maxdefault;

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
                        max = (int)Config.Main.DisableNode.default_code3_maxboost;
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


            if ((bool)Config.Main.Logging.LogTile)
            {
                LogKLP.TileLogS += $"<{DateTime.Now.ToString("s")}> {args.Player.Name} | PaintWall|type:{args.type}|x:{args.X}|y:{args.Y}\n";
            }

            #region ( Threshold )

            bool PaintThreshold()
            {
                if (!(bool)Config.Main.DisableNode.Using_Default_Code3) return false;
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_3)) return false;

                int max = (int)Config.Main.DisableNode.default_code3_maxdefault;

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
                        max = (int)Config.Main.DisableNode.default_code3_maxboost;
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
                if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_protectsurface_placeliquid) && (bool)Config.Main.AntiGrief.Using_AntiGrief_Surface_PlaceLiquid)
                {
                    args.Player.SendErrorMessage(Config.Main.AntiGrief.Message_AntiGrief_Surface_PlaceLiquid);
                    args.Player.SendTileSquareCentered(TileX, TileY, 4);
                    args.Handled = true;
                    return;
                }
            }

            #region ( Threshold )

            bool LiquidThreshold()
            {
                if (!(bool)Config.Main.DisableNode.Using_Default_Code4) return false;
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_4)) return false;

                int max = (int)Config.Main.DisableNode.default_code4_maxdefault;

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
                        max = (int)Config.Main.DisableNode.default_code4_maxboost;
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
                        max = (int)Config.Main.DisableNode.default_code4_maxbomb;
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

        public static List<int> WhiteList_Projectile_Identity = new();
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

                if (WhiteList_Projectile_Identity.Contains(ident))
                {
                    WhiteList_Projectile_Identity.Remove(ident);
                    return;
                }
                if (ProjectileThreshold()) return;

                Dictionary<short, string> GetIllegalProj = SurvivalManager.GetIllegalProjectile();

                if (args.Player.IsLoggedIn && IllegalProjectileProgression.ContainsKey(type) &&
                    !args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_2) &&
                    (bool)Config.Main.DisableNode.Using_Survival_Code2)
                {
                    if (PunishPlayer(MKLP_CodeType.Survival, 2, args.Player, $"{GetIllegalProj[type]} Projectile", $"Player **{args.Player.Name}** spawned illegal Projectile progression `itemheld: {args.Player.SelectedItem.netID} projectile: {Lang.GetProjectileName(type)}` **{GetIllegalProj[type]}**"))
                    {
                        args.Player.RemoveProjectile(ident, owner);
                        argsHandled();
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
                    if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_infection) && (bool)Config.Main.AntiGrief.Using_AntiGrief_Infection)
                    {
                        args.Player.SendErrorMessage(Config.Main.AntiGrief.Message_AntiGrief_Infection);
                        argsHandled();
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
                    if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_spray) && (bool)Config.Main.AntiGrief.Using_AntiGrief_Spray)
                    {
                        args.Player.SendErrorMessage(Config.Main.AntiGrief.Message_AntiGrief_Spray);
                        argsHandled();
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

                    if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_protectsurface_explosive) && (bool)Config.Main.AntiGrief.Using_AntiGrief_Surface_Explosive)
                    {
                        args.Player.SendErrorMessage(Config.Main.AntiGrief.Message_AntiGrief_Surface_Explosive);
                        argsHandled();
                        return;
                    }
                }

                if (!(bool)Config.Main.Allow_Players_MultipleFishingBobber && Main.projectile[ident].bobber)
                {
                    foreach (var get in Main.projectile)
                    {
                        if (get.identity == ident) continue;
                        if (get.owner != owner) continue;
                        if (get.bobber)
                        {
                            argsHandled();
                            return;
                        }
                    }
                }

                bool ProjectileThreshold()
                {
                    if (!(bool)Config.Main.DisableNode.Using_Default_Code5) return false;
                    if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_5)) return false;

                    int max = (int)Config.Main.DisableNode.default_code5_maxdefault;

                    if (Main.hardMode) max = (int)Config.Main.DisableNode.default_code5_maxHM;

                    if (args.Player.ProjectileThreshold >= max)
                    {
                        if (PunishPlayer(MKLP_CodeType.Default, 5, args.Player, $"Spawning too many projectiles at onces!", $"Player **{args.Player.Name}** Spawned to many projectile at onces! `itemheld: {args.Player.SelectedItem.netID} projectile id: {type}` `Threshold: {max}`"))
                        {
                            argsHandled();
                            return true;
                        }
                    }

                    return false;
                }

                void argsHandled()
                {

                    args.Player.RemoveProjectile(ident, owner);
                    Main.projectile[ident].active = false;
                    TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", ident);
                    //TSPlayer.All.SendData(PacketTypes.ProjectileDestroy, "", ident, owner);
                    args.Handled = true;
                }
            }
            catch (OutOfMemoryException e)
            {
                MKLP_Console.SendLog_Exception(e);
                args.Handled = true;
            }

            #endregion
        }

        private void OnHealOtherPlayer(object sender, GetDataHandlers.HealOtherPlayerEventArgs args)
        {
            #region code
            if (HealOtherThreshold()) return;

            bool HealOtherThreshold()
            {
                if (!(bool)Config.Main.DisableNode.Using_Default_Code6) return false;
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_6)) return false;

                int max = (int)Config.Main.DisableNode.default_code6_maxdefault;

                if (NPC.downedPlantBoss) max = (int)Config.Main.DisableNode.default_code6_maxPlant;

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
                        max += (int)Config.Main.DisableNode.default_code6_addmax_spectrehood;
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

            if (!(bool)Config.BossManager.UsingBossManager) return;

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
			    396, // Moon Lord
            };

            if (!BossIDs.Contains(Main.npc[args.NpcId].type)) return;

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
                        TShock.Utils.Broadcast(GetText("King Slime isn't allowed yet!"), Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.KingSlime_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("There aren't enough players to fight King Slime!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.KingSlime_RequiredPlayersforBoss}"), Color.MediumPurple);
                    }
                }

                if (!NPC.downedBoss1 && npc.type == NPCID.EyeofCthulhu) // Eye of Cthulhu
                {
                    if (!(bool)Config.BossManager.AllowEyeOfCthulhu)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("Eye of Cthulhu isn't allowed yet!"), Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.EyeOfCthulhu_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("There aren't enough players to fight Eye of Cthulhu!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.EyeOfCthulhu_RequiredPlayersforBoss}"), Color.MediumPurple);
                    }
                }

                if (!NPC.downedBoss2 && (npc.type is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail)) // Eater of Worlds
                {
                    if (!(bool)Config.BossManager.AllowEaterOfWorlds)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("Eater of Worlds isn't allowed yet!"), Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.EaterOfWorlds_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("There aren't enough players to fight Eater of Worlds!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.EaterOfWorlds_RequiredPlayersforBoss}"), Color.MediumPurple);
                    }
                }

                if (!NPC.downedBoss2 && npc.type == NPCID.BrainofCthulhu) // Brain of Cthulhu
                {
                    if (!(bool)Config.BossManager.AllowBrainOfCthulhu)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("Brain of Cthulhu isn't allowed yet!"), Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.BrainOfCthulhu_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("There aren't enough players to fight Brain of Cthulhu!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.BrainOfCthulhu_RequiredPlayersforBoss}"), Color.MediumPurple);
                    }
                }

                if (!NPC.downedQueenBee && npc.type == NPCID.QueenBee) // Queen Bee
                {
                    if (!(bool)Config.BossManager.AllowQueenBee)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("Queen Bee isn't allowed yet!"), Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.QueenBee_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("There aren't enough players to fight Queen Bee!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.QueenBee_RequiredPlayersforBoss}"), Color.MediumPurple);
                    }
                }

                if (!NPC.downedBoss3 && npc.type == NPCID.SkeletronHead) // Skeletron
                {
                    if (!(bool)Config.BossManager.AllowSkeletron)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("Skeletron isn't allowed yet!"), Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Skeletron_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("There aren't enough players to fight Skeletron!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.Skeletron_RequiredPlayersforBoss}"), Color.MediumPurple);
                    }
                }

                if (!NPC.downedDeerclops && npc.type == NPCID.Deerclops) // Deerclops
                {
                    if (!(bool)Config.BossManager.AllowDeerclops)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("Deerclops isn't allowed yet!"), Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Deerclops_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("There aren't enough players to fight Deerclops!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.Deerclops_RequiredPlayersforBoss}"), Color.MediumPurple);
                    }
                }

                if (!Main.hardMode && npc.type == NPCID.WallofFlesh) // Wall of Flesh
                {
                    if (!(bool)Config.BossManager.AllowWallOfFlesh)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("Wall of Flesh isn't allowed yet!"), Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.WallOfFlesh_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("There aren't enough players to fight Wall of Flesh!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.WallOfFlesh_RequiredPlayersforBoss}"), Color.MediumPurple);
                    }
                }

                if (!NPC.downedQueenSlime && npc.type == NPCID.QueenSlimeBoss) // Queen Slime
                {
                    if (!(bool)Config.BossManager.AllowQueenSlime)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("Queen Slime isn't allowed yet!"), Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.QueenSlime_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("There aren't enough players to fight Queen Slime!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.QueenSlime_RequiredPlayersforBoss}"), Color.MediumPurple);
                    }
                }

                if (Main.zenithWorld)
                {
                    int[] MechdusaIDs = { NPCID.Retinazer, NPCID.Spazmatism, NPCID.Probe, NPCID.TheDestroyer, NPCID.TheDestroyerBody, NPCID.TheDestroyerTail, NPCID.SkeletronPrime, NPCID.PrimeCannon, NPCID.PrimeLaser, NPCID.PrimeSaw, NPCID.PrimeVice };
                    if ((!NPC.downedMechBoss1 || !NPC.downedMechBoss2 || !NPC.downedMechBoss1) &&
                        (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism || (npc.type == NPCID.TheDestroyer || npc.type == NPCID.TheDestroyerBody || npc.type == NPCID.TheDestroyerTail) || npc.type == NPCID.SkeletronPrime)
                         //&& NPC_Is_Active(new int[] { NPCID.Retinazer, NPCID.Spazmatism, NPCID.Probe, NPCID.TheDestroyer, NPCID.TheDestroyerBody, NPCID.TheDestroyerTail, NPCID.SkeletronPrime, NPCID.PrimeCannon, NPCID.PrimeLaser, NPCID.PrimeSaw, NPCID.PrimeVice })
                         )
                    {
                        await Task.Run(async () => {
                            if (!NPC_Is_Active(MechdusaIDs))
                            {
                                await Task.Delay(800);
                                if (!(bool)Config.BossManager.AllowMechdusa)
                                {
                                    DespawnNPC();
                                    DespawnNPCs(MechdusaIDs);
                                }
                                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Mechdusa_RequiredPlayersforBoss)
                                {
                                    DespawnNPC();
                                    DespawnNPCs(MechdusaIDs);
                                }
                            }
                            else
                            {
                                if (!(bool)Config.BossManager.AllowMechdusa)
                                {
                                    DespawnNPC();
                                    DespawnNPCs(MechdusaIDs);
                                    TShock.Utils.Broadcast(GetText("Mechdusa isn't allowed yet!"), Color.MediumPurple);
                                }
                                if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Mechdusa_RequiredPlayersforBoss)
                                {
                                    DespawnNPC();
                                    DespawnNPCs(MechdusaIDs);
                                    TShock.Utils.Broadcast(GetText("There aren't enough players to fight Mechdusa!") +
                                    GetText($"\nPlayers Needed: {(int)Config.BossManager.Mechdusa_RequiredPlayersforBoss}"), Color.MediumPurple);
                                }
                            }
                        });
                        
                    }
                }
                else
                {
                    if (!NPC.downedMechBoss2 && (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism)) // The Twins
                    {
                        if (!(bool)Config.BossManager.AllowTheTwins)
                        {
                            DespawnNPC();
                            TShock.Utils.Broadcast(GetText("The Twins isn't allowed yet!"), Color.MediumPurple);
                        }
                        if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.TheTwins_RequiredPlayersforBoss)
                        {
                            DespawnNPC();
                            TShock.Utils.Broadcast(GetText("There aren't enough players to fight The Twins!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.TheTwins_RequiredPlayersforBoss}"), Color.MediumPurple);
                        }
                    }

                    if (!NPC.downedMechBoss1 && (npc.type == NPCID.TheDestroyer || npc.type == NPCID.TheDestroyerBody || npc.type == NPCID.TheDestroyerTail)) // The Destroyer
                    {
                        if (!(bool)Config.BossManager.AllowTheDestroyer)
                        {
                            DespawnNPC();
                            TShock.Utils.Broadcast(GetText("The Destroyer isn't allowed yet!"), Color.MediumPurple);
                        }
                        if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.TheDestroyer_RequiredPlayersforBoss)
                        {
                            DespawnNPC();
                            TShock.Utils.Broadcast(GetText("There aren't enough players to fight The Destroyer!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.TheDestroyer_RequiredPlayersforBoss}"), Color.MediumPurple);
                        }
                    }

                    if (!NPC.downedMechBoss3 && npc.type == NPCID.SkeletronPrime) // Skeletron Prime
                    {
                        if (!(bool)Config.BossManager.AllowSkeletronPrime)
                        {
                            DespawnNPC();
                            TShock.Utils.Broadcast(GetText("Skeletron Prime isn't allowed yet!"), Color.MediumPurple);
                        }
                        if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.SkeletronPrime_RequiredPlayersforBoss)
                        {
                            DespawnNPC();
                            TShock.Utils.Broadcast(GetText("There aren't enough players to fight Skeletron Prime!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.SkeletronPrime_RequiredPlayersforBoss}"), Color.MediumPurple);
                        }
                    }
                }



                if (!NPC.downedPlantBoss && npc.type == NPCID.Plantera) // Plantera
                {
                    if (!(bool)Config.BossManager.AllowPlantera)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("Plantera isn't allowed yet!"), Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Plantera_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("There aren't enough players to fight Plantera!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.Plantera_RequiredPlayersforBoss}"), Color.MediumPurple);
                    }
                }

                if (!NPC.downedGolemBoss && npc.type == NPCID.Golem) // Golem
                {
                    if (!(bool)Config.BossManager.AllowGolem)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("Golem isn't allowed yet!"), Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.Golem_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("There aren't enough players to fight Golem!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.Golem_RequiredPlayersforBoss}"), Color.MediumPurple);
                    }
                }

                if (!NPC.downedFishron && npc.type == NPCID.DukeFishron) // Duke Fishron
                {
                    if (!(bool)Config.BossManager.AllowDukeFishron)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("Duke Fishron isn't allowed yet!"), Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.DukeFishron_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("There aren't enough players to fight Duke Fishron!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.DukeFishron_RequiredPlayersforBoss}"), Color.MediumPurple);
                    }
                }

                if (!NPC.downedEmpressOfLight && npc.type == NPCID.HallowBoss) // Empress of Light
                {
                    if (!(bool)Config.BossManager.AllowEmpressOfLight)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("Empress of Light isn't allowed yet!"), Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.EmpressOfLight_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("There aren't enough players to fight Empress of Light!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.EmpressOfLight_RequiredPlayersforBoss}"), Color.MediumPurple);
                    }
                }

                if (!NPC.downedAncientCultist && npc.type == NPCID.CultistBoss) // Lunatic Cultist
                {
                    if (!(bool)Config.BossManager.AllowLunaticCultist)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("Lunatic Cultist isn't allowed yet!"), Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.LunaticCultist_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("There aren't enough players to fight Lunatic Cultist!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.LunaticCultist_RequiredPlayersforBoss}"), Color.MediumPurple);
                    }
                }

                if (!NPC.downedMoonlord && npc.type == NPCID.MoonLordCore) // Moon Lord
                {
                    if (!(bool)Config.BossManager.AllowMoonLord)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("Moon Lord isn't allowed yet!"), Color.MediumPurple);
                    }
                    if (TShock.Utils.GetActivePlayerCount() < (int)Config.BossManager.MoonLord_RequiredPlayersforBoss)
                    {
                        DespawnNPC();
                        TShock.Utils.Broadcast(GetText("There aren't enough players to fight Moon Lord!") +
                            GetText($"\nPlayers Needed: {(int)Config.BossManager.MoonLord_RequiredPlayersforBoss}"), Color.MediumPurple);
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

            bool DespawnNPCs(int[] npcIDs)
            {
                int NPCDel = 0;

                for (int i = 0; i < Main.npc.Length; i++)
                {
                    if (Main.npc[i] == null) continue;
                    if (!Main.npc[i].active) continue;
                    if (npcIDs.Contains(Main.npc[i].type))
                    {
                        Main.npc[i].active = false;
                        Main.npc[i].type = 0;
                        TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                        NPCDel++;
                    }
                }

                return NPCDel > 0;
            }

            bool NPC_Is_Active(int[] npcIDs)
            {
                List<int> result = new();
                foreach (int get in npcIDs)
                {
                    result.Add(get);
                }

                foreach (var gnpc in Main.npc)
                {
                    if (gnpc == null) continue;
                    if (!gnpc.active) continue;
                    if (result.Contains(gnpc.type))
                    {
                        result.Remove(gnpc.type);
                    }
                }

                return result.Count > 0;
            }
            #endregion
        }

        static bool OnCheckIllegal = false;
        private void OnNPCKilled(NpcKilledEventArgs args)
        {
            #region code
            OnCheckIllegal = true;
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

            //if (args.npc.boss)
            if (!BossIDs.Contains(args.npc.type))
            {
                IllegalItemProgression = SurvivalManager.GetIllegalItem();

                IllegalProjectileProgression = SurvivalManager.GetIllegalProjectile();

                IllegalTileProgression = SurvivalManager.GetIllegalTile();

                IllegalWallProgression = SurvivalManager.GetIllegalWall();
            }

            OnCheckIllegal = false;
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

            if (plr != args.Player.Index)
                return true;

            if (args.Player.HasPermission(Config.Permissions.IgnoreMainCode_2) || !(bool)Config.Main.DisableNode.Using_Main_Code2) return false;

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
                                    return PunishPlayer(MKLP_CodeType.Main, 2, args.Player, $"null item boss/invasion spawn", $"Player **{args.Player.Name}** had triggered null item boss/event spawn `itemheld: {args.Player.SelectedItem.Name}` `boss: Eater Of Worlds`");
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

        private void OnSignChange(object? sender, GetDataHandlers.SignEventArgs args)
        {
            #region code
            // Reading the data
            args.Data.Seek(0, SeekOrigin.Begin);
            int signId = args.Data.ReadInt16();
            int posX = args.Data.ReadInt16();
            int posY = args.Data.ReadInt16();
            string newText = args.Data.ReadString();

            if ((bool)Config.Main.Logging.LogSign)
            {
                LogKLP.SignLogS += $"<{DateTime.Now.ToString("s")}> {args.Player.Name} | ChangeSign|x:{posX}|y:{posY}|text : {newText}\n";
            }
            #endregion
        }

        #region later
        /*
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
            #endregion
        }


        private void OnProjectileAIUpdate(ProjectileAiUpdateEventArgs args)
        {
            #region code
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
            #endregion
        }
        */
        #endregion

        private void OnPlayerDamage(object sender, GetDataHandlers.PlayerDamageEventArgs args)
        {
            #region code

            if ((bool)Config.Main.AntiRaid.DeathMessage_OnlyToLoginUser && args.Player.IsLoggedIn && args.ID != args.Player.Index && args.PlayerDeathReason._sourceOtherIndex != 16 &&
                (args.PlayerDeathReason._sourcePlayerIndex == -1 || args.PlayerDeathReason._sourceNPCIndex != -1 || args.PlayerDeathReason._sourceOtherIndex != -1 || args.PlayerDeathReason._sourceCustomReason != null))
            {
                args.Handled = true;
                return;
            }

            #endregion
        }

        private void OnKillMe(object sender, GetDataHandlers.KillMeEventArgs args)
        {
            #region code

            if (args.PlayerDeathReason != null)
            {
                if ((bool)Config.Main.AntiRaid.DeathMessage_OnlyToLoginUser && args.Player.IsLoggedIn && args.PlayerDeathReason._sourceCustomReason != null)
                {
                    args.Handled = true;
                    return;
                }
            }

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
            args.Player.SendMessage(GetText("MKLP config reloaded!"), Microsoft.Xna.Framework.Color.Purple);

            if (!HasBanGuardPlugin && (bool)Config.Main.UsingBanGuardPlugin)
            {
                Config.Main.UsingBanGuardPlugin = false;
                Config.Changeall();
                args.Player.SendWarningMessage(GetText("Warning: BanGuard plugin doesn't Exist on 'ServerPlugins' Folder!"));
                MKLP_Console.SendLog_Warning(GetText("Warning: BanGuard plugin doesn't Exist on \"ServerPlugins\" Folder!"));
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

            LogKLP.SaveLog();
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

        public static void check_bosssched()
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
            }
            else
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

        public static void checkplayers()
        {
            #region code
            foreach (TSPlayer player in TShock.Players)
            {
                if (player == null) continue;
                try
                {
                    DBManager.CheckPlayerMute(player, true);
                }
                catch { }
            }
            #endregion
        }


        #endregion

        #endregion



        #region [ Function ]

        #region [[{ GetText }]]
        public static string GetText(string text)
        {
            return text;
        }
        public static string GetText(string text, params object?[] obj)
        {
            return string.Format(text, obj);
        }
        #endregion

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
            }
            else
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

        public static bool PunishPlayer(MKLP_CodeType CodeType, byte CodeNumber, TSPlayer player, string getReason, string getWarningMessage, bool RevertInventory = false)
        {
            #region code

            string Reason = GetText(getReason);
            string WarningMessage = GetText(getWarningMessage);

            if ((bool)Config.Main.Logging.ModLogTXT_Enable)
            {
                LogKLP.Log_ModLog(
                    $"▬▬▬▬▬▬▬▬▬ {CodeType.ToString()} code {CodeNumber} ▬▬▬▬▬▬▬▬▬" +
                    $"\n{WarningMessage}" +
                    $"\n▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬\n\n"
                    );
            }

            if (CodeType == MKLP_CodeType.Main)
            {
                switch ((PunishmentType)Config.Main.DisableNode.Main_Code_PunishmentType)
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
                if (OnCheckIllegal) return true;
                switch ((PunishmentType)Config.Main.DisableNode.Survival_Code_PunishmentType)
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
                switch ((PunishmentType)Config.Main.DisableNode.Default_Code_PunishmentType)
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
                switch ((PunishmentType)Config.Main.DisableNode.SuspiciousDupe_PunishmentType)
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

        public static List<UserAccount> GetMatchUUID_UserAccount(string playername, string UUID)
        {
            #region code
            using var reader = TShock.DB.QueryReader("SELECT * FROM Users WHERE UUID = @0", UUID);

            List<UserAccount> result = new();

            while (reader.Read())
            {
                if (reader.Get<string>("Username") == playername) continue;
                result.Add(new UserAccount
                {
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

    #region LogKLP
    public static class LogKLP
    {
        public static string LogPath_ModLog = Path.Combine(TShock.SavePath, "logs", "MKLP", "MKLP-ModLogs");
        public static string LogPath_ReportLog = Path.Combine(TShock.SavePath, "logs", "MKLP", "MKLP-ReportLogs");

        public static string LogPath_Tile = Path.Combine(TShock.SavePath, "logs", "MKLP", "Log-Tile");
        public static string LogPath_Sign = Path.Combine(TShock.SavePath, "logs", "MKLP", "Log-Sign");
        public static DateTime Currentlogfile = DateTime.Now;

        public static void InitializeLogging()
        {
            Currentlogfile = DateTime.Now;

            if (!Directory.Exists(LogPath_ModLog) && (bool)MKLP.Config.Main.Logging.ModLogTXT_Enable) Directory.CreateDirectory(LogPath_ModLog);
            if (!Directory.Exists(LogPath_ReportLog) && (bool)MKLP.Config.Main.Logging.ReportLogTXT_Enable) Directory.CreateDirectory(LogPath_ReportLog);

            if (!Directory.Exists(LogPath_Tile) && (bool)MKLP.Config.Main.Logging.LogTile) Directory.CreateDirectory(LogPath_Tile);
            if (!Directory.Exists(LogPath_Sign) && (bool)MKLP.Config.Main.Logging.LogSign) Directory.CreateDirectory(LogPath_Sign);

            if ((bool)MKLP.Config.Main.Logging.ModLogTXT_Enable) LogPath_ModLog = Path.Combine(LogPath_ModLog, $"{Currentlogfile.ToString("dd/MM/yyyy")}.log".Replace("/", "-"));
            if ((bool)MKLP.Config.Main.Logging.ReportLogTXT_Enable) LogPath_ReportLog = Path.Combine(LogPath_ReportLog, $"{Currentlogfile.ToString("dd/MM/yyyy")}.log".Replace("/", "-"));

            if ((bool)MKLP.Config.Main.Logging.LogTile) LogPath_Tile = Path.Combine(LogPath_Tile, $"{Currentlogfile.ToString("dd/MM/yyyy")}.log".Replace("/", "-"));
            if ((bool)MKLP.Config.Main.Logging.LogSign) LogPath_Sign = Path.Combine(LogPath_Sign, $"{Currentlogfile.ToString("dd/MM/yyyy")}.log".Replace("/", "-"));

        }

        public static string TileLogS = "";
        public static string SignLogS = "";

        #region Main
        public static void Log_ModLog(string text)
        {
            if (!(bool)MKLP.Config.Main.Logging.ModLogTXT_Enable) return;

            using (StreamWriter writer = new StreamWriter(LogPath_ModLog, true))
            {
                writer.WriteLine($"{text}");
            }
        }
        public static void Log_Report(string text)
        {
            if (!(bool)MKLP.Config.Main.Logging.ReportLogTXT_Enable) return;

            using (StreamWriter writer = new StreamWriter(LogPath_ReportLog, true))
            {
                writer.WriteLine($"{text}");
            }
        }
        #endregion

        #region server
        public static void Log_Tile(string text)
        {
            if (!(bool)MKLP.Config.Main.Logging.LogTile) return;

            using (StreamWriter writer = new StreamWriter(LogPath_Tile, true))
            {
                writer.WriteLine($"{text}");
            }
        }
        public static void Log_Sign(string text)
        {
            if (!(bool)MKLP.Config.Main.Logging.LogSign) return;

            using (StreamWriter writer = new StreamWriter(LogPath_Sign, true))
            {
                writer.WriteLine($"{text}");
            }
        }
        #endregion


        #region SaveLog

        public static void SaveLog()
        {

            if ((bool)MKLP.Config.Main.Logging.LogTile && TileLogS != "")
            {
                LogKLP.Log_Tile(TileLogS);
            }
            TileLogS = "";
            if ((bool)MKLP.Config.Main.Logging.LogSign && SignLogS != "")
            {
                LogKLP.Log_Sign(SignLogS);
            }
            SignLogS = "";
        }

        #endregion
    }
    #endregion

}