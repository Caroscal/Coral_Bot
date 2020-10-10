using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot_Test
{
    class FunFact
    {
        internal static async Task PostAFact(SocketMessage e)
        {
            try
            {
                Random random = new Random();
                if (random.Next(100) < 95)
                {

                    //Go to the TodayILearned subreddit, get the whole front page, and post a random title (minus the TIL)
                    string page = "https://www.reddit.com/r/todayilearned/hot.json";
                    string searchResult = BooruUtilities.GetPage(page);
                    dynamic stuff = JsonConvert.DeserializeObject(searchResult);

                    //get data.children
                    string fact = null;



                    int totalFacts = Convert.ToInt32(stuff.data.children.Count);
                    int factNum = random.Next(totalFacts);

                    fact = stuff.data.children[factNum].data.title;
                    fact = fact.Substring(4);

                    await e.Channel.SendMessageAsync("**Did you know:** `<" + fact + ">`");
                }
                else
                {
                    string[] result = new string[] {"Historia's hair smells like a library!",
                                                    "Historia does not like using fish as a bookmark :c  ",
                                                    "I sometimes illustrate Historia's books with crayon!... Then she yells at me",
                                                    "My middle name is fish!",
                                                    "Historia's trying her best to learn sign language!",
                                                    "I love all of you!",
                                                    "I like fish!"};
                    int r = random.Next(result.Count());
                    await e.Channel.SendMessageAsync("**Did you know:** `<" + result[r] + ">`");
                }
            }
            catch (Exception ex)
            {
                await e.Channel.SendMessageAsync("`Coral is running around panicking because something went badly wrong!!!!`\n" + ex.Message);
            }
        }
    }
}
