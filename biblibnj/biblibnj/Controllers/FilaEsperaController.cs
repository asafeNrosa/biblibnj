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

        // Extrai o Id do usuário logado a partir do token JWT.
        // O [Authorize] no topo da classe já garante que só chega aqui
        // requisição autenticada, então a claim sempre deve existir.
        private int ObterUsuarioId()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null)
            {
                throw new UnauthorizedAccessException("Usuário não autenticado.");
            }
            return int.Parse(usuarioIdClaim);
        }

        [HttpPost("entrar")]
        public async Task<ActionResult<PosicaoFilaReadDto>> EntrarNaFila([FromBody] EntradaFilaDto dto)
        {
            if (User.IsInRole("Admin"))
            {
                return BadRequest(new { mensagem = "Administradores não podem entrar na fila de espera." });
            }

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

            int posicao = await _context.FilaEspera
                .CountAsync(f => f.LivroId == dto.LivroId) + 1;

            var novaEntrada = new FilaEspera
            {
                LivroId = dto.LivroId,
                UsuarioId = usuarioId,
                DataEntrada = DateTime.Now,
                Posicao = posicao
            };

            _context.FilaEspera.Add(novaEntrada);
            await _context.SaveChangesAsync();

            return Ok(new PosicaoFilaReadDto
            {
                LivroId = livro.Id,
                TituloLivro = livro.Titulo,
                Posicao = posicao,
                DataEntrada = novaEntrada.DataEntrada,
                QuantidadeDisponivel = livro.QuantidadeDisponivel,
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
                QuantidadeDisponivel = registroFila.Livro?.QuantidadeDisponivel ?? 0,
                Mensagem = $"Sua posição na fila é: {posicao}."
            });
        }

        // 1. Obter todas as filas em que o usuário atual está inserido
        [HttpGet("minhas")]
        public async Task<IActionResult> ObterMinhasFilas()
        {
            var usuarioId = ObterUsuarioId(); // Método auxiliar para extrair do Token JWT

            var filas = await _context.FilaEspera
                .Include(f => f.Livro)
                .Where(f => f.UsuarioId == usuarioId)
                .Select(f => new PosicaoFilaReadDto
                {
                    LivroId = f.LivroId,
                    TituloLivro = f.Livro != null ? f.Livro.Titulo : string.Empty,
                    Posicao = f.Posicao,
                    DataEntrada = f.DataEntrada,
                    QuantidadeDisponivel = f.Livro != null ? f.Livro.QuantidadeDisponivel : 0,
                    Mensagem = $"Você está na posição {f.Posicao} da fila."
                })
                .ToListAsync();

            return Ok(filas);
        }

        // 2. Remover o usuário da fila de espera
        [HttpDelete("sair/{livroId}")]
        public async Task<IActionResult> SairDaFila(int livroId)
        {
            var usuarioId = ObterUsuarioId();

            var registroFila = await _context.FilaEspera
                .FirstOrDefaultAsync(f => f.LivroId == livroId && f.UsuarioId == usuarioId);

            if (registroFila == null)
            {
                return NotFound(new { mensagem = "Você não está na fila de espera deste livro." });
            }

            _context.FilaEspera.Remove(registroFila);
            await _context.SaveChangesAsync();

            // Reordena as posições dos leitores restantes na fila do mesmo livro
            var filaRestante = await _context.FilaEspera
                .Where(f => f.LivroId == livroId && f.Posicao > registroFila.Posicao)
                .ToListAsync();

            foreach (var item in filaRestante)
            {
                item.Posicao--;
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Você saiu da fila de espera com sucesso." });
        }

    }
}