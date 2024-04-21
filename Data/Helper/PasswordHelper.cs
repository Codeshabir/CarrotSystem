using System;
using System.Security.Cryptography;
using System.Text;

namespace CarrotSystem.Helper
{
    public static class PasswordHelper
    {
        public static string Hash(string password, string userid = "")
        {
            const string salt = "cf78d53e41c741c39fa7ba7677d981b47c7b349a36da4733b3e3010e57f8a6b70cbff83e8f814a6da8162b100115d74adf8a6bf6132a464d876b5a243a7fb697";
            const string salt2 = "00bc48a248aa4319b27a2e085995908fe46fce65c22e4b2898df43e25f02e7e8eef20568c0ea4400bf8409445f693e3b33a892b36873473d81dd94a43afaef50";

            var toBeHashed = BitConverter.ToString(new SHA512Managed().ComputeHash(Encoding.Unicode.GetBytes(password + salt))).Replace("-", "");
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(toBeHashed, Encoding.Unicode.GetBytes(salt2), 250))
            {
                return Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(32));
            }
        }

        public static string GetPasswordHash(string password)
        {
            return Convert.ToBase64String(new SHA512Managed().ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        public static bool Verify(string password, string hashedPassword)
        {
            return (hashedPassword.Equals(GetPasswordHash(password)));
        }
    }
}