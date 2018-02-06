using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Modules.Libraries.Configuration;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Web.UI.ContentUI.Config;

namespace Sitefinity.LibraryItemsDownloader.Helpers
{
    public class InstallationsHelper
    {
        public static void Initialize()
        {
            var manager = ConfigManager.GetManager();

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

                if (!view.Scripts.ContainsKey(key))
                {
                    var kc = new ClientScriptElement(view.Scripts);
                    kc.ScriptLocation = key;
                    kc.LoadMethodName = "OnMasterViewLoadedCustom";
                    view.Scripts.Add(kc);

                    // In initialize the context is not authenticated to save.
                    manager.SaveSection(a.Section, true);
                }
            }
        }
    }
}
