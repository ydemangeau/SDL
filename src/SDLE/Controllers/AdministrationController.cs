using SDLL.Configuration;
using System.IO;
using System.Web.Mvc;

namespace SDLE.Views
{
    [Authorize(Users = "Administrateur")]
    public class AdministrationController : Controller
    {
        // GET: Administration
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ServeurDomaine()
        {
            DomainServerConfiguration domainServerConfiguration = new DomainServerConfiguration();

            if (System.IO.File.Exists(Path.Combine(Server.MapPath("~").ToString(), "serveurDomaine.bin")))
                domainServerConfiguration = DomainServerConfiguration.Charger(Path.Combine(Server.MapPath("~").ToString(), "serveurDomaine.bin"));

            return View(domainServerConfiguration);
        }

        [HttpPost]
        public ActionResult ServeurDomaine(DomainServerConfiguration domainServerConfiguration)
        {
            if (!ModelState.IsValid)
                return View(domainServerConfiguration);

            ViewBag.Information = "Les informations ont bien été sauvegardées.";

            domainServerConfiguration.Sauvegarder(Path.Combine(Server.MapPath("~").ToString(), "serveurDomaine.bin"));

            return View();
        }

        [HttpGet]
        public ActionResult BDD()
        {
            BDDConfiguration informationsServeurDomaine = new BDDConfiguration();

            if (System.IO.File.Exists(Path.Combine(Server.MapPath("~").ToString(), "bdd.bin")))
                informationsServeurDomaine = BDDConfiguration.Load(Path.Combine(Server.MapPath("~").ToString(), "bdd.bin"));

            return View(informationsServeurDomaine);
        }

        [HttpPost]
        public ActionResult BDD(BDDConfiguration bddConfiguration)
        {
            if (!ModelState.IsValid)
                return View(bddConfiguration);

            ViewBag.Information = "Les informations ont bien été sauvegardées.";

            bddConfiguration.Save(Path.Combine(Server.MapPath("~").ToString(), "bdd.bin"));

            return View();
        }
    }
}