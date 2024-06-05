# Result

The `Result` type (also commonly known as "Either") can hold either an `Ok` value or an `Err` value.

```csharp
public Result<int, string> Divide(int a, int b) =>
    b == 0
        ? Err("Cannot divide by zero")
        : Ok(a / b);
```

## TryGet

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
            NotANumber: ReadAndParseError.FileDidNotContainNumber.Of),
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

* [`[EnumMatch]`](/annotations/enum-match) is used to generate an exhaustive `Match` method for the enum type.
* [`[Union]`](/annotations/union) declares the type as a union type and marks it `abstract` - each `partial record` defined inside will
  inherit the decorated type. Methods like `Of` and `Match` are generated to enable inference-friendly construction
  and exhaustive matching, respectively.
* `MapErr` maps the error of the result, in case the result is in the error state
* `MapErrT` is like `.Map(x => x.MapErr(y => ...))` - the reason we used it here is because we are working
* with `Task<Result<..>>` rather than `Result` directly.
* `AsTask` is used to turn a `Result` into a `Task<Result<..>>` in order to satisfy the compiler - this is needed for
  the LINQ syntax to work.