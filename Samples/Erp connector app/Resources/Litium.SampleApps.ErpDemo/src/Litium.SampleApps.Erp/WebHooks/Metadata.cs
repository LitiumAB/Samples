using System;

namespace Litium.SampleApps.Erp.WebHooks
{
    public class Metadata
    {
        public int Attempt { get; set; }

        public Guid EventSystemId { get; set; }

        public string RegistrationId { get; set; }
    }
}