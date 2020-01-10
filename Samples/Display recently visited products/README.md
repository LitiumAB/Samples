
# Display recently visited products

**Tested in Litium version: 7.2.3**

This sample shows how to create a service that will add and keep track of the products the user has recently visited for the session. The data is saved to a cookie. 

Per default, up to 4 products will be displayed. This is hard-coded, but could easily be changed to a website setting instead. The current product is not displayed. 

## Instructions

1. Copy the following files to `Src\Litium.Accelerator\Services`
	* `IRecentlyVisitedService.cs`
	* `RecentlyVisitedService.cs`
2. Copy `_RecentlyVisitedItems.cshtml` to `Src\Litium.Accelerator.Mvc\Views\Product`
3. Update `ProductPageViewModel` and add a property `List<ProductModel> RecentlyVisitedItems`
4. Inject the service into the `ProductPageViewModelBuilder` class and add the lines below to `Build(ProductModel productModel)` before it returns
    ```
    _recentlyVisitedService.Add(productModel);
    viewModel.RecentlyVisitedItems = _recentlyVisitedService.Get().Select(item => _itemViewModelBuilder.Build(item)).ToList();
    ```
5. Update your product display templates to include the partial view by adding `@Html.PartialView("_RecentlyVisitedItems", Model.RecentlyVisitedItems)` 
