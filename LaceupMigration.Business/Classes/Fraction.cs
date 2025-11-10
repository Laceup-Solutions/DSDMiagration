





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class Fraction
    {
        public int Numerator { get; set; }
        public int Denominator { get; set; }

        public Fraction(int numerator, int denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public static Fraction ParseFraction(string input)
        {
            string[] parts = input.Split('/');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int numerator) || !int.TryParse(parts[1], out int denominator))
            {
                throw new ArgumentException("Invalid fraction format.");
            }

            return new Fraction(numerator, denominator);
        }

        public static Fraction SubtractFractions(string fraction1Str, string fraction2Str)
        {
            Fraction fraction1 = ParseFraction(fraction1Str);
            Fraction fraction2 = ParseFraction(fraction2Str);

            // Find a common denominator
            int commonDenominator = LCM(fraction1.Denominator, fraction2.Denominator);

            // Adjust the numerators based on the common denominator
            int adjustedNumerator1 = (commonDenominator / fraction1.Denominator) * fraction1.Numerator;
            int adjustedNumerator2 = (commonDenominator / fraction2.Denominator) * fraction2.Numerator;

            // Subtract the adjusted numerators
            int resultNumerator = adjustedNumerator1 - adjustedNumerator2;

            // Simplify the result fraction
            int gcd = GCD(Math.Abs(resultNumerator), commonDenominator);
            resultNumerator /= gcd;
            commonDenominator /= gcd;

            return new Fraction(resultNumerator, commonDenominator);
        }

        // Helper method to calculate the least common multiple (LCM) of two numbers
        private static int LCM(int a, int b)
        {
            return (a * b) / GCD(a, b);
        }

        // Helper method to calculate the greatest common divisor (GCD) of two numbers
        private static int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public override string ToString()
        {
            return $"{Numerator}/{Denominator}";
        }
    }
}