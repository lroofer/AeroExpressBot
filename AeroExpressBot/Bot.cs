using FileProcessing;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AeroExpressBot;

public class Bot
{
    private readonly BotOptions _botOptions;
    private readonly Manager _manager;
    private string BotToken { get; }

    public Bot(string botToken = "6359168538:AAHbx-fnUR8BlOoTL1MzR9YVTTulDqF9c2w")
    {
        _manager = new Manager();
        _botOptions = new BotOptions(_manager);
        BotToken = botToken;
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

        _manager.Logger.LogInformation($"Start listening for {me.Username}");
    }

    /// <summary>
    ///  An update handler for queries from Telegram bot.
    /// Telegram bot invokes it.
    /// </summary>
    /// <param name="botClient">Responsive Telegram bot.</param>
    /// <param name="update">Update info.</param>
    /// <param name="cancellationToken"></param>
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        if (update.Message is null) return;
        var message = update.Message;
        var username = message.Chat.Username;
        // Users are distinguished by their username field. 
        if (username == null)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: BotOptions.NoUsername,
                cancellationToken: cancellationToken
            );
            return;
        }

        ReplyKeyboardMarkup basicReplyKeyboardMarkup = new(new[]
        {
            new KeyboardButton[] { "Sort", "Filter", "View" },
            new KeyboardButton[] { "Export", "Open another" },
        })
        {
            ResizeKeyboard = true
        };
        // Check for an attached document.
        if (update.Message.Document is not null)
        {
            var document = update.Message.Document;
            var resOpen = await _manager.TryOpenUserFile(username);
            if (resOpen)
            {
                await botClient.SendTextMessageAsync(
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
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: _botOptions.Error("Telegram didn't process the file"),
                    replyMarkup: new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "Sort", "Filter", "View" },
                        new KeyboardButton[] { "Export", "Open another" },
                    })
                    {
                        ResizeKeyboard = true
                    },
                    cancellationToken: cancellationToken
                );
                return;
            }

            // Try to download the file from Telegram to a temporary location.
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
                _manager.Logger.LogError($"There's been a error with a file: {e.Message}");
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: _botOptions.Error("Can't load file"),
                    replyMarkup: new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "Sort", "Filter", "View" },
                        new KeyboardButton[] { "Export", "Open another" },
                    })
                    {
                        ResizeKeyboard = true
                    },
                    cancellationToken: cancellationToken
                );
                return;
            }

            try
            {
                await _manager.ProcessFile(destinationAddress, username);
                _manager.Logger.LogInformation($"File was loaded to the system: {destinationAddress}");
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "File was loaded. How do you want to proceed?",
                    replyMarkup: basicReplyKeyboardMarkup,
                    cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                _manager.Logger.LogError($"[Error downloading file: {e.Message}]");
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: _botOptions.Error(e.Message),
                    cancellationToken: cancellationToken
                );
            }

            return;
        }

        // Check for any available message.
        if (message.Text is not { } messageText) return;
        var chatId = message.Chat.Id;
        _manager.Logger.LogInformation($"Received a '{messageText}' message in chat {chatId}'");
        string reply;
        IReplyMarkup replyMarkup;
        try
        {
            if (_botOptions.HandleBasicCommand(messageText, username, out reply, out replyMarkup))
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: reply,
                    replyMarkup: replyMarkup,
                    cancellationToken: cancellationToken
                );
                return;
            }
        }
        catch (Exception e)
        {
            _manager.Logger.LogError(_botOptions.Error(e.Message));
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: _botOptions.Error(e.Message),
                replyMarkup: basicReplyKeyboardMarkup,
                cancellationToken: cancellationToken
            );
            return;
        }

        try
        {
            switch (reply)
            {
                case "Exporting to json":
                    await using (var stream1 = await _manager.ExportData(username, "json"))
                    {
                        await botClient.SendDocumentAsync(chatId: chatId,
                            document: InputFile.FromStream(stream: stream1, fileName: $"{username}.json"),
                            caption: reply,
                            replyMarkup: basicReplyKeyboardMarkup,
                            cancellationToken: cancellationToken);
                        stream1.Close();
                        return;
                    }
                case "Exporting to csv":
                    await using (var stream2 = await _manager.ExportData(username, "csv"))
                    {
                        await botClient.SendDocumentAsync(chatId: chatId,
                            document: InputFile.FromStream(stream: stream2, fileName: $"{username}.csv"),
                            caption: reply,
                            replyMarkup: replyMarkup,
                            cancellationToken: cancellationToken);
                        stream2.Close();
                        return;
                    }
            }
        }
        catch (Exception e)
        {
            _manager.Logger.LogError($"Export error: {e.Message}");
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Exporting wasn't completed due technical issues. Try again!",
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);
        }

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Unknown command:\n{messageText}",
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken);
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
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
}