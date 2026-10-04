# Mayday domain language

## Geometry

**Xyz**:
A position or offset in space, with a length unit. It has no maximum magnitude; any spatial coordinate is valid.

**Q**:
A rotation quaternion. It describes an orientation change and is never used to hold a position. Its components stay close to the unit range.

**Rpy**:
Roll, pitch and yaw angles describing a rotation; convertible to a **Q**.

**Transform**:
A position (**Xyz**) together with a rotation (**Q**), composed as if applied one after the other.
