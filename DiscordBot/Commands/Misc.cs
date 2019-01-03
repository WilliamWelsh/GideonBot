﻿using System;
using Discord;
using System.Text;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gideon.Handlers
{
    [RequireContext(ContextType.Guild)]
	public class Misc : ModuleBase<SocketCommandContext>
	{
        [Command("rolecolors")]
		public async Task DisplayRoleColors()
		{
            StringBuilder text = new StringBuilder();
			foreach (var x in Context.Guild.Roles)
				text.AppendLine($"{x.Name}, {x.Color}");
			await Context.Channel.SendMessageAsync(text.ToString());
		}

		[Command("dicksize")]
		public async Task DickSize() => await Context.Channel.SendMessageAsync($"8{new string('=', new Random().Next(1, 13))}D");

        // Used to turn text upside down
        [Command("australia")]
        [Alias("aussie")]
        public async Task AussieText([Remainder]string message)
        {
            char[] X = @"¿/˙'\‾¡zʎxʍʌnʇsɹbdouɯlʞɾıɥƃɟǝpɔqɐ".ToCharArray();
            string V = @"?\.,/_!zyxwvutsrqponmlkjihgfedcba";
            string upsideDownText = new string((from char obj in message.ToCharArray()
                               select (V.IndexOf(obj) != -1) ? X[V.IndexOf(obj)] : obj).Reverse().ToArray()); // thanks stack overflow
            await Utilities.SendEmbed(Context.Channel, "Australian Translator", upsideDownText, new Color(1, 33, 105), "", "");
        }

		// Make Gideon say something
		[Command("say")]
		public async Task Say([Remainder]string message)
		{
            if (!await Utilities.CheckForSuperadmin(Context, Context.User)) return;
            await Context.Channel.DeleteMessageAsync(Context.Message.Id);
			await Context.Channel.SendMessageAsync(message);
		}

		// Make Gideon DM someone something
		[Command("dm")]
		public async Task DM(SocketGuildUser target, [Remainder]string message)
		{
            if (!await Utilities.CheckForSuperadmin(Context, Context.User)) return;
            await Context.Channel.DeleteMessageAsync(Context.Message.Id);
			await target.SendMessageAsync(message);
		}

		// Spongebob Mock Meme
		[Command("mock")]
		public async Task Mock([Remainder]string message)
		{
			char[] letters = message.ToCharArray();
			for (int n = 0; n < letters.Length; n += 2)
				letters[n] = char.ToUpper(letters[n]);
			string name = ((SocketGuildUser)Context.User).Nickname ?? Context.User.Username;
			await Utilities.SendEmbed(Context.Channel, "", new string(letters), Colors.Yellow, $"Mocked by {name}", "http://i0.kym-cdn.com/photos/images/masonry/001/255/479/85b.png");
		}

		// Display a random person on the server
		[Command("someone")]
		public async Task GetRandomPerson()
		{
			SocketGuildUser[] s = Context.Guild.Users.ToArray();
			SocketGuildUser randomUser = s[Utilities.GetRandomNumber(0, s.Length)];
			string footer = $"{((1.0m / Context.Guild.MemberCount) * 100).ToString("N3")}% chance of selecting this user.";
			await Utilities.SendEmbed(Context.Channel, "Random User", randomUser.ToString(), Utilities.DomColorFromURL(randomUser.GetAvatarUrl()), footer, randomUser.GetAvatarUrl());
		}

		// See how many tries it would take to randomly get a specific user
		[Command("someone")]
		public async Task GetRandomPerson(SocketGuildUser user)
		{
			SocketGuildUser[] s = Context.Guild.Users.ToArray();
			uint count = 0;
			bool hasFound = false;
			do
			{
				count++;
				SocketGuildUser randomUser = s[Utilities.GetRandomNumber(0, s.Length)];
				if (randomUser == user) hasFound = true;
			} while (!hasFound);

			string footer = $"{((1.0m / Context.Guild.MemberCount) * 100).ToString("N3")}% chance of selecting this user.";
			await Utilities.SendEmbed(Context.Channel, "Random User", $"It took {count} tries to find {user.Mention}.", Utilities.DomColorFromURL(user.GetAvatarUrl()), footer, user.GetAvatarUrl());
		}

		[Command("onlinecount")]
		public async Task CountUsersOnline() => await Context.Channel.SendMessageAsync($"There are currently {Context.Guild.Users.ToArray().Length} members online.");

        [Command("joined")]
        public async Task JoinedAt(SocketGuildUser user = null) => await Context.Channel.SendMessageAsync(StatsHandler.GetJoinedDate((user ?? (SocketGuildUser)Context.User)));

        [Command("created")]
		public async Task Created(SocketGuildUser user = null) => await Context.Channel.SendMessageAsync(StatsHandler.GetCreatedDate(user ?? Context.User));

        [Command("avatar")]
        public async Task GetAvatar(SocketGuildUser user = null) => await Context.Channel.SendMessageAsync("", false, Utilities.ImageEmbed("", "", Utilities.DomColorFromURL((user ?? Context.User).GetAvatarUrl()), "", (user ?? Context.User).GetAvatarUrl().Replace("?size=128", "?size=512")));

        // Disabled !time because I use my new TimeBot so other servers can use it
        // TimeBot: https://github.com/WilliamWelsh/TimeBot
        // View local time for a user (if it's set up for them)
        //[Command("time")]
        //public async Task ViewTime(SocketGuildUser user = null) => await StatsHandler.DisplayTime(Context, user ?? (SocketGuildUser)Context.User);

        // View a User's country (if it's set up for them)
        [Command("country")]
		public async Task ViewCountry(SocketGuildUser user = null) => await StatsHandler.DisplayCountry(Context, user ?? (SocketGuildUser)Context.User);

		// See stats for a certain user
		[Command("stats")]
		public async Task DisplayUserStats(SocketGuildUser user = null) => await StatsHandler.DisplayUserStats(Context, user ?? (SocketGuildUser)Context.User);

        // View custom server emotes
        // broken
        [Command("emotes")]
        [Alias("emojis")]
        public async Task ViewServerEmotes() => await Utilities.SendEmbed(Context.Channel, $"Server Emotes ({Context.Guild.Emotes.Count})", string.Join("", Context.Guild.Emotes), Colors.LightBlue, "", "");

        // View server stats
        [Command("serverstats")]
		public async Task ServerStats() => await StatsHandler.DisplayServerStats(Context);

		// Set the time for a user
		[Command("time set")]
		public async Task UserTimeSet(SocketGuildUser target, int offset)
		{
            if (!await Utilities.CheckForSuperadmin(Context, Context.User)) return;
            UserAccounts.GetAccount(target).localTime = offset;
			UserAccounts.SaveAccounts();
			await Context.Channel.SendMessageAsync("User updated.");
		}

		// Set the nationality for a user
		[Command("country set")]
		public async Task UserSetCountry(SocketGuildUser target, [Remainder]string name)
		{
            if (!await Utilities.CheckForSuperadmin(Context, Context.User)) return;
            UserAccounts.GetAccount(target).country = name;
			UserAccounts.SaveAccounts();
			await Context.Channel.SendMessageAsync("User updated.");
		}

		// Delete a certain amount of messages
		[Command("delete")]
		public async Task DeleteMessage([Remainder]string amount)
		{
            if (!await Utilities.CheckForSuperadmin(Context, Context.User)) return;
            //await Context.Channel.DeleteMessageAsync(await Context.Channel.GetMessagesAsync(int.Parse(amount) + 1).Fl);
		}

		// Nickname a user
		[Command("nick")]
		public async Task NicknameUser(SocketGuildUser user, [Remainder]string input)
		{
            if (!await Utilities.CheckForSuperadmin(Context, Context.User)) return;
            await user.ModifyAsync(x => { x.Nickname = input; });
			await Context.Channel.SendMessageAsync("User updated.");
		}

		[Command("gideon")]
		public async Task GideonGreet() => await Context.Channel.SendMessageAsync($"Greetings. How may I be of service, {Context.User.Mention}?\n`!help`");

		// View Stats for a movie
		[Command("movie")]
		public async Task SearchMovie([Remainder]string search)
		{
			MediaFetchHandler.Movie media = MediaFetchHandler.FetchMovie(search);

			string RTScore = "N/A", IMDBScore;

			for (int i = 0; i < media.Ratings.Length; i++)
				if (media.Ratings[i].Source == "Rotten Tomatoes") RTScore = media.Ratings[i].Value;

			IMDBScore = media.imdbRating == "N/A" ? "N/A" : $"{media.imdbRating}/10";

			await Context.Channel.SendMessageAsync(null, false, new EmbedBuilder()
                .WithTitle($":film_frames: {media.Title} ({media.Year})")
                .WithThumbnailUrl(media.Poster)
                .WithDescription(media.Plot)
                .AddField("Director", media.Director)
                .AddField("Runtime", media.Runtime)
                .AddField("Box Office", media.BoxOffice)
                .AddField("IMDB Score", IMDBScore)
                .AddField("Rotten Tomatoes", RTScore)
                .WithColor(Utilities.DomColorFromURL(media.Poster))
                .Build());
		}

		// View stats for a TV show
		[Command("tv")]
		public async Task SearchShows([Remainder]string search)
		{
			MediaFetchHandler.Movie media = MediaFetchHandler.FetchMovie(search);

			string IMDBScore = media.imdbRating == "N/A" ? "N/A" : $"{media.imdbRating}/10";
			media.Year = media.Year.Replace("â€“", "-");

			await Context.Channel.SendMessageAsync(null, false, new EmbedBuilder()
                .WithTitle($":film_frames: {media.Title} ({media.Year})")
                .WithThumbnailUrl(media.Poster)
                .WithDescription(media.Plot)
                .AddField("Runtime", media.Runtime)
                .AddField("IMDB Score", IMDBScore)
                .WithColor(Utilities.DomColorFromURL(media.Poster))
                .Build());
		}

		// Print a link to Gideon's sourcecode
		[Command("source")]
        [Alias("sourcecode", "github", "code")]
		public async Task GetSourceCode1() => await Context.Channel.SendMessageAsync("https://github.com/WilliamWelsh/GideonBot");

		// Display available commands
		[Command("help")]
		public async Task Help() => await Context.Channel.SendMessageAsync($"View available commands here:\nhttps://github.com/WilliamWelsh/GideonBot/blob/master/README.md");

        // Makes an emoji big, @Elias WE FINALLY DID IT
        [Command("url")]
        [Alias("jumbo")]
        public async Task PrintEmojis([Remainder]string input)
        {
            var emojis = input.Split('>');
            foreach (var s in emojis)
                await PrintEmoji(s + ">");
        }

        private async Task PrintEmoji(string emoji)
        {
            string url = $"https://cdn.discordapp.com/emojis/{Regex.Replace(emoji, "[^0-9.]", "")}.{(emoji.StartsWith("<a:") ? "gif" : "png")}";
            await Context.Channel.SendMessageAsync("", false, Utilities.ImageEmbed("", "", Utilities.DomColorFromURL(url), "", url));
        }

        // Convert a hexadecimal to an RGB value
        [Command("rgb")]
		public async Task HexToRGB(string input)
		{
			input = input.Replace("#", "");

			if (!new Regex("^[a-zA-Z0-9]*$").IsMatch(input) || input.Length != 6)
			{
                await Utilities.PrintError(Context.Channel, $"Please enter a valid hexadecimal, {Context.User.Mention}.");
				return;
			}

			var RGB = Utilities.HexToRGB(input);
			string result = $"`#{input}` = `{RGB.R}, {RGB.G}, {RGB.B}`\n\n" +
				$"`Red: {RGB.R}`\n" +
				$"`Green: {RGB.G}`\n" +
				$"`Blue: {RGB.B}`\n";
			await Utilities.SendEmbed(Context.Channel, "Hexadecimal to RGB", result, RGB, "", "");
		}

		// Convert an RGB value to a hexadecimal
		[Command("hex")]
		public async Task RGBToHex(int R, int G, int B)
		{
			if (R < 0 || R > 255)
			{
                await Utilities.PrintError(Context.Channel, $"Please enter a valid red value, {Context.User.Mention}.");
                return;
			}

			else if (G < 0 || G > 255)
			{
                await Utilities.PrintError(Context.Channel, $"Please enter a valid green value, {Context.User.Mention}.");
                return;
			}

			else if (B < 0 || B > 255)
			{
                await Utilities.PrintError(Context.Channel, $"Please enter a valid blue value, {Context.User.Mention}.");
                return;
			}
			await Utilities.SendEmbed(Context.Channel, "RGB to Hexadecimal", $"`{R}, {G}, {B}` = `#{R:X2}{G:X2}{B:X2}`", new Color(R, G, B), "", "");
		}

        // Send a random picture of Alani
        [Command("alani")]
        public async Task RandomAlaniPic()
        {
            string pic = Config.bot.alaniPics[Utilities.GetRandomNumber(0, Config.bot.alaniPics.Count)];
            await Context.Channel.SendMessageAsync("", false, Utilities.ImageEmbed("", "", Utilities.DomColorFromURL(pic), "", pic));
        }

        // Send a random picture of R
        [Command("r")]
        public async Task DisplayRandomR()
        {
            string pic = Config.bot.Rs[Utilities.GetRandomNumber(0, Config.bot.Rs.Count)];
            await Context.Channel.SendMessageAsync("", false, Utilities.ImageEmbed("", "", Utilities.DomColorFromURL(pic), "", pic));
        }

        // Give xp to a user
        [Command("xp add")]
        public async Task AddXP(SocketUser user, int xp)
        {
            if (!await Utilities.CheckForSuperadmin(Context, Context.User)) return;
            RankHandler.GiveUserXP(user, xp);
            await RankHandler.CheckXP(Context, user);
            await Context.Channel.SendMessageAsync("Updated user.");
        }

        [Command("xp")]
        [Alias("level", "rank")]
        public async Task ViewXP(SocketUser user = null) => await RankHandler.DisplayLevelAndXP(Context, user ?? Context.User);

        [Command("ranks")]
        [Alias("levels")]
        public async Task ViewRanks()
        {
            string ranks = "Level 0-5 Noob\n" +
                "Level 6-10 Symbiote\n" +
                "Level 11-15 Speedster\n" +
                "Level 16-20 Kaiju Slayer\n" +
                "Level 21-25 Avenger";
            await Utilities.SendEmbed(Context.Channel, "Ranks", ranks, Colors.LightBlue, "You get 15-25 xp for sending a message, but only once a minute.", "");
        }

        //[Command("playing")]
        //public async Task ViewRanks(SocketUser user) => await Context.Channel.SendMessageAsync($"{(user.Activity.Name == "" ? "Not currently playing anything." : user.Activity.Name.ToString())}");

        // Convert ASCII to Binary
        [Command("binary")]
        public async Task ASCIIToBinary([Remainder]string ascii)
        {
            StringBuilder binary = new StringBuilder();
            foreach (char c in ascii)
                binary.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            if (binary.Length > 2000)
            {
                await Utilities.SendEmbed(Context.Channel, "ASCII to Binary Converter", "The resulting binary is too long (over 2000 characters).\nDue to Discord's character count limitation, I am unable to send the message.", Colors.Green, "", "");
                return;
            }
            await Utilities.SendEmbed(Context.Channel, "ASCII to Binary Converter", binary.ToString(), Colors.Green, "", "");
        }

        // Convert Binary to ASCII
        [Command("ascii")]
        public async Task BinaryToASCII([Remainder]string input)
        {
            if (input.Length % 8 != 0)
            {
                await Utilities.SendEmbed(Context.Channel, "Binary to ASCII Converter", "Sorry, that cannot be converted to text.\nThe length of the binary must be a multiple of 8.", Colors.Green, "", "");
                return;
            }
            var list = new List<byte>();
            for (int i = 0; i < input.Length; i += 8)
            {
                string t = input.Substring(i, 8);
                list.Add(Convert.ToByte(t, 2));
            }

            await Utilities.SendEmbed(Context.Channel, "Binary to ASCII Converter", Encoding.ASCII.GetString(list.ToArray()), Colors.Green, "", "");
        }

        // Show everyone on the server who is a fan of Shawn Mendes
        [Command("mendesarmy")]
        public async Task ShowMendesArmy()
        {
            StringBuilder users = new StringBuilder();
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Shawn Mendes Fan");
            foreach (var user in Context.Guild.Users)
                if (user.Roles.Contains(role))
                    users.AppendLine(user.Mention);
            await Utilities.SendEmbed(Context.Channel, $"Mendes Army ({users.ToString().Split('\n').Length - 1})", users.ToString(), new Color(208, 185, 179), "Shawn Mendes Fans", "https://cdn.discordapp.com/avatars/519261973737635842/55e95c3bd26751828c96802292897a41.png?size=128");
        }

        // Display All Bans
        [Command("bans")]
        public async Task ShowBans()
        {
            StringBuilder bans = new StringBuilder();
            var banArray = Context.Guild.GetBansAsync().Result.ToArray();
            foreach (var ban in banArray)
                bans.AppendLine($"{ban.User} for `{ban.Reason}`").AppendLine();
            await Utilities.SendEmbed(Context.Channel, $"Server Bans ({banArray.Length})", bans.ToString(), Colors.Red, "", "");
        }

        private string FindPeopleWithRoles(string targetRole)
        {
            SocketRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name == targetRole);
            StringBuilder description = new StringBuilder();
            foreach (SocketGuildUser user in Context.Guild.Users.ToArray())
                if (user.Roles.Contains(role))
                    description.AppendLine($"{user.Mention}, Level {UserAccounts.GetAccount(user).level}");
            return description.ToString();
        }

        // Find the users that are in a certain role.
        [Command("roles")]
        public async Task FindPeopleInRoles([Remainder]string role) => await Utilities.SendEmbed(Context.Channel, "", FindPeopleWithRoles(role), Context.Guild.Roles.FirstOrDefault(x => x.Name == role).Color, "", "");

        // Show the current version of the bot, the changelog, and a link to the commit
        [Command("version")]
        public async Task GetCurrentVersion()
        {
            string html = Utilities.webClient.DownloadString("https://github.com/WilliamWelsh/GideonBot");
            html = html.Substring(html.IndexOf("commit-tease-sha"));
            html = html.Substring(html.IndexOf("href=\"") + 6);

            string link = "https://github.com" + html.Substring(0, html.IndexOf("\""));
            html = Utilities.webClient.DownloadString(link);

            string title = html, description = html, time = html;

            title = title.Substring(title.IndexOf("commit-title\">") + 14);
            title = title.Substring(0, title.IndexOf("</p>")).Replace(" ", "");

            description = description.Substring(description.IndexOf("<pre>") + 5);
            description = description.Substring(0, description.IndexOf("</pre>"));

            time = time.Substring(time.IndexOf("datetime"));
            time = time.Substring(time.IndexOf(">") + 1);
            time = time.Substring(0, time.IndexOf("</"));

            await Utilities.SendEmbed(Context.Channel, "Current Version", $"Version Name: {title}\nUpdate Description:\n{description}\n\nTo view the changed files and code, visit {link}", Colors.LightBlue, $"Last updated {time}", "https://cdn.discordapp.com/avatars/437458514906972180/2d44b9b229fe91b5bff8d13799a1bcf9.png?size=128");
        }

        // Get the dominant color of an image
        [Command("color")]
        public async Task GetDomColor(string url)
        {
            var color = Utilities.DomColorFromURL(url);
            await Utilities.SendEmbed(Context.Channel, "Dominant Color", $"The dominant color for the image is:\n\nHexadecimal:\n`#{color.R:X2}{color.G:X2}{color.B:X2}`\n\nRGB:\n`Red: {color.R}`\n`Green: {color.G}`\n`Blue: {color.B}`", color, "", url);
        }

        [Command("ping")]
        public async Task Pong() => await Context.Channel.SendMessageAsync("pong!");

        //[Command("join", RunMode = RunMode.Async)]
        //public async Task JoinVC() => await Config.AudioHandler.Join(Context);

        [Command("userdata")]
        public async Task DisplayData(SocketUser user = null)
        {
            var account = UserAccounts.GetAccount(user ?? Context.User);
            StringBuilder data = new StringBuilder();
            data.Append("```json\n  {").AppendLine();
            data.Append($"    \"userID\": {(user == null ? Context.User.Id : user.Id)},").AppendLine();
            data.Append($"    \"coins\": {account.coins},").AppendLine();
            data.Append($"    \"superadmin\": {account.superadmin.ToString().ToLower()},").AppendLine();
            data.Append($"    \"localTime\": {account.localTime},").AppendLine();
            data.Append($"    \"country\": {account.country},").AppendLine();
            data.Append($"    \"xp\": {account.xp},").AppendLine();
            data.Append($"    \"level\": {account.level}").AppendLine();
            data.Append("  }```").AppendLine();
            await Context.Channel.SendMessageAsync(data.ToString());
        }
    }
}