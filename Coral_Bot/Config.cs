using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Coral_Bot
{
    static class Config
    {
        //Config fields
        public static ulong adminRole;
        public static string about;
        public static string token;
        public static ulong[] nsfwChannelIDs;
        public static ulong[] adminIDs;
        public static string derpiKey;
        public static string userAgent;

        //load config
        public static void LoadConfig()
        {
            dynamic stuff;
            //read from file
            using (StreamReader r = new StreamReader("config.json"))
            {
                string json = r.ReadToEnd();
                stuff = JsonConvert.DeserializeObject(json);
            }
            try
            {
                adminRole = stuff.admin_role;
                about = stuff.about;
                token = stuff.token;
                derpiKey = stuff.derpi_key;
                userAgent = stuff.user_agent;
                //convert to array
                JArray ids = stuff.nsfw_channels;
                nsfwChannelIDs = ids.Select(v => (ulong)v).ToArray();
                JArray ads = stuff.admins;
                adminIDs = ads.Select(v => (ulong)v).ToArray();
            }
            catch
            {
                throw new Exception("Could not load values from config file.");
            }


        }
    }
}
