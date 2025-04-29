namespace AuthenticationService.Services
{
    public class PasswordHasher
    {
        public string HashPassword(string password)
        {
            // BCrypt handles salting automatically
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                // Handle potential exceptions during verification (e.g., invalid hash format)
                return false;
            }
        }
    }
}