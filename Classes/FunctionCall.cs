using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cobra.Classes
{
    public class FunctionCall
    {
        public string FunctionName { get; set; }
        public List<Co_Object> Arguments { get; set; }

        public override string ToString()
        {
            return $"{FunctionName}({string.Join(", ", Arguments)})";
        }
    }

}
