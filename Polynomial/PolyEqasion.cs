using System.Numerics;

namespace Polynomial
{
    public struct PolyEquation : IMathematicalElement<PolyEquation>,
        IAdditionOperators<PolyEquation, PolynomialElement, PolyEquation>, ISubtractionOperators<PolyEquation, PolynomialElement, PolyEquation>,
        IMultiplyOperators<PolyEquation, PolynomialElement, PolyEquation>, IDivisionOperators<PolyEquation, PolynomialElement, PolyEquation>
    {
        private const double EPSILON = 1e-5;
        private const int MAX_RESEARCH_DISTANCE = sizeof(int) * 8 - 1;

        private readonly List<PolynomialElement> elements = [];

        // O(n)
        public PolyEquation(IEnumerable<PolynomialElement> elements) : this()
        {
            Dictionary<int, double> exponentToCoefficient = [];

            foreach (var element in elements)
            {
                if (exponentToCoefficient.ContainsKey(element.Exponent))
                    exponentToCoefficient[element.Exponent] += element.Coefficient;
                else
                    exponentToCoefficient[element.Exponent] = element.Coefficient;
            }

            this.elements = exponentToCoefficient
                .Where(kvp => kvp.Value != 0)
                .Select(kvp => new PolynomialElement(kvp.Value, kvp.Key))
                .OrderByDescending(e => e.Exponent)
                .ToList();
        }

        public PolyEquation(params PolynomialElement[] elements) : this(elements.ToList())
        { }

        // O(n) nothing can be done
        public readonly double Evaluate(double x)
        {
            double result = 0;
            foreach (var element in elements)
                result += element.Coefficient * Math.Pow(x, element.Exponent);

            return result;
        }

        // O(n) nothing can be done
        public readonly PolyEquation Derivative()
        {
            List<PolynomialElement> derivativeElements = [];
            foreach (var element in elements)
            {
                PolynomialElement derivativeElement = element.Derivative();
                if (derivativeElement.Coefficient != 0)
                    derivativeElements.Add(derivativeElement);
            }
            return new PolyEquation(derivativeElements);
        }

        // O(1)
        private readonly HashSet<double> SolSimple(int degree)
        {
            if (2 < degree)
                throw new ArgumentException("Degree must be 2 or less for this method.");

            if (degree == 2)
            {
                double a = elements.FirstOrDefault(e => e.Exponent == 2).Coefficient;
                double b = elements.FirstOrDefault(e => e.Exponent == 1).Coefficient;
                double c = elements.FirstOrDefault(e => e.Exponent == 0).Coefficient;
                double discriminant = b * b - 4 * a * c;
                if (discriminant < 0)
                    return [];
                double root1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
                double root2 = (-b - Math.Sqrt(discriminant)) / (2 * a);
                return [root1, root2];
            }
            else if (degree == 1)
            {
                double b = elements.FirstOrDefault(e => e.Exponent == 1).Coefficient;
                double c = elements.FirstOrDefault(e => e.Exponent == 0).Coefficient;
                double root = -c / b;
                return [root];
            }

            return [];
        }

        // No change
        public readonly List<DeterministicPoint> GetDeterministicPoints()
        {
            List<DeterministicPoint> points = [];

            PolyEquation derivativePoly = Derivative();
            PolyEquation secondDerivativePoly = derivativePoly.Derivative();

            HashSet<double> criticalPoints = derivativePoly.Sol();

            foreach (double x in criticalPoints)
            {
                DeterministicPointType type = secondDerivativePoly.Evaluate(x) switch
                {
                    > 0 => DeterministicPointType.Minimal,
                    < 0 => DeterministicPointType.Maximal,
                    _ => DeterministicPointType.None
                };
                points.Add(new DeterministicPoint(type, x, Evaluate(x)));
            }

            return points;
        }

        // O(n log(n)) nothing can be done
        private readonly double FindRootBisection(double x1, double x2, bool isGoingUp)
        {
            double middleVal = Evaluate((x1 + x2) / 2);

            if (Math.Abs(middleVal) < EPSILON || x2 - x1 < EPSILON)
                return (x1 + x2) / 2;

            if (isGoingUp)
            {
                if (middleVal < 0)
                    return FindRootBisection((x1 + x2) / 2, x2, isGoingUp);
                else
                    return FindRootBisection(x1, (x1 + x2) / 2, isGoingUp);
            }
            else
            {
                if (middleVal < 0)
                    return FindRootBisection(x1, (x1 + x2) / 2, isGoingUp);
                else
                    return FindRootBisection((x1 + x2) / 2, x2, isGoingUp);
            }
        }

        // O(n) nothing can be done
        private readonly double FindRootInDetraction(double x, bool isLeft)
        {
            int sign = isLeft ? -1 : 1;
            double y = Evaluate(x);

            double beforeZero = x;
            double afterZero = x;
            for (int i = 0, step = 1; i < MAX_RESEARCH_DISTANCE; i++, step <<= 1)
            {
                double currentX = x + sign * step;

                if (Math.Sign(Evaluate(currentX)) * Math.Sign(y) < 0)
                {
                    afterZero = currentX;
                    break;
                }

                beforeZero = currentX;
            }

            if (afterZero == x) return double.NaN;


            double start = Math.Min(beforeZero, afterZero);
            double end = Math.Max(beforeZero, afterZero);

            return FindRootBisection(start, end, Evaluate(start) < Evaluate(end));
        }

        // O(n) nothing can be done
        public PolyEquation Normalize(int minExponent)
        {
            List<PolynomialElement> newElements = [];
            foreach (var element in elements)
                newElements.Add(element with { Exponent = element.Exponent - minExponent });

            return new PolyEquation(newElements);
        }

        // O(n) nothing can be done
        private readonly double FindRootNewton(double guess)
        {
            PolyEquation derivative = Derivative();
            double currentX = guess;

            for (int i = 0; i < 50; i++)
            {
                double y = Evaluate(currentX);
                if (Math.Abs(y) < EPSILON) return currentX;

                double dy = derivative.Evaluate(currentX);
                if (dy == 0) break;

                currentX -= y / dy;
            }
            return currentX;
        }

        // No change possible
        public HashSet<double> Sol()
        {
            int minExponent = elements.Min(e => e.Exponent);
            if (minExponent < 0)
            {
                HashSet<double> normalizedRoots = Normalize(minExponent).Sol();
                normalizedRoots.Remove(0);
                return normalizedRoots;
            }

            int degree = elements.Max(e => e.Exponent);
            if (degree <= 2)
                return SolSimple(degree);

            if (0 < minExponent)
            {
                HashSet<double> normalizedRoots = Normalize(minExponent).Sol();
                normalizedRoots.Add(0);
                return normalizedRoots;
            }



            PolyEquation derivativePoly = Derivative();

            List<DeterministicPoint> deterministicPoints = GetDeterministicPoints().OrderBy(p => p.X).ToList();

            if (deterministicPoints.Count == 0)
                return [FindRootNewton(0)];


            HashSet<double> results = [];

            results.UnionWith(deterministicPoints.Where(p => p.Y == 0).Select(p => p.X));

            DeterministicPoint first = deterministicPoints.First();

            if (derivativePoly.Evaluate(first.X - 0.1) * first.Y > 0)
            {
                double res = FindRootInDetraction(first.X, true);
                if (!double.IsNaN(res))
                    results.Add(res);
            }

            for (int i = 0; i < deterministicPoints.Count - 1; i++)
                if (Math.Sign(deterministicPoints[i].Y) * Math.Sign(deterministicPoints[i + 1].Y) < 0)
                    results.Add(FindRootBisection(deterministicPoints[i].X, deterministicPoints[i + 1].X,
                        deterministicPoints[i].Y < deterministicPoints[i + 1].Y));

            DeterministicPoint last = deterministicPoints.Last();

            if (Math.Sign(derivativePoly.Evaluate(last.X + 0.1)) * Math.Sign(last.Y) < 0)
            {
                double res = FindRootInDetraction(last.X, false);
                if (!double.IsNaN(res))
                    results.Add(res);
            }

            return results;
        }

        // No change possible
        public readonly override string ToString()
        {
            return string.Join(" + ", elements.OrderByDescending(e => e.Exponent));
        }

        public static PolyEquation operator +(PolyEquation left, PolyEquation right)
        {
            List<PolynomialElement> newElements = [.. left.elements];
            newElements.AddRange(right.elements);
            return new PolyEquation(newElements);
        }

        public static PolyEquation operator -(PolyEquation left, PolyEquation right)
        {
            List<PolynomialElement> newElements = [.. left.elements];
            newElements.AddRange(right.elements.Select(e => e with { Coefficient = -e.Coefficient, Exponent = e.Exponent }));
            return new PolyEquation(newElements);
        }


        public static PolyEquation operator *(PolyEquation left, double right)
        {
            return new PolyEquation(left.elements.Select(e => e with { Coefficient = e.Coefficient * right }));
        }

        public static PolyEquation operator /(PolyEquation left, double right)
        {
            return new PolyEquation(left.elements.Select(e => e with { Coefficient = e.Coefficient / right }));
        }


        public static PolyEquation operator +(PolyEquation left, PolynomialElement right)
        {
            List<PolynomialElement> newElements = [.. left.elements];
            newElements.Add(right);
            return new PolyEquation(newElements);
        }

        public static PolyEquation operator -(PolyEquation left, PolynomialElement right)
        {
            List<PolynomialElement> newElements = [.. left.elements];
            newElements.Add(right with { Coefficient = -right.Coefficient });
            return new PolyEquation(newElements);
        }


        public static PolyEquation operator *(PolyEquation left, PolynomialElement right)
        {
            return (left * right.Coefficient).Normalize(right.Exponent);
        }

        public static PolyEquation operator /(PolyEquation left, PolynomialElement right)
        {
            return (left * right.Coefficient).Normalize(right.Exponent);
        }


        public static PolyEquation operator *(PolyEquation left, PolyEquation right)
        {
            PolyEquation result = new(left.elements);
            foreach (var element in right.elements)
                result *= element;
            return result;
        }

        public static PolyEquation operator /(PolyEquation left, PolyEquation right)
        {
            PolyEquation result = new(left.elements);
            foreach (var element in right.elements)
                result /= element;
            return result;
        }
    }
}
