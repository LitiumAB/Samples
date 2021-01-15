using Litium.Connect.Erp;
using System;
using System.Threading.Tasks;

namespace Litium.SampleApps.Erp.Rmas
{
    public interface ILitiumRmaService
    {
        Task<Rma> BuildRmaFromReturnSlipAsync(ReturnSlip returnSlip);
        Task<Rma> SetStatePackageReceivedAsync(Guid rmaSystemId);
        Task<Rma> SetStateProcessingAsync(Guid rmaSystemId);
        Task<Rma> ApproveRmaAsync(Guid rmaSystemId);
        Task<Order> GetOrderAsync(Guid rmaSystemId);
    }
}