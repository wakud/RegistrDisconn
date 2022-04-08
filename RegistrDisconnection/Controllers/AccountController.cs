using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistrDisconnection.Data;
using RegistrDisconnection.Models.Users;
using RegistrDisconnection.MyClasses;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RegistrDisconnection.Controllers
{
    /// <summary>
    /// Авторизація користувача
    /// </summary>
    public class AccountController : Controller
    {
        private readonly MainContext db;
        public AccountController(MainContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await db.Users
                    .Include(u => u.Prava)
                .FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == Utils.Encrypt(model.Password));

                if (user != null)
                {
                    await Authenticate(user);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Не вірний логін і(або) пароль");
                    ViewBag.Error = "BedLogin";
                }
            }
            return View(model);
        }

        private async Task Authenticate(User user)
        {
            // создаем один claim
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Prava?.Name)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

    }
}
