from math import tau


class AngularVelocity(float):
    unit = 'radians/s'

    @classmethod
    def from_rpm(cls, val):
        seconds_per_minute = 60.0
        factor_rpm_to_rad_per_sec = tau / seconds_per_minute
        return cls(val * factor_rpm_to_rad_per_sec)
