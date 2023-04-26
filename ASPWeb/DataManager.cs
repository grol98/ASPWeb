using ASPWeb.Interfaces;

namespace ASPWeb
{
    public class DataManager
    {
        public IWorkersRep workers;
        public IGroupsRep groups;
        public DataManager(IWorkersRep workersRep, IGroupsRep groupsRep) {
            workers = workersRep;
            groups = groupsRep;
        }
    }
}
