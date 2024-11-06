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
            var documentMetadata = db.DocumentMetadata.Include(d => d.iCareWorker).Include(d => d.ModificationHistory);
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
        public ActionResult Create()
        {
            ViewBag.userID = new SelectList(db.iCareWorker, "ID", "ID");
            ViewBag.docID = new SelectList(db.ModificationHistory, "ID", "description");
            var docTypes = db.DocumentMetadata.Select(d => d.docType).Distinct().ToList();
            ViewBag.docType = new SelectList(db.DocumentMetadata, "docType", "docType");

            TempData["DocType"] = docType; // Store the document type in TempData
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
            if (ModelState.IsValid)
            {
                db.DocumentMetadata.Add(documentMetadata);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

                // Redirect based on the document type
                if (docType == "PATIENT")
                {
                    return RedirectToAction("Create", "ManagePatient", new { id = documentMetadata.docID });
                }
                else if (docType == "TREATMENT")
                {
                    return RedirectToAction("Create", "ManageTreatment", new { id = documentMetadata.docID });
                }
                else if (docType == "UPLOADS")
                {
                    return RedirectToAction("Create", "ImportImage", new { id = documentMetadata.docID });
                }
                else
                {
                    // Handle the case where the docType does not match any known type
                    return RedirectToAction("Error", "Home"); // Or any other fallback action
                }
            }
            ViewBag.userID = new SelectList(db.iCareWorker, "ID", "ID", documentMetadata.userID);
            ViewBag.docID = new SelectList(db.ModificationHistory, "ID", "description", documentMetadata.docID);
            //ViewBag.DocType = docType;
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
