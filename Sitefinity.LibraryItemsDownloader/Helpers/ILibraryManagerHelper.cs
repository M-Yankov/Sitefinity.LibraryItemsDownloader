namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    using System;
    using System.IO;
    using System.Linq;
    using Telerik.Sitefinity;
    using Telerik.Sitefinity.GenericContent.Model;
    using Telerik.Sitefinity.Libraries.Model;

    /// <summary>
    /// A wrapper for LibrariesManager.
    /// </summary>
    public interface ILibraryManagerHelper
    {
        /// <summary>
        /// Invokes LibrariesManager.Provider.GetLiveBase&lt;<typeparamref name="TContent"/>&gt;();
        /// </summary>
        /// <typeparam name="TContent">The type of the content. One of Image, Video or Document.</typeparam>
        /// <param name="content">Master version content.</param>
        /// <returns>Live version of the content.</returns>
        TContent GetLiveBase<TContent>(TContent content) where TContent : Content;

        /// <summary>
        /// Invokes LibrariesManager.Download(content); The content can be stored on different locations: Database, FileSystem, Azure Storage or custom storage provider.
        /// </summary>
        /// <param name="content">The content to download (Image, Video, Document).</param>
        /// <returns>Stream data of the content.</returns>
        Stream Download(MediaContent content);

        /// <summary>
        /// Gets child folders of a folder. Invokes LibrariesManager.GetChildFolders()
        /// </summary>
        /// <param name="parentFolder">The parent folder.</param>
        /// <returns>Collection of child folders. <seealso cref="Telerik.Sitefinity.Model.Folder"/>.</returns>
        IQueryable<IFolder> GetChildFolders(IFolder parentFolder);

        /// <summary>
        /// Gets child content items. Images, Videos, Documents. They cannot be mixed. 
        /// </summary>
        /// <param name="parentFolder">The parent folder.</param>
        /// <returns>The child content items. </returns>
        IQueryable<MediaContent> GetChildItems(IFolder parentFolder);

        /// <summary>
        /// Gets a folder by id.
        /// </summary>
        /// <param name="id">The identifier of the folder.</param>
        /// <returns>The folder <seealso cref="Telerik.Sitefinity.Model.Folder"/>.</returns>
        IFolder GetFolder(Guid id);

        /// <summary>
        /// Gets a content item as object. Simply invokes LibrariesManager.GetItem();
        /// </summary>
        /// <param name="itemType">Type of the item. (typeof(Image),  typeof(Video), typeof(Document))</param>
        /// <param name="id">The identifier of the item.</param>
        /// <returns>The content item as object.</returns>
        object GetItem(Type itemType, Guid id);
    }
}