using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RegistrDisconnection.Controllers
{
    /// <summary>
    /// Адміністрування програми
    /// </summary>
    public class AdminController : Controller
    {
        // GET: Admin
        [Authorize(Roles = "Адміністратор")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
