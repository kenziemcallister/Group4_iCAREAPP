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


        // GET: ImportImage/Details/5
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

        // GET: ImportImage/Create
        public ActionResult Create()
        {
            // Generate a unique 10-character ID
            string docID = Guid.NewGuid().ToString("N").Substring(0, 10);

            // Initialize DocumentMetadata with pre-set docID
            var documentMetadata = new DocumentMetadata
            {
                docID = docID
            };

            var users = db.iCareUser.ToList();
            ViewBag.userID = new SelectList(users, "ID", "ID");
            return View(documentMetadata); // Pass initialized object to the view
        }

        // POST: ImportImage/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HttpPostedFileBase file, [Bind(Include = "docID,userID,docName,dateOfCreation,versions")] DocumentMetadata documentMetadata, [Bind(Include = "dateOfModification,description")] ModificationHistory modificationHistory)
        {
            if (file != null && file.ContentLength > 0 && ModelState.IsValid)
            {
                // Save the file to a directory

                string folderPath = Server.MapPath("~/Content/UploadedImages");
                Directory.CreateDirectory(folderPath); // Creates the folder if it doesn’t exist
                string filePath = Path.Combine(folderPath, file.FileName);
                file.SaveAs(filePath);

                // Set Document Metadata properties
                documentMetadata.docID = Guid.NewGuid().ToString().Substring(0, 10); // Ensure it is 10 characters
                documentMetadata.docName = Path.GetFileName(file.FileName);
                documentMetadata.dateOfCreation = DateTime.Now;

                // Set Modification History properties
                modificationHistory.ID = documentMetadata.docID; // Match IDs
                modificationHistory.dateOfModification = DateTime.Now; // or use the posted value if it should come from the form

                // Add to the database
                db.ModificationHistory.Add(modificationHistory);

                // Populate versions from the submitted modification history date
                documentMetadata.versions = modificationHistory.dateOfModification?.ToString("yyyy-MM-dd"); // Set the versions field as a formatted string

                db.DocumentMetadata.Add(documentMetadata);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.userID = new SelectList(db.iCareWorker, "ID", "profession", documentMetadata.userID);
            return View(documentMetadata);
        }



        // GET: ImportImage/Edit/5
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

        // POST: ImportImage/Edit/5
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
