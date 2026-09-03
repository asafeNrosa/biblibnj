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
        public int LivroId { get; set; }
        public string TituloLivro { get; set; } = string.Empty;
        public int Posicao { get; set; }
        public DateTime DataEntrada { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }
}