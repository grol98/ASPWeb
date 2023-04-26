namespace ASPWeb.Models
{
    public class RelationsControllersWorkers
    {
        public int Id { get; set; }
        public int sn { get; set; }              // серийный номер контроллера
        public int workerId { get; set; }          // ИДН сотрудника
        public int reader { get; set; }          // вход/выход
    }
}
