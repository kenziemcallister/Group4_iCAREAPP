using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Group4_iCAREAPP.Models;

namespace Group4_iCAREAPP.Controllers
{
    public class UserAuthenticationController : Controller
    {
        private Group4_iCAREDBEntities db = new Group4_iCAREDBEntities();

        // GET: UserAuthentication
        public ActionResult Index()
        {
            var iCareUser = db.iCareUser.Include(i => i.iCareAdmin).Include(i => i.UserPassword);
            return View(iCareUser.ToList());
        }

        // GET: UserAuthentication/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            iCareUser iCareUser = db.iCareUser.Find(id);
            if (iCareUser == null)
            {
                return HttpNotFound();
            }
            return View(iCareUser);
        }

        // GET: UserAuthentication/Create
        public ActionResult Create()
        {
            ViewBag.ID = new SelectList(db.iCareAdmin, "ID", "ID");
            ViewBag.userShadow = new SelectList(db.UserPassword, "ID", "userName");
            return View();
        }

        // POST: UserAuthentication/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,name,userShadow")] iCareUser iCareUser)
        {
            if (ModelState.IsValid)
            {
                db.iCareUser.Add(iCareUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID = new SelectList(db.iCareAdmin, "ID", "ID", iCareUser.ID);
            ViewBag.userShadow = new SelectList(db.UserPassword, "ID", "userName", iCareUser.userShadow);
            return View(iCareUser);
        }

        // GET: UserAuthentication/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            iCareUser iCareUser = db.iCareUser.Find(id);
            if (iCareUser == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID = new SelectList(db.iCareAdmin, "ID", "ID", iCareUser.ID);
            ViewBag.userShadow = new SelectList(db.UserPassword, "ID", "userName", iCareUser.userShadow);
            return View(iCareUser);
        }

        // POST: UserAuthentication/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,name,userShadow")] iCareUser iCareUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(iCareUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ID = new SelectList(db.iCareAdmin, "ID", "ID", iCareUser.ID);
            ViewBag.userShadow = new SelectList(db.UserPassword, "ID", "userName", iCareUser.userShadow);
            return View(iCareUser);
        }

        // GET: UserAuthentication/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            iCareUser iCareUser = db.iCareUser.Find(id);
            if (iCareUser == null)
            {
                return HttpNotFound();
            }
            return View(iCareUser);
        }

        // POST: UserAuthentication/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            iCareUser iCareUser = db.iCareUser.Find(id);
            db.iCareUser.Remove(iCareUser);
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



        //Login implmentation below

        // GET: UserAuthentication/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: UserAuthentication/Login
        [HttpPost, ActionName("Login")]
        [ValidateAntiForgeryToken]

        public ActionResult Login(iCareUser model)
        {
            if (ModelState.IsValid)
            {
                bool isAuthenticated = AuthenticateUser(model.ID, model.userShadow); //function to see if the id and password are valid for a user

                if (isAuthenticated)
                {
                    FormsAuthentication.SetAuthCookie(model.ID, false);
                    return RedirectToAction("Index", "Home"); //if yes, redirect back to home

                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid User ID or Password. Please try again."; //if not valid login, put an error and stay on the login page
                  
                }
            }
            return View(model);
        }

        private bool AuthenticateUser(string id, string password)
        {

            var user = db.iCareUser.SingleOrDefault(u => u.ID == id);
            if (user != null && user.userShadow == password)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

    }
}
