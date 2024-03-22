﻿using FxKit.CompilerServices.CodeGenerators.Transformers;
using FxKit.CompilerServices.Tests.TestUtils;

namespace FxKit.CompilerServices.Tests.UnitTests.CodeGenerators;

public class TransformerGeneratorTests
{
    [Test]
    public async Task GeneratesTransformers()
    {
        var output = Generate(
            @"
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
        where TNewOk : notnull
        where TErr : notnull
        where TOk : notnull => throw new NotImplementedException();
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
");

        await output.VerifyGeneratedCode();
    }

    private static string Generate(string source) =>
        CodeGeneratorTestUtil.GetGeneratedOutput(new TransformerGenerator(), source);
}
