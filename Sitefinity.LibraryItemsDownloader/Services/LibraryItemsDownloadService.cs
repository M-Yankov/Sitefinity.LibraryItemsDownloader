﻿namespace Sitefinity.LibraryItemsDownloader.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using Telerik.Sitefinity.Libraries.Model;
    using Telerik.Sitefinity.Modules.Libraries;
    using Telerik.Sitefinity.Utilities.Zip;
    using Telerik.Sitefinity.Web.Services;

    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class LibraryItemsDownloadService : ILibraryItemsDownloadService
    {
        public const string WebServicePath = "LibrariesService";

        public string DownloadImages(string[] imageIds)
        {
            LibrariesManager libraryManager = this.GetLibrariesManager();
            string result = this.GetDownloadableContent<Image>(libraryManager, imageIds);
            return result;
        }

        public string DownloadVideos(string[] videoIds)
        {
            LibrariesManager libraryManager = this.GetLibrariesManager();
            string result = this.GetDownloadableContent<Video>(libraryManager, videoIds);
            return result;
        }

        public string DownloadDocuments(string[] documentIds)
        {
            LibrariesManager libraryManager = this.GetLibrariesManager();
            string result = this.GetDownloadableContent<Document>(libraryManager, documentIds);
            return result;
        }

        public string GetDownloadableContent<TContent>(LibrariesManager libraryManager, string[] contentItemIds) where TContent : MediaContent
        {
            this.VerifyUserHasPermissionsToAceessService();

            string result = string.Empty;

            using (MemoryStream memoryStream = new MemoryStream())
            using (ZipFile zipFiles = new ZipFile())
            {
                foreach (string id in contentItemIds ?? Enumerable.Empty<string>())
                {
                    Guid itemId;
                    if (Guid.TryParse(id, out itemId))
                    {
                        TContent contentItem = libraryManager.GetItem(typeof(TContent), itemId) as TContent;
                        if (contentItem != null)
                        {
                            TContent contentItemLiveVersion = libraryManager.Provider.GetLiveBase<TContent>(contentItem);
                            Stream downloadStream = libraryManager.Download(contentItemLiveVersion);
                            string contentItemName = Path.GetFileName(contentItem.FilePath);
                            zipFiles.AddFileStream(contentItemName, string.Empty, downloadStream);
                        }
                    }
                }

                zipFiles.Save(memoryStream);

                byte[] byteResult = memoryStream.ToArray();
                result = Convert.ToBase64String(byteResult);
            }

            return result;
        }

        public virtual LibrariesManager GetLibrariesManager()
        {
            LibrariesManager libraryManager = LibrariesManager.GetManager();
            return libraryManager;
        }

        public virtual void VerifyUserHasPermissionsToAceessService()
        {
            ServiceUtility.RequestBackendUserAuthentication();
        }
    }
}