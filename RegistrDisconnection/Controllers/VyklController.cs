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
using RegistrDisconnection.MyClasses;
using RegistrDisconnection.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RegistrDisconnection.Controllers
{
    /// <summary>
    /// Робота з відключиними
    /// </summary>
    [Authorize(Roles = "Адміністратор, Користувач, Бухгалтер")]
    public class VyklController : Controller
    {
        private readonly MainContext _context;
        private readonly IWebHostEnvironment appEnvir;
        public DateTime per;
        public string period;

        public VyklController(MainContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            appEnvir = appEnvironment;
        }

        // GET: Vykl
        public IActionResult Index()
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            string cokCode = currentUser.Cok.Code;
            string period = currentUser.Cok.CurrPeriod.ToString()[..4] + "-"
                        + currentUser.Cok.CurrPeriod.ToString().Substring(4, 2) + "-01";
            per = DateTime.Parse(period);
            int currentPeriod = currentUser.Cok.CurrPeriod;

            IQueryable<ActualDataPerson> mainContext = _context.ActualDatas
            .Include(a => a.Finance)
            .Include(a => a.Lichylnyk)
            .Include(a => a.Person)
            .ThenInclude(p => p.Cok)
            .Include(a => a.Vykl)
            .Include(a => a.Saldo)
            .Where(
                a => (
                        a.Person.CokId == currentUser.CokId || currentUser.CokId == null
                ) && a.Vykl.Status == true && a.SaldoId != null
            ).OrderBy(p => p.Person.OsRah);

            //вибираємо актуальне сальдо
            List<ActualDataPerson> actuals = _context.ActualDatas
                .Include(a => a.Person)
                .Include(a => a.Saldo)
                .Where(a => a.Person.CokId == currentUser.Cok.Id
                        && a.Saldo.AktPeriod < currentPeriod
                        && a.Saldo.ZakrPeriod != true)
                .ToList();

            if (actuals.Count != 0)
            {
                int pr = 0;
                foreach (ActualDataPerson periods in actuals)
                {
                    pr = (int)periods.Saldo.AktPeriod;
                }

                DateTime peridNot = DateTime.Parse(pr.ToString()[..4] + "-" + pr.ToString().Substring(4, 2) + "-01");

                AbonentVykl vModel = new AbonentVykl
                {
                    MainContext = mainContext.ToList(),
                    PeriodStr = per.ToString("Y"),
                    ZakrPeriod = peridNot.ToString("Y")
                };

                ViewBag.error = "NotZakrPer";
                return View(vModel);
            }

            AbonentVykl viewModel = new AbonentVykl
            {
                MainContext = mainContext.ToList(),
                PeriodStr = per.ToString("Y")
            };

            return View(viewModel);
        }

        // GET: Vykl/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ActualDataPerson actualDataPerson = await _context.ActualDatas
                .Include(a => a.Finance)
                .Include(a => a.Lichylnyk)
                .Include(a => a.Person)
                .Include(a => a.Poperedgenia)
                .Include(a => a.Vykl)
                .Include(a => a.Saldo)
                .FirstOrDefaultAsync(m => m.Id == id);

            return actualDataPerson == null
                ? NotFound()
                : (IActionResult)View(actualDataPerson);
        }

        // GET: Vykl/Create
        public IActionResult Create()
        {
            ViewData["FinanceId"] = new SelectList(_context.Finances, "Id", "Id");
            ViewData["LichylnykId"] = new SelectList(_context.Lichylnyks, "Id", "Number");
            ViewData["PersonId"] = new SelectList(_context.Persons, "Id", "AccountId");
            ViewData["PoperedgeniaId"] = new SelectList(_context.Poperedgenias, "Id", "Id");
            ViewData["VyklId"] = new SelectList(_context.Vykls, "Id", "Id");
            return View();
        }

        // POST: Vykl/Create (добавлення абонента поодиночно)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string OsRah)
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            string cokCode = currentUser.CokId == null ? "ORG" : currentUser.Cok.Code;

            Organization cok = _context.Coks.FirstOrDefault(c => c.Code == cokCode);
            string SumaBorgu = "0";
            ActualDataPerson adPerson;

            //робимо пошук по ос рах 
            IQueryable<ActualDataPerson> people = _context.ActualDatas
                .Include(ad => ad.Person).ThenInclude(p => p.Cok)
                .Include(ad => ad.Person).ThenInclude(p => p.Address).ThenInclude(a => a.Direction)
                .Include(ad => ad.Finance)
                .Include(ad => ad.Poperedgenia)
                .Include(ad => ad.Lichylnyk)
                .Include(ad => ad.Vykl)
                .Where(ad => ad.Person.Cok.Code == cokCode);

            if (!string.IsNullOrEmpty(OsRah))
            {
                bool isNum = int.TryParse(OsRah, out int Num);
                people = isNum
                        ? people.Where(m => m.Person.OsRah.Contains(OsRah))
                        : people.Where(m => m.Person.FullName.Contains(OsRah));

                if (isNum)
                {
                    string period = Period.Per_now().Per_str;
                    DataTable men = BillingUtils.AddVykl(appEnvir, OsRah, cokCode, period);

                    if (men != null && men.Rows.Count > 0)
                    {
                        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = _context.Database.BeginTransaction();
                        try
                        {
                            LoadUtility item = LoadDataCreator.CreateLoadData(men.Rows[0], cokCode);
                            CreateAbonents AbonentCreator = new CreateAbonents(cokCode, cok.Id, item, _context, currentUser, SumaBorgu, false);
                            AbonentCreator.CreateAddressWithDirection();
                            AbonentCreator.CreatePerson();
                            AbonentCreator.CreateFinance();
                            AbonentCreator.CreateLichylnyk();
                            AbonentCreator.CreateVykl();
                            AbonentCreator.CreatePoper();
                            adPerson = AbonentCreator.CreateActualData();

                            bool SaldoExisted = true;
                            Saldo saldo = _context.Saldos.FirstOrDefault(ts => ts.Id == adPerson.SaldoId);

                            if (saldo == null)
                            {
                                SaldoExisted = false;
                                saldo = new Saldo
                                {
                                    PersonId = (int)adPerson.PersonId,
                                    Vykl = adPerson.Vykl,
                                    AktPeriod = cok.CurrPeriod,
                                    DebPoch = 0,
                                    KredPoch = 0,
                                    Oplata = 0,
                                    Recount = 0,
                                    SumaVykl = 0,
                                    SumaVkl = 0,
                                    BorgZaEE = 0,
                                    OplataZaEE = 0
                                };
                            }
                            DateTime dbDisconn = item.DateDiscon.Trim() != ""
                                                 ? Convert.ToDateTime(item.DateDiscon.Trim())
                                                 : DateTime.Now;
                            int saldoPeriod = (int)saldo.AktPeriod;
                            //TODO: тут ловимо помилку
                            //при пошуку
                            //якщо абонент є, а дати відключення немає
                            if (!SaldoExisted || saldo.Vykl.DateVykl != dbDisconn)
                            {
                                saldo.StartPeriod = Period.WithOffset(dbDisconn, 0).Per_int;
                                saldo.Narah = item.RestSumm;
                                saldo.BorgZaEE = item.RestSumm;
                                saldo.Vykl = adPerson.Vykl;
                            }
                            else
                            {
                                if (saldo.StartPeriod != Period.WithOffset(dbDisconn, 0).Per_int)
                                {
                                    saldo.Recount = item.RestSumm - saldo.DebPoch;
                                    saldo.BorgZaEE = item.RestSumm;
                                }
                            }

                            //Так було для обєднаних борг і оплати за активну + викл/вкл
                            //decimal? narah = saldo.DebPoch + saldo.Narah + saldo.Recount + saldo.SumaVkl + saldo.SumaVykl;
                            //Без активної е/е
                            decimal? narah = saldo.DebPoch + saldo.SumaVkl + saldo.SumaVykl;
                            decimal? oplat = saldo.KredPoch + saldo.Oplata;
                            decimal? saldoEnd = narah - oplat;
                            decimal? oplataEE = saldo.Narah - saldo.BorgZaEE;
                            saldo.DebKin = 0;
                            saldo.KredKin = 0;
                            saldo.OplataZaEE = oplataEE > 0 ? oplataEE : 0;

                            if (saldoEnd > 0)
                            {
                                saldo.DebKin = saldoEnd;
                            }
                            else
                            {
                                saldo.KredKin = saldoEnd * -1;
                            }
                            if (!SaldoExisted)
                            {
                                _ = _context.Saldos.Add(saldo);
                            }
                            _ = _context.SaveChanges();

                            adPerson.SaldoId = saldo.Id;
                            _ = _context.SaveChanges();

                            ActualDataPerson ab = await _context.ActualDatas
                                .Include(ad => ad.Person)
                                .ThenInclude(p => p.Address)
                                .ThenInclude(a => a.Direction)
                                .Include(ad => ad.Finance)
                                .Include(ad => ad.Poperedgenia)
                                .Include(ad => ad.Lichylnyk)
                                .Include(ad => ad.Vykl)
                                .FirstOrDefaultAsync(ad => ad.Id == adPerson.Id);

                            transaction.Commit();
                            return ab == null ? NotFound() : (IActionResult)View("Details", ab);
                        }
                        catch (Exception ex)
                        {
                            Console.OutputEncoding = System.Text.Encoding.UTF8;
                            Console.WriteLine("Pomylka - " + ex);
                            transaction.Rollback();
                        }
                    }
                    else
                    {
                        TempData["error"] = "Netu";
                        return RedirectToAction("Index", "Vykl");
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Vykl");
                }
            }

            TempData["error"] = "BadOs";
            return RedirectToAction("Index", "Vykl");
        }

        // GET: Vykl/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ActualDataPerson ab = await _context.ActualDatas
                .Include(ad => ad.Person)
                .ThenInclude(p => p.Address)
                .ThenInclude(a => a.Direction)
                .Include(ad => ad.Finance)
                .Include(ad => ad.Poperedgenia)
                .Include(ad => ad.Lichylnyk)
                .Include(ad => ad.Vykl)
                .Include(ad => ad.Saldo)
                .FirstOrDefaultAsync(m => m.Id == id);
            return View(ab);
        }

        // POST: Vykl/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, decimal? SumaVykl, decimal? SumaVkl, decimal? Oplata, decimal? Narah,
                                                string? Pokazy, decimal? DebKin, decimal? KredKin, decimal? OplataZaEE,
                                                decimal? BorgZaEE, IFormCollection form, DateTime dateVykl)
        {
            ActualDataPerson ab = await _context.ActualDatas
                .Include(ad => ad.Person)
                .ThenInclude(p => p.Address)
                .ThenInclude(a => a.Direction)
                .Include(ad => ad.Finance)
                .Include(ad => ad.Poperedgenia)
                .Include(ad => ad.Lichylnyk)
                .Include(ad => ad.Vykl)
                .Include(ad => ad.Saldo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (id != ab.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (ab.Saldo.DebPoch > 0)
                {
                    ab.Saldo.KredPoch = 0;
                }

                ab.Saldo.SumaVykl = SumaVykl != null ? SumaVykl : 0;
                ab.Saldo.SumaVkl = SumaVkl != null ? SumaVkl : 0;
                ab.Saldo.Oplata = Oplata != null ? Oplata : 0;
                ab.Saldo.Narah = Narah != null ? Narah : 0;
                ab.Saldo.OplataZaEE = OplataZaEE != null ? OplataZaEE : 0;
                ab.Saldo.BorgZaEE = BorgZaEE != null ? BorgZaEE : 0;

                if (Pokazy != null)
                {
                    ab.Lichylnyk.Pokazy = Pokazy;
                }

                if (DebKin != null)
                {
                    ab.Saldo.DebKin = DebKin;
                }

                if (KredKin != null)
                {
                    ab.Saldo.KredKin = KredKin;
                }

                if (dateVykl != null)
                {
                    ab.Vykl.DateVykl = dateVykl;
                }

                try
                {
                    _ = _context.Update(ab);
                    _ = await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActualDataPersonExists(ab.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ab);
        }

        // GET: Vykl/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ActualDataPerson actualDataPerson = await _context.ActualDatas
                .Include(a => a.Finance)
                .Include(a => a.Lichylnyk)
                .Include(a => a.Person)
                .Include(a => a.Poperedgenia)
                .Include(a => a.Vykl)
                .FirstOrDefaultAsync(m => m.Id == id);

            return actualDataPerson == null
                ? NotFound()
                : (IActionResult)View(actualDataPerson);
        }

        // POST: Vykl/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ActualDataPerson actualDataPerson = await _context.ActualDatas
                .Include(ad => ad.Saldo)
                .FirstOrDefaultAsync(ad => ad.Id == id);

            _ = _context.ActualDatas.Remove(actualDataPerson);
            _ = _context.Saldos.Remove(actualDataPerson.Saldo);

            _ = await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        /// <summary>
        /// Оновлення інформації
        /// </summary>
        /// <returns></returns>
        public ActionResult Update()
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            int currentPeriod = currentUser.Cok.CurrPeriod;

            string cokCode = currentUser.CokId == null ? "ORG" : currentUser.Cok.Code;
            Organization cok = _context.Coks.FirstOrDefault(c => c.Code == cokCode);

            List<ActualDataPerson> actualsaldo = _context.ActualDatas
                                .Include(a => a.Person)
                                .Include(a => a.Vykl)
                                .Include(a => a.Saldo)
                                .Where(a => a.Vykl.Status == true && a.Person.CokId == currentUser.CokId
                                        && a.Saldo.AktPeriod == currentPeriod)
                                .ToList();

            DataTable dtres = BillingUtils.UpdateVykl(appEnvir, actualsaldo, cokCode);
            if (dtres != null)
            {
                List<UpdateVykl> loads = new List<UpdateVykl>();
                foreach (DataRow row in dtres.Rows)
                {
                    string accId = row[1].ToString().Trim();

                    Person person = _context.Persons
                        .Include(p => p.Cok)
                        .FirstOrDefault(p => p.AccountId == accId && p.Cok.Code == cokCode);

                    ActualDataPerson actualData = _context.ActualDatas
                        .Include(ad => ad.Saldo)
                        .Include(ad => ad.Vykl)
                        .FirstOrDefault(a => a.Person.AccountId == accId && a.Saldo.AktPeriod == currentPeriod);


                    bool SaldoExisted = true;
                    Saldo saldo = _context.Saldos.FirstOrDefault(s => s.Id == actualData.Saldo.Id);

                    Vykl vykl = actualData.Vykl;
                    decimal NewDebet = decimal.Parse(row[2].ToString().Trim());
                    DateTime DataVykl;
                    DataVykl = DateTime.Parse(row[3].ToString().Trim());
                    bool StatusVykl = row[4].ToString().Trim() == "1";

                    if (saldo.Vykl.DateVykl != DataVykl || !SaldoExisted)
                    {
                        vykl.DateVykl = DataVykl;
                        vykl.Period = Period.WithOffset(DataVykl, 0).Per_int;
                        vykl.Status = StatusVykl;
                        _ = _context.SaveChanges();
                    }

                    bool saldoWasChanged = false;
                    if (saldo.AktPeriod == currentPeriod)
                    {
                        if (NewDebet != saldo.BorgZaEE && saldoWasChanged != true)
                        {
                            saldoWasChanged = true;
                            saldo.BorgZaEE = NewDebet;
                            _ = _context.SaveChanges();
                        }
                    }
                }
            }
            else
            {
                ViewBag.error = "NotUpdate";
            }
            List<ActualDataPerson> people = _context.ActualDatas
                            .Include(a => a.Finance)
                            .Include(a => a.Lichylnyk)
                            .Include(a => a.Person)
                            .Include(a => a.Vykl)
                            .Include(a => a.Saldo)
                            .Where(a => a.Vykl.Status == true && a.Person.CokId == currentUser.CokId
                                    && a.Saldo.AktPeriod == currentPeriod)
                            .ToList();

            return View("AddAbon", people);
        }

        private bool ActualDataPersonExists(int id)
        {
            return _context.ActualDatas.Any(e => e.Id == id);
        }

        /// <summary>
        /// Закриття періоду
        /// </summary>
        /// <returns></returns>
        public ActionResult ClosePeriod()
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users
                                .Include(u => u.Cok)
                                .FirstOrDefault(u => u.Login == userName);
            string cokCode = currentUser.CokId == null ? "ORG" : currentUser.Cok.Code;
            Organization cok = _context.Coks.FirstOrDefault(c => c.Code == cokCode);

            //проводимо закриття періоду
            int currentPeriod = cok.CurrPeriod;
            int nextPeriod = Period.WithOffset(new Period(currentPeriod).Per_date, 1).Per_int;

            DateTime ProgaPeriod = DateTime.Parse(currentUser.Cok.CurrPeriod.ToString().Substring(0, 4) + "-"
                                    + currentUser.Cok.CurrPeriod.ToString().Substring(4, 2) + "-01");
            DateTime NowDate = DateTime.Now;
            DateTime PeriodFrom = new DateTime(NowDate.Year, NowDate.Month, 10);
            DateTime Periodto = new DateTime(NowDate.Year, NowDate.Month, 1).AddMonths(1).AddDays(-1);
            DateTime Today = NowDate.Date;
            int KalendarPeriod = new Period(Today).Per_int;

            //робимо перевірку, щоб не закривали період на перед
            #region
            if (Today >= PeriodFrom && Today <= Periodto && currentPeriod < KalendarPeriod)
            {
                //вибираємо сальдовки де період в програмі = періоду сальда
                List<Saldo> currentSaldos = _context.Saldos
                                                .Include(s => s.Vykl)
                                                .Where(s => s.AktPeriod == currentPeriod
                                                        && s.Person.CokId == cok.Id
                                                        && s.ZakrPeriod != true)
                                                .ToList();
                //вибираємо сальдовки де період в програмі > періоду сальда
                List<Saldo> PrevSaldos = _context.Saldos
                                                .Include(s => s.Vykl)
                                                .Where(s => s.AktPeriod < currentPeriod
                                                        && s.Person.CokId == cok.Id
                                                        && s.ZakrPeriod != true)
                                                .ToList();

                //закриваємо період
                foreach (Saldo s in currentSaldos)
                {
                    using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = _context.Database.BeginTransaction();
                    try
                    {
                        //тут потрібно зробити перевірку чи таке сальдо вже є, якщо так просто переписати, якщо ні то створити нове
                        bool SaldoExist = true;
                        Saldo existedSaldo = _context.Saldos.FirstOrDefault(ss => ss.AktPeriod == nextPeriod
                            && ss.PersonId == s.PersonId);
                        if (existedSaldo == null)
                        {
                            SaldoExist = false;
                            existedSaldo = new Saldo
                            {
                                PersonId = s.PersonId,
                                DebPoch = s.DebKin,
                                KredPoch = s.KredKin,
                                AktPeriod = nextPeriod,
                                KredKin = s.KredKin,
                                DebKin = s.DebKin,
                                Narah = 0,
                                Oplata = 0,
                                Recount = 0,
                                SumaVkl = 0,
                                SumaVykl = 0,
                                StartPeriod = s.StartPeriod,
                                VyklId = s.VyklId,
                                BorgZaEE = s.BorgZaEE,
                                OplataZaEE = s.OplataZaEE
                            };
                        }
                        else
                        {
                            existedSaldo.PersonId = s.PersonId;
                            existedSaldo.DebPoch = s.DebKin;
                            existedSaldo.KredPoch = s.KredKin;
                            existedSaldo.AktPeriod = nextPeriod;
                            existedSaldo.KredKin = s.KredKin;
                            existedSaldo.DebKin = s.DebKin;
                            existedSaldo.Narah = 0;
                            existedSaldo.Oplata = 0;
                            existedSaldo.Recount = 0;
                            existedSaldo.SumaVkl = 0;
                            existedSaldo.SumaVykl = 0;
                            existedSaldo.StartPeriod = s.StartPeriod;
                            existedSaldo.VyklId = s.VyklId;
                            existedSaldo.BorgZaEE = s.BorgZaEE;
                            existedSaldo.OplataZaEE = 0;
                            _ = _context.SaveChanges();
                        }

                        if (s.DebKin != 0 || s.KredKin != 0)
                        {
                            ActualDataPerson adp = _context.ActualDatas.FirstOrDefault(ad => ad.PersonId == s.PersonId);
                            if (!SaldoExist)
                            {
                                _ = _context.Saldos.Add(existedSaldo);
                                s.ZakrPeriod = true;
                            }

                            s.ZakrPeriod = true;
                            _ = _context.SaveChanges();

                            adp.SaldoId = existedSaldo.Id;
                            _ = _context.SaveChanges();
                        }
                        else if (s.DebKin == 0 && s.KredKin == 0)
                        {
                            s.ZakrPeriod = true;
                            _ = _context.SaveChanges();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        transaction.Rollback();
                    }
                }
                //проставляємо новий період в програмі
                if (nextPeriod <= KalendarPeriod)
                {
                    cok.CurrPeriod = nextPeriod;
                    _ = _context.SaveChanges();
                }

                IQueryable<ActualDataPerson> mainContext = _context.ActualDatas
                    .Include(a => a.Finance)
                    .Include(a => a.Lichylnyk)
                    .Include(a => a.Person)
                    .ThenInclude(p => p.Cok)
                    .Include(a => a.Vykl)
                    .Include(a => a.Saldo)
                    .Where(
                        a => (
                                a.Person.CokId == currentUser.CokId || currentUser.CokId == null
                        ) && a.Vykl.Status == true && a.SaldoId != null
                    ).OrderBy(p => p.Person.OsRah);

                string period = currentUser.Cok.CurrPeriod.ToString().Substring(0, 4) + "-"
                    + currentUser.Cok.CurrPeriod.ToString().Substring(4, 2) + "-01";
                DateTime per = DateTime.Parse(period);

                AbonentVykl viewModel = new AbonentVykl
                {
                    MainContext = mainContext.ToList(),
                    PeriodStr = per.ToString("Y")
                };

                return View("Index", viewModel);

            }
            #endregion
            else
            {
                IQueryable<ActualDataPerson> mainContext = _context.ActualDatas
                    .Include(a => a.Finance)
                    .Include(a => a.Lichylnyk)
                    .Include(a => a.Person)
                    .ThenInclude(p => p.Cok)
                    .Include(a => a.Vykl)
                    .Include(a => a.Saldo)
                    .Where(
                        a => (
                                a.Person.CokId == currentUser.CokId || currentUser.CokId == null
                        ) && a.Vykl.Status == true && a.SaldoId != null
                    ).OrderBy(p => p.Person.OsRah);

                string period = currentUser.Cok.CurrPeriod.ToString().Substring(0, 4) + "-"
                    + currentUser.Cok.CurrPeriod.ToString().Substring(4, 2) + "-01";
                DateTime per = DateTime.Parse(period);

                AbonentVykl viewModel = new AbonentVykl
                {
                    MainContext = mainContext.ToList(),
                    PeriodStr = per.ToString("Y")
                };

                TempData["error"] = "BadPeriod";
                return View("Index", viewModel);
            }
        }

    }
}
