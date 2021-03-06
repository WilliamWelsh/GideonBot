﻿using Discord;
using System.Text;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Handlers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DiscordBot.Minigames
{
    class RussianRoulette
    {
        private int round, PlayerSlots, currentTurn;
        private bool isGameGoing;

        private SocketUser host;
        private readonly List<SocketUser> Players = new List<SocketUser>();

        private int RandomChamber() => Utilities.GetRandomNumber(0, 6);

        private Embed Embed(string description, string footer, bool showPlayers)
        {
            var embed = new EmbedBuilder()
                .WithTitle($":gun: Russian Roulette")
                .WithColor(Utilities.ClearColor)
                .WithDescription(description)
                .WithFooter(footer);
            if (showPlayers)
            {
                StringBuilder PlayerDesc = new StringBuilder();
                for (int i = 0; i < Players.Count; i++)
                    PlayerDesc.AppendLine($"Player {i + 1}: {Players.ElementAt(i).Mention}");
                embed.AddField("Players", PlayerDesc);
            }
            return embed.Build();
        }

        private Embed gameEmbed(string description, string footer) => Utilities.Embed($":gun: Russian Roulette - Round {round}", description, Utilities.ClearColor, footer, "");

        public async Task TryToStartGame(SocketCommandContext context, string input)
        {
            if (isGameGoing)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"Sorry, {host.Mention} is currently hosting a game.", "", false));
                return;
            }
            if (input == "!rr")
            {
                await context.Channel.SendMessageAsync("", false, Embed("Please enter an amount of players.\n\n`!rr #` - # = Players\n\nExample: \n`!rr 5` will start a game with 5 players.", "", false));
                return;
            }
            input = input.Replace("!rr ", "");
            int.TryParse(input, out PlayerSlots);
            if (PlayerSlots > 6)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"Sorry, 6 is the max amount of players for Russian Roulette.", "", false));
                return;
            }
            else if (PlayerSlots < 2)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"Sorry, 2 is the minimum amount of players for Russian Roulette.", "", false));
                return;
            }
            await StartGame(context).ConfigureAwait(false);
        }

        public async Task StartGame(SocketCommandContext context)
        {
            host = (SocketGuildUser)context.User;
            await context.Channel.SendMessageAsync("", false, Embed($"{host.Mention} has started a game of Russian Roulette with {PlayerSlots} players!\n\nType `!join rr` to play!", "", false));
            Players.Add(host);
            isGameGoing = true;
        }

        public async Task TryToJoin(SocketCommandContext context)
        {
            if (!isGameGoing || Players.Count == PlayerSlots || Players.Contains(context.User)) return;

            SocketUser newPlayer = context.User;
            Players.Add(newPlayer);
            if (Players.Count != PlayerSlots)
                await context.Channel.SendMessageAsync("", false, Embed($"{PlayerSlots - Players.Count} more player(s) needed!\n\nType `!join rr` to play!", "", true));
            else
                await context.Channel.SendMessageAsync("", false, gameEmbed($"Initial round.\n\nWaiting for {Players.ElementAt(0).Mention} to pull the trigger. (`!pt`)", ""));
        }

        public async Task PullTrigger(SocketCommandContext context)
        {
            SocketGuildUser player = (SocketGuildUser)context.User;
            if (Players.ElementAt(currentTurn) != player) return;
            await DoRound(player, context).ConfigureAwait(false);
        }

        private async Task DoRound(SocketGuildUser player, SocketCommandContext context)
        {
            round++;
            int badChamber = RandomChamber();
            int currentChamber = RandomChamber();
            currentTurn = currentTurn == (Players.Count-1) ? currentTurn = 0 : currentTurn + 1;

            if (currentChamber == badChamber)
            {
                await DieAndCheckForWin(player, context).ConfigureAwait(false);
                await context.Channel.SendMessageAsync("", false, gameEmbed($"The cylinder spins...\n\n*BANG*\n\n{player.Mention} died and lost 3 Coins!\n\nWaiting for {Players.ElementAt(currentTurn).Mention} to pull the trigger. (`!pt`)", ""));
                CoinsHandler.AdjustCoins(player, -3);
            }
            else
                await context.Channel.SendMessageAsync("", false, gameEmbed($"The cylinder spins...\n\n*click*\n\n{player.Mention} survived!\n\nWaiting for {Players.ElementAt(currentTurn).Mention} to pull the trigger. (`!pt`)", ""));
        }

        private async Task DieAndCheckForWin(SocketGuildUser player, SocketCommandContext context)
        {
            Players.Remove(player);
            if(Players.Count == 1)
            {
                CoinsHandler.AdjustCoins(Players.ElementAt(0), 3 + (2 * PlayerSlots));
                CoinsHandler.AdjustCoins(player, -3);

                await context.Channel.SendMessageAsync("", false, gameEmbed($"The cylinder spins...\n\n*BANG*\n\n{player.Mention} died and lost 3 Coins!\n\n{Players.ElementAt(0).Mention} won the game and got {3 + (2*PlayerSlots)} coins!", ""));
                Reset();
            }
        }

        public void Reset()
        {
            round = 0;
            host = null;
            currentTurn = 0;
            isGameGoing = false;
            Players.Clear();
        }
    }
}