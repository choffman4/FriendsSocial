using Microsoft.AspNetCore.Mvc;

namespace ProfileService.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
