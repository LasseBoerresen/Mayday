from math import tau
from typing import List
from unittest.mock import MagicMock, call, create_autospec, patch

import pytest

from dynamixel_adapter import DynamixelAdapter
from mayday_robot import *
from motor import DxlMotor
from motor_state import MotorState


class TestInit:
    def test_when_set_start_position__then_calls_set_pose_on_all_legs(self):
        mock_leg_factory = create_autospec(LegFactory)
        mayday_factory = MaydayRobotFactory(mock_leg_factory, adapter=MagicMock())
        may = mayday_factory.create_basic()

        may.set_joint_positions_for_all_legs(Leg.STARTING_POSE)

        for leg in may.legs:
            assert call.set_joint_positions(LegPose((0, tau * 0.3, -tau * 0.2))) in leg.method_calls

    def test_when_set_standing_position__then_calls_set_pose_on_all_legs(self):
        mock_leg_factory = create_autospec(LegFactory)
        mayday_factory = MaydayRobotFactory(mock_leg_factory, adapter=MagicMock())
        may = mayday_factory.create_basic()

        may.set_joint_positions_for_all_legs(Leg.STANDING_POSE)

        for leg in may.legs:
            assert call.set_joint_positions(LegPose((0, tau * 0.2, -tau * 0.25))) in leg.method_calls

    @pytest.mark.skip('not implemented')
    def test_when_set_start_position__then_waits_for_goal_position_reached(self):
        raise NotImplementedError()

    @pytest.mark.skip('not_implemented')
    def test_when_set_start_position__then_start_up_torque_not_exceeded(self):
        raise NotImplementedError()


class TestLegFactory:
    def test_given_base_id_0__then_joint_0_has_id_1(self):
        base_id = 0
        leg_factory = LegFactory()
        leg = leg_factory.create_basic(base_id, MagicMock(), None)
        assert leg.joints[0].id == 1

    def test_given_base_id_1__then_joint_0_has_id_4(self):
        base_id = 1
        leg_factory = LegFactory()
        leg = leg_factory.create_basic(base_id, MagicMock(), None)
        assert leg.joints[0].id == 4

    def test_given_base_id_5__then_joint_2_has_id_18(self):
        base_id = 5
        leg_factory = LegFactory()
        leg = leg_factory.create_basic(base_id, MagicMock(), None)
        assert leg.joints[2].id == 18

    def test_given_same_adapter__all_joins_point_to_that(self):
        mock_adapter = MagicMock()
        factory = LegFactory()
        leg = factory.create_basic(3, mock_adapter, None)

        for joint in leg.joints:
            assert joint.adapter == mock_adapter

    def test_given_side_right__then_sets_joint_0_as_drive_mode_backward(self):
        base_id = 5
        side = 'right'
        leg_factory = LegFactory()
        mock_adapter = create_autospec(DynamixelAdapter)

        leg = leg_factory.create_basic(base_id, mock_adapter, side)

        assert leg.joints[0].drive_mode == 'backward'

    def test_given_side_left__then_sets_joint_0_as_drive_mode_forward(self):
        base_id = 1
        side = 'left'
        leg_factory = LegFactory()
        mock_adapter = create_autospec(DynamixelAdapter)

        leg = leg_factory.create_basic(base_id, mock_adapter, side)

        assert leg.joints[0].drive_mode == 'forward'

    def test_given_side_right__then_sets_joint_1_as_drive_mode_backward(self):
        base_id = 5
        side = 'right'
        leg_factory = LegFactory()
        mock_adapter = create_autospec(DynamixelAdapter)

        leg = leg_factory.create_basic(base_id, mock_adapter, side)

        assert leg.joints[1].drive_mode == 'backward'

    def test_given_side_right__then_sets_joint_2_as_drive_mode_forward(self):
        base_id = 5
        side = 'right'
        leg_factory = LegFactory()
        mock_adapter = create_autospec(DynamixelAdapter)

        leg = leg_factory.create_basic(base_id, mock_adapter, side)

        assert leg.joints[2].drive_mode == 'forward'

    def test_given_side_left__then_sets_joint_2_as_drive_mode_forward(self):
        base_id = 2
        side = 'left'
        leg_factory = LegFactory()
        mock_adapter = create_autospec(DynamixelAdapter)

        leg = leg_factory.create_basic(base_id, mock_adapter, side)

        assert leg.joints[2].drive_mode == 'forward'

@pytest.fixture()
def mayday_factory():
    leg_factory = LegFactory()
    mayday_factory = MaydayRobotFactory(leg_factory, adapter=MagicMock())
    return mayday_factory


class TestMaydayRobotFactory:

    def test_when_create_basic__then_has_6_legs(self, mayday_factory):
        may = mayday_factory.create_basic()

        assert len(may.legs) == 6

    def test_when_calling_create_basic__then_sets_last_3_legs_side_to_right(self):
        mock_leg_factory = create_autospec(LegFactory)
        mock_adapter = MagicMock()
        mayday_factory = MaydayRobotFactory(mock_leg_factory, adapter=mock_adapter)

        may = mayday_factory.create_basic()

        left_side_leg_numbers = [0, 1, 2]
        for leg_num, leg in enumerate(may.legs):
            if leg_num in left_side_leg_numbers:
                expected_side = 'left'
            else:
                expected_side = 'right'

            assert call.create_basic(leg_num, mock_adapter, expected_side) in mock_leg_factory.method_calls
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
