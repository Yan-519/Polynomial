using System.Numerics;
namespace Polynomial
{
    public record struct PolynomialElement(double Coefficient, double Exponent) : IMathematicalElement<PolynomialElement>,
        IMultiplyOperators<PolynomialElement, PolynomialElement, PolynomialElement>, IDivisionOperators<PolynomialElement, PolynomialElement, PolynomialElement>
    {
        public double Coefficient = Coefficient;
        public double Exponent = Exponent;

        public bool DoesNegativeXExists
        {
            get
            {
                if (!double.IsFinite(Exponent))
                    return false;

                if (double.IsInteger(Exponent))
                    return true;

                var (p, q) = Utility.DoubleToFraction(Exponent);
                return q > 0 && !q.IsEven;
            }
        }


        public PolynomialElement(double coefficient) : this(coefficient, 0)
        { }

        public readonly double Evaluate(double x)
        {
            if (x >= 0)
                return Coefficient * Math.Pow(x, Exponent);

            if (double.IsInteger(Exponent))
                return Coefficient * Math.Pow(x, Exponent);

            var (numerator, denominator) = Utility.DoubleToFraction(Exponent);

            if (denominator.IsEven)
                return double.NaN;

            double negativeBaseSign = numerator.IsEven ? 1 : -1;
            return Coefficient * negativeBaseSign * Math.Pow(Math.Abs(x), Exponent);
        }

        public readonly PolynomialElement Derivative()
        {
            if (Exponent == 0)
                return new PolynomialElement(0, 0);
            else
                return new PolynomialElement(Coefficient * Exponent, Exponent - 1);
        }

        public readonly override string ToString()
        {
            if (Coefficient == 0)
                return "0";

            string str = 0 < Coefficient ? $"{Coefficient}" : $"({Coefficient})";
            if (Coefficient == 1) str = string.Empty;

            if (Exponent == 0)
                return str;
            else if (Exponent == 1)
                return $"{str}x";
            else
                return $"{str}x^{{{Exponent}}}";
        }

        public static PolynomialElement operator +(PolynomialElement left, PolynomialElement right)
        {
            if (left.Exponent != right.Exponent)
                throw new ArgumentException("Cannot add polynomial elements with different exponents.");
            return new PolynomialElement(left.Coefficient + right.Coefficient, left.Exponent);
        }

        public static PolynomialElement operator -(PolynomialElement left, PolynomialElement right)
        {
            if (left.Exponent != right.Exponent)
                throw new ArgumentException("Cannot subtract polynomial elements with different exponents.");
            return new PolynomialElement(left.Coefficient - right.Coefficient, left.Exponent);
        }


        public static PolynomialElement operator *(PolynomialElement left, double right)
        {
            return new PolynomialElement(left.Coefficient * right, left.Exponent);
        }

        public static PolynomialElement operator /(PolynomialElement left, double right)
        {
            return new PolynomialElement(left.Coefficient / right, left.Exponent);
        }

        public static PolynomialElement operator *(PolynomialElement left, PolynomialElement right)
        {
            return new PolynomialElement(left.Coefficient * right.Coefficient, left.Exponent + right.Exponent);
        }

        public static PolynomialElement operator /(PolynomialElement left, PolynomialElement right)
        {
            return new PolynomialElement(left.Coefficient / right.Coefficient, left.Exponent - right.Exponent);
        }
    }
}
