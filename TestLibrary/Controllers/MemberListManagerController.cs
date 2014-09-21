﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Data.Entity;
using TestLibrary.Models;
using TestLibrary.DataAccess;

namespace TestLibrary.Controllers
{
    public class MemberListManagerController : Controller
    {
        LibraryContext db = new LibraryContext();
        [Authorize]
        public ActionResult Index()
        {
            Session["LoginUser"] = HttpContext.User.Identity.Name;
            if (HttpContext.User.Identity.Name.ToString().Substring(0, 2) != "A_")
                return RedirectToAction("Index", "Account");
            return View(db.Members.ToList());
        }

        [Authorize]
        public ActionResult View([DefaultValue(0)]int id)
        {
            Session["LoginUser"] = HttpContext.User.Identity.Name;
            if (HttpContext.User.Identity.Name.ToString().Substring(0, 2) != "A_")
                return RedirectToAction("Index", "Account");
            Member target = db.Members.Find(id);
            if (target != null)
                return View(target);
            else
            {
                TempData["Notification"] = "Please specified correct Member ID";
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        public ActionResult Delete([DefaultValue(0)]int id)
        {
            Session["LoginUser"] = HttpContext.User.Identity.Name;
            if (HttpContext.User.Identity.Name.ToString().Substring(0, 2) != "A_")
                return RedirectToAction("Index", "Account");
            Member target = db.Members.Find(id);
            if (target != null)
                return View(target);
            else
            {
                TempData["Notification"] = "Please specified correct Member ID";
                return RedirectToAction("Index");
            }
        }


        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Delete(Member target,string answer)
        {
            if (ModelState.IsValid && answer == "Yes")
            {
                //1.Check whether member has remain book if yes do not delete.
                //2.Delete related borrowlist.
                List<BorrowEntry> BorrowListToDelete = db.BorrowList.Where(entry => entry.UserID == target.UserID).ToList();
                if (BorrowListToDelete.Count != 0)
                {
                    if (BorrowListToDelete.Where(entry => entry.ReturnDate == null).ToList().Count > 0)
                    {
                        TempData["Notification"] = "Can't delete " + target.UserName + " due to this member has book that not returned.";
                        return RedirectToAction("Index");
                    }
                    db.BorrowList.RemoveRange(BorrowListToDelete);
                }

                //3.Check whether there is reserved book of that member in which turn status of that book to Available.
                List<RequestEntry> RequestListToDelete = db.RequestList.Where(entry => entry.UserID == target.UserID).ToList();
                if (RequestListToDelete.Count != 0)
                {
                    foreach (var item in RequestListToDelete.Where(entry => entry.ExpireDate != null).ToList())
                    {
                            item.RequestBook.BookStatus = Status.Available;
                    }
                    //4.Delete related requestlist.
                    db.RequestList.RemoveRange(RequestListToDelete);
                }

                //5.Delete member.
                db.Entry(target).State = EntityState.Deleted;
                db.SaveChanges();
                TempData["Notification"] = "Delete member " + target.UserName + " successfully.";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}