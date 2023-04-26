using ASPWeb.Models;

namespace ASPWeb.Interfaces
{
    public interface IGroupsRep
    {
        IEnumerable<Groups> groups { get; }
        Groups GetGroupById(int id);
    }
}
