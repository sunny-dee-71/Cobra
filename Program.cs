using cobra;
using cobra.Classes;

class Program
{
    public static Dictionary<string, Co_Object> devVariables = new();
    [STAThread]
    static async Task Main()
    {
        devVariables.Add("exitImmediately", new Co_Object(true));
        devVariables.Add("showParsed", new Co_Object(false));
        string code = @"
# showParsed = false
# exitImmediately = false
import(test)
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
repeat(Thing < 0)
\2\set(Thing, Thing + 1)
\2\clear()
\2\print(Thing)

def test(a)
\1\print(""This is a test function!"")

test(1)
//And that is it

print(eq(1, 1))

def thing(a, b)
\1\return(a)


print(test.funcTime(10))


";

        var parser = new Parser();
        var parsedLines = parser.Parse(code);

        Evaluator evaluator = new Evaluator();
        await Task.Delay(100);
        try
        {
            await evaluator.Evaluate(parsedLines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error in evaluation: {ex.Message}");
        }

        if (devVariables.ContainsKey("showParsed") && devVariables["showParsed"].Type == Co_Object.ObjectType.Boolean && (bool)devVariables["showParsed"].Value)
        {
            foreach (var line in parsedLines)
            {
                Console.WriteLine(line);
            }
        }

        if (!Evaluator.Exiting)
        {
            var Exiter = parser.Parse(@"exit(0, "" Exited successfully. "")");
            await evaluator.Evaluate(Exiter);
        }

        if (devVariables.ContainsKey("exitImmediately") && devVariables["exitImmediately"].Type == Co_Object.ObjectType.Boolean && !(bool)devVariables["exitImmediately"].Value)
        {
            Console.WriteLine("Exited.");
            Console.WriteLine($" ├─ Code   : {Evaluator.ExitCode}");
            Console.WriteLine($" └─ Message: {Evaluator.ExitMessage ?? "None"}");
            Console.WriteLine(" \nPress any key to exit...");
            Console.ReadKey();
            Environment.Exit(Evaluator.ExitCode);
        }
    }
}
