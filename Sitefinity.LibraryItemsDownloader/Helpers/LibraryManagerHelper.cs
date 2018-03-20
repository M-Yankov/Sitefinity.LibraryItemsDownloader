namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    using System;
    using System.IO;
    using System.Linq;
    using Telerik.Sitefinity;
    using Telerik.Sitefinity.Libraries.Model;
    using Telerik.Sitefinity.Modules.GenericContent;
    using Telerik.Sitefinity.Modules.Libraries;

    public class LibraryManagerHelper : ILibraryManagerHelper
    {
        private readonly LibrariesManager librariesManager;

        public LibraryManagerHelper() 
            : this(LibrariesManager.GetManager())
        {
        }

        public LibraryManagerHelper(LibrariesManager librariesManager)
        {
            this.librariesManager = librariesManager;
        }

        public virtual IQueryable<IFolder> GetChildFolders(IFolder parentFolder)
        {
            return this.librariesManager.GetChildFolders(parentFolder);
        }

        public virtual IQueryable<MediaContent> GetChildItems(IFolder parentFolder)
        {
            return this.librariesManager.GetChildItems(parentFolder);
        }

        public virtual Stream Download(MediaContent content)
        {
            return this.librariesManager.Download(content);
        }

        public virtual object GetItem(Type itemType, Guid id)
        {
            return this.librariesManager.GetItem(itemType, id);
        }

        public virtual ContentDataProviderBase Provider
        {
            get
            {
                return this.librariesManager.Provider;
            }
        }

        public virtual IFolder GetFolder(Guid id)
        {
            return this.librariesManager.GetFolder(id);
        }
    }
}
