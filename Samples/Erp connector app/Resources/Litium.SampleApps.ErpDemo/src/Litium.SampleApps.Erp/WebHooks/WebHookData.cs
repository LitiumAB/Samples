using System.Collections.Generic;

namespace Litium.SampleApps.Erp.WebHooks
{
    public class WebHookData
    {
        public string Action { get; set; }

        public Dictionary<string, object> Properties { get; set; }

        public object Resource { get; set; }

        public Metadata Metadata { get; set; }
    }
}