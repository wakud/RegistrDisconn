using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistrDisconnection.Models;
using System.Diagnostics;

namespace RegistrDisconnection.Controllers
{
    /// <summary>
    /// Головна сторінка програми
    /// </summary>
    public class HomeController : Controller
    {
        [Authorize(Roles = "Адміністратор, Користувач, Бухгалтер")]     //сторінка індекс відкривається тільки для авторизованих користувачів
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
