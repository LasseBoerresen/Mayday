from unittest.mock import MagicMock, call, create_autospec

import pytest

from motor import *
from motor_state import MotorState


class TestInitMotorConfig:
    def test_when_create_motor__then_calls_adapter_init_single(self):
        id_num = 4
        mock_adapter = create_autospec(DynamixelAdapter)

        motor = DxlMotor(id_num, mock_adapter, MotorState())

        assert call.init_single(id_num) in mock_adapter.method_calls


class TestGetStateAcceptanceTestCase:
    def test_given_adapter_returns_state__then_motor_has_new_state(self):
        dxl_id = 13
        state_new = MotorState(position=3, velocity=5, torque=7, temperature=11, position_goal=13)
        mock_dxl_adapter = MagicMock()
        mock_dxl_adapter.read_state = MagicMock(return_value=state_new)
        motor = DxlMotor(dxl_id, mock_dxl_adapter, MotorState())

        assert state_new == motor.state

    # TODO test write commands
    # TODO test write configurations


class TestSetGoalPosition:
    def test_given_position__when_set_goal_position__then_calls_adapter_write_goal_pos(self):
        position = 3
        idd = 5
        mock_dxl_adapter = create_autospec(DynamixelAdapter)
        motor = DxlMotor(idd, mock_dxl_adapter, MotorState())

        motor.set_goal_position(position)

        assert call.write_goal_pos(motor.id, position) in mock_dxl_adapter.method_calls


if __name__ == '__main__':
    pytest.main()


