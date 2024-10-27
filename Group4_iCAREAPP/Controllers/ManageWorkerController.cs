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
    public class ManageWorkerController : Controller
    {
        private Group4_iCAREDBEntities db = new Group4_iCAREDBEntities();

        // GET: ManageWorker
        public ActionResult Index()
        {
            var iCareWorker = db.iCareWorker.Include(i => i.iCareAdmin).Include(i => i.UserRole);
            return View(iCareWorker.ToList());
        }

        // GET: ManageWorker/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            iCareWorker iCareWorker = db.iCareWorker.Find(id);
            if (iCareWorker == null)
            {
                return HttpNotFound();
            }
            return View(iCareWorker);
        }

        // GET: ManageWorker/Create
        public ActionResult Create()
        {
            ViewBag.ID = new SelectList(db.iCareUser, "ID", "ID"); // Populate dropdown with iCareUser IDs
            ViewBag.creator = new SelectList(db.iCareAdmin, "ID", "ID");
            ViewBag.userPermission = new SelectList(db.UserRole, "roleID", "roleName");
            ViewBag.profession = new SelectList(new List<string> { "Doctor", "Nurse" });


            return View();
        }

        // POST: ManageWorker/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,profession,creator,userPermission")] iCareWorker iCareWorker)
        {
            if (ModelState.IsValid)
            {
                db.iCareWorker.Add(iCareWorker);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID = new SelectList(db.iCareUser, "ID", "ID", iCareWorker.ID); // Repopulate dropdown on error
            ViewBag.creator = new SelectList(db.iCareAdmin, "ID", "ID", iCareWorker.creator);
            ViewBag.userPermission = new SelectList(db.UserRole, "roleID", "roleName", iCareWorker.userPermission);
            ViewBag.profession = new SelectList(new List<string> { "Doctor", "Nurse" }, iCareWorker.profession); // Repopulate profession dropdown

            return View(iCareWorker);
        }

        // GET: ManageWorker/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            iCareWorker iCareWorker = db.iCareWorker.Find(id);
            if (iCareWorker == null)
            {
                return HttpNotFound();
            }
            ViewBag.creator = new SelectList(db.iCareAdmin, "ID", "ID", iCareWorker.creator);
            ViewBag.userPermission = new SelectList(db.UserRole, "roleID", "roleName", iCareWorker.userPermission);
            ViewBag.profession = new SelectList(new List<string> { "Doctor", "Nurse" }, iCareWorker.profession);
            return View(iCareWorker);
        }

        // POST: ManageWorker/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,profession,creator,userPermission")] iCareWorker iCareWorker)
        {
            if (ModelState.IsValid)
            {
                db.Entry(iCareWorker).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.creator = new SelectList(db.iCareAdmin, "ID", "ID", iCareWorker.creator);
            ViewBag.userPermission = new SelectList(db.UserRole, "roleID", "roleName", iCareWorker.userPermission);
            ViewBag.profession = new SelectList(new List<string> { "Doctor", "Nurse" }, iCareWorker.profession);
            return View(iCareWorker);
        }

        // GET: ManageWorker/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            iCareWorker iCareWorker = db.iCareWorker.Find(id);
            if (iCareWorker == null)
            {
                return HttpNotFound();
            }
            return View(iCareWorker);
        }

        // POST: ManageWorker/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            iCareWorker iCareWorker = db.iCareWorker.Find(id);
            db.iCareWorker.Remove(iCareWorker);
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
