using ASPWeb.Models;

namespace ASPWeb.Interfaces
{
    public interface IWorkersRep
    {
        IEnumerable<Workers> workers { get; }
        Workers GetWorkerById(int id);
        void EditWorker(Workers worker);
    }
}
