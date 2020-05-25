using Microsoft.AspNet.Identity;
using SDLE.Models;
using System.IO;
using System.Web.Mvc;
using Dev2Be.Toolkit.Encrypting;

namespace SDLE.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        //
        // GET: /Manage/Index
        public ActionResult Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Votre mot de passe a été changé."
                : message == ManageMessageId.SetMailSuccess ? "Votre e-mail a été défini."
                : message == ManageMessageId.Error ? "Une erreur s'est produite."
                : "";

            var username = User.Identity.GetUserName();

            Users users = Users.Load(Path.Combine(Server.MapPath("~").ToString(), "users.local.credential"));

            IndexViewModel model = new IndexViewModel();

            foreach (User user in users)
                if (user.Username == username) 
                    model = new IndexViewModel { Email = user.Email };

            return View(model);
        }

        //
        // GET: /Manage/ChangePassword
        [HttpGet]
        public ActionResult ChangePassword(string tips)
        {
            if (tips == "firstUse")
                ViewBag.Tips = "Pour des questions de sécurité, votre mot de passe doit être changé.";

            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!System.IO.File.Exists(Path.Combine(Server.MapPath("~").ToString(), "users.local.credential")))
            {
                if (User.Identity.GetUserName() == "Administrateur" && model.OldPassword == "admin")
                {
                    Users users = new Users()
                    {
                        new User { Username = User.Identity.GetUserName(), Password = Hash.SHA512(model.NewPassword) }
                    };

                    users.Save(Path.Combine(Server.MapPath("~"), "users.local.credential"));

                    return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
                }
            }
            else
            {
                Users users = Users.Load(Path.Combine(Server.MapPath("~"), "users.local.credential"));

                foreach (User user in users)
                {
                    if (user.Username == User.Identity.GetUserName())
                    {
                        if (user.Password == Hash.SHA512(model.OldPassword))
                            user.Password = Hash.SHA512(model.NewPassword);
                        else
                            ModelState.AddModelError("", "Le mot de passe actuel est erroné.");
                    }
                }

                users.Save(Path.Combine(Server.MapPath("~"), "users.local.credential"));

                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }

            return View(model);
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetMailSuccess,
            Error
        }
    }
}