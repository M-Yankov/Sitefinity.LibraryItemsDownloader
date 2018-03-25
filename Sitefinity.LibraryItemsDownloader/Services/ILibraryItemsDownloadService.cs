namespace Sitefinity.LibraryItemsDownloader.Services
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using Sitefinity.LibraryItemsDownloader.Services.Models;
    using Telerik.Sitefinity.Libraries.Model;

    /// <summary>
    /// The Service interface that client makes Ajax requests.
    /// </summary>
    [ServiceContract]
    public interface ILibraryItemsDownloadService
    {
        /// <summary>
        /// Downloads selected images including folders, if they are selected.
        /// </summary>
        /// <param name="imagesRequest">Request model which items to add in the archive.</param>
        /// <returns>A zip archive as string content.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/DownloadImages", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        string DownloadImages(IEnumerable<DownloadLibaryItemRequestModel> imagesRequest);

        /// <summary>
        /// Downloads selected videos including folders, if they are selected.
        /// </summary>
        /// <param name="videosRequest">Request model which items to add in the archive.</param>
        /// <returns>A zip archive as string content.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/DownloadVideos", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        string DownloadVideos(IEnumerable<DownloadLibaryItemRequestModel> videosRequest);

        /// <summary>
        /// Downloads selected documents including folders, if they are selected.
        /// </summary>
        /// <param name="documentsRequest">Request model which items to add in the archive.</param>
        /// <returns>A zip archive as string content.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/DownloadDocuments", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        string DownloadDocuments(IEnumerable<DownloadLibaryItemRequestModel> documentsRequest);

        /// <summary>
        /// Generic method that works with images, videos or documents and returns a zip archive as string.
        /// In case of broken items - an empty zip archive is returned.
        /// </summary>
        /// <typeparam name="TContent">The type of the content (Image, Video, Document).</typeparam>
        /// <param name="contentItemsRequest">Request model which items to add in the archive.</param>
        /// <returns>A zip archive as string content.</returns>
        string GetDownloadableContent<TContent>(IEnumerable<DownloadLibaryItemRequestModel> contentItemsRequest) where TContent : MediaContent;

        /// <summary>
        /// Allow only back-end users to access this service.
        /// </summary>
        void VerifyUserHasPermissionsToAceessService();
    }
}
