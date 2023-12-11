from math import tau

import numpy as np
import pytest

from geometry.pose import Pose
from geometry.vec3 import Vec3
from physical_quantities.angle import Angle
from side import LeftSide, RightSide, Side
from structure.components import CoxaJoint, CoxaMotorLink, CoxaMotorJoint
from structure.leg_position import BackLegPosition, CenterLegPosition, FrontLegPosition, LegPosition
from structure.structure import Structure
from tests.structure_tests.fake_motor import FakeMotor


class TestStructure:

    @pytest.fixture()
    def structure(self):
        return Structure()

    def test__when_get_thorax_position__then_returns_0_0_0_0_0_0_pose(self, structure):
        # Given

        # When
        actual = structure.get_thorax_position()

        # Then
        expected = Pose.zeros()
        assert actual == expected

    @pytest.mark.parametrize(
        "side,leg_position,expected_pose",
        [
            (RightSide, CenterLegPosition, Pose(Vec3(0, 0.086, 0), Vec3(0, 0, -tau / 4))),
            (RightSide, FrontLegPosition, Pose(Vec3(0.077, 0.074, 0), Vec3(0, 0, -tau / 4 - Angle.from_deg(55)))),
            (RightSide, BackLegPosition, Pose(Vec3(-0.077, 0.074, 0), Vec3(0, 0, -tau / 4 + Angle.from_deg(55)))),
            (LeftSide, CenterLegPosition, Pose(Vec3(0, -0.086, 0), Vec3(0, 0, tau / 4))),
            (LeftSide, FrontLegPosition, Pose(Vec3(0.077, -0.074, 0), Vec3(0, 0, tau / 4 + Angle.from_deg(55)))),
            (LeftSide, BackLegPosition, Pose(Vec3(-0.077, -0.074, 0), Vec3(0, 0, tau / 4 - Angle.from_deg(55)))),
        ])
    def test__given_CoxaMotorJoint__when_get_origin__then_returns_expected_pose(
            self, side: Side, leg_position: LegPosition, expected_pose: Pose):

        # Given
        joint = CoxaMotorJoint(side, leg_position)

        # When
        actual = joint.origin

        # Then
        assert actual == expected_pose

    def test__given_CoxaJoint__when_get_origin__then_returns_pose_zeros(self):
        # Given
        joint = CoxaJoint(FakeMotor())

        # When
        actual = joint.origin

        # Then
        assert actual == Pose.zeros()

    def test__given_FemurMotorJoint__when_get_origin__then_returns_expected_pose(self):
        # Given
        joint = FemurMotorJoint()

        # When
        actual = joint.origin

        # Then
        assert actual == Pose(Vec3(0.1, 0, -0.1), Vec3(tau/4, 0, tau/4))


# TODO TDD simplest cases of forward Kinematics test

if __name__ == '__main__':
    pytest.main()
