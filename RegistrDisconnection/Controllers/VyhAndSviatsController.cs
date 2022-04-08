using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistrDisconnection.Data;
using RegistrDisconnection.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using RegistrDisconnection.Models.Users;

namespace RegistrDisconnection.Controllers
{
    /// <summary>
    /// Добавлення вихідних і святкових днів
    /// </summary>
    [Authorize(Roles = "Адміністратор")]
    public class VyhAndSviatsController : Controller
    {
        private readonly MainContext _context;

        public VyhAndSviatsController(MainContext context)
        {
            _context = context;
        }

        // GET: VyhAndSviats
        public async Task<IActionResult> Index()
        {
            return View(await _context.VyhAndSviats.Where(s => s.Year == DateTime.Today.Year || s.Year == null).ToListAsync());
        }

        // POST: VyhAndSviats
        [HttpPost]
        public async Task<IActionResult> Index(IFormCollection form)
        {
            List<VyhAndSviat> holidays = await _context.VyhAndSviats.ToListAsync();
            bool isChecked = form.TryGetValue("ShowAll", out Microsoft.Extensions.Primitives.StringValues checkValue);
            if (!isChecked)
            {
                holidays = await _context.VyhAndSviats.Where(s => s.Year == DateTime.Today.Year || s.Year == null).ToListAsync();
                ViewData["checked"] = null;
            }
            else
            {
                ViewData["checked"] = checkValue;
            }
            return View(holidays);
        }

        // GET: VyhAndSviats/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            VyhAndSviat vyhAndSviat = await _context.VyhAndSviats
                .FirstOrDefaultAsync(m => m.Id == id);
            return vyhAndSviat == null ? NotFound() : (IActionResult)View(vyhAndSviat);
        }

        // GET: VyhAndSviats/Create
        public IActionResult Create()
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            if (currentUser.Login == "admin")
            {
                return View();
            }
            ViewBag.error = "NoRight";
            return RedirectToAction(nameof(Index));
        }

        // POST: VyhAndSviats/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,Year,Month,Day,Name")] VyhAndSviat vyhAndSviat)
        {
            if (ModelState.IsValid)
            {
                _ = _context.Add(vyhAndSviat);
                _ = await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vyhAndSviat);
        }

        // GET: VyhAndSviats/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            VyhAndSviat vyhAndSviat = await _context.VyhAndSviats.FindAsync(id);
            return vyhAndSviat == null ? NotFound() : (IActionResult)View(vyhAndSviat);
        }

        // POST: VyhAndSviats/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Year,Month,Day,Name")] VyhAndSviat vyhAndSviat)
        {
            if (id != vyhAndSviat.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _ = _context.Update(vyhAndSviat);
                    _ = await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VyhAndSviatExists(vyhAndSviat.Id))
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
            return View(vyhAndSviat);
        }

        // GET: VyhAndSviats/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            VyhAndSviat vyhAndSviat = await _context.VyhAndSviats
                .FirstOrDefaultAsync(m => m.Id == id);
            return vyhAndSviat == null ? NotFound() : (IActionResult)View(vyhAndSviat);
        }

        // POST: VyhAndSviats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            VyhAndSviat vyhAndSviat = await _context.VyhAndSviats.FindAsync(id);
            _ = _context.VyhAndSviats.Remove(vyhAndSviat);
            _ = await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VyhAndSviatExists(int id)
        {
            return _context.VyhAndSviats.Any(e => e.Id == id);
        }
    }
}
