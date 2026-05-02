using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace InfraMonitor.Api.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var report = await _healthCheckService.CheckHealthAsync();
            var response = new
            {
                statusGlobal = report.Status.ToString(),
                tempoTotal = report.TotalDuration.TotalMilliseconds + "ms",
                // Mapeia cada banco individualmente
                verificacoes = report.Entries.Select(e => new
                {
                    componente = e.Key,
                    status = e.Value.Status.ToString(),
                    descricao = e.Value.Description,
                    duracao = e.Value.Duration.TotalMilliseconds + "ms",
                    // Se houver uma exceção
                    detalheErro = e.Value.Exception?.Message
                })
            };
            
            if(report.Status == HealthStatus.Healthy)
                return Ok(response);
            else
                return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }
    }

}
