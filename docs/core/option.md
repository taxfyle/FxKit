# Option

The `Option` type is a replacement for nullable types. It can be used to represent a value that may or may not be
present.

```csharp
public Option<string> OnlyNonWhitespace(string? value) =>
    string.IsNullOrWhiteSpace(value)
        ? None
        : Some(value);
```

## TryGet / Unwrap

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

## Map / Select

Transforming the value:

```csharp
OnlyNonWhitespace("  hello  ")
    .Map(x => x.Trim());
```

## Match

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

## Where

Filtering:

```csharp
Option<int> nonZeroLength = OnlyNonWhitespace("  hello  ")
    .Map(x => x.Trim())
    .Map(x => x.Length)
    .Where(x => x.Length > 0);
```

## FlatMap / SelectMany

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

## OkOr / OkOrElse

You can turn `Option`s into other types, such as `Result`:

```csharp
Result<int, string> result = OnlyNonWhitespace("  hello  ")
    .Map(x => x.Trim())
    .Map(x => x.Length)
    .Where(x => x.Length > 0)
    .OkOr("That string was only whitespace! Bad!")
```

## Traverse

You can _traverse_ between various other container types. For example:

```csharp
IReadOnlyList<int> list = [2, 4, 6];

Option<IReadOnlyList<int>> listOfOnlyEvenNumbers =
    list.Traverse(x => x % 2 == 0 ? Some(x) : None);
// Some([2, 4, 6])
```