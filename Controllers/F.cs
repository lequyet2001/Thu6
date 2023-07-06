using System.Security.Cryptography;

namespace Thu6.Controllers
{
    public class F
    { 

        public string GenerateSecureToken(int length)
        {

            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] tokenBytes = new byte[length];
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes);
        }
        public string GenerateKeyToken()
        {
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] tokenBytes = new byte[5];
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes);
        }

        public string GenerateSecretToken(int length)
        {
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] tokenBytes = new byte[length];
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes);
        }
        public string GenerateSecretKeyToken()
        {
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] tokenBytes = new byte[5];
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes);
        }

        
    }
}
