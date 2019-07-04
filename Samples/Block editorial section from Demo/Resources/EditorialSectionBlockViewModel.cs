using AutoMapper;
using JetBrains.Annotations;
using Litium.Accelerator.Constants;
using Litium.FieldFramework;
using Litium.Accelerator.Extensions;
using Litium.FieldFramework.FieldTypes;
using Litium.Runtime.AutoMapper;
using Litium.Web.Models;
using System;
using System.Globalization;
using Litium.Accelerator.Builders;
using Litium.Web.Models.Blocks;

namespace Litium.Accelerator.ViewModels.Block
{
    public class EditorialSectionBlockViewModel : IViewModel, IAutoMapperConfiguration
    {
        public ImageModel Image { get; set; }
        public string ActionText { get; set; }
        public string LinkUrl { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }

        [UsedImplicitly]
        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<BlockModel, EditorialSectionBlockViewModel>()
                .ForMember(x => x.Title, m => m.MapFromField(BlockFieldNameConstants.BlockTitle))
                .ForMember(x => x.ActionText, m => m.MapFromField(BlockFieldNameConstants.LinkText))
                .ForMember(x => x.Image, m => m.MapFrom(c => c.Fields.GetValue<Guid>(BlockFieldNameConstants.BlockImagePointer, CultureInfo.CurrentUICulture).MapTo<ImageModel>()))
                .ForMember(x => x.Text, m => m.MapFromField(BlockFieldNameConstants.BlockBodyText))
                .ForMember(x => x.LinkUrl, m => m.MapFrom<LinkUrlResolver>());
        }

        [UsedImplicitly]
        private class LinkUrlResolver : IValueResolver<BlockModel, EditorialSectionBlockViewModel, string>
        {
            public string Resolve(BlockModel source, EditorialSectionBlockViewModel destination, string destMember, ResolutionContext context)
            {
                var linkToPage = source.Fields.GetValue<PointerPageItem>(BlockFieldNameConstants.LinkToPage, CultureInfo.CurrentUICulture).MapTo<LinkModel>();
                if (!string.IsNullOrEmpty(linkToPage?.Href))
                {
                    return linkToPage.Href;
                }
                return "";
            }
        }
    }
}
