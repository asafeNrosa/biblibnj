using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace biblibnj.Services
{
    public interface IPasswordHasher
    {
        string HashPassword(string senha);
        bool VerifyPassword(string senha, string hashArmazenado);
        bool EstaEmFormatoDeHash(string valor);
    }
}

