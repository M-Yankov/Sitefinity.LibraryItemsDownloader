namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    using System;
    using System.IO;
    using System.Linq;
    using Telerik.Sitefinity;
    using Telerik.Sitefinity.GenericContent.Model;
    using Telerik.Sitefinity.Libraries.Model;
    using Telerik.Sitefinity.Modules.Libraries;

    /// <summary>
    /// A wrapper for LibrariesManager.
    /// </summary>
    /// <seealso cref="Sitefinity.LibraryItemsDownloader.Helpers.ILibraryManagerHelper" />
    public class LibraryManagerHelper : ILibraryManagerHelper
    {
        private readonly LibrariesManager librariesManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryManagerHelper"/> class. Calls the other constructor with  this(LibrariesManager.GetManager()).
        /// </summary>
        public LibraryManagerHelper()
            : this(LibrariesManager.GetManager())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryManagerHelper"/> class.
        /// </summary>
        /// <param name="librariesManager">The libraries manager.</param>
        public LibraryManagerHelper(LibrariesManager librariesManager)
        {
            this.librariesManager = librariesManager;
        }

        /// <summary>
        /// Invokes LibrariesManager.Provider.GetLiveBase&lt;<typeparamref name="TContent" />&gt;();
        /// </summary>
        /// <typeparam name="TContent">The type of the content. One of Image, Video or Document.</typeparam>
        /// <param name="content">Master version content.</param>
        /// <returns>
        /// Live version of the content.
        /// </returns>
        public virtual TContent GetLiveBase<TContent>(TContent content) where TContent : Content
        {
            return this.librariesManager.Provider.GetLiveBase<TContent>(content);
        }

        /// <summary>
        /// Gets child folders of a folder. Invokes LibrariesManager.GetChildFolders()
        /// </summary>
        /// <param name="parentFolder">The parent folder.</param>
        /// <returns>
        /// Collection of child folders. <seealso cref="Telerik.Sitefinity.Model.Folder" />.
        /// </returns>
        public virtual IQueryable<IFolder> GetChildFolders(IFolder parentFolder)
        {
            return this.librariesManager.GetChildFolders(parentFolder);
        }

        /// <summary>
        /// Gets child content items. Images, Videos, Documents. They cannot be mixed.
        /// </summary>
        /// <param name="parentFolder">The parent folder.</param>
        /// <returns>
        /// The child content items.
        /// </returns>
        public virtual IQueryable<MediaContent> GetChildItems(IFolder parentFolder)
        {
            return this.librariesManager.GetChildItems(parentFolder);
        }

        /// <summary>
        /// Invokes LibrariesManager.Download(content); The content can be stored on different locations: Database, FileSystem, Azure Storage or custom storage provider.
        /// </summary>
        /// <param name="content">The content to download (Image, Video, Document).</param>
        /// <returns>
        /// Stream data of the content.
        /// </returns>
        public virtual Stream Download(MediaContent content)
        {
            return this.librariesManager.Download(content);
        }

        /// <summary>
        /// Gets a content item as object. Simply invokes LibrariesManager.GetItem();
        /// </summary>
        /// <param name="itemType">Type of the item. (typeof(Image),  typeof(Video), typeof(Document))</param>
        /// <param name="id">The identifier of the item.</param>
        /// <returns>
        /// The content item as object.
        /// </returns>
        public virtual object GetItem(Type itemType, Guid id)
        {
            return this.librariesManager.GetItem(itemType, id);
        }

        /// <summary>
        /// Gets a folder by id.
        /// </summary>
        /// <param name="id">The identifier of the folder.</param>
        /// <returns>
        /// The folder <seealso cref="Telerik.Sitefinity.Model.Folder" />.
        /// </returns>
        public virtual IFolder GetFolder(Guid id)
        {
            return this.librariesManager.GetFolder(id);
        }
    }
}
