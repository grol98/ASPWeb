namespace ASPWeb.Models
{
    public class Groups
    {
        public int Id { get; set; }
        public string group { get; set; }                // название группы
        public List<string> accessGroups { get; set; }   // список групп доступа
    }
}
