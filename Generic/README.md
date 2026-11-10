# Generic

## Overview

General-purpose code that any .NET project could use: extension methods over
built-in types (`IEnumerable<T>`, dictionaries, `KeyValuePair`, `TimeSpan`,
`Random`) and external libraries (`UnitsNet` quantities, `LanguageExt`
`Option<T>`), plus utility tools such as the substitutable terminal in
[`System`](./System/README.md) and `ExceptionHelper`.

## Responsibility

General-use extensions to built-in and external libraries, and general utility
tools, none of them particular to robotics. Code belongs here only when it would
make sense unchanged in a project that has nothing to do with robots.

## Public APIs

`Generic.System.Terminal`, `SystemTerminal`, and extension methods for units,
time, dictionaries, and sequences.

## Local Interface & Invariants

All types below are `public static` classes in namespace `Generic`.

### `EnumerableExtensions` (`EnumerableExtensions.cs`)

- `SingleOption<T>()` / `SingleOption<T>(Func<T, bool> predicate)`: `None` for
  an empty (filtered) sequence, otherwise `Single()`, which throws
  `InvalidOperationException` when more than one element matches.
- `FirstOption<T>()`, `LastOption<T>()`: `None` when empty.
- `ExtendWithSelectMany<T>(Func<T, IEnumerable<T>> extension)`: for each
  element, yields `extension(e)` followed by `e` itself.
- `GetCurrentAndNextAsPair<T>()` and `ZipWithNext<T>()`: consecutive pairs
  `(current, next)`; empty for sequences shorter than two.
- `ZipWithTwo<T, U, Y>(IEnumerable<U>, IEnumerable<Y>)`: triples, truncated to
  the shortest input.
- `IsDistinct<T>()`: `Count() == Distinct().Count()`.
- `FilterForSome<T>()` over `IEnumerable<Option<T>>`: the values of the `Some`
  elements.
- `ContainsAll<T>(IEnumerable<T> expected)`: every expected element is in the
  actual sequence.
- `ForEach<T>(Action<T>)`: runs the action eagerly on each element.
- `Map<T, R>(Func<T, R>)`: alias of `Select`.
- `Combinations(int n)` (C# extension block on `IEnumerable<T>`): all
  order-preserving `n`-element subsets; `n == 0` yields one empty combination.

### `DictionaryExtentions` (`DictionaryExtentions.cs`)

- `MapValue` (`IDictionary`) / `MapValueToReadonly` (`IReadOnlyDictionary`):
  a new dictionary with the same keys and mapped values; the source is unchanged.
- `LookFor(key)` on `IDictionary` and `IReadOnlyDictionary`: `Option<TValue>`,
  `None` when the key is missing.
- `AppendElement(key, element)` on `IDictionary<TKey, List<TElement>>`: adds to
  the key's list, creating `[element]` when the key is missing.

### Other helpers

- `KeyValuePairExtensions.Map<K, T, U>(Func<T, U>)`: same key, mapped value.
- `TimeSpanExtensions.ClampToPositive()`: the value if greater than
  `TimeSpan.Zero`, else `TimeSpan.Zero`.
- `RandomExtensions.NextDoubleNeg1ToPos1()`: `random.NextDouble() * 2 - 1`,
  in `[-1, 1)`.
- `UnitsNetExtensions` (static helpers, not extension methods):
  - `RotationalAccelerationFromRpm2(double rpmSquared)`: returns
    `RotationalAcceleration.FromRadiansPerSecondSquared(rpmSquared / 60²)`; it
    rescales minutes to seconds only and applies no revolution-to-radian (2π)
    factor. No caller in the solution uses it.
  - `IsAlmostEqualSingleRotation(Angle, Angle, Angle precision)`: compares the
    sine and cosine of both angles, so angles a full turn apart are equal.
  - `IsAlmostEqual(Length, Length, Length precision)`: strict
    `|a - b| < precision`.
  - `Modulo(Length a, Length b)`: `a - b * Floor(a / b)` (floored; the result
    takes the sign of `b`).
- `ExceptionHelper.ThrowExceptionOnAnyKey(string message)`: prints the message,
  waits for a key press, then throws `Exception(message)`.
- `DateTimeExtensions`: empty; declares no members.

### Local invariants

- `SingleOption`, `FirstOption`, `LastOption`, `IsDistinct`, `ZipWithNext`,
  `GetCurrentAndNextAsPair`, `Combinations` and `ContainsAll` (its `actual`
  sequence) enumerate the source more than once; pass re-enumerable sequences,
  not one-shot iterators.
- Only `AppendElement`, `ForEach` (through its action), `RandomExtensions` and
  `ExceptionHelper` have effects; every other helper is a pure function of its
  inputs.
- Physical quantities cross this API as `UnitsNet` types (`Angle`, `Length`,
  `RotationalAcceleration`); only `RotationalAccelerationFromRpm2` takes a bare
  `double`, and returns a `UnitsNet` type.

## Subdirectories

- System: [./System/README.md](./System/README.md) - `Terminal` interface and
  console-backed `SystemTerminal`, the substitutable terminal IO boundary.

## Key Concepts

- **Extension methods over wrappers:** missing operations are added to types
  we do not own (`IEnumerable<T>`, `IDictionary`, `Length`) as extension
  methods, so callers use them like built-in members instead of through new
  types.
- **`Option<T>` for absence:** a lookup that may find nothing (`LookFor`,
  `SingleOption`, `FirstOption`) returns `Option<T>` instead of null or a
  thrown exception.
- **Pure by default:** helpers return new values and leave their inputs
  unchanged; the few with effects are listed under Global Side Effects.
- **Console behind an interface:** terminal IO goes through `Terminal`, so a
  test or another front end can replace the console.

## Dependencies

No project references. Package references in `Generic.csproj`:
`LanguageExt.Core` (for `Option<T>`) and `UnitsNet`.

## Dependency Rules

Do not add Mayday, Ellie, or hardware project references here.

## Global Invariants

- Utilities should not create robot hardware or own robot lifecycle.
- `Generic` references no other project in the solution (`Generic.csproj`).

## Global Side Effects

- `AppendElement` mutates the dictionary passed in.
- `ForEach` runs the caller's action on each element, with whatever effects
  that action has.
- `NextDoubleNeg1ToPos1` advances the state of the `Random` passed in.
- `ExceptionHelper.ThrowExceptionOnAnyKey` writes to the console, blocks on
  `Console.ReadKey`, and always throws.
- `SystemTerminal` reads standard input and writes standard output
  (see [System](./System/README.md)).

## Architectural Constraints

Prefer explicit dependencies over global system access in domain code.

## Common Pitfalls

- Do not put robot-specific behavior in generic extension methods.
- `ExceptionHelper.ThrowExceptionOnAnyKey` blocks on a key press; do not call it
  from headless code or tests.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.Generic"`
  (covers `UnitsNetExtensions`; other helpers are exercised through their
  callers in `dotnet test Test\Test.csproj`).

## Related ADRs

[Stable abstractions](../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

None recorded.

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
