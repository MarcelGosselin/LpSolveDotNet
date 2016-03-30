/*
    LpSolveDotNet is a .NET wrapper allowing usage of the 
    Mixed Integer Linear Programming (MILP) solver lp_solve.

    Copyright (C) 2016 Marcel Gosselin

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
    USA

	https://github.com/MarcelGosselin/LpSolveDotNet/blob/master/LICENSE

 * 
 * This file is a copy of demo.cs from lpsolve project available at
 *      https://sourceforge.net/projects/lpsolve/files/lpsolve/5.5.2.0/lp_solve_5.5.2.0_cs.net.zip/download
 * modified to:
 *      - Be a console app instead of a WinForms app.
 */

using System;
using System.Diagnostics;
using System.Threading;

namespace LpSolveDotNet.Demo
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            System.Diagnostics.Debug.WriteLine(System.Environment.CurrentDirectory);
            lpsolve.Init(".\\NativeBinaries\\win32");

            Test();

            //TestMultiThreads();
        }

        /* unsafe is needed to make sure that these function are not relocated in memory by the CLR. If that would happen, a crash occurs */
        /* go to the project property page and in “configuration properties>build” set Allow Unsafe Code Blocks to True. */
        /* see http://msdn2.microsoft.com/en-US/library/chfa2zb8.aspx and http://msdn2.microsoft.com/en-US/library/t2yzs44b.aspx */
        private /* unsafe */ static void logfunc(int lp, int userhandle, string Buf)
        {
            System.Diagnostics.Debug.Write(Buf);
        }

        private /* unsafe */ static bool ctrlcfunc(int lp, int userhandle)
        {
            /* 'If set to true, then solve is aborted and returncode will indicate this. */
            return (false);
        }

        private /* unsafe */ static void msgfunc(int lp, int userhandle, lpsolve.lpsolve_msgmask message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        private static void ThreadProc(object filename)
        {
            int lp;
            lpsolve.lpsolve_return ret;
            double o;

            lp = lpsolve.read_LP((string)filename, 0, "");
            ret = lpsolve.solve(lp);
            o = lpsolve.get_objective(lp);
            Debug.Assert(ret == lpsolve.lpsolve_return.OPTIMAL && Math.Round(o, 13) == 1779.4810350637485);
            lpsolve.delete_lp(lp);
        }

        private static void TestMultiThreads()
        {
            int release = 0, Major = 0, Minor = 0, build = 0;

            lpsolve.lp_solve_version(ref Major, ref Minor, ref release, ref build);

            for (int i = 1; i <= 5000; i++)
            {
                Thread myThread = new Thread(new ParameterizedThreadStart(ThreadProc));
                myThread.Start("ex4.lp");
            }

            Thread.Sleep(5000);
        }

        private static void Test()
        {
            const string NewLine = "\n";

            int lp;
            int release = 0, Major = 0, Minor = 0, build = 0;
            double[] Row;
            double[] Lower;
            double[] Upper;
            double[] Col;
            double[] Arry;

            lp = lpsolve.make_lp(0, 4);

            lpsolve.lp_solve_version(ref Major, ref Minor, ref release, ref build);

            /* let's first demonstrate the logfunc callback feature */
            lpsolve.put_logfunc(lp, new lpsolve.logfunc(logfunc), 0);
            lpsolve.print_str(lp, "lp_solve " + Major + "." + Minor + "." + release + "." + build + " demo" + NewLine + NewLine);
            lpsolve.solve(lp); /* just to see that a message is send via the logfunc routine ... */
            /* ok, that is enough, no more callback */
            lpsolve.put_logfunc(lp, null, 0);

            /* Now redirect all output to a file */
            lpsolve.set_outputfile(lp, "result.txt");

            /* set an abort function. Again optional */
            lpsolve.put_abortfunc(lp, new lpsolve.ctrlcfunc(ctrlcfunc), 0);

            /* set a message function. Again optional */
            lpsolve.put_msgfunc(lp, new lpsolve.msgfunc(msgfunc), 0, (int)(lpsolve.lpsolve_msgmask.MSG_PRESOLVE | lpsolve.lpsolve_msgmask.MSG_LPFEASIBLE | lpsolve.lpsolve_msgmask.MSG_LPOPTIMAL | lpsolve.lpsolve_msgmask.MSG_MILPEQUAL | lpsolve.lpsolve_msgmask.MSG_MILPFEASIBLE | lpsolve.lpsolve_msgmask.MSG_MILPBETTER));

            lpsolve.print_str(lp, "lp_solve " + Major + "." + Minor + "." + release + "." + build + " demo" + NewLine + NewLine);
            lpsolve.print_str(lp, "This demo will show most of the features of lp_solve " + Major + "." + Minor + "." + release + "." + build + NewLine);

            lpsolve.print_str(lp, NewLine + "We start by creating a new problem with 4 variables and 0 constraints" + NewLine);
            lpsolve.print_str(lp, "We use: lp = lpsolve.make_lp(0, 4);" + NewLine);

            lpsolve.set_timeout(lp, 0);

            lpsolve.print_str(lp, "We can show the current problem with lpsolve.print_lp(lp);" + NewLine);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "Now we add some constraints" + NewLine);
            lpsolve.print_str(lp, "lpsolve.add_constraint(lp, Row, lpsolve.lpsolve_constr_types.LE, 4);" + NewLine);
            // pay attention to the 1 base and ignored 0 column for constraints
            lpsolve.add_constraint(lp, new double[] { 0, 3, 2, 2, 1 }, lpsolve.lpsolve_constr_types.LE, 4);
            lpsolve.print_lp(lp);

            // check ROW array works
            Row = new double[] { 0, 0, 4, 3, 1 };
            lpsolve.print_str(lp, "lpsolve.add_constraint(lp, Row, lpsolve.lpsolve_constr_types.GE, 3);" + NewLine);
            lpsolve.add_constraint(lp, Row, lpsolve.lpsolve_constr_types.GE, 3);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "Set the objective function" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_obj_fn(lp, Row);" + NewLine);
            lpsolve.set_obj_fn(lp, new double[] { 0, 2, 3, -2, 3 });
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "Now solve the problem with lpsolve.solve(lp);" + NewLine);
            lpsolve.print_str(lp, lpsolve.solve(lp) + ": " + lpsolve.get_objective(lp) + NewLine);

            Col = new double[lpsolve.get_Ncolumns(lp)];
            lpsolve.get_variables(lp, Col);

            Row = new double[lpsolve.get_Nrows(lp)];
            lpsolve.get_constraints(lp, Row);

            Arry = new double[lpsolve.get_Ncolumns(lp) + lpsolve.get_Nrows(lp) + 1];
            lpsolve.get_dual_solution(lp, Arry);

            Arry = new double[lpsolve.get_Ncolumns(lp) + lpsolve.get_Nrows(lp)];
            Lower = new double[lpsolve.get_Ncolumns(lp) + lpsolve.get_Nrows(lp)];
            Upper = new double[lpsolve.get_Ncolumns(lp) + lpsolve.get_Nrows(lp)];
            lpsolve.get_sensitivity_rhs(lp, Arry, Lower, Upper);

            Lower = new double[lpsolve.get_Ncolumns(lp) + 1];
            Upper = new double[lpsolve.get_Ncolumns(lp) + 1];
            lpsolve.get_sensitivity_obj(lp, Lower, Upper);

            lpsolve.print_str(lp, "The value is 0, this means we found an optimal solution" + NewLine);
            lpsolve.print_str(lp, "We can display this solution with lpsolve.print_solution(lp);" + NewLine);
            lpsolve.print_objective(lp);
            lpsolve.print_solution(lp, 1);
            lpsolve.print_constraints(lp, 1);

            lpsolve.print_str(lp, "The dual variables of the solution are printed with" + NewLine);
            lpsolve.print_str(lp, "lpsolve.print_duals(lp);" + NewLine);
            lpsolve.print_duals(lp);

            lpsolve.print_str(lp, "We can change a single element in the matrix with" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_mat(lp, 2, 1, 0.5);" + NewLine);
            lpsolve.set_mat(lp, 2, 1, 0.5);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "If we want to maximize the objective function use lpsolve.set_maxim(lp);" + NewLine);
            lpsolve.set_maxim(lp);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "after solving this gives us:" + NewLine);
            lpsolve.solve(lp);
            lpsolve.print_objective(lp);
            lpsolve.print_solution(lp, 1);
            lpsolve.print_constraints(lp, 1);
            lpsolve.print_duals(lp);

            lpsolve.print_str(lp, "Change the value of a rhs element with lpsolve.set_rh(lp, 1, 7.45);" + NewLine);
            lpsolve.set_rh(lp, 1, 7.45);
            lpsolve.print_lp(lp);
            lpsolve.solve(lp);
            lpsolve.print_objective(lp);
            lpsolve.print_solution(lp, 1);
            lpsolve.print_constraints(lp, 1);

            lpsolve.print_str(lp, "We change C4 to the integer type with" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_int(lp, 4, true);" + NewLine);
            lpsolve.set_int(lp, 4, true);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "We set branch & bound debugging on with lpsolve.set_debug(lp, true);" + NewLine);

            lpsolve.set_debug(lp, true);
            lpsolve.print_str(lp, "and solve..." + NewLine);

            lpsolve.solve(lp);
            lpsolve.print_objective(lp);
            lpsolve.print_solution(lp, 1);
            lpsolve.print_constraints(lp, 1);

            lpsolve.print_str(lp, "We can set bounds on the variables with" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_lowbo(lp, 2, 2); & lpsolve.set_upbo(lp, 4, 5.3);" + NewLine);
            lpsolve.set_lowbo(lp, 2, 2);
            lpsolve.set_upbo(lp, 4, 5.3);
            lpsolve.print_lp(lp);

            lpsolve.solve(lp);
            lpsolve.print_objective(lp);
            lpsolve.print_solution(lp, 1);
            lpsolve.print_constraints(lp, 1);

            lpsolve.print_str(lp, "Now remove a constraint with lpsolve.del_constraint(lp, 1);" + NewLine);
            lpsolve.del_constraint(lp, 1);
            lpsolve.print_lp(lp);
            lpsolve.print_str(lp, "Add an equality constraint" + NewLine);
            Row = new double[] { 0, 1, 2, 1, 4 };
            lpsolve.add_constraint(lp, Row, lpsolve.lpsolve_constr_types.EQ, 8);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "A column can be added with:" + NewLine);
            lpsolve.print_str(lp, "lpsolve.add_column(lp, Col);" + NewLine);
            lpsolve.add_column(lp, new double[] { 3, 2, 2 });
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "A column can be removed with:" + NewLine);
            lpsolve.print_str(lp, "lpsolve.del_column(lp, 3);" + NewLine);
            lpsolve.del_column(lp, 3);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "We can use automatic scaling with:" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_scaling(lp, lpsolve.lpsolve_scales.SCALE_MEAN);" + NewLine);
            lpsolve.set_scaling(lp, lpsolve.lpsolve_scales.SCALE_MEAN);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "The function lpsolve.get_mat(lp, row, column); returns a single" + NewLine);
            lpsolve.print_str(lp, "matrix element" + NewLine);
            lpsolve.print_str(lp, "lpsolve.get_mat(lp, 2, 3); lpsolve.get_mat(lp, 1, 1); gives " + lpsolve.get_mat(lp, 2, 3) + ", " + lpsolve.get_mat(lp, 1, 1) + NewLine);
            lpsolve.print_str(lp, "Notice that get_mat returns the value of the original unscaled problem" + NewLine);

            lpsolve.print_str(lp, "If there are any integer type variables, then only the rows are scaled" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_int(lp, 3, false);" + NewLine);
            lpsolve.set_int(lp, 3, false);
            lpsolve.print_lp(lp);

            lpsolve.solve(lp);
            lpsolve.print_str(lp, "print_solution gives the solution to the original problem" + NewLine);
            lpsolve.print_objective(lp);
            lpsolve.print_solution(lp, 1);
            lpsolve.print_constraints(lp, 1);

            lpsolve.print_str(lp, "Scaling is turned off with lpsolve.unscale(lp);" + NewLine);
            lpsolve.unscale(lp);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "Now turn B&B debugging off and simplex tracing on with" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_debug(lp, false); lpsolve.set_trace(lp, true); and lpsolve.solve(lp);" + NewLine);
            lpsolve.set_debug(lp, false);
            lpsolve.set_trace(lp, true);

            lpsolve.solve(lp);
            lpsolve.print_str(lp, "Where possible, lp_solve will start at the last found basis" + NewLine);
            lpsolve.print_str(lp, "We can reset the problem to the initial basis with" + NewLine);
            lpsolve.print_str(lp, "default_basis lp. Now solve it again..." + NewLine);

            lpsolve.default_basis(lp);
            lpsolve.solve(lp);

            lpsolve.print_str(lp, "It is possible to give variables and constraints names" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_row_name(lp, 1, \"speed\"); lpsolve.set_col_name(lp, 2, \"money\");" + NewLine);
            lpsolve.set_row_name(lp, 1, "speed");
            lpsolve.set_col_name(lp, 2, "money");
            lpsolve.print_lp(lp);
            lpsolve.print_str(lp, "As you can see, all column and rows are assigned default names" + NewLine);
            lpsolve.print_str(lp, "If a column or constraint is deleted, the names shift place also:" + NewLine);

            lpsolve.print_str(lp, "lpsolve.del_column(lp, 1);" + NewLine);
            lpsolve.del_column(lp, 1);
            lpsolve.print_lp(lp);

            lpsolve.write_lp(lp, "lp.lp");
            lpsolve.write_mps(lp, "lp.mps");

            lpsolve.set_outputfile(lp, null);

            lpsolve.delete_lp(lp);

            lp = lpsolve.read_LP("lp.lp", 0, "test");
            if (lp == 0)
            {
                Console.Error.WriteLine("Can't find lp.lp, stopping");
                return;
            }

            lpsolve.set_outputfile(lp, "result2.txt");

            lpsolve.print_str(lp, "An lp structure can be created and read from a .lp file" + NewLine);
            lpsolve.print_str(lp, "lp = lpsolve.read_LP(\"lp.lp\", 0, \"test\");" + NewLine);
            lpsolve.print_str(lp, "The verbose option is disabled" + NewLine);

            lpsolve.print_str(lp, "lp is now:" + NewLine);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "solution:" + NewLine);
            lpsolve.set_debug(lp, true);
            lpsolve.lpsolve_return statuscode = lpsolve.solve(lp);
            string status = lpsolve.get_statustext(lp, (int)statuscode);
            Debug.WriteLine(status);

            lpsolve.set_debug(lp, false);
            lpsolve.print_objective(lp);
            lpsolve.print_solution(lp, 1);
            lpsolve.print_constraints(lp, 1);

            lpsolve.write_lp(lp, "lp.lp");
            lpsolve.write_mps(lp, "lp.mps");

            lpsolve.set_outputfile(lp, null);

            lpsolve.delete_lp(lp);
        }   //Test
    }
}
