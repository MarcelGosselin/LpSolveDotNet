# LpSolveDotNet Release Notes

##  2.0.0 ~~1.1.0~~

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