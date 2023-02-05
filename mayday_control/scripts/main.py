import time

from dynamixel_sdk import PortHandler, PacketHandler

from dynamixel.dynamixel_adapter import DynamixelAdapter
from dynamixel.dynamixel_port_adapter import DynamixelPortAdapter
from mayday_robot import MaydayRobot
from leg import Leg
from leg_factory import LegFactory
from mayday_robot_factory import MaydayRobotFactory


def main():
    may: MaydayRobot = create_mayday()

    may.enable_torque()
    may.set_joint_positions_for_all_legs(Leg.POSE_STARTING)
    time.sleep(1)
    may.set_joint_positions_for_all_legs(Leg.POSE_STANDING)


def create_mayday() -> MaydayRobot:
    port_handler = PortHandler(port_name='/dev/ttyUSB0')
    packet_handler = PacketHandler(protocol_version=2.0)
    dxl_port_adapter = DynamixelPortAdapter(port_handler, packet_handler)
    dxl_port_adapter.init_communication()

    dxl_adapter = DynamixelAdapter(dxl_port_adapter)

    leg_factory = LegFactory(dxl_adapter)
    mayday_factory = MaydayRobotFactory(leg_factory, dxl_adapter)

    return mayday_factory.create_basic()


if __name__ == '__main__':
    main()
