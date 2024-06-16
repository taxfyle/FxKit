# Transformer methods

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

## How it works

Transformers work on the basis of [functors][functors]. These are types that wrap another generic type, and they
provide a function called `fmap` subject to certain laws which allows us to transform the type of `T` to another type but without losing the
outer type. In C#, `Select` is the canonical equivalent to `fmap`.

For example, `IEnumerable<T>` is a functor because it provides a `Select<T, U>` that lets us transform
an `IEnumerable<T>` to an `IEnumerable<U>`.

::: tip
FxKit supplies both `Map` and `Select`, but they do the same thing. The former is nicer to use
when chaining, and the latter enables using LINQ like
```csharp
from x in functor
select x
```
:::

When working with stacked functors (for example, `IEnumerable<Option<T>>`), if you want to map the `T`, you
would need to use 2 `Map` calls:

```csharp
IEnumerable<Option<int>> source;

source.Map(option => option.Map(item => item * 2));
```

We know that in order to operate on the innermost type, we always have to call `Map` on the outermost one.
Because of this property always holding true for functors, we can automate it away by generating `T`-suffixed wrapper
methods.

```csharp
source.Map(option => option.Unwrap());
// becomes
source.UnwrapT();
```

FxKit already ships with transformer methods generated for the functors it defines.

[functors]: https://en.wikipedia.org/wiki/Functor_(functional_programming)#:~:text=In%20functional%20programming%2C%20a%20functor,structure%20of%20the%20generic%20type.

## Generating transformers

To generate transformers for your own types, the type must satisfy the following:
1. It must be marked as a `[Functor]`.
2. It must provide an implementation of `Map` in the form of an extension method.
3. There must be no [type parameter collision](#type-parameter-collision).

```csharp
using FxKit.CompilerServices;

namespace App;

[Functor]
public record Box<T>(T Item);

public static class Box
{
    // Map the value held within the box.
    [GenerateTransformer] // will have a transformer generated for it, and will be used by generated transformers.
    public static Box<U> Map<T, U>(this Box<T> source, Func<T, U> selector) =>
        new Box<U>(selector(source.Item));
}
```

Now you can add `[GenerateTransformer]` to any method that fits the following criteria:
1. Is an extension method.
2. The first parameter is the type (functor).

```csharp
public static class MoreBoxExtensions
{
    [GenerateTransformer]
    public static T Unwrap(this Box<T> box) => box.Item;
}
```

This will now result in `MapT` and `UnwrapT`-variant being generated for every known functor type in your project.

```csharp
var box = new Box<int>(2);
var outer = Task.FromResult(box);
Task<Box<string>> mapped = outer.MapT(x => x.ToString());
Task<string> unwrapped = mapped.UnwrapT();
```

```csharp
var box = new Box<int>(2);
IEnumerable<Box<int>> outer = new[] { box };
IEnumerable<Box<string>> mapped = outer.MapT(x => x.ToString());
IEnumerable<string> unwrapped = mapped.UnwrapT();
```

### External types

The transformer generator will consider project references as well as external assembly references when
discovering functors. When a project uses the transformer generator, the following assembly attribute is emitted:

```csharp
[assembly: FxKit.CompilerServices.ContainsFunctors]
```

This is used to quickly scan all references for whether they contain functors. If they do,
the generator scans the assembly for all types marked as `[Functor]` as well as any `Map` method
that operates on any known functors.

::: warning
Currently, transformers are not generated for external types' `[GenerateTransformer]` methods. For example,
if you define a `MyCustomFunctor<T>` with a `Map`, the generated transformer goes only one way.
You would get a `Option<MyCustomFunctor<U>> MapT` but **not** a `MyCustomFunctor<Option<T>> MapT`.

This is done to make sure we don't potentially generate ambiguous methods. This limitation may be
lifted in the future.
:::

### Type parameter collision

When generating transformers, it's important to be mindful of type parameter collisions.

Imagine the following 2 types that we will generate transformers for:

```csharp
[Functor]
struct Result<T, E>;

[Functor]
struct Validation<T, E>;
```

Naturally, they each have an implementation of `Map` with a signature like:

```csharp
public static Result<R, E> Map<T, E, R>(
    this Result<T, E> source,
    Func<T, R> selector);

public static Validation<R, E> Map<T, E, R>(
    this Validation<T, E> source,
    Func<T, R> selector);
```

So far so good. Let's pretend we have the following stacked concrete type:

```csharp
Result<Validation<int, SomeValidationError>, DifferentResultError>
```

Notice how both the `Result` and `Validation` have a different type in their `E` type parameter.
When we need to generate transformers that operate on _both types_, we'll
run into some problems. This is what would be generated:

```csharp
public static Result<Validation<R, E>, E> MapT<T, E, R, E>(
    this Result<Validation<T, E>, E> source,
    Func<T, R> selector);
```

1. The `E` is declared twice in the `MapT<>` signature.
2. The 2nd type argument `Validation` and `Result`  being `E` is wrong,
   it was supposed to be `SomeValidationError` and `DifferentResultError` respectively.

::: tip
The generator will spot the collision and report it as a diagnostic with ID `FXKIT0008`.
:::


To get around this, the FxKit types which have multiple type parameters use a contextual suffix for their
type parameter names:

```csharp
[Functor]
public struct Result<TOk, TErr>;

[Functor]
public struct Validation<TValid, TInvalid>;
```

Similarly, the `Map` implementation follows a pattern of using `TNew*`, like so:

```csharp
public static Result<TNewOk, TErr> Map<TOk, TErr, TNewOk>(
    this Result<TNewOk, TErr> source,
    Func<TOk, TNewOk> selector);

public static Validation<TNewValid, TInvalid> Map<TValid, TInvalid, TNewInvalid>(
    this Validation<TValid, TInvalid> source,
    Func<TValid, TNewValid> selector);
```

We can now generate the transformer without type parameter name collisions:

```csharp
public static Result<Validation<TNewValid, TInvalid>, TErr> MapT<TValid, TInvalid, TNewValid, TErr>(
    this Result<Validation<TValid, TInvalid>, TErr> source,
    Func<TValid, TNewValid> selector);
```

## Transformers for `Task`

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