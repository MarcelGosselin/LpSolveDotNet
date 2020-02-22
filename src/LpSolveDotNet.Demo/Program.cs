using System;

namespace LpSolveDotNet.Demo
{
    internal class Program
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
                "idiom-original" => Idiomatic.OriginalSample.Test(),
                "idiom-formulate" => Idiomatic.FormulateSample.Test(),
                _ => -1
            };
        }
    }
}
