using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.IO.Ports;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ASPWeb.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public string Message { get; set; }
        public List<Users> users { get; set; }
        public List<Cards> cards { get; set; }
        public List<Cards> emptyCards { get; set; }
        public List<Controllers> controllers { get; set; }
        public List<Events> events { get; set; }
        public List<EventCodes> eventCodes { get; set; }
        public List<Workers> workers { get; set; }
        public List<Groups> groups { get; set; }
        public List<AccessGroups> accessGroups { get; set; }
        public List<RelationsControllersAccessGroups> relationControllersAccessGroups { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            users = new List<Users>();
            cards = new List<Cards>();
            controllers = new List<Controllers>();
            events = new List<Events>();
            eventCodes = new List<EventCodes>();
            workers = new List<Workers>();
            groups = new List<Groups>();
            accessGroups = new List<AccessGroups>();
            relationControllersAccessGroups = new List<RelationsControllersAccessGroups>();
        }

        public void OnGet()
        {
            if (!HttpContext.Session.Keys.Contains("logged"))
            {             
                HttpContext.Session.SetString("logged", "false");
                HttpContext.Session.SetString("BadRequest", "false");
                HttpContext.Session.SetString("tables", "false");
            }
            else
            {                
                using (ApplicationContext db = new ApplicationContext())
                {
                    users = db.Users.OrderBy(p => p.Id).ToList();
                    cards = db.Cards.OrderBy(p => p.Id).ToList();
                    emptyCards = db.Cards.Where(p => p.worker == null).ToList();
                    controllers = db.Controllers.OrderBy(p => p.Id).ToList();
                    events = db.Events.OrderByDescending(p => p.Id).ToList();
                    eventCodes = db.EventCodes.OrderBy(p => p.Id).ToList();
                    workers = db.Workers.OrderBy(p => p.Id).ToList();
                    groups = db.Groups.OrderBy(p => p.Id).ToList();
                    accessGroups = db.AccessGroups.OrderBy(p => p.Id).ToList();
                    relationControllersAccessGroups = db.RelationsControllersAccessGroups.OrderBy(p => p.Id).ToList();
                }
            }
        }


        public IActionResult OnPost()
        {
            var form = HttpContext.Request.Form;
            if (form.ContainsKey("login") && form.ContainsKey("password"))
            {
                string login = form["login"];
                string password = form["password"];
                using (ApplicationContext db = new ApplicationContext())
                {
                    Users? user = db.Users.FirstOrDefault(p => p.login == login && p.password == password);
                    if (user is null)
                    {
                        HttpContext.Session.SetString("BadRequest", "true");
                    }
                    else
                    {
                        var users = db.Users.ToList();
                        HttpContext.Session.SetString("logged", "true");
                        HttpContext.Session.SetString("BadRequest", "false");
                        HttpContext.Session.SetString("userLogin", user.login);
                        HttpContext.Session.SetString("userId", user.Id.ToString());
                    }
                }
            }
            if (form.ContainsKey("logout"))
            {
                HttpContext.Session.SetString("logged", "false");
            }
            string url = "/";
            return Redirect(url);
        }


        // Удаление пользователя, сотрудника, карты или контроллера
        public IActionResult OnGetDelete(int id, string flag)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            if (flag == "user")
            {
                DeleteUser(id);
            }
            if (flag == "worker")
            {
                DeleteWorker(id);
            }
            if (flag == "card")
            {
                DeleteCard(id);
            }
            if (flag == "controller")
            {
                DeleteController(id);
            }
            if (flag == "group")
            {
                return Content(DeleteGroup(id));
            }
            if (flag == "acessGroup")
            {
                return Content(DeleteAccessGroup(id));
            }
            if (flag == "relation")
            {
                DeleteRelationsControllersAccessGroups(id);
            }

            return Content("true");
        }


        // Удаление пользователя
        public void DeleteUser(int id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                Users? user = db.Users.FirstOrDefault(p => p.Id == id);
                db.Users.Remove(user);
                _logger.LogInformation($"deleted user id: {user.Id}; login: {user.login}");

                db.SaveChanges();
            }
        }


        // Удаление сотрудника
        public void DeleteWorker(int id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                Workers? workerdb = db.Workers.FirstOrDefault(p => p.Id == id);
                var controllersdb = db.Controllers.ToList();
                if (workerdb is null || controllersdb.Count == 0)
                {
                    return;
                }
                
                List<string> accessGroupsLocal;
                if (workerdb.personalCheck == 1)
                {
                    accessGroupsLocal = workerdb.accessGroups;
                }
                else
                {
                    var groupsdb = db.Groups.FirstOrDefault(p => p.group == workerdb.group);
                    if (groupsdb is null)
                    {
                        return;
                    }
                    accessGroupsLocal = groupsdb.accessGroups;
                }
                if (accessGroupsLocal.Count == 0)
                {
                    return;
                }
                db.Workers.Remove(workerdb);

                var cardsdb = db.Cards.Where(p => p.worker == workerdb.worker).ToList();
                List<RelationsControllersAccessGroups> relationsdb;
                foreach (Controllers controller in controllersdb)
                {
                    relationsdb = new List<RelationsControllersAccessGroups>();
                    foreach (string accessGroup in accessGroupsLocal)
                    {
                        relationsdb.AddRange(db.RelationsControllersAccessGroups.Where(p => p.accessGroup == accessGroup && p.sn == controller.sn).ToList());
                    }
                    if (relationsdb.Count > 0)
                    {
                        foreach (Cards card in cardsdb)
                        {
                            // Удаление из контроллера
                            DeleteCardFromController(controller.sn, card.card16);
                        }
                    }
                }
                foreach (Cards card in cardsdb) // удаление сотрудника из всех карт
                {
                    card.worker = null;
                    db.Cards.Update(card);
                }

                _logger.LogInformation($"deleted worker: {JsonSerializer.Serialize(workerdb)}");
                db.SaveChanges();
            }
        }


        // Удаление карты
        public void DeleteCard(int id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                Cards? card = db.Cards.FirstOrDefault(p => p.Id == id);
                db.Cards.Remove(card);
                if (card is null || card.worker is null)
                {
                    return;
                }
                Workers? workerdb = db.Workers.FirstOrDefault(p => p.worker == card.worker);
                var controllersdb = db.Controllers.ToList();
                if (workerdb is null || controllersdb.Count == 0)
                {
                    return;
                }

                List<string> accessGroupsLocal;
                if (workerdb.personalCheck == 1)
                {
                    accessGroupsLocal = workerdb.accessGroups;
                }
                else
                {
                    var groupsdb = db.Groups.FirstOrDefault(p => p.group == workerdb.group);
                    if (groupsdb is null)
                    {
                        return;
                    }
                    accessGroupsLocal = groupsdb.accessGroups;
                }
                if (accessGroupsLocal.Count == 0)
                {
                    return;
                }
                //List<RelationsControllersAccessGroups> relationsdb;
                foreach (Controllers controller in controllersdb)
                {

                    var relationsdb = db.RelationsControllersAccessGroups.Where(p => p.sn == controller.sn && accessGroupsLocal.Contains(p.accessGroup)).ToList();
                    if (relationsdb.Count > 0)
                    {
                        // Удаление из контроллера
                        DeleteCardFromController(controller.sn, card.card16);
                    }
                }
                
                db.SaveChanges();
                _logger.LogInformation($"deleted card: {JsonSerializer.Serialize(card)}");
            }
        }


        // Удаление контроллера
        public void DeleteController(int id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                Controllers? controller = db.Controllers.FirstOrDefault(p => p.Id == id);
                var relations = db.RelationsControllersAccessGroups.Where(p => p.sn == controller.sn).ToList();
                db.RelationsControllersAccessGroups.RemoveRange(relations);
                db.Controllers.Remove(controller);
                _logger.LogInformation($"deleted controller id: {controller.Id}; login: {controller.sn}");
                db.SaveChanges();                
            }
        }


        // Удаление группы
        public string DeleteGroup(int id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                Groups group = db.Groups.FirstOrDefault(p => p.Id == id);
                if (group == null)
                {
                    return "Нет такой группы";
                }
                var worker = db.Workers.FirstOrDefault(p => p.group == group.group);
                if (worker != null)
                {
                    return "В этой группе находятся сотрудники";
                }
                db.Groups.Remove(group);
                db.SaveChanges();
                return "true";
            }            
        }


        // Удаление группы доступа
        public string DeleteAccessGroup(int id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var accessGroup = db.AccessGroups.FirstOrDefault(p => p.Id == id);
                if (accessGroup is null)
                {
                    return "Нет такой группы доступа";
                }
                var workersdb = db.Workers.ToList();
                var relationsdb = db.RelationsControllersAccessGroups.Where(p => p.accessGroup == accessGroup.accessGroup).ToList();
                var controllersdb = db.Controllers.Where(p => relationsdb.Exists(d => d.sn == p.sn)).ToList();
                foreach (var worker in workersdb)
                {
                    List<string> accessGroupsNew;
                    if (worker.personalCheck == 1)
                    {
                        if (!worker.accessGroups.Contains(accessGroup.accessGroup))
                        {
                            continue;
                        }
                        worker.accessGroups.Remove(accessGroup.accessGroup);
                        db.Workers.Update(worker);
                        accessGroupsNew = worker.accessGroups;
                    }
                    else
                    {
                        var groupdb = db.Groups.FirstOrDefault(p => p.group == worker.group);
                        if (!groupdb.accessGroups.Contains(accessGroup.accessGroup))
                        {
                            continue;
                        }
                        accessGroupsNew = groupdb.accessGroups;
                        accessGroupsNew.Remove(accessGroup.accessGroup);
                    }
                    foreach (var controller in controllersdb)
                    {
                        var relationOld = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == controller.sn && p.accessGroup == accessGroup.accessGroup);
                        var relationNew = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == controller.sn && accessGroupsNew.Contains(p.accessGroup));

                        if (relationOld is null || relationNew is not null)
                        {
                            continue;
                        }
                        var cardsdb = db.Cards.Where(p => p.worker == worker.worker).ToList();
                        foreach (var card in cardsdb)
                        {
                            DeleteCardFromController(controller.sn, card.card16);
                        }
                    }
                }
                var groupsdb = db.Groups.Where(p => p.accessGroups.Contains(accessGroup.accessGroup)).ToList();
                foreach (var group in groupsdb)
                {
                    group.accessGroups.Remove(accessGroup.accessGroup);
                    db.Groups.Update(group);
                }
                
                db.AccessGroups.Remove(accessGroup);
                db.SaveChanges();
                return "true";
            }
        }


        // Удаление связи контроллер - группа доступа
        public void DeleteRelationsControllersAccessGroups(int id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                RelationsControllersAccessGroups? relation = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.Id == id);
                int sn = relation.sn;                

                RelationsControllersAccessGroups? relationdb = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == sn && p.accessGroup == relation.accessGroup);
                db.RelationsControllersAccessGroups.Remove(relation);
                db.SaveChanges();
                if (relationdb != null) 
                {
                    return;
                }
                var workersdb = db.Workers.ToList();
                var groupsdb = db.Groups.Where(p => p.accessGroups.Contains(relationdb.accessGroup)).ToList();
                List<string> accessGroupsLocal;
                foreach(var worker in workersdb)
                {
                    if (worker.personalCheck == 1)
                    {
                        if (!worker.accessGroups.Contains(relationdb.accessGroup))
                        {
                            continue;
                        }        
                        accessGroupsLocal = worker.accessGroups;
                    }
                    else
                    {
                        if (!groupsdb.Exists(p => p.group == worker.group))
                        {
                            continue;
                        }
                        accessGroupsLocal = groupsdb.Find(p => p.group == worker.group).accessGroups;
                    }
                    bool flag = false;
                    foreach (var accessGroup in accessGroupsLocal)
                    {
                        var relationsdb = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.accessGroup == accessGroup && p.sn == sn);
                        if (relationsdb != null)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        continue;
                    }
                    var cardsdb = db.Cards.Where(p => p.worker == worker.worker).ToList();
                    foreach (var card in cardsdb)
                    {
                        DeleteCardFromController(sn, card.card16);
                    }
                }
            }
        }


        // Удаление карты из контроллера
        public void DeleteCardFromController(int sn, string card16)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                MessagesDB mesdb = new MessagesDB
                {
                    operation = "del_cards",
                    card = card16
                };

                MessagesDB mesdb2 = new MessagesDB
                {
                    operation = "wait"
                };

                mesdb.sn = sn;
                db.Messages.Add(mesdb);
                mesdb2.sn = sn;
                db.Messages.Add(mesdb2);

                db.SaveChanges();
            }
        }


        // Добавление пользователя
        public IActionResult OnGetAddUser(string login, string password)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            string response = "true";
            if (login is null)
            {
                return Content("Введите логин");
            }
            if (password is null)
            {
                return Content("Пароль не может быть пустым");
            }
            if (login.Length > 30 || login.Length < 5)
            {
                return Content("Логин должен быть от 5 до 30");
            }
            if (password.Length > 30 || password.Length < 5)
            {
                return Content("Пароль должен быть от 5 до 30");
            }
            var correctLogin = string.Concat(login.Where(char.IsLetterOrDigit));
            var correctPassword = string.Concat(password.Where(char.IsLetterOrDigit));
            if (login != correctLogin || password != correctPassword)
            {
                return Content("Логин и пароль должны состоять из букв и цифр");
            }
            using (ApplicationContext db = new ApplicationContext())
            {
                Users? user = db.Users.FirstOrDefault(p => p.login == login);
                if (user is not null)
                {
                    return Content("Этот логин уже занят");
                }
                Users? newUser = new Users()
                {
                    login = login,
                    password = password
                };

                db.Users.Add(newUser);
                _logger.LogInformation($"add user id: {newUser.Id}; login: {newUser.login}");
                db.SaveChanges();
            }
            return Content(response);
        }

        
        // Добавление сотрудника
        public IActionResult OnGetAddWorker(string worker, string fio, string card, string position, string comment, string group)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            string response = "true";
            if (worker is null || fio is null || position is null || group is null)
            {
                return Content("Обязательные поля должны быть заполнены");
            }
            if (!worker.All(char.IsDigit))
            {
                return Content("ИДН должен быть числом");
            }
            using (ApplicationContext db = new ApplicationContext())
            {
                Workers? workerdb = db.Workers.FirstOrDefault(p => p.worker == int.Parse(worker));
                if (workerdb is not null)
                {
                    return Content("Такой сотрудник уже есть");
                }
                if (card is not null)
                {
                    if (!card.StartsWith("Em-Marine"))
                    {
                        return Content("неправильно введена карта");
                    }
                    Cards? carddb = db.Cards.FirstOrDefault(p => p.card == card);
                    if (carddb is not null)
                    {
                        if (carddb.worker is not null)
                        {
                            return Content("Эта карта уже занята");
                        }
                    }
                    else
                    {
                        carddb = new Cards();
                        carddb.card = card;
                        carddb.card16 = RefactCard(card);
                        carddb.worker = int.Parse(worker);
                        db.Cards.Add(carddb);
                        db.SaveChanges();
                    }
                }
                workerdb = new Workers();
                workerdb.worker = int.Parse(worker);
                workerdb.fio = fio;
                workerdb.position = position;
                workerdb.comment = comment;
                workerdb.group = group;
                workerdb.personalCheck = 0;
                workerdb.accessGroups = new List<string>();
                db.Workers.Add(workerdb);
                db.SaveChanges();

                if (card is not null)
                {
                    AddCardInWorker(workerdb, card);
                }
            }
            return Content(response);
        }

        
        // Добавления карты сотруднику с помощью кнопки
        public IActionResult OnGetAddCardInWorker(string card, int worker)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            string response = "true";
            if (card is null)
            {
                return Content("неправильно введена карта");
            }
            if (card.StartsWith("Em-Marine"))
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    Cards? carddb = db.Cards.FirstOrDefault(p => p.card == card);
                    Workers? workerdb = db.Workers.FirstOrDefault(p => p.worker == worker);
                    if (workerdb is null)
                    {
                        return Content("Нет такого сотрудника");
                    }
                    if (carddb is not null)
                    {
                        if (carddb.worker is not null)
                        {
                            return Content("Эта карта уже занята");
                        }
                    }
                    else
                    {
                        Cards newCard = new Cards();
                        newCard.card = card;
                        newCard.card16 = RefactCard(card);
                        db.Cards.Add(newCard);
                        db.SaveChanges();
                        response = "newcard";
                    }

                    AddCardInWorker(workerdb, card);  // добавление карты в список карт работника
                }
            }
            else
            {
                response = "неправильно введена карта";
            }
            return Content(response);
        }

        
        // Убирания карты сотрудника с помощью кнопки
        public IActionResult OnGetRemoveCardFromWorker(string cardId, int worker)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            string response = "true";
            if (cardId is null)
            {
                return Content("Карта не выбрана");
            }
            using (ApplicationContext db = new ApplicationContext())
            {
                int id = int.Parse(cardId);
                Cards? carddb = db.Cards.FirstOrDefault(p => p.Id == id);
                if (carddb is null)
                {
                    return Content("Нет такой карты");
                }
                if (carddb.worker is null || carddb.worker != worker)
                {
                    return Content("Карта не привязана к этому работнику");
                }

                carddb.worker = null;
                db.Cards.Update(carddb);
                db.SaveChanges();

                Workers? workerdb = db.Workers.FirstOrDefault(p => p.worker == worker);
                var controllersdb = db.Controllers.ToList();
                if (workerdb is null || controllersdb.Count == 0)
                {
                    return Content("true");
                }

                List<string> accessGroupsLocal;
                if (workerdb.personalCheck == 1)
                {
                    accessGroupsLocal = workerdb.accessGroups;
                }
                else
                {
                    var groupsdb = db.Groups.FirstOrDefault(p => p.group == workerdb.group);
                    if (groupsdb is null)
                    {
                        return Content("true");
                    }
                    accessGroupsLocal = groupsdb.accessGroups;
                }
                if (accessGroupsLocal.Count == 0)
                {
                    return Content("true");
                }

                foreach (Controllers controller in controllersdb)
                {
                    var relationsdb = db.RelationsControllersAccessGroups.Where(p => p.sn == controller.sn && accessGroupsLocal.Contains(p.accessGroup)).ToList();
                    if (relationsdb.Count > 0)
                    {
                        // Удаление из контроллера
                        DeleteCardFromController(controller.sn, carddb.card16);
                    }
                }               
            }
            return Content(response);
        }


        // Изменение индивидуальных групп доступа
        public IActionResult OnGetChangeAccessGroups(int workerId, string checkboxJson)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            var checkboxArr = JsonSerializer.Deserialize<string[]>(checkboxJson);
            var checkboxList = checkboxArr.ToList();
            using (ApplicationContext db = new ApplicationContext())
            {
                Workers workerdb = db.Workers.FirstOrDefault(p => p.Id == workerId);
                var accessGroupsOld = workerdb.accessGroups;
                workerdb.accessGroups = checkboxList;
                db.Workers.Update(workerdb);
                db.SaveChanges();

                if (workerdb.personalCheck == 0)
                {
                    return Content("true");
                }
                var controllersdb = db.Controllers.ToList();
                foreach (var controller in controllersdb)
                {
                    var relationOld = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == controller.sn && accessGroupsOld.Contains(p.accessGroup));
                    var relationNew = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == controller.sn && checkboxList.Contains(p.accessGroup));
                    // Удаление карт из контроллера
                    if (relationOld is not null && relationNew is null)
                    {
                        var cardsdb = db.Cards.Where(p => p.worker == workerdb.worker);
                        foreach (var card in cardsdb)
                        {
                            DeleteCardFromController(controller.sn, card.card16);
                        }
                    }
                    // Добавление карт в контроллер
                    if (relationOld is null && relationNew is not null)
                    {
                        var cardsdb = db.Cards.Where(p => p.worker == workerdb.worker);
                        foreach (var card in cardsdb)
                        {
                            AddCardInController(controller.sn, card.card16);
                        }
                    }
                }
            }
            return Content("true");
        }


        // Изменение переключателя индивидуальных групп доступа
        public IActionResult OnGetChangePersonalCheck(int workerId)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            using (ApplicationContext db = new ApplicationContext())
            {
                Workers workerdb = db.Workers.FirstOrDefault(p => p.Id == workerId);
                var groupdb = db.Groups.FirstOrDefault(p => p.group == workerdb.group);
                List<string> accessGroupNew;
                List<string> accessGroupOld;
                if (workerdb.personalCheck == 1)
                {
                    workerdb.personalCheck = 0;
                    accessGroupNew = groupdb.accessGroups;
                    accessGroupOld = workerdb.accessGroups;
                }
                else
                {
                    workerdb.personalCheck = 1;
                    accessGroupOld = groupdb.accessGroups;
                    accessGroupNew = workerdb.accessGroups;
                }
                db.Workers.Update(workerdb);
                db.SaveChanges();

                var controllersdb = db.Controllers.ToList();
                foreach (var contr in controllersdb)
                {
                    var relationNew = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == contr.sn && accessGroupNew.Contains(p.accessGroup));
                    var relationOld = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == contr.sn && accessGroupOld.Contains(p.accessGroup));

                    // Удаление карт из контроллера
                    if (relationOld is not null && relationNew is null)
                    {
                        var cardsdb = db.Cards.Where(p => p.worker == workerdb.worker);
                        foreach (var card in cardsdb)
                        {
                            DeleteCardFromController(contr.sn, card.card16);
                        }
                    }
                    // Добавление карт в контроллер
                    if (relationOld is null && relationNew is not null)
                    {
                        var cardsdb = db.Cards.Where(p => p.worker == workerdb.worker);
                        foreach (var card in cardsdb)
                        {
                            AddCardInController(contr.sn, card.card16);
                        }
                    }
                }
            }
            return Content("true");
        }


        // Редактирование сотрудника
        public IActionResult OnGetRedactWorker(int id, int workerNew, string fio, string position, string group, string comment)            
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            _logger.LogInformation($"{workerNew}, {fio}, {position}, {group}");
            if (workerNew == 0 || fio is null || position is null || group is null)
            {
                return Content("Обязательные поля должны быть заполнены");
            }
            using (ApplicationContext db = new ApplicationContext())
            {
                var workerdb = db.Workers.FirstOrDefault(p => p.Id == id);
                var groupOld = db.Groups.FirstOrDefault(p => p.group == workerdb.group);
                var groupNew = db.Groups.FirstOrDefault(p => p.group == group);
                if (workerdb is null)
                {
                    return Content("Нет такого сотрудника");
                }
                if(workerdb.worker != workerNew && db.Workers.FirstOrDefault(p => p.worker == workerNew) is not null)
                {
                    return Content("Этот ИДН занят");
                }
                if (groupNew is null)
                {
                    return Content("Нет такой группы");
                }
                workerdb.worker = workerNew;
                workerdb.fio = fio;
                workerdb.position = position;
                workerdb.group = group;
                workerdb.comment = comment;
                db.Workers.Update(workerdb);
                db.SaveChanges();

                if(workerdb.personalCheck == 1 || groupOld.group == group)
                {
                    return Content("true");
                }
                var accessGroupsOld = groupOld.accessGroups;
                var accessGroupsNew = groupNew.accessGroups;
                var controllersdb = db.Controllers.ToList();
                foreach(var contr in controllersdb)
                {
                    var relationOld = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == contr.sn && accessGroupsOld.Contains(p.accessGroup));
                    var relationNew = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == contr.sn && accessGroupsNew.Contains(p.accessGroup));
                    var cardsdb = db.Cards.Where(p => p.worker == workerNew);
                    foreach (var card in cardsdb) {
                        if (relationOld is null && relationNew is not null)
                        {
                            AddCardInController(contr.sn, card.card16);
                        }
                        if (relationOld is not null && relationNew is null)
                        {
                            DeleteCardFromController(contr.sn, card.card16);
                        } 
                    }
                }
            }
            return Content("true");
        }


        // Добавление карты
        public IActionResult OnGetAddCard(string card, string worker)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            string response = "true";
            if (card is null)
            {
                return Content("неправильно введена карта");
            }
            if (card.StartsWith("Em-Marine"))
            {
                Cards newCard = new Cards();
                newCard.card = card;
                newCard.card16 = RefactCard(card);
                using (ApplicationContext db = new ApplicationContext())
                {
                    Cards? carddb = db.Cards.FirstOrDefault(p => p.card == card);
                    Workers? workerdb = new Workers();
                    if (carddb is not null)
                    {
                        if (carddb.worker is not null)
                        {
                            return Content("Эта карта уже занята");
                        }
                    }
                    else
                    {
                        db.Cards.Add(newCard);
                        db.SaveChanges();
                    }
                    if (worker is not null)     // проверка есть ли такой работник в БД
                    {
                        workerdb = db.Workers.FirstOrDefault(p => p.worker == int.Parse(worker));
                        if (workerdb is null)
                        {
                            return Content("Нет такого сотрудника");
                        }
                        AddCardInWorker(workerdb, card);  // добавление карты в список карт работника
                    }
                }
            }
            else
            {
                response = "неправильно введена карта";
            }
            return Content(response);
        }


        // добавление карты в список карт сотрудника
        public void AddCardInWorker(Workers workerdb, string card)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return;
            }
            using (ApplicationContext db = new ApplicationContext())
            {
                Cards carddb = db.Cards.FirstOrDefault(p => p.card == card);
                carddb.worker = workerdb.worker;
                db.Cards.Update(carddb);
                db.SaveChanges();

                List<string> accessGroups;
                if (workerdb.personalCheck == 1)
                {
                    accessGroups = workerdb.accessGroups;
                }
                else
                {
                    var groupdb = db.Groups.FirstOrDefault(p => p.group == workerdb.group);
                    accessGroups = groupdb.accessGroups;
                }

                // добавление карты в контроллер
                var controllers = db.Controllers.ToList();
                if (controllers.Count == 0)
                {
                    return;
                }
                foreach (Controllers controller in controllers)
                {
                    var relationsdb = db.RelationsControllersAccessGroups.Where(p => p.sn == controller.sn && accessGroups.Contains(p.accessGroup)).ToList();
                    if (relationsdb.Count > 0)
                    {
                        AddCardInController(controller.sn, carddb.card16);
                    }
                }
            }
        }

        
        // добавление группы
        public IActionResult OnGetAddGroup(string group)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            using (ApplicationContext db = new ApplicationContext())
            {
                Groups groupdb = db.Groups.FirstOrDefault(p => p.group.ToLower() == group.ToLower());
                if (groupdb is not null)
                {
                    return Content("Такая группа уже есть");
                }
                Groups newGroup = new Groups();
                newGroup.group = group;
                newGroup.accessGroups = new List<string>();
                db.Groups.Add(newGroup);
                db.SaveChanges();
                return Content("true");
            }
        }

        
        // Показывает форму добавления группы доступа в группу
        public IActionResult OnGetAccessGroupInAddGroupForm(int groupId)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            using (ApplicationContext db = new ApplicationContext())
            {
                Groups? groupdb = db.Groups.FirstOrDefault(p => p.Id == groupId);
                return Content(JsonSerializer.Serialize(groupdb.accessGroups));                
            }
        }

        
        // Добавление группы доступа в группу
        public IActionResult OnGetAddAccessGroupInGroup(int groupId, string accessGroup)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            if (accessGroup is null)
            {
                return Content("Выберите группу доступа");
            }
            string response = "true";
            using (ApplicationContext db = new ApplicationContext())
            {
                Groups? groupdb = db.Groups.FirstOrDefault(p => p.Id == groupId);
                AccessGroups? accessGroupdb = db.AccessGroups.FirstOrDefault(p => p.accessGroup == accessGroup);
                if (groupdb is null)
                {
                    return Content("Нет такой группы");
                }
                if (accessGroupdb is null)
                {
                    return Content("Нет такой группы доступа");
                }                

                var workersdb = db.Workers.Where(p => p.group == groupdb.group && p.personalCheck == 0).ToList();
                var controllersdb = db.Controllers.ToList();
                foreach (var controller in controllersdb)
                {
                    var relationNew = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == controller.sn && p.accessGroup == accessGroupdb.accessGroup);
                    var relationOld = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == controller.sn && groupdb.accessGroups.Contains(p.accessGroup));
                    if (relationNew is not null && relationOld is null)
                    {
                        foreach (var worker in workersdb)
                        {
                            var cardsdb = db.Cards.Where(p => p.worker == worker.worker).ToList();
                            foreach (var card in cardsdb)
                            {
                                AddCardInController(controller.sn, card.card16);
                            }
                        }
                    }

                }
                groupdb.accessGroups.Add(accessGroupdb.accessGroup);
                db.Groups.Update(groupdb);
                db.SaveChanges();
            }
            return Content(response);
        }

        
        // Убирание группы доступа из группы
        public IActionResult OnGetRemoveAccessGroupFromGroup(int groupId, string accessGroup)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            string response = "true";
            using (ApplicationContext db = new ApplicationContext())
            {
                Groups? groupdb = db.Groups.FirstOrDefault(p => p.Id == groupId);
                if (groupdb is null)
                {
                    return Content("Нет такой группы");
                }
                if (!groupdb.accessGroups.Contains(accessGroup))
                {
                    return Content("Нет такой группы доступа");
                }
                groupdb.accessGroups.Remove(accessGroup);
                db.Groups.Update(groupdb);
                db.SaveChanges();

                var workersdb = db.Workers.Where(p => p.group == groupdb.group && p.personalCheck == 0).ToList();
                if (workersdb.Count == 0)
                {
                    return Content("true");
                }
                var controllersdb = db.Controllers.ToList();
                foreach (var controller in controllersdb)
                {
                    var relationOld = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == controller.sn && p.accessGroup == accessGroup);
                    var relationNew = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == controller.sn && groupdb.accessGroups.Contains(p.accessGroup));
                    if (relationOld is null || relationNew is not null)
                    {
                        continue;
                    }
                    foreach (var worker in workersdb)
                    {
                        var cardsdb = db.Cards.Where(p => p.worker == worker.worker).ToList();
                        foreach (var card in cardsdb)
                        {
                            DeleteCardFromController(controller.sn, card.card16);
                        }                        
                    }
                }
            }
            return Content(response);
        }


        // добавление группы доступа
        public IActionResult OnGetAddAccessGroup(string accessGroup)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            string response = "Такая группа доступа уже есть";
            using (ApplicationContext db = new ApplicationContext())
            {
                AccessGroups accessGroupdb = db.AccessGroups.FirstOrDefault(p => p.accessGroup.ToLower() == accessGroup.ToLower());
                if (accessGroupdb is null)
                {
                    AccessGroups newGroup = new AccessGroups();
                    newGroup.accessGroup = accessGroup;
                    db.AccessGroups.Add(newGroup);
                    db.SaveChanges();
                    response = "true";
                }
            }
            return Content(response);
        }

        
        // добавление связи контроллер - группа доступа
        public IActionResult OnGetAddRelationsControllersAccessGroups(string? sn, string? accessGroup, bool cbIn, bool cbOut)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            string response = "true";
            if (!cbIn && !cbOut)
            {
                return Content("Не выбран вход или выход");
            }
            if (sn is null || accessGroup is null)
            {
                return Content("Обязательные поля должны быть заполнены");
            }
            if (!sn.All(char.IsDigit))
            {
                return Content("Серийный номер контроллера должен быть числом");
            }
            var snInt = int.Parse(sn);
            using (ApplicationContext db = new ApplicationContext())
            {
                Controllers? controllerdb = db.Controllers.FirstOrDefault(p => p.sn == snInt);
                AccessGroups? accessGroupdb = db.AccessGroups.FirstOrDefault(p => p.accessGroup == accessGroup);
                if (controllerdb is null)
                {
                    return Content("Нет такого контроллера");
                }
                if (accessGroupdb is null)
                {
                    return Content("Нет такой группы доступа");
                }

                List<RelationsControllersAccessGroups> relations = db.RelationsControllersAccessGroups.Where(p => p.sn == snInt && p.accessGroup == accessGroup).ToList();
               
                if (relations.Count == 2)
                {
                    return Content("Оба входа контроллера уже привязаны к этой группе доступа");
                }
                foreach(var rel in relations)
                {
                    if (cbIn && rel.reader == 1)
                    {
                        return Content("Вход контроллера уже привязаны к этой группе доступа");
                    }
                    if (cbOut && rel.reader == 2)
                    {
                        return Content("Выход контроллера уже привязаны к этой группе доступа");
                    }
                }

                if (relations.Count == 0)
                {
                    var workersdb = db.Workers.ToList();
                    var groupsdb = db.Groups.Where(p => p.accessGroups.Contains(accessGroup)).ToList();
                    List<string> accessGroupsLocal;
                    foreach (var worker in workersdb)
                    {
                        if (worker.personalCheck == 1)
                        {
                            if (!worker.accessGroups.Contains(accessGroup))
                            {
                                continue;
                            }
                            accessGroupsLocal = worker.accessGroups;
                        }
                        else
                        {
                            if (!groupsdb.Exists(p => p.group == worker.group))
                            {
                                continue;
                            }
                            accessGroupsLocal = groupsdb.Find(p => p.group == worker.group).accessGroups;
                        }
                        var relationNew = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == snInt && accessGroupsLocal.Contains(p.accessGroup));
                        if (relationNew != null)
                        {
                            continue;
                        }
                        var cardsdb = db.Cards.Where(p => p.worker == worker.worker).ToList();
                        foreach (var card in cardsdb)
                        {
                            AddCardInController(snInt, card.card16);
                        }
                    }

                }

                if (cbIn)
                {
                    RelationsControllersAccessGroups newRelation = new RelationsControllersAccessGroups();
                    newRelation.sn = snInt;
                    newRelation.accessGroup = accessGroup;
                    newRelation.reader = 1;
                    db.RelationsControllersAccessGroups.Add(newRelation);
                    db.SaveChanges();
                }
                if (cbOut)
                {
                    RelationsControllersAccessGroups newRelation = new RelationsControllersAccessGroups();
                    newRelation.sn = snInt;
                    newRelation.accessGroup = accessGroup;
                    newRelation.reader = 2;
                    db.RelationsControllersAccessGroups.Add(newRelation);
                    db.SaveChanges();
                }                
            }
            return Content(response);
        }        


        // добавление карты в контроллер
        public void AddCardInController(int sn, string card16)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                MessagesDB mesdb = new MessagesDB
                {
                    operation = "add_cards",
                    card = card16,
                    flag = 0,
                    tz = 255
                };

                MessagesDB mesdb2 = new MessagesDB
                {
                    operation = "wait"
                };
                mesdb.sn = sn;
                db.Messages.Add(mesdb);
                mesdb2.sn = sn;
                db.Messages.Add(mesdb2);

                db.SaveChanges();
            }
        }


        // Считывание карты
        public IActionResult OnGetReadCard()
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            SerialPort serialPort = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
            serialPort.Handshake = Handshake.None;

            serialPort.Open();
            string fullInputString = serialPort.ReadLine();
            serialPort.Close();

            return Content(fullInputString);
        }


        // Преобразование кода карты в шестнадцатеричный вид
        public string RefactCard(string fullInputString)
        {
            string resultCard = "000000"; //+ fullInputString.Substring(10, 4);
            string secondPath = fullInputString.Substring(16);
            string[] str = secondPath.Split(new char[] { ',' });
            string firstBits = Convert.ToString(int.Parse(str[0]), 16).ToUpper();
            string secondBits = Convert.ToString(int.Parse(str[1]), 16).ToUpper();
            while (firstBits.Length < 2)
            {
                firstBits = "0" + firstBits;
            }
            while (secondBits.Length < 4)
            {
                secondBits = "0" + secondBits;
            }
            resultCard = resultCard + firstBits + secondBits;
            return resultCard;
        }


        // фильтрация карт
        public IActionResult OnGetFilterCard(string filter)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            using (ApplicationContext db = new ApplicationContext())
            {
                List<int> res = new List<int>();
                var filterCards = db.Cards.ToList();
                if (filter is null)
                {
                    return Content("false");
                } 
                foreach (Cards? filterCard in filterCards)
                {               
                    if (filterCard.card.IndexOf(filter) != -1 || 
                        filterCard.card16.IndexOf(filter) != -1 || 
                        (filterCard.worker is not null && filterCard.worker.ToString().IndexOf(filter) != -1))
                    {
                        res.Add(filterCard.Id);
                    }       
                }
                return Content(JsonSerializer.Serialize(res));
            }
        }


        // фильтрация сотрудников
        public IActionResult OnGetFilterWorker(string filter)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            using (ApplicationContext db = new ApplicationContext())
            {
                //List<int> res = new List<int>();
                List<string> res = new List<string>();
                var filterWorkers = db.Workers.ToList();
                if (filter is null)
                {
                    return Content("false");
                }
                foreach (Workers? filterWorker in filterWorkers)
                {
                    if (filterWorker.worker.ToString().IndexOf(filter) != -1 ||
                        filterWorker.fio.IndexOf(filter) != -1 ||
                        filterWorker.position.IndexOf(filter) != -1 ||
                        filterWorker.group.IndexOf(filter) != -1)
                    {
                        var group = db.Groups.FirstOrDefault(p => p.group == filterWorker.group);
                        res.Add(filterWorker.Id.ToString()+";"+ group.Id);
                    }
                }
                return Content(JsonSerializer.Serialize(res));
            }
        }


        // фильтрация связи контроллер - группа доступа
        public IActionResult OnGetFilterRelationsControllersAccessGroups(string filter)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            using (ApplicationContext db = new ApplicationContext())
            {
                List<int> res = new List<int>();
                List<RelationsControllersAccessGroups> filterRelations = db.RelationsControllersAccessGroups.ToList();
                if (filter is null)
                {
                    return Content("false");
                }
                foreach (RelationsControllersAccessGroups? filterRelation in filterRelations)
                {
                    if (filterRelation.sn.ToString().IndexOf(filter) != -1 ||
                        filterRelation.accessGroup.IndexOf(filter) != -1)
                    {
                        res.Add(filterRelation.Id);
                    }
                }
                return Content(JsonSerializer.Serialize(res));
            }
        }      
    }
}