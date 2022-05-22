from math import tau
from typing import List

from leg_pose import LegPose
from dxl_motor import DxlMotor


class Leg(object):
    POSE_NEUTRAL = LegPose((0, 0, 0))
    POSE_STARTING = LegPose((0, tau * 0.3, -tau * 0.2))
    POSE_STANDING = LegPose((0, tau * 0.2, -tau * 0.25))
    POSE_STANDING_WIDE = LegPose((0, tau * 0.1, -tau * 0.1))

    # TODO test that there are always 3 motors. Or should it be dynamic?
    def __init__(self, joints: List[DxlMotor]):
        self.joints = joints

    def set_joint_positions(self, positions: LegPose):
        for joint, position in zip(self.joints, positions):
            joint.set_goal_position(position)

    def get_joint_positions(self) -> LegPose:
        positions = []
        for joint in self.joints:
            positions.append(joint.state.position)

        return LegPose(positions)
