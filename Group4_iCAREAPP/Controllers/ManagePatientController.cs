using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Group4_iCAREAPP.Models;

namespace Group4_iCAREAPP.Controllers
{
    public class ManagePatientController : Controller
    {
        private Group4_iCAREDBEntities db = new Group4_iCAREDBEntities();

        // GET: ManagePatient
        public ActionResult Index()
        {
            var patientRecord = db.PatientRecord.Include(p => p.DocumentMetadata).Include(p => p.GeoCodes).Include(p => p.ModificationHistory);
            return View(patientRecord.ToList());
        }

        // GET: ManagePatient/Details/5
        public ActionResult Details(string ID)
        {
            if (ID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PatientRecord patientRecord = db.PatientRecord.Find(ID);

            if (patientRecord == null)
            {
                return HttpNotFound();
            }
            return View(patientRecord);
        }

        // GET: ManagePatient/Create
        public ActionResult Create()
        {
            // Fetch the logged-in user's data
            var userId = User.Identity.Name; // Adjust this based on how you get the user ID
            var currentUser = db.iCareUser.FirstOrDefault(u => u.ID == userId);
            var currentWorker = db.iCareWorker.FirstOrDefault(w => w.ID == userId);

            // Store the user and worker information in the ViewBag for use in the layout
            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentWorker = currentWorker;

            ViewBag.UserID = new SelectList(db.iCareWorker, "ID", "ID"); // Dropdown for existing iCareWorkers
            ViewBag.geographicalUnit = new SelectList(db.GeoCodes, "ID", "description");
            ViewBag.modifierID = new SelectList(db.ModificationHistory, "ID", "description");

            return View();
        }

        // POST: ManagePatient/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,name,address,dateOfBirth,weight,height,bloodGroup,bedID,treatmentArea,geographicalUnit,docID,doctorCount,nurseCount,modifierID")] PatientRecord patientRecord)
        {
            if (ModelState.IsValid)
            {
                // generate metadata docID with a new, unique ID
                string docID = Guid.NewGuid().ToString("N").Substring(0, 5);

                // auto set the name of the document to be the patient's name:
                string docName = "Patient: " + patientRecord.name;

                // fill out required metadata info
                var metadata = new DocumentMetadata
                {
                    docID = docID,
                    userID = User.Identity.Name,
                    docName = docName,
                    dateOfCreation = DateTime.Now,   // auto fills out automatically
                    versions = 1.ToString(),
                    docType = "PATIENT"
                };

                // create metadata entry in db
                db.DocumentMetadata.Add(metadata);

                // assign variables in the patient record
                patientRecord.docID = docID;
                patientRecord.ID = docID;
                patientRecord.doctorCount = 0;
                patientRecord.nurseCount = 0;
                db.PatientRecord.Add(patientRecord);
                db.SaveChanges();

                TempData["SuccessMessage"] = $"Patient record for {patientRecord.name} created and worker assigned successfully.";
                return RedirectToAction("Index", "iCareBoard");
            }

            ViewBag.DocID = patientRecord.docID;
            ViewBag.geographicalUnit = new SelectList(db.GeoCodes, "ID", "description", patientRecord.geographicalUnit);
            ViewBag.modifierID = new SelectList(db.ModificationHistory, "ID", "description", patientRecord.modifierID);
            return View(patientRecord);
        }

        // GET: ManagePatient/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PatientRecord patientRecord = db.PatientRecord.Find(id);
            if (patientRecord == null)
            {
                return HttpNotFound();
            }
            ViewBag.docID = new SelectList(db.DocumentMetadata, "docID", "userID", patientRecord.docID);
            ViewBag.geographicalUnit = new SelectList(db.GeoCodes, "ID", "description", patientRecord.geographicalUnit);
            ViewBag.modifierID = new SelectList(db.ModificationHistory, "ID", "description", patientRecord.modifierID);
            return View(patientRecord);
        }

        // POST: ManagePatient/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,name,address,dateOfBirth,weight,height,bloodGroup,bedID,treatmentArea,geographicalUnit")] PatientRecord patientRecord)
        {
            if (ModelState.IsValid)
            {
                var originalPatientRecord = db.PatientRecord.FirstOrDefault(pr => pr.ID == patientRecord.ID);
                
                // save unmodifiable values:
                patientRecord.docID = originalPatientRecord.docID;
                patientRecord.nurseCount = originalPatientRecord.nurseCount;
                patientRecord.doctorCount = originalPatientRecord.doctorCount;

                db.Entry(originalPatientRecord).State = EntityState.Modified;

                // save new changes:
                originalPatientRecord.name = patientRecord.name;
                originalPatientRecord.address = patientRecord.address;
                originalPatientRecord.dateOfBirth = patientRecord.dateOfBirth;
                originalPatientRecord.weight = patientRecord.weight;
                originalPatientRecord.height = patientRecord.height;
                originalPatientRecord.bloodGroup = patientRecord.bloodGroup;
                originalPatientRecord.bedID = patientRecord.bedID;
                originalPatientRecord.treatmentArea = patientRecord.treatmentArea;
                originalPatientRecord.geographicalUnit = patientRecord.geographicalUnit;

                db.SaveChanges();
                return RedirectToAction("DocList", "DisplayPalette");
            }
            ViewBag.geographicalUnit = new SelectList(db.GeoCodes, "ID", "description", patientRecord.geographicalUnit);
            return View(patientRecord);
        }

        // GET: ManagePatient/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PatientRecord patientRecord = db.PatientRecord.Find(id);
            if (patientRecord == null)
            {
                return HttpNotFound();
            }
            return View(patientRecord);
        }

        // POST: ManagePatient/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            PatientRecord patientRecord = db.PatientRecord.Find(id);
            db.PatientRecord.Remove(patientRecord);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
