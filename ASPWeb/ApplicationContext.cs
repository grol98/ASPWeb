using Microsoft.EntityFrameworkCore;

namespace ASPWeb
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Users> Users { get; set; } = null!;
        public DbSet<Cards> Cards { get; set; } = null!;
        public DbSet<Controllers> Controllers { get; set; } = null!;
        public DbSet<Events> Events { get; set; } = null!;
        public DbSet<EventCodes> EventCodes { get; set; } = null!;
        public DbSet<Workers> Workers { get; set; } = null!;
        public DbSet<Groups> Groups { get; set; } = null!;
        public DbSet<AccessGroups> AccessGroups { get; set; } = null!;
        public DbSet<MessagesDB> Messages { get; set; } = null!;
        public DbSet<RelationsControllersAccessGroups> RelationsControllersAccessGroups { get; set; } = null!;

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=usersdb;Username=postgres;Password=2");
        }
    }

    public class Users
    {
        public int Id { get; set; }
        public string login { get; set; }
        public string? password { get; set; }
    }

    public class Cards
    {
        public int Id { get; set; }
        public string card16 { get; set; }     // номер карты в шестнадцатеричном виде
        public string? card { get; set; }      // номер карты
        public int? worker { get; set; }       // ИДН сотрудника
    }

    public class Controllers
    {
        public int Id { get; set; }
        public string? type { get; set; }          // тип контроллера
        public int sn { get; set; }                // серийный номер контроллера
        public string? fw { get; set; }            // версия прошивки контроллера
        public string? conn_fw { get; set; }       // версия прошивки модуля связи
        public string? controller_ip { get; set; } // IP адрес контроллера в локальной сети
    }

    public class Events
    {
        public int Id { get; set; }
        public int? sn { get; set; }       // серийный номер контроллера
        public int Event { get; set; }     // тип события
        public string? card { get; set; }  // номер карты в шестнадцатеричном виде
        public string? time { get; set; }  // время события
        public int flag { get; set; }      // флаги события
    }

    public class EventCodes
    {
        public int Id { get; set; }
        public int eventCode { get; set; }    // код события
        public string Event { get; set; }     // тип события
    }

    public class Workers
    {
        public int Id { get; set; }
        public int worker { get; set; }                  // ИДН сотрудника
        public string? fio { get; set; }                 // ФИО сотрудника
        public string? position { get; set; }            // Должность сотрудника
        public string? group { get; set; }               // Группа к которой относится сотрудник (например отдел)
        public string? comment { get; set; }             // Коментарий
        public int personalCheck { get; set; }           // Триггер индивидуальных групп доступа (1 да/ 0 нет)
        public List<string> accessGroups { get; set; }   // список групп доступа
    }

    public class Groups
    {
        public int Id { get; set; }
        public string group { get; set; }                // название группы
        public List<string> accessGroups { get; set; }   // список групп доступа
    }

    public class AccessGroups
    {
        public int Id { get; set; }
        public string accessGroup { get; set; }  // название группы доступа
    }

    public class RelationsControllersAccessGroups
    {
        public int Id { get; set; }
        public int sn { get; set; }              // серийный номер контроллера
        public string accessGroup { get; set; }  // название группы доступа
        public int reader { get; set; }          // вход/выход
    }

    public class MessagesDB
    {
        public int Id { get; set; }
        public int sn { get; set; }
        public string operation { get; set; }
        public string? card { get; set; }
        public int? flag { get; set; }
        public int? tz { get; set; }
    }
}