using System;
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

namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    public class InstallationsHelper
    {
        private const string WidgetBarSectionName = "toolbar";
        private const string MoreActionsWidgetName = "MoreActionsWidget";
        private const string DownloadSelectedImagesCommandName = "DownloadSelectedImages";
        private const string DownloadSelectedVideosCommandName = "DownloadSelectedVideos";
        private const string DownloadSelectedDocumentsCommandName = "DownloadSelectedVideos";

        public void Initialize()
        {
            ConfigManager manager = ConfigManager.GetManager();

            // In initialize the context is not authenticated to save.
            using (ElevatedModeRegion elevatedMode = new ElevatedModeRegion(manager))
            {
                LibrariesConfig libsConfig = manager.GetSection<LibrariesConfig>();
                this.ConfigureSection(manager, libsConfig, ImagesDefinitions.BackendImagesDefinitionName, ImagesDefinitions.BackendListViewName, DownloadSelectedImagesCommandName, "Download selected images");
                this.ConfigureSection(manager, libsConfig, VideosDefinitions.BackendVideosDefinitionName, VideosDefinitions.BackendListViewName, DownloadSelectedVideosCommandName, "Download selected videos");
                this.ConfigureSection(manager, libsConfig, DocumentsDefinitions.BackendDefinitionName, DocumentsDefinitions.BackendListViewName, DownloadSelectedDocumentsCommandName, "Download selected documents");
            }
        }

        public void ConfigureSection(ConfigManager manager, LibrariesConfig libsConfig, string definitionName, string backendListViewName, string commandName, string commandText)
        {
            if (!libsConfig.ContentViewControls.ContainsKey(definitionName))
            {
                string exceptionMessage = string.Format("Cannot find content view control: {0} related with libraries configuration.", definitionName);
                throw new NullReferenceException(exceptionMessage);
            }

            ContentViewControlElement imagesBackend = libsConfig.ContentViewControls[definitionName];
            if (!imagesBackend.ContainsView(backendListViewName))
            {
                string exceptionMessage = string.Format("Cannot find back-end view: {0}.", backendListViewName);
                throw new NullReferenceException(exceptionMessage);
            }

            ContentViewDefinitionElement view = imagesBackend.ViewsConfig[backendListViewName];

            Assembly itemsDownloaderAssembly = typeof(Installer).Assembly;
            string javascriptKey = this.GetJavaScriptQualifiedNameKey(itemsDownloaderAssembly);

            #region Scripts
            if (!view.Scripts.ContainsKey(javascriptKey))
            {
                var scriptElement = new ClientScriptElement(view.Scripts);
                scriptElement.ScriptLocation = javascriptKey;
                scriptElement.LoadMethodName = "OnMasterViewLoadedCustom";
                view.Scripts.Add(scriptElement);

                manager.SaveSection(imagesBackend.Section, true);
            }
            #endregion

            #region Toolbar
            MasterGridViewElement masterV = view as MasterGridViewElement;

            WidgetBarElement toolbarConfig = (masterV.ToolbarConfig as WidgetBarElement);
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
                manager.SaveSection(imagesBackend.Section, true);
            }

            #endregion
        }

        public string GetJavaScriptQualifiedNameKey(Assembly libraryItemsDownloaderAssembly)
        {
            var fullNamespaceJavaScriptFile = libraryItemsDownloaderAssembly.GetManifestResourceNames().FirstOrDefault(fileName => fileName.EndsWith("LibraryItemsDownloadService.js"));
            if (fullNamespaceJavaScriptFile == null)
            {
                throw new NullReferenceException("Cannot find embedded file LibraryItemsDownloadService.js");
            }

            string javaScriptQualifiedNameKey = Assembly.CreateQualifiedName(libraryItemsDownloaderAssembly.FullName, fullNamespaceJavaScriptFile);
            return javaScriptQualifiedNameKey;
        }
    }
}
