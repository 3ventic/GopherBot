using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GopherBot
{
    class Program
    {
        static void Main(string[] args) => new Program().Run(args[0]).Wait(); /// TODO: read token from config

        private DiscordSocketClient client = new DiscordSocketClient();
        private ISocketMessageChannel welcomeChannel = null;
        private IRole role = null;
        private RequestOptions reqOpt = new RequestOptions()
        {
            RetryMode = RetryMode.RetryRatelimit
        };

        private async Task Run(string token)
        {
            client.Connected += Client_Connected;
            client.MessageReceived += Client_MessageReceived;
            client.UserJoined += Client_UserJoined;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Client_Connected() => Task.Run(() =>
                                                     {
                                                         welcomeChannel = (ISocketMessageChannel) client.GetChannel(304963802904920065L); /// TODO: read from config
                                                         role = client.GetGuild(231457403546107905L).GetRole(304973381860851714L); /// TODO: read from config
                                                     });

        private async Task Client_UserJoined(SocketGuildUser user)
            => await welcomeChannel.SendMessageAsync($"Welcome, {user.Mention}! Please @ me to get full Minion access to the Discord. See #info for useful general information.");

        private async Task Client_MessageReceived(SocketMessage message)
        {
            Console.WriteLine($"[Received] {message.Channel.Name} - {welcomeChannel?.Name}: {message.Content}");
            if (message.Channel.Id == welcomeChannel?.Id)
            {
                if (message.Content.ToLower().Contains("@GopherBot") || message.MentionedUsers.Contains(client.CurrentUser))
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