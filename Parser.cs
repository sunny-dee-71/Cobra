using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace cobra.Classes
{
    public class Parser
    {
        private static readonly Regex LinePattern = new Regex(@"^(?:\\(\d+)\\)?(\w+)\((.*)\)$");
        public List<ParsedLine> Parse(string code)
        {
            var lines = code.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new List<ParsedLine>();
            var controlFlowStack = new Stack<string>();

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;

                int commentIndex = line.IndexOf("//");
                if (commentIndex >= 0)
                {
                    line = line.Substring(0, commentIndex).Trim();
                }

                line = new string(line.Where(c => !char.IsWhiteSpace(c)).ToArray());


                {
                    var match = LinePattern.Match(line);
                    if (!match.Success)
                        continue;

                    int indent = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
                    string name = match.Groups[2].Value;
                    string argsRaw = match.Groups[3].Value;

                    var args = ParseArguments(argsRaw);

                    result.Add(new ParsedLine
                    {
                        IndentLevel = indent,
                        FunctionName = name,
                        Arguments = args
                    });
                }
            }

            return result;
        }

        public List<Co_Object> ParseArguments(string input)
        {
            var args = new List<Co_Object>();
            var matches = Regex.Matches(input, "\".*?\"|[^,()]+|[(),]");

            var parenDepth = 0;
            var currentArg = new StringBuilder();

            foreach (Match match in matches)
            {
                string matchValue = match.Value.Trim();

                // Handle opening parentheses (start of function calls or nested expressions)
                if (matchValue == "(")
                {
                    parenDepth++;
                    currentArg.Append(matchValue);
                }
                // Handle closing parentheses (end of function calls or nested expressions)
                else if (matchValue == ")")
                {
                    parenDepth--;
                    currentArg.Append(matchValue);
                    if (parenDepth == 0) // End of a function call or nested expression
                    {
                        args.Add(ParseSingleArgument(currentArg.ToString().Trim()));
                        currentArg.Clear();
                    }
                }
                // Handle commas outside function calls
                else if (matchValue == ",")
                {
                    if (parenDepth == 0) // Split arguments at commas outside function calls
                    {
                        if (currentArg.Length > 0)
                        {
                            args.Add(ParseSingleArgument(currentArg.ToString().Trim()));
                            currentArg.Clear();
                        }
                    }
                    else
                    {
                        currentArg.Append(matchValue);
                    }
                }
                // Otherwise, just append the value (operator, string, etc.)
                else
                {
                    currentArg.Append(matchValue);
                }
            }

            // Add the last argument if any
            if (currentArg.Length > 0)
            {
                // Check for binary operation and split the operator separately
                var binMatch = Regex.Match(currentArg.ToString(), @"^(.+?)\s*(==|!=|>=|<=|>|<|\+|\-|\*|\/|%)\s*(.+)$");

                if (binMatch.Success)
                {
                    var left = ParseSingleArgument(binMatch.Groups[1].Value.Trim());
                    var op = new Co_Object(binMatch.Groups[2].Value.Trim()); // Operator as its own argument
                    var right = ParseSingleArgument(binMatch.Groups[3].Value.Trim());

                    // Add the parsed arguments and the operator as a separate item
                    args.Add(left);
                    args.Add(op);
                    args.Add(right);
                }
                else
                {
                    // If there is no binary operation, add it as a single argument
                    args.Add(ParseSingleArgument(currentArg.ToString().Trim()));
                }
            }

            return args;
        }






        private Co_Object ParseSingleArgument(string val)
        {
            if (val.StartsWith("\"") && val.EndsWith("\""))
            {
                val = val.Substring(1, val.Length - 2);
                return new Co_Object(val);
            }
            else if (int.TryParse(val, out int intValue))
            {
                return new Co_Object(intValue);
            }
            else if (float.TryParse(val, out float floatValue))
            {
                return new Co_Object(floatValue);
            }
            else if (bool.TryParse(val, out bool boolValue))
            {
                return new Co_Object(boolValue);
            }
            else if (val.Contains("(") && val.Contains(")"))
            {
                var funcName = val.Substring(0, val.IndexOf("("));
                var argsString = val.Substring(val.IndexOf("(") + 1, val.IndexOf(")") - val.IndexOf("(") - 1);

                var funcArgs = ParseArguments(argsString);

                return new Co_Object(new FunctionCall { FunctionName = funcName, Arguments = funcArgs });
            }
            else
            {
                var obj = new Co_Object(val);
                obj.Type = Co_Object.ObjectType.Variable;
                return obj;
            }
        }


    }
}
