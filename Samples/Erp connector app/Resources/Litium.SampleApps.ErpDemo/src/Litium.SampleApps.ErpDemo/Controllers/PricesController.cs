using Litium.Connect.Erp;
using Litium.SampleApps.Erp.PriceLists;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Litium.SampleApps.ErpDemo.Controllers
{
    [RoutePrefix("prices")]
    public class PricesController : ApiController
    {
        private readonly ILitiumPriceListService _litiumPriceListService;

        public PricesController(ILitiumPriceListService litiumPriceListService)
        {
            _litiumPriceListService = litiumPriceListService;
        }
        
        /// <summary>
        /// Gets the price list.
        /// </summary>
        /// <param name="priceListId">The price list identifier.</param>
        [Route("{priceListId}", Name = "GetPriceListById")]
        [HttpGet]
        [ResponseType(typeof(PriceList))]
        public async Task<IHttpActionResult> Get(string priceListId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            return Ok(await _litiumPriceListService.Get(priceListId));
        }

        /// <summary>
        /// Update price list items for the given price list.
        /// </summary>
        /// <param name="priceListId">The price list identifier.</param>
        /// <param name="priceListItems">The price list items</param>
        [Route("{priceListId}", Name = "UpdatePriceListItemsById")]
        [HttpPost]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Update(string priceListId, [FromBody] List<PriceListItem> priceListItems)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _litiumPriceListService.Update(priceListId, priceListItems);

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Delete price list items for the given price list.
        /// </summary>
        /// <param name="inventoryId">The price list identifier.</param>
        /// <param name="inventoryItems">The price list items</param>
        [Route("{priceListId}", Name = "DeletePriceListItemsById")]
        [HttpDelete]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Delete(string priceListId, [FromBody] List<PriceListItem> priceListItems)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _litiumPriceListService.Delete(priceListId, priceListItems);

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }
    }
}
