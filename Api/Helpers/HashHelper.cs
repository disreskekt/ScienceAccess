using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Api.Helpers
{
    public static class HashHelper
    {
        public static string GenerateVerySecretHash(this string pass, string email)
        {
            string salt = email.Split('@').First() + pass;
            pass += salt;
            byte[] hash = ComputeHash(pass, new SHA256CryptoServiceProvider());
            return Convert.ToBase64String(hash);
        }
        
        private static byte[] ComputeHash(string input, HashAlgorithm algorithm)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            return algorithm.ComputeHash(inputBytes);
        }
    }
}