using System;
using System.Collections.Generic;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.eWay.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Payments;

namespace Nop.Plugin.Payments.eWay
{
    /// <summary>
    /// eWay payment processor
    /// </summary>
    public class eWayPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly eWayPaymentSettings _eWayPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        private string APPROVED_RESPONSE = "00";
        private string HONOUR_RESPONSE = "08";

        #endregion

        #region Ctor

        public eWayPaymentProcessor(ICustomerService customerService, eWayPaymentSettings eWayPaymentSettings,
            ISettingService settingService, IStoreContext storeContext)
        {
            this._customerService = customerService;
            this._eWayPaymentSettings = eWayPaymentSettings;
            this._settingService = settingService;
            this._storeContext = storeContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets eWay URL
        /// </summary>
        /// <returns></returns>
        private string GeteWayUrl()
        {
            //return useSandBox ? "https://www.eway.com.au/gateway/xmltest/TestPage.asp" :
            //    "https://www.eway.com.au/gateway/xmlpayment.asp";

            return _eWayPaymentSettings.UseSandbox ? "https://www.eway.com.au/gateway_cvn/xmltest/TestPage.asp" :
                "https://www.eway.com.au/gateway_cvn/xmlpayment.asp";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            var eWaygateway = new GatewayConnector();
            var eWayRequest = new GatewayRequest();
            if (_eWayPaymentSettings.UseSandbox)
                eWayRequest.EwayCustomerID = _eWayPaymentSettings.TestCustomerId;
            else
                eWayRequest.EwayCustomerID = _eWayPaymentSettings.LiveCustomerId;

            eWayRequest.CardNumber = processPaymentRequest.CreditCardNumber;
            eWayRequest.CardExpiryMonth = processPaymentRequest.CreditCardExpireMonth.ToString("D2");
            eWayRequest.CardExpiryYear = processPaymentRequest.CreditCardExpireYear.ToString();
            eWayRequest.CardHolderName = processPaymentRequest.CreditCardName;
            //Integer
            eWayRequest.InvoiceAmount = Convert.ToInt32(processPaymentRequest.OrderTotal * 100);

            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            var billingAddress = customer.BillingAddress;
            eWayRequest.PurchaserFirstName = billingAddress.FirstName;
            eWayRequest.PurchaserLastName = billingAddress.LastName;
            eWayRequest.PurchaserEmailAddress = billingAddress.Email;
            eWayRequest.PurchaserAddress = billingAddress.Address1;
            eWayRequest.PurchaserPostalCode = billingAddress.ZipPostalCode;
            eWayRequest.InvoiceReference = processPaymentRequest.OrderGuid.ToString();
            eWayRequest.InvoiceDescription = _storeContext.CurrentStore.Name + ". Order #" + processPaymentRequest.OrderGuid.ToString();
            eWayRequest.TransactionNumber = processPaymentRequest.OrderGuid.ToString();
            eWayRequest.CVN = processPaymentRequest.CreditCardCvv2;
            eWayRequest.EwayOption1 = string.Empty;
            eWayRequest.EwayOption2 = string.Empty;
            eWayRequest.EwayOption3 = string.Empty;

            // Do the payment, send XML doc containing information gathered
            eWaygateway.Uri = GeteWayUrl();
            GatewayResponse eWayResponse = eWaygateway.ProcessRequest(eWayRequest);
            if (eWayResponse != null)
            {
                // Payment succeeded get values returned
                if (eWayResponse.Status && (eWayResponse.Error.StartsWith(APPROVED_RESPONSE) || eWayResponse.Error.StartsWith(HONOUR_RESPONSE)))
                {
                    result.AuthorizationTransactionCode = eWayResponse.AuthorisationCode;
                    result.AuthorizationTransactionResult = eWayResponse.InvoiceReference;
                    result.AuthorizationTransactionId = eWayResponse.TransactionNumber;
                    result.NewPaymentStatus = PaymentStatus.Paid;
                    //processPaymentResult.AuthorizationDate = DateTime.UtcNow;
                }
                else
                {
                    result.AddError("An invalid response was recieved from the payment gateway." + eWayResponse.Error);
                    //full error: eWAYRequest.ToXml().ToString()
                }
            }
            else
            {
                // invalid response recieved from server.
                result.AddError("An invalid response was recieved from the payment gateway.");
                //full error: eWAYRequest.ToXml().ToString()
            }


            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _eWayPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");
            
            //it's not a redirection payment method. So we always return false
            return false;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymenteWay";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.eWay.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymenteWay";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.eWay.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymenteWayController);
        }

        public override void Install()
        {
            var settings = new eWayPaymentSettings()
            {
                UseSandbox = true,
                TestCustomerId = "",
                LiveCustomerId= "",
                AdditionalFee = 0,
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.eWay.UseSandbox", "Use sandbox");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.eWay.UseSandbox.Hint", "Use sandbox?");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.eWay.TestCustomerId", "Test customer ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.eWay.TestCustomerId.Hint", "Enter test customer ID.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.eWay.LiveCustomerId", "Live customer ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.eWay.LiveCustomerId.Hint", "Enter live customer ID.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.eWay.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.eWay.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            
            base.Install();
        }
        
        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.eWay.UseSandbox");
            this.DeletePluginLocaleResource("Plugins.Payments.eWay.UseSandbox.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.eWay.TestCustomerId");
            this.DeletePluginLocaleResource("Plugins.Payments.eWay.TestCustomerId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.eWay.LiveCustomerId");
            this.DeletePluginLocaleResource("Plugins.Payments.eWay.LiveCustomerId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.eWay.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.eWay.AdditionalFee.Hint");
            

            base.Uninstall();
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Standard;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        #endregion
    }
}
