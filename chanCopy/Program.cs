﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using TL;

namespace chanCopy
{
    class Program
    {

        static void Main(string[] args)
        {

            Settings settings = new Settings();
            try
            {
                settings.load();

                if (string.IsNullOrEmpty(settings.bot_token) ||
                    string.IsNullOrEmpty(settings.api_id) ||
                    string.IsNullOrEmpty(settings.api_hash) ||
                    string.IsNullOrEmpty(settings.phone_number) ||
                    string.IsNullOrEmpty(settings.first_name) ||
                    string.IsNullOrEmpty(settings.inputChannelID) ||
                    string.IsNullOrEmpty(settings.inputTelegramLink) ||
                    string.IsNullOrEmpty(settings.outputChannelID) ||
                    string.IsNullOrEmpty(settings.outputTelegramLink)) throw new Exception();

            } catch (Exception ex)
            {
                Console.WriteLine("Error: Settings loading fail");
                return;
            }

            ChannelDuplicate ch = new ChannelDuplicate(settings);
            Task.Run(async () => { await ch.start(); }).Wait();            
        }
    }

    class Settings
    {
        #region vars
        string path = "settings.json";
        #endregion

        #region properties
        public string bot_token { get; set; } = "";
        public string api_id { get; set; } = "";
        public string api_hash { get; set; } = "";
        public string phone_number { get; set; } = "";
        //public string verification_code { get; set; } = "";
        public string first_name { get; set; } = "";
        public string last_name { get; set; } = "";

        string inputchid = "";
        public string inputChannelID {
            get => inputchid;
            set
            {
                inputchid = value.Replace("-100", "");
            }
        }
        public string inputTelegramLink { get; set; } = "";

        string outputchid;
        public string outputChannelID { 
            get => outputchid;
            set
            {
                outputchid = value.Replace("-100", ""); 
            }
        }
        public string outputTelegramLink { get; set; } = "";
        #endregion

        #region private
        public void save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            try
            {
                if (File.Exists(path))
                    File.Delete(path);

                File.WriteAllText(path, json);

            } catch (Exception ex)
            {
                throw new Exception("Не удалось сохранить файл JSON");
            }
        }
        #endregion

        public void load()
        {
            if (!File.Exists("settings.json"))
            {
                save();
            }

            string rd = File.ReadAllText(path);

            var p = JsonConvert.DeserializeObject<Settings>(rd);
            if (p != null)
            {
                bot_token = p.bot_token;
                api_id = p.api_id;
                api_hash = p.api_hash;
                phone_number = p.phone_number;
                first_name = p.first_name;
                last_name = p.last_name;
                inputChannelID = p.inputChannelID;
                inputTelegramLink = p.inputTelegramLink;
                outputChannelID = p.outputChannelID;   
                outputTelegramLink = p.outputTelegramLink;
            }
        }

        public override string ToString()
        {
            return $"Spy USER:\n" +
                   $"{phone_number}\n" +
                   $"{first_name}\n" +
                   $"{last_name}\n" +
                   $"Input channel:\n" +
                   $"{inputChannelID}\n" +
                   $"{inputTelegramLink}\n" +
                   $"Output channel:\n" +
                   $"{outputChannelID}\n" +
                   $"{outputTelegramLink}\n";
        }
    }

    class Bot
    {
        #region const
        //мой тестовый
        //const string Token = "5488924440:AAFZWawuQNbBFBW2Kel_Wk_hrM8ZbTWG7Oo";

        //боевой 1
        //const string Token = "5597667104:AAGjH9xOyAzTPOLBY98_D88XZaMkOMKGCNg";

        #endregion

        #region vars
        ITelegramBotClient botClient;
        CancellationTokenSource cts;
        Settings settings;
        long outputChannelName;
        #endregion

        public Bot(Settings _settings)
        {
            this.settings = _settings;
            //long outputChnannelName
            botClient = new TelegramBotClient(settings.bot_token);
            outputChannelName =   long.Parse("-100" + settings.outputChannelID);
        }

        public void Start()
        {
            cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message }
            };
            botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, new CancellationToken());

        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(update);
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        #region public      
        public async void PostMedia(string inlineText, string inlineUrl, byte[] mediaBytes, CancellationToken cts)
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
                {
                    InlineKeyboardButton.WithUrl(
                        text: inlineText,
                        url: inlineUrl
                    )
                }
            );
            Telegram.Bot.Types.Message sentMessage = await botClient.SendVideoNoteAsync(
                chatId: outputChannelName,
                videoNote: new MemoryStream(mediaBytes),
                replyMarkup: inlineKeyboard,
                cancellationToken: cts);

        }


        void insertTag(string tag, int offset, int length, ref string s, ref int tagLengtCntr)
        {
            string startTeg = $"<{tag}>";
            s = s.Insert(offset + tagLengtCntr, startTeg);
            tagLengtCntr += startTeg.Length;
            string stopTeg = $"</{tag}>";
            s = s.Insert(offset + length + tagLengtCntr, stopTeg);
            tagLengtCntr += stopTeg.Length;
        }

        void insertTagUrl(int offset, int length, ref string s, ref int tagLengtCntr, string url)
        {            
            string startTeg = $"<a href=\"{url}\">";
            s = s.Insert(offset + tagLengtCntr, startTeg);            
            tagLengtCntr += startTeg.Length;
            string stopTeg = $"</a>";
            s = s.Insert(offset + length + tagLengtCntr, stopTeg);
            tagLengtCntr += stopTeg.Length;
        }

        public async void PostWebPage(string inlineText, string inlineUrl, string message, string webPage, MessageEntity[] entities, CancellationToken cts)
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
                {
                    InlineKeyboardButton.WithUrl(
                        text: inlineText,
                        url: inlineUrl
                    )
                }
            );

            var u = "\"" + webPage + "\"";
            int tagLenCntr = 0;

            foreach (var item in entities)
            {
                //if (item is MessageEntityTextUrl)
                //{
                //    insertTagUrl(item.offset, item.length, ref message, ref tagLenCntr, ((MessageEntityTextUrl)item).url);
                //}

                if (item is MessageEntityItalic)
                {
                    insertTag("i", item.offset, item.length, ref message, ref tagLenCntr);
                }
                if (item is MessageEntityBold)
                {
                    insertTag("b", item.offset, item.length, ref message, ref tagLenCntr);
                }


            }

            var t = message + "<a href=" + u + ">&#8288;</a>";

            Telegram.Bot.Types.Message sentMessage = await botClient.SendTextMessageAsync(
            //chatId: channelName,
            chatId: outputChannelName,
            text: t,           
            replyMarkup: inlineKeyboard,
            parseMode: ParseMode.Html,
            cancellationToken: cts);
        }
        #endregion
    }

    class ChannelDuplicate
    {
        string Config(string what)
        {
            switch (what)
            {
                case "api_id": return settings.api_id;
                case "api_hash": return settings.api_hash;
                case "phone_number": return settings.phone_number;
                case "verification_code": Console.Write("Code: "); return Console.ReadLine();
                case "first_name": return settings.first_name;// if sign-up is required
                case "last_name": return settings.last_name;        // if sign-up is required
                case "password": return "#QqAa123456";     // if user has enabled 2FA
                default: return null;                  // let WTelegramClient decide the default config
            }


#if DEBUG

            //switch (what)
            //{
            //    case "api_id": return "10007326";
            //    case "api_hash": return "5dd41a6fe9bf34ee8e7782eaf27e5f6f";
            //    case "phone_number": return "+84568357459";
            //    case "verification_code": /*return "65420";*/Console.Write("Code: "); return Console.ReadLine();
            //    case "first_name": return "Stevie";      // if sign-up is required
            //    case "last_name": return "Voughan";        // if sign-up is required
            //    case "password": return "secret!";     // if user has enabled 2FA
            //    default: return null;                  // let WTelegramClient decide the default config
            //}

            //Боевой, анонимный юзер 1
            //switch (what)
            //{
            //    case "api_id": return "16256446";
            //    case "api_hash": return "40c83143fb936994b2fcfd30b6c4d236";
            //    case "phone_number": return "+84568357437";
            //    case "verification_code": return "13805";//Console.Write("Code: "); return Console.ReadLine();
            //    case "first_name": return "Stevie";      // if sign-up is required
            //    case "last_name": return "Voughan";        // if sign-up is required
            //    case "password": return "secret!";     // if user has enabled 2FA
            //    default: return null;                  // let WTelegramClient decide the default config
            //}

#else
#endif

        }

        Settings settings;
        readonly Dictionary<long, User> Users = new();
        readonly Dictionary<long, ChatBase> Chats = new();
        TL.Messages_Chats chats;
        WTelegram.Client client;        
        System.Timers.Timer mediaTimer = new System.Timers.Timer();

#if DEBUG

        
        long inputChannelID;        
        long outputChannelID;
        string inputTelegramLink;
        string outputTelegramLink;
        string outputButtonButtonUrl;


        //Боевые 1
        //long inputChannelID = 1165730518;
        //long outputChannelID = 1604783623;
        //string outputChannelName = "??DISPARA TUS INGRESOS??";
        //string outputChannelName = "-1001604783623";


#else
        //test output channel parameters
        long outputChannelID = 1597383421;
        string outputChannelName = "@mytestlalalalal";
        string outputTargetTgLink = "@daavid_gzlez";
        string outoutTelegramLink = "@ceoxtime";
        string outputButtonButtonUrl = "http://t.me/ceoxtime";
#endif

        Bot bot;

        public ChannelDuplicate(Settings settings)
        {
            this.settings = settings;
            inputChannelID = long.Parse(settings.inputChannelID);
            outputChannelID = long.Parse(settings.outputChannelID);
            inputTelegramLink = settings.inputTelegramLink;
            outputTelegramLink = settings.outputTelegramLink;
            outputButtonButtonUrl = $"http://t.me/{settings.outputTelegramLink.Replace("@", "")}";
        }

        public async Task start()
        {
            Console.WriteLine("chanCopy 0.2 -> клон 2");

            try
            {
                client = new WTelegram.Client(Config);
                var user = await client.LoginUserIfNeeded();                
                chats = await client.Messages_GetAllChats();
                
                foreach (var item in chats.chats)
                {
                    Console.WriteLine($"{item.Key} : {item.Value}");
                }

                client.Update += Client_Update;

                mediaTimer.Interval = 5000;
                mediaTimer.AutoReset = false;
                mediaTimer.Elapsed += MediaTimer_Elapsed;

                bot = new Bot(settings);
                bot.Start();


            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadKey();
        }

        private void MediaTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                var target = chats.chats[outputChannelID];
                client.Messages_SendMultiMedia(target, mediaList.ToArray(), false, false, false, false, null, null, null);
                mediaList.Clear();
            } catch (Exception ex)
            {
                Console.WriteLine(">>" + ex.Message);
            }
        }

        private void Client_Update(TL.IObject arg)
        {
            if (arg is not UpdatesBase updates) return;
            updates.CollectUsersChats(Users, Chats);

            foreach (var update in updates.UpdateList)
                switch (update)
                {
                    case UpdateNewMessage unm:
                        try
                        {
                            ProcessMessage(unm.message);
                        } catch (Exception ex)
                        {
                            Console.WriteLine(">" + ex.Message);
                        }
                        break;               
                }
        }

        List<InputSingleMedia> mediaList = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="login_to_remove">@toremove</param>
        /// <param name="login_to_insert">@toinsert</param>
        /// <returns></returns>
        private string updateMessage(string input)
        {   
            return input.Replace(settings.inputTelegramLink, settings.outputTelegramLink);
        }

        private async void ProcessMessage(MessageBase messageBase, bool edit = false)
        {

            switch (messageBase)
            {
                case Message m:
                    Console.WriteLine($"{Peer(m.from_id) ?? m.post_author} in {Peer(m.peer_id)}> {m.message}");

                    if (m.Peer.ID.Equals(outputChannelID))
                        return;

                    if (!m.Peer.ID.Equals(inputChannelID))
                        return;

                    var target = chats.chats[outputChannelID];

                    Random r = new Random();

                    MessageMedia mm = m.media;                    
                    if (mm is MessageMediaDocument { document: Document document })
                    {
                        var mmd = (MessageMediaDocument)mm;
                        var doc = mmd.document;

                        ReplyMarkup markUp = m.reply_markup;
                        if (markUp != null)
                        {                            
                            var fileStream = new MemoryStream();
                            await client.DownloadFileAsync(document, fileStream);
                            byte[] mediaBytes = fileStream.ToArray();
                            if (markUp != null)
                            {
                                string inlineText = "";
                                string inlineUrl = "";
                                var rows = ((ReplyInlineMarkup)markUp).rows;
                                foreach (KeyboardButtonRow buttonRow in rows)
                                {
                                    foreach (KeyboardButtonBase button in buttonRow.buttons)
                                    {
                                        if (button is KeyboardButtonUrl)
                                        {
                                            KeyboardButtonUrl buttonUrl = (KeyboardButtonUrl)button;
                                            inlineText = buttonUrl.Text;
                                            //inlineUrl = buttonUrl.url;                                            
                                            inlineUrl = outputButtonButtonUrl;
                                        }
                                    }
                                }
                                if (inlineUrl != "" && inlineText != "")
                                    bot.PostMedia(inlineText, inlineUrl, mediaBytes, new CancellationToken());
                            }
                            
                        } else
                            await client.SendMessageAsync(target, updateMessage(m.message), doc, 0, m.entities);
                        return;
                    }


                    if (mm is MessageMediaPoll)
                    {
                        var mmp = (MessageMediaPoll)mm;
                        InputMediaPoll imp = new InputMediaPoll();
                        imp.poll = mmp.poll;
                        await client.Messages_SendMedia(target, imp, "", r.Next(), false, false, false, false, null, null, m.entities, null, null);
                        return;
                    }


                    if (mm is MessageMediaWebPage)
                    {
                        var mmw = (MessageMediaWebPage)mm;
                        var wp = mmw.webpage;
                        var url = ((WebPage)wp).url;

                        ReplyMarkup markUp = m.reply_markup;

                        if (markUp != null)
                        {
                            string inlineText = "";
                            string inlineUrl = "";                            

                            var rows = ((ReplyInlineMarkup)markUp).rows;
                            foreach (KeyboardButtonRow buttonRow in rows)
                            {
                                foreach (KeyboardButtonBase button in buttonRow.buttons)
                                {
                                    if (button is KeyboardButtonUrl)
                                    {
                                        KeyboardButtonUrl buttonUrl = (KeyboardButtonUrl)button;
                                        inlineText = buttonUrl.Text;
                                        //inlineUrl = buttonUrl.url;                                        
                                        inlineUrl = outputButtonButtonUrl;
                                    }
                                }
                            }
                            if (inlineUrl != "" && inlineText != "") 
                                bot.PostWebPage(inlineText, inlineUrl, updateMessage(m.message), url, m.entities, new CancellationToken());
                        } else                      
                            await client.SendMessageAsync(target, updateMessage(m.message), null, 0, m.entities, default, true);                         
                        return;
                    }


                    if (mm is MessageMediaPhoto)
                    {
                        var mmd = (MessageMediaPhoto)mm;
                        var doc = mmd.photo;

                        InputSingleMedia sm = new InputSingleMedia();
                        sm.media = doc;
                        sm.message = updateMessage(m.message);                        
                        sm.entities = m.entities;
                        sm.random_id = r.Next();
                        sm.flags = InputSingleMedia.Flags.has_entities;

                        if (mediaList.Count == 0)
                            mediaTimer.Start();

                        mediaList.Add(sm);                       
                        return;
                    }

                    await client.SendMessageAsync(target, updateMessage(m.message), null, 0, m.entities);                   

                    break;
                case MessageService ms: Console.WriteLine($"{Peer(ms.from_id)} in {Peer(ms.peer_id)} [{ms.action.GetType().Name[13..]}]"); break;
            }
        }

        private string User(long id) => Users.TryGetValue(id, out var user) ? user.ToString() : $"User {id}";
        private string Chat(long id) => Chats.TryGetValue(id, out var chat) ? chat.ToString() : $"Chat {id}";
        private string Peer(Peer peer) => peer is null ? null : peer is PeerUser user ? User(user.user_id)
            : peer is PeerChat or PeerChannel ? Chat(peer.ID) : $"{peer.ID}";
    }
}
