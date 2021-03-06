﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ParatabLib.DataAccess;
using ParatabLib.Models;
using ParatabLib.ViewModels;
namespace ParatabLib.Controllers
{
    //This class use to handle news viewing action
    public class NewsController : Controller
    {
        LibraryRepository libRepo = new LibraryRepository();
        private static volatile List<News> latestNews;

        public static void setLatestNews(List<News> list)
        {
            latestNews = list;
        }

        public static List<News> getLatestNews()
        {
            return latestNews;
        }

        //This method use to view news detail by passing id of news that want to see as integer.
        [OutputCache(Duration = 0, NoStore = true)]
        public ActionResult View(int id)
        {
            News newstoview = libRepo.NewsRepo.Find(id);
            if (newstoview != null)
                return View(newstoview);
            TempData["ErrorNoti"] = "No news with that ID exists.";
            return RedirectToAction("ViewAll");
        }

        /* This method use to show all news in library
         * by passing page and pageSize as parameter for paging list.
         */
        [OutputCache(Duration = 0, NoStore = true)]
        public ActionResult ViewAll(int page = 1,int pageSize = 10)
        {
            TempData["pageSize"] = pageSize;
            TempData["page"] = page;
            List<News> newsList = libRepo.NewsRepo.List().OrderByDescending(news => news.PostTime).ToList();
            PageList<News> pglist = new PageList<News>(newsList, page, pageSize);
            switch (pglist.Categorized())
            {
                case PageListResult.Ok: { return View(pglist); }
                case PageListResult.Empty:
                    {
                        TempData["ErrorNoti"] = "No news list to show now.";
                        return View();
                    }
                default:
                    {
                        TempData["ErrorNoti"] = "Invalid list view parameter please refresh this page to try again.";
                        return View();
                    }
            }
        }


        /* [Override method]
         * This method use to check that whether current user is exist in system or not.
         * If not,call and pass by reference of current HTTPrequest in AuthenticateController.OnInvalidSession
         * to set appropiate page result.
         */
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                if (AuthenticateController.IsUserValid(HttpContext.User.Identity.Name.Substring(2)))
                {
                    Session["LoginUser"] = HttpContext.User.Identity.Name;
                }
                else
                {
                    AuthenticateController.OnInvalidSession(ref filterContext);
                    return;
                }
            }
            else
                Session["LoginUser"] = null;
        }
    }
}
