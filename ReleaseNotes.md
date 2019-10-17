# LpSolveDotNet Release Notes

## vNext -- not released yet

### New Features/Improvements
* Document enum values
* Add missing enum values to be equal to those in lpsolve's source code.

### Bug Fixes
* Fixes issue #6: Missing enum value `lpsolve_return.ACCURACYERROR` with value `25`
* Fix all enum values to be equal to those in lpsolve's source code.

### API changes
* Changed value of `lpsolve_msgmask.MSG_MILPEQUAL` from `32` to `256` which is the value expected from lp_solve.
* Changes to the pivot modes in the `lpsolve_piv_rules` enum to follow lp_solve's source code:
  * `PRICE_AUTOPARTIALCOLS` and `PRICE_AUTOPARTIALROWS` now obsolete and replaced by `PRICE_AUTOPARTIAL` and  `PRICE_AUTOMULTIPLE` respectively.
  * **The behaviour of `PRICE_AUTOPARTIAL` has changed**. The previous definition of `PRICE_AUTOPARTIAL` was `PRICE_AUTOPARTIALCOLS | PRICE_AUTOPARTIALROWS` but now only has the first component.  If you used this previously and want to retain same behaviour, you must use `lpsolve_piv_rules.PRICE_AUTOPARTIAL | lpsolve_piv_rules.PRICE_AUTOMULTIPLE`.

## 3.1.0

* Update lpsolve dlls to [5.5.2.5](http://lp-solve.2324885.n4.nabble.com/lpsolve-version-5-5-2-5-released-td10331.html)

## 3.0.1

* Fixes issue #3: Native binaries are not copied when project is built with property OutDir=some_path.

## 3.0.0

To see all commits for this version, [click here](https://github.com/MarcelGosselin/LpSolveDotNet/compare/v2.0.0...v3.0.0).

* Update lpsolve dlls to [5.5.2.3](http://lp-solve.2324885.n4.nabble.com/lpsolve-5-5-2-3-released-tt10210.html).

### API changes

* `get_PseudoCosts()` and `set_PseudoCosts()`  were removed as they are no longer part of the [lpsolve API](http://lpsolve.sourceforge.net/5.5/).

## 2.0.1 (built after 3.0.0)

* Fixes issue #3: Native binaries are not copied when project is built with property OutDir=some_path.

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