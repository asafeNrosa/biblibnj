using biblibnj.Context;
using biblibnj.DTOs;
using biblibnj.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace biblibnj.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilaEsperaController : ControllerBase
    {
        private readonly BiblibnjDbContext _context;

        public FilaEsperaController(BiblibnjDbContext context)
        {
            _context = context;
        }

        [HttpPost("entrar")]
        public async Task<ActionResult<PosicaoFilaReadDto>> EntrarNaFila([FromBody] EntradaFilaDto dto)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null) return Unauthorized();
            int usuarioId = int.Parse(usuarioIdClaim);

            var livro = await _context.Livros.FindAsync(dto.LivroId);
            if (livro == null)
            {
                return NotFound(new { mensagem = "Livro não encontrado." });
            }

            if (livro.QuantidadeDisponivel > 0)
            {
                return BadRequest(new { mensagem = "O livro possui exemplares disponíveis para empréstimo direto." });
            }

            var jaEstaNaFila = await _context.FilaEspera
                .AnyAsync(f => f.LivroId == dto.LivroId && f.UsuarioId == usuarioId);

            if (jaEstaNaFila)
            {
                return BadRequest(new { mensagem = "Você já está na fila de espera deste livro." });
            }

            var novaEntrada = new FilaEspera
            {
                LivroId = dto.LivroId,
                UsuarioId = usuarioId,
                DataEntrada = DateTime.Now
            };

            _context.FilaEspera.Add(novaEntrada);
            await _context.SaveChangesAsync();

            int posicao = await _context.FilaEspera
                .CountAsync(f => f.LivroId == dto.LivroId && f.DataEntrada <= novaEntrada.DataEntrada);

            return Ok(new PosicaoFilaReadDto
            {
                LivroId = livro.Id,
                TituloLivro = livro.Titulo,
                Posicao = posicao,
                DataEntrada = novaEntrada.DataEntrada,
                Mensagem = $"Sua reserva foi registrada com sucesso! Posição atual na fila: {posicao}."
            });
        }

        [HttpGet("posicao/{livroId}")]
        public async Task<ActionResult<PosicaoFilaReadDto>> ObterMinhaPosicao(int livroId)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null) return Unauthorized();
            int usuarioId = int.Parse(usuarioIdClaim);

            var registroFila = await _context.FilaEspera
                .Include(f => f.Livro)
                .FirstOrDefaultAsync(f => f.LivroId == livroId && f.UsuarioId == usuarioId);

            if (registroFila == null)
            {
                return NotFound(new { mensagem = "Você não está na fila de espera deste livro." });
            }

            int posicao = await _context.FilaEspera
                .CountAsync(f => f.LivroId == livroId && f.DataEntrada <= registroFila.DataEntrada);

            return Ok(new PosicaoFilaReadDto
            {
                LivroId = registroFila.LivroId,
                TituloLivro = registroFila.Livro?.Titulo ?? string.Empty,
                Posicao = posicao,
                DataEntrada = registroFila.DataEntrada,
                Mensagem = $"Sua posição na fila é: {posicao}."
            });
        }
    }
}