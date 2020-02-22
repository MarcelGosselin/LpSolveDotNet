# Migrating from version 4 to idiomatic .NET

Before porting your code to idiomatic .NET version of LpSolveDotNet, make sure you have migrated to [LpSolveDotNet 4.0](./MigratingFrom3To4.md)
and that you did not migrate to a version of LpSolveDotNet >= 5.0 which will remove the methods with the Obsolete attributes helping you migrate.

## Changes to do in order

1. In all your code files replace `LpSolveDotNet.` by `LpSolveDotNet.Idiomatic.` to replace fully qualified class names.
1. In all your code files replace `using LpSolveDotNet;` by `using LpSolveDotNet.Idiomatic;`. It is important to do this as the second step otherwise you end up with `using LpSolveDotNet.Idiomatic.Idiomatic;`.
1. Compile and fix `Obsolete` errors that tell you what to use to fix your code.
1. Compile and fix other errors about API changes. There should not be many, among those are:
   1. Log, Abort and Message callbacks have different signatures.