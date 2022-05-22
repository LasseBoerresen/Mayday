from typing import List

# from urdf_parser_py.urdf import URDF
from leg import Leg
from leg_pose import LegPose


class MaydayRobot:
    def __init__(self, legs: List[Leg]):
        self.legs = legs
        self.description = None

    def set_joint_positions_for_all_legs(self, pose: LegPose):
        for leg in self.legs:
            leg.set_joint_positions(pose)

    def disable_torque(self):
        NotImplementedError()
