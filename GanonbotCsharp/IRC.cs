using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Configuration;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using RestSharp;
using System.Media;

namespace GanonbotCsharp
{
    class IRC
    {
        #region Globals

        TcpClient Client;
        NetworkStream NwStream;
        StreamReader Reader;
        StreamWriter Writer;
        Thread listen;
        string botUsername;
        string channel;
        string userToken;
        DateTime timer = DateTime.Now;
        DateTime interactionTimer = DateTime.Now;
        int currentChatCount = 0;
        int minuteUpdateCount = 0;
        string[] builtInCommandNames = { "!help", "!addCommand", "!aCommand", "!removeCommand", "!rcommand", "!game", "!uptime", "!title", "!status", "!setTitle", "!setStatus", "!setGame", "!ping", "!ring" }; 
        List<List<string>> interaction = new List<List<string>>();
        List<List<string>> timeoutList = new List<List<string>>();
        List<List<string>> commandList = new List<List<string>>();
        List<List<string>> banList = new List<List<string>>();
        SQLiteParser sql = new SQLiteParser();

        #endregion

        #region Constructor
        public IRC()
        {
            //"https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id=" + ConfigurationManager.AppSettings["client_id"] + "&redirect_uri=http://localhost&scope=channel_read+channel_editor+channel_commercial+channel_subscriptions+channel_check_subscription
            
            Client = new TcpClient("irc.twitch.tv", 6667);
            NwStream = Client.GetStream();
            Reader = new StreamReader(NwStream, Encoding.GetEncoding("iso8859-1"));
            Writer = new StreamWriter(NwStream, Encoding.GetEncoding("iso8859-1"));
            botUsername = ConfigurationManager.AppSettings["username"];
            channel = ConfigurationManager.AppSettings["channel"];
            timeoutList = sql.readSQLite("timeoutWords", "name", "duration");
            banList = sql.readSQLite("bannedwords", "name");
            commandList = sql.readSQLite("commands", "command", "response");


            listen = new Thread(new ThreadStart(Listen));
            listen.Start();



            Writer.WriteLine("PASS " + ConfigurationManager.AppSettings["password"]);
            Writer.Flush();
            Writer.WriteLine("NICK " + botUsername);
            Writer.Flush();
            Writer.WriteLine("USER " + botUsername + "8 * :" + botUsername);
            Writer.Flush();
            Writer.WriteLine("JOIN #" + channel + "\n");
            Writer.Flush();

        }
        #endregion

        #region Main Loop

        private void Listen()
        {
            string Data = "";
            SendTMIMessage("CAP REQ :twitch.tv/tags");
            //>@color=#8A2BE2;display-name=ShivoPlays;emotes=;subscriber=0;turbo=0;user-id=37565404;user-type=mod :shivoplays!shivoplays@shivoplays.tmi.twitch.tv PRIVMSG #shivoplays :1

            while ((Data = Reader.ReadLine()) != null)
            {
                string _nick = "";
                string _message = "";
                string _color = "";
                bool _isMod = false;
                Color nameColor = Color.Black;
                string[] ex;
                string[] userInfo;


                //writes to main terminal
                //Program.mainForm.AddLine(Data);

                //responds to PINGs
                #region Parse Message
                ex = Data.Split(new char[] { ' ' }, 5);
                if (ex[0] == "PING")
                {
                    SendTMIMessage("PONG " + ex[1]);
                    Console.WriteLine("PONG " + ex[1]);
                }

                if (ex[0] != "PING")
                {
                    userInfo = ex[0].Split(new char[] { ';' });
                    List<string> userString = new List<string>();
                    foreach (string info in userInfo)
                    {
                        if (info.StartsWith("@color"))
                        {
                            _color = info.Split(new char[] { '=' })[1];
                            nameColor = ColorTranslator.FromHtml(_color);
                        }
                        if (!info.StartsWith("emotes"))
                        {
                            userString.Add(info);
                        }
                        if (info.StartsWith("user-type"))
                        {
                            _isMod = info.Split(new char[] { '=' })[1].ToLower().Equals("mod");
                        }
                        if (info.StartsWith("display-name"))
                        {
                            _nick = info.Split(new char[] { '=' })[1];
                            if (_nick == "")
                            {
                                _nick = ex[1].Split(new char[] { '!' })[0].TrimStart(':');
                            }
                        }
                    }
                    if( _nick.ToLower() == channel.ToLower())
                    {
                        _isMod = true;
                    }
                    ex[0] = "";
                    foreach (string use in userString)
                    {
                        ex[0] += use;
                    }


                    Data = "";
                    foreach (string item in ex)
                    {
                        Data += item;
                    }
                }


                //"@color=#8A2BE2display-name=ShivoPlayssubscriber=0turbo=0user-id=37565404user-type=mod:shivoplays!shivoplays@shivoplays.tmi.twitch.tvPRIVMSG#shivoplays:http://i.imgur.com/PNcuiwF.gif test"
                char[] delimeters = { ':' };
                string[] split1 = Data.Split(delimeters, 3);
                if (split1.Length > 1 && !split1[0].StartsWith("PING"))
                {
                    try
                    {
                        //Get message
                        if (split1.Length > 2)
                        {
                            _message = split1[2];
                        }
                    }
                    catch
                    {
                    }
                }
                #endregion

                #region Message Checker
                if (_message != "")
                {
                    #region modCommands
                    if (_isMod)
                    {
                        if (_message.StartsWith("!"))
                        {

                            bool isCommand = CheckCommandList(_nick, _message);
                            bool isBuiltInCommand = BuiltInCommands(_nick, _message); 
                            if(!isCommand && !isBuiltInCommand)
                            SendMessage("Command invalid: " + _message.Split(' ')[0]);
                        }
                    }
                    #endregion
                    #region userChat
                    else
                    {
                        CheckTimeoutList(_nick, _message);
                        CheckBanList(_nick, _message);
                    }
                    #endregion
                }
                #endregion

                #region Chart Stats
                if (_nick != "")
                {
                    AddToList(_nick);
                }

                if ((DateTime.Now - timer).TotalSeconds > 10)
                {
                    interaction = OrderList(interaction);
                    Program.mainForm.ClearChart();
                    List<List<string>> displayList = new List<List<string>>();
                    displayList.AddRange(interaction.Take(10));
                    foreach (List<string> li in displayList)
                    {
                        if (_nick != "")
                        {
                            AddToTopUserInteractionChart(li[0], displayList);
                        }
                    }
                    timer = DateTime.Now;
                }
                if ((DateTime.Now - interactionTimer).TotalSeconds > 30)
                {
                    AddToInteractionChart();
                    interactionTimer = DateTime.Now;
                }
                currentChatCount++;
                #endregion

                DisplayChat(_message, _nick, _isMod, nameColor);
            }
        }




        #endregion

        #region Private Methods
        private string GetJSON(string channel)
        {
            string json = "";
            using (WebClient wc = new WebClient())
            {
                json = wc.DownloadString("https://api.twitch.tv/kraken/channels/" + channel);
            }
            return json;
        }
        private dynamic GetJSONFromLink(string link)
        {
            string json = "";
            using (WebClient wc = new WebClient())
            {
                json = wc.DownloadString(link);
            }
            return JObject.Parse(json);
            
        }
        private string GetStreamJSON(string channel)
        {
            string json = "";
            using (WebClient wc = new WebClient())
            {
                json = wc.DownloadString("https://api.twitch.tv/kraken/streams/" + channel);
            }
            return json;
        }
        private void PushStatusJSON(string url, string status)
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.PUT);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", "OAuth " + userToken);
            request.AddHeader("accept", "application/vnd.twitchtv.v3+json");
            request.AddParameter("application/json", "{\"channel\":{\"status\":\""+status+"\"}}",
                ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
        }
        private void PushGameJSON(string url, string game)
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.PUT);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", "OAuth " + userToken);
            request.AddHeader("accept", "application/vnd.twitchtv.v3+json");
            request.AddParameter("application/json", "{\"channel\":{\"game\":\"" + game + "\"}}",
                ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
        }
        private dynamic ParseJSON(string json)
        {
            return JObject.Parse(json);
        }
        private void DisplayChat(string message, string name = "", bool isMod = false, Color? color = null)
        {
            if (color == null)
            {
                color = Color.FromArgb(0, 0, 0);
            }
            Program.mainForm.SetText(message, (Color)color, isMod, name);
        }
        private void AddToList(string name)
        {
            bool existsInList = false;
            int nameIndex = -1;
            int count = -1;
            foreach (List<string> temp in interaction)
            {
                if (temp[0].Equals(name))
                {
                    existsInList = true;
                    nameIndex = interaction.IndexOf(temp);
                    count = int.Parse(temp[1]) + 1;
                    temp[0] = name;
                    temp[1] = count.ToString();
                }
            }
            if (!existsInList)
            {
                interaction.Add(new List<string>() { name, "1" });
                nameIndex = interaction.Count - 1;
                count = 1;
            }

        }
        private void AddToTopUserInteractionChart(string name, List<List<string>> displayList)
        {
            bool existsInList = false;
            int nameIndex = -1;
            int count = -1;
            foreach (List<string> temp in displayList)
            {
                if (temp[0].Equals(name))
                {
                    nameIndex = displayList.IndexOf(temp);
                    count = int.Parse(temp[1]);
                }
            }
            Program.mainForm.AddUserInteractionChart(name, nameIndex, count, existsInList);

        }
        private void AddToInteractionChart()
        {
            int dataPoint = currentChatCount - minuteUpdateCount;
            Program.mainForm.AddInteractionChart(dataPoint);
            minuteUpdateCount = currentChatCount;
        }
        private List<List<string>> OrderList(List<List<string>> unorderedList)
        {
            List<List<string>> orderedList = new List<List<string>>();
            foreach (List<string> tup in unorderedList)
            {
                List<string> temp = new List<string>() { "", "-1" };
                foreach (List<string> unlis in unorderedList)
                {
                    bool isDupe = false;
                    foreach (List<string> t in orderedList)
                    {
                        if (unlis[0].Equals(t[0]))
                        {
                            isDupe = true;
                        }

                    }
                    if (int.Parse(unlis[1]) > int.Parse(temp[1]) && !isDupe)
                    {
                        temp[0] = unlis[0];
                        temp[1] = unlis[1];
                    }
                }
                orderedList.Add(new List<string>() { temp[0], temp[1] });
            }
            return orderedList;
        }
        private void CheckTimeoutList(string username, string message)
        {
            foreach(List<string> timeout in timeoutList)
            {
                message = " " + message.Trim() + " ";
                message = message.ToLower();
                timeout[0] = " " + timeout[0].Trim() + " ";
                timeout[0] = timeout[0].ToLower();
                if (message.Contains(timeout[0]))
                {
                    SendMessage(".timeout " + username + " " + timeout[1]);
                }
            }
        }
        private void CheckBanList(string username, string message)
        {
            foreach (List<string> bannedWord in banList)
            {
                message = " " + message.Trim() + " ";
                message = message.ToLower();
                bannedWord[0] = " " + bannedWord[0].Trim() + " ";
                bannedWord[0] = bannedWord[0].ToLower();
                if (message.Contains(bannedWord[0]))
                {
                    SendMessage(".ban " + username);
                }
            }
        }
        private bool CheckCommandList(string username, string message)
        {
            foreach (List<string> command in commandList)
            {
                message = " " + message.Trim() + " ";
                message = message.ToLower();
                command[0] = " " + command[0].Trim() + " ";
                command[0] = command[0].ToLower();
                if (message.StartsWith(command[0]))
                {
                    SendMessage(command[1]);
                    return true;
                }
            } 
            return false;
        }
        private bool BuiltInCommands(string _nick, string _message)
        {
            try
            {
            char[] separatingChars = {' '};
            string[] messageSplit = _message.Split(separatingChars, 3);
            string json = "";
            dynamic channelInformation;
            bool isBuiltIn = false;
            switch (messageSplit[0].ToLower())
            {
                case "!help":
                case "!commands":
                    string commands = "Built in commands: ";
                    foreach (string command in builtInCommandNames)
                    {
                        commands += command + " ";
                    }
                    SendMessage(commands);

                    commands = "Custom Commands: ";
                    foreach (List<string> command in commandList)
                    {
                        commands += command[0] + " ";
                    }
                    SendMessage(commands);
                    return true;
                case "!addcommand":
                case "!acommand":
                    if (!messageSplit[1].StartsWith("!"))
                    {
                        messageSplit[1] = "!"+messageSplit[1];
                    }
                    foreach (string command in builtInCommandNames)
                    {
                        if (messageSplit[1] == command)
                        {
                            isBuiltIn = true;
                        }
                    }
                    if (isBuiltIn)
                    {
                        SendMessage(messageSplit[1] + " is a built in command. Please choose a different name.");
                        return true;
                    }
                    else
                    {
                        sql.addToSQLite("commands", messageSplit[1], messageSplit[2]);
                        SendMessage("Command: " + messageSplit[1] + " has been added.");
                        ReloadConfigs();
                        return true;
                    }
                case "!removecommand":
                case "!rcommand":
                    if (!messageSplit[1].StartsWith("!"))
                    {
                        messageSplit[1] = "!" + messageSplit[1];
                    }
                    foreach (string command in builtInCommandNames)
                    {
                        if (messageSplit[1] == command)
                        {
                            isBuiltIn = true;
                        }
                    }
                    if (isBuiltIn)
                    {
                        SendMessage(messageSplit[1] + " is a built in command. The channel owner can disable these in the bot's dashboard.");
                        return true;
                    }
                    else
                    {
                        sql.removeValue("commands", messageSplit[1]);
                        SendMessage("Command: " + messageSplit[1] + " has been removed");
                        ReloadConfigs();
                        return true;
                    }
                case "!game":
                    //TODO change name
                    json = GetJSON("allule");
                    channelInformation = ParseJSON(json);
                    SendMessage((string)(channelInformation.display_name + " is currently playing " + channelInformation.game)); 
                    return true;
                case "!uptime":
                    //TODO change name
                    json = GetStreamJSON("allule");
                    channelInformation = ParseJSON(json);
                    string createdAt = channelInformation.stream.created_at;
                    DateTime streamStart = DateTime.Parse(createdAt);
                    TimeSpan uptime = DateTime.UtcNow - streamStart;
                    SendMessage(char.ToUpper(channel[0])+channel.Substring(1)+" has been streaming for " + uptime.ToString(@"hh\:mm\:ss"));
                    return true;
                case "!title":
                case "!status":
                case "!settitle":
                case "!setstatus":
                    json = GetJSON(channel);
                    channelInformation = ParseJSON(json);
                    if (messageSplit.Length>2)
                        PushStatusJSON("https://api.twitch.tv/kraken/channels/" + channel, messageSplit[1] + " " + messageSplit[2]);
                    else
                        PushStatusJSON("https://api.twitch.tv/kraken/channels/" + channel, messageSplit[1]);
                    SendMessage("Channel title has been updated");
                    return true;
                case "!setgame":
                    json = GetJSON(channel);
                    channelInformation = ParseJSON(json);
                    if (messageSplit.Length>2)
                        PushGameJSON("https://api.twitch.tv/kraken/channels/" + channel, messageSplit[1] + " " + messageSplit[2]);
                    else
                        PushGameJSON("https://api.twitch.tv/kraken/channels/" + channel, messageSplit[1]);
                    SendMessage("Channel title has been updated");
                    return true;
                case "!ping":
                case "!ring":
                    SoundPlayer sp = new SoundPlayer(Directory.GetCurrentDirectory() + "\\ring.wav");
                    sp.Play();
                    return true;
                default:
                    return false;
            }
            }
            catch(Exception)
            {
                return false;
            }
        }


        #endregion

        #region Public Methods
        public void SendTMIMessage(string message)
        {
            Writer.WriteLine(message);
            Writer.Flush();
        }

        public void SendMessage(string message)
        {
            Program.mainForm.SetText(message, Color.Red, false, "BOT "+botUsername);
            Writer.WriteLine(":" + botUsername + "!" + botUsername + "@" + botUsername + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
            Writer.Flush();
        }

        public void closeProgram()
        {
            listen.Abort();
            Environment.Exit(1);
        }

        public void ReloadConfigs()
        {
            timeoutList = sql.readSQLite("timeoutWords", "name", "duration");
            banList = sql.readSQLite("bannedWords", "name");
            commandList = sql.readSQLite("commands", "command", "response");
        }

        public void ClearStats()
        {
            interaction.Clear();
            currentChatCount = 0;
            minuteUpdateCount = 0;
        }

        public void SetAccountToken(string url)
        {
            //{http://localhost/#access_token=jr20lumnvydmks9pydjbc8unvys4c8&scope=channel_read+channel_editor+channel_commercial+channel_subscriptions+channel_check_subscription}
            char[] delimeters = { '#', '&' };
            string[] urlSplit = url.Split(delimeters);
            string[] accessTokenSplit = urlSplit[1].Split('=');
            userToken = accessTokenSplit[1];
        }

        public bool IsAuthenticated()
        {
            //TODO fix this method to return correct value
            return false;
        }

        public string GetAuthenticationTokenUrl()
        {
            return ("https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id=" + ConfigurationManager.AppSettings["client_id"] + "&redirect_uri=http://asdfghjkl&scope=channel_read+channel_editor+channel_commercial+channel_subscriptions+channel_check_subscription");
        }        
        #endregion

    }
}
