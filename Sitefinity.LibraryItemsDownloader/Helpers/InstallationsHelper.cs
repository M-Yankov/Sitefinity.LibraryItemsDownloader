using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Modules.Libraries.Configuration;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Web.UI.Backend.Elements.Config;
using Telerik.Sitefinity.Web.UI.Backend.Elements.Widgets;
using Telerik.Sitefinity.Web.UI.ContentUI.Config;
using Telerik.Sitefinity.Web.UI.ContentUI.Views.Backend.Master.Config;

namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    public class InstallationsHelper
    {
        public static void Initialize()
        {
            var manager = ConfigManager.GetManager();

            // In initialize the context is not authenticated to save.
            using (ElevatedModeRegion elevatedMode = new ElevatedModeRegion(manager))
            {
                var libsConfig = manager.GetSection<LibrariesConfig>();
                if (!libsConfig.ContentViewControls.ContainsKey("ImagesBackend"))
                {
                    return;// Error!;
                }

                var a = libsConfig.ContentViewControls["ImagesBackend"];
                if (!a.ContainsView("ImagesBackendList"))
                {
                    return; // Error; 
                }

                var view = a.ViewsConfig["ImagesBackendList"];

                var ass = typeof(Installer).Assembly;
                var fullNamespaceJavaScriptFile = ass.GetManifestResourceNames().FirstOrDefault(fileName => fileName.EndsWith("LibraryItemsDownloadService.js"));
                if (fullNamespaceJavaScriptFile == null)
                {
                    return; // Error; 
                }

                string key = Assembly.CreateQualifiedName(ass.FullName, fullNamespaceJavaScriptFile);

                #region Scripts
                // var moreActionWidget = toolbar.Sections.Elements.FirstOrDefault(w => w.Name == "MoreActionsWidget");
                // ActionMenuWidgetElement
                if (!view.Scripts.ContainsKey(key))
                {
                    var kc = new ClientScriptElement(view.Scripts);
                    kc.ScriptLocation = key;
                    kc.LoadMethodName = "OnMasterViewLoadedCustom";
                    view.Scripts.Add(kc);


                    manager.SaveSection(a.Section, true);
                }
                #endregion

                #region Toolbar
                MasterGridViewElement masterV = view as MasterGridViewElement;
                // masterV.ToolbarConfig.Sections.Add(new WidgetBarSectionElement(null));

                WidgetBarElement toolbarConfig = (masterV.ToolbarConfig as WidgetBarElement);

                WidgetBarSectionElement widgetBarSectionElement = toolbarConfig.Sections.OfType<WidgetBarSectionElement>().FirstOrDefault(s => s.Name == "toolbar");
                ActionMenuWidgetElement moreActionsWidgetElement = widgetBarSectionElement.Items.OfType<ActionMenuWidgetElement>().FirstOrDefault(w => w.Name == "MoreActionsWidget");
                CommandWidgetElement downloadSelectedImagesCommand = moreActionsWidgetElement.MenuItems.OfType<CommandWidgetElement>().FirstOrDefault(c => c.Name == "DownloadSelectedImages");

                if (downloadSelectedImagesCommand == null)
                {
                    downloadSelectedImagesCommand = new CommandWidgetElement(moreActionsWidgetElement.MenuItems);
                    downloadSelectedImagesCommand.Name = "DownloadSelectedImages";
                    downloadSelectedImagesCommand.CssClass = "sfDownloadSelectedImages sfDownloadItm";
                    downloadSelectedImagesCommand.ButtonType = Telerik.Sitefinity.Web.UI.Backend.Elements.Enums.CommandButtonType.Standard;
                    downloadSelectedImagesCommand.WrapperTagId = System.Web.UI.HtmlTextWriterTag.Li.ToString();
                    downloadSelectedImagesCommand.WrapperTagKey = System.Web.UI.HtmlTextWriterTag.Li;
                    downloadSelectedImagesCommand.WidgetType = typeof(CommandWidget);
                    downloadSelectedImagesCommand.CommandName = "DownloadSelectedImages";
                    downloadSelectedImagesCommand.Text = "Download Selected Images";

                    moreActionsWidgetElement.MenuItems.Add(downloadSelectedImagesCommand);
                    manager.SaveSection(a.Section, true);
                }

                #endregion
            }
        }

        private static string GetConfigAsString(CommandWidgetElement widget)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("\nActionName: {0}", widget.ActionName);
            sb.AppendFormat("\nButtonCssClass: {0}", widget.ButtonCssClass);
            sb.AppendFormat("\nButtonType: {0}", widget.ButtonType);
            sb.AppendFormat("\nCollectionItemName: {0}", widget.CollectionItemName);
            sb.AppendFormat("\nCommandArgument: {0}", widget.CommandArgument);
            sb.AppendFormat("\nCommandName: {0}", widget.CommandName);
            sb.AppendFormat("\nCondition: {0}", widget.Condition);
            sb.AppendFormat("\nContainerId: {0}", widget.ContainerId);
            sb.AppendFormat("\nCssClass: {0}", widget.CssClass);
            sb.AppendFormat("\nGetKey(): {0}", widget.GetKey());
            sb.AppendFormat("\nGetPath(): {0}", widget.GetPath());
            sb.AppendFormat("\nIsFilterCommand:{0}", widget.IsFilterCommand);
            sb.AppendFormat("\nIsSeparator: {0}", widget.IsSeparator);
            sb.AppendFormat("\nModuleName: {0}", widget.ModuleName);
            sb.AppendFormat("\nName: {0}", widget.Name);
            sb.AppendFormat("\nNavigateUrl: {0}", widget.NavigateUrl);
            sb.AppendFormat("\nObjectProviderName: {0}", widget.ObjectProviderName);
            sb.AppendFormat("\nOpenInSameWindow: {0}", widget.OpenInSameWindow);
            sb.AppendFormat("\nParent: {0}", widget.Parent != null ? widget.Parent.TagName : "Null");
            sb.AppendFormat("\nPermissionSet: {0}", widget.PermissionSet);
            sb.AppendFormat("\nProperties: [\"{0}\"],", string.Join("\",\"", (widget.Properties ?? Enumerable.Empty<ConfigProperty>()).Select(p => p.Name)));
            sb.AppendFormat("\nRelatedSecuredObjectId: {0}", widget.RelatedSecuredObjectId);
            sb.AppendFormat("\nRelatedSecuredObjectManagerTypeName: {0}", widget.RelatedSecuredObjectManagerTypeName);
            sb.AppendFormat("\nRelatedSecuredObjectProviderName: {0}", widget.RelatedSecuredObjectProviderName);
            sb.AppendFormat("\nRelatedSecuredObjectTypeName: {0}", widget.RelatedSecuredObjectTypeName);
            sb.AppendFormat("\nRequiredActions: {0}", widget.RequiredActions);
            sb.AppendFormat("\nRequiredPermissionSet: {0}", widget.RequiredPermissionSet);
            sb.AppendFormat("\nResourceClassId: {0}", widget.ResourceClassId);
            sb.AppendFormat("\nSection: {0}", widget.Section != null ? widget.Section.TagName : "NUll");
            sb.AppendFormat("\nTagName: {0}", widget.TagName);
            sb.AppendFormat("\nText: {0}", widget.Text);
            sb.AppendFormat("\nToolTip: {0}", widget.ToolTip);
            sb.AppendFormat("\nVisible: {0}", widget.Visible);
            sb.AppendFormat("\nWidgetType: {0}", widget.WidgetType != null ? widget.WidgetType.FullName : "NULL");
            sb.AppendFormat("\nWidgetVirtualPath: {0}", widget.WidgetVirtualPath);
            sb.AppendFormat("\nWrapperTagId: {0}", widget.WrapperTagId);
            sb.AppendFormat("\nWrapperTagKey: {0}", widget.WrapperTagKey);

            return sb.ToString();
        }
    }
}
