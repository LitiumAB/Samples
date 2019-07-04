using System.Web.Mvc;
using Litium.Accelerator.Builders.Block;
using Litium.Web.Models.Blocks;

namespace Litium.Accelerator.Mvc.Controllers.Blocks
{
    public class EditorialSectionBlockController : ControllerBase
    {
        private readonly EditorialSectionBlockViewModelBuilder _builder;

        public EditorialSectionBlockController(EditorialSectionBlockViewModelBuilder builder)
        {
            _builder = builder;
        }

        [HttpGet]
        public ActionResult Index(BlockModel currentBlockModel)
        {
            var model = _builder.Build(currentBlockModel);
            return PartialView("~/Views/Block/EditorialSection.cshtml", model);
        }
    }
}