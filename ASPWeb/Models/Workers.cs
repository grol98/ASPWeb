using System.ComponentModel.DataAnnotations.Schema;

namespace ASPWeb.Models
{
    public class Workers
    {
        public int Id { get; set; }
        public int worker { get; set; }                  // ИДН сотрудника
        public string? LastName { get; set; }            // ФИО сотрудника
        public string? FirstName { get; set; }                
        public string? FatherName { get; set; }                
        public string? position { get; set; }            // Должность сотрудника
        public string? group { get; set; }               // Группа к которой относится сотрудник (например отдел)
        public string? comment { get; set; }             // Коментарий
        public int personalCheck { get; set; }           // Триггер индивидуальных групп доступа (1 да/ 0 нет)
        public List<string> accessGroups { get; set; }   // Список групп доступа
        public string? Image { get; set; }               // Путь к фотографии
        public string? lockDate { get; set; }            // Дата блокировки
        public IEnumerable<Cards> cards { get; set; }
    }
}
