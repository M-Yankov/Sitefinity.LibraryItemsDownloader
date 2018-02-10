using System.ServiceModel;
using System.ServiceModel.Web;

namespace Sitefinity.LibraryItemsDownloader.Custom.Services
{
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
    }
}
