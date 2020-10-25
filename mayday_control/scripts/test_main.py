import json
from unittest.mock import create_autospec, call, patch, MagicMock

import pytest

import main
from mayday_robot import MaydayRobot

with open('mayday_config.json', 'r') as f:
    config = json.load(f)


class TestMainEndToEndIntegration:
    """
    Don't mock anything, literaly turn on Mayday and test basic maneuvers, like, start up and lift leg.
    """
    pass


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

        assert [call.set_start_position(), call.set_standing_position()] == mock_mayday.method_calls


if __name__ == '__main__':
    pytest.main()
