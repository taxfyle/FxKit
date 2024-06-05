# Transformer Methods

When working with stacked types, such as `Task<Option<T>>`, `Result<Option<T>, E>` etc, it can be a pain to operate
on the innermost type.

Most of these have auto-generated transformer methods - that is, the method with a `T` suffix, for
example `MapT`, `OkOrElseT`, `UnwrapT` etc.

```csharp
var result =
    // <> needed here as compiler cannot infer the error type.
    Ok<Option<string>, string>(Some("Hello"));

// Maps the value in the inner `Option`
Result<Option<int>, string> mapped = result.MapT(x => x.Length);
```

---

## Transformers for `Task`

The `Task` type is probably the most common "outer" type in a type stack in most real-world programs.
Additionally, `async` is used very frequently, therefore, special methods exist that allow returning `Task`s
such as  `MapAsync`, `FlatMapAsync`, `MapAsyncT`, `FlatMapAsyncT` to name a few.

For example, if you have a `Task<Result<int, string>> stacked`, if you want to map the `int` held in the
innermost type (the `Result`) using an `async` method:

```csharp
// `value` is the `int`, assuming `SomeAsyncMapping` returns `Task<bool>`:
Task<Result<bool, string>> taskOfResultMapped = stacked.MapAsyncT(
    async value => await SomeAsyncMapping(value))
```