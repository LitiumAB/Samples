using System;
using Litium.Accelerator.Constants;
using Litium.Blocks;
using Litium.FieldFramework;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.Blocks
{
    internal class EditorialSectionBlockTemplateSetup : FieldTemplateSetup
    {
		private readonly CategoryService _categoryService;

		public EditorialSectionBlockTemplateSetup(CategoryService categoryService)
		{
			_categoryService = categoryService;
		}

        public override IEnumerable<FieldTemplate> GetTemplates()
        {
			var pageCategoryId = _categoryService.Get(BlockCategoryNameConstants.Pages)?.SystemId ?? Guid.Empty;

			var templates = new List<FieldTemplate>
            {
                new BlockFieldTemplate(BlockTemplateNameConstants.EditorialSectionBlock)
                {
                    CategorySystemId = pageCategoryId,
					Icon = "fas fa-indent",
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
                                BlockFieldNameConstants.LinkText,
                                BlockFieldNameConstants.BlockBodyText

                            }
                        }
                    }
                }
            };
            return templates;
        }
    }
}
