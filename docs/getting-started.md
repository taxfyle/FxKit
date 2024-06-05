# Getting Started

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

## Using

### The `Unit` type

`Unit` is useful when you need to return a value from a method that doesn't have a meaningful value to return.
It's similar to `void`, but can be used as a value.

```csharp
public Unit DoSomething()
{
    // Do something
    return Unit.Value;

    // Alternatively:
    // return default;
}
```

---

### The `Option` type

The `Option` type is a replacement for nullable types. It can be used to represent a value that may or may not be
present.

```csharp
public Option<string> OnlyNonWhitespace(string? value) =>
    string.IsNullOrWhiteSpace(value)
        ? None
        : Some(value);
```

### The `Result` type

The `Result` type (also commonly known as "Either") can hold either an `Ok` value or an `Err` value.

```csharp
public Result<int, string> Divide(int a, int b) =>
    b == 0
        ? Err("Cannot divide by zero")
        : Ok(a / b);
```