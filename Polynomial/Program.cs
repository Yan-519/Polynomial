using Polynomial;

string latex = "5x^{-1.2}-x^{3}";

PolyEquation poly = Utility.LatexConvertor(latex);
//new(
//new(1, 7),
//new(-58, 3),
//new(10, 1),
//new(-9),
//new(15, -3)
//);

Console.WriteLine(latex + " = " + poly);

HashSet<double> roots = poly.Solve();

Console.WriteLine();
if (roots.Count == 0)
    Console.WriteLine("No real roots found.");

else
{
    Console.WriteLine("Roots:");
    foreach (var root in roots)
        Console.WriteLine(root);
}

//while (true) Console.ReadLine();