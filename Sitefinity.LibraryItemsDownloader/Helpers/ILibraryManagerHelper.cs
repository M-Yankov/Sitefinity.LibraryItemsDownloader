namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    using System;
    using System.IO;
    using System.Linq;
    using Telerik.Sitefinity;
    using Telerik.Sitefinity.GenericContent.Model;
    using Telerik.Sitefinity.Libraries.Model;

    public interface ILibraryManagerHelper
    {
        TContent GetLiveBase<TContent>(TContent content) where TContent : Content;

        Stream Download(MediaContent content);

        IQueryable<IFolder> GetChildFolders(IFolder parentFolder);

        IQueryable<MediaContent> GetChildItems(IFolder parentFolder);

        IFolder GetFolder(Guid id);

        object GetItem(Type itemType, Guid id);
    }
}