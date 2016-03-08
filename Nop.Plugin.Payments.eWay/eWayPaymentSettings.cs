using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.eWay
{
    public class eWayPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }
        public string TestCustomerId { get; set; }
        public string LiveCustomerId { get; set; }
        public decimal AdditionalFee { get; set; }
    }
}
