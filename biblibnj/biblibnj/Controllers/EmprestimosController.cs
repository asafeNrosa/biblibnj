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
        private const int PRAZO_DIAS = 7;
        private const int LIMITE_RENOVACOES = 2;
        private const decimal MULTA_POR_DIA = 1.00m;

        private readonly BiblibnjDbContext _context;

        public EmprestimosController(BiblibnjDbContext context)
        {
            _context = context;
        }

        private int ObterUsuarioId()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null)
            {
                throw new UnauthorizedAccessException("Usuário não autenticado.");
            }
            return int.Parse(usuarioIdClaim);
        }

        // Marca como "Atrasado" todo empréstimo "EmAberto" cuja data prevista já passou.
        // Não existe job em segundo plano no projeto, então essa checagem roda
        // no início das ações que dependem de saber o status real e atualizado.
        private async Task AtualizarAtrasadosAsync()
        {
            var atrasados = await _context.Emprestimos
                .Where(e => e.Status == "EmAberto" && e.DataDevolucaoPrevista < DateTime.Now)
                .ToListAsync();

            if (atrasados.Count > 0)
            {
                foreach (var e in atrasados)
                {
                    e.Status = "Atrasado";
                }
                await _context.SaveChangesAsync();
            }
        }

        private static decimal CalcularMultaEstimada(Emprestimo e)
        {
            if (e.Status != "Atrasado") return 0;
            int diasAtraso = (DateTime.Now.Date - e.DataDevolucaoPrevista.Date).Days;
            return diasAtraso > 0 ? diasAtraso * MULTA_POR_DIA : 0;
        }

        private static EmprestimoReadDto ParaDto(Emprestimo e)
        {
            return new EmprestimoReadDto
            {
                Id = e.Id,
                LivroId = e.LivroId,
                TituloLivro = e.Livro?.Titulo ?? string.Empty,
                ISBNLivro = e.Livro?.ISBN ?? string.Empty,
                UsuarioId = e.UsuarioId,
                NomeUsuario = e.Usuario?.Nome ?? string.Empty,
                EmailUsuario = e.Usuario?.Email ?? string.Empty,
                DataEmprestimo = e.DataEmprestimo,
                DataDevolucaoPrevista = e.DataDevolucaoPrevista,
                DataDevolucaoReal = e.DataDevolucaoReal,
                RenovacoesRealizadas = e.RenovacoesRealizadas,
                Status = e.Status,
                MultaEstimada = CalcularMultaEstimada(e)
            };
        }

        [HttpGet("meus")]
        public async Task<ActionResult<IEnumerable<EmprestimoReadDto>>> ObterMeusEmprestimos()
        {
            int usuarioId = ObterUsuarioId();
            await AtualizarAtrasadosAsync();

            var emprestimos = await _context.Emprestimos
                .Include(e => e.Livro)
                .Include(e => e.Usuario)
                .Where(e => e.UsuarioId == usuarioId)
                .OrderByDescending(e => e.DataEmprestimo)
                .ToListAsync();

            return Ok(emprestimos.Select(ParaDto));
        }

        [HttpGet("todos")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<EmprestimoReadDto>>> ObterTodosEmprestimos()
        {
            await AtualizarAtrasadosAsync();

            var emprestimos = await _context.Emprestimos
                .Include(e => e.Livro)
                .Include(e => e.Usuario)
                .Where(e => e.Status == "EmAberto" || e.Status == "Atrasado")
                .OrderBy(e => e.DataDevolucaoPrevista)
                .ToListAsync();

            return Ok(emprestimos.Select(ParaDto));
        }

        // Solicitações aguardando aprovação do admin.
        [HttpGet("pendentes")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<EmprestimoReadDto>>> ObterPendentes()
        {
            var pendentes = await _context.Emprestimos
                .Include(e => e.Livro)
                .Include(e => e.Usuario)
                .Where(e => e.Status == "Pendente")
                .OrderBy(e => e.DataEmprestimo)
                .ToListAsync();

            return Ok(pendentes.Select(ParaDto));
        }

        // Histórico completo (todos os status) - usado na tela de gerenciamento do admin.
        [HttpGet("historico")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<EmprestimoReadDto>>> ObterHistorico()
        {
            await AtualizarAtrasadosAsync();

            var emprestimos = await _context.Emprestimos
                .Include(e => e.Livro)
                .Include(e => e.Usuario)
                .OrderByDescending(e => e.DataEmprestimo)
                .ToListAsync();

            return Ok(emprestimos.Select(ParaDto));
        }

        [HttpPost]
        public async Task<ActionResult<EmprestimoReadDto>> CriarEmprestimo([FromBody] EmprestimoCreateDto dto)
        {
            if (User.IsInRole("Admin"))
            {
                return BadRequest(new { mensagem = "Administradores não podem solicitar empréstimos." });
            }

            int usuarioId = ObterUsuarioId();
            await AtualizarAtrasadosAsync();

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return Unauthorized();

            if (usuario.MultaPendente > 0)
            {
                return BadRequest(new
                {
                    mensagem = $"Você possui uma multa pendente de R$ {usuario.MultaPendente:F2}. " +
                               "Regularize no balcão da biblioteca para voltar a solicitar empréstimos."
                });
            }

            var possuiEmprestimoAtivo = await _context.Emprestimos
                .AnyAsync(e => e.UsuarioId == usuarioId &&
                              (e.Status == "Pendente" || e.Status == "EmAberto" || e.Status == "Atrasado"));

            if (possuiEmprestimoAtivo)
            {
                return BadRequest(new { mensagem = "Você já possui um empréstimo em andamento. Só é permitido 1 livro por vez." });
            }

            var livro = await _context.Livros.FindAsync(dto.LivroId);
            if (livro == null)
            {
                return NotFound(new { mensagem = "Livro não encontrado." });
            }

            if (livro.QuantidadeDisponivel <= 0)
            {
                return BadRequest(new { mensagem = "Livro indisponível no momento. Você pode entrar na Fila de Espera." });
            }

            // O exemplar já é reservado no momento da solicitação, antes da aprovação do admin.
            livro.QuantidadeDisponivel -= 1;

            var novoEmprestimo = new Emprestimo
            {
                UsuarioId = usuarioId,
                LivroId = dto.LivroId,
                DataEmprestimo = DateTime.Now,
                DataDevolucaoPrevista = DateTime.Now.AddDays(PRAZO_DIAS), // recalculada de verdade na aprovação
                Status = "Pendente"
            };

            _context.Emprestimos.Add(novoEmprestimo);
            await _context.SaveChangesAsync();

            novoEmprestimo.Livro = livro;
            novoEmprestimo.Usuario = usuario;

            return CreatedAtAction(nameof(ObterMeusEmprestimos), new { id = novoEmprestimo.Id }, ParaDto(novoEmprestimo));
        }

        [HttpPut("{id}/aprovar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AprovarEmprestimo(int id)
        {
            var emprestimo = await _context.Emprestimos
                .Include(e => e.Livro)
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprestimo == null)
            {
                return NotFound(new { mensagem = "Solicitação de empréstimo não encontrada." });
            }

            if (emprestimo.Status != "Pendente")
            {
                return BadRequest(new { mensagem = "Esta solicitação já foi processada." });
            }

            emprestimo.DataEmprestimo = DateTime.Now;
            emprestimo.DataDevolucaoPrevista = DateTime.Now.AddDays(PRAZO_DIAS);
            emprestimo.Status = "EmAberto";

            await _context.SaveChangesAsync();

            return Ok(ParaDto(emprestimo));
        }

        [HttpPut("{id}/vetar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VetarEmprestimo(int id)
        {
            var emprestimo = await _context.Emprestimos
                .Include(e => e.Livro)
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprestimo == null)
            {
                return NotFound(new { mensagem = "Solicitação de empréstimo não encontrada." });
            }

            if (emprestimo.Status != "Pendente")
            {
                return BadRequest(new { mensagem = "Esta solicitação já foi processada." });
            }

            emprestimo.Status = "Rejeitado";

            // Libera de volta o exemplar que tinha sido reservado na solicitação.
            if (emprestimo.Livro != null)
            {
                emprestimo.Livro.QuantidadeDisponivel += 1;
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Solicitação de empréstimo recusada." });
        }

        [HttpPut("{id}/renovar")]
        public async Task<IActionResult> RenovarEmprestimo(int id)
        {
            int usuarioIdAtual = ObterUsuarioId();
            bool isAdmin = User.IsInRole("Admin");

            var emprestimo = await _context.Emprestimos
                .Include(e => e.Livro)
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprestimo == null)
            {
                return NotFound(new { mensagem = "Empréstimo não encontrado." });
            }

            if (!isAdmin && emprestimo.UsuarioId != usuarioIdAtual)
            {
                return Forbid();
            }

            if (emprestimo.Status != "EmAberto" && emprestimo.Status != "Atrasado")
            {
                return BadRequest(new { mensagem = "Este empréstimo não pode ser renovado." });
            }

            if (emprestimo.RenovacoesRealizadas >= LIMITE_RENOVACOES)
            {
                return BadRequest(new { mensagem = $"Este empréstimo já atingiu o limite de {LIMITE_RENOVACOES} renovações. É necessário devolver o livro." });
            }

            decimal multaGerada = 0;

            // Se já estava atrasado, a multa gerada até agora fica registrada
            // para o usuário mesmo renovando - o prazo só recomeça a contar.
            if (emprestimo.Status == "Atrasado")
            {
                int diasAtraso = (DateTime.Now.Date - emprestimo.DataDevolucaoPrevista.Date).Days;
                multaGerada = diasAtraso > 0 ? diasAtraso * MULTA_POR_DIA : 0;

                if (emprestimo.Usuario != null)
                {
                    emprestimo.Usuario.MultaPendente += multaGerada;
                }
            }

            emprestimo.DataDevolucaoPrevista = emprestimo.DataDevolucaoPrevista > DateTime.Now
                ? emprestimo.DataDevolucaoPrevista.AddDays(PRAZO_DIAS)
                : DateTime.Now.AddDays(PRAZO_DIAS);

            emprestimo.Status = "EmAberto";
            emprestimo.RenovacoesRealizadas += 1;

            await _context.SaveChangesAsync();

            var mensagem = multaGerada > 0
                ? $"Empréstimo renovado. Atenção: R$ {multaGerada:F2} de multa pelo atraso foram registrados na sua conta."
                : "Empréstimo renovado com sucesso.";

            return Ok(new { mensagem, emprestimo = ParaDto(emprestimo) });
        }

        [HttpPut("{id}/devolver")]
        public async Task<IActionResult> DevolverLivro(int id)
        {
            int usuarioIdAtual = ObterUsuarioId();
            bool isAdmin = User.IsInRole("Admin");

            var emprestimo = await _context.Emprestimos
                .Include(e => e.Livro)
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprestimo == null)
            {
                return NotFound(new { mensagem = "Empréstimo não encontrado." });
            }

            if (!isAdmin && emprestimo.UsuarioId != usuarioIdAtual)
            {
                return Forbid();
            }

            if (emprestimo.Status == "Devolvido")
            {
                return BadRequest(new { mensagem = "Este empréstimo já foi devolvido anteriormente." });
            }

            var agora = DateTime.Now;
            emprestimo.DataDevolucaoReal = agora;

            decimal multaGerada = 0;
            if (agora.Date > emprestimo.DataDevolucaoPrevista.Date)
            {
                int diasAtraso = (agora.Date - emprestimo.DataDevolucaoPrevista.Date).Days;
                multaGerada = diasAtraso * MULTA_POR_DIA;

                if (emprestimo.Usuario != null)
                {
                    emprestimo.Usuario.MultaPendente += multaGerada;
                }
            }

            emprestimo.Status = "Devolvido";

            if (emprestimo.Livro != null)
            {
                emprestimo.Livro.QuantidadeDisponivel += 1;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = multaGerada > 0
                    ? $"Devolução registrada com atraso. Multa de R$ {multaGerada:F2} adicionada à sua conta. Regularize no balcão."
                    : "Devolução registrada com sucesso!",
                dataDevolucao = emprestimo.DataDevolucaoReal,
                multaGerada,
                novaQuantidadeDisponivel = emprestimo.Livro?.QuantidadeDisponivel
            });
        }

        // Zera a multa de um usuário depois que ele paga (em dinheiro, no balcão).
        [HttpPut("usuarios/liberar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LiberarUsuario([FromBody] LiberarUsuarioDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
            if (usuario == null)
            {
                return NotFound(new { mensagem = "Usuário não encontrado." });
            }

            usuario.MultaPendente = 0;
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = $"{usuario.Nome} foi liberado(a) e pode voltar a solicitar empréstimos." });
        }

        // Usuários com multa pendente, para a tela do admin liberar.
        [HttpGet("usuarios/com-multa")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObterUsuariosComMulta()
        {
            var usuarios = await _context.Usuarios
                .Where(u => u.MultaPendente > 0)
                .Select(u => new { u.Id, u.Nome, u.Email, u.MultaPendente })
                .ToListAsync();

            return Ok(usuarios);
        }
    }
}