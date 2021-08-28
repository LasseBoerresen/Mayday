import json
import time
from unittest.mock import MagicMock
import pytest
from test_dynamixel_adapter import initialized_dxl_adapter

with open('mayday_config.json', 'r') as f:
    config = json.load(f)


@pytest.mark.skipif(not config['robot_is_connected'], reason='Robot is not connectd')
class TestDynamixelAdapterIntegrationTestCase:
    """
    Must have a Mayday robot connected by usb to /dev/ttyUSB0
    """

    def test_when_init_dxl_communication__then_port_handler_is_open(self, initialized_dxl_adapter):
        assert initialized_dxl_adapter.port_adapter.port_handler.is_open

    def test_when_init_dxl_communication__then_port_handler_baud_rate_is_set(self, initialized_dxl_adapter):
        assert initialized_dxl_adapter.port_adapter.port_handler.baudrate == initialized_dxl_adapter.BAUD_RATE

    def test_when_init_dxl_communication__then_packet_handler_protocol_is_2(self, initialized_dxl_adapter):
        assert initialized_dxl_adapter.packet_handler.getProtocolVersion() == 2.0

    def test_when_torque_enable_dxl_1__then_is_read_as_enabled(self, initialized_dxl_adapter):
        dxl_id = 1

        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Torque Enable', 1)

        assert initialized_dxl_adapter.port_adapter.read(dxl_id, 'Torque Enable')

    def test_when_torque_disable_dxl_1__then_is_read_as_disabled(self, initialized_dxl_adapter):
        dxl_id = 1

        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Torque Enable', 0)

        assert not initialized_dxl_adapter.port_adapter.read(dxl_id, 'Torque Enable')

    def test_when_set_goal_position_2200__then_present_position_is_2200(self, initialized_dxl_adapter):
        dxl_id = 1
        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Torque Enable', 0)
        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Min Position Limit', 0)
        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Max Position Limit', 4095)
        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Torque Enable', 1)

        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Goal Position', 2200)
        time.sleep(0.5)

        for i in range(5):
            if initialized_dxl_adapter.port_adapter.read(dxl_id, 'Moving'):
                time.sleep(0.5)
        assert abs(2200 - initialized_dxl_adapter.port_adapter.read(dxl_id, 'Present Position')) < 10

    def test_when_set_goal_position_2000__then_present_position_is_2000(self, initialized_dxl_adapter):
        dxl_id = 1
        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Torque Enable', 0)
        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Min Position Limit', 0)
        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Max Position Limit', 4095)
        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Torque Enable', 1)

        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Goal Position', 2000)
        time.sleep(0.5)

        for i in range(5):
            if initialized_dxl_adapter.port_adapter.read(dxl_id, 'Moving'):
                time.sleep(0.5)
        assert abs(2000 - initialized_dxl_adapter.port_adapter.read(dxl_id, 'Present Position')) < 10

    def test_given_bad_position_limit__when_set_goal_pos__then_raises_limit_exceeded(self, initialized_dxl_adapter):
        dxl_id = 1
        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Torque Enable', 0)
        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Min Position Limit', 2000)
        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Max Position Limit', 4095)
        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Torque Enable', 1)

        with pytest.raises(Exception) as e:
            initialized_dxl_adapter.port_adapter.write(dxl_id, 'Goal Position', 1500)
        assert "The data value exceeds the limit value" in str(e)

    def test_when_init_single__then_torque_is_enabled(self, initialized_dxl_adapter):
        dxl_id = 4
        initialized_dxl_adapter.port_adapter.write(dxl_id, 'Torque Enable', 0)

        initialized_dxl_adapter.init_single(dxl_id, 'forward')

        assert initialized_dxl_adapter.port_adapter.read(dxl_id, 'Torque Enable')

    def test_give_drive_mode_forward__when_init_single__writes_drive_mode(self, initialized_dxl_adapter):
        dxl_id = 4
        drive_mode = 'forward'
        initialized_dxl_adapter.write_drive_mode = MagicMock()

        initialized_dxl_adapter.init_single(dxl_id, drive_mode)

        initialized_dxl_adapter.write_drive_mode.assert_called_once_with(dxl_id, drive_mode)

    def test_given_drive_mode_backward__when_write__then_drive_mode_is_1(self, initialized_dxl_adapter):
        dxl_id = 7
        drive_mode = 'backward'

        initialized_dxl_adapter.write_drive_mode(dxl_id, drive_mode)

        drive_mode_expected = 1
        assert initialized_dxl_adapter.port_adapter.read(dxl_id, 'Drive Mode') == drive_mode_expected

    def test_given_drive_mode_forward__when_write__then_drive_mode_is_0(self, initialized_dxl_adapter):
        dxl_id = 7
        drive_mode = 'forward'

        initialized_dxl_adapter.write_drive_mode(dxl_id, drive_mode)

        drive_mode_expected = 0
        assert initialized_dxl_adapter.port_adapter.read(dxl_id, 'Drive Mode') == drive_mode_expected
