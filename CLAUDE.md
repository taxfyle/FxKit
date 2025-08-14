# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FxKit is a functional programming library for C# that provides railway-oriented programming patterns using abstract data types like `Result`, `Option`, and `Validation`. It includes Roslyn-based analyzers and source generators for compile-time code generation of union types, exhaustive match patterns, and more.

## Development Commands

### Build and Test

```bash
# Build the solution (from src/ directory)
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Create NuGet packages
dotnet pack --output nupkg
```

### Code Quality

```bash
# Run ReSharper code inspection (from project root)
./scripts/inspect-code.sh

# The inspection uses dotnet tool jb (JetBrains) and checks for code quality issues
```

### Working with Individual Tests

```bash
# Run a specific test project
dotnet test FxKit.Tests/FxKit.Tests.csproj

# Run tests matching a specific filter
dotnet test --filter "FullyQualifiedName~Option"
```

### Documentation

```bash
# Documentation commands (from docs/ directory)
npm run docs:dev     # Start development server
npm run docs:build   # Build documentation site
npm run docs:preview # Preview built documentation
```

## Architecture Overview

### Core Library Structure

The solution contains four main projects:

1. **FxKit** - Core library with functional data types (`Option<T>`, `Result<TOk, TErr>`, `Validation<T, TErr>`)
2. **FxKit.CompilerServices** - Roslyn analyzers and source generators for compile-time code generation
3. **FxKit.CompilerServices.Annotations** - Attributes used to trigger source generation (`[Union]`, `[Lambda]`, `[Functor]`, etc.)
4. **FxKit.Testing** - FluentAssertions extensions for testing FxKit types

### Key Architectural Patterns

#### Functional Data Types

The library centers around functional programming patterns with immutable data types:

- **Option&lt;T&gt;**: Represents optional values, avoiding null references
- **Result&lt;TOk, TErr&gt;**: Railway-oriented programming for error handling
- **Validation&lt;T, TErr&gt;**: Accumulative error handling for validation scenarios
- **Unit**: Represents the absence of a meaningful value

#### Source Generation Architecture

The `FxKit.CompilerServices` project uses Roslyn's IIncrementalGenerator API to generate code at compile time:

1. **Union Generator** (`UnionGenerator.cs`): Creates discriminated unions from partial records marked with `[Union]`
2. **Lambda Generator** (`LambdaGenerator.cs`): Generates lambda expressions for methods and constructors marked with `[Lambda]`
3. **Transformer Generator** (`TransformerGenerator.cs`): Creates transformer classes for functors marked with `[GenerateTransformer]`
4. **EnumMatch Generator** (`EnumMatchGenerator.cs`): Generates exhaustive match extension methods for enums marked with `[EnumMatch]`

Each generator follows a pipeline pattern:

- **Syntax Detection**: Identifies relevant syntax nodes with attributes
- **Semantic Analysis**: Validates and transforms syntax into semantic models
- **Code Generation**: Uses IndentedTextWriter to generate formatted C# code
- **Source Output**: Registers generated code with the compilation

#### Analyzer Architecture

The analyzers ensure correct usage of attributes and enforce coding patterns:

- **UnionAnalyzer**: Validates union type declarations
- **LambdaAttributeAnalyzer**: Ensures methods with `[Lambda]` are static
- **MustBePartialAnalyzer**: Enforces partial modifiers on types requiring code generation

### Global Configuration

The project uses:

- **.NET 8.0** as the target framework
- **C# 12** language features
- **Nullable reference types** enabled globally
- **ImplicitUsings** enabled for common namespaces
- **MinVer** for automatic semantic versioning from Git tags

### Testing Approach

Tests use:

- **NUnit** as the test framework
- **FluentAssertions** for readable assertions
- **Verify** library for snapshot testing of generated code
- Separate test projects for core library and compiler services

The `FxKit.CompilerServices.Tests` project includes:

- Unit tests for analyzers with diagnostic verification
- Code generator tests with snapshot testing
- Compiled tests that verify generated code actually compiles and runs correctly

### Key Design Decisions

1. **Immutable by Design**: All functional types are readonly structs
2. **Non-nullable Constraints**: Generic type parameters have `notnull` constraints to prevent null values
3. **Source Generation Over Reflection**: Compile-time code generation for performance and type safety
4. **Incremental Generation**: Uses Roslyn's incremental generator API for efficient IDE performance
5. **Attribute-Driven**: Source generation triggered by attributes, keeping the API surface clean
