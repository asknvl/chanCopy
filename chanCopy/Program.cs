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

            ChannelDuplicate ch = new ChannelDuplicate();
            ch.start();
            Console.ReadLine();
        }
    }

    class Bot
    {
        #region const
        const string Token = "5488924440:AAFZWawuQNbBFBW2Kel_Wk_hrM8ZbTWG7Oo";
        #endregion

        #region vars
        ITelegramBotClient botClient;
        CancellationTokenSource cts;
        string channelName;
        #endregion

        public Bot(string cnannelName)
        {
            botClient = new TelegramBotClient(Token);
            this.channelName = cnannelName;
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
        public async void PostMedia(string channelName, string inlineText, string inlineUrl, byte[] mediaBytes, CancellationToken cts)
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
                chatId: channelName,
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

        public async void PostWebPage(string channelName, string inlineText, string inlineUrl, string message, string webPage, MessageEntity[] entities, CancellationToken cts)
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
            chatId: channelName,            
            text: t,
            //entities: new Telegram.Bot.Types.MessageEntity[]
            //{
            //    new Telegram.Bot.Types.MessageEntity()
            //    {
            //        Type = MessageEntityType.Bold,
            //        Offset = 2,
            //        Length = 10
            //    }
            //},
            replyMarkup: inlineKeyboard,            
            parseMode: ParseMode.Html,            
            cancellationToken: cts);
        }
        #endregion
    }

    class ChannelDuplicate
    {
        static string Config(string what)
        {
            switch (what)
            {
                case "api_id": return "13180345";
                case "api_hash": return "df78e4859fb0cbd03dc5cf83d5d0c0cb";
                case "phone_number": return "+79256186936";
                case "verification_code": return "38615"; /*Console.Write("Code: "); return Console.ReadLine();*/
                case "first_name": return "Alexey";      // if sign-up is required
                //case "last_name": return "Doe";        // if sign-up is required
                //case "password": return "secret!";     // if user has enabled 2FA
                default: return null;                  // let WTelegramClient decide the default config
            }

            //switch (what)
            //{
            //    case "api_id": return "15092071";
            //    case "api_hash": return "49cb5f96f44c30729ae7caeb93dea54d";
            //    case "phone_number": return "+79267481530";
            //    case "verification_code": return "79559"; /*Console.Write("Code: "); return Console.ReadLine();*/
            //    case "first_name": return "dnklknvl";      // if sign-up is required
            //    case "last_name": return "";        // if sign-up is required
            //    case "password": return "secret!";     // if user has enabled 2FA
            //    default: return null;                  // let WTelegramClient decide the default config
            //}

#if DEBUG

            //switch (what)
            //{
            //    case "api_id": return "10007326";
            //    case "api_hash": return "5dd41a6fe9bf34ee8e7782eaf27e5f6f";
            //    case "phone_number": return "+84568357459";
            //    case "verification_code": return "78337"; /*Console.Write("Code: "); return Console.ReadLine();*/
            //    case "first_name": return "John";      // if sign-up is required
            //    case "last_name": return "Doe";        // if sign-up is required
            //    case "password": return "secret!";     // if user has enabled 2FA
            //    default: return null;                  // let WTelegramClient decide the default config
            //}
#else
#endif

        }

        User My;
        readonly Dictionary<long, User> Users = new();
        readonly Dictionary<long, ChatBase> Chats = new();
        TL.Messages_Chats chats;
        WTelegram.Client client;
        TL.Messages_Dialogs dialogs;
        System.Timers.Timer mediaTimer = new System.Timers.Timer();

#if DEBUG
        //test output channel parameters
        long outputChannelID = 1597383421;
        string outputChannelName = "@mytestlalalalal";
        string outputTargetTgLink = "@daavid_gzlez";
        string outputTelegramLink = "@Daavid_Gonzalez";
        string outputButtonButtonUrl = "http://t.me/Daavid_Gonzalez";
#else
        //test output channel parameters
        long outputChannelID = 1597383421;
        string outputChannelName = "@mytestlalalalal";
        string outputTargetTgLink = "@daavid_gzlez";
        string outoutTelegramLink = "@ceoxtime";
        string outputButtonButtonUrl = "http://t.me/ceoxtime";
#endif

        Bot bot;

        public async void start()
        {
            Console.WriteLine("chanCopy 0.2");

            try
            {
                client = new WTelegram.Client(Config);
                var user = await client.LoginUserIfNeeded();
                dialogs = await client.Messages_GetAllDialogs();
                chats = await client.Messages_GetAllChats();
                
                foreach (var item in chats.chats)
                {
                    Console.WriteLine($"{item.Key} : {item.Value}");
                }

                client.Update += Client_Update;

                mediaTimer.Interval = 2000;
                mediaTimer.AutoReset = false;
                mediaTimer.Elapsed += MediaTimer_Elapsed;

                bot = new Bot(outputChannelName);
                bot.Start();


            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void MediaTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var target = chats.chats[outputChannelID];
            client.Messages_SendMultiMedia(target, mediaList.ToArray(), false, false, false, false, null, null, null);
            mediaList.Clear();
        }

        private void Client_Update(TL.IObject arg)
        {
            if (arg is not UpdatesBase updates) return;
            updates.CollectUsersChats(Users, Chats);

            foreach (var update in updates.UpdateList)
                switch (update)
                {
                    case UpdateNewMessage unm:
                        ProcessMessage(unm.message);                        
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
        private string updateMessage(string input, string login_to_remove, string login_to_insert)
        {            
            return input.Replace(login_to_remove, login_to_insert);
        }

        private async void ProcessMessage(MessageBase messageBase, bool edit = false)
        {

            switch (messageBase)
            {
                case Message m:
                    Console.WriteLine($"{Peer(m.from_id) ?? m.post_author} in {Peer(m.peer_id)}> {m.message}");

                    if (m.Peer.ID.Equals(outputChannelID))
                        return;

                    if (!m.Peer.ID.Equals(1443613753))
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
                                    bot.PostMedia(outputChannelName, inlineText, inlineUrl, mediaBytes, new CancellationToken());
                            }
                            
                        } else
                            await client.SendMessageAsync(target, updateMessage(m.message, outputTargetTgLink, outputTelegramLink), doc, 0, m.entities);
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

                        string ns = m.message.Replace("@daavid_gzlez", "@ceoxtime");
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
                                bot.PostWebPage(outputChannelName, inlineText, inlineUrl, updateMessage(m.message, outputTargetTgLink, outputTelegramLink), url, m.entities, new CancellationToken());
                        } else                      
                            await client.SendMessageAsync(target, updateMessage(m.message, outputTargetTgLink, outputTelegramLink), null, 0, m.entities, default, true);                         
                        return;
                    }


                    if (mm is MessageMediaPhoto)
                    {
                        var mmd = (MessageMediaPhoto)mm;
                        var doc = mmd.photo;

                        InputSingleMedia sm = new InputSingleMedia();
                        sm.media = doc;
                        sm.message = updateMessage(m.message, outputTargetTgLink, outputTelegramLink);                        
                        sm.entities = m.entities;
                        sm.random_id = r.Next();
                        sm.flags = InputSingleMedia.Flags.has_entities;

                        if (mediaList.Count == 0)
                            mediaTimer.Start();

                        mediaList.Add(sm);
                        return;
                    }

                    await client.SendMessageAsync(target, updateMessage(m.message, outputTargetTgLink, outputTelegramLink), null, 0, m.entities);                   

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
