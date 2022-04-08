using DotNetDBF;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistrDisconnection.Data;
using RegistrDisconnection.Models.Abonents;
using RegistrDisconnection.Models.Operations;
using RegistrDisconnection.Models.Users;
using RegistrDisconnection.MyClasses;
using RegistrDisconnection.ViewModels;
using SharpDocx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RegistrDisconnection.Controllers
{
    /// <summary>
    /// Формування інформації для укрпошти
    /// </summary>
    public class UkrPoshta : Controller
    {
        private readonly MainContext _context;
        private readonly IWebHostEnvironment appEnvir;

        public UkrPoshta(MainContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            appEnvir = appEnvironment;
        }

        // GET: UkrPoshta
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

        /// <summary>
        /// робимо друк конвертів у ворд
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Converty(int id)
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            string vykonavets = currentUser.FullName;
            string cokCode = currentUser.CokId == null ? "ORG" : currentUser.Cok.Code;

            string filePath = "\\files\\";
            string generatedPath = "converty.docx";
            string fileName = "conv.docx";
            string fullPath = appEnvir.WebRootPath + filePath + fileName;
            string fullGenerated = appEnvir.WebRootPath + filePath + generatedPath;

            //джойнимо таблиці і вибираємо тільки активні по організації
            IQueryable<ActualDataPerson> people = _context.ActualDatas
                .Include(ad => ad.Person)
                .ThenInclude(p => p.Address)
                .ThenInclude(a => a.Direction)
                .Include(a => a.Person.Cok)
                .Include(ad => ad.Poperedgenia)
                .Where(ad => ad.Person.Cok.Code == cokCode && ad.Person.StatusAktyv);

            FilterNapr viewModel = new FilterNapr
            {
                People = people.Where(p => p.Poperedgenia.PoperDrukGroupId == id)
                               .OrderBy(p => p.Person.FullAddress)
                               .ToList(),
            };

            //створюємо список абонентів для друку і заповнюємо його
            List<PrintPoper> cfp = new List<PrintPoper>();
            foreach (ActualDataPerson item in viewModel.People)
            {
                PrintPoper convForPrint = MyClasses.Utils.CreatePrintPoperFromActual(item, vykonavets);
                cfp.Add(convForPrint);
            }

            DocumentBase convert = DocumentFactory.Create(fullPath, cfp);
            convert.Generate(fullGenerated);

            //видаємо файл користувачу
            string NewFileName = "converty_" + DateTime.Now.ToString() + ".docx";
            return File(
                System.IO.File.ReadAllBytes(fullGenerated),
                System.Net.Mime.MediaTypeNames.Application.Octet,
                NewFileName
            );
        }

        /// <summary>
        /// робимо формування ДБФ файлу для пошти
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DBF(int id)
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            string cokCode;
            string vykonavets = currentUser.FullName;
            cokCode = currentUser.CokId == null ? "ORG" : currentUser.Cok.Code;

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
                People = people.Where(p => p.Poperedgenia.PoperDrukGroupId == id)
                               .OrderBy(p => p.Person.FullAddress)
                               .ToList(),
            };
            string filePath = "\\files\\";
            string fileName = "42145798_10_";
            string fullPath = appEnvir.WebRootPath + filePath + fileName;

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            List<PrintPoper> personForDbf = new List<PrintPoper>();
            foreach (ActualDataPerson item in viewModel.People)
            {
                PrintPoper popDbf = MyClasses.Utils.CreatePrintPoperFromActual(item, vykonavets);
                personForDbf.Add(popDbf);
            }

            using (Stream fos = System.IO.File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using DBFWriter writer = new DBFWriter
                {
                    CharEncoding = Encoding.GetEncoding("windows-1251"),
                    Signature = DBFSignature.DBase3
                };
                DBFField field1 = new DBFField("psgvno", NativeDbType.Numeric, 9);
                DBFField field2 = new DBFField("psbarc", NativeDbType.Char, 13);
                DBFField field3 = new DBFField("rccn3c", NativeDbType.Numeric, 9);
                DBFField field4 = new DBFField("rcpidx", NativeDbType.Char, 5);
                DBFField field5 = new DBFField("rcaddr", NativeDbType.Char, 80);
                DBFField field6 = new DBFField("rcname", NativeDbType.Char, 40);
                DBFField field7 = new DBFField("snmtdc", NativeDbType.Numeric, 9);
                DBFField field8 = new DBFField("psappc", NativeDbType.Numeric, 9);
                DBFField field9 = new DBFField("pscatc", NativeDbType.Numeric, 9);
                DBFField field10 = new DBFField("psrazc", NativeDbType.Numeric, 9);
                DBFField field11 = new DBFField("psnotc", NativeDbType.Numeric, 9);
                DBFField field12 = new DBFField("pswgt", NativeDbType.Numeric, 9);
                DBFField field13 = new DBFField("pkprice", NativeDbType.Numeric, 10, 2);
                DBFField field14 = new DBFField("aftpay", NativeDbType.Numeric, 10, 2);
                DBFField field15 = new DBFField("phone", NativeDbType.Char, 15);

                writer.Fields = new[] { field1, field2, field3, field4, field5, field6, field7, field8, field9, field10,
                                    field11, field12, field13, field14, field15 };

                int psgvno = 1;
                string psbarc = "";
                int rccn3c = 804;
                int snmtdc = 1;
                int psappc = 2;
                int pscatc = 2;
                int psrazc = 1;
                int psnotc = 1;
                int pswgt = 40;
                int pkprice = 0;
                int aftpay = 0;
                string phone = "";
                foreach (PrintPoper dibifi in personForDbf)
                {
                    string rcpidx = dibifi.PostalCode.ToString();
                    string rcaddr = dibifi.FullAddress.ToString();
                    string rcname = dibifi.FullName;

                    writer.AddRecord(psgvno, psbarc, rccn3c, rcpidx, rcaddr, rcname, snmtdc, psappc,
                                        pscatc, psrazc, psnotc, pswgt, pkprice, aftpay, phone);
                }
                writer.Write(fos);
            }
            string fileNameNew = fileName + DateTime.Now.ToString() + ".dbf";
            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileNameNew);
        }

        /// <summary>
        /// Формуємо супровідний файл до дбф
        /// </summary>
        /// <param name="id"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public ActionResult SuprDBF(int id, string price)
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            string vykonavets = currentUser.FullName;
            string cokCode = currentUser.CokId == null ? "ORG" : currentUser.Cok.Code;
            string filePath = "\\files\\";
            string generatedPath = "suprovidna_DBF.docx";
            string fileName = "suprovidDBF.docx";
            string fullPath = appEnvir.WebRootPath + filePath + fileName;
            string fullGenerated = appEnvir.WebRootPath + filePath + generatedPath;

            //джойнимо всі таблиці
            IQueryable<ActualDataPerson> people = _context.ActualDatas
                .Include(ad => ad.Person)
                .ThenInclude(p => p.Address)
                .ThenInclude(a => a.Direction)
                .Include(a => a.Person.Cok)
                .Include(ad => ad.Poperedgenia)
                .Where(ad => ad.Person.Cok.Code == cokCode && ad.Person.StatusAktyv);

            FilterNapr viewModel = new FilterNapr
            {
                People = people.Where(p => p.Poperedgenia.PoperDrukGroupId == id)
                               .OrderBy(p => p.Person.FullAddress)
                               .ToList(),
            };

            int kt = viewModel.People.Count();
            decimal suma = kt * decimal.Parse(price);
            string sumaStr = MoneyToStr.GrnPhrase(suma);    //сума прописом

            List<PrintPoper> cfp = new List<PrintPoper>();
            foreach (ActualDataPerson item in viewModel.People)
            {
                PrintPoper convForPrint = MyClasses.Utils.CreatePrintPoperFromActual(item, vykonavets);
                cfp.Add(convForPrint);
            }

            PrintPoperPostal context = new PrintPoperPostal
            {
                PrintPopers = cfp,
                SumDecimal = suma,
                SumText = sumaStr,
                Postal = currentUser.Cok.Index,
                Price = price,
                Nach = currentUser.Cok.Nach,
                Buh = currentUser.Cok.Buh
            };

            DocumentBase suprovid = DocumentFactory.Create(fullPath, context);
            suprovid.Generate(fullGenerated);
            string NewFileName = "suprovidna_" + DateTime.Now.ToString() + ".docx";
            return File(
                System.IO.File.ReadAllBytes(fullGenerated),
                System.Net.Mime.MediaTypeNames.Application.Octet,
                NewFileName
            );
        }
    }
}
