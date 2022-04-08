using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RegistrDisconnection.Data;
using RegistrDisconnection.Models.Dictionaries;
using RegistrDisconnection.Models.Users;
using RegistrDisconnection.MyClasses;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistrDisconnection.Controllers
{
    /// <summary>
    /// Напрямки відключення
    /// </summary>
    public class DirectionDictsController : Controller
    {
        private readonly MainContext _context;
        private readonly MainContext db;
        private readonly IWebHostEnvironment appEnvir;

        public DirectionDictsController(MainContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            appEnvir = appEnvironment;
            db = context;
        }

        // GET: DirectionDicts
        public async Task<IActionResult> Index()
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            List<DirectionDict> directions = await _context.DirectionDicts
                .Include(d => d.Cok)
                .Where(d => (currentUser.Cok == null || d.CokId == currentUser.CokId) && d.ParentDirectionId == null)
                .ToListAsync();
            return View(directions);
        }

        // GET: вивід списку напрямків
        public async Task<IActionResult> Details(int? id)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (id == null)
            {
                return NotFound();
            }

            DirectionDict directionDict = await _context.DirectionDicts
                .Include(d => d.Cok)
                .Include(d => d.DirectionCityMaps)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (directionDict == null)
            {
                return NotFound();
            }
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            string cokCode = currentUser.Cok.Code;

            int RegionId = _context.Coks.FirstOrDefault(c => c.Code == cokCode).RegionId;
            if (cokCode == "TR40")
            {
                return RedirectToAction("Index");
            }
            DataTable cityMap = BillingUtils.GetCity(appEnvir, RegionId);
            List<CitySelect> SelectionList = new List<CitySelect>();

            foreach (DataRow row in cityMap.Rows)
            {
                DirectionCityMap existedForDirection = _context.DirectionCityMaps.FirstOrDefault(dcm => dcm.UtilityCityId == row.Field<short>("CityId"));
                if (existedForDirection == null)
                {
                    SelectionList.Add(new CitySelect { Id = row.Field<short>("CityId"), Name = row.Field<string>("Name") });
                }
            }
            ViewData["City"] = new SelectList(SelectionList, "Id", "Name");

            return View(directionDict);
        }

        // добавлення в напрямки населені пункти
        [HttpPost]
        public IActionResult Details(IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                DataTable ScriptRes = BillingUtils.GetCityById(appEnvir, int.Parse(form["city"]));
                DirectionCityMap dcm = new DirectionCityMap
                {
                    DirectionDictId = int.Parse(form["Id"]),
                    UtilityCityId = int.Parse(form["city"]),
                    Name = ScriptRes.Rows[0]["Name"].ToString()
                };

                DirectionCityMap dirD = db.DirectionCityMaps.FirstOrDefault(d => d.UtilityCityId == int.Parse(form["city"]));
                if (dirD == null)
                {
                    _ = _context.DirectionCityMaps.Add(dcm);
                    _ = _context.SaveChanges();
                }
                else
                {
                    ViewBag.error = "BadCity";
                    return RedirectToAction("Details");
                }
            }
            return RedirectToAction("Details");
        }

        // POST: DirectionDicts/Delete/5
        public IActionResult DeleteCity(int id)
        {
            DirectionCityMap directionDictCity = _context.DirectionCityMaps
                .Include(d => d.Direction)
                .FirstOrDefault(dd => dd.Id == id);
            if (directionDictCity != null)
            {
                _ = _context.DirectionCityMaps.Remove(directionDictCity);
                _ = _context.SaveChanges();
                return RedirectToAction("Details", new { id = directionDictCity.Direction.Id });
            }
            return RedirectToAction("Index");
        }

        // GET: DirectionDicts/Create
        public IActionResult Create()
        {
            ViewData["CokId"] = new SelectList(_context.Coks, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CokId")] DirectionDict directionDict)
        {
            if (ModelState.IsValid)
            {
                _ = _context.Add(directionDict);
                _ = await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CokId"] = new SelectList(_context.Coks, "Id", "Id", directionDict.CokId);
            return View(directionDict);
        }

        // GET: DirectionDicts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DirectionDict directionDict = await _context.DirectionDicts.FindAsync(id);
            if (directionDict == null)
            {
                return NotFound();
            }
            ViewData["CokId"] = new SelectList(_context.Coks, "Id", "Code", directionDict.CokId);
            return View(directionDict);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Number,CokId")] DirectionDict directionDict)
        {
            if (id != directionDict.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _ = _context.Update(directionDict);
                    _ = await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DirectionDictExists(directionDict.Id))
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
            ViewData["CokId"] = new SelectList(_context.Coks, "Id", "Id", directionDict.CokId);
            return View(directionDict);
        }

        // GET: DirectionDicts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DirectionDict directionDict = await _context.DirectionDicts
                .Include(d => d.Cok)
                .FirstOrDefaultAsync(m => m.Id == id);

            return directionDict == null ? NotFound() : (IActionResult)View(directionDict);
        }

        // POST: DirectionDicts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            DirectionDict directionDict = await _context.DirectionDicts.FindAsync(id);
            _ = _context.DirectionDicts.Remove(directionDict);
            _ = await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DirectionDictExists(int id)
        {
            return _context.DirectionDicts.Any(e => e.Id == id);
        }
    }
}
