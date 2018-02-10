using Sitefinity.LibraryItemsDownloader.Custom.Services;
using Sitefinity.LibraryItemsDownloader.Helpers;
using System;
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
            new InstallationsHelper().Initialize();
        }
    }
}
