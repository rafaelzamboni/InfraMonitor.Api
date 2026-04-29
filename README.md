# InfraMonitor 🛡️📊

O **InfraMonitor** é uma ferramenta de observabilidade desenvolvida em **C# .NET 10**, focada em garantir a resiliência e a saúde de ecossistemas de microsserviços. 

O projeto demonstra a implementação de padrões modernos de monitoramento de infraestrutura, fornecendo diagnósticos detalhados sobre dependências críticas como bancos de dados (PostgreSQL e SQL Server) e serviços externos.

## 🏗️ Arquitetura Pragmática e Modular

Diferente de sistemas com regras de negócio complexas, este projeto adota uma **arquitetura orientada a serviços e camadas lógicas** dentro de um único projeto. Essa abordagem foca na performance e na facilidade de manutenção para ferramentas de infraestrutura:

* **Controllers:** Endpoints otimizados para exposição de status de saúde e gerenciamento de logs.
* **Data (Repositories & Interfaces):** Implementação do *Repository Pattern* para isolar a lógica de acesso a dados. A configuração de banco é centralizada no `DatabaseConfig.cs`.
* **Models & DTOs:** Separação clara entre as entidades de banco de dados (`LogEvento`) e os objetos de transferência de dados (`LogCreatedDto`), garantindo segurança e integridade.
* **Logs Estruturados:** Pasta local configurada para persistência de logs via Serilog (integrada ao `.gitignore` para manter o repositório limpo).

## 🚀 Funcionalidades e Diferenciais Técnicos

* **Health Checks Customizados:** Monitoramento de status para múltiplos bancos de dados utilizando tags para filtragem de prontidão (*Readiness*) e vitalidade (*Liveness*).
* **Observabilidade com Serilog:** Configuração de logs estruturados em arquivos, facilitando o rastreio de falhas e o troubleshooting em produção.
* **Injeção de Dependência:** Uso extensivo de DI nativo do .NET para garantir desacoplamento entre os controladores e as camadas de dados.
* **SRE (Site Reliability Engineering):** Preparado para diagnosticar falhas de conectividade sem interromper o serviço principal.

## 🛠️ Tecnologias Utilizadas

* **C# 12 / .NET 8**
* **Microsoft.Extensions.Diagnostics.HealthChecks**
* **Serilog** (Structured Logging)
* **Entity Framework Core / Dapper**
* **PostgreSQL & SQL Server**

## 🛤️ Roadmap de Desenvolvimento (WIP)

Este projeto está em desenvolvimento. As próximas etapas são:

- [ ] **Conteinerização:** Criação de `docker-compose.yml` para orquestrar a API e as instâncias de bancos de dados.
- [ ] **Dashboard Visual:** Interface para visualização em tempo real dos status de saúde.
- [ ] **Alertas Automáticos:** Notificações via Slack ou E-mail em caso de indisponibilidade de serviços.

## ⚙️ Como Executar o Projeto

### Pré-requisitos
* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* Instâncias de PostgreSQL ou SQL Server (ou via Docker após a conclusão do roadmap).

### Passos Rápidos

Abra o seu terminal e execute os comandos abaixo em sequência (lembre-se de configurar o `appsettings.json` antes de rodar):

```bash
# 1. Clone o repositório
git clone [https://github.com/](https://github.com/)[SEU-USUARIO-GITHUB]/InfraMonitor.git

# 2. Acesse a pasta da API
cd InfraMonitor/InfraMonitor.Api

# 3. Restaure as dependências e execute a aplicação
dotnet run

# 3. Restaure as dependências e execute a aplicação
dotnet run
