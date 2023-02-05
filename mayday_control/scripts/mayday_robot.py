from dynamixel.dynamixel_adapter import DynamixelAdapter
from leg import Leg
from leg_pose import LegPose


class MaydayRobot:
    def __init__(self, legs: list[Leg], joint_adapter: DynamixelAdapter):
        self.legs = legs
        self.joint_adapter = joint_adapter
        self.description = None

    def set_joint_positions_for_all_legs(self, pose: LegPose):
        for leg in self.legs:
            leg.set_joint_positions(pose)

    def disable_torque(self):
        NotImplementedError()
