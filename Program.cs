using cobra;
using cobra.Classes;

class Program
{
    [STAThread]
    static void Main()
    {
        string code = @"
print(""hello workld"")
";


        var parser = new Parser();
        var parsedLines = parser.Parse(code);

        Evaluator evaluator = new Evaluator();
        evaluator.Evaluate(parsedLines);

        foreach (var line in parsedLines)
        {
            Console.WriteLine(line);
        }
    }
}