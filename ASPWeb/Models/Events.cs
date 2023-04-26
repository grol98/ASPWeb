namespace ASPWeb.Models
{
    public class Events
    {
        public int Id { get; set; }
        public int sn { get; set; }             // серийный номер контроллера
        public int Event { get; set; }          // тип события
        public string? card { get; set; }       // номер карты в шестнадцатеричном виде
        public DateTime dateTime { get; set; }  // время события
        public int flag { get; set; }           // флаги события
        public int? workerId { get; set; }      // идн сотрудника
        public Workers worker { get; set; }     // сотрудник
    }
}
