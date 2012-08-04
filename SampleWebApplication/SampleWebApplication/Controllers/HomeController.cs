using System.Web.Mvc;

namespace SampleWebApplication.Controllers
{
    public class HomeController : BaseController
    {
        [Authorize]
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to Usermanagement library showcase!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
