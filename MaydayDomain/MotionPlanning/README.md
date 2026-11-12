# Mayday motion planning

## Overview

Decides how Mayday executes a motion (namespace
`MaydayDomain.MotionPlanning`). Behavior chooses what the robot should do, for
example move forward, as a goal `Motion`; motion planning turns that goal into
commands on a `MaydayStructure`.

The folder also holds a neural-network approach to inverse leg kinematics. It
is unfinished and not a priority (see Limitations and planned work).

## Responsibility

Plan hexapod movements and evaluate/learn inverse leg kinematics.

## Public APIs

`MaydayMotionPlanner`, `TrackingMaydayMotionPlanner`, `StepByStepLearningMaydayMotionPlanner`, inverse-kinematics input/output/evaluators.

## Interface

All types are in namespace `MaydayDomain.MotionPlanning`. Each heading below
is one type; the line under it says what the type is for.

### Motion and stance

#### `Motion`

What the robot should do. Record. File: `Motion.cs`.

| Member | Meaning |
|---|---|
| `Twist` | Movement away from the current spot. |
| `Lean` | The pose of the thorax relative to the ground. |
| `StanceRadius` | The radius of the circle of footprints, measured from the thorax origin. |
| `StandingStill` | A ready-made motion: no twist, the thorax 0.08 m up, a stance radius of 0.22 m. The two values were chosen by trial on the robot. |

#### `Stance`

Where the tips go for a motion's lean and stance radius. Static, pure
geometry; it ignores the twist. File: `Stance.cs`.

| Method | What it returns |
|---|---|
| `GetFootprintInGroundFrameFor(legId, stanceRadius)` | Where the leg stands on the ground: the point at `stanceRadius` along the X axis of the leg's thorax mount, with Z set to zero. |
| `GetTipPositionInThoraxFrameFor(legId, motion)` | That footprint seen from the leaned thorax: the footprint minus `motion.Lean`. |
| `GetTipPositionsInThoraxFrameFor(motion)` | The same for all six legs. |

Only `GetTipPositionsInThoraxFrameFor` is called by production code
(`DefaultMaydayStructure.SetStance`). The other two are public for the tests.

### Planners

#### `MaydayMotionPlanner`

The interface behaviors use to move Mayday. It extends
`RobotDomain.Motion.MotionPlanner<Motion>`, which brings `Goal` and `Start`.
File: `MaydayMotionPlanner.cs`.

| Member | What it does |
|---|---|
| `GetPostures()`, `GetPostureOf(legId)` | The current posture of every leg, or of one. |
| `GetPositionsOf(linkName)`, `GetPositionOf(linkName, legId)` | The position of a link, for every leg or for one. |
| `GetOrientationsOf(linkName)`, `GetTransformsOf(linkName)` | The orientation or full transform of a link, for every leg. |
| `SetPosture(posture)` | Sets a posture per leg, or one posture for all legs. |
| `SetTipPositionsForLegs(tipPositions)` | Moves each tip to a position. |
| `MoveTipPositions(tipDeltas)` | Moves each tip by an offset. |

#### `TrackingMaydayMotionPlanner`

The working planner: it keeps placing the tips for the goal's lean and stance
radius.
Constructed from a `MaydayStructure`, a `TimeProvider` and a
`FatalErrorHandler`. File: `TrackingMaydayMotionPlanner.cs`.

| Member | What it does |
|---|---|
| `Goal` | The motion to reach. Setting it replaces `Plan` with a `LinearStepsMotionPlan<Motion>` holding that one goal, or with `None`. |
| `Plan` | The current plan. Read-only from outside. |
| `Start(cancellationToken)` | Creates a `PeriodicScheduler`. Every 0.2 s until cancelled, when a plan exists, it calls `Structure.SetStance` with the plan's target for the current time. |
| `SetTipPositionsForLegs` | Calls `Structure.MoveTipsTo`. |
| `SetPosture(legPosture)` | Applies the posture to all six legs. |
| `MoveTipPositions` | Throws `NotSupportedException`. Virtual. A gap to fill, not a design choice. |
| `GetPosture(legId)` | Same as `GetPostureOf`. Not on the interface. A leftover, to be removed. |
| Everything else | Forwards to the structure. |

#### `StepByStepLearningMaydayMotionPlanner`

An experiment: a `TrackingMaydayMotionPlanner` that moves tips with postures
predicted by a neural network. Takes an `InverseLegKinematicsNeuralNetwork` in
addition. File: `StepByStepLearningMaydayMotionPlanner.cs`.

`MoveTipPositions` is its one override. For each leg it:

1. Builds an `InverseLegKinematicsInput` from the current tip position, the
   requested offset and the current posture.
2. Asks the network to `Predict` a posture.
3. Sets the predicted postures on the structure and sleeps 1 s.
4. Reads the tip positions back and computes the errors.

The errors and data points are then discarded: the call to `Train` is
commented out.

### Inverse leg kinematics data

The data types of the neural-network approach.

#### `InverseLegKinematicsInput`

What the network is given: 12 `float`s. Record.
File: `InverseLegKinematicsInput.cs`.

| Fields | Content |
|---|---|
| `EndX`, `EndY`, `EndZ` | Where the tip should end up. |
| `StartX`, `StartY`, `StartZ` | Where the tip is. |
| `StartCoxaSin`, `StartFemurSin`, `StartTibiaSin` | Sines of the current joint angles. |
| `StartCoxaCos`, `StartFemurCos`, `StartTibiaCos` | Cosines of the current joint angles. |

| Member | What it does |
|---|---|
| `Create(endXyz, startXyz, startPosture)` | Builds an input: positions in meters, angles as sine and cosine. |
| `EndXyz`, `StartPosture` | The end position and start posture back as domain types. |
| `ToNdArray` | A 1 × 12 array (a batch of one), in `InputColumnNames` order. |
| `InputColumnNames`, `Shape` | The field names in order, and the shape (12). |

#### `InverseLegKinematicsOutput`

What the network returns: a posture as six `float`s. Class.
File: `InverseLegKinematicsOutput.cs`.

| Member | What it does |
|---|---|
| `CoxaSin`, `CoxaCos`, `FemurSin`, `FemurCos`, `TibiaSin`, `TibiaCos` | Sine and cosine of each joint angle. |
| `Create(posture)` | An output from a posture. |
| `ToPosture()` | The posture back, through `MaydayLegPosture.FromSines`. |
| `FromArray(values)` | An output from elements 0 to 5 of an array, in `OutputColumnNames` order. |
| `OutputColumnNames`, `Length` | The property names in order, and their count (6). |

#### Small records and interfaces

| Type | What it is | File |
|---|---|---|
| `InverseLegKinematicsError(double Value)` | How far a tip ended from where it should be. `From(expected, actual)` gives the distance in meters. | `InverseLegKinematicsError.cs` |
| `InverseLegKinematicsDataPoint(Input, Output, Error)` | One prediction with its error. | `InverseLegKinematicsDataPoint.cs` |
| `PositionLoss(double Value)` | A loss value. For the planned training. | `PositionLoss.cs` |
| `InverseLegKinematicsEvaluator` | Interface: `Evaluate(output)` returns an `InverseLegKinematicsError`. No implementation yet; for the planned training. | `InverseLegKinematicsEvaluator.cs` |
| `LegLossEvaluator` | Interface: `Evaluate(dataPoint)` returns a `PositionLoss`. No implementation yet; for the planned training. | `LegLossEvaluator.cs` |

### Neural network

#### `InverseLegKinematicsNeuralNetwork`

The interface to a network that predicts a posture.
File: `InverseLegKinematicsNeuralNetwork.cs`.

| Member | What it does |
|---|---|
| `Predict(input)` | Returns an `InverseLegKinematicsOutput`. |
| `Train(inputs, evaluator)` | Trains the network on inputs, scored by the evaluator. |

#### `InverseLegKinematicsNeuralNetwortTensorflowNetImpl`

The TensorFlow.NET implementation.
File: `InverseLegKinematicsNeuralNetwortTensorflowNetImpl.cs`.

| Member | What it does |
|---|---|
| Constructor, `Create()` | Builds and compiles a Keras model: 12 inputs, a dense layer of 32 with ReLU, dropout 0.5, a dense output of 6. Adam with learning rate 1e-3, mean squared error. |
| `Predict(input)` | Runs the model on `input.ToNdArray` and converts the first result row with `InverseLegKinematicsOutput.FromArray`. |
| `Train(inputs, evaluator)` | Throws `NotImplementedException`. |

## Key Concepts

Timed motion, stance, position loss, leg targets, learned kinematics.

## Dependencies

Mayday leg/structure types, `RobotDomain` motion/time, ML packages via `MaydayDomain`.

## Dependency Rules

Behavior chooses goals; planning transforms them without owning native port access.

## Invariants

Goals must respect leg reachability, joint limits, and units. Ensuring this is a core responsibility of motion planning, not of the behavior layer: behavior decides the desired motion, planning decides how to execute it. No code here enforces it yet (see Limitations and planned work).

`Stance` assumes flat ground at z = 0 in the ground frame and measures the
stance radius from the thorax origin, so all six footprints lie on one circle.
Flat ground is a long-standing assumption, not a permanent one; there is no
plan to handle other ground soon.

`TrackingMaydayMotionPlanner` tracks only the lean and stance radius of the
goal. It does not act on `Motion.Twist`, and it hands the structure the plan's
current target without splitting the movement into intermediate steps.

`InverseLegKinematicsInput.ToNdArray` and `InputColumnNames` list the fields in
the same order, and `InverseLegKinematicsOutput.FromArray` reads in
`OutputColumnNames` order. Keep each pair in step.

## Limitations and planned work

Nothing in this section is implemented. Each item is a known limitation, with
the fix that is planned or under consideration.

### Planning

| Limitation today | Planned |
|---|---|
| `Motion.Twist` is ignored: the robot cannot move away from its spot. | Stepping, so a twist is executed. |
| A goal becomes a plan of one step, and the structure receives the final target at once. | Plans that start at the current state and split the movement into steps. |
| `TrackingMaydayMotionPlanner.MoveTipPositions` throws. | Implement it. |
| Reachability and joint limits are not checked; an unreachable goal is passed on to the structure. | Planning enforces them. |
| Planning reports nothing back. | Planning tells behavior what it found, for example that a motion was impossible, so behavior can decide better. |
| `TrackingMaydayMotionPlanner.GetPosture(legId)` duplicates `GetPostureOf`. | Remove it. |

### Naming of stance and lean

To be clarified. `TrackingMaydayMotionPlanner`'s XML doc says it tracks the
thorax lean, while it calls `SetStance`, which applies lean and stance radius.
Both describe the same thing from different sides: it tracks a full goal
motion.

The `Stance` type may be doing too much. A stance, read plainly, is where the
tips are placed in the ground plane. Today `Stance` also bends the legs to
reach a lean. It also exposes three public methods of which production code
calls one, which suggests the abstraction is not yet right.

### Neural-network inverse kinematics

Planned, but not a priority. Analytical inverse kinematics will probably
supersede it; it stays as an interesting approach. `Train` throws, the
learning planner only predicts, and the evaluator and loss types wait for the
training to be written.

Under consideration: a sub-namespace of its own, for example
`NeuralNetworkInverseKinematics`, holding the `InverseLegKinematics*` types,
`LegLossEvaluator`, `PositionLoss` and
`StepByStepLearningMaydayMotionPlanner`.

## Side Effects

- `TrackingMaydayMotionPlanner.Start` starts a periodic task that calls
  `MaydayStructure.SetStance` every 0.2 s while a plan exists. With hardware
  joints this moves the robot.
- `Start` constructs a `PeriodicScheduler`
  ([RobotDomain time](../../RobotDomain/Time/README.md)). The first one
  constructed in a process sets the process priority to High and turns on the
  high-resolution Windows timer. If the tracking action throws, the exception
  goes to the `FatalErrorHandler`; a handler used on a real robot ends the
  process.
- `SetTipPositionsForLegs`, both `SetPosture` overloads and
  `StepByStepLearningMaydayMotionPlanner.MoveTipPositions` set joint goals on
  the structure.
- `StepByStepLearningMaydayMotionPlanner.MoveTipPositions` blocks the calling
  thread for 1 s (`Thread.Sleep`).
- Constructing `InverseLegKinematicsNeuralNetwortTensorflowNetImpl` builds a
  TensorFlow model and calls its `summary()`.

## Architectural Constraints

The enclosing project currently references `Dynamixel`; [trace this exception](../../knowledge/findings/mayday-domain-hardware-reference.md) before restructuring.

## Common Pitfalls

Do not train or drive a real leg as an ordinary unit test.

- The input lists all sines before all cosines (`StartCoxaSin`,
  `StartFemurSin`, `StartTibiaSin`, then the cosines); the output interleaves
  them per joint (`CoxaSin`, `CoxaCos`, ...).
- `Start` constructs its own `PeriodicScheduler`; see
  [Scheduler construction](../../knowledge/findings/scheduler-constructed-inside-domain-classes.md).

## Verification

- Run: `dotnet test Test\Test.csproj --filter "FullyQualifiedName~Test.Unit.MaydayDomain.MotionPlanning"`
  (covers `Stance`).

## Related ADRs

[Stable abstractions](../../docs/adr/0001-stable-robot-abstractions.md).

## Related Findings

[Hardware reference](../../knowledge/findings/mayday-domain-hardware-reference.md), [Scheduler construction](../../knowledge/findings/scheduler-constructed-inside-domain-classes.md).

## Related Root Causes

None recorded.

## Related Lessons Learned

None recorded.

## Related Failed Attempts

None recorded.
