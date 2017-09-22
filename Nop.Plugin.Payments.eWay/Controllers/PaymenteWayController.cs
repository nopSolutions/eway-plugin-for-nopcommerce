using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.eWay.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Security;

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

        [AuthorizeAdmin]
        [Area("Admin")]
        public IActionResult Configure()
        {
            var model = new ConfigurationModel
            {
                UseSandbox = _eWayPaymentSettings.UseSandbox,
                CustomerId = _eWayPaymentSettings.CustomerId,
                AdditionalFee = _eWayPaymentSettings.AdditionalFee
            };

            return View("~/Plugins/Payments.eWay/Views/Configure.cshtml", model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area("Admin")]
        public IActionResult Configure(ConfigurationModel model)
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





    }
}