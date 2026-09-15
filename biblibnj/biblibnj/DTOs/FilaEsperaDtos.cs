using System.ComponentModel.DataAnnotations;

namespace biblibnj.DTOs
{
    public class EntradaFilaDto
    {
        [Required]
        public int LivroId { get; set; }
    }

    public class PosicaoFilaReadDto
    {
        public int Id { get; set; }
        public int LivroId { get; set; }
        public string TituloLivro { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string EmailUsuario { get; set; } = string.Empty;
        public int Posicao { get; set; }
        public DateTime DataEntrada { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }

    public class AutorizarFilaDto
    {
        [Required]
        public int FilaEsperaId { get; set; }
    }
}