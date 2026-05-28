namespace KeySolution.Api.Models.DTOs
{
    public class FilaSalvarDto
    {
        public long queueid { get; set; }
        public string queuename { get; set; } = string.Empty;
        public string? queuedescription { get; set; }
        public long? siteid { get; set; }
        public string? sendername { get; set; }
        public string? replyaddress { get; set; }
        public long? ciid { get; set; }
    }
}