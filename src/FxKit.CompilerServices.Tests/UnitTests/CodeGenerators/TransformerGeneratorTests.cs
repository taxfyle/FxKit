using FluentAssertions;
using FxKit.CompilerServices.CodeGenerators.Transformers;
using FxKit.CompilerServices.Tests.TestUtils;
using Microsoft.CodeAnalysis.CSharp;

namespace FxKit.CompilerServices.Tests.UnitTests.CodeGenerators;

public class TransformerGeneratorTests
{
    private const string ResultTypeAndImplementation =
        """
        namespace Monads
        {
            using FxKit.CompilerServices;
            using System.Collections.Generic;
            using System.Threading.Tasks;

            [Functor]
            public struct Result<TOk, TErr>
                where TErr : notnull
                where TOk : notnull
            {}

            public static class Result
            {
                [GenerateTransformer]
                public static Result<TNewOk, TErr> Map<TOk, TErr, TNewOk>(
                    this Result<TOk, TErr> source,
                    Func<TOk, TNewOk> selector)
                    where TNewOk : notnull
                    where TErr : notnull
                    where TOk : notnull => throw new NotImplementedException();
            }
        }
        """;

    [Test]
    public async Task GeneratesTransformers()
    {
        var output = Generate(
            """
            namespace Monads
            {
                using FxKit.CompilerServices;
                using System.Collections.Generic;
                using System.Threading.Tasks;

                [Functor]
                public struct Result<TOk, TErr>
                    where TOk : notnull
                    where TErr : notnull
                {}

                [Functor]
                public struct Validation<TValid, TInvalid>
                    where TValid : notnull
                    where TInvalid : notnull
                {}

                public static class Result
                {
                    [GenerateTransformer]
                    public static Result<TNewOk, TErr> Map<TOk, TErr, TNewOk>(
                        this Result<TOk, TErr> source,
                        Func<TOk, TNewOk> selector)
                    where TOk : notnull
                    where TNewOk : notnull
                    where TErr : notnull  => throw new NotImplementedException();

                    [GenerateTransformer]
                    public static TOk Unwrap<TOk, TErr>(this Result<TOk, TErr> result)
                    where TOk : notnull
                    where TErr : notnull => throw new NotImplementedException();
                }

                public static class Validation
                {
                    [GenerateTransformer]
                    public static Validation<TNewValid, TInvalid> Map<TValid, TInvalid, TNewValid>(
                        this Validation<TValid, TInvalid> source,
                        Func<TValid, TNewValid> selector)
                        where TValid : notnull
                        where TNewValid : notnull
                        where TInvalid : notnull
                        => throw new NotImplementedException();
                }

                public static class TaskExt
                {
                    [GenerateTransformer]
                    public static async Task<IEnumerable<R>> Traverse<T, R>(
                        this IEnumerable<T> source,
                        Func<T, Task<R>> selector)
                    {
                        var tasks = source.Select(selector);
                        return await Task.WhenAll(tasks);
                    }

                    [GenerateTransformer]
                    public static async Task<IEnumerable<T>> Sequence<T>(
                        this IEnumerable<Task<T>> source)
                    {
                        return await Task.WhenAll(source);
                    }
                }
            }

            namespace Core
            {
                namespace TaskExtensions
                {
                    using System;
                    using System.Threading.Tasks;
                    using System.Collections.Generic;

                    public static class E
                    {
                        public static async Task<U> Map<T, U>(
                            this Task<T> source,
                            Func<T, U> selector) => selector(await source);
                    }
                }

                namespace EnumerableExtensions
                {
                    using System;
                    using System.Collections.Generic;

                    public static class E
                    {
                         public static IEnumerable<U> Map<T, U>(
                            this IEnumerable<T> source,
                            Func<T, U> selector) => source.Select(selector);

                        public static IReadOnlyList<U> Map<T, U>(
                            this IReadOnlyList<T> source,
                            Func<T, U> selector) => source.Select(selector);
                    }
                }
            }
            """);

        await output.VerifyGeneratedCode();
    }

    [Test]
    public void ReportsTypeParameterCollisions()
    {
        var compilation = CompilationUtil.CreateCompilation(
            """
            namespace Monads
            {
                using FxKit.CompilerServices;
                using System.Collections.Generic;
                using System.Threading.Tasks;

                [Functor]
                public struct Result<TOk, TErr>
                    where TErr : notnull
                    where TOk : notnull
                {}

                public static class Result
                {
                    [GenerateTransformer]
                    public static Result<TNewOk, TErr> Map<TOk, TErr, TNewOk>(
                        this Result<TOk, TErr> source,
                        Func<TOk, TNewOk> selector)
                        where TNewOk : notnull
                        where TErr : notnull
                        where TOk : notnull => throw new NotImplementedException();
                }

                // These have the same type parameter names as Result which is a collision.
                [Functor]
                public struct Validation<TOk, TErr>
                    where TErr : notnull
                    where TOk : notnull
                {}

                public static class Validation
                {
                    [GenerateTransformer]
                    public static Validation<TNewOk, TErr> Map<TOk, TErr, TNewOk>(
                        this Validation<TOk, TErr> source,
                        Func<TOk, TNewOk> selector)
                        where TNewOk : notnull
                        where TErr : notnull
                        where TOk : notnull => throw new NotImplementedException();
                }
            }
            """);

        var generator = new TransformerGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out _,
            out var diagnostics);
        diagnostics.Should().HaveCount(1);
        diagnostics[0]
            .Descriptor.Should()
            .Be(DiagnosticsDescriptors.MethodDeclarationCannotAllowCollidingTypeParameters);
    }

    [Test]
    public async Task RespectsTypeConstraints()
    {
        var output = Generate(
            $$"""
              {{ResultTypeAndImplementation}}

              namespace App
              {
                  using System;
                  using System.Collections.Generic;
                  using FxKit.CompilerServices;
                  using StringAlias = string;
                  using Monads;

                  [Functor]
                  public record Filtered<T>(IReadOnlyList<T> Items)
                      where T : IEquatable<T>, IEquatable<StringAlias>;

                  public static class FilteredExtensions
                  {
                      [GenerateTransformer]
                      public static async Filtered<U> Map<T, U>(
                          this Filtered<T> source,
                          Func<T, U> selector)
                          where T : IEquatable<T>, IEquatable<StringAlias>
                          where U : IEquatable<U>, IEquatable<StringAlias>
                          => new Filtered<U>(source.Items.Select(selector).ToList())
                  }

                  public static class Testing
                  {
                      public static void Test()
                      {
                          Result<int, string> item = default;
                          var filtered = new Filtered<Result<int, string>>(list);
                      }
                  }
              }
              """);

        await output.VerifyGeneratedCode();
    }

    [Test]
    public async Task PreservesParameterDefault()
    {
        var output = Generate(
            $$"""
                {{ResultTypeAndImplementation}}

                namespace App
                {
                  using System;
                  using System.Collections.Generic;
                  using FxKit.CompilerServices;
                  using Monads;
                  using StringAlias = string;

                  public static class ResultExtensions
                  {
                      [GenerateTransformer]
                      public static TOk Unwrap<TOk, TErr>(
                          this Result<TOk, TErr> source,
                          StringAlias? exceptionMessage = null)
                          where TOk : notnull
                          where TErr : notnull
                          => throw new NotImplementedException();
                  }
                }

                namespace TaskExtensions
                {
                    using System;
                    using System.Threading.Tasks;
                    using System.Collections.Generic;

                    public static class E
                    {
                        public static async Task<U> Map<T, U>(
                            this Task<T> source,
                            Func<T, U> selector) => selector(await source);
                    }
                }
            """);

        await output.VerifyGeneratedCode();
    }

    private static string Generate(string source) =>
        CodeGeneratorTestUtil.GetGeneratedOutput(new TransformerGenerator(), source);
}
