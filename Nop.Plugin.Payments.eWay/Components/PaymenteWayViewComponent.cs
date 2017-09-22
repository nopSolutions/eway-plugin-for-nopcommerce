﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Payments.eWay.Models;

namespace Nop.Plugin.Payments.eWay.Components
{
    [ViewComponent(Name = "PaymenteWay")]
    public class PaymentManualViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {

            //CC types

            var model = new PaymentInfoModel()
            {
                CreditCardTypes = new List<SelectListItem>
                {
                    new SelectListItem { Text = "VISA", Value = "VISA" },
                    new SelectListItem { Text = "MASTERCARD", Value = "MASTER CARD" },
                    new SelectListItem { Text = "BANKCARD", Value = "BANK CARD" },
                    new SelectListItem { Text = "AMEX", Value = "AMEX" },
                    new SelectListItem { Text = "DINERS", Value = "DINERS" },
                    new SelectListItem { Text = "JCB", Value = "JCB" }
                }
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
    }
}
