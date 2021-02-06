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
                lp.set_col_name(1, "x");
                lp.set_col_name(2, "y");

                //create space large enough for one row
                int[] colno = new int[Ncol];
                double[] row = new double[Ncol];

                // makes building the model faster if it is done rows by row
                lp.set_add_rowmode(true);

                int j = 0;
                //construct first row (120 x + 210 y <= 15000)
                colno[j] = 1; // first column
                row[j++] = 120;

                colno[j] = 2; // second column
                row[j++] = 210;

                // add the row to lpsolve
                if (lp.add_constraintex(j, row, colno, lpsolve_constr_types.LE, 15000) == false)
                {
                    return 3;
                }

                //construct second row (110 x + 30 y <= 4000)
                j = 0;
                colno[j] = 1; // first column
                row[j++] = 110;

                colno[j] = 2; // second column
                row[j++] = 30;

                // add the row to lpsolve
                if (lp.add_constraintex(j, row, colno, lpsolve_constr_types.LE, 4000) == false)
                {
                    return 3;
                }

                //construct third row (x + y <= 75)
                j = 0;
                colno[j] = 1; // first column
                row[j++] = 1;

                colno[j] = 2; // second column
                row[j++] = 1;

                // add the row to lpsolve
                if (lp.add_constraintex(j, row, colno, lpsolve_constr_types.LE, 75) == false)
                {
                    return 3;
                }

                //rowmode should be turned off again when done building the model
                lp.set_add_rowmode(false);

                //set the objective function (143 x + 60 y)
                j = 0;
                colno[j] = 1; // first column
                row[j++] = 143;

                colno[j] = 2; // second column
                row[j++] = 60;

                if (lp.set_obj_fnex(j, row, colno) == false)
                {
                    return 4;
                }

                // set the object direction to maximize
                lp.set_maxim();

                // just out of curioucity, now show the model in lp format on screen
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
                Debug.WriteLine("Objective value: " + lp.get_objective());

                // variable values
                lp.get_variables(row);
                for (j = 0; j < Ncol; j++)
                {
                    Console.WriteLine(lp.get_col_name(j + 1) + ": " + row[j]);
                }
            }
            return 0;
        } //Demo
    }
}
