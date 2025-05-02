using cobra;
using cobra.Classes;

class Program
{
    [STAThread]
    static async Task Main()
    {
        string code = @"
print(""Welcome To Cobra"")
set(""count"", 0)
set(Hi, ""Hello"")
print(Hi)
set(shouldPrint, true)

if(shouldPrint)
\1\print(""This should print because the condition is true!"")

repeat(3)
\1\print(""Loop running"")
\2\repeat(3)
\3\print(""thing"")

//And that is it

";

        var parser = new Parser();
        var parsedLines = parser.Parse(code);

        Evaluator evaluator = new Evaluator();
        await evaluator.Evaluate(parsedLines);

        foreach (var line in parsedLines)
        {
            Console.WriteLine(line);
        }
    }
}
