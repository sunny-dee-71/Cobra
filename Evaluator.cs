using cobra.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace cobra
{
    public class Evaluator
    {
        public Dictionary<string, Co_Object> variables = new();
        public static Dictionary<string, Co_Object> constants = new();
        private Dictionary<string, UserFunction> userFunctions = new();
        public Dictionary<string, Class> loadedLibarys = new();
        public static bool Exiting = false;
        public static string ExitMessage = null;
        public static int ExitCode = 0;

        public async Task<Class> Evaluate(List<ParsedLine> lines )
        {
            int i = 0;
            while (i < lines.Count)
            {
                if (Exiting)
                    return new Class { variables = variables, userFunctions = userFunctions };
                var line = lines[i];
                string name = line.FunctionName;
                var args = line.Arguments;
                int indent = line.IndentLevel;

                if (name == "if")
                {
                    var resolvedArgs = await ResolveArguments(args);
                    if (resolvedArgs.Count != 1 || resolvedArgs[0].Type != Co_Object.ObjectType.Boolean)
                        throw new Exception("[if] expects 1 boolean argument");

                    bool condition = (bool)resolvedArgs[0].Value;

                    int blockStart = i + 1;
                    int blockEnd = blockStart;
                    while (blockEnd < lines.Count && lines[blockEnd].IndentLevel > indent)
                        blockEnd++;

                    if (condition)
                    {
                        var block = lines.GetRange(blockStart, blockEnd - blockStart);
                        await Evaluate(block);
                    }

                    i = blockEnd;
                }
                else if (name == "def")
                {
                    if (args.Count < 1)
                        throw new Exception("[def] must have a function name");

                    string functionName = args[0].Value.ToString();
                    List<string> parameters = new List<string>();

                    for (int j = 1; j < args.Count; j++)
                    {
                        if (args[j].Type != Co_Object.ObjectType.String)
                            throw new Exception("Function parameters must be identifiers (strings)");

                        parameters.Add(args[j].Value.ToString());
                    }

                    int blockStart = i + 1;
                    int blockEnd = blockStart;

                    while (blockEnd < lines.Count && lines[blockEnd].IndentLevel > indent)
                        blockEnd++;

                    var body = lines.GetRange(blockStart, blockEnd - blockStart);


                    userFunctions[functionName] = new UserFunction(functionName, parameters, body);

                    i = blockEnd;
                }
                else if (name == "return")
                {
                    if (args.Count == 0)
                        throw new Exception("[return] requires a value");

                    var returnValue = await ResolveArguments(args);
                    throw new ReturnException(returnValue[0]);
                }
                else if (name == "repeat")
                {
                    var resolvedArgs = await ResolveArguments(args);
                    if (resolvedArgs.Count != 1)
                        throw new Exception("[repeat] expects 1 argument");

                    int blockStart = i + 1;
                    int blockEnd = blockStart;
                    while (blockEnd < lines.Count && lines[blockEnd].IndentLevel > indent)
                        blockEnd++;

                    var block = lines.GetRange(blockStart, blockEnd - blockStart);

                    double maxIterations = 100;
                    int iteration = 0;

                    var arg = resolvedArgs[0];

                    if (arg.Type == Co_Object.ObjectType.Int)
                    {
                        int times = (int)arg.Value;
                        for (; iteration < times; iteration++)
                            await Evaluate(block);
                    }
                    else if (arg.Type == Co_Object.ObjectType.Boolean)
                    {
                        while ((bool)arg.Value)
                        {
                            await Evaluate(block);

                            resolvedArgs = await ResolveArguments(args);
                            arg = resolvedArgs[0];

                            if (arg.Type != Co_Object.ObjectType.Boolean)
                                throw new Exception("[repeat while] condition must remain a boolean : you gave: " + arg.Type);

                            iteration++;
                        }
                    }
                    else
                    {
                        throw new Exception("[repeat] argument must be either an integer or boolean : you gave: " + arg.Type);
                    }


                    i = blockEnd;
                }
                else if (name == "import")
                {
                    var thing1 = Converter.ConvertVarToString(args[0]);
                    if (args.Count != 1 || thing1.Type != Co_Object.ObjectType.String )
                        throw new Exception("[import] requires a single string argument (resource name)");

                    string script = Importer.LoadLibrary(thing1.Value.ToString());

                    var parser = new Parser();
                    var parsedLines = parser.Parse(script);

                    var thing = await Evaluate(parsedLines);

                    loadedLibarys[thing1.Value.ToString()] = thing;
                    i++;
                }

                else
                {
                    await RunFunction(name, args);
                    i++;
                }
            }
            return new Class { variables = variables, userFunctions = userFunctions };
        }

        public async Task<Co_Object> RunFunction(string functionName, List<Co_Object> args)
        {
            Class calss = new Class { variables = variables, userFunctions = userFunctions };

            if (functionName.Contains("."))
            {
                string[] parts = functionName.Split('.');
                string className = parts[0];
                functionName = parts[1];
                calss = null;
                loadedLibarys.TryGetValue(className, out calss);
            }


            if (calss.userFunctions.ContainsKey(functionName))
            {
                var func = userFunctions[functionName];
                var resolvedArgs = await ResolveArguments(args);
                return await func.Invoke(resolvedArgs, calss);
            }
            else
            {
                var func = Objects.GetFunction(functionName);
                if (func == null)
                {
                    Console.WriteLine($"Function '{functionName}' not found.");
                    return new Co_Object(null);
                }

                try
                {
                    List<Co_Object> resolvedArgs;

                    if (functionName == "set" && args.Count >= 2)
                    {
                        var expressionArgs = args.GetRange(1, args.Count - 1);
                        var resolvedExpression = await ResolveArguments(expressionArgs);
                        var thiss = new Co_Object(this);
                        resolvedArgs = new List<Co_Object> { args[0], resolvedExpression[0], thiss };

                    }
                    else
                    {
                        resolvedArgs = await ResolveArguments(args);
                    }

                    return await func.Invoke(resolvedArgs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Error in function '{functionName}': {ex.Message}");
                }
            }

            return new Co_Object(null);
        }

        public async Task<Co_Object> getVar(string name)
        {
            Console.WriteLine($"[getVar] {name}");
            Class calss = new Class { variables = variables, userFunctions = userFunctions };
            string className = "this";
            if (name.Contains("."))
            {
                string[] parts = name.Split('.');
                className = parts[0];
                name = parts[1];
                calss = null;
                loadedLibarys.TryGetValue(className, out calss);
            }

            if (calss == null)
                throw new Exception($"[getVar] Class '{className}' not found.");

            if (calss.variables.TryGetValue(name, out var value))
            {
                return value;
            }
            return new Co_Object(null);
        }

        public async Task<List<Co_Object>> ResolveArguments(List<Co_Object> args)
        {
            var resolved = new List<Co_Object>();
            int i = 0;

            while (i < args.Count)
            {
                var arg = args[i];

                if (arg.Type == Co_Object.ObjectType.Variable)
                {
                    string varName = arg.Value.ToString();
                    try
                    {
                        arg = await getVar(varName);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"[Resolve] Variable '{varName}' not found. " + ex.Message, ex);
                    }
                }

                if (arg.Type == Co_Object.ObjectType.Function)
                {
                    var funcCall = (FunctionCall)arg.Value;
                    var funcArgs = await ResolveArguments(funcCall.Arguments);
                    arg = await RunFunction(funcCall.FunctionName, funcArgs);
                }

                if (i + 2 < args.Count)
                {
                    var op = args[i + 1];
                    var right = args[i + 2];

                    if (op.Type == Co_Object.ObjectType.String && IsOperator(op.Value.ToString()))
                    {
                        if (right.Type == Co_Object.ObjectType.Variable)
                        {
                            string varName = right.Value.ToString();
                            try
                            {
                                right = await getVar(varName);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"[Resolve] Variable '{varName}' not found. " + ex.Message, ex);
                            }
                        }
                        else if (right.Type == Co_Object.ObjectType.Function)
                        {
                            var funcCall = (FunctionCall)right.Value;
                            var funcArgs = await ResolveArguments(funcCall.Arguments);
                            right = await RunFunction(funcCall.FunctionName, funcArgs);
                        }

                        var result = EvaluateExpression(arg, op.Value.ToString(), right);
                        resolved.Add(result);
                        i += 3;
                        continue;
                    }
                }

                resolved.Add(arg);
                i++;
            }

            return resolved;
        }





        private bool IsOperator(string value)
        {
            return value is "+" or "-" or "&&" or "==" or "!=" or ">" or "<" or ">=" or "<=" or "/" or "*" or "%" or "||";
        }

        public static Co_Object EvaluateExpression(Co_Object left, string op, Co_Object right)
        {
            List<Co_Object> args = new List<Co_Object> { left, right };
            args = Converter.ConvertToLowestNumberType(args);
            left = args[0];
            right = args[1];
            switch (op)
            {
                case "+":
                    if (left.Type == Co_Object.ObjectType.String || right.Type == Co_Object.ObjectType.String)
                        return new Co_Object(left.Value.ToString() + right.Value.ToString());

                    if (left.Type == Co_Object.ObjectType.Int && right.Type == Co_Object.ObjectType.Int)
                        return new Co_Object((int)left.Value + (int)right.Value);

                    if (left.Type == Co_Object.ObjectType.Float && right.Type == Co_Object.ObjectType.Float)
                        return new Co_Object((float)left.Value + (float)right.Value);

                    throw new Exception($"Unsupported types for +: {left.Type} + {right.Type}");

                case "-":
                    if (left.Type == Co_Object.ObjectType.Int && right.Type == Co_Object.ObjectType.Int)
                        return new Co_Object((int)left.Value - (int)right.Value);

                    if (left.Type == Co_Object.ObjectType.Float && right.Type == Co_Object.ObjectType.Float)
                        return new Co_Object((float)left.Value - (float)right.Value);

                    throw new Exception($"Unsupported types for -: {left.Type} - {right.Type}");

                case "&&":
                    if (left.Type == Co_Object.ObjectType.Boolean && right.Type == Co_Object.ObjectType.Boolean)
                        return new Co_Object((bool)left.Value && (bool)right.Value);

                    throw new Exception("&& only supports boolean values");

                case "==":
                    return new Co_Object(Equals(left.Value, right.Value));

                case "!=":
                    return new Co_Object(!Equals(left.Value, right.Value));

                case ">":
                    if (left.Type == Co_Object.ObjectType.Int && right.Type == Co_Object.ObjectType.Int)
                        return new Co_Object((int)left.Value > (int)right.Value);

                    if (left.Type == Co_Object.ObjectType.Float && right.Type == Co_Object.ObjectType.Float)
                        return new Co_Object((float)left.Value > (float)right.Value);

                    if (left.Type == Co_Object.ObjectType.String && right.Type == Co_Object.ObjectType.String)
                        return new Co_Object(string.Compare((string)left.Value, (string)right.Value) > 0);

                    throw new Exception("> only supports int, float, or string");

                case "<":
                    if (left.Type == Co_Object.ObjectType.Int && right.Type == Co_Object.ObjectType.Int)
                        return new Co_Object((int)left.Value < (int)right.Value);

                    if (left.Type == Co_Object.ObjectType.Float && right.Type == Co_Object.ObjectType.Float)
                        return new Co_Object((float)left.Value < (float)right.Value);

                    if (left.Type == Co_Object.ObjectType.String && right.Type == Co_Object.ObjectType.String)
                        return new Co_Object(string.Compare((string)left.Value, (string)right.Value) < 0);

                    throw new Exception("< only supports int, float, or string");

                case ">=":
                    if (left.Type == Co_Object.ObjectType.Int && right.Type == Co_Object.ObjectType.Int)
                        return new Co_Object((int)left.Value >= (int)right.Value);

                    if (left.Type == Co_Object.ObjectType.Float && right.Type == Co_Object.ObjectType.Float)
                        return new Co_Object((float)left.Value >= (float)right.Value);

                    if (left.Type == Co_Object.ObjectType.String && right.Type == Co_Object.ObjectType.String)
                        return new Co_Object(string.Compare((string)left.Value, (string)right.Value) >= 0);

                    throw new Exception(">= only supports int, float, or string");

                case "<=":
                    if (left.Type == Co_Object.ObjectType.Int && right.Type == Co_Object.ObjectType.Int)
                        return new Co_Object((int)left.Value <= (int)right.Value);

                    if (left.Type == Co_Object.ObjectType.Float && right.Type == Co_Object.ObjectType.Float)
                        return new Co_Object((float)left.Value <= (float)right.Value);

                    if (left.Type == Co_Object.ObjectType.String && right.Type == Co_Object.ObjectType.String)
                        return new Co_Object(string.Compare((string)left.Value, (string)right.Value) <= 0);

                    throw new Exception("<= only supports int, float, or string");

                case "/":
                    if (left.Type == Co_Object.ObjectType.Int && right.Type == Co_Object.ObjectType.Int)
                        return new Co_Object((int)left.Value / (int)right.Value);
                    if (left.Type == Co_Object.ObjectType.Float && right.Type == Co_Object.ObjectType.Float)
                        return new Co_Object((float)left.Value / (float)right.Value);
                    throw new Exception("/ only supports int or float : recived " + left + " " + right);

                case "%":
                    if (left.Type == Co_Object.ObjectType.Int && right.Type == Co_Object.ObjectType.Int)
                        return new Co_Object((int)left.Value % (int)right.Value);
                    if (left.Type == Co_Object.ObjectType.Float && right.Type == Co_Object.ObjectType.Float)
                        return new Co_Object((float)left.Value % (float)right.Value);
                    throw new Exception("% only supports int or float");

                case "*":
                    if (left.Type == Co_Object.ObjectType.Int && right.Type == Co_Object.ObjectType.Int)
                        return new Co_Object((int)left.Value * (int)right.Value);
                    if (left.Type == Co_Object.ObjectType.Float && right.Type == Co_Object.ObjectType.Float)
                        return new Co_Object((float)left.Value * (float)right.Value);
                    throw new Exception("* only supports int or float");

                case "||":
                    if (left.Type == Co_Object.ObjectType.Boolean && right.Type == Co_Object.ObjectType.Boolean)
                        return new Co_Object((bool)left.Value || (bool)right.Value);
                    throw new Exception("|| only supports boolean values");

                default:
                    throw new Exception($"Unknown operator '{op}'");
            }
        }



    }

}
