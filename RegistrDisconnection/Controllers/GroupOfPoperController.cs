using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistrDisconnection.Data;
using RegistrDisconnection.Models.Abonents;
using RegistrDisconnection.Models.Operations;
using RegistrDisconnection.Models.Users;
using RegistrDisconnection.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistrDisconnection.Controllers
{
    /// <summary>
    /// Пачки попереджень згідно напрямків
    /// </summary>
    public class GroupOfPoperController : Controller
    {
        private readonly MainContext _context;

        public GroupOfPoperController(MainContext context)
        {
            _context = context;
        }

        // GET: GroupOfPoper
        public async Task<IActionResult> Index()
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);

            List<PoperDrukGroup> poperDrukGroup = await _context.DrukGroups
                        .Include(dg => dg.DirectionDict)
                        .Where(dg => currentUser.Cok == null || (dg.Id != 1 && dg.DirectionDict.CokId == currentUser.CokId))
                        .OrderBy(dg => dg.DirectionName)
                        .ThenByDescending(dg => dg.VydanePoper)
                        .ToListAsync();

            return View(poperDrukGroup);
        }

        // GET: GroupOfPoper/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            string cokCode = currentUser.CokId == null ? "ORG" : currentUser.Cok.Code;

            IQueryable<ActualDataPerson> people = _context.ActualDatas
                .Include(ad => ad.Person)
                .ThenInclude(p => p.Address)
                .ThenInclude(a => a.Direction)
                .Include(ad => ad.Finance)
                .Include(ad => ad.Poperedgenia)
                .Include(ad => ad.Lichylnyk)
                .Include(ad => ad.Vykl)
                .Where(ad => ad.Person.Cok.Code == cokCode && ad.Person.StatusAktyv);

            if (id == null)
            {
                return NotFound();
            }

            PoperDrukGroup poperDrukGroup = await _context.DrukGroups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (poperDrukGroup == null)
            {
                return NotFound();
            }

            people = people.Where(p => p.Poperedgenia.PoperDrukGroupId == poperDrukGroup.Id);
            FilterNapr viewModel = new FilterNapr
            {
                People = people.ToList(),
            };

            return View(viewModel);
        }

        // GET: GroupOfPoper/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PoperDrukGroup poperDrukGroup = await _context.DrukGroups
                .FirstOrDefaultAsync(m => m.Id == id);

            return poperDrukGroup == null ? NotFound() : (IActionResult)View(poperDrukGroup);
        }

        // POST: GroupOfPoper/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int Id)
        {
            if (Id == 1)
            {
                return RedirectToAction(nameof(Index));
            }

            IEnumerable<ActualDataPerson> people = await _context
                .ActualDatas
                .Include(a => a.Poperedgenia)
                .Where(a => a.Poperedgenia.PoperDrukGroupId == Id)
                .ToListAsync();
            foreach (ActualDataPerson item in people)
            {
                Poperedgenia poper = item.Poperedgenia;
                poper.PoperDrukGroupId = 1;
                poper.Napramok = 0;
                poper.VydanePoper = false;
                poper.Poper = null;
                poper.DateVykl = null;
                poper.StanNa = null;
                _ = await _context.SaveChangesAsync();
            }

            PoperDrukGroup poperDrukGroup = await _context.DrukGroups.FindAsync(Id);
            _ = _context.DrukGroups.Remove(poperDrukGroup);
            _ = await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: GroupOfPoper/DeleteOne/5
        public async Task<IActionResult> DeleteOne(int? id, [FromQuery(Name = "returnToController")] string retCtr,
            [FromQuery(Name = "returnToAction")] string retAct, int? direction)
        {
            if (id == null)
            {
                return NotFound();
            }

            ActualDataPerson poperDrukOne = await _context.ActualDatas
                .Include(p => p.Person)
                .Include(p => p.Finance)
                .Include(p => p.Poperedgenia)
                .ThenInclude(p => p.PoperDruk)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poperDrukOne == null)
            {
                return NotFound();
            }

            FilterViewModel viewModel = new FilterViewModel
            {
                DataPerson = poperDrukOne,
                Direction = direction
            };

            ViewData["retCtr"] = retCtr;
            ViewData["retAct"] = retAct;
            return View(viewModel);
        }

        // POST: GroupOfPoper/Delete/5
        [HttpPost, ActionName("DeleteOne")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOneConfirmed(int Id, [FromQuery(Name = "returnToController")] string retCtr,
            [FromQuery(Name = "returnToAction")] string retAct, int? direction)
        {
            ActualDataPerson poperDrukOne = await _context.ActualDatas
                .Include(p => p.Person)
                .Include(p => p.Poperedgenia)
                .ThenInclude(p => p.PoperDruk)
                .FirstOrDefaultAsync(p => p.Id == Id);

            Poperedgenia poper = poperDrukOne.Poperedgenia;
            poper.PoperDrukGroupId = 1;
            poper.Napramok = 0;
            poper.VydanePoper = false;
            //poper.Poper = null;
            //poper.DateVykl = null;
            poper.StanNa = null;
            poper.Person.StatusAktyv = false;

            PoperDrukGroup drukGroup = poperDrukOne.Poperedgenia.PoperDruk;
            drukGroup.CountAbon -= 1;
            _ = await _context.SaveChangesAsync();

            retCtr = "Poper";
            retAct = "Napr";
            return retCtr == null || retAct == null
                ? RedirectToAction(nameof(Index))
                : RedirectToAction(retAct, retCtr, new { @_direc = direction });
        }

        private bool PoperDrukGroupExists(int id)
        {
            return _context.DrukGroups.Any(e => e.Id == id);
        }
    }
}
