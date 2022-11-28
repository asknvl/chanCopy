using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    class MediaContent
    {
        public byte[] mediaBytes { get; set; }
        public string message { get; set; }
        public Telegram.Bot.Types.MessageEntity[] entities { get; set; }
        public IReplyMarkup markup { get; set; }
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
        System.Timers.Timer mediaTimer;
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

            mediaTimer = new System.Timers.Timer();
            mediaTimer.Interval = 5000;
            mediaTimer.AutoReset = false;
            mediaTimer.Elapsed += MediaTimer_Elapsed;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message }
            };
            botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, new CancellationToken());

        }

        private async void MediaTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //var album = new
            var album = new List<Telegram.Bot.Types.IAlbumInputMedia>();
            int cntr = 0;
            foreach (var item in mediaList)
            {
                Telegram.Bot.Types.InputMedia media = new Telegram.Bot.Types.InputMedia(new MemoryStream(item.mediaBytes), $"file{cntr++}");
                Telegram.Bot.Types.InputMediaPhoto imp = new Telegram.Bot.Types.InputMediaPhoto(media);
                imp.CaptionEntities = item.entities;
                imp.Caption = item.message;
                album.Add(imp);
            }

            mediaList.Clear();

            await botClient.SendMediaGroupAsync(outputChannelName, album);
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
        public async void PostVideo(string inlineText, string inlineUrl, bool isround, string message, byte[] mediaBytes, MessageEntity[] entities, CancellationToken cts)
        {
            try
            {
                InlineKeyboardMarkup inlineKeyboard = null;
                if (!string.IsNullOrEmpty(inlineText) && !string.IsNullOrEmpty(inlineUrl))
                {
                    inlineKeyboard = new(new[]
                    {
                    InlineKeyboardButton.WithUrl(
                        text: inlineText,
                        url: inlineUrl
                    )
                }
                    );
                }

                if (isround)
                {
                    await botClient.SendVideoNoteAsync(
                        chatId: outputChannelName,
                        videoNote: new MemoryStream(mediaBytes),
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cts);
                }
                else
                {
                    await botClient.SendVideoAsync(
                        chatId: outputChannelName,
                        video: new MemoryStream(mediaBytes),
                        caption: message,
                        captionEntities: convertEntities(entities),
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cts);
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }


        List<MediaContent> mediaList = new();
        public void PostPhoto(string inlineText, string inlineUrl, string message, byte[] mediaBytes, MessageEntity[] entities, CancellationToken cts)
        {
           
            if (mediaList.Count == 0)
                mediaTimer.Start();

            InlineKeyboardMarkup inlineKeyboard = new(new[]
                {
                    InlineKeyboardButton.WithUrl(
                        text: inlineText,
                        url: inlineUrl
                    )
                }
            );

            mediaList.Add(new MediaContent() {
                mediaBytes = mediaBytes,
                message = message,
                entities = (entities != null) ? convertEntities(entities).ToArray() : null,
                markup = inlineKeyboard
            });

         

            //await botClient.SendPhotoAsync(outputChannelName, new MemoryStream(mediaBytes));
        }

        List<Telegram.Bot.Types.MessageEntity> convertEntities(MessageEntity[] entities)
        {
            List<Telegram.Bot.Types.MessageEntity> _entities = null;

            if (entities != null) {
                _entities = new List<Telegram.Bot.Types.MessageEntity>();
                foreach (var item in entities)
                {

                    MessageEntityType type = MessageEntityType.Mention;
                    string? url = null;

                    switch (item)
                    {
                        case TL.MessageEntityBold:
                            type = MessageEntityType.Bold;
                            break;
                        case TL.MessageEntityItalic:
                            type = MessageEntityType.Italic;
                            break;
                        case TL.MessageEntityMention:
                            type = MessageEntityType.Mention;
                            break;
                        case TL.MessageEntityTextUrl:
                            type = MessageEntityType.TextLink;
                            url = ((MessageEntityTextUrl)item).url;
                            break;
                        case MessageEntityUnderline:
                            type = MessageEntityType.Underline;
                            break;
                        case MessageEntityUrl:
                            type = MessageEntityType.Url;

                            break;
                        case MessageEntityCode:
                            type = MessageEntityType.Code;
                            break;
                    }

                    _entities.Add(new Telegram.Bot.Types.MessageEntity()
                    {
                        Type = type,
                        Offset = item.offset,
                        Length = item.length,
                        Url = url

                    });
                }
            }

            return _entities;
        }

        public async void PostWebPage(string inlineText, string inlineUrl, string message, string webPage, MessageEntity[] entities, CancellationToken cts)
        {
            try
            {
                InlineKeyboardMarkup inlineKeyboard = new(new[]
                    {
                    InlineKeyboardButton.WithUrl(
                        text: inlineText,
                        url: inlineUrl
                    )
                }
                );

                Telegram.Bot.Types.Message sentMessage = await botClient.SendTextMessageAsync(
                //chatId: channelName,
                chatId: outputChannelName,
                text: message,
                replyMarkup: inlineKeyboard,
                entities: convertEntities(entities),
                cancellationToken: cts);

            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task PostVoice(byte[] mediaBytes, int duration)
        {
            try
            {
                await botClient.SendVoiceAsync(outputChannelName, new MemoryStream(mediaBytes));
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
        }

        Settings settings;
        readonly Dictionary<long, User> Users = new();
        readonly Dictionary<long, ChatBase> Chats = new();
        TL.Messages_Chats chats;
        WTelegram.Client client;        
       
        long inputChannelID;        
        long outputChannelID;
        string inputTelegramLink;
        string outputTelegramLink;
        string outputButtonButtonUrl;

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
            Console.WriteLine("channelCpy 0.0");

            try
            {
                client = new WTelegram.Client(Config);
                var user = await client.LoginUserIfNeeded();                
                chats = await client.Messages_GetAllChats();
                
                foreach (var item in chats.chats)
                {
                    Console.WriteLine($"{item.Key} : {item.Value}");
                }

                client.OnUpdate += Client_Update;
               

                bot = new Bot(settings);
                bot.Start();


            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadKey();
        }

        private Task Client_OnUpdate(IObject arg)
        {
            throw new NotImplementedException();
        }

        //private void MediaTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    try
        //    {
        //        var target = chats.chats[outputChannelID];
        //        client.Messages_SendMultiMedia(target, mediaList.ToArray(), false, false, false, false, null, null, null);
        //        mediaList.Clear();
        //    } catch (Exception ex)
        //    {
        //        Console.WriteLine(">>" + ex.Message);
        //    }
        //}

        private async Task Client_Update(IObject arg)
        {

            await Task.Run(() => { 

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

            });
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
            return input;
        }

        private async void ProcessMessage(MessageBase messageBase, bool edit = false)
        {

            switch (messageBase)
            {
                case Message m:
                    Console.WriteLine($"{Peer(m.from_id) ?? m.post_author} in {Peer(m.peer_id)}> {m.message}");

                    if (m.Peer.ID.Equals(outputChannelID))
                        return;

                    //if (!m.Peer.ID.Equals(inputChannelID) && !m.Peer.ID.Equals(inputChannelID))
                    //    return;

                    var target = chats.chats[outputChannelID];

                    Random r = new Random();

                    MessageMedia mm = m.media;

                   

                    if (mm is MessageMediaDocument { document: Document document })
                    {

                        var fileStream = new MemoryStream();
                        await client.DownloadFileAsync(document, fileStream);
                        byte[] mediaBytes = fileStream.ToArray();

                        if (document.mime_type.Contains("video"))
                        {
                            ReplyMarkup markUp = m.reply_markup;

                            string inlineText = "";
                            string inlineUrl = "";

                            if (markUp != null)
                            {
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
                            }

                            var videoAttribute = document.attributes.FirstOrDefault(a => a is DocumentAttributeVideo);

                            bool isRound = false;
                            int duration = 0;
                            int w = 0, h = 0;

                            if (videoAttribute != null)
                            {
                                var flags = ((DocumentAttributeVideo)videoAttribute).flags;
                                isRound = flags.HasFlag(DocumentAttributeVideo.Flags.round_message);

                                duration = ((DocumentAttributeVideo)videoAttribute).duration;
                                w = ((DocumentAttributeVideo)videoAttribute).w;
                                h = ((DocumentAttributeVideo)videoAttribute).h;
                            }

                            bot.PostVideo(inlineText, inlineUrl, isRound, m.message, mediaBytes, m.entities, new CancellationToken());



                            //var inputFile = await client.UploadFileAsync(new MemoryStream(mediaBytes), "fn");
                            //await client.SendMessageAsync(target, m.message, new InputMediaUploadedDocument
                            //{
                            //    file = inputFile,
                            //    mime_type = "video/mp4",
                            //    attributes = new[] {
                            //    new DocumentAttributeVideo { duration = duration, w = w, h = h,
                            //      flags = DocumentAttributeVideo.Flags.supports_streaming }
                            //  }
                            //});


                            return;
                        } else
                            if (document.mime_type.Contains("audio"))
                        {
                            var audioAttribute = document.attributes.FirstOrDefault(a => a is DocumentAttributeAudio);
                            if (audioAttribute != null)
                            {
                                int duration = ((DocumentAttributeAudio)audioAttribute).duration;
                                await bot.PostVoice(mediaBytes, duration);
                                return;
                            }
                        }

                    }

                    if (mm is MessageMediaPoll)
                    {
                        var mmp = (MessageMediaPoll)mm;
                        InputMediaPoll imp = new InputMediaPoll();
                        imp.poll = mmp.poll;
                        await client.Messages_SendMedia(target, imp, "", r.Next(), false, false, false, false, false, null, null, null, m.entities, null, null);
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


                    if (mm is MessageMediaPhoto { photo: Photo photo })
                    {

                        var mmd = (MessageMediaPhoto)mm;
                        var doc = mmd.photo;

                        var fileStream = new MemoryStream();
                        await client.DownloadFileAsync(photo, fileStream);
                        byte[] mediaBytes = fileStream.ToArray();

                        Console.WriteLine($"photo {mediaBytes.Length} {mediaBytes[1024]}");
                        bot.PostPhoto(null, null, m.message, mediaBytes, m.entities, new CancellationToken());
                
                        return;
                    }

                    if (!string.IsNullOrEmpty(m.message))
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
