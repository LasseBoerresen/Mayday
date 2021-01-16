from dynamixel_adapter import DynamixelAdapter
from mayday_robot import MaydayRobotFactory, LegFactory, MaydayRobot


def main():

    may = create_mayday()
    may.set_legs_to_start_position()
    may.set_legs_to_standing_position()


def create_mayday() -> MaydayRobot:
    dxl_adapter = DynamixelAdapter(None)
    dxl_adapter.init_communication()
    leg_factory = LegFactory()
    mayday_factory = MaydayRobotFactory(leg_factory)
    may = mayday_factory.create_basic(dxl_adapter)

    return may


if __name__ == '__main__':
    main()