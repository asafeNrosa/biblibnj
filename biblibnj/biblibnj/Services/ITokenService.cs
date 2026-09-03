using biblibnj.Entities;

namespace biblibnj.Services
{
    public interface ITokenService
    {
       string GerarToken(Usuario usuario);
    }
}
