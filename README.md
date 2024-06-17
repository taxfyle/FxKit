# FxKit

A library for C# to enable functional, railway-oriented programming using common abstract data types
like `Result`, `Option` and `Validation`. Also includes Roslyn-based analyzers and source generators
for generating union types, exhaustive `Match`, and much more.

## Documentation

Visit the [official documentation](https://taxfyle.github.io/FxKit/) to get started.

## Installation

FxKit is broken up into the following packages:

* [FxKit](https://nuget.org/packages/FxKit): The core library. Can be used on its own.
* [FxKit.CompilerServices](https://nuget.org/packages/FxKit.CompilerServices): Roslyn analyzers and source generators.
* [FxKit.CompilerServices.Annotations](https://nuget.org/packages/FxKit.CompilerServices.Annotations): Attributes used
  by the compiler services.
* [FxKit.Testing](https://nuget.org/packages/FxKit.Testing): Contains test helpers for asserting on the FxKit types.

It is recommended to add the following `global using` to make it easy to construct the core data types.

```csharp
global using static FxKit.Prelude;
```

This makes functions like `Some(value)` and `Ok(value)` available everywhere.

# License

MIT