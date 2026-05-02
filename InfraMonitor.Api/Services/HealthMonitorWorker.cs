using InfraMonitor.Api.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace InfraMonitor.Api.Services;

public class HealthMonitorWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HealthMonitorWorker> _logger;
    private readonly SmtpSettings _smtpSettings;

    private bool _sistemaEstavaSaudavel = true;

    public HealthMonitorWorker(
        IServiceProvider serviceProvider,
        ILogger<HealthMonitorWorker> logger,
        IOptions<SmtpSettings> smtpSettings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _smtpSettings = smtpSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Serviço de Monitoramento de E-mail iniciado com sucesso.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var healthService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();
                var report = await healthService.CheckHealthAsync(stoppingToken);

                if (report.Status != HealthStatus.Healthy && _sistemaEstavaSaudavel)
                {
                    _logger.LogWarning("Falha detectada! Enviando alerta de INCIDENTE...");
                    await EnviarEmailAlertaAsync(report, "INCIDENTE");

                    _sistemaEstavaSaudavel = false; // Atualiza a memória: agora o sistema está quebrado
                }
              
                else if (report.Status == HealthStatus.Healthy && !_sistemaEstavaSaudavel)
                {
                    _logger.LogInformation("Sistema recuperado! Enviando alerta de RESOLUÇÃO...");
                    await EnviarEmailAlertaAsync(report, "RESOLVIDO");

                    _sistemaEstavaSaudavel = true; // Atualiza a memória: sistema voltou ao normal
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task EnviarEmailAlertaAsync(HealthReport report, string tipoAlerta)
    {
        try
        {
            using var smtpClient = new SmtpClient(_smtpSettings.Host)
            {
                Port = _smtpSettings.Port,
                Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Pass),
                EnableSsl = true,
            };

            // Ajustando as cores e o texto baseado no tipo de alerta
            string cor = tipoAlerta == "INCIDENTE" ? "#d9534f" : "#5cb85c"; // Vermelho para erro, Verde para sucesso
            string icone = tipoAlerta == "INCIDENTE" ? "⚠️ [URGENTE]" : "✅ [RECUPERADO]";

            var mailMessage = new MailMessage
            {
                From = new MailAddress("alertas@inframonitor.com", "InfraMonitor Alertas"),
                Subject = $"{icone} Status da Infraestrutura: {tipoAlerta}",
                IsBodyHtml = true
            };

            mailMessage.To.Add(_smtpSettings.ToEmail);

            var corpoHtml = $@"
                <div style='font-family: Arial, sans-serif;'>
                    <h2 style='color: {cor};'>Alerta de Monitoramento: {tipoAlerta}</h2>
                    <p>Status atual do sistema: <b>{report.Status}</b></p>
                    <hr />
                    <ul>";

            // Se for incidente, lista o que quebrou. Se for recuperação, lista que tá tudo OK.
            if (tipoAlerta == "INCIDENTE")
            {
                foreach (var entry in report.Entries.Where(x => x.Value.Status != HealthStatus.Healthy))
                {
                    corpoHtml += $"<li><b>{entry.Key}</b>: {entry.Value.Exception?.Message ?? "Falha de conexão."}</li>";
                }
            }
            else
            {
                corpoHtml += "<li>Todos os serviços voltaram a operar normalmente.</li>";
            }

            corpoHtml += "</ul><br /><p><i>Este é um alerta automático gerado pelo InfraMonitor.Api.</i></p></div>";
            mailMessage.Body = corpoHtml;

            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro crítico ao tentar enviar e-mail de alerta: {ex.Message}");
        }
    }
}