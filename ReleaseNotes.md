# LpSolveDotNet Release Notes

## 3.0.1

To see all commits for this version, [click here](https://github.com/MarcelGosselin/LpSolveDotNet/compare/v3.0.0...v3.0.1).

* Fixes issue #3: Native binaries are not copied when project is built with property OutDir=some_path.

## 3.0.0

To see all commits for this version, [click here](https://github.com/MarcelGosselin/LpSolveDotNet/compare/v2.0.0...v3.0.0).

* Update lpsolve dlls to 5.5.2.3.

### API changes

* `get_PseudoCosts()` and `set_PseudoCosts()`  were removed as they are no longer part of the [lpsolve API](http://lpsolve.sourceforge.net/5.5/).

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