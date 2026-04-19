using InfraMonitor.Api.Data;
using InfraMonitor.Api.Data.Interfaces;
using InfraMonitor.Api.Data.Repositories;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configura o Serilog logo no início
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/monitor-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 1. ADICIONA OS SERVIÇOS
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Verificação de segurança: Se a connection string estiver nula, o app vai avisar no log
if (string.IsNullOrEmpty(connectionString))
{
    Log.Error("A ConnectionString 'DefaultConnection' não foi encontrada no appsettings.json!");
}

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString ?? "", name: "PostgreSQL", tags: new[] { "db", "data", "infra" });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI - Interfaces e Repositórios
builder.Services.AddSingleton<DatabaseConfig>();
builder.Services.AddScoped<LogRepository>();
builder.Services.AddScoped<ILogRepository>(sp => sp.GetRequiredService<LogRepository>());
builder.Services.AddScoped<IDbInitializer>(sp => sp.GetRequiredService<LogRepository>());

var app = builder.Build();

// 2. CONFIGURA O PIPELINE (Mova o Swagger para CIMA da inicialização do banco)
// Assim, se o banco falhar, o Swagger pelo menos tenta abrir
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. INICIALIZA O BANCO
using (var scope = app.Services.CreateScope())
{
    try
    {
        Log.Information("Tentando inicializar o banco de dados...");
        var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        initializer.InicializarBanco();
        Log.Information("Banco de dados inicializado com sucesso.");
    }
    catch (Exception ex)
    {
        // Se o banco falhar, o app LOGA o erro mas NÃO TRAVA a subida do Swagger
        Log.Error(ex, "Erro ao inicializar o banco de dados. Verifique se o Docker está rodando.");
    }
}

app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();