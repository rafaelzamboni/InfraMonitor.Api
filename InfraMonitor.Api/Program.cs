using InfraMonitor.Api.Data;
using InfraMonitor.Api.Data.Interfaces;
using InfraMonitor.Api.Data.Repositories;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. ADICIONA OS SERVIÇOS
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString, // O C# já sabe que o primeiro texto é a connection string
        name: "PostgreSQL",
        tags: new[] { "db", "data", "infra" }
    );

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Isso aqui é o que instala o Swagger

//Interfaces
builder.Services.AddSingleton<DatabaseConfig>();
// Registra a classe em si
builder.Services.AddScoped<LogRepository>();
// Diz que ILogRepository usa a instância de LogRepository
builder.Services.AddScoped<ILogRepository>(sp => sp.GetRequiredService<LogRepository>());
// Diz que IDbInitializer também usa a mesma instância de LogRepository
builder.Services.AddScoped<IDbInitializer>(sp => sp.GetRequiredService<LogRepository>());

// Configura o Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Nível mínimo de log (pode ser Debug para mais detalhes)
    .WriteTo.Console()
    .WriteTo.File("logs/monitor-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); // Diz ao .NET para usar o Serilog em vez do logger padrão

var app = builder.Build();

// 1. INICIALIZA O BANCO DE DADOS (Garante que a tabela exista antes de qualquer requisição)
using (var scope = app.Services.CreateScope())
{
    // Aqui pede apenas o Initializer, não o repositório completo
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    initializer.InicializarBanco();
}


// 2. CONFIGURA O PIPELINE (O caminho que a requisição faz)
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers(); // Isso aqui faz o .NET achar o seu LogsController

app.Run();