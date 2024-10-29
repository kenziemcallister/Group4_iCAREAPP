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
    public class DisplayMyBoardController : Controller
    {
        private Group4_iCAREDBEntities db = new Group4_iCAREDBEntities();

        // GET: DisplayMyBoard
        public ActionResult Index()
        {
            //ViewBag.UserName = User.Identity.Name;
            var patientRecord = db.PatientRecord.Include(p => p.DocumentMetadata).Include(p => p.GeoCodes).Include(p => p.iCareWorker).Include(p => p.ModificationHistory);
            var userId = User.Identity.Name; // Replace this with the correct way to get the user ID

            // Pass the user ID to the view for the message to be displayed at the top of the index of displayMyBoard
            ViewBag.UserId = userId;

            // Fetch the user to get the first name
            var user = db.iCareUser.FirstOrDefault(u => u.ID == userId);
            if (user != null)
            {
                Session["FirstName"] = user.name; // Assuming 'name' is the first name of the user
            }

            //need to get the patients related to the userId here:
            // Query TreatmentRecord to find patients assigned to the logged-in iCareWorker
            var assignedPatients = db.TreatmentRecord
                .Where(tr => tr.treatedBy == userId)
                .Select(tr => tr.PatientRecord) // Assuming `TreatmentRecord` has a navigation property to `PatientRecord`
                .Include(pr => pr.DocumentMetadata)
                .Include(pr => pr.GeoCodes)
                .Include(pr => pr.iCareWorker)
                .Include(pr => pr.ModificationHistory)
                .ToList();

            return View(assignedPatients);

            //return View(patientRecord.ToList());
        }

        // GET: DisplayMyBoard/Details/5
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

        // GET: DisplayMyBoard/Create
        public ActionResult Create()
        {
            ViewBag.docID = new SelectList(db.DocumentMetadata, "docID", "userID");
            ViewBag.geographicalUnit = new SelectList(db.GeoCodes, "ID", "description");
            ViewBag.treatedBy = new SelectList(db.iCareWorker, "ID", "profession");
            ViewBag.modifierID = new SelectList(db.ModificationHistory, "ID", "description");
            return View();
        }

        // POST: DisplayMyBoard/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,name,address,dateOfBirth,weight,height,bloodGroup,bedID,treatmentArea,geographicalUnit,treatedBy,docID,modifierID")] PatientRecord patientRecord)
        {
            if (ModelState.IsValid)
            {
                db.PatientRecord.Add(patientRecord);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.docID = new SelectList(db.DocumentMetadata, "docID", "userID", patientRecord.docID);
            ViewBag.geographicalUnit = new SelectList(db.GeoCodes, "ID", "description", patientRecord.geographicalUnit);
            ViewBag.treatedBy = new SelectList(db.iCareWorker, "ID", "profession", patientRecord.treatedBy);
            ViewBag.modifierID = new SelectList(db.ModificationHistory, "ID", "description", patientRecord.modifierID);
            return View(patientRecord);
        }

        // GET: DisplayMyBoard/Edit/5
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

        // POST: DisplayMyBoard/Edit/5
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

        // GET: DisplayMyBoard/Delete/5
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

        // POST: DisplayMyBoard/Delete/5
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
