# Block editorial section from Demo

**Tested in Litium version: 7.2.3**

Source code for implementing the *Editorial section* block from the Litium demo site. This is a full width block with both a banner area and a text section. 

## Instructions

1. Copy `EditorialSectionBlockTemplateSetup.cs` to `\Src\Litium.Accelerator\Definitions\Blocks` and include.
2. Copy `EditorialSectionBlock.resx` to `\Src\Litium.Accelerator.Mvc\Site\Resources\Administration\` and include.
3. Copy `EditorialSectionBlockViewModel.cs` to `\Src\Litium.Accelerator\ViewModels\Block` and include.
4. Copy `EditorialSectionBlockViewModelBuilder.cs` to `\Src\Litium.Accelerator\Builders\Block` and include.
5. Copy `EditorialSectionBlockController.cs` to `\Src\Litium.Accelerator.Mvc\Controllers\Blocks` and include.
6. Copy `EditorialSection.cshtml` to `\Src\Litium.Accelerator.Mvc\Views\Block` and include. 
7. Add the constant `EditorialSectionBlock` with value `EditorialSection` to `Litium.Accelerator.Constants.BlockTemplateNameConstants`
8. Add the constant `BlockBodyText` with value `BlockBodyText` to `Litium.Accelerator.Constants.BlockFieldNameConstants`
9. Add the Block to the `_controllerMapping` list in `Litium.Accelerator.Mvc.Definitions.FieldTemplateSetupDecorator`, set type to `Blocks.BlockArea` and reference the controller you added in a previous step
10. Copy `editorialblock.scss` to `\Src\Litium.Accelerator.Mvc\Client\Styles\DefaultTheme\Block` and import it into `index.scss` 
11. Build the client project and make any changes needed to the scss file to match your solution
12. Build solution