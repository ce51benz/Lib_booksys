﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Data.Entity;
using TestLibrary.Models;
using TestLibrary.DataAccess;
using TestLibrary.ViewModels;
namespace TestLibrary.Controllers
{
    public class MemberListManagerController : Controller
    {
        LibraryRepository libRepo = new LibraryRepository();
        [Authorize]
        public ActionResult Index(int page = 1,int pageSize = 10)
        {
            Session["LoginUser"] = HttpContext.User.Identity.Name;
            if (HttpContext.User.Identity.Name.ToString().Substring(0, 2) != "A_")
                return RedirectToAction("Index", "Account");
            TempData["pageSize"] = pageSize;
            TempData["page"] = page;
            List<Member> memberList = libRepo.MemberRepo.List();
            PageList<Member> pglist = new PageList<Member>(memberList, page, pageSize);
            switch (pglist.Categorized())
            {
                case PageListResult.Ok: { return View(pglist); }
                case PageListResult.Empty:
                    {
                        TempData["Notification"] = "No member list to show.";
                        return View();
                    }
                default:
                    {
                        TempData["Notification"] = "Invalid list view parameter please refresh this page to try again.";
                        return View();
                    }
            }
        }

        [Authorize]
        public ActionResult View([DefaultValue(0)]int id)
        {
            Session["LoginUser"] = HttpContext.User.Identity.Name;
            if (HttpContext.User.Identity.Name.ToString().Substring(0, 2) != "A_")
                return RedirectToAction("Index", "Account");
            Member target = libRepo.MemberRepo.Find(id);
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
            Member target = libRepo.MemberRepo.Find(id);
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
                List<BorrowEntry> BorrowListToDelete = libRepo.BorrowEntryRepo.ListWhere(entry => entry.UserID == target.UserID);
                if (BorrowListToDelete.Count != 0)
                {
                    if (BorrowListToDelete.Where(entry => entry.ReturnDate == null).ToList().Count > 0)
                    {
                        TempData["Notification"] = "Can't delete " + target.UserName + " due to this member has book that not returned.";
                        return RedirectToAction("Index");
                    }
                    libRepo.BorrowEntryRepo.Remove(BorrowListToDelete);
                }

                //3.Check whether there is reserved book of that member in which turn status of that book to Available.
                List<RequestEntry> RequestListToDelete = libRepo.RequestEntryRepo.ListWhere(entry => entry.UserID == target.UserID);
                if (RequestListToDelete.Count != 0)
                {
                    foreach (var item in RequestListToDelete.Where(entry => entry.ExpireDate != null).ToList())
                    {
                        Book bookToUpdate = item.GetRequestBook();
                             bookToUpdate.BookStatus = Status.Available;
                             libRepo.BookRepo.Update(bookToUpdate);
                    }
                    //4.Delete related requestlist.
                    libRepo.RequestEntryRepo.Remove(RequestListToDelete);
                }

                //5.Delete member.
                libRepo.MemberRepo.Remove(target);
                libRepo.Save();
                TempData["SuccessNoti"] = "Delete member " + target.UserName + " successfully.";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}
