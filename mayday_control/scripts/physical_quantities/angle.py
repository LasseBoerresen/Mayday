from __future__ import annotations

from math import tau


class Angle(float):
    unit = 'radian'

    @classmethod
    def from_deg(cls, value: float) -> Angle:
        return cls(value * tau / 360)
