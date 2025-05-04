using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cobra.Classes
{
    public class ReturnException : Exception
    {
        public Co_Object ReturnValue { get; }

        public ReturnException(Co_Object returnValue)
        {
            ReturnValue = returnValue;
        }
    }
}

