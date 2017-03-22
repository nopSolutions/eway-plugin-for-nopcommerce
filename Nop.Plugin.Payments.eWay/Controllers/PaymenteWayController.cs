using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Plugin.Payments.eWay.Models;
using Nop.Plugin.Payments.eWay.Validators;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.eWay.Controllers
{
    public class PaymenteWayController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly eWayPaymentSettings _eWayPaymentSettings;

        public PaymenteWayController(ISettingService settingService, 
            ILocalizationService localizationService, eWayPaymentSettings eWayPaymentSettings)
        {
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._eWayPaymentSettings = eWayPaymentSettings;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel
            {
                UseSandbox = _eWayPaymentSettings.UseSandbox,
                CustomerId = _eWayPaymentSettings.CustomerId,
                AdditionalFee = _eWayPaymentSettings.AdditionalFee
            };

            return View("~/Plugins/Payments.eWay/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _eWayPaymentSettings.UseSandbox = model.UseSandbox;
            _eWayPaymentSettings.CustomerId = model.CustomerId;
            _eWayPaymentSettings.AdditionalFee = model.AdditionalFee;
            _settingService.SaveSetting(_eWayPaymentSettings);

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            
            //CC types
            model.CreditCardTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "VISA", Value = "VISA" },
                new SelectListItem { Text = "MASTERCARD", Value = "MASTER CARD" },
                new SelectListItem { Text = "BANKCARD", Value = "BANK CARD" },
                new SelectListItem { Text = "AMEX", Value = "AMEX" },
                new SelectListItem { Text = "DINERS", Value = "DINERS" },
                new SelectListItem { Text = "JCB", Value = "JCB" }
            };

            //years
            for (var i = 0; i < 15; i++)
            {
                var year = Convert.ToString(DateTime.Now.Year + i);
                model.ExpireYears.Add(new SelectListItem { Text = year, Value = year });
            }

            //months
            for (var i = 1; i <= 12; i++)
            {
                var text = string.Format("{0:00}", i);
                model.ExpireMonths.Add(new SelectListItem()
                {
                    Text = text,
                    Value = i.ToString(),
                });
            }

            //set postback values
            var form = Request.Form;
            model.CardholderName = form["CardholderName"];
            model.CardNumber = form["CardNumber"];
            model.CardCode = form["CardCode"];
            var selectedCcType = model.CreditCardTypes.FirstOrDefault(x => x.Value.Equals(form["CreditCardType"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedCcType != null)
                selectedCcType.Selected = true;
            var selectedMonth = model.ExpireMonths.FirstOrDefault(x => x.Value.Equals(form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedMonth != null)
                selectedMonth.Selected = true;
            var selectedYear = model.ExpireYears.FirstOrDefault(x => x.Value.Equals(form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedYear != null)
                selectedYear.Selected = true;

            return View("~/Plugins/Payments.eWay/Views/PaymentInfo.cshtml", model);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();

            //validate
            var validator = new PaymentInfoValidator(_localizationService);
            var model = new PaymentInfoModel()
            {
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
            };
            var validationResult = validator.Validate(model);
            if (validationResult.IsValid) return warnings;

            warnings.AddRange(validationResult.Errors.Select(error => error.ErrorMessage));
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest
            {
                CreditCardType = form["CreditCardType"],
                CreditCardName = form["CardholderName"],
                CreditCardNumber = form["CardNumber"],
                CreditCardExpireMonth = int.Parse(form["ExpireMonth"]),
                CreditCardExpireYear = int.Parse(form["ExpireYear"]),
                CreditCardCvv2 = form["CardCode"]
            };
            return paymentInfo;
        }
    }
}