# Unit

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

## Prelude

The `Prelude` class provides the following functions for `Unit`:

### Unit Function

`Unit` value can be returned using the `Unit()` function.

```csharp
public Result<Unit, string> DoSomething()
{
    // Do something
    return Unit();
}
```

### Ignore Function

The `Ignore` function transforms any value into a `Unit` value.