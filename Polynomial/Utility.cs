using System.Globalization;
using System.Numerics;

namespace Polynomial
{
    public static class Utility
    {
        public static PolyEquation LatexConvertor(string latex)
        {
            static PolynomialElement LatexConvertorElement(string latex)
            {
                latex = latex.Trim();

                if (!latex.Contains('x'))
                    return new PolynomialElement(double.Parse(latex, NumberStyles.Float, CultureInfo.InvariantCulture));

                int idxX = latex.IndexOf('x');
                string coefficientText = latex[..idxX].Trim();
                double coefficient;

                if (coefficientText.Length == 0 || coefficientText == "+")
                    coefficient = 1;
                else if (coefficientText == "-")
                    coefficient = -1;
                else
                    coefficient = double.Parse(coefficientText, NumberStyles.Float, CultureInfo.InvariantCulture);

                int exponentIndex = latex.IndexOf('^', idxX);
                if (exponentIndex == -1)
                    return new PolynomialElement(coefficient, 1);

                string exponentText = latex[(exponentIndex + 1)..].Trim();
                if (exponentText.StartsWith('{') && exponentText.EndsWith('}'))
                    exponentText = exponentText[1..^1];

                double exponent = double.Parse(exponentText, NumberStyles.Float, CultureInfo.InvariantCulture);
                return new PolynomialElement(coefficient, exponent);
            }

            int idxPlus = -1;
            int idxMinus = -1;
            int braceDepth = 0;

            for (int i = 0; i < latex.Length; i++)
            {
                if (latex[i] == '{') braceDepth++;
                else if (latex[i] == '}') braceDepth--;
                else if (braceDepth == 0)
                {
                    if (latex[i] == '+' && idxPlus == -1)
                        idxPlus = i;
                    else if (latex[i] == '-' && i > 0 && idxMinus == -1)
                        idxMinus = i;
                }
            }

            if (idxPlus == -1 && idxMinus == -1)
            {
                return new(LatexConvertorElement(latex));
            }
            else if (idxPlus != -1 && (idxMinus == -1 || idxPlus < idxMinus))
            {
                return LatexConvertor(latex[(idxPlus + 1)..]) + LatexConvertorElement(latex[..idxPlus]);
            }
            else
            {
                return LatexConvertor(latex[idxMinus..]) + LatexConvertorElement(latex[..idxMinus]);
            }
        }

        public static (BigInteger p, BigInteger q) DoubleToFraction(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Value must be a finite number.", nameof(value));

            // Convert to string via round-trip format or decimal to avoid binary float artifacts (e.g., 0.1 -> 0.10000000000000000555...)
            string text = value.ToString("G15", CultureInfo.InvariantCulture);

            // Handle scientific notation if present (e.g., 1E-05)
            decimal decValue = decimal.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);

            int[] bits = decimal.GetBits(decValue);
            int scale = (bits[3] >> 16) & 0x7F;
            bool isNegative = (bits[3] & 0x80000000) != 0;

            // Reconstruct the unscaled 96-bit integer
            BigInteger numerator = ((BigInteger)(uint)bits[2] << 64) |
                                   ((BigInteger)(uint)bits[1] << 32) |
                                   (uint)bits[0];

            if (isNegative)
                numerator = -numerator;

            BigInteger denominator = BigInteger.Pow(10, scale);

            // Simplify the fraction
            BigInteger gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);
            return (numerator / gcd, denominator / gcd);
        }
    }
}