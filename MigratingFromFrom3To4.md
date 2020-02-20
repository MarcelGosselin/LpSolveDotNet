# Migrating from version 3 to 4

Below are the steps to follow for a smooth transition from versions 3.x to version 4.0.0 of LpSolveDotNet. For any doubt when migrating, look at [the release notes](./ReleaseNotes.md#400) which contain details about changed APIs.

## 1. NuGet Packages

There is a **major** difference between versions 3 and 4 regarding NuGet packages: **the native libraries have been moved to separate packages**. You will need to add them manually.

Determine which packages you need to reference (you can pick more than one).
| OS      | Architecture | Package to reference | Notes |
| ------- | ------------ | -------------------- | -- |
| Windows | x64          | LpSolveDotNet.Native.win-x64 | |
| Windows | x86 (or 32 bit on x64) | LpSolveDotNet.Native.win-x86 | |
| Linux   | x64          | LpSolveDotNet.Native.linux-x64 | |
| Linux   | x86          | LpSolveDotNet.Native.linux-x86 | |
| OSX     | x86          | LpSolveDotNet.Native.osx-x86 | Not tested |
| Others  | Others       | LpSolveDotNet | **See other platforms usage below** |

All you need to do now is:
1. Update version of LpSolveDotNet
1. Add the native packages you selected in the grid above

Note that if your project uses the new [PackageReference](https://docs.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)-style dependencies for NuGet, you can replace the current
```xml
<PackageReference Include="LpSolveDotNet" Version="3.1.0" />
```
by the native packages since those new packages depend on LpSolveDotNet and the new project system supports transitive dependencies.
```xml
<PackageReference Include="LpSolveDotNet.Native.win-x64" Version="4.0.0" />
```

**Note:**
> To have exact same behavior as versions 3.x, you should add **both** `LpSolveDotNet.Native.win-x64` and `LpSolveDotNet.Native.win-x86`

## 2. Removed method

Method `read_freeMPS` was removed, use `read_MPS` with `lpsolve_mps_options.MPS_FREE` for its `option` argument.

## 3. Enum numerical value changed

Some numerical values for enums were different from latest lp_solve C code so the values in LpSolveDotNet were aligned with the C library. However, if you used any of the enum values below, you have to validate whether:
* Your application thought it was using the right value but did not
* You explicitly used a different value because you knew they were off

In the first case, leave your code as is. However in the second case you need to update your code. If you need to modify your code, look at [the release notes](./ReleaseNotes.md#400) for details. The enums that were modified are:

* `lpsolve_msgmask.MSG_MILPEQUAL`
* `lpsolve_piv_rules.PRICE_AUTOPARTIALCOLS`
* `lpsolve_piv_rules.PRICE_AUTOPARTIALROWS`
* `lpsolve_piv_rules.PRICE_AUTOPARTIAL`
* `lpsolve_piv_rules.PRICE_AUTOMULTIPLE`
* `lpsolve_piv_rules.PRICE_AUTOPARTIALCOLS`

Also note that all these `lpsolve_piv_rules` have changed enum type as well, see below.

## 4. Enums split in two

Some enums were split in two because they convey 2 types of information. Calls to the following methods are broken and need to use the appropriate enums:
* Related to pivoting: `get_pivoting`, `set_pivoting`, `is_piv_rule` and `is_piv_mode`
   * Use new enums: `PivotRuleAndModes`, `lpsolve_pivot_rule` and `lpsolve_pivot_modes`
* Related to scaling: `get_scaling`, `set_scaling`, `is_scaletype` and `is_scalemode`
   * Use new enums: `ScalingAlgorithmAndParameters`, `lpsolve_scale_algorithm` and `lpsolve_scale_parameters`
* Related to MPS loading: `read_MPS`
   * Use new enums: `lpsolve_mps_options` and `lpsolve_verbosity`

## 5. Magic integers changed to enums

Methods which used to receive or return integers now handle enums, use the ones appropriate for your case:
* `lpsolve_verbosity` in `read_LP`, `read_MPS`, `read_XLI`, `get_verbose` and `set_verbose`
* `lpsolve_constr_types` in `is_constr_type`
* `lpsolve_msgmask` in `put_msgfunc`
* `lpsolve_print_sol_option` in `get_print_sol` and `set_print_sol`
