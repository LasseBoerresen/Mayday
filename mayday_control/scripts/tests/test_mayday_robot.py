from math import tau
from unittest.mock import call, create_autospec

import pytest

from leg_factory import LegFactory
from mayday_robot import *
from mayday_robot_factory import MaydayRobotFactory


class TestInit:
    def test_when_set_start_position__then_calls_set_pose_on_all_legs(self):
        mock_leg_factory = create_autospec(LegFactory)
        mock_dynamixel_adapter = create_autospec(DynamixelAdapter)
        mayday_factory = MaydayRobotFactory(mock_leg_factory, mock_dynamixel_adapter)
        may = mayday_factory.create_basic()

        may.set_joint_positions_for_all_legs(Leg.POSE_STARTING)

        for leg in may.legs:
            assert call.set_joint_positions(LegPose((0, tau * 0.3, -tau * 0.2))) in leg.method_calls

    def test_when_set_standing_position__then_calls_set_pose_on_all_legs(self):
        mock_leg_factory = create_autospec(LegFactory)
        mock_dynamixel_adapter = create_autospec(DynamixelAdapter)
        mayday_factory = MaydayRobotFactory(mock_leg_factory, mock_dynamixel_adapter)
        may = mayday_factory.create_basic()

        may.set_joint_positions_for_all_legs(Leg.POSE_STANDING)

        for leg in may.legs:
            assert call.set_joint_positions(LegPose((0, tau * 0.2, -tau * 0.25))) in leg.method_calls

    @pytest.mark.skip('not implemented')
    def test_when_set_start_position__then_waits_for_goal_position_reached(self):
        raise NotImplementedError()

    @pytest.mark.skip('not_implemented')
    def test_when_set_start_position__then_start_up_torque_not_exceeded(self):
        raise NotImplementedError()


if __name__ == '__main__':
    pytest.main()
