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
        [MaxLength(20)]

        public string Telefone { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Rua { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Numero { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Cidade { get; set; } = string.Empty;

        [MaxLength(10)]
        public string CEP { get; set; } = string.Empty;
        public decimal MultaPendente { get; set; } = 0;
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }
}