namespace KeySolution.Api.Models
{
    public sealed class OrdemServicoDetalheDto
    {
        public OrdemServicoDto OrdemServico { get; set; } = new();

        public string? AberturaNome { get; set; }

        public string? ResponsavelNome { get; set; }

        public string? ResponsavelSetor { get; set; }

        public long? ResponsavelQueueId { get; set; }

        public string? ClienteNome { get; set; }

        public string? ResolucaoAtual { get; set; }

        public List<OrdemServicoHistoricoDto> Historicos { get; set; } = new();
    }

    public sealed class OrdemServicoDto
    {
        public long Workorderid { get; set; }

        public long Requesterid { get; set; }

        public long? Createdbyid { get; set; }

        public long Createdtime { get; set; }

        public string? Title { get; set; }

        public long Ownerid { get; set; }

        public long Statusid { get; set; }

        public string? Statusname { get; set; }

        public string? Description { get; set; }

        public string? Fulldescription { get; set; }
    }

    public sealed class OrdemServicoHistoricoDto
    {
        public long Historyid { get; set; }

        public long Workorderid { get; set; }

        public long Operationownerid { get; set; }

        public long Operationtime { get; set; }

        public string? Description { get; set; }

        public string? Operation { get; set; }

        public string? UsuarioNome { get; set; }
    }
}