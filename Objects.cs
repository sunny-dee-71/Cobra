using cobra.Classes;
using System;
using System.Collections.Generic;

namespace cobra
{
    class Objects
    {
        public static List<Function> Functions = new List<Function>();

        static Objects()
        {
            Functions.Add(new Function("print", new List<string> { "message" }, async args =>
            {
                Console.WriteLine(args[0].Value);
            }));

            Functions.Add(new Function("add", new List<string> { "a", "b" }, async args =>
            {
                if (args[0].Type == Co_Object.ObjectType.Int && args[1].Type == Co_Object.ObjectType.Int)
                {
                    int result = (int)args[0].Value + (int)args[1].Value;
                    Console.WriteLine("Sum: " + result);
                }
            }));

            Functions.Add(new Function("clear", new List<string> { }, async args =>
            {
                Console.Clear();
            }));

            Functions.Add(new Function("exit", new List<string> { }, async args =>
            {
                Environment.Exit(0);
            }));

            Functions.Add(new Function("wait", new List<string> { "seconds" }, async args =>
            {
                if (args[0].Type == Co_Object.ObjectType.Int)
                {
                    int seconds = (int)args[0].Value;
                    await Task.Delay(seconds * 1000);
                }
                else
                {
                    throw new Exception($"[wait] Expected an integer, got {args[0].Type}");
                }
            }));

            Functions.Add(new Function("slowPrint", new List<string> { "message", "seconds" }, async args =>
            {
                var messageObj = args[0];
                var secondsObj = args[1];

                if (messageObj.Type != Co_Object.ObjectType.String)
                    throw new Exception("[slowPrint] First argument must be a string.");

                if (secondsObj.Type != Co_Object.ObjectType.Float)
                    throw new Exception("[slowPrint] Second argument must be a float.");

                string message = (string)messageObj.Value;
                float seconds = (float)secondsObj.Value;

                foreach (char c in message)
                {
                    Console.Write(c);
                    await Task.Delay((int)(seconds * 1000)); 
                }

                Console.WriteLine();
            }));

            Functions.Add(new Function("Test", new List<string> { "Repeats" }, async args =>
            {
                int repeats = (int)args[0].Value;
                int count = 0;
                DateTime startTime = DateTime.Now;

                for (int i = 0; i < repeats; i++)
                {                    count++;
                    Console.Write($"");

                    if ((DateTime.Now - startTime).TotalSeconds >= 0.1)
                    {
                        Console.WriteLine($"{count} functions in the last 0.1 seconds.");


                        startTime = DateTime.Now;

                        count = 0;
                    }

                    
                }
                Console.WriteLine($"Total functions executed: {count}");
            }));

            Functions.Add(new Function("set", new List<string> { "name", "value" }, async args =>
            {
                string varName = args[0].Value.ToString();
                var value = args[1];

                if (value.Type == Co_Object.ObjectType.Variable)
                {
                    if (Evaluator.variables.TryGetValue(value.Value.ToString(), out var resolved))
                    {
                        value = resolved;
                    }
                    else
                    {
                        throw new Exception($"[set] Variable '{value.Value}' not found.");
                    }
                }

                Evaluator.variables[varName] = value;

            }));



        }

        public static Function GetFunction(string name)
        {
            return Functions.FirstOrDefault(f => f.Name == name);
        }
    }
}
