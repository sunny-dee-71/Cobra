using cobra.Classes;
using System;
using System.Collections.Generic;

namespace cobra
{
    public class Evaluator
    {
        public static Dictionary<string, Co_Object> variables = new();

        public async Task Evaluate(List<ParsedLine> lines)
        {
            int i = 0;
            while (i < lines.Count)
            {
                var line = lines[i];
                string name = line.FunctionName;
                var args = line.Arguments;
                int indent = line.IndentLevel;

                if (name == "if")
                {
                    var resolvedArgs = ResolveArguments(args);
                    if (resolvedArgs.Count != 1 || resolvedArgs[0].Type != Co_Object.ObjectType.Boolean)
                        throw new Exception($"[if] expects 1 boolean argument");

                    bool condition = (bool)resolvedArgs[0].Value;

                    // Get block of lines with greater indent
                    int blockStart = i + 1;
                    int blockEnd = blockStart;
                    while (blockEnd < lines.Count && lines[blockEnd].IndentLevel > indent)
                        blockEnd++;

                    if (condition)
                    {
                        var block = lines.GetRange(blockStart, blockEnd - blockStart);
                        await Evaluate(block); // Recursively evaluate the block
                    }

                    i = blockEnd; // Skip the block
                }
                else if (name == "repeat")
                {
                    var resolvedArgs = ResolveArguments(args);
                    if (resolvedArgs.Count != 1 || resolvedArgs[0].Type != Co_Object.ObjectType.Int)
                        throw new Exception($"[repeat] expects 1 integer argument");

                    int times = (int)resolvedArgs[0].Value;

                    int blockStart = i + 1;
                    int blockEnd = blockStart;
                    while (blockEnd < lines.Count && lines[blockEnd].IndentLevel > indent)
                        blockEnd++;

                    var block = lines.GetRange(blockStart, blockEnd - blockStart);

                    for (int r = 0; r < times; r++)
                        await Evaluate(block); // Recursively evaluate the block

                    i = blockEnd;
                }
                else
                {
                    await RunFunction(name, args);
                    i++;
                }
            }
        }




        public async Task RunFunction(string functionName, List<Co_Object> args)
        {
            var func = Objects.GetFunction(functionName);
            if (func == null)
            {
                Console.WriteLine($"Function '{functionName}' not found.");
                return;
            }

            try
            {
                List<Co_Object> resolvedArgs;

                if (functionName == "set" && args.Count >= 2)
                {
                    var resolvedSecond = ResolveArguments(new List<Co_Object> { args[1] });
                    resolvedArgs = new List<Co_Object> { args[0], resolvedSecond[0] };
                }
                else
                {
                    resolvedArgs = ResolveArguments(args);
                }

                await func.Invoke(resolvedArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error in function '{functionName}': {ex.Message}");
            }
        }


        private List<Co_Object> ResolveArguments(List<Co_Object> args)
        {
            var resolved = new List<Co_Object>();
            foreach (var arg in args)
            {
                if (arg.Type == Co_Object.ObjectType.Variable)
                {
                    string varName = arg.Value.ToString();
                    if (variables.TryGetValue(varName, out var resolvedValue))
                    {
                        resolved.Add(resolvedValue);
                    }
                    else
                    {
                        throw new Exception($"[Resolve] Variable '{varName}' not found.");
                    }
                }
                else
                {
                    resolved.Add(arg);
                }
            }
            return resolved;
        }



    }

}
