namespace ASPWeb.Models
{
    public class Cards
    {
        public int Id { get; set; }
        public string card16 { get; set; }     // номер карты в шестнадцатеричном виде
        public string? card { get; set; }      // номер карты
        public int? workerId { get; set; }       // ИДН сотрудника
        public Workers worker { get; set; }
    }
}
