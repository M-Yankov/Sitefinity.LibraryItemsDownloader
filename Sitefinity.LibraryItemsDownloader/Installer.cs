namespace Sitefinity.LibraryItemsDownloader
{
    using System;
    using Sitefinity.LibraryItemsDownloader.Helpers;
    using Sitefinity.LibraryItemsDownloader.Services;
    using Telerik.Sitefinity.Configuration;
    using Telerik.Sitefinity.Services;

    public class Installer
    {
        public static void PreApplicationStart()
        {
            SystemManager.ApplicationStart += new EventHandler<EventArgs>(ApplicationStartHandler);
        }

        private static void ApplicationStartHandler(object sender, EventArgs e)
        {
            SystemManager.RegisterWebService(typeof(LibraryItemsDownloadService), LibraryItemsDownloadService.WebServicePath);

            IConfigManagerHelper managerHelper = new ConfigManagerHelper(ConfigManager.GetManager());
            new InstallationsHelper(managerHelper).Initialize();
        }
    }
}
