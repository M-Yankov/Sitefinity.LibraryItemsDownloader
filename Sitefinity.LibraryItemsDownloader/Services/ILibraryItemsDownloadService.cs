namespace Sitefinity.LibraryItemsDownloader.Services
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using Sitefinity.LibraryItemsDownloader.Services.Models;
    using Telerik.Sitefinity.Libraries.Model;

    [ServiceContract]
    public interface ILibraryItemsDownloadService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/DownloadImages", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        string DownloadImages(IEnumerable<DownloadLibaryItemRequestModel> imagesRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/DownloadVideos", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        string DownloadVideos(IEnumerable<DownloadLibaryItemRequestModel> imagesRequest);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/DownloadDocuments", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        string DownloadDocuments(IEnumerable<DownloadLibaryItemRequestModel> imagesRequest);

        string GetDownloadableContent<TContent>(IEnumerable<DownloadLibaryItemRequestModel> imagesRequest) where TContent : MediaContent;

        void VerifyUserHasPermissionsToAceessService();
    }
}
