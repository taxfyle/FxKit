# Lambda Generator

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