using Inframonitor.Api.Models;

namespace InfraMonitor.Api.Data.Interfaces
{
    public interface ILogRepository
    {
       
        Task Salvar(LogEvento log);
        Task<IEnumerable<LogEvento>> ListarTodos();
        Task<IEnumerable<LogEvento>> GetById(int id);
        Task<bool> Atualizar(int id, LogEvento log);
        Task<bool> Excluir(int id);

    }
}
