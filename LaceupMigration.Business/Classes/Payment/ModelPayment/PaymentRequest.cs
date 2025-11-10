





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class PaymentRequest
    {
        public string MerchantKey { get; set; }
        public string AccountType { get; set; }
        public string CustomerName { get; set; }
        public string AccountNumber { get; set; }
        public int ClientId { get; set; }
        public string RoutingNumber { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }
        public string Cvv { get; set; }
        public string Amount { get; set; }
        public bool IsDefault { get; set; }
        public string Currency { get; set; }
        public string AccountToken { get; set; }
        public int CardType { get; set; }
        public TypePayment Type { get; set; }

        public string paymentType { get; set; }
        //Ach

        public string FirstNameBankAch { get; set; }
        public string LastNameBankAch { get; set; }
        public string modalcostumerIdBankAch { get; set; }
        public string modalRoutingNumberBankAch { get; set; }
        public string modalAccountNumberBankAch { get; set; }
        public string modalEmailBankAch { get; set; } 

        public string spinnerBankAccount { get; set; }

        public string ZipCode { get; set; }

        public string CustomerID { get; set; }

        public string CustomerEmail { get; set; }
    }
}