using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

public class TelegramAuthService
{
    private readonly string _botToken;

    public TelegramAuthService(IConfiguration configuration)
    {
        _botToken = configuration["TelegramBotToken"] ?? throw new ArgumentNullException("TelegramBotToken is missing");
    }

    public bool ValidateTelegramData(Dictionary<string, string> data)
    {
        if (!data.ContainsKey("hash")) return false;

        var hash = data["hash"];
        data.Remove("hash");

        // Сортировка данных в алфавитном порядке
        var dataString = string.Join("\n", data
            .OrderBy(d => d.Key)
            .Select(d => $"{d.Key}={d.Value}"));

        // Формирование ключа с использованием SHA256 от bot_token
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(_botToken));

        using var hmac = new HMACSHA256(key);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString));
        var computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        return computedHash == hash;
    }
}
