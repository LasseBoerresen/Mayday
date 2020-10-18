import os
import xacro

from urdf_parser_py.urdf import URDF


class MaydayRobot:
    def __init__(self):
        self.state = None
        self.description = None

        self.read_description()

        self.state = State(self.urdf)

    def read_description(self):
        description_xacro_path = os.path.join(os.path.dirname(__file__), 'mayday.urdf.xacro')
        description_urdf = xacro.process(description_xacro_path)
        self.description = URDF.from_xml_string(description_urdf)

    def build_state_from_description(self):
        self.state = State


class State:
    def __init__(self, urdf):
        urdf