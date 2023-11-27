from __future__ import annotations
from dataclasses import dataclass


@dataclass
class Pose:
    x: float
    y: float
    z: float
    R: float
    P: float
    Y: float

    @classmethod
    def zeros(cls) -> Pose:
        return Pose(0, 0, 0, 0, 0, 0)
