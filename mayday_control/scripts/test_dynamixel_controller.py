import time

import pytest
import pandas as pd
from dynamixel_controller import DxlController


ROBOT_IS_CONNECTED = False

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
def dxl_controller():
    return DxlController()


@pytest.fixture()
def initialized_dxl_controller():
    dxl_controller = DxlController()
    dxl_controller.init_dxl_communication()
    return dxl_controller


class TestDxlController:
    def test_when_init__then_ctrl_table_is_pd_dataframe(self, dxl_controller):
        assert type(dxl_controller.control_table) is pd.DataFrame

    def test_when_init__then_ctrl_table_index_is_data_name(self, dxl_controller):
        assert dxl_controller.control_table.index.name == 'Data Name'

    def test_when_init__then_torque_enable_address_is_64(self, dxl_controller):
        actual_te_address = dxl_controller.control_table.loc['Torque Enable', 'Address']
        assert 64, actual_te_address

    def test_given_port_not_available__raises_no_robot_exception(self, dxl_controller):
        with pytest.raises(DxlController.NoRobotException) as e:
            dxl_controller.init_dxl_communication()


@pytest.mark.skipif(not ROBOT_IS_CONNECTED, reason='Robot is not connectd')
class TestDxlControllerIntegration:
    """
    Must have a Mayday robot connected by usb to /dev/ttyUSB0
    """

    def test_when_init_dxl_communication__then_port_handler_is_open(self, initialized_dxl_controller):
        assert initialized_dxl_controller.port_handler.is_open

    def test_when_init_dxl_communication__then_port_handler_baud_rate_is_set(self, initialized_dxl_controller):
        assert initialized_dxl_controller.port_handler.baudrate == initialized_dxl_controller.BAUD_RATE

    def test_when_init_dxl_communication__then_packet_handler_protocol_is_2(self, initialized_dxl_controller):
        assert initialized_dxl_controller.packet_handler.getProtocolVersion() == 2.0

    def test_when_torque_enable_dxl_1__then_is_read_as_enabled(self, initialized_dxl_controller):
        dxl_id = 1

        initialized_dxl_controller.dxl_write(dxl_id, 'Torque Enable', 1)

        assert initialized_dxl_controller.dxl_read(dxl_id, 'Torque Enable')

    def test_when_torque_disable_dxl_1__then_is_read_as_disabled(self, initialized_dxl_controller):
        dxl_id = 1

        initialized_dxl_controller.dxl_write(dxl_id, 'Torque Enable', 0)

        assert not initialized_dxl_controller.dxl_read(dxl_id, 'Torque Enable')

    def test_when_set_goal_position_2200__then_present_position_is_2200(self, initialized_dxl_controller):
        dxl_id = 1
        initialized_dxl_controller.dxl_write(dxl_id, 'Torque Enable', 0)
        initialized_dxl_controller.dxl_write(dxl_id, 'Min Position Limit', 0)
        initialized_dxl_controller.dxl_write(dxl_id, 'Max Position Limit', 4095)
        initialized_dxl_controller.dxl_write(dxl_id, 'Torque Enable', 1)

        initialized_dxl_controller.dxl_write(dxl_id, 'Goal Position', 2200)
        time.sleep(0.5)

        for i in range(5):
            if initialized_dxl_controller.dxl_read(dxl_id, 'Moving'):
                time.sleep(0.5)
        assert abs(2200 - initialized_dxl_controller.dxl_read(dxl_id, 'Present Position')) < 10

    def test_when_set_goal_position_2000__then_present_position_is_2000(self, initialized_dxl_controller):
        dxl_id = 1
        initialized_dxl_controller.dxl_write(dxl_id, 'Torque Enable', 0)
        initialized_dxl_controller.dxl_write(dxl_id, 'Min Position Limit', 0)
        initialized_dxl_controller.dxl_write(dxl_id, 'Max Position Limit', 4095)
        initialized_dxl_controller.dxl_write(dxl_id, 'Torque Enable', 1)

        initialized_dxl_controller.dxl_write(dxl_id, 'Goal Position', 2000)
        time.sleep(0.5)

        for i in range(5):
            if initialized_dxl_controller.dxl_read(dxl_id, 'Moving'):
                time.sleep(0.5)
        assert abs(2000 - initialized_dxl_controller.dxl_read(dxl_id, 'Present Position')) < 10

    def test_given_bad_position_limit__when_set_goal_pos__then_raises_limit_exceeded(self, initialized_dxl_controller):
        dxl_id = 1
        initialized_dxl_controller.dxl_write(dxl_id, 'Torque Enable', 0)
        initialized_dxl_controller.dxl_write(dxl_id, 'Min Position Limit', 2000)
        initialized_dxl_controller.dxl_write(dxl_id, 'Max Position Limit', 4095)
        initialized_dxl_controller.dxl_write(dxl_id, 'Torque Enable', 1)

        with pytest.raises(Exception) as e:
            initialized_dxl_controller.dxl_write(dxl_id, 'Goal Position', 1500)
        assert "The data value exceeds the limit value" in str(e)

    def test_when_init_dxl__then_torque_is_enabled(self, initialized_dxl_controller):
        dxl_id = 4
        initialized_dxl_controller.dxl_write(dxl_id, 'Torque Enable', 0)

        initialized_dxl_controller.init_dxl(dxl_id, side='left')

        assert initialized_dxl_controller.dxl_read(dxl_id, 'Torque Enable')

    def test_given_side_right__when_init_dxl__then_drive_mode_is_1(self, initialized_dxl_controller):
        dxl_id = 7
        initialized_dxl_controller.dxl_write(dxl_id, 'Torque Enable', 0)
        initialized_dxl_controller.dxl_write(dxl_id, 'Drive Mode', 0)

        initialized_dxl_controller.init_dxl(dxl_id, side='right')

        assert initialized_dxl_controller.dxl_read(dxl_id, 'Drive Mode') == 1

    def test_given_side_left__when_init_dxl__then_drive_mode_is_0(self, initialized_dxl_controller):
        dxl_id = 16
        initialized_dxl_controller.dxl_write(dxl_id, 'Torque Enable', 0)
        initialized_dxl_controller.dxl_write(dxl_id, 'Drive Mode', 0)

        initialized_dxl_controller.init_dxl(dxl_id, side='left')

        assert initialized_dxl_controller.dxl_read(dxl_id, 'Drive Mode') == 0

    def test_given_side_lleft__when_init_dxl__then_raises_value_error(self, initialized_dxl_controller):
        dxl_id = 15
        initialized_dxl_controller.dxl_write(dxl_id, 'Torque Enable', 0)

        with pytest.raises(ValueError) as e:
            initialized_dxl_controller.init_dxl(dxl_id, side='lleft')
        assert 'Side not recognized, expected right or left, got: lleft' in str(e)

"""
Converting back and forth from rad to int range to rad should give the same value back as has been put in
>>> t=DxlController()
>>> t.rad_to_int_range(t.int_range_to_rad(0, 0, 4095), 0, 4095)
0

>>> t=DxlController()
>>> t.rad_to_int_range(t.int_range_to_rad(1, 0, 4095), 0, 4095)
1

>>> t=DxlController()
>>> t.rad_to_int_range(t.int_range_to_rad(2048, 0, 4095), 0, 4095)
2048

>>> t=DxlController()
>>> t.rad_to_int_range(t.int_range_to_rad(4095, 0, 4095), 0, 4095)
4095

>>> t=DxlController()
>>> t.rad_to_int_range(t.int_range_to_rad(0, 0, 4095), 0, 4095)
0
"""

if __name__ == '__main__':
    pytest.main()
