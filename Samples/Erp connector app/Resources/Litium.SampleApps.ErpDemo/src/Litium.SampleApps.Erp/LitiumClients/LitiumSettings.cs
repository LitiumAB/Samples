using System.Configuration;

namespace Litium.SampleApps.Erp.LitiumClients
{
    public static class LitiumSettings
    {
        public static string Host = ConfigurationManager.AppSettings["WebHookSelfRegistrationHost"];
        public static string ClientId = ConfigurationManager.AppSettings["WebHookSelfRegistrationClientId"];
        public static string ClientSecret = ConfigurationManager.AppSettings["WebHookSelfRegistrationClientSecret"];
        public static string CallbackHost = ConfigurationManager.AppSettings["WebHookSelfRegistrationCallbackHost"];
        public static string Secret = ConfigurationManager.AppSettings["MS_WebHookReceiverSecret_Litium"];
    }
}
