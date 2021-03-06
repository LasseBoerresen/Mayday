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
    # TODO maybe base id should be starting id, and then joint 0 just has that as dxl_id
    def create_basic(self, base_id, adapter, side) -> Leg:
        N_JOINTS = 3
        joints = []
        for joint_num in range(N_JOINTS):
            id_num = base_id * 3 + joint_num + 1
            if joint_num == 0:
                drive_mode = 'forward' if side == 'left' else 'backward'
            elif joint_num == 1:
                drive_mode = 'backward'
            elif joint_num == 2:
                drive_mode = 'forward'
            joints.append(DxlMotor(id_num, adapter, MotorState(), drive_mode))

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

    def set_all_legs_to_pose(self, pose):
        for leg in self.legs:
            leg.set_pose(pose)

    def set_neutral_position(self):
        self.set_all_legs_to_pose((0, 0, 0))

    def set_start_position(self):
        self.set_all_legs_to_pose((0, tau * 0.3, -tau * 0.2))

    def set_standing_position(self):
        self.set_all_legs_to_pose((0, tau * 0.2, -tau * 0.25))

    def set_standing_wide_position(self):
        self.set_all_legs_to_pose((0, tau * 0.1, -tau * 0.1))


class MaydayRobotFactory(object):
    def __init__(self, leg_factory: LegFactory):
        self.leg_factory = leg_factory

    def from_urdf(self, urdf):
        raise NotImplementedError()

    def create_basic(self, adapter):
        N_LEGS = 6
        LEFT_SIDE_LEG_NUMBERS = [0, 1, 2]

        legs = []
        for leg_num in range(N_LEGS):
            side = 'left' if leg_num in LEFT_SIDE_LEG_NUMBERS else 'right'
            leg = self.leg_factory.create_basic(leg_num, adapter, side)
            legs.append(leg)

        return MaydayRobot(legs)

    # TODO build MaydayRobot from urdf description
