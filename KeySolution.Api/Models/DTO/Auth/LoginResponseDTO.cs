namespace KeySolution.Models.DTO.Auth
{
    public class LoginResponseDTO
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = "";

        public int UsuarioCodigo { get; set; }
        public long UserId { get; set; }

        public string Nome { get; set; } = "";
        public string Email { get; set; } = "";
        public string Nivel { get; set; } = "";
        public string Setor { get; set; } = "";

        public List<string> Queues { get; set; } = new();
    }
}