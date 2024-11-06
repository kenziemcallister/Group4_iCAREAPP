using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Group4_iCAREAPP.Models;

namespace Group4_iCAREAPP.Controllers
{
    public class iCAREBoardController : Controller
    {
        private Group4_iCAREDBEntities db = new Group4_iCAREDBEntities();

        // GET: iCAREBoard
        public ActionResult Index()
        {
            // Fetch the logged-in user's data
            var userId = User.Identity.Name; // Adjust this based on how you get the user ID
            var currentUser = db.iCareUser.FirstOrDefault(u => u.ID == userId);
            var currentWorker = db.iCareWorker.FirstOrDefault(w => w.ID == userId);

            // Store the user and worker information in the ViewBag for use in the layout
            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentWorker = currentWorker;

            // Populate Geographical Unit dropdown list
            ViewBag.GeographicalUnitList = new SelectList(db.GeoCodes, "ID", "description");

            // Load all patients by default
            var patientRecords = db.PatientRecord.Include(p => p.GeoCodes)
                                                 .Include(p => p.DocumentMetadata)
                                                 .Include(p => p.ModificationHistory).ToList();

            return View(patientRecords);
        }

        // Action for AJAX filtering by geographical unit
        public ActionResult FilterByGeographicalUnit(string geographicalUnit)
        {
            var patientRecords = db.PatientRecord.Include(p => p.GeoCodes)
                                                 .Include(p => p.DocumentMetadata)
                                                 .Include(p => p.ModificationHistory);

            if (!string.IsNullOrEmpty(geographicalUnit))
            {
                patientRecords = patientRecords.Where(p => p.geographicalUnit == geographicalUnit);
            }

            return PartialView("_PatientList", patientRecords.ToList());
        }
    }
}

