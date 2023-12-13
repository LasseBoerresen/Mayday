from geometry.pose import Pose
from geometry.vec3 import Vec3
from side import Side
from structure.components import Joint, Link
from structure.leg_position import LegPosition


class Structure():
    def __init__(self, links: [Link], joints: [Joint]):
        pass

    def get_pose_of(self, ):
        pass


class MaydayStructure(Structure):
    def get_thorax_pose(self) -> Pose:
        return Pose.zeros()
        # TODO fix cheat value

    def get_leg_root_pose(self, leg_position: LegPosition, side: Side) -> Pose:
        return self._get_leg(leg_position, side).origin

    # def _get_leg(self, leg_position: LegPosition, side: Side) -> Leg:
    #     return self._legs[(leg_position, side)]



