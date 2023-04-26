namespace ASPWeb.Models
{
    public class RelationsControllersAccessGroups
    {
        public int Id { get; set; }
        public int sn { get; set; }              // серийный номер контроллера
        public string accessGroup { get; set; }  // название группы доступа
        public int reader { get; set; }          // вход/выход
    }
}
