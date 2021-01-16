import json
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
    #@pytest.mark.skip(reason='not ready for automatic run')
    def test_given_mayday__when_calling_set_start_position__then_all_legs_reach_start_position(self):
        may = main.create_mayday()
        may.set_legs_to_start_position()
        # may.set_standing_wide_position()
        # may.set_standing_position()
        # may.set_standing_wide_position()
        # may.set_start_position()

        for leg in may.legs:
            assert leg.get_joint_positions() == (0, tau * 0.3, -tau * 0.2)

    # TODO test that no motor is in error state when starting.


class TestMainAcceptance:
    """
    Tests run underlying code in a test environment, thus, should not run real robot, unless it specifically tests the
    physical robot. Instead, the robot should be mocked out, and main should simply be tested agains what we expect
    main functionality to do, like create a robot object, initialize and start autonomy function.
    """
    def test_calls_set_start_and_stand_position(self):
        mock_mayday = create_autospec(MaydayRobot)
        with patch('main.create_mayday', return_value=mock_mayday) as mock_create_mayday:
            main.main()

        assert [call.set_legs_to_start_position(), call.set_legs_to_standing_position()] == mock_mayday.method_calls


if __name__ == '__main__':
    pytest.main()
