using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ErrorController : Controller
    {
        [Route("Admin/Error")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
