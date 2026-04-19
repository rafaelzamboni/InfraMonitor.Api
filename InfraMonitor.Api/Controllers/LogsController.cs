using Inframonitor.Api.Models;
using InfraMonitor.Api.Data;
using InfraMonitor.Api.Data.Repositories;
using InfraMonitor.Api.Dto;
using Microsoft.AspNetCore.Mvc;

namespace InfraMonitor.Api.Controllers;

[ApiController] // Define a rota base para este controlador
[Route("api/[controller]")] // Define a rota base para este controlador
public class LogsController : ControllerBase
{
    private readonly LogRepository _logRepository;
    private readonly ILogger<LogsController> _logger;
    public LogsController(LogRepository logRepository, ILogger<LogsController> logger)
    {
        _logRepository = logRepository;
        _logRepository.InicializarBanco(); // Garante que a tabela exista
        _logger = logger;
    }
    [HttpGet]
    public async Task<IActionResult> GetLogs()
    {
        try
        {
            var logs = await _logRepository.ListarTodos();

            return logs.Any() ? Ok(logs) : NoContent(); // Retorna os logs com status 200
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Erro ao consultar");

            return StatusCode(500, "Erro interno ao acessar a base de dados");
        }
        
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLogById(int id)
    {
        try
        {
            var logs = await _logRepository.GetById(id);
            return logs.Any() ? Ok(logs) : NotFound(); // Retorna os logs com status 200 ou 404 se não encontrado
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Erro ao consultar por ID");
            return StatusCode(500, "Erro interno ao acessar a base de dados");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostLogs([FromForm] LogCreateDto dto) 
    {
        _logger.LogInformation("Recebendo log do servidor: {Servidor}", dto.Servidor);

        if (string.IsNullOrEmpty(dto.Servidor) || string.IsNullOrEmpty(dto.Status) || string.IsNullOrEmpty(dto.Mensagem))
        {
            _logger.LogWarning("Tentativa de POST com campos obrigatórios ausentes");

            //Retorna 400
            return BadRequest("Servidor, Status e Mensagem são campos obrigatórios.");
        }

        var novoLog = new LogEvento
        {
            Servidor = dto.Servidor,
            Status = dto.Status,
            Mensagem = dto.Mensagem
        };

        await _logRepository.Salvar(novoLog);

        // Retorna o log criado com status 201
        return CreatedAtAction(nameof(GetLogs), new { id = novoLog.Id }, novoLog);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLog(int id, [FromForm] LogCreateDto dto)
    {
        if (string.IsNullOrEmpty(dto.Servidor) || string.IsNullOrEmpty(dto.Status) || string.IsNullOrEmpty(dto.Mensagem))
        {
            return BadRequest("Servidor, Status e Mensagem são campos obrigatórios.");
        }
        var logAtualizado = new LogEvento
        {
            Servidor = dto.Servidor,
            Status = dto.Status,
            Mensagem = dto.Mensagem
        };
        var resultado = await _logRepository.Atualizar(id, logAtualizado);
        return resultado ? NoContent() : NotFound(); // Retorna 204 se atualizado ou 404 se não encontrado
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLog(int id)
    {
        var resultado = await _logRepository.Excluir(id);
        return resultado ? NoContent() : NotFound(); // Retorna 204 se excluído ou 404 se não encontrado
    }
}

