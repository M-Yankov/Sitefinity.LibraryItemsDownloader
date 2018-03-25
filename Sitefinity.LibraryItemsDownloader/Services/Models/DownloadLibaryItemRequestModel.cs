namespace Sitefinity.LibraryItemsDownloader.Services.Models
{
    /// <summary>
    /// It is model that contains data sent from JavaScript with Ajax request. 
    /// </summary>
    public class DownloadLibaryItemRequestModel
    {
        public string Id { get; set; }

        public bool IsFolder { get; set; }
    }
}
