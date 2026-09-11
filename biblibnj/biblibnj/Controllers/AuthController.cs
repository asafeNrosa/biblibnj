using biblibnj.Context;
using biblibnj.Entities;
using biblibnj.DTOs;
using biblibnj.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace biblibnj.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly BiblibnjDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;

        public AuthController(BiblibnjDbContext context, ITokenService tokenService, IPasswordHasher passwordHasher)
        {
            _context = context;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (usuario == null || !_passwordHasher.VerifyPassword(dto.Senha, usuario.SenhaHash))
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

        [HttpPost("cadastrar")]
        public async Task<ActionResult<LoginResponseDto>> Cadastrar([FromBody] CadastroRequestDto dto)
        {
            var emailJaExiste = await _context.Usuarios
                .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (emailJaExiste)
            {
                return BadRequest(new { mensagem = "Já existe uma conta cadastrada com este e-mail." });
            }

            var novoUsuario = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                SenhaHash = _passwordHasher.HashPassword(dto.Senha),
                Telefone = dto.Telefone,
                Rua = dto.Rua,
                Numero = dto.Numero,
                Cidade = dto.Cidade,
                CEP = dto.CEP,
                Perfil = "Comum"
            };

            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync();

            var token = _tokenService.GerarToken(novoUsuario);

            var resposta = new LoginResponseDto
            {
                Token = token,
                Nome = novoUsuario.Nome,
                Email = novoUsuario.Email,
                Perfil = novoUsuario.Perfil,
                Expiracao = DateTime.UtcNow.AddHours(8)
            };

            return Ok(resposta);
        }

        // Simulação: não envia e-mail/SMS de verdade. Como a senha agora é
        // criptografada (hash de mão única), não é possível "recuperar" a senha
        // original - ninguém consegue, nem o próprio sistema. Em vez disso,
        // gera uma senha temporária nova, já criptografada e salva no lugar da
        // antiga, e devolve essa senha temporária no conteúdo simulado.
        [HttpPost("recuperar-senha")]
        public async Task<ActionResult<RecuperarSenhaResponseDto>> RecuperarSenha([FromBody] RecuperarSenhaRequestDto dto)
        {
            var valor = dto.EmailOuTelefone.Trim();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u =>
                u.Email.ToLower() == valor.ToLower() || u.Telefone == valor);

            if (usuario == null)
            {
                return NotFound(new { mensagem = "Nenhum usuário encontrado com esse e-mail ou telefone." });
            }

            string senhaTemporaria = GerarSenhaTemporaria();
            usuario.SenhaHash = _passwordHasher.HashPassword(senhaTemporaria);
            await _context.SaveChangesAsync();

            bool ehEmail = valor.Contains('@');
            string canal = ehEmail ? "email" : "sms";
            string destino = ehEmail ? usuario.Email : usuario.Telefone;

            var conteudo = $"Olá, {usuario.Nome}! Sua senha temporária na biblibnj é: {senhaTemporaria}. " +
                           "Faça login com ela e altere sua senha em seguida.";

            return Ok(new RecuperarSenhaResponseDto
            {
                Mensagem = $"Simulação de envio: uma mensagem seria enviada por {(ehEmail ? "e-mail" : "SMS")} para {destino}.",
                CanalSimulado = canal,
                ConteudoSimulado = conteudo
            });
        }

        [HttpPut("alterar-senha")]
        [Authorize]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto dto)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null) return Unauthorized();

            int usuarioId = int.Parse(usuarioIdClaim);
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return Unauthorized();

            if (!_passwordHasher.VerifyPassword(dto.SenhaAtual, usuario.SenhaHash))
            {
                return BadRequest(new { mensagem = "A senha atual informada está incorreta." });
            }

            usuario.SenhaHash = _passwordHasher.HashPassword(dto.NovaSenha);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Senha alterada com sucesso." });
        }

        private static string GerarSenhaTemporaria()
        {
            const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var bytes = RandomNumberGenerator.GetBytes(10);
            var chars = new char[10];
            for (int i = 0; i < 10; i++)
            {
                chars[i] = caracteres[bytes[i] % caracteres.Length];
            }
            return new string(chars);
        }
    }
}