using Litium.Connect.Erp;
using Litium.SampleApps.Erp.LitiumClients;
using System;
using System.Threading.Tasks;

namespace Litium.SampleApps.Erp.Rmas
{
    internal class LitiumRmaService : ILitiumRmaService
    {
        private readonly LitiumClientFactory _litiumClientFactory;

        public LitiumRmaService(LitiumClientFactory litiumClientFactory)
        {
            _litiumClientFactory = litiumClientFactory;
        }

        public async Task<Rma> ApproveRmaAsync(Guid rmaSystemId)
        {
            var client = await _litiumClientFactory.CreateConnectClient();
            return await client.Rmas_ApproveRmaAsync(rmaSystemId, null, null);
        }

        public async Task<Rma> BuildRmaFromReturnSlipAsync(ReturnSlip returnSlip)
        {
            var client = await _litiumClientFactory.CreateConnectClient();
            return await client.Rmas_BuildFromReturnSlipAsync(null, null, returnSlip);
        }

        public async Task<Order> GetOrderAsync(Guid rmaSystemId)
        {
            var client = await _litiumClientFactory.CreateConnectClient();
            return await client.Rmas_GetOrderAsync(rmaSystemId, null, null);
        }

        public async Task<Rma> SetStatePackageReceivedAsync(Guid rmaSystemId)
        {
            var client = await _litiumClientFactory.CreateConnectClient();
            return await client.Rmas_SetStatePackageReceivedAsync(rmaSystemId, null, null);
        }

        public async Task<Rma> SetStateProcessingAsync(Guid rmaSystemId)
        {
            var client = await _litiumClientFactory.CreateConnectClient();
            return await client.Rmas_SetStateProcessingAsync(rmaSystemId, null, null);
        }
    }
}