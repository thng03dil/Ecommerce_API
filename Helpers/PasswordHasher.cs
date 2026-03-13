namespace Ecommerce_API.Helpers
{
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public static bool Verify(string password,string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash)) return false;
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
