# Robots integration tests

## Overview

Groups integration test helpers for [Robots](../../../Robots/README.md). It
holds no tests and no direct source files; the tests that start a
`MaydayRobot` live in [Main](../Main/README.md), namespace
`Test.Integration.Main`.

## Subdirectories & Boundaries

- Base: [./Base/README.md](./Base/README.md) - `TestObjectMother.MaydayRobotFactory`,
  the real `MaydayRobotFactory` with the neutral map and a fail-fast handler;
  its `Create...` methods touch the serial adapter.
