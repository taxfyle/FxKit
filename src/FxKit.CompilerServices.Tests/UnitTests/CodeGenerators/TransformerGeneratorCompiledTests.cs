using FluentAssertions;
using FxKit.CompilerServices;
using Monads;

// ReSharper disable UnusedTypeParameter

namespace FxKit.CompilerServices.Tests.UnitTests.CodeGenerators
{
    public class TransformerGeneratorCompiledTests
    {
        [Test]
        public void GeneratesWorkingTransformers()
        {
            var validation = new Validation<int, string>();

            var resultOfValidation = new Result<Validation<int, string>, string>(validation);
            var taskOfValidation = Task.FromResult(validation);
            var listOfValidation = new List<Validation<int, string>>
            {
                validation
            };
            IEnumerable<Validation<int, string>> enumerableOfValidation =
                new List<Validation<int, string>>
                {
                    validation
                };

            GetInnerValue(resultOfValidation.MapT(_ => "abc")).Should().Be("abc");
            GetInnerValue(taskOfValidation.MapT(_ => "abc")).Result.Should().Be("abc");
            GetInnerValue(listOfValidation.MapT(_ => "abc")).Should().Be("abc");
            GetInnerValue(enumerableOfValidation.MapT(_ => "abc")).Should().Be("abc");
        }

        private async Task<T> GetInnerValue<T, U>(Task<Validation<T, U>> val)
            where T : notnull where U : notnull
        {
            var validation = await val;
            return validation.Value;
        }

        private T GetInnerValue<T, U>(IReadOnlyList<Validation<T, U>> val)
            where T : notnull where U : notnull
        {
            return val[0].Value;
        }

        private T GetInnerValue<T, U>(IEnumerable<Validation<T, U>> val)
            where T : notnull where U : notnull
        {
            return val.ToList()[0].Value;
        }

        private T GetInnerValue<T, U, V>(Result<Validation<T, U>, V> val)
            where T : notnull where U : notnull where V : notnull
        {
            return val.Value.Value;
        }
    }
}

namespace Monads
{
    [Functor]
    public struct Result<TOk, TErr>(TOk value = default!)
        where TErr : notnull
        where TOk : notnull
    {
        public TOk Value { get; set; } = value;
    }

    [Functor]
    public struct Validation<TValid, TInvalid>(TValid value = default!)
        where TValid : notnull
        where TInvalid : notnull
    {
        public TValid Value { get; set; } = value;
    }

    public static class Result
    {
        [GenerateTransformer]
        public static Result<TNewOk, TErr> Map<TOk, TErr, TNewOk>(
            this Result<TOk, TErr> source,
            Func<TOk, TNewOk> selector)
            where TNewOk : notnull
            where TErr : notnull
            where TOk : notnull => new(selector(source.Value));
    }

    public static class Validation
    {
        [GenerateTransformer]
        public static Validation<TNewValid, TInvalid> Map<TValid, TInvalid, TNewValid>(
            this Validation<TValid, TInvalid> source,
            Func<TValid, TNewValid> selector)
            where TValid : notnull
            where TNewValid : notnull
            where TInvalid : notnull =>
            new(selector(source.Value));
    }
}

namespace Core
{
    namespace TaskExtensions
    {
        public static class E
        {
            public static async Task<U> Map<T, U>(
                this Task<T> source,
                Func<T, U> selector) => selector(await source);
        }
    }

    namespace EnumerableExtensions
    {
        public static class E
        {
            public static IEnumerable<U> Map<T, U>(
                this IEnumerable<T> source,
                Func<T, U> selector) => source.Select(selector);

            public static IReadOnlyList<U> Map<T, U>(
                this IReadOnlyList<T> source,
                Func<T, U> selector) => source.Select(selector).ToList();
        }
    }
}
