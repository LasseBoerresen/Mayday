# Unit test base

## Overview

The marker interface for test object mothers (namespace `Test.Unit.Base`).
It holds no tests and tests no production code.

## Interface

### `TestObjectMother`

Marks a class as a source of commonly used setup objects for tests. It has no
members.
File: `TestObjectMother.cs`.

`Test.Integration.MaydayDomain.Base.TestObjectMother` is the only class that
implements it. Other classes named `TestObjectMother`, such as
`Test.Integration.Robots.Base.TestObjectMother`, do not.

## Invariants

- The interface has no members, so implementing it adds no obligation.

## Side Effects

- None.

## Verification

- No tests. It is compiled with the rest of the project:
  `dotnet build Test\Test.csproj`.
