import json
import time
from math import tau
from unittest.mock import create_autospec, call, patch, MagicMock

import pytest

import main
from mayday_robot import MaydayRobot

with open('mayday_config.json', 'r') as f:
    config = json.load(f)


@pytest.mark.skipif(not config['robot_is_connected'], reason='Robot is not connected')
class TestMainEndToEndIntegration:
    """
    Don't mock anything, literally turn on Mayday and test basic maneuvers, like, start up and lift leg.
    """
    def test_given_mayday_in_neutral_pos__when_calling_set_start_position__then_all_legs_reach_start_position(self):
        # Given
        may = main.create_mayday()

        may.set_legs_to_neutral_position()
        time.sleep(5)

        goal_positions = (0, 0, 0)
        self.assert_goal_positions_are_reached(goal_positions, may)

        # When
        may.set_legs_to_start_position()
        time.sleep(5)

        # Then
        goal_positions = (0, tau * 0.3, -tau * 0.2)
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
    def test_calls_set_start_and_stand_position(self):
        mock_mayday = create_autospec(MaydayRobot)
        with patch('main.create_mayday', return_value=mock_mayday) as mock_create_mayday:
            main.main()

        assert [call.set_legs_to_start_position(), call.set_legs_to_standing_position()] == mock_mayday.method_calls


if __name__ == '__main__':
    pytest.main()
