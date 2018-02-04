using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Web.UI;
using Telerik.Sitefinity.Web.UI.ContentUI.Contracts;
using Telerik.Sitefinity.Web.UI.Backend.Elements;
using Telerik.Sitefinity.Web.UI.Backend;
using Telerik.Sitefinity.Web.UI.Backend.Elements.Widgets;
using Telerik.Web.UI;
using System.Web.UI;

namespace SitefinityWebApp.Custom
{
    public class LibrariesMasterGridViewExtended : LibrariesMasterGridView
    {
        private const string ToolbarId = "contextBar";
        private const string ActionsMenuName = "FolderActionsWidget";

        protected override void InitializeControls(GenericContainer container, IContentViewDefinition definition)
        {
            base.InitializeControls(container, definition);
            if (this.Page.IsPostBack)
            {
                return;
            }

            WidgetBar widgetBar = container.GetControl<WidgetBar>(ToolbarId, true);
                     
            IWidget widget = widgetBar.Widgets.FirstOrDefault(w => w.Definition.Name == ActionsMenuName);

            string path = widgetBar.LayoutTemplatePath;
            ActionMenuWidget actionMenuWidget = (ActionMenuWidget)widget;
            string path2 = actionMenuWidget.LayoutTemplatePath;
            // actionMenuWidget.

            // var extendedMenu = this.Container.GetControl<RadMenu>("menuButton", false);

           //RadMenu menu = GetControlRecourcive<RadMenu>(actionMenuWidget, "menuButton");
           // menu.ItemClick += this.Menu_ItemClick;
        }

        private void Menu_ItemClick(object sender, RadMenuEventArgs e)
        {
            throw new NotImplementedException();
        }

        public T GetControlRecourcive<T> (Control control, string controlId) where T: Control
        {
            T result = null;

            // control.HasControls() - Not working well.
            if (control.Controls != null && control.Controls.Count > 0)
            {
                Control tempControl = control.FindControl(controlId);
                if (tempControl is T)
                {
                    return (T)tempControl;
                }

                foreach (Control controlItem in control.Controls)
                {
                    result = this.GetControlRecourcive<T>(controlItem, controlId);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return result;
        }
    }
}