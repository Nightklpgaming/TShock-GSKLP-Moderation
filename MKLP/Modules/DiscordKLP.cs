
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
//microsoft
using Microsoft.Xna.Framework;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
//discord
using Discord;
using Discord.Net;
using Discord.Interactions;
using Discord.WebSocket;
using TShockAPI;
using Color = Microsoft.Xna.Framework.Color;
using TShockAPI.DB;
using Terraria;
using Microsoft.Xna.Framework.Input;

namespace MKLP.Modules
{
    public class DiscordKLP
    {

        private DiscordSocketClient _client;
        //private MessageQueue messageQueue { get; set; }

        public async void Initialize()
        {
            #region | Discord Initialize |

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent | GatewayIntents.All,
            });

            _client.Log += Log;
            _client.Ready += Ready;
            _client.ButtonExecuted += ButtonHandler;
            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.MessageReceived += MessageRecieved;
            _client.ModalSubmitted += ModalHandler;
            _client.SelectMenuExecuted += SelectMenuHandler;

            _client.LoginAsync(TokenType.Bot, MKLP.Config.Discord.BotToken).Wait();
            _client.StartAsync().Wait();

            //messageQueue = new MessageQueue(500);
            //messageQueue.OnReadyToSend += this.OnMessageReadyToSend;

            await Task.Delay(-1);

            #endregion
        }


        #region ==[ Discord ]==

        const string TSStaffPermission = "MKLP.Staff";

        static readonly Discord.Color EmbedColor = Discord.Color.DarkBlue;

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        private async Task Ready()
        {
            #region code

            #region [ Slash Commands ]

            List<SlashCommandBuilder> Guildcommands = new()
            {
                new SlashCommandBuilder()
                    .WithName("moderation")
                    .WithDescription("Manage Server in-game"),
                new SlashCommandBuilder()
                    .WithName("user")
                    .WithDescription("Manage Players")
            };
            #endregion

            var guild = _client.GetGuild(MKLP.Config.Discord.MainGuildID);

            try
            {
                // building slash command commands
                foreach (var command in Guildcommands)
                {
                    await guild.CreateApplicationCommandAsync(command.Build());
                }
            }
            catch (ApplicationCommandException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                Console.WriteLine(json);
            }

            // logs
            Console.WriteLine("Bot is connected and ready!");

            #endregion
        }

        private async Task ButtonHandler(SocketMessageComponent message)
        {
            #region code

            if (message.Data.CustomId == "XXX")
            {
                await message.RespondAsync("||https://tenor.com/view/rick-roll-rick-ashley-never-gonna-give-you-up-gif-22113173||", ephemeral: true);
                return;
            }

            if (message.Data.CustomId.Split("_")[0] != "MKLP") return;

            UserAccount? executer = null;

            try
            {
                executer = TShock.UserAccounts.GetUserAccountByName(ManagePlayer.GetPlayerNameFromUserID(message.User.Id));
            }
            catch (NullReferenceException) { }

            if (executer == null)
            {
                await message.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                return;
            }

            var getgroup = TShock.Groups.GetGroupByName(executer.Group);

            if (getgroup == null)
            {
                await message.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                return;
            }

            if (!getgroup.HasPermission(TSStaffPermission))
            {
                await message.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                return;
            }

            switch (message.Data.CustomId.Split("_")[1])
            {
                case "DismissMsg":
                    #region ( Type | DismissMessage )
                    {
                        switch (message.Data.CustomId.Split("_")[2])
                        {
                            case "Disable":
                                #region ( Type: disable message )
                                {
                                    var buttons = new ComponentBuilder()
                                        .WithButton("Dismiss", "XXX", ButtonStyle.Secondary, disabled: true)
                                        .WithButton("Check Player", "X1", emote: new Emoji("\U0001F4B3"), disabled: true)
                                        .WithButton("Quick Ban [ permanent ]", "X2", ButtonStyle.Danger, emote: new Emoji("\U0001F528"), row: 1, disabled: true)
                                        .WithButton("Enable", "X3", ButtonStyle.Success, emote: new Emoji("\U00002705"), row: 1, disabled: true);
                                    await message.ModifyOriginalResponseAsync(msg => {
                                        msg.Components = buttons.Build();
                                    });
                                    return;
                                }
                                #endregion
                            case "Warning":
                                #region ( Type: disable message )
                                {
                                    var buttons = new ComponentBuilder()
                                        .WithButton("Dismiss", "XXX", ButtonStyle.Secondary, disabled: true)
                                        .WithButton("Check Player", "X1", emote: new Emoji("\U0001F4B3"), disabled: true)
                                        .WithButton("Quick Ban [ permanent ]", "X2", ButtonStyle.Danger, emote: new Emoji("\U0001F528"), row: 1, disabled: true);
                                    await message.ModifyOriginalResponseAsync(msg => {
                                        msg.Components = buttons.Build();
                                    });
                                    return;
                                }
                                #endregion
                        }
                        return;
                    }
                    #endregion
                case "SendMsg":
                    #region ( Type | SendMessage )
                    {

                        switch (message.Data.CustomId.Split("_")[2])
                        {
                            case "PlayerModView":
                                #region ( Type: PlayerModView )
                                {
                                    TSPlayer? targetplayer = null;
                                    foreach (TSPlayer player in TShock.Players)
                                    {
                                        if (player == null || !player.Active) continue;

                                        if (player.Account.Name == message.Data.CustomId.Split("_")[4])
                                        {
                                            targetplayer = player;
                                        }
                                    }

                                    if (targetplayer == null)
                                    {
                                        await message.RespondAsync($"Player {message.Data.CustomId.Split("_")[4]} isn't online", ephemeral: true);
                                        return;
                                    }
                                    
                                    switch (message.Data.CustomId.Split("_")[3])
                                    {
                                        case "Main":
                                            {
                                                var buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerModView_Main_"+targetplayer.Account.Name, ButtonStyle.Secondary)
                                                    .WithButton("Main", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("View Inventory", "MKLP_SendMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary, row: 1)
                                                    .WithButton("Ban Player", "MKLP_InGame_PlayerAction_Ban_" + targetplayer.Account.Name, ButtonStyle.Danger, row: 2)
                                                    .WithButton("Disable Player", "MKLP_InGame_PlayerAction_Disable_" + targetplayer.Account.Name, ButtonStyle.Danger, row: 2)
                                                    .WithButton("Mute Player", "MKLP_InGame_PlayerAction_Mute_" + targetplayer.Account.Name, ButtonStyle.Danger, row: 2);


                                                var embed = new EmbedBuilder()
                                                    .WithTitle($"Player [ {targetplayer.Index} ] {targetplayer.Name}")
                                                    .WithDescription($"**Health:** `{targetplayer.PlayerData.maxHealth}❤️`" +
                                                    $"\n**Mana:** `{targetplayer.PlayerData.maxMana}⭐`" +
                                                    $"\n" +
                                                    $"\n**Coords:** `{targetplayer.TileX} {targetplayer.TileY}` `x y`" +
                                                    $"\n" +
                                                    $"\n**Account Info**" +
                                                    $"\n> **Account Name:** `{targetplayer.Account.Name}`" +
                                                    $"\n> **Account ID:** `{targetplayer.Account.ID}`" +
                                                    $"\n> **Registered:** `{targetplayer.Account.Registered}`" +
                                                    $"\n" +
                                                    $"\n`LoggedIn: {(targetplayer.IsLoggedIn ? "✅" : "❌")}` " +
                                                    $"`Disabled: {((MKLP.DisabledKey.Contains(targetplayer.IP) || MKLP.DisabledKey.Contains(targetplayer.UUID)) ? "✅" : "❌")}` " +
                                                    $"`Muted: {(targetplayer.mute ? "✅" : "❌")}`")
                                                    .WithColor(EmbedColor);

                                                await message.RespondAsync(embed: embed.Build(), ephemeral: true, components: buttons.Build());
                                                

                                                break;
                                            }
                                    }



                                    return;
                                }
                            #endregion
                            case "PlayerViewInventory":
                                #region ( Type: PlayerViewInventory )
                                {
                                    TSPlayer? targetplayer = null;
                                    foreach (TSPlayer player in TShock.Players)
                                    {
                                        if (player == null || !player.Active) continue;

                                        if (player.Account.Name == message.Data.CustomId.Split("_")[4])
                                        {
                                            targetplayer = player;
                                        }
                                    }

                                    if (targetplayer == null)
                                    {
                                        await message.RespondAsync($"Player {message.Data.CustomId.Split("_")[4]} isn't online", ephemeral: true);
                                        return;
                                    }

                                    string EmbedDescription = $"**Held Item:** `{targetplayer.TPlayer.HeldItem.Name} ({targetplayer.TPlayer.HeldItem.stack})`" +
                                        $"\n\n```";

                                    var buttons = new ComponentBuilder();

                                    #region { inventory type }
                                    switch (message.Data.CustomId.Split("_")[3])
                                    {
                                        case "Inventory":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "inventory").Inventory;
                                                break;
                                            }
                                        case "Equipment":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "equipment").Equipment;
                                                break;
                                            }
                                        case "PiggyBank":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "piggybank").PiggyBank;
                                                break;
                                            }
                                        case "Safe":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "safe").Safe;
                                                break;
                                            }
                                        case "DefForge":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "defenderforge").DefenderForge;
                                                break;
                                            }
                                        case "VoidVault":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "voidvault").VoidVault;
                                                break;
                                            }
                                        case "InventoryLogs":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3, disabled: true);
                                                int total = 0;
                                                foreach (string invlog in InventoryManager.InventoryLogs)
                                                {
                                                    if (invlog.Split("|")[0] == targetplayer.Account.Name)
                                                    {
                                                        total++;
                                                    }
                                                }
                                                int count = 0;
                                                foreach (string invlog in InventoryManager.InventoryLogs)
                                                {
                                                    if (invlog.Split("|")[0] == targetplayer.Account.Name)
                                                    {
                                                        count++;
                                                        string previtem = $"{Lang.GetItemNameValue(int.Parse(invlog.Split("|")[3].Split(",")[0]))} ({invlog.Split("|")[3].Split(",")[1]})";
                                                        string item = $"{Lang.GetItemNameValue(int.Parse(invlog.Split("|")[4].Split(",")[0]))} ({invlog.Split("|")[4].Split(",")[1]})";
                                                        if (count < (total - 20)) continue;
                                                        EmbedDescription += $"{invlog.Split("|")[1]}{invlog.Split("|")[2]}: {previtem} => {item}\n";
                                                    }
                                                }
                                                break;
                                            }
                                    }
                                    #endregion

                                    var embed = new EmbedBuilder()
                                        .WithTitle($"Player [ {targetplayer.Index} ] {targetplayer.Name}")
                                        .WithDescription(EmbedDescription+"```")
                                        .WithColor(EmbedColor);

                                    await message.RespondAsync(embed: embed.Build(), ephemeral: true, components: buttons.Build());

                                    return;
                                }
                                #endregion
                        }

                        return;
                    }
                #endregion
                case "EditMsg":
                    #region ( Type | EditMessage )
                    {

                        switch (message.Data.CustomId.Split("_")[2])
                        {
                            case "PlayerModView":
                                #region ( Type: PlayerModView )
                                {
                                    TSPlayer? targetplayer = null;
                                    foreach (TSPlayer player in TShock.Players)
                                    {
                                        if (player == null || !player.Active) continue;

                                        if (player.Account.Name == message.Data.CustomId.Split("_")[4])
                                        {
                                            targetplayer = player;
                                        }
                                    }

                                    if (targetplayer == null)
                                    {
                                        await message.RespondAsync($"Player {message.Data.CustomId.Split("_")[4]} isn't online", ephemeral: true);
                                        return;
                                    }

                                    switch (message.Data.CustomId.Split("_")[3])
                                    {
                                        case "Main":
                                            {
                                                var buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerModView_Main_" + targetplayer.Account.Name, ButtonStyle.Secondary)
                                                    .WithButton("Main", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("View Inventory", "MKLP_SendMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary, row: 1)
                                                    .WithButton("Ban Player", "MKLP_InGame_PlayerAction_Ban_" + targetplayer.Account.Name, ButtonStyle.Danger, row: 2)
                                                    .WithButton("Disable Player", "MKLP_InGame_PlayerAction_Disable_" + targetplayer.Account.Name, ButtonStyle.Danger, row: 2)
                                                    .WithButton("Mute Player", "MKLP_InGame_PlayerAction_Mute_" + targetplayer.Account.Name, ButtonStyle.Danger, row: 2);


                                                var embed = new EmbedBuilder()
                                                    .WithTitle($"Player [ {targetplayer.Index} ] {targetplayer.Name}")
                                                    .WithDescription($"**Health:** `{targetplayer.PlayerData.maxHealth}❤️`" +
                                                    $"\n**Mana:** `{targetplayer.PlayerData.maxMana}⭐`" +
                                                    $"\n" +
                                                    $"\n**Coords:** `{targetplayer.TileX} {targetplayer.TileY}` `x y`" +
                                                    $"\n" +
                                                    $"\n**Account Info**" +
                                                    $"\n> **Account Name:** `{targetplayer.Account.Name}`" +
                                                    $"\n> **Account ID:** `{targetplayer.Account.ID}`" +
                                                    $"\n> **Registered:** `{targetplayer.Account.Registered}`" +
                                                    $"\n" +
                                                    $"\n`LoggedIn: {(targetplayer.IsLoggedIn ? "✅" : "❌")}` " +
                                                    $"`Disabled: {((MKLP.DisabledKey.Contains(targetplayer.IP) || MKLP.DisabledKey.Contains(targetplayer.UUID)) ? "✅" : "❌")}` " +
                                                    $"`Muted: {(targetplayer.mute ? "✅" : "❌")}`")
                                                    .WithColor(EmbedColor);

                                                await message.DeferAsync(true);

                                                await message.ModifyOriginalResponseAsync(msg => {
                                                    msg.Embed = embed.Build();
                                                    msg.Components = buttons.Build();
                                                });

                                                break;
                                            }
                                    }



                                    return;
                                }
                                #endregion
                            case "PlayerViewInventory":
                                #region ( Type: PlayerViewInventory )
                                {
                                    TSPlayer? targetplayer = null;
                                    foreach (TSPlayer player in TShock.Players)
                                    {
                                        if (player == null || !player.Active) continue;

                                        if (player.Account.Name == message.Data.CustomId.Split("_")[4])
                                        {
                                            targetplayer = player;
                                        }
                                    }

                                    if (targetplayer == null)
                                    {
                                        await message.RespondAsync($"Player {message.Data.CustomId.Split("_")[4]} isn't online", ephemeral: true);
                                        return;
                                    }

                                    string EmbedDescription = $"**Held Item:** `{targetplayer.TPlayer.HeldItem.Name} ({targetplayer.TPlayer.HeldItem.stack})`" +
                                        $"\n**ActiveChest:** `{targetplayer.ActiveChest}`" +
                                        $"\n\n```";

                                    var buttons = new ComponentBuilder();

                                    #region { inventory type }
                                    switch (message.Data.CustomId.Split("_")[3])
                                    {
                                        case "Inventory":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "inventory").Inventory;
                                                break;
                                            }
                                        case "Equipment":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "equipment").Equipment;
                                                break;
                                            }
                                        case "PiggyBank":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "piggybank").PiggyBank;
                                                break;
                                            }
                                        case "Safe":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "safe").Safe;
                                                break;
                                            }
                                        case "DefForge":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "defenderforge").DefenderForge;
                                                break;
                                            }
                                        case "VoidVault":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "voidvault").VoidVault;
                                                break;
                                            }
                                        case "InventoryLogs":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3, disabled: true);
                                                int total = 0;
                                                foreach (string invlog in InventoryManager.InventoryLogs)
                                                {
                                                    if (invlog.Split("|")[0] == targetplayer.Account.Name)
                                                    {
                                                        total++;
                                                    }
                                                }
                                                int count = 0;
                                                foreach (string invlog in InventoryManager.InventoryLogs)
                                                {
                                                    if (invlog.Split("|")[0] == targetplayer.Account.Name)
                                                    {
                                                        count++;
                                                        string previtem = $"{Lang.GetItemNameValue(int.Parse(invlog.Split("|")[3].Split(",")[0]))} ({invlog.Split("|")[3].Split(",")[1]})";
                                                        string item = $"{Lang.GetItemNameValue(int.Parse(invlog.Split("|")[4].Split(",")[0]))} ({invlog.Split("|")[4].Split(",")[1]})";
                                                        if (count < (total - 20)) continue;
                                                        EmbedDescription += $"{invlog.Split("|")[1]}{invlog.Split("|")[2]}: {previtem} => {item}\n";
                                                    }
                                                }

                                                break;
                                            }
                                    }
                                    #endregion

                                    var embed = new EmbedBuilder()
                                        .WithTitle($"Player [ {targetplayer.Index} ] {targetplayer.Name}")
                                        .WithDescription(EmbedDescription+"```")
                                        .WithColor(EmbedColor);

                                    await message.DeferAsync(true);

                                    await message.ModifyOriginalResponseAsync(msg => {
                                        msg.Embed = embed.Build();
                                        msg.Components = buttons.Build();
                                    });

                                    return;
                                }
                            #endregion
                            case "ServerModView":
                                #region ( Type: ServerModView )
                                {
                                    string stringplayers = "";

                                    foreach (TSPlayer ply in TShock.Players)
                                    {
                                        if (ply != null && ply.Active)
                                        {
                                            string stplayer = $"- {ply.Name} ";
                                            try
                                            {
                                                KLPAccount stplayerklpaccount = new(ply.Name);
                                                if (stplayerklpaccount.Discord != null)
                                                {
                                                    stplayer += "[ <@!" + stplayerklpaccount.Discord + "> ]";
                                                }

                                            }
                                            catch (NullReferenceException) { }
                                            stringplayers += stplayer + "\n";
                                        }
                                    }

                                    if (stringplayers == "") stringplayers = "No Players Online...";


                                    var embed = new EmbedBuilder()
                                            .WithTitle("Server Moderation Menu")
                                            .WithDescription($"**IP:** `N4.MCST.IO` **Port:** `26266`")
                                            .WithColor(EmbedColor)
                                            .WithFields(
                                                new EmbedFieldBuilder()
                                                    .WithName($"Online Players [{Main.player.Where(x => x.name.Length != 0).Count()}/{Main.maxNetPlayers}]")
                                                    .WithValue(stringplayers)
                                                )
                                            .Build();


                                    if (Main.player.Where(x => x.name.Length != 0).Count() != 0)
                                    {

                                        var menuBuilder = new SelectMenuBuilder()
                                        .WithPlaceholder("Select a Player")
                                        .WithCustomId("MKLP_SendMsg_PlayerModView_Main")
                                        .WithMinValues(1)
                                        .WithMaxValues(1);

                                        foreach (TSPlayer player in TShock.Players)
                                        {
                                            if (player == null) continue;
                                            if (player.Name == "" ||
                                                player.Name.Replace("*", "") == "" ||
                                                player.Name == " ") continue;
                                            if (!player.IsLoggedIn) continue;
                                            menuBuilder.AddOption(player.Name, player.Account.Name, $"Account: {player.Account.Name}");
                                        }

                                        var component = new ComponentBuilder()
                                            .WithButton("Refresh", "MKLP_EditMsg_ServerModView", ButtonStyle.Secondary, row: 0)
                                            .WithSelectMenu(menuBuilder, row: 1);

                                        await message.DeferAsync(true);

                                        await message.ModifyOriginalResponseAsync(msg => {
                                            msg.Embed = embed;
                                            msg.Components = component.Build();
                                        });

                                    }
                                    else
                                    {
                                        var component = new ComponentBuilder()
                                            .WithButton("Refresh", "MKLP_EditMsg_ServerModView", ButtonStyle.Secondary, row: 0);

                                        await message.DeferAsync(true);

                                        await message.ModifyOriginalResponseAsync(msg => {
                                            msg.Embed = embed;
                                            msg.Components = component.Build();
                                        });
                                    }
                                    return;
                                }
                                #endregion
                        }

                        return;
                    }
                #endregion
                case "InGame":
                    #region ( Type | InGame )
                    {

                        switch (message.Data.CustomId.Split("_")[2])
                        {
                            case "PlayerAction":
                                #region ( Type => PlayerAction )
                                {

                                    switch (message.Data.CustomId.Split("_")[3])
                                    {
                                        case "Ban":
                                            #region ( Type: Ban )
                                            {

                                                var modal = new ModalBuilder()
                                                    .WithTitle($"Banning [ {message.Data.CustomId.Split("_")[4]} ]")
                                                    .WithCustomId("MKLP_InGame_PlayerAction_Ban_"+ message.Data.CustomId.Split("_")[4])
                                                    .AddTextInput("Reason", "Ban_reason", TextInputStyle.Paragraph, "Cheating", value: "No Reason Specified")
                                                    .AddTextInput("Duration", "Ban_duration", TextInputStyle.Short, "0d 0h 0m 0s", maxLength: 15, value: "permanent");

                                                await message.RespondWithModalAsync(modal.Build());

                                                return;
                                            }
                                            #endregion
                                        case "QBan":
                                            #region ( Type: QBan )
                                            {
                                                TSPlayer? targetplayer = null;
                                                foreach (TSPlayer player in TShock.Players)
                                                {
                                                    if (player == null || !player.Active) continue;

                                                    if (player.Account.Name == message.Data.CustomId.Split("_")[4])
                                                    {
                                                        targetplayer = player;
                                                    }
                                                }

                                                if (targetplayer != null)
                                                {
                                                    targetplayer.Disconnect("You were Banned By " + executer.Name +
                                                        "\nReason: " + message.Data.CustomId.Split("_")[5]);
                                                }

                                                UserAccount account = TShock.UserAccounts.GetUserAccountByName(message.Data.CustomId.Split("_")[4]);

                                                if (ManagePlayer.OfflineBan(account, message.Data.CustomId.Split("_")[5], executer.Name, DateTime.MaxValue, true, true))
                                                {
                                                    await message.RespondAsync($"Successfully banned **{account.Name}**", ephemeral: true);
                                                } else
                                                {
                                                    await message.RespondAsync($"Player **{account.Name}** was already banned", ephemeral: true);
                                                }

                                                return;
                                            }
                                        #endregion
                                        case "Disable":
                                            #region ( Type: Disable )
                                            {
                                                var modal = new ModalBuilder()
                                                    .WithTitle($"Disable [ {message.Data.CustomId.Split("_")[4]} ]")
                                                    .WithCustomId("MKLP_InGame_PlayerAction_Disable_" + message.Data.CustomId.Split("_")[4])
                                                    .AddTextInput("Reason", "Disable_reason", TextInputStyle.Paragraph, "Cheating", value: "No Reason Specified");

                                                await message.RespondWithModalAsync(modal.Build());
                                                return;
                                            }
                                            #endregion
                                        case "Undisable":
                                            #region ( Type: Undisable )
                                            {

                                                TSPlayer? targetplayer = null;
                                                foreach (TSPlayer player in TShock.Players)
                                                {
                                                    if (player == null || !player.Active) continue;

                                                    if (player.Account.Name == message.Data.CustomId.Split("_")[4])
                                                    {
                                                        targetplayer = player;
                                                    }
                                                }

                                                if (targetplayer == null)
                                                {
                                                    await message.RespondAsync($"Player **{message.Data.CustomId.Split("_")[4]}** is offline", ephemeral: true);
                                                    return;
                                                }

                                                if (ManagePlayer.UnDisablePlayer(targetplayer, executer.Name))
                                                {
                                                    await message.RespondAsync($"Successfully Unisable **{message.Data.CustomId.Split("_")[4]}**", ephemeral: true);
                                                } else
                                                {
                                                    await message.RespondAsync($"Player **{message.Data.CustomId.Split("_")[4]}** isn't disabled", ephemeral: true);
                                                }

                                                return;
                                            }
                                            #endregion
                                    }

                                    return;
                                }
                                #endregion
                        }

                        return;
                    }
                    #endregion
            }

            #endregion
        }

        private async Task ModalHandler(SocketModal modal)
        {
            #region code

            if (modal.Data.CustomId.Split("_")[0] != "MKLP") return;

            UserAccount? executer = null;
            
            try
            {
                executer = TShock.UserAccounts.GetUserAccountByName(ManagePlayer.GetPlayerNameFromUserID(modal.User.Id));
            } catch (NullReferenceException) { }

            if (executer == null)
            {
                await modal.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                return;
            }

            var getgroup = TShock.Groups.GetGroupByName(executer.Group);

            if (getgroup == null)
            {
                await modal.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                return;
            }

            if (!getgroup.HasPermission(TSStaffPermission))
            {
                await modal.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                return;
            }

            List<SocketMessageComponentData> components = modal.Data.Components.ToList();

            switch (modal.Data.CustomId.Split("_")[1])
            {
                case "InGame":
                    #region ( Type | InGame )
                    {
                        
                        switch (modal.Data.CustomId.Split("_")[2])
                        {
                            case "PlayerAction":
                                #region ( Type => PlayerAction )
                                {

                                    switch (modal.Data.CustomId.Split("_")[3])
                                    {
                                        case "Ban":
                                            #region ( Type: Ban )
                                            {

                                                string reason = components
                                                    .First(x => x.CustomId == "Ban_reason").Value;
                                                string duration = components
                                                    .First(x => x.CustomId == "Ban_duration").Value;

                                                DateTime expiration = DateTime.MaxValue;

                                                TSPlayer? targetplayer = null;
                                                foreach (TSPlayer player in TShock.Players)
                                                {
                                                    if (player == null || !player.Active) continue;

                                                    if (player.Account.Name == modal.Data.CustomId.Split("_")[4])
                                                    {
                                                        targetplayer = player;
                                                    }
                                                }

                                                if (TShock.Utils.TryParseTime(duration, out ulong seconds))
                                                {
                                                    expiration = DateTime.UtcNow.AddSeconds(seconds);
                                                }

                                                if (targetplayer != null)
                                                {
                                                    targetplayer.Disconnect("You were Banned By " + executer.Name +
                                                        "\nReason: " + reason);
                                                }

                                                UserAccount account = TShock.UserAccounts.GetUserAccountByName(modal.Data.CustomId.Split("_")[4]);

                                                if (ManagePlayer.OfflineBan(account, reason, executer.Name, expiration, true, true))
                                                {
                                                    await modal.RespondAsync($"Successfully banned **{account.Name}**" +
                                                        "\n**Reason:** " + reason, ephemeral: true);
                                                } else
                                                {
                                                    await modal.RespondAsync($"Player **{account.Name}** was already banned", ephemeral: true);
                                                }

                                                

                                                return;
                                            }
                                        #endregion
                                        case "Disable":
                                            #region ( Type: Disable )
                                            {

                                                string reason = components
                                                    .First(x => x.CustomId == "Disable_reason").Value;

                                                TSPlayer? targetplayer = null;
                                                foreach (TSPlayer player in TShock.Players)
                                                {
                                                    if (player == null || !player.Active) continue;

                                                    if (player.Account.Name == modal.Data.CustomId.Split("_")[4])
                                                    {
                                                        targetplayer = player;
                                                    }
                                                }

                                                if (targetplayer == null)
                                                {
                                                    await modal.RespondAsync($"Player **{modal.Data.CustomId.Split("_")[4]}** is offline", ephemeral: true);
                                                    return;
                                                }

                                                if (ManagePlayer.DisablePlayer(targetplayer, reason, executer.Name))
                                                {
                                                    await modal.RespondAsync($"Successfully disabled **{targetplayer.Name}**", ephemeral: true);
                                                } else
                                                {
                                                    await modal.RespondAsync($"Player **{targetplayer.Name}** was already disabled", ephemeral: true);
                                                }

                                                return;
                                            }
                                            #endregion
                                    }

                                    return;
                                }
                                #endregion
                        }

                        return;
                    }
                    #endregion
            }

            #endregion
        }

        private async Task SelectMenuHandler(SocketMessageComponent message)
        {
            #region code

            if (message.Data.CustomId.Split("_")[0] != "MKLP") return;

            UserAccount? executer = null;

            try
            {
                executer = TShock.UserAccounts.GetUserAccountByName(ManagePlayer.GetPlayerNameFromUserID(message.User.Id));
            }
            catch (NullReferenceException) { }

            if (executer == null)
            {
                await message.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                return;
            }

            var getgroup = TShock.Groups.GetGroupByName(executer.Group);

            if (getgroup == null)
            {
                await message.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                return;
            }

            if (!getgroup.HasPermission(TSStaffPermission))
            {
                await message.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                return;
            }

            var Value = string.Join(", ", message.Data.Values);

            switch (message.Data.CustomId.Split("_")[1])
            {
                case "SendMsg":
                    #region ( Type | SendMessage )
                    {

                        switch (message.Data.CustomId.Split("_")[2])
                        {
                            case "PlayerModView":
                                #region ( Type: PlayerModView )
                                {
                                    TSPlayer? targetplayer = null;
                                    foreach (TSPlayer player in TShock.Players)
                                    {
                                        if (player == null || !player.Active) continue;

                                        if (player.Account.Name == Value)
                                        {
                                            targetplayer = player;
                                        }
                                    }

                                    if (targetplayer == null)
                                    {
                                        await message.RespondAsync($"Player {Value} isn't online", ephemeral: true);
                                        return;
                                    }

                                    switch (message.Data.CustomId.Split("_")[3])
                                    {
                                        case "Main":
                                            {
                                                var buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerModView_Main_" + targetplayer.Account.Name, ButtonStyle.Secondary)
                                                    .WithButton("Main", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("View Inventory", "MKLP_SendMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary, row: 1)
                                                    .WithButton("Ban Player", "MKLP_InGame_PlayerAction_Ban_" + targetplayer.Account.Name, ButtonStyle.Danger, row: 2)
                                                    .WithButton("Disable Player", "MKLP_InGame_PlayerAction_Disable_" + targetplayer.Account.Name, ButtonStyle.Danger, row: 2)
                                                    .WithButton("Mute Player", "MKLP_InGame_PlayerAction_Mute_" + targetplayer.Account.Name, ButtonStyle.Danger, row: 2);


                                                var embed = new EmbedBuilder()
                                                    .WithTitle($"Player [ {targetplayer.Index} ] {targetplayer.Name}")
                                                    .WithDescription($"**Health:** `{targetplayer.PlayerData.maxHealth}❤️`" +
                                                    $"\n**Mana:** `{targetplayer.PlayerData.maxMana}⭐`" +
                                                    $"\n" +
                                                    $"\n**Coords:** `{targetplayer.TileX} {targetplayer.TileY}` `x y`" +
                                                    $"\n" +
                                                    $"\n**Account Info**" +
                                                    $"\n> **Account Name:** `{targetplayer.Account.Name}`" +
                                                    $"\n> **Account ID:** `{targetplayer.Account.ID}`" +
                                                    $"\n> **Registered:** `{targetplayer.Account.Registered}`" +
                                                    $"\n" +
                                                    $"\n`LoggedIn: {(targetplayer.IsLoggedIn ? "✅" : "❌")}` " +
                                                    $"`Disabled: {((MKLP.DisabledKey.Contains(targetplayer.IP) || MKLP.DisabledKey.Contains(targetplayer.UUID)) ? "✅" : "❌")}` " +
                                                    $"`Muted: {(targetplayer.mute ? "✅" : "❌")}`")
                                                    .WithColor(EmbedColor);

                                                await message.RespondAsync(embed: embed.Build(), ephemeral: true, components: buttons.Build());


                                                break;
                                            }
                                    }



                                    return;
                                }
                                #endregion
                            case "PlayerViewInventory":
                                #region ( Type: PlayerViewInventory )
                                {
                                    TSPlayer? targetplayer = null;
                                    foreach (TSPlayer player in TShock.Players)
                                    {
                                        if (player == null || !player.Active) continue;

                                        if (player.Account.Name == Value)
                                        {
                                            targetplayer = player;
                                        }
                                    }

                                    if (targetplayer == null)
                                    {
                                        await message.RespondAsync($"Player {Value} isn't online", ephemeral: true);
                                        return;
                                    }

                                    string EmbedDescription = $"**Held Item:** `{targetplayer.TPlayer.HeldItem.Name} ({targetplayer.TPlayer.HeldItem.stack})`" +
                                        $"\n\n```";

                                    var buttons = new ComponentBuilder();

                                    #region { inventory type }
                                    switch (message.Data.CustomId.Split("_")[3])
                                    {
                                        case "Inventory":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "inventory").Inventory;
                                                break;
                                            }
                                        case "Equipment":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "XXX" + targetplayer.Account.Name, ButtonStyle.Primary, disabled: true)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "equipment").Equipment;
                                                break;
                                            }
                                        case "PiggyBank":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "piggybank").PiggyBank;
                                                break;
                                            }
                                        case "Safe":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "safe").Safe;
                                                break;
                                            }
                                        case "DefForge":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "defenderforge").DefenderForge;
                                                break;
                                            }
                                        case "VoidVault":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2, disabled: true)
                                                    .WithButton("Inventory Logs", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3);
                                                EmbedDescription += new InvView_InventoryString(false, targetplayer, "voidvault").VoidVault;
                                                break;
                                            }
                                        case "InventoryLogs":
                                            {
                                                buttons = new ComponentBuilder()
                                                    .WithButton("Refresh", "MKLP_EditMsg_PlayerViewInventory_InventoryLogs_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Inventory", "MKLP_EditMsg_PlayerViewInventory_Inventory_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Equipment", "MKLP_EditMsg_PlayerViewInventory_Equipment_" + targetplayer.Account.Name, ButtonStyle.Primary)
                                                    .WithButton("Piggy Bank", "MKLP_EditMsg_PlayerViewInventory_PiggyBank_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Safe", "MKLP_EditMsg_PlayerViewInventory_Safe_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Defender Forge", "MKLP_EditMsg_PlayerViewInventory_DefForge_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Void Vault", "MKLP_EditMsg_PlayerViewInventory_VoidVault_" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 2)
                                                    .WithButton("Inventory Logs", "XXX" + targetplayer.Account.Name, ButtonStyle.Secondary, row: 3, disabled: true);
                                                break;
                                            }
                                    }
                                    #endregion

                                    var embed = new EmbedBuilder()
                                        .WithTitle($"Player [ {targetplayer.Index} ] {targetplayer.Name}")
                                        .WithDescription(EmbedDescription + "```")
                                        .WithColor(EmbedColor);

                                    await message.RespondAsync(embed: embed.Build(), ephemeral: true, components: buttons.Build());

                                    return;
                                }
                                #endregion
                        }

                        return;
                    }
                    #endregion
            }

            #endregion
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            #region code

            switch (command.Data.Name)
            {
                case "moderation":
                    #region ( Command | moderation )
                    {
                        UserAccount? executer = null;

                        try
                        {
                            executer = TShock.UserAccounts.GetUserAccountByName(ManagePlayer.GetPlayerNameFromUserID(command.User.Id));
                        }
                        catch (NullReferenceException) { }

                        if (executer == null)
                        {
                            await command.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                            return;
                        }

                        var getgroup = TShock.Groups.GetGroupByName(executer.Group);

                        if (getgroup == null)
                        {
                            await command.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                            return;
                        }

                        if (!getgroup.HasPermission(TSStaffPermission))
                        {
                            await command.RespondAsync("You do not have permission to proceed this interaction!", ephemeral: true);
                            return;
                        }

                        string stringplayers = "";

                        foreach (TSPlayer ply in TShock.Players)
                        {
                            if (ply != null && ply.Active)
                            {
                                string stplayer = $"- {ply.Name} ";
                                try
                                {
                                    KLPAccount stplayerklpaccount = new(ply.Name);
                                    if (stplayerklpaccount.Discord != null)
                                    {
                                        stplayer += "[ <@!" + stplayerklpaccount.Discord + "> ]";
                                    }

                                }
                                catch (NullReferenceException) { }
                                stringplayers += stplayer + "\n";
                            }
                        }

                        if (stringplayers == "") stringplayers = "No Players Online...";


                        var embed = new EmbedBuilder()
                                .WithTitle("Server Moderation Menu")
                                .WithDescription($"**IP:** `N4.MCST.IO` **Port:** `26266`")
                                .WithColor(EmbedColor)
                                .WithFields(
                                    new EmbedFieldBuilder()
                                        .WithName($"Online Players [{Main.player.Where(x => x.name.Length != 0).Count()}/{Main.maxNetPlayers}]")
                                        .WithValue(stringplayers)
                                    )
                                .Build();


                        if (Main.player.Where(x => x.name.Length != 0).Count() != 0)
                        {

                            var menuBuilder = new SelectMenuBuilder()
                            .WithPlaceholder("Select a Player")
                            .WithCustomId("MKLP_SendMsg_PlayerModView_Main")
                            .WithMinValues(1)
                            .WithMaxValues(1);

                            foreach (TSPlayer player in TShock.Players)
                            {
                                if (player == null) continue;
                                if (player.Name == "" ||
                                    player.Name.Replace("*", "") == "" ||
                                    player.Name == " ") continue;
                                if (!player.IsLoggedIn) continue;
                                menuBuilder.AddOption(player.Name, player.Account.Name, $"Account: {player.Account.Name}");
                            }

                            var component = new ComponentBuilder()
                                .WithButton("Refresh", "MKLP_EditMsg_ServerModView", ButtonStyle.Secondary, row: 0)
                                .WithSelectMenu(menuBuilder, row: 1);

                            await command.RespondAsync(embed: embed, components: component.Build(), ephemeral: true);

                        }
                        else
                        {
                            var component = new ComponentBuilder()
                                .WithButton("Refresh", "MKLP_EditMsg_ServerModView", ButtonStyle.Secondary, row: 0);
                            await command.RespondAsync(embed: embed, components: component.Build(), ephemeral: true);
                        }
                        return;
                    }
                    #endregion
                case "moderation-user":
                    #region ( Command | moderation-user )
                    {
                        UnavailableCommand();
                        return;
                    }
                    #endregion
            }

            async void UnavailableCommand()
            {
                await command.RespondAsync("This Command Is Unavailable!", ephemeral: true);
            }

            #endregion
        }

        private Task MessageRecieved(SocketMessage message)
        {
            #region chat relay
            if (message is IUserMessage userMessage && !userMessage.Author.IsBot)
            {
                string messagecontent = message.Content;
                if (userMessage.Channel.Id == 0)
                {

                }
            }
            return Task.CompletedTask;
            #endregion
        }

        #region [ Actions ]

        public async void KLPBotSendMessage(ulong channel, string message)
        {

            var targetchannel = _client.GetChannel(channel);

            await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + message);
            return;

        }


        string TitleLog = "<:Announcement_Box:1269150552860659724> **[ MKLP ] :** ";

        public async void KLPBotSendMessageLog(ulong channel, string message)
        {
            if (channel == 0) return;

            var targetchannel = _client.GetChannel(channel);

            await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + message);
            return;

            
        }

        public async void KLPBotSendMessage_Disabled(string message, string playername = "none", string reason = "No Reason Provided")
        {
            if (MKLP.Config.Discord.MainChannelLog == 0) return;

            var targetchannel = _client.GetChannel(MKLP.Config.Discord.MainChannelLog);

            var buttons = new ComponentBuilder()
                .WithButton("Dismiss", "MKLP_DismissMsg_Disabled", ButtonStyle.Secondary)
                .WithButton("Check Player", "MKLP_SendMsg_PlayerModView_Main_" + playername, emote: new Emoji("\U0001F4B3"))
                .WithButton("Quick Ban [ permanent ]", $"MKLP_InGame_PlayerAction_QBan_{playername}_{reason}", ButtonStyle.Danger, emote: new Emoji("\U0001F528"), row: 1)
                .WithButton("Enable", "MKLP_InGame_PlayerAction_Undisable_" + playername, ButtonStyle.Success, emote: new Emoji("\U00002705"), row: 1);

            await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + message, components: buttons.Build());
            return;
        }

        public async void KLPBotSendMessage_Warning(string message, string playername = "none", string reason = "No Reason Provided")
        {
            if (MKLP.Config.Discord.MainChannelLog == 0) return;

            var targetchannel = _client.GetChannel(MKLP.Config.Discord.MainChannelLog);

            var buttons = new ComponentBuilder()
                .WithButton("Dismiss", "MKLP_DismissMsg_Warning", ButtonStyle.Secondary)
                .WithButton("Check Player", "MKLP_SendMsg_PlayerModView_Main_" + playername, emote: new Emoji("\U0001F4B3"))
                .WithButton("Quick Ban [ permanent ]", $"MKLP_InGame_PlayerAction_QBan_{playername}_{reason}", ButtonStyle.Danger, emote: new Emoji("\U0001F528"), row: 1);

            await ((SocketTextChannel)targetchannel).SendMessageAsync(TitleLog + message, components: buttons.Build());
            return;
        }

        #endregion


        #endregion

    }

}
