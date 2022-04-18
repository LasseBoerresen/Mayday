from dynamixel_adapter import DynamixelAdapter
from dynamixel_port_adapter import DynamixelPortAdapter
from mayday_robot import MaydayRobotFactory, LegFactory, MaydayRobot


def main():

    may = create_mayday()
    may.set_legs_to_start_position()
    may.set_legs_to_standing_position()


def create_mayday() -> MaydayRobot:
    dxl_port_adapter = DynamixelPortAdapter()
    dxl_adapter = DynamixelAdapter(dxl_port_adapter)  # Todo why does this not just init communication?
    dxl_adapter.port_adapter.init_communication()
    leg_factory = LegFactory()
    mayday_factory = MaydayRobotFactory(leg_factory, dxl_adapter)
    may = mayday_factory.create_basic()

    return may


if __name__ == '__main__':
    main()