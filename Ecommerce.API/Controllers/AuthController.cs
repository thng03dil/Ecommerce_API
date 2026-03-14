using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
