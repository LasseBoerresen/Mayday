from typing import List

# from urdf_parser_py.urdf import URDF
from leg import Leg
from leg_pose import LegPose


class MaydayRobot:
    def __init__(self, legs: List[Leg]):
        self.legs = legs
        self.description = None

        # self.read_description()

    # def read_description(self):
    #     description_xacro_path = os.path.join(os.path.dirname(__file__), 'mayday.urdf.xacro')
    #     description_urdf = xacro.process(description_xacro_path)
    #     self.description = URDF.from_xml_string(description_urdf)

    def set_joint_positions_for_all_legs(self, pose: LegPose):
        for leg in self.legs:
            leg.set_joint_positions(pose)

    def disable_torque(self):
        NotImplementedError()


