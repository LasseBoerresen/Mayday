# Ellie

Ellie is a separate robot: articulated steering and wheel-driven motion. Read [README.md](README.md) and [EllieMain/README.md](EllieMain/README.md).

- `EllieMain` references only `RobotDomain` and `Dynamixel`. Share foundational contracts; keep `MaydayDomain` and Mayday structure out of Ellie.
- `EllieMain/Program.cs` initializes a real communication bus, so running `EllieMain` drives hardware. Verify through `EllieMainTests` instead.
- `DefaultEllieStructure.MoveAt`/`GetState` and parts of the planner throw `NotImplementedException`. Treat them as unimplemented in code and docs.
- Tests: `dotnet test Ellie\EllieMainTests\EllieMainTests.csproj`; end-to-end tests live in `EllieMainTests/EndToEnd/`.
