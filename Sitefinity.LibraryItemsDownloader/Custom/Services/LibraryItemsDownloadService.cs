using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Utilities.Zip;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Configuration;

namespace Sitefinity.LibraryItemsDownloader.Custom.Services
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class LibraryItemsDownloadService : ILibraryItemsDownloadService
    {
        public const string WebServicePath = "LibrariesService";

        public string DownloadImages(string[] imageIds)
        {
            string result = string.Empty;
            var libraryManager = LibrariesManager.GetManager();

            using (MemoryStream memoryStream = new MemoryStream())
            using (ZipFile zipFiles = new ZipFile())
            {
                foreach (string id in imageIds ?? Enumerable.Empty<string>())
                {
                    Guid imageId;
                    if (Guid.TryParse(id, out imageId))
                    {
                        Image image = libraryManager.GetImage(imageId);
                        if (image != null)
                        {
                            Image liveImage = libraryManager.Provider.GetLiveBase<Image>(image);
                            Stream stream = libraryManager.Download(liveImage);
                            string imageName = Path.GetFileName(image.FilePath);
                            zipFiles.AddFileStream(imageName, string.Empty, stream);
                        }
                    }
                }

                zipFiles.Save(memoryStream);

                byte[] byteResult = memoryStream.ToArray();
                result = Convert.ToBase64String(byteResult);
            }

            return result;
        }
    }
}