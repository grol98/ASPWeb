using Aspose.Pdf;
using Aspose.Pdf.Text;
using ASPWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Reflection.Metadata;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ASPWeb.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationContext dbcontext;
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
        public List<RelationsControllersAccessGroups> relationsControllersAccessGroups { get; set; }
        public List<RelationsControllersWorkers> relationsControllersWorkers { get; set; }
        public IFormFile? formFile { get; set; }

        private Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment;
        public DataManager _dataManager;

        public IndexModel(ILogger<IndexModel> logger, ApplicationContext db, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, DataManager dataManager)
        {
            var prevDay = DateTime.UtcNow.AddDays(-1);
            _logger = logger;
            users = db.Users.OrderBy(p => p.Id).ToList();
            cards = db.Cards.OrderByDescending(p => p.Id).ToList();
            emptyCards = cards.Where(p => p.workerId == null).OrderBy(p => p.card).ToList();
            controllers = db.Controllers.OrderBy(p => p.Id).ToList();
            events = db.Events.Where(p => p.dateTime > prevDay).OrderByDescending(p => p.Id).Include(p => p.worker).ToList();
            eventCodes = db.EventCodes.OrderBy(p => p.Id).ToList();
            workers = db.Workers.OrderBy(p => p.LastName).Include(p => p.cards).ToList();
            groups = db.Groups.OrderBy(p => p.group).ToList();
            accessGroups = db.AccessGroups.OrderBy(p => p.Id).ToList();
            relationsControllersAccessGroups = db.RelationsControllersAccessGroups.OrderBy(p => p.sn).ThenBy(p => p.accessGroup).ToList();
            relationsControllersWorkers = db.RelationsControllersWorkers.OrderBy(p => p.sn).ThenBy(p => p.workerId).ToList();
            hostingEnvironment = environment;
            dbcontext = db;
            _dataManager = dataManager;
        }

        ~IndexModel() 
        {
            dbcontext.Dispose();
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

        public IActionResult OnGetLogOn(string login, string password, string tab)
        {
            if (tab != null)
            {
                HttpContext.Session.SetString("tab", tab);
            }
            Users? user = dbcontext.Users.FirstOrDefault(p => p.login == login && p.password == password);
            if (user is null)
            {
                return Content("Логин и/или пароль не правильно введены");
            }
            HttpContext.Session.SetString("logged", "true");
            HttpContext.Session.SetString("userLogin", user.login);
            HttpContext.Session.SetString("userId", user.Id.ToString());
            return Content("true");
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
            Users? user = dbcontext.Users.FirstOrDefault(p => p.Id == id);
            dbcontext.Users.Remove(user);
            _logger.LogInformation($"deleted user id: {user.Id}; login: {user.login}");

            dbcontext.SaveChanges();
        }
        }


        // Удаление сотрудника
        public void DeleteWorker(int id)
        {
            Workers? workerdb = dbcontext.Workers.FirstOrDefault(p => p.Id == id);
            if (workerdb is null)
            {
                Workers? workerdb = db.Workers.FirstOrDefault(p => p.Id == id);
                var controllersdb = db.Controllers.ToList();
                if (workerdb is null || controllersdb.Count == 0)
                {
                return;
            }

            List<int> controllersSn = new List<int>();
            var accessPoints = dbcontext.RelationsControllersWorkers.Where(p => p.workerId == workerdb.Id).ToList();
            if (workerdb.personalCheck == 1)
            {
                var groupsdb = dbcontext.Groups.FirstOrDefault(p => p.group == workerdb.group);
                if (groupsdb is null) { return; }
                controllersSn = GetControllersListByAccessGroups(groupsdb.accessGroups);
            }
            if (workerdb.personalCheck == 2) { controllersSn = GetControllersListByAccessGroups(workerdb.accessGroups); }
            if (workerdb.personalCheck == 3) { controllersSn = GetControllersListByRelatinsControllerWorker(accessPoints); }
            dbcontext.RelationsControllersWorkers.RemoveRange(accessPoints);

            if (workerdb.Image != null)
            {
                RemoveImage(workerdb.Image);
            }
            dbcontext.Workers.Remove(workerdb);

            var cardsdb = dbcontext.Cards.Where(p => p.workerId == workerdb.Id).ToList();
            foreach (Cards card in cardsdb) // удаление сотрудника из всех карт
            {
                card.workerId = null;
                dbcontext.Cards.Update(card);
            }
                db.Workers.Remove(workerdb);

            var controllersdb = dbcontext.Controllers.ToList();
            foreach (var contr in controllersdb)
            {
                // Удаление карт из контроллера
                if (controllersSn.Contains(contr.sn))
                {
                    foreach (var card in cardsdb)
                    {
                        DeleteCardFromController(contr.sn, card.card16);
                    }
                }
            }
                foreach (Cards card in cardsdb) // удаление сотрудника из всех карт
                {
                    card.worker = null;
                    db.Cards.Update(card);
                }

            _logger.LogInformation($"deleted worker: {JsonSerializer.Serialize(workerdb)}");
            dbcontext.SaveChanges();
        }

        public IActionResult OnGetDeleteImage(int workerId)
        {
            var workerdb = _dataManager.workers.GetWorkerById(workerId);
            if (workerdb is null)
            {
                return Content("Нет такого сотрудника");     
            }
            if (workerdb.Image == null)
            {
                return Content("Нет такой фотографии");
            }
            RemoveImage(workerdb.Image);
            workerdb.Image = null;
            dbcontext.Workers.Update(workerdb);
            dbcontext.SaveChanges();
            return Content("true");
        }


        // Удаление карты
        public void DeleteCard(int id)
        {
            Cards? card = dbcontext.Cards.FirstOrDefault(p => p.Id == id);
            if (card is not null)
            {
                dbcontext.Cards.Remove(card);
            }
            if (card.workerId is null)
            {
                dbcontext.SaveChanges();
                return;
            }
            Workers? workerdb = dbcontext.Workers.FirstOrDefault(p => p.Id == card.workerId);
            if (workerdb is null)
            {
                return;
            }

            List<int> controllersSn = new List<int>();
            var accessPoints = dbcontext.RelationsControllersWorkers.Where(p => p.workerId == workerdb.Id).ToList();
            if (workerdb.personalCheck == 1)
            {
                var groupsdb = dbcontext.Groups.FirstOrDefault(p => p.group == workerdb.group);
                if (groupsdb is null) { return; }
                controllersSn = GetControllersListByAccessGroups(groupsdb.accessGroups);
            }
            if (workerdb.personalCheck == 2) { controllersSn = GetControllersListByAccessGroups(workerdb.accessGroups); }
            if (workerdb.personalCheck == 3) { controllersSn = GetControllersListByRelatinsControllerWorker(accessPoints); }
            dbcontext.RelationsControllersWorkers.RemoveRange(accessPoints);

            var controllersdb = dbcontext.Controllers.ToList();
            foreach (var contr in controllersdb)
            {
                // Удаление карт из контроллера
                if (controllersSn.Contains(contr.sn))
                {
                    DeleteCardFromController(contr.sn, card.card16);
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

            dbcontext.SaveChanges();
            _logger.LogInformation($"deleted card: {JsonSerializer.Serialize(card)}");

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
                var relationsAccessGroups = db.RelationsControllersAccessGroups.Where(p => p.sn == controller.sn).ToList();
                var relationsWorkers = db.RelationsControllersWorkers.Where(p => p.sn == controller.sn).ToList();
                db.RelationsControllersAccessGroups.RemoveRange(relationsAccessGroups);
                db.RelationsControllersWorkers.RemoveRange(relationsWorkers);
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
                var groupsdb = db.Groups.Where(p => p.accessGroups.Contains(accessGroup.accessGroup)).ToList();
                var workersdb = db.Workers.Where(p => p.accessGroups.Contains(accessGroup.accessGroup) ||
                    (groupsdb.Count > 0 && groupsdb.Exists(d => d.group == p.group))).ToList();

                foreach (var contr in controllers)
                {
                    int cardCount = 0;
                    List<string> cardList = new List<string>();
                    foreach (var worker in workersdb)
                    {
                        // Поиск старого и нового списков контролеров
                        List<int> controllersSnOld = new List<int>();
                        List<int> controllersSnNew = new List<int>();
                        if (worker.personalCheck == 1)
                        {
                            var groupdb = db.Groups.FirstOrDefault(p => p.group == worker.group);
                            controllersSnOld = GetControllersListByAccessGroups(groupdb.accessGroups);
                        }
                        if (worker.personalCheck == 2) { controllersSnOld = GetControllersListByAccessGroups(worker.accessGroups); }
                        if (worker.accessGroups.Contains(accessGroup.accessGroup))
                        {
                            worker.accessGroups.Remove(accessGroup.accessGroup);
                        }
                        worker.accessGroups.Remove(accessGroup.accessGroup);
                        db.Workers.Update(worker);
                        if (worker.personalCheck == 2) { controllersSnNew = GetControllersListByAccessGroups(worker.accessGroups); }


                        // Удаление карт из контроллера
                        if (controllersSnOld.Contains(contr.sn) && !controllersSnNew.Contains(contr.sn))
                        {
                            
                            var cardsdb = db.Cards.Where(p => p.workerId == worker.Id);
                            foreach (var card in cardsdb)
                            {
                                cardCount++;
                                cardList.Add(card.card16);
                                if (cardCount == 10)
                                {
                                    DeleteCardFromControllerRange(contr.sn, cardList);
                                    cardList = new List<string>();
                                    cardCount = 0;
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
                    DeleteCardFromControllerRange(contr.sn, cardList);
                }
                }
                var groupsdb = db.Groups.Where(p => p.accessGroups.Contains(accessGroup.accessGroup)).ToList();
                foreach (var group in groupsdb)
                {
                    group.accessGroups.Remove(accessGroup.accessGroup);
                    db.Groups.Update(group);
                }
                var relationsdb = db.RelationsControllersAccessGroups.Where(p => p.accessGroup == accessGroup.accessGroup).ToList();
                foreach (var rel in relationsdb)
                {
                    db.RelationsControllersAccessGroups.Remove(rel);
                }
                db.AccessGroups.Remove(accessGroup);
                db.SaveChanges();
                return "true";
            }
        }


        // Удаление связи контроллер - группа доступа
        public void DeleteRelationsControllersAccessGroups(int id)
        {
            RelationsControllersAccessGroups? relation = dbcontext.RelationsControllersAccessGroups.FirstOrDefault(p => p.Id == id);
            int sn = relation.sn;
            int relCount = dbcontext.RelationsControllersAccessGroups.Where(p => p.sn == sn && p.accessGroup == relation.accessGroup).Count();
           
            dbcontext.RelationsControllersAccessGroups.Remove(relation);
            dbcontext.SaveChanges();

            var groupsdb = dbcontext.Groups.Where(p => p.accessGroups.Contains(relation.accessGroup)).ToList();
            var groupNames = groupsdb.Select(c => c.group).ToList();
            if (relCount == 1)
            {
                var workersdb = dbcontext.Workers.Where(p => p.personalCheck != 3 &&
                    (p.accessGroups.Contains(relation.accessGroup) ||
                    groupNames.Contains(p.group))).Include(p => p.cards).ToList();

                int cardCount = 0;
                List<int> controllersSn;
                List<string> cardList = new List<string>();
                foreach (var worker in workersdb)
                {
                    // Поиск нового списков контролеров
                    controllersSn = new List<int>();
                    if (worker.personalCheck == 1)
                    {
                        var groupdb = groupsdb.FirstOrDefault(p => p.group == worker.group);
                        controllersSn = GetControllersListByAccessGroups(groupdb.accessGroups);
                    }
                    if (worker.personalCheck == 2)
                    {
                        controllersSn = GetControllersListByAccessGroups(worker.accessGroups);
                    }

                    if (!controllersSn.Contains(sn))
                    {
                        foreach (var card in worker.cards)
                        {
                            cardCount++;
                            cardList.Add(card.card16);
                            if (cardCount == 10)
                            {
                                DeleteCardFromControllerRange(sn, cardList);
                                cardList = new List<string>();
                                cardCount = 0;
                            }
                    var cardsdb = db.Cards.Where(p => p.worker == worker.worker).ToList();
                    foreach (var card in cardsdb)
                    {
                        DeleteCardFromController(sn, card.card16);
                        }
                    }
                }
                DeleteCardFromControllerRange(sn, cardList);
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
        public IActionResult OnGetAddWorker(int worker, string LastName, string FirstName, string FatherName, string card, string position, string comment, string group, string image)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            string response = "true";
            if (LastName is null || group is null)
            {
                return Content("Обязательные поля должны быть заполнены");
            }
            if (!worker.All(char.IsDigit))
            {
                return Content("ИДН должен быть числом");
            }
            using (ApplicationContext db = new ApplicationContext())
            {
                Workers? workerdb = db.Workers.FirstOrDefault(p => worker != 0 && p.worker == worker);
                if (workerdb is not null)
                {
                    return Content("Такой сотрудник уже есть");
                }
                if (card is not null)
                {
                    if (card.StartsWith("Em-Marine"))
                    {
                        card = card.Substring(16);
                    }
                    string[] inputStr = card.Split(new char[] { ',' });
                    if (inputStr[0].Length != 3 || inputStr[1].Length != 5 || !inputStr[0].All(char.IsDigit) || !inputStr[1].All(char.IsDigit))
                    {
                        return Content("неправильно введена карта");
                    }
                    Cards? carddb = db.Cards.FirstOrDefault(p => p.card == card);
                    if (carddb is not null)
                    {
                        if (carddb.workerId is not null)
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
                if (image != "empty")
                {
                    workerdb.Image = image;
                }
                workerdb.worker = worker;
                workerdb.LastName = LastName;
                workerdb.FirstName = FirstName;
                workerdb.FatherName = FatherName;
                workerdb.position = position;
                workerdb.comment = comment;
                workerdb.group = group;
                workerdb.personalCheck = 1;
                workerdb.accessGroups = new List<string>();
                db.Workers.Add(workerdb);
                db.SaveChanges();

                workerdb = db.Workers.OrderByDescending(p => p.Id).FirstOrDefault();
                if (card is not null)
                {
                    AddCardInWorker(workerdb, card);
                }
            }
            return Content(response);
        }


        // Добавления карты сотруднику с помощью кнопки
        public IActionResult OnGetAddCardInWorker(string card, int workerId)
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
                card = card.Substring(16);
            }
            string[] inputStr = card.Split(new char[] { ',' });
            if (inputStr[0].Length != 3 || inputStr[1].Length != 5 || !inputStr[0].All(char.IsDigit) || !inputStr[1].All(char.IsDigit))
            {
                return Content("неправильно введена карта");
            }

            Cards? carddb = dbcontext.Cards.FirstOrDefault(p => p.card == card);
            Workers? workerdb = dbcontext.Workers.FirstOrDefault(p => p.Id == workerId);
            if (workerdb is null)
            {
                return Content("Нет такого сотрудника");
            }
            if (carddb is not null)
            {
                if (carddb.workerId is not null)
                {
                    return Content("Эта карта уже занята");
                }
            }
            else
            {
                Cards newCard = new Cards();
                newCard.card = card;
                newCard.card16 = RefactCard(card);
                dbcontext.Cards.Add(newCard);
                dbcontext.SaveChanges();
                response = "newcard";
            }

            AddCardInWorker(workerdb, card);  // добавление карты в список карт работника

            return Content(response);
        }


        // Убирания карты сотрудника с помощью кнопки
        public IActionResult OnGetRemoveCardFromWorker(string card, int workerId)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            if (card is null) { return Content("Карта не выбрана"); }

            Cards? carddb = dbcontext.Cards.FirstOrDefault(p => p.card == card);
            if (carddb is null) { return Content("Нет такой карты"); }
            if (carddb.workerId is null || carddb.workerId != workerId) { return Content("Карта не привязана к этому работнику"); }

            carddb.workerId = null;
            dbcontext.Cards.Update(carddb);
            dbcontext.SaveChanges();

            Workers? workerdb = dbcontext.Workers.FirstOrDefault(p => p.Id == workerId);
            if (workerdb is null) { return Content("true"); }

            // Поиск списка контролеров
            List<int> controllersSn = new List<int>();
            if (workerdb.personalCheck == 1)
            {
                var groupsdb = dbcontext.Groups.FirstOrDefault(p => p.group == workerdb.group);
                if (groupsdb is null) { return Content("true"); }
                controllersSn = GetControllersListByAccessGroups(groupsdb.accessGroups);
            }
            if (workerdb.personalCheck == 2) { controllersSn = GetControllersListByAccessGroups(workerdb.accessGroups); }
            if (workerdb.personalCheck == 3)
            {
                var accessPoints = dbcontext.RelationsControllersWorkers.Where(p => p.workerId == workerId).ToList();
                controllersSn = GetControllersListByRelatinsControllerWorker(accessPoints);
            }

            var controllersdb = dbcontext.Controllers.Where(p => controllersSn.Contains(p.sn)).ToList();
            foreach (var contr in controllersdb)
            {
                // Удаление карт из контроллера
                DeleteCardFromController(contr.sn, carddb.card16);
            }
            return Content("true");
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

            Workers workerdb = dbcontext.Workers.FirstOrDefault(p => p.Id == workerId);
            var accessGroupsOld = workerdb.accessGroups;
            workerdb.accessGroups = checkboxList;
            dbcontext.Workers.Update(workerdb);
            dbcontext.SaveChanges();

            if (workerdb.personalCheck == 1 || workerdb.personalCheck == 3)
            {
                return Content("true");
            }

            var controllersSnOld = GetControllersListByAccessGroups(accessGroupsOld);
            var controllersSnNew = GetControllersListByAccessGroups(workerdb.accessGroups);

            var controllersdb = dbcontext.Controllers.ToList();
            foreach (var contr in controllersdb)
            {
                // Удаление карт из контроллера
                if (controllersSnOld.Contains(contr.sn) && !controllersSnNew.Contains(contr.sn))
                {
                    var cardsdb = dbcontext.Cards.Where(p => p.workerId == workerdb.Id);
                    foreach (var card in cardsdb)
                    {
                        DeleteCardFromController(contr.sn, card.card16);
                    }
                if (accessGroupsLocal.Count == 0)
                {
                    return Content("true");
                }
                // Добавление карт в контроллер
                if (!controllersSnOld.Contains(contr.sn) && controllersSnNew.Contains(contr.sn))
                {
                    var cardsdb = dbcontext.Cards.Where(p => p.workerId == workerdb.Id);
                    foreach (var card in cardsdb)
                    {
                        AddCardInController(contr.sn, card.card16);
                    }
                }
            }
            return Content("true");
        }


        // Изменение индивидуальных точек доступа
        public IActionResult OnGetChangeAccessPoints(int workerId, string checkboxJson)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            var checkboxArr = JsonSerializer.Deserialize<string[]>(checkboxJson);
            var checkboxList = checkboxArr.ToList();
            var relationPointsOld = dbcontext.RelationsControllersWorkers.Where(p => p.workerId == workerId).ToList();
            var relationPointsRemove = relationPointsOld;
            var workerdb = dbcontext.Workers.FirstOrDefault(p => p.Id == workerId);

            foreach (var checkbox in checkboxList)
            {
                string[] inputStr = checkbox.Split(new char[] { '_' });
                int sn = int.Parse(inputStr[0]);
                int reader = int.Parse(inputStr[1]);
                var relationPoint = relationPointsOld.FirstOrDefault(p => p.sn == sn && p.reader == reader);
                if (relationPoint == null)
                {
                    RelationsControllersWorkers rel = new RelationsControllersWorkers();
                    rel.sn = sn;
                    rel.workerId = workerId;
                    rel.reader = reader;
                    dbcontext.RelationsControllersWorkers.Add(rel);
                }
                else
                {
                    relationPointsRemove.Remove(relationPoint);
                }
            }
            foreach (var relOld in relationPointsRemove)
            {
                dbcontext.RelationsControllersWorkers.Remove(relOld);
            }
            dbcontext.SaveChanges();

            if (workerdb.personalCheck == 3)
            {
                var relationPointsNew = dbcontext.RelationsControllersWorkers.Where(p => p.workerId == workerId).ToList();
                foreach (var controller in controllers)
                {
                    var relNew = relationPointsNew.FirstOrDefault(p => p.sn == controller.sn);
                    var relOld = relationPointsOld.FirstOrDefault(p => p.sn == controller.sn);

                    if (relOld is not null && relNew is null)
                    {
                        var cardsdb = dbcontext.Cards.Where(p => p.workerId == workerId);
                        foreach (var card in cardsdb)
                        {
                            DeleteCardFromController(controller.sn, card.card16);
                        }
                    }
                    // Добавление карт в контроллер
                    if (relOld is null && relNew is not null)
                    {
                        var cardsdb = dbcontext.Cards.Where(p => p.workerId == workerId);
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
        public IActionResult OnGetChangePersonalCheck(int workerId, int personalCheck)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }
            using (ApplicationContext db = new ApplicationContext())
            {
                Workers workerdb = db.Workers.FirstOrDefault(p => p.Id == workerId);
                if (workerdb.personalCheck == personalCheck)
                {
                    return Content("true");
                }
                var groupdb = db.Groups.FirstOrDefault(p => p.group == workerdb.group);
                List<int> controllersSnOld = new List<int>();
                List<int> controllersSnNew = new List<int>();
                if (workerdb.personalCheck == 1) { controllersSnOld = GetControllersListByAccessGroups(groupdb.accessGroups); }
                if (workerdb.personalCheck == 2) { controllersSnOld = GetControllersListByAccessGroups(workerdb.accessGroups); }
                if (workerdb.personalCheck == 3) { controllersSnOld = GetControllersListByRelatinsControllerWorker(db.RelationsControllersWorkers.Where(p => p.workerId == workerdb.Id).ToList()); }

                if (personalCheck == 1) { controllersSnNew = GetControllersListByAccessGroups(groupdb.accessGroups); }
                if (personalCheck == 2) { controllersSnNew = GetControllersListByAccessGroups(workerdb.accessGroups); }
                if (personalCheck == 3) { controllersSnNew = GetControllersListByRelatinsControllerWorker(db.RelationsControllersWorkers.Where(p => p.workerId == workerdb.Id).ToList()); }

                workerdb.personalCheck = personalCheck;
                db.Workers.Update(workerdb);
                db.SaveChanges();

                var controllersdb = db.Controllers.ToList();
                foreach (var contr in controllersdb)
                {
                    var relationNew = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == contr.sn && accessGroupNew.Contains(p.accessGroup));
                    var relationOld = db.RelationsControllersAccessGroups.FirstOrDefault(p => p.sn == contr.sn && accessGroupOld.Contains(p.accessGroup));

                    // Удаление карт из контроллера
                    if (controllersSnOld.Contains(contr.sn) && !controllersSnNew.Contains(contr.sn))
                    {
                        var cardsdb = db.Cards.Where(p => p.workerId == workerdb.Id);
                        foreach (var card in cardsdb)
                        {
                            DeleteCardFromController(contr.sn, card.card16);
                        }
                    }
                    // Добавление карт в контроллер
                    if (!controllersSnOld.Contains(contr.sn) && controllersSnNew.Contains(contr.sn))
                    {
                        var cardsdb = db.Cards.Where(p => p.workerId == workerdb.Id);
                        foreach (var card in cardsdb)
                        {
                            AddCardInController(contr.sn, card.card16);
                        }
                    }
                }
            }
            return Content("true");
        }


        // Возвращает список серийных номеров контроллеров по списку групп доступа
        public List<int> GetControllersListByAccessGroups(List<string> accessGroups)
        {
            var relations = dbcontext.RelationsControllersAccessGroups
                .Where(p => accessGroups.Contains(p.accessGroup))
                .Select(p => p.sn)
                .Distinct()
                .ToList();
            return relations;
        }

        // Возвращает список серийных номеров контроллеров по списку точек доступа
        public List<int> GetControllersListByRelatinsControllerWorker(List<RelationsControllersWorkers> relations)
        {
            List<int> result = new List<int>();
            foreach (var rel in relations)
            {
                if (!result.Contains(rel.sn))
                {
                    result.Add(rel.sn);
                }
            }
            return result;
        }


        // Возвращает список индивидуальных групп доступа сотрудника
        public IActionResult OnGetShowModalAccessGroups(int workerId)
        {
            List<string> res = new List<string>();
            Workers workerdb = _dataManager.workers.GetWorkerById(workerId);
            foreach (var accessGroup in workerdb.accessGroups)
            {
                res.Add($"Modal_worker_access_groups_checkbox{accessGroup}");
            }

            return Content(JsonSerializer.Serialize(res));
        }


        // Возвращает список индивидуальных точек доступа сотрудника
        public IActionResult OnGetShowModalAccessPoints(int workerId)
        {
            List<string> res = new List<string>();
            var relationsdb = relationsControllersWorkers.Where(p => p.workerId == workerId).ToList();
            foreach (var rel in relationsdb)
            {
                if (rel.reader == 1)
                {
                    res.Add($"Modal_worker_access_points_checkbox_in{rel.sn}");
                }
                else
                {
                    res.Add($"Modal_worker_access_points_checkbox_out{rel.sn}");
                }
            }
            return Content(JsonSerializer.Serialize(res));
        }


        public IActionResult OnGetCarPartial(int id = default)
        {
            var worker = new Workers();
            if (id != default)
            {
                worker = _dataManager.workers.GetWorkerById(id);
            }
            ViewDataDictionary customViewData = new ViewDataDictionary<Workers>(ViewData, worker);
            customViewData["groups"] = _dataManager.groups.groups.ToList();
            customViewData["emptyCards"] = emptyCards;
            return new PartialViewResult
            {
                ViewName = "_WorkerEditPartial",
                ViewData = customViewData,
            };
        }


        // Редактирование сотрудника
        public IActionResult OnGetEditWorker(int id, int workerNew, string LastName, string FirstName, string FatherName, string position, string group, string comment, string image)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }

            if (LastName is null || group is null)
            {
                return Content("Error! Обязательные поля должны быть заполнены");
            }

            Workers workerdb = _dataManager.workers.GetWorkerById(id);
            var groupOld = dbcontext.Groups.FirstOrDefault(p => p.group == workerdb.group);
            var groupNew = dbcontext.Groups.FirstOrDefault(p => p.group == group);
            if (workerdb is null)
            {
                return Content("Error! Нет такого сотрудника");
            }
            if (workerdb.worker != workerNew && dbcontext.Workers.FirstOrDefault(p => p.worker == workerNew) is not null)
            {
                return Content("Error! Этот ИДН занят");
            }
            if (groupNew is null)
            {
                return Content("Error! Нет такой группы");
            }
            if (image != "empty") // замена фотографии с удалением старой
            {
                if (workerdb.Image != null)
                {
                    RemoveImage(workerdb.Image);
                }
                workerdb.Image = image;
            }

            workerdb.worker = workerNew;
            workerdb.LastName = LastName;
            workerdb.FirstName = FirstName;
            workerdb.FatherName = FatherName;
            workerdb.position = position;
            workerdb.group = group;
            workerdb.comment = comment;
            dbcontext.Workers.Update(workerdb);
            dbcontext.SaveChanges();

            var worker = workers.FirstOrDefault(p => p.Id == id);
            ViewDataDictionary customViewData = new ViewDataDictionary<Workers>(ViewData, worker);

            if (workerdb.personalCheck != 1 || groupOld.group == group)
            {               
                return new PartialViewResult
                {
                    ViewName = "_WorkerTableTrPartial",
                    ViewData = customViewData,
                };
            }

            var controllersSnOld = GetControllersListByAccessGroups(groupOld.accessGroups);
            var controllersSnNew = GetControllersListByAccessGroups(groupNew.accessGroups);

            var controllersdb = dbcontext.Controllers.ToList();
            foreach (var contr in controllersdb)
            {
                // Удаление карт из контроллера
                if (controllersSnOld.Contains(contr.sn) && !controllersSnNew.Contains(contr.sn))
                {
                    var cardsdb = dbcontext.Cards.Where(p => p.workerId == workerdb.Id);
                    foreach (var card in cardsdb)
                    {
                        DeleteCardFromController(contr.sn, card.card16);
                    }
                }
                // Добавление карт в контроллер
                if (!controllersSnOld.Contains(contr.sn) && controllersSnNew.Contains(contr.sn))
                {
                    var cardsdb = dbcontext.Cards.Where(p => p.workerId == workerdb.Id);
                    foreach (var card in cardsdb)
                    {
                        AddCardInController(contr.sn, card.card16);
                    }
                }
            }
            
            return new PartialViewResult
            {
                ViewName = "_WorkerTableTrPartial",
                ViewData = customViewData,
            };
        }


        // Удаление даты блокировки
        public void OnGetDeleteLockDate(int id)
        {
            var workerdb = dbcontext.Workers.FirstOrDefault(p => p.Id == id);
            workerdb.lockDate = null;
            dbcontext.Workers.Update(workerdb);
            dbcontext.SaveChanges(); 
        }


        // Добавление даты блокировки
        public IActionResult OnGetAddLockDate(int id, string lockDate)
        {
            if (lockDate == null)
            {
                return Content("Задайте дату блокировки");
            }

            var workerdb = dbcontext.Workers.FirstOrDefault(p => p.Id == id);
            workerdb.lockDate = lockDate;
            dbcontext.Workers.Update(workerdb);
            dbcontext.SaveChanges();
            return Content("true");
        }


        // Добавление карты
        public IActionResult OnGetAddCard(string card, int workerId)
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
                card = card.Substring(16);
            }
            string[] inputStr = card.Split(new char[] { ',' });
            if (inputStr[0].Length != 3 || inputStr[1].Length != 5 || !inputStr[0].All(char.IsDigit) || !inputStr[1].All(char.IsDigit))
            {
                return Content("неправильно введена карта");
            }
            Cards newCard = new Cards();
            newCard.card = card;
            newCard.card16 = RefactCard(card);
            using (ApplicationContext db = new ApplicationContext())
            {
                Cards? carddb = db.Cards.FirstOrDefault(p => p.card == card);
                    Workers? workerdb = new Workers();
                if (carddb is not null)
                {
                    return Content("Такая карта уже есть");
                }
                    }
                    else
                    {
                db.Cards.Add(newCard);
                db.SaveChanges();

                if (workerId != 0)
                {
                    Workers workerdb = _dataManager.workers.GetWorkerById(workerId);
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
                carddb.workerId = workerdb.Id;
                db.Cards.Update(carddb);
                db.SaveChanges();

                // Поиск списка контролеров
                List<int> controllersSn = new List<int>();
                if (workerdb.personalCheck == 1)
                {
                    var groupsdb = dbcontext.Groups.FirstOrDefault(p => p.group == workerdb.group);
                    if (groupsdb is null) { return; }
                    controllersSn = GetControllersListByAccessGroups(groupsdb.accessGroups);
                }
                if (workerdb.personalCheck == 2) { controllersSn = GetControllersListByAccessGroups(workerdb.accessGroups); }
                if (workerdb.personalCheck == 3)
                {
                    var accessPoints = dbcontext.RelationsControllersWorkers.Where(p => p.workerId == workerdb.Id).ToList();
                    controllersSn = GetControllersListByRelatinsControllerWorker(accessPoints);
                }

                var controllersdb = dbcontext.Controllers.Where(p => controllersSn.Contains(p.sn)).ToList();
                foreach (var contr in controllersdb)
                {
                    // Добавление карты в контроллер
                    AddCardInController(contr.sn, carddb.card16);
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

            Groups? groupdb = dbcontext.Groups.FirstOrDefault(p => p.Id == groupId);
            AccessGroups? accessGroupdb = dbcontext.AccessGroups.FirstOrDefault(p => p.accessGroup == accessGroup);
            if (groupdb is null)
            {
                return Content("Нет такой группы");
            }
            if (accessGroupdb is null)
            {
                return Content("Нет такой группы доступа");
            }

            var controllersSnOld = GetControllersListByAccessGroups(groupdb.accessGroups);
            groupdb.accessGroups.Add(accessGroupdb.accessGroup);
            dbcontext.Groups.Update(groupdb);
            dbcontext.SaveChanges();

            var controllersSnNew = GetControllersListByAccessGroups(groupdb.accessGroups);
            var workersdb = dbcontext.Workers.Where(p => p.group == groupdb.group && p.personalCheck == 1).ToList();
            var controllersdb = dbcontext.Controllers.ToList();
            foreach (var controller in controllersdb)
            {

                if (!controllersSnOld.Contains(controller.sn) && controllersSnNew.Contains(controller.sn))
                {
                    int cardCount = 0;
                    List<string> cardsList = new List<string>();
                    foreach (var worker in workersdb)
                    {
                        var cardsdb = dbcontext.Cards.Where(p => p.workerId == worker.Id).ToList();
                        foreach (var card in cardsdb)
                        {
                            cardCount++;
                            cardsList.Add(card.card16);
                            if (cardCount == 10)
                            {
                                AddCardInControllerRange(controller.sn, cardsList);
                                cardsList = new List<string>();
                                cardCount = 0;
                            }
                        }
                    }
                    AddCardInControllerRange(controller.sn, cardsList);
                }
                groupdb.accessGroups.Add(accessGroupdb.accessGroup);
                db.Groups.Update(groupdb);
                db.SaveChanges();
            }
            return Content("true");
        }


        // Убирание группы доступа из группы
        public IActionResult OnGetRemoveAccessGroupFromGroup(int groupId, string accessGroup)
        {
            if (HttpContext.Session.GetString("logged") != "true")
            {
                return Content("Перезагрузите страницу");
            }

            Groups? groupdb = dbcontext.Groups.FirstOrDefault(p => p.Id == groupId);
            AccessGroups? accessGroupdb = dbcontext.AccessGroups.FirstOrDefault(p => p.accessGroup == accessGroup);
            if (groupdb is null)
            {
                return Content("Нет такой группы");
            }
            if (!groupdb.accessGroups.Contains(accessGroup) || accessGroupdb is null)
            {
                return Content("Нет такой группы доступа");
            }

            var controllersSnOld = GetControllersListByAccessGroups(groupdb.accessGroups);
            groupdb.accessGroups.Remove(accessGroup);
            dbcontext.Groups.Update(groupdb);
            dbcontext.SaveChanges();

            var controllersSnNew = GetControllersListByAccessGroups(groupdb.accessGroups);
            var workersdb = dbcontext.Workers.Where(p => p.group == groupdb.group && p.personalCheck == 1).ToList();
            var controllersdb = dbcontext.Controllers.ToList();
            foreach (var controller in controllersdb)
            {
                if (controllersSnOld.Contains(controller.sn) && !controllersSnNew.Contains(controller.sn))
                {
                    int cardCount = 0;
                    List<string> cardList = new List<string>();
                    // Удаление карт из контроллера
                    foreach (var worker in workersdb)
                    {
                        var cardsdb = dbcontext.Cards.Where(p => p.workerId == worker.Id).ToList();
                        foreach (var card in cardsdb)
                        {
                            cardCount++;
                            cardList.Add(card.card16);
                            if (cardCount == 10)
                            {
                                DeleteCardFromControllerRange(controller.sn, cardList);
                                cardList = new List<string>();
                                cardCount = 0;
                            }
                        }
                    }
                    DeleteCardFromControllerRange(controller.sn, cardList);
                }
            return Content(response);
            }
            return Content("true");
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
            Controllers? controllerdb = dbcontext.Controllers.FirstOrDefault(p => p.sn == snInt);
            AccessGroups? accessGroupdb = dbcontext.AccessGroups.FirstOrDefault(p => p.accessGroup == accessGroup);
            if (controllerdb is null)
            {
                return Content("Нет такого контроллера");
            }
            if (accessGroupdb is null)
            {
                return Content("Нет такой группы доступа");
            }

            List<RelationsControllersAccessGroups> relations = dbcontext.RelationsControllersAccessGroups.Where(p => p.sn == snInt && p.accessGroup == accessGroup).ToList();
            if (relations != null) relations.ToList();

            if (relations.Count == 2)
            {
                return Content("Оба входа контроллера уже привязаны к этой группе доступа");
            }
            foreach (var rel in relations)
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
                var workersdb = dbcontext.Workers.Where(p => p.personalCheck != 3).Include(c => c.cards).ToList();
                var groupsdb = dbcontext.Groups.Where(p => p.accessGroups.Contains(accessGroup)).ToList();
                int cardCount = 0;
                List<string> cardList = new List<string>();
                foreach (var worker in workersdb)
                {
                    // Поиск списка контролеров
                    var controllersSn = new List<int>();                   
                    if (worker.personalCheck == 1 && groupsdb.Exists(p => p.group == worker.group))
                    {
                        controllersSn = GetControllersListByAccessGroups(groupsdb.FirstOrDefault(p => p.group == worker.group).accessGroups);
                    }
                    if (worker.personalCheck == 2 && worker.accessGroups.Contains(accessGroup))
                    {
                        controllersSn = GetControllersListByAccessGroups(worker.accessGroups);
                    }

                    if (controllersSn.Contains(snInt))
                    {
                        // Добавление карт в контроллер
                        foreach (var card in worker.cards)
                        {
                            cardCount++;
                            cardList.Add(card.card16);
                            if (cardCount == 10)
                            {
                                AddCardInControllerRange(snInt, cardList);
                                cardList = new List<string>();
                                cardCount = 0;
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
                AddCardInControllerRange(snInt, cardList);
            }

                }

            if (cbIn)
            {
                RelationsControllersAccessGroups newRelation = new RelationsControllersAccessGroups();
                newRelation.sn = snInt;
                newRelation.accessGroup = accessGroup;
                newRelation.reader = 1;
                dbcontext.RelationsControllersAccessGroups.Add(newRelation);
                dbcontext.SaveChanges();
            }
            if (cbOut)
            {
                RelationsControllersAccessGroups newRelation = new RelationsControllersAccessGroups();
                newRelation.sn = snInt;
                newRelation.accessGroup = accessGroup;
                newRelation.reader = 2;
                dbcontext.RelationsControllersAccessGroups.Add(newRelation);
                dbcontext.SaveChanges();
            }
            return Content(response);
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

        // Удаление списка карт из контроллера
        public void DeleteCardFromControllerRange(int sn, List<string> cards16)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                MessagesDB mesdb = new MessagesDB
                {
                    operation = "del_cards_range",
                    card = JsonSerializer.Serialize(cards16)
                };

                mesdb.sn = sn;
                db.Messages.Add(mesdb);

                db.SaveChanges();
            }
        }


        // Удаление всех карт из контроллера
        public void OnGetDeleteAllCardFromController(string checkboxJson)
        {
            var snList = JsonSerializer.Deserialize<string[]>(checkboxJson);
            foreach (var sn in snList)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    MessagesDB mesdb = new MessagesDB
                    {
                        operation = "clear_cards",

                    };

                    mesdb.sn = int.Parse(sn);
                    db.Messages.Add(mesdb);
                    db.SaveChanges();
                }
            }
        }


        // Добавление всех карт в контроллер
        public void OnGetAddAllCardInController(string checkboxJson)
        {
            var snList = JsonSerializer.Deserialize<string[]>(checkboxJson);
            foreach (var snStr in snList)
            {
                int sn = int.Parse(snStr);
                var workersdb = dbcontext.Workers.Include(p => p.cards).ToList();
                var accessGroupsdb = dbcontext.RelationsControllersAccessGroups.Where(p => p.sn == sn).Select(p => p.accessGroup).Distinct().ToList();
                var relationsWorkersdb = dbcontext.RelationsControllersWorkers.Where(p => p.sn == sn).ToList();
                var groupsdb = dbcontext.Groups.ToList();

                int cardCount = 0;
                List<string> cardList = new List<string>();
                foreach (var worker in workersdb)
                {
                    bool flag = false;
                    if (worker.personalCheck == 1)
                    {
                        var groupdb = groupsdb.FirstOrDefault(p => p.group == worker.group);
                        if (groupdb.accessGroups.Intersect(accessGroupsdb).Count() > 0)
                        {
                            flag = true;
                        }
                    }
                    if (worker.personalCheck == 2 && worker.accessGroups.Intersect(accessGroupsdb).Count() > 0)
                    {
                        flag = true;
                    }
                    if (worker.personalCheck == 3 && relationsWorkersdb.FirstOrDefault(p => p.workerId == worker.Id) is not null)
                    {
                        flag = true;
                    }

                    if (flag)
                    {
                        foreach (var card in worker.cards)
                        {
                            cardCount++;
                            cardList.Add(card.card16);
                            if (cardCount == 10)
                            {
                                AddCardInControllerRange(sn, cardList);
                                cardList = new List<string>();
                                cardCount = 0;
                            }
                        }
                    }
                }
                _logger.LogInformation(JsonSerializer.Serialize(cardList));
                AddCardInControllerRange(sn, cardList);
            }
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


        // добавление списка карт в контроллер
        public void AddCardInControllerRange(int sn, List<string> cards16)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                MessagesDB mesdb = new MessagesDB
                {
                    operation = "add_cards_range",
                    card = JsonSerializer.Serialize(cards16),
                    flag = 0,
                    tz = 255
                };

                mesdb.sn = sn;
                db.Messages.Add(mesdb);

                db.SaveChanges();
            }
        }


        // Преобразование кода карты в шестнадцатеричный вид
        public string RefactCard(string fullInputString)
        {
            string resultCard = "000000";
            string secondPath = fullInputString;
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
            var filterCards = dbcontext.Cards.ToList();
            if (filter is null)
            {
                return Content("false");
            }
            var workersdb = dbcontext.Workers.Where(p => (p.LastName + " " + p.FirstName + " " + p.FatherName).ToUpper().IndexOf(filter.ToUpper()) != -1).ToList();
            foreach (Cards? filterCard in filterCards)
            {
                if (filterCard.card.IndexOf(filter) != -1 ||
                    filterCard.card16.IndexOf(filter) != -1 ||
                    (filterCard.workerId is not null && (filterCard.workerId.ToString().IndexOf(filter) != -1 || workersdb.Exists(p => p.Id == filterCard.workerId))))
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
                    if (filterWorker.Id.ToString().IndexOf(filter) != -1 ||
                        filterWorker.worker.ToString().IndexOf(filter) != -1 ||
                        System.String.Concat(new string[] { filterWorker.LastName, " ", filterWorker.FirstName, " ", filterWorker.FatherName }).ToUpper().IndexOf(filter.ToUpper()) != -1 ||
                        (filterWorker.position is not null && filterWorker.position.ToUpper().IndexOf(filter.ToUpper()) != -1) ||
                        filterWorker.group.ToUpper().IndexOf(filter.ToUpper()) != -1 ||
                        (filterWorker.comment is not null && filterWorker.comment.ToUpper().IndexOf(filter.ToUpper()) != -1))
                    {
                        var group = db.Groups.FirstOrDefault(p => p.group == filterWorker.group);
                        res.Add(filterWorker.Id.ToString() + ";" + group.Id);
                    }
                }
                return Content(JsonSerializer.Serialize(res));
            }
        }


        // Поиск сотрудников с комментариями
        public IActionResult OnGetFindComments()
        {
            List<string> res = new List<string>();

            foreach (Workers worker in workers)
            {

                if (worker.comment != "" && worker.comment != null)
                {
                    var group = groups.FirstOrDefault(p => p.group == worker.group);
                    res.Add(worker.Id.ToString() + ";" + group.Id);
                }
            }
            return Content(JsonSerializer.Serialize(res));

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
                    var controllerName = controllers.FirstOrDefault(p => p.sn == filterRelation.sn).name;
                    if (filterRelation.sn.ToString().IndexOf(filter) != -1 ||
                        filterRelation.accessGroup.ToUpper().IndexOf(filter.ToUpper()) != -1 ||
                        (controllerName != null && controllerName.ToUpper().IndexOf(filter.ToUpper()) != -1))
                    {
                        res.Add(filterRelation.Id);
                    }
                }
                return Content(JsonSerializer.Serialize(res));
            }
        }



        public IActionResult OnGetFilterEvents(string snCheckboxJson, int workerId, string fio, string group, string dateBeg, string dateEnd)
        {
            var snList = JsonSerializer.Deserialize<List<string>>(snCheckboxJson);
            List<EventsWeb> res = GetEventsWebByFilter(snList, workerId, fio, group, dateBeg, dateEnd).OrderByDescending(p => p.Id).ToList();

            ViewDataDictionary customViewData = new ViewDataDictionary<List<EventsWeb>>(ViewData, res);
            return new PartialViewResult
            {
                ViewName = "_EventsFilterPartial",
                ViewData = customViewData,
            };
        }

        public List<EventsWeb> GetEventsWebByFilter(List<string> snList, int workerId, string fio, string group, string dateBeg, string dateEnd)
        {
            if (snList.Count == 0 && workerId == 0 && fio == null && dateBeg == null && dateEnd == null)
            { dateBeg = DateTime.Now.AddDays(-1).ToString(); }
            List<EventsWeb> res = new List<EventsWeb>();
            var eventsdb = dbcontext.Events.Include(c => c.worker).ToList();
            foreach (var ev in eventsdb)
            {
                bool flag = true;
                if (snList.Count != 0 && !snList.Contains(ev.sn.ToString()))
                {
                    flag = false;
                }
                if (workerId != 0 && ev.workerId != workerId)
                {
                    flag = false;
                }                
                if (fio != null)
                {
                    var workerdb = ev.worker;
                    if (workerdb == null || System.String.Concat(new string[] { workerdb.LastName, " ", workerdb.FirstName, " ", workerdb.FatherName }).ToUpper().IndexOf(fio.ToUpper()) == -1)
                    {
                        flag = false;
                    }
                }
                if (group != null && (ev.worker == null || ev.worker.group != group))
                {
                    flag = false;
                }
                if (dateBeg != null && DateTime.Parse(dateBeg) > ev.dateTime)
                {
                    flag = false;
                }
                if (dateEnd != null && DateTime.Parse(dateEnd).AddDays(1) < ev.dateTime)
                {
                    flag = false;
                }
                if (flag)
                {
                    var controllerdb = dbcontext.Controllers.FirstOrDefault(p => p.sn == ev.sn);
                    var eventWeb = new EventsWeb();
                    eventWeb.Id = ev.Id;
                    if (controllerdb != null && controllerdb.name is not null)
                    {
                        eventWeb.name = controllerdb.name;
                    }
                    else
                    {
                        eventWeb.name = ev.sn.ToString();
                    }
                    var eventCode = eventCodes.FirstOrDefault(p => p.eventCode == ev.Event);
                    if (eventCode is null)
                    {
                        eventWeb.Event = ev.Event.ToString();
                    }
                    else
                    {
                        eventWeb.Event = eventCode.Event;
                    }
                    eventWeb.card = ev.card;
                    eventWeb.flag = ev.flag;
                    eventWeb.time = ev.dateTime.ToString();
                    eventWeb.workerId = ev.workerId;
                    var workerdb = dbcontext.Workers.FirstOrDefault(p => p.Id == ev.workerId);
                    if (workerdb != null)
                    {
                        eventWeb.fio = System.String.Concat(new string[] { workerdb.LastName, " ", workerdb.FirstName, " ", workerdb.FatherName });
                    }
                    res.Add(eventWeb);
                }
            }
            return (res);
        }


        // Отчет по событиям
        public IActionResult OnGetReportEvents(string snCheckboxJson, int workerId, string fio, string group, string dateBeg, string dateEnd)
        {
            string filename = "ReportFiles/" + Guid.NewGuid().ToString() + ".xlsx";
            string fullPath = hostingEnvironment.WebRootPath + "/" + filename;
            var snList = JsonSerializer.Deserialize<List<string>>(snCheckboxJson);
            var report = GetEventsByFilter(snList, workerId, fio, group, dateBeg, dateEnd);
            report = report.OrderByDescending(p => p.Id).ToList();

            FileInfo newFile = new FileInfo(fullPath);
            var package = new ExcelPackage(newFile);
            ExcelWorksheet sheet = package.Workbook.Worksheets.Add("Отчет");

            sheet.Cells[1, 2].Value = "Отчет по событиям";
            sheet.Cells[3, 1].Value = "Имя (Серийный номер контроллера)";
            sheet.Cells[3, 2].Value = "Тип события";
            sheet.Cells[3, 3].Value = "Номер карты";
            sheet.Cells[3, 4].Value = "ID сотрудника";
            sheet.Cells[3, 5].Value = "ФИО сотрудника";
            sheet.Cells[3, 6].Value = "Время события";
            sheet.Cells[3, 7].Value = "Флаги события";
            sheet.Columns[1].Width = 30;
            sheet.Columns[2].Width = 40;
            sheet.Columns[3].Width = 15;
            sheet.Columns[4].Width = 20;
            sheet.Columns[5].Width = 30;
            sheet.Columns[6].Width = 20;
            sheet.Columns[7].Width = 15;
            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(1).Style.Font.Size = 25;
            sheet.Row(3).Style.Font.Bold = true;
            sheet.Row(3).Style.Font.Size = 15;
            sheet.Row(3).Style.WrapText = true;
            sheet.Column(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

            int row = 4;
            int column = 1;
            foreach (var ev in report)
            {

                if (controllers.FirstOrDefault(p => p.sn == ev.sn).name is null)
                {
                    sheet.Cells[row, column].Value = ev.sn;
                }
                else
                {
                    sheet.Cells[row, column].Value = controllers.FirstOrDefault(p => p.sn == ev.sn).name;
                }

                if (eventCodes.FirstOrDefault(p => p.eventCode == ev.Event) is null)
                {
                    sheet.Cells[row, column + 1].Value = ev.Event;
                }
                else
                {
                    sheet.Cells[row, column + 1].Value = eventCodes.FirstOrDefault(p => p.eventCode == ev.Event).Event;
                }
                sheet.Cells[row, column + 2].Value = ev.card;
                sheet.Cells[row, column + 3].Value = ev.workerId;
                var workerdb = dbcontext.Workers.FirstOrDefault(p => p.Id == ev.workerId);
                if (workerdb is not null)
                {
                    sheet.Cells[row, column + 4].Value = System.String.Concat(new string[] { workerdb.LastName, " ", workerdb.FirstName, " ", workerdb.FatherName });
                }
                sheet.Cells[row, column + 5].Value = ev.dateTime.ToString();
                sheet.Cells[row, column + 6].Value = ev.flag;
                row++;
            }

            package.Save();
            return Content(filename);
        }


        // Печать событий       
        public IActionResult OnGetPrintEvents(string snCheckboxJson, int workerId, string fio, string group, string dateBeg, string dateEnd)
        {
            string filename = "ReportFiles/" + Guid.NewGuid().ToString() + ".pdf";
            string fullPath = hostingEnvironment.WebRootPath + "/" + filename;
            //var snList = JsonSerializer.Deserialize<List<string>>(snCheckboxJson);
            List<string> snList = new List<string>();
            List<Events> eventsListAll = GetEventsByFilter(snList, workerId, fio, group, dateBeg, dateEnd).OrderBy(p => p.Id).ToList();
            if (dateBeg == null) { dateBeg = eventsListAll.LastOrDefault().dateTime.ToShortDateString(); }
            if (dateEnd == null) { dateEnd = eventsListAll.FirstOrDefault().dateTime.ToShortDateString(); }
            List<Workers> workersdb = eventsListAll.Select(p => p.worker).Distinct().OrderBy(p => p.LastName).ToList();

            var controllersdb = dbcontext.Controllers.ToList();
            Aspose.Pdf.Document document = new Aspose.Pdf.Document();
            Aspose.Pdf.Page page = document.Pages.Add();

            foreach (var worker in workersdb)
            {
                string fioLocal = System.String.Concat(new string[] { worker.LastName, " ", worker.FirstName, " ", worker.FatherName });
                Aspose.Pdf.Text.TextFragment name = new Aspose.Pdf.Text.TextFragment(fioLocal);
                name.TextState.FontSize = 10;
                name.TextState.Font = FontRepository.FindFont("Arial");
                name.TextState.FontStyle = FontStyles.Bold;
                name.Margin.Bottom = 10;
                name.Margin.Right = 20;
                name.HorizontalAlignment = HorizontalAlignment.Right;
                page.Paragraphs.Add(name);

                var eventsList = eventsListAll.Where(p => p.workerId == worker.Id).ToList();
                Aspose.Pdf.Table table = new Aspose.Pdf.Table();
                table.Border = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, .5f, Aspose.Pdf.Color.FromRgb(System.Drawing.Color.LightGray));
                table.DefaultCellBorder = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, .5f, Aspose.Pdf.Color.FromRgb(System.Drawing.Color.LightGray));
                table.Margin.Bottom = 10;
                table.ColumnWidths = "94 94 94 94 34";

                Row row = table.Rows.Add();
                row.DefaultCellTextState.FontSize = 10;
                row.DefaultCellTextState.Font = FontRepository.FindFont("Arial");
                row.DefaultCellTextState.FontStyle = FontStyles.Bold;
                row.Cells.Add("Вход");
                row.Cells.Add("Дата Время");
                row.Cells.Add("Выход");
                row.Cells.Add("Дата Время");
                row.Cells.Add("Итого");

                DateTime dt0 = new DateTime();
                TimeSpan allTime = new TimeSpan();
                for (int i = 0; i < eventsList.Count - 1; i++)
                {                    
                    row = table.Rows.Add();
                    row.FixedRowHeight = 25;
                    row.DefaultCellTextState.FontSize = 10;
                    row.DefaultCellTextState.Font = FontRepository.FindFont("Arial");

                    // left
                    var ev = eventsList[i];
                    var contr = controllersdb.FirstOrDefault(p => p.sn == ev.sn);
                    string reader = "Вход";
                    if (ev.Event == 5) { reader = "Выход"; }

                    if (contr == null || contr.name == null) { row.Cells.Add($"{contr.sn} {reader}"); }
                    else { row.Cells.Add($"{contr.name} {reader}"); }
                    var dt1 = ev.dateTime;

                    row.Cells.Add($"{dt1}");
                    
                    //right
                    ev = eventsList[i + 1];
                    contr = controllersdb.FirstOrDefault(p => p.sn == ev.sn);
                    reader = "Вход";
                    if (ev.Event == 5) { reader = "Выход"; }

                    if (contr == null || contr.name == null) { row.Cells.Add($"{contr.sn} {reader}"); }
                    else { row.Cells.Add($"{contr.name} {reader}"); }
                    var dt2 = ev.dateTime;
                    row.Cells.Add($"{dt2}");

                    bool home = (ev.sn == 45001400 && ev.Event == 5) || (ev.sn == 45001162 && ev.Event == 5);
                    if (dt1.AddMinutes(5) < dt2 || i == eventsList.Count - 2 || home)
                    {
                        if (!dt0.Equals(DateTime.MinValue))
                        {
                            dt1 = dt0;
                            dt0 = new DateTime();
                        }
                        var timeDelta = dt2.Subtract(dt1);
                        var cell = row.Cells.Add(timeDelta.ToString(@"hh\:mm"));
                        cell.DefaultCellTextState.FontStyle = FontStyles.Bold;
                        cell.DefaultCellTextState.HorizontalAlignment = HorizontalAlignment.Center;
                        allTime += timeDelta;
                    }
                    else if (dt0.Equals(DateTime.MinValue)) { dt0 = dt1; }
                    if (home) { i++; }
                }

                document.Pages[1].Paragraphs.Add(table);
                Aspose.Pdf.Text.TextFragment timeTotal = new Aspose.Pdf.Text.TextFragment(((int)allTime.TotalHours).ToString() + allTime.ToString(@"\:mm"));
                timeTotal.TextState.FontSize = 10;
                timeTotal.TextState.Font = FontRepository.FindFont("Arial");
                timeTotal.TextState.FontStyle = FontStyles.Bold;
                timeTotal.Margin.Bottom = 40;
                timeTotal.Margin.Right = 5;
                timeTotal.HorizontalAlignment = HorizontalAlignment.Right;
                page.Paragraphs.Add(timeTotal);
            }
            document.Save(fullPath);

            document = new Aspose.Pdf.Document(fullPath);

            PageNumberStamp pageNumberStamp = new PageNumberStamp();
            pageNumberStamp.Background = false;
            pageNumberStamp.Format = "Страница # из " + document.Pages.Count;
            pageNumberStamp.BottomMargin = 10;
            pageNumberStamp.RightMargin = 20;
            pageNumberStamp.HorizontalAlignment = HorizontalAlignment.Right;
            pageNumberStamp.StartingNumber = 1;
            pageNumberStamp.TextState.Font = FontRepository.FindFont("Arial");
            pageNumberStamp.TextState.FontSize = 14.0F;
            pageNumberStamp.TextState.FontStyle = FontStyles.Bold;
            pageNumberStamp.TextState.FontStyle = FontStyles.Italic;

            TextStamp headerTextStamp = new TextStamp($"{DateOnly.Parse(dateBeg)} - {DateOnly.Parse(dateEnd)}");
            headerTextStamp.TopMargin = 30;
            headerTextStamp.HorizontalAlignment = HorizontalAlignment.Center;
            headerTextStamp.VerticalAlignment = VerticalAlignment.Top;
            headerTextStamp.TextState.Font = FontRepository.FindFont("Arial");
            headerTextStamp.TextState.FontSize = 14.0F;
            headerTextStamp.TextState.FontStyle = FontStyles.Bold;

            foreach (Aspose.Pdf.Page pageLocal in document.Pages)
            {
                pageLocal.AddStamp(headerTextStamp);
                pageLocal.AddStamp(pageNumberStamp);
            }

            document.Save(fullPath);
            return Content(filename);
        }


        public List<Events> GetEventsByFilter(List<string> snList, int workerId, string fio, string group, string dateBeg, string dateEnd)
        {
            List<EventsWeb> res = new List<EventsWeb>();
            var eventsdb = dbcontext.Events.Include(c => c.worker).Where(c => c.worker != null
                && (c.Event == 4 || c.Event == 5)
                && (snList.Count == 0 || snList.Contains(c.sn.ToString()))
                && (workerId == 0 || c.workerId == workerId)).ToList();

            eventsdb = eventsdb.Where(p => (dateBeg == null || DateTime.Parse(dateBeg) < p.dateTime)
                && (dateEnd == null || DateTime.Parse(dateEnd).AddDays(1) > p.dateTime)
                && p.worker != null
                && (fio == null || System.String.Concat(new string[] { p.worker.LastName, " ", p.worker.FirstName, " ", p.worker.FatherName }).ToUpper().IndexOf(fio.ToUpper()) != -1)
                && (group == null || p.worker.group == group)).ToList();
           
            return (eventsdb);
        }


        // Задание имени контроллера
        public IActionResult OnGetSetControllerName(int id, string name)
        {
            var controllerdb = dbcontext.Controllers.FirstOrDefault(p => p.name == name);
            if (name != null && controllerdb != null)
            {
                return Content("Это имя занято");
            }
            controllerdb = dbcontext.Controllers.FirstOrDefault(p => p.Id == id);
            if (controllerdb == null)
            {
                return Content("Нет такого контроллера");
            }
            controllerdb.name = name;
            dbcontext.Controllers.Update(controllerdb);
            dbcontext.SaveChanges();
            return Content("true");
        }


        // Добавление фотографии в папку
        public IActionResult OnPostSetImage()
        {
            if (formFile is null)
            {
                _logger.LogInformation("null file");
                return Content("empty");
            }
            string filename = Guid.NewGuid().ToString();
            filename += Path.GetExtension(formFile.FileName);
            using (var stream = new FileStream(Path.Combine(hostingEnvironment.WebRootPath, "images/", filename), FileMode.Create))
            {
                formFile.CopyTo(stream);
            }
            return Content(filename);
        }


        // Удаление фотографии
        public void RemoveImage(string image)
        {
            System.IO.File.Delete(Path.Combine(hostingEnvironment.WebRootPath, "images/", image));
        }


        // Переключение вкладки
        public IActionResult OnGetChangePage(string tab)
        {
            HttpContext.Session.SetString("tab", tab);
            return Content("true");
        }
    }
}