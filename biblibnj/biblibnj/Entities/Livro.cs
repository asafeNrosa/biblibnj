using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace biblibnj.Entities
{
    [Table("Livros")]
    public class Livro
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Autor { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ISBN { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Editora { get; set; }

        public int? AnoPublicacao { get; set; }

        [Required]
        public int QuantidadeTotal { get; set; }

        [Required]
        public int QuantidadeDisponivel { get; set; }
    }
}
