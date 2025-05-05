using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cobra.Classes
{
    class Time
    {
        private static readonly Stopwatch stopwatch = Stopwatch.StartNew();

        public static float time => MathF.Round((float)stopwatch.Elapsed.TotalSeconds, 4);
    }
}
