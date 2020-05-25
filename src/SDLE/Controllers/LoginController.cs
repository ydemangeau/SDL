using Dev2Be.Toolkit.Encrypting;
using SDLE.Models;
using SDLL;
using SDLL.Configuration;
using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Security;

namespace SDLE.Controllers
{
    public enum SignInStatus
    {
        Success,
        Failure,
        AdminAccount,
        ChangePassword,
        ChangePasswordFirstUse
    }

    public class LoginController : Controller
    {
        // GET: Login
        [HttpGet]
        public ActionResult Index(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public SignInStatus CheckCredential(LoginViewModel model)
        {
            if (!System.IO.File.Exists(Path.Combine(Server.MapPath("~"), "users.local.credential")) && model.Username.Equals("Administrateur", StringComparison.CurrentCultureIgnoreCase) && model.Password == "admin")
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);

                return SignInStatus.ChangePasswordFirstUse;
            }
            else if (System.IO.File.Exists(Path.Combine(Server.MapPath("~"), "users.local.credential")))
            {
                Users users = Users.Load(Path.Combine(Server.MapPath("~").ToString(), "users.local.credential"));

                foreach (User user in users)
                {
                    if (user.Username.Equals(model.Username, StringComparison.CurrentCultureIgnoreCase) && user.Password == Hash.SHA512(model.Password))
                    {
                        FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);

                        return SignInStatus.AdminAccount;
                    }
                }
            }

            if (System.IO.File.Exists(Path.Combine(Server.MapPath("~"), "serveurDomaine.bin")))
            {
                DomainServerConfiguration informationsServeurDomaine = DomainServerConfiguration.Charger(Path.Combine(Server.MapPath("~").ToString(), "serveurDomaine.bin"));

                string ipDomaine = informationsServeurDomaine.IPAddress + "/";
                string[] domaines = informationsServeurDomaine.DomainName.Split('.');

                foreach (string domaine in domaines)
                    ipDomaine += "DC=" + domaine + ",";

                ipDomaine = ipDomaine.TrimEnd(',');

                Ldap ldap = new Ldap(ipDomaine, model.Username, model.Password);

                if (ldap.Authentification())
                {
                    Session["User"] = new User() { FirstName = ldap.GetFirstName(), Groups = ldap.GetGroup(), LastName = ldap.GetLastName(), Username = model.Username };
                    
                    //if ((Session["User"] as User).FirstName == "")
                        FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    /*else
                        FormsAuthentication.SetAuthCookie((Session["User"] as User).FirstName, false);*/

                    if (!Directory.Exists(Path.Combine(Server.MapPath("~"), "credential")))
                        Directory.CreateDirectory(Path.Combine(Server.MapPath("~"), "credential"));

                    new Users { (Session["User"] as User) }.Save(Path.Combine(Server.MapPath("~"), "credential", model.Username + ".credential"));

                    return SignInStatus.Success;
                }
            }

            return SignInStatus.Failure;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = CheckCredential(model);

            switch(result)
            {
                case SignInStatus.Success:
                    BDDAccess bddAccess = new BDDAccess();

                    if(bddAccess.Connect(Path.Combine(Server.MapPath("~"), "bdd.bin")))
                    {
                        if (bddAccess.GetIdEtudiant((Session["User"] as User).FirstName, (Session["User"] as User).LastName) == -1)
                            bddAccess.CreateStudent((Session["User"] as User).FirstName, (Session["User"] as User).LastName, Student.GetClass((Session["User"] as User).Groups[0]));
                    }
                    else
                    {
                        ViewBag.Error = bddAccess.Information;

                        return View("Error");
                    }

                    if (Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    else
                        return RedirectToAction("Index", "Activity");
                case SignInStatus.ChangePasswordFirstUse:
                    return RedirectToAction("ChangePassword", "Manage", new { tips = "firstUse" });
                case SignInStatus.AdminAccount:
                    return RedirectToAction("Index", "Administration");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Tentative de connexion non valide.");
                    return View(model);
            }
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Activity");
        }
    }
}