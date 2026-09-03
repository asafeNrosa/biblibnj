using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace biblibnj.Entities
{
    [Table("Usuarios")]
    public class Usuario
    {
            [Key]
            public int Id { get; set; }

            [Required]
            [MaxLength(150)]
            public string Nome { get; set; } = string.Empty;

            [Required]
            [MaxLength(150)]
            public string Email { get; set; } = string.Empty;

            [Required]
            [MaxLength(255)]
            public string SenhaHash { get; set; } = string.Empty;

            [Required]
            [MaxLength(20)]
            public string Perfil { get; set; } = "Comum";

            public DateTime DataCadastro { get; set; } = DateTime.Now;
        
    }
}
