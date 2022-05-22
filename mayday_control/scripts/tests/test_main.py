import json
import time
from math import tau
from os.path import dirname, join
from unittest.mock import create_autospec, call, patch, MagicMock

import pytest

import main
from mayday_robot import MaydayRobot
from leg import Leg

with open(join(dirname(dirname(__file__)), 'mayday_config.json'), 'r') as f:
    config = json.load(f)


@pytest.mark.skipif(not config['robot_is_connected'], reason='Robot is not connected')
class TestMainEndToEndIntegration:
    """
    Don't mock anything, literally turn on Mayday and test basic maneuvers, like, start up and lift leg.
    """
    def test_given_mayday_in_starting_pos__when_calling_set_standing_position__then_all_legs_reach_that_position(self):
        # Given
        may = main.create_mayday()

        may.set_joint_positions_for_all_legs(Leg.POSE_STARTING)
        time.sleep(5)

        goal_positions = Leg.POSE_STARTING
        self.assert_goal_positions_are_reached(goal_positions, may)

        # When
        may.set_joint_positions_for_all_legs(Leg.POSE_STANDING)
        time.sleep(5)

        # Then
        goal_positions = Leg.POSE_STANDING
        self.assert_goal_positions_are_reached(goal_positions, may)

    def assert_goal_positions_are_reached(self, goal_positions, may):
        for leg in may.legs:
            for joint_pos, goal in zip(leg.get_joint_positions(), goal_positions):
                assert abs(goal - joint_pos) < tau / 100

    # TODO test that no motor is in error state when starting.
    # TODO test that motors does not error just because they cannot reach their goal
    #  or continously use current
    # TODO test that when starting robot, errors have been reset


class TestMainAcceptance:
    """
    Tests run underlying code in a test environment, thus, should not run real robot, unless it
    specifically tests the physical robot. Instead, the robot should be mocked out, and main should
    simply be tested against what we expect main functionality to do, like create a robot object,
    initialize and start autonomy function.
    """
    def setup(self):
        self.mock_mayday: MagicMock = create_autospec(MaydayRobot)

    def test__calls_disable_torque(self):
        # When
        with patch('main.create_mayday', return_value=self.mock_mayday):
            main.main()

        # Then
        self.mock_mayday.disable_torque.assert_called()

    def test_calls_set_start_pose_and_then_standing_pose(self):
        # When
        with patch('main.create_mayday', return_value=self.mock_mayday):
            main.main()

        # Then
        expected_method = self.mock_mayday.set_joint_positions_for_all_legs
        expected_method.assert_any_call(Leg.POSE_STARTING)

    def test_calls_set_stand_position(self):
        # When
        with patch('main.create_mayday', return_value=self.mock_mayday):
            main.main()

        # Then
        expected_method = self.mock_mayday.set_joint_positions_for_all_legs
        expected_method.assert_called_with(Leg.POSE_STANDING)  # read called last with


    # TODO test calls loop for waiting to reach this position.


if __name__ == '__main__':
    pytest.main()
