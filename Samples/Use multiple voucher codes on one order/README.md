# Use multiple voucher codes on a single order

**Tested in Litium version: 7.6.0**

By default, the voucher code condition included with the Litium platform only supports 1 voucher code per order. This sample campaign condition is very similar but has some adjusted logic to evaluate multiple codes. 

By default, the actual voucher code is saved as a string to the `OrderCarrier.CampaignInfo` property. In this sample, we are saving multiple codes in a single string, separated by `|`. When the condition is processed, this string is split into multiple codes and a check is made if any of them is valid for the campaign. 

Besides the condition itself, some changes are needed in `CheckoutService` as well, in `SetCampaignCode`. This is done via a [service decorator](https://docs.litium.com/documentation/architecture/dependency-injection/service-decorator) in this sample. 

This sample includes the condition and re-uses the web control from the original voucher code condition. It does not include any front end changes. For testing purposes, you could add a code, then clear the code input box to add another. 

## Instructions

1. Copy `MultipleVoucherCodeCondition.cs` to `Src\Litium.Accelerator.Mvc\Campaigns` and include in the project.<sup>1</sup> 
1. Copy `MultipleVoucherCodeCondition.resx` and `MultipleVoucherCodeCondition.sv-se.resx` to `\Src\Litium.Accelerator.Mvc\Site\Resources\ModuleECommerce` and include. (Used for adding a condition name in the dropdown)
1. Copy `CheckoutServiceDecorator.cs` to `Src\Litium.Accelerator\Services` and include. <sup>1</sup> This decorator will adjust the logic of `SetCampaignCode` so that instead of setting `.CampaignInfo` to `string.Empty` you save the current saved codes. Before calculating the new order total you join the current codes with the new code. If the new code is not valid, you set `.CampaignInfo` to the saved codes from before. 

<sup>1</sup> Best practice is instead to keep your additions in a separate project, as described here: https://docs.litium.com/documentation/litium-documentation/sales/campaigns/creating-a-custom-condition_1_1