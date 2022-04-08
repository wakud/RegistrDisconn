using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RegistrDisconnection.Data;
using RegistrDisconnection.Models;
using RegistrDisconnection.Models.Abonents;
using RegistrDisconnection.Models.Dictionaries;
using RegistrDisconnection.Models.Operations;
using RegistrDisconnection.Models.Users;
using RegistrDisconnection.Models.Zvity;
using RegistrDisconnection.MyClasses;
using RegistrDisconnection.ViewModels;
using SharpDocx;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RegistrDisconnection.Controllers
{
    /// <summary>
    /// Робота з попередженнями
    /// </summary>
    public class PoperController : Controller
    {
        private readonly MainContext _context;
        private readonly IWebHostEnvironment appEnvir;

        public PoperController(MainContext context, IWebHostEnvironment appEnvironment)
        {
            Console.OutputEncoding = System.Text.Encoding.GetEncoding(1251);
            _context = context;
            appEnvir = appEnvironment;
        }
        /// <summary>
        /// Робота з попередженнями, сторінка список абонентів на відключення 
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Адміністратор, Користувач")]     //сторінка індекс відкривається тільки для авторизованих користувачів
        public ActionResult Abonents()
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            //потрібно зпобити перевірку на головного адміна він без цоку
            string cokCode = currentUser.CokId == null ? "TR39" : currentUser.Cok.Code;
            Organization cok = _context.Coks.FirstOrDefault(c => c.Code == cokCode);

            List<ActualDataPerson> allAbonents = _context.ActualDatas
                .Include(ad => ad.Person)
                .Include(ad => ad.Finance)
                .Where(ad => ad.Person.CokId == currentUser.CokId || currentUser.CokId == null)
                .ToList();
            return View(allAbonents);
        }
        /// <summary>
        /// сторінка друк по напрямках
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="name"></param>
        /// <param name="filtr"></param>
        /// <param name="word"></param>
        /// <param name="converty"></param>
        /// <param name="Vymogy"></param>
        /// <param name="_direc"></param>
        /// <returns></returns>
        public ActionResult Napr(int? direction, string name, string filtr, string word, string converty, string Vymogy, int? _direc)
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            string vykonavets = currentUser.FullName;
            string cokCode = currentUser.CokId == null ? "TR39" : currentUser.Cok.Code;

            IQueryable<ActualDataPerson> people = _context.ActualDatas
                .Include(ad => ad.Person).ThenInclude(p => p.Cok)
                .Include(ad => ad.Person).ThenInclude(p => p.Address).ThenInclude(a => a.Direction)
                .Include(ad => ad.Person).ThenInclude(p => p.UpdateFinances)
                .Include(ad => ad.Finance)
                .Include(ad => ad.Poperedgenia)
                .Include(ad => ad.Lichylnyk)
                .Include(ad => ad.Vykl)
                .Where(ad => ad.Person.Cok.Code == cokCode && ad.Person.StatusAktyv && ad.Vykl.DateVykl == null);

            if (direction != null && direction != 0)
            {
                people = people.Where(p => p.Person.Address.DirectionDictId == direction);
            }
            else if (_direc != null && _direc != 0)
            {
                direction = _direc;
                people = people.Where(p => p.Person.Address.DirectionDictId == direction);
            }

            if (!string.IsNullOrEmpty(name))
            {
                bool isNum = int.TryParse(name, out int Num);
                people = isNum
                    ? people.Where(p => p.Person.OsRah.Contains(name))
                    : people.Where(p => p.Person.FullName.Contains(name));
            }

            List<DirectionDict> directions = _context.DirectionDicts
                .Where(d => d.Cok.Code == cokCode && d.ParentDirectionId == null)
                .ToList();
            directions.Insert(0, new DirectionDict { Name = "Всі", Id = 0 });

            SelectList directionsSelect = direction != null
                ? new SelectList(directions, "Id", "Name", direction)
                : new SelectList(directions, "Id", "Name", 0);

            FilterNapr viewModel = new FilterNapr
            {
                People = people.ToList(),
                Directions = directionsSelect,
                DirectionList = directions,
                Name = name
            };

            if (filtr != null)
            {
                return View(viewModel);
            }
            //формуємо попередження для друку у ворд
            #region
            if (word != null)
            {
                //перевіряємо чи вибраний напрямок
                if (direction.Value == 0)
                {
                    ViewBag.error = "BadDir";
                    return View(viewModel);
                }

                UpdateGroup lastUpdate = _context.UpdateGroups.FirstOrDefault(
                    ug => ug.DateUpdate.Date == DateTime.Now.Date &&
                    (currentUser.CokId == _context.Users.FirstOrDefault(u => u.Login == userName).CokId)
                );
                //перевірямо чи була оновлена інформація (оплати, перерахунки та ін.)
                if (lastUpdate == null)
                {
                    TempData["error"] = "BadUpdate";
                    return RedirectToAction("Abonents", "Poper");
                }

                string filePath = "\\files\\";
                string generatedPath = "poper.docx";
                string fileName = "shablon.docx";
                string fullPath = appEnvir.WebRootPath + filePath + fileName;
                string fullGenerated = appEnvir.WebRootPath + filePath + generatedPath;

                DateTime now = DateTime.Now;
                DateTime first = new DateTime(now.Year, now.Month, 1);
                List<PrintPoper> personForPrint = new List<PrintPoper>();
                DateTime datePoper = DateTime.Today;
                DateTime StanNa = first;
                IQueryable<PoperDrukGroup> LastDateVykl = _context.DrukGroups
                    .Include(p => p.DirectionDict).Where(p => p.DirectionDict.CokId == currentUser.CokId);
                DateTime DateVykl = Utils.GetFreePoperDay(_context.VyhAndSviats, DateTime.Today, LastDateVykl);
                DirectionDict dir = _context.DirectionDicts.FirstOrDefault(d => d.Id == direction.Value);

                //робимо зміни в БД
                using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        PoperDrukGroup poperDrukGroup = new PoperDrukGroup
                        {
                            CountAbon = 0,
                            DirectionName = dir.Name,
                            DirectionDictId = dir.Id,
                            Stanomna = StanNa,
                            VydanePoper = datePoper,
                            Vykl = DateVykl
                        };

                        _ = _context.DrukGroups.Add(poperDrukGroup);
                        _ = _context.SaveChanges();
                        foreach (ActualDataPerson item in viewModel.People)
                        {
                            Poperedgenia poper = item.Poperedgenia;
                            if (poper.VydanePoper == false || poper.VydanePoper == null)
                            {
                                poper.PoperDrukGroupId = poperDrukGroup.Id;
                                poper.Poper = datePoper;
                                poper.Napramok = direction.Value;
                                poper.StanNa = StanNa;
                                poper.DateVykl = DateVykl;
                                poper.Period = Period.Per_now().Per_int;
                                poper.VydanePoper = true;       //якщо друкувалося вже попередження то ставимо true
                                poper.NumberPoper = poper.Id;
                                _ = _context.SaveChanges();
                                PrintPoper forPrint = Utils.CreatePrintPoperFromActual(item, vykonavets);
                                personForPrint.Add(forPrint);
                            }
                        }

                        int countPoper = personForPrint.Count;
                        poperDrukGroup.CountAbon = countPoper;
                        _ = _context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        transaction.Rollback();
                    }
                }

                DocumentBase document = DocumentFactory.Create(fullPath, personForPrint);
                document.Generate(fullGenerated);

                //формуємо фійл і видаємо користувачу
                string NewFileName = "Poper_" + DateTime.Now.ToString() + ".docx";
                return File(
                    System.IO.File.ReadAllBytes(fullGenerated),
                    System.Net.Mime.MediaTypeNames.Application.Octet,
                    NewFileName
                );
            }
            #endregion
            //робимо друк конвертів у ворд і формування DBF для укрпошти
            if (converty != null)
            {
                int idNapr = direction.Value;
                return RedirectToAction("Index", "UkrPoshta", new { id = idNapr });
            }

            //сторінка друк вимог
            if (Vymogy != null)
            {
                UpdateGroup lastUpdate = _context.UpdateGroups.FirstOrDefault(
                    ug => ug.DateUpdate.Date == DateTime.Now.Date &&
                    (currentUser.CokId == _context.Users.FirstOrDefault(u => u.Login == userName).CokId)
                );
                if (lastUpdate == null)
                {
                    TempData["error"] = "BadUpdate";
                    return RedirectToAction("Abonents", "Poper");
                }

                int idNapr = direction.Value;
                return RedirectToAction("Index", "Vymogy", new { id = direction });
            }
            return View(viewModel);
        }
        /// <summary>
        /// Сторінка звітів
        /// </summary>
        /// <returns></returns>
        public IActionResult Zvit()
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);

            UpdateGroup lastUpdate = _context.UpdateGroups.FirstOrDefault(
                ug => ug.DateUpdate.Date == DateTime.Now.Date &&
                (currentUser.CokId == _context.Users.FirstOrDefault(u => u.Login == userName).CokId));

            //перевіряємо чи було поновлення
            if (lastUpdate == null)
            {
                TempData["error"] = "BadUpdate";
                return RedirectToAction("Abonents", "Poper");
            }
            return View();
        }
        /// <summary>
        /// Формування звітів
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Zvit(IFormCollection form)
        {
            string? zvit1;
            string? zvit2;
            string? zvit3;
            string? zvit4;
            string stanom_na;
            zvit1 = form["zvit1"];
            zvit2 = form["zvit2"];
            zvit3 = form["zvit3"];
            zvit4 = form["zvit4"];
            stanom_na = form["stanom_na"];

            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            string vykonavets = currentUser.FullName;
            string cokCode = currentUser.CokId == null ? "ORG" : currentUser.Cok.Code;
            stanom_na += "-01";
            Period period = Period.WithOffset(DateTime.Parse(stanom_na));
            DateTime from = period.Per_date;
            DateTime to = DateTime.Parse(stanom_na);

            //вибираємо інформацію для звітів 1, 2, 3
            if (zvit1 != null || zvit2 != null || zvit3 != null)
            {
                DataTable dtres = BillingUtils.Zvit1(appEnvir, cokCode, stanom_na);
                List<Zvit1> zvits = new List<Zvit1>();
                if (dtres != null)
                {
                    int count = 1;
                    foreach (DataRow row in dtres.Rows)
                    {
                        Zvit1 zvit = new Zvit1
                        {
                            Id = count++,
                            OsRah = !typeof(DBNull).IsInstanceOfType(row["ос.рах"]) ? (string)row["ос.рах"] : null,
                            FullName = !typeof(DBNull).IsInstanceOfType(row["ПІП"]) ? (string)row["ПІП"] : null,
                            FullAdres = !typeof(DBNull).IsInstanceOfType(row["Адреса"]) ? (string)row["Адреса"] : null,
                            Borg = (decimal)row["борг"],
                            Month = !typeof(DBNull).IsInstanceOfType(row["місяць"]) ? (int?)row["місяць"] : null,
                            DataVykl = !typeof(DBNull).IsInstanceOfType(row["Дата відкл"]) ? (string?)row["Дата відкл"] : null
                        };
                        ActualDataPerson person = _context.ActualDatas
                                                    .Include(ad => ad.Person)
                                                    .Include(ad => ad.Poperedgenia)
                                                    .ThenInclude(p => p.PoperDruk)
                                                    .FirstOrDefault(ad => ad.Person.OsRah == zvit.OsRah);
                        zvit.DataPoper = null;
                        if (person != null)
                        {
                            DateTime? Poper = person.Poperedgenia.Poper;

                            if (Poper <= to)
                            {
                                zvit.DataPoper = person.Poperedgenia.Poper.ToString();
                            }
                        }

                        zvits.Add(zvit);
                    }
                }

                //формуємо звіт 1
                if (zvit1 != null)
                {
                    Excel excelController = new Excel(_context);
                    excelController.CreateZvit1(zvits, currentUser, stanom_na);
                    byte[] content = excelController.CreateFile();
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "zvit.xlsx"
                    );

                }

                //формуємо звіт 2
                if (zvit2 != null)
                {
                    DateTime per = period.Per_date;
                    Excel excelController = new Excel(_context);
                    excelController.CreateZvit2(zvits, currentUser, stanom_na, per);
                    byte[] content = excelController.CreateFile();
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "zvit.xlsx"
                    );
                }

                //формуємо звіт 3
                if (zvit3 != null)
                {
                    DateTime per = period.Per_date;
                    Excel excelController = new Excel(_context);
                    excelController.CreateZvit3(currentUser, stanom_na, per);
                    byte[] content = excelController.CreateFile();
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "zvit.xlsx"
                    );
                }

            }

            //формуємо звіт 4
            if (zvit4 != null)
            {
                List<ActualDataPerson> people = _context.ActualDatas
                    .Include(ad => ad.Person)
                    .Include(ad => ad.Poperedgenia)
                    .Include(ad => ad.Finance)
                    .Include(ad => ad.Vykl)
                    .Where(ad => ad.Person.Cok.Code == cokCode
                        && ad.Poperedgenia.Poper >= from && ad.Poperedgenia.Poper <= to)
                    .ToList();

                Dictionary<int, string> keyValue = new Dictionary<int, string>();
                DataTable dtr = BillingUtils.Zvit4(appEnvir, people, cokCode, from, to);
                //Наповнюємо словник даними зі скрипта дата викл. оср.
                foreach (DataRow row in dtr.Rows)
                {
                    keyValue.Add((int)row["AccountId"],
                        !typeof(DBNull).IsInstanceOfType(row["dateVykl"]) ? (string?)row["dateVykl"] : null);
                }

                List<Zvit1> zvits = new List<Zvit1>();
                int count = 1;

                foreach (ActualDataPerson ad in people)
                {
                    string DataVyklOsr;
                    int accID = int.Parse(ad.Person.AccountId);
                    Zvit1 zvit = new Zvit1
                    {
                        Id = count++,
                        OsRah = ad.Person.NewOsRah,
                        AccountId = int.Parse(ad.Person.AccountId),
                        FullName = ad.Person.FullName,
                        DebLoad = ad.Finance.DebLoad,
                        Borg = ad.Finance.DebPoch,
                        DataPoper = ad.Poperedgenia.Poper?.ToString("d"),
                        DataVykl = ad.Poperedgenia.DateVykl?.ToString("d"),
                        DataVyklOsr = keyValue.ContainsKey(int.Parse(ad.Person.AccountId))
                                       ? DataVyklOsr = keyValue[accID]
                                       : ad.Vykl.DateVykl?.ToString("d")
                    };
                    zvits.Add(zvit);
                }

                Excel excelController = new Excel(_context);
                excelController.CreateZvit4(currentUser, from, zvits);
                byte[] content = excelController.CreateFile();
                return File(
                    content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "zvit.xlsx"
                );
            }
            return View();
        }
        /// <summary>
        /// Завантаження абонентів в яких є борг
        /// </summary>
        /// <param name="SumaBorgu"></param>
        /// <param name="notPay"></param>
        /// <param name="monthBorg"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadAbon(string SumaBorgu, string notPay, string monthBorg)
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            string cokCode = currentUser.CokId == null ? "ORG" : currentUser.Cok.Code;
            Organization cok = _context.Coks.FirstOrDefault(c => c.Code == cokCode);
            bool isloads = cok.IsLoads;
            DataTable dtres;
            //вибираємо абонентів в кого є борг і він новий в програмі
            #region
            if (SumaBorgu != null && isloads == false)
            {
                cok.IsLoads = true;
                _ = _context.Coks.Update(cok);
                _ = _context.SaveChanges();

                DateTime date = DateTime.Now;
                int monthNotPay = 6;
                int monthBorgu = date.Day >= 20 ? 5 : 6;    //перевіряємо на 20 число місяця

                dtres = notPay != null && monthBorg != null
                    ? BillingUtils.GetUtilResults(cokCode, appEnvir, SumaBorgu, monthNotPay, monthBorgu)
                    : notPay == null && monthBorg != null
                        ? BillingUtils.GetUtilResults(cokCode, appEnvir, SumaBorgu, 0, monthBorgu)
                        : notPay != null && monthBorg == null
                                            ? BillingUtils.GetUtilResults(cokCode, appEnvir, SumaBorgu, monthNotPay, 0)
                                            : BillingUtils.GetUtilResults(cokCode, appEnvir, SumaBorgu, 0, 0);

                if (dtres != null)
                {
                    UpdateGroup updateGroup = new UpdateGroup
                    {
                        DateUpdate = DateTime.Now,
                        UserUpd = currentUser.FullName,
                        RiznSum = false,
                        IsLoad = true
                    };
                    _ = _context.UpdateGroups.Add(updateGroup);
                    _ = _context.SaveChanges();

                    List<LoadUtility> load = new List<LoadUtility>();

                    foreach (DataRow row in dtres.Rows)
                    {
                        LoadUtility loadUtility = LoadDataCreator.CreateLoadData(row, cokCode);
                        load.Add(loadUtility);
                    }

                    //Заповнюємо БД
                    foreach (LoadUtility item in load)
                    {
                        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = _context.Database.BeginTransaction();
                        try
                        {
                            CreateAbonents AbonentCreator = new CreateAbonents(cokCode, cok.Id, item, _context, currentUser, SumaBorgu, true);
                            AbonentCreator.CreateAddressWithDirection();
                            AbonentCreator.CreatePerson();
                            AbonentCreator.CreateFinance();
                            AbonentCreator.CreateLichylnyk();
                            AbonentCreator.CreateVykl();
                            AbonentCreator.CreatePoper();
                            _ = AbonentCreator.CreateActualData();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            transaction.Rollback();
                        }
                    }
                }
            }
            #endregion
            else
            {
                TempData["error"] = "BadSum";
                return RedirectToAction("Abonents", "Poper");
            }

            List<ActualDataPerson> loadperson = _context.ActualDatas
                .Include(ad => ad.Person)
                .Include(ad => ad.Finance)
                .Include(ad => ad.Poperedgenia)
                .Where(ad =>
                    (ad.Person.CokId == currentUser.CokId || currentUser.CokId == null)
                    && ad.Poperedgenia.LoadDay.Day == DateTime.Now.Day
                    && ad.Poperedgenia.LoadDay.Month == DateTime.Now.Month
                    && ad.Poperedgenia.LoadDay.Year == DateTime.Now.Year
                 )
                .ToList();

            cok.IsLoads = false;
            _ = _context.Coks.Update(cok);
            _ = _context.SaveChanges();

            return View("UploadAbon", loadperson);
        }
        /// <summary>
        /// Робимо поновлення даних (оплати, перерахунки та ін.)
        /// </summary>
        /// <returns></returns>
        public ActionResult Update()
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            //потрібно зробити перевірку на головного адміна він без організації
            string cokCode = currentUser.CokId == null ? "ORG" : currentUser.Cok.Code;

            Organization cok = _context.Coks.FirstOrDefault(c => c.Code == cokCode);
            List<Person> people = _context.Persons
                .Where(p => p.StatusAktyv && p.Cok.Code == cokCode)
                .ToList();
            DataTable dtres = BillingUtils.UpdatePoper(appEnvir, people, cokCode);

            List<int> updatedPeoples = new List<int>();
            if (dtres != null)
            {
                UpdateGroup updateGroup = new UpdateGroup
                {
                    DateUpdate = DateTime.Now,
                    UserUpd = currentUser.FullName,
                    RiznSum = false
                };
                List<UpdateFinance> actualUpdateFinances = new List<UpdateFinance>();

                //оновлюємо наші таблиці
                foreach (DataRow row in dtres.Rows)
                {
                    Person person = _context.Persons
                        .Include(p => p.Cok)
                        .FirstOrDefault(p => p.OsRah == row[0].ToString().Trim() && p.Cok.Code == cokCode);

                    Poperedgenia pop = _context.Poperedgenias
                        .Include(po => po.Person)
                        .ThenInclude(p => p.Cok)
                        .FirstOrDefault(
                            po => po.Person.OsRah == row[0].ToString().Trim() && po.Person.Cok.Code == cokCode
                        );

                    ActualDataPerson actualData = _context.ActualDatas
                        .Include(ad => ad.Finance)
                        .Include(ad => ad.Vykl)
                        .FirstOrDefault(a => a.PersonId == person.Id);

                    //виловлюємо в кого немає актуальних даних
                    //Console.WriteLine("osrahPerson - " + person.OsRah);
                    //Console.WriteLine("osrahPOP - " + pop.Person.OsRah);
                    //Console.WriteLine("OSRAH_actualData - " + actualData.Person.OsRah);

                    Finance fin = actualData.Finance;
                    Vykl vykl = actualData.Vykl;
                    string? osRah = row[0].ToString().Trim();
                    decimal NewDebet = decimal.Parse(row[2].ToString().Trim());
                    string? NewVykl = row[3].ToString().Trim();
                    bool financeWasChanged = false;

                    UpdateFinance upd = new UpdateFinance
                    {
                        PersonId = person.Id
                    };
                    //робимо перевірку чи є запис в цьому періоді з фінансами
                    if (fin.Period == Period.Per_now().Per_int)        //якщо є оновлюємо фінанси
                    {
                        if (NewDebet != fin.DebPoch)
                        {
                            financeWasChanged = true;
                            upd.PrevFinanceId = fin.Id;
                            upd.PrevFinanceSum = fin.DebPoch;
                            upd.NextFinanceId = fin.Id;
                            upd.NextFinanceSum = NewDebet;
                            updatedPeoples.Add(person.Id);
                            fin.DebPoch = NewDebet;
                        }
                    }
                    else     //якщо немає то створюємо новий
                    {
                        fin = new Finance
                        {
                            PersonId = person.Id,
                            DebPoch = NewDebet,
                            KredPoch = 0,
                            Period = Period.Per_now().Per_int
                        };
                        _ = _context.Add(fin);
                        _ = _context.SaveChanges();
                        financeWasChanged = true;
                        upd.NextFinanceId = fin.Id;
                        upd.NextFinanceSum = fin.DebPoch;
                        actualData.FinanceId = fin.Id;
                        updatedPeoples.Add(person.Id);
                    }

                    //статус пасивний коли борг = 0 або абонента відключили
                    if (fin.DebPoch == 0 || actualData.Vykl.DateVykl != null)
                    {
                        person.StatusAktyv = false;
                    }
                    // статус пасивний в кого не видано попередження і борг менший 1187
                    else if (fin.DebPoch < 1187 && pop.VydanePoper == false)
                    {
                        person.StatusAktyv = false;
                    }
                    // статус пасивний в кого вийшов час відключення і не відключили
                    else if (pop.DateVykl < DateTime.Now.Date && actualData.Vykl.DateVykl == null)
                    {
                        person.StatusAktyv = false;
                    }

                    if (financeWasChanged)
                    {
                        updateGroup.RiznSum = true;
                        actualUpdateFinances.Add(upd);
                    }

                    if (!string.IsNullOrEmpty(NewVykl))
                    {
                        DateTime newVyklDate = DateTime.Parse(NewVykl);
                        int rik = newVyklDate.Year;
                        int mis = newVyklDate.Month;

                        if (mis < 10)
                        {
                            mis = int.Parse("0" + mis.ToString());
                        }

                        string Per_str = rik.ToString() + mis.ToString();
                        int Per_int = int.Parse(Per_str);

                        //якщо абонента відключили в цьому місяці то статус ставимо не активний
                        if (Per_int == Period.Per_now().Per_int)
                        {
                            person.StatusAktyv = false;
                        }
                        vykl.DateVykl = newVyklDate;
                    }
                    else
                    {
                        vykl.DateVykl = null;
                    }
                }

                UpdateGroup lastUpdateGroup = _context.UpdateGroups
                    .OrderByDescending(u => u.DateUpdate)
                    .FirstOrDefault(u => u.UserUpd == userName);

                //Якщо є зміни у фінансах і є різниця суми то зберігаємо
                if ((actualUpdateFinances.Count > 0 && updateGroup.RiznSum) || lastUpdateGroup == null)
                {
                    _ = _context.UpdateGroups.Add(updateGroup);
                    _ = _context.SaveChanges();
                    foreach (UpdateFinance uf in actualUpdateFinances)
                    {
                        uf.UpdateGroupId = updateGroup.Id;
                        _ = _context.UpdateFinances.Add(uf);
                    }
                }
                //якщо немає апдейта за цю дату
                else if (lastUpdateGroup.DateUpdate.Date != DateTime.Now.Date)
                {
                    _ = _context.UpdateGroups.Add(updateGroup);
                }
                _ = _context.SaveChanges();
            }

            List<ActualDataPerson> allAbonents = _context.ActualDatas
                .Include(ad => ad.Person)
                .Include(ad => ad.Finance)
                .Where(ad => ad.Person.CokId == currentUser.CokId || currentUser.CokId == null)
                .ToList();
            ViewData["Updated"] = updatedPeoples;

            UpdateGroup ug = _context.UpdateGroups
                .Include(ug => ug.UpdateFinances)
                .ThenInclude(up => up.Person)
                .OrderByDescending(u => u.DateUpdate)
                .FirstOrDefault(ug => ug.UserUpd == currentUser.FullName);

            return ug.UpdateFinances.Count == 0
                ? View("Abonents", allAbonents)
                : View("UpdateFinance", ug.UpdateFinances);
        }
        /// <summary>
        /// Проставляння або відміна статусу "активний"
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult SetActivePerson(int id)
        {
            Person person = _context.Persons
                .Include(p => p.Poperedgenias)
                .FirstOrDefault(p => p.Id == id);

            if (person == null)
            {
                return StatusCode(404);
            }
            if (HttpContext.Request.Query.ContainsKey("status"))
            {
                try
                {
                    bool actyv = HttpContext.Request.Query["status"].ToString() == "1";
                    person.StatusAktyv = actyv;
                    //тут треба добавити відміну попередження
                    _ = _context.SaveChanges();
                }
                catch (Exception)
                {
                    return StatusCode(500);
                }
                return StatusCode(200);
            }
            else
            {
                return StatusCode(400);
            }
        }
        /// <summary>
        /// Добавлення абонента, який не попадає в програму, але потрібно його подати на відключення
        /// </summary>
        /// <param name="OsRah"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Create(string OsRah)
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            string cokCode = currentUser.CokId == null ? "ORG" : currentUser.Cok.Code;

            Organization cok = _context.Coks.FirstOrDefault(c => c.Code == cokCode);
            string SumaBorgu = "0";

            if (!string.IsNullOrEmpty(OsRah))
            {
                bool isNum = int.TryParse(OsRah, out int Num);
                if (isNum)
                {
                    DataTable men = BillingUtils.AddAbons(appEnvir, OsRah, cokCode);
                    if (men != null && men.Rows.Count > 0)
                    {
                        LoadUtility item = LoadDataCreator.CreateLoadData(men.Rows[0], cokCode);
                        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = _context.Database.BeginTransaction();
                        try
                        {
                            // робимо зміни в БД
                            CreateAbonents AbonentCreator = new CreateAbonents(cokCode, cok.Id, item, _context, currentUser, SumaBorgu, false);
                            AbonentCreator.CreateAddressWithDirection();
                            AbonentCreator.CreatePerson();
                            AbonentCreator.CreateFinance();
                            AbonentCreator.CreateLichylnyk();
                            AbonentCreator.CreateVykl();
                            AbonentCreator.CreatePoper();
                            _ = AbonentCreator.CreateActualData();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            transaction.Rollback();
                        }
                    }
                    else
                    {
                        TempData["error"] = "Netu";
                        return RedirectToAction("Abonents", "Poper");
                    }
                }
            }
            else
            {
                TempData["error"] = "BadOs";
                return RedirectToAction("Abonents", "Poper");
            }

            int menId = _context.ActualDatas
                .Include(ad => ad.Person)
                .FirstOrDefault(ad => ad.Person.OsRah == OsRah && ad.Person.Cok.Code == cokCode)
                .Id;

            return RedirectToAction("Index", "AbonDetails", new { Id = menId });
        }
    }
}