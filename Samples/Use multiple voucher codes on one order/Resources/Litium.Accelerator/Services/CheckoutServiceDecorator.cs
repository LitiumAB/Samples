using Litium.Accelerator.Routing;
using Litium.Accelerator.ViewModels.Checkout;
using Litium.Foundation.Modules.ECommerce;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Foundation.Modules.ECommerce.Plugins.Payments;
using Litium.Foundation.Security;
using Litium.Runtime.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Litium.Accelerator.Services
{
	[ServiceDecorator(typeof(CheckoutService))]
	public class CheckoutServiceDecorator : CheckoutService
	{
		private readonly CheckoutService _parent;
		private readonly RequestModelAccessor _requestModelAccessor;
		private readonly ModuleECommerce _moduleECommerce;

		public CheckoutServiceDecorator(CheckoutService parent,
			RequestModelAccessor requestModelAccessor,
			ModuleECommerce moduleECommerce)
		{
			_parent = parent;
			_requestModelAccessor = requestModelAccessor;
			_moduleECommerce = moduleECommerce;
		}

		public override void ChangeDeliveryMethod(Guid deliveryMethodId)
		{
			_parent.ChangeDeliveryMethod(deliveryMethodId);
		}

		public override void ChangePaymentMethod(string paymentMethodId)
		{
			_parent.ChangePaymentMethod(paymentMethodId);
		}

		public override string GetOrderConfirmationPageUrl(OrderCarrier orderCarrier)
		{
			return _parent.GetOrderConfirmationPageUrl(orderCarrier);
		}

		public override string HandlePaymentResult(bool isSuccess, string errorMessage)
		{
			return _parent.HandlePaymentResult(isSuccess, errorMessage);
		}

		public override ExecutePaymentResult PlaceOrder(CheckoutViewModel model, string responseUrl, string cancelUrl, out string redirectUrl)
		{
			var result = _parent.PlaceOrder(model, responseUrl, cancelUrl, out string originalRedirectUrl);
			redirectUrl = originalRedirectUrl;
			return result;
		}

		public override bool SetCampaignCode(string campaignCode)
		{
			var currentOrderCarrier = _requestModelAccessor.RequestModel.Cart.OrderCarrier;

			// save any current codes
			var currentCampaignCodes = currentOrderCarrier.CampaignInfo;
			_moduleECommerce.Orders.CalculateOrderTotals(currentOrderCarrier, SecurityToken.CurrentSecurityToken);
			var originalOrderGrandTotal = currentOrderCarrier.GrandTotal;

			var campaignIdsWithoutVoucherCode = new List<Guid>();
			campaignIdsWithoutVoucherCode.AddRange(currentOrderCarrier.OrderRows.Select(row => row.CampaignID));
			campaignIdsWithoutVoucherCode.AddRange(currentOrderCarrier.Deliveries.Select(row => row.CampaignID));
			campaignIdsWithoutVoucherCode.AddRange(currentOrderCarrier.Fees.Select(row => row.CampaignID));
			campaignIdsWithoutVoucherCode.AddRange(currentOrderCarrier.OrderDiscounts.Select(row => row.CampaignID));

			// join the new code with any pre-existing ones
			currentOrderCarrier.CampaignInfo = string.Join("|", new[] { currentCampaignCodes, campaignCode.Trim() });
			_moduleECommerce.Orders.CalculateOrderTotals(currentOrderCarrier, SecurityToken.CurrentSecurityToken);
			var orderGrandTotalWithDiscount = currentOrderCarrier.GrandTotal;

			var campaignIdsWithVoucherCode = new List<Guid>();
			campaignIdsWithVoucherCode.AddRange(currentOrderCarrier.OrderRows.Select(row => row.CampaignID));
			campaignIdsWithVoucherCode.AddRange(currentOrderCarrier.Deliveries.Select(row => row.CampaignID));
			campaignIdsWithVoucherCode.AddRange(currentOrderCarrier.Fees.Select(row => row.CampaignID));
			campaignIdsWithVoucherCode.AddRange(currentOrderCarrier.OrderDiscounts.Select(row => row.CampaignID));

			//If campaignIdsWithVoucherCode list has a new campaign id then it means voucher code is valid
			var isValidVoucherCode = !campaignIdsWithVoucherCode.All(f => campaignIdsWithoutVoucherCode.Any(s => f == s));

			if (!isValidVoucherCode && orderGrandTotalWithDiscount >= originalOrderGrandTotal)
			{
				// restore the saved codes from before
				currentOrderCarrier.CampaignInfo = currentCampaignCodes;
				return false;
			}

			return true;
		}

		public override void SetOrderDetails(CheckoutViewModel model)
		{
			_parent.SetOrderDetails(model);
		}

		public override bool Validate(ModelState modelState, CheckoutViewModel viewModel)
		{
			return _parent.Validate(modelState, viewModel);
		}

		public override bool ValidateOrder(out string message)
		{
			var result = _parent.ValidateOrder(out string originalMessage);
			message = originalMessage;
			return result;
		}
	}
}
