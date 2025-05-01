using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cobra.Classes
{
    public class ParsedLine
    {
        public int IndentLevel { get; set; }
        public string FunctionName { get; set; }
        public List<Co_Object> Arguments { get; set; }

        public override string ToString()
        {
            return $"Indent: {IndentLevel}, Name: \"{FunctionName}\", Args: [{string.Join(", ", Arguments ?? new List<Co_Object>())}]";
        }


    }
}
