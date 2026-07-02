using Microsoft.AspNetCore.Mvc;

namespace medicalapp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Doctors()
        {
            return View();
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