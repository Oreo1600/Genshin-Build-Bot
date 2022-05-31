using buildBot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace buildBot
{
    internal class Handlers
    {
        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            Console.WriteLine(exception.StackTrace);
            return Task.CompletedTask;
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!,update),
                /*UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),*/
                /*UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),*/
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message,Update update)
        {
            if (message.Type == MessageType.Document)
            {
                await Main.processPhoto(botClient, message, update);
            }
            if (message.Type != MessageType.Text)
                return;
            string messageText = message.Text;
            Console.WriteLine($"Received a Message from {message.From.FirstName}. Says: {messageText}");            

            if (message.Text.Contains('@'))
            {
                string firstIndex;
                string username;
                int startsAt;
                int endAt;
                if (message.Text.Contains(' '))
                {
                    firstIndex = message.Text.Split(" ")[0];
                    username = firstIndex.Split("@")[1];

                    if (username != Program.me.Username)
                    {
                        return;
                    }
                    startsAt = firstIndex.IndexOf('@');
                    endAt = username.Length + 1;
                    messageText = firstIndex.Remove(startsAt, endAt) + " " + message.Text.Split(" ")[1];
                }
                else
                {
                    username = messageText.Split("@")[1];
                    if (username != Program.me.Username)
                    {
                        return;
                    }
                    startsAt = messageText.IndexOf('@');
                    endAt = username.Length + 1;
                    messageText = messageText.Remove(startsAt, endAt);
                }
            }

            var action = messageText!.Split(' ')[0] switch
            {
                "/start" => Main.sendStart(botClient, message),
                "/help" => Main.sendHelp(botClient, message),
                "/build" => Main.sendBuild(botClient, message,update),
                "/test" => Main.sendTest(botClient,message,update),
                
                _ => notImplement(),
            };

        }

        //yet to Implement
        /* private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
         {

         }*/
        
        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }
        public static Task notImplement()
        {
            return Task.CompletedTask;
        }
    }
}
