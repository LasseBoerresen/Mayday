from unittest.mock import MagicMock, call, create_autospec

import pytest

from dxl_motor import *
from motor_state import MotorState


class TestInitMotorConfig:
    def test_when_init__then_calls_adapter_init_single(self):
        id_num = 4
        drive_mode = DriveMode.FORWARD
        mock_adapter = create_autospec(DynamixelAdapter)

        DxlMotor(id_num, mock_adapter, drive_mode)

        assert call.init_single(id_num, drive_mode) in mock_adapter.method_calls


class TestGetStateAcceptanceTestCase:
    def test_given_adapter_returns_state__then_motor_has_new_state(self):
        dxl_id = 13
        state_new = MotorState(position=3, velocity=5, torque=7, temperature=11, position_goal=13)
        mock_dxl_adapter = MagicMock()
        mock_dxl_adapter.read_state = MagicMock(return_value=state_new)
        motor = DxlMotor(dxl_id, mock_dxl_adapter, DriveMode.FORWARD)

        assert state_new == motor.state

    # TODO test write commands
    # TODO test write configurations


class TestSetGoalPosition:
    def test_given_position__when_set_goal_position__then_calls_adapter_write_goal_pos(self):
        position = 3
        idd = 5
        mock_dxl_adapter = create_autospec(DynamixelAdapter)
        motor = DxlMotor(idd, mock_dxl_adapter, DriveMode.FORWARD)

        motor.set_goal_position(position)

        assert call.write_goal_position(motor.id, position) in mock_dxl_adapter.method_calls


class TestGetPosition:
    def test_given_adapter_returns_state_with_pos_4__when_get_state__then_returns_4(self):
        id = 5
        position = 4
        state = MotorState(position)
        mock_adapter = create_autospec(DynamixelAdapter)
        mock_adapter.read_state = MagicMock(return_value=state)
        motor = DxlMotor(id, mock_adapter, DriveMode.FORWARD)

        actual = motor.state.position

        assert actual == position


if __name__ == '__main__':
    pytest.main()


