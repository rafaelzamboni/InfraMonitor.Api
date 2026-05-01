using Microsoft.Extensions.Configuration; // Para usar IConfiguration
using Npgsql;
using System.Data;

namespace InfraMonitor.Api.Data
{
    public class DatabaseConfig
    {
        private readonly IConfiguration _configuration;

        public DatabaseConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }
       
        public IDbConnection GetConnection()
        {
            //Tenta pegar a string de conexão na variável de ambiente se não encontrar utiliza a default 
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
                ?? _configuration.GetConnectionString("PostgresConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Conexão não encontrada");
            }

            return new NpgsqlConnection(connectionString);
        }
    }
}
