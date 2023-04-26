namespace ASPWeb.Models
{
    public class EventsWeb
    {
        public int Id { get; set; }
        public string? name { get; set; }         // серийный номер контроллера
        public string Event { get; set; }    // тип события
        public string? card { get; set; }    // номер карты в шестнадцатеричном виде
        public string? time { get; set; }    // время события
        public int flag { get; set; }        // флаги события
        public int? workerId { get; set; }   // идн сотрудника
        public string? fio { get; set; }     // ФИО сотрудника
    }
}
