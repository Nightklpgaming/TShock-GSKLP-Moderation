//Microsoft
using IL.Terraria.Graphics;
using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework;
using MKLP.Modules;
using Org.BouncyCastle.Asn1.X509;



//System
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using System.Threading.Channels;



//Terraria
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
//TShock
using TShockAPI;
using TShockAPI.Hooks;

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

        //private IDbConnection db;
        //public static Database.DBManager dbManager;

        public static DiscordKLP Discordklp = new();

        public static List<string> DisabledKey = new();

        public static IDbConnection MAINKLP_DB = new SqliteConnection(("Data Source=" + Path.Combine(TShock.SavePath, "MAINKLP.sqlite")));

        public static Dictionary<TSPlayer, int> PlayerLevel = new();
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

            ServerApi.Hooks.NpcSpawn.Register(this, OnNPCSpawn);

            //=====================Server===================
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);

            GeneralHooks.ReloadEvent += OnReload;

            #region [ Commands Initialize ]

            Commands.ChatCommands.Add(new Command("MKLP.default.latency", CMD_ping, "ping")
            {
                HelpText = "Ping"
            });

            Commands.ChatCommands.Add(new Command("MKLP.default.bossevent", CMD_BE, "bossevent", "be")
            {
                HelpText = "displays defeated bosses and events"
            });

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_InventoryView, InventoryManager.InventoryView, "inventoryview", "invview", "inview")
            {
                HelpText = "View's inventory of a player"
            });

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_Disable, CMD_disable, "disable")
            {
                HelpText = "Acts as Ban but prevents players from doing anything \nwarning: disable's are temporary!"
            });

            Commands.ChatCommands.Add(new Command(Config.Permissions.CMD_Disable, CMD_undisable, "undisable")
            {
                HelpText = "enable a player that got disabled"
            });

            #endregion

            if (Config.Discord.BotToken != "NONE")
            {
                Discordklp.Initialize();
            } else
            {
                Console.WriteLine("Token has not been set");
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

                ServerApi.Hooks.NpcSpawn.Deregister(this, OnNPCSpawn);

                //=====================Server===================
                ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);

                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region [ Events ]

        #region { Player }
        private static void OnPlayerUpdate(object? sender, GetDataHandlers.PlayerUpdateEventArgs args)
        {

        }

        private void OnPlayerJoin(JoinEventArgs args)
        {
            #region code
            var player = TShock.Players[args.Who];
            if (player != null)
            {
                string[] illegalnames =
                {
                    "ServerConsole",
                    "Server"
                };
                if (illegalnames.Contains(player.Name))
                {
                    player.Disconnect("Illegal Name");
                    return;
                }
                if (!HasSymbols(player.Name))
                {
                    player.Disconnect("Your name contains Symbols and is not allowed on this server.");
                    return;
                }
                if (IsIllegalName(player.Name))
                {
                    player.Disconnect("Your name contains inappropriate language and is not allowed on this server.");
                    return;
                }
                foreach (string check in DisabledKey)
                {
                    if (check == player.IP || check == player.UUID || check == player.Name)
                    {
                        player.SendErrorMessage("Your still disabled");
                    }
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
            SyncPlayerLevel();

            if (args.Handled || args.Player == null)
                return;
            if (!args.Player.RealPlayer)
                return;

            Command command = args.CommandList.FirstOrDefault();

            if (command == null)
                return;

            if (DisabledKey.Contains(args.Player.IP) ||
                DisabledKey.Contains(args.Player.UUID))
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

            Discordklp.KLPBotSendMessageLog(Config.Discord.CommandLogChannel, $"Player **{args.Player.Account.Name}** Executed" +
                $" `/{command.Name}{((string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count) == "") ? "": " ")}{string.Join(" ", args.Parameters.ToArray(), 0, args.Parameters.Count)}`");
        }

        public void SyncPlayerLevel()
        {
            foreach (TSPlayer player in TShock.Players)
            {

                if (player == null || !player.Active) continue;

                if (!player.IsLoggedIn)
                {
                    if (PlayerLevel.ContainsKey(player))
                    {
                        PlayerLevel[player] = 1;
                        continue;
                    }
                    else
                    {
                        PlayerLevel.Add(player, 1);
                        continue;
                    }
                }

                KLPAccount playeraccount = new(player.Account.Name);

                if (PlayerLevel.ContainsKey(player))
                {
                    PlayerLevel[player] = playeraccount.Level;
                }
                else
                {
                    PlayerLevel.Add(player, playeraccount.Level);
                }
            }
        }

        #endregion

        #region { Game }
        private void OnUpdate(EventArgs e)
        {


            foreach (TSPlayer player in TShock.Players)
            {

                if (player == null || !player.Active) continue;

                if (!player.IsLoggedIn)
                {
                    continue;
                }

                if (DisabledKey.Contains(player.IP) ||
                    DisabledKey.Contains(player.UUID))
                {
                    if (TShockAPI.Utils.Distance(value2: new Vector2((int)player.TPlayer.position.X / 16, (int)player.TPlayer.position.Y / 16), value1: new Vector2(Main.spawnTileX, Main.spawnTileY)) >= 3f)
                    {
                        player.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);
                        
                    }
                    player.SetBuff(149, 330, true);
                    continue;
                }

                #region ( Inventory Checking )
                /*
                Console.WriteLine($"{player.Name}" +
                    $"tilekill: {player.TileKillThreshold}");
                Console.WriteLine($"{player.Name}" +
                    $"tileplace: {player.TilePlaceThreshold}");
                Console.WriteLine($"{player.Name}" +
                    $"tileliquid: {player.TileLiquidThreshold}");
                Console.WriteLine($"{player.Name}" +
                    $"proj: {player.ProjectileThreshold}");
                Console.WriteLine($"{player.Name}" +
                    $"heal: {player.HealOtherThreshold}");
                */

                if (player.ContainsData("MKLP_PrevInventory"))
                {

                    Item[] prevplayer = player.GetData<Item[]>("MKLP_PrevInventory");

                    if ((!player.HasPermission("tshock.item") || !player.HasPermission("tshock.item.*")) && !player.HasPermission(Config.Permissions.IgnoreSurvivalCode_1)) ManagePlayer.CheckIllegalItemInventory(player);

                    ManagePlayer.CheckPreviousInventory(player, player.TPlayer.inventory, prevplayer);

                    player.SetData("MKLP_PrevInventory", player.TPlayer.inventory.Clone());
                }
                else
                {

                    if ((!player.HasPermission("tshock.item") || !player.HasPermission("tshock.item.*")) && !player.HasPermission(Config.Permissions.IgnoreSurvivalCode_1)) ManagePlayer.CheckIllegalItemInventory(player);

                    player.SetData("MKLP_PrevInventory", player.TPlayer.inventory.Clone());
                }

                #endregion

            }

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

            /*
            Dictionary<short, string> GetIllegalProj = SurvivalManager.GetIllegalProjectile();

            if (GetIllegalProj.ContainsKey(type) && args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_3))
            {
                ManagePlayer.DisablePlayer(args.Player, $"{GetIllegalProj[type]} Block Place", ServerReason: $"Survival,code,3|{type}|{GetIllegalTile[type]}");
                args.Player.RemoveProjectile(ident, owner);
                args.Handled = true;
                return;
            }
            */


            if (args.Action == GetDataHandlers.EditAction.KillTile ||
                args.Action == GetDataHandlers.EditAction.KillWall ||
                args.Action == GetDataHandlers.EditAction.TryKillTile)
            {
                if (tileY < 600)
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


                    if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_protectsurface_break))
                    {
                        args.Player.SendErrorMessage("you Cannot Break any tiles on surface till you get level 5");
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
                if (tileY < 600)
                {
                    ushort[] whitelist =
                    {
                        TileID.WorkBenches,
                        TileID.Furnaces,
                        TileID.Anvils
                    };
                    if (args.Action == GetDataHandlers.EditAction.PlaceTile &&
                        whitelist.Contains(Main.tile[tileX, tileY].type)) return;


                    if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_protectsurface_break))
                    {
                        args.Player.SendErrorMessage("you Cannot Place any tiles on surface till you get level 5");
                        args.Player.SendTileSquareCentered(tileX, tileY, 4);
                        args.Handled = true;
                        return;
                    }
                }
            }

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
                    args.Player.HasPermission("tshock.item") ||
                    args.Player.HasPermission("tshock.item.*")
                    )
                    )
                {
                    Discordklp.KLPBotSendMessage_Warning($"Player **{args.Player.Name}** was Able to use Wire/Actuator on pre skeletron! `{tileX}, {tileY}`", args.Player.Account.Name, "Illegal Wire/Actuator Progression");
                    args.Player.SendErrorMessage("This is Illegal on this progression!");
                    args.Player.SendTileSquareCentered(tileX, tileY, 4);
                    args.Handled = true;
                    return;
                }
            }

            #region ( Threshold )
            bool KillThreshold()
            {
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_1)) return false;

                int max = 150;

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
                        max = 250;
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
                        max = 450;
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
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_2)) return false;

                int max = 20;


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
                        max = 40;
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
                        max = 160;
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
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_3)) return false;

                int max = 20;

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
                        max = 40;
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
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_4)) return false;

                int max = 20;

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
                        max = 40;
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
                        max = 160;
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
            if (HealOtherThreshold()) return;

            Dictionary<short, string> GetIllegalProj = SurvivalManager.GetIllegalProjectile();

            /*
            if (GetIllegalProj.ContainsKey(type) && args.Player.HasPermission(Config.Permissions.IgnoreSurvivalCode_2))
            {
                ManagePlayer.DisablePlayer(args.Player, $"{GetIllegalProj[type]} Projectile", ServerReason: $"Survival,code,2|{type}|{GetIllegalProj[type]}");
                args.Player.RemoveProjectile(ident, owner);
                args.Handled = true;
                return;
            }
            */

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
                if (args.Player.TileY < 600) return;

                if (!args.Player.HasPermission(Config.Permissions.IgnoreAntiGrief_protectsurface_explosive))
                {
                    args.Player.SendErrorMessage("You can't use any explosives here!");
                    args.Player.RemoveProjectile(ident, owner);
                    args.Handled = true;
                    return;
                }
            }


            bool ProjectileThreshold()
            {
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_5)) return false;

                int max = 40;

                if (Main.hardMode) max = 55;

                if (NPC.downedFishron) max = 65;

                if (NPC.downedGolemBoss) max = 75;

                if (args.Player.ProjectileThreshold >= max)
                {
                    ManagePlayer.DisablePlayer(args.Player, $"Spawning too many projectiles at onces!", ServerReason: $"Default,code,5|{args.Player.SelectedItem.netID}|{type}");
                    args.Player.RemoveProjectile(ident, owner);
                    args.Handled = true;
                    return true;
                }

                return false;
            }

            bool HealOtherThreshold()
            {
                if (args.Player.HasPermission(Config.Permissions.IgnoreDefaultCode_6)) return false;
                
                int max = 30;

                if (NPC.downedPlantBoss) max = 45;

                foreach (TSPlayer player in TShock.Players)
                {
                    bool head = false; bool chestplate = false; bool leggings = false;
                    foreach (Item check in player.TPlayer.armor)
                    {
                        if (check.netID == ItemID.SpectreHood) head = true;
                        if (check.netID == ItemID.SpectreRobe) chestplate = true;
                        if (check.netID == ItemID.SpectrePants) leggings = true;

                    }
                    if (head && chestplate && leggings)
                    {
                        max += 10;
                    }
                }

                if (args.Player.HealOtherThreshold >= max)
                {
                    ManagePlayer.DisablePlayer(args.Player, $"Healing others to fast!", ServerReason: $"Default,code,6|{args.Player.SelectedItem.netID}");
                    args.Player.RemoveProjectile(ident, owner);
                    args.Handled = true;
                    return true;
                }
                
                return false;
            }
            #endregion
        }

        public void OnNPCSpawn(NpcSpawnEventArgs args)
        {
            #region code
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (Config.BossManager.PreventIllegalBoss)
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

                if (!Config.BossManager.AllowKingSlime && npc.type == NPCID.KingSlime) // King Slime
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowEyeOfCthulhu && npc.type == NPCID.EyeofCthulhu) // Eye of Cthulhu
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowEaterOfWorlds && npc.type == NPCID.EaterofWorldsHead) // Eater of Worlds
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowBrainOfCthulhu && npc.type == NPCID.BrainofCthulhu) // Brain of Cthulhu
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowQueenBee && npc.type == NPCID.QueenBee) // Queen Bee
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowSkeletron && npc.type == NPCID.SkeletronHead) // Skeletron
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowDeerclops && npc.type == NPCID.Deerclops) // Deerclops
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowWallOfFlesh && npc.type == NPCID.WallofFlesh) // Wall of Flesh
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowQueenSlime && npc.type == NPCID.QueenSlimeBoss) // Queen Slime
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowTheTwins && (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism)) // The Twins
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowTheDestroyer && npc.type == NPCID.TheDestroyer) // The Destroyer
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowSkeletronPrime && npc.type == NPCID.SkeletronPrime) // Skeletron Prime
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowPlantera && npc.type == NPCID.Plantera) // Plantera
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowGolem && npc.type == NPCID.Golem) // Golem
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowDukeFishron && npc.type == NPCID.DukeFishron) // Duke Fishron
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowEmpressOfLight && npc.type == NPCID.HallowBoss) // Empress of Light
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowLunaticCultist && npc.type == NPCID.CultistBoss) // Lunatic Cultist
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }

                if (!Config.BossManager.AllowMoonLord && npc.type == NPCID.MoonLordCore) // Moon Lord
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }
            }
            #endregion
        }

        #endregion

        #region { Server }

        private void OnGetData(GetDataEventArgs args)
        {

            if (args.Handled)
                return;

            var player = TShock.Players[args.Msg.whoAmI];
            if (player == null || !player.Active || player.Dead)
                return;

            if (args.MsgID == PacketTypes.PlayerSlot ||
                args.MsgID == PacketTypes.PlayerUpdate ||
                args.MsgID == PacketTypes.ItemOwner ||
                args.MsgID == PacketTypes.ClientSyncedInventory)
                return;

            if (DisabledKey.Contains(player.IP) ||
                DisabledKey.Contains(player.UUID))
            {
                // Prevent the packet from being processed
                args.Handled = true;
            }

        }

        private void OnReload(ReloadEventArgs args)
        {
            Config = Config.Read();
            args.Player.SendInfoMessage("MKLP config reloaded!");
        }

        #endregion

        #endregion


        private void CMD_BE(CommandArgs args)
        {
            #region { stringdefeatedbosses }
            string GetListDefeatedBoss()
            {
                CONFIG_BOSSES getenabledboss = Config.BossManager;
                Dictionary<string, bool> defeatedbosses = new();
                if (getenabledboss.AllowKingSlime)
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
                if (getenabledboss.AllowEyeOfCthulhu)
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
                if (getenabledboss.AllowEaterOfWorlds || getenabledboss.AllowBrainOfCthulhu)
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
                if (getenabledboss.AllowDeerclops)
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
                if (getenabledboss.AllowQueenBee)
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
                if (getenabledboss.AllowSkeletron)
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
                if (getenabledboss.AllowWallOfFlesh)
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
                if (getenabledboss.AllowQueenSlime)
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
                    if (getenabledboss.AllowTheDestroyer && getenabledboss.AllowTheTwins && getenabledboss.AllowSkeletronPrime)
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
                    if (getenabledboss.AllowTheDestroyer)
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
                    if (getenabledboss.AllowTheTwins)
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
                    if (getenabledboss.AllowSkeletronPrime)
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

                if (getenabledboss.AllowDukeFishron)
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
                if (getenabledboss.AllowPlantera)
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
                if (getenabledboss.AllowEmpressOfLight)
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
                if (getenabledboss.AllowGolem)
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
                if (getenabledboss.AllowLunaticCultist)
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
                if (getenabledboss.AllowMoonLord)
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

        #region [ Disable Functions ]

        #region { Commands }
        private void CMD_disable(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Proper Usage: /disable <player> <reason>");
                return;
            }

            var getplayers = TSPlayer.FindByNameOrID(args.Parameters[0]);
            if (getplayers.Count == 0)
            {
                args.Player.SendErrorMessage("Invalid Player");
                return;
            }

            if (args.Parameters.Count == 1)
            {

                ManagePlayer.DisablePlayer(getplayers[0], executername: args.Player.Name);
                args.Player.SendSuccessMessage($"Player {getplayers[0].Name} disabled");

            } else
            {
                ManagePlayer.DisablePlayer(getplayers[0], args.Parameters[1], executername: args.Player.Name);
                args.Player.SendSuccessMessage($"Player {getplayers[0].Name} disabled");

            }

            return;
        }

        private void CMD_undisable(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Proper Usage: /undisable <player>");
                return;
            }

            var getplayers = TSPlayer.FindByNameOrID(args.Parameters[0]);
            if (getplayers.Count == 0)
            {
                args.Player.SendErrorMessage("Invalid Player");
                return;
            }

            if (ManagePlayer.UnDisablePlayer(getplayers[0], args.Player.Account.Name))
            {
                args.Player.SendSuccessMessage($"Player {getplayers[0].Name} enabled");
            } else
            {
                args.Player.SendErrorMessage($"Player {getplayers[0].Name} isn't disabled");
            }



        }

        private void CMD_ping(CommandArgs args)
        {

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
        }

        #endregion

        #region { Functions }


        #endregion

        #endregion
    }
}