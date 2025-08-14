# Testing Patterns

This guide covers common patterns and best practices for testing functional code with FxKit. These patterns help you write maintainable, expressive tests that effectively validate your functional programming logic.

## Testing Pure Functions

Pure functions are the foundation of functional programming. They always return the same output for the same input and have no side effects.

### Basic Pure Function Testing

```csharp
[Test]
public void Add_WithTwoNumbers_ReturnsSum()
{
    // Pure functions are easy to test
    var result = Add(2, 3);
    result.Should().Be(5);
}

[Test]
public void ParseAge_WithValidInput_ReturnsSome()
{
    // Test the happy path
    var result = ParseAge("25");
    result.Should().BeSome(25);
}

[Test]
public void ParseAge_WithInvalidInput_ReturnsNone()
{
    // Test the error case
    var result = ParseAge("invalid");
    result.Should().BeNone();
}
```

### Property-Based Testing

Property-based testing verifies that certain properties hold for all valid inputs:

```csharp
[Test]
public void Option_Map_PreservesNone()
{
    // Property: Mapping over None always returns None
    var none = Option<int>.None;
    var result = none.Map(x => x * 2);
    result.Should().BeNone();
}

[Test]
public void Result_Map_PreservesError()
{
    // Property: Mapping over Err preserves the error
    var error = Err<int, string>("error");
    var result = error.Map(x => x * 2);
    result.Should().BeErr("error");
}

[Test]
[TestCase(1)]
[TestCase(10)]
[TestCase(100)]
public void Option_Roundtrip_PreservesValue(int value)
{
    // Property: Converting to None and back preserves the value
    var option = Some(value);
    var result = option.ToNullable();
    Option.FromNullable(result).Should().BeSome(value);
}
```

## Testing Monadic Chains

Functional code often involves chaining operations using `Map`, `FlatMap`, and other monadic operations.

### Testing Map Chains

```csharp
[Test]
public void ProcessUser_WithValidData_TransformsCorrectly()
{
    // Arrange
    var userId = "123";
    
    // Act - chain multiple transformations
    var result = GetUser(userId)
        .Map(user => user.Name)
        .Map(name => name.ToUpper())
        .Map(name => $"Hello, {name}!");
    
    // Assert
    result.Should().BeSome("Hello, JOHN!");
}
```

### Testing FlatMap Chains

```csharp
[Test]
public void ValidateAndProcess_WithValidData_Succeeds()
{
    // Arrange
    var input = new RequestData { Value = 42 };
    
    // Act - chain operations that return Result
    var result = ValidateRequest(input)
        .FlatMap(ProcessRequest)
        .FlatMap(SaveResult);
    
    // Assert
    result.Should().BeOk();
}

[Test]
public void ValidateAndProcess_WithInvalidData_FailsFast()
{
    // Arrange
    var input = new RequestData { Value = -1 };
    
    // Act - validation fails, subsequent operations are skipped
    var result = ValidateRequest(input)
        .FlatMap(ProcessRequest)
        .FlatMap(SaveResult);
    
    // Assert
    result.Should().BeErr("Value must be positive");
}
```

## Testing Validation Scenarios

Validation often involves checking multiple conditions and accumulating errors.

### Testing Single Validations

```csharp
[Test]
public void ValidateEmail_WithValidEmail_ReturnsValid()
{
    // Act
    var result = Email.Parse("user@example.com");
    
    // Assert
    result.Should().BeValid();
    var email = result.Unwrap();
    email.Value.Should().Be("user@example.com");
}

[Test]
public void ValidateEmail_WithInvalidEmail_ReturnsInvalid()
{
    // Act
    var result = Email.Parse("not-an-email");
    
    // Assert
    var errors = result.Should().BeInvalid();
    errors.Should().Contain("Invalid email format");
}
```

### Testing Composite Validations

```csharp
[Test]
public void ValidatePerson_WithAllValidData_ReturnsValid()
{
    // Act - validate multiple fields
    var result = 
        Valid(Person.λ)
            .Apply(Name.Parse("John Doe"))
            .Apply(Age.Parse(25))
            .Apply(Email.Parse("john@example.com"));
    
    // Assert
    var person = result.Should().BeValid();
    person.Name.Value.Should().Be("John Doe");
    person.Age.Value.Should().Be(25);
    person.Email.Value.Should().Be("john@example.com");
}

[Test]
public void ValidatePerson_WithMultipleErrors_AccumulatesAllErrors()
{
    // Act - multiple validations fail
    var result = 
        Valid(Person.λ)
            .Apply(Name.Parse(""))           // Invalid
            .Apply(Age.Parse(17))            // Invalid
            .Apply(Email.Parse("invalid"));  // Invalid
    
    // Assert - all errors are accumulated
    var errors = result.Should().BeInvalid();
    errors.Should().HaveCount(3);
    errors.Should().Contain("Name is required");
    errors.Should().Contain("Must be at least 18 years old");
    errors.Should().Contain("Invalid email format");
}
```

## Testing Async Operations

Many real-world operations are asynchronous. FxKit provides Task-based variants of its types.

### Testing Async Options

```csharp
[Test]
public async Task FetchUserPreference_WhenExists_ReturnsSome()
{
    // Arrange
    var userId = "123";
    
    // Act
    var result = await FetchUserPreferenceAsync(userId);
    
    // Assert
    await result.Should().BeSome();
    var preference = await result.Should().BeSome();
    preference.Theme.Should().Be("dark");
}

[Test]
public async Task FetchUserPreference_WhenNotExists_ReturnsNone()
{
    // Arrange
    var userId = "unknown";
    
    // Act
    var result = await FetchUserPreferenceAsync(userId);
    
    // Assert
    await result.Should().BeNone();
}
```

### Testing Async Results

```csharp
[Test]
public async Task ProcessPayment_WithValidCard_Succeeds()
{
    // Arrange
    var payment = new PaymentRequest 
    { 
        Amount = 100.00m,
        CardNumber = "4111111111111111"
    };
    
    // Act
    var result = await ProcessPaymentAsync(payment);
    
    // Assert
    var confirmation = await result.Should().BeOk();
    confirmation.TransactionId.Should().NotBeEmpty();
    confirmation.Status.Should().Be("approved");
}

[Test]
public async Task ProcessPayment_WithInvalidCard_ReturnsError()
{
    // Arrange
    var payment = new PaymentRequest 
    { 
        Amount = 100.00m,
        CardNumber = "invalid"
    };
    
    // Act
    var result = await ProcessPaymentAsync(payment);
    
    // Assert
    var error = await result.Should().BeErr();
    error.Code.Should().Be("INVALID_CARD");
}
```

## Testing Error Recovery

Functional programming emphasizes explicit error handling. Test both error cases and recovery strategies.

### Testing Fallback Values

```csharp
[Test]
public void GetConfiguration_WithFallback_UsesDefaultWhenMissing()
{
    // Act - use UnwrapOr for fallback
    var timeout = GetConfiguration("timeout")
        .UnwrapOr(30);
    
    // Assert
    timeout.Should().Be(30);
}

[Test]
public void CalculateDiscount_WithFallback_UsesDefaultOnError()
{
    // Arrange
    var invalidCoupon = "EXPIRED";
    
    // Act - use UnwrapOr for error recovery
    var discount = CalculateDiscount(invalidCoupon)
        .UnwrapOr(0.0m);
    
    // Assert
    discount.Should().Be(0.0m);
}
```

### Testing Error Transformation

```csharp
[Test]
public void ParseAndValidate_TransformsParseErrorToValidationError()
{
    // Arrange
    var input = "invalid-json";
    
    // Act - transform parse error to domain error
    var result = ParseJson(input)
        .MapErr(parseError => new ValidationError
        {
            Field = "data",
            Message = $"Invalid JSON: {parseError}"
        });
    
    // Assert
    var error = result.Should().BeErr();
    error.Field.Should().Be("data");
    error.Message.Should().Contain("Invalid JSON");
}
```

## Testing Railway-Oriented Programming

Railway-oriented programming uses `Result` types to chain operations that might fail.

### Testing Success Path

```csharp
[Test]
public void ProcessOrder_WithValidOrder_CompletesSuccessfully()
{
    // Arrange
    var order = new Order { Id = "123", Total = 100.00m };
    
    // Act - each step can fail, but all succeed
    var result = ValidateOrder(order)
        .FlatMap(CalculateTax)
        .FlatMap(ApplyDiscount)
        .FlatMap(ChargePayment)
        .FlatMap(SendConfirmation);
    
    // Assert
    var confirmation = result.Should().BeOk();
    confirmation.OrderId.Should().Be("123");
    confirmation.Status.Should().Be("completed");
}
```

### Testing Failure at Different Points

```csharp
[Test]
public void ProcessOrder_WithInvalidOrder_FailsAtValidation()
{
    // Arrange
    var order = new Order { Id = "", Total = 100.00m };
    
    // Act - fails at first step
    var result = ValidateOrder(order)
        .FlatMap(CalculateTax)
        .FlatMap(ApplyDiscount)
        .FlatMap(ChargePayment)
        .FlatMap(SendConfirmation);
    
    // Assert - error from validation
    result.Should().BeErr("Order ID is required");
}

[Test]
public void ProcessOrder_WithPaymentFailure_FailsAtPayment()
{
    // Arrange
    var order = new Order { Id = "123", Total = 1000000.00m };
    
    // Act - fails at payment step
    var result = ValidateOrder(order)
        .FlatMap(CalculateTax)
        .FlatMap(ApplyDiscount)
        .FlatMap(ChargePayment)        // Fails here
        .FlatMap(SendConfirmation);
    
    // Assert - error from payment
    result.Should().BeErr("Insufficient funds");
}
```

## Testing Side Effects

While functional programming minimizes side effects, they're sometimes necessary. Test them explicitly.

### Testing with Mocks

```csharp
[Test]
public void SaveUser_WithValidUser_PersistsToDatabase()
{
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var user = new User { Id = "123", Name = "John" };
    
    mockRepo
        .Setup(r => r.Save(It.IsAny<User>()))
        .Returns(Ok(Unit.Value));
    
    // Act
    var result = SaveUser(user, mockRepo.Object);
    
    // Assert
    result.Should().BeOk();
    mockRepo.Verify(r => r.Save(user), Times.Once);
}
```

### Testing Do Notation Side Effects

```csharp
[Test]
public void ProcessWithSideEffects_ExecutesSideEffectsOnSuccess()
{
    // Arrange
    var sideEffectExecuted = false;
    var input = Some(42);
    
    // Act
    var result = input
        .Do(value => sideEffectExecuted = true)
        .Map(x => x * 2);
    
    // Assert
    result.Should().BeSome(84);
    sideEffectExecuted.Should().BeTrue();
}

[Test]
public void ProcessWithSideEffects_SkipsSideEffectsOnNone()
{
    // Arrange
    var sideEffectExecuted = false;
    var input = Option<int>.None;
    
    // Act
    var result = input
        .Do(value => sideEffectExecuted = true)
        .Map(x => x * 2);
    
    // Assert
    result.Should().BeNone();
    sideEffectExecuted.Should().BeFalse();
}
```

## Common Testing Patterns

### Arrange-Act-Assert Pattern

```csharp
[Test]
public void StandardTestStructure()
{
    // Arrange - set up test data
    var input = new TestData { Value = 42 };
    var expected = Ok(84);
    
    // Act - perform the operation
    var result = ProcessData(input);
    
    // Assert - verify the outcome
    result.Should().Be(expected);
}
```

### Testing Multiple Scenarios with TestCases

```csharp
[TestCase("123", true)]
[TestCase("abc", false)]
[TestCase("", false)]
[TestCase(null, false)]
public void IsNumeric_VariousInputs_ReturnsExpectedResult(string input, bool expected)
{
    // Act
    var result = IsNumeric(input);
    
    // Assert
    result.IsSome.Should().Be(expected);
}
```

### Testing Collections of Results

```csharp
[Test]
public void ProcessBatch_WithMixedResults_HandlesEachCorrectly()
{
    // Arrange
    var items = new[] { "1", "2", "invalid", "4" };
    
    // Act
    var results = items.Select(ParseInt).ToList();
    
    // Assert
    results.Should().HaveCount(4);
    results[0].Should().BeSome(1);
    results[1].Should().BeSome(2);
    results[2].Should().BeNone();
    results[3].Should().BeSome(4);
    
    // Check aggregate statistics
    results.Where(r => r.IsSome).Should().HaveCount(3);
    results.Where(r => !r.IsSome).Should().HaveCount(1);
}
```

## Best Practices

### 1. Use Specific Assertions

Prefer specific assertions over general ones for better error messages:

```csharp
// Good - provides clear error message
result.Should().BeOk(5);

// Less ideal - generic assertion
result.IsOk.Should().BeTrue();
result.Unwrap().Should().Be(5);
```

### 2. Test Both Success and Failure Cases

Always test both the happy path and error cases:

```csharp
[Test]
public void Operation_WithValidInput_Succeeds() { /* ... */ }

[Test]
public void Operation_WithInvalidInput_Fails() { /* ... */ }
```

### 3. Use Descriptive Test Names

Follow a clear naming pattern that describes the scenario and expected outcome:

```csharp
[Test]
public void MethodName_StateUnderTest_ExpectedBehavior() { /* ... */ }
```

### 4. Leverage Return Values

Many assertions return the unwrapped value for further assertions:

```csharp
var value = result.Should().BeOk();
value.Should().BeGreaterThan(0);
value.Should().BeLessThan(100);
```

### 5. Test at the Right Level

- **Unit test pure functions** - Fast, isolated tests for business logic
- **Integration test side effects** - Test database operations, API calls, file I/O
- **End-to-end test critical paths** - Test complete user workflows

### 6. Make Tests Readable

Use clear arrangement and descriptive variable names:

```csharp
[Test]
public void CalculateDiscount_WithValidCoupon_AppliesCorrectPercentage()
{
    // Arrange - clearly show test setup
    var originalPrice = 100.00m;
    var couponCode = "SAVE20";
    var expectedDiscount = 20.00m;
    
    // Act - single operation being tested
    var result = CalculateDiscount(originalPrice, couponCode);
    
    // Assert - verify the outcome
    result.Should().BeOk(expectedDiscount);
}
```

### 7. Avoid Testing Implementation Details

Focus on behavior, not internal structure:

```csharp
// Good - tests behavior
[Test]
public void GetUser_ReturnsUserWithEmail()
{
    var user = GetUser("123");
    user.Should().BeSome();
    user.Unwrap().Email.Should().NotBeEmpty();
}

// Bad - tests implementation details
[Test]
public void GetUser_CallsDatabaseExactlyOnce()
{
    // Don't test HOW it works, test WHAT it does
}
```

### 8. Use Property-Based Testing

Verify that fundamental properties hold for all inputs:

```csharp
[Test]
public void Map_PreservesStructure()
{
    // Property: mapping None always returns None
    Option<int>.None.Map(x => x * 2).Should().BeNone();
    
    // Property: mapping Some preserves Some
    Some(5).Map(x => x * 2).IsSome.Should().BeTrue();
}
```

### 9. Keep Tests Independent

Each test should be able to run in isolation:

```csharp
[Test]
public void Test_ShouldNotDependOnOtherTests()
{
    // Each test sets up its own data
    var testData = CreateTestData();
    
    // Performs its own operations
    var result = ProcessData(testData);
    
    // And cleans up if necessary
    CleanupTestData(testData);
    
    // Assert
    result.Should().BeOk();
}
```

### 10. Test Error Messages

Verify that errors provide useful information:

```csharp
[Test]
public void Validation_WithInvalidData_ProvidesHelpfulError()
{
    // Act
    var result = ValidateEmail("not-an-email");
    
    // Assert - check the error is helpful
    var error = result.Should().BeErr();
    error.Should().Contain("must be a valid email address");
    error.Should().Contain("example@domain.com"); // Shows expected format
}
```