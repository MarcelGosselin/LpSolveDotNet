using System;
using System.Diagnostics;

namespace LpSolveDotNet.Demo
{
    /// <summary>
    /// This class demonstrates how to reproduce the example model from http://lpsolve.sourceforge.net/5.5/formulate.htm#CS.NET
    /// using LpSolveDotNet.
    /// </summary>
    internal class FormulateSample
    {
        public static int Test()
        {
            LpSolve.Init();

            return Demo();
        }

        static class MyModelColumnIndices
        {
            internal const int X = 1;
            internal const int Y = 2;
        };

        private static int Demo()
        {
            // We will build the model row by row
            // So we start with creating a model with 0 rows and 2 columns
            int Ncol = 2; // there are two variables in the model 

            using (LpSolve lp = LpSolve.make_lp(0, Ncol))
            {
                if (lp == null)
                {
                    return 1; // couldn't construct a new model...
                }

                //let us name our variables. Not required, but can be useful for debugging
                lp.set_col_name(MyModelColumnIndices.X, "x");
                lp.set_col_name(MyModelColumnIndices.Y, "y");

                // makes building the model faster if it is done rows by row
                lp.set_add_rowmode(true);

                // construct first row (120 x + 210 y <= 15000) and add it to model
                {
                    // both columnIndices and columnValues must have the same length
                    var columnIndices = new int[] { MyModelColumnIndices.X, MyModelColumnIndices.Y };
                    var columnValues = new double[] { 120, 210 };
                    if (lp.add_constraintex(columnIndices.Length, columnValues, columnIndices, lpsolve_constr_types.LE, 15000) == false)
                    {
                        return 3;
                    }
                }
                // construct second row (110 x + 30 y <= 4000) and add it to model
                {
                    var columnIndices = new int[] { MyModelColumnIndices.X, MyModelColumnIndices.Y };
                    var columnValues = new double[] { 110, 30 };
                    if (lp.add_constraintex(columnIndices.Length, columnValues, columnIndices, lpsolve_constr_types.LE, 4000) == false)
                    {
                        return 3;
                    }
                }
                // construct third row (x + y <= 75) and add it to model
                {
                    var columnIndices = new int[] { MyModelColumnIndices.X, MyModelColumnIndices.Y };
                    var columnValues = new double[] { 1, 1 };
                    if (lp.add_constraintex(columnIndices.Length, columnValues, columnIndices, lpsolve_constr_types.LE, 75) == false)
                    {
                        return 3;
                    }
                }

                // rowmode should be turned off again when done building the model
                lp.set_add_rowmode(false);

                // set the objective function (143 x + 60 y)
                {
                    var columnIndices = new int[] { MyModelColumnIndices.X, MyModelColumnIndices.Y };
                    var columnValues = new double[] { 143, 60 };
                    if (lp.set_obj_fnex(columnIndices.Length, columnValues, columnIndices) == false)
                    {
                        return 4;
                    }
                }

                // set the objective direction to maximize
                lp.set_maxim();

                // just out of curiosity, now show the model in lp format on screen
                // this only works if this is a console application. If not, use write_lp and a filename
                lp.write_lp("model.lp");

                // I only want to see important messages on screen while solving
                lp.set_verbose(lpsolve_verbosity.IMPORTANT);

                // Now let lpsolve calculate a solution
                lpsolve_return s = lp.solve();
                if (s != lpsolve_return.OPTIMAL)
                {
                    return 5;
                }

                // a solution is calculated, now lets get some results

                // objective value
                Console.WriteLine("Objective value: " + lp.get_objective());

                // variable values
                var results = new double[Ncol];
                lp.get_variables(results);
                for (int j = 0; j < Ncol; j++)
                {
                    Console.WriteLine(lp.get_col_name(j + 1) + ": " + results[j]);
                }
            }
            return 0;
        } //Demo
    }
}
