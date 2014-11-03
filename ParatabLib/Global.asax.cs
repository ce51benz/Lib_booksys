﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Optimization;
using System.IO;
using ParatabLib.Models;

namespace ParatabLib
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DataAccess.LibraryRepository libRepo = new DataAccess.LibraryRepository();
            foreach (Member member in libRepo.MemberRepo.List())
                Controllers.AuthenticateController.AddUser(member.UserName);
            foreach (Librarian librarian in libRepo.LibrarianRepo.List())
                Controllers.AuthenticateController.AddUser(librarian.UserName);
            if (!File.Exists(HttpRuntime.AppDomainAppPath+"/lib-config"))
            {

                File.AppendAllText(HttpRuntime.AppDomainAppPath + "/lib-config", "5");
            }
            else
            {
                Controllers.ConfigurationController.setFine(int.Parse
                    (File.ReadAllText(HttpRuntime.AppDomainAppPath + "/lib-config")));
            }
        }
    }
}