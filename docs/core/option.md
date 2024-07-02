# Option

The `Option` type is a powerful construct used to represent the presence or absence of a value.
This type is particularly useful for eliminating null reference errors, a common source of bugs in C# applications.

An `Option`  can be either `Some` (containing a value), or `None` (indicating the absence of a value).
By using `Option`, developers can enforce safer code practices, making it explicit when a variable,
property, or parameter might not have a value and requires handling both cases.

For error handling, you can use the [`Result`](result) type, which can encapsulate the error information.

## Overview

The `Option` type is a container for a value, so it can be treated as a collection with a single value
supporting operations such as:
- [Mapping](#map--select): apply transformations to the value within a `Option`
- [Filtering](#where): filters a value based on a predicate
- [Flattening](#flatmap--selectmany): apply a function that returns an `Option` and flattens the result

### Usage

The following example demonstrates how to use `Option` to handle non-whitespace strings:

```csharp
public Option<string> OnlyNonWhitespace(string? value) =>
    string.IsNullOrWhiteSpace(value)
        ? None
        : Some(value);
```

In this example:
- The function `OnlyNonWhitespace` takes an input string that may be _null_ or contain whitespace.
- If the input string is null, empty or contains only whitespace, the function returns `None`, indicating the absence of a valid string.
- If the input string is non-whitespace, the function returns `Some` with the actual string value.

## Accessors and Unwrapping

Functions that are used to access or extract values from their containers.

### TryGet / Unwrap

You can use `TryGet` as an escape hatch to get the value out.

```csharp
Option<string> nonWhitespace = OnlyNonWhitespace("hello");

if (nonWhitespace.TryGet(out var value))
{
    Console.WriteLine(value);
}

// Alternatively, `Unwrap` can be used,
// but will throw an exception if in the `None` state.
string value = nonWhitespace.Unwrap();
```

Or you can use `UnwrapOr` / `UnwrapOrElse` and specify a fallback value for when option is empty.

```csharp
Option<string> nonWhitespace = OnlyNonWhitespace("hello");

string value = nonWhitespace.UnwrapOr("<undefined>");
```

### ToNullable

The `ToNullable` / `ToNullableValue` is used to unwrap the value from option and fallback to `null` when it is empty.

```csharp
Option<string> nonWhitespace = OnlyNonWhitespace("hello");

string? value = nonWhitespace.ToNullable();
```

## Pattern Matching and Transformation

Functions that are used to apply transformations or perform pattern matching on values within containers.

### Match

To handle the possible states of the option:

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

### OkOr / OkOrElse

You can turn `Option`s into other types, such as `Result`:

```csharp
Result<int, string> result = OnlyNonWhitespace("  hello  ")
    .Map(x => x.Trim())
    .Where(x => x.Length > 0)
    .OkOr("That string was only whitespace! Bad!")
```

### ValidOr / ValidOrElse

To convert an `Option` to `Validation`:

```csharp
Validation<int, string> result = Some("Hello")
    .Where(x => x.Length < 10)
    .ValidOr("Too long text");
```

## Filtering and Conditional Operators

Functions that are used to filter or conditionally manipulate values within containers.

### Where

Filter the element of an option, if any, based on a predicate.

```csharp
Option<int> nonZeroLength = OnlyNonWhitespace("  hello  ")
    .Map(x => x.Trim())
    .Map(x => x.Length)
    .Where(x => x.Length > 0);
```

### OfType

Use `OfType` to filter the value held in `Some` based on its type.

> This is essentially a shorthand for `FlatMap(a => a is U u ? Some(u) : None)`

## Mapping and Flat Mapping

Functions that are used to apply transformations to each element within a container and manage nested containers.

### Map / Select

Transforming the value:

```csharp
OnlyNonWhitespace("  hello  ")
    .Map(x => x.Trim());
```

LINQ syntax is supported too.

```csharp
Option<int> result =
    from a in Some(3)
    select a + 1;
```

### FlatMap / SelectMany

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

### Do

Use `Do` to execute an imperative operation when the option has a value.

```csharp
Some("Hello")
    .Do(x => Console.WriteLine(x));
```

## Aggregation and Collection Operations

Functions that are used to aggregate or collect values from multiple containers.

### Somes

Use the `Somes` to extract the values from a sequence of options.

```csharp
IEnumerable<int> numbers = ListOf
    .Many(Some(4), Some(3), None, Some(10), None, None, Some(2))
    .AsEnumerable()
    .Somes()
```

### SomesMap

You can use `SomesMap` as a shortcut for `Somes` + `Map`.

```csharp
IEnumerable<int> lengths = ListOf
    .Many(Some("Hello"), Some("world"))
    .SomesMap(v => v.Length)
```

### Traverse

You can _traverse_ between various other container types. For example:

```csharp
IReadOnlyList<int> list = [2, 4, 6];

Option<IReadOnlyList<int>> listOfOnlyEvenNumbers =
    list.Traverse(x => x % 2 == 0 ? Some(x) : None);
// Some([2, 4, 6])
```

### Sequence

Use `Sequence` to traverse without the mapping step.

> This is equivalent to `Traverse(Identity)` / `Traverse(x => x)`

```csharp
IReadOnlyList<Option<int>> list = [Some(2), Some(4), None, Some(6)];

Option<IReadOnlyList<int>> sequenced =
    list.Sequence();
```

## Prelude

The `Prelude` class provides the following functions for `Option`:

### Some / None

Returns a wrapped value or an empty `Option`.

```csharp
public Option<string> OnlyNonWhitespace(string? value) =>
    string.IsNullOrWhiteSpace(value)
        ? None
        : Some(value);
```

### Optional

Use `Optional` to convert a nullable value to `Option`.
If the input value is **not** `null`, the result will be `Some(value)`,
otherwise the result will be an empty option (`None`).

```csharp
string? value = "Hello";
Option<string> wrapped = Optional(value);
```