using System.Security.Cryptography;

namespace biblibnj.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int ITERACOES = 100_000;
        private const int TAMANHO_SALT = 16;
        private const int TAMANHO_HASH = 32;

        public string HashPassword(string senha)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(TAMANHO_SALT);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password: senha,
                salt: salt,
                iterations: ITERACOES,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: TAMANHO_HASH);

            return $"{ITERACOES}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool VerifyPassword(string senha, string hashArmazenado)
        {
            if (!EstaEmFormatoDeHash(hashArmazenado))
            {
                return false;
            }

            var partes = hashArmazenado.Split('.');
            int iteracoes = int.Parse(partes[0]);
            byte[] salt = Convert.FromBase64String(partes[1]);
            byte[] hashEsperado = Convert.FromBase64String(partes[2]);

            byte[] hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
                password: senha,
                salt: salt,
                iterations: iteracoes,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: hashEsperado.Length);

            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
        }

        public bool EstaEmFormatoDeHash(string valor)
        {
            var partes = valor.Split('.');
            return partes.Length == 3 && int.TryParse(partes[0], out _);
        }
    }
}

