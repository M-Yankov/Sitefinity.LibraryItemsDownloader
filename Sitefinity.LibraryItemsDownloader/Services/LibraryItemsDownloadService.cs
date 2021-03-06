﻿namespace Sitefinity.LibraryItemsDownloader.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using Sitefinity.LibraryItemsDownloader.Helpers;
    using Sitefinity.LibraryItemsDownloader.Services.Models;
    using Telerik.Sitefinity;
    using Telerik.Sitefinity.Abstractions;
    using Telerik.Sitefinity.GenericContent.Model;
    using Telerik.Sitefinity.Libraries.Model;
    using Telerik.Sitefinity.Modules.Libraries;
    using Telerik.Sitefinity.Utilities.Zip;
    using Telerik.Sitefinity.Web.Services;

    /// <summary>
    /// The Client (Browser, JavaScirpt) makes ajax calls to this service.
    /// </summary>
    /// <seealso cref="Sitefinity.LibraryItemsDownloader.Services.ILibraryItemsDownloadService" />
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class LibraryItemsDownloadService : ILibraryItemsDownloadService
    {
        public const string WebServicePath = "LibrariesService";
        private const string ExceptionMessageFormat = "LibraryItemsDownloader Error: {0}{1}Item title: \"{2}\", id: {3}";
        private const string CannotFindLiveVersionFormat = "LibraryItemsDownloader Info: Cannot find the live version of item. Title: {0}, id: {1}";
        private readonly IUtilityHelper utilityHelper;
        private readonly ILibraryManagerHelper libraryManagerHelper;

        public LibraryItemsDownloadService()
            : this(new UtilityHelper(), new LibraryManagerHelper())
        {
        }

        public LibraryItemsDownloadService(IUtilityHelper helper, ILibraryManagerHelper libraryManagerHelper)
        {
            this.utilityHelper = helper;
            this.libraryManagerHelper = libraryManagerHelper;
        }

        /// <summary>
        /// Downloads selected images including folders, if they are selected.
        /// </summary>
        /// <param name="imagesRequest">Request model which items to add in the archive.</param>
        /// <returns>
        /// The zip archive as string content.
        /// </returns>
        public virtual string DownloadImages(IEnumerable<DownloadLibaryItemRequestModel> imagesRequest)
        {
            string result = this.GetDownloadableContent<Image>(imagesRequest);
            return result;
        }

        /// <summary>
        /// Downloads selected videos including folders, if they are selected.
        /// </summary>
        /// <param name="videosRequest">Request model which items to add in the archive.</param>
        /// <returns>
        /// A zip archive as string content.
        /// </returns>
        public string DownloadVideos(IEnumerable<DownloadLibaryItemRequestModel> videosRequest)
        {
            string result = this.GetDownloadableContent<Video>(videosRequest);
            return result;
        }

        /// <summary>
        /// Downloads selected documents including folders, if they are selected.
        /// </summary>
        /// <param name="documentsRequest">Request model which items to add in the archive.</param>
        /// <returns>
        /// A zip archive as string content.
        /// </returns>
        public string DownloadDocuments(IEnumerable<DownloadLibaryItemRequestModel> documentsRequest)
        {
            string result = this.GetDownloadableContent<Document>(documentsRequest);
            return result;
        }

        /// <summary>
        /// Generic method that works with images, videos or documents and returns a zip archive as string.
        /// In case of broken items - an empty zip archive is returned.
        /// </summary>
        /// <typeparam name="TContent">The type of the content (Image, Video, Document).</typeparam>
        /// <param name="contentItemsRequest">Request model which items to add in the archive.</param>
        /// <returns>A zip archive as string content.</returns>
        public virtual string GetDownloadableContent<TContent>(IEnumerable<DownloadLibaryItemRequestModel> requestModels) where TContent : MediaContent
        {
            this.VerifyUserHasPermissionsToAceessService();

            string result = string.Empty;

            using (MemoryStream memoryStream = new MemoryStream())
            using (ZipFile zipFiles = new ZipFile())
            {
                ICollection<Guid> selectedItemIds = new HashSet<Guid>();

                foreach (DownloadLibaryItemRequestModel requestModel in requestModels ?? Enumerable.Empty<DownloadLibaryItemRequestModel>())
                {
                    Guid requestId;
                    if (!Guid.TryParse(requestModel.Id, out requestId))
                    {
                        continue;
                    }

                    if (requestModel.IsFolder)
                    {
                        IFolder libraryFolder = this.libraryManagerHelper.GetFolder(requestId);
                        if (libraryFolder != null)
                        {
                            // Save selected folders
                            this.SaveLibraryItemsToStreamRecursively<TContent>(libraryFolder, zipFiles, string.Empty);
                        }
                    }
                    else
                    {
                        selectedItemIds.Add(requestId);
                    }
                }

                // Save selected content Items
                this.SaveLibraryItemsToStream<TContent>(selectedItemIds, zipFiles, string.Empty);

                zipFiles.Save(memoryStream);
                 
                byte[] byteResult = memoryStream.ToArray();
                result = Convert.ToBase64String(byteResult);
            }

            return result;
        }

        /// <summary>
        /// Allow only back-end users to access this service.
        /// </summary>
        public virtual void VerifyUserHasPermissionsToAceessService()
        {
            ServiceUtility.RequestBackendUserAuthentication();
        }

        /// <summary>
        /// Saves items from folder into <paramref name="zipStream"/> recursively (Including all nested folders). Invalid symbols for a windows file will be replaced.
        /// </summary>
        /// <typeparam name="TContent">The type of the content (Image, Video, Document).</typeparam>
        /// <param name="folder">The folder with content items.</param>
        /// <param name="zipStream">The zip stream.</param>
        /// <param name="directoryPathInArchive">The directory path in archive. Can be string.Empty.</param>
        public virtual void SaveLibraryItemsToStreamRecursively<TContent>(IFolder folder, ZipFile zipStream, string directoryPathInArchive) where TContent : MediaContent
        {
            IEnumerable<IFolder> innerFolders = this.libraryManagerHelper.GetChildFolders(folder).ToList();
            string titleSaveName = this.utilityHelper.ReplaceInvalidCharacters(folder.Title.Trim());
            string innerFolderPathName = Path.Combine(directoryPathInArchive, titleSaveName);

            foreach (IFolder innerFolder in innerFolders ?? Enumerable.Empty<IFolder>())
            {
                this.SaveLibraryItemsToStreamRecursively<TContent>(innerFolder, zipStream, innerFolderPathName);
            }

            IEnumerable<TContent> contentItems = this.libraryManagerHelper
                                .GetChildItems(folder)
                                .Where(content => content.Status == ContentLifecycleStatus.Live)
                                .Cast<TContent>()
                                .ToList();

            foreach (TContent contentItem in contentItems)
            {
                Stream downloadStream = this.libraryManagerHelper.Download(contentItem);
                string contentItemName = Path.GetFileName(contentItem.FilePath);

                zipStream.AddFileStream(contentItemName, innerFolderPathName, downloadStream);
            }
        }

        /// <summary>
        /// Saves selected items into <paramref name="zipStream"/>. Logs into sitefinit log files message, in case an item cannot be downloaded.
        /// </summary>
        /// <typeparam name="TContent">The type of the content (Image, Video, Document).</typeparam>
        /// <param name="selectedItemIds">Ids the content items.</param>
        /// <param name="zipStream">The zip stream.</param>
        /// <param name="directoryPathInArchive">The directory path in archive. Can be string.Empty.</param>
        public virtual void SaveLibraryItemsToStream<TContent>(IEnumerable<Guid> selectedItemIds, ZipFile zipStream, string directoryPathInArchive) where TContent : MediaContent
        {
            foreach (Guid selectedId in selectedItemIds ?? Enumerable.Empty<Guid>())
            {
                TContent contentItem = this.libraryManagerHelper.GetItem(typeof(TContent), selectedId) as TContent;
                if (contentItem != null)
                {
                    TContent contentItemLiveVersion = this.libraryManagerHelper.GetLiveBase<TContent>(contentItem);

                    if (contentItemLiveVersion == null)
                    {
                        string logMessage = string.Format(CannotFindLiveVersionFormat, contentItem.Title, contentItem.Id);
                        Log.Write(logMessage, ConfigurationPolicy.Trace);
                        continue;
                    }

                    Stream downloadStream = null;
                    try
                    {
                        downloadStream = this.libraryManagerHelper.Download(contentItemLiveVersion);
                    }
                    catch (Exception exception)
                    {
                         string logMessage = string.Format(
                             ExceptionMessageFormat,
                             exception.Message,
                             Environment.NewLine,
                             contentItemLiveVersion.Title,
                             contentItemLiveVersion.Id);

                        Log.Write(logMessage, ConfigurationPolicy.ErrorLog);
                        continue;
                    }

                    string contentItemName = Path.GetFileName(contentItem.FilePath);

                    zipStream.AddFileStream(contentItemName, directoryPathInArchive, downloadStream);
                }
            }
        }
    }
}