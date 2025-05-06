using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cobra.Classes
{
    public class UserFunction
    {
        public List<string> Parameters { get; }
        public List<ParsedLine> Body { get; }
        public string Name { get; }

        public UserFunction(string name, List<string> parameters, List<ParsedLine> body)
        {
            Name = name;
            Parameters = parameters ?? new List<string>();
            Body = body;
        }


        public async Task<Co_Object> Invoke(List<Co_Object> args, Calss __instance)
        {
            if (args.Count != Parameters.Count)
                throw new Exception($"Function expected {Parameters.Count} arguments, got {args.Count}");

            var backup = new Dictionary<string, Co_Object>(__instance.variables);


            var eval = new Evaluator();
            try
            {
                eval.variables = backup;
                for (int i = 0; i < Parameters.Count; i++)
                    eval.variables[Parameters[i]] = args[i];
                await Task.Run(() => eval.Evaluate(Body));
                return new Co_Object(null);
            }
            catch (ReturnException ret)
            {
                return ret.ReturnValue;
            }
        }


        public override string ToString()
        {
            var bodyString = string.Join("\n", Body.Select(b => b.ToString()));
            return $"Function Name: {Name}\n" +
                   $"Parameters: {string.Join(", ", Parameters)}\n" +
                   $"Body:\n{bodyString}";
        }
    }

}
