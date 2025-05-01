using cobra.Classes;
using System;
using System.Collections.Generic;

namespace cobra
{
    public class Evaluator
    {
        public void Evaluate(List<ParsedLine> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

            }
        }


        public void RunFunction(string functionName, List<object> args)
        {
            Console.WriteLine($"Running function: {functionName} with arguments: {string.Join(", ", args)}");
        }
    }
}
