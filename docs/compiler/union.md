# Union Generator

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
* `BoxedValue.[Member].λ`: A `Func` version of the above, see [the `[Lambda]` generator](/compiler/lambda).
* `BoxedValue.Match`: A function for exhaustive matching on the union's members,
  see [the `[EnumMatch]` generator](/compiler/enum-match).
  ```csharp
  var matched = value.Match(
    StringValue: sv => $"string:{sv.Value}",
    IntValue: iv => $"int:{iv.Value}");
  ```