using Microsoft.AspNetCore.Mvc;

namespace StockManagemant.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}
