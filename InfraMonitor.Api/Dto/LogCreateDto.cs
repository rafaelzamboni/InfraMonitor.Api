namespace InfraMonitor.Api.Dto
{
    public class LogCreateDto
    {
        public string Servidor { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
    }
}
