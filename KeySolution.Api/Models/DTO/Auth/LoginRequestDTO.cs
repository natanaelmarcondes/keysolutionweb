namespace KeySolution.Models.DTO.Auth
{
    public class LoginRequestDTO
    {
        public string Email { get; set; } = "";
        public string Senha { get; set; } = "";
        public bool LembrarUsuario { get; set; }
    }
}