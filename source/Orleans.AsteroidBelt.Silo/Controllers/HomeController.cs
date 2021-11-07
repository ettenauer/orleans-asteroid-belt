using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Orleans.AsteroidBelt.Silo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
