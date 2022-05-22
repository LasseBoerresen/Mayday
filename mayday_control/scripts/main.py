from dynamixel_adapter import DynamixelAdapter
from dynamixel_port_adapter import DynamixelPortAdapter
from mayday_robot import MaydayRobotFactory, LegFactory, MaydayRobot, Leg


def main():
    may: MaydayRobot = create_mayday()

    may.disable_torque()
    may.set_joint_positions_for_all_legs(Leg.POSE_STARTING)
    may.set_joint_positions_for_all_legs(Leg.POSE_STANDING)


def create_mayday() -> MaydayRobot:
    dxl_port_adapter = DynamixelPortAdapter()
    dxl_adapter = DynamixelAdapter(dxl_port_adapter)  # Todo why does this not just init communication?
    dxl_adapter.port_adapter.init_communication()
    leg_factory = LegFactory(dxl_adapter)
    mayday_factory = MaydayRobotFactory(leg_factory)
    may = mayday_factory.create_basic()

    return may


if __name__ == '__main__':
    main()