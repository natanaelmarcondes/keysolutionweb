namespace KeySolution.Api.Models
{
    public sealed class OrdemServicoListItemDto
    {
        public long OsCodigo { get; set; }

        public long OsData { get; set; }

        public string? OsTitulo { get; set; }

        public string? Cliente { get; set; }

        public string? UsuarioAbertura { get; set; }

        public string? UsuarioResponsavel { get; set; }

        public string? Status { get; set; }

        public int? IsRead { get; set; }
    }
}