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
        /// Defines Component name as null so <see cref="Url"/> is used.
        /// </summary>
        public override string ComponentName => null;

        /// <summary>
        /// Defines the Url to be loaded by pointing to the path of SamplePanelPage.aspx.
        /// More information: https://docs.litium.com/documentation/architecture/back-office_1/creating-custom-panel
        /// </summary>
        public override string Url => "/Site/ECommerce/Panels/SamplePanelPage.aspx";

        public override bool PermissionCheck()
        {
            return true;
        }

        public class SettingsModel : IPanelSettings
        {
        }
    }
}
