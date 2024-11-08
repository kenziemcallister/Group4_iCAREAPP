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

            // Check if there is a success message (from after creating a new patient record
            ViewBag.SuccessMessage = TempData["SuccessMessage"];


            // Populate Geographical Unit dropdown list
            ViewBag.GeographicalUnitList = new SelectList(db.GeoCodes, "ID", "description");

            // Load all patients and calculate visibility for Assign Patient button
            var patientRecords = db.PatientRecord.Include(p => p.GeoCodes)
                                                 .Include(p => p.DocumentMetadata)
                                                 .Include(p => p.ModificationHistory)
                                                 .ToList();

            // Create a list of view models with button visibility based on conditions
            var viewModel = patientRecords.Select(patient => new PatientViewModel
            {
                Patient = patient,
                IsAssignButtonVisible = ShouldShowAssignButton(patient, currentWorker)
            }).ToList();

            return View(viewModel);
        }

        // Helper method to determine if Assign Patient button should be shown
        private bool ShouldShowAssignButton(PatientRecord patient, iCareWorker currentWorker)
        {
            var userId = User.Identity.Name;
            ViewBag.workerID = userId;
            // Condition 1: Check if the patient has 3 nurses and 1 doctor (button hidden if true)
            if (patient.nurseCount >= 3 && patient.doctorCount >= 1 && !db.TreatmentRecord.Any(tr => tr.patientID == patient.ID && tr.workerID == userId))
                return false;

            // Condition 2: If logged-in user is a nurse, show button if nurse count < 3
            if (currentWorker.profession == "nurse" && patient.nurseCount < 3 && !db.TreatmentRecord.Any(tr => tr.patientID == patient.ID && tr.workerID == userId))
                return true;

            // Condition 3: If logged-in user is a doctor, show button if no doctor and at least 1 nurse
            if (currentWorker.profession == "doctor" && patient.doctorCount == 0 && patient.nurseCount > 0 && !db.TreatmentRecord.Any(tr => tr.patientID == patient.ID && tr.workerID == userId))
                return true;

            return false;
        }

        // GET: iCAREBoard/Details/id
        public ActionResult Details(string id) // adjust the type if PatientRecordID is not string
        {
            // Fetch the patient record by ID
            var patientRecord = db.PatientRecord.Include(p => p.GeoCodes)
                                                .Include(p => p.DocumentMetadata)
                                                .Include(p => p.ModificationHistory)
                                                .FirstOrDefault(p => p.ID == id);

            if (patientRecord == null)
            {
                return HttpNotFound(); // Return a 404 if the record isn't found
            }

            return View(patientRecord); // Pass the patient record to the Details view
        }

        // Action for AJAX filtering by geographical unit
        public ActionResult FilterByGeographicalUnit(string geographicalUnit)
        {
            var userId = User.Identity.Name;
            var currentWorker = db.iCareWorker.FirstOrDefault(w => w.ID == userId);

            // Fetch and filter patient records based on geographical unit
            var patientRecords = db.PatientRecord.Include(p => p.GeoCodes)
                                                 .Include(p => p.DocumentMetadata)
                                                 .Include(p => p.ModificationHistory);

            if (!string.IsNullOrEmpty(geographicalUnit))
            {
                patientRecords = patientRecords.Where(p => p.geographicalUnit == geographicalUnit);
            }

            // Create the filtered view model list
            var viewModel = patientRecords.ToList().Select(patient => new PatientViewModel
            {
                Patient = patient,
                IsAssignButtonVisible = ShouldShowAssignButton(patient, currentWorker)
            }).ToList();

            return PartialView("_PatientList", viewModel);
        }
    }

    // View model to pass patient data and assign button visibility
    public class PatientViewModel
    {
        public PatientRecord Patient { get; set; }
        public bool IsAssignButtonVisible { get; set; }
    }
}


