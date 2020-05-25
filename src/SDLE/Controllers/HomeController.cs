using Microsoft.AspNet.Identity;
using SDLE.Models;
using System.IO;
using System.Web.Mvc;

namespace SDLE.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (System.IO.File.Exists(Path.Combine(Server.MapPath("~"), "users.local.credential")))
            {
                Users users = Users.Load(Path.Combine(Server.MapPath("~").ToString(), "users.local.credential"));

                foreach (User user in users)
                    if (user.Username == User.Identity.GetUserName())
                        return RedirectToAction("Index", "Administration");
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}