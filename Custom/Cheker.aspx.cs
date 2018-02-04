using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Sitefinity.Configuration.Web;
using Telerik.Sitefinity.Web.Utilities;

namespace SitefinityWebApp.Custom
{
    public partial class Cheker : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //var configSectionItems = new ConfigSectionItems();//                                                                                                                6                           5                              4                        3                      2                                1             0
            //var nodeName = "ClientScriptElement_Telerik.Sitefinity.Scheduling.Web.UI.Scripts.TaskCommands.js_Telerik.Sitefinity_Version=10.2.6602.0_Culture=neutral_PublicKeyToken=b28c218413bdf563,Scripts,ContentViewDefinitionElement_ImagesBackendList,ViewsConfig,ContentViewControlElement_ImagesBackend,ContentViewControls,librariesConfig";

            //var result = configSectionItems.GetConfigSetionItems(nodeName, string.Empty, string.Empty, null, "Form", null);

            var type = typeof(Sitefinity.LibraryItemsDownloader.Program);
            var manager = new ClientResourceManager();
            var resourceName = manager.GetEmbeddedResourceName("LibraryItemsDownloadService.js", true);
            this.Page.ClientScript.GetWebResourceUrl(type, resourceName);
        }
    }
}