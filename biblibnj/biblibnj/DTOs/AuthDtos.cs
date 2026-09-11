using System.ComponentModel.DataAnnotations;

namespace biblibnj.DTOs
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Senha { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty;
        public DateTime Expiracao { get; set; }
    }
    public class CadastroRequestDto
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [MaxLength(20)]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "A rua é obrigatória.")]
        [MaxLength(200)]
        public string Rua { get; set; } = string.Empty;

        [Required(ErrorMessage = "O número é obrigatório.")]
        [MaxLength(20)]
        public string Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "A cidade é obrigatória.")]
        [MaxLength(100)]
        public string Cidade { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CEP é obrigatório.")]
        [MaxLength(10)]
        public string CEP { get; set; } = string.Empty;
    }

    public class RecuperarSenhaRequestDto
    {
        [Required(ErrorMessage = "Informe o e-mail ou telefone cadastrado.")]
        public string EmailOuTelefone { get; set; } = string.Empty;
    }

    public class RecuperarSenhaResponseDto
    {
        public string Mensagem { get; set; } = string.Empty;
        public string CanalSimulado { get; set; } = string.Empty; // "email" ou "sms"
        public string ConteudoSimulado { get; set; } = string.Empty;
    }

    public class AlterarSenhaDto
    {
        [Required(ErrorMessage = "Informe a senha atual.")]
        public string SenhaAtual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a nova senha.")]
        [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
        public string NovaSenha { get; set; } = string.Empty;
    }

}