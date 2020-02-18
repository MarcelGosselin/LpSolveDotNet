![CI](https://github.com/MarcelGosselin/LpSolveDotNet/workflows/CI/badge.svg)

# LpSolveDotNet
LpSolveDotNet is a .NET wrapper for the Mixed Integer Linear Programming (MILP) solver lp_solve. The solver lp_solve solves pure linear, (mixed) integer/binary, semi-cont and special ordered sets (SOS) models.

The wrapper works with these .NET implementations:

   * .NET Framework versions 2.0 and up
   * .NET Core 3.0 and up
   * [.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) 1.5 and up **See other platforms usage below**

on these platforms:

| OS      | Architecture | Package to reference | Notes |
| ------- | ------------ | -------------------- | -- |
| Windows | x64          | LpSolveDotNet.Native.win-x64 | |
| Windows | x86 (or 32 bit on x64) | LpSolveDotNet.Native.win-x86 | |
| Linux   | x64          | LpSolveDotNet.Native.linux-x64 | |
| Linux   | x86          | LpSolveDotNet.Native.linux-x86 | |
| OSX     | x86          | LpSolveDotNet.Native.osx-x86 | Not tested |
| Others  | Others       | LpSolveDotNet | **See other platforms usage below** |

## Standard Usage
To use `LpSolveDotNet` with one of the supported platforms listed above, follow these steps:

1. Find the package name for your target platform(s) in the table above and add it (them) to your project. The `LpSolveDotNet.Native.???` packages, which reference the `LpSolveDotNet` package, will not only add the .NET wrapper of the `lp_solve` library, it will also take care of copying the native library to your build output.
1. In your project, add a call to the `LpSolveDotNet.LpSolve.Init()` method. This will ensure that the right native library is called. If you are
1. Use one of the factory methods on `LpSolve` class to create an `LpSolve` instance:
   * make_lp
   * read_LP
   * read_MPS
   * read_XLI
1. Place the returned `LpSolve` instance into a `using` statement so it is properly disposed of when done with it.
1. Use methods on that instance. The methods are like the [official lpsolve documentation](http://lpsolve.sourceforge.net/5.5/index.htm) except the first parameter which is passed implicitly by this instance.

*In a future version, the API will look more like .NET than C. Don't worry the old syntax will stay there.*

## Other Platforms Usage

If you need to target a platform not listed above, you need to do extra steps to use `LpSolveDotNet`.

### Other .NET Targets

If your application resolves to use LpSolveDotNet's .NET Stantards targets (with an app targeting .NET Core earlier than 3.0, Xamarin...), the library will not pick up the native library by itself. The different ways to fix this are (pick one):
1. Using property `LpSolve.CustomLoadNativeLibrary`
   1. Create a method that takes in a file path and enables your .NET implementation to load it
   1. Assign this method to the property `LpSolve.CustomLoadNativeLibrary`
   1. Call method `LpSolve.Init()`
1. Placing the native library in a folder in the paths searched by the .NET implementation. This could also be what your method `LpSolve.CustomLoadNativeLibrary` is doing.

### Other OS / Architectures

If your application runs on an OS / Architecture not listed above, you need to do the following:
1. Build [lp_solve](https://sourceforge.net/projects/lpsolve/) for your OS / Architecture combination.
1. Deploy the built native library along your application
1. (optional) Depending on your .NET version, you may need to look at [previous section](#Other-.NET-Targets) 
1. If you did **not** put the native library in a folder in the paths searched by the .NET implementation, call `LpSolve.Init()` and tweak the arguments to match your case

## Examples
You can see examples in the [Demo project](./src/LpSolveDotNet.Demo/LpSolveDotNet.Demo.csproj) translated from lpsolve's original C# samples.

* [Formulation of an lp model in lpsolve](./src/LpSolveDotNet.Demo/FormulateSample.cs)
* [demo inside lp_solve_5.5.2.3_cs.net.zip](./src/LpSolveDotNet.Demo/OriginalSample.cs)

## Releases
You can see the [release history](./ReleaseNotes.md).
