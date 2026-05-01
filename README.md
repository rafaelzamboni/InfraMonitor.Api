# InfraMonitor 🛡️📊

O **InfraMonitor** é uma ferramenta de observabilidade desenvolvida em **C# .NET 10**, focada em garantir a resiliência e a saúde de ecossistemas de microsserviços. 

O projeto demonstra a implementação de padrões modernos de monitoramento de infraestrutura, fornecendo diagnósticos detalhados sobre dependências críticas como bancos de dados (PostgreSQL e SQL Server) e serviços externos.

## 🏗️ Arquitetura Pragmática e Modular

Para garantir máxima performance e facilidade de manutenção em uma ferramenta de infraestrutura, **este projeto adota uma arquitetura orientada a serviços e camadas lógicas dentro de um único projeto, não utilizando os princípios de Domain-Driven Design (DDD).**

* **Controllers:** Endpoints otimizados para exposição de status de saúde e gerenciamento de logs.
* **Data (Repositories & Interfaces):** Implementação do *Repository Pattern* para isolar a lógica de acesso a dados. A injeção da string de conexão (priorizando variáveis de ambiente) é centralizada no `DatabaseConfig.cs`.
* **Models & DTOs:** Separação clara entre as entidades de banco de dados (`LogEvento`) e os objetos de transferência de dados (`LogCreatedDto`), garantindo segurança e integridade.
* **Logs Estruturados:** Pasta local configurada para persistência de logs via Serilog (integrada ao `.gitignore` para manter o repositório limpo).

## 🚀 Funcionalidades e Diferenciais Técnicos

* **Health Checks Customizados:** Monitoramento de status focado atualmente na prontidão (*Readiness*) e vitalidade (*Liveness*) do banco PostgreSQL.
* **Observabilidade com Serilog:** Configuração de logs estruturados em arquivos, facilitando o rastreio de falhas e o troubleshooting em produção.
* **Injeção de Dependência:** Uso extensivo de DI nativo do .NET para garantir desacoplamento entre os controladores e as camadas de dados.
* **SRE (Site Reliability Engineering):** Preparado para diagnosticar falhas de conectividade sem interromper o serviço principal (12-Factor App methodology).

## 🛠️ Tecnologias Utilizadas

* **C# 12 / .NET 10 SDK**
* **Microsoft.Extensions.Diagnostics.HealthChecks**
* **Serilog** (Structured Logging)
* **Entity Framework Core / Dapper**
* **PostgreSQL & SQL Server**
* **Docker & Docker Compose** (Infraestrutura)

## 🛤️ Roadmap de Desenvolvimento (WIP)

Este projeto está em desenvolvimento. As próximas etapas são:

- [x] **Conteinerização:** Criação de `docker-compose.yml` para orquestrar as instâncias de bancos de dados localmente.
- [ ] **Dashboard Visual:** Interface para visualização em tempo real dos status de saúde.
- [ ] **Alertas Automáticos:** Notificações via Slack ou E-mail em caso de indisponibilidade de serviços.

## ⚙️ Como Executar o Projeto

### Pré-requisitos
* [.NET 10 SDK](https://dotnet.microsoft.com/download) instalado.
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) rodando em background.
* IDE de sua preferência (recomendado: Visual Studio 2022 ou VS Code).

### 1. Clonando o Repositório
Primeiro, baixe o código para a sua máquina e acesse a pasta raiz do projeto:
```bash
git clone [https://github.com/](https://github.com/)[SEU-USUARIO-GITHUB]/InfraMonitor.git
cd InfraMonitor
```

### 2. Subindo a Infraestrutura (Bancos de Dados via Docker)
O projeto utiliza o **Docker Compose** para orquestrar os contêineres do PostgreSQL (banco principal de logs) e do SQL Server. Para garantir a segurança das credenciais, utilizamos variáveis de ambiente.

Na raiz do repositório (junto do arquivo `docker-compose.yml`), crie um arquivo chamado **`.env`** e preencha com as suas senhas:
```env
# Arquivo .env (⚠️ NUNCA commite este arquivo. Ele deve estar no seu .gitignore)
DB_POSTGRES_USER=postgres
DB_POSTGRES_PASSWORD=SuaSenhaFortePostgres123!
DB_SQLSERVER_PASSWORD=SuaSenhaForteSqlServer123!
```

Com o `.env` criado, abra o terminal na raiz do projeto e execute:
```bash
docker-compose up -d
```
*Aguarde os contêineres iniciarem e verifique o status de "Running" no Docker Desktop.*

### 3. Configurando a API
Acesse a pasta da API e configure o projeto para enxergar o banco de dados que acabou de subir:
```bash
cd InfraMonitor.Api
```

Abra o arquivo `appsettings.json` (e o `appsettings.Development.json`, se existir) e configure as *Connection Strings* para refletirem as credenciais definidas no seu `.env`:
```json
  "ConnectionStrings": {
    "PostgresConnection": "Host=localhost;Database=InfraMonitorDb;Username=postgres;Password=SuaSenhaFortePostgres123!",
    "SqlServerConnection": "Server=localhost,1433;Database=master;User Id=sa;Password=SuaSenhaForteSqlServer123!;TrustServerCertificate=True"
  }
```

### 4. Rodando a Aplicação
Com a infraestrutura no ar e o arquivo `appsettings.json` configurado, execute a aplicação:
```bash
dotnet run
```

Para validar se a API subiu corretamente e está se conectando ao banco de dados conteinerizado, acesse o endpoint `/health` no seu navegador ou via Postman. O retorno deve indicar o status **"Healthy"**.
