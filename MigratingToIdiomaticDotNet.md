# Migrating from version 4 to idiomatic .NET

Before porting your code to idiomatic .NET version of LpSolveDotNet, make sure you have migrated to [LpSolveDotNet 4.0](./MigratingFrom3To4.md)
and that you did not migrate to a version of LpSolveDotNet >= 5.0 which will remove the methods with the Obsolete attributes helping you migrate.

## Changes between idiomatic and original versions

1. The idiomatic version of LpSolveDotNet is in a different namespace: `LpSolveDotNet.Idiomatic` and all changes declared below refer to that namespace.
1. Whenever possible, properties were used instead of get/set pairs or get/set/is trios.
1. Enum names, enum value names and method names use PascalCase.
1. Method overloads and/or optional parameters were used to prevent similar method names like `method_name(p1, p2)` and  `method_name_ex(p1, p2, p3)`.
1. Methods building the model with string representations was removed from that namespace:
   * `str_add_constraint`
   * `str_add_column`
   * `str_set_obj_fn`
   * `str_set_rh_vec`


## Changes to do in order

Careful thought was put into doing a smooth transition from one model to the other. Please follow the below steps **in order** and **until the end**. Many `Obsolete` classes, methods, enums and values were created for the transition but at one point the code will **compile** but will **fail to run**. Make sure you go until the end of the steps.

1. In all your code files replace `LpSolveDotNet.` by `LpSolveDotNet.Idiomatic.` to replace fully qualified class names.
1. In all your code files replace `using LpSolveDotNet;` by `using LpSolveDotNet.Idiomatic;`. It is important to do this as the second step otherwise you end up with `using LpSolveDotNet.Idiomatic.Idiomatic;`.
1. Compile and fix all `Obsolete` **errors** that tell you what to use to fix your code. _Leave the **warnings** for next step._
1. Once all compilation **errors** are fixed, fix all `Obsolete` **warnings**. If you have many warnings and they relate to the same enums, you could use _Replace All_ with the following mappings:
   | From | To |
   | ---- | -- |
   | `lpsolve_msgmask` | `MessageMask` |
   | `lpsolve_constr_types` | `ConstraintOperator` |
   | `lpsolve_scale_algorithm` | `ScaleAlgorithm` |
   | `lpsolve_scale_parameters` | `ScaleParameters` |
   | `lpsolve_improves` | `IterativeImprovementLevels` |
   | `lpsolve_pivot_rule` | `PivotRule` |
   | `lpsolve_pivot_modes` | `PivotModes` |
   | `lpsolve_presolve` | `PreSolveLevels` |
   | `lpsolve_anti_degen` | `AntiDegeneracyRules` |
   | `lpsolve_basiscrash` | `BasisCrashMode` |
   | `lpsolve_simplextypes` | `SimplexType` |
   | `lpsolve_BBstrategies` | `BranchAndBoundRule` and/or `BranchAndBoundModes` |
   | `lpsolve_return` | `SolveResult` |
   | `lpsolve_branch` | `BranchMode` |
   | `lpsolve_verbosity` | `Verbosity` |
   | `lpsolve_mps_options` | `MPSOptions` |
   | `lpsolve_print_sol_option` | `PrintSolutionOption` |
   | `lpsolve_epsilon_level` | `ToleranceEpsilonLevel` |

1. Previous step will make new `Obsolete` **errors** appear that will need to be fixed as well.
1. Compile and fix other errors about API changes. There should not be many, among those are:
   1. Log, Abort and Message callbacks have different signatures.