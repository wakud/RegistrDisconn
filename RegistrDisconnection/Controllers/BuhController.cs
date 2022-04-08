using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistrDisconnection.Data;
using RegistrDisconnection.Models;
using RegistrDisconnection.Models.Abonents;
using RegistrDisconnection.Models.Dictionaries;
using RegistrDisconnection.Models.Users;
using RegistrDisconnection.Models.Zvity;
using RegistrDisconnection.MyClasses;
using RegistrDisconnection.ViewModels;
using SharpDocx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistrDisconnection.Controllers
{
    /// <summary>
    /// Тільки для бухгалтерів
    /// </summary>
    public class BuhController : Controller
    {
        private readonly MainContext _context;
        private readonly IWebHostEnvironment appEnvir;

        public BuhController(MainContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            appEnvir = appEnvironment;
        }

        // GET: Buh
        [Authorize(Roles = "Адміністратор, Бухгалтер")]
        public async Task<IActionResult> Index(string name, string myDatepicker, int? CokId)
        {
            string userName = User.Identity.Name;
            User currentUser = await _context.Users
                .Include(u => u.Cok)
                .Include(u => u.Prava)
                .FirstOrDefaultAsync(u => u.Login == userName);
            string cokCode;
            //потрібно зпобити перевірку на головного бухгалтера і адміна він без організації
            if (CokId == null && currentUser.CokId == null &&
                (currentUser.Prava.Name == "Бухгалтер" || currentUser.Prava.Name == "Адміністратор"))
            {
                return RedirectToAction("BuhIndex", "Buh");
            }
            //це для бухгалтера організації
            #region
            else
            {
                Organization Cok;
                if (CokId != null)
                {
                    Cok = await _context.Coks.FirstOrDefaultAsync(c => c.Id == CokId);
                    cokCode = Cok.Code;
                }
                else
                {
                    Cok = currentUser.Cok;
                    cokCode = currentUser.Cok.Code;     //витягуємо код цоку
                }
                int CokPeriod = Cok.CurrPeriod;         // витягуємо активний період ЦОКу
                DateTime per = DateTime.Now;           // текучий календарний період тип дата
                string SelectPeriod;
                int SelectPeriodInt;
                //перевіряємо чи вибраний період
                if (myDatepicker == null || myDatepicker.Trim() == "")
                {
                    SelectPeriodInt = CokPeriod;
                    SelectPeriod = CokPeriod.ToString().Insert(4, "-");
                }
                else
                {
                    SelectPeriodInt = int.Parse(myDatepicker.Remove(4, 1));
                    SelectPeriod = myDatepicker;
                }
                //робимо вибірку даних по цоку згідно активного періоду ЦОКу
                IEnumerable<Saldo> people = await _context.Saldos
                        .Include(s => s.Person)
                        .Where(s => s.Person.Cok.Code == cokCode && s.AktPeriod == SelectPeriodInt)
                        .ToListAsync();
                //робимо пошук по ос рах або піп
                if (!string.IsNullOrEmpty(name))
                {
                    bool isNum = int.TryParse(name, out int Num);
                    people = isNum
                        ? people.Where(m => m.Person.OsRah.Contains(name))
                        : people.Where(m => m.Person.FullName.Contains(name));
                }
                //Заповнюємо модель даними
                ViewArhiv viewModel = new ViewArhiv
                {
                    MainContexts = people,
                    PeriodStr = per.ToString("Y"),
                    Period = Cok.CurrPeriod,
                    SearchPeriod = SelectPeriod,
                    Name = name
                };

                return View(viewModel);
            }
            #endregion 
        }
        /// <summary>
        /// Головна сторінка головного бузгалтера (список організацій і їх фінанси)
        /// </summary>
        /// <param name="myDatepicker"></param>
        /// <returns></returns>
        [Authorize(Roles = "Адміністратор, Бухгалтер")]
        public IActionResult BuhIndex(string myDatepicker)
        {
            DateTime per = DateTime.Now;
            int perInt = Period.Per_now().Per_int;
            int SelectPeriodInt = myDatepicker == null || myDatepicker.Trim() == ""
                ? perInt
                : int.Parse(myDatepicker.Remove(4, 1));
            //вибираємо сальдовки згідно вибраного періоду
            IEnumerable<Saldo> Saldos = _context.Saldos
                .Include(s => s.Person)
                .ThenInclude(p => p.Cok)
                .Include(s => s.Vykl)
                .Where(s => s.AktPeriod == SelectPeriodInt)
                .ToList();
            //створюємо словник де ключ-організація, значення - поля для моделі
            Dictionary<int, SelectedGrouping> Selected = new Dictionary<int, SelectedGrouping>();
            foreach (Saldo s in Saldos)
            {
                if (!Selected.ContainsKey(s.Person.Cok.Id))
                {
                    Selected[s.Person.Cok.Id] = new SelectedGrouping
                    {
                        CokId = s.Person.Cok.Id,
                        Cok = s.Person.Cok.Name,
                        DebStart = (decimal)0.0,
                        CredStart = (decimal)0.0,
                        Narah = (decimal)0.0,
                        Opl = (decimal)0.0,
                        DebEnd = (decimal)0.0,
                        CredEnd = (decimal)0.0
                    };
                }
                //сумуємо кожне поле по організації
                Selected[s.Person.Cok.Id].DebStart += s.DebPoch;
                Selected[s.Person.Cok.Id].CredStart += s.KredPoch;
                Selected[s.Person.Cok.Id].Narah += s.SumaVykl + s.SumaVkl;
                Selected[s.Person.Cok.Id].Opl += s.Oplata;
                Selected[s.Person.Cok.Id].DebEnd += s.DebKin;
                Selected[s.Person.Cok.Id].CredEnd += s.KredKin;
            }

            ViewArhiv viewModelArch = new ViewArhiv
            {
                Selected = Selected,
                PeriodStr = per.ToString("Y"),
                Period = perInt,
                SearchPeriod = SelectPeriodInt.ToString().Insert(4, "-"),
            };
            return View(viewModelArch);
        }
        /// <summary>
        /// Детально по кожній організації (для гол.бух.)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ActualDataPerson actualDataPerson = await _context.ActualDatas
                .Include(a => a.Lichylnyk)
                .Include(a => a.Person)
                .ThenInclude(p => p.Saldos)
                .Include(a => a.Vykl)
                .Include(a => a.Saldo)
                .FirstOrDefaultAsync(a => a.PersonId == id);

            return actualDataPerson == null
                ? NotFound()
                : (IActionResult)View(actualDataPerson);
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
                .Include(ad => ad.Lichylnyk)
                .Include(ad => ad.Vykl)
                .Include(ad => ad.Saldo)
                .FirstOrDefaultAsync(m => m.Id == id);
            return View(ab);
        }

        // POST: Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, decimal? SumaVykl, decimal? SumaVkl, decimal? Oplata, decimal? Narah,
                                                string? Pokazy, decimal? DebKin, decimal? KredKin, decimal? DebPoch,
                                                decimal? KredPoch, decimal? BorgZaEE, decimal? OplataZaEE, DateTime dateVykl)
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
                .FirstOrDefaultAsync(ad => ad.Id == id);

            if (id != ab.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (DebPoch != null)
                {
                    ab.Saldo.DebPoch = DebPoch;
                }

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

                if (KredPoch != null)
                {
                    ab.Saldo.KredPoch = KredPoch;
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
                //return RedirectToAction("Details", ab);
                return RedirectToAction(nameof(Index));
            }
            return View(ab);
        }

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
                .FirstOrDefaultAsync(m => m.SaldoId == id);

            return actualDataPerson == null
                ? NotFound()
                : (IActionResult)View(actualDataPerson);
        }

        // POST: Buh/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ActualDataPerson actualDataPerson = await _context.ActualDatas
                .Include(ad => ad.Saldo)
                .FirstOrDefaultAsync(ad => ad.SaldoId == id);

            _ = _context.ActualDatas.Remove(actualDataPerson);
            _ = _context.Saldos.Remove(actualDataPerson.Saldo);

            _ = await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
            //потрібно зробити перевірку на головного адміна він без організації
            string cokCode = currentUser.CokId == null
                ? "ORG"
                : currentUser.Cok.Code;
            Organization cok = _context.Coks.FirstOrDefault(c => c.Code == cokCode);

            IQueryable<ActualDataPerson> mainContext = _context.ActualDatas
                .Include(a => a.Person)
                .Include(a => a.Vykl)
                .Include(a => a.Lichylnyk)
                .Include(a => a.Saldo)
                .Where(a => (
                            a.Person.CokId == currentUser.CokId || currentUser.CokId == null
                       )
                        && a.SaldoId != null
                );
            int currentPeriod = Period.Per_now().Per_int;
            int PrevPeriod = Period.WithOffset().Per_int;

            return View();
        }
        /// <summary>
        /// Формування загального звіту по організаціях
        /// </summary>
        /// <param name="myDatepicker"></param>
        /// <param name="CokId"></param>
        /// <returns></returns>
        public async Task<ActionResult> Zvit(string myDatepicker, int CokId)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            string stanom_na;
            stanom_na = DateTime.Now.ToString();
            string userName = User.Identity.Name;
            string cokCode;
            Organization Cok;
            int CokPeriod;

            User currentUser = _context.Users
                .Include(u => u.Cok)
                .Include(u => u.Prava)
                .FirstOrDefault(u => u.Login == userName);
            string vykonavets = currentUser.FullName;
            List<ZvitVykl> zvits = new List<ZvitVykl>();
            //Перевірка на головного бухгалтера і друк загальної форми
            #region
            if (currentUser.CokId == null && currentUser.Prava.Name == "Бухгалтер" && CokId == 0)
            {
                DateTime per = DateTime.Now;
                int perInt = Period.Per_now().Per_int;
                int SelectPeriodInt = myDatepicker == null || myDatepicker.Trim() == ""
                    ? perInt
                    : int.Parse(myDatepicker.Remove(4, 1));

                IEnumerable<Saldo> Saldos = _context.Saldos
                    .Include(s => s.Person)
                    .ThenInclude(p => p.Cok)
                    .Include(s => s.Vykl)
                    .Where(s => s.AktPeriod == SelectPeriodInt)
                    .ToList();

                Dictionary<int, SelectedGrouping> Selected = new Dictionary<int, SelectedGrouping>();
                foreach (Saldo s in Saldos)
                {
                    if (!Selected.ContainsKey(s.Person.Cok.Id))
                    {
                        Selected[s.Person.Cok.Id] = new SelectedGrouping
                        {
                            CokId = s.Person.Cok.Id,
                            Cok = s.Person.Cok.Name,
                            DebStart = (decimal)0.0,
                            CredStart = (decimal)0.0,
                            Narah = (decimal)0.0,
                            Opl = (decimal)0.0,
                            DebEnd = (decimal)0.0,
                            CredEnd = (decimal)0.0
                        };
                    }
                    Selected[s.Person.Cok.Id].DebStart += s.DebPoch;
                    Selected[s.Person.Cok.Id].CredStart += s.KredPoch;
                    Selected[s.Person.Cok.Id].Narah += s.SumaVykl + s.SumaVkl;
                    Selected[s.Person.Cok.Id].Opl += s.Oplata;
                    Selected[s.Person.Cok.Id].DebEnd += s.DebKin;
                    Selected[s.Person.Cok.Id].CredEnd += s.KredKin;
                }
                //видаємо звіт користувачу
                Excel excel = new Excel(_context);
                excel.CreateZvitVyklAll(Selected, currentUser, stanom_na);
                byte[] contents = excel.CreateFile();
                return File(
                                contents,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "zvit.xlsx"
                            );
            }
            #endregion
            //Бухгалтер головний і друк районної форми
            #region
            else if (currentUser.Login == "buh" && CokId != 0)
            {
                Cok = _context.Coks.FirstOrDefault(c => c.Id == CokId);
                cokCode = Cok.Code;     //витягуємо код організації
                CokPeriod = Cok.CurrPeriod; //витягуємо активний період організації
                int SelectPeriodInt = myDatepicker == null || myDatepicker.Trim() == ""
                    ? CokPeriod
                    : int.Parse(myDatepicker.Remove(4, 1));

                IEnumerable<ActualDataPerson> people = await _context.ActualDatas
                    .Include(a => a.Person)
                        .ThenInclude(p => p.Lichylnyks)
                        .ThenInclude(l => l.Vykls)
                    .Include(p => p.Person.Address)
                    .Include(a => a.Finance)
                    .Include(a => a.Saldo)
                    .Include(a => a.Person.UpdateFinances)
                    .Where(a => a.Person.Cok.Code == cokCode && a.Saldo.AktPeriod == SelectPeriodInt)
                    .ToListAsync();
                int count = 1;
                foreach (ActualDataPerson item in people)
                {
                    ZvitVykl zvitVykl = new ZvitVykl
                    {
                        Id = count++,
                        OsRah = item.Person.NewOsRah,
                        FullName = item.Person.FullName,
                        FullAdres = item.Person.Address.CityTypeShort + " " + item.Person.Address.CityName + ", "
                                        + item.Person.Address.StreetTypeShortName + " " + item.Person.Address.StreetName,
                        DataVykl = item.Vykl.DateVykl.ToString(),
                        DataVkl = item.Vykl.DateVkl.ToString(),
                        DebPoch = item.Saldo.DebPoch,
                        DebKin = item.Saldo.DebKin,
                        KredPoch = item.Saldo.KredPoch,
                        KredKin = item.Saldo.KredKin,
                        Sumavykl = item.Saldo.SumaVykl,
                        SumaVkl = item.Saldo.SumaVkl,
                        Oplata = item.Saldo.Oplata,
                        Borg = item.Saldo.Narah,
                        OplataEE = item.Saldo.OplataZaEE,
                        BorgEE = item.Saldo.BorgZaEE
                    };

                    if (item.Person.Address.Building != null && item.Person.Address.Building != "")
                    {
                        zvitVykl.FullAdres = zvitVykl.FullAdres + ", буд." + item.Person.Address.Building;
                    }
                    if (item.Person.Address.BuildingPart != null && item.Person.Address.BuildingPart != "")
                    {
                        zvitVykl.FullAdres = zvitVykl.FullAdres + ", корп." + item.Person.Address.BuildingPart;
                    }
                    if (item.Person.Address.Apartment != null && item.Person.Address.Apartment != "")
                    {
                        zvitVykl.FullAdres = zvitVykl.FullAdres + ", кв." + item.Person.Address.Apartment;
                    }
                    zvits.Add(zvitVykl);
                }
                Excel excelController = new Excel(_context);
                excelController.CreateZvitReestr(zvits, currentUser, stanom_na);
                byte[] content = excelController.CreateFile();
                return File(
                    content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "zvit.xlsx"
                );
            }
            #endregion
            //для бухгалтера організації
            #region
            else
            {
                Cok = currentUser.Cok;
                cokCode = currentUser.Cok.Code;     //витягуємо код організації
                CokPeriod = currentUser.Cok.CurrPeriod; //витягуємо активний період організації

                int SelectPeriodInt = myDatepicker == null || myDatepicker.Trim() == ""
                    ? CokPeriod
                    : int.Parse(myDatepicker.Remove(4, 1));

                IEnumerable<ActualDataPerson> people = await _context.ActualDatas
                    .Include(a => a.Person)
                        .ThenInclude(p => p.Lichylnyks)
                        .ThenInclude(l => l.Vykls)
                    .Include(p => p.Person.Address)
                    .Include(a => a.Finance)
                    .Include(a => a.Saldo)
                    .Include(a => a.Person.UpdateFinances)
                    .Where(a => a.Person.Cok.Code == cokCode && a.Saldo.AktPeriod == SelectPeriodInt)
                    .OrderByDescending(v => v.Vykl.DateVykl)
                    .ToListAsync();

                int count = 1;
                foreach (ActualDataPerson item in people)
                {
                    ZvitVykl zvitVykl = new ZvitVykl
                    {
                        Id = count++,
                        OsRah = item.Person.NewOsRah,
                        FullName = item.Person.FullName,
                        FullAdres = item.Person.Address.CityTypeShort + " " + item.Person.Address.CityName + ", "
                                        + item.Person.Address.StreetTypeShortName + " " + item.Person.Address.StreetName,
                        DataVykl = item.Vykl.DateVykl.ToString(),
                        DataVkl = item.Vykl.DateVkl.ToString(),
                        DebPoch = item.Saldo.DebPoch,
                        DebKin = item.Saldo.DebKin,
                        KredPoch = item.Saldo.KredPoch,
                        KredKin = item.Saldo.KredKin,
                        Sumavykl = item.Saldo.SumaVykl,
                        SumaVkl = item.Saldo.SumaVkl,
                        Oplata = item.Saldo.Oplata,
                        Borg = item.Saldo.Narah,
                        OplataEE = item.Saldo.OplataZaEE,
                        BorgEE = item.Saldo.BorgZaEE
                    };

                    if (item.Person.Address.Building != null && item.Person.Address.Building != "")
                    {
                        zvitVykl.FullAdres = zvitVykl.FullAdres + ", буд." + item.Person.Address.Building;
                    }
                    if (item.Person.Address.BuildingPart != null && item.Person.Address.BuildingPart != "")
                    {
                        zvitVykl.FullAdres = zvitVykl.FullAdres + ", корп." + item.Person.Address.BuildingPart;
                    }
                    if (item.Person.Address.Apartment != null && item.Person.Address.Apartment != "")
                    {
                        zvitVykl.FullAdres = zvitVykl.FullAdres + ", кв." + item.Person.Address.Apartment;
                    }
                    zvits.Add(zvitVykl);
                }

                Excel excelController = new Excel(_context);
                excelController.CreateZvitReestr(zvits, currentUser, stanom_na);
                byte[] content = excelController.CreateFile();
                return File(
                    content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "zvit.xlsx"
                );
            }
            #endregion
        }
        /// <summary>
        /// Формування рахунку
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Rah(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string filePath = "\\files\\";
            string generatedFile = "rah.docx";
            string fileName = "shablon_rah.docx";
            string fullPath = appEnvir.WebRootPath + filePath + fileName;
            string fullGenerated = appEnvir.WebRootPath + filePath + generatedFile;
            DateTime now = DateTime.Now;
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            string vykonavets = currentUser.FullName;

            ActualDataPerson ab = await _context.ActualDatas
                .Include(ad => ad.Person)
                .ThenInclude(p => p.Saldos)
                .Include(ad => ad.Saldo)
                .FirstOrDefaultAsync(ad => ad.Id == id);

            Saldo VyklSaldo = ab.Person
                .Saldos.Where(s => s.VyklId == ab.VyklId && s.SumaVkl != 0)
                .OrderBy(s => s.AktPeriod).FirstOrDefault();

            decimal deb_kin = (decimal)ab.Saldo.DebKin;
            decimal cina_bez_pdv = 0;
            decimal pdv = 0;
            decimal vsogo = 0;
            string sumaStr = "";

            if (VyklSaldo == null)
            {
                cina_bez_pdv = deb_kin - (deb_kin / 6);
                pdv = deb_kin / 6;
                vsogo = cina_bez_pdv + pdv;
                sumaStr = MoneyToStr.GrnPhrase(cina_bez_pdv + pdv);
            }
            else
            {
                if ((decimal)VyklSaldo.SumaVkl != 0 || (decimal)VyklSaldo.SumaVykl != 0)
                {
                    decimal cinaVkl = (decimal)VyklSaldo.SumaVkl;
                    decimal cinaVykl = (decimal)VyklSaldo.SumaVykl;
                    cina_bez_pdv = cinaVkl + cinaVykl - ((cinaVkl + cinaVykl) / 6);
                    pdv = (cinaVkl + cinaVykl) / 6;
                    vsogo = cina_bez_pdv + pdv;
                    sumaStr = MoneyToStr.GrnPhrase(cina_bez_pdv + pdv);
                }
            }

            RahEndAkt abon = new RahEndAkt
            {
                Data = now.ToString("D"),
                OsRah = ab.Person.NewOsRah,
                FullName = ab.Person.FullName.Trim(),
                FullAddress = ab.Person.FullAddress,
                CinaBezPdv = cina_bez_pdv.ToString("N"),
                SumaBezPdv = cina_bez_pdv.ToString("N"),
                PDV = pdv.ToString("N"),
                Vsogo = vsogo.ToString("N"),
                SumaStr = sumaStr,
                Vykon = vykonavets
            };

            DocumentBase document = DocumentFactory.Create(fullPath, abon);
            document.Generate(fullGenerated);

            string NewFileName = "Рахунок_" + abon.FullName + ".docx";
            return File(
                System.IO.File.ReadAllBytes(fullGenerated),
                System.Net.Mime.MediaTypeNames.Application.Octet,
                NewFileName
            );
        }
        /// <summary>
        /// Формування акту
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Akt(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string filePath = "\\files\\";
            string generatedFile = "akt.docx";
            string fileName = "shablon_Akt.docx";
            string fullPath = appEnvir.WebRootPath + filePath + fileName;
            string fullGenerated = appEnvir.WebRootPath + filePath + generatedFile;
            DateTime now = DateTime.Now;

            ActualDataPerson ab = await _context.ActualDatas
                .Include(ad => ad.Person)
                .ThenInclude(p => p.Saldos)
                .Include(ad => ad.Saldo)
                .FirstOrDefaultAsync(ad => ad.Id == id);

            Saldo VyklSaldo = ab.Person
                .Saldos.Where(s => s.VyklId == ab.VyklId && s.SumaVkl != 0)
                .OrderBy(s => s.AktPeriod).FirstOrDefault();

            decimal deb_kin = (decimal)ab.Saldo.DebKin;
            decimal cina_bez_pdv = 0;
            decimal pdv = 0;
            decimal vsogo = 0;
            string sumaStr = "";

            if (VyklSaldo == null)
            {
                cina_bez_pdv = deb_kin - (deb_kin / 6);
                pdv = deb_kin / 6;
                vsogo = cina_bez_pdv + pdv;
                sumaStr = MoneyToStr.GrnPhrase(cina_bez_pdv + pdv);
            }
            else
            {
                if ((decimal)VyklSaldo.SumaVkl != 0 || (decimal)VyklSaldo.SumaVykl != 0)
                {
                    decimal cinaVkl = (decimal)VyklSaldo.SumaVkl;
                    decimal cinaVykl = (decimal)VyklSaldo.SumaVykl;
                    cina_bez_pdv = cinaVkl + cinaVykl - ((cinaVkl + cinaVykl) / 6);
                    pdv = (cinaVkl + cinaVykl) / 6;
                    vsogo = cina_bez_pdv + pdv;
                    sumaStr = MoneyToStr.GrnPhrase(cina_bez_pdv + pdv);
                }
            }

            RahEndAkt abon = new RahEndAkt
            {
                Data = now.ToString("D"),
                OsRah = ab.Person.OsRah,
                FullName = ab.Person.FullName.Trim(),
                CinaBezPdv = cina_bez_pdv.ToString("N"),
                SumaBezPdv = cina_bez_pdv.ToString("N"),
                PDV = pdv.ToString("N"),
                Vsogo = vsogo.ToString("N"),
                SumaStr = sumaStr,
            };

            DocumentBase document = DocumentFactory.Create(fullPath, abon);
            document.Generate(fullGenerated);

            string NewFileName = "Акт_" + abon.FullName + ".docx";
            return File(
                System.IO.File.ReadAllBytes(fullGenerated),
                System.Net.Mime.MediaTypeNames.Application.Octet,
                NewFileName
            );
        }

        private bool ActualDataPersonExists(int id)
        {
            return _context.ActualDatas.Any(e => e.Id == id);
        }
    }
}
