# Block hero banner from Demo

**Tested in Litium version: 7.2.3**

Source code for implementing the *Hero banner* block from the Litium demo site. This is a full width banner block with a customizable CTA button. 

## Instructions

1. Copy `HeroBannerBlockTemplateSetup.cs` to `\Src\Litium.Accelerator\Definitions\Blocks` and include.
2. Add translations for the block to `\Src\Litium.Accelerator.Mvc\Site\Resources\Administration\Administration.resx`
    1. **key**=`fieldtemplate.blockarea.herobanner.name` **value**=`Hero banner`
    2. **key**=`fieldtemplate.blockarea.herobanner.fieldgroup.general.name` **value**=`General`
3. Copy `HeroBannerBlockViewModel.cs` to `\Src\Litium.Accelerator\ViewModels\Block` and include.
4. Copy `HeroBannerBlockViewModelBuilder.cs` to `\Src\Litium.Accelerator\Builders\Block` and include.
5. Copy `HeroBannerBlockController.cs` to `\Src\Litium.Accelerator.Mvc\Controllers\Blocks` and include.
6. Copy `HeroBanner.cshtml` to `\Src\Litium.Accelerator.Mvc\Views\Block` and include. 
7. Add the constant `HeroBannerBlock` with value `HeroBanner` to `Litium.Accelerator.Constants.BlockTemplateNameConstants`
8. Add the constant `BlockBodyText` with value `BlockBodyText` to `Litium.Accelerator.Constants.BlockFieldNameConstants`
9. Add the Block to the `_controllerMapping` list in `Litium.Accelerator.Mvc.Definitions.FieldTemplateSetupDecorator`, set type to `Blocks.BlockArea` and reference the controller you added in a previous step
10. Copy `hero.scss` to `\Src\Litium.Accelerator.Mvc\Client\Styles\DefaultTheme\Block` and import it into `index.scss` 
11. Build the client project and make any changes needed to the scss file to match your solution
12. Build solution