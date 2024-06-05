# Core

FxKit exposes C#-friendly implementations of common functional programming data types:

* `Unit`: A replacement for `void` that can be used as a value.
* `Option`: Alternative to nullable types.
* `Result`: Contains either a success value or an error value. The API is similar to that of Rust's `Result` type.
* `Validation`: Like `Result` but can hold multiple errors.

All of them contain escape hatches for when you need to do imperative work. Additionally, they do not allow
nullable reference types (that is, types annotated with `?`, e.g. `int?`, `string?`) in any of their values.

`Option`, `Result`, and `Validation` all contain:

* `Map` / `Select`: transform the inner value.
* `FlatMap` / `SelectMany`: also called monadic bind.
* `Match`: exhaustive pattern match on the type's constituents.
* `Unwrap`: escape hatch which may throw. There are variations.
* `TryGet`: escape hatch using `if` control flow.
* Implicit conversions: each type has implicit conversions where possible (`Option<int> o = 1;` would be the same
  as `Some(1)`).

Extension methods have also been provided for the built-in `Task` to make interop seamless.