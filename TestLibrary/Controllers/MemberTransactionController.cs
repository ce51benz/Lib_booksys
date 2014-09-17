﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using TestLibrary.Models;
using TestLibrary.DataAccess;
namespace TestLibrary.Controllers
{
    public class MemberTransactionController : Controller
    {
        LibraryContext db = new LibraryContext();

        [Authorize]
        public ActionResult Renew(int id)
        {
            Session["LoginUser"] = HttpContext.User.Identity.Name;
            if (HttpContext.User.Identity.Name.ToString().Substring(0, 2) != "M_")
                return RedirectToAction("Index", "Manage");

            BorrowEntry renewentry = db.BorrowList.SingleOrDefault(target => target.ID == id &&
                                        target.ReturnDate == null);
            if (renewentry == null)
            {
                TempData["Notification"] = "Invalid renew book id.";
                return RedirectToAction("Index", "Member");
            }

            if (renewentry.Borrower.UserName != Session["LoginUser"].ToString().Substring(2))
            {
                TempData["Notification"] = "Invalid renew operation.";
                return RedirectToAction("Index", "Member");
            }

            if (renewentry.RenewCount == 3)
            {
                TempData["Notification"] = "Your renew of book ID." + renewentry.BorrowBook.BookID + " is exceed maximum!";
                return RedirectToAction("Index", "Member");
            }
            return View(renewentry);
        }


        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Renew(BorrowEntry renewentry, string answer)
        {
            if (ModelState.IsValid && answer == "Yes")
            {
                if (db.RequestList.ToList().LastOrDefault(target => target.BookID == renewentry.BookID && target.ExpireDate == null) != null)
                {
                    TempData["Notification"] = "This book is ON HOLD.";
                }
                else
                {
                    renewentry.DueDate = DateTime.Now.Date.AddDays(7);
                    renewentry.RenewCount++;
                    db.Entry(renewentry).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Notification"] = "Renew successful!";
                }
            }
            return RedirectToAction("Index", "Member");
        }


        [Authorize]
        public ActionResult Request()
        {
            Session["LoginUser"] = HttpContext.User.Identity.Name;
            if (HttpContext.User.Identity.Name.ToString().Substring(0, 2) != "M_")
                return RedirectToAction("Index", "Manage");
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Request(RequestEntry newentry)
        {
            if (ModelState.IsValid)
            {
                Book booktorequest;
                if ((booktorequest = db.Books.Find(newentry.BookID)) == null)
                {
                    TempData["Notification"] = "No book with prefer ID exists.";
                    return View();
                }
                if (booktorequest.BookStatus != Status.Borrowed && booktorequest.BookStatus != Status.Reserved)
                {
                    TempData["Notification"] = "Can't request this book due to it is "
                        + booktorequest.BookStatus.ToString() + ".";
                    return View();
                }

                if (db.RequestList.ToList().LastOrDefault(target => target.BookID == booktorequest.BookID
                                && target.ExpireDate == null) != null || booktorequest.BookStatus == Status.Reserved)
                {
                    TempData["Notification"] = "This book is already requested.";
                    return View();
                }

                Member request_member = db.Members.Where(target => target.UserName ==
                                                HttpContext.User.Identity.Name.ToString().Substring(2)).Single();

                if (db.BorrowList.ToList().LastOrDefault(target => target.BookID == newentry.BookID &&
                                                        target.Borrower == request_member && target.ReturnDate == null) != null)
                {
                    TempData["Notification"] = "Can't request your current borrowed book.";
                    return View();
                }

                newentry.UserID = request_member.UserID;
                newentry.RequestDate = DateTime.Now;
                db.RequestList.Add(newentry);
                db.SaveChanges();
                TempData["Notification"] = "Request book successfully.";
                return RedirectToAction("Index", "Member");
            }
            else
                return View();
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}