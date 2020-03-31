using Gip.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Gip.Controllers
{
    [Authorize(Roles ="Admin, Lector, Student")]
    public class PlannerController : Controller
    {
        private gipDatabaseContext db = new gipDatabaseContext();

        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public PlannerController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        // GET /planner
        [HttpGet]
        [Route("planner")]
        public async Task<ActionResult> Index(int week)
        {
            int weekToUse = GetIso8601WeekOfYear(DateTime.Now)+week;
            try
            {
                List<Planner> planners = new List<Planner>();
                var user = await userManager.GetUserAsync(User);

                if (User.IsInRole("Student"))
                {
                    var qry2 = from c in db.CourseUser
                               where c.ApplicationUserId == user.Id
                               select c;

                    List<int?> vakMetStud = new List<int?>();
                    
                    foreach (var cu in qry2) 
                    {
                        if (cu.GoedGekeurd) 
                        {
                            vakMetStud.Add(cu.CourseId);
                        }
                    }

                    var _qry = from cm in db.CourseMoment
                               join c in db.Course on cm.CourseId equals c.Id
                               join s in db.Schedule on cm.ScheduleId equals s.Id
                               join r in db.Room on cm.RoomId equals r.Id
                               join u in db.Users on cm.ApplicationUserId equals u.Id
                               where (int)((s.Datum.DayOfYear / 7.0) + 0.2) == weekToUse
                               where vakMetStud.Contains(cm.CourseId)
                               orderby s.Datum, s.Startmoment, s.Eindmoment, r.Gebouw, r.Verdiep, r.Nummer
                               select new
                               {
                                   cmId = cm.Id,
                                   datum = s.Datum,
                                   startmoment = s.Startmoment,
                                   gebouw = r.Gebouw,
                                   verdiep = r.Verdiep,
                                   nummer = r.Nummer,
                                   vakcode = c.Vakcode,
                                   titel = c.Titel,
                                   eindmoment = s.Eindmoment,
                               };
                    foreach (var qry in _qry)
                    {
                        Planner planner = new Planner { cmId = qry.cmId, 
                                                        Datum = qry.datum, 
                                                        Startmoment = qry.startmoment, 
                                                        Gebouw = qry.gebouw, 
                                                        Verdiep = qry.verdiep, 
                                                        Nummer = qry.nummer, 
                                                        Vakcode = qry.vakcode, 
                                                        Titel = qry.titel, 
                                                        Eindmoment = qry.eindmoment};
                        planners.Add(planner);
                    }
                }
                else {
                    var _qry = from cm in db.CourseMoment
                               join c in db.Course on cm.CourseId equals c.Id
                               join s in db.Schedule on cm.ScheduleId equals s.Id
                               join r in db.Room on cm.RoomId equals r.Id
                               join u in db.Users on cm.ApplicationUserId equals u.Id
                               where (int)((s.Datum.DayOfYear / 7.0) + 0.2) == weekToUse
                               orderby s.Datum, s.Startmoment, s.Eindmoment, r.Gebouw, r.Verdiep, r.Nummer
                               select new
                               {
                                   cmId = cm.Id,
                                   cId = c.Id,
                                   datum = s.Datum,
                                   startmoment = s.Startmoment,
                                   gebouw = r.Gebouw,
                                   verdiep = r.Verdiep,
                                   nummer = r.Nummer,
                                   vakcode = c.Vakcode,
                                   titel = c.Titel,
                                   eindmoment = s.Eindmoment,
                                   lessenlijst = cm.LessenLijst
                               };
                    var lokaalQry = from lok in db.Room
                                    orderby lok.Gebouw, lok.Verdiep, lok.Nummer
                                    select lok;
                    var vakQry = from vak in db.Course
                                 orderby vak.Vakcode
                                 select vak;

                    foreach (var qry in _qry)
                    {
                        Planner planner = new Planner { cmId = qry.cmId, 
                                                        cId = qry.cId,
                                                        Datum = qry.datum, 
                                                        Startmoment = qry.startmoment, 
                                                        Gebouw = qry.gebouw, 
                                                        Verdiep = qry.verdiep, 
                                                        Nummer = qry.nummer, 
                                                        Vakcode = qry.vakcode, 
                                                        Titel = qry.titel, 
                                                        Eindmoment = qry.eindmoment,
                                                        LessenLijst = qry.lessenlijst};
                        planners.Add(planner);
                    }

                    foreach (var qry in lokaalQry)
                    {
                        Planner planner = new Planner { rId = qry.Id, Gebouw = qry.Gebouw, Verdiep = qry.Verdiep, Nummer = qry.Nummer, Capaciteit = qry.Capaciteit};
                        planners.Add(planner);
                    }

                    foreach (var qry in vakQry)
                    {
                        Planner planner = new Planner { cId = qry.Id, Vakcode = qry.Vakcode, Titel = qry.Titel};
                        planners.Add(planner);
                    }
                }

                ViewBag.maandag = FirstDayOfWeek(weekToUse).ToString("dd-MM-yyyy");
                ViewBag.vrijdag = FirstDayOfWeek(weekToUse).AddDays(4).ToString("dd-MM-yyyy");

                ViewBag.nextWeek = week += 1;
                ViewBag.prevWeek = week -= 2;

                if (TempData["error"] != null)
                {
                    ViewBag.error = TempData["error"].ToString();
                    TempData["error"] = null;
                }
                if (ViewBag.error == null || !ViewBag.error.Contains("addError") && !ViewBag.error.Contains("addGood") && !ViewBag.error.Contains("deleteError") && !ViewBag.error.Contains("deleteGood") && !ViewBag.error.Contains("editError") && !ViewBag.error.Contains("editGood") && !ViewBag.error.Contains("topicError"))
                {
                    ViewBag.error = "indexLokaalGood";
                }
                return View("../Planning/Index",planners);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TempData["error"] = "indexVakError" + "/" + "Er liep iets mis bij het ophalen van de planner.";
                return RedirectToAction("Index", "Home");
            }
        }

        //fixed - nog iets aan toevoegen
        [HttpPost]
        [Route("planner/add")]
        [Authorize(Roles = "Admin, Lector")]
        public async Task<IActionResult> Add(string dat, string uur, int lokaalId, double duratie, int vakid, string? lessenlijst,bool? checkbox, int lokaal2Id)
        {
            DateTime datum = DateTime.ParseExact(dat, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime tijd = new DateTime(1800, 1, 1, int.Parse(uur.Split(":")[0]), int.Parse(uur.Split(":")[1]), 0);
            double _duratie = Convert.ToDouble(duratie);

            if (_duratie <=0) {
                TempData["error"] = "addError" + "/" + "De duratie mag niet negatief zijn, noch 0.";
                return RedirectToAction("Index", "Planner");
            }

            DateTime eindmoment = tijd.AddHours(_duratie);

            try
            {
                //hier code schrijven zodat er niet altijd een nieuwe schedule wordt aangemaakt
                Schedule schedule = new Schedule { Datum = datum, Startmoment = tijd, Eindmoment = eindmoment };

                db.Schedule.Add(schedule);
                db.SaveChanges();

                var user = await userManager.GetUserAsync(User);

                CourseMoment moment = new CourseMoment { CourseId = vakid, ScheduleId = schedule.Id, RoomId = lokaalId, ApplicationUserId = user.Id, LessenLijst = lessenlijst};

                //hier code schrijven zodat er geen tweede dezelfde coursemoment aangemaakt kan worden

                db.CourseMoment.Add(moment);
                db.SaveChanges();

                if (checkbox != null && checkbox == true)
                {
                    CourseMoment moment2 = new CourseMoment { CourseId = vakid, ScheduleId = schedule.Id, RoomId = lokaal2Id, ApplicationUserId = user.Id, LessenLijst = lessenlijst};

                    //hier code schrijven zodat er geen tweede dezelfde coursemoment aangemaakt kan worden

                    db.CourseMoment.Add(moment2);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TempData["error"] = "addError" + "/" + e.Message;
                return RedirectToAction("Index", "Planner");
            }
            TempData["error"] = "addGood";
            return RedirectToAction("Index", "Planner");
        }

        [HttpGet]
        [Route("planner/add")]
        [Authorize(Roles = "Admin, Lector")]
        public ActionResult Add()
        {
            try
            {
                var lokaalQry = from lok in db.Room
                                orderby lok.Gebouw, lok.Verdiep, lok.Nummer
                                select lok;
                var vakQry = from vak in db.Course
                             orderby vak.Vakcode
                             select vak;

                List < Planner > planners = new List<Planner>();
                foreach (var qry in lokaalQry)
                {
                    Planner planner = new Planner { rId = qry.Id, Gebouw = qry.Gebouw, Verdiep = qry.Verdiep, Nummer = qry.Nummer, Capaciteit = qry.Capaciteit};
                    planners.Add(planner);
                }
                foreach (var qry in vakQry) {
                    Planner planner = new Planner { cId = qry.Id, Vakcode = qry.Vakcode, Titel = qry.Titel};
                    planners.Add(planner);
                }
                return View("../Planner/Index",planners);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TempData["error"] = "indexVakError" + "/" + e.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Route("planner/delete")]
        [Authorize(Roles = "Admin, Lector")]
        public ActionResult Delete(int cmId) {
            CourseMoment moment = db.CourseMoment.Find(cmId);
            if (moment == null) {
                TempData["error"] = "deleteError" + "/" + "Er is geen overeenkomend moment gevonden.";
                return RedirectToAction("Index", "Planner");
            }
            try
            {
                db.CourseMoment.Remove(moment);
                db.SaveChanges();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                TempData["error"] = "deleteError" + "/" + "Er is een databank probleem opgetreden. " + e.InnerException.Message == null ? " " : e.InnerException.Message;
                return RedirectToAction("Index", "Planner");
            }
            TempData["error"] = "deleteGood";
            return RedirectToAction("Index", "Planner");
        }

        //fixed - nog iets aan toevoegen.
        [HttpPost]
        [Route("planner/edit")]
        [Authorize(Roles = "Admin, Lector")]
        public async Task<ActionResult> Edit(int cmId,
            int newVakcode, 
            string newDatum, string newStartMoment, double newDuratie,
            int newLokaalid, string newLessenlijst) {
            try
            {
                CourseMoment oldMoment = db.CourseMoment.Find(cmId);
                if (oldMoment == null)
                {
                    TempData["error"] = "deleteError" + "/" + "Er is geen overeenkomend moment gevonden in de databank.";
                    return RedirectToAction("Index", "Planner");
                }

                if (oldMoment.RoomId != newLokaalid) 
                {
                    oldMoment.RoomId = db.Room.Find(newLokaalid).Id;
                    db.SaveChanges();
                }

                double _duratie = Convert.ToDouble(newDuratie);

                DateTime datum = DateTime.ParseExact(newDatum, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime tijd = new DateTime(1800, 1, 1, int.Parse(newStartMoment.Split(":")[0]), int.Parse(newStartMoment.Split(":")[1]), 0);
                DateTime eindmoment = tijd.AddHours(_duratie);

                if (oldMoment.Schedule.Datum != datum || oldMoment.Schedule.Startmoment != tijd || oldMoment.Schedule.Eindmoment != eindmoment) 
                {
                    // Deze lijn moet nog vervangen worden zodat er niet altijd een nieuwe reeds bestaande schedule wordt aangemaakt: Schedule schedule = db.Schedule.Find(datum, tijd,eindmoment);

                    Schedule schedule = new Schedule { Datum = datum, Startmoment = tijd, Eindmoment = eindmoment};

                    db.Schedule.Add(schedule);
                    db.SaveChanges();

                    oldMoment.ScheduleId = schedule.Id;
                    db.SaveChanges();
                }

                if (oldMoment.CourseId != newVakcode) 
                {
                    oldMoment.CourseId = db.Course.Find(newVakcode).Id;
                    db.SaveChanges();
                }

                oldMoment.LessenLijst = newLessenlijst;

                var user = await userManager.GetUserAsync(User);

                oldMoment.ApplicationUserId = user.Id;
                db.SaveChanges();
            }
            catch (Exception e) {
                Console.WriteLine(e);
                TempData["error"] = "editError" + "/" + e.Message + " " + e.InnerException.Message == null ? " " : e.InnerException.Message;
                return RedirectToAction("Index","Planner");
            }
            TempData["error"] = "editGood";
            db.SaveChanges();
            return RedirectToAction("Index", "Planner");
        }

        [HttpGet]
        [Route("planner/viewTopic")]
        public ActionResult ViewTopic(int cmId)
        {
            try {
                var qryCm = from cm in db.CourseMoment
                            join c in db.Course on cm.CourseId equals c.Id
                            join s in db.Schedule on cm.ScheduleId equals s.Id
                            join r in db.Room on cm.RoomId equals r.Id
                            join u in db.Users on cm.ApplicationUserId equals u.Id
                            where cm.Id == cmId
                            select new
                            {
                                Datum = s.Datum,
                                Startmoment = s.Startmoment,
                                Eindmoment = s.Eindmoment,
                                Gebouw = r.Gebouw,
                                Verdiep = r.Verdiep,
                                Nummer = r.Nummer,
                                Vakcode = c.Vakcode,
                                Titel = c.Titel,
                                LessenLijst = cm.LessenLijst
                            };

                if (qryCm.Any())
                {
                    Planner planner = new Planner
                    {
                        Datum = qryCm.FirstOrDefault().Datum,
                        Startmoment = qryCm.FirstOrDefault().Startmoment,
                        Eindmoment = qryCm.FirstOrDefault().Eindmoment,
                        Gebouw = qryCm.FirstOrDefault().Gebouw,
                        Verdiep = qryCm.FirstOrDefault().Verdiep,
                        Nummer = qryCm.FirstOrDefault().Nummer,
                        Vakcode = qryCm.FirstOrDefault().Vakcode,
                        Titel = qryCm.FirstOrDefault().Titel,
                        LessenLijst = qryCm.FirstOrDefault().LessenLijst
                    };

                    return View("../Planning/ViewTopi", planner);
                }
                else 
                {
                    TempData["error"] = "topicError" + "/" + "Databank fout.";
                    return RedirectToAction("Index", "Planner");
                }
                ////users, courses, room & schedule == null 
                //CourseMoment moment = db.CourseMoment.Find(cmId);

                            //Planner planner = new Planner(
                            //                                moment.Room.Gebouw, 
                            //                                moment.Room.Verdiep, 
                            //                                moment.Room.Nummer, 
                            //                                moment.Courses.Vakcode, 
                            //                                moment.Courses.Titel, 
                            //                                moment.LessenLijst);

                            //return View("../Planning/ViewTopi", planner);
            }
            catch (Exception e) {
                Console.WriteLine(e);
                TempData["error"] = "topicError" + "/" + "Databank fout.";
                return RedirectToAction("Index", "Planner");
            }
        }

        [HttpGet]
        [Route("planner/viewCourseMoments")]
        public ActionResult ViewCourseMoments(int vakcode)
        {
            try
            {
                var _qry = from cm in db.CourseMoment
                           join c in db.Course on cm.CourseId equals c.Id
                           join s in db.Schedule on cm.ScheduleId equals s.Id
                           join r in db.Room on cm.RoomId equals r.Id
                           where cm.CourseId == vakcode
                           select new
                           {
                               datum = s.Datum,
                               startmoment = s.Startmoment,
                               gebouw = r.Gebouw,
                               verdiep = r.Verdiep,
                               nummer = r.Nummer,
                               vakcode = c.Vakcode,
                               titel = c.Titel,
                               eindmoment = s.Eindmoment
                           };

                List<Planner> planners = new List<Planner>();
                foreach (var qry in _qry)
                {
                    Planner planner = new Planner(qry.datum, qry.startmoment, qry.gebouw, qry.verdiep, qry.nummer, qry.vakcode, qry.titel, qry.eindmoment);
                    planners.Add(planner);
                }

                ViewBag.error = "coursemomentsGood";
                return View("../Planning/courseMomentsOffTopic", planners);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TempData["error"] = "topicError" + "/" + "Databank fout.";
                return RedirectToAction("Index", "Planner");
            }
        }

        public static int GetIso8601WeekOfYear(DateTime time)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            return (time.DayOfYear / 7);
        }

        public static DateTime FirstDayOfWeek(int weekOfYear) {
            DateTime jan1 = new DateTime(DateTime.Today.Year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var weekNum = weekOfYear + 1 ;
            if (firstWeek == 1) {
                weekNum -= 1;
            }

            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }
    }
}