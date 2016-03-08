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
            var model = new ConfigurationModel();
            model.UseSandbox = _eWayPaymentSettings.UseSandbox;
            model.TestCustomerId = _eWayPaymentSettings.TestCustomerId;
            model.LiveCustomerId = _eWayPaymentSettings.LiveCustomerId;
            model.AdditionalFee = _eWayPaymentSettings.AdditionalFee;

            return View("~/Plugins/Payments.eWay/Views/PaymenteWay/Configure.cshtml", model);
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
            _eWayPaymentSettings.TestCustomerId = model.TestCustomerId;
            _eWayPaymentSettings.LiveCustomerId = model.LiveCustomerId;
            _eWayPaymentSettings.AdditionalFee = model.AdditionalFee;
            _settingService.SaveSetting(_eWayPaymentSettings);

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            
            //CC types
            model.CreditCardTypes.Add(new SelectListItem()
                {
                    Text = "VISA",
                    Value = "VISA",
                });
            model.CreditCardTypes.Add(new SelectListItem()
            {
                Text = "MASTERCARD",
                Value = "MASTER CARD",
            });
            model.CreditCardTypes.Add(new SelectListItem()
            {
                Text = "BANKCARD",
                Value = "BANK CARD",
            });
            model.CreditCardTypes.Add(new SelectListItem()
            {
                Text = "AMEX",
                Value = "AMEX",
            });
            model.CreditCardTypes.Add(new SelectListItem()
            {
                Text = "DINERS",
                Value = "DINERS",
            });
            model.CreditCardTypes.Add(new SelectListItem()
            {
                Text = "JCB",
                Value = "JCB",
            });
            
            //years
            for (int i = 0; i < 15; i++)
            {
                string year = Convert.ToString(DateTime.Now.Year + i);
                model.ExpireYears.Add(new SelectListItem()
                {
                    Text = year,
                    Value = year,
                });
            }

            //months
            for (int i = 1; i <= 12; i++)
            {
                string text = (i < 10) ? "0" + i.ToString() : i.ToString();
                model.ExpireMonths.Add(new SelectListItem()
                {
                    Text = text,
                    Value = i.ToString(),
                });
            }

            //set postback values
            var form = this.Request.Form;
            model.CardholderName = form["CardholderName"];
            model.CardNumber = form["CardNumber"];
            model.CardCode = form["CardCode"];
            var selectedCcType = model.CreditCardTypes.Where(x => x.Value.Equals(form["CreditCardType"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (selectedCcType != null)
                selectedCcType.Selected = true;
            var selectedMonth = model.ExpireMonths.Where(x => x.Value.Equals(form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (selectedMonth != null)
                selectedMonth.Selected = true;
            var selectedYear = model.ExpireYears.Where(x => x.Value.Equals(form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (selectedYear != null)
                selectedYear.Selected = true;

            return View("~/Plugins/Payments.eWay/Views/PaymenteWay/PaymentInfo.cshtml", model);
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
            if (!validationResult.IsValid)
                foreach (var error in validationResult.Errors)
                    warnings.Add(error.ErrorMessage);
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            paymentInfo.CreditCardType = form["CreditCardType"];
            paymentInfo.CreditCardName = form["CardholderName"];
            paymentInfo.CreditCardNumber = form["CardNumber"];
            paymentInfo.CreditCardExpireMonth = int.Parse(form["ExpireMonth"]);
            paymentInfo.CreditCardExpireYear = int.Parse(form["ExpireYear"]);
            paymentInfo.CreditCardCvv2 = form["CardCode"];
            return paymentInfo;
        }
    }
}