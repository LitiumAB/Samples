using System.Web.Mvc;
using Litium.Accelerator.Builders.Block;
using Litium.Web.Models.Blocks;

namespace Litium.Accelerator.Mvc.Controllers.Blocks
{
    public class HeroBannerBlockController : ControllerBase
    {
        private readonly HeroBannerBlockViewModelBuilder _builder;

        public HeroBannerBlockController(HeroBannerBlockViewModelBuilder builder)
        {
            _builder = builder;
        }

        [HttpGet]
        public ActionResult Index(BlockModel currentBlockModel)
        {
            var model = _builder.Build(currentBlockModel);
            return PartialView("~/Views/Block/HeroBanner.cshtml", model);
        }
    }
}