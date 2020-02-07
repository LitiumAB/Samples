using System;

namespace Litium.Accelerator.Mvc.Site.ECommerce.Panels
{
    public partial class SamplePanel : System.Web.UI.UserControl
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            Heading.InnerText = "Hello world " + Guid.NewGuid().ToString("N");
        }
    }
}