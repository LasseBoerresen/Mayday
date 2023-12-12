from math import tau

import numpy as np
import pytest

from geometry.pose import Pose
from geometry.vec3 import Vec3
from physical_quantities.angle import Angle
from side import LeftSide, RightSide, Side
from structure.components import CoxaJoint, CoxaMotorAttachment, FemurJoint, FemurMotorAttachment, Joint
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

    @pytest.mark.parametrize("side, leg_position, expected_pose",
    [
        (RightSide, CenterLegPosition, Pose(Vec3(0, 0.086, 0), Vec3(0, 0, -tau / 4))),
        (RightSide, FrontLegPosition, Pose(Vec3(0.077, 0.074, 0), Vec3(0, 0, -tau / 4 - Angle.from_deg(55)))),
        (RightSide, BackLegPosition, Pose(Vec3(-0.077, 0.074, 0), Vec3(0, 0, -tau / 4 + Angle.from_deg(55)))),
        (LeftSide, CenterLegPosition, Pose(Vec3(0, -0.086, 0), Vec3(0, 0, tau / 4))),
        (LeftSide, FrontLegPosition, Pose(Vec3(0.077, -0.074, 0), Vec3(0, 0, tau / 4 + Angle.from_deg(55)))),
        (LeftSide, BackLegPosition, Pose(Vec3(-0.077, -0.074, 0), Vec3(0, 0, tau / 4 - Angle.from_deg(55)))),
    ])
    def test__given_CoxaMotorAttachment__when_get_origin__then_returns_expected_pose(
            self, side: Side, leg_position: LegPosition, expected_pose: Pose):
        # Given
        joint = CoxaMotorAttachment(side, leg_position)

        # When
        actual = joint.origin

        # Then
        assert actual == expected_pose

    @pytest.mark.parametrize('joint, expected_pose',
    [
        (CoxaJoint(FakeMotor()), Pose.zeros()),
        (FemurMotorAttachment(), Pose(Vec3(0.032, 0, -0.011), Vec3(tau / 4, 0, tau / 4))),
        (FemurJoint(FakeMotor()), Pose.zeros())
    ])
    def test__given_CoxaJoint__when_get_origin__then_returns_expected_pose(self, joint: Joint, expected_pose: Pose):
        # Given

        # When
        actual = joint.origin

        # Then
        assert actual == expected_pose

    @pytest.mark.parametrize('joint_type',
    [
        CoxaJoint,
        FemurJoint
    ])
    def test__given_RotaionalJoint__when_get_axis__then_returns_z(self, joint_type: type):
        # Given
        joint = joint_type(FakeMotor())

        # When
        actual = joint.axis

        # Then
        assert actual == Vec3.z_axis()


# TODO TDD simplest cases of forward Kinematics test
# TODO TDD printing meaninful name of each component, based on its parents.

if __name__ == '__main__':
    pytest.main()
