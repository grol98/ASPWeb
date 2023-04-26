using ASPWeb.Interfaces;
using ASPWeb.Models;

namespace ASPWeb.Repository
{
    public class GroupsRepository:IGroupsRep
    {
        private readonly ApplicationContext db;
        public GroupsRepository(ApplicationContext context)
        {
            db = context;
        }

        public IEnumerable<Groups> groups => db.Groups.OrderBy(p => p.group);


        public Groups GetGroupById(int id) => groups.FirstOrDefault(p => p.Id == id);
    }
}
