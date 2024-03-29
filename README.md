# FxKit

A library for C# to enable functional programming using common data types like `Result`, `Option` and `Validation`.
Also includes source generators for generating union types, and much more.

> Documentation is a work in progress.

## Installation

FxKit is broken up into the following packages:

* [FxKit](https://nuget.org/packages/FxKit): The core library. Can be used on its own.
* [FxKit.CompilerServices](https://nuget.org/packages/FxKit.CompilerServices): Roslyn analyzers and source generators.
* [FxKit.CompilerServices.Annotations](https://nuget.org/packages/FxKit.CompilerServices.Annotations): Annotations`: Attributes used by the compiler services.
* [FxKit.Testing](https://nuget.org/packages/FxKit.Testing): Contains test helpers for asserting on the FxKit types.

It is recommended to add the following `global using` to make it easy to construct the core data types.

```csharp
global using static FxKit.Prelude;
```

This makes functions like `Some(value)` and `Ok(value)` available everywhere. 

## Core data types

FxKit exposes C#-friendly implementations of common functional programming data types:

* `Unit`: A replacement for `void` that can be used as a value.
* `Option`: Alternative to nullable types.
* `Result`: Contains either a success value or an error value. The API is similar to that of Rust's `Result` type.
* `Validation`: Like `Result` but can hold multiple errors.

All of them contain escape hatches for when you need to do imperative work. Additionally, they do not allow 
nullable reference types (that is, types annotated with `?`, e.g. `int?`, `string?`) in any of their values.

`Option`, `Result`, and `Validation` all contain:
* `Map` / `Select`: transform the inner value
* `FlatMap` / `SelectMany`: also called monadic bind.
* `Match`: exhaustive pattern match on the type's constituents.
* `Unwrap`: escape hatch which may throw. There are variations.
* `TryGet`: escape hatch using `if` control flow.

Extension methods have also been provided for the built-in `Task` to make interop seamless. 

> The following usage documentation is not an exhaustive listing of the available APIs.

### `Unit`

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

### `Option`

The `Option` type is a replacement for nullable types. It can be used to represent a value that may or may not be present.

```csharp
public Option<string> OnlyNonWhitespace(string? value) =>
    string.IsNullOrWhiteSpace(value)
        ? None
        : Some(value);
```

The `Option` escape hatch is `TryGet`.

```csharp
Option<string> nonWhitespace = OnlyNonWhitespace("hello");

if (nonWhitespace.TryGet(out var value)) 
{
    Console.WriteLine(value);
}

// Alternatively, `Unwrap` can be used, but will throw an exception if in the `None` state.
string value = nonWhitespace.Unwrap();
```

Transforming the value:

```csharp
OnlyNonWhitespace("  hello  ")
    .Map(x => x.Trim());
```

Matching:

```csharp
int length = OnlyNonWhitespace("  hello  ")
    .Map(x => x.Trim())
    .Match(
        Some: x => x.Length,
        None: () => 0);
```

There are shortcuts for a lot of these - for example, the above could be written as:

```csharp
int length = OnlyNonWhitespace("  hello  ")
    .Map(x => x.Trim())
    .Map(x => x.Length)
    .UnwrapOr(0);
```

Filtering:

```csharp
Option<int> nonZeroLength = OnlyNonWhitespace("  hello  ")
    .Map(x => x.Trim())
    .Map(x => x.Length)
    .Where(x => x.Length > 0);
```

Monadic bind (also called flat mapping):

```csharp
Option<string> greeting = OnlyNonWhitespace("  hello  ")
    .Map(x => x.Trim())
    .FlatMap(a => 
        OnlyNonWhitespace("   world   ")
            .Map(y => y.Trim())
            .Map(b => $"{a} {b}");
```

That wasn't very pretty - you can use LINQ to make it nicer:

```csharp
Option<string> greeting = 
    from a in OnlyNonWhitespace("  hello  ").Map(x => x.Trim())
    from b in OnlyNonWhitespace("   world   ").Map(y => y.Trim())
    select $"{a} {b}";
```

You can turn `Option`s into other types, such as `Result`:

```csharp
Result<int, string> result = OnlyNonWhitespace("  hello  ")
    .Map(x => x.Trim())
    .Map(x => x.Length)
    .Where(x => x.Length > 0)
    .OkOr("That string was only whitespace! Bad!")
```

You can _traverse_ between various other container types. For example:

```csharp
IReadOnlyList<int> list = [2, 4, 6];

Option<IReadOnlyList<int>> listOfOnlyEvenNumbers = 
    list.Traverse(x => x % 2 == 0 ? Some(x) : None);
// Some([2, 4, 6])
```

### `Result`

The `Result` type is used to represent a value that may or may not be present, but also may contain an error.

```csharp
public Result<int, string> Divide(int a, int b) =>
    b == 0
        ? Err("Cannot divide by zero")
        : Ok(a / b);
```

The `Result` escape hatch is `TryGet`.

```csharp
Result<int, string> result = Divide(10, 2);
if (result.TryGet(out var value, out var error)) 
{
    // `value` will be non-null, `error` will be null.
    Console.WriteLine(value);
}
else 
{
    // `value` will be null, `error` will be non-null.
    Console.WriteLine(error);
}
```

`Result`s are very useful for error handling without using exceptions.

For example, say we've got a function that returns an async result like so:

```csharp
public enum FileError
{
    NoSuchFile,
    PermissionDenied
}

// Implementation omitted
public Task<Result<string, NameProblem>> ReadFileAsStringAsync(string path);
```

And we have another method for parsing a string as a number like this:

```csharp
[EnumMatch] // FxKit magic sauce 👀 See the section on source generation
public enum ParseError
{
    NotANumber,
    Overflow
}

// Implementation omitted
public Result<int, ParseError> ParseInt(string value);
```

Now we want to use them together:

```csharp
[Union] // FxKit magic sauce 👀 See the section on source generation
public partial ReadAndParseError
{
    // For the file error, we want to pass it along.
    partial record ReadingFileFailed(FileError Error);
    // We'll clarify the parse errors at this layer instead.
    partial record FileDidNotContainNumber;
    partial record NumberOverflow;
}

public async Task<Result<int, ReadAndParseError>> ReadAndParseAsync(string path) =>
    // Read the file contents
    from contents in ReadFileAsStringAsync(path)
        .MapErrT(ReadAndParseError.ReadingFileFailed.Of) // forward the error by wrapping it in our error type
    
    // Parse the number
    from parsed in ParseInt(contents)
        // Map the inner error to the shape we want. 
        .MapErr(e => e.Match(
            NotANumber: ReadAndParseError.FileDidNotContainNumber.Of),
            Overflow: ReadAndParseError.NumberOverflow.Of))
        .AsTask() // The `AsTask` is needed to align the types
    
    // Return the value
    select parsed;
```

That's a lot to unpack.

First, we define our functions - one of them happens to be async (returns `Task`). 
We also define our error types for the file reading and the number parsing.  Then, we define 
a new error type that combines the two. This is a union type, which is a type that can be one of 
several types.

In `ReadAndParseAsync`, we start by reading the file. If that fails, we wrap the error in our new error type.
If it succeeds, we parse the number. If that fails, we map the error to our new error type. Finally, we return the value.

You may have noticed some interesting bits and pieces such as the `[EnumMatch]`, `[Union]`, `MapErr` and `MapErrT`, and `AsTask`.

* `[EnumMatch]` is used to generate an exhaustive `Match` method for the enum type.
* `[Union]` declares the type as a union type and marks it `abstract` - each `partial record` defined inside will inherit the
   decorated type. Methods like `Of` and `Match` are generated to enable inference-friendly construction and exhaustive 
* matching, respectively.
* `MapErr` maps the error of the result, in case the result is in the error state
* `MapErrT` is like `.Map(x => x.MapErr(y => ...))` - the reason we used it here is because we are working 
* with `Task<Result<..>>` rather than `Result` directly`.
* `AsTask` is used to turn a `Result` into a `Task<Result<..>>` in order to satisfy the compiler - this is needed for
  the LINQ syntax to work.

## Source generation

TBD.

## Transformer methods

TBD.
