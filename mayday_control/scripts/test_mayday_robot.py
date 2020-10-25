from math import tau
from typing import List
from unittest.mock import MagicMock, call, create_autospec

import pytest

from dynamixel_adapter import DynamixelAdapter
from mayday_robot import *
from motor import DxlMotor
from motor_state import MotorState


class TestInit:
    def test_when_set_start_position__then_calls_set_pose_on_all_legs(self):
        mock_leg_factory = create_autospec(LegFactory)
        mayday_factory = MaydayRobotFactory(mock_leg_factory)
        may = mayday_factory.create_basic(MagicMock())

        may.set_start_position()

        for leg in may.legs:
            assert call.set_pose((0, tau * 3 / 8, -tau * 3 / 8)) in leg.method_calls

    def test_when_set_standing_position__then_calls_set_pose_on_all_legs(self):
        mock_leg_factory = create_autospec(LegFactory)
        mayday_factory = MaydayRobotFactory(mock_leg_factory)
        may = mayday_factory.create_basic(MagicMock())

        may.set_standing_position()

        for leg in may.legs:
            assert call.set_pose((0, tau / 4, -tau * 3 / 8)) in leg.method_calls

    def test_when_set_start_position__then_waits_for_goal_position_reached(self):
        raise NotImplementedError()

    @pytest.mark.skip('not_implemented')
    def test_when_set_start_position__then_start_up_torque_not_exceeded(self):
        raise NotImplementedError()


class TestLeg:
    def test_given_pose__when_set_pose__then_calls_set_goal_position_on_all_joints(self):
        pose = (2, 3, 5)

        leg = Leg(joints=[
            create_autospec(DxlMotor),
            create_autospec(DxlMotor),
            create_autospec(DxlMotor)])

        leg.set_pose(pose)

        for joint, position in zip(leg.joints, pose):
            assert call.set_goal_position(position) in joint.method_calls


class TestLegFactory:
    def test_given_base_id_0__then_joint_0_has_id_1(self):
        base_id = 0
        leg_factory = LegFactory()
        leg = leg_factory.create_basic(base_id, MagicMock())
        assert leg.joints[0].id == 1

    def test_given_base_id_1__then_joint_0_has_id_4(self):
        base_id = 1
        leg_factory = LegFactory()
        leg = leg_factory.create_basic(base_id, MagicMock())
        assert leg.joints[0].id == 4

    def test_given_base_id_5__then_joint_2_has_id_18(self):
        base_id = 5
        leg_factory = LegFactory()
        leg = leg_factory.create_basic(base_id, MagicMock())
        assert leg.joints[2].id == 18

    def test_given_same_adapter__all_joins_point_to_that(self):
        mock_adapter = MagicMock()
        factory = LegFactory()
        leg = factory.create_basic(3, mock_adapter)

        for joint in leg.joints:
            assert joint.adapter == mock_adapter


@pytest.fixture()
def mayday_factory():
    leg_factory = LegFactory()
    mayday_factory = MaydayRobotFactory(leg_factory)
    return mayday_factory


class TestMaydayRobotFactory:

    def test_when_calling_create_basic__then_has_6_legs(self, mayday_factory):
        mock_adapter = MagicMock()

        may = mayday_factory.create_basic(mock_adapter)

        assert len(may.legs) == 6

    def test_when_calling_create_basic__then_sets_right_side_coxa_motor_mirrored(self, mayday_factory):
        mock_adapter = MagicMock()

        may = mayday_factory.create_basic(mock_adapter)

        left_side_leg_numbers = [0, 1, 2]
        for leg_num, leg in enumerate(may.legs):
            if leg_num in left_side_leg_numbers:
                expected_drive_mode = 0
            else:
                expected_drive_mode = 1
            assert call.write_drive_mode(leg.joints[0].id, expected_drive_mode) in mock_adapter.method_calls

    # TODO when creating a mayday robot, motors should be initialized. But should that happen on construction? Also
    #  known as __init__ aka initialization. That means, I cannot create a mayday object without having a connected
    #  dynamixel controller and actual motors... This seems difficult to work with.

    # def test_given_xacro__when_calling_from_xacro__then_(self):
    #     raise NotImplementedError()

    # def test_when_init__then_description_is_set(self):
    #     may = MaydayRobot(None)
    #     assert may.description is not None
    #
    # def test_when_init__then_description_contains_joints(self):
    #     may = MaydayRobot(None)
    #     assert len(may.description.joints) > 0


if __name__ == '__main__':
    pytest.main()
