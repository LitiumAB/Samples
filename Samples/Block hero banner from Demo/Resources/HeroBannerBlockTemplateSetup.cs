using System;
using Litium.Accelerator.Constants;
using Litium.Blocks;
using Litium.FieldFramework;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.Blocks
{
    internal class HeroBannerBlockTemplateSetup : FieldTemplateSetup
    {
		private readonly CategoryService _categoryService;

		public HeroBannerBlockTemplateSetup(CategoryService categoryService)
		{
			_categoryService = categoryService;
		}

        public override IEnumerable<FieldTemplate> GetTemplates()
        {
			var pageCategoryId = _categoryService.Get(BlockCategoryNameConstants.Pages)?.SystemId ?? Guid.Empty;

			var templates = new List<FieldTemplate>
            {
                new BlockFieldTemplate(BlockTemplateNameConstants.HeroBannerBlock)
                {
					CategorySystemId = pageCategoryId,
					Icon = "far fa-image",
					FieldGroups = new []
                    {
                        new FieldTemplateFieldGroup()
                        {
                            Id = "General",
                            Collapsed = false,
                            Fields =
                            {
                                SystemFieldDefinitionConstants.Name,
                                BlockFieldNameConstants.BlockTitle,
                                BlockFieldNameConstants.BlockImagePointer,
                                BlockFieldNameConstants.LinkToPage,
                                BlockFieldNameConstants.LinkText

                            }
                        }
                    }
                }
            };
            return templates;
        }
    }
}
