using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RegistrDisconnection.Data;
using RegistrDisconnection.MyClasses;
using RegistrDisconnection.Models.Users;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace RegistrDisconnection.Controllers
{
    /// <summary>
    /// Адміністрування користувачів
    /// </summary>
    [Authorize(Roles = "Адміністратор")]
    public class UsersController : Controller
    {
        private readonly MainContext _context;

        public UsersController(MainContext context)
        {
            _context = context;
        }

        /// <summary>
        /// інформація про користувача
        /// </summary>
        /// <returns></returns>
        public PartialViewResult GetUserInfo()
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            return PartialView("UserInfo.cshtml", currentUser);
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            string userName = User.Identity.Name;
            User currentUser = _context.Users.Include(u => u.Cok).FirstOrDefault(u => u.Login == userName);
            IQueryable<User> mainContext = _context.Users
                .Include(u => u.Cok)
                .Include(u => u.Prava)
                .Where(u => currentUser.Cok == null || u.CokId == currentUser.CokId);

            return View(await mainContext.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User user = await _context.Users
                .Include(u => u.Cok)
                .Include(u => u.Prava)
                .FirstOrDefaultAsync(m => m.Id == id);
            return user == null ? NotFound() : (IActionResult)View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["CokId"] = new SelectList(_context.Coks, "Id", "Name");
            ViewData["PravaId"] = new SelectList(_context.Rights, "Id", "Name");
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,Login,Password,CokId,PravaId")] User user)
        {
            if (ModelState.IsValid)
            {
                user.Password = Utils.Encrypt(user.Password);
                _ = _context.Add(user);
                _ = await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CokId"] = new SelectList(_context.Coks, "Id", "Id", user.CokId);
            ViewData["PravaId"] = new SelectList(_context.Rights, "Id", "Id", user.PravaId);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            string decryptedData = Utils.Decrypt(user.Password);
            ViewData["CokId"] = new SelectList(_context.Coks, "Id", "Name", user.CokId);
            ViewData["PravaId"] = new SelectList(_context.Rights, "Id", "Name", user.PravaId);
            ViewData["Pass"] = decryptedData;

            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Login,Password,CokId,PravaId")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    user.Password = Utils.Encrypt(user.Password);
                    _ = _context.Update(user);
                    _ = await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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
            ViewData["CokId"] = new SelectList(_context.Coks, "Id", "Id", user.CokId);
            ViewData["PravaId"] = new SelectList(_context.Rights, "Id", "Id", user.PravaId);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User user = await _context.Users
                .Include(u => u.Cok)
                .Include(u => u.Prava)
                .FirstOrDefaultAsync(m => m.Id == id);
            return user == null ? NotFound() : (IActionResult)View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            User user = await _context.Users.FindAsync(id);
            _ = _context.Users.Remove(user);
            _ = await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

    }
}
