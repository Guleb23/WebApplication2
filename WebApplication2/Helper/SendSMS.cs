using System;

namespace WebApplication2.Helper
{
    public class SendSMS
    {
        private readonly HttpClient _httpClient;
        public SendSMS(HttpClient httpClient)
        {
            httpClient = new HttpClient();
            _httpClient = httpClient;
        }

        public async Task<bool> SendSmsAsync(string phone, string password)
        {
          
                try
                {
                    string login = "Guleb23";
                    string pass = "20033002Tim.";
                    string path = $"https://smsc.ru/sys/send.php?login={login}&psw={pass}&phones={phone}&mes=Ваш пароль для входа в личный кабинет {password}";
                    var response = await _httpClient.GetAsync(path);
                    response.EnsureSuccessStatusCode();
                    return true;
                }
                catch (TaskCanceledException ex)
                {
                    Console.WriteLine($"Запрос был отменён: {ex.Message}");
                    return false;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Ошибка при отправке SMS: {ex.Message}");
                    return false;
                }
            }
    }
}
