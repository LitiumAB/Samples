using Litium.Accelerator.ViewModels.Block;
using Litium.Runtime.AutoMapper;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Models.Blocks;

namespace Litium.Accelerator.Builders.Block
{
    [Service(ServiceType = typeof(EditorialSectionBlockViewModelBuilder))]
    public class EditorialSectionBlockViewModelBuilder : IViewModelBuilder<EditorialSectionBlockViewModel>
    {
        /// <summary>
        /// Build the editorial section block view model
        /// </summary>
        /// <param name="blockModel">The current editorial section block</param>
        /// <returns>Return the editorial section block view model</returns>
        public virtual EditorialSectionBlockViewModel Build(BlockModel blockModel)
        {
            return blockModel.MapTo<EditorialSectionBlockViewModel>();
        }
    }
}
