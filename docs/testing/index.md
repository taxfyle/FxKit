# Testing

FxKit.Testing provides specialized testing utilities and FluentAssertions extensions for working with functional data types in FxKit. These tools make it easier to write clear, expressive tests when working with `Option`, `Result`, `Validation`, and their async counterparts.

## Overview

Testing functional code requires different patterns than traditional imperative code. FxKit.Testing bridges this gap by providing:

- **FluentAssertions Extensions**: Custom assertion methods for FxKit types that integrate seamlessly with FluentAssertions
- **Type-Safe Assertions**: Compile-time checked assertions that understand the structure of functional types
- **Async Support**: Full support for testing `Task<Option<T>>`, `Task<Result<T, E>>`, and other async variants
- **Clear Failure Messages**: Detailed error messages that show exactly what went wrong in your tests

## Installation

Add the FxKit.Testing package to your test project:

```xml
<PackageReference Include="FxKit.Testing" Version="*" />
```

The package depends on FluentAssertions and extends it with FxKit-specific assertion methods.

## Example Functions

Here are the example functions used throughout this documentation:

```csharp
// Option-returning function
public static Option<int> ParseAge(string input)
{
    if (int.TryParse(input, out var age) && age >= 0 && age <= 120)
        return Some(age);
    return None;
}

// Result-returning function
public static Result<int, string> Divide(int dividend, int divisor)
{
    if (divisor == 0)
        return Err<int, string>("Cannot divide by zero");
    return Ok<int, string>(dividend / divisor);
}

// Validation-returning function
public static Validation<Person, string> ValidatePerson(string name, int age)
{
    var errors = new List<string>();
    
    if (string.IsNullOrEmpty(name))
        errors.Add("Name must not be empty");
    
    if (age < 18)
        errors.Add("You must be at least 18 years of age");
    
    if (errors.Any())
        return Invalid<Person, string>(errors);
    
    return Valid(new Person { Name = new Name(name), Age = new Age(age) });
}

// Async versions for Task-based testing
public static Task<Option<int>> ParseAgeAsync(string input)
{
    return Task.FromResult(ParseAge(input));
}

public static Task<Result<int, string>> DivideAsync(int dividend, int divisor)
{
    return Task.FromResult(Divide(dividend, divisor));
}
```

## Quick Start

### Testing Option Types

```csharp
using FxKit.Testing.FluentAssertions;

[Test]
public void ParseAge_WithValidAge_ReturnsSome()
{
    // Arrange
    var input = "25";

    // Act
    var result = ParseAge(input);

    // Assert
    result.Should().BeSome(25);
}

[Test]
public void ParseAge_WithInvalidAge_ReturnsNone()
{
    // Arrange
    var input = "invalid";

    // Act
    var result = ParseAge(input);

    // Assert
    result.Should().BeNone();
}
```

### Testing Result Types

```csharp
[Test]
public void Divide_WithNonZeroDivisor_ReturnsOk()
{
    // Act
    var result = Divide(10, 2);

    // Assert
    result.Should().BeOk(5);
}

[Test]
public void Divide_WithZeroDivisor_ReturnsErr()
{
    // Act
    var result = Divide(10, 0);

    // Assert
    result.Should().BeErr("Cannot divide by zero");
}
```

### Testing Validation Types

```csharp
[Test]
public void ValidatePerson_WithValidData_ReturnsValid()
{
    // Arrange
    var name = "John Doe";
    var age = 25;

    // Act
    var result = ValidatePerson(name, age);

    // Assert
    var person = result.Should().BeValid();
    person.Name.Value.Should().Be("John Doe");
    person.Age.Value.Should().Be(25);
}

[Test]
public void ValidatePerson_WithInvalidData_ReturnsInvalid()
{
    // Arrange
    var name = "";
    var age = 17;

    // Act
    var result = ValidatePerson(name, age);

    // Assert
    var errors = result.Should().BeInvalid();
    errors.Should().Contain("Name must not be empty");
    errors.Should().Contain("You must be at least 18 years of age");
}
```

### Testing Async Operations

```csharp
[Test]
public async Task DivideAsync_WithNonZeroDivisor_ReturnsOk()
{
    // Act
    var taskResult = DivideAsync(10, 2);

    // Assert
    var quotient = await taskResult.Should().BeOk();
    quotient.Should().Be(5);
}

[Test]
public async Task DivideAsync_WithZeroDivisor_ReturnsErr()
{
    // Act
    var taskResult = DivideAsync(10, 0);

    // Assert
    await taskResult.Should().BeErr("Cannot divide by zero");
}

[Test]
public async Task ParseAgeAsync_WithValidAge_ReturnsSome()
{
    // Arrange
    var input = "25";

    // Act
    var taskOption = ParseAgeAsync(input);

    // Assert
    var age = await taskOption.Should().BeSome();
    age.Should().Be(25);
}

[Test]
public async Task ParseAgeAsync_WithInvalidAge_ReturnsNone()
{
    // Arrange
    var input = "invalid";

    // Act
    var taskOption = ParseAgeAsync(input);

    // Assert
    await taskOption.Should().BeNone();
}
```

## Available Assertions

FxKit.Testing provides the following assertion extensions:

### Option Assertions

- `BeSome()` - Assert that the option contains a value
- `BeSome(expectedValue)` - Assert that the option contains a specific value
- `BeNone()` - Assert that the option is empty
- `Be(expectedOption)` - Assert that two options are equal

### Result Assertions

- `BeOk()` - Assert that the result is in the Ok state
- `BeOk(expectedValue)` - Assert that the result contains a specific Ok value
- `BeErr()` - Assert that the result is in the Err state
- `BeErr(expectedError)` - Assert that the result contains a specific error
- `Be(expectedResult)` - Assert that two results are equal

### Validation Assertions

- `BeValid()` - Assert that the validation is valid
- `BeValid(expectedValue)` - Assert that the validation contains a specific valid value
- `BeInvalid()` - Assert that the validation is invalid
- `BeInvalid(expectedErrors)` - Assert that the validation contains specific errors

### Task-based Assertions

All the above assertions are also available for their async counterparts:

- `Task<Option<T>>` - Use with `await` for async option assertions
- `Task<Result<T, E>>` - Use with `await` for async result assertions

## Further Reading

- [Core Types](/core/) - Understanding the types you're testing
