using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistrDisconnection.Data;
using System.Linq;

namespace RegistrDisconnection.Controllers
{
    /// <summary>
    /// Детальна інформація про абонента
    /// </summary>
    public class AbonDetails : Controller
    {

        private readonly MainContext _context;
        private readonly IWebHostEnvironment appEnvir;

        public AbonDetails(MainContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            appEnvir = appEnvironment;
        }

        public IActionResult Index(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            //джойнимо всі таблиці
            Models.Abonents.ActualDataPerson men = _context.ActualDatas
                .Include(ad => ad.Person)
                .ThenInclude(p => p.Address)
                .ThenInclude(a => a.Direction)
                .Include(ad => ad.Finance)
                .Include(ad => ad.Poperedgenia)
                .Include(ad => ad.Lichylnyk)
                .Include(ad => ad.Vykl)
                .FirstOrDefault(m => m.Id == Id);

            return men == null ? NotFound() : (IActionResult)View(men);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }
    }
}
