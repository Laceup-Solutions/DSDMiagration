





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class PaymentTool
    {
        public string MaskNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
                return cardNumber;

            string lastFourDigits = cardNumber.Substring(cardNumber.Length - 4, 4);
            string maskedSection = new string('x', cardNumber.Length - 4);

            return maskedSection + lastFourDigits;
        }
    }
}
