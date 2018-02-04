using System.ServiceModel;
using System.ServiceModel.Web;

namespace Sitefinity.LibraryItemsDownloader.Custom.Services
{
    [ServiceContract]
    public interface ILibraryItemsDownloadService
    {
        [WebInvoke(Method = "POST", UriTemplate = "/DownloadImages", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        [OperationContract]
        string DownloadImages(string[] imageIds);
    }
}
