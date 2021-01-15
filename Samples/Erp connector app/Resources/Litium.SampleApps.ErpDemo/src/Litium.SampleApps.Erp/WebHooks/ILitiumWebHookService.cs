namespace Litium.SampleApps.Erp.WebHooks
{
    public interface ILitiumWebHookService
    {
        void AddOrUpdate(params string[] events);
    }
}
