using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Group4_iCAREAPP.Models;
//using Microsoft.AspNet.Identity;

namespace Group4_iCAREAPP.Controllers
{
    public class DisplayPaletteController : Controller
    {
        private Group4_iCAREDBEntities db = new Group4_iCAREDBEntities();

        // GET: DisplayPalette
        public ActionResult Index()
        {
            // Fetch the logged-in user's data
            var userId = User.Identity.Name; // Adjust this based on how you get the user ID
            var currentUser = db.iCareUser.FirstOrDefault(u => u.ID == userId);
            var currentWorker = db.iCareWorker.FirstOrDefault(w => w.ID == userId);

            // Store the user and worker information in the ViewBag for use in the layout
            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentWorker = currentWorker;

            var documentMetadata = db.DocumentMetadata.Include(d => d.iCareWorker).Include(d => d.ModificationHistory);
            return View(documentMetadata.ToList());
        }

        // GET: DisplayPalette/DocList/5
        public ActionResult DocList(string id)
        {
            // Fetch the logged-in user's data
            var userId = User.Identity.Name; // Adjust this based on how you get the user ID
            var currentUser = db.iCareUser.FirstOrDefault(u => u.ID == userId);
            var currentWorker = db.iCareWorker.FirstOrDefault(w => w.ID == userId);

            // Store the user and worker information in the ViewBag for use in the layout
            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentWorker = currentWorker;

            ViewBag.DocTypeList = new SelectList(db.DocumentMetadata.Select(d => d.docType).Distinct().ToList());
            var documentMetadata = db.DocumentMetadata.Include(d => d.iCareWorker).Include(d => d.ModificationHistory);
            return View(documentMetadata.ToList());
        }

        // Custom action to redirect the "View Details" buttons to the correct type of document
        public ActionResult RedirectViewDetails(string docID)
        {
            if (string.IsNullOrEmpty(docID))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Get document metadata using docID
            var documentMetadata = db.DocumentMetadata.FirstOrDefault(d => d.docID == docID);
            if (documentMetadata == null)
            {
                return HttpNotFound();
            }

            // Check the docType, find the corresponding record, and redirect accordingly
            if (documentMetadata.docType == "PATIENT")
            {
                var patientRecord = db.PatientRecord.FirstOrDefault(pr => pr.docID == docID);
                return RedirectToAction("Details", "ManagePatient", new { id = patientRecord.ID });
            }
            else if (documentMetadata.docType == "TREATMENT")
            {
                var treatmentRecord = db.TreatmentRecord.FirstOrDefault(tr => tr.docID == docID);
                return RedirectToAction("Details", "AssignPatient", new { id = treatmentRecord.treatmentID });
            }
            else if (documentMetadata.docType == "UPLOADS")
            {
                return RedirectToAction("Details", "ImportImage", new { id = docID });
            }
            else
            {
                return RedirectToAction("Details", "ManageDocument", new { id = docID });
            }
        }

        // Custom action to redirect the "Modify" buttons to the correct type of document
        public ActionResult RedirectModify(string docID)
        {
            if (string.IsNullOrEmpty(docID))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Get document metadata using docID
            var documentMetadata = db.DocumentMetadata.FirstOrDefault(d => d.docID == docID);
            if (documentMetadata == null)
            {
                return HttpNotFound();
            }

            // Check the docType, find the corresponding record, and redirect accordingly
            if (documentMetadata.docType == "PATIENT")
            {
                var patientRecord = db.PatientRecord.FirstOrDefault(pr => pr.docID == docID);
                return RedirectToAction("Edit", "ManagePatient", new { id = patientRecord.ID });
            }
            else if (documentMetadata.docType == "TREATMENT")
            {
                var treatmentRecord = db.TreatmentRecord.FirstOrDefault(tr => tr.docID == docID);
                return RedirectToAction("Edit", "AssignPatient", new { id = treatmentRecord.treatmentID });
            }
            else if (documentMetadata.docType == "UPLOADS")
            {
                return RedirectToAction("Edit", "ImportImage", new { id = docID });
            }
            else
            {
                return RedirectToAction("Edit", "ManageDocument", new { id = docID });
            }

        }

        // GET: DisplayPalette/ChooseDoc
        public ActionResult ChooseDoc()
        {
            // Fetch the logged-in user's data
            var userId = User.Identity.Name; // Adjust this based on how you get the user ID
            var currentUser = db.iCareUser.FirstOrDefault(u => u.ID == userId);
            var currentWorker = db.iCareWorker.FirstOrDefault(w => w.ID == userId);

            // Store the user and worker information in the ViewBag for use in the layout
            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentWorker = currentWorker;

            return View();
        }


        // GET: DisplayPalette/Create
        public ActionResult Create()
        {
            ViewBag.userID = new SelectList(db.iCareWorker, "ID", "profession");
            ViewBag.docID = new SelectList(db.ModificationHistory, "ID", "description");
            return View();
        }

        // POST: DisplayPalette/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "docID,userID,docName,dateOfCreation,versions")] DocumentMetadata documentMetadata)
        {
            if (ModelState.IsValid)
            {
                // Automatically set userID to the doc:
                //documentMetadata.userID = User.Identity.GetUserId();

                db.DocumentMetadata.Add(documentMetadata);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.userID = new SelectList(db.iCareWorker, "ID", "profession", documentMetadata.userID);
            ViewBag.docID = new SelectList(db.ModificationHistory, "ID", "description", documentMetadata.docID);
            return View(documentMetadata);
        }

        // GET: DisplayPalette/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentMetadata documentMetadata = db.DocumentMetadata.Find(id);
            if (documentMetadata == null)
            {
                return HttpNotFound();
            }
            ViewBag.userID = new SelectList(db.iCareWorker, "ID", "ID", documentMetadata.userID);
            ViewBag.docID = new SelectList(db.ModificationHistory, "ID", "description", documentMetadata.docID);
            return View(documentMetadata);
        }

        // Action for AJAX filtering by docType
        public ActionResult FilterByDocType(string docType)
        {
            // Fetch all documents
            var documents = db.DocumentMetadata.AsQueryable();

            // Apply filter if docType is provided
            if (!string.IsNullOrEmpty(docType) && docType != "Select All")
            {
                documents = documents.Where(d => d.docType == docType);
            }

            // Return the filtered documents to the view
            return PartialView("_DocumentList", documents.ToList());
        }

        // POST: DisplayPalette/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "docID,userID,docName,dateOfCreation,versions")] DocumentMetadata documentMetadata)
        {
            if (ModelState.IsValid)
            {
                db.Entry(documentMetadata).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.userID = new SelectList(db.iCareWorker, "ID", "ID", documentMetadata.userID);
            ViewBag.docID = new SelectList(db.ModificationHistory, "ID", "description", documentMetadata.docID);
            return View(documentMetadata);
        }

        // GET: DisplayPalette/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentMetadata documentMetadata = db.DocumentMetadata.Find(id);
            if (documentMetadata == null)
            {
                return HttpNotFound();
            }
            return View(documentMetadata);
        }

        // POST: DisplayPalette/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            DocumentMetadata documentMetadata = db.DocumentMetadata.Find(id);
            db.DocumentMetadata.Remove(documentMetadata);
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
