# Source Generation

FxKit ships with a few (optional) source generators, each enabled by their respective attribute:

* `[EnumMatch]`: generates a `Match` extension method for the enum type.
  Useful to guarantee exhaustive matching. Each enum member will be represented as a function parameter to `Match`.
* `[Lambda]`: when used at the type level, generates a `λ` as a `Func`, allowing you to pass a type's constructor as a
  function.
  When used on a method, generates a `Func` for that method suffixed with `λ` in the name.
* `[Union]`: turns the record into a union type. Each member gets a `λ` and `Of` methods for constructing the member.
  The return type of these is the base type which makes it useful for mapping.

To use, install the `FxKit.CompilerServices` and `FxKit.CompilerServices.Annotations` packages.