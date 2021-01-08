using Litium.Accelerator.Constants;
using Litium.Accelerator.Extensions;
using Litium.Accelerator.Mailing.Models;
using Litium.Accelerator.Routing;
using Litium.Accelerator.StateTransitions;
using Litium.Accelerator.Utilities;
using Litium.Accelerator.ViewModels.Checkout;
using Litium.Accelerator.ViewModels.Persons;
using Litium.Customers;
using Litium.FieldFramework;
using Litium.FieldFramework.FieldTypes;
using Litium.Foundation.Modules.ECommerce;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Foundation.Modules.ECommerce.Payments;
using Litium.Foundation.Modules.ECommerce.Plugins.Orders;
using Litium.Foundation.Modules.ECommerce.Plugins.Payments;
using Litium.Foundation.Modules.ECommerce.ShoppingCarts;
using Litium.Foundation.Security;
using Litium.Globalization;
using Litium.Runtime.AutoMapper;
using Litium.Security;
using Litium.Studio.Extenssions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Litium.Websites;
using Litium.Web;

namespace Litium.Accelerator.Services
{
    internal class CheckoutServiceImpl : CheckoutService
    {
        private readonly ModuleECommerce _moduleECommerce;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly CartService _cartService;
        private readonly IPaymentInfoFactory _paymentInfoFactory;
        private readonly LanguageService _languageService;
        private readonly UserValidationService _userValidationService;
        private readonly SecurityContextService _securityContextService;
        private readonly PersonService _personService;
        private readonly LoginService _loginService;
        private readonly MailService _mailService;
        private readonly WelcomeEmailDefinitionResolver _welcomeEmailDefinitionResolver;
        private readonly FieldTemplateService _templateService;
        private readonly AddressTypeService _addressTypeService;
        private readonly WebsiteService _websiteService;
        private readonly UrlService _urlService;
        private readonly PageService _pageService;
        private readonly PersonStorage _personStorage;
        private readonly CheckoutState _checkoutState;

        public CheckoutServiceImpl(
            ModuleECommerce moduleECommerce,
            RequestModelAccessor requestModelAccessor,
            CartService cartService,
            IPaymentInfoFactory paymentInfoFactory,
            LanguageService languageService,
            UserValidationService userValidationService,
            SecurityContextService securityContextService,
            PersonService personService,
            LoginService loginService,
            MailService mailService,
            WelcomeEmailDefinitionResolver welcomeEmailDefinitionResolver,
            FieldTemplateService templateService,
            WebsiteService websiteService,
            AddressTypeService addressTypeService,
            UrlService urlService,
            PageService pageService,
            PersonStorage personStorage,
            CheckoutState checkoutState)
        {
            _moduleECommerce = moduleECommerce;
            _requestModelAccessor = requestModelAccessor;
            _cartService = cartService;
            _paymentInfoFactory = paymentInfoFactory;
            _languageService = languageService;
            _userValidationService = userValidationService;
            _securityContextService = securityContextService;
            _personService = personService;
            _loginService = loginService;
            _mailService = mailService;
            _welcomeEmailDefinitionResolver = welcomeEmailDefinitionResolver;
            _templateService = templateService;
            _addressTypeService = addressTypeService;
            _websiteService = websiteService;
            _pageService = pageService;
            _personStorage = personStorage;
            _checkoutState = checkoutState;
            _urlService = urlService;
        }

        public override string HandlePaymentResult(bool isSuccess, string errorMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                throw new Exception($"{CheckoutConstants.StringPaymentUnsuccessful.AsWebSiteString()}: {errorMessage}");
            }

            if (!isSuccess)
            {
                throw new Exception(CheckoutConstants.StringPaymentUnsuccessful.AsWebSiteString());
            }

            //recheck whether the order state is correct.
            var order = _moduleECommerce.Orders.GetOrder(_requestModelAccessor.RequestModel.Cart.OrderCarrier.ID, _moduleECommerce.AdminToken)?.GetAsCarrier(true, true, true, true, true, true);
            _requestModelAccessor.RequestModel.Cart.OrderCarrier = order;
            if (_requestModelAccessor.RequestModel.Cart.OrderCarrier == null || !IsOrderPlaced((OrderState)_requestModelAccessor.RequestModel.Cart.OrderCarrier.OrderStatus))
            {
                throw new Exception(CheckoutConstants.StringPaymentUnsuccessful.AsWebSiteString());
            }

            return GetOrderConfirmationPageUrl(order);
        }

        private bool IsOrderPlaced(OrderState orderState)
        {
            return orderState == OrderState.WaitingConfirmation
                   || orderState == OrderState.Confirmed
                   || orderState == OrderState.Processing
                   || orderState == OrderState.Completed;
        }

        public override void ChangePaymentMethod(string paymentMethodId)
        {
            var currentLanguage = _languageService.Get(_requestModelAccessor.RequestModel.ChannelModel.Channel.WebsiteLanguageSystemId.Value);
            var checkoutFlowInfo = _requestModelAccessor.RequestModel.Cart.CheckoutFlowInfo;
            checkoutFlowInfo.SetValue(CheckoutConstants.ClientLanguage, currentLanguage.CultureInfo.Name);
            checkoutFlowInfo.SetValue(CheckoutConstants.ClientTwoLetterISOLanguageName, currentLanguage.CultureInfo.TwoLetterISOLanguageName);

            var currentOrderCarrier = _requestModelAccessor.RequestModel.Cart.OrderCarrier;
            var paymentMethodParts = paymentMethodId.Split(':');
            var paymentMethod = _moduleECommerce.PaymentMethods.Get(paymentMethodParts[1], paymentMethodParts[0], SecurityToken.CurrentSecurityToken);

            var existingPaymentInfo = currentOrderCarrier.PaymentInfo.Any(x => x.PaymentProvider == paymentMethod.PaymentProviderName && x.PaymentMethod == paymentMethod.Name);
            _moduleECommerce.CheckoutFlow.AddPaymentInfo(currentOrderCarrier, paymentMethod.PaymentProviderName, paymentMethod.Name, new AddressCarrier(), SecurityToken.CurrentSecurityToken);

            if (!existingPaymentInfo)
            {
                var currentPaymentInfo = currentOrderCarrier.PaymentInfo.First(x => x.PaymentProvider == paymentMethod.PaymentProviderName && x.PaymentMethod == paymentMethod.Name);
                currentPaymentInfo.TransactionNumber = null;
                currentPaymentInfo.TransactionReference = null;
            }

            _moduleECommerce.Orders.CalculateOrderTotals(currentOrderCarrier, SecurityToken.CurrentSecurityToken);
        }

        public override void ChangeDeliveryMethod(Guid deliveryMethodId)
        {
            var currentOrderCarrier = _requestModelAccessor.RequestModel.Cart.OrderCarrier;
            _moduleECommerce.CheckoutFlow.AddDelivery(currentOrderCarrier, deliveryMethodId, new AddressCarrier(), SecurityToken.CurrentSecurityToken);
            _moduleECommerce.Orders.CalculateOrderTotals(currentOrderCarrier, SecurityToken.CurrentSecurityToken);
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

        public override ExecutePaymentResult PlaceOrder(CheckoutViewModel model, string responseUrl, string cancelUrl, out string redirectUrl)
        {
            var shouldRedirect = false;
            var redirectArgs = string.Empty;
            var checkoutFlowInfo = _requestModelAccessor.RequestModel.Cart.CheckoutFlowInfo;
            checkoutFlowInfo.RequireConsumerConfirm = false;
            checkoutFlowInfo.ExecuteScript = (args, redirect) => { redirectArgs = args; shouldRedirect = redirect; };
            checkoutFlowInfo.ResponseUrl = responseUrl;
            checkoutFlowInfo.CancelUrl = cancelUrl;

            var currentLanguage = _languageService.Get(_requestModelAccessor.RequestModel.ChannelModel.Channel.WebsiteLanguageSystemId.Value);
            checkoutFlowInfo.SetValue(CheckoutConstants.ClientLanguage, currentLanguage.CultureInfo.Name);
            checkoutFlowInfo.SetValue(CheckoutConstants.ClientTwoLetterISOLanguageName, currentLanguage.CultureInfo.TwoLetterISOLanguageName);

            if (model != null)
            {
                SetOrderDetails(model);
            }

            _cartService.PlaceOrder(out PaymentInfo[] paymentInfos);

            if (paymentInfos != null && paymentInfos.Length > 0)
            {
                try
                {
                    var result = paymentInfos[0].ExecutePayment(checkoutFlowInfo, SecurityToken.CurrentSecurityToken);
                    // execute payment updates the order, so it needs to be fetched back into session
                    _requestModelAccessor.RequestModel.Cart.OrderCarrier = _moduleECommerce.Orders.GetOrder(paymentInfos[0].OrderID, _moduleECommerce.AdminToken)?.GetAsCarrier(true, true, true, true, true, true);
                    return result;
                }
                catch (ShoppingCartItemInvalidException cartInvalid)
                {
                    throw new CheckoutException($"{CheckoutConstants.StringPaymentUnsuccessful.AsWebSiteString()}: {cartInvalid.Message}", cartInvalid);
                }
                catch (PaymentProviderException ex)
                {
                    throw new CheckoutException($"{CheckoutConstants.StringPaymentUnsuccessful.AsWebSiteString()}, {CheckoutConstants.StringExternalMessage.AsWebSiteString()}: {ex.Message}", ex);
                }
                catch (ArgumentOutOfRangeException aoor)
                {
                    throw new CheckoutException(CheckoutConstants.StringPaymentUnsuccessful.AsWebSiteString(), aoor);
                }
                catch (Exception e)
                {
                    throw new CheckoutException(e.Message, e);
                }
                finally
                {
                    if (shouldRedirect)
                    {
                        redirectUrl = redirectArgs;
                    }
                    else
                    {
                        redirectUrl = string.Empty;
                    }
                }
            }
            else
            {
                redirectUrl = string.Empty;
            }
            return null;
        }
        public override void SetOrderDetails(CheckoutViewModel model)
        {
            SetCustomerDetails(model);
            AddDeliveryMethod(model, model.SelectedDeliveryMethod.Value);
            // Changes in a paymentInfo must be done after deliveryMethods are set for an order, because the paymentInfo adds deliveryMethods to the paymentInfo.Rows collection
            AddPaymentInfo(model, model.SelectedPaymentMethod);

            if (!SecurityToken.CurrentSecurityToken.IsAnonymousUser)
            {
                var person = _personService.Get(SecurityToken.CurrentSecurityToken.UserID).MakeWritableClone();
                if (person != null)
                {
                    UpdatePersonInformation(person, model);
                }
            }
            else if (model.SignUp && !model.IsBusinessCustomer)
            {
                RegisterNewUser();
            }
        }

        public override string GetOrderConfirmationPageUrl(OrderCarrier orderCarrier)
        {
            var website = _websiteService.Get(orderCarrier.WebSiteID);
            var pointerPage = website.Fields.GetValue<PointerPageItem>(AcceleratorWebsiteFieldNameConstants.OrderConfirmationPage);

            if (pointerPage == null)
            {
                throw new CheckoutException("Order is created, order confirmation page is missing.");
            }

            var channelSystemId = pointerPage.ChannelSystemId != Guid.Empty ? pointerPage.ChannelSystemId : orderCarrier.ChannelID;
            var url = _urlService.GetUrl(_pageService.Get(pointerPage.EntitySystemId), new PageUrlArgs(channelSystemId));

            if (string.IsNullOrEmpty(url))
            {
                throw new CheckoutException("Order is created, order confirmation page is missing.");
            }

            return $"{url}?orderId={orderCarrier.ID}";
        }

        private string ToModelStateField(string stateKey, string field)
        {
            return $"{stateKey}-{field.ToCamelCase()}";
        }

        public override bool Validate(ModelState modelState, CheckoutViewModel viewModel)
        {
            var validationRules = new List<ValidationRuleItem<CheckoutViewModel>>()
            {
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.PhoneNumber)),
                    Rule = model => !string.IsNullOrWhiteSpace(viewModel.CustomerDetails?.PhoneNumber),
                    ErrorMessage = () => "validation.required".AsWebSiteString()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.PhoneNumber)),
                    Rule = model => _userValidationService.IsValidPhone(viewModel.CustomerDetails?.PhoneNumber),
                    ErrorMessage = () => "validation.phone".AsWebSiteString()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.FirstName)),
                    Rule = model => !string.IsNullOrWhiteSpace(model.CustomerDetails?.FirstName),
                    ErrorMessage = () => "validation.required".AsWebSiteString()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.LastName)),
                    Rule = model => !string.IsNullOrEmpty(model.CustomerDetails?.LastName),
                    ErrorMessage = () => "validation.required".AsWebSiteString()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = nameof(viewModel.SelectedDeliveryMethod),
                    Rule = model => model.SelectedDeliveryMethod.HasValue && model.SelectedDeliveryMethod.Value != Guid.Empty,
                    ErrorMessage = () => "validation.required".AsWebSiteString()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.Email)),
                    Rule = model => !string.IsNullOrEmpty(model.CustomerDetails?.Email),
                    ErrorMessage = () => "validation.required".AsWebSiteString()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.Email)),
                    Rule = model => _userValidationService.IsValidEmail(model.CustomerDetails?.Email),
                    ErrorMessage = () => "validation.email".AsWebSiteString()
                },
            };

            if (viewModel.IsBusinessCustomer)
            {
                validationRules.Add(new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = nameof(viewModel.SelectedCompanyAddressId),
                    Rule = model => viewModel.SelectedCompanyAddressId.HasValue && viewModel.SelectedCompanyAddressId.Value != Guid.Empty,
                    ErrorMessage = () => "validation.required".AsWebSiteString()
                });

                return viewModel.IsValid(validationRules, modelState);
            }

            validationRules.AddRange(new List<ValidationRuleItem<CheckoutViewModel>>()
            {
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.Address)),
                    Rule = model => !string.IsNullOrWhiteSpace(model.CustomerDetails?.Address),
                    ErrorMessage = () => "validation.required".AsWebSiteString()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.ZipCode)),
                    Rule = model => !string.IsNullOrWhiteSpace(model.CustomerDetails?.ZipCode),
                    ErrorMessage = () => "validation.required".AsWebSiteString()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.City)),
                    Rule = model => !string.IsNullOrWhiteSpace(model.CustomerDetails?.City),
                    ErrorMessage = () => "validation.required".AsWebSiteString()
                },
                new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.Country)),
                    Rule = model => !string.IsNullOrWhiteSpace(model.CustomerDetails?.Country),
                    ErrorMessage = () => "validation.required".AsWebSiteString()
                },
            });

            if (viewModel.SignUp && !_securityContextService.GetIdentityUserSystemId().HasValue)
            {
                validationRules.Add(new ValidationRuleItem<CheckoutViewModel>
                {
                    Field = ToModelStateField(nameof(viewModel.CustomerDetails), nameof(viewModel.CustomerDetails.Email)),
                    Rule = model =>
                    {
                        var existingUserId = _securityContextService.GetPersonSystemId(viewModel.CustomerDetails.Email);
                        return existingUserId == null || !existingUserId.HasValue;
                    },
                    ErrorMessage = () => "validation.emailinused".AsWebSiteString()
                });
            }

            if (viewModel.ShowAlternativeAddress && !(string.IsNullOrEmpty(viewModel.AlternativeAddress.FirstName) && string.IsNullOrEmpty(viewModel.AlternativeAddress.LastName) && string.IsNullOrEmpty(viewModel.AlternativeAddress.Address)
                && string.IsNullOrEmpty(viewModel.AlternativeAddress.ZipCode) && string.IsNullOrEmpty(viewModel.AlternativeAddress.City) && string.IsNullOrEmpty(viewModel.AlternativeAddress.PhoneNumber)))
            {
                validationRules.AddRange(new List<ValidationRuleItem<CheckoutViewModel>>()
                {

                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.PhoneNumber)),
                        Rule = model => !string.IsNullOrWhiteSpace(viewModel.AlternativeAddress?.PhoneNumber),
                        ErrorMessage = () => "validation.required".AsWebSiteString()
                    },
                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.PhoneNumber)),
                        Rule = model => _userValidationService.IsValidPhone(viewModel.AlternativeAddress?.PhoneNumber),
                        ErrorMessage = () => "validation.phone".AsWebSiteString()
                    },
                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.FirstName)),
                        Rule = model => !string.IsNullOrWhiteSpace(model.AlternativeAddress?.FirstName),
                        ErrorMessage = () => "validation.required".AsWebSiteString()
                    },
                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.LastName)),
                        Rule = model => !string.IsNullOrEmpty(model.AlternativeAddress?.LastName),
                        ErrorMessage = () => "validation.required".AsWebSiteString()
                    },
                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.Address)),
                        Rule = model => !string.IsNullOrWhiteSpace(model.AlternativeAddress?.Address),
                        ErrorMessage = () => "validation.required".AsWebSiteString()
                    },
                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.ZipCode)),
                        Rule = model => !string.IsNullOrWhiteSpace(model.AlternativeAddress?.ZipCode),
                        ErrorMessage = () => "validation.required".AsWebSiteString()
                    },
                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.City)),
                        Rule = model => !string.IsNullOrWhiteSpace(model.AlternativeAddress?.City),
                        ErrorMessage = () => "validation.required".AsWebSiteString()
                    },
                    new ValidationRuleItem<CheckoutViewModel>
                    {
                        Field = ToModelStateField(nameof(viewModel.AlternativeAddress), nameof(viewModel.AlternativeAddress.Country)),
                        Rule = model => !string.IsNullOrWhiteSpace(model.AlternativeAddress?.Country),
                        ErrorMessage = () => "validation.required".AsWebSiteString()
                    },
                });
            }

            return viewModel.IsValid(validationRules, modelState);
        }

        public override bool ValidateOrder(out string message)
        {
            try
            {
                _cartService.PreOrderValidate();
            }
            catch (PreOrderValidationException ex)
            {
                _cartService.CalculatePrices();
                message = ex.Message;
                return false;
            }

            message = string.Empty;
            return true;
        }

        private void RegisterNewUser()
        {
            try
            {
                var person = GetPersonFromOrder();
                person.LoginCredential.NewPassword = _loginService.GeneratePassword();

                using (_securityContextService.ActAsSystem())
                {
                    _personService.Create(person);
                }

                var orderCarrier = _requestModelAccessor.RequestModel.Cart.OrderCarrier;
                orderCarrier.CustomerInfo.PersonID = person.SystemId;
                orderCarrier.CustomerInfo.CustomerNumber = person.Id;

                _mailService.SendEmail(_welcomeEmailDefinitionResolver.Get(person.MapTo<WelcomeEmailModel>(), person.Email), false);

                _loginService.Login(person.LoginCredential.Username, person.LoginCredential.NewPassword, out SecurityToken token);
            }
            catch (Exception ex)
            {
                this.Log().Error("New user registration failed.", ex);
                throw;
            }
        }

        private void SetCustomerDetails(CheckoutViewModel model)
        {
            var orderCarrier = _requestModelAccessor.RequestModel.Cart.OrderCarrier;
            orderCarrier.Comments = model.OrderNote;

            //add customer information.
            var customerAddress = new AddressCarrier();
            customerAddress.MapFrom(model.CustomerDetails);

            //change delivery address.
            var deliveryAddress = customerAddress;
            if (model.IsBusinessCustomer && model.SelectedCompanyAddressId.HasValue)
            {
                deliveryAddress.MapFrom(GetCompanyAddress(model.SelectedCompanyAddressId.Value));
                deliveryAddress.MobilePhone = model.CustomerDetails.PhoneNumber;
                //Set selected company address to CustomerInfo.Address 
                _moduleECommerce.CheckoutFlow.AddOrEditCustomerInfo(orderCarrier, deliveryAddress, SecurityToken.CurrentSecurityToken.UserID, _personStorage.CurrentSelectedOrganization.SystemId, SecurityToken.CurrentSecurityToken);
                _checkoutState.CopyAddressValues(customerAddress, deliveryAddress);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(model.AlternativeAddress?.FirstName) && !string.IsNullOrWhiteSpace(model.AlternativeAddress?.LastName) && !string.IsNullOrWhiteSpace(model.AlternativeAddress?.Address))
                {
                    _checkoutState.CopyDeliveryAddressValues(model.AlternativeAddress.MapTo<AddressCarrier>());
                }
                _checkoutState.CopyAddressValues(customerAddress);

                _moduleECommerce.CheckoutFlow.AddOrEditCustomerInfo(orderCarrier, customerAddress, SecurityToken.CurrentSecurityToken);
            }

            //update the payment info, with customer address.
            var paymentInfo = orderCarrier.PaymentInfo.FirstOrDefault(x => !x.CarrierState.IsMarkedForDeleting);
            if (paymentInfo != null)
            {
                _moduleECommerce.CheckoutFlow.AddPaymentInfo(orderCarrier, paymentInfo.PaymentProvider, paymentInfo.PaymentMethod, deliveryAddress, SecurityToken.CurrentSecurityToken);
            }

            _checkoutState.NeedToRegister = model.SignUp;
            _checkoutState.AlternativeAddressEnabled = model.ShowAlternativeAddress;

            //obtain the existing delivery, and change the address.
            var deliveryCarrier = orderCarrier.Deliveries.FirstOrDefault(x => !x.CarrierState.IsMarkedForDeleting);
            if (deliveryCarrier != null)
            {
                //Add delivery with new address. In default checkoutflow this causes the delivery address to be changed in existing delivery.
                _moduleECommerce.CheckoutFlow.AddDelivery(orderCarrier,
                    deliveryCarrier.DeliveryMethodID, deliveryAddress,
                    SecurityToken.CurrentSecurityToken);
            }
        }

        private void UpdatePersonInformation(Person person, CheckoutViewModel model)
        {
            var orderCarrier = _requestModelAccessor.RequestModel.Cart.OrderCarrier;
            var addressCarrier = orderCarrier.CustomerInfo.Address;
            var deliveryCarrier = orderCarrier.Deliveries.FirstOrDefault();

            AddressCarrier deliveryAddressCarrier = null;
            if (deliveryCarrier != null)
            {
                deliveryAddressCarrier = deliveryCarrier.Address;
            }

            var isUpdated = false;
            if (addressCarrier != null)
            {
                var firstName = person.FirstName ?? string.Empty;
                var lastName = person.LastName ?? string.Empty;
                var email = person.Email ?? string.Empty;
                var phone = person.Phone ?? string.Empty;
                // Check if person information was changed
                if (!firstName.Equals(addressCarrier.FirstName, StringComparison.OrdinalIgnoreCase)
                    || !lastName.Equals(addressCarrier.LastName, StringComparison.OrdinalIgnoreCase)
                    || !email.Equals(addressCarrier.Email, StringComparison.OrdinalIgnoreCase)
                    || !phone.Equals(addressCarrier.MobilePhone, StringComparison.OrdinalIgnoreCase))
                {
                    person.FirstName = addressCarrier.FirstName;
                    person.LastName = addressCarrier.LastName;
                    person.Email = addressCarrier.Email;
                    person.Phone = addressCarrier.MobilePhone;
                    isUpdated = true;
                }

                //Update address for private person
                if (!model.IsBusinessCustomer)
                {
                    var addressType = _addressTypeService.Get(AddressTypeNameConstants.Address);
                    var address = person.Addresses.FirstOrDefault(x => x.AddressTypeSystemId == addressType.SystemId);
                    if (address == null)
                    {
                        address = new Address(addressType.SystemId);
                        person.Addresses.Add(address);
                    }

                    // Check if the user address and the address given in the order are the same. If they are different update the user address
                    if (!IsEquals(address, addressCarrier))
                    {
                        address.Address1 = addressCarrier.Address1;
                        address.CareOf = addressCarrier.CareOf;
                        address.ZipCode = addressCarrier.Zip;
                        address.City = addressCarrier.City;
                        address.Country = addressCarrier.Country;
                        address.PhoneNumber = addressCarrier.MobilePhone;
                        isUpdated = true;
                    }
                }
            }

            //Update address for private person
            if (!model.IsBusinessCustomer)
            {
                var alternativeAddressType = _addressTypeService.Get(AddressTypeNameConstants.AlternativeAddress);
                var deliveryAddress = person.Addresses.FirstOrDefault(x => x.AddressTypeSystemId == alternativeAddressType.SystemId);
                // Check if the user delivery addres and the given delivery address in the order are the same. If they are different update/create the user delivery address
                if (deliveryAddressCarrier != null && !IsEquals(deliveryAddress, deliveryAddressCarrier))
                {
                    if (deliveryAddress != null)
                    {
                        deliveryAddress.Address1 = deliveryAddressCarrier.Address1;
                        deliveryAddress.CareOf = deliveryAddressCarrier.CareOf;
                        deliveryAddress.ZipCode = deliveryAddressCarrier.Zip;
                        deliveryAddress.City = deliveryAddressCarrier.City;
                        deliveryAddress.Country = deliveryAddressCarrier.Country;
                        deliveryAddress.PhoneNumber = deliveryAddressCarrier.MobilePhone;
                    }
                    else
                    {
                        person.Addresses.Add(new Address(alternativeAddressType.SystemId)
                        {
                            Address1 = deliveryAddressCarrier.Address1,
                            Address2 = deliveryAddressCarrier.Address2,
                            CareOf = deliveryAddressCarrier.CareOf,
                            ZipCode = deliveryAddressCarrier.Zip,
                            City = deliveryAddressCarrier.City,
                            Country = deliveryAddressCarrier.Country,
                            State = deliveryAddressCarrier.State,
                            PhoneNumber = deliveryAddressCarrier.MobilePhone
                        });
                    }

                    isUpdated = true;
                }
            }

            if (isUpdated)
            {
                using (_securityContextService.ActAsSystem())
                {
                    _personService.Update(person);
                }
            }
        }

        private bool IsEquals(Address address1, AddressCarrier address2)
        {
            if (address2 == null || address1 == null)
            {
                return false;
            }
            return string.Equals(address1.Address1 ?? string.Empty, address2.Address1 ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(address1.Address2 ?? string.Empty, address2.Address2 ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(address1.CareOf ?? string.Empty, address2.CareOf ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(address1.City ?? string.Empty, address2.City ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(address1.State ?? string.Empty, address2.State ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(address1.Country ?? string.Empty, address2.Country ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(address1.ZipCode ?? string.Empty, address2.Zip ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(address1.PhoneNumber ?? string.Empty, address2.MobilePhone ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        private Person GetPersonFromOrder()
        {
            var orderCarrier = _requestModelAccessor.RequestModel.Cart.OrderCarrier;
            var addressCarrier = orderCarrier.CustomerInfo.Address;
            var addressType = _addressTypeService.Get(AddressTypeNameConstants.Address);
            var template = _templateService.Get<PersonFieldTemplate>(typeof(CustomerArea), DefaultWebsiteFieldValueConstants.CustomerTemplateId);
            var person = new Person(template.SystemId)
            {
                FirstName = addressCarrier.FirstName,
                LastName = addressCarrier.LastName,
                Email = addressCarrier.Email,
                LoginCredential = { Username = addressCarrier.Email },
                Addresses = new List<Address>(){{new Address(addressType.SystemId)
                {
                    SystemId = Guid.NewGuid(),
                    CareOf = addressCarrier.CareOf,
                    Address1 = addressCarrier.Address1,
                    Address2 = addressCarrier.Address2,
                    City = addressCarrier.City,
                    Country = addressCarrier.Country,
                    ZipCode = addressCarrier.Zip,
                    PhoneNumber = addressCarrier.MobilePhone
                }}},
                Phone = addressCarrier.MobilePhone
            };

            var deliveryCarrier = orderCarrier.Deliveries.FirstOrDefault();
            if (deliveryCarrier != null)
            {
                var alternativeAddressType = _addressTypeService.Get(AddressTypeNameConstants.AlternativeAddress);
                person.Addresses.Add(new Address(alternativeAddressType.SystemId)
                {
                    SystemId = Guid.NewGuid(),
                    CareOf = deliveryCarrier.Address.CareOf,
                    Address1 = deliveryCarrier.Address.Address1,
                    Address2 = deliveryCarrier.Address.Address2,
                    City = deliveryCarrier.Address.City,
                    Country = deliveryCarrier.Address.Country,
                    ZipCode = deliveryCarrier.Address.Zip,
                    PhoneNumber = deliveryCarrier.Address.MobilePhone
                }
                );
            }
            return person;
        }

        private void AddPaymentInfo(CheckoutViewModel model, string paymentMethodId)
        {
            var orderCarrier = _requestModelAccessor.RequestModel.Cart.OrderCarrier;
            var paymentMethodParts = paymentMethodId.Split(':');
            var paymentMethod = _moduleECommerce.PaymentMethods.Get(paymentMethodParts[1], paymentMethodParts[0], _moduleECommerce.AdminToken);
            PaymentInfoCarrier paymentInfoCarrier;
            if (orderCarrier.PaymentInfo.Any())
            {
                paymentInfoCarrier = orderCarrier.PaymentInfo.First();
                paymentInfoCarrier.PaymentProvider = paymentMethod.PaymentProviderName;
                paymentInfoCarrier.PaymentMethod = paymentMethod.Name;
                paymentInfoCarrier.Rows.Clear();
            }
            else
            {
                paymentInfoCarrier = _paymentInfoFactory.Create(orderCarrier, model.CustomerDetails.SystemId, null, paymentMethod.PaymentProviderName, paymentMethod.Name, _moduleECommerce.AdminToken);
                paymentInfoCarrier.OrderID = orderCarrier.ID;
                orderCarrier.PaymentInfo.Add(paymentInfoCarrier);
            }

            paymentInfoCarrier.BillingAddress.MapFrom(model.CustomerDetails);
            if (model.IsBusinessCustomer && model.SelectedCompanyAddressId.HasValue)
            {
                paymentInfoCarrier.BillingAddress.MapFrom(GetCompanyAddress(model.SelectedCompanyAddressId.Value));
                paymentInfoCarrier.BillingAddress.MobilePhone = model.CustomerDetails.PhoneNumber;
                paymentInfoCarrier.BillingAddress.OrganizationName = model.CompanyName;
            }
            _moduleECommerce.Orders.CalculateOrderTotals(orderCarrier, SecurityToken.CurrentSecurityToken);
            _moduleECommerce.Orders.CalculatePaymentInfoAmounts(orderCarrier, SecurityToken.CurrentSecurityToken);
        }

        private void AddDeliveryMethod(CheckoutViewModel model, Guid deliveryMethodId)
        {
            var orderCarrier = _requestModelAccessor.RequestModel.Cart.OrderCarrier;
            DeliveryCarrier delivery;
            if (orderCarrier.Deliveries.Any())
            {
                delivery = orderCarrier.Deliveries.First();
            }
            else
            {
                delivery = new DeliveryCarrier { ID = Guid.NewGuid(), Address = new AddressCarrier { ID = Guid.NewGuid() } };
                delivery.OrderID = orderCarrier.ID;
                var deliveryMethod = _moduleECommerce.DeliveryMethods.Get(deliveryMethodId, _moduleECommerce.AdminToken);
                delivery.DeliveryMethodID = deliveryMethod.ID;
            }

            delivery.Comments = string.Empty;

            if (!orderCarrier.Deliveries.Any(x => x.ID == delivery.ID))
            {
                orderCarrier.Deliveries.Add(delivery);
            }

            delivery.Address.MapFrom(model.CustomerDetails);
            if (model.IsBusinessCustomer && model.SelectedCompanyAddressId.HasValue)
            {
                delivery.Address.MapFrom(GetCompanyAddress(model.SelectedCompanyAddressId.Value));
                delivery.Address.MobilePhone = model.CustomerDetails.PhoneNumber;
                delivery.Address.OrganizationName = model.CompanyName;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(model.AlternativeAddress?.FirstName) && !string.IsNullOrWhiteSpace(model.AlternativeAddress?.LastName) && !string.IsNullOrWhiteSpace(model.AlternativeAddress?.Address))
                {
                    delivery.Address.MapFrom(model.AlternativeAddress);
                }
            }

            foreach (var item in orderCarrier.OrderRows)
            {
                AddDeliveryRow(orderCarrier, delivery, item, _moduleECommerce.AdminToken);
            }
        }

        private void AddDeliveryRow(OrderCarrier orderCarrier, DeliveryCarrier deliveryCarrier, OrderRowCarrier orderRowCarrier, SecurityToken token)
        {
            if (orderRowCarrier == null)
                return;

            if (deliveryCarrier != null && !deliveryCarrier.CarrierState.IsMarkedForDeleting)
            {
                orderRowCarrier.DeliveryID = deliveryCarrier.ID;
            }
            else
            {
                AddDeliveryRow(orderCarrier, deliveryCarrier ?? new DeliveryCarrier(), orderRowCarrier, token);
            }
        }

        private AddressViewModel GetCompanyAddress(Guid companyAddressId)
        {
            var companyAddress = _personStorage.CurrentSelectedOrganization.Addresses
                .Single(a => a != null && a.SystemId == companyAddressId)
                .MapTo<AddressViewModel>();
            return companyAddress;
        }
    }
}
