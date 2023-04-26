namespace ASPWeb.Models
{
    public class Controllers
    {
        public int Id { get; set; }
        public string? type { get; set; }          // тип контроллера
        public int sn { get; set; }                // серийный номер контроллера
        public string? fw { get; set; }            // версия прошивки контроллера
        public string? conn_fw { get; set; }       // версия прошивки модуля связи
        public string? controller_ip { get; set; } // IP адрес контроллера в локальной сети
        public string? name { get; set; }          // Имя контроллера
        public string lastCall { get; set; }       // Время последнего отклика
    }
}
