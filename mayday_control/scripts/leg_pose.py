class LegPose(tuple):
    def __new__(cls, iterable):
        if len(iterable) != 3:
            raise ValueError(f'joint_angles must have 3 values, got: {iterable}')
        return super().__new__(cls, iterable)
