namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    using System.Reflection;
    using Telerik.Sitefinity.Configuration;
    using Telerik.Sitefinity.Modules.Libraries.Configuration;

    public interface IInstallationsHelper
    {
        void ConfigureLibrarySection(ConfigManager manager, LibrariesConfig libsConfig, string definitionName, string backendListViewName, string commandName, string commandText);

        string GetJavaScriptQualifiedNameKey(Assembly libraryItemsDownloaderAssembly, string scriptFileName);

        void Initialize();
    }
}