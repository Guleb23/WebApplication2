using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebApplication2.Models;
using WebApplication2.Database;
using System.Security.Cryptography;

public class TelegramBotService : BackgroundService
{
    private readonly TelegramBotClient _botClient;
    private readonly IServiceScopeFactory _scopeFactory;

    public TelegramBotService(IServiceScopeFactory scopeFactory)
    {
        _botClient = new TelegramBotClient("7928217386:AAEuHVvG0HKLTVbKWWaF-EKH68E_RfXealI");  // Замените на ваш токен
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var me = await _botClient.GetMeAsync(stoppingToken);
        Console.WriteLine($"✅ Бот @{me.Username} запущен");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()  // Разрешаем все обновления
        };

        // Запускаем приём сообщений
        await _botClient.ReceiveAsync(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: stoppingToken
        );
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            // Если это сообщение с командой /start
            if (update.Message?.Text == "/registration")
            {
                var chatId = update.Message.Chat.Id;

                // Отправляем сообщение с кнопкой для запроса контакта
                var keyboard = new ReplyKeyboardMarkup(
                    new[] { new KeyboardButton[] { new KeyboardButton("Отправить номер телефона") { RequestContact = true } } })
                {
                    OneTimeKeyboard = true,
                    ResizeKeyboard = true
                };

                Console.WriteLine("📲 Кнопка отправки номера телефона отправлена.");

                await bot.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Привет! Пожалуйста, отправьте свой номер телефона для авторизации:",
                    replyMarkup: keyboard,
                    cancellationToken: token
                );
            }

            // Если сообщение содержит контакт
            else if (update.Message?.Contact != null)
            {
                var contact = update.Message.Contact;
                var phoneNumber = contact.PhoneNumber;
                var chatId = update.Message.Chat.Id;

                // ✅ Нормализация номера: добавляем + если его нет
                if (!phoneNumber.StartsWith("+"))
                    phoneNumber = "+" + phoneNumber;

                Console.WriteLine($"📲 Пользователь отправил номер телефона: {phoneNumber} (chat_id: {chatId})");

                // Проверка, зарегистрирован ли пользователь в базе данных
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Phone == phoneNumber);

                if (existingUser != null)
                {
                    await bot.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Вы уже зарегистрированы, ваш пароль {existingUser.Password} и логин {existingUser.Phone}",
                        cancellationToken: token
                    );
                }
                else
                {
                    var password = GenerateRandomPassword();
                    UserModel userModel = new UserModel()
                    {
                        FirstName = "",
                        LastName = "",
                        Email = "",
                        Phone = phoneNumber,
                        Password = password,
                        RoleId = 1, 
                        PaymentMethodId = 1,
                        GetDocsSposobId = 1,
                    };

                    _dbContext.Users.Add(userModel);
                    await _dbContext.SaveChangesAsync();

                    PersonalDataModel pesonal = new PersonalDataModel()
                    {
                        Seria = "",
                        Nomer = "",
                        SNILS = "",
                        DateVidachi = "",
                        Propiska = "",
                        WhoVidal = "",
                        UserId = userModel.Id,
                    };

                    _dbContext.PersonalData.Add(pesonal);
                    await _dbContext.SaveChangesAsync();

                    await bot.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Регистрация прошла успешно!\nВаш логин: {userModel.Phone}\nВаш пароль: {password}",
                        cancellationToken: token
                    );
                }
            }
            else
            {
                var chatId = update.Message?.Chat.Id;

                // Если это не контакт, просто игнорируем или обрабатываем другие типы сообщений
                if (chatId != null)
                {
                    Console.WriteLine($"📝 Не контакт, а текстовое сообщение: {update.Message?.Text}");
                    await bot.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Нажмите на кнопку ниже, чтобы отправить свой номер телефона для авторизации.",
                        cancellationToken: token
                    );
                }
            }

        }
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"❌ Ошибка Telegram бота: {exception.Message}");
        return Task.CompletedTask;
    }

    // Метод для генерации случайного пароля
    private string GenerateRandomPassword()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            string Digits = "0123456789";
            var result = new char[4];
            var buffer = new byte[sizeof(uint)];

            for (int i = 0; i < 4; i++)
            {
                rng.GetBytes(buffer);
                var num = BitConverter.ToUInt32(buffer, 0);
                result[i] = Digits[(int)(num % (uint)Digits.Length)];
            }

            return new string(result);
        }
    }
}