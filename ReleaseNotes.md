# LpSolveDotNet Release Notes

## vNext -- not released yet

### New Features/Improvements
* Add missing methods `set_break_numeric_accuracy`, `get_break_numeric_accuracy` and `get_accuracy`.


## 4.0.0

To see all commits for this version, [click here](https://github.com/MarcelGosselin/LpSolveDotNet/compare/v3.1.0...v4.0.0).

### New Features/Improvements
* Support for .NET Core and more (.NET Standard 1.5 and 2.0).
* Issue #1: Add API documentation for Intellisense.
* **Breaking see below** Add missing enum values to be equal to those in lpsolve's source code.
* **Breaking see below** The native libraries have been extracted to separate NuGet packages.
* Add missing methods `put_bb_nodefunc` and `put_bb_branchfunc`.

### Bug Fixes
* Fix issue #4: Loading native library from wrong location when running with NUnit.
* Fix issue #6: Missing enum value `lpsolve_return.ACCURACYERROR` with value `25`.
* Fix all enum values to be equal to those in lpsolve's source code.

### API changes
* Changed value of `lpsolve_msgmask.MSG_MILPEQUAL` from `32` to `256` which is the value expected from lp_solve.
* Changes to the pivot rules and modes:
  * `lpsolve_piv_rules` enum split into `lpsolve_pivot_rule` and `lpsolve_pivot_modes`
  * The value returned from `get_pivoting` is now of the new struct `PivotRuleAndModes` which split the rule and modes in two.
  * Modified the modes to follow lp_solve's source code:
    * `PRICE_AUTOPARTIALCOLS` and `PRICE_AUTOPARTIALROWS` now replaced by `PRICE_AUTOPARTIAL` and  `PRICE_AUTOMULTIPLE` respectively.
    * **The behaviour of `PRICE_AUTOPARTIAL` has changed**. The previous definition of `PRICE_AUTOPARTIAL` was `PRICE_AUTOPARTIALCOLS | PRICE_AUTOPARTIALROWS` but now only has the first component.  If you used this previously and want to retain same behaviour, you must use `lpsolve_pivot_modes.PRICE_AUTOPARTIAL | lpsolve_pivot_modes.PRICE_AUTOMULTIPLE`.
* Changes to the scaling rules and modes:
  * `lpsolve_scales` enum split into `lpsolve_scale_algorithm` and `lpsolve_scale_parameters`
  * The value returned from `get_scaling` is now of the new struct `ScalingModeAndTypes` which split the algorithm and parameters in two.
  * `set_scaling` now takes both enums as parameters.
  * `is_scalemode` now takes both enums as parameters, with defaults to `lpsolve_scale_algorithm.SCALE_NONE` and `lpsolve_scale_parameters.SCALE_NONE` to allow comparing either enum at a time like before.
* All `verbose` parameters and return values are now of type `lpsolve_verbosity`.
* Method `read_freeMPS` removed, use `read_MPS` and add `lpsolve_mps_options.MPS_FREE` to the methods `option` argument.
* Method `read_MPS`'s `option` parameter is now split into two parameters of types `lpsolve_verbosity` and `lpsolve_mps_options`.
* Method `is_constr_type`'s `mask` parameter is now of type `lpsolve_constr_types`
* Enum value `lpsolve_basiscrash.CRASH_NOTHING` renamed `lpsolve_basiscrash.CRASH_NONE` to follow C code.
* Method `get_Lrows` was removed as lpsolve's Lagrangian solver does not work.
* Methods `get_print_sol` and `set_print_sol` now use a `lpsolve_print_sol_option` instead of an `int`.
* Method `put_msgfunc`'s mask parameter is now a `lpsolve_msgmask` enum.

### Other breaking changes
* The native libraries have been extracted to separate NuGet packages:
   * `LpSolveDotNet.Native.win-x64`
   * `LpSolveDotNet.Native.win-x86`

  You need to reference at least one of them from your project. [See README for details](./README.md).

## 3.1.0

To see all commits for this version, [click here](https://github.com/MarcelGosselin/LpSolveDotNet/compare/v3.0.1...v3.1.0).

* Update lpsolve dlls to [5.5.2.5](https://sourceforge.net/projects/lpsolve/files/lpsolve/5.5.2.5/). Since 5.5.2.3:
    - When using set_lowbo and set_upbo to set bounds on a variable and the new low/up bounds are very close to each other
  but not equal then they are set equal for numerical stability.
    - When all variables in the model are integer, but not all binary (in fact difference between upper and lower bound 1),
      then it could happen that not the most optimal integer solution was found.

## 3.0.1

To see all commits for this version, [click here](https://github.com/MarcelGosselin/LpSolveDotNet/compare/v3.0.0...v3.0.1).

* Fixes issue #3: Native binaries are not copied when project is built with property OutDir=some_path.

## 3.0.0

To see all commits for this version, [click here](https://github.com/MarcelGosselin/LpSolveDotNet/compare/v2.0.0...v3.0.0).

- Update lpsolve dlls to [5.5.2.3](https://sourceforge.net/projects/lpsolve/files/lpsolve/5.5.2.3/). Since 5.5.2.0:
    - fixed a small error in new and improved MIP_stepOF function to find integer solutions.
    - For integer models with semi-cont variables it happened sometimes that a message
      "fillbranches_BB: Inconsistent equal-valued bounds for ..." occured and that the semi-cont condition
      was not respected.
    - New functions added: get_accuracy to get the numeric accuracy after solve.
    - New functions added: set_break_numeric_accuracy, get_break_numeric_accuracy to let lp_solve return ACCURACYERROR
      instead of FEASIBLE when numerical accuracy if worse then the provided values.
      In the past, lp_solve only returned a non-optimal status in case of very severe numerical instability.
      Now it will return already ACCURACYERROR when it finds a relative inaccuracy of 5e-7
    - When reading a model from the lp-format and important issues are detected such as already bounds on variables being overruled
      later with for example a bin keyword, this is now reported in the default verbose level such that this is seen easier.
    - For some models with integer variables, lp_solve did not find the most optimal solution.

### API changes

* `get_PseudoCosts()` and `set_PseudoCosts()`  were removed as they are no longer part of the [lpsolve API](http://lpsolve.sourceforge.net/5.5/).

## 2.0.1 (built after 3.0.0)

* Fixes issue #3: Native binaries are not copied when project is built with property OutDir=some_path.

To see all commits for this version, [click here](https://github.com/MarcelGosselin/LpSolveDotNet/compare/v2.0.0...v2.0.1).

##  2.0.0 ~~1.1.0~~

To see all commits for this version, [click here](https://github.com/MarcelGosselin/LpSolveDotNet/compare/v1.0.0...v2.0.0).

Was originally released under 1.1.0 but then re-released as 2.0.0 to follow [semantic versioning rules](http://semver.org/).

### New Features/Improvements

* LpSolve is now a class with instance methods instead of just a static class. It still has static factory methods to create the LP.
* Removed methods that according to [lpsolve API docs](http://lpsolve.sourceforge.net/5.5/) were either internal or non-functioning.
* Do not build with "unsafe" mode anymore.

### Bug Fixes

* Use right enum types to pass to methods.
* Use IntPtr in a few more places that were forgotten.

## 1.0.0

* Initial revision with lpsolve 5.5.2.0.