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

        // GET: ManagePatient/Create
        public ActionResult Create(string docID)
        {
            DocumentMetadata documentMetadata = db.DocumentMetadata.Find(docID);
            if (documentMetadata == null)
            {
                // add temporary error message:
                TempData["ErrorMessage"] = "The document could not be found.";
                return RedirectToAction("Create", "ManageDocument");
            }


            ViewBag.UserID = new SelectList(db.iCareUser, "ID", "ID"); // Dropdown for existing iCareUsers
            ViewBag.geographicalUnit = new SelectList(db.GeoCodes, "ID", "description");
            ViewBag.modifierID = new SelectList(db.ModificationHistory, "ID", "description");

            ViewBag.DocID = docID;
            return View();
        }

        // POST: ManagePatient/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,name,address,dateOfBirth,weight,height,bloodGroup,bedID,treatmentArea,geographicalUnit,treatedBy,docID,modifierID")] PatientRecord patientRecord)
        { 
            if (ModelState.IsValid)
            {
                try 
                {
                    db.PatientRecord.Add(patientRecord);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException?.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
                    {
                        ModelState.AddModelError("ID", "The Patient ID is taken.");
                    }
                }

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
            ViewBag.modifierID = new SelectList(db.ModificationHistory, "ID", "description", patientRecord.modifierID);
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
