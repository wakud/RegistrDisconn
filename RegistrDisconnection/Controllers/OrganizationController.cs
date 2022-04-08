using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistrDisconnection.Data;
using RegistrDisconnection.Models.Dictionaries;
using RegistrDisconnection.Models.Users;
using System.Linq;
using System.Threading.Tasks;

namespace RegistrDisconnection.Controllers
{
    /// <summary>
    /// Сторінка організації
    /// </summary>
    public class OrganizationController : Controller
    {
        private readonly MainContext _context;

        public OrganizationController(MainContext context)
        {
            _context = context;
        }

        // GET: 
        public async Task<IActionResult> Index()
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            IQueryable<Organization> mainContext = _context.Coks.Where(m => currentUser.Cok == null || m.Id == currentUser.CokId);
            return View(await mainContext.ToListAsync());
        }

        // GET: HomeController1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Organization cok = await _context.Coks.FirstOrDefaultAsync(c => c.Id == id);
            return cok == null ? NotFound() : (IActionResult)View(cok);
        }

        // GET: HomeController1/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: HomeController1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Code,Name,NameRod,Nach,Buh,Index,RegionId,Oblast,Rajon,City,Street,Address,Tel," +
                    "EDRPOU,MFO,RozRah,NameREM, OrganizationUnitGUID, DbConfigName, IPN")]
            Organization cok)
        {
            if (ModelState.IsValid)
            {
                _ = _context.Add(cok);
                _ = await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cok);
        }

        // GET: HomeController1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Organization cok = await _context.Coks.FindAsync(id);
            return cok == null ? NotFound() : (IActionResult)View(cok);
        }

        // POST: HomeController1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Code,Name,NameRod,Nach,Buh,Index,Oblast,Rajon,City,Street,Address,Tel," +
                    "EDRPOU,MFO,RozRah,NameREM, OrganizationUnitGUID, DbConfigName, IPN, CurrPeriod, RegionId")]
            Organization cok)
        {
            if (id != cok.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _ = _context.Update(cok);
                    _ = await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizationExist(cok.Id))
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
            return View(cok);
        }

        // GET: HomeController1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Organization cok = await _context.Coks.FirstOrDefaultAsync(cok => cok.Id == id);
            return cok == null ? NotFound() : (IActionResult)View(cok);
        }

        // POST: HomeController1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Organization cok = await _context.Coks.FindAsync(id);
            _ = _context.Coks.Remove(cok);
            _ = await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrganizationExist(int id)
        {
            return _context.Coks.Any(c => c.Id == id);
        }
    }
}
