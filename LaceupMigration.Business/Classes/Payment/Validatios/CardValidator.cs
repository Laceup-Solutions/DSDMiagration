





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LaceupMigration
{
    public class CardValidator
    {
        public static CardType FindType(string cardNumber)
        {
            if (Regex.Match(cardNumber, @"^4[0-9]{12}(?:[0-9]{3})?$").Success)
            {
                return CardType.Visa;
            }
            if (Regex.Match(cardNumber, @"^(?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12}$").Success)
            {
                return CardType.MasterCard;
            }
            if (Regex.Match(cardNumber, @"^3[47][0-9]{13}$").Success)
            {
                return CardType.AmericanExpress;
            }
            if (Regex.Match(cardNumber, @"^6(?:011|5[0-9]{2})[0-9]{12}$").Success)
            {
                return CardType.Discover;
            }
            if (Regex.Match(cardNumber, @"^(?:2131|1800|35\d{3})\d{11}$").Success)
            {
                return CardType.JCB;
            }
            if (Regex.Match(cardNumber, @"^(300|30[1-5]|36|38)\d{11,13}$").Success)
            {
                return CardType.Diners;
            }

            return CardType.Unkown;
        }

        public bool IsValidData(string mes)
        {
            string patron = @"^(0?[1-9]|1[0-2])$";
            return Regex.IsMatch(mes, patron);
        }

        public bool IsValidCreditCardFormat(string crediCard)
        {
            string regex = @"^(?:4[0-9]{12}(?:[0-9]{3})?|(?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|6(?:011|5[0-9]{2})[0-9]{12}|(?:2131|1800|35\d{3})\d{11})$";
            return Regex.IsMatch(crediCard, regex);
        }

        public bool IsValidYear(int modalExpiryYearInt)
        {
            return modalExpiryYearInt >= DateTime.Now.Year;

        }
    }
}