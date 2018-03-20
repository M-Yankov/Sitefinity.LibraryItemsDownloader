namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    using System;
    using System.IO;
    using System.Linq;
    using Telerik.Sitefinity;
    using Telerik.Sitefinity.Libraries.Model;
    using Telerik.Sitefinity.Modules.GenericContent;

    public interface ILibraryManagerHelper
    {
        ContentDataProviderBase Provider { get; }

        Stream Download(MediaContent content);

        IQueryable<IFolder> GetChildFolders(IFolder parentFolder);

        IQueryable<MediaContent> GetChildItems(IFolder parentFolder);

        IFolder GetFolder(Guid id);

        object GetItem(Type itemType, Guid id);
    }
}