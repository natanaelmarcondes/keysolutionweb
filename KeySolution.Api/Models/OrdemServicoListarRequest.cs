namespace KeySolution.Api.Models
{
    public sealed class OrdemServicoListarRequest
    {
        public long? Numero { get; set; }

        public string? Texto { get; set; }

        public string? FiltroCampo { get; set; }

        public long? FiltroUsuarioId { get; set; }

        public string? FiltroUsuarioNome { get; set; }

        public long? StatusId { get; set; }

        public string? StatusIds { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 18;
    }
}