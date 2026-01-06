using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace LaceupMigration
{
    public class MilagroPayment
    {
        public class MilagroPaymentResult
        {
            public bool Approved { get; set; }
            public string GatewayId { get; set; } = "";
            public string Status { get; set; } = "";
            public string DecisionCode { get; set; } = "";
            public decimal Amount { get; set; }
            public string DocumentId { get; set; } = "";
            public string Error { get; set; } = ""; // non-empty when something goes wrong
        }

        public static async Task<MilagroPaymentResult> ValidatePaymentAsync(decimal amount, string documentId)
        {
            return new MilagroPaymentResult() { Approved = true };
            //do this in the communicator :)
        }
    }
}
