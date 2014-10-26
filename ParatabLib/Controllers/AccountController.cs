﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using System.Data.Entity;
using ParatabLib.Models;
using ParatabLib.DataAccess;
using ParatabLib.ViewModels;
namespace ParatabLib.Controllers
{
    public class AccountController : Controller
    {
        LibraryRepository libRepo = new LibraryRepository();

        [Authorize]
        public ActionResult Index()
        {
            string userName;
            userName = HttpContext.User.Identity.Name;
            userName = userName.Substring(2);
            Person CurrentLoginUser = libRepo.LibrarianRepo.ListWhere(target => target.UserName == userName).SingleOrDefault();
            if (CurrentLoginUser == null)
                CurrentLoginUser = libRepo.MemberRepo.ListWhere(target => target.UserName == userName).SingleOrDefault();
            if (CurrentLoginUser.Identify().StartsWith("Member"))
            {
                List<BorrowEntry> checkList = libRepo.BorrowEntryRepo.ListWhere(entry => entry.UserID == CurrentLoginUser.UserID 
                                                        && entry.ReturnDate == null && entry.DueDate.Date < DateTime.Now.Date);
                if (checkList.Count > 0)
                    TempData["WarnNoti"] = "You have overdue borrow please check your borrowlist";
            }
            return View(CurrentLoginUser);
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(PasswordChanger pwdToChange){
            if (ModelState.IsValid)
            {
                string userName = HttpContext.User.Identity.Name.ToString().Substring(2);
                Person target = libRepo.LibrarianRepo.ListWhere(librarian => librarian.UserName == userName).SingleOrDefault();
                if (target == null)
                    target = libRepo.MemberRepo.ListWhere(member => member.UserName == userName).SingleOrDefault();
                if (Crypto.VerifyHashedPassword(target.Password, pwdToChange.oldPassword))
                {
                    if (pwdToChange.isEqualPassword())
                    {
                        target.Password = Crypto.HashPassword(pwdToChange.newPassword);
                        if (target.Identify().StartsWith("Librarian"))
                            libRepo.LibrarianRepo.Update((Librarian)target);
                        else
                            libRepo.MemberRepo.Update((Member)target);
                        libRepo.Save();
                        TempData["SuccessNoti"] = "Change password successfully.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["ErrorNoti"] = "New password and confirm password is not match.";
                        return View();
                    }
                }
                else
                {
                    TempData["ErrorNoti"] = "Your current password information is incorrect.";
                    return View();
                }
            }
            return View();
        }

        [Authorize]
        public ActionResult EditAccount()
        {
            Person target = libRepo.LibrarianRepo.ListWhere(librarian => librarian.UserName == HttpContext.User.Identity.Name.ToString().Substring(2)).SingleOrDefault();
            if (target == null)
                target = libRepo.MemberRepo.ListWhere(member => member.UserName == HttpContext.User.Identity.Name.ToString().Substring(2)).SingleOrDefault();
            AccountEditor editor = new AccountEditor();
            editor.Name = target.Name;
            editor.Email = target.Email;
            return View(editor);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditAccount(AccountEditor editor)
        {
            
            if (ModelState.IsValid)
            {
                string userName = HttpContext.User.Identity.Name.ToString().Substring(2);
                if ((libRepo.MemberRepo.ListWhere(targetusr => targetusr.Email.ToLower() == editor.Email.ToLower() && targetusr.UserName.ToLower() != userName.ToLower()).SingleOrDefault() == null) &&
                   (libRepo.LibrarianRepo.ListWhere(targetusr => targetusr.Email.ToLower() == editor.Email.ToLower() && targetusr.UserName.ToLower() != userName.ToLower()).SingleOrDefault() == null))
                {
                    Person target = libRepo.LibrarianRepo.ListWhere(librarian => librarian.UserName == userName).SingleOrDefault();
                    if (target == null)
                    {
                        target = libRepo.MemberRepo.ListWhere(member => member.UserName == userName).SingleOrDefault();
                    }
                    target.Name = editor.Name;
                    target.Email = editor.Email;
                    if (target.Identify().StartsWith("Librarian"))
                        libRepo.LibrarianRepo.Update((Librarian)target);
                    else
                        libRepo.MemberRepo.Update((Member)target);
                    libRepo.Save();
                    TempData["SuccessNoti"] = "Edit account successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorNoti"] = "This e-mail is already exists.";
                    return View(editor);
                }
            }
            return View(editor);
        }

        [Authorize]
        public ActionResult LibrarianPortal()
        {
            return View();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Request.HttpMethod == "GET")
            {
                if (AuthenticateController.IsUserValid(HttpContext.User.Identity.Name.Substring(2))){
                    Session["LoginUser"] = HttpContext.User.Identity.Name;
                    if (filterContext.ActionDescriptor.ActionName == "LibrarianPortal")
                    {
                        if (HttpContext.User.Identity.Name.ToString().Substring(0, 2) != "A_")
                        {
                            filterContext.Result = RedirectToAction("Index");
                            return;
                        }
                    }
                }
                else{
                    AuthenticateController.OnInvalidSession(ref filterContext);
                    return;
                }
            }
        }
    }

}