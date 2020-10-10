using Coral_Bot;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DiscordBot_Test
{
    class Derpi_Search
    {
        public static BooruResult SearchForImage(string SearchTerm, bool isSafeSearch)
        {
            //replace any instance of >= <= > and < with the relevant syntax
            SearchTerm = SearchTerm.Replace(">=", ".gte:");
            SearchTerm = SearchTerm.Replace("<=", ".lte:");
            SearchTerm = SearchTerm.Replace(">", ".gt:");
            SearchTerm = SearchTerm.Replace("<", ".lt:");

            string[] searchParts = SearchTerm.Split(',');
            for (int i = 0; i < searchParts.Length; i++)
            {
                searchParts[i] = searchParts[i].Trim();
            }

            //Aliases
            for (int i = 0; i < searchParts.Length; i++)
            {
                if (searchParts[i] == "nsfw") searchParts[i] = "(explicit OR questionable)";
                if (searchParts[i] == "tiddies") searchParts[i] = "breasts";
            }
            SearchTerm = "";
            foreach (string par in searchParts)
            {
                SearchTerm += par + ",";
            }

            SearchTerm = SearchTerm.Trim(new char[] { ',' });

            //make a new container for our result
            BooruResult result = new BooruResult();
            //make safe search
            if (isSafeSearch)
            {
                SearchTerm += ", (Safe OR Suggestive)";
            }

            //constructing a search string
            string baseLink = "https://derpibooru.org/search.json?q=";
            string keyPart = "&key=" + Config.derpiKey;
            string pagePart = "&page=";
            int page = 1;
            Random random = new Random();
            int seed = random.Next(900000);

            string currentPage = baseLink + SearchTerm + keyPart + pagePart + Convert.ToString(page) + "&sf=random:" + seed.ToString() + "&sd=desc";

            string searchResult = BooruUtilities.GetPage(currentPage);
            dynamic stuff = JsonConvert.DeserializeObject(searchResult);
            if (stuff.total > 0)
            {
                //result.link = "https:" + stuff.search[0].representations.full;
                //extract page link
                result.link = "https://derpibooru.org/" + stuff.search[0].id;
                //image score
                result.score = stuff.search[0].score;
                //get all the tags
                string tags = stuff.search[0].tags;
                string[] tagList = tags.Split(',');

                //we only want to show the artist though
                foreach (string tag in tagList)
                {
                    if (tag.Trim().StartsWith("artist:"))
                    {
                        result.artist = tag;
                    }
                }
            }

            return result;
        }

        internal static async Task SearchAndPost(string searchTerm, SocketMessage e, bool isSafeSearch)
        {
            BooruResult result = Derpi_Search.SearchForImage(searchTerm, isSafeSearch);
            string message = "";


            if (result.link != null)
            {
                if (isSafeSearch)
                    message += "<:coral:279967024438837249> <Coral returns happily with the picture you wanted!> \n`" + searchTerm + "`\n" + result.link;
                else
                    message += "<:coral:279967024438837249> <Coral returns excitedly with the picture you wanted!> \n`" + searchTerm + "`\n" + result.link;

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

    public class BooruResult
    {
        public string artist { get; set; }
        public string link { get; set; }
        public string score { get; set; }
    }

    public class BooruUtilities
    {
        public static string GetPage(string URL)
        {
            string result = "";

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
            request.UserAgent = Config.userAgent;
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            result = reader.ReadToEnd();

            reader.Close();
            response.Close();

            return result;
        }
    }

}
