# FluentAssertions Extensions

FxKit.Testing extends FluentAssertions with specialized assertion methods for FxKit's functional types. These extensions provide a fluent, readable API for testing `Option`, `Result`, `Validation`, and their async variants.

## Option Assertions

The `Option<T>` type represents values that may or may not exist. FxKit.Testing provides several assertion methods to validate the state and content of options.

### BeSome()

Asserts that the option contains a value and returns it for further assertions.

```csharp
[Test]
public void ParseInt_WithValidString_ReturnsSome()
{
    // Act
    var result = ParseInt("42");
    
    // Assert - returns the value for further assertions
    var value = result.Should().BeSome();
    value.Should().BeGreaterThan(0);
}
```

### BeSome(expectedValue)

Asserts that the option contains a specific value.

```csharp
[Test]
public void GetConfiguration_ReturnsExpectedValue()
{
    // Act
    var config = GetConfiguration("timeout");
    
    // Assert
    config.Should().BeSome(30);
}
```

### BeNone()

Asserts that the option is empty (contains no value).

```csharp
[Test]
public void FindUser_WithInvalidId_ReturnsNone()
{
    // Act
    var user = FindUser("invalid-id");
    
    // Assert
    user.Should().BeNone();
}
```

### Be(expected)

Asserts that two options are equal.

```csharp
[Test]
public void Transform_ProducesExpectedOption()
{
    // Arrange
    var expected = Some("transformed");
    
    // Act
    var result = Transform(Some("input"));
    
    // Assert
    result.Should().Be(expected);
}
```

## Result Assertions

The `Result<TOk, TErr>` type represents operations that can succeed with a value or fail with an error. FxKit.Testing provides comprehensive assertions for both states.

### BeOk()

Asserts that the result is in the Ok state and returns the value.

```csharp
[Test]
public void Calculate_WithValidInput_ReturnsOk()
{
    // Act
    var result = Calculate(10, 5);
    
    // Assert - returns the Ok value
    var value = result.Should().BeOk();
    value.Should().Be(50);
}
```

### BeOk(expectedValue)

Asserts that the result contains a specific Ok value.

```csharp
[Test]
public void Divide_ReturnsExpectedResult()
{
    // Act
    var result = Divide(10, 2);
    
    // Assert
    result.Should().BeOk(5);
}
```

### BeErr()

Asserts that the result is in the Err state and returns the error.

```csharp
[Test]
public void Validate_WithInvalidData_ReturnsErr()
{
    // Act
    var result = Validate(invalidData);
    
    // Assert - returns the error for further assertions
    var error = result.Should().BeErr();
    error.Message.Should().Contain("validation failed");
}
```

### BeErr(expectedError)

Asserts that the result contains a specific error.

```csharp
[Test]
public void Parse_WithInvalidFormat_ReturnsExpectedError()
{
    // Act
    var result = ParseDate("not-a-date");
    
    // Assert
    result.Should().BeErr("Invalid date format");
}
```

### Be(expectedResult)

Asserts that two results are equal.

```csharp
[Test]
public void Process_ReturnsExpectedResult()
{
    // Arrange
    var expected = Ok<int, string>(42);
    
    // Act
    var result = Process(input);
    
    // Assert
    result.Should().Be(expected);
}
```

## Validation Assertions

The `Validation<T, TErr>` type is similar to `Result` but can accumulate multiple errors. It's particularly useful for validating complex objects with multiple fields.

### BeValid()

Asserts that the validation is valid and returns the value.

```csharp
[Test]
public void ValidateUser_WithValidData_ReturnsValid()
{
    // Arrange
    var userData = new UserData { Name = "John", Age = 25 };
    
    // Act
    var result = ValidateUser(userData);
    
    // Assert - returns the valid value
    var user = result.Should().BeValid();
    user.Name.Should().Be("John");
}
```

### BeValid(expectedValue)

Asserts that the validation contains a specific valid value.

```csharp
[Test]
public void CreateAge_WithValidAge_ReturnsValid()
{
    // Act
    var result = Age.Parse(25);
    
    // Assert
    result.Should().BeValid(new Age(25));
}
```

### BeInvalid()

Asserts that the validation is invalid and returns the errors.

```csharp
[Test]
public void ValidatePerson_WithMultipleErrors_ReturnsInvalid()
{
    // Arrange
    var data = new PersonData { Name = "", Age = 17 };
    
    // Act
    var result = ValidatePerson(data);
    
    // Assert - returns the errors for further assertions
    var errors = result.Should().BeInvalid();
    errors.Should().HaveCount(2);
    errors.Should().Contain("Name is required");
    errors.Should().Contain("Must be at least 18 years old");
}
```

## Task-based Assertions

All assertion methods support async operations through their Task-based counterparts. These work with `Task<Option<T>>`, `Task<Result<TOk, TErr>>`, and similar types.

### Async Option Assertions

```csharp
[Test]
public async Task FetchData_WhenExists_ReturnsSome()
{
    // Act
    var result = await FetchDataAsync("key");
    
    // Assert
    await result.Should().BeSome("expected value");
}

[Test]
public async Task QueryCache_WhenMiss_ReturnsNone()
{
    // Act
    var result = await QueryCacheAsync("missing-key");
    
    // Assert
    await result.Should().BeNone();
}
```

### Async Result Assertions

```csharp
[Test]
public async Task SaveData_WithValidData_ReturnsOk()
{
    // Arrange
    var data = new Data { Id = 1, Value = "test" };
    
    // Act
    var result = await SaveDataAsync(data);
    
    // Assert
    await result.Should().BeOk();
}

[Test]
public async Task FetchUser_WithInvalidId_ReturnsErr()
{
    // Act
    var result = await FetchUserAsync("invalid");
    
    // Assert
    await result.Should().BeErr("User not found");
}
```

## Chaining Assertions

Many assertion methods return the unwrapped value, allowing you to chain additional assertions:

```csharp
[Test]
public void ComplexValidation_ReturnsExpectedStructure()
{
    // Act
    var result = ProcessComplexData(input);
    
    // Assert - chain multiple assertions
    var data = result.Should().BeOk();
    data.Should().NotBeNull();
    data.Items.Should().HaveCount(3);
    data.Items.Should().OnlyContain(x => x.IsValid);
    data.Total.Should().BeGreaterThan(0);
}
```

## Custom Failure Messages

All assertions support custom failure messages through the `because` parameter:

```csharp
[Test]
public void CriticalOperation_MustSucceed()
{
    // Act
    var result = PerformCriticalOperation();
    
    // Assert with custom message
    result.Should().BeOk(
        because: "critical operations must never fail in production");
}

[Test]
public void Configuration_MustBePresent()
{
    // Act
    var config = GetConfiguration("database");
    
    // Assert with formatted message
    config.Should().BeSome(
        because: "the {0} configuration is required for startup", 
        "database");
}
```

## Integration with FluentAssertions

FxKit assertions integrate seamlessly with standard FluentAssertions methods:

```csharp
[Test]
public void CompleteWorkflow_ProducesExpectedResults()
{
    // Act
    var results = ProcessWorkflow(inputs);
    
    // Mix FxKit and standard assertions
    results.Should().HaveCount(5);
    results[0].Should().BeOk(42);
    results[1].Should().BeErr("validation failed");
    results.Should().Contain(r => r.IsOk);
    results.Select(r => r.IsOk).Should().NotAllBe(false);
}
```

## Tips and Best Practices

### 1. Use the Most Specific Assertion

Always use the most specific assertion available for clearer error messages:

```csharp
// Good - specific assertion with clear failure message
result.Should().BeOk(42);

// Less ideal - generic assertion
result.IsOk.Should().BeTrue();
result.Unwrap().Should().Be(42);
```

### 2. Leverage Return Values

Take advantage of assertions that return unwrapped values:

```csharp
// The assertion returns the unwrapped value
var user = result.Should().BeOk();

// Now you can make additional assertions on the user
user.Id.Should().BePositive();
user.Email.Should().MatchRegex(@"^[\w\.-]+@[\w\.-]+\.\w+$");
user.Roles.Should().Contain("Admin");
```

### 3. Test Error Details

When testing error cases, validate the error content:

```csharp
var error = result.Should().BeErr();
error.Code.Should().Be("VALIDATION_ERROR");
error.Message.Should().Contain("invalid format");
error.Details.Should().NotBeEmpty();
```

### 4. Use Async Assertions Properly

Remember to await async assertions:

```csharp
// Correct - awaiting the assertion
await result.Should().BeOk();

// Incorrect - forgetting to await
result.Should().BeOk(); // This won't work for Task<Result<T, E>>
```