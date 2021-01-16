import json
import time
from math import tau
from unittest.mock import MagicMock, call

import pytest
import pandas as pd
from dynamixel_adapter import DynamixelAdapter
from motor_state import MotorState

with open('mayday_config.json', 'r') as f:
    config = json.load(f)

"""
I should have a separate test module for integration testing mayday, making sure all 
motors are online and can move etc.

And a separarte test module for unittesting the dynamixel conrtoller, mocking out real
dynamixels, I know what is supposed to be send, right? Well, no, but I do know that the
underlying dxl communication library must be called, and that is my boundary. The tests
should be to call the basic I need for controlling mayday, like enable torque, set goal, 
reset motors that failed, etc. The dynamixel controller should handle a set of dynamixls,
but not know about mayday. This is a more generic module. 
"""


@pytest.fixture()
def dxl_adapter():
    return DynamixelAdapter(None)


@pytest.fixture()
def initialized_dxl_adapter():
    dxl_adapter = DynamixelAdapter(None)
    dxl_adapter.init_communication()
    return dxl_adapter


class TestDynamixelAdapterTestCase:
    def test_when_init__then_ctrl_table_is_pd_dataframe(self, dxl_adapter):
        assert type(dxl_adapter.control_table) is pd.DataFrame

    def test_when_init__then_ctrl_table_index_is_data_name(self, dxl_adapter):
        assert dxl_adapter.control_table.index.name == 'Data Name'

    def test_when_init__then_torque_enable_address_is_64(self, dxl_adapter):
        actual_te_address = dxl_adapter.control_table.loc['Torque Enable', 'Address']
        assert 64, actual_te_address

    @pytest.mark.skip('does not mock failing port, so if port actually exists, test fails')
    def test_given_port_not_available__raises_no_robot_exception(self, dxl_adapter):
        with pytest.raises(DynamixelAdapter.NoRobotException) as e:
            dxl_adapter.init_communication()

    def test_given_drive_mode_forward__when_write_drive_mode__then_calls_dxl_write_with_0(self):
        id_num = 11
        drive_mode = 'forward'
        adapter = DynamixelAdapter(None)
        adapter.write_config = MagicMock()

        adapter.write_drive_mode(id_num, drive_mode)

        drive_mode_expected = 0
        adapter.write_config.assert_called_with(id_num, 'Drive Mode', drive_mode_expected)

    def test_given_drive_mode_backward__when_write_drive_mode__then_calls_dxl_write_with_1(self):
        id_num = 11
        drive_mode = 'backward'
        adapter = DynamixelAdapter(None)
        adapter.write_config = MagicMock()

        adapter.write_drive_mode(id_num, drive_mode)

        drive_mode_expected = 1
        adapter.write_config.assert_called_with(id_num, 'Drive Mode', drive_mode_expected)

    def test_given_drive_mode_1__when_read_drive_mode__then_returns_backward(self):
        id_num = 11
        drive_mode = 1
        adapter = DynamixelAdapter(None)
        adapter.dxl_read = MagicMock(return_value=drive_mode)

        actual = adapter.read_drive_mode(id_num)

        adapter.dxl_read.assert_called_with(id_num, 'Drive Mode')
        assert actual == 'backward'

    def test_given_drive_mode_bafwards__when_write_drive_mode__then_raises_value_error(self):
        id_num = 11
        drive_mode = 'bafwards'
        adapter = DynamixelAdapter(None)
        adapter.dxl_write = MagicMock()

        with pytest.raises(ValueError) as cm:
            adapter.write_drive_mode(id_num, drive_mode)


class TestRadianConversion:
    @pytest.mark.parametrize('angle, expected', [(0.0, 2048), (-tau/2, 1), (tau/2, 4095)])
    def test_given_angle__when_rad_to_int_range__then_returns_expected(self, angle, expected):
        actual = DynamixelAdapter.rad_to_int_range(angle)

        assert actual == expected

    @pytest.mark.parametrize('int_value, expected', [(2048, 0.0), (1, -tau/2), (4095, tau/2)])
    def test_given_int__when_int_range_to_rad__then_returns_expected(self, int_value, expected):
        actual = DynamixelAdapter.int_range_to_rad(int_value)

        assert actual == expected

    @pytest.mark.parametrize('angle', [tau, -tau, tau/2+0.0001, -tau/2-0.0001])
    def test_given_too_big_angle__when_rad_to_int_range__then_raises_value_error(self, angle):
        with pytest.raises(ValueError) as cm:
            actual = DynamixelAdapter.rad_to_int_range(angle)

    @pytest.mark.parametrize('int_value', [0, 4096])
    def test_given_too_big_int_value__when_int_range_to_rad__then_raises_value_error(self, int_value):
        with pytest.raises(ValueError) as cm:
            actual = DynamixelAdapter.int_range_to_rad(int_value)


class TestReadState:
    @pytest.fixture
    def mock_port_adapter(self):
        port_adapter = MagicMock()
        port_adapter.read = MagicMock(return_value=3)

        return port_adapter

    def test_then_returns_motor_state(self, mock_port_adapter):
        dxl_id = 4
        adapter = DynamixelAdapter(mock_port_adapter)

        actual = adapter.read_state(dxl_id)

        assert type(actual) == MotorState

    def test_given_read_present_pos_int_2048_then_returns_radians_0(self, mock_port_adapter):
        dxl_id = 4
        mock_port_adapter.read = MagicMock(return_value=2048)
        adapter = DynamixelAdapter(mock_port_adapter)

        actual = adapter.read_state(dxl_id).position

        assert actual == 0.0

    def test_given_read_present_vel_int_0__then_returns_0(self, mock_port_adapter):
        dxl_id = 4
        mock_port_adapter.read = MagicMock(return_value=0)
        adapter = DynamixelAdapter(mock_port_adapter)

        actual = adapter.read_state(dxl_id).velocity

        assert actual == 0.0

    def test_given_read_present_vel_int_1__then_returns_0_024(self, mock_port_adapter):
        dxl_id = 4
        mock_port_adapter.read = MagicMock(return_value=1)
        adapter = DynamixelAdapter(mock_port_adapter)

        actual = adapter.read_state(dxl_id).velocity

        assert 0.024 - actual < 0.01


if __name__ == '__main__':
    pytest.main()
