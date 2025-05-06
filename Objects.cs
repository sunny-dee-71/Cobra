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
                await Task.Run(() =>
                {
                    Console.WriteLine(args[0].Value);
                });
                return new Co_Object(args[0].Value);
            }));


            Functions.Add(new Function("clear", new List<string> { }, async args =>
            {
                Console.Clear();
                return new Co_Object(null);
            }));

            Functions.Add(new Function("exit", new List<string> { }, async args =>
            {
                int thing = 0;

                if (args.Count > 0 && args[0].Value != null)
                {
                    if (args[0].Type == Co_Object.ObjectType.Int)
                    {
                        thing = (int)args[0].Value;
                    }
                }

                Evaluator.Exiting = true;
                Evaluator.ExitCode = thing;

                if (args.Count > 1 && args[1].Value != null)
                {
                    if (args[1].Type == Co_Object.ObjectType.String)
                    {
                        Evaluator.ExitMessage = (string)args[1].Value;
                    }
                }

                return new Co_Object(null);
            }));


            Functions.Add(new Function("wait", new List<string> { "seconds" }, async args =>
            {
                if (args[0].Type == Co_Object.ObjectType.Int)
                {
                    int seconds = (int)args[0].Value;
                    await Task.Delay(seconds * 1000);
                    return new Co_Object(args[0].Value.ToString());
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
                return new Co_Object(args[0].Value);
            }));


            Functions.Add(new Function("set", new List<string> { "name", "value", "instance" }, async args =>
            {
                string varName = args[0].Value.ToString();
                var value = args[1];
                Evaluator instance = args[2].Value as Evaluator;

                if (value.Type == Co_Object.ObjectType.Variable)
                {
                    if (instance.variables.TryGetValue(value.Value.ToString(), out var resolved))
                    {
                        value = resolved;
                    }
                    else
                    {
                        throw new Exception($"[set] Variable '{value.Value}' not found.");
                    }
                }

                instance.variables[varName] = value;

                return new Co_Object(varName);

            }));

            Functions.Add(new Function("input", new List<string> { "prompt" }, async args =>
            {
                Console.Write(args[0].Value);
                string input = await Task.Run(() => Console.ReadLine());
                return new Co_Object(input);
            }));

            Functions.Add(new Function("getKey", new List<string> { "prompt" }, async args =>
            {
                Console.Write(args[0].Value);
                string input = await Task.Run(() => Console.ReadKey().ToString());
                Console.WriteLine();
                return new Co_Object(input);
            }));

            Functions.Add(new Function("time", new List<string> { }, async args =>
            {
                return new Co_Object(Time.time);
            }));
        }

        public static Function GetFunction(string name)
        {
            return Functions.FirstOrDefault(f => f.Name == name);
        }
    }
}
