from math import tau


class AngularAcceleration(float):
    unit = 'radians/sec/sec'

    @classmethod
    def from_rpmm(cls, val):
        seconds_per_minute = 60.0
        factor_rpmm_to_rad_per_sec_per_sec = tau / seconds_per_minute ** 2
        return cls(val * factor_rpmm_to_rad_per_sec_per_sec)
