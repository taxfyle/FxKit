# What is FxKit

FxKit is a library for C# to enable functional programming and railway-oriented programming
using common data types like `Result`, `Option`, and `Validation`.

FxKit also includes source generators for generating union types, exhaustive `Match` functions,
`Func<>`-friendly constructors, and much more.

## Main features

FxKit is broken up into the following packages:

|            Feature             |                      Description                       |
|:------------------------------:|:------------------------------------------------------:|
|         [Core](/core/)         |        The core library. Can be used on its own        |
| [CompilerServices](/compiler/) |         Roslyn analyzers and source generators         |
|      [Testing](/testing/)      | Contains test helpers for asserting on the FxKit types |