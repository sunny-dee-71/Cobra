using System;
using System.Collections.Generic;

namespace cobra.Classes
{
    public class Function
    {
        public string Name { get; private set; }
        public List<string> ParameterNames { get; private set; }
        public Func<List<Co_Object>, Task> Body { get; private set; }

        public Function(string name, List<string> parameterNames, Func<List<Co_Object>, Task> body)
        {
            Name = name;
            ParameterNames = parameterNames ?? new List<string>();
            Body = body;
        }

        public async Task Invoke(List<Co_Object> arguments)
        {
            if (arguments == null) arguments = new List<Co_Object>();

            if (arguments.Count != ParameterNames.Count)
            {
                Console.WriteLine($"[ERROR] Function '{Name}' expected {ParameterNames.Count} argument(s), but got {arguments.Count}.");
                return;
            }

            try
            {
                await Body(arguments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Function '{Name}' threw an exception: {ex.Message}");
            }
        }
    }

}
