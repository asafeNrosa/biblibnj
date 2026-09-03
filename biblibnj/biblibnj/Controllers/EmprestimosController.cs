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
    public class EmprestimosController : ControllerBase
    {
        private readonly BiblibnjDbContext _context;

        public EmprestimosController(BiblibnjDbContext context)
        {
            _context = context;
        }

        [HttpGet("meus")]
        public async Task<ActionResult<IEnumerable<EmprestimoReadDto>>> ObterMeusEmprestimos()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null) return Unauthorized();

            int usuarioId = int.Parse(usuarioIdClaim);

            var emprestimos = await _context.Emprestimos
                .Include(e => e.Livro)
                .Where(e => e.UsuarioId == usuarioId)
                .OrderByDescending(e => e.DataEmprestimo)
                .Select(e => new EmprestimoReadDto
                {
                    Id = e.Id,
                    LivroId = e.LivroId,
                    TituloLivro = e.Livro!.Titulo,
                    ISBNLivro = e.Livro.ISBN,
                    DataEmprestimo = e.DataEmprestimo,
                    DataDevolucaoPrevista = e.DataDevolucaoPrevista,
                    DataDevolucaoReal = e.DataDevolucaoReal,
                    Status = e.Status
                })
                .ToListAsync();

            return Ok(emprestimos);
        }

        [HttpGet("todos")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<EmprestimoReadDto>>> ObterTodosEmprestimos()
        {
            var emprestimos = await _context.Emprestimos
                .Include(e => e.Livro)
                .OrderByDescending(e => e.DataEmprestimo)
                .Select(e => new EmprestimoReadDto
                {
                    Id = e.Id,
                    LivroId = e.LivroId,
                    TituloLivro = e.Livro!.Titulo,
                    ISBNLivro = e.Livro.ISBN,
                    DataEmprestimo = e.DataEmprestimo,
                    DataDevolucaoPrevista = e.DataDevolucaoPrevista,
                    DataDevolucaoReal = e.DataDevolucaoReal,
                    Status = e.Status
                })
                .ToListAsync();

            return Ok(emprestimos);
        }

        [HttpPost]
        public async Task<ActionResult<EmprestimoReadDto>> CriarEmprestimo([FromBody] EmprestimoCreateDto dto)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null) return Unauthorized();
            int usuarioLogadoId = int.Parse(usuarioIdClaim);

            int usuarioIdFinal = User.IsInRole("Admin") ? dto.UsuarioId : usuarioLogadoId;

            var livro = await _context.Livros.FindAsync(dto.LivroId);
            if (livro == null)
            {
                return NotFound(new { mensagem = "Livro não encontrado." });
            }

            if (livro.QuantidadeDisponivel <= 0)
            {
                return BadRequest(new { mensagem = "Livro indisponível no momento. Você pode entrar na Fila de Espera." });
            }

            var possuiEmprestimoAtivo = await _context.Emprestimos
                .AnyAsync(e => e.UsuarioId == usuarioIdFinal && e.LivroId == dto.LivroId && e.Status == "EmAberto");

            if (possuiEmprestimoAtivo)
            {
                return BadRequest(new { mensagem = "Você já possui um exemplar deste livro em aberto." });
            }

            var novoEmprestimo = new Emprestimo
            {
                UsuarioId = usuarioIdFinal,
                LivroId = dto.LivroId,
                DataEmprestimo = DateTime.Now,
                DataDevolucaoPrevista = DateTime.Now.AddDays(14), // Prazo padrão: 14 dias
                Status = "EmAberto"
            };

            livro.QuantidadeDisponivel -= 1;

            _context.Emprestimos.Add(novoEmprestimo);
            await _context.SaveChangesAsync();

            var retornoDto = new EmprestimoReadDto
            {
                Id = novoEmprestimo.Id,
                LivroId = livro.Id,
                TituloLivro = livro.Titulo,
                ISBNLivro = livro.ISBN,
                DataEmprestimo = novoEmprestimo.DataEmprestimo,
                DataDevolucaoPrevista = novoEmprestimo.DataDevolucaoPrevista,
                Status = novoEmprestimo.Status
            };

            return CreatedAtAction(nameof(ObterMeusEmprestimos), new { id = novoEmprestimo.Id }, retornoDto);
        }

        [HttpPut("{id}/devolver")]
        public async Task<IActionResult> DevolverLivro(int id)
        {
            var emprestimo = await _context.Emprestimos
                .Include(e => e.Livro)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprestimo == null)
            {
                return NotFound(new { mensagem = "Empréstimo não encontrado." });
            }

            if (emprestimo.Status == "Devolvido")
            {
                return BadRequest(new { mensagem = "Este empréstimo já foi devolvido anteriormente." });
            }

            emprestimo.DataDevolucaoReal = DateTime.Now;
            emprestimo.Status = "Devolvido";

            if (emprestimo.Livro != null)
            {
                emprestimo.Livro.QuantidadeDisponivel += 1;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Devolução registrada com sucesso!",
                dataDevolucao = emprestimo.DataDevolucaoReal,
                novaQuantidadeDisponivel = emprestimo.Livro?.QuantidadeDisponivel
            });
        }
    }
}