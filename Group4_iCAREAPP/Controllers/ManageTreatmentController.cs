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
    public class ManageTreatmentController : Controller
    {
        private Group4_iCAREDBEntities db = new Group4_iCAREDBEntities();

        // GET: ManageTreatment
        public ActionResult Index()
        {
            var treatmentRecord = db.TreatmentRecord.Include(t => t.iCareWorker).Include(t => t.PatientRecord).Include(t => t.DrugsManagementSystem).Include(t => t.DocumentMetadata);
            return View(treatmentRecord.ToList());
        }

        // GET: ManageTreatment/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TreatmentRecord treatmentRecord = db.TreatmentRecord.Find(id);
            if (treatmentRecord == null)
            {
                return HttpNotFound();
            }
            return View(treatmentRecord);
        }

        // GET: ManageTreatment/Create
        public ActionResult Create(string docID)
        {
            DocumentMetadata documentMetadata = db.DocumentMetadata.Find(docID);
            if (documentMetadata == null)
            {
                // add temporary error message:
                TempData["ErrorMessage"] = "The document could not be found.";
                return RedirectToAction("Create", "ManageDocument");
            }

            var userID = User.Identity.Name;

            //Find distinct patients assigned to the logged in worker:
            var assignedPatientIDs = db.TreatmentRecord
                .Where(treatmentRecord => treatmentRecord.workerID == userID)
                .Select(treatmentRecord => treatmentRecord.patientID)
                .Distinct()
                .ToList();

            var assignedPatients = db.PatientRecord
                .Where(patientRecord => assignedPatientIDs.Contains(patientRecord.ID))
                .ToList();

            ViewBag.workerID = new SelectList(db.iCareWorker, "ID", "ID");
            ViewBag.assignedPatientID = new SelectList(assignedPatients, "ID", "name");
            ViewBag.drugID = new SelectList(db.DrugsManagementSystem, "drugID", "drugName");
            ViewBag.docID = docID;
            return View();
        }

        // POST: ManageTreatment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "treatmentID,description,treatmentDate,patientID,workerID,drugID,docID")] TreatmentRecord treatmentRecord)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.TreatmentRecord.Add(treatmentRecord);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException?.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
                    {
                        ModelState.AddModelError("treatmentID", "The Treatment ID is taken.");
                    }
                }
            }

            ViewBag.workerID = new SelectList(db.iCareWorker, "ID", "profession", treatmentRecord.workerID);
            ViewBag.patientID = treatmentRecord.patientID;
            ViewBag.drugID = new SelectList(db.DrugsManagementSystem, "drugID", "drugName", treatmentRecord.drugID);
            ViewBag.docID = treatmentRecord.docID;
            return View(treatmentRecord);
        }

        // GET: ManageTreatment/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TreatmentRecord treatmentRecord = db.TreatmentRecord.Find(id);
            if (treatmentRecord == null)
            {
                return HttpNotFound();
            }
            ViewBag.workerID = new SelectList(db.iCareWorker, "ID", "profession", treatmentRecord.workerID);
            ViewBag.patientID = new SelectList(db.PatientRecord, "ID", "name", treatmentRecord.patientID);
            ViewBag.drugID = new SelectList(db.DrugsManagementSystem, "drugID", "drugName", treatmentRecord.drugID);
            ViewBag.docID = new SelectList(db.DocumentMetadata, "docID", "userID", treatmentRecord.docID);
            return View(treatmentRecord);
        }

        // POST: ManageTreatment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "treatmentID,description,treatmentDate,patientID,workerID,drugID,docID")] TreatmentRecord treatmentRecord)
        {
            if (ModelState.IsValid)
            {
                db.Entry(treatmentRecord).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.workerID = new SelectList(db.iCareWorker, "ID", "profession", treatmentRecord.workerID);
            ViewBag.patientID = new SelectList(db.PatientRecord, "ID", "name", treatmentRecord.patientID);
            ViewBag.drugID = new SelectList(db.DrugsManagementSystem, "drugID", "drugName", treatmentRecord.drugID);
            ViewBag.docID = new SelectList(db.DocumentMetadata, "docID", "userID", treatmentRecord.docID);
            return View(treatmentRecord);
        }

        // GET: ManageTreatment/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TreatmentRecord treatmentRecord = db.TreatmentRecord.Find(id);
            if (treatmentRecord == null)
            {
                return HttpNotFound();
            }
            return View(treatmentRecord);
        }

        // POST: ManageTreatment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            TreatmentRecord treatmentRecord = db.TreatmentRecord.Find(id);
            db.TreatmentRecord.Remove(treatmentRecord);
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
