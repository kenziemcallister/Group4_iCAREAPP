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
    public class ManageDocumentController : Controller
    {
        private Group4_iCAREDBEntities db = new Group4_iCAREDBEntities();

        // GET: ManageDocument
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

            foreach (var document in documentMetadata)
            {
                if (document.dateOfCreation.HasValue)
                {
                    document.dateOfCreation = document.dateOfCreation.Value.Date; // Remove time portion
                }

                var creator = db.iCareUser.FirstOrDefault(u => u.ID == document.userID);
                if (creator != null)
                {
                    document.Author = creator.name;
                }
            }

            return View(documentMetadata.ToList());
        }

        // GET: ManageDocument/Details/5
        public ActionResult Details(string id)
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

        // GET: ManageDocument/Create
        public ActionResult Create(string docType)
        {
            // Fetch the logged-in user's data
            var userId = User.Identity.Name; // Adjust this based on how you get the user ID
            var currentUser = db.iCareUser.FirstOrDefault(u => u.ID == userId);
            var currentWorker = db.iCareWorker.FirstOrDefault(w => w.ID == userId);

            // Store the user and worker information in the ViewBag for use in the layout
            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentWorker = currentWorker;


            ViewBag.UserID = new SelectList(db.iCareWorker, "ID", "ID"); 
            ViewBag.docID = new SelectList(db.ModificationHistory, "ID", "description");
            ViewBag.DocType = docType;

            var model = new DocumentMetadata(); // Create your metadata model
            return View(model);
        }

        // POST: ManageDocument/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "docID,userID,docName,dateOfCreation,versions,docType")] DocumentMetadata documentMetadata, string docType)
        {
            // Ensure docID isn't already taken:
            bool docIDExists = db.DocumentMetadata.Any(d => d.docID == documentMetadata.docID);
            if (docIDExists)
            {
                ModelState.AddModelError("docID", "The document ID has already been taken.");
            }

            if (ModelState.IsValid)
            {
                documentMetadata.docType = docType;

                db.DocumentMetadata.Add(documentMetadata);
                db.SaveChanges();

                // Redirect based on the document type
                if (docType == "PATIENT")
                {
                    return RedirectToAction("Create", "ManagePatient", new { docID = documentMetadata.docID });
                }
                else if (docType == "TREATMENT")
                {
                    return RedirectToAction("Create", "ManageTreatment", new { docID = documentMetadata.docID });
                }
                else if (docType == "UPLOADS")
                {
                    return RedirectToAction("Create", "ImportImage", new { docID = documentMetadata.docID });
                }
            }
            ViewBag.userID = new SelectList(db.iCareWorker, "ID", "ID", documentMetadata.userID);
            ViewBag.docID = new SelectList(db.ModificationHistory, "ID", "description", documentMetadata.docID);
            return View(documentMetadata);
        }

        // GET: ManageDocument/Edit/5
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
            ViewBag.userID = new SelectList(db.iCareWorker, "ID", "profession", documentMetadata.userID);
            ViewBag.docID = new SelectList(db.ModificationHistory, "ID", "description", documentMetadata.docID);
            return View(documentMetadata);
        }

        // POST: ManageDocument/Edit/5
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
            ViewBag.userID = new SelectList(db.iCareWorker, "ID", "profession", documentMetadata.userID);
            ViewBag.docID = new SelectList(db.ModificationHistory, "ID", "description", documentMetadata.docID);
            return View(documentMetadata);
        }

        // GET: ManageDocument/Delete/5
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

        // POST: ManageDocument/Delete/5
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
