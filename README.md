# LpSolveDotNet

![CI](https://github.com/MarcelGosselin/LpSolveDotNet/workflows/CI/badge.svg)

[LpSolveDotNet](https://github.com/MarcelGosselin/LpSolveDotNet) is a .NET wrapper for the Mixed Integer Linear Programming (MILP) solver [lp_solve](https://github.com/lp-solve/lp_solve) which is itself written in C.

LpSolveDotNet works on a wide variety of .NET implementations and platforms:

| Supported | |
| -- | -- |
| .NET | Core 3.0, Core 3.1, 5, 6, 7, 8, 9, 10, _future_ |
| .NET Framework | 2.0, 3.0, 3.5, 4.0, 4.5, 4.5.1, 4.5.2, 4.6, 4.6.1, 4.6.2, 4.7, 4.7.1, 4.7.2, 4.8, 4.8.1, _future_ |
| .NET Standard | 1.5, 1.6, 2.0, 2.1 |
| OS / Architecture | Windows x86 and x64, Unix x86 and x64, OSX x86 and others (for others you need to build lp_solve yourself) |

## How to Use - Short Version

Here is the short version, see [long version below](#how-to-use---long-version) for more details.

In your project file, add platform-specific package for your platform to your project: 
```xml
<PackageReference Include="LpSolveDotNet.Native.win-x64" Version="4.x.x"/>
```

In your code (error-handling ommitted for brevity):
```cs
// Call once per process, before any other usage of LpSolve
LpSolve.Init();

// We build a model with 3 constraints and 2 variables
const int Ncol = 2;
using var lp = LpSolve.make_lp(3, Ncol);

// NOTE: set_obj_fnex/add_constraintex should be preferred on set_obj_fn/add_constraint
//       as they can specify only non-zero elements when working with big model.
//       The methods without _ex_ suffix will ignore the first array element so
//       let's use a constant for this for clarity.
const double Ignored = 0;

// set the objective function: maximize (143 x + 60 y)
lp.set_maxim();
lp.set_obj_fn(new double[] { Ignored, 143, 60 });

// add constraints to the model
//   120 x + 210 y <= 15000
//   110 x +  30 y <= 4000
//       x +     y <= 75
lp.set_add_rowmode(true);
lp.add_constraint( new double[] { Ignored, 120, 210 }, lpsolve_constr_types.LE, 15000);
lp.add_constraint( new double[] { Ignored, 110, 30 }, lpsolve_constr_types.LE, 4000);
lp.add_constraint( new double[] { Ignored, 1, 1 }, lpsolve_constr_types.LE, 75);
lp.set_add_rowmode(false);

// We only want to see important messages on screen while solving
lp.set_verbose(lpsolve_verbosity.IMPORTANT);

// Now let lp_solve calculate a solution
lpsolve_return s = lp.solve();
if (s == lpsolve_return.OPTIMAL)
{
   Console.WriteLine("Objective value: " + lp.get_objective());

   var results = new double[Ncol];
   lp.get_variables(results);
   for (int j = 0; j < Ncol; j++)
   {
      Console.WriteLine(lp.get_col_name(j + 1) + ": " + results[j]);
   }
}
```

## How to Use - Long Version

### Library Initialization

Before _LpSolveDotNet_ can be used, it needs to be initialized properly so it loads the native library of _lp_solve_ for your platform.

#### Standard Usage

Find the package name for your target platform(s) in the table below and add it (them) to your project. The _LpSolveDotNet.Native.???_ packages, which reference the _LpSolveDotNet_ package, will not only add the .NET wrapper of the _lp_solve_ library, it will also take care of copying the native library to your build output.

| OS      | Architecture | Package to reference | Notes |
| ------- | ------------ | -------------------- | -- |
| Windows | x64          | LpSolveDotNet.Native.win-x64 | |
| Windows | x86 (or 32 bit on x64) | LpSolveDotNet.Native.win-x86 | |
| Linux   | x64          | LpSolveDotNet.Native.linux-x64 | |
| Linux   | x86          | LpSolveDotNet.Native.linux-x86 | |
| OSX     | x86          | LpSolveDotNet.Native.osx-x86 | |
| Others  | Others       | LpSolveDotNet | See [_Other OS / Architectures_ below](#other-os--architectures) |

#### Other OS / Architectures

If your application runs on an OS / Architecture that is not listed above, you need to do the following:

1. Build [lp_solve](https://github.com/lp-solve/lp_solve) for your OS / Architecture combination.
1. Deploy the built native library alongside your application
1. If your application does not load lp_solve properly, you may need to look at [_Other .NET Targets_ section](#other-net-targets) for further instructions.
1. If you did **not** put the native library in a folder in the paths searched by the .NET implementation, tweak the arguments you pass to `LpSolveDotNet.LpSolve.Init()` when you call it to match your case.

#### Other .NET Targets

If building your application resolves to use LpSolveDotNet's .NET Stantards targets (with an app targeting .NET Core earlier than 3.0, Xamarin...), the library will not pick up the native library by itself. The different ways to fix this are (pick one):

1. Using property `LpSolveDotNet.LpSolve.CustomLoadNativeLibrary`
   1. Create a method that takes in a file path and enables your .NET implementation to load it
   1. Assign this method to the property `LpSolveDotNet.LpSolve.CustomLoadNativeLibrary`
1. Placing the native library in a folder in the paths searched by the .NET implementation. This could also be what your method `LpSolveDotNet.LpSolve.CustomLoadNativeLibrary` is doing.
1. Tweak the arguments you pass to `LpSolveDotNet.LpSolve.Init()` when you call it to match your case.

#### Complete the initialization

To complete the initialization from previous steps, your code **must** call **once** the `LpSolveDotNet.LpSolve.Init()` method. This will ensure that the right native library is loaded and called.

### Using the library

1. Use one of the factory methods on `LpSolve` class to create an `LpSolve` instance:
   * make_lp
   * read_LP
   * read_MPS
   * read_XLI
1. Place the returned `LpSolve` instance into a `using` statement so it is properly disposed of when done with it.
1. Use methods on that instance like the example in [How to Use - Short Version](#how-to-use---short-version) or those in the [Demo project](https://github.com/MarcelGosselin/LpSolveDotNet/tree/master/src/LpSolveDotNet.Demo).

## More documentation

- [Online documentation (WIP)](https://marcelgosselin.github.io/LpSolveDotNet/) The online documentation is begin worked on and should appear eventually.
- [official lp_solve documentation](https://lp-solve.github.io/), the LpSolveDotNet's LpSolve object has the same methods as [lp_solve API](https://lp-solve.github.io/lp_solveAPIreference.htm) except the first parameter `lprec *lp` which is passed implicitly instead.

*In a future version, the API will be more idiomatic to .NET instead of looking like C.*
*Don't worry, we'll keep backwards compatibility so your existing code will continue to compile as-is.*

## Examples

You can see examples in the [Demo project](https://github.com/MarcelGosselin/LpSolveDotNet/tree/master/src/LpSolveDotNet.Demo) translated from lpsolve's original C# samples.

* [Formulation of an lp model in lpsolve](https://github.com/MarcelGosselin/LpSolveDotNet/tree/master/src/LpSolveDotNet.Demo/FormulateSample.cs)
* [demo inside lp_solve_5.5.2.3_cs.net.zip](https://github.com/MarcelGosselin/LpSolveDotNet/tree/master/src/LpSolveDotNet.Demo/OriginalSample.cs)

## Feedback

LpSolve is released as open source under the LGPL-2.1 license. Bug reports and contributions are welcome at the [GitHub repository](https://github.com/MarcelGosselin/LpSolveDotNet).
