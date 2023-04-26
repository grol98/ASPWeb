using ASPWeb.Interfaces;
using ASPWeb.Models;

namespace ASPWeb
{
    public class WorkersRepository:IWorkersRep
    {
        private readonly ApplicationContext db;
        public WorkersRepository(ApplicationContext context) {
            db = context;
        }

        public IEnumerable<Workers> workers => db.Workers;

        public void EditWorker(Workers worker)
        {
            throw new NotImplementedException();
        }

        public Workers GetWorkerById(int id) => workers.FirstOrDefault(p => p.Id == id);
    }
}
