namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    using System.Reflection;
    using Telerik.Sitefinity.Configuration;
    using Telerik.Sitefinity.Modules.Libraries.Configuration;
    using Telerik.Sitefinity.Web.UI.ContentUI.Config;

    /// <summary>
    /// This interface is used to install the necessary configuration items and references in the sitefinity back-end. 
    /// </summary>
    public interface IInstallationsHelper
    {
        /// <summary>
        /// Starts the whole configuration. Set up back-end sections. Invoked on PreApplicationStart.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Set up library back-end section.
        /// </summary>
        /// <param name="manager">The manager wrapper for libraries section.</param>
        /// <param name="libsConfig">The library configuration (LibrariesConfig.Config).</param>
        /// <param name="definitionName">Name of the back-end section.</param>
        /// <param name="backendListViewName">Name of the back-end ListView (ImagesBackEnd, VideosBackend, DocumentsBackeEnd).</param>
        /// <param name="commandName">Name of the command referenced in JavaScript file. On download selected items it rises up the provided command. (For images, video or document)</param>
        /// <param name="commandText">The command text to be displayed on the front-end.</param>
        void ConfigureLibrarySection(IConfigManagerHelper manager, LibrariesConfig libsConfig, string definitionName, string backendListViewName, string commandName, string commandText);

        /// <summary>
        /// Gets the JavaScript qualified name key.
        /// <para>
        /// Example: "Sitefinity.LibraryItemsDownloader.LibraryItemsDownloadService.js, Sitefinity.LibraryItemsDownloader, Version=7.3.5600, Culture=neutral, PublicKeyToken=null"
        /// </para>
        /// </summary>
        /// <param name="libraryItemsDownloaderAssembly">The Sitefinity.LibraryItemsDownloader assembly.</param>
        /// <param name="scriptFileName">Name of the script file.</param>
        /// <returns>The full name of the JavaScript resource file with full namespace.<para></para>
        /// </returns>
        string GetJavaScriptQualifiedNameKey(Assembly libraryItemsDownloaderAssembly, string scriptFileName);

        /// <summary>
        /// Adds or updates a script reference in the back-end. It's important to use the latest version of the script when a new version is installed.
        /// </summary>
        /// <param name="configKey">The configuration key that will be added in <paramref name="scriptsElements"/>.</param>
        /// <param name="currentAssembly">The Sitefinity.LibraryItemsDownloader assembly. Using the name of the assembly determines the reference exists or not.</param>
        /// <param name="scriptsElements">A collection of all script elements referenced in the back-end section.</param>
        /// <returns>Whether to save section or not/</returns>
        bool AddOrUpdateScriptReference(string configKey, Assembly currentAssembly, ConfigElementDictionary<string, ClientScriptElement> scriptsElements);
    }
}