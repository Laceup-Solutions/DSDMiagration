





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class PaymentAtutorizeToken
    {
        public string MerchantKey { get; set; }
        public string AccountToken { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }

        public string AccountNumber { get; set; }
        public string RoutingNumber { get; set; }
    }
}