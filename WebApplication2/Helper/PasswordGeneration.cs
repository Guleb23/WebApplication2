using System.Security.Cryptography;

namespace WebApplication2.Helper
{
    public class PasswordGeneration
    {
        private const string Digits = "0123456789";

        public string GeneratePassword()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
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
}