using Coral_Bot;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot_Test
{


    class Program
    {
        CultureInfo culture = new CultureInfo("en-GB");

        private DiscordSocketClient _client;

        public static void Main(string[] args)
        {
            Config.LoadConfig();
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task MainAsync()
        {
            Console.WriteLine("Welcome to Coral Bot!");
            _client = new DiscordSocketClient();

            _client.Log += Log;

            _client.MessageReceived += async (e) =>
            {
                await HandleCommands(e);
            };

            _client.UserJoined += async (e) =>
            {
                await HandleUserJoin(e);
            };


            _client.UserLeft += async (e) =>
            {
                await HandleUserLeave(e);
            };

            await _client.LoginAsync(TokenType.Bot, Config.token);
            await _client.SetGameAsync("!help for command list");
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }


        private async Task HandleUserJoin(SocketGuildUser e)
        {
            SocketGuildChannel chRules = null;
            //get a link to the rules channel
            foreach (SocketTextChannel ch in e.Guild.TextChannels)
            {
                if (ch.Id == 467696346069991435)
                    chRules = ch;
            }
            //welcome the new user in the general channel
            foreach (SocketTextChannel ch in e.Guild.TextChannels)
            {
                if ((ch.Name == "dailychats" || ch.Name == "general-goop" || ch.Name == "pleb-chat") && chRules != null)
                {
                    await ch.SendMessageAsync(":star: CoralBot jumps excitedly and points at " + e.Mention + ". Welcome to the server!! :star:\n**<Please check out " + MentionUtils.MentionChannel(chRules.Id) + " and have a great time! If you don't check, Red will gut you! :) >**");
                    Console.WriteLine("User Joined - " + e.Username + " at " + DateTime.Now.ToString(culture));
                    break;
                }
            }
        }

        private async Task HandleUserLeave(SocketGuildUser e)
        {
            foreach (SocketTextChannel ch in e.Guild.TextChannels)
            {
                if (ch.Name == "dailychats" || ch.Name == "general" || ch.Name == "pleb-chat")
                {
                    //await ch.SendMessage(":cry: CoralBot waves goodbye to " + e.Author.Mention + ". :cry: \n`<I hope you have a good time out there!>`");
                    Console.WriteLine("User Left - " + e.Username + " at " + DateTime.Now.ToString(culture));
                    break;
                }
            }
        }

        private async Task HandleCommands(SocketMessage e)
        {
            //is this a DM and what is the author's name?
            bool isDM = false;
            string senderName = e.Author.Username;
            string lowerContent = e.Content.ToLower();
            SocketGuildUser user = null;
            try
            {
                user = (SocketGuildUser)e.Author;
            }
            catch
            {
                Console.WriteLine("PM from " + e.Author.Username + ":");
                Console.WriteLine(e.Content);
            }
            if (e.Channel.GetType() == typeof(SocketDMChannel))
                isDM = true;
            else
                senderName = user.Nickname ?? e.Author.Username;

            //Show help to user
            //Basic echo function
            if (Config.adminIDs.Contains(e.Author.Id) && lowerContent.StartsWith("!echo "))
            {
                await e.Channel.SendMessageAsync(e.Content.Substring(6));
                await e.DeleteAsync();
            }
            //Derpi search
            if (!IsAuthor(e) && lowerContent.StartsWith("!derpi "))
            {
                //get search term
                string searchTerm = e.Content.Substring(7);

                //find a result
                //post resulting image
                try
                {
                    if ( Config.nsfwChannelIDs.Contains(e.Channel.Id) )
                        await Derpi_Search.SearchAndPost(searchTerm, e, false);
                    else
                        await Derpi_Search.SearchAndPost(searchTerm, e, true);
                }
                catch
                {
                    await e.Channel.SendMessageAsync("`**-Coral runs around because everything is broken and on fire!!!!-**`");
                }
            }
            //E621 search
            if (!IsAuthor(e) && e.Channel.Id == 282826164127399936 && lowerContent.StartsWith("!e621 "))
            {
                //get search term
                string searchTerm = e.Content.Substring(6);

                //find a result
                //post resulting image
                await E621Search.SearchAndPost(searchTerm, e);
            }
            //Coin Flip
            if (!IsAuthor(e) && lowerContent.StartsWith("!flip"))
            {
                Random random = new Random();
                await e.Channel.SendMessageAsync(random.Next(2) == 0 ? "`Heads`" : "`Tails`");
            }
            if (!IsAuthor(e) && lowerContent.StartsWith("!ghost"))
            {
                Random random = new Random();
                await e.Channel.SendMessageAsync("**-Coral baps the ghost!-** <Go away spooker!>");
            }
            //Dice Rolling
            if (!IsAuthor(e) && lowerContent.StartsWith("!roll"))
            {
                string sub = "";
                if (e.Content.Length > 6)
                {
                    sub = e.Content.Substring(6);
                }
                else
                {
                    sub = "1d20";
                }
                //await DiceRoll(sub, e);
                await Dice_Roller.DiceRoll(sub, e);
            }
            //8-ball
            if (!IsAuthor(e) && lowerContent.StartsWith("!8ball"))
            {
                await EightBall(e);
            }
            //Spin
            if (!IsAuthor(e) && lowerContent.StartsWith("!spin"))
            {
                await e.Channel.SendFileAsync("UkY5mCJ.gif");
            }
            //F To pay respects
            if (!IsAuthor(e) && lowerContent.StartsWith("!f"))
            {
                string[] sub = e.Content.Split(' ');
                if (sub[0].Length == 2)
                    await e.Channel.SendMessageAsync("**-" + e.Author.Mention + " pays their respects.-** :pray:");
            }
            //Daily fact
            if (!IsAuthor(e) && lowerContent.StartsWith("!fact"))
            {
                await DailyFact(e);
            }
            //Channel ID
            if (!IsAuthor(e) && lowerContent.StartsWith("!channelID"))
            {
                await e.Channel.SendMessageAsync("This channel's ID is" + e.Channel.Id);
            }
            //Lennny Face
            if (!IsAuthor(e) && lowerContent.StartsWith("!lenny"))
            {
                await e.Channel.SendMessageAsync("( ͡° ͜ʖ ͡°)");
            }
            //Set Role
            if (!IsAuthor(e) && lowerContent.StartsWith("!set"))
            {
                await SetRole(e);
            }
            //Unset Role
            if (!IsAuthor(e) && lowerContent.StartsWith("!unset"))
            {
                await UnsetRole(e);
            }
            //hug
            if (!IsAuthor(e) && lowerContent.StartsWith("!hug"))
            {
                string thing = null;

                if (e.Content.Length > 5)
                    thing = e.Content.Substring(5);

                if ((e.MentionedUsers.Count <= 0 && thing == null) || isDM)
                {
                    await e.Channel.SendMessageAsync("**-CoralBot hugs " + senderName + ", who now smells like fish-**");
                }
                else if (e.MentionedUsers.Count <= 0)
                {
                    await e.Channel.SendMessageAsync("**-:hearts: `" + senderName + "` hugs `" + thing + "` :hearts: -**");
                }
                else if (e.MentionedUsers.Count == 1)
                {
                    SocketGuildUser mentionUser = (SocketGuildUser)e.MentionedUsers.ElementAt<SocketUser>(0);
                    string target = mentionUser.Nickname ?? mentionUser.Username;
                    await e.Channel.SendMessageAsync("**-:hearts: `" + senderName + "` hugs `" + target + "` :hearts: -**");
                }
            }
            if (!IsAuthor(e) && lowerContent.StartsWith("!killme"))
            {
                if (e.MentionedUsers.Count <= 0)
                {
                    await e.Channel.SendMessageAsync("**-CoralBot hugs " + senderName + "-** <No!>");
                }
            }
            //Bap
            if (!IsAuthor(e) && lowerContent.StartsWith("!bap"))
            {
                string thing = null;
                if (e.Content.Length > 5)
                    thing = e.Content.Substring(5);

                if ((e.MentionedUsers.Count <= 0 && thing == null) || isDM)
                {
                    await e.Channel.SendMessageAsync("**-CoralBot baps " + senderName + " right on the nose with a rolled up newspaper-** :newspaper2:");
                }
                else if (e.MentionedUsers.Count <= 0)
                {
                    await e.Channel.SendMessageAsync("**-`" + senderName + "` baps `" + thing + "` with a rolled up newspaper-** :newspaper2:");
                }
                else if (e.MentionedUsers.Count == 1)
                {
                    SocketGuildUser mentionUser = (SocketGuildUser)e.MentionedUsers.ElementAt<SocketUser>(0);
                    string target = mentionUser.Nickname ?? mentionUser.Username;
                    await e.Channel.SendMessageAsync("**-`" + senderName + "` baps `" + target + "` with a rolled up newspaper-** :newspaper2:");
                }
            }
            //About Bot
            if (!IsAuthor(e) && lowerContent.StartsWith("!about"))
            {
                await e.Channel.SendMessageAsync(Config.about);
            }
            //Help Menu
            if (!IsAuthor(e) && lowerContent.StartsWith("!help"))
            {
                //await e.Author.SendMessage(@"```Markdown
                await e.Channel.SendMessageAsync(@"```Markdown
CoralBot Commands
==============
!set Watcher - give yourself the Watcher role
!unset Watcher - remove the role from yourself
!derpi [search terms] - Find an image on Derpibooru. (NSFW results limited to NSFW channel)
!e621 [search terms] - Find an image on e621. (Only works in the nsfw-others channel)
!flip - Flip a coin, heads or tails.
!roll [dice or modifiers] - Rolls a d20 dice, or your specified dice (eg: 2d8+5-1d20)
!8ball [your question] - Ask the magic 8-ball your question.
!fact - Get a fun fact of the day!
!spin - Spin the Red.
!lenny - Lenny Face ( ͡° ͜ʖ ͡°)
!f - Pay your respects
!hug [user or thing] - Hug CoralBot, someone or something
!bap [user or thing] - Bap CoralBot, someone or something with a rolled up newspaper
!ghost - Tell the spooker to go away
!about - Information about CoralBot.
!help - Get command list.
```");
            }
            if (!IsAuthor(e) && lowerContent.StartsWith("@Coral-Bot"))
            {
                if (e.MentionedUsers.Count == 1)
                {
                }
            }

        }

        private bool IsAuthor(SocketMessage e)
        {
            if (_client.CurrentUser.Id == e.Author.Id)
                return true;
            else
                return false;
        }

        private async Task SetRole(SocketMessage e)
        {
            SocketGuildChannel chan = (SocketGuildChannel)e.Channel;
            SocketGuildUser user = (SocketGuildUser)e.Author;
            //check we're in the right server
            if (chan.Guild.Id == 467695581054107658)
            {
                ulong roleID = 0;
                //get the role to add
                string[] parts = e.Content.Split(' ');
                if (parts.Length > 1)
                {

                    switch (parts[1].ToLower())
                    {
                        case "watcher":
                            roleID = 471611370530406410;
                            break;
                        default:
                            break;
                    }

                    if (roleID != 0)
                    {
                        //assign role to user
                        List<SocketRole> roleList = new List<SocketRole>();
                        SocketRole r = chan.Guild.GetRole(roleID);
                        roleList.Add(r);
                        if (r != null)
                        {
                            await user.AddRolesAsync(roleList);
                            await e.Channel.SendMessageAsync("`" + r.Name + " has been added to " + e.Author.Username + "`");
                        }
                    }
                    else
                    {
                        await e.Channel.SendMessageAsync("**-Coral Bot looks at you confused-**");
                    }
                }
                else
                {
                    await e.Channel.SendMessageAsync("**-Coral Bot looks at you confused-**");
                }
            }
            else
            {
                await e.Channel.SendMessageAsync("<Whoops, this command doesn't work in this server!>");
            }
        }

        private async Task UnsetRole(SocketMessage e)
        {
            SocketGuildChannel chan = (SocketGuildChannel)e.Channel;
            SocketGuildUser user = (SocketGuildUser)e.Author;
            //check we're in the right server
            if (chan.Guild.Id == 467695581054107658)
            {
                ulong roleID = 0;
                //get the role to add
                string[] parts = e.Content.Split(' ');
                if (parts.Length > 1)
                {

                    switch (parts[1].ToLower())
                    {
                        case "watcher":
                            roleID = 471611370530406410;
                            break;
                        default:
                            break;
                    }
                    if (roleID != 0)
                    {
                        //remove role from user
                        List<SocketRole> roleList = new List<SocketRole>();
                        SocketRole r = chan.Guild.GetRole(roleID);
                        roleList.Add(r);
                        if (r != null)
                        {
                            await user.RemoveRolesAsync(roleList);
                            await e.Channel.SendMessageAsync("`" + r.Name + " has been removed from " + e.Author.Username + "`");
                        }
                    }
                    else
                    {
                        await e.Channel.SendMessageAsync("**-Coral Bot looks at you confused-**");
                    }
                }
                else
                {
                    await e.Channel.SendMessageAsync("**-Coral Bot looks at you confused-**");
                }
            }
            else
            {
                await e.Channel.SendMessageAsync("<Whoops, this command doesn't work in this server!>");
            }
        }

        private async Task DailyFact(SocketMessage e)
        {
            await FunFact.PostAFact(e);
        }

        private async Task DiceRoll(string v, SocketMessage e)
        {
            Random random = new Random();
            int result = random.Next(20) + 1;
            await e.Channel.SendMessageAsync("`" + result.ToString() + "`");
        }

        private async Task EightBall(SocketMessage e)
        {
            //Console.WriteLine("Number of 8Ball uses is " + counter8Ball.ToString());
            Random random = new Random();
            int result = random.Next(21);
            string phrase;
            switch (result)
            {
                case 0: phrase = "It is certain"; break;
                case 1: phrase = "It is decidedly so"; break;
                case 2: phrase = "Without a doubt"; break;
                case 3: phrase = "Yes, definitely"; break;
                case 4: phrase = "You may rely on it"; break;
                case 5: phrase = "As I see it, yes"; break;
                case 6: phrase = "Most likely"; break;
                case 7: phrase = "Outlook good"; break;
                case 8: phrase = "Yes!"; break;
                case 9: phrase = "Signs point to yes"; break;
                case 10: phrase = "Reply hazy try again"; break;
                case 11: phrase = "Ask again later"; break;
                case 12: phrase = "Better not tell you now"; break;
                case 13: phrase = "Cannot predict now"; break;
                case 14: phrase = "Concentrate and ask again"; break;
                case 15: phrase = "Don't count on it"; break;
                case 16: phrase = "My reply is no"; break;
                case 17: phrase = "My sources say no"; break;
                case 18: phrase = "Outlook not so good"; break;
                case 19: phrase = "Very doubtful"; break;
                default: phrase = "Coral drops and cracks the magic 8-Ball. Good work, Coral."; break;
            }

            //phrase = "Fuck yeah!";

            await e.Channel.SendMessageAsync(":8ball: `" + phrase + "`");


        }
    }
}
