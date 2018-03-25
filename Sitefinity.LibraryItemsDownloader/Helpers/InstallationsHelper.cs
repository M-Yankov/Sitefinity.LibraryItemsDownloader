namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.UI;
    using Telerik.Sitefinity.Configuration;
    using Telerik.Sitefinity.Data;
    using Telerik.Sitefinity.Modules.Libraries.Configuration;
    using Telerik.Sitefinity.Modules.Libraries.Documents;
    using Telerik.Sitefinity.Modules.Libraries.Images;
    using Telerik.Sitefinity.Modules.Libraries.Videos;
    using Telerik.Sitefinity.Web.UI.Backend.Elements.Config;
    using Telerik.Sitefinity.Web.UI.Backend.Elements.Enums;
    using Telerik.Sitefinity.Web.UI.Backend.Elements.Widgets;
    using Telerik.Sitefinity.Web.UI.ContentUI.Config;
    using Telerik.Sitefinity.Web.UI.ContentUI.Views.Backend.Master.Config;

    /// <summary>
    /// Installs the necessary configuration items and references in the sitefinity back-end. 
    /// </summary>
    /// <seealso cref="Sitefinity.LibraryItemsDownloader.Helpers.IInstallationsHelper" />
    public class InstallationsHelper : IInstallationsHelper
    {
        private const string WidgetBarSectionName = "toolbar";
        private const string MoreActionsWidgetName = "MoreActionsWidget";
        private const string DownloadSelectedImagesCommandName = "DownloadSelectedImages";
        private const string DownloadSelectedVideosCommandName = "DownloadSelectedVideos";
        private const string DownloadSelectedDocumentsCommandName = "DownloadSelectedDocuments";
        private const string ClientMasterViewLoadMethodName = "OnMasterViewLoadedCustom";
        private const string NullMessage = "NULL";
#if DEBUG
        private const string JavascriptFileName = "LibraryItemsDownloadService.js";
#else
        private const string JavascriptFileName = "LibraryItemsDownloadService.min.js";
#endif

        private readonly IConfigManagerHelper configManagerHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallationsHelper"/> class.
        /// </summary>
        /// <param name="configManagerHelper">The configuration manager helper.</param>
        public InstallationsHelper(IConfigManagerHelper configManagerHelper)
        {
            this.configManagerHelper = configManagerHelper;
        }

        /// <summary>
        /// Starts the whole configuration. Set up back-end sections. Invoked on PreApplicationStart.
        /// </summary>
        public void Initialize()
        {
            // While initializing, the context is not authenticated to save.
            using (ElevatedModeRegion elevatedMode = new ElevatedModeRegion(this.configManagerHelper.Manager))
            {
                LibrariesConfig libsConfig = this.configManagerHelper.GetSection<LibrariesConfig>();
                this.ConfigureLibrarySection(this.configManagerHelper, libsConfig, ImagesDefinitions.BackendImagesDefinitionName, ImagesDefinitions.BackendListViewName, DownloadSelectedImagesCommandName, "Download selected images");
                this.ConfigureLibrarySection(this.configManagerHelper, libsConfig, VideosDefinitions.BackendVideosDefinitionName, VideosDefinitions.BackendListViewName, DownloadSelectedVideosCommandName, "Download selected videos");
                this.ConfigureLibrarySection(this.configManagerHelper, libsConfig, DocumentsDefinitions.BackendDefinitionName, DocumentsDefinitions.BackendListViewName, DownloadSelectedDocumentsCommandName, "Download selected documents");
            }
        }

        /// <summary>
        /// Set up library back-end section.
        /// </summary>
        /// <param name="manager">The manager wrapper for libraries section.</param>
        /// <param name="libsConfig">The library configuration (LibrariesConfig.Config).</param>
        /// <param name="definitionName">Name of the back-end section.</param>
        /// <param name="backendListViewName">Name of the back-end ListView (ImagesBackEnd, VideosBackend, DocumentsBackeEnd).</param>
        /// <param name="commandName">Name of the command referenced in JavaScript file. On download selected items it rises up the provided command. (For images, video or document)</param>
        /// <param name="commandText">The command text to be displayed on the front-end.</param>
        /// <exception cref="NullReferenceException">
        /// </exception>
        public virtual void ConfigureLibrarySection(IConfigManagerHelper manager, LibrariesConfig libsConfig, string definitionName, string backendListViewName, string commandName, string commandText)
        {
            if (libsConfig == null || string.IsNullOrWhiteSpace(definitionName) || !libsConfig.ContentViewControls.ContainsKey(definitionName))
            {
                string exceptionMessage = string.Format("Cannot find content view control: {0} related with libraries configuration.", definitionName ?? NullMessage);
                throw new NullReferenceException(exceptionMessage);
            }

            ContentViewControlElement backendDefinition = libsConfig.ContentViewControls[definitionName];
            if (string.IsNullOrWhiteSpace(definitionName) || !backendDefinition.ContainsView(backendListViewName))
            {
                string exceptionMessage = string.Format("Cannot find back-end view: {0}.", backendListViewName ?? NullMessage);
                throw new NullReferenceException(exceptionMessage);
            }

            ContentViewDefinitionElement backendListView = backendDefinition.ViewsConfig[backendListViewName];

            Assembly itemsDownloaderAssembly = typeof(Installer).Assembly;
            string javascriptKey = this.GetJavaScriptQualifiedNameKey(itemsDownloaderAssembly, JavascriptFileName);

            bool shouldSaveSection = this.AddOrUpdateScriptReference(javascriptKey, itemsDownloaderAssembly, backendListView.Scripts);
            if (shouldSaveSection)
            {
                manager.SaveSection(backendDefinition.Section, true);
            }

            MasterGridViewElement masterView = backendListView as MasterGridViewElement;

            WidgetBarElement toolbarConfig = masterView.ToolbarConfig as WidgetBarElement;
            WidgetBarSectionElement widgetBarSectionElement = toolbarConfig.Sections.OfType<WidgetBarSectionElement>().FirstOrDefault(s => s.Name == WidgetBarSectionName);
            ActionMenuWidgetElement moreActionsWidgetElement = widgetBarSectionElement.Items.OfType<ActionMenuWidgetElement>().FirstOrDefault(w => w.Name == MoreActionsWidgetName);
            CommandWidgetElement downloadSelectedImagesCommand = moreActionsWidgetElement.MenuItems.OfType<CommandWidgetElement>().FirstOrDefault(c => c.Name == commandName);

            if (downloadSelectedImagesCommand == null)
            {
                downloadSelectedImagesCommand = new CommandWidgetElement(moreActionsWidgetElement.MenuItems);
                downloadSelectedImagesCommand.Name = commandName;
                downloadSelectedImagesCommand.CssClass = string.Format("sf{0} sfDownloadItm", commandName);
                downloadSelectedImagesCommand.ButtonType = CommandButtonType.Standard;
                downloadSelectedImagesCommand.WrapperTagId = HtmlTextWriterTag.Li.ToString();
                downloadSelectedImagesCommand.WrapperTagKey = HtmlTextWriterTag.Li;
                downloadSelectedImagesCommand.WidgetType = typeof(CommandWidget);
                downloadSelectedImagesCommand.CommandName = commandName;
                downloadSelectedImagesCommand.Text = commandText;

                moreActionsWidgetElement.MenuItems.Add(downloadSelectedImagesCommand);
                manager.SaveSection(backendDefinition.Section, true);
            }
        }

        /// <summary>
        /// Gets the JavaScript qualified name key. 
        /// <para>
        /// Example: "Sitefinity.LibraryItemsDownloader.LibraryItemsDownloadService.js, Sitefinity.LibraryItemsDownloader, Version=7.3.5600, Culture=neutral, PublicKeyToken=null"
        /// </para>
        /// </summary>
        /// <param name="libraryItemsDownloaderAssembly">The Sitefinity.LibraryItemsDownloader assembly.</param>
        /// <param name="scriptFileName">Name of the script file.</param>
        /// <returns>
        /// The full name of the JavaScript resource file with full namespace.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// libraryItemsDownloaderAssembly - Assembly cannot be null.
        /// or
        /// scriptFileName - Script file name is null.
        /// </exception>
        /// <exception cref="NullReferenceException"></exception>
        public virtual string GetJavaScriptQualifiedNameKey(Assembly libraryItemsDownloaderAssembly, string scriptFileName)
        {
            if (libraryItemsDownloaderAssembly == null)
            {
                throw new ArgumentNullException("libraryItemsDownloaderAssembly", "Assembly cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(scriptFileName))
            {
                throw new ArgumentNullException("scriptFileName", "Script file name is null.");
            }

            string fullNamespaceJavaScriptFile = libraryItemsDownloaderAssembly.GetManifestResourceNames().FirstOrDefault(fileName => fileName.EndsWith(scriptFileName));
            if (fullNamespaceJavaScriptFile == null)
            {
                string exceptionMessage = string.Format("Cannot find embedded file {0}", scriptFileName);
                throw new NullReferenceException(exceptionMessage);
            }

            string javaScriptQualifiedNameKey = Assembly.CreateQualifiedName(libraryItemsDownloaderAssembly.FullName, fullNamespaceJavaScriptFile);
            return javaScriptQualifiedNameKey;
        }

        /// <summary>
        /// Adds or updates a script reference in the back-end. It's important to use the latest version of the script when a new version is installed.
        /// </summary>
        /// <param name="configKey">The configuration key that will be added in <paramref name="scriptsElements" />.</param>
        /// <param name="currentAssembly">The Sitefinity.LibraryItemsDownloader assembly. Using the name of the assembly determines the reference exists or not.</param>
        /// <param name="scriptsElements">A collection of all script elements referenced in the back-end section.</param>
        /// <returns>
        /// Whether to save section or not/
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// configKey - Config key cannot be null or empty.
        /// or
        /// currentAssembly - Assembly cannot be null.
        /// or
        /// scriptsElements - The scripts elements collection cannot be null.
        /// </exception>
        public virtual bool AddOrUpdateScriptReference(string configKey, Assembly currentAssembly, ConfigElementDictionary<string, ClientScriptElement> scriptsElements)
        {
            if (string.IsNullOrWhiteSpace(configKey))
            {
                throw new ArgumentNullException("configKey", "Config key cannot be null or empty.");
            }

            if (currentAssembly == null)
            {
                throw new ArgumentNullException("currentAssembly", "Assembly cannot be null.");
            }

            if (scriptsElements == null)
            {
                throw new ArgumentNullException("scriptsElements", "The scripts elements collection cannot be null.");
            }

            if (scriptsElements.ContainsKey(configKey))
            {
                return false;
            }

            AssemblyName itemsDownloaderAssemblyName = currentAssembly.GetName();
            IEnumerable<ClientScriptElement> oldScriptRefences = scriptsElements
                .Cast<ClientScriptElement>()
                .Where(s => s.ScriptLocation.Contains(itemsDownloaderAssemblyName.Name))
                .ToList();

            foreach (ClientScriptElement scriptReference in oldScriptRefences)
            {
                scriptsElements.Remove(scriptReference);
            }
            
            ClientScriptElement scriptElement = new ClientScriptElement(scriptsElements);
            scriptElement.ScriptLocation = configKey;
            scriptElement.LoadMethodName = ClientMasterViewLoadMethodName;

            scriptsElements.Add(scriptElement);

            return true;
        }
    }
}
