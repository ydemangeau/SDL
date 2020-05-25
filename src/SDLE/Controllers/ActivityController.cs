using Microsoft.AspNet.Identity;
using SDLE.Models;
using SDLL;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;

namespace SDLE.Controllers
{
    [Authorize]
    public class ActivityController : Controller
    {
        // GET: Activite
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult NewActivities()
        {
            BDDAccess bddAccess = new BDDAccess();

            List<Activity> activities = new List<Activity>();

            if (bddAccess.Connect(Path.Combine(Server.MapPath("~").ToString(), "bdd.bin")))
            {
                if (bddAccess.Information != null && bddAccess.Information != "")
                {
                    ViewBag.Erreur = bddAccess.Information;

                    return View("Error");
                }

                if (Session["User"] == null)
                {
                    Users users = Users.Load(Path.Combine(Server.MapPath("~"), "credential", User.Identity.GetUserName() + ".credential"));

                    Session["User"] = users[0];
                }

                Student student = new Student() { FirstName = (Session["User"] as User).FirstName, LastName = (Session["User"] as User).LastName, Class = new Class() { ClassName = Student.GetClass((Session["User"] as User).Groups[0]) } };

                activities = bddAccess.GetActivities(student, ActivitiesVisibility.Visible);
            }
            else
            {
                ViewBag.Error = bddAccess.Information;

                return View("Error");
            }

            return View(activities);
        }

        [HttpGet]
        public ActionResult SavedActivities()
        {
            BDDAccess bddAccess = new BDDAccess();

            List<Activity> activities = new List<Activity>();

            if (bddAccess.Connect(Path.Combine(Server.MapPath("~").ToString(), "bdd.bin")))
            {
                if (bddAccess.Information != null && bddAccess.Information != "")
                {
                    ViewBag.Erreur = bddAccess.Information;

                    return View("Error");
                }

                if (Session["User"] == null)
                {
                    Users users = Users.Load(Path.Combine(Server.MapPath("~"), "credential", User.Identity.GetUserName() + ".credential"));

                    Session["User"] = users[0];
                }

                Student student = new Student() { FirstName = (Session["User"] as User).FirstName, LastName = (Session["User"] as User).LastName, Class = new Class() { ClassName = Student.GetClass((Session["User"] as User).Groups[0]) } };

                activities = bddAccess.GetSavedActivities(student, ActivitiesVisibility.Visible);
            }
            else
            {
                ViewBag.Error = bddAccess.Information;

                return View("Error");
            }

            return View(activities);
        }

        [HttpGet]
        public ActionResult Work(int id, string tag)
        {
            Activity model = new Activity();

            BDDAccess bddAccess = new BDDAccess();

            if (bddAccess.Connect(Path.Combine(Server.MapPath("~").ToString(), "bdd.bin")))
            {
                if (bddAccess.Information != null && bddAccess.Information != "")
                {
                    ViewBag.Erreur = bddAccess.Information;

                    return View("Error");
                }

                Student student = new Student() { FirstName = (Session["User"] as User).FirstName, LastName = (Session["User"] as User).LastName, Class = new Class() { ClassName = Student.GetClass((Session["User"] as User).Groups[0]) } };

                if (tag == "saved")
                    model = bddAccess.GetSavedActivity(id, student);
                else
                    model = bddAccess.GetActivity(id);

                model.GetTextToDisplay("");

                model.CountWords();

                Session["Activity"] = model;
            }
            else
            {
                ViewBag.Error = bddAccess.Information;

                return View("Error");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Work(string motAChercher)
        {
            Activity model = Session["Activity"] as Activity;
           
            BDDAccess bddAccess = new BDDAccess();

            if (bddAccess.Connect(Path.Combine(Server.MapPath("~").ToString(), "bdd.bin")))
            {
                if (!string.IsNullOrEmpty(motAChercher) && motAChercher != null)
                    model.CheckWordPresence(motAChercher); 

                Session["Activity"] = model;

                return View(model);
            }
            else
            {
                ViewBag.Error = bddAccess.Information;

                return View("Error");
            }
        }
        
        public ActionResult ProvidedWords()
        {
            Activity model = Session["Activity"] as Activity;

            model.DisplayProvidedWords = true;
            model.GetTextToDisplay("");

            return View("Work", model);
        }

        public ActionResult Save()
        {
            Activity model = Session["Activity"] as Activity;
            
            BDDAccess bddAccess = new BDDAccess();

            if (bddAccess.Connect(Path.Combine(Server.MapPath("~").ToString(), "bdd.bin")))
            {
                if (bddAccess.Information != null && bddAccess.Information != "")
                {
                    ViewBag.Erreur = bddAccess.Information;

                    return View("Error");
                }

                bddAccess.SaveActivities(model);
            }
            else
            {
                ViewBag.Error = bddAccess.Information;

                return View("Error");
            }

            return View("Work", model);
        }
    }
}