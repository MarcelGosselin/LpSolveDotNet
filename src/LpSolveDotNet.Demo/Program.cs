using System;

namespace LpSolveDotNet.Demo
{
    class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            if (args.Length == 0 || args[0].Equals("original", StringComparison.CurrentCultureIgnoreCase))
            {
                return OriginalSample.Test();
            }
            return args[0].ToLowerInvariant() switch
            {
                "original" => OriginalSample.Test(),
                "formulate" => FormulateSample.Test(),
                _ => -1
            };
        }
    }
}
