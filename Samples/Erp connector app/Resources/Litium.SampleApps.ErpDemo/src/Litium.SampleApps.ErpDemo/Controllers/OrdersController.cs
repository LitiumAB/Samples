using Litium.Connect.Erp;
using Litium.SampleApps.Erp.Orders;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Litium.SampleApps.ErpDemo.Controllers
{
    [RoutePrefix("orders")]
    public class OrdersController : ApiController
    {
        private readonly ILitiumOrderService _litiumOrderService;
        public OrdersController(ILitiumOrderService litiumOrderService)
        {
            _litiumOrderService = litiumOrderService;
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <param name="orderId">The external order identifier.</param>
        [Route("{orderId}", Name = "GetOrderById")]
        [HttpGet]
        [ResponseType(typeof(Order))]
        public async Task<IHttpActionResult> Get(string orderId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            return Ok(await _litiumOrderService.GetOrderAsync(orderId));
        }



        /// <summary>
        /// Set all delivery state to processing after export order to ERP.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        [Route("{orderId}/orderExportedToErp", Name = "OrderExportedToErp")]
        [HttpPost]
        [ResponseType(typeof(Order))]
        public async Task<IHttpActionResult> OrderExportedToErp(string orderId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok(await _litiumOrderService.ExportedToErpAsync(orderId));
        }

        /// <summary>
        /// Creates a delivery.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <param name="shipment">The shipment information.</param>
        [Route("{orderId}/shipment")]
        [HttpPost]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> CreateShipment(string orderId, [FromBody]Shipment shipment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok(await _litiumOrderService.CreateShipmentAsync(orderId, shipment));
        }

        /// <summary>
        /// Notify order has being delivered to end customer.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <param name="deliveryExtRefId">The delivery identifier.</param>
        [Route("{orderId}/deliveries/{deliveryId}/notify/delivered")]
        [HttpPost]
        [ResponseType(typeof(Order))]
        public async Task<IHttpActionResult> NotifyOrderDelivered(string orderId, string deliveryId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok(await _litiumOrderService.NotifyOrderDeliveredAsync(orderId, deliveryId));
        }

        /// <summary>
        /// Confirm the sales return order. 
        /// In implementation, this should send a mail to the end-customer about the return as confirmed, and refund is under processing.
        /// </summary>
        /// <param name="orderId">The sales return order identifier.</param>
        [Route("{orderId}/confirmReturn", Name = "ConfirmReturnBySalesReturnOrderId")]
        [HttpPost]
        [ResponseType(typeof(Order))]
        public async Task<IHttpActionResult> ConfirmReturn(string orderId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok(await _litiumOrderService.ConfirmReturnOrderAsync(orderId));
        }

        /// <summary>
        /// Refund the money to customer, and finish the sales return order process.
        /// </summary>
        /// <param name="orderId"> Sales return order identifier.</param>
        [Route("{orderId}/refund", Name = "RefundBySalesReturnOrderId")]
        [HttpPost]
        [ResponseType(typeof(Order))]
        public async Task<IHttpActionResult> Refund(string orderId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok(await _litiumOrderService.RefundOrderAsync(orderId));
        }
    }
}
