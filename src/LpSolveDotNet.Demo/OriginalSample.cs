using System;
using System.Diagnostics;

namespace LpSolveDotNet.Demo
{
    /// <summary>
    /// This is almost identical to the demo provided with lp_solve_5.5.2.5_cs.net.zip file from lp_solve's sourceforge web site.
    /// </summary>
    internal class OriginalSample
    {
        /* unsafe is needed to make sure that these function are not relocated in memory by the CLR. If that would happen, a crash occurs */
        /* go to the project property page and in “configuration properties>build” set Allow Unsafe Code Blocks to True. */
        /* see http://msdn2.microsoft.com/en-US/library/chfa2zb8.aspx and http://msdn2.microsoft.com/en-US/library/t2yzs44b.aspx */
        private /* unsafe */ static void logfunc(IntPtr lp, IntPtr userhandle, string Buf)
        {
            Debug.Write(Buf);
        }

        private /* unsafe */ static bool ctrlcfunc(IntPtr lp, IntPtr userhandle)
        {
            // 'If set to true, then solve is aborted and returncode will indicate this.
            return (false);
        }

        private /* unsafe */ static void msgfunc(IntPtr lp, IntPtr userhandle, lpsolve_msgmask message)
        {
            Debug.WriteLine(message);
        }

        private static void ThreadProc(object filename)
        {
            using (var lp = LpSolve.read_LP((string)filename, 0, ""))
            {
                lpsolve_return ret = lp.solve();
                double o = lp.get_objective();
                Debug.Assert(ret == lpsolve_return.OPTIMAL && Math.Round(o, 13) == 1779.4810350637485);
            }
        }

        public static int Test()
        {
            LpSolve.Init();

            return Demo();
        }

        private static int Demo()
        {
            const string NewLine = "\n";

            double[] Row;
            double[] Lower;
            double[] Upper;
            double[] Col;
            double[] Arry;

            using (var lp = LpSolve.make_lp(0, 4))
            {
                Version version = LpSolve.LpSolveVersion;

                /* let's first demonstrate the logfunc callback feature */
                lp.put_logfunc(logfunc, IntPtr.Zero);
                lp.print_str("lp_solve " + version + " demo" + NewLine + NewLine);
                lp.solve(); /* just to see that a message is send via the logfunc routine ... */
                /* ok, that is enough, no more callback */
                lp.put_logfunc(null, IntPtr.Zero);

                /* Now redirect all output to a file */
                lp.set_outputfile("result.txt");

                /* set an abort function. Again optional */
                lp.put_abortfunc(ctrlcfunc, IntPtr.Zero);

                /* set a message function. Again optional */
                lp.put_msgfunc(msgfunc, IntPtr.Zero, lpsolve_msgmask.MSG_PRESOLVE | lpsolve_msgmask.MSG_LPFEASIBLE | lpsolve_msgmask.MSG_LPOPTIMAL | lpsolve_msgmask.MSG_MILPEQUAL | lpsolve_msgmask.MSG_MILPFEASIBLE | lpsolve_msgmask.MSG_MILPBETTER);

                lp.print_str("lp_solve " + version + " demo" + NewLine + NewLine);
                lp.print_str("This demo will show most of the features of lp_solve " + version + NewLine);

                lp.print_str(NewLine + "We start by creating a new problem with 4 variables and 0 constraints" + NewLine);
                lp.print_str("We use: lp = LpSolve.make_lp(0, 4);" + NewLine);

                lp.set_timeout(0);

                lp.print_str("We can show the current problem with lp.print_lp();" + NewLine);
                lp.print_lp();

                lp.print_str("Now we add some constraints" + NewLine);
                lp.print_str("lp.add_constraint(Row, lpsolve_constr_types.LE, 4);" + NewLine);
                // pay attention to the 1 base and ignored 0 column for constraints
                lp.add_constraint(new double[] { 0, 3, 2, 2, 1 }, lpsolve_constr_types.LE, 4);
                lp.print_lp();

                // check ROW array works
                Row = new double[] { 0, 0, 4, 3, 1 };
                lp.print_str("lp.add_constraint(Row, lpsolve_constr_types.GE, 3);" + NewLine);
                lp.add_constraint(Row, lpsolve_constr_types.GE, 3);
                lp.print_lp();

                lp.print_str("Set the objective function" + NewLine);
                lp.print_str("lp.set_obj_fn(Row);" + NewLine);
                lp.set_obj_fn(new double[] { 0, 2, 3, -2, 3 });
                lp.print_lp();

                lp.print_str("Now solve the problem with lp.solve();" + NewLine);
                lp.print_str(lp.solve() + ": " + lp.get_objective() + NewLine);

                Col = new double[lp.get_Ncolumns()];
                lp.get_variables(Col);

                Row = new double[lp.get_Nrows()];
                lp.get_constraints(Row);

                Arry = new double[lp.get_Ncolumns() + lp.get_Nrows() + 1];
                lp.get_dual_solution(Arry);

                Arry = new double[lp.get_Ncolumns() + lp.get_Nrows()];
                Lower = new double[lp.get_Ncolumns() + lp.get_Nrows()];
                Upper = new double[lp.get_Ncolumns() + lp.get_Nrows()];
                lp.get_sensitivity_rhs(Arry, Lower, Upper);

                Lower = new double[lp.get_Ncolumns() + 1];
                Upper = new double[lp.get_Ncolumns() + 1];
                lp.get_sensitivity_obj(Lower, Upper);

                lp.print_str("The value is 0, this means we found an optimal solution" + NewLine);
                lp.print_str("We can display this solution with lp.print_solution();" + NewLine);
                lp.print_objective();
                lp.print_solution(1);
                lp.print_constraints(1);

                lp.print_str("The dual variables of the solution are printed with" + NewLine);
                lp.print_str("lp.print_duals();" + NewLine);
                lp.print_duals();

                lp.print_str("We can change a single element in the matrix with" + NewLine);
                lp.print_str("lp.set_mat(2, 1, 0.5);" + NewLine);
                lp.set_mat(2, 1, 0.5);
                lp.print_lp();

                lp.print_str("If we want to maximize the objective function use lp.set_maxim();" + NewLine);
                lp.set_maxim();
                lp.print_lp();

                lp.print_str("after solving this gives us:" + NewLine);
                lp.solve();
                lp.print_objective();
                lp.print_solution(1);
                lp.print_constraints(1);
                lp.print_duals();

                lp.print_str("Change the value of a rhs element with lp.set_rh(1, 7.45);" + NewLine);
                lp.set_rh(1, 7.45);
                lp.print_lp();
                lp.solve();
                lp.print_objective();
                lp.print_solution(1);
                lp.print_constraints(1);

                lp.print_str("We change C4 to the integer type with" + NewLine);
                lp.print_str("lp.set_int(4, true);" + NewLine);
                lp.set_int(4, true);
                lp.print_lp();

                lp.print_str("We set branch & bound debugging on with lp.set_debug(true);" + NewLine);

                lp.set_debug(true);
                lp.print_str("and solve..." + NewLine);

                lp.solve();
                lp.print_objective();
                lp.print_solution(1);
                lp.print_constraints(1);

                lp.print_str("We can set bounds on the variables with" + NewLine);
                lp.print_str("lp.set_lowbo(2, 2); & lp.set_upbo(4, 5.3);" + NewLine);
                lp.set_lowbo(2, 2);
                lp.set_upbo(4, 5.3);
                lp.print_lp();

                lp.solve();
                lp.print_objective();
                lp.print_solution(1);
                lp.print_constraints(1);

                lp.print_str("Now remove a constraint with lp.del_constraint(1);" + NewLine);
                lp.del_constraint(1);
                lp.print_lp();
                lp.print_str("Add an equality constraint" + NewLine);
                Row = new double[] { 0, 1, 2, 1, 4 };
                lp.add_constraint(Row, lpsolve_constr_types.EQ, 8);
                lp.print_lp();

                lp.print_str("A column can be added with:" + NewLine);
                lp.print_str("lp.add_column(Col);" + NewLine);
                lp.add_column(new double[] { 3, 2, 2 });
                lp.print_lp();

                lp.print_str("A column can be removed with:" + NewLine);
                lp.print_str("lp.del_column(3);" + NewLine);
                lp.del_column(3);
                lp.print_lp();

                lp.print_str("We can use automatic scaling with:" + NewLine);
                lp.print_str("lp.set_scaling(lpsolve_scale_algorithm.SCALE_MEAN, lpsolve_scale_parameters.SCALE_NONE);" + NewLine);
                lp.set_scaling(lpsolve_scale_algorithm.SCALE_MEAN, lpsolve_scale_parameters.SCALE_NONE);
                lp.print_lp();

                lp.print_str("The function lp.get_mat(row, column); returns a single" + NewLine);
                lp.print_str("matrix element" + NewLine);
                lp.print_str("lp.get_mat(2, 3); lp.get_mat(1, 1); gives " + lp.get_mat(2, 3) + ", " + lp.get_mat(1, 1) + NewLine);
                lp.print_str("Notice that get_mat returns the value of the original unscaled problem" + NewLine);

                lp.print_str("If there are any integer type variables, then only the rows are scaled" + NewLine);
                lp.print_str("lp.set_int(3, false);" + NewLine);
                lp.set_int(3, false);
                lp.print_lp();

                lp.solve();
                lp.print_str("print_solution gives the solution to the original problem" + NewLine);
                lp.print_objective();
                lp.print_solution(1);
                lp.print_constraints(1);

                lp.print_str("Scaling is turned off with lp.unscale();" + NewLine);
                lp.unscale();
                lp.print_lp();

                lp.print_str("Now turn B&B debugging off and simplex tracing on with" + NewLine);
                lp.print_str("lp.set_debug(false); lp.set_trace(true); and lp.solve();" + NewLine);
                lp.set_debug(false);
                lp.set_trace(true);

                lp.solve();
                lp.print_str("Where possible, lp_solve will start at the last found basis" + NewLine);
                lp.print_str("We can reset the problem to the initial basis with" + NewLine);
                lp.print_str("default_basis lp. Now solve it again..." + NewLine);

                lp.default_basis();
                lp.solve();

                lp.print_str("It is possible to give variables and constraints names" + NewLine);
                lp.print_str("lp.set_row_name(1, \"speed\"); lp.set_col_name(2, \"money\");" + NewLine);
                lp.set_row_name(1, "speed");
                lp.set_col_name(2, "money");
                lp.print_lp();
                lp.print_str("As you can see, all column and rows are assigned default names" + NewLine);
                lp.print_str("If a column or constraint is deleted, the names shift place also:" + NewLine);

                lp.print_str("lp.del_column(1);" + NewLine);
                lp.del_column(1);
                lp.print_lp();

                lp.write_lp("lp.lp");
                lp.write_mps("lp.mps");

                lp.set_outputfile(null);
            }

            using (var lp = LpSolve.read_LP("lp.lp", 0, "test"))
            {
                if (lp == null)
                {
                    Console.Error.WriteLine("Can't find lp.lp, stopping");
                    return 1;
                }

                lp.set_outputfile("result2.txt");

                lp.print_str("An lp structure can be created and read from a .lp file" + NewLine);
                lp.print_str("lp = LpSolve.read_lp(\"lp.lp\", 0, \"test\");" + NewLine);
                lp.print_str("The verbose option is disabled" + NewLine);

                lp.print_str("lp is now:" + NewLine);
                lp.print_lp();

                lp.print_str("solution:" + NewLine);
                lp.set_debug(true);
                lpsolve_return statuscode = lp.solve();
                string status = lp.get_statustext((int)statuscode);
                Debug.WriteLine(status);

                lp.set_debug(false);
                lp.print_objective();
                lp.print_solution(1);
                lp.print_constraints(1);

                lp.write_lp("lp.lp");
                lp.write_mps("lp.mps");

                lp.set_outputfile(null);
                return 0;
            }
        }   //Test
    }
}
