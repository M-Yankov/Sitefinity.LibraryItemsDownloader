using Sitefinity.LibraryItemsDownloader.Custom.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }
    }
}
