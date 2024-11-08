using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
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
            var userId = User.Identity.Name;

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
                try
                {
                    db.iCareUser.Add(iCareUser);
                    db.SaveChanges();
                    return RedirectToAction("Login");
                }
                catch (DbUpdateException ex)
                {
                    if(ex.InnerException?.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
                    {
                        ModelState.AddModelError("ID", "The username or password is taken.");
                        ModelState.AddModelError("userShadow", " ");
                    }
                }
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
            /*if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }*/
            iCareUser iCareUser = db.iCareUser.Find(id);
            /*if (iCareUser == null)
            {
                return HttpNotFound();
            }*/
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
                    // Retrieve the user's first name
                    var user = db.iCareUser.SingleOrDefault(u => u.ID == model.ID);

                    // Check if user is an admin:
                    var adminUser = db.iCareAdmin.SingleOrDefault(admin => admin.ID == model.ID);
                    // Check if user is an iCareWorker:
                    var worker = db.iCareWorker.SingleOrDefault(w => w.ID == model.ID);


                    if (user != null)
                    {
                        // Store first name in session
                        Session["FirstName"] = user.name;

                        // If the user is an admin, store that user role:
                        if (adminUser != null)
                        {
                            Session["Profession"] = "Admin";
                        }
                        // If the user is a worker, store their profession (Nurse or Doctor)
                        else if (worker != null)
                        {
                            if (worker.profession == "doctor")
                            {
                                Session["Profession"] = "Dr.";
                            }
                            else if (worker.profession == "nurse")
                            {
                                Session["Profession"] = "Nurse";
                            }
                        }
                        else
                        {
                            Session["Profession"] = "User";
                        }

                        TempData["LoginMessage"] = $"Login successful. Welcome to iCARE, {Session["Profession"]} {Session["FirstName"]}!"; //success message to display on the home page. see home page index html file for other part

                        FormsAuthentication.SetAuthCookie(model.ID, false);
                        return RedirectToAction("Index", "Home"); //if yes, redirect back to home
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid User ID or Password. Please try again."; //if not valid login, put an error and stay on the login page
                  
                }
            }
            return View(model);
        }

        //helper function for making sure the password is the right password for the user
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

        //Logout code below for controller
        public ActionResult Logout()
        {
            // Clear the authentication cookie and session
            FormsAuthentication.SignOut();
            Session.Clear();  // Clear all session data

            TempData["LogoutMessage"] = $"Logout successful."; 

            return RedirectToAction("Index", "Home");
        }

    }
}
