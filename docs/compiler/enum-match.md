# EnumMatch Generator

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