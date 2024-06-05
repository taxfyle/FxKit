# FxKit

A library for C# to enable functional programming using common data types like `Result`, `Option` and `Validation`.
Also includes source generators for generating union types, and much more.

> The documentation is available at https://taxfyle.github.io/FxKit/.

* [Installation](#installation)
* [Core data types](#core-data-types)
    + [The `Unit` type](#the-unit-type)
    + [The `Option` type](#the-option-type)
    + [The `Result` type](#the-result-type)
    + [The `Validation` type](#the-validation-type)
* [Source generation](#source-generation)
    + [The `[EnumMatch]` generator](#the-enummatch-generator)
    + [The `[Lambda]` generator](#the-lambda-generator)
    + [The `[Union]` generator](#the-union-generator)
* [Transformer methods](#transformer-methods)
    + [Transformers for `Task`](#transformers-for-task)

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

## Core data types

FxKit exposes C#-friendly implementations of common functional programming data types:

* `Unit`: A replacement for `void` that can be used as a value.
* `Option`: Alternative to nullable types.
* `Result`: Contains either a success value or an error value. The API is similar to that of Rust's `Result` type.
* `Validation`: Like `Result` but can hold multiple errors.

All of them contain escape hatches for when you need to do imperative work. Additionally, they do not allow
nullable reference types (that is, types annotated with `?`, e.g. `int?`, `string?`) in any of their values.

`Option`, `Result`, and `Validation` all contain:

* `Map` / `Select`: transform the inner value.
* `FlatMap` / `SelectMany`: also called monadic bind.
* `Match`: exhaustive pattern match on the type's constituents.
* `Unwrap`: escape hatch which may throw. There are variations.
* `TryGet`: escape hatch using `if` control flow.
* Implicit conversions: each type has implicit conversions where possible (`Option<int> o = 1;` would be the same
  as `Some(1)`).

Extension methods have also been provided for the built-in `Task` to make interop seamless.

> The following usage documentation is not an exhaustive listing of the available APIs.

---

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

You can use `TryGet` as an escape hatch to get the value out.

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

---

### The `Result` type

The `Result` type (also commonly known as "Either") can hold either an `Ok` value or an `Err` value.

```csharp
public Result<int, string> Divide(int a, int b) =>
    b == 0
        ? Err("Cannot divide by zero")
        : Ok(a / b);
```

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

* `[EnumMatch]` is used to generate an exhaustive `Match` method for the enum type.
* `[Union]` declares the type as a union type and marks it `abstract` - each `partial record` defined inside will
  inherit the
  decorated type. Methods like `Of` and `Match` are generated to enable inference-friendly construction and exhaustive
  matching, respectively.
* `MapErr` maps the error of the result, in case the result is in the error state
* `MapErrT` is like `.Map(x => x.MapErr(y => ...))` - the reason we used it here is because we are working
* with `Task<Result<..>>` rather than `Result` directly.
* `AsTask` is used to turn a `Result` into a `Task<Result<..>>` in order to satisfy the compiler - this is needed for
  the LINQ syntax to work.

---

### The `Validation` type

The `Validation` type is like `Result`, but can hold multiple errors.

```csharp
// `ValueOf` is a simple value wrapper type that overrides equality checks,  hashcode, and provides
// implicit conversions to the underlying type.
public sealed class Age : ValueOf<int>
{
    private Age(int value)
        : base(value)
    {
    }

    public static Validation<Age, string> Parse(int age)
    {
        var errors = new List<string>();
        if (age < 18)
        {
            errors.Add("You must be at least 18 years of age");
        }

        if (age % 2 != 0)
        {
            errors.Add("Your age must be an even number - sorry, I don't make the rules");
        }

        return errors.Count == 0
            ? Valid(new Age(age))
            : Invalid(errors.AsEnumerable());
    }
}

public sealed class Name : ValueOf<string>
{
    private Name(string value)
        : base(value)
    {
    }

    public static Validation<Name, string> Parse(string name) =>
        // `NonNullOrWhiteSpace` returns an `Option<string>` which
        // we can turn into a `Validation` like so:
        StringParser.NonNullOrWhiteSpace(name)
            .ValidOr("Name must not be empty")
            .Map(name => new Name(name));
}

[Lambda] // generates a function we can use for lifting
public partial record Person(Name Name, Age Age);
```

We have defined 2 value objects with `Parse` methods that allow us to parse a primitive value into
a rich type. We have also defined a `Person` type which is a composition of the 2 previous types.

How do we construct a `Person` but also collect all validation errors together without a bunch of boilerplate?

We can use the applicative property of `Validation` to do this.

```csharp
Validation<Person, string> personValidation =
    // λ is a function generated by the `[Lambda]` attribute which
    // lets us pass `Person`'s constructor as a regular function.
    // Use your editor's autocomplete for this.
    Valid(Person.λ)
        // Next, we can apply each parameter.
        .Apply(Name.Parse("Bob"))
        .Apply(Age.Parse(26));

// If all the validations are valid, then we'll get a constructed `Person`.
// Otherwise, all the errors will be collected.
if (personValidation.TryGet(out var person, out var errors))
{
    // ...
}
```

---

## Source generation

FxKit ships with a few (optional) source generators, each enabled by their respective attribute:

* `[EnumMatch]`: generates a `Match` extension method for the enum type.
  Useful to guarantee exhaustive matching. Each enum member will be represented as a function parameter to `Match`.
* `[Lambda]`: when used at the type level, generates a `λ` as a `Func`, allowing you to pass a type's constructor as a
  function.
  When used on a method, generates a `Func` for that method suffixed with `λ` in the name.
* `[Union]`: turns the record into a union type. Each member gets a `λ` and `Of` methods for constructing the member.
  The return type of these is the base type which makes it useful for mapping.

To use, install the `FxKit.CompilerServices` and `FxKit.CompilerServices.Annotations` packages.

---

### The `[EnumMatch]` generator

When using `switch` on enums, it is not an exhaustive match - that means if you add new members to the enum, the
compiler does not guarantee that you handle it.

The `[EnumMatch]` generator will generate a `Match` function with parameters for each enum member.

```csharp
[EnumMatch]
public enum DotNetLanguage
{
    CSharp,
    FSharp,
    VB
}

// Usage:
public string GetLanguageName(DotNetLanguage language) =>
    language.Match(
        CSharp: () => "C#",
        FSharp: () => "F#",
        VB: () => "Visual Basic");
```

If you add a new entry to the enum, the `Match` method is regenerated and will cause a compilation
error due to missing a parameter.

---

### The `[Lambda]` generator

Generates a `Func` alias for the decorated type's constructor. Useful for lifting into an applicative such as `Valid`.

```csharp

// Methods for validation
public static Validation<string, string> ValidateName(string name);
public static Validation<int, string> ValidateAge(int age);

// Add to a partial type (record, struct, class)
[Lambda]
public partial record Person(string Name, int Age);

// That generates a `Func` named `λ`. Can be used like this:
var person = Valid(Person.λ)
    .Apply(ValidateName("Bob"))
    .Apply(ValidateAge(48))
    // Turn the validation into a result and join the errors
    // into a string.
    .OkOrElse(error => error.ToJoinedString())
    .Unwrap();
```

---

### The `[Union]` generator

Simplifies the construction of discriminated unions using `record`s. It also includes an analyzer to ensure correct
usage.

```csharp
// Makes `BoxedValue` abstract
[Union]
public partial record BoxedValue
{
    // Each entry becomes `public sealed partial record`
    partial record StringValue(string Value);
    partial record IntValue(int Value);
}
```

This grants `BoxedValue` some new powers.

* `BoxedValue.[Member].Of`: A static function alias for the constructor that returns the base type.
  Useful for inference.
  ```csharp
  // Does not compile
  var value = true ? new BoxedValue.StringValue("One") : new BoxedValue.IntValue(2);

  // Compiles
  var value = true ? BoxedValue.StringValue.Of("One") : BoxedValue.IntValue.Of(2);
  ```
* `BoxedValue.[Member].λ`: A `Func` version of the above, see [the `[Lambda]` generator](#the-lambda-generator).
* `BoxedValue.Match`: A function for exhaustive matching on the union's members,
  see [the `[EnumMatch]` generator](#the-enummatch-generator).
  ```csharp
  var matched = value.Match(
    StringValue: sv => $"string:{sv.Value}",
    IntValue: iv => $"int:{iv.Value}");
  ```

---

## Transformer methods

When working with stacked types, such as `Task<Option<T>>`, `Result<Option<T>, E>` etc, it can be a pain to operate
on the innermost type.

Most of these have auto-generated transformer methods - that is, the method with a `T` suffix, for
example `MapT`, `OkOrElseT`, `UnwrapT` etc.

```csharp
var result =
    // <> needed here as compiler cannot infer the error type.
    Ok<Option<string>, string>(Some("Hello"));

// Maps the value in the inner `Option`
Result<Option<int>, string> mapped = result.MapT(x => x.Length);
```

---

### Transformers for `Task`

The `Task` type is probably the most common "outer" type in a type stack in most real-world programs.
Additionally, `async` is used very frequently, therefore, special methods exist that allow returning `Task`s
such as  `MapAsync`, `FlatMapAsync`, `MapAsyncT`, `FlatMapAsyncT` to name a few.

For example, if you have a `Task<Result<int, string>> stacked`, if you want to map the `int` held in the
innermost type (the `Result`) using an `async` method:

```csharp
// `value` is the `int`, assuming `SomeAsyncMapping` returns `Task<bool>`:
Task<Result<bool, string>> taskOfResultMapped = stacked.MapAsyncT(
    async value => await SomeAsyncMapping(value))
```
