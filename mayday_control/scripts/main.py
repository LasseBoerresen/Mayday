from dynamixel_sdk import PortHandler, PacketHandler

from dynamixel_adapter import DynamixelAdapter
from dynamixel_port_adapter import DynamixelPortAdapter
from mayday_robot import MaydayRobot
from leg import Leg
from leg_factory import LegFactory
from mayday_robot_factory import MaydayRobotFactory


def main():
    may: MaydayRobot = create_mayday()

    may.disable_torque()
    may.set_joint_positions_for_all_legs(Leg.POSE_STARTING)
    may.set_joint_positions_for_all_legs(Leg.POSE_STANDING)


def create_mayday() -> MaydayRobot:
    dxl_port_adapter = DynamixelPortAdapter(PortHandler('/dev/ttyUSB0'), PacketHandler(2.0))
    dxl_port_adapter.init_communication()

    dxl_adapter = DynamixelAdapter(dxl_port_adapter)  # Todo why does this not just init communication?

    leg_factory = LegFactory(dxl_adapter)
    mayday_factory = MaydayRobotFactory(leg_factory)
    return mayday_factory.create_basic()


if __name__ == '__main__':
    main()
