# Result

The `Result` type, also commonly known as "Either", represents values that can be either an `Ok` value
or an `Err` value. This type is particularly useful for error handling, allowing you to represent either
a successful computation with a value or an error with a message or other error information.

For optional values that do not carry errors, you should use the [`Option`](option) type.

## Overview

In many programming scenarios, operations can either succeed or fail. The `Result` type provides a type-safe
way to handle these outcomes explicitly, avoiding the use of exceptions. Exceptions are not ideal for
reporting failures, as exceptions are not part of a method's signature, and the compiler doesn't enforce
handling them. Additionally, exceptions are known to degrade performance.

The `Result` type supports operations such as:
- [Mapping](#map--select): apply transformations to the value within a `Result`
- [Filtering](#ensure): filters a value based on a predicate
- [Flattening](#flatmap--selectmany): apply a function that returns a `Result` and flattens the result

### Usage

The following example demonstrates how to use the `Result` type to handle division by zero:

```csharp
public Result<int, string> Divide(int a, int b) =>
    b == 0
        ? Err("Cannot divide by zero")
        : Ok(a / b);
```

In this example:
- The function `Divide` takes two integers as input.
- It checks if the divisor `b` is zero.
- If `b` is zero, the function returns `Err` with an error message.
- If `b` is not zero, the function returns `Ok` with the result of the division.

## Accessors and Unwrapping

Functions that are used to access or extract values from their containers.

### TryGet

You can use `TryGet` as an escape hatch to get the value and error out.

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

### Unwrap

The `Unwrap` / `UnwrapOrThrow` methods get the `Ok` value, or throw an exception otherwise.

```csharp
Result<int, string> result = Divide(10, 2);
int resultValue = result.Unwrap("Optional error message");
```

Or you can use `UnwrapOr` / `UnwrapOrElse` and specify a fallback value for when the result is in the error state.

```csharp
Result<int, string> result = Divide(10, 2);
int resultValue = result.UnwrapOr(0);
```

### UnwrapErr

The `UnwrapErr` method gets the `Err` value, or throws an exception otherwise.

```csharp
Result<int, string> result = Divide(10, 0);
string error = result.UnwrapErr("Optional error message");
```

### UnwrapEither

The `UnwrapEither` method returns either the Ok value or the Err value when both are of the same type.

```csharp
Result<string, string> result = ParseName("John Locke");
string nameValue = result.UnwrapEither();
```

## Pattern Matching and Transformation

Functions that are used to apply transformations or perform pattern matching on values within containers.

### Match

Use `Match` to handle the possible states of the result:

```csharp
string message = ParseInt("15")
    .Match(
        Ok: x => $"The value is {x}",
        Err: x => $"Error parsing value: {x}");
```

### ToOption

You can turn `Result`s into other types, such as `Option`:

```csharp
Option<int> number = ParseInt("40").ToOption();
```

### ToValidation

To convert an `Result` to `Validation`:

```csharp
Validation<int, string> number = ParseInt("40").ToValidation();
```

## Filtering and Conditional Operators

Functions that are used to filter or conditionally manipulate values within containers.

### Ensure

Filter the value of a result, if any, based on a predicate.

```csharp
Result<int, string> result = ParseInt("10")
    .Ensure(x => x >= 0, "Value is less than zero");
```

> This is essentially a shorthand for `FlatMap(x => x >= 0 ? Ok(x) : Err("Value is less than zero"))`

## Mapping and Flat Mapping

Functions that are used to apply transformations to each element within a container and manage nested containers.

### Map / Select

Transforms the value:

```csharp
Result<int, string> result = Divide(10, 2);
Result<string, string> mappedResult = result.Map(x => x.ToString());
```

### MapErr

Transforms the error value:

```csharp
Result<int, string> result = Divide(10, 0);
Result<int, string> mappedResult = result.Map(x => x.ToUpper());
```

### FlatMap / SelectMany

Monadic bind (also called flat mapping):

```csharp
Result<int, string> result = ParseInt("10")
    .FlatMap(a => Divide(a, 2));
```

That wasn't very pretty - you can use LINQ to make it nicer:

```csharp
Result<int, string> result =
    from a in ParseInt("10")
    from b in Divide(a, 2)
    select b;
```

### FlatMapErr

For binding the error:

```csharp
Result<int, string> result = ParseInt("Invalid")
    .FlatMapErr(a => ParseInt("10"));
```

### Do

Use `Do` to execute an imperative operation when the result has a value.

```csharp
ParseInt("10")
    .Do(x => Console.WriteLine(x));
```

### DoErr

Use `Do` to execute an imperative operation when the result is in the error state.

```csharp
ParseInt("Invalid")
    .DoErr(x => Console.WriteLine(x));
```

## Aggregation and Collection Operations

Functions that are used to aggregate or collect values from multiple containers.

### Traverse

You can _traverse_ between various other container types. For example:

```csharp
IReadOnlyList<string> list = ["7", "Hello", "12", "9"];

Result<IReadOnlyList<int>, string> listOfNumbers =
    list.Traverse(x => ParseInt(x));
// Ok([7, 12, 9])
```

### Sequence

Use `Sequence` to traverse without the mapping step.

> This is equivalent to `Traverse(Identity)` / `Traverse(x => x)`

```csharp
IReadOnlyList<Result<int, string>> list = [ParseInt("7"), ParseInt("Hello"), ParseInt("6")];

Result<IReadOnlyList<int>, string> sequenced =
    list.Sequence();
```

### TryAggregate

Use `TryAggregate` to attempt an aggregation of a sequence with a custom function that can short-circuit if any step
fails, returning either the final accumulated value or an error.

```csharp
IReadOnlyList<int> numbers = [1, 2, 3, 4];
Result<int, string> sumResult = numbers.TryAggregate(
    seed: 0,
    func: (acc, item) => item > 0
        ? Ok<int, string>(acc + item)
        : Err<int, string>("Negative number encountered")
);
```

## Prelude

The `Prelude` class provides the following functions for `Result`:

### Ok / Err

Returns a wrapped value or error `Result`.

```csharp
public Result<int, ParseError> ParseInt(string value) =>
    string.IsNullOrWhiteSpace(value) ? Err<int, ParseError>(ParseError.Empty) :
    int.TryParse(value, out int number) ? Ok<int, ParseError>(number) :
    Err<int, ParseError>(ParseError.NotANumber);
```

## Example

`Result`s are very useful for error handling without using exceptions.

For example, say we've got a function that returns an async result like so:

```csharp
public enum FileError
{
    NoSuchFile,
    PermissionDenied
}

// Implementation omitted
public Task<Result<string, FileError>> ReadFileAsStringAsync(string path);
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
public partial record ReadAndParseError
{
    // For the file error, we want to pass it along.
    partial record ReadingFileFailed(FileError Error);
    // We'll clarify the parse errors at this layer instead.
    partial record FileDidNotContainNumber;
    partial record NumberOverflow;
}

public Task<Result<int, ReadAndParseError>> ReadAndParseAsync(string path) =>
    // Read the file contents
    from contents in ReadFileAsStringAsync(path)
        .MapErrT(ReadAndParseError.ReadingFileFailed.Of) // forward the error by wrapping it in our error type

    // Parse the number
    from parsed in ParseInt(contents)
        // Map the inner error to the shape we want.
        .MapErr(e => e.Match(
            NotANumber: ReadAndParseError.FileDidNotContainNumber.Of,
            Overflow: ReadAndParseError.NumberOverflow.Of))
        .AsTask() // The `AsTask` is needed to align the types

    // Return the value
    select parsed;
```

First, we define our functions - one of them happens to be async (returns `Task`).
We also define our error types for the file reading and the number parsing. Then, we define
a new error type that combines the two. This is a union type, which is a type that can be one of
several types.

In `ReadAndParseAsync`, we start by reading the file. If that fails, we wrap the error in our new error type.
If it succeeds, we parse the number. If that fails, we map the error to our new error type. Finally, we return the
value.

You may have noticed some interesting bits and pieces such as the `[EnumMatch]`, `[Union]`, `MapErr` and `MapErrT`,
and `AsTask`.

* [`[EnumMatch]`](/compiler/enum-match) is used to generate an exhaustive `Match` method for the enum type.
* [`[Union]`](/compiler/union) declares the type as a union type and marks it `abstract` - each `partial record` defined inside will
  inherit the decorated type. Methods like `Of` and `Match` are generated to enable inference-friendly construction
  and exhaustive matching, respectively.
* [`MapErr`](#maperr) maps the error of the result, in case the result is in the error state
* `MapErrT` is like `.Map(x => x.MapErr(y => ...))` - the reason we used it here is because we are working
  with `Task<Result<..>>` rather than `Result` directly.
* `AsTask` is used to turn a `Result` into a `Task<Result<..>>` in order to satisfy the compiler - this is needed for
  the LINQ syntax to work.