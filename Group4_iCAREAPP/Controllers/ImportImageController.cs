using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Group4_iCAREAPP.Models;
using System.Diagnostics;

namespace Group4_iCAREAPP.Controllers
{
    public class ImportImageController : Controller
    {
        private Group4_iCAREDBEntities db = new Group4_iCAREDBEntities();

        // GET: ImportImage
        public ActionResult Index()
        {
            // Fetch the logged-in user's data
            var userId = User.Identity.Name; // Adjust this based on how you get the user ID
            var currentUser = db.iCareUser.FirstOrDefault(u => u.ID == userId);
            var currentWorker = db.iCareWorker.FirstOrDefault(w => w.ID == userId);

            // Store the user and worker information in the ViewBag for use in the layout
            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentWorker = currentWorker;


            var documentMetadata = db.DocumentMetadata
                .Include(d => d.iCareWorker)
                .Include(d => d.ModificationHistory)
                .ToList();

            // Map file paths for each document
            foreach (var doc in documentMetadata)
            {
                string filePath = Path.Combine(Server.MapPath("~/UploadedImages"), doc.docName);
                if (System.IO.File.Exists(filePath))
                {
                    // Set a property in your model or view model to store the URL for display
                    doc.FileUrl = Url.Content("~/UploadedImages/" + doc.docName);
                }
                else
                {
                    doc.FileUrl = null; // Handle case where file is missing
                }
            }

            return View(documentMetadata);
        }

        // GET : Details
        public ActionResult Details(string id)
        {
            // Fetch the logged-in user's ID
            var userId = User.Identity.Name;
            var currentUser = db.iCareUser.FirstOrDefault(u => u.ID == userId);
            var currentWorker = db.iCareWorker.FirstOrDefault(w => w.ID == userId);

            // user and worker information
            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentWorker = currentWorker;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DocumentMetadata documentMetadata = db.DocumentMetadata.Find(id);
            if (documentMetadata == null)
            {
                return HttpNotFound();
            }

            // Check if the file exists in the UploadedImages folder
            string filePath = Path.Combine(Server.MapPath("~/Content/UploadedImages"), documentMetadata.docName);
            ViewBag.FileExists = System.IO.File.Exists(filePath);
            ViewBag.FileUrl = Url.Content("~/Content/UploadedImages" + documentMetadata.docName); 

            return View(documentMetadata);
        }


        // GET: ImportImage/Create
        public ActionResult Create()
        {           
            // Fetch the logged-in user's ID
            var userId = User.Identity.Name;
            var currentUser = db.iCareUser.FirstOrDefault(u => u.ID == userId);
            var currentWorker = db.iCareWorker.FirstOrDefault(w => w.ID == userId);

            // user and worker information
            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentWorker = currentWorker;

            // Generate a unique 10-character ID for the document
            string docID = Guid.NewGuid().ToString("N").Substring(0, 5);

            // Initialize DocumentMetadata with pre-set docID and userID
            var documentMetadata = new DocumentMetadata
            {
                docID = docID,
                userID = userId // Set the userID to the logged-in user's ID
            };

            ViewBag.docType = new SelectList(new[] { "PATIENT", "TREATMENT", "UPLOADS" });

            return View(documentMetadata); // Pass initialized object to the view
        }


        // POST: ImportImage/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HttpPostedFileBase file, [Bind(Include = "docID,userID,docName,dateOfCreation,versions,docType")] DocumentMetadata documentMetadata, [Bind(Include = "dateOfModification,description")] ModificationHistory modificationHistory)
        {
            if (file != null && file.ContentLength > 0 && ModelState.IsValid)
            {
                // Save the file to a directory

                string folderPath = Server.MapPath("~/Content/UploadedImages");
                Directory.CreateDirectory(folderPath); // Creates the folder if it doesn’t exist
                string filePath = Path.Combine(folderPath, file.FileName);
                file.SaveAs(filePath);

                // Set Document Metadata properties
                documentMetadata.docID = Guid.NewGuid().ToString().Substring(0, 5); // Ensure it is 5 characters
                documentMetadata.docName = Path.GetFileName(file.FileName);
                documentMetadata.dateOfCreation = DateTime.Now;

                db.DocumentMetadata.Add(documentMetadata);
                db.SaveChanges();

                return RedirectToAction("Details", "DisplayPalette");
            }

            ViewBag.userID = new SelectList(db.iCareWorker, "ID", "ID", documentMetadata.userID);
            ViewBag.docType = new SelectList(new[] { "PATIENT", "TREATMENT", "UPLOADS" }, documentMetadata.docType); // Ensure the selected value is retained in case of validation failure

            return View(documentMetadata);
        }

        //GET : ImportImage/Download/5
        public ActionResult Download(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DocumentMetadata documentMetadata = db.DocumentMetadata.Find(id);
            if (documentMetadata == null)
            {
                return HttpNotFound();
            }

            // Set the file path
            string filePath = Path.Combine(Server.MapPath("~/UploadedImages"), documentMetadata.docName);

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound(); // Handle missing file
            }

            // Return the file as a download
            return File(filePath, MimeMapping.GetMimeMapping(documentMetadata.docName), documentMetadata.docName);
        }



        // GET: ImportImage/Edit/5
        public ActionResult Edit(string id)
        {
            // Fetch the logged-in user's ID
            var userId = User.Identity.Name;
            var currentUser = db.iCareUser.FirstOrDefault(u => u.ID == userId);
            var currentWorker = db.iCareWorker.FirstOrDefault(w => w.ID == userId);

            // user and worker information
            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentWorker = currentWorker;

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

        // POST: ImportImage/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "docID,userID,docName,dateOfCreation,versions,docType")] DocumentMetadata documentMetadata)
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

        // GET: ImportImage/Delete/5
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

        // POST: ImportImage/Delete/5
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
