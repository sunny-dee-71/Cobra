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
            var parenDepth = 0;
            var currentArg = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '(') parenDepth++;
                if (c == ')') parenDepth--;

                if (c == ',' && parenDepth == 0)
                {
                    AddParsedArgument(currentArg.ToString(), args);
                    currentArg.Clear();
                }
                else
                {
                    currentArg.Append(c);
                }
            }

            if (currentArg.Length > 0)
            {
                AddParsedArgument(currentArg.ToString(), args);
            }

            return args;
        }

        private void AddParsedArgument(string raw, List<Co_Object> args)
        {
            raw = raw.Trim();

            var binMatch = Regex.Match(raw, @"^(.+?)\s*(==|!=|>=|<=|>|<|\+|\-|\*|\/|%)\s*(.+)$");
            if (binMatch.Success)
            {
                var left = ParseSingleArgument(binMatch.Groups[1].Value.Trim());
                var op = new Co_Object(binMatch.Groups[2].Value.Trim());
                var right = ParseSingleArgument(binMatch.Groups[3].Value.Trim());

                args.Add(left);
                args.Add(op);
                args.Add(right);
            }
            else
            {
                args.Add(ParseSingleArgument(raw));
            }
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
            else if (val.Contains("(") && val.EndsWith(")"))
            {
                int openParenIndex = val.IndexOf('(');
                string funcName = val.Substring(0, openParenIndex).Trim();

                int parenDepth = 1;
                int i = openParenIndex + 1;
                StringBuilder argsBuilder = new StringBuilder();

                while (i < val.Length && parenDepth > 0)
                {
                    char c = val[i];

                    if (c == '(') parenDepth++;
                    else if (c == ')') parenDepth--;

                    if (parenDepth > 0) argsBuilder.Append(c);
                    i++;
                }

                var argsString = argsBuilder.ToString();
                var funcArgs = ParseArguments(argsString);

                return new Co_Object(new FunctionCall
                {
                    FunctionName = funcName,
                    Arguments = funcArgs
                });
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
