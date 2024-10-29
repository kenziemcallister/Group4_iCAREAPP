using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Group4_iCAREAPP.Models;

namespace Group4_iCAREAPP.Controllers
{
    public class AssignPatientController : Controller
    {
        private Group4_iCAREDBEntities db = new Group4_iCAREDBEntities();

        // GET: AssignPatient/GetPatientName
        public JsonResult GetPatientName(string id)
        {
            var patient = db.PatientRecord.Find(id);
            if (patient != null)
            {
                return Json(new { name = patient.name }, JsonRequestBehavior.AllowGet); // Return patient's name
            }
            return Json(new { name = string.Empty }, JsonRequestBehavior.AllowGet); // Return empty if not found
        }

        // GET: AssignPatient
        public ActionResult Index()
        {
            var patientRecord = db.PatientRecord.Include(p => p.DocumentMetadata).Include(p => p.GeoCodes).Include(p => p.iCareWorker).Include(p => p.ModificationHistory);
            return View(patientRecord.ToList());
        }

        // GET: AssignPatient/Details/5
        public ActionResult Details(string id)
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

        // GET: AssignPatient/Assign
        public ActionResult Assign()
        {
            ViewBag.docID = new SelectList(db.DocumentMetadata, "docID", "userID");
            ViewBag.geographicalUnit = new SelectList(db.GeoCodes, "ID", "description");
            ViewBag.treatedBy = new SelectList(db.iCareWorker, "ID", "ID");
            ViewBag.modifierID = new SelectList(db.ModificationHistory, "ID", "description");

            ViewBag.PatientList = new SelectList(db.PatientRecord, "ID", "ID");
            ViewBag.treatedBy = new SelectList(db.iCareWorker, "ID", "ID");

            return View();
        }

        // POST: AssignPatient/Assign
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Assign([Bind(Include = "ID,name,address,dateOfBirth,weight,height,bloodGroup,bedID,treatmentArea,geographicalUnit,treatedBy,docID,modifierID")] PatientRecord patientRecord)
        {
            if (ModelState.IsValid)
            {
                // Create a new TreatmentRecord
                TreatmentRecord treatmentRecord = new TreatmentRecord
                {
                    patientsList = patientRecord.ID, // Link to the selected patient
                    treatedBy = User.Identity.Name // Link to the selected worker
                };

                // Add the TreatmentRecord to the database
                db.TreatmentRecord.Add(treatmentRecord); // Make sure you have a DbSet<TreatmentRecord> in your DbContext
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.docID = new SelectList(db.DocumentMetadata, "docID", "userID", patientRecord.docID);
            ViewBag.geographicalUnit = new SelectList(db.GeoCodes, "ID", "description", patientRecord.geographicalUnit);
            ViewBag.treatedBy = new SelectList(db.iCareWorker, "ID", "ID", patientRecord.treatedBy);
            ViewBag.modifierID = new SelectList(db.ModificationHistory, "ID", "description", patientRecord.modifierID);

            ViewBag.PatientList = new SelectList(db.PatientRecord, "ID", "ID", patientRecord.ID); // Adjust the properties as needed
            ViewBag.treatedBy = new SelectList(db.iCareWorker, "ID", "profession", patientRecord.treatedBy);
            return View(patientRecord);
        }

        // GET: AssignPatient/Edit/5
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
            ViewBag.treatedBy = new SelectList(db.iCareWorker, "ID", "profession", patientRecord.treatedBy);
            ViewBag.modifierID = new SelectList(db.ModificationHistory, "ID", "description", patientRecord.modifierID);
            return View(patientRecord);
        }

        // POST: AssignPatient/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,name,address,dateOfBirth,weight,height,bloodGroup,bedID,treatmentArea,geographicalUnit,treatedBy,docID,modifierID")] PatientRecord patientRecord)
        {
            if (ModelState.IsValid)
            {
                db.Entry(patientRecord).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.docID = new SelectList(db.DocumentMetadata, "docID", "userID", patientRecord.docID);
            ViewBag.geographicalUnit = new SelectList(db.GeoCodes, "ID", "description", patientRecord.geographicalUnit);
            ViewBag.treatedBy = new SelectList(db.iCareWorker, "ID", "profession", patientRecord.treatedBy);
            ViewBag.modifierID = new SelectList(db.ModificationHistory, "ID", "description", patientRecord.modifierID);
            return View(patientRecord);
        }

        // GET: AssignPatient/Delete/5
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

        // POST: AssignPatient/Delete/5
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
