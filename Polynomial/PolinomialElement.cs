using System.Numerics;

namespace Polynomial
{
    public record struct PolynomialElement(double Coefficient, int Exponent) : IMathematicalElement<PolynomialElement>,
        IMultiplyOperators<PolynomialElement, PolynomialElement, PolynomialElement>, IDivisionOperators<PolynomialElement, PolynomialElement, PolynomialElement>
    {
        public double Coefficient = Coefficient;
        public int Exponent = Exponent;

        public PolynomialElement(double coefficient) : this(coefficient, 0)
        { }

        public readonly double Evaluate(double x)
        {
            return Coefficient * Math.Pow(x, Exponent);
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
