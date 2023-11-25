from dataclasses import dataclass


@dataclass
class Pose:
    x: float
    y: float
    z: float
    r: float
    p: float
    j: float
