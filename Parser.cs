using System;
using System.Collections.Generic;
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
                        throw new Exception($"Syntax error in line: {line}");

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

        private List<Co_Object> ParseArguments(string input)
        {
            var args = new List<Co_Object>();
            var matches = Regex.Matches(input, "\".*?\"|[^,\\s]+");

            foreach (Match match in matches)
            {
                string val = match.Value.Trim();

                if (val.StartsWith("\"") && val.EndsWith("\""))
                {
                    val = val.Substring(1, val.Length - 2);
                    args.Add(new Co_Object(val));
                }
                else if (int.TryParse(val, out int intValue))
                {
                    args.Add(new Co_Object(intValue));
                }
                else if (float.TryParse(val, out float floatValue))
                {
                    args.Add(new Co_Object(floatValue));
                }
                else if (bool.TryParse(val, out bool boolValue))
                {
                    args.Add(new Co_Object(boolValue));
                }
                else
                {
                    var obj = new Co_Object(val);
                    obj.Type = Co_Object.ObjectType.Variable;
                    args.Add(obj);
                }

            }

            return args;
        }

    }
}
