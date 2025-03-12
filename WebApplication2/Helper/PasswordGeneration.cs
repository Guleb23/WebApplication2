using System.Security.Cryptography;

namespace WebApplication2.Helper
{
    public class PasswordGeneration
    {
        private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        private const string SpecialCharacters = "!@#$%^&*()-_=+[]{};:,.<>?";
        public string GeneratePassword()
        {

            var validChars = "";
            validChars += LowercaseLetters;
            validChars += UppercaseLetters;
            validChars += Digits;
            validChars += SpecialCharacters;

            

            // Используем криптографически безопасный генератор случайных чисел
            using (var rng = new RNGCryptoServiceProvider())
            {
                var result = new char[8];
                var buffer = new byte[sizeof(uint)];

                for (int i = 0; i < 8; i++)
                {
                    rng.GetBytes(buffer); // Заполняем буфер случайными байтами
                    var num = BitConverter.ToUInt32(buffer, 0); // Преобразуем байты в число
                    result[i] = validChars[(int)(num % (uint)validChars.Length)]; // Выбираем символ
                }

                return new string(result);
            }
        }
    }
}
