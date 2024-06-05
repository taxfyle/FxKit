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