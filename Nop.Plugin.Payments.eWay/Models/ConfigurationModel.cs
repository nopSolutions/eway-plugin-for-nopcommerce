using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.eWay.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.eWay.UseSandbox")]
        public bool UseSandbox { get; set; }

        [NopResourceDisplayName("Plugins.Payments.eWay.TestCustomerId")]
        public string TestCustomerId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.eWay.LiveCustomerId")]
        public string LiveCustomerId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.eWay.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
    }
}