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


        public async Task<Co_Object> Invoke(List<Co_Object> args)
        {

            Console.WriteLine($"[DEBUG] Invoking function '{Name}' with arguments: {string.Join(", ", args.Select(a => a.ToString()))}");
            if (args.Count != Parameters.Count)
                throw new Exception($"Function expected {Parameters.Count} arguments, got {args.Count}");

            var backup = new Dictionary<string, Co_Object>(Evaluator.variables);

            for (int i = 0; i < Parameters.Count; i++)
                Evaluator.variables[Parameters[i]] = args[i];



            try
            {
                await Task.Run(() => new Evaluator().Evaluate(Body));
                return new Co_Object(null);
            }
            catch (ReturnException ret)
            {
                return ret.ReturnValue;
            }
            finally
            {
                Evaluator.variables = backup;
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
