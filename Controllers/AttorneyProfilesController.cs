using Microsoft.AspNetCore.Mvc;

namespace thwala_attorneys.Controllers
{
    public class AttorneyProfilesController : Controller
    {
        public IActionResult NomthandazoProfile()
        {
            return View();
        }
        public IActionResult NkatekoProfile()
        {
            return View();
        }
        public IActionResult NtshuxekoProfile()
        {
            return View();
        }
    }
}
