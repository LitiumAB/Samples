using Litium.Connect.Erp;
using Litium.SampleApps.Erp.Inventories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Litium.SampleApps.ErpDemo.Controllers
{
    [RoutePrefix("inventories")]
    public class InventoriesController : ApiController
    {
        private readonly ILitiumInventoryService _litiumInventoryService;

        public InventoriesController(ILitiumInventoryService litiumInventoryService)
        {
            _litiumInventoryService = litiumInventoryService;
        }

        /// <summary>
        /// Gets the inventory.
        /// </summary>
        /// <param name="inventoryId">The inventory identifier.</param>
        [Route("{inventoryId}", Name = "GetInventoryById")]
        [HttpGet]
        [ResponseType(typeof(Inventory))]
        public async Task<IHttpActionResult> Get(string inventoryId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            return Ok(await _litiumInventoryService.Get(inventoryId));
        }

        /// <summary>
        /// Update inventory items for the given inventory.
        /// </summary>
        /// <param name="inventoryId">The inventory identifier.</param>
        /// <param name="inventoryItems">The inventory items</param>
        [Route("{inventoryId}", Name = "UpdateInventoryItemsById")]
        [HttpPost]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Update(string inventoryId, [FromBody] List<InventoryItem> inventoryItems)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _litiumInventoryService.Update(inventoryId, inventoryItems);

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Delete inventory items for the given inventory.
        /// </summary>
        /// <param name="inventoryId">The inventory identifier.</param>
        /// <param name="inventoryItems">The inventory items</param>
        [Route("{inventoryId}", Name = "DeleteInventoryItemsById")]
        [HttpDelete]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Delete(string inventoryId, [FromBody] List<InventoryItem> inventoryItems)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _litiumInventoryService.Delete(inventoryId, inventoryItems);

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }
    }
}
