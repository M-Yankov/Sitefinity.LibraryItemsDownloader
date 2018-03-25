namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    using Telerik.Sitefinity.Configuration;
    using Telerik.Sitefinity.Data;

    public interface IConfigManagerHelper
    {
        IManager Manager { get; }

        TSection GetSection<TSection>() where TSection : ConfigSection, new();

        void SaveSection(ConfigSection section, bool useFileSystemMode);
    }
}