using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GopherBot
{
    class Program
    {
        static void Main(string[] args) => new Program().Run().Wait();

        private DiscordSocketClient client = new DiscordSocketClient();
        private ISocketMessageChannel welcomeChannel = null;
        private IRole role = null;
        private RequestOptions reqOpt = new RequestOptions()
        {
            RetryMode = RetryMode.RetryRatelimit
        };

        private async Task Run()
        {
            string token = Environment.GetEnvironmentVariable("GOPHERTOKEN");
            client.Connected += Client_Connected;
            client.GuildAvailable += Client_GuildAvailable;
            client.MessageReceived += Client_MessageReceived;
            client.UserJoined += Client_UserJoined;
            client.Disconnected += Client_Disconnected;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Client_Disconnected(Exception arg)
        {
            Console.WriteLine($"Disconnected due to {arg}");
            Environment.Exit(2);
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(SocketGuild guild) => Task.Run(() =>
        {
            Console.WriteLine($"Guild available: {guild.Name} ({guild.Id})");
            if (guild.Id == 231457403546107905L) /// TODO: read from config
            {
                welcomeChannel = (ISocketMessageChannel)client.GetChannel(304963802904920065L); /// TODO: read from config
                foreach (IRole grole in guild.Roles)
                {
                    Console.WriteLine($"Role: {grole.Name} ({grole.Id})");
                    if (grole.Name == "Minion")
                    {
                        role = grole;
                    }
                }
                Console.WriteLine($"Welcome channel {welcomeChannel?.Name}, role {role?.Name}");
            }
        });

        private Task Client_Connected() => Task.Run(() =>
        {
            Console.WriteLine($"Connected.");
        });

        private async Task Client_UserJoined(SocketGuildUser user)
            => await welcomeChannel.SendMessageAsync($"Welcome, {user.Mention}! Please @ me ({client.CurrentUser.Mention}) to get full Minion access to the Discord. See #info for useful general information.");

        private async Task Client_MessageReceived(SocketMessage message)
        {
            Console.WriteLine($"[Received] {message.Channel.Name} - {welcomeChannel?.Name}: {message.Content}");
            if (!message.Author.IsBot && message.Channel.Id == welcomeChannel?.Id)
            {
                if (message.Content.ToLower().Contains("@GopherBot") || message.MentionedUsers.Where(mention => mention.Id == client.CurrentUser.Id).Count() > 0)
                {
                    SocketGuildUser user = (SocketGuildUser) message.Author;
                    await user.AddRoleAsync(role);
                    await welcomeChannel.SendMessageAsync($"{user.Mention} was made into {role.Name}");
                }
                await message.DeleteAsync(reqOpt);
            }
        }
    }
}