namespace KeySolution.Api.Models.DTO.Usuarios
{
    public class UsuarioSalvarDTO
    {
        public long USER_ID { get; set; }

        public string usr_nome { get; set; } = "";

        public string usr_email { get; set; } = "";

        public string usr_nivel { get; set; } = "";

        public int set_codigo { get; set; }

        public long? QUEUEID { get; set; }

        public string? Senha { get; set; }
    }
}
