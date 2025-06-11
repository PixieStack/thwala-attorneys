using Microsoft.AspNetCore.Mvc;

namespace thwala_attorneys.Controllers
{
    public class AboutUsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
