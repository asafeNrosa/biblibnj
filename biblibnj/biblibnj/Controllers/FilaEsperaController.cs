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
                Id = novaEntrada.Id,
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
                Id = registroFila.Id,
                LivroId = registroFila.LivroId,
                TituloLivro = registroFila.Livro?.Titulo ?? string.Empty,
                Posicao = posicao,
                DataEntrada = registroFila.DataEntrada,
                QuantidadeDisponivel = registroFila.Livro?.QuantidadeDisponivel ?? 0,
                Mensagem = $"Sua posição na fila é: {posicao}."
            });
        }

        [HttpGet("minhas")]
        public async Task<IActionResult> ObterMinhasFilas()
        {
            var usuarioId = ObterUsuarioId();

            var filas = await _context.FilaEspera
                .Include(f => f.Livro)
                .Where(f => f.UsuarioId == usuarioId)
                .Select(f => new PosicaoFilaReadDto
                {
                    Id = f.Id,
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

        // Visão administrativa: todas as filas de espera de todos os usuários,
        // para o admin saber quem autorizar assim que um livro fica disponível.
        [HttpGet("todas")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObterTodasAsFilas()
        {
            var filas = await _context.FilaEspera
                .Include(f => f.Livro)
                .Include(f => f.Usuario)
                .OrderBy(f => f.LivroId).ThenBy(f => f.Posicao)
                .Select(f => new PosicaoFilaReadDto
                {
                    Id = f.Id,
                    LivroId = f.LivroId,
                    TituloLivro = f.Livro != null ? f.Livro.Titulo : string.Empty,
                    UsuarioId = f.UsuarioId,
                    NomeUsuario = f.Usuario != null ? f.Usuario.Nome : string.Empty,
                    EmailUsuario = f.Usuario != null ? f.Usuario.Email : string.Empty,
                    Posicao = f.Posicao,
                    DataEntrada = f.DataEntrada,
                    QuantidadeDisponivel = f.Livro != null ? f.Livro.QuantidadeDisponivel : 0,
                    Mensagem = $"Posição {f.Posicao} na fila."
                })
                .ToListAsync();

            return Ok(filas);
        }

        // Admin autoriza diretamente o empréstimo de quem está na vez da fila,
        // sem precisar que o próprio leitor clique em "Solicitar Empréstimo".
        // Já cria o empréstimo liberado (EmAberto), pulando a etapa de aprovação,
        // já que é o próprio admin que está autorizando aqui.
        [HttpPost("autorizar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AutorizarDaFila([FromBody] AutorizarFilaDto dto)
        {
            var registroFila = await _context.FilaEspera
                .Include(f => f.Livro)
                .Include(f => f.Usuario)
                .FirstOrDefaultAsync(f => f.Id == dto.FilaEsperaId);

            if (registroFila == null)
            {
                return NotFound(new { mensagem = "Registro de fila de espera não encontrado." });
            }

            if (registroFila.Livro == null || registroFila.Usuario == null)
            {
                return BadRequest(new { mensagem = "Dados de livro ou usuário inconsistentes para esta fila." });
            }

            if (registroFila.Posicao != 1)
            {
                return BadRequest(new { mensagem = "Só é possível autorizar quem está na posição 1 da fila." });
            }

            if (registroFila.Livro.QuantidadeDisponivel <= 0)
            {
                return BadRequest(new { mensagem = "Não há exemplares disponíveis deste livro no momento." });
            }

            if (registroFila.Usuario.MultaPendente > 0)
            {
                return BadRequest(new { mensagem = $"{registroFila.Usuario.Nome} possui multa pendente e não pode receber novo empréstimo até regularizar." });
            }

            var possuiEmprestimoAtivo = await _context.Emprestimos
                .AnyAsync(e => e.UsuarioId == registroFila.UsuarioId &&
                              (e.Status == "Pendente" || e.Status == "EmAberto" || e.Status == "Atrasado"));

            if (possuiEmprestimoAtivo)
            {
                return BadRequest(new { mensagem = $"{registroFila.Usuario.Nome} já possui um empréstimo em andamento." });
            }

            registroFila.Livro.QuantidadeDisponivel -= 1;

            var novoEmprestimo = new Emprestimo
            {
                UsuarioId = registroFila.UsuarioId,
                LivroId = registroFila.LivroId,
                DataEmprestimo = DateTime.Now,
                DataDevolucaoPrevista = DateTime.Now.AddDays(7),
                Status = "EmAberto"
            };
            _context.Emprestimos.Add(novoEmprestimo);

            _context.FilaEspera.Remove(registroFila);

            var filaRestante = await _context.FilaEspera
                .Where(f => f.LivroId == registroFila.LivroId && f.Posicao > registroFila.Posicao)
                .ToListAsync();

            foreach (var item in filaRestante)
            {
                item.Posicao--;
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = $"Empréstimo autorizado para {registroFila.Usuario.Nome}." });
        }

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