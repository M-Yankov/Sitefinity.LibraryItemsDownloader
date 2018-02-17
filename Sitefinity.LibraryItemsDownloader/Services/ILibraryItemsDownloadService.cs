namespace Sitefinity.LibraryItemsDownloader.Services
{
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using Telerik.Sitefinity.Libraries.Model;
    using Telerik.Sitefinity.Modules.Libraries;

    [ServiceContract]
    public interface ILibraryItemsDownloadService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/DownloadImages", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        string DownloadImages(string[] imageIds);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/DownloadVideos", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        string DownloadVideos(string[] videoIds);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/DownloadDocuments", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        string DownloadDocuments(string[] documentIds);

        string GetDownloadableContent<TContent>(LibrariesManager libraryManager, string[] contentItemIds) where TContent : MediaContent;

        LibrariesManager GetLibrariesManager();
    }
}
