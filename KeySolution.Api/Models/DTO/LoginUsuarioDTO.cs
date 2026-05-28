namespace KeySolution.Api.Models.DTO
{
    public class LoginUsuarioDTO
    {
        public int usr_codigo { get; set; }
        public long USER_ID { get; set; }

        public string usr_nome { get; set; } = "";
        public string usr_email { get; set; } = "";
        public string usr_nivel { get; set; } = "";
        public string usr_senha_hash { get; set; } = "";

        public string? setor { get; set; }
    }
}