# Data Types

FxKit exposes C#-friendly implementations of common functional programming data types:

* [`Unit`](unit): A replacement for `void` that can be used as a value.
* [`Option`](option): Alternative to nullable types.
* [`Result`](result): Contains either a success value or an error value. The API is similar to that of Rust's `Result` type.
* [`Validation`](validation): Like `Result` but can hold multiple errors.

They do not allow nullable types (that is, types annotated with `?`, e.g. `int?`, `string?`) in any of their type parameters.

## C# interop

FxKit, being a C# library, exposes the following escape hatches and convenience methods for all abstract data types:

* `Unwrap`: escape hatch which may throw. There are variations, for example `Result` has an `UnwrapErr`.
* `TryGet`: escape hatch using `if` control flow.
* `Match`: exhaustive pattern match on the type's constituents.

::: tip
Each type has implicit conversions where possible. Example:
```csharp
Option<int> o = 1;
```
Is the same as

```csharp
Option<int> o = Some(1);
```
:::

## Functors

`Option`, `Result`, and `Validation`  all share the property of being [functors][functors]. In FxKit, that means they all have
a `Map` / `Select` method which transform the inner value.

In addition, methods have also been provided for the following built-in standard library types in order to
be considered functors by FxKit:

* `System.Threading.Task<T>`
* `System.Collections.Generic.IReadOnlyList<T>`
* `System.Collections.Generic.IEnumerable<T>`

Therefore, these types are also considered for [transformer generation](/compiler/transformer).

**Examples**

::: code-group

```csharp [Option]
Option<int> maybeNumber = 420;

// If the option has a value, map it to a string.
Option<string> maybeNumberAsString =
    maybeNumber.Map(x => x.ToString());
// or
Option<string> maybeNumberAsString =
    maybeNumber.Select(x => x.ToString());
// or
Option<string> maybeNumberAsString =
    from x in maybeNumber
    select x.ToString();
```

```csharp [Result]
Result<int, string> maybeNumber = 420;

// If the result is Ok, map it to a boolean indicating
// whether the number is even.
Result<bool, string> maybeIsEven =
    maybeNumber.Map(x => x % 2 == 0);
// or
Result<bool, string> maybeIsEven =
    maybeNumber.Select(x => x % 2 == 0);
// or
Result<bool, string> maybeIsEven =
    from x in maybeNumber
    select x % 2 == 0;
```

```csharp [IEnumerable]
IEnumerable<int> numbers = [1, 2, 3, 4, 5];

// If the IEnumerable has values, map them to their squares.
IEnumerable<int> squaredNumbers =
    numbers.Map(x => x * x);
// or (these are provided by the .NET standard library
// in `System.Linq`, this is just to show that IEnumerable
// shares the same properties as FxKit types)
IEnumerable<int> squaredNumbers =
    numbers.Select(x => x * x);
// or
IEnumerable<int> squaredNumbers =
    from x in numbers
    select x * x;
```
:::

[functors]: https://en.wikipedia.org/wiki/Functor_(functional_programming)#:~:text=In%20functional%20programming%2C%20a%20functor,structure%20of%20the%20generic%20type.

## Monads

In short, [monads][monads] in FxKit are types which have a `FlatMap` / `SelectMany` method subject to certain laws (also called monadic bind).
All the functors listed above also happen to be monads. `FlatMap` is used to transform and flatten nested monadic
structures (like `Task<Task<T>>` or `Option<Option<T>>`) into a single monad (like `Task<T>` or `Option<T>`).

**Examples**

::: code-group
```csharp [Option]
// Function that parses a string as a number if possible.
public static Option<int> ParseNumber(string str) =>
    int.TryParse(str, out var parsed)
        ? Some(parsed)
        : None;

Option<string> input = "420";

Option<int> maybeNumber =
    input.FlatMap(x => ParseNumber(x));
// or
Option<int> maybeNumber =
    input.SelectMany(x => ParseNumber(x));
// or
Option<int> maybeNumber =
    from x in input
    from parsed in ParseNumber(x)
    select x;
```

```csharp [Task]
// Imagine the following task-returning methods
public static Task<string> GetCurrentLocationAsync();
public static Task<string> GetTemperatureAtLocationAsync(string location);

Task<int> temperatureAtCurrentLocation = GetCurrentLocationAsync()
    .FlatMap(location => GetTemperatureAtLocationAsync(location));
// or
Task<int> temperatureAtCurrentLocation = GetCurrentLocationAsync()
    .SelectMany(location => GetTemperatureAtLocationAsync(location));
// or
Task<int> temperatureAtCurrentLocation =
    from location in GetCurrentLocationAsync()
    from temperature in GetTemperatureAtLocationAsync(location)
    select temperature;
```
:::

[monads]: https://en.wikipedia.org/wiki/Monad_(functional_programming)

### Do-notation

Most languages that support functional programming as the primary paradigm (such as F#, OCaml, and Haskell) have
built-in support at the syntax level to deal with nested flat-mapping. This syntax is generally referred
to as _do-notation_.

In C#, we can emulate do-notation using LINQ query syntax by providing two implementations of `SelectMany`.
Here they are for `Option<T>`:

```csharp
// Basic `SelectMany`, used for unnested calls.
public static Option<U> SelectMany<T, U>(
    this Option<T> source,
    Func<T, Option<U>> selector)

// Overload that accepts an intermediary transformation.
public static Option<UU> SelectMany<T, U, UU>(
    this Option<T> source,
    Func<T, Option<U>> bind,
    Func<T, U, UU> selector)
```

This allows us to use LINQ query syntax:

::: code-group
```csharp [Query syntax]
Option<int> leftOption = 1;
Option<int> rightOption = 2;

Option<string> result =
    from left in leftOption
    from right in rightOption
    let sum = left + right
    select sum.ToString();
```

```csharp [Method syntax]
// The former is translated to this method chain by the compiler.
Option<int> leftOption = 1;
Option<int> rightOption = 2;

Option<string> result =
    leftOption.SelectMany(
        left => rightOption,
        (left, right) => new
        {
            left,
            right
        })
    .Select(
        @t => new
        {
            @t,
            sum = @t.left + @t.right
        })
    .Select(@t => @t.sum.ToString());
```
:::

The overload that accepts 2 functions is used under certain situations, such as accessing a
property that is also of the same type. This is done as an optimization.

::: code-group
```csharp [Query syntax]
public record Language(
    string Name,
    Option<string> DoNotation);

Option<Language> languageOption = new Language(
    Name: "C#",
    DoNotation: "LINQ");

Option<string> howDoesLanguageImplementDoNotation =
    from language in languageOption
    from doNotation in language.DoNotation
    select $"{language} implements do-notation using {doNotation}";
```

```csharp [Method syntax]
public record Language(
    string Name,
    Option<string> DoNotation);

Option<Language> languageOption = new Language(
    Name: "C#",
    DoNotation: "LINQ");

Option<string> howDoesLanguageImplementDoNotation =
    languageOption.SelectMany(
        language => language.DoNotation,
        (language, doNotation) =>
            $"{language} implements do-notation using {doNotation}");
```
:::