using Litium.Accelerator.ViewModels.Block;
using Litium.Runtime.AutoMapper;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Models.Blocks;

namespace Litium.Accelerator.Builders.Block
{
    [Service(ServiceType = typeof(HeroBannerBlockViewModelBuilder))]
    public class HeroBannerBlockViewModelBuilder : IViewModelBuilder<HeroBannerBlockViewModel>
    {
        /// <summary>
        /// Build the banner block view model
        /// </summary>
        /// <param name="blockModel">The current banner block banner</param>
        /// <returns>Return the banner block view model</returns>
        public virtual HeroBannerBlockViewModel Build(BlockModel blockModel)
        {
            return blockModel.MapTo<HeroBannerBlockViewModel>();
        }
    }
}
