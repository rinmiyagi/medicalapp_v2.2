using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicalapp.Data;
using System.Threading.Tasks;
using System.Linq;

namespace medicalapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Dashboard", "Admin");
                if (User.IsInRole("Doctor"))
                    return RedirectToAction("Dashboard", "Doctor");
                if (User.IsInRole("Receptionist"))
                    return RedirectToAction("Dashboard", "Receptionist");
                
                return RedirectToAction("Dashboard", "Patient");
            }
            return View();
        }

        public async Task<IActionResult> Doctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.IsVerified)
                .ToListAsync();

            return View(doctors);
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult Packages()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
    }
}