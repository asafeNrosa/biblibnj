using biblibnj.Context;
using biblibnj.DTOs;
using biblibnj.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace biblibnj.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LivrosController : ControllerBase
    {
        private readonly BiblibnjDbContext _context;

        public LivrosController(BiblibnjDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<LivroReadDto>>> GetLivros([FromQuery] string? busca)
        {
            var query = _context.Livros.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                busca = busca.ToLower();
                query = query.Where(l => l.Titulo.ToLower().Contains(busca) ||
                                         l.Autor.ToLower().Contains(busca) ||
                                         l.ISBN.Contains(busca));
            }

            var livros = await query.Select(l => new LivroReadDto
            {
                Id = l.Id,
                Titulo = l.Titulo,
                Autor = l.Autor,
                ISBN = l.ISBN,
                Editora = l.Editora,
                AnoPublicacao = l.AnoPublicacao,
                QuantidadeTotal = l.QuantidadeTotal,
                QuantidadeDisponivel = l.QuantidadeDisponivel
            }).ToListAsync();

            return Ok(livros);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LivroReadDto>> GetLivroPorId(int id)
        {
            var livro = await _context.Livros.FindAsync(id);

            if (livro == null)
            {
                return NotFound(new { mensagem = "Livro não encontrado no acervo." });
            }

            var dto = new LivroReadDto
            {
                Id = livro.Id,
                Titulo = livro.Titulo,
                Autor = livro.Autor,
                ISBN = livro.ISBN,
                Editora = livro.Editora,
                AnoPublicacao = livro.AnoPublicacao,
                QuantidadeTotal = livro.QuantidadeTotal,
                QuantidadeDisponivel = livro.QuantidadeDisponivel
            };

            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<LivroReadDto>> CadastrarLivro([FromBody] LivroCreateDto dto)
        {
            var isbnExistente = await _context.Livros.AnyAsync(l => l.ISBN == dto.ISBN);
            if (isbnExistente)
            {
                return BadRequest(new { mensagem = "Já existe um livro cadastrado com este ISBN." });
            }

            var novoLivro = new Livro
            {
                Titulo = dto.Titulo,
                Autor = dto.Autor,
                ISBN = dto.ISBN,
                Editora = dto.Editora,
                AnoPublicacao = dto.AnoPublicacao,
                QuantidadeTotal = dto.QuantidadeTotal,
                QuantidadeDisponivel = dto.QuantidadeTotal
            };

            _context.Livros.Add(novoLivro);
            await _context.SaveChangesAsync();

            var retornoDto = new LivroReadDto
            {
                Id = novoLivro.Id,
                Titulo = novoLivro.Titulo,
                Autor = novoLivro.Autor,
                ISBN = novoLivro.ISBN,
                Editora = novoLivro.Editora,
                AnoPublicacao = novoLivro.AnoPublicacao,
                QuantidadeTotal = novoLivro.QuantidadeTotal,
                QuantidadeDisponivel = novoLivro.QuantidadeDisponivel
            };

            return CreatedAtAction(nameof(GetLivroPorId), new { id = novoLivro.Id }, retornoDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AtualizarLivro(int id, [FromBody] LivroUpdateDto dto)
        {
            var livro = await _context.Livros.FindAsync(id);

            if (livro == null)
            {
                return NotFound(new { mensagem = "Livro não encontrado para atualização." });
            }

            livro.Titulo = dto.Titulo;
            livro.Autor = dto.Autor;
            livro.Editora = dto.Editora;
            livro.AnoPublicacao = dto.AnoPublicacao;

            _context.Entry(livro).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}/estoque")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AjustarEstoque(int id, [FromBody] AjusteEstoqueDto dto)
        {
            var livro = await _context.Livros.FindAsync(id);

            if (livro == null)
            {
                return NotFound(new { mensagem = "Livro não encontrado." });
            }

            int diferenca = dto.NovaQuantidadeTotal - livro.QuantidadeTotal;
            int novaQuantidadeDisponivel = livro.QuantidadeDisponivel + diferenca;

            if (novaQuantidadeDisponivel < 0)
            {
                return BadRequest(new { mensagem = "A quantidade total não pode ser menor que o total de exemplares atualmente emprestados." });
            }

            livro.QuantidadeTotal = dto.NovaQuantidadeTotal;
            livro.QuantidadeDisponivel = novaQuantidadeDisponivel;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Estoque atualizado com sucesso.",
                quantidadeTotal = livro.QuantidadeTotal,
                quantidadeDisponivel = livro.QuantidadeDisponivel
            });
        }
    }
}