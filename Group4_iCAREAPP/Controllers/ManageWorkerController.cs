using System.Linq;
using System.Web.Mvc;
using Group4_iCAREAPP.Models; 
using System.Data.Entity;
using System.Net;

namespace Group4_iCAREAPP.Controllers // Replace with your actual namespace
{
    public class ManageWorkerController : Controller
    {
        private Group4_iCAREDBEntities db = new Group4_iCAREDBEntities(); // DbContext for Entity Framework

        // GET: ManageWorker
        public ActionResult Index()
        {
            // Retrieve all workers along with their roles and geographical units
            var workers = db.iCareWorker.Include(w => w.UserRole).Include(w => w.GeoCodes).ToList();
            return View(workers);
        }

        // GET: ManageWorker/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            iCareWorker worker = db.iCareWorker.Include(w => w.UserRole).Include(w => w.GeoCodes)
                                                .FirstOrDefault(w => w.ID == id);
            if (worker == null)
                return HttpNotFound();

            return View(worker);
        }

        // GET: ManageWorker/Create
        public ActionResult Create()
        {
            ViewBag.UserID = new SelectList(db.iCareUser, "ID", "ID"); // Dropdown for existing iCareUsers
            ViewBag.UserPermission = new SelectList(db.UserRole, "roleID", "roleName"); // Dropdown for roles
            ViewBag.GeographicalUnit = new SelectList(db.GeoCodes, "ID", "description"); // Dropdown for geographical units

            // Hardcoded list for profession
            ViewBag.Profession = new SelectList(new[] { "Doctor", "Nurse" });

            return View();
        }


        // POST: ManageWorker/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID, profession, creator, userPermission, geographicalUnit")] iCareWorker worker)
        {
            if (ModelState.IsValid)
            {
                db.iCareWorker.Add(worker);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(db.iCareUser, "ID", "ID", worker.ID);
            ViewBag.UserPermission = new SelectList(db.UserRole, "roleID", "roleName", worker.userPermission);
            ViewBag.GeographicalUnit = new SelectList(db.GeoCodes, "ID", "description", worker.geographicalUnit);
            return View(worker);
        }

        // GET: ManageWorker/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            iCareWorker worker = db.iCareWorker.Find(id);
            if (worker == null)
                return HttpNotFound();

            ViewBag.UserID = new SelectList(db.iCareUser, "ID", "ID", worker.ID);
            ViewBag.UserPermission = new SelectList(db.UserRole, "roleID", "roleName", worker.userPermission);
            ViewBag.GeographicalUnit = new SelectList(db.GeoCodes, "ID", "description", worker.geographicalUnit);
            return View(worker);
        }

        // POST: ManageWorker/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID, profession, creator, userPermission, geographicalUnit")] iCareWorker worker)
        {
            if (ModelState.IsValid)
            {
                db.Entry(worker).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(db.iCareUser, "ID", "ID", worker.ID);
            ViewBag.UserPermission = new SelectList(db.UserRole, "roleID", "roleName", worker.userPermission);
            ViewBag.GeographicalUnit = new SelectList(db.GeoCodes, "ID", "description", worker.geographicalUnit);
            return View(worker);
        }

        // GET: ManageWorker/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            iCareWorker worker = db.iCareWorker.Include(w => w.UserRole).Include(w => w.GeoCodes)
                                                .FirstOrDefault(w => w.ID == id);
            if (worker == null)
                return HttpNotFound();

            return View(worker);
        }

        // POST: ManageWorker/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            iCareWorker worker = db.iCareWorker.Find(id);
            db.iCareWorker.Remove(worker);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}

