using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using System.Net;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using System.Numerics;

namespace buildBot
{
    internal class Main
    {
        public static Dictionary<long,Players> database = new Dictionary<long,Players>();
        public static async Task<Message> sendStart(ITelegramBotClient botClient, Message mess)
        {
            Message reply =
            await botClient.SendTextMessageAsync(mess.Chat.Id, "Hello, My name is Genshin Character Builder made by @Veebapun.\nYou can send me all the photos of your character build and i will combine them in one photo.\nPlease Send /help to know how to use me.");
            return reply;
        }
        public static async Task<Message> sendHelp(ITelegramBotClient botClient, Message mess)
        {
            Message reply =
            await botClient.SendTextMessageAsync(mess.Chat.Id, "I will crop and combine all your character build screenshot into one image.\nNote: Only 16:9 ratio screenshots are compaitable with the bot. If your screenshot doesn't match the required resolution, It will give an error.\n\nCommand:\n/build {character name}\n\nYou have to send all your build screenshot in a predetermind order and the caption of the photo should be the index of the photo with \'#\' as a prefix.\n\nHere is a example of the screenshots:");

            await botClient.SendDocumentAsync(mess.Chat.Id, "https://cdn.discordapp.com/attachments/916000654227546208/980886877118804078/unknown.png",caption: "#1");
            await botClient.SendDocumentAsync(mess.Chat.Id, "https://cdn.discordapp.com/attachments/916000654227546208/980886909096189962/unknown.png", caption: "#2");
            await botClient.SendDocumentAsync(mess.Chat.Id, "https://cdn.discordapp.com/attachments/916000654227546208/980886928897490984/unknown.png", caption: "#3");
            await botClient.SendDocumentAsync(mess.Chat.Id, "https://cdn.discordapp.com/attachments/916000654227546208/980886953446739998/unknown.png", caption: "#4");
            await botClient.SendDocumentAsync(mess.Chat.Id, "https://cdn.discordapp.com/attachments/916000654227546208/980886975181647992/unknown.png", caption: "#5");
            await botClient.SendDocumentAsync(mess.Chat.Id, "https://cdn.discordapp.com/attachments/916000654227546208/980887007951736892/unknown.png", caption: "#6");
            await botClient.SendDocumentAsync(mess.Chat.Id, "https://cdn.discordapp.com/attachments/916000654227546208/980887025718812742/unknown.png", caption: "#7");
            await botClient.SendPhotoAsync(mess.Chat.Id, "https://cdn.discordapp.com/attachments/916000654227546208/980888940556984431/IMG_20220530_231150.jpg", caption: "Example Part-1");
            await botClient.SendPhotoAsync(mess.Chat.Id, "https://cdn.discordapp.com/attachments/916000654227546208/980888941030957126/IMG_20220530_231202.jpg", caption: "Example Part-2");
            return reply;
        }
        
        public static async Task<Message> sendBuild(ITelegramBotClient botClient, Message mess,Update e)
        {
            if (!database.ContainsKey(mess.From.Id))
            {
                database.Add(mess.From.Id, new Players(mess.From.Id, 0, false));
            }
            Players player = database[e.Message.From.Id];
            string[] messageSplit = mess.Text.Split(" ", 2);
            if (messageSplit.Length == 1)
            {
                player.charName = "Character";
            }
            else
            {
                player.charName = messageSplit[1];
            }
            player.playerName = mess.From.FirstName;
            if (player.photoCount == 0 && player.canSendPhotos == false)
            {
                player.canSendPhotos = true;
                player.photoCount = 1;
                return await botClient.SendTextMessageAsync(mess.Chat.Id, "Please send me 7 different photos of your build with their corresponding caption. If you are not aware of the format then please check /help command.\n\nPLEASE SEND IMAGES AS A FILE AND NOT THE PHOTO.");
            }
            database.Remove(e.Message.From.Id);
            return await botClient.SendTextMessageAsync(mess.Chat.Id, "Previous Building process was interrupted, Please Start Again by sending /build");
        }

        public static async Task<Message> sendTest(ITelegramBotClient botClient, Message mess, Update e)
        {
            Players player = new Players(e.Message.From.Id,0,false);
            MemoryStream output = await processResultImage(player);

            return await botClient.SendPhotoAsync(e.Message.Chat.Id, output);
        }

        public static async Task<Message> processPhoto(ITelegramBotClient botClient, Message mess, Update e)
        {
            if (mess.Caption == null)
            {
                Main.database.Remove(mess.From.Id);
                return await botClient.SendTextMessageAsync(mess.Chat.Id, "No caption found on Photo,Building Process was cancled please send /build to start again,Please refer to /help command for guide!");
            }

            if (!database.ContainsKey(e.Message.From.Id))
            {
                Players pl = new Players(e.Message.From.Id, 0, false);
                Main.database.Add(e.Message.From.Id, pl);
            }

            Players player = database[e.Message.From.Id];

            if (player.photoCount == 1 && player.canSendPhotos == true && e.Message.Caption == "#1")
            {
                Telegram.Bot.Types.File path = await botClient.GetFileAsync(e.Message.Document.FileId);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await botClient.DownloadFileAsync(path.FilePath, memoryStream);
                    player.photo1 = memoryStream.ToArray();
                }
                player.photoCount = 2;
                return await botClient.SendTextMessageAsync(mess.Chat.Id, "Valid! Photo accepted, Now send the Photo #2");
            }
            else if (player.photoCount == 2 && player.canSendPhotos == true && e.Message.Caption == "#2")
            {
                Telegram.Bot.Types.File path = await botClient.GetFileAsync(e.Message.Document.FileId);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await botClient.DownloadFileAsync(path.FilePath, memoryStream);
                    player.photo2 = memoryStream.ToArray();
                }
                player.photoCount = 3;
                return await botClient.SendTextMessageAsync(mess.Chat.Id, "Valid! Photo accepted, Now send the Photo #3");
            }
            else if (player.photoCount == 3 && player.canSendPhotos == true && e.Message.Caption == "#3")
            {
                Telegram.Bot.Types.File path = await botClient.GetFileAsync(e.Message.Document.FileId);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await botClient.DownloadFileAsync(path.FilePath, memoryStream);
                    player.photo3 = memoryStream.ToArray();
                }
                player.photoCount = 4;
                return await botClient.SendTextMessageAsync(mess.Chat.Id, "Valid! Photo accepted, Now send the Photo #4");
            }
            else if (player.photoCount == 4 && player.canSendPhotos == true && e.Message.Caption == "#4")
            {
                Telegram.Bot.Types.File path = await botClient.GetFileAsync(e.Message.Document.FileId);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await botClient.DownloadFileAsync(path.FilePath, memoryStream);
                    player.photo4 = memoryStream.ToArray();
                }
                player.photoCount = 5;
                return await botClient.SendTextMessageAsync(mess.Chat.Id, "Valid! Photo accepted, Now send the Photo #5");
            }
            else if (player.photoCount == 5 && player.canSendPhotos == true && e.Message.Caption == "#5")
            {
                Telegram.Bot.Types.File path = await botClient.GetFileAsync(e.Message.Document.FileId);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await botClient.DownloadFileAsync(path.FilePath, memoryStream);
                    player.photo5 = memoryStream.ToArray();
                }
                player.photoCount = 6;
                return await botClient.SendTextMessageAsync(mess.Chat.Id, "Valid! Photo accepted, Now send the Photo #6");
            }
            else if (player.photoCount == 6 && player.canSendPhotos == true && e.Message.Caption == "#6")
            {
                Telegram.Bot.Types.File path = await botClient.GetFileAsync(e.Message.Document.FileId);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await botClient.DownloadFileAsync(path.FilePath, memoryStream);
                    player.photo6 = memoryStream.ToArray();
                }
                player.photoCount = 7;
                return await botClient.SendTextMessageAsync(mess.Chat.Id, "Valid! Photo accepted, Now send the Photo #7");
            }
            else if (player.photoCount == 7 && player.canSendPhotos == true && e.Message.Caption == "#7")
            {
                Telegram.Bot.Types.File path = await botClient.GetFileAsync(e.Message.Document.FileId);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await botClient.DownloadFileAsync(path.FilePath, memoryStream);
                    player.photo7 = memoryStream.ToArray();
                }
                database.Remove(e.Message.From.Id);
                await botClient.SendTextMessageAsync(mess.Chat.Id, "Good Job! You've successfully sent all the photos, Now please wait for the output.");
                MemoryStream output = await processResultImage(player);
                if (output == null)
                {
                    return await botClient.SendTextMessageAsync(mess.From.Id, "One of the uploaded photo does not match the required resolution: 1920x1080 (16:9 ratio)\nPlease try again");
                }
                return await botClient.SendPhotoAsync(e.Message.Chat.Id, output);
            }
            database.Remove(e.Message.From.Id);
            return await botClient.SendTextMessageAsync(mess.Chat.Id, "Invalid Caption or No Build Process are currently running!");
        }

        public static async Task<MemoryStream> processResultImage(Players player)
        {
            WebClient web = new WebClient();

            Byte[] bgByte = web.DownloadData("https://cdn.discordapp.com/attachments/921046992971522078/980494284224598026/Background.png");
            MemoryStream background = new MemoryStream(bgByte);
            Image bg = Image.Load(background);
            MemoryStream outputStream = new MemoryStream();

            MemoryStream[] streams = new MemoryStream[7]
            {
                new MemoryStream(player.photo1),
                new MemoryStream(player.photo2),
                new MemoryStream(player.photo3),
                new MemoryStream(player.photo4),
                new MemoryStream(player.photo5),
                new MemoryStream(player.photo6),
                new MemoryStream(player.photo7),
            };

            Image[] images = new Image[7];

            for (int i = 0; i < 7; i++)
            {
                images[i] = Image.Load(streams[i]);
                if (!(images[i].Height == 1080 && images[i].Width == 1920 || images[i].Height == 720 && images[i].Width == 1280))
                {
                    return null;
                }
            }
            //1
            images[0].Mutate(i => i.Resize(1920,1080)
              .Crop(new Rectangle(new Point(1450, 110),new Size(350,460))));

            bg.Mutate(o => o
                .DrawImage(images[0], new Point(x: 3 , y: 3), 1f));

            //2
            images[1].Mutate(i => i.Resize(1920, 1080)
              .Crop(new Rectangle(new Point(1450, 110), new Size(350, 503))));

            bg.Mutate(o => o
                .DrawImage(images[1], new Point(x: 3, y: 470), 1f));

            //3
            images[2].Mutate(i => i.Resize(1920, 1080)
              .Crop(new Rectangle(new Point(160, 475), new Size(1250, 440))));

            bg.Mutate(o => o
                .DrawImage(images[2], new Point(x: 3, y: 980), 1f));

            //4
            images[3].Mutate(i => i.Resize(1920, 1080)
              .Crop(new Rectangle(new Point(180, 60), new Size(1200, 970))));

            bg.Mutate(o => o
                .DrawImage(images[3], new Point(x: 355, y: 3), 1f));

            //5
            images[4].Mutate(i => i.Resize(1920, 1080)
              .Crop(new Rectangle(new Point(1450, 130), new Size(350, 412))));

            bg.Mutate(o => o
                .DrawImage(images[4], new Point(x: 1560, y: 3), 1f));

            //6
            images[5].Mutate(i => i.Resize(1920, 1080)
              .Crop(new Rectangle(new Point(1450, 215), new Size(400, 675))));

            bg.Mutate(o => o
                .DrawImage(images[5], new Point(x: 1560, y: 420), 1f));

            //7
            images[6].Mutate(i => i.Resize(1920, 1080)
              .Crop(new Rectangle(new Point(1200, 90), new Size(640, 320))));

            bg.Mutate(o => o
                .DrawImage(images[6], new Point(x: 1260, y: 1100), 1f));
            try
            {
                byte[] calibribyte = web.DownloadData("https://cdn.discordapp.com/attachments/916000654227546208/981148475171434536/calibri.ttf");
                byte[] rockwellbyte = web.DownloadData("https://cdn.discordapp.com/attachments/916000654227546208/981148839597727775/rockwell.ttf");
                MemoryStream[] fonts = new MemoryStream[2] { new MemoryStream(calibribyte),new MemoryStream(rockwellbyte)};
                FontCollection collection = new();
                collection.Add(fonts[0]);
                collection.Add(fonts[1]);

                Font font = new(collection.Get("Calibri"),50,FontStyle.Bold);
                bg.Mutate(x => x.DrawText("@Veebapun", font, Color.LightSkyBlue, new PointF(1270, 1000)));

                Font font1 = new(collection.Get("Rockwell"), 50,FontStyle.Bold);
                bg.Mutate(x => x.DrawText($"{player.playerName}'s {player.charName}", font1, Brushes.Solid(Color.White), Pens.Solid(Color.Black, 1), new PointF(800, 5)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            bg.SaveAsPng(outputStream);
            StreamReader sr = new StreamReader(outputStream);
            sr.BaseStream.Seek(0, SeekOrigin.Begin);

            return outputStream;
        }
    }
}