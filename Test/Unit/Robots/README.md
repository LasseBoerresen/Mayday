# Unit tests of Robots

## Overview

Groups the unit tests of the [Robots](../../../Robots/README.md) project, the
Mayday robot assembly and factories. It holds no code itself; its one
subdirectory covers `MaydayRobotFactory`.

## Subdirectories & Boundaries

- Mayday: [Mayday/README.md](./Mayday/README.md) - `MaydayDayRobotFactoryTests`,
  which builds the effect of `MaydayRobotFactory.CreateWithSwayBehavior`
  without running it. Building it already opens the FTDI adapter when one is
  plugged in.

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.Robots.Mayday"`.
  No `Tier=Physical` or `Tier=Manual` tests are included, but the test is not
  hardware-free; see [Mayday/README.md](./Mayday/README.md).
