using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistrDisconnection.Data;
using RegistrDisconnection.Models.Abonents;
using RegistrDisconnection.Models.Operations;
using RegistrDisconnection.Models.Users;
using RegistrDisconnection.ViewModels;
using SharpDocx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistrDisconnection.Controllers
{
    /// <summary>
    /// Формування вимоги для ОСР
    /// </summary>
    public class VymogyController : Controller
    {
        private readonly MainContext _context;
        private readonly IWebHostEnvironment appEnvir;

        public VymogyController(MainContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            appEnvir = appEnvironment;
        }

        // GET: VymogyController
        public ActionResult Index(int id)
        {
            return id != 0
                ? View(_context.DrukGroups
                            .Where(dg => dg.DirectionDictId == id)
                            .OrderByDescending(dg => dg.VydanePoper)
                            .ToList()
                            )
                : View(_context.DrukGroups
                        .Where(dg => dg.Id != 1)
                        .OrderBy(dg => dg.DirectionName)
                        .ThenByDescending(dg => dg.VydanePoper)
                        .ToList()
                        );
        }

        // GET: VymogyController/Details/5
        public ActionResult PoperVymoga(int id)
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            string vykonavets = currentUser.FullName;
            string cokCode = currentUser.CokId == null ? "ORG" : currentUser.Cok.Code;
            string filePath = "\\files\\";
            string generatedPath = "vymoga_.docx";
            string fileName = "vymoga.docx";
            string fullPath = appEnvir.WebRootPath + filePath + fileName;
            string fullGenerated = appEnvir.WebRootPath + filePath + generatedPath;

            IQueryable<ActualDataPerson> people = _context.ActualDatas
                .Include(ad => ad.Person)
                .ThenInclude(p => p.Address)
                .ThenInclude(a => a.Direction)
                .Include(ad => ad.Finance)
                .Include(ad => ad.Poperedgenia)
                .Include(ad => ad.Lichylnyk)
                .Include(ad => ad.Vykl)
                .Where(ad => ad.Person.Cok.Code == cokCode && ad.Person.StatusAktyv);

            FilterNapr viewModel = new FilterNapr
            {
                People = people.Where(p => p.Poperedgenia.PoperDrukGroupId == id).ToList()
                        .OrderBy(p => p.Person.FullAddress)     //робимо сортування по адресі FullAdress
            };

            List<PrintPoper> cfp = new List<PrintPoper>();
            foreach (ActualDataPerson item in viewModel.People)
            {
                PrintPoper convForPrint = MyClasses.Utils.CreatePrintPoperFromActual(item, vykonavets);
                cfp.Add(convForPrint);
            }

            string NameDoc = "";
            if (currentUser.Cok.Code == "ORG" || currentUser.Cok.Code == "ORG")
            {
                NameDoc = currentUser.Cok.Name.Remove(12) + "ого ЦОКу";
            }
            else
            {
                NameDoc = currentUser.Cok.Name.Trim();
                NameDoc = NameDoc.Remove(NameDoc.Length - 6) + "ого ЦОКу";
            }

            PrintVymogy vymogy = new PrintVymogy
            {
                PrintPopers = cfp,
                Nach = currentUser.Cok.Nach,
                Vykonavets = currentUser.FullName,
                REM = currentUser.Cok.NameREM,
                Cok = NameDoc
            };

            DocumentBase document = DocumentFactory.Create(fullPath, vymogy);
            document.Generate(fullGenerated);

            string NewFileName = "vymoga_" + DateTime.Now.ToString() + ".docx";
            return File(
                System.IO.File.ReadAllBytes(fullGenerated),
                System.Net.Mime.MediaTypeNames.Application.Octet,
                NewFileName
            );
        }

        // GET: VymogyController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: VymogyController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: VymogyController/Edit/5
        public ActionResult Edit()
        {
            return View();
        }

        // POST: VymogyController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: VymogyController/Delete/5
        public ActionResult Delete()
        {
            return View();
        }

        // POST: VymogyController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
