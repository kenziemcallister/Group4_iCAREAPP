﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Group4_iCAREAPP.Models;

namespace Group4_iCAREAPP.Controllers
{
    public class AssignPatientController : Controller
    {
        // creates instance of DB 
        private Group4_iCAREDBEntities db = new Group4_iCAREDBEntities();


        // INDEX                GET: AssignPatient
        public ActionResult Index()
        {
            var userId = User.Identity.Name;
            var currentUser = db.iCareUser.FirstOrDefault(u => u.ID == userId);
            var currentWorker = db.iCareWorker.FirstOrDefault(w => w.ID == userId);

            // user and worker information
            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentWorker = currentWorker;

            // from ImportImageController
            var documentMetadata = db.DocumentMetadata
                .Include(d => d.iCareWorker)
                .Include(d => d.ModificationHistory)
                .ToList();

            // import treatmentRecord table from DB
            var treatmentRecord = db.TreatmentRecord
                .Include(t => t.iCareWorker)
                .Include(t => t.PatientRecord);
            return View(treatmentRecord.ToList());
        }


        // DETAILS              GET: AssignPatient/Details/5
        public ActionResult Details(string id)
        {
            // Fetch the logged-in user's data
            var userId = User.Identity.Name; // Adjust this based on how you get the user ID
            var currentUser = db.iCareUser.FirstOrDefault(u => u.ID == userId);
            var currentWorker = db.iCareWorker.FirstOrDefault(w => w.ID == userId);

            // Store the user and worker information in the ViewBag for use in the layout
            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentWorker = currentWorker;

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
        

        // CREATE               GET: AssignPatient/Create
        public ActionResult Create()
        {
            // get & show logged-in user's data
            var userId = User.Identity.Name;
            ViewBag.workerID = userId;

            var currentUser = db.iCareUser.FirstOrDefault(u => u.ID == userId);
            var currentWorker = db.iCareWorker.FirstOrDefault(w => w.ID == userId);

            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentWorker = currentWorker;

            // filter patient list to only show patients eligible to be assigned to user
            var eligiblePatients = db.PatientRecord
                .Where(p =>
                    // check that worker isn't already assigned to the patient
                    !db.TreatmentRecord.Any(tr => tr.patientID == p.ID && tr.workerID == userId)
                    &&
                    // check that patient is eligible for another nurse or doctor
                    (((p.doctorCount == 0 && p.nurseCount >= 1) && db.iCareWorker.Any(w => w.ID == userId && w.profession == "doctor"))
                    || (p.nurseCount < 3 && db.iCareWorker.Any(w => w.ID == userId && w.profession == "nurse")))
                )
                .ToList();

            ViewBag.patientID = new SelectList(eligiblePatients, "ID", "name");
            ViewBag.DrugsList = new SelectList(db.DrugsManagementSystem, "drugID", "drugName");

            return View();
        }

        // POST CREATE          POST: AssignPatient/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "treatmentID,description,treatmentDate,patientID,workerID,drugID,docID")] TreatmentRecord treatmentRecord)
        {
            if (ModelState.IsValid)
            {
                // generate metadata docID with a new, unique ID
                string docID = Guid.NewGuid().ToString("N").Substring(0, 5);

                // fill out required metadata info
                var metadata = new DocumentMetadata
                {
                    docID = docID,
                    userID = User.Identity.Name,
                    docType = "TREATMENT",
                    dateOfCreation = DateTime.Now   // not required, but can fill out automatically
                };

                // create metadata entry in db
                db.DocumentMetadata.Add(metadata);

                // assign variables in the treatment record
                treatmentRecord.docID = docID;
                treatmentRecord.treatmentID = docID;
                treatmentRecord.workerID = User.Identity.Name;

                // new chat code
                var worker = db.iCareWorker.FirstOrDefault(w => w.ID == treatmentRecord.workerID);
                var patientRecord = db.PatientRecord.FirstOrDefault(p => p.ID == treatmentRecord.patientID);
                if (worker == null || patientRecord == null)
                {
                    return Json(new { success = false, message = "Worker or Patient not found." });
                }

                // double check that patient can be assigned another worker
                string workerProfession = worker.profession;
                if ((workerProfession == "doctor" && (patientRecord.doctorCount ?? 0) >= 1) ||
                    (workerProfession == "nurse" && (patientRecord.nurseCount ?? 0) >= 3) ||
                    (workerProfession == "doctor" && (patientRecord.nurseCount ?? 0) == 0))
                {
                    return Json(new { success = false, message = "Patient has reached worker limits or requires a nurse first." });
                }

                // update patient counts based on profession
                if (workerProfession == "doctor") patientRecord.doctorCount += 1;
                else if (workerProfession == "nurse") patientRecord.nurseCount += 1;

                // add to treatment record & save changes
                db.TreatmentRecord.Add(treatmentRecord);
                db.SaveChanges();

                return Json(new { success = true, message = "Treatment record created and worker assigned successfully." });
            }

            return Json(new { success = false, message = "Invalid form data. Please correct and try again." });
        }


        public ActionResult CreateFromBoard(string id)
        {

            var userId = User.Identity.Name;
            ViewBag.workerID = userId;

            var patient = db.PatientRecord.FirstOrDefault(p => p.ID == id);
            if (patient == null)
            {
                return HttpNotFound("Patient not found.");
            }
            
            ViewBag.patientIDBoard = id;
            ViewBag.DrugsList = new SelectList(db.DrugsManagementSystem, "drugID", "drugName");

            return View("CreateFromBoard");
        }


        // POST: AssignPatient/CreateFromBoard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateFromBoard([Bind(Include = "treatmentID,description,treatmentDate,patientID,workerID,drugID,docID")] TreatmentRecord treatmentRecord)
        {
            if (ModelState.IsValid)
            {
                // generate a docIC
                string docID = Guid.NewGuid().ToString("N").Substring(0, 5);

                var patientRecord = db.PatientRecord.FirstOrDefault(p => p.ID == treatmentRecord.patientID);
                var workerRecord = db.iCareUser.FirstOrDefault(p => p.ID == User.Identity.Name);

                // auto set the name of the document to be the patient's name:
                string docName = workerRecord.name + "'s Treatment: " + patientRecord.name;

                // create a doc
                var metadata = new DocumentMetadata
                {
                    docName=docName,
                    docID = docID,
                    userID = User.Identity.Name,
                    docType = "TREATMENT",
                    dateOfCreation = DateTime.Now
                };
                db.DocumentMetadata.Add(metadata);

                // assign treatment record info
                treatmentRecord.docID = docID;
                treatmentRecord.treatmentID = docID;
                treatmentRecord.workerID = User.Identity.Name;

                // check for workers and patients
                var worker = db.iCareWorker.FirstOrDefault(w => w.ID == treatmentRecord.workerID);
                //var patientRecord = db.PatientRecord.FirstOrDefault(p => p.ID == treatmentRecord.patientID);
                if (worker == null || patientRecord == null)
                {
                    return Json(new { success = false, message = "Worker or Patient not found." });
                }

                // check if the worker / patients can be assigned
                string workerProfession = worker.profession;
                if ((workerProfession == "doctor" && (patientRecord.doctorCount ?? 0) >= 1) ||
                    (workerProfession == "nurse" && (patientRecord.nurseCount ?? 0) >= 3) ||
                    (workerProfession == "doctor" && (patientRecord.nurseCount ?? 0) == 0))
                {
                    return Json(new { success = false, message = "Patient has reached worker limits or requires a nurse first." });
                }

                // add to patient counts
                if (workerProfession == "doctor") patientRecord.doctorCount += 1;
                else if (workerProfession == "nurse") patientRecord.nurseCount += 1;

                db.TreatmentRecord.Add(treatmentRecord);
                db.SaveChanges();

                return Json(new { success = true, message = "Treatment record created successfully." });
            }

            return Json(new { success = false, message = "Invalid form data. Please correct and try again." });
        }


        // EDIT                 GET: AssignPatient/Edit/5
        public ActionResult Edit(string id)
        {
            // Fetch the logged-in user's data
            var userId = User.Identity.Name; // Adjust this based on how you get the user ID
            var currentUser = db.iCareUser.FirstOrDefault(u => u.ID == userId);
            var currentWorker = db.iCareWorker.FirstOrDefault(w => w.ID == userId);

            // Store the user and worker information in the ViewBag for use in the layout
            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentWorker = currentWorker;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TreatmentRecord treatmentRecord = db.TreatmentRecord.Find(id);
            if (treatmentRecord == null)
            {
                return HttpNotFound();
            }

            ViewBag.workerID = treatmentRecord.workerID;   // pass worker ID
            ViewBag.DrugsList = new SelectList(db.DrugsManagementSystem, "drugID", "drugName");
            ViewBag.docID = treatmentRecord.docID;   // pass document ID

            return View(treatmentRecord);
        }

        // POST EDIT            POST: AssignPatient/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "treatmentID,treatmentDate,description,patientID,workerID,drugID,docID")] TreatmentRecord treatmentRecord)
        {
            if (ModelState.IsValid)
            {
                var originalTreatmentRecord = db.TreatmentRecord.FirstOrDefault(tr => tr.treatmentID == treatmentRecord.treatmentID);
                
                // save unmodifiable values:
                treatmentRecord.treatmentID = originalTreatmentRecord.treatmentID;
                treatmentRecord.patientID = originalTreatmentRecord.patientID;
                treatmentRecord.workerID = originalTreatmentRecord.workerID;
                treatmentRecord.docID = originalTreatmentRecord.docID;

                // save new changes:
                originalTreatmentRecord.description = treatmentRecord.description;
                originalTreatmentRecord.treatmentDate = treatmentRecord.treatmentDate;
                originalTreatmentRecord.drugID = treatmentRecord.drugID;

                db.SaveChanges();
                return RedirectToAction("DocList", "DisplayPalette");
            }

            ViewBag.DrugsList = new SelectList(db.DrugsManagementSystem, "drugID", "drugName");
            return View(treatmentRecord);
        }


        // DELETE               GET: AssignPatient/Delete/5
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

        // POST DELETE          POST: AssignPatient/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            TreatmentRecord treatmentRecord = db.TreatmentRecord.Find(id);
            db.TreatmentRecord.Remove(treatmentRecord);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        // DISPOSE
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
