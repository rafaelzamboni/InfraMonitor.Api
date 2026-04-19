using Dapper;
using Inframonitor.Api.Models;
using InfraMonitor.Api.Data;
using InfraMonitor.Api.Data.Interfaces;
using Npgsql;
using System.Collections.Generic; // Para usar IEnumerable e List
using System.Data;

namespace InfraMonitor.Api.Data.Repositories;

public class LogRepository : ILogRepository, IDbInitializer
{
    private readonly DatabaseConfig _dbConfig;
    private readonly ILogger<LogRepository> _logger;

    public LogRepository(DatabaseConfig dbConfig, ILogger<LogRepository> logger)
    {
        _dbConfig = dbConfig;
        _logger = logger;
    }
    public void InicializarBanco()
    {
        using var connection = _dbConfig.GetConnection();

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS LogsInfra (
                Id SERIAL PRIMARY KEY,
                Data TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                Servidor VARCHAR(50),
                Status VARCHAR(20),
                Mensagem TEXT
            )"
        );
    }

    public async Task Salvar(LogEvento log)
    {
        try 
        {
            using var connection = _dbConfig.GetConnection();

            _logger.LogDebug("Executando INSERT no Postgres para o servidor {Servidor}", log.Servidor);

            //"RETURNING Id" ao final da query
            string sql = @"INSERT INTO LogsInfra (Servidor, Status, Mensagem) 
               VALUES (@Servidor, @Status, @Mensagem) 
               RETURNING Id";

            // Faz o C# esperar um valor de volta do banco
            int idGerado = await connection.QuerySingleAsync<int>(sql, log);

            //Atualiza o objeto log com o ID real do banco
            log.Id = idGerado;

            _logger.LogInformation("Log salvo com sucesso de forma assíncrona. ID: {Id}", log.Id);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "ERRO DE BANCO: Não foi possível salvar no Postgres.");

            throw new Exception("O sistema de logs está temporariamente indisponível. Verifique a conexão com o banco.");
        }
    }

    public async Task<IEnumerable<LogEvento>> GetById(int id)
    {
        _logger.LogDebug("Executando SELECT no Postgres para buscar log por ID: {Id}", id);
        try
        {
            using var connection = _dbConfig.GetConnection();
            string sql = @"
                SELECT 
                    Id, 
                    Data, 
                    Servidor, 
                    Status, 
                    Mensagem 
                FROM LogsInfra
                WHERE Id = @Id
            ";
            return await connection.QueryAsync<LogEvento>(sql, new { Id = id });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar log por ID no banco.");
            //Retorna lista vazia
            return Enumerable.Empty<LogEvento>();
        }
    }

    public async Task<IEnumerable<LogEvento>> ListarTodos()
    {
        _logger.LogDebug("Executando SELECT no Postgres para listar todos os logs");

        try
        {
            using var connection = _dbConfig.GetConnection();

            string sql = @"
                SELECT 
                    Id, 
                    Data, 
                    Servidor, 
                    Status, 
                    Mensagem 
                FROM LogsInfra
                ORDER BY Data DESC
                LIMIT 100
            ";

            return await connection.QueryAsync<LogEvento>(sql);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar logs do banco.");

            //Retona lista vazia
            return Enumerable.Empty<LogEvento>();
        }
    }

    public async Task<bool> Atualizar(int id, LogEvento log)
    {
        _logger.LogDebug("Executando UPDATE no Postgres para atualizar log ID: {Id}", id);
        try
        {
            using var connection = _dbConfig.GetConnection();
            string sql = @"
                UPDATE LogsInfra
                SET 
                    Servidor = @Servidor, 
                    Status = @Status, 
                    Mensagem = @Mensagem
                WHERE Id = @Id
            ";
            int linhasAfetadas = await connection.ExecuteAsync(sql, new { log.Servidor, log.Status, log.Mensagem, Id = id });
            return linhasAfetadas > 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar log no banco.");
            return false;
        }
    }

    public async Task<bool> Excluir(int id)
    {
        _logger.LogDebug("Executando DELETE no Postgres para excluir log ID: {Id}", id);
        try
        {
            using var connection = _dbConfig.GetConnection();
            string sql = "DELETE FROM LogsInfra WHERE Id = @Id";
            int linhasAfetadas = await connection.ExecuteAsync(sql, new { Id = id });
            return linhasAfetadas > 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir log do banco.");
            return false;
        }
    }
}
