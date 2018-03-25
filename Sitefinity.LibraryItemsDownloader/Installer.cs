namespace Sitefinity.LibraryItemsDownloader
{
    using System;
    using Sitefinity.LibraryItemsDownloader.Helpers;
    using Sitefinity.LibraryItemsDownloader.Services;
    using Telerik.Sitefinity.Configuration;
    using Telerik.Sitefinity.Services;

    /// <summary>
    /// <see cref="https://docs.sitefinity.com/custom-error-trace-listener-create-the-installer-class"/>
    /// </summary>
    public class Installer
    {
        public static void PreApplicationStart()
        {
            SystemManager.ApplicationStart += new EventHandler<EventArgs>(ApplicationStartHandler);
        }

        /// <summary>
        /// Applications the start handler invoked on PreApplicationStart event.
        /// </summary>
        private static void ApplicationStartHandler(object sender, EventArgs e)
        {
            SystemManager.RegisterWebService(typeof(LibraryItemsDownloadService), LibraryItemsDownloadService.WebServicePath);

            IConfigManagerHelper managerHelper = new ConfigManagerHelper(ConfigManager.GetManager());
            new InstallationsHelper(managerHelper).Initialize();
        }
    }
}
