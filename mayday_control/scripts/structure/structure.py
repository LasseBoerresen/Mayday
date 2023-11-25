from geometry.pose import Pose


class Structure:
    def get_thorax_position(self) -> Pose:
        return Pose(0.0, 0.0, 0.0, 0.0, 0.0, 0.0)
        # TODO fix cheat value
