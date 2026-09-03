using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace biblibnj.Entities
{
    [Table("FilaEspera")]
    public class FilaEspera
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LivroId { get; set; }

        [ForeignKey(nameof(LivroId))]
        public Livro? Livro { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario? Usuario { get; set; }

        public DateTime DataEntrada { get; set; } = DateTime.Now;

        [Required]
        public int Posicao { get; set; }
    }
}
