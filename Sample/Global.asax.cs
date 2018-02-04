using Sitefinity.LibraryItemsDownloader.Custom.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Telerik.Sitefinity.Services;

namespace SitefinityWebApp
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            SystemManager.ApplicationStart += new EventHandler<EventArgs>(ApplicationStartHandler);
        }

        private void ApplicationStartHandler(object sender, EventArgs e)
        {
            SystemManager.RegisterWebService(typeof(LibraryItemsDownloadService), "LibrariesService");
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}