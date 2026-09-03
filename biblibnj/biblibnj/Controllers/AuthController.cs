using biblibnj.Context;
using biblibnj.Entities;
using biblibnj.DTOs;
using biblibnj.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace biblibnj.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly BiblibnjDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(BiblibnjDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (usuario == null || usuario.SenhaHash != dto.Senha)
            {
                return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });
            }

            var token = _tokenService.GerarToken(usuario);

            var resposta = new LoginResponseDto
            {
                Token = token,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Perfil = usuario.Perfil,
                Expiracao = DateTime.UtcNow.AddHours(8)
            };

            return Ok(resposta);
        }
    }
}