using Litium.Foundation.Modules.ECommerce;
using Litium.Web.Administration.Panels;

namespace Litium.Accelerator.Mvc.Site.ECommerce.Panels
{
    /// <summary>
    /// Defines panel definition, to display it in EcommerceArea.
    /// </summary>
    public class SamplePanelDefinition : PanelDefinitionBase<EcommerceArea, SamplePanelDefinition.SettingsModel>
    {
        /// <summary>
        /// Points to the path of SamplePanelPage.aspx
        /// </summary>
        public override string Url => "https://localhost/Site/ECommerce/Panels/SamplePanelPage.aspx";

        public override string ComponentName => null;

        public override bool PermissionCheck()
        {
            return true;
        }

        public class SettingsModel : IPanelSettings
        {
        }
    }
}