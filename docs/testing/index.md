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
public async Task FetchUser_WithValidId_ReturnsOk()
{
    // Arrange
    var userId = "123";

    // Act
    var taskResult = FetchUserAsync(userId);

    // Assert
    var user = await taskResult.Should().BeOk();
    user.Id.Should().Be("123");
}

[Test]
public async Task FetchOptionalData_WhenExists_ReturnsSome()
{
    // Act
    var taskOption = FetchOptionalDataAsync("key");

    // Assert
    await taskOption.Should().BeSome("expected value");
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
