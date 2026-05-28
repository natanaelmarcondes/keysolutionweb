namespace KeySolution.Api.Models
{
    public class Fila
    {
        public long queueid { get; set; }
        public string queuename { get; set; } = string.Empty;
        public string? queuedescription { get; set; }
        public long? siteid { get; set; }
        public string? sendername { get; set; }
        public string? replyaddress { get; set; }
        public long? ciid { get; set; }
        public long tecnicosVinculados { get; set; }
    }
}