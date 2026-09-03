using System.ComponentModel.DataAnnotations;

namespace biblibnj.DTOs
{
    public class LivroReadDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string? Editora { get; set; }
        public int? AnoPublicacao { get; set; }
        public int QuantidadeTotal { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public bool DisponivelParaEmprestimo => QuantidadeDisponivel > 0;
    }

    public class LivroCreateDto
    {
        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O autor é obrigatório.")]
        [StringLength(150)]
        public string Autor { get; set; } = string.Empty;

        [Required(ErrorMessage = "O ISBN é obrigatório.")]
        [StringLength(20)]
        public string ISBN { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Editora { get; set; }

        public int? AnoPublicacao { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quantidade total deve ser maior ou igual a zero.")]
        public int QuantidadeTotal { get; set; }
    }

    public class LivroUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Autor { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Editora { get; set; }

        public int? AnoPublicacao { get; set; }
    }

    public class AjusteEstoqueDto
    {
        [Range(0, int.MaxValue, ErrorMessage = "A nova quantidade total não pode ser negativa.")]
        public int NovaQuantidadeTotal { get; set; }
    }
}