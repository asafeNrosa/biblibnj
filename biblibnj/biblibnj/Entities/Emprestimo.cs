using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace biblibnj.Entities
{

    [Table("Emprestimos")]
    public class Emprestimo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario? Usuario { get; set; }

        [Required]
        public int LivroId { get; set; }

        [ForeignKey(nameof(LivroId))]
        public Livro? Livro { get; set; }

        public DateTime DataEmprestimo { get; set; } = DateTime.Now;

        [Required]
        public DateTime DataDevolucaoPrevista { get; set; }

        public DateTime? DataDevolucaoReal { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "EmAberto";
    }
}
