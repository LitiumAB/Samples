using Litium.Connect.Erp;
using Litium.SampleApps.Erp.Rmas;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Litium.SampleApps.ErpDemo.Controllers
{
    [RoutePrefix("rmas")]
    public class RmasController : ApiController
    {
        private readonly ILitiumRmaService _litiumRmaService;

        public RmasController(ILitiumRmaService litiumRmaService)
        {
            _litiumRmaService = litiumRmaService;
        }

        /// <summary>
        /// Gets the order which is composed with RMA.
        /// </summary>
        /// <param name="systemId">The rma system identifier.</param>
        [Route("{systemId}/order", Name = "Erp_Rma_GetOrderByRmaSystemId")]
        [HttpGet]
        [ResponseType(typeof(Order))]
        public async Task<IHttpActionResult> GetOrder(Guid systemId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            return Ok(await _litiumRmaService.GetOrderAsync(systemId));
        }

        /// <summary>
        /// Build RMA from information in the return slip.
        /// </summary>
        /// <param name="returnSlip">The return slip.</param>
        [Route("", Name = "Erp_Rma_BuildByReturnSlip")]
        [HttpPost]
        [ResponseType(typeof(Rma))]
        public async Task<IHttpActionResult> BuildFromReturnSlip([FromBody]ReturnSlip returnSlip)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok(await _litiumRmaService.BuildRmaFromReturnSlipAsync(returnSlip));
        }

        /// <summary>
        /// Set the rma state to processing.
        /// </summary>
        /// <param name="systemId">The rma system identifier.</param>
        [Route("{systemid}/notify/processing", Name = "Erp_Rma_SetStateProcessing")]
        [HttpPost]
        [ResponseType(typeof(Rma))]
        public async Task<IHttpActionResult> SetStateProcessing(Guid systemId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok(await _litiumRmaService.SetStateProcessingAsync(systemId));
        }

        /// <summary>
        /// Set the rma state to package received.
        /// </summary>
        /// <param name="systemId">The rma system identifier.</param>
        /// <returns></returns>
        [Route("{systemid}/notify/packageReceived", Name = "Erp_Rma_SetStatePackageReceived")]
        [HttpPost]
        [ResponseType(typeof(Rma))]
        public async Task<IHttpActionResult> SetStatePackageReceived(Guid systemId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok(await _litiumRmaService.SetStatePackageReceivedAsync(systemId));
        }

        // <summary>
        /// Approves the RMA.
        /// </summary>
        /// <param name="systemId">The rma system identifier.</param>
        [Route("{systemid}/approve", Name = "Erp_Rma_ApproveRma")]
        [HttpPost]
        [ResponseType(typeof(Rma))]
        public async Task<IHttpActionResult> ApproveRma(Guid systemId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok(await _litiumRmaService.ApproveRmaAsync(systemId));
        }
    }
}