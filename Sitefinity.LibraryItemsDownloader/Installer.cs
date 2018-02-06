using Sitefinity.LibraryItemsDownloader.Custom.Services;
using Sitefinity.LibraryItemsDownloader.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Modules.Libraries.Configuration;
using Telerik.Sitefinity.Services;

namespace Sitefinity.LibraryItemsDownloader
{
    public class Installer
    {
        public static void PreApplicationStart()
        {
            SystemManager.ApplicationStart += new EventHandler<EventArgs>(ApplicationStartHandler);
        }

        private static void ApplicationStartHandler(object sender, EventArgs e)
        {
            SystemManager.RegisterWebService(typeof(LibraryItemsDownloadService), LibraryItemsDownloadService.WebServicePath);
            InstallationsHelper.Initialize();
        }
    }
}
