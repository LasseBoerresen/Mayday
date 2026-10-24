# Domain language

Shared robotics and geometry terms, not specific to one robot.

## Geometry

**Xyz**:
A position or offset in space, with a length unit. It has no maximum magnitude; any spatial coordinate is valid.

**Q**:
A rotation quaternion. It describes an orientation change and is never used to hold a position. Its components stay close to the unit range.

**Rpy**:
Roll, pitch and yaw angles describing a rotation; convertible to a **Q**.

**Transform**:
A position (**Xyz**) together with a rotation (**Q**), composed as if applied one after the other.

## Verification

**Connected**:
The robot's serial adapter is present and its port opens. A fact the software checks; it says nothing about whether the robot may move.

**Clear-to-move**:
The operator's standing statement that the connected robot may move under test: placed safely, powered, nothing in its reach. Only the operator gives it; software never sets it.

**Quarantined test**:
A test known to fail, marked with the cause of its failure, still run so its recovery shows. Quarantine never changes what the test expects.
