﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using ParatabLib.Models;
using ParatabLib.DataAccess;
using ParatabLib.ViewModels;
namespace ParatabLib.Controllers
{
    //This class use to handle member list management.
    public class MemberListManagerController : Controller
    {
        LibraryRepository libRepo = new LibraryRepository();
        
        /* This method use to call index page of member list management with
         * parameterized of page and pageSize for paging member list then
         * return result as page to user.
         */ 
        [Authorize]
        [OutputCache(Duration = 0, NoStore = true)]
        public ActionResult Index(int page = 1,int pageSize = 10)
        {
            TempData["pageSize"] = pageSize;
            TempData["page"] = page;
            List<Member> memberList = libRepo.MemberRepo.List();
            PageList<Member> pglist = new PageList<Member>(memberList, page, pageSize);
            switch (pglist.Categorized())
            {
                case PageListResult.Ok: { return View(pglist); }
                case PageListResult.Empty:
                    {
                        TempData["ErrorNoti"] = "No member list to show.";
                        return View();
                    }
                default:
                    {
                        TempData["ErrorNoti"] = "Invalid list view parameter please refresh this page to try again.";
                        return View();
                    }
            }
        }

        /* This method use to get member detail base on id parameter
         * find and check whether there is member with desired id exists or not.
         * If yes return member detail in page,otherwise notify user to input correct member ID.
         */
        [Authorize]
        [OutputCache(Duration = 0, NoStore = true)]
        public ActionResult View([DefaultValue(0)]int id)
        {
            Member target = libRepo.MemberRepo.Find(id);
            if (target != null)
                return View(target);
            else
            {
                TempData["ErrorNoti"] = "Please specified correct Member ID";
                return RedirectToAction("Index");
            }
        }

        /* This method use to call confirmation delete member page for desired member
         * check the existence of desire member base on id parameter.
         * If yes return confirmation delete member page with desired member,
         * otherwise notify user to input correct member ID.
         */ 
        [Authorize]
        [OutputCache(Duration = 0, NoStore = true)]
        public ActionResult Delete([DefaultValue(0)]int id)
        {
            Member target = libRepo.MemberRepo.Find(id);
            if (target != null)
                return View(target);
            else
            {
                TempData["ErrorNoti"] = "Please specified correct Member ID";
                return RedirectToAction("Index");
            }
        }

        /* This method use to submit delete confirmation and remove member
         * instruction is in sub comment..
         */
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Delete(Member target)
        {
            if (ModelState.IsValid)
            {
                //1.Check whether member has remain book if yes do not delete and notify user that cannot delete.
                //2.Delete related borrowlist.
                List<BorrowEntry> BorrowListToDelete = libRepo.BorrowEntryRepo.ListWhere(entry => entry.UserID == target.UserID);
                if (BorrowListToDelete.Count != 0)
                {
                    if (BorrowListToDelete.Where(entry => entry.ReturnDate == null).ToList().Count > 0)
                    {
                        TempData["ErrorNoti"] = "Can't delete " + target.UserName + " due to this member has book that not returned.";
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
                        Book bookToUpdate = item.GetRequestBook(ref libRepo);
                             bookToUpdate.BookStatus = Status.Available;
                             libRepo.BookRepo.Update(bookToUpdate);
                    }
                    //4.Delete related requestlist.
                    libRepo.RequestEntryRepo.Remove(RequestListToDelete);
                }

                //5.Delete member.
                libRepo.MemberRepo.Remove(target);
                libRepo.Save();
                AuthenticateController.RemoveUser(target.UserName);
                TempData["SuccessNoti"] = "Delete member " + target.UserName + " successfully.";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        /* [Override method]
         * This method use to check that whether current user for current session is exist in system or not.
         * If not,call and pass by reference of current HTTPrequest in AuthenticateController.OnInvalidSession
         * to set appropiate page result.Moreover check that current user is librarian if not redirect user to
         * account index page.
         */
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
                if (AuthenticateController.IsUserValid(HttpContext.User.Identity.Name.Substring(2)))
                {
                    Session["LoginUser"] = HttpContext.User.Identity.Name;
                    if (HttpContext.User.Identity.Name.ToString().Substring(0, 2) != "A_")
                    {
                        filterContext.Result = RedirectToAction("Index", "Account");
                        return;
                    }
                }
                else
                {
                    AuthenticateController.OnInvalidSession(ref filterContext);
                    return;
                }
        }

        /* [Override method]
         * This method use to handle exception that may occur in system
         * for some specific exception Redirect user to another page and pretend that no error occur
         * for another exception throw it and use HTTP error 500 page to handle instead.
         */
        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;
            if (filterContext.Exception.GetType().Name == typeof(HttpAntiForgeryException).Name)
            {
                filterContext.Result = RedirectToAction("Index", "Account");
            }
            else if ((filterContext.Exception.GetType().Name ==
                    typeof(System.Data.Entity.Infrastructure.DbUpdateConcurrencyException).Name)
                    && filterContext.RouteData.Values["action"].ToString() == "Delete")
            {
                TempData["ErrorNoti"] = "The member that you want to delete is already deleted.";
                filterContext.Result = RedirectToAction("Index");
            }
            else
            {
                throw filterContext.Exception;
            }

        }
    }
}
