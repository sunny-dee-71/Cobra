using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cobra.Classes
{
    class Importer
    {
        public static string LoadLibrary(string name)
        {
            string resourceName = name + ".co";
            string fullPrefix = "cobra.DefaultLibraries.";

            var assembly = typeof(Evaluator).Assembly;
            string[] resources = assembly.GetManifestResourceNames();

            string fullResourceName = resources.FirstOrDefault(r =>
                r.Equals(fullPrefix + resourceName, StringComparison.OrdinalIgnoreCase));

            if (fullResourceName != null)
            {
                using var stream = assembly.GetManifestResourceStream(fullResourceName);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }

            // Find closest match
            string closest = resources
                .Where(r => r.StartsWith(fullPrefix) && r.EndsWith(".co"))
                .Select(r => r.Substring(fullPrefix.Length, r.Length - fullPrefix.Length - 3)) 
                .OrderBy(r => Levenshtein(r.ToLower(), name.ToLower()))
                .FirstOrDefault();

            if (closest != null)
            {
                string suggestedResourceName = fullPrefix + closest + ".co";
                Console.WriteLine($"[WARN] Resource '{name}' does not exist. Did you mean '{closest}'?");

                using var stream = assembly.GetManifestResourceStream(suggestedResourceName);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }

            throw new FileNotFoundException($"Embedded resource '{name}' not found, and no similar resources exist.");
        }


        public static int Levenshtein(string a, string b)
        {
            int[,] d = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++)
                d[i, 0] = i;

            for (int j = 0; j <= b.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                    d[i, j] = new[] {
                    d[i - 1, j] + 1,       
                    d[i, j - 1] + 1,  
                    d[i - 1, j - 1] + cost 
                }.Min();
                }
            }

            return d[a.Length, b.Length];
        }
    }
}
