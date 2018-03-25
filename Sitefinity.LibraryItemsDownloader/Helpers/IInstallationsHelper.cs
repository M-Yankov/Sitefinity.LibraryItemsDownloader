namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    using System.Reflection;
    using Telerik.Sitefinity.Configuration;
    using Telerik.Sitefinity.Modules.Libraries.Configuration;
    using Telerik.Sitefinity.Web.UI.ContentUI.Config;

    public interface IInstallationsHelper
    {
        void Initialize();

        void ConfigureLibrarySection(IConfigManagerHelper manager, LibrariesConfig libsConfig, string definitionName, string backendListViewName, string commandName, string commandText);

        string GetJavaScriptQualifiedNameKey(Assembly libraryItemsDownloaderAssembly, string scriptFileName);

        bool AddOrUpdateScriptReference(string configKey, Assembly currentAssembly, ConfigElementDictionary<string, ClientScriptElement> scriptsElements);
    }
}