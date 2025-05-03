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

\2\repeat(false)
\3\print(""thing"")

set(Thing, 0)
\1\print(Thing)
repeat(Thing < 100)
\2\set(Thing, Thing + 1)
\2\print(Thing)



//And that is it

";

        var parser = new Parser();
        var parsedLines = parser.Parse(code);

        Evaluator evaluator = new Evaluator();
        await Task.Delay(100);
        await evaluator.Evaluate(parsedLines);

        foreach (var line in parsedLines)
        {
            Console.WriteLine(line);
        }
    }
}
