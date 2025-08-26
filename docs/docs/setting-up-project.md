# Setting Up Your Project

## Package Reference

To use LpSolveDotNet, you will first need to reference its NuGet package from your project:
```xml
<PackageReference Include="LpSolveDotNet" Version="4.x.x"/>
```

> [!TIP]
> If you end up using one of the `LpSolveDotNet.Native.*` packages below, because they have a reference to the `LpSolveDotNet` package, the reference above becomes redundant.

## Library Initialization

The second step in setting up your project is to initialize the _LpSolveDotNet_ library.
Because it is a wrapper on top of a native library written in C, it needs to load the native library before we can use LpSolveDotNet which is described in the tabs below.
If you encounter issues with the solutions in the tab for your platform below, you might want to look at the _Other .NET Targets_ tab for a solution instead.

## [Windows x64](#tab/win-x64)

In your project file: 
```xml
<PackageReference Include="LpSolveDotNet.Native.win-x64" Version="4.x.x"/>
```

In your code:
```cs
// Call once per process, before any other usage of LpSolve
LpSolve.Init();
```

## [Windows x86](#tab/win-x86)

In your project file: 
```xml
<PackageReference Include="LpSolveDotNet.Native.win-x86" Version="4.x.x"/>
```

In your code:
```cs
// Call once per process, before any other usage of LpSolve
LpSolve.Init();
```

## [Unix/Linux x64](#tab/linux-x64)

In your project file: 
```xml
<PackageReference Include="LpSolveDotNet.Native.linux-x64" Version="4.x.x"/>
```

In your code:
```cs
// Call once per process, before any other usage of LpSolve
LpSolve.Init();
```

## [Unix/Linux x86](#tab/linux-x86)

In your project file: 
```xml
<PackageReference Include="LpSolveDotNet.Native.linux-x86" Version="4.x.x"/>
```

In your code:
```cs
// Call once per process, before any other usage of LpSolve
LpSolve.Init();
```

## [OSX x86](#tab/osx-x86)

> [!WARNING]
> Starting at version 5.0 of LpSolveDotNet, solution below does not work for OSX x86, you need to follow the procedure for _Other Platforms_ (see next tab).

In your project file: 
```xml
<PackageReference Include="LpSolveDotNet.Native.osx-x86" Version="4.x.x"/>
```

In your code:
```cs
// Call once per process, before any other usage of LpSolve
LpSolve.Init();
```

## [Other Platforms](#tab/other-platforms)

In your project file: 
```xml
<PackageReference Include="LpSolveDotNet" Version="4.x.x"/>
```

If your application runs on an OS / Architecture that is not listed above, you need to do the following:

1. Build [lp_solve](https://github.com/lp-solve/lp_solve) for your OS / Architecture combination.
1. Deploy the built native library alongside your application.
1. Follow the procedure for _Other .NET Targets_ (see next tab).

## [Other .NET Targets](#tab/other-dotnet-targets)

This procedure is a complement to the ones in the other tabs to handle special cases when the _lp_solve_ native library will not load because either:
- There are no binaries available on _lp_solve_ website for that platform and you had to build it yourself.
- You run within a .NET target that is neither .NET Frameworek, .NETCore 3+ nor .NET 5+ so code falls back to netstandard which does not have a method to load a library.
- When building/running your application, your compiler or runtime decided to fallback to netstandard.

In either case your options are **either**:
1. Ensure the native library is in a folder in the paths searched by the .NET implementation for libraries before calling `LpSolveDotNet.LpSolve.Init()`.
1. Tweak the arguments you pass to `LpSolveDotNet.LpSolve.Init()` when you call it to match your case: `nativeLibraryFolderPath`, `nativeLibraryNamePrefix`, `nativeLibraryExtension`.
1. Set a custom loader in `LpSolveDotNet.LpSolve.CustomLoadNativeLibrary = ...` before calling `LpSolveDotNet.LpSolve.Init()`.

***