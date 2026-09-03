using System.ComponentModel.DataAnnotations;

namespace biblibnj.DTOs
{
    public class EmprestimoCreateDto
    {
        [Required]
        public int LivroId { get; set; }

        [Required]
        public int UsuarioId { get; set; }
    }

    public class EmprestimoReadDto
    {
        public int Id { get; set; }
        public int LivroId { get; set; }
        public string TituloLivro { get; set; } = string.Empty;
        public string ISBNLivro { get; set; } = string.Empty;
        public DateTime DataEmprestimo { get; set; }
        public DateTime DataDevolucaoPrevista { get; set; }
        public DateTime? DataDevolucaoReal { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}