import os
from math import tau
from typing import List

import xacro

from urdf_parser_py.urdf import URDF

from motor import DxlMotor
from motor_state import MotorState


class Leg(object):
    # TODO test that there are always 3 motors. Or should it be dynamic?
    def __init__(self, joints: List[DxlMotor]):
        self.joints = joints

    def set_pose(self, pose):
        for joint, position in zip(self.joints, pose):
            joint.set_goal_position(position)


class LegFactory(object):
    def create_basic(self, base_id, adapter) -> Leg:
        N_JOINTS = 3
        joints = []
        for joint_num in range(N_JOINTS):
            id_num = base_id * 3 + joint_num + 1
            joints.append(DxlMotor(id_num, adapter, MotorState()))

        return Leg(joints)


class MaydayRobot:
    def __init__(self, legs):
        self.legs = legs
        self.description = None

        self.read_description()

    def read_description(self):
        description_xacro_path = os.path.join(os.path.dirname(__file__), 'mayday.urdf.xacro')
        description_urdf = xacro.process(description_xacro_path)
        self.description = URDF.from_xml_string(description_urdf)

    def set_start_position(self):
        LEG_START_POSE = (0, tau * 3 / 8, -tau * 3 / 8)
        for leg in self.legs:
            leg.set_pose(LEG_START_POSE)

    def set_standing_position(self):
        LEG_STANDING_POSE = (0, tau / 4, -tau * 3 / 8)
        for leg in self.legs:
            leg.set_pose(LEG_STANDING_POSE)


class MaydayRobotFactory(object):
    def __init__(self, leg_factory: LegFactory):
        self.leg_factory = leg_factory

    def from_urdf(self, urdf):
        raise NotImplementedError()

    def create_basic(self, adapter):
        N_LEGS = 6
        LEFT_SIDE_LEG_NUMBERS = [0, 1, 2]
        DRIVE_MODE_LEFT = 0
        DRIVE_MODE_RIGHT = 1

        legs = []
        for leg_num in range(N_LEGS):
            leg = self.leg_factory.create_basic(leg_num, adapter)
            if leg_num in LEFT_SIDE_LEG_NUMBERS:
                drive_mode = DRIVE_MODE_LEFT
            else:
                drive_mode = DRIVE_MODE_RIGHT
            leg.joints[0].set_drive_mode(drive_mode)

            legs.append(leg)

        return MaydayRobot(legs)


# TODO build MaydayRobot from urdf description