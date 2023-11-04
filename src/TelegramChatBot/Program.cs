using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var telegramToken = config["token"];

var botClient = new TelegramBotClient(telegramToken);

using CancellationTokenSource cts = new();

ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};

await botClient.DeleteWebhookAsync(true);
botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

cts.Cancel();

Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken token)
{
    Console.WriteLine(exception.Message);

    return Task.CompletedTask;
}

Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
{
    Console.WriteLine($"Handled update: {update.Type}");

    switch (update.Type)
    {
        case UpdateType.Message:
            HandleMessage(update, token);
            break;

    }

    return Task.CompletedTask;
}

async void HandleMessage(Update update, CancellationToken token)
{
    Console.WriteLine($"Handled message: {update.Message?.Text} with type: {update.Message?.Type}");

    if (update.Message?.Text == "markup")
    {
        await SendMarkupAsync(new ChatId(update.Message?.Chat.Id ?? 0l), token);
    }
}

async Task SendMarkupAsync(ChatId chatId, CancellationToken cancellationToken)
{
    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
    {
        new KeyboardButton[] { "Help me" },
        new KeyboardButton[] { "Call me ☎️" },
    })
    {
        ResizeKeyboard = true
    };

    Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Choose a response",
        replyMarkup: replyKeyboardMarkup,
        cancellationToken: cancellationToken);
}