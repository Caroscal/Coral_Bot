using Discord.WebSocket;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DiscordBot_Test
{
    class E621Search
    {
        public static BooruResult SearchForImage(string SearchTerm)
        {
            //replace any instance of >= <= > and < with the relevant syntax
            SearchTerm = SearchTerm.Replace(">=", ":>=");
            SearchTerm = SearchTerm.Replace("<=", ":<=");
            SearchTerm = SearchTerm.Replace(">", ":>");
            SearchTerm = SearchTerm.Replace("<", ":<");

            //make a new container for our result
            BooruResult result = new BooruResult();

            //constructing a search string
            string baseLink = "https://e621.net/post/index.json?tags=";

            //https://e621.net/post/index.json?tags=penis+order%3Arandom
            string currentPage = baseLink + SearchTerm + "+order%3Arandom";

            string searchResult = BooruUtilities.GetPage(currentPage);
            dynamic stuff = JsonConvert.DeserializeObject(searchResult);
            if (stuff.Count > 0)
            {
                result.link = stuff[0].file_url;
                //extract page link
                //image score
                result.score = stuff[0].score;
                //e621 handily has an artist tag
                if (stuff[0].artist.Count > 0)
                    result.artist = stuff[0].artist[0];

            }

            return result;
        }

        internal static async Task SearchAndPost(string searchTerm, SocketMessage e)
        {
            BooruResult result = E621Search.SearchForImage(searchTerm);
            string message = "";


            if (result.link != null)
            {
                message += "<Coral returns excitedly with the picture you wanted!> \n`" + searchTerm + "`\n" + result.link;

                if (result.score != null)
                    message += "\n`Score:" + result.score + "`";

                if (result.artist != null)
                    message += "\n`" + result.artist + "`";
            }
            else
            {
                message += "<Coral returns sadly, she couldn't find anything for `" + searchTerm + "` > :cry: ";
            }

            await e.Channel.SendMessageAsync(message);
        }
    }
}
