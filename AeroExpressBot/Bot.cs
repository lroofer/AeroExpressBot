using FileProcessing;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AeroExpressBot;

using static Markup;

public class Bot
{
    private readonly BotOptions _botOptions;
    private readonly Manager _manager;
    private string BotToken { get; }
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is null) return;
        var message = update.Message;
        var username = message.Chat.Username ?? "user_0";
        if (update.Message.Document is not null)
        {
            var document = update.Message.Document;
            if (_manager.TryOpenUserFile(username))
            {
                _ = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: BotOptions.BadOpenFile,
                    cancellationToken: cancellationToken
                );
                return;
            }
            
            var fileId = document.FileId;
            var fileInfo = await botClient.GetFileAsync(fileId, cancellationToken: cancellationToken);
            var filePath = fileInfo.FilePath;
            if (filePath == null)
            {
                _ = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: _botOptions.Error("Telegram didn't process the file"),
                    cancellationToken: cancellationToken
                );
                return;
            }

            string destinationAddress;
            try
            {
                destinationAddress = _manager.GetUserFileName(username, filePath.Split('.')[^1]);
                await using var fileStream = System.IO.File.Create(destinationAddress);
                await botClient.DownloadFileAsync(
                    filePath: filePath,
                    destination: fileStream,
                    cancellationToken: cancellationToken);
                fileStream.Close();
            }
            catch (Exception e)
            {
                Warning($"There's been a error with a file: {e.Message}");
                _ = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: _botOptions.Error("Can't load file"),
                    cancellationToken: cancellationToken
                );
                return;
            }

            if (_manager.ProcessFile(destinationAddress, username, out var msg))
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
                    new KeyboardButton[] { "Sort", "Filter", "View"},
                    new KeyboardButton[] {"Export", "Open another"},
                })
                {
                    ResizeKeyboard = true
                };
                _ = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "File was loaded. How do you want to proceed?",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
            }
            else
            {
                Warning($"[Error downloading file: {msg}]");
                _ = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: _botOptions.Error(msg),
                    cancellationToken: cancellationToken
                );
            }

            return;
        }
        if (message.Text is not { } messageText) return;
        var chatId = message.Chat.Id;
        Item($"Received a '{messageText}' message in chat {chatId}'");
        if (_botOptions.HandleCommand(messageText, username, out var reply, out var replyMarkup))
        {
            _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: reply,
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken
            );
            return;
        } 
        _ = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Unknown command:\n{messageText}",
            cancellationToken: cancellationToken);
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API error:\n [{apiRequestException.ErrorCode}]\n[{apiRequestException.Message}]",
            _ => exception.ToString()
        };
        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
    public async Task StartBot()
    {
        var botClient = new TelegramBotClient(BotToken);

        using CancellationTokenSource cts = new();

        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMeAsync(cancellationToken: cts.Token);

        Success($"Start listening for {me.Username}");
    }

    public Bot(string botToken = "6359168538:AAHbx-fnUR8BlOoTL1MzR9YVTTulDqF9c2w")
    {
        _manager = new Manager();
        _botOptions = new BotOptions(_manager);
        BotToken = botToken;
    }
}